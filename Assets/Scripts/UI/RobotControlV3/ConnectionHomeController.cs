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

        internal event System.Action<PendantV3PreviewState.Definition> PreviewChanged;

        private readonly List<(Button button, EventCallback<ClickEvent> callback)> presetButtons = new();

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
        private PendantV3ConnectionSessionAdapter connectionSessionAdapter;
        private PopupCoordinatorV3 popupCoordinator;

        private PanelElements desktopPanel;
        private PanelElements tabletPanel;
        private PendantV3PreviewState.Kind previewState = PendantV3PreviewState.Kind.ConnectedServoOff;
        private bool isHomeActive;
        private bool isInitialized;
        private Coroutine initializeCoroutine;

        internal PendantV3PreviewState.Kind CurrentPreviewState => previewState;

        internal PendantV3PreviewState.Definition CurrentPreviewDefinition => PendantV3PreviewState.GetDefinition(previewState);

        internal PendantV3ConnectionSessionState CurrentSessionState =>
            connectionSessionAdapter != null
                ? connectionSessionAdapter.CurrentState
                : PendantV3ConnectionSessionState.DefaultDisconnected();

        internal bool IsMockMode => CurrentSessionState.IsMockMode;

        internal bool IsLiveArmActive => CurrentSessionState.IsLiveArmActive;

        internal bool ActualMoveAllowed => CurrentSessionState.ActualMoveAllowed;

        internal string ActualMoveBlockReason => CurrentSessionState.ActualMoveBlockReason;

        public void ApplyServoEnablePolicy()
        {
            if (connectionSessionAdapter != null)
            {
                connectionSessionAdapter.ApplyServoEnablePolicy();
                return;
            }

            if (previewState == PendantV3PreviewState.Kind.ConnectedServoOff)
            {
                SetPreviewState(PendantV3PreviewState.Kind.ConnectedUnsynced);
            }
        }

        public void ApplySyncPolicy()
        {
            if (connectionSessionAdapter != null)
            {
                connectionSessionAdapter.ApplySyncPolicy();
                return;
            }

            if (previewState == PendantV3PreviewState.Kind.ConnectedUnsynced)
            {
                SetPreviewState(PendantV3PreviewState.Kind.ReadyToJog);
            }
        }

        public void ApplyRunPolicy()
        {
            if (connectionSessionAdapter != null)
            {
                connectionSessionAdapter.ApplyRunPolicy();
                return;
            }

            if (previewState is PendantV3PreviewState.Kind.ConnectedUnsynced or PendantV3PreviewState.Kind.ReadyToJog)
            {
                SetPreviewState(PendantV3PreviewState.Kind.ReadyToJog);
            }
        }

        public void ApplyResetErrorPolicy()
        {
            if (connectionSessionAdapter != null)
            {
                connectionSessionAdapter.ApplyResetErrorPolicy();
                return;
            }

            if (previewState == PendantV3PreviewState.Kind.Fault)
            {
                SetPreviewState(PendantV3PreviewState.Kind.ConnectedServoOff);
            }
        }

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
            if (connectionSessionAdapter != null)
            {
                connectionSessionAdapter.StateChanged -= HandleSessionStateChanged;
            }

            if (popupCoordinator != null)
            {
                popupCoordinator.PopupStateChanged -= HandlePopupStateChanged;
            }

            UnbindPresetButtons();
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
            if (!isInitialized)
            {
                TryInitialize();
            }

            ApplyHomeVisibility();
            ApplyPreview();
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
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
            return $"initialized={isInitialized}; template={templateName}; document={(document != null)}; liveRoot={(liveRoot != null)}; livePanelHost={(livePanelHost != null)}; liveSheetHost={(liveSheetHost != null)}; isHomeActive={isHomeActive}; panelChildren={panelChildren}; panelHidden={panelHidden}; sheetChildren={sheetChildren}; sheetHidden={sheetHidden}; previewState={previewState}; session={CurrentSessionState.ToDebugSummary()}";
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
            connectionSessionAdapter ??= GetComponent<PendantV3ConnectionSessionAdapter>();
            popupCoordinator ??= GetComponent<PopupCoordinatorV3>();
            root = document?.rootVisualElement;
            if (root == null || connectionHomeTemplate == null)
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

            if (connectionSessionAdapter != null)
            {
                connectionSessionAdapter.StateChanged -= HandleSessionStateChanged;
                connectionSessionAdapter.StateChanged += HandleSessionStateChanged;
                connectionSessionAdapter.ForceInitialize();
            }

            if (popupCoordinator != null)
            {
                popupCoordinator.PopupStateChanged -= HandlePopupStateChanged;
                popupCoordinator.PopupStateChanged += HandlePopupStateChanged;
                connectionSessionAdapter?.SetPopupBlockActive(popupCoordinator.HasActivePopup);
            }

            ApplyShellStateSnapshot();
            ApplyPreview();
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
        }

        private void BuildPanels()
        {
            UnbindPresetButtons();
            desktopPanel = CreatePanel(homePanelHost);
            tabletPanel = CreatePanel(homeSheetHost);
            ApplyPreview();
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
            RegisterPanelActions(panel);
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
            button.RegisterCallback(callback);
            presetButtons.Add((button, callback));
        }

        private void RegisterPanelActions(PanelElements panel)
        {
            RegisterButton(panel.BtnConnect, OnConnectClicked);
            RegisterButton(panel.BtnDisconnect, OnDisconnectClicked);
            RegisterButton(panel.BtnMockMode, OnMockModeClicked);
            RegisterButton(panel.BtnLiveMode, OnLiveModeClicked);
            RegisterButton(panel.BtnArmLive, OnArmLiveClicked);
            RegisterButton(panel.BtnDisarmLive, OnDisarmLiveClicked);
            RegisterButton(panel.BtnQuickAction, OnQuickActionClicked);
            RegisterButton(panel.BtnPrimaryAction, OnPrimaryActionClicked);
        }

        private static void RegisterButton(Button button, System.Action handler)
        {
            if (button == null || handler == null)
            {
                return;
            }

            button.clicked += handler;
        }

        private void UnbindPresetButtons()
        {
            foreach (var (button, callback) in presetButtons)
            {
                button.UnregisterCallback(callback);
            }

            presetButtons.Clear();
        }

        private void SetPreviewState(PendantV3PreviewState.Kind state)
        {
            if (connectionSessionAdapter != null)
            {
                connectionSessionAdapter.SetDebugDisplayKind(MapDisplayKind(state));
                return;
            }

            previewState = state;
            ApplyPreview();
        }

        private void ApplyPreview()
        {
            previewState = ResolvePreviewStateFromSession();
            var data = CurrentPreviewDefinition;
            ApplyTopStatusBar(data);
            ApplyPanel(desktopPanel, data);
            ApplyPanel(tabletPanel, data);
            PreviewChanged?.Invoke(data);
        }

        private void ApplyTopStatusBar(PendantV3PreviewState.Definition data)
        {
            var session = CurrentSessionState;
            if (robotNameLabel != null)
            {
                robotNameLabel.text = data.RobotTitle;
            }

            ApplyChip(connectionIndicator, $"연결: {session.ConnectionSummary}", data.ConnectionClass);
            ApplyChip(modeLabel, $"모드: {session.ModeSummary}", data.ModeClass);
            ApplyChip(speedLabel, data.SpeedChip, data.SpeedClass);
            ApplyChip(coordSystemLabel, data.CoordChip, "rc-status-chip--muted");
            ApplyChip(toolLabel, $"Tool: {session.ToolId:00}", "rc-status-chip--muted");
            ApplyChip(userLabel, $"User: {session.UserId:00}", "rc-status-chip--muted");
            ApplyChip(safetyLabel, $"안전: {session.SafetySummary}", data.SafetyClass);
            ApplyChip(faultLabel, $"Fault: {session.FaultSummary}", data.FaultClass);

            btnServoEnable?.SetEnabled(data.ServoEnabled && !session.ReconnectActive);
            btnRun?.SetEnabled(data.RunEnabled);
            btnStop?.SetEnabled(data.StopEnabled);
            btnPause?.SetEnabled(data.PauseEnabled);
            btnSync?.SetEnabled(data.SyncEnabled && !session.ReconnectActive);
            btnResetError?.SetEnabled(data.ResetEnabled);
        }

        private void ApplyPanel(PanelElements panel, PendantV3PreviewState.Definition data)
        {
            if (panel == null)
            {
                return;
            }

            var session = CurrentSessionState;
            panel.ConnectionRobot.text = data.RobotTitle;
            panel.ConnectionIp.text = $"IP: {session.IpAddress}";
            panel.ConnectionStatus.text = BuildConnectionStatusText(data, session);
            panel.BtnConnect.SetEnabled(!session.IsConnected || session.ReconnectFailed);
            panel.BtnDisconnect.SetEnabled(session.IsConnected && !session.ReconnectActive);

            panel.QuickServo.text = BuildQuickServoText(session);
            panel.QuickMode.text = $"모드: {session.ModeSummary}";
            panel.QuickSync.text = BuildQuickSyncText(session, data);
            panel.QuickControllerMode.text = $"컨트롤러: {(session.IsMockMode ? "Mock" : "Live")}";
            panel.QuickLiveArm.text = $"Live Arm: {session.LiveArmSummary}";
            panel.BtnMockMode.SetEnabled(!session.ReconnectActive && !session.IsMockMode);
            panel.BtnLiveMode.SetEnabled(!session.ReconnectActive && session.IsMockMode);
            panel.BtnArmLive.SetEnabled(!session.IsMockMode && !session.IsLiveArmActive && !session.ReconnectActive && session.IsConnected && session.IsEnabled);
            panel.BtnDisarmLive.SetEnabled(session.IsLiveArmActive);
            panel.BtnQuickAction.text = ResolveQuickActionLabel(session, data);
            panel.BtnQuickAction.SetEnabled(ResolveQuickActionEnabled(session, data));

            panel.ActionNow.text = ResolveActionNow(session, data);
            panel.ActionPrimary.text = ResolveActionPrimary(session, data);
            panel.ActionWhy.text = ResolveActionWhy(session, data);
            panel.BtnPrimaryAction.text = ResolvePrimaryActionLabel(session, data);
            panel.BtnPrimaryAction.SetEnabled(ResolvePrimaryActionEnabled(session, data));

            SetPresetActive(panel.BtnPresetDisconnected, previewState == PendantV3PreviewState.Kind.Disconnected);
            SetPresetActive(panel.BtnPresetServoOff, previewState == PendantV3PreviewState.Kind.ConnectedServoOff);
            SetPresetActive(panel.BtnPresetUnsynced, previewState == PendantV3PreviewState.Kind.ConnectedUnsynced);
            SetPresetActive(panel.BtnPresetReady, previewState == PendantV3PreviewState.Kind.ReadyToJog);
            SetPresetActive(panel.BtnPresetFault, previewState == PendantV3PreviewState.Kind.Fault);
            SetPresetActive(panel.BtnPresetReconnect, previewState == PendantV3PreviewState.Kind.AutoReconnect);
        }

        private void OnConnectClicked()
        {
            connectionSessionAdapter?.ConnectNow();
        }

        private void OnDisconnectClicked()
        {
            connectionSessionAdapter?.DisconnectNow();
        }

        private void OnMockModeClicked()
        {
            connectionSessionAdapter?.SetMockMode(true);
        }

        private void OnLiveModeClicked()
        {
            connectionSessionAdapter?.SetMockMode(false);
        }

        private void OnArmLiveClicked()
        {
            if (popupCoordinator != null)
            {
                var session = CurrentSessionState;
                var summary = $"{session.IpAddress} / {session.ConnectionSummary} / 서보 {session.ServoSummary} / 실기 이동 허용";
                popupCoordinator.OpenMoveConfirmForPolicy("Live Arm 확인", summary, () => connectionSessionAdapter?.SetLiveArmState(true), "Arm Live");
                return;
            }

            connectionSessionAdapter?.SetLiveArmState(true);
        }

        private void OnDisarmLiveClicked()
        {
            connectionSessionAdapter?.SetLiveArmState(false);
        }

        private void OnQuickActionClicked()
        {
            if (CurrentSessionState.ReconnectFailed)
            {
                connectionSessionAdapter?.ConnectNow();
                return;
            }

            if (CurrentSessionState.IsMockMode)
            {
                connectionSessionAdapter?.SetMockMode(false);
                return;
            }

            ApplyPrimaryActionCore();
        }

        private void OnPrimaryActionClicked()
        {
            ApplyPrimaryActionCore();
        }

        private void ApplyPrimaryActionCore()
        {
            switch (CurrentPreviewState)
            {
                case PendantV3PreviewState.Kind.ConnectedServoOff:
                    ApplyServoEnablePolicy();
                    break;
                case PendantV3PreviewState.Kind.ConnectedUnsynced:
                    ApplySyncPolicy();
                    break;
                case PendantV3PreviewState.Kind.ReadyToJog:
                    if (!CurrentSessionState.IsMockMode && !CurrentSessionState.IsLiveArmActive)
                    {
                        OnArmLiveClicked();
                        break;
                    }

                    RouteToEasyMotion();
                    break;
                case PendantV3PreviewState.Kind.Disconnected:
                    connectionSessionAdapter?.ConnectNow();
                    break;
            }
        }

        private void RouteToEasyMotion()
        {
            var shellStateController = GetComponent<PendantV3ShellStateController>();
            if (shellStateController == null)
            {
                return;
            }

            var shell = shellStateController.GetStateSnapshot();
            shellStateController.SetDebugSelection("NavMotion", "TabEasyMotion", "BottomTabEasyMotion");
        }

        private void HandleSessionStateChanged(PendantV3ConnectionSessionState _)
        {
            ApplyPreview();
        }

        private void HandlePopupStateChanged()
        {
            connectionSessionAdapter?.SetPopupBlockActive(popupCoordinator != null && popupCoordinator.HasActivePopup);
            ApplyPreview();
        }

        private void ApplyHomeVisibility()
        {
            workPanelBody?.EnableInClassList("rc-hidden", !isHomeActive);
            bottomSheetBody?.EnableInClassList("rc-hidden", !isHomeActive);
            homePanelHost?.EnableInClassList("rc-hidden", !isHomeActive);
            homeSheetHost?.EnableInClassList("rc-hidden", !isHomeActive);
            workTabBar?.EnableInClassList("rc-hidden", isHomeActive);
            bottomTabBar?.EnableInClassList("rc-hidden", isHomeActive);

            if (workPanelTitle != null)
            {
                workPanelTitle.text = isHomeActive ? "연결 홈" : "쉬운 조작 패널";
            }

            if (workPanelSummary != null)
            {
                workPanelSummary.EnableInClassList("rc-hidden", isHomeActive);
            }

            if (bottomSheetTitle != null)
            {
                bottomSheetTitle.text = isHomeActive ? "BottomSheet · 연결 홈" : "BottomSheet · 쉬운조작";
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

        private PendantV3PreviewState.Kind ResolvePreviewStateFromSession()
        {
            return MapPreviewKind(CurrentSessionState.DisplayKind);
        }

        private static PendantV3PreviewState.Kind MapPreviewKind(PendantV3ConnectionDisplayKind kind)
        {
            return kind switch
            {
                PendantV3ConnectionDisplayKind.ConnectedServoOff => PendantV3PreviewState.Kind.ConnectedServoOff,
                PendantV3ConnectionDisplayKind.ConnectedUnsynced => PendantV3PreviewState.Kind.ConnectedUnsynced,
                PendantV3ConnectionDisplayKind.ReadyToJog => PendantV3PreviewState.Kind.ReadyToJog,
                PendantV3ConnectionDisplayKind.Fault => PendantV3PreviewState.Kind.Fault,
                PendantV3ConnectionDisplayKind.AutoReconnect => PendantV3PreviewState.Kind.AutoReconnect,
                _ => PendantV3PreviewState.Kind.Disconnected,
            };
        }

        private static PendantV3ConnectionDisplayKind MapDisplayKind(PendantV3PreviewState.Kind kind)
        {
            return kind switch
            {
                PendantV3PreviewState.Kind.ConnectedServoOff => PendantV3ConnectionDisplayKind.ConnectedServoOff,
                PendantV3PreviewState.Kind.ConnectedUnsynced => PendantV3ConnectionDisplayKind.ConnectedUnsynced,
                PendantV3PreviewState.Kind.ReadyToJog => PendantV3ConnectionDisplayKind.ReadyToJog,
                PendantV3PreviewState.Kind.Fault => PendantV3ConnectionDisplayKind.Fault,
                PendantV3PreviewState.Kind.AutoReconnect => PendantV3ConnectionDisplayKind.AutoReconnect,
                _ => PendantV3ConnectionDisplayKind.Disconnected,
            };
        }

        private static string BuildConnectionStatusText(PendantV3PreviewState.Definition data, PendantV3ConnectionSessionState session)
        {
            if (session.ReconnectActive)
            {
                return $"상태: 재연결 시도 중 ({session.ReconnectAttempt + 1}/{session.ReconnectAttemptMax})";
            }

            if (session.ReconnectFailed)
            {
                return "상태: 자동 재연결 실패 / 수동 연결 필요";
            }

            return data.ConnectionCardStatus;
        }

        private static string BuildQuickServoText(PendantV3ConnectionSessionState session)
        {
            return session.ReconnectActive
                ? "서보: 보류 / 복귀 대기"
                : $"서보: {session.ServoSummary}";
        }

        private static string BuildQuickSyncText(PendantV3ConnectionSessionState session, PendantV3PreviewState.Definition data)
        {
            if (session.ReconnectActive)
            {
                return $"다음 재시도: {Mathf.CeilToInt(session.ReconnectSecondsUntilRetry)}초 후";
            }

            if (session.ReconnectFailed)
            {
                return "자동 재연결 실패 / 수동 연결 필요";
            }

            return data.QuickSync;
        }

        private static string ResolveQuickActionLabel(PendantV3ConnectionSessionState session, PendantV3PreviewState.Definition data)
        {
            if (session.ReconnectFailed)
            {
                return "수동 연결";
            }

            if (session.IsMockMode)
            {
                return "Live로 전환";
            }

            return data.QuickActionLabel;
        }

        private static bool ResolveQuickActionEnabled(PendantV3ConnectionSessionState session, PendantV3PreviewState.Definition data)
        {
            if (session.ReconnectActive)
            {
                return false;
            }

            if (session.ReconnectFailed)
            {
                return true;
            }

            if (session.IsMockMode)
            {
                return !session.ReconnectActive;
            }

            return data.QuickActionEnabled;
        }

        private static string ResolveActionNow(PendantV3ConnectionSessionState session, PendantV3PreviewState.Definition data)
        {
            if (session.ReconnectActive)
            {
                return $"지금 상태: 자동 재연결 중 ({session.ReconnectAttempt}/{session.ReconnectAttemptMax})";
            }

            if (session.ReconnectFailed)
            {
                return "지금 상태: 자동 재연결 실패";
            }

            if (!session.IsMockMode && !session.IsLiveArmActive)
            {
                return "지금 상태: Live 연결 / Disarmed";
            }

            return data.ActionNow;
        }

        private static string ResolveActionPrimary(PendantV3ConnectionSessionState session, PendantV3PreviewState.Definition data)
        {
            if (session.ReconnectActive)
            {
                return $"다음 행동: {Mathf.CeilToInt(session.ReconnectSecondsUntilRetry)}초 뒤 재시도 결과를 확인";
            }

            if (session.ReconnectFailed)
            {
                return "다음 행동: 수동 연결로 복귀";
            }

            if (!session.IsMockMode && !session.IsLiveArmActive)
            {
                return "다음 행동: Live Arm 확인";
            }

            return data.ActionPrimary;
        }

        private static string ResolveActionWhy(PendantV3ConnectionSessionState session, PendantV3PreviewState.Definition data)
        {
            if (session.ReconnectActive)
            {
                return $"3초 간격 자동 재시도를 진행 중이다. 남은 시도는 {Mathf.Max(0, session.ReconnectAttemptMax - session.ReconnectAttempt)}회다.";
            }

            if (session.ReconnectFailed)
            {
                return string.IsNullOrWhiteSpace(session.ReconnectFailureSummary)
                    ? "자동 복구가 끝까지 실패했으니 수동 연결부터 다시 시도해라."
                    : session.ReconnectFailureSummary;
            }

            if (!session.ActualMoveAllowed)
            {
                return session.ActualMoveBlockReason;
            }

            return data.ActionWhy;
        }

        private static string ResolvePrimaryActionLabel(PendantV3ConnectionSessionState session, PendantV3PreviewState.Definition data)
        {
            if (session.ReconnectActive)
            {
                return "재시도 대기";
            }

            if (session.ReconnectFailed)
            {
                return "수동 연결 →";
            }

            if (!session.IsMockMode && !session.IsLiveArmActive)
            {
                return "Live Arm →";
            }

            return data.PrimaryActionLabel;
        }

        private static bool ResolvePrimaryActionEnabled(PendantV3ConnectionSessionState session, PendantV3PreviewState.Definition data)
        {
            if (session.ReconnectActive)
            {
                return false;
            }

            if (session.ReconnectFailed)
            {
                return true;
            }

            if (!session.IsMockMode && !session.IsLiveArmActive)
            {
                return session.IsConnected && session.IsEnabled && !session.ReconnectActive;
            }

            return data.PrimaryActionEnabled;
        }
    }
}
