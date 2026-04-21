// Folder: UI - HUD/view components only; no kinematics logic.
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 TCP 조그 패널과 3D 방향 오버레이 첫 슬라이스를 주입합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ConnectionHomeController))]
    public sealed partial class TcpJogController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset tcpJogTemplate;
        [SerializeField] private VisualTreeAsset cartesianArrowsOverlayTemplate;

        private readonly float[] currentValues = new float[AxisSpecs.Length];

        private VisualElement root;
        private VisualElement workPanelBody;
        private VisualElement bottomSheetBody;
        private VisualElement tcpJogPanelHost;
        private VisualElement tcpJogSheetHost;
        private VisualElement cartesianOverlayHost;
        private ConnectionHomeController connectionHomeController;
        private RobotControlV3RuntimeController runtimeController;

        private PanelElements desktopPanel;
        private PanelElements tabletPanel;
        private OverlayElements overlay;
        private string activeCoordSystem = PendantV3LocalState.DefaultCoordSystem;
        private int highlightedAxis = -1;
        private int highlightedDirection = 1;
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

        public void SetShellState(string activeNavSection, string activeWorkTab, string activeTabletTab)
        {
            isDesktopVisible = activeNavSection == "NavMotion" && activeWorkTab == "TabTcpJog";
            isTabletVisible = activeNavSection == "NavMotion" && activeTabletTab == "BottomTabTcpJog";
            activeCoordSystem = GetLocalState().CoordSystem;
            if (!isInitialized)
            {
                TryInitialize();
            }

            ApplyAll();
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            var activeAxis = highlightedAxis >= 0 ? $"{AxisSpecs[highlightedAxis].Label}{(highlightedDirection > 0 ? "+" : "-")}" : "none";
            var overlayHidden = cartesianOverlayHost?.ClassListContains("rc-hidden") ?? true;
            return $"initialized={isInitialized}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}; coord={activeCoordSystem}; increment={GetIncrementValue():0.#}; activeAxis={activeAxis}; overlayHidden={overlayHidden}; x={currentValues[0]:0.0}; rz={currentValues[5]:0.0}";
        }

        public string NudgeAxisForDebug(string axisLabel, int direction)
        {
            var index = ResolveAxisIndex(axisLabel);
            AdjustAxis(index, direction >= 0 ? 1 : -1);
            return GetDebugSummary();
        }

        public string SetCoordSystemForDebug(string coordSystem)
        {
            SetCoordSystem(coordSystem);
            return GetDebugSummary();
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            root = document?.rootVisualElement;
            if (root == null || tcpJogTemplate == null || cartesianArrowsOverlayTemplate == null || connectionHomeController == null || runtimeController == null)
            {
                return false;
            }

            workPanelBody = root.Q<VisualElement>("WorkPanelBody");
            bottomSheetBody = root.Q<VisualElement>("BottomSheetBody");
            tcpJogPanelHost = root.Q<VisualElement>("TcpJogPanelHost");
            tcpJogSheetHost = root.Q<VisualElement>("TcpJogSheetHost");
            cartesianOverlayHost = root.Q<VisualElement>("CartesianArrowsOverlayHost");
            if (tcpJogPanelHost == null || tcpJogSheetHost == null || cartesianOverlayHost == null)
            {
                isInitialized = false;
                return false;
            }

            if (desktopPanel == null || tabletPanel == null || overlay == null || tcpJogPanelHost.childCount == 0 || tcpJogSheetHost.childCount == 0 || cartesianOverlayHost.childCount == 0)
            {
                desktopPanel = CreatePanel(tcpJogPanelHost);
                tabletPanel = CreatePanel(tcpJogSheetHost);
                overlay = CreateOverlay(cartesianOverlayHost);
            }

            var localState = GetLocalState();
            activeCoordSystem = localState.CoordSystem;
            isDesktopVisible = localState.ActiveNavSection == "NavMotion" && localState.ActiveWorkTab == "TabTcpJog";
            isTabletVisible = localState.ActiveNavSection == "NavMotion" && localState.ActiveTabletTab == "BottomTabTcpJog";
            connectionHomeController.PreviewChanged -= ApplyPreview;
            connectionHomeController.PreviewChanged += ApplyPreview;
            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            isInitialized = true;
            return true;
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

        private PanelElements CreatePanel(VisualElement host)
        {
            host.Clear();
            var tree = tcpJogTemplate.CloneTree();
            host.Add(tree);
            var panel = new PanelElements(tree);
            RegisterPanel(panel);
            return panel;
        }

        private OverlayElements CreateOverlay(VisualElement host)
        {
            host.Clear();
            var tree = cartesianArrowsOverlayTemplate.CloneTree();
            host.Add(tree);
            var result = new OverlayElements(tree);
            RegisterOverlay(result);
            return result;
        }

        private void RegisterPanel(PanelElements panel)
        {
            RegisterClick(panel.BtnCoordBase, () => SetCoordSystem("Base"));
            RegisterClick(panel.BtnCoordTool, () => SetCoordSystem("Tool"));
            RegisterClick(panel.BtnCoordUser, () => SetCoordSystem("User"));
            RegisterClick(panel.BtnPreview, PreviewCurrentPose);
            RegisterClick(panel.BtnApply, ApplyCurrentPose);
            for (var index = 0; index < panel.Rows.Length; index++)
            {
                var capturedIndex = index;
                RegisterClick(panel.Rows[index].MinusButton, () => AdjustAxis(capturedIndex, -1));
                RegisterClick(panel.Rows[index].PlusButton, () => AdjustAxis(capturedIndex, 1));
            }
        }

        private void RegisterOverlay(OverlayElements value)
        {
            for (var index = 0; index < value.Axes.Length; index++)
            {
                var capturedIndex = index;
                RegisterClick(value.Axes[index].MinusButton, () => AdjustAxis(capturedIndex, -1));
                RegisterClick(value.Axes[index].PlusButton, () => AdjustAxis(capturedIndex, 1));
            }
        }

        private static void RegisterClick(Button button, System.Action handler)
        {
            if (button == null || handler == null)
            {
                return;
            }

            button.RegisterCallback<ClickEvent>(_ => handler());
        }

        private void ApplyPreview(RobotControlV3RuntimeSnapshot data)
        {
            for (var index = 0; index < AxisSpecs.Length && index < data.TcpValues.Length; index++)
            {
                currentValues[index] = ParseValue(data.TcpValues[index]);
            }

            ApplyAll();
        }

        private void ApplyAll()
        {
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
            ApplyOverlay();
            ApplyVisibility();
        }

        private void ApplyPanel(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            panel.BtnCoordBase.EnableInClassList("rc-tcp-coord-button--active", activeCoordSystem == "Base");
            panel.BtnCoordTool.EnableInClassList("rc-tcp-coord-button--active", activeCoordSystem == "Tool");
            panel.BtnCoordUser.EnableInClassList("rc-tcp-coord-button--active", activeCoordSystem == "User");
            panel.Hint.text = GetCoordHint();
            panel.IncrementSummary.text = $"증분: {GetIncrementValue():0.#} mm / {GetIncrementValue():0.#}°";
            panel.SpeedSummary.text = $"속도: {GetLocalState().SpeedPercent}%";
            panel.OverlaySummary.text = highlightedAxis >= 0
                ? $"보조 조작 강조: {AxisSpecs[highlightedAxis].Label}{(highlightedDirection > 0 ? "+" : "-")} ({activeCoordSystem})"
                : "3D 방향 조작은 보조패널에서만 보여 로봇 메인 뷰를 가리지 않는다.";
            panel.BtnPreview.SetEnabled(CanPreview());
            panel.BtnApply.SetEnabled(CanApply());

            for (var index = 0; index < panel.Rows.Length; index++)
            {
                panel.Rows[index].Root.EnableInClassList("rc-tcp-row--active", highlightedAxis == index);
                panel.Rows[index].Value.text = currentValues[index].ToString("0.0", CultureInfo.InvariantCulture);
            }
        }

        private void ApplyOverlay()
        {
            if (overlay == null)
            {
                return;
            }

            overlay.CoordBadge.text = activeCoordSystem;
            overlay.Hint.text = GetCoordHint();
            overlay.Summary.text = highlightedAxis >= 0
                ? $"마지막 조작 축: {AxisSpecs[highlightedAxis].Label}{(highlightedDirection > 0 ? "+" : "-")} / 증분 {GetIncrementValue():0.#}"
                : "마지막 조작 축: 없음 / 메인 로봇 뷰는 계속 유지";
            for (var index = 0; index < overlay.Axes.Length; index++)
            {
                overlay.Axes[index].Root.EnableInClassList("rc-cartesian-axis--active", highlightedAxis == index);
                overlay.Axes[index].Value.text = $"{AxisSpecs[index].Label} {currentValues[index]:0.0}";
            }
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

            tcpJogPanelHost?.EnableInClassList("rc-hidden", !isDesktopVisible);
            tcpJogSheetHost?.EnableInClassList("rc-hidden", !isTabletVisible);
            cartesianOverlayHost?.EnableInClassList("rc-hidden", !(isDesktopVisible || isTabletVisible));
        }

        private void SetCoordSystem(string coordSystem)
        {
            activeCoordSystem = coordSystem is "Tool" or "User" ? coordSystem : "Base";
            var shellState = GetComponent<PendantV3ShellStateController>();
            if (shellState != null)
            {
                shellState.SetCoordSystemSelection(activeCoordSystem);
                return;
            }

            var localState = GetLocalState();
            localState.CoordSystem = activeCoordSystem;
            LocalSettingsStore.Save(localState);
            ApplyAll();
        }

        private void AdjustAxis(int index, int direction)
        {
            currentValues[index] = Mathf.Clamp(currentValues[index] + direction * GetIncrementValue(), AxisSpecs[index].MinValue, AxisSpecs[index].MaxValue);
            highlightedAxis = index;
            highlightedDirection = direction;
            ApplyAll();
            runtimeController?.PreviewTcpPose(ToTcpPoseArray(), $"TCP {AxisSpecs[index].Label}{(direction > 0 ? "+" : "-")} 프리뷰");
        }

        private PendantV3LocalState GetLocalState()
        {
            var shellState = GetComponent<PendantV3ShellStateController>();
            return shellState != null
                ? shellState.GetStateSnapshot()
                : PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault());
        }

        private float GetIncrementValue() => GetLocalState().JogIncrement;
        private bool CanPreview() => connectionHomeController.CurrentPreviewState is not PendantV3PreviewState.Kind.Disconnected and not PendantV3PreviewState.Kind.AutoReconnect;
        private bool CanApply() => connectionHomeController.CurrentPreviewState == PendantV3PreviewState.Kind.ReadyToJog;
        private static float ParseValue(string rawValue) => float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0f;

        private static int ResolveAxisIndex(string axisLabel)
        {
            for (var index = 0; index < AxisSpecs.Length; index++)
            {
                if (string.Equals(AxisSpecs[index].Label, axisLabel, System.StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return 0;
        }

        private string GetCoordHint()
        {
            return activeCoordSystem switch
            {
                "Tool" => "Tool은 공구 끝 방향 기준으로 미세 접근할 때 감각이 가장 잘 맞는다.",
                "User" => "User는 작업대 기준 좌표를 맞출 때 쓰고, 아직 값이 없다면 Base부터 확인한다.",
                _ => "Base는 베이스 기준으로 앞/좌/상 감각을 먼저 잡기 좋다.",
            };
        }

        private void PreviewCurrentPose()
        {
            runtimeController?.PreviewTcpPose(ToTcpPoseArray(), $"TCP {activeCoordSystem} 미리보기");
        }

        private void ApplyCurrentPose()
        {
            runtimeController?.ApplyTcpPose(ToTcpPoseArray(), $"TCP {activeCoordSystem} 적용");
        }

        private double[] ToTcpPoseArray()
        {
            var result = new double[currentValues.Length];
            for (var i = 0; i < currentValues.Length; i++)
            {
                result[i] = currentValues[i];
            }

            return result;
        }
    }
}
