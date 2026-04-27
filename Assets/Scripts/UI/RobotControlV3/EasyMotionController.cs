// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 쉬운 조작 패널 시안을 desktop/tablet host에 주입합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ConnectionHomeController))]
    public sealed class EasyMotionController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset easyMotionTemplate;

        private VisualElement root;
        private VisualElement workPanelBody;
        private VisualElement bottomSheetBody;
        private VisualElement easyMotionPanelHost;
        private VisualElement easyMotionSheetHost;
        private ConnectionHomeController connectionHomeController;
        private RobotControlV3RuntimeController runtimeController;
        private string selectedPresetName = "Ready";

        private PanelElements desktopPanel;
        private PanelElements tabletPanel;
        private bool isDesktopVisible;
        private bool isTabletVisible;
        private bool isInitialized;
        private Coroutine initializeCoroutine;

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
            if (isInitialized && connectionHomeController != null)
            {
                connectionHomeController.PreviewChanged -= ApplyPreview;
            }

            if (initializeCoroutine != null)
            {
                StopCoroutine(initializeCoroutine);
                initializeCoroutine = null;
            }

            isInitialized = false;
        }

        public string GetDebugSummary()
        {
            var liveRoot = document != null ? document.rootVisualElement : null;
            var livePanelHost = liveRoot?.Q<VisualElement>("EasyMotionPanelHost");
            var liveSheetHost = liveRoot?.Q<VisualElement>("EasyMotionSheetHost");
            var panelChildren = easyMotionPanelHost?.childCount ?? -1;
            var sheetChildren = easyMotionSheetHost?.childCount ?? -1;
            var panelHidden = easyMotionPanelHost?.ClassListContains("rc-hidden") ?? false;
            var sheetHidden = easyMotionSheetHost?.ClassListContains("rc-hidden") ?? false;
            var templateName = easyMotionTemplate != null ? easyMotionTemplate.name : "null";
            return $"initialized={isInitialized}; template={templateName}; document={(document != null)}; liveRoot={(liveRoot != null)}; livePanelHost={(livePanelHost != null)}; liveSheetHost={(liveSheetHost != null)}; panelHost={(easyMotionPanelHost != null)}; panelChildren={panelChildren}; panelHidden={panelHidden}; sheetHost={(easyMotionSheetHost != null)}; sheetChildren={sheetChildren}; sheetHidden={sheetHidden}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}";
        }

        public void SetShellState(string activeNavSection, string activeWorkTab, string activeTabletTab)
        {
            isDesktopVisible = activeNavSection == "NavMotion" && activeWorkTab == "TabEasyMotion";
            isTabletVisible = activeNavSection == "NavMotion" && activeTabletTab == "BottomTabEasyMotion";
            if (!isInitialized)
            {
                TryInitialize();
            }

            ApplyVisibility();
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            root = document?.rootVisualElement;
            if (root == null || easyMotionTemplate == null || connectionHomeController == null || runtimeController == null)
            {
                return false;
            }

            CacheShellElements();
            if (easyMotionPanelHost == null || easyMotionSheetHost == null)
            {
                isInitialized = false;
                return false;
            }

            if (desktopPanel == null || tabletPanel == null || easyMotionPanelHost.childCount == 0 || easyMotionSheetHost.childCount == 0)
            {
                BuildPanels();
            }

            ApplyShellStateSnapshot();
            connectionHomeController.PreviewChanged -= ApplyPreview;
            connectionHomeController.PreviewChanged += ApplyPreview;
            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            ApplyVisibility();
            isInitialized = true;
            return true;
        }

        private void ApplyShellStateSnapshot()
        {
            var shellStateController = GetComponent<PendantV3ShellStateController>();
            var localState = shellStateController != null
                ? shellStateController.GetStateSnapshot()
                : PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault());
            isDesktopVisible = localState.ActiveNavSection == "NavMotion" && localState.ActiveWorkTab == "TabEasyMotion";
            isTabletVisible = localState.ActiveNavSection == "NavMotion" && localState.ActiveTabletTab == "BottomTabEasyMotion";
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
            workPanelBody = root.Q<VisualElement>("WorkPanelBody");
            bottomSheetBody = root.Q<VisualElement>("BottomSheetBody");
            easyMotionPanelHost = root.Q<VisualElement>("EasyMotionPanelHost");
            easyMotionSheetHost = root.Q<VisualElement>("EasyMotionSheetHost");
        }

        private void BuildPanels()
        {
            desktopPanel = CreatePanel(easyMotionPanelHost);
            tabletPanel = CreatePanel(easyMotionSheetHost);
        }

        private PanelElements CreatePanel(VisualElement host)
        {
            if (host == null)
            {
                return null;
            }

            host.Clear();
            var tree = easyMotionTemplate.CloneTree();
            host.Add(tree);
            var panel = new PanelElements(tree);
            RegisterPanel(panel);
            return panel;
        }

        private void RegisterPanel(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            RegisterClick(panel.BtnEasyHome, () => SelectPresetAndPreview("Home"));
            RegisterClick(panel.BtnEasyReady, () => SelectPresetAndPreview("Ready"));
            RegisterClick(panel.BtnEasyFolded, () => SelectPresetAndPreview("Folded"));
            RegisterClick(panel.BtnEasyZero, () => SelectPresetAndPreview("Zero"));
            RegisterClick(panel.BtnEasyPreview, PreviewSelectedPreset);
            RegisterClick(panel.BtnEasyApply, ApplySelectedPreset);
            RegisterClick(panel.BtnGripperOpen, () => runtimeController?.SetGripperOpen(true));
            RegisterClick(panel.BtnGripperClose, () => runtimeController?.SetGripperOpen(false));
        }

        private static void RegisterClick(Button button, System.Action handler)
        {
            if (button == null || handler == null)
            {
                return;
            }

            button.clicked += handler;
        }

        private void ApplyPreview(RobotControlV3RuntimeSnapshot data)
        {
            ApplyPanel(desktopPanel, data, connectionHomeController.CurrentPreviewState);
            ApplyPanel(tabletPanel, data, connectionHomeController.CurrentPreviewState);
        }

        private void ApplyPanel(PanelElements panel, RobotControlV3RuntimeSnapshot data, PendantV3PreviewState.Kind state)
        {
            if (panel == null)
            {
                return;
            }

            panel.EasyStateSummary.text = $"{data.StatusConnection} · Tool {data.StatusTool} · User {data.StatusUser}";
            panel.EasyModeBadge.text = state == PendantV3PreviewState.Kind.ReadyToJog ? "조작 가능" : "초보자 시작";
            panel.EasyDryRunLabel.text = $"{data.GripperSummary} · {data.ToolDoSummary}";
            panel.EasyActionHint.text = $"{data.ActionWhy} / {data.PeripheralFeedback}";

            var canPreview = state != PendantV3PreviewState.Kind.AutoReconnect;
            var canApply = data.DryRunEnabled || state == PendantV3PreviewState.Kind.ReadyToJog;
            var canGrip = data.DryRunEnabled
                || state is PendantV3PreviewState.Kind.ReadyToJog
                    or PendantV3PreviewState.Kind.ConnectedUnsynced
                    or PendantV3PreviewState.Kind.ConnectedServoOff
                    or PendantV3PreviewState.Kind.Disconnected;
            var canPreset = canPreview;

            panel.BtnEasyHome.SetEnabled(canPreset);
            panel.BtnEasyReady.SetEnabled(canPreset);
            panel.BtnEasyFolded.SetEnabled(canPreset);
            panel.BtnEasyZero.SetEnabled(canPreset);
            panel.BtnEasyPreview.SetEnabled(canPreview);
            panel.BtnEasyApply.SetEnabled(canApply);
            panel.BtnGripperOpen.SetEnabled(canGrip);
            panel.BtnGripperClose.SetEnabled(canGrip);
        }

        private void SelectPresetAndPreview(string presetName)
        {
            selectedPresetName = string.IsNullOrWhiteSpace(presetName) ? "Ready" : presetName;
            runtimeController?.PreviewPreset(selectedPresetName);
        }

        private void PreviewSelectedPreset()
        {
            runtimeController?.PreviewPreset(selectedPresetName);
        }

        private void ApplySelectedPreset()
        {
            runtimeController?.ApplyPreset(selectedPresetName);
        }

        private void ApplyVisibility()
        {
            if (isDesktopVisible)
            {
                workPanelBody?.EnableInClassList("rc-hidden", false);
            }

            if (isTabletVisible)
            {
                bottomSheetBody?.EnableInClassList("rc-hidden", false);
            }

            easyMotionPanelHost?.EnableInClassList("rc-hidden", !isDesktopVisible);
            easyMotionSheetHost?.EnableInClassList("rc-hidden", !isTabletVisible);
        }

        private sealed class PanelElements
        {
            public PanelElements(VisualElement root)
            {
                EasyModeBadge = root.Q<Label>("EasyModeBadge");
                EasyStateSummary = root.Q<Label>("EasyStateSummary");
                EasyDryRunLabel = root.Q<Label>("EasyDryRunLabel");
                EasyActionHint = root.Q<Label>("EasyActionHint");
                BtnEasyHome = root.Q<Button>("BtnEasyHome");
                BtnEasyReady = root.Q<Button>("BtnEasyReady");
                BtnEasyFolded = root.Q<Button>("BtnEasyFolded");
                BtnEasyZero = root.Q<Button>("BtnEasyZero");
                BtnGripperOpen = root.Q<Button>("BtnGripperOpen");
                BtnGripperClose = root.Q<Button>("BtnGripperClose");
                BtnEasyPreview = root.Q<Button>("BtnEasyPreview");
                BtnEasyApply = root.Q<Button>("BtnEasyApply");
            }

            public Label EasyModeBadge { get; }
            public Label EasyStateSummary { get; }
            public Label EasyDryRunLabel { get; }
            public Label EasyActionHint { get; }
            public Button BtnEasyHome { get; }
            public Button BtnEasyReady { get; }
            public Button BtnEasyFolded { get; }
            public Button BtnEasyZero { get; }
            public Button BtnGripperOpen { get; }
            public Button BtnGripperClose { get; }
            public Button BtnEasyPreview { get; }
            public Button BtnEasyApply { get; }
        }
    }
}
