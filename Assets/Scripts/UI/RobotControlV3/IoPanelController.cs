// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 그리퍼 개도 조작 패널을 구성합니다.
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
                ApplyGripperPosition(panel, draftPositionPercent);
            });
            panel.PositionInput.RegisterValueChangedCallback(evt =>
            {
                draftPositionPercent = Mathf.Clamp(evt.newValue, 0, 100);
                panel.PositionSlider.SetValueWithoutNotify(draftPositionPercent);
                panel.PositionInput.SetValueWithoutNotify(draftPositionPercent);
                ApplyGripperPosition(panel, draftPositionPercent);
            });
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
            public readonly SliderInt PositionSlider = new("위치 %", 0, 100) { name = "GripperPositionSlider", value = 100 };
            public readonly IntegerField PositionInput = new("위치") { name = "GripperPositionInput", value = 100 };
            public readonly Button BtnGripperApply = new() { name = "BtnIoGripperApply", text = "위치 적용" };
            public readonly Button BtnGripperOpen = new() { name = "BtnIoGripperOpen", text = "그리퍼 열기" };
            public readonly Button BtnGripperClose = new() { name = "BtnIoGripperClose", text = "그리퍼 닫기" };

            public PanelElements()
            {
                Root.Add(new Label("그리퍼") { name = "IoPanelTitle" });
                Root.Q<Label>("IoPanelTitle").AddToClassList("rc-panel-title");
                PositionSlider.AddToClassList("rc-speed-slider");
                PositionInput.AddToClassList("rc-point-field");
                Root.Add(PositionSlider);
                AddRow(PositionInput, BtnGripperApply);
                AddRow(BtnGripperOpen, BtnGripperClose);
            }

            public void SetEnabled(bool enabled)
            {
                PositionSlider.SetEnabled(enabled);
                PositionInput.SetEnabled(enabled);
                BtnGripperApply.SetEnabled(enabled);
                BtnGripperOpen.SetEnabled(enabled);
                BtnGripperClose.SetEnabled(enabled);
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
