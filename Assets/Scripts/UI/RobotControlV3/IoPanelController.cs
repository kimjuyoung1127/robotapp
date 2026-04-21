// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 I/O와 그리퍼 mock/live-gated 상태 패널을 구성합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class IoPanelController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;

        private VisualElement root;
        private VisualElement workPanelBody;
        private VisualElement bottomSheetBody;
        private VisualElement ioPanelHost;
        private VisualElement ioSheetHost;
        private RobotControlV3RuntimeController runtimeController;
        private ConnectionHomeController connectionHomeController;
        private PanelElements desktopPanel;
        private PanelElements tabletPanel;
        private bool isDesktopVisible;
        private bool isTabletVisible;
        private bool isInitialized;

        private void OnEnable()
        {
            TryInitialize();
        }

        private void OnDisable()
        {
            if (connectionHomeController != null)
            {
                connectionHomeController.PreviewChanged -= ApplyPreview;
            }

            isInitialized = false;
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            return $"initialized={isInitialized}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}; panelChildren={ioPanelHost?.childCount ?? -1}; sheetChildren={ioSheetHost?.childCount ?? -1}";
        }

        public void SetShellState(string activeNavSection, string activeWorkTab, string activeTabletTab)
        {
            isDesktopVisible = activeNavSection == "NavIo";
            isTabletVisible = activeTabletTab == "BottomTabIo";
            if (!isInitialized)
            {
                TryInitialize();
            }

            ApplyVisibility();
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            root = document?.rootVisualElement;
            if (root == null || runtimeController == null || connectionHomeController == null)
            {
                return false;
            }

            workPanelBody = root.Q<VisualElement>("WorkPanelBody");
            bottomSheetBody = root.Q<VisualElement>("BottomSheetBody");
            ioPanelHost = root.Q<VisualElement>("IoPanelHost");
            ioSheetHost = root.Q<VisualElement>("IoSheetHost");
            if (ioPanelHost == null || ioSheetHost == null)
            {
                return false;
            }

            if (desktopPanel == null || tabletPanel == null || ioPanelHost.childCount == 0 || ioSheetHost.childCount == 0)
            {
                desktopPanel = CreatePanel(ioPanelHost);
                tabletPanel = CreatePanel(ioSheetHost);
            }

            connectionHomeController.PreviewChanged -= ApplyPreview;
            connectionHomeController.PreviewChanged += ApplyPreview;
            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            isInitialized = true;
            return true;
        }

        private PanelElements CreatePanel(VisualElement host)
        {
            host.Clear();
            var panel = new PanelElements();
            panel.Root.AddToClassList("rc-point-root");
            host.Add(panel.Root);
            Register(panel);
            return panel;
        }

        private void Register(PanelElements panel)
        {
            panel.BtnGripperOpen.clicked += () => runtimeController.SetGripperOpen(true);
            panel.BtnGripperClose.clicked += () => runtimeController.SetGripperOpen(false);
            panel.BtnDo0On.clicked += () => runtimeController.SetRobotDigitalOutput(0, true);
            panel.BtnDo0Off.clicked += () => runtimeController.SetRobotDigitalOutput(0, false);
            panel.BtnDo1On.clicked += () => runtimeController.SetRobotDigitalOutput(1, true);
            panel.BtnDo1Off.clicked += () => runtimeController.SetRobotDigitalOutput(1, false);
            panel.BtnToolDo0On.clicked += () => runtimeController.SetToolDigitalOutput(0, true);
            panel.BtnToolDo0Off.clicked += () => runtimeController.SetToolDigitalOutput(0, false);
            panel.BtnToolDo1On.clicked += () => runtimeController.SetToolDigitalOutput(1, true);
            panel.BtnToolDo1Off.clicked += () => runtimeController.SetToolDigitalOutput(1, false);
        }

        private void ApplyPreview(RobotControlV3RuntimeSnapshot snapshot)
        {
            ApplyPanel(desktopPanel, snapshot);
            ApplyPanel(tabletPanel, snapshot);
        }

        private void ApplyPanel(PanelElements panel, RobotControlV3RuntimeSnapshot snapshot)
        {
            if (panel == null || snapshot == null)
            {
                return;
            }

            panel.State.text = $"{snapshot.GripperSummary} · visual={(snapshot.GripperVisualAttached ? "attached" : "no visual")}";
            panel.Output.text = $"{snapshot.RobotDoSummary}\n{snapshot.ToolDoSummary}";
            panel.Feedback.text = snapshot.PeripheralFeedback;
            var enabled = snapshot.DryRunEnabled || snapshot.StatusKind is RobotControlV3RuntimeStatusKind.ReadyToJog or RobotControlV3RuntimeStatusKind.ConnectedUnsynced;
            panel.SetEnabled(enabled);
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

            ioPanelHost?.EnableInClassList("rc-hidden", !isDesktopVisible);
            ioSheetHost?.EnableInClassList("rc-hidden", !isTabletVisible);
        }

        private sealed class PanelElements
        {
            public readonly VisualElement Root = new();
            public readonly Label State = new("Gripper: --");
            public readonly Label Output = new("DO0 OFF / DO1 OFF");
            public readonly Label Feedback = new("주변장치 조작 전");
            public readonly Button BtnGripperOpen = new() { text = "그리퍼 열기" };
            public readonly Button BtnGripperClose = new() { text = "그리퍼 닫기" };
            public readonly Button BtnDo0On = new() { text = "DO0 ON" };
            public readonly Button BtnDo0Off = new() { text = "DO0 OFF" };
            public readonly Button BtnDo1On = new() { text = "DO1 ON" };
            public readonly Button BtnDo1Off = new() { text = "DO1 OFF" };
            public readonly Button BtnToolDo0On = new() { text = "TDO0 ON" };
            public readonly Button BtnToolDo0Off = new() { text = "TDO0 OFF" };
            public readonly Button BtnToolDo1On = new() { text = "TDO1 ON" };
            public readonly Button BtnToolDo1Off = new() { text = "TDO1 OFF" };

            public PanelElements()
            {
                Root.Add(new Label("I/O + 그리퍼") { name = "IoPanelTitle" });
                Root.Q<Label>("IoPanelTitle").AddToClassList("rc-panel-title");
                AddCopy(State);
                AddCopy(Output);
                AddCopy(Feedback);
                AddRow(BtnGripperOpen, BtnGripperClose);
                AddRow(BtnDo0On, BtnDo0Off, BtnDo1On, BtnDo1Off);
                AddRow(BtnToolDo0On, BtnToolDo0Off, BtnToolDo1On, BtnToolDo1Off);
            }

            public void SetEnabled(bool enabled)
            {
                BtnGripperOpen.SetEnabled(enabled);
                BtnGripperClose.SetEnabled(enabled);
                BtnDo0On.SetEnabled(enabled);
                BtnDo0Off.SetEnabled(enabled);
                BtnDo1On.SetEnabled(enabled);
                BtnDo1Off.SetEnabled(enabled);
                BtnToolDo0On.SetEnabled(enabled);
                BtnToolDo0Off.SetEnabled(enabled);
                BtnToolDo1On.SetEnabled(enabled);
                BtnToolDo1Off.SetEnabled(enabled);
            }

            private void AddCopy(Label label)
            {
                label.AddToClassList("rc-panel-copy");
                label.AddToClassList("rc-panel-copy--compact");
                Root.Add(label);
            }

            private void AddRow(params Button[] buttons)
            {
                var row = new VisualElement();
                row.AddToClassList("rc-point-action-row");
                for (var i = 0; i < buttons.Length; i++)
                {
                    buttons[i].AddToClassList("rc-point-action-button");
                    row.Add(buttons[i]);
                }

                Root.Add(row);
            }
        }
    }
}
