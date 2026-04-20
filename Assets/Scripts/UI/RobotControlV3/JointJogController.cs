// Folder: UI - HUD/view components only; no kinematics logic.
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 관절 조그 패널 첫 슬라이스를 desktop/tablet host에 주입합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ConnectionHomeController))]
    public sealed class JointJogController : MonoBehaviour
    {
        private static readonly JointAxisSpec[] AxisSpecs =
        {
            new("J1", -170f, 170f),
            new("J2", -120f, 120f),
            new("J3", -225f, 225f),
            new("J4", -360f, 360f),
            new("J5", -360f, 360f),
            new("J6", -360f, 360f),
        };

        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset jointJogTemplate;

        private readonly float[] currentValues = new float[AxisSpecs.Length];

        private VisualElement root;
        private VisualElement workPanelBody;
        private VisualElement bottomSheetBody;
        private VisualElement jointJogPanelHost;
        private VisualElement jointJogSheetHost;
        private ConnectionHomeController connectionHomeController;
        private PopupCoordinatorV3 popupCoordinator;
        private PendantV3VisualizationOrchestrator visualizationOrchestrator;
        private RobotControlMotionRuntime motionRuntime;

        private PanelElements desktopPanel;
        private PanelElements tabletPanel;
        private bool useSingleAxisMode;
        private bool isDesktopVisible;
        private bool isTabletVisible;
        private bool isInitialized;
        private Coroutine initializeCoroutine;
        private string lastFeedback = "미리보기부터 확인해라.";

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
            isDesktopVisible = activeNavSection == "NavMotion" && activeWorkTab == "TabJointJog";
            isTabletVisible = activeNavSection == "NavMotion" && activeTabletTab == "BottomTabJointJog";
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

        public string GetDebugSummary()
        {
            return $"initialized={isInitialized}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}; mode={(useSingleAxisMode ? "SingleAxis" : "Slider")}; canApply={connectionHomeController?.ActualMoveAllowed ?? false}; j1={currentValues[0]:0.0}; j6={currentValues[5]:0.0}; feedback={lastFeedback}";
        }

        public string GetJointRowDebugSummary(int axisNumber)
        {
            var row = GetActiveRow(axisNumber);
            if (row == null)
            {
                return $"axis={axisNumber}; row=missing";
            }

            return BuildRowDebugSummary(axisNumber, row);
        }

        public string FocusJointInputForDebug(int axisNumber)
        {
            var row = GetActiveRow(axisNumber);
            if (row == null)
            {
                return $"axis={axisNumber}; row=missing";
            }

            row.Input.Focus();
            row.Input.SelectAll();
            return BuildRowDebugSummary(axisNumber, row);
        }

        public string SetJointSliderForDebug(int axisNumber, float value)
        {
            var row = GetActiveRow(axisNumber);
            if (row == null)
            {
                return $"axis={axisNumber}; row=missing";
            }

            row.Slider.value = value;
            return BuildRowDebugSummary(axisNumber, row);
        }

        public string SetJointInputForDebug(int axisNumber, string rawValue)
        {
            var row = GetActiveRow(axisNumber);
            if (row == null)
            {
                return $"axis={axisNumber}; row=missing";
            }

            row.Input.value = rawValue;
            return BuildRowDebugSummary(axisNumber, row);
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            popupCoordinator ??= GetComponent<PopupCoordinatorV3>();
            visualizationOrchestrator ??= GetComponent<PendantV3VisualizationOrchestrator>();
            root = document?.rootVisualElement;
            if (root == null || jointJogTemplate == null || connectionHomeController == null)
            {
                return false;
            }

            CacheShellElements();
            if (jointJogPanelHost == null || jointJogSheetHost == null)
            {
                isInitialized = false;
                return false;
            }

            if (desktopPanel == null || tabletPanel == null || jointJogPanelHost.childCount == 0 || jointJogSheetHost.childCount == 0)
            {
                BuildPanels();
            }

            ApplyShellStateSnapshot();
            connectionHomeController.PreviewChanged -= ApplyPreview;
            connectionHomeController.PreviewChanged += ApplyPreview;
            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            ApplyModeState();
            ApplyVisibility();
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

        private void CacheShellElements()
        {
            workPanelBody = root.Q<VisualElement>("WorkPanelBody");
            bottomSheetBody = root.Q<VisualElement>("BottomSheetBody");
            jointJogPanelHost = root.Q<VisualElement>("JointJogPanelHost");
            jointJogSheetHost = root.Q<VisualElement>("JointJogSheetHost");
        }

        private void BuildPanels()
        {
            desktopPanel = CreatePanel(jointJogPanelHost);
            tabletPanel = CreatePanel(jointJogSheetHost);
        }

        private PanelElements CreatePanel(VisualElement host)
        {
            if (host == null)
            {
                return null;
            }

            host.Clear();
            var tree = jointJogTemplate.CloneTree();
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

            panel.BtnModeSlider.clicked += () => SetMode(singleAxis: false);
            panel.BtnModeSingleAxis.clicked += () => SetMode(singleAxis: true);
            panel.BtnRestore.clicked += ResetFromPreview;
            panel.BtnPreview.clicked += PreviewJointGhost;
            panel.BtnApply.clicked += ApplyJointMove;

            for (var index = 0; index < panel.Rows.Length; index++)
            {
                var capturedIndex = index;
                panel.Rows[index].MinusButton.clicked += () => AdjustJoint(capturedIndex, -GetIncrementDegrees());
                panel.Rows[index].PlusButton.clicked += () => AdjustJoint(capturedIndex, GetIncrementDegrees());
                panel.Rows[index].Slider.RegisterValueChangedCallback(evt => SetJointValue(capturedIndex, evt.newValue));
                panel.Rows[index].Input.RegisterCallback<FocusInEvent>(_ => panel.Rows[capturedIndex].Input.SelectAll());
                panel.Rows[index].Input.RegisterValueChangedCallback(evt => HandleTextChanged(capturedIndex, evt.newValue));
                panel.Rows[index].Input.RegisterCallback<FocusOutEvent>(_ => SyncRowValue(panel.Rows[capturedIndex], capturedIndex));
            }
        }

        private void ApplyShellStateSnapshot()
        {
            var shellStateController = GetComponent<PendantV3ShellStateController>();
            var localState = shellStateController != null
                ? shellStateController.GetStateSnapshot()
                : PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault());
            isDesktopVisible = localState.ActiveNavSection == "NavMotion" && localState.ActiveWorkTab == "TabJointJog";
            isTabletVisible = localState.ActiveNavSection == "NavMotion" && localState.ActiveTabletTab == "BottomTabJointJog";
        }

        private void ApplyPreview(PendantV3PreviewState.Definition data)
        {
            for (var index = 0; index < AxisSpecs.Length && index < data.JointValues.Length; index++)
            {
                currentValues[index] = ParseJointValue(data.JointValues[index]);
            }

            ApplyPanelState(desktopPanel, data);
            ApplyPanelState(tabletPanel, data);
        }

        private void ApplyPanelState(PanelElements panel, PendantV3PreviewState.Definition data)
        {
            if (panel == null)
            {
                return;
            }

            panel.IncrementSummary.text = $"증분: {GetIncrementDegrees():0.#}°";
            panel.SpeedSummary.text = $"속도: {PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault()).SpeedPercent}%";
            panel.Hint.text = useSingleAxisMode
                ? $"단일축 조그에서는 J- / J+를 먼저 미리보기로 확인한다. {lastFeedback}"
                : $"슬라이더나 입력칸으로 목표값을 바꾸고 ghost donor를 먼저 확인한다. {lastFeedback}";

            var canPreview = connectionHomeController.CurrentPreviewState is not PendantV3PreviewState.Kind.Disconnected and not PendantV3PreviewState.Kind.AutoReconnect;
            var canApply = connectionHomeController.CurrentPreviewState == PendantV3PreviewState.Kind.ReadyToJog;
            panel.BtnPreview.SetEnabled(canPreview);
            panel.BtnApply.SetEnabled(canApply);

            for (var index = 0; index < panel.Rows.Length; index++)
            {
                SyncRowValue(panel.Rows[index], index);
            }
        }

        private void SetMode(bool singleAxis)
        {
            useSingleAxisMode = singleAxis;
            ApplyModeState();
        }

        private void ApplyModeState()
        {
            ApplyModeState(desktopPanel);
            ApplyModeState(tabletPanel);
        }

        private void ApplyModeState(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            panel.BtnModeSlider.EnableInClassList("rc-joint-mode-button--active", !useSingleAxisMode);
            panel.BtnModeSingleAxis.EnableInClassList("rc-joint-mode-button--active", useSingleAxisMode);
            foreach (var row in panel.Rows)
            {
                row.SliderGroup.EnableInClassList("rc-hidden", useSingleAxisMode);
            }
        }

        private void HandleTextChanged(int index, string rawValue)
        {
            if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue))
            {
                return;
            }

            SetJointValue(index, parsedValue);
        }

        private void AdjustJoint(int index, float delta)
        {
            SetJointValue(index, currentValues[index] + delta);
        }

        private void SetJointValue(int index, float value)
        {
            currentValues[index] = Mathf.Clamp(value, AxisSpecs[index].MinDegrees, AxisSpecs[index].MaxDegrees);
            visualizationOrchestrator?.PreviewJointPose(BuildJointTarget(), index, $"관절 ghost · {AxisSpecs[index].Label} {currentValues[index]:0.0}°", false);
            SyncAllRows(index);
        }

        private void SyncAllRows(int index)
        {
            SyncRowValue(desktopPanel?.Rows[index], index);
            SyncRowValue(tabletPanel?.Rows[index], index);
            ApplyPanelState(desktopPanel, connectionHomeController.CurrentPreviewDefinition);
            ApplyPanelState(tabletPanel, connectionHomeController.CurrentPreviewDefinition);
        }

        private void SyncRowValue(JointRowElements row, int index)
        {
            if (row == null)
            {
                return;
            }

            var value = currentValues[index];
            row.Slider.SetValueWithoutNotify(value);
            row.Input.SetValueWithoutNotify(value.ToString("0.0", CultureInfo.InvariantCulture));
            row.Value.text = $"{value:0.0}°";
            var warningThreshold = Mathf.Abs(AxisSpecs[index].MaxDegrees) * 0.9f;
            row.Value.EnableInClassList("rc-joint-value--warning", Mathf.Abs(value) >= warningThreshold);
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

            jointJogPanelHost?.EnableInClassList("rc-hidden", !isDesktopVisible);
            jointJogSheetHost?.EnableInClassList("rc-hidden", !isTabletVisible);
        }

        private void ResetFromPreview()
        {
            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            visualizationOrchestrator?.ClearPreview();
            lastFeedback = "현재 자세로 다시 맞췄다.";
            ApplyPanelState(desktopPanel, connectionHomeController.CurrentPreviewDefinition);
            ApplyPanelState(tabletPanel, connectionHomeController.CurrentPreviewDefinition);
        }

        private void PreviewJointGhost()
        {
            visualizationOrchestrator?.PreviewJointPose(BuildJointTarget(), FindDominantJointIndex(), $"MoveJ preview · J1 {currentValues[0]:0.0} / J2 {currentValues[1]:0.0} / J3 {currentValues[2]:0.0}", false);
            lastFeedback = "joint ghost와 하이라이트를 갱신했다.";
            ApplyPanelState(desktopPanel, connectionHomeController.CurrentPreviewDefinition);
            ApplyPanelState(tabletPanel, connectionHomeController.CurrentPreviewDefinition);
        }

        private void ApplyJointMove()
        {
            if (!connectionHomeController.ActualMoveAllowed)
            {
                lastFeedback = connectionHomeController.ActualMoveBlockReason;
                ApplyPanelState(desktopPanel, connectionHomeController.CurrentPreviewDefinition);
                ApplyPanelState(tabletPanel, connectionHomeController.CurrentPreviewDefinition);
                return;
            }

            var target = BuildJointTarget();
            void Execute()
            {
                var runtimeResult = EnsureMotionRuntime();
                if (!runtimeResult.IsSuccess)
                {
                    lastFeedback = runtimeResult.Message;
                }
                else
                {
                    var speed = PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault()).SpeedPercent;
                    var result = motionRuntime.DispatchMoveJ(target, speed);
                    lastFeedback = result.IsSuccess
                        ? $"MoveJ 실행 완료 · J1 {target[0]:0.0} / J2 {target[1]:0.0} / J3 {target[2]:0.0}"
                        : $"MoveJ 실패 · {result.Message}";
                    if (result.IsSuccess)
                    {
                        visualizationOrchestrator?.SetRuntimePose(target, connectionHomeController.CurrentSessionState.IsConnected ? GetCurrentTcpPoseFallback() : null);
                        visualizationOrchestrator?.ClearPreview();
                    }
                }

                ApplyPanelState(desktopPanel, connectionHomeController.CurrentPreviewDefinition);
                ApplyPanelState(tabletPanel, connectionHomeController.CurrentPreviewDefinition);
            }

            if (popupCoordinator != null)
            {
                popupCoordinator.OpenMoveConfirmForPolicy("JointJog 실행 확인", $"J1 {target[0]:0.0} / J2 {target[1]:0.0} / J3 {target[2]:0.0}", Execute, "MoveJ 실행");
                return;
            }

            Execute();
        }

        private FairinoResult<RobotControlMotionRuntime> EnsureMotionRuntime()
        {
            var robotId = RobotSelectionBridge.GetSelectedRobotId();
            if (string.IsNullOrWhiteSpace(robotId))
            {
                motionRuntime = null;
                return FairinoResult<RobotControlMotionRuntime>.Fail(-1, "선택된 로봇이 없어서 JointJog runtime을 준비하지 못했다.");
            }

            if (motionRuntime != null && motionRuntime.RobotId == robotId)
            {
                return FairinoResult<RobotControlMotionRuntime>.Ok(motionRuntime, "joint runtime 재사용");
            }

            var createResult = RobotControlMotionRuntime.CreateFromSelection();
            if (!createResult.IsSuccess)
            {
                motionRuntime = null;
                return createResult;
            }

            motionRuntime = createResult.Value;
            return createResult;
        }

        private double[] BuildJointTarget()
        {
            var target = new double[currentValues.Length];
            for (var index = 0; index < currentValues.Length; index++)
            {
                target[index] = currentValues[index];
            }

            return target;
        }

        private int FindDominantJointIndex()
        {
            var maxDelta = 0f;
            var maxIndex = 0;
            var preview = connectionHomeController.CurrentPreviewDefinition.JointValues;
            for (var index = 0; index < currentValues.Length && index < preview.Length; index++)
            {
                var previewValue = ParseJointValue(preview[index]);
                var delta = Mathf.Abs(currentValues[index] - previewValue);
                if (delta > maxDelta)
                {
                    maxDelta = delta;
                    maxIndex = index;
                }
            }

            return maxIndex;
        }

        private double[] GetCurrentTcpPoseFallback()
        {
            var values = connectionHomeController.CurrentPreviewDefinition.TcpValues;
            var target = new double[values.Length];
            for (var index = 0; index < values.Length; index++)
            {
                target[index] = ParseJointValue(values[index]);
            }

            return target;
        }

        private JointRowElements GetActiveRow(int axisNumber)
        {
            var index = Mathf.Clamp(axisNumber - 1, 0, AxisSpecs.Length - 1);
            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            if (panel == null || panel.Rows == null || index >= panel.Rows.Length)
            {
                return null;
            }

            return panel.Rows[index];
        }

        private string BuildRowDebugSummary(int axisNumber, JointRowElements row)
        {
            var sliderValue = row.Slider.value;
            var inputValue = row.Input.value;
            var labelValue = row.Value.text;
            var focused = row.Input.panel?.focusController?.focusedElement == row.Input;
            return $"axis={axisNumber}; slider={sliderValue:0.0}; input={inputValue}; label={labelValue}; focused={focused}; cursor={row.Input.cursorIndex}; select={row.Input.selectIndex}";
        }

        private static float ParseJointValue(string rawValue)
        {
            return float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0f;
        }

        private static float GetIncrementDegrees()
        {
            return PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault()).JogIncrement;
        }

        private readonly struct JointAxisSpec
        {
            public JointAxisSpec(string label, float minDegrees, float maxDegrees)
            {
                Label = label;
                MinDegrees = minDegrees;
                MaxDegrees = maxDegrees;
            }

            public string Label { get; }
            public float MinDegrees { get; }
            public float MaxDegrees { get; }
        }

        private sealed class PanelElements
        {
            public PanelElements(VisualElement root)
            {
                BtnModeSlider = root.Q<Button>("BtnJointModeSlider");
                BtnModeSingleAxis = root.Q<Button>("BtnJointModeSingleAxis");
                Hint = root.Q<Label>("JointJogHint");
                IncrementSummary = root.Q<Label>("JointIncrementSummary");
                SpeedSummary = root.Q<Label>("JointSpeedSummary");
                BtnRestore = root.Q<Button>("BtnJointRestore");
                BtnPreview = root.Q<Button>("BtnJointPreview");
                BtnApply = root.Q<Button>("BtnJointApply");
                Rows = new[]
                {
                    new JointRowElements(root, 1),
                    new JointRowElements(root, 2),
                    new JointRowElements(root, 3),
                    new JointRowElements(root, 4),
                    new JointRowElements(root, 5),
                    new JointRowElements(root, 6),
                };
            }

            public Button BtnModeSlider { get; }
            public Button BtnModeSingleAxis { get; }
            public Label Hint { get; }
            public Label IncrementSummary { get; }
            public Label SpeedSummary { get; }
            public Button BtnRestore { get; }
            public Button BtnPreview { get; }
            public Button BtnApply { get; }
            public JointRowElements[] Rows { get; }
        }

        private sealed class JointRowElements
        {
            public JointRowElements(VisualElement root, int axisNumber)
            {
                SliderGroup = root.Q<VisualElement>($"JointSliderGroup{axisNumber}");
                Slider = root.Q<Slider>($"JointSlider{axisNumber}");
                MinusButton = root.Q<Button>($"BtnJoint{axisNumber}Minus");
                PlusButton = root.Q<Button>($"BtnJoint{axisNumber}Plus");
                Input = root.Q<TextField>($"JointInput{axisNumber}");
                Value = root.Q<Label>($"JointValue{axisNumber}");
            }

            public VisualElement SliderGroup { get; }
            public Slider Slider { get; }
            public Button MinusButton { get; }
            public Button PlusButton { get; }
            public TextField Input { get; }
            public Label Value { get; }
        }
    }
}
