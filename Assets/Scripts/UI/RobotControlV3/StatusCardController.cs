// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UIElements;
using KineTutor3D.App.Fairino;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 우측 StatusCard와 CoordStrip 시안을 구성하고 preview state를 적용합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ConnectionHomeController))]
    public sealed class StatusCardController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset coordStripTemplate;
        [SerializeField] private VisualTreeAsset statusCardTemplate;

        private VisualElement root;
        private VisualElement coordStripHost;
        private VisualElement statusCardHost;
        private Label actionHintTitle;
        private Label actionHintPrimary;
        private Label actionHintSummary;
        private Label coordOverlayRowA;
        private Label coordOverlayRowB;
        private ConnectionHomeController connectionHomeController;
        private PendantV3ShellStateController shellStateController;
        private EventCallback<ClickEvent> faultDetailClickCallback;
        private EventCallback<ClickEvent> safetyDetailClickCallback;

        private CoordStripElements coordStrip;
        private StatusCardElements statusCard;
        private CoordStripMode coordStripMode = CoordStripMode.Both;
        private bool isInitialized;
        private bool isInitializing;
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

        private bool TryInitialize()
        {
            if (isInitialized)
            {
                return true;
            }

            if (isInitializing)
            {
                return false;
            }

            isInitializing = true;
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            shellStateController ??= GetComponent<PendantV3ShellStateController>();
            root = document?.rootVisualElement;
            try
            {
                if (root == null || coordStripTemplate == null || statusCardTemplate == null || connectionHomeController == null)
                {
                    return false;
                }

                CacheShellElements();
                if (coordStripHost == null || statusCardHost == null)
                {
                    isInitialized = false;
                    return false;
                }

                if (coordStrip == null || statusCard == null || coordStripHost.childCount == 0 || statusCardHost.childCount == 0)
                {
                    BuildPanels();
                }

                if (statusCard?.BtnFaultDetail != null)
                {
                    faultDetailClickCallback ??= _ => HandleFaultDetailClicked();
                    statusCard.BtnFaultDetail.clicked -= HandleFaultDetailClicked;
                    statusCard.BtnFaultDetail.clicked += HandleFaultDetailClicked;
                    statusCard.BtnFaultDetail.UnregisterCallback<ClickEvent>(faultDetailClickCallback);
                    statusCard.BtnFaultDetail.RegisterCallback<ClickEvent>(faultDetailClickCallback);
                }

                if (statusCard?.BtnSafetyDetail != null)
                {
                    safetyDetailClickCallback ??= _ => HandleSafetyDetailClicked();
                    statusCard.BtnSafetyDetail.clicked -= HandleSafetyDetailClicked;
                    statusCard.BtnSafetyDetail.clicked += HandleSafetyDetailClicked;
                    statusCard.BtnSafetyDetail.UnregisterCallback<ClickEvent>(safetyDetailClickCallback);
                    statusCard.BtnSafetyDetail.RegisterCallback<ClickEvent>(safetyDetailClickCallback);
                }

                BindCoordModeButtons();

                isInitialized = true;
                ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
                return true;
            }
            finally
            {
                isInitializing = false;
            }
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            return $"initialized={isInitialized}; coordMode={coordStripMode}; jointHidden={coordStrip?.JointGrid?.ClassListContains("rc-hidden") ?? true}; tcpHidden={coordStrip?.TcpGrid?.ClassListContains("rc-hidden") ?? true}";
        }

        public string SetCoordStripModeForDebug(string mode)
        {
            coordStripMode = mode switch
            {
                "Joint" => CoordStripMode.Joint,
                "TCP" => CoordStripMode.Tcp,
                "Tcp" => CoordStripMode.Tcp,
                _ => CoordStripMode.Both,
            };
            ApplyCoordStripMode();
            return GetDebugSummary();
        }

        internal void RefreshFromBinder(RobotControlV3RuntimeSnapshot data)
        {
            if (!isInitialized && !TryInitialize())
            {
                return;
            }

            ApplyPreview(data);
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

        private void CacheShellElements()
        {
            coordStripHost = root.Q<VisualElement>("CoordStripHost");
            statusCardHost = root.Q<VisualElement>("StatusCardHost");
            actionHintTitle = root.Q<Label>("ActionHintTitle");
            actionHintPrimary = root.Q<Label>("ActionHintPrimary");
            actionHintSummary = root.Q<Label>("ActionHintSummary");
            coordOverlayRowA = root.Q<Label>("CoordOverlayRowA");
            coordOverlayRowB = root.Q<Label>("CoordOverlayRowB");
        }

        private void BuildPanels()
        {
            coordStrip = CreateCoordStrip(coordStripHost);
            statusCard = CreateStatusCard(statusCardHost);
        }

        private CoordStripElements CreateCoordStrip(VisualElement host)
        {
            if (host == null)
            {
                return null;
            }

            host.Clear();
            var tree = coordStripTemplate.CloneTree();
            host.Add(tree);
            return new CoordStripElements(tree);
        }

        private StatusCardElements CreateStatusCard(VisualElement host)
        {
            if (host == null)
            {
                return null;
            }

            host.Clear();
            var tree = statusCardTemplate.CloneTree();
            host.Add(tree);
            return new StatusCardElements(tree);
        }

        private void ApplyPreview(RobotControlV3RuntimeSnapshot data)
        {
            ApplyCoordStrip(data);
            ApplyStatusCard(data);
            ApplyContextGuidance(data);

            if (coordOverlayRowA != null)
            {
                coordOverlayRowA.text = data.CoordOverlayJointLine;
            }

            if (coordOverlayRowB != null)
            {
                coordOverlayRowB.text = data.CoordOverlayTcpLine;
            }
        }

        private void ApplyCoordStrip(RobotControlV3RuntimeSnapshot data)
        {
            if (coordStrip == null)
            {
                return;
            }

            coordStrip.CoordSystemBadge.text = data.CoordChip;
            SetValues(coordStrip.JointValues, data.JointValues);
            SetValues(coordStrip.TcpValues, data.TcpValues);
            ApplyCoordStripMode();
        }

        private void ApplyStatusCard(RobotControlV3RuntimeSnapshot data)
        {
            if (statusCard == null)
            {
                return;
            }

            statusCard.StatusConnectionValue.text = data.StatusConnection;
            statusCard.StatusModeValue.text = data.StatusMode;
            statusCard.StatusServoValue.text = data.StatusServo;
            statusCard.StatusMotionValue.text = data.StatusMotion;
            statusCard.StatusFaultValue.text = data.StatusFault;
            statusCard.StatusSafetyValue.text = data.StatusSafety;
            statusCard.StatusToolValue.text = data.StatusTool;
            statusCard.StatusUserValue.text = data.StatusUser;
            statusCard.StatusSpeedValue.text = data.StatusSpeed;
            statusCard.BtnFaultDetail?.SetEnabled(data.FaultDetailEnabled);
            statusCard.BtnSafetyDetail?.SetEnabled(data.SafetyDetailEnabled);

            ApplyValueState(statusCard.StatusConnectionValue, data.StatusConnectionClass);
            ApplyValueState(statusCard.StatusModeValue, data.StatusModeClass);
            ApplyValueState(statusCard.StatusServoValue, data.StatusServoClass);
            ApplyValueState(statusCard.StatusMotionValue, data.StatusMotionClass);
            ApplyValueState(statusCard.StatusFaultValue, data.StatusFaultClass);
            ApplyValueState(statusCard.StatusSafetyValue, data.StatusSafetyClass);
        }

        private void ApplyContextGuidance(RobotControlV3RuntimeSnapshot data)
        {
            if (actionHintTitle != null)
            {
                actionHintTitle.text = "운영자 상태";
            }

            if (actionHintPrimary != null)
            {
                actionHintPrimary.text = data.OperatorStatusHeadline;
            }

            if (actionHintSummary != null)
            {
                actionHintSummary.text = $"{data.LiveTrackingStatus} / {BuildGateSummary(data)}";
            }
        }

        private static string BuildGateSummary(RobotControlV3RuntimeSnapshot data)
        {
            if (data == null)
            {
                return "잠금 이유: 확인 중 · 언제 풀리는지: 확인 중";
            }

            var reason = !string.IsNullOrWhiteSpace(data.MotionGateWhyLocked)
                ? data.MotionGateWhyLocked
                : !string.IsNullOrWhiteSpace(data.LiveBlockedReason)
                    ? $"잠금 이유: {data.LiveBlockedReason}"
                    : data.MotionGateDetail;

            var unlock = !string.IsNullOrWhiteSpace(data.MotionGateUnlockWhen)
                ? data.MotionGateUnlockWhen
                : data.MotionGateNextStep;

            return $"{reason} · {unlock}";
        }

        private static void SetValues(Label[] labels, string[] values)
        {
            if (labels == null || values == null)
            {
                return;
            }

            var count = Mathf.Min(labels.Length, values.Length);
            for (var index = 0; index < count; index++)
            {
                if (labels[index] != null)
                {
                    labels[index].text = values[index];
                }
            }
        }

        private static void SetCoordModeActive(Button button, bool active)
        {
            button?.EnableInClassList("rc-coord-mode-button--active", active);
        }

        private void BindCoordModeButtons()
        {
            if (coordStrip == null)
            {
                return;
            }

            coordStrip.BtnCoordModeJoint.clicked -= SetCoordModeJoint;
            coordStrip.BtnCoordModeTcp.clicked -= SetCoordModeTcp;
            coordStrip.BtnCoordModeBoth.clicked -= SetCoordModeBoth;
            coordStrip.BtnCoordModeJoint.clicked += SetCoordModeJoint;
            coordStrip.BtnCoordModeTcp.clicked += SetCoordModeTcp;
            coordStrip.BtnCoordModeBoth.clicked += SetCoordModeBoth;
        }

        private void SetCoordModeJoint()
        {
            coordStripMode = CoordStripMode.Joint;
            ApplyCoordStripMode();
        }

        private void SetCoordModeTcp()
        {
            coordStripMode = CoordStripMode.Tcp;
            ApplyCoordStripMode();
        }

        private void SetCoordModeBoth()
        {
            coordStripMode = CoordStripMode.Both;
            ApplyCoordStripMode();
        }

        private void ApplyCoordStripMode()
        {
            if (coordStrip == null)
            {
                return;
            }

            var showJoint = coordStripMode is CoordStripMode.Joint or CoordStripMode.Both;
            var showTcp = coordStripMode is CoordStripMode.Tcp or CoordStripMode.Both;
            coordStrip.JointTitle?.EnableInClassList("rc-hidden", !showJoint);
            coordStrip.JointGrid?.EnableInClassList("rc-hidden", !showJoint);
            coordStrip.TcpTitle?.EnableInClassList("rc-hidden", !showTcp);
            coordStrip.TcpGrid?.EnableInClassList("rc-hidden", !showTcp);
            SetCoordModeActive(coordStrip.BtnCoordModeJoint, coordStripMode == CoordStripMode.Joint);
            SetCoordModeActive(coordStrip.BtnCoordModeTcp, coordStripMode == CoordStripMode.Tcp);
            SetCoordModeActive(coordStrip.BtnCoordModeBoth, coordStripMode == CoordStripMode.Both);
        }

        private static void ApplyValueState(Label label, string className)
        {
            if (label == null)
            {
                return;
            }

            label.EnableInClassList("rc-status-value--default", className == "rc-status-value--default");
            label.EnableInClassList("rc-status-value--muted", className == "rc-status-value--muted");
            label.EnableInClassList("rc-status-value--success", className == "rc-status-value--success");
            label.EnableInClassList("rc-status-value--warning", className == "rc-status-value--warning");
            label.EnableInClassList("rc-status-value--danger", className == "rc-status-value--danger");
        }

        private void HandleFaultDetailClicked()
        {
            shellStateController?.SetDebugSelection("NavHelp", "TabEasyMotion", "BottomTabHelp");
        }

        private void HandleSafetyDetailClicked()
        {
            shellStateController?.SetDebugSelection("NavHelp", "TabEasyMotion", "BottomTabHelp");
        }

        private sealed class CoordStripElements
        {
            public CoordStripElements(VisualElement root)
            {
                CoordSystemBadge = root.Q<Label>("CoordSystemBadge");
                JointTitle = root.Q<Label>("CoordJointTitle");
                JointGrid = root.Q<VisualElement>("CoordJointGrid");
                TcpTitle = root.Q<Label>("CoordTcpTitle");
                TcpGrid = root.Q<VisualElement>("CoordTcpGrid");
                JointValues = new[]
                {
                    root.Q<Label>("JointValue1"),
                    root.Q<Label>("JointValue2"),
                    root.Q<Label>("JointValue3"),
                    root.Q<Label>("JointValue4"),
                    root.Q<Label>("JointValue5"),
                    root.Q<Label>("JointValue6"),
                };
                TcpValues = new[]
                {
                    root.Q<Label>("TcpValueX"),
                    root.Q<Label>("TcpValueY"),
                    root.Q<Label>("TcpValueZ"),
                    root.Q<Label>("TcpValueRx"),
                    root.Q<Label>("TcpValueRy"),
                    root.Q<Label>("TcpValueRz"),
                };
                BtnCoordModeJoint = root.Q<Button>("BtnCoordModeJoint");
                BtnCoordModeTcp = root.Q<Button>("BtnCoordModeTcp");
                BtnCoordModeBoth = root.Q<Button>("BtnCoordModeBoth");
            }

            public Label CoordSystemBadge { get; }
            public Label JointTitle { get; }
            public VisualElement JointGrid { get; }
            public Label TcpTitle { get; }
            public VisualElement TcpGrid { get; }
            public Label[] JointValues { get; }
            public Label[] TcpValues { get; }
            public Button BtnCoordModeJoint { get; }
            public Button BtnCoordModeTcp { get; }
            public Button BtnCoordModeBoth { get; }
        }

        private sealed class StatusCardElements
        {
            public StatusCardElements(VisualElement root)
            {
                StatusConnectionValue = root.Q<Label>("StatusConnectionValue");
                StatusModeValue = root.Q<Label>("StatusModeValue");
                StatusServoValue = root.Q<Label>("StatusServoValue");
                StatusMotionValue = root.Q<Label>("StatusMotionValue");
                StatusFaultValue = root.Q<Label>("StatusFaultValue");
                StatusSafetyValue = root.Q<Label>("StatusSafetyValue");
                StatusToolValue = root.Q<Label>("StatusToolValue");
                StatusUserValue = root.Q<Label>("StatusUserValue");
                StatusSpeedValue = root.Q<Label>("StatusSpeedValue");
                BtnFaultDetail = root.Q<Button>("BtnFaultDetail");
                BtnSafetyDetail = root.Q<Button>("BtnSafetyDetail");
            }

            public Label StatusConnectionValue { get; }
            public Label StatusModeValue { get; }
            public Label StatusServoValue { get; }
            public Label StatusMotionValue { get; }
            public Label StatusFaultValue { get; }
            public Label StatusSafetyValue { get; }
            public Label StatusToolValue { get; }
            public Label StatusUserValue { get; }
            public Label StatusSpeedValue { get; }
            public Button BtnFaultDetail { get; }
            public Button BtnSafetyDetail { get; }
        }

        private enum CoordStripMode
        {
            Joint,
            Tcp,
            Both,
        }
    }
}
