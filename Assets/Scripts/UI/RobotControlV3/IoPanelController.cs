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
        private int draftPositionPercent = 100;

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
            isDesktopVisible = activeNavSection == "NavMotion" && activeWorkTab == "TabEasyMotion";
            isTabletVisible = activeNavSection == "NavMotion" && activeTabletTab == "BottomTabEasyMotion";
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
            RegisterClick(panel.BtnGripperOpen, () => ApplyGripperPosition(panel, 100));
            RegisterClick(panel.BtnGripperClose, () => ApplyGripperPosition(panel, 0));
            RegisterClick(panel.BtnGripperApply, () => ApplyGripperPosition(panel, panel.PositionInput.value));
            panel.PositionSlider.RegisterValueChangedCallback(evt =>
            {
                draftPositionPercent = Mathf.Clamp(evt.newValue, 0, 100);
                panel.PositionInput.SetValueWithoutNotify(draftPositionPercent);
            });
            panel.PositionInput.RegisterValueChangedCallback(evt =>
            {
                draftPositionPercent = Mathf.Clamp(evt.newValue, 0, 100);
                panel.PositionSlider.SetValueWithoutNotify(draftPositionPercent);
                panel.PositionInput.SetValueWithoutNotify(draftPositionPercent);
            });
            RegisterClick(panel.BtnDo0On, () => runtimeController.SetRobotDigitalOutput(0, true));
            RegisterClick(panel.BtnDo0Off, () => runtimeController.SetRobotDigitalOutput(0, false));
            RegisterClick(panel.BtnDo1On, () => runtimeController.SetRobotDigitalOutput(1, true));
            RegisterClick(panel.BtnDo1Off, () => runtimeController.SetRobotDigitalOutput(1, false));
            RegisterClick(panel.BtnToolDo0On, () => runtimeController.SetToolDigitalOutput(0, true));
            RegisterClick(panel.BtnToolDo0Off, () => runtimeController.SetToolDigitalOutput(0, false));
            RegisterClick(panel.BtnToolDo1On, () => runtimeController.SetToolDigitalOutput(1, true));
            RegisterClick(panel.BtnToolDo1Off, () => runtimeController.SetToolDigitalOutput(1, false));
        }

        private static void RegisterClick(Button button, System.Action handler)
        {
            if (button == null || handler == null)
            {
                return;
            }

            button.clicked += handler;
        }

        private void ApplyGripperPosition(PanelElements panel, int positionPercent)
        {
            draftPositionPercent = Mathf.Clamp(positionPercent, 0, 100);
            panel?.PositionSlider.SetValueWithoutNotify(draftPositionPercent);
            panel?.PositionInput.SetValueWithoutNotify(draftPositionPercent);
            runtimeController.SetGripperPositionPercent(draftPositionPercent);
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

            draftPositionPercent = snapshot.GripperCommandedPositionPercent;
            panel.PositionSlider.SetValueWithoutNotify(snapshot.GripperCommandedPositionPercent);
            panel.PositionInput.SetValueWithoutNotify(snapshot.GripperCommandedPositionPercent);
            panel.State.text = $"{snapshot.GripperSummary} · visual={(snapshot.GripperVisualAttached ? "attached" : "no visual")}";
            panel.GripSafety.text = snapshot.GripperObjectDetected
                ? $"물체 감지 {(snapshot.GripperHoldingObject ? "· 잡은 상태" : $"· 정지선 {snapshot.GripperObjectStopPercent}%")}"
                : "물체 감지 없음 · 0%까지 완전 닫힘";
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
            public readonly Label GripSafety = new("물체 감지 없음 · 0%까지 완전 닫힘");
            public readonly Label Output = new("DO0 OFF / DO1 OFF");
            public readonly Label Feedback = new("주변장치 조작 전");
            public readonly SliderInt PositionSlider = new("위치 %", 0, 100) { name = "GripperPositionSlider", value = 100 };
            public readonly IntegerField PositionInput = new("위치") { name = "GripperPositionInput", value = 100 };
            public readonly Button BtnGripperApply = new() { name = "BtnIoGripperApply", text = "위치 적용" };
            public readonly Button BtnGripperOpen = new() { name = "BtnIoGripperOpen", text = "그리퍼 열기" };
            public readonly Button BtnGripperClose = new() { name = "BtnIoGripperClose", text = "그리퍼 닫기" };
            public readonly Button BtnDo0On = new() { name = "BtnRobotDo0On", text = "DO0 ON" };
            public readonly Button BtnDo0Off = new() { name = "BtnRobotDo0Off", text = "DO0 OFF" };
            public readonly Button BtnDo1On = new() { name = "BtnRobotDo1On", text = "DO1 ON" };
            public readonly Button BtnDo1Off = new() { name = "BtnRobotDo1Off", text = "DO1 OFF" };
            public readonly Button BtnToolDo0On = new() { name = "BtnToolDo0On", text = "TDO0 ON" };
            public readonly Button BtnToolDo0Off = new() { name = "BtnToolDo0Off", text = "TDO0 OFF" };
            public readonly Button BtnToolDo1On = new() { name = "BtnToolDo1On", text = "TDO1 ON" };
            public readonly Button BtnToolDo1Off = new() { name = "BtnToolDo1Off", text = "TDO1 OFF" };

            public PanelElements()
            {
                Root.Add(new Label("그리퍼 / I/O") { name = "IoPanelTitle" });
                Root.Q<Label>("IoPanelTitle").AddToClassList("rc-panel-title");
                AddCopy(State);
                AddCopy(GripSafety);
                PositionSlider.AddToClassList("rc-speed-slider");
                PositionInput.AddToClassList("rc-point-field");
                Root.Add(PositionSlider);
                AddRow(PositionInput, BtnGripperApply);
                AddCopy(Output);
                AddCopy(Feedback);
                AddRow(BtnGripperOpen, BtnGripperClose);
                AddRow(BtnDo0On, BtnDo0Off, BtnDo1On, BtnDo1Off);
                AddRow(BtnToolDo0On, BtnToolDo0Off, BtnToolDo1On, BtnToolDo1Off);
            }

            public void SetEnabled(bool enabled)
            {
                PositionSlider.SetEnabled(enabled);
                PositionInput.SetEnabled(enabled);
                BtnGripperApply.SetEnabled(enabled);
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

            private void AddRow(params VisualElement[] elements)
            {
                var row = new VisualElement();
                row.AddToClassList("rc-point-action-row");
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].AddToClassList("rc-point-action-button");
                    row.Add(elements[i]);
                }

                Root.Add(row);
            }
        }
    }
}
