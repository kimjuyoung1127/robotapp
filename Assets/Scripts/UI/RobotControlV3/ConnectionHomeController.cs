// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections.Generic;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 연결 홈 패널과 상태 프리셋 시안을 제어합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed partial class ConnectionHomeController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset connectionHomeTemplate;

        internal event System.Action<RobotControlV3RuntimeSnapshot> PreviewChanged;

        private readonly List<(Button button, System.Action clicked)> presetButtons = new();

        private VisualElement root;
        private VisualElement workTabBar;
        private VisualElement workPanelBody;
        private VisualElement bottomTabBar;
        private VisualElement bottomSheetBody;
        private VisualElement homePanelHost;
        private VisualElement homeSheetHost;
        private Label workPanelTitle;
        private Label workPanelSummary;
        private Label bottomSheetTitle;
        private Label bottomSheetSummary;
        private Label robotNameLabel;
        private Label connectionIndicator;
        private Label modeLabel;
        private Label speedLabel;
        private Label coordSystemLabel;
        private Label toolLabel;
        private Label userLabel;
        private Label safetyLabel;
        private Label faultLabel;
        private Label quickControllerModeLabel;
        private Label quickLiveArmLabel;
        private Label headerNextActionLabel;
        private Button btnServoEnable;
        private Button btnHeaderModeAuto;
        private Button btnHeaderModeManual;
        private Button btnRun;
        private Button btnStop;
        private Button btnPause;
        private Button btnSync;
        private Button btnResetError;
        private Button btnPopupProbe;
        private Button btnRunBottom;
        private Button btnStopBottom;
        private System.Action connectClickedAction;
        private System.Action disconnectClickedAction;
        private System.Action primaryClickedAction;
        private System.Action servoClickedAction;
        private System.Action runClickedAction;
        private System.Action stopClickedAction;
        private System.Action resetClickedAction;
        private System.Action pauseClickedAction;
        private System.Action autoModeClickedAction;
        private System.Action manualModeClickedAction;
        private System.Action syncClickedAction;
        private RobotControlV3RuntimeController runtimeController;

        private PanelElements desktopPanel;
        private PanelElements tabletPanel;
        private PendantV3PreviewState.Kind previewState = PendantV3PreviewState.Kind.ConnectedServoOff;
        private RobotControlV3RuntimeSnapshot debugOverrideSnapshot;
        private bool isHomeActive;
        private bool isPointsActive;
        private bool isInitialized;
        private bool isInitializing;
        private bool isApplyingPreview;
        private RobotControlV3RuntimeSnapshot queuedPreview;
        private Coroutine initializeCoroutine;

        internal PendantV3PreviewState.Kind CurrentPreviewState => previewState;
        internal RobotControlV3RuntimeSnapshot CurrentPreviewDefinition
            => debugOverrideSnapshot
            ?? (runtimeController != null && runtimeController.IsInitialized
                ? runtimeController.CurrentSnapshot
                : CreateFallbackSnapshot(previewState));

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
            UnbindPresetButtons();
            UnbindRuntimeButtons();
            if (runtimeController != null)
            {
                runtimeController.SnapshotChanged -= HandleRuntimeSnapshotChanged;
            }
            if (initializeCoroutine != null)
            {
                StopCoroutine(initializeCoroutine);
                initializeCoroutine = null;
            }

            isInitialized = false;
            isApplyingPreview = false;
            queuedPreview = null;
        }

        public void SetShellState(string activeNavSection, string activeWorkTab, string activeTabletTab)
        {
            isHomeActive = activeNavSection == "NavHome";
            isPointsActive = activeNavSection == "NavPoints";
            if (!isInitialized)
            {
                TryInitialize();
            }

            ApplyHomeVisibility();
            ApplyPreview(CurrentPreviewDefinition);
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        internal string SetPreviewStateForDebug(string stateName)
        {
            if (!isInitialized && !TryInitialize())
            {
                return GetDebugSummary();
            }

            var state = stateName switch
            {
                "Disconnected" => PendantV3PreviewState.Kind.Disconnected,
                "ServoOff" => PendantV3PreviewState.Kind.ConnectedServoOff,
                "ConnectedServoOff" => PendantV3PreviewState.Kind.ConnectedServoOff,
                "Unsynced" => PendantV3PreviewState.Kind.ConnectedUnsynced,
                "ConnectedUnsynced" => PendantV3PreviewState.Kind.ConnectedUnsynced,
                "Ready" => PendantV3PreviewState.Kind.ReadyToJog,
                "ReadyToJog" => PendantV3PreviewState.Kind.ReadyToJog,
                "Fault" => PendantV3PreviewState.Kind.Fault,
                "AutoReconnect" => PendantV3PreviewState.Kind.AutoReconnect,
                _ => previewState,
            };
            SetPreviewState(state);
            return GetDebugSummary();
        }

        public string GetDebugSummary()
        {
            var liveRoot = document != null ? document.rootVisualElement : null;
            var livePanelHost = liveRoot?.Q<VisualElement>("HomePanelHost");
            var liveSheetHost = liveRoot?.Q<VisualElement>("HomeSheetHost");
            var panelChildren = homePanelHost?.childCount ?? -1;
            var sheetChildren = homeSheetHost?.childCount ?? -1;
            var panelHidden = homePanelHost?.ClassListContains("rc-hidden") ?? false;
            var sheetHidden = homeSheetHost?.ClassListContains("rc-hidden") ?? false;
            var templateName = connectionHomeTemplate != null ? connectionHomeTemplate.name : "null";
            return $"initialized={isInitialized}; template={templateName}; document={(document != null)}; liveRoot={(liveRoot != null)}; livePanelHost={(livePanelHost != null)}; liveSheetHost={(liveSheetHost != null)}; isHomeActive={isHomeActive}; panelChildren={panelChildren}; panelHidden={panelHidden}; sheetChildren={sheetChildren}; sheetHidden={sheetHidden}; previewState={previewState}";
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
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            root = document?.rootVisualElement;
            try
            {
                if (root == null || connectionHomeTemplate == null || runtimeController == null)
                {
                    return false;
                }

                CacheShellElements();
                if (homePanelHost == null || homeSheetHost == null)
                {
                    isInitialized = false;
                    return false;
                }

                if (desktopPanel == null || tabletPanel == null || homePanelHost.childCount == 0 || homeSheetHost.childCount == 0)
                {
                    BuildPanels();
                }

                runtimeController.SnapshotChanged -= HandleRuntimeSnapshotChanged;
                runtimeController.SnapshotChanged += HandleRuntimeSnapshotChanged;
                UnbindRuntimeButtons();
                BindRuntimeButtons();
                ApplyShellStateSnapshot();
                debugOverrideSnapshot = null;
                isInitialized = true;
                ApplyPreview(CurrentPreviewDefinition ?? CreateFallbackSnapshot(previewState));
                ApplyHomeVisibility();
                return true;
            }
            finally
            {
                isInitializing = false;
            }
        }

        private void ApplyShellStateSnapshot()
        {
            var shellStateController = GetComponent<PendantV3ShellStateController>();
            var localState = shellStateController != null
                ? shellStateController.GetStateSnapshot()
                : PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault());
            isHomeActive = localState.ActiveNavSection == "NavHome";
            isPointsActive = localState.ActiveNavSection == "NavPoints";
        }

        private void CacheShellElements()
        {
            workTabBar = root.Q<VisualElement>("WorkTabBar");
            workPanelBody = root.Q<VisualElement>("WorkPanelBody");
            bottomTabBar = root.Q<VisualElement>("BottomTabBar");
            bottomSheetBody = root.Q<VisualElement>("BottomSheetBody");
            homePanelHost = root.Q<VisualElement>("HomePanelHost");
            homeSheetHost = root.Q<VisualElement>("HomeSheetHost");
            workPanelTitle = root.Q<Label>("WorkPanelTitle");
            workPanelSummary = root.Q<Label>("WorkPanelSummary");
            bottomSheetTitle = root.Q<Label>("BottomSheetTitle");
            bottomSheetSummary = root.Q<Label>("BottomSheetSummary");
            robotNameLabel = root.Q<Label>("RobotNameLabel");
            connectionIndicator = root.Q<Label>("ConnectionIndicator");
            modeLabel = root.Q<Label>("ModeLabel");
            speedLabel = root.Q<Label>("SpeedLabel");
            coordSystemLabel = root.Q<Label>("CoordSystemLabel");
            toolLabel = root.Q<Label>("ToolLabel");
            userLabel = root.Q<Label>("UserLabel");
            safetyLabel = root.Q<Label>("SafetyLabel");
            faultLabel = root.Q<Label>("FaultLabel");
            quickControllerModeLabel = root.Q<Label>("QuickControllerMode");
            quickLiveArmLabel = root.Q<Label>("QuickLiveArm");
            headerNextActionLabel = root.Q<Label>("HeaderNextActionLabel");
            btnServoEnable = root.Q<Button>("BtnServoEnable");
            btnHeaderModeAuto = root.Q<Button>("BtnHeaderModeAuto");
            btnHeaderModeManual = root.Q<Button>("BtnHeaderModeManual");
            btnRun = root.Q<Button>("BtnRun");
            btnStop = root.Q<Button>("BtnStop");
            btnPause = root.Q<Button>("BtnPause");
            btnSync = root.Q<Button>("BtnSync");
            btnResetError = root.Q<Button>("BtnResetError");
            btnPopupProbe = root.Q<Button>("BtnPopupProbe");
            btnRunBottom = root.Q<Button>("BtnRunBottom");
            btnStopBottom = root.Q<Button>("BtnStopBottom");
            ApplyTopHeaderVisibility();
        }

        private void BuildPanels()
        {
            UnbindPresetButtons();
            desktopPanel = CreatePanel(homePanelHost);
            tabletPanel = CreatePanel(homeSheetHost);
            ApplyPreview(CurrentPreviewDefinition);
        }

        private PanelElements CreatePanel(VisualElement host)
        {
            if (host == null)
            {
                return null;
            }

            host.Clear();
            var tree = connectionHomeTemplate.CloneTree();
            host.Add(tree);

            var panel = new PanelElements(tree);
            RegisterPresetRow(panel);
            return panel;
        }

        private void RegisterPresetRow(PanelElements panel)
        {
            BindPreset(panel.BtnPresetDisconnected, PendantV3PreviewState.Kind.Disconnected);
            BindPreset(panel.BtnPresetServoOff, PendantV3PreviewState.Kind.ConnectedServoOff);
            BindPreset(panel.BtnPresetUnsynced, PendantV3PreviewState.Kind.ConnectedUnsynced);
            BindPreset(panel.BtnPresetReady, PendantV3PreviewState.Kind.ReadyToJog);
            BindPreset(panel.BtnPresetFault, PendantV3PreviewState.Kind.Fault);
            BindPreset(panel.BtnPresetReconnect, PendantV3PreviewState.Kind.AutoReconnect);
        }

        private void BindPreset(Button button, PendantV3PreviewState.Kind state)
        {
            if (button == null)
            {
                return;
            }

            System.Action clicked = () => SetPreviewState(state);
            button.clicked += clicked;
            presetButtons.Add((button, clicked));
        }

        private void UnbindPresetButtons()
        {
            foreach (var (button, clicked) in presetButtons)
            {
                button.clicked -= clicked;
            }

            presetButtons.Clear();
        }

        private void SetPreviewState(PendantV3PreviewState.Kind state)
        {
            previewState = state;
            debugOverrideSnapshot = CreateFallbackSnapshot(state);
            ApplyPreview(debugOverrideSnapshot);
        }

        private void ApplyPreview(RobotControlV3RuntimeSnapshot data)
        {
            if (data == null)
            {
                return;
            }

            if (isApplyingPreview)
            {
                queuedPreview = data.Clone();
                return;
            }

            isApplyingPreview = true;
            try
            {
                var current = data;
                do
                {
                    queuedPreview = null;
                    previewState = MapStatusKind(current.StatusKind);
                    ApplyTopStatusBar(current);
                    ApplyPanel(desktopPanel, current);
                    ApplyPanel(tabletPanel, current);
                    PreviewChanged?.Invoke(current);
                    current = queuedPreview;
                }
                while (current != null);
            }
            finally
            {
                isApplyingPreview = false;
            }
        }

        private void ApplyTopStatusBar(RobotControlV3RuntimeSnapshot data)
        {
            if (robotNameLabel != null)
            {
                robotNameLabel.text = data.RobotTitle;
            }

            ApplyChip(connectionIndicator, data.ConnectionChip, data.ConnectionClass);
            ApplyChip(modeLabel, data.ModeChip, data.ModeClass);
            ApplyChip(speedLabel, data.SpeedChip, data.SpeedClass);
            ApplyChip(coordSystemLabel, data.CoordChip, "rc-status-chip--muted");
            ApplyChip(toolLabel, data.ToolChip, "rc-status-chip--muted");
            ApplyChip(userLabel, data.UserChip, "rc-status-chip--muted");
            ApplyChip(safetyLabel, data.SafetyChip, data.SafetyClass);
            ApplyChip(faultLabel, data.FaultChip, data.FaultClass);
            ApplyChip(quickControllerModeLabel, data.QuickControllerMode, "rc-status-chip--muted");
            ApplyChip(quickLiveArmLabel, data.QuickLiveArm, "rc-status-chip--muted");
            if (headerNextActionLabel != null)
            {
                headerNextActionLabel.text = data.HeaderNextAction;
            }

            btnServoEnable?.SetEnabled(data.ServoEnabled);
            btnHeaderModeAuto?.SetEnabled(data.AutoModeSwitchEnabled);
            btnHeaderModeManual?.SetEnabled(data.ManualModeSwitchEnabled);
            btnRun?.SetEnabled(data.RunEnabled);
            btnStop?.SetEnabled(data.StopEnabled);
            btnPause?.SetEnabled(data.PauseEnabled);
            btnSync?.SetEnabled(data.SyncEnabled);
            btnResetError?.SetEnabled(data.ResetEnabled);
            ApplyTopHeaderVisibility();
        }

        private void ApplyTopHeaderVisibility()
        {
            SetTopHeaderButtonVisible(btnServoEnable, true);
            SetTopHeaderButtonVisible(btnHeaderModeAuto, true);
            SetTopHeaderButtonVisible(btnHeaderModeManual, true);
            SetTopHeaderButtonVisible(btnRun, false);
            SetTopHeaderButtonVisible(btnStop, false);
            SetTopHeaderButtonVisible(btnPause, false);
            SetTopHeaderButtonVisible(btnSync, false);
            SetTopHeaderButtonVisible(btnResetError, false);
            SetTopHeaderButtonVisible(btnPopupProbe, false);
        }

        private static void SetTopHeaderButtonVisible(Button button, bool visible)
        {
            if (button == null)
            {
                return;
            }

            button.SetEnabled(visible);
            button.EnableInClassList("rc-hidden", !visible);
            button.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ApplyPanel(PanelElements panel, RobotControlV3RuntimeSnapshot data)
        {
            if (panel == null)
            {
                return;
            }

            panel.ConnectionRobot.text = data.RobotTitle;
            panel.ConnectionIp.text = data.IpAddress;
            panel.ConnectionStatus.text = data.ConnectionCardStatus;
            panel.BtnConnect.SetEnabled(data.ConnectEnabled);
            panel.BtnDisconnect.SetEnabled(data.DisconnectEnabled);

            panel.QuickServo.text = data.QuickServo;
            panel.QuickMode.text = data.QuickMode;
            panel.QuickSync.text = data.QuickSync;
            panel.QuickControllerMode.text = data.QuickControllerMode;
            panel.QuickLiveArm.text = data.QuickLiveArm;
            panel.BtnModeAuto?.SetEnabled(data.AutoModeSwitchEnabled);
            panel.BtnModeManual?.SetEnabled(data.ManualModeSwitchEnabled);
            panel.BtnQuickAction.text = data.QuickActionLabel;
            panel.BtnQuickAction.SetEnabled(data.QuickActionEnabled);

            panel.ActionNow.text = data.ActionNow;
            panel.ActionPrimary.text = $"다음 행동: {data.OperatorNextAction}";
            panel.ActionWhy.text = $"분류: {data.FailureCategory} · {BuildGateSummary(data)}";
            panel.BtnPrimaryAction.text = data.PrimaryActionLabel;
            panel.BtnPrimaryAction.SetEnabled(data.PrimaryActionEnabled);
            SetPresetActive(panel.BtnPresetDisconnected, previewState == PendantV3PreviewState.Kind.Disconnected);
            SetPresetActive(panel.BtnPresetServoOff, previewState == PendantV3PreviewState.Kind.ConnectedServoOff);
            SetPresetActive(panel.BtnPresetUnsynced, previewState == PendantV3PreviewState.Kind.ConnectedUnsynced);
            SetPresetActive(panel.BtnPresetReady, previewState == PendantV3PreviewState.Kind.ReadyToJog);
            SetPresetActive(panel.BtnPresetFault, previewState == PendantV3PreviewState.Kind.Fault);
            SetPresetActive(panel.BtnPresetReconnect, previewState == PendantV3PreviewState.Kind.AutoReconnect);
        }

        private void ApplyHomeVisibility()
        {
            // WorkPanelBody / BottomSheetBody are shared shells for Home and Motion tabs.
            // Only panel hosts should toggle here; shared bodies must remain available.
            workPanelBody?.EnableInClassList("rc-hidden", false);
            bottomSheetBody?.EnableInClassList("rc-hidden", false);
            homePanelHost?.EnableInClassList("rc-hidden", !isHomeActive);
            homeSheetHost?.EnableInClassList("rc-hidden", !isHomeActive);
            workTabBar?.EnableInClassList("rc-hidden", isHomeActive || isPointsActive);
            bottomTabBar?.EnableInClassList("rc-hidden", isHomeActive);

            workPanelSummary?.EnableInClassList("rc-hidden", false);

            if (bottomSheetTitle != null)
            {
                bottomSheetTitle.text = isHomeActive ? "BottomSheet · 연결 홈" : "BottomSheet";
            }

            if (bottomSheetSummary != null)
            {
                bottomSheetSummary.EnableInClassList("rc-hidden", isHomeActive);
            }
        }

        private static void ApplyChip(Label label, string text, string className)
        {
            if (label == null)
            {
                return;
            }

            label.text = text;
            label.EnableInClassList("rc-status-chip--muted", className == "rc-status-chip--muted");
            label.EnableInClassList("rc-status-chip--success", className == "rc-status-chip--success");
            label.EnableInClassList("rc-status-chip--warning", className == "rc-status-chip--warning");
            label.EnableInClassList("rc-status-chip--danger", className == "rc-status-chip--danger");
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

        private static void SetPresetActive(Button button, bool active)
        {
            button?.EnableInClassList("rc-home-state-button--active", active);
        }

        private void BindRuntimeButtons()
        {
            connectClickedAction ??= HandleConnectClicked;
            disconnectClickedAction ??= HandleDisconnectClicked;
            primaryClickedAction ??= HandlePrimaryActionClicked;
            servoClickedAction ??= HandleServoClicked;
            runClickedAction ??= HandleRunClicked;
            stopClickedAction ??= HandleStopClicked;
            resetClickedAction ??= HandleResetClicked;
            pauseClickedAction ??= HandlePauseClicked;
            syncClickedAction ??= HandleSyncClicked;
            autoModeClickedAction ??= HandleAutoModeClicked;
            manualModeClickedAction ??= HandleManualModeClicked;
            if (btnServoEnable != null)
            {
                btnServoEnable.clicked += servoClickedAction;
            }

            if (btnRun != null)
            {
                btnRun.clicked += runClickedAction;
            }

            if (btnStop != null)
            {
                btnStop.clicked += stopClickedAction;
            }

            if (btnRunBottom != null)
            {
                btnRunBottom.clicked += runClickedAction;
            }

            if (btnHeaderModeAuto != null)
            {
                btnHeaderModeAuto.clicked += autoModeClickedAction;
            }

            if (btnHeaderModeManual != null)
            {
                btnHeaderModeManual.clicked += manualModeClickedAction;
            }

            if (btnStopBottom != null)
            {
                btnStopBottom.clicked += stopClickedAction;
            }

            if (btnPause != null)
            {
                btnPause.clicked += pauseClickedAction;
            }

            if (btnSync != null)
            {
                btnSync.clicked += syncClickedAction;
            }

            if (btnResetError != null)
            {
                btnResetError.clicked += resetClickedAction;
            }

            if (desktopPanel != null)
            {
                desktopPanel.BtnConnect.clicked += connectClickedAction;
                desktopPanel.BtnDisconnect.clicked += disconnectClickedAction;
                if (desktopPanel.BtnModeAuto != null)
                {
                    desktopPanel.BtnModeAuto.clicked += autoModeClickedAction;
                }
                if (desktopPanel.BtnModeManual != null)
                {
                    desktopPanel.BtnModeManual.clicked += manualModeClickedAction;
                }
                desktopPanel.BtnQuickAction.clicked += primaryClickedAction;
                desktopPanel.BtnPrimaryAction.clicked += primaryClickedAction;
            }

            if (tabletPanel != null)
            {
                tabletPanel.BtnConnect.clicked += connectClickedAction;
                tabletPanel.BtnDisconnect.clicked += disconnectClickedAction;
                if (tabletPanel.BtnModeAuto != null)
                {
                    tabletPanel.BtnModeAuto.clicked += autoModeClickedAction;
                }
                if (tabletPanel.BtnModeManual != null)
                {
                    tabletPanel.BtnModeManual.clicked += manualModeClickedAction;
                }
                tabletPanel.BtnQuickAction.clicked += primaryClickedAction;
                tabletPanel.BtnPrimaryAction.clicked += primaryClickedAction;
            }
        }

        private void UnbindRuntimeButtons()
        {
            if (btnServoEnable != null && servoClickedAction != null)
            {
                btnServoEnable.clicked -= servoClickedAction;
            }

            if (btnRun != null && runClickedAction != null)
            {
                btnRun.clicked -= runClickedAction;
            }

            if (btnStop != null && stopClickedAction != null)
            {
                btnStop.clicked -= stopClickedAction;
            }

            if (btnRunBottom != null && runClickedAction != null)
            {
                btnRunBottom.clicked -= runClickedAction;
            }

            if (btnHeaderModeAuto != null)
            {
                if (autoModeClickedAction != null)
                {
                    btnHeaderModeAuto.clicked -= autoModeClickedAction;
                }
            }

            if (btnHeaderModeManual != null)
            {
                if (manualModeClickedAction != null)
                {
                    btnHeaderModeManual.clicked -= manualModeClickedAction;
                }
            }

            if (btnStopBottom != null && stopClickedAction != null)
            {
                btnStopBottom.clicked -= stopClickedAction;
            }

            if (btnPause != null)
            {
                if (pauseClickedAction != null)
                {
                    btnPause.clicked -= pauseClickedAction;
                }
            }

            if (btnSync != null)
            {
                if (syncClickedAction != null)
                {
                    btnSync.clicked -= syncClickedAction;
                }
            }

            if (btnResetError != null && resetClickedAction != null)
            {
                btnResetError.clicked -= resetClickedAction;
            }

            if (desktopPanel != null)
            {
                UnregisterPanelCallbacks(desktopPanel);
            }

            if (tabletPanel != null)
            {
                UnregisterPanelCallbacks(tabletPanel);
            }
        }

        private void UnregisterPanelCallbacks(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            if (connectClickedAction != null)
            {
                panel.BtnConnect.clicked -= connectClickedAction;
            }

            if (disconnectClickedAction != null)
            {
                panel.BtnDisconnect.clicked -= disconnectClickedAction;
            }

            if (autoModeClickedAction != null)
            {
                if (panel.BtnModeAuto != null)
                {
                    panel.BtnModeAuto.clicked -= autoModeClickedAction;
                }
            }

            if (manualModeClickedAction != null)
            {
                if (panel.BtnModeManual != null)
                {
                    panel.BtnModeManual.clicked -= manualModeClickedAction;
                }
            }

            if (primaryClickedAction != null)
            {
                panel.BtnQuickAction.clicked -= primaryClickedAction;
                panel.BtnPrimaryAction.clicked -= primaryClickedAction;
            }
        }

        private void HandleRuntimeSnapshotChanged(RobotControlV3RuntimeSnapshot data)
        {
            debugOverrideSnapshot = null;
            ApplyPreview(data);
        }

        private void HandleConnectClicked()
        {
            debugOverrideSnapshot = null;
            runtimeController?.ConnectAndSyncDefaultAsync();
        }

        private void HandleDisconnectClicked()
        {
            debugOverrideSnapshot = null;
            runtimeController?.Disconnect();
        }

        private void HandlePrimaryActionClicked()
        {
            debugOverrideSnapshot = null;
            runtimeController?.ExecutePrimaryAction();
        }

        private void HandleServoClicked()
        {
            debugOverrideSnapshot = null;
            runtimeController?.EnableServo();
        }

        private void HandleRunClicked()
        {
            debugOverrideSnapshot = null;
            runtimeController?.ExecutePrimaryAction();
        }

        private void HandleStopClicked()
        {
            debugOverrideSnapshot = null;
            runtimeController?.StopMotion();
        }

        private void HandlePauseClicked()
        {
            debugOverrideSnapshot = null;
            runtimeController?.TogglePause();
        }

        private void HandleSyncClicked()
        {
            debugOverrideSnapshot = null;
            runtimeController?.SyncCurrentStateAsync();
        }

        private void HandleAutoModeClicked()
        {
            debugOverrideSnapshot = null;
            runtimeController?.RequestAutoMode();
        }

        private void HandleManualModeClicked()
        {
            debugOverrideSnapshot = null;
            runtimeController?.RequestManualMode();
        }

        private void HandleResetClicked()
        {
            debugOverrideSnapshot = null;
            runtimeController?.ResetErrors();
        }

        private static PendantV3PreviewState.Kind MapStatusKind(RobotControlV3RuntimeStatusKind statusKind)
        {
            return statusKind switch
            {
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => PendantV3PreviewState.Kind.ConnectedServoOff,
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => PendantV3PreviewState.Kind.ConnectedUnsynced,
                RobotControlV3RuntimeStatusKind.ReadyToJog => PendantV3PreviewState.Kind.ReadyToJog,
                RobotControlV3RuntimeStatusKind.Fault => PendantV3PreviewState.Kind.Fault,
                RobotControlV3RuntimeStatusKind.AutoReconnect => PendantV3PreviewState.Kind.AutoReconnect,
                _ => PendantV3PreviewState.Kind.Disconnected,
            };
        }

        private static RobotControlV3RuntimeSnapshot CreateFallbackSnapshot(PendantV3PreviewState.Kind kind)
        {
            var definition = PendantV3PreviewState.GetDefinition(kind);
            return new RobotControlV3RuntimeSnapshot
            {
                StatusKind = kind switch
                {
                    PendantV3PreviewState.Kind.ConnectedServoOff => RobotControlV3RuntimeStatusKind.ConnectedServoOff,
                    PendantV3PreviewState.Kind.ConnectedUnsynced => RobotControlV3RuntimeStatusKind.ConnectedUnsynced,
                    PendantV3PreviewState.Kind.ReadyToJog => RobotControlV3RuntimeStatusKind.ReadyToJog,
                    PendantV3PreviewState.Kind.Fault => RobotControlV3RuntimeStatusKind.Fault,
                    PendantV3PreviewState.Kind.AutoReconnect => RobotControlV3RuntimeStatusKind.AutoReconnect,
                    _ => RobotControlV3RuntimeStatusKind.Disconnected,
                },
                RobotTitle = definition.RobotTitle,
                IpAddress = definition.IpAddress,
                ConnectionCardStatus = definition.ConnectionCardStatus,
                QuickServo = definition.QuickServo,
                QuickMode = definition.QuickMode,
                QuickSync = definition.QuickSync,
                QuickActionLabel = definition.QuickActionLabel,
                QuickActionEnabled = definition.QuickActionEnabled,
                ConnectEnabled = definition.ConnectEnabled,
                DisconnectEnabled = definition.DisconnectEnabled,
                ActionNow = definition.ActionNow,
                ActionPrimary = definition.ActionPrimary,
                ActionWhy = definition.ActionWhy,
                PrimaryActionLabel = definition.PrimaryActionLabel,
                PrimaryActionEnabled = definition.PrimaryActionEnabled,
                ConnectionChip = definition.ConnectionChip,
                ModeChip = definition.ModeChip,
                SpeedChip = definition.SpeedChip,
                CoordChip = definition.CoordChip,
                SafetyChip = definition.SafetyChip,
                FaultChip = definition.FaultChip,
                ToolChip = definition.ToolChip,
                UserChip = definition.UserChip,
                ConnectionClass = definition.ConnectionClass,
                ModeClass = definition.ModeClass,
                SpeedClass = definition.SpeedClass,
                SafetyClass = definition.SafetyClass,
                FaultClass = definition.FaultClass,
                ServoEnabled = definition.ServoEnabled,
                RunEnabled = definition.RunEnabled,
                StopEnabled = definition.StopEnabled,
                PauseEnabled = definition.PauseEnabled,
                SyncEnabled = definition.SyncEnabled,
                ResetEnabled = definition.ResetEnabled,
                StatusConnection = definition.StatusConnection,
                StatusMode = definition.StatusMode,
                StatusServo = definition.StatusServo,
                StatusMotion = definition.StatusMotion,
                StatusFault = definition.StatusFault,
                StatusSafety = definition.StatusSafety,
                StatusTool = definition.StatusTool,
                StatusUser = definition.StatusUser,
                StatusSpeed = definition.StatusSpeed,
                StatusConnectionClass = definition.StatusConnectionClass,
                StatusModeClass = definition.StatusModeClass,
                StatusServoClass = definition.StatusServoClass,
                StatusMotionClass = definition.StatusMotionClass,
                StatusFaultClass = definition.StatusFaultClass,
                StatusSafetyClass = definition.StatusSafetyClass,
                FaultDetailEnabled = definition.FaultDetailEnabled,
                SafetyDetailEnabled = definition.SafetyDetailEnabled,
                CoordSystem = definition.CoordSystem,
                JointValues = (string[])definition.JointValues.Clone(),
                TcpValues = (string[])definition.TcpValues.Clone(),
                CoordOverlayJointLine = definition.CoordOverlayJointLine,
                CoordOverlayTcpLine = definition.CoordOverlayTcpLine,
            };
        }
    }
}
