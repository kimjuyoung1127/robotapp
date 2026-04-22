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

        private readonly List<(Button button, EventCallback<ClickEvent> callback, System.Action clicked)> presetButtons = new();

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
        private Button btnServoEnable;
        private Button btnRun;
        private Button btnStop;
        private Button btnPause;
        private Button btnSync;
        private Button btnResetError;
        private Button btnRunBottom;
        private Button btnStopBottom;
        private EventCallback<ClickEvent> connectClickCallback;
        private EventCallback<ClickEvent> disconnectClickCallback;
        private EventCallback<ClickEvent> primaryActionClickCallback;
        private EventCallback<ClickEvent> servoClickCallback;
        private EventCallback<ClickEvent> runClickCallback;
        private EventCallback<ClickEvent> stopClickCallback;
        private EventCallback<ClickEvent> resetClickCallback;
        private EventCallback<ClickEvent> pauseClickCallback;
        private EventCallback<ClickEvent> syncClickCallback;
        private RobotControlV3RuntimeController runtimeController;

        private PanelElements desktopPanel;
        private PanelElements tabletPanel;
        private PendantV3PreviewState.Kind previewState = PendantV3PreviewState.Kind.ConnectedServoOff;
        private RobotControlV3RuntimeSnapshot debugOverrideSnapshot;
        private bool isHomeActive;
        private bool isPointsActive;
        private bool isInitialized;
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
            document ??= GetComponent<UIDocument>();
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            root = document?.rootVisualElement;
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
            ApplyPreview(CurrentPreviewDefinition ?? CreateFallbackSnapshot(previewState));
            ApplyHomeVisibility();
            isInitialized = true;
            return true;
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
            btnServoEnable = root.Q<Button>("BtnServoEnable");
            btnRun = root.Q<Button>("BtnRun");
            btnStop = root.Q<Button>("BtnStop");
            btnPause = root.Q<Button>("BtnPause");
            btnSync = root.Q<Button>("BtnSync");
            btnResetError = root.Q<Button>("BtnResetError");
            btnRunBottom = root.Q<Button>("BtnRunBottom");
            btnStopBottom = root.Q<Button>("BtnStopBottom");
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

            EventCallback<ClickEvent> callback = _ => SetPreviewState(state);
            System.Action clicked = () => SetPreviewState(state);
            button.RegisterCallback<ClickEvent>(callback);
            button.clicked += clicked;
            presetButtons.Add((button, callback, clicked));
        }

        private void UnbindPresetButtons()
        {
            foreach (var (button, callback, clicked) in presetButtons)
            {
                button.UnregisterCallback<ClickEvent>(callback);
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

            previewState = MapStatusKind(data.StatusKind);
            ApplyTopStatusBar(data);
            ApplyPanel(desktopPanel, data);
            ApplyPanel(tabletPanel, data);
            PreviewChanged?.Invoke(data);
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

            btnServoEnable?.SetEnabled(data.ServoEnabled);
            btnRun?.SetEnabled(data.RunEnabled);
            btnStop?.SetEnabled(data.StopEnabled);
            btnPause?.SetEnabled(data.PauseEnabled);
            btnSync?.SetEnabled(data.SyncEnabled);
            btnResetError?.SetEnabled(data.ResetEnabled);
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
            panel.BtnQuickAction.text = data.QuickActionLabel;
            panel.BtnQuickAction.SetEnabled(data.QuickActionEnabled);

            panel.ActionNow.text = data.ActionNow;
            panel.ActionPrimary.text = data.ActionPrimary;
            panel.ActionWhy.text = data.ActionWhy;
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

        private static void SetPresetActive(Button button, bool active)
        {
            button?.EnableInClassList("rc-home-state-button--active", active);
        }

        private void BindRuntimeButtons()
        {
            connectClickCallback ??= _ => HandleConnectClicked();
            disconnectClickCallback ??= _ => HandleDisconnectClicked();
            primaryActionClickCallback ??= _ => HandlePrimaryActionClicked();
            servoClickCallback ??= _ => HandleServoClicked();
            runClickCallback ??= _ => HandleRunClicked();
            stopClickCallback ??= _ => HandleStopClicked();
            resetClickCallback ??= _ => HandleResetClicked();
            pauseClickCallback ??= _ => HandlePauseClicked();
            syncClickCallback ??= _ => HandleSyncClicked();

            if (btnServoEnable != null)
            {
                btnServoEnable.RegisterCallback(servoClickCallback);
            }

            if (btnRun != null)
            {
                btnRun.RegisterCallback(runClickCallback);
            }

            if (btnStop != null)
            {
                btnStop.RegisterCallback(stopClickCallback);
            }

            if (btnRunBottom != null)
            {
                btnRunBottom.RegisterCallback(runClickCallback);
            }

            if (btnStopBottom != null)
            {
                btnStopBottom.RegisterCallback(stopClickCallback);
            }

            if (btnPause != null)
            {
                btnPause.RegisterCallback(pauseClickCallback);
            }

            if (btnSync != null)
            {
                btnSync.RegisterCallback(syncClickCallback);
            }

            if (btnResetError != null)
            {
                btnResetError.RegisterCallback(resetClickCallback);
            }

            if (desktopPanel != null)
            {
                desktopPanel.BtnConnect.RegisterCallback(connectClickCallback);
                desktopPanel.BtnDisconnect.RegisterCallback(disconnectClickCallback);
                desktopPanel.BtnQuickAction.RegisterCallback(primaryActionClickCallback);
                desktopPanel.BtnPrimaryAction.RegisterCallback(primaryActionClickCallback);
            }

            if (tabletPanel != null)
            {
                tabletPanel.BtnConnect.RegisterCallback(connectClickCallback);
                tabletPanel.BtnDisconnect.RegisterCallback(disconnectClickCallback);
                tabletPanel.BtnQuickAction.RegisterCallback(primaryActionClickCallback);
                tabletPanel.BtnPrimaryAction.RegisterCallback(primaryActionClickCallback);
            }
        }

        private void UnbindRuntimeButtons()
        {
            if (btnServoEnable != null && servoClickCallback != null)
            {
                btnServoEnable.UnregisterCallback(servoClickCallback);
            }

            if (btnRun != null && runClickCallback != null)
            {
                btnRun.UnregisterCallback(runClickCallback);
            }

            if (btnStop != null && stopClickCallback != null)
            {
                btnStop.UnregisterCallback(stopClickCallback);
            }

            if (btnRunBottom != null && runClickCallback != null)
            {
                btnRunBottom.UnregisterCallback(runClickCallback);
            }

            if (btnStopBottom != null && stopClickCallback != null)
            {
                btnStopBottom.UnregisterCallback(stopClickCallback);
            }

            if (btnPause != null)
            {
                if (pauseClickCallback != null)
                {
                    btnPause.UnregisterCallback(pauseClickCallback);
                }
            }

            if (btnSync != null)
            {
                if (syncClickCallback != null)
                {
                    btnSync.UnregisterCallback(syncClickCallback);
                }
            }

            if (btnResetError != null && resetClickCallback != null)
            {
                btnResetError.UnregisterCallback(resetClickCallback);
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

            if (connectClickCallback != null)
            {
                panel.BtnConnect.UnregisterCallback(connectClickCallback);
            }

            if (disconnectClickCallback != null)
            {
                panel.BtnDisconnect.UnregisterCallback(disconnectClickCallback);
            }

            if (primaryActionClickCallback != null)
            {
                panel.BtnQuickAction.UnregisterCallback(primaryActionClickCallback);
                panel.BtnPrimaryAction.UnregisterCallback(primaryActionClickCallback);
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
            runtimeController?.ConnectDefault();
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
            runtimeController?.SyncCurrentState();
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
