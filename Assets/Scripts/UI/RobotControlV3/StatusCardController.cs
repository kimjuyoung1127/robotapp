// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 우측 StatusCard와 CoordStrip 시안을 구성하고 preview state를 적용합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ConnectionHomeController))]
    public sealed class StatusCardController : MonoBehaviour
    {
        private enum ContextTabMode
        {
            Status,
            Coordinate
        }

        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset coordStripTemplate;
        [SerializeField] private VisualTreeAsset statusCardTemplate;

        private VisualElement root;
        private Button btnContextTabStatus;
        private Button btnContextTabCoordinate;
        private VisualElement coordStripHost;
        private VisualElement statusCardHost;
        private VisualElement safetyDiagnosticsHost;
        private VisualElement actionHintCard;
        private Label actionHintTitle;
        private Label actionHintPrimary;
        private Label actionHintSummary;
        private Label coordOverlayRowA;
        private Label coordOverlayRowB;
        private ConnectionHomeController connectionHomeController;
        private SafetyDiagnosticsController safetyDiagnosticsController;
        private WhyItMovedController whyItMovedController;
        private ContextTabMode activeContextTab = ContextTabMode.Status;
        private bool isCoordStripCollapsed;
        private EventCallback<ClickEvent> coordStripToggleCallback;
        private EventCallback<ClickEvent> contextTabStatusCallback;
        private EventCallback<ClickEvent> contextTabCoordinateCallback;

        private CoordStripElements coordStrip;
        private StatusCardElements statusCard;
        private bool isInitialized;
        private Coroutine initializeCoroutine;

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
            UnbindCoordStripToggle();
            if (initializeCoroutine != null)
            {
                StopCoroutine(initializeCoroutine);
                initializeCoroutine = null;
            }

            isInitialized = false;
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            root = document?.rootVisualElement;
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

            isInitialized = true;
            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            return true;
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            var summaryTitle = statusCard?.StatusSafetySummaryTitle?.text ?? "missing";
            var summaryBody = statusCard?.StatusSafetySummaryBody?.text ?? "missing";
            var statusVisible = !(statusCardHost?.ClassListContains("rc-hidden") ?? true);
            var coordVisible = !(coordStripHost?.ClassListContains("rc-hidden") ?? true);
            var actionVisible = !(actionHintCard?.ClassListContains("rc-hidden") ?? true);
            return $"initialized={isInitialized}; tab={activeContextTab}; coordCollapsed={isCoordStripCollapsed}; statusVisible={statusVisible}; coordVisible={coordVisible}; actionVisible={actionVisible}; summaryTitle={summaryTitle}; summaryBody={summaryBody}";
        }

        internal void RefreshFromBinder(PendantV3PreviewState.Definition data)
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
            btnContextTabStatus = root.Q<Button>("BtnContextTabStatus");
            btnContextTabCoordinate = root.Q<Button>("BtnContextTabCoordinate");
            safetyDiagnosticsHost = root.Q<VisualElement>("SafetyDiagnosticsHost");
            actionHintCard = root.Q<VisualElement>("ActionHint");
            actionHintTitle = root.Q<Label>("ActionHintTitle");
            actionHintPrimary = root.Q<Label>("ActionHintPrimary");
            actionHintSummary = root.Q<Label>("ActionHintSummary");
            coordOverlayRowA = root.Q<Label>("CoordOverlayRowA");
            coordOverlayRowB = root.Q<Label>("CoordOverlayRowB");
            safetyDiagnosticsController ??= GetComponent<SafetyDiagnosticsController>();
            whyItMovedController ??= GetComponent<WhyItMovedController>();
        }

        private void BuildPanels()
        {
            coordStrip = CreateCoordStrip(coordStripHost);
            statusCard = CreateStatusCard(statusCardHost);
            BindCoordStripToggle();
            BindContextTabs();
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

        private void ApplyPreview(PendantV3PreviewState.Definition data)
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

        private void ApplyCoordStrip(PendantV3PreviewState.Definition data)
        {
            if (coordStrip == null)
            {
                return;
            }

            coordStrip.CoordSystemBadge.text = $"좌표계: {data.CoordSystem}";
            SetValues(coordStrip.JointValues, data.JointValues);
            SetValues(coordStrip.TcpValues, data.TcpValues);
            SetCoordModeActive(coordStrip.BtnCoordModeJoint, false);
            SetCoordModeActive(coordStrip.BtnCoordModeTcp, false);
            SetCoordModeActive(coordStrip.BtnCoordModeBoth, true);
            ApplyCoordStripCollapsedState();
        }

        private void ApplyStatusCard(PendantV3PreviewState.Definition data)
        {
            if (statusCard == null)
            {
                return;
            }

            ApplySafetySummary(data);

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
            ApplyContextTabVisibility();
        }

        private void ApplySafetySummary(PendantV3PreviewState.Definition data)
        {
            if (statusCard?.StatusSafetySummary == null)
            {
                return;
            }

            var previewState = connectionHomeController.CurrentPreviewState;
            var session = connectionHomeController.CurrentSessionState;
            var isFault = previewState == PendantV3PreviewState.Kind.Fault;
            var isWarning = previewState is PendantV3PreviewState.Kind.ConnectedUnsynced or PendantV3PreviewState.Kind.AutoReconnect;
            var title = session.ReconnectFailed
                ? "수동 연결 필요"
                : !session.ActualMoveAllowed
                ? "실기 이동 잠금"
                : isFault
                ? "Fault 복구 우선"
                : isWarning
                    ? "안전 확인 우선"
                    : "정상 대기";
            var body = session.ReconnectActive
                ? $"지금은 {Mathf.CeilToInt(session.ReconnectSecondsUntilRetry)}초 뒤 자동 재시도를 기다리는 상태다. 조작보다 복귀 여부를 먼저 본다."
                : session.ReconnectFailed
                    ? (string.IsNullOrWhiteSpace(session.ReconnectFailureSummary)
                        ? "자동 재연결이 끝까지 실패했다. 수동 연결로 다시 복귀하는 게 먼저다."
                        : session.ReconnectFailureSummary)
                : !session.ActualMoveAllowed
                    ? session.ActualMoveBlockReason
                : isFault
                ? $"{data.ActionPrimary} 전에 오류 코드와 Safety 상태를 먼저 확인해라."
                : isWarning
                    ? $"{data.ActionWhy} 지금은 조작보다 동기화/재연결 확인이 먼저다."
                    : "오른쪽 상세 진단 카드는 숨기고, 상태 요약만 유지한 채 작업을 이어간다.";

            statusCard.StatusSafetySummaryTitle.text = title;
            statusCard.StatusSafetySummaryBody.text = body;
            statusCard.StatusSafetySummary.EnableInClassList("rc-status-summary-card--safe", !isFault && !isWarning);
            statusCard.StatusSafetySummary.EnableInClassList("rc-status-summary-card--warning", !isFault && isWarning);
            statusCard.StatusSafetySummary.EnableInClassList("rc-status-summary-card--danger", isFault);
        }

        private void ApplyContextGuidance(PendantV3PreviewState.Definition data)
        {
            if (actionHintTitle != null)
            {
                actionHintTitle.text = "다음 행동 추천";
            }

            if (actionHintPrimary != null)
            {
                actionHintPrimary.text = data.ActionPrimary;
            }

            if (actionHintSummary != null)
            {
                actionHintSummary.text = connectionHomeController.CurrentSessionState.ReconnectActive
                    ? $"재시도 {connectionHomeController.CurrentSessionState.ReconnectAttempt}/{connectionHomeController.CurrentSessionState.ReconnectAttemptMax} · {Mathf.CeilToInt(connectionHomeController.CurrentSessionState.ReconnectSecondsUntilRetry)}초 뒤 다시 연결을 시도한다."
                    : data.ActionWhy;
            }

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

        private void BindCoordStripToggle()
        {
            if (coordStrip?.BtnCoordStripToggle == null)
            {
                return;
            }

            coordStripToggleCallback ??= _ => ToggleCoordStripCollapsed();
            coordStrip.BtnCoordStripToggle.UnregisterCallback(coordStripToggleCallback);
            coordStrip.BtnCoordStripToggle.RegisterCallback(coordStripToggleCallback);
        }

        private void BindContextTabs()
        {
            if (btnContextTabStatus == null || btnContextTabCoordinate == null)
            {
                return;
            }

            contextTabStatusCallback ??= _ => SetContextTab(ContextTabMode.Status);
            contextTabCoordinateCallback ??= _ => SetContextTab(ContextTabMode.Coordinate);
            btnContextTabStatus.UnregisterCallback(contextTabStatusCallback);
            btnContextTabCoordinate.UnregisterCallback(contextTabCoordinateCallback);
            btnContextTabStatus.RegisterCallback(contextTabStatusCallback);
            btnContextTabCoordinate.RegisterCallback(contextTabCoordinateCallback);
        }

        private void UnbindCoordStripToggle()
        {
            if (coordStrip?.BtnCoordStripToggle == null || coordStripToggleCallback == null)
            {
                return;
            }

            coordStrip.BtnCoordStripToggle.UnregisterCallback(coordStripToggleCallback);
            if (btnContextTabStatus != null && contextTabStatusCallback != null)
            {
                btnContextTabStatus.UnregisterCallback(contextTabStatusCallback);
            }

            if (btnContextTabCoordinate != null && contextTabCoordinateCallback != null)
            {
                btnContextTabCoordinate.UnregisterCallback(contextTabCoordinateCallback);
            }
        }

        private void ToggleCoordStripCollapsed()
        {
            isCoordStripCollapsed = !isCoordStripCollapsed;
            ApplyCoordStripCollapsedState();
        }

        private void SetContextTab(ContextTabMode mode)
        {
            activeContextTab = mode;
            ApplyContextTabVisibility();
        }

        private void ApplyCoordStripCollapsedState()
        {
            if (coordStrip == null)
            {
                return;
            }

            coordStrip.Root.EnableInClassList("rc-coord-strip-root--collapsed", isCoordStripCollapsed);
            if (coordStrip.BtnCoordStripToggle != null)
            {
                coordStrip.BtnCoordStripToggle.text = isCoordStripCollapsed ? "펼치기" : "접기";
            }
        }

        private void ApplyContextTabVisibility()
        {
            var isStatusTab = activeContextTab == ContextTabMode.Status;
            btnContextTabStatus?.EnableInClassList("rc-context-tab--active", isStatusTab);
            btnContextTabCoordinate?.EnableInClassList("rc-context-tab--active", !isStatusTab);
            statusCardHost?.EnableInClassList("rc-hidden", !isStatusTab);
            coordStripHost?.EnableInClassList("rc-hidden", isStatusTab);
            actionHintCard?.EnableInClassList("rc-hidden", !isStatusTab);
            safetyDiagnosticsHost?.EnableInClassList("rc-hidden", !isStatusTab);
            safetyDiagnosticsController?.SetContextVisible(isStatusTab);
            whyItMovedController?.SetContextVisible(!isStatusTab);
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

        private sealed class CoordStripElements
        {
            public CoordStripElements(VisualElement root)
            {
                Root = root;
                CoordSystemBadge = root.Q<Label>("CoordSystemBadge");
                CoordStripBody = root.Q<VisualElement>("CoordStripBody");
                BtnCoordStripToggle = root.Q<Button>("BtnCoordStripToggle");
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

            public VisualElement Root { get; }
            public Label CoordSystemBadge { get; }
            public VisualElement CoordStripBody { get; }
            public Button BtnCoordStripToggle { get; }
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
                StatusSafetySummary = root.Q<VisualElement>("StatusSafetySummary");
                StatusSafetySummaryTitle = root.Q<Label>("StatusSafetySummaryTitle");
                StatusSafetySummaryBody = root.Q<Label>("StatusSafetySummaryBody");
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

            public VisualElement StatusSafetySummary { get; }
            public Label StatusSafetySummaryTitle { get; }
            public Label StatusSafetySummaryBody { get; }
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
    }
}
