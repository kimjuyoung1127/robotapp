// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections.Generic;
using KineTutor3D.App;
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

        private PanelElements desktopPanel;
        private PanelElements tabletPanel;
        private PendantV3PreviewState.Kind previewState = PendantV3PreviewState.Kind.ConnectedServoOff;
        private bool isHomeActive;
        private bool isInitialized;
        private Coroutine initializeCoroutine;

        internal PendantV3PreviewState.Kind CurrentPreviewState => previewState;

        internal PendantV3PreviewState.Definition CurrentPreviewDefinition => PendantV3PreviewState.GetDefinition(previewState);

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
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
            previewState = state;
            ApplyPreview();
        }

        private void ApplyPreview()
        {
            var data = CurrentPreviewDefinition;
            ApplyTopStatusBar(data);
            ApplyPanel(desktopPanel, data);
            ApplyPanel(tabletPanel, data);
            PreviewChanged?.Invoke(data);
        }

        private void ApplyTopStatusBar(PendantV3PreviewState.Definition data)
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

        private void ApplyPanel(PanelElements panel, PendantV3PreviewState.Definition data)
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
            workPanelBody?.EnableInClassList("rc-hidden", !isHomeActive);
            bottomSheetBody?.EnableInClassList("rc-hidden", !isHomeActive);
            homePanelHost?.EnableInClassList("rc-hidden", !isHomeActive);
            homeSheetHost?.EnableInClassList("rc-hidden", !isHomeActive);
            workTabBar?.EnableInClassList("rc-hidden", isHomeActive);
            bottomTabBar?.EnableInClassList("rc-hidden", isHomeActive);

            if (workPanelTitle != null)
            {
                workPanelTitle.text = isHomeActive ? "연결 홈" : "WorkPanel";
            }

            if (workPanelSummary != null)
            {
                workPanelSummary.EnableInClassList("rc-hidden", isHomeActive);
            }

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
    }
}
