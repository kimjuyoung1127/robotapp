// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 안전/진단 패널과 fault 오버레이 시안을 구성하고 preview state를 반영합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ConnectionHomeController))]
    public sealed class SafetyDiagnosticsController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset safetyDiagnosticsTemplate;
        [SerializeField] private VisualTreeAsset faultOverlayTemplate;

        private VisualElement root;
        private VisualElement safetyDiagnosticsHost;
        private VisualElement faultOverlayHost;
        private ConnectionHomeController connectionHomeController;
        private SafetyElements safetyElements;
        private FaultOverlayElements faultOverlayElements;
        private bool isInitialized;
        private Coroutine initializeCoroutine;

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
            if (initializeCoroutine != null)
            {
                StopCoroutine(initializeCoroutine);
                initializeCoroutine = null;
            }

            isInitialized = false;
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        internal void RefreshFromBinder(PendantV3PreviewState.Definition data)
        {
            if (!isInitialized && !TryInitialize())
            {
                return;
            }

            ApplyPreview(data);
        }

        public string GetDebugSummary()
        {
            var state = connectionHomeController != null ? connectionHomeController.CurrentPreviewState.ToString() : "none";
            var overlayVisible = faultOverlayHost != null && !faultOverlayHost.ClassListContains("rc-hidden");
            var bannerText = safetyElements?.SafetyBannerText?.text ?? "missing";
            var faultText = safetyElements?.FaultStateValue?.text ?? "missing";
            return $"initialized={isInitialized}; state={state}; overlayVisible={overlayVisible}; banner={bannerText}; fault={faultText}";
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            root = document?.rootVisualElement;
            if (root == null || safetyDiagnosticsTemplate == null || faultOverlayTemplate == null || connectionHomeController == null)
            {
                return false;
            }

            safetyDiagnosticsHost = root.Q<VisualElement>("SafetyDiagnosticsHost");
            faultOverlayHost = root.Q<VisualElement>("FaultOverlayHost");
            if (safetyDiagnosticsHost == null || faultOverlayHost == null)
            {
                isInitialized = false;
                return false;
            }

            if (safetyElements == null || safetyDiagnosticsHost.childCount == 0)
            {
                safetyElements = CreateSafetyPanel(safetyDiagnosticsHost);
            }

            if (faultOverlayElements == null || faultOverlayHost.childCount == 0)
            {
                faultOverlayElements = CreateFaultOverlay(faultOverlayHost);
            }

            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            isInitialized = true;
            return true;
        }

        private System.Collections.IEnumerator WaitForInitialize()
        {
            for (var frame = 0; frame < 30 && !isInitialized; frame++)
            {
                TryInitialize();
                if (isInitialized)
                {
                    break;
                }

                yield return null;
            }

            initializeCoroutine = null;
        }

        private SafetyElements CreateSafetyPanel(VisualElement host)
        {
            host.Clear();
            var tree = safetyDiagnosticsTemplate.CloneTree();
            host.Add(tree);
            return new SafetyElements(tree);
        }

        private FaultOverlayElements CreateFaultOverlay(VisualElement host)
        {
            host.Clear();
            var tree = faultOverlayTemplate.CloneTree();
            host.Add(tree);
            return new FaultOverlayElements(tree);
        }

        private void ApplyPreview(PendantV3PreviewState.Definition data)
        {
            if (safetyElements == null)
            {
                return;
            }

            var state = connectionHomeController.CurrentPreviewState;
            var isFault = state == PendantV3PreviewState.Kind.Fault;
            var isWarning = state is PendantV3PreviewState.Kind.ConnectedUnsynced or PendantV3PreviewState.Kind.AutoReconnect;

            safetyElements.SafetyBannerText.text = isFault
                ? "안전 상태: Fault 감지 · 조작 잠금"
                : isWarning
                    ? "안전 상태: 주의 · 동기화/재연결 확인"
                    : "안전 상태: 정상";
            SetBannerState(safetyElements.SafetyBanner, isFault, isWarning);

            safetyElements.SafetyStateValue.text = $"Safety: {data.StatusSafety}";
            safetyElements.FaultStateValue.text = $"Fault: {data.StatusFault}";
            SetChipState(safetyElements.SafetyStateValue, isFault, isWarning);
            SetFaultChipState(safetyElements.FaultStateValue, isFault);

            safetyElements.RecoveryNow.text = $"지금 상태: {data.ActionNow}";
            safetyElements.RecoveryPrimary.text = $"다음 행동: {data.ActionPrimary}";
            safetyElements.RecoveryWhy.text = data.ActionWhy;
            safetyElements.BtnRecoveryPrimary.text = data.PrimaryActionLabel;
            safetyElements.BtnRecoveryPrimary.SetEnabled(data.PrimaryActionEnabled);

            safetyElements.EventLogLine1.text = $"연결 {data.StatusConnection} · 서보 {data.StatusServo}";
            safetyElements.EventLogLine2.text = $"상태 {data.StatusMotion} · Safety {data.StatusSafety}";
            safetyElements.EventLogLine3.text = $"Fault {data.StatusFault} · Coord {data.CoordSystem}";

            ApplyFaultOverlay(data, isFault);
        }

        private void ApplyFaultOverlay(PendantV3PreviewState.Definition data, bool isFault)
        {
            if (faultOverlayHost == null || faultOverlayElements == null)
            {
                return;
            }

            faultOverlayHost.EnableInClassList("rc-hidden", !isFault);
            faultOverlayElements.FaultOverlaySummary.text = isFault
                ? $"코드 {data.StatusFault} · Safety {data.StatusSafety}"
                : "오류 없음";
            faultOverlayElements.FaultOverlaySteps.text = isFault
                ? "1. 주변 안전 확인 → 2. 원인 확인 → 3. 오류 초기화"
                : "안전 상태가 정상이면 오버레이는 자동으로 닫힌다.";
            faultOverlayElements.BtnFaultOverlayReset.SetEnabled(isFault && data.ResetEnabled);
            faultOverlayElements.BtnFaultOverlayClose.SetEnabled(isFault);
        }

        private static void SetBannerState(VisualElement banner, bool isFault, bool isWarning)
        {
            if (banner == null)
            {
                return;
            }

            banner.EnableInClassList("rc-safety-banner--safe", !isFault && !isWarning);
            banner.EnableInClassList("rc-safety-banner--warning", !isFault && isWarning);
            banner.EnableInClassList("rc-safety-banner--danger", isFault);
        }

        private static void SetChipState(Label label, bool isFault, bool isWarning)
        {
            if (label == null)
            {
                return;
            }

            label.EnableInClassList("rc-safety-chip--safe", !isFault && !isWarning);
            label.EnableInClassList("rc-safety-chip--warning", !isFault && isWarning);
            label.EnableInClassList("rc-safety-chip--danger", isFault);
            label.EnableInClassList("rc-safety-chip--muted", false);
        }

        private static void SetFaultChipState(Label label, bool isFault)
        {
            if (label == null)
            {
                return;
            }

            label.EnableInClassList("rc-safety-chip--danger", isFault);
            label.EnableInClassList("rc-safety-chip--muted", !isFault);
            label.EnableInClassList("rc-safety-chip--safe", false);
            label.EnableInClassList("rc-safety-chip--warning", false);
        }

        private sealed class SafetyElements
        {
            public SafetyElements(VisualElement root)
            {
                SafetyBanner = root.Q<VisualElement>("SafetyBanner");
                SafetyBannerText = root.Q<Label>("SafetyBannerText");
                SafetyStateValue = root.Q<Label>("SafetyStateValue");
                FaultStateValue = root.Q<Label>("FaultStateValue");
                RecoveryNow = root.Q<Label>("RecoveryNow");
                RecoveryPrimary = root.Q<Label>("RecoveryPrimary");
                RecoveryWhy = root.Q<Label>("RecoveryWhy");
                BtnRecoveryPrimary = root.Q<Button>("BtnRecoveryPrimary");
                EventLogLine1 = root.Q<Label>("EventLogLine1");
                EventLogLine2 = root.Q<Label>("EventLogLine2");
                EventLogLine3 = root.Q<Label>("EventLogLine3");
            }

            public VisualElement SafetyBanner { get; }
            public Label SafetyBannerText { get; }
            public Label SafetyStateValue { get; }
            public Label FaultStateValue { get; }
            public Label RecoveryNow { get; }
            public Label RecoveryPrimary { get; }
            public Label RecoveryWhy { get; }
            public Button BtnRecoveryPrimary { get; }
            public Label EventLogLine1 { get; }
            public Label EventLogLine2 { get; }
            public Label EventLogLine3 { get; }
        }

        private sealed class FaultOverlayElements
        {
            public FaultOverlayElements(VisualElement root)
            {
                FaultOverlaySummary = root.Q<Label>("FaultOverlaySummary");
                FaultOverlaySteps = root.Q<Label>("FaultOverlaySteps");
                BtnFaultOverlayReset = root.Q<Button>("BtnFaultOverlayReset");
                BtnFaultOverlayClose = root.Q<Button>("BtnFaultOverlayClose");
            }

            public Label FaultOverlaySummary { get; }
            public Label FaultOverlaySteps { get; }
            public Button BtnFaultOverlayReset { get; }
            public Button BtnFaultOverlayClose { get; }
        }
    }
}
