// Folder: UI - HUD/view components only; no kinematics logic.
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 포인트 이동 패널 최소 scaffold를 desktop/tablet host에 주입합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ConnectionHomeController))]
    public sealed partial class PointMoveController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset pointMoveTemplate;

        private readonly float[] currentValues = new float[6];

        private VisualElement root;
        private VisualElement workPanelBody;
        private VisualElement bottomSheetBody;
        private VisualElement pointMovePanelHost;
        private VisualElement pointMoveSheetHost;
        private ConnectionHomeController connectionHomeController;
        private RobotControlMotionRuntime motionRuntime;
        private string lastFeedback = "아직 실행한 명령이 없다.";
        private int lastInvalidIndex = -1;
        private bool isPointNameInvalid;

        private PanelElements desktopPanel;
        private PanelElements tabletPanel;
        private string activeCoordSystem = PendantV3LocalState.DefaultCoordSystem;
        private string motionKind = "MoveJ";
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
            isDesktopVisible = activeNavSection == "NavMotion" && activeWorkTab == "TabPointMove";
            isTabletVisible = activeNavSection == "NavMotion" && activeTabletTab == "BottomTabPointMove";
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

        public string SetMotionKindForDebug(string nextMotionKind)
        {
            SetMotionKind(nextMotionKind);
            return GetDebugSummary();
        }

        public string PreviewForDebug()
        {
            PreviewMotionCandidate();
            return GetDebugSummary();
        }

        public string ApplyForDebug()
        {
            ApplyMotionCandidate();
            return GetDebugSummary();
        }

        public string GetDebugSummary()
        {
            var pointName = desktopPanel?.PointNameInput?.value ?? tabletPanel?.PointNameInput?.value ?? "Point";
            var runtimeRobot = motionRuntime?.RobotId ?? "none";
            var canPreviewAction = CanPreview() && IsAnyPanelVisible();
            var canApplyAction = CanApply() && IsMoveLDispatchMode() && IsAnyPanelVisible();
            return $"initialized={isInitialized}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}; coord={activeCoordSystem}; motion={motionKind}; previewState={connectionHomeController.CurrentPreviewState}; canPreview={canPreviewAction}; canApply={canApplyAction}; runtimeRobot={runtimeRobot}; name={pointName}; x={currentValues[0]:0.0}; rz={currentValues[5]:0.0}; feedback={lastFeedback}";
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            root = document?.rootVisualElement;
            if (root == null || pointMoveTemplate == null || connectionHomeController == null)
            {
                return false;
            }

            workPanelBody = root.Q<VisualElement>("WorkPanelBody");
            bottomSheetBody = root.Q<VisualElement>("BottomSheetBody");
            pointMovePanelHost = root.Q<VisualElement>("PointMovePanelHost");
            pointMoveSheetHost = root.Q<VisualElement>("PointMoveSheetHost");
            if (pointMovePanelHost == null || pointMoveSheetHost == null)
            {
                isInitialized = false;
                return false;
            }

            if (desktopPanel == null || tabletPanel == null || pointMovePanelHost.childCount == 0 || pointMoveSheetHost.childCount == 0)
            {
                desktopPanel = CreatePanel(pointMovePanelHost);
                tabletPanel = CreatePanel(pointMoveSheetHost);
            }

            var localState = GetLocalState();
            activeCoordSystem = localState.CoordSystem;
            isDesktopVisible = localState.ActiveNavSection == "NavMotion" && localState.ActiveWorkTab == "TabPointMove";
            isTabletVisible = localState.ActiveNavSection == "NavMotion" && localState.ActiveTabletTab == "BottomTabPointMove";
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
            var tree = pointMoveTemplate.CloneTree();
            host.Add(tree);
            var panel = new PanelElements(tree);
            RegisterPanel(panel);
            return panel;
        }

        private void RegisterPanel(PanelElements panel)
        {
            RegisterClick(panel.BtnCoordBase, () => SetCoordSystem("Base"));
            RegisterClick(panel.BtnCoordTool, () => SetCoordSystem("Tool"));
            RegisterClick(panel.BtnCoordUser, () => SetCoordSystem("User"));
            RegisterClick(panel.BtnMoveJ, () => SetMotionKind("MoveJ"));
            RegisterClick(panel.BtnMoveL, () => SetMotionKind("MoveL"));
            RegisterClick(panel.BtnRestore, RestoreFromPreview);
            RegisterClick(panel.BtnPreview, PreviewMotionCandidate);
            RegisterClick(panel.BtnApply, ApplyMotionCandidate);
            for (var index = 0; index < panel.ValueInputs.Length; index++)
            {
                var capturedIndex = index;
                panel.ValueInputs[index].RegisterValueChangedCallback(evt => HandleValueChanged(capturedIndex, evt.newValue));
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

        private void ApplyPreview(PendantV3PreviewState.Definition data)
        {
            for (var index = 0; index < currentValues.Length && index < data.TcpValues.Length; index++)
            {
                currentValues[index] = ParseValue(data.TcpValues[index]);
            }

            isPointNameInvalid = false;
            ApplyAll();
        }

        private void ApplyAll()
        {
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
            ApplyVisibility();
        }

        private void ApplyPanel(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            panel.BtnCoordBase.EnableInClassList("rc-point-coord-button--active", activeCoordSystem == "Base");
            panel.BtnCoordTool.EnableInClassList("rc-point-coord-button--active", activeCoordSystem == "Tool");
            panel.BtnCoordUser.EnableInClassList("rc-point-coord-button--active", activeCoordSystem == "User");
            panel.BtnMoveJ.EnableInClassList("rc-point-motion-button--active", motionKind == "MoveJ");
            panel.BtnMoveL.EnableInClassList("rc-point-motion-button--active", motionKind == "MoveL");
            panel.Hint.text = motionKind == "MoveL"
                ? "직선 접근이 필요할 때는 MoveL 후보로 보고, 먼저 미리보기로 궤적 감각을 확인한다."
                : "관절 기준으로 먼저 접근해도 되는 위치라면 MoveJ 후보로 빠르게 확인한다.";
            panel.CoordSummary.text = $"좌표계: {activeCoordSystem} / 현재 TCP 기준으로 시작";
            panel.MotionSummary.text = motionKind == "MoveL"
                ? "이동 방식: MoveL / 공구 경로를 직선으로 먼저 확인"
                : "이동 방식: MoveJ / 관절 중심으로 먼저 후보를 확인";
            panel.PreviewSummary.text = BuildDeltaSummary(panel.PointNameInput.value);
            panel.FeedbackSummary.text = lastFeedback;
            var canPreview = CanPreview() && IsAnyPanelVisible();
            var canApply = CanApply() && IsMoveLDispatchMode() && IsAnyPanelVisible();
            panel.BtnRestore.SetEnabled(canPreview);
            panel.BtnPreview.SetEnabled(canPreview);
            panel.BtnApply.SetEnabled(canApply);
            panel.BtnApply.text = IsMoveLDispatchMode()
                ? (CanApply() ? "적용" : "적용 (연결 대기)")
                : "적용 (MoveJ 준비중)";
            panel.PointNameInput.EnableInClassList("rc-point-name-input--danger", isPointNameInvalid);

            for (var index = 0; index < panel.ValueInputs.Length && index < currentValues.Length; index++)
            {
                panel.ValueInputs[index].SetValueWithoutNotify(currentValues[index].ToString("0.0", CultureInfo.InvariantCulture));
                panel.ValueInputs[index].EnableInClassList("rc-point-cell-input--danger", index == lastInvalidIndex);
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

            pointMovePanelHost?.EnableInClassList("rc-hidden", !isDesktopVisible);
            pointMoveSheetHost?.EnableInClassList("rc-hidden", !isTabletVisible);
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

        private void SetMotionKind(string nextMotionKind)
        {
            motionKind = nextMotionKind == "MoveL" ? "MoveL" : "MoveJ";
            ApplyAll();
        }

        private void RestoreFromPreview()
        {
            if (!CanPreview() || !IsAnyPanelVisible())
            {
                SetFeedback("연결이 준비될 때까지 현재값 복원을 잠시 잠근다.");
                return;
            }

            isPointNameInvalid = false;
            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            SetFeedback("현재 preview TCP 값으로 다시 채웠다.");
        }

        private void HandleValueChanged(int index, string rawValue)
        {
            if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return;
            }

            currentValues[index] = parsed;
            lastInvalidIndex = -1;
            isPointNameInvalid = false;
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
        }

        private void PreviewMotionCandidate()
        {
            if (!IsAnyPanelVisible())
            {
                SetFeedback("포인트 이동 패널이 열려 있을 때만 미리보기를 실행한다.");
                return;
            }

            if (!CanPreview())
            {
                SetFeedback("연결 상태가 준비되지 않아 미리보기를 잠시 잠근다.");
                return;
            }

            if (!TryReadActivePanelValues(out var _, out var validationMessage))
            {
                SetFeedback(validationMessage);
                return;
            }

            var pointName = desktopPanel?.PointNameInput?.value ?? tabletPanel?.PointNameInput?.value ?? "Point";
            if (motionKind == "MoveL")
            {
                SetFeedback($"[Preview] MoveL 후보 · {pointName} · X {currentValues[0]:0.0} / Y {currentValues[1]:0.0} / Z {currentValues[2]:0.0}");
                return;
            }

            SetFeedback("[Preview] MoveJ 후보 · 현재는 IK 연결 전이라 관절 후보 계산만 후속으로 붙일 예정.");
        }

        private void ApplyMotionCandidate()
        {
            if (!IsAnyPanelVisible())
            {
                SetFeedback("포인트 이동 패널이 열려 있을 때만 적용할 수 있다.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 적용할 수 없다. 연결/에러 상태를 먼저 확인해라.");
                return;
            }

            if (!TryReadActivePanelValues(out var target, out var validationMessage))
            {
                SetFeedback(validationMessage);
                return;
            }

            if (motionKind != "MoveL")
            {
                SetFeedback("MoveJ 적용은 IK 연결 전이라 아직 보류 상태다. 지금은 MoveL만 실제 dispatch 된다.");
                return;
            }

            var runtimeResult = EnsureMotionRuntime();
            if (!runtimeResult.IsSuccess)
            {
                SetFeedback(runtimeResult.Message);
                return;
            }

            var state = GetLocalState();
            var speed = Mathf.Clamp(state.SpeedPercent, 1, 100);
            var result = motionRuntime.DispatchMoveL(target, speed);
            SetFeedback(result.IsSuccess
                ? $"[Dispatch] MoveL 완료 · speed {speed}% · X {target[0]:0.0} / Y {target[1]:0.0} / Z {target[2]:0.0}"
                : $"[Dispatch] MoveL 실패 · {result.Message}");
        }

        private FairinoResult<RobotControlMotionRuntime> EnsureMotionRuntime()
        {
            var robotId = RobotSelectionBridge.GetSelectedRobotId();
            if (string.IsNullOrWhiteSpace(robotId))
            {
                motionRuntime = null;
                return FairinoResult<RobotControlMotionRuntime>.Fail(-1, "선택된 로봇이 없어서 PointMove runtime을 준비하지 못했다.");
            }

            if (motionRuntime != null && string.Equals(motionRuntime.RobotId, robotId, System.StringComparison.Ordinal))
            {
                return FairinoResult<RobotControlMotionRuntime>.Ok(motionRuntime, $"{robotId} runtime 재사용");
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

        private void SetFeedback(string message)
        {
            lastFeedback = string.IsNullOrWhiteSpace(message) ? "..." : message;
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
        }

        private bool TryReadActivePanelValues(out double[] target, out string validationMessage)
        {
            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            target = new double[currentValues.Length];
            validationMessage = "입력 검증 통과";
            lastInvalidIndex = -1;
            isPointNameInvalid = false;

            if (panel == null)
            {
                validationMessage = "포인트 이동 패널을 찾지 못했다.";
                return false;
            }

            var pointName = panel.PointNameInput?.value?.Trim();
            if (string.IsNullOrWhiteSpace(pointName))
            {
                isPointNameInvalid = true;
                ApplyPanel(desktopPanel);
                ApplyPanel(tabletPanel);
                validationMessage = "포인트 이름을 먼저 넣어라.";
                return false;
            }

            for (var index = 0; index < panel.ValueInputs.Length && index < target.Length; index++)
            {
                var rawValue = panel.ValueInputs[index].value;
                if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                    || double.IsNaN(parsed)
                    || double.IsInfinity(parsed))
                {
                    lastInvalidIndex = index;
                    ApplyPanel(desktopPanel);
                    ApplyPanel(tabletPanel);
                    validationMessage = $"{GetAxisLabel(index)} 값 형식을 확인해라.";
                    return false;
                }

                if (index >= 3 && System.Math.Abs(parsed) > 360.0)
                {
                    lastInvalidIndex = index;
                    ApplyPanel(desktopPanel);
                    ApplyPanel(tabletPanel);
                    validationMessage = $"{GetAxisLabel(index)} 는 -360°~360° 범위 안으로 넣어라.";
                    return false;
                }

                target[index] = parsed;
            }

            for (var index = 0; index < currentValues.Length && index < target.Length; index++)
            {
                currentValues[index] = (float)target[index];
            }

            return true;
        }

        private string BuildDeltaSummary(string pointName)
        {
            var preview = connectionHomeController.CurrentPreviewDefinition;
            if (preview.TcpValues == null || preview.TcpValues.Length < 3)
            {
                return $"미리보기로 {pointName} 위치를 먼저 확인한 뒤 실제 이동을 보낸다.";
            }

            var dx = currentValues[0] - ParseValue(preview.TcpValues[0]);
            var dy = currentValues[1] - ParseValue(preview.TcpValues[1]);
            var dz = currentValues[2] - ParseValue(preview.TcpValues[2]);
            return $"미리보기 ΔTCP · X {dx:+0.0;-0.0;0.0} / Y {dy:+0.0;-0.0;0.0} / Z {dz:+0.0;-0.0;0.0}";
        }

        private static string GetAxisLabel(int index)
        {
            return index switch
            {
                0 => "X",
                1 => "Y",
                2 => "Z",
                3 => "RX",
                4 => "RY",
                _ => "RZ",
            };
        }

        private PendantV3LocalState GetLocalState()
        {
            var shellState = GetComponent<PendantV3ShellStateController>();
            return shellState != null
                ? shellState.GetStateSnapshot()
                : PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault());
        }

        private bool CanPreview() => connectionHomeController.CurrentPreviewState is not PendantV3PreviewState.Kind.Disconnected and not PendantV3PreviewState.Kind.AutoReconnect;
        private bool IsAnyPanelVisible() => isDesktopVisible || isTabletVisible;
        private bool IsMoveLDispatchMode() => motionKind == "MoveL";
        private bool CanApply()
        {
            return connectionHomeController.CurrentPreviewState is not PendantV3PreviewState.Kind.Disconnected
                and not PendantV3PreviewState.Kind.AutoReconnect
                and not PendantV3PreviewState.Kind.Fault;
        }

        private static float ParseValue(string rawValue)
        {
            return float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0f;
        }
    }
}
