// Folder: UI - HUD/view components only; no kinematics logic.
using System;
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
        private static readonly string[] TcpAxisLabels = { "X", "Y", "Z", "RX", "RY", "RZ" };
        private static readonly string[] JointAxisLabels = { "J1", "J2", "J3", "J4", "J5", "J6" };
        private static readonly string[] TcpAxisUnits = { "mm", "mm", "mm", "deg", "deg", "deg" };
        private static readonly string[] JointAxisUnits = { "deg", "deg", "deg", "deg", "deg", "deg" };

        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset pointMoveTemplate;

        private readonly float[] previewTcpValues = new float[PendantV3LocalState.PointAxisCount];
        private readonly float[] previewJointValues = new float[PendantV3LocalState.PointAxisCount];
        private readonly float[] tcpDraftValues = new float[PendantV3LocalState.PointAxisCount];
        private readonly float[] jointDraftValues = new float[PendantV3LocalState.PointAxisCount];

        private VisualElement root;
        private VisualElement workPanelBody;
        private VisualElement bottomSheetBody;
        private VisualElement pointMovePanelHost;
        private VisualElement pointMoveSheetHost;
        private ConnectionHomeController connectionHomeController;
        private PendantV3ShellStateController shellStateController;
        private PopupCoordinatorV3 popupCoordinator;
        private PendantV3VisualizationOrchestrator visualizationOrchestrator;
        private RobotControlMotionRuntime motionRuntime;
        private string lastFeedback = "아직 실행한 명령이 없다.";
        private string pointName = PendantV3LocalState.DefaultPointName;
        private int lastInvalidIndex = -1;
        private bool isPointNameInvalid;

        private PanelElements desktopPanel;
        private PanelElements tabletPanel;
        private string activeCoordSystem = PendantV3LocalState.DefaultCoordSystem;
        private string motionKind = PendantV3LocalState.DefaultPointMotionKind;
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
            if (!TryBuildMotionRequest(out var request, out var validationMessage))
            {
                SetFeedback(validationMessage);
                return GetDebugSummary();
            }

            ExecuteMotionRequest(request);
            return GetDebugSummary();
        }

        public string GetDebugSummary()
        {
            var pointName = desktopPanel?.PointNameInput?.value ?? tabletPanel?.PointNameInput?.value ?? "Point";
            var runtimeRobot = motionRuntime?.RobotId ?? "none";
            var canPreviewAction = CanPreview() && IsAnyPanelVisible();
            var canApplyAction = CanApply() && IsAnyPanelVisible();
            var activeDraftValues = GetActiveDraftValues();
            return $"initialized={isInitialized}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}; coord={activeCoordSystem}; motion={motionKind}; previewState={connectionHomeController.CurrentPreviewState}; canPreview={canPreviewAction}; canApply={canApplyAction}; runtimeRobot={runtimeRobot}; name={pointName}; a0={activeDraftValues[0]:0.0}; a5={activeDraftValues[5]:0.0}; feedback={lastFeedback}";
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

            shellStateController ??= GetComponent<PendantV3ShellStateController>();
            popupCoordinator ??= GetComponent<PopupCoordinatorV3>();
            visualizationOrchestrator ??= GetComponent<PendantV3VisualizationOrchestrator>();
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
            pointName = localState.PointName;
            motionKind = localState.PointMotionKind;
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
            RegisterClick(panel.BtnApply, HandleApplyClicked);
            panel.PointNameInput.RegisterValueChangedCallback(evt => HandlePointNameChanged(evt.newValue));
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
            for (var index = 0; index < previewTcpValues.Length && index < data.TcpValues.Length; index++)
            {
                previewTcpValues[index] = ParseValue(data.TcpValues[index]);
            }

            for (var index = 0; index < previewJointValues.Length && index < data.JointValues.Length; index++)
            {
                previewJointValues[index] = ParseValue(data.JointValues[index]);
            }

            isPointNameInvalid = false;
            LoadDraftFromState();
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

            var moveJMode = IsMoveJDispatchMode();
            var activeDraftValues = GetActiveDraftValues();
            panel.BtnCoordBase.EnableInClassList("rc-point-coord-button--active", activeCoordSystem == "Base");
            panel.BtnCoordTool.EnableInClassList("rc-point-coord-button--active", activeCoordSystem == "Tool");
            panel.BtnCoordUser.EnableInClassList("rc-point-coord-button--active", activeCoordSystem == "User");
            panel.BtnMoveJ.EnableInClassList("rc-point-motion-button--active", motionKind == "MoveJ");
            panel.BtnMoveL.EnableInClassList("rc-point-motion-button--active", motionKind == "MoveL");
            panel.PointNameInput.SetValueWithoutNotify(pointName);
            panel.Hint.text = moveJMode
                ? "관절 목표를 바로 보낼 때는 MoveJ를 쓰고, 먼저 작은 각도 차이부터 미리보기로 확인한다."
                : "직선 접근이 필요할 때는 MoveL 후보로 보고, 먼저 미리보기로 궤적 감각을 확인한다.";
            panel.CoordSummary.text = moveJMode
                ? "관절 타깃: J1~J6 / MoveJ에서는 좌표계 전환 없이 관절 목표를 직접 보낸다."
                : $"좌표계: {activeCoordSystem} / 현재 TCP 기준으로 시작";
            panel.MotionSummary.text = moveJMode
                ? "이동 방식: MoveJ / 관절 목표를 바로 dispatch"
                : "이동 방식: MoveL / 공구 경로를 직선으로 먼저 확인";
            panel.PreviewSummary.text = BuildDeltaSummary(pointName);
            panel.FeedbackSummary.text = lastFeedback;
            var canPreview = CanPreview() && IsAnyPanelVisible();
            var canApply = CanApply() && IsAnyPanelVisible();
            panel.BtnRestore.SetEnabled(canPreview);
            panel.BtnPreview.SetEnabled(canPreview);
            panel.BtnApply.SetEnabled(canApply);
            panel.BtnApply.text = canApply
                ? (moveJMode ? "적용 (MoveJ)" : "적용 (MoveL)")
                : (moveJMode ? "적용 (MoveJ 대기)" : "적용 (MoveL 대기)");
            panel.PointNameInput.EnableInClassList("rc-point-name-input--danger", isPointNameInvalid);
            panel.BtnCoordBase.SetEnabled(!moveJMode);
            panel.BtnCoordTool.SetEnabled(!moveJMode);
            panel.BtnCoordUser.SetEnabled(!moveJMode);

            for (var index = 0; index < panel.ValueInputs.Length && index < activeDraftValues.Length; index++)
            {
                panel.ValueInputs[index].SetValueWithoutNotify(activeDraftValues[index].ToString("0.0", CultureInfo.InvariantCulture));
                panel.ValueInputs[index].EnableInClassList("rc-point-cell-input--danger", index == lastInvalidIndex);
                if (panel.AxisLabels[index] != null)
                {
                    panel.AxisLabels[index].text = moveJMode ? JointAxisLabels[index] : TcpAxisLabels[index];
                }

                if (panel.AxisUnits[index] != null)
                {
                    panel.AxisUnits[index].text = moveJMode ? JointAxisUnits[index] : TcpAxisUnits[index];
                }
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
            PersistDraftState();
            ApplyAll();
        }

        private void RestoreFromPreview()
        {
            if (!CanPreview() || !IsAnyPanelVisible())
            {
                SetFeedback("연결이 준비될 때까지 현재값 복원을 잠시 잠근다.");
                return;
            }

            ResetActiveDraftToPreview();
            isPointNameInvalid = false;
            PersistDraftState();
            PublishVisualizationPreview(false);
            ApplyAll();
            SetFeedback(IsMoveJDispatchMode()
                ? "현재 preview joint 값으로 다시 채웠다."
                : "현재 preview TCP 값으로 다시 채웠다.");
        }

        private void HandleValueChanged(int index, string rawValue)
        {
            if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return;
            }

            var activeDraftValues = GetActiveDraftValues();
            activeDraftValues[index] = parsed;
            lastInvalidIndex = -1;
            isPointNameInvalid = false;
            PersistDraftState();
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

            var target = BuildCurrentTargetValues();
            PublishVisualizationPreview(true);
            if (!IsMoveJDispatchMode())
            {
                SetFeedback($"[Preview] MoveL 후보 · {pointName} · X {target[0]:0.0} / Y {target[1]:0.0} / Z {target[2]:0.0}");
                return;
            }

            SetFeedback($"[Preview] MoveJ 후보 · {pointName} · J1 {target[0]:0.0} / J2 {target[1]:0.0} / J3 {target[2]:0.0}");
        }

        private void HandleApplyClicked()
        {
            if (!TryBuildMotionRequest(out var request, out var validationMessage))
            {
                SetFeedback(validationMessage);
                return;
            }

            if (popupCoordinator != null)
            {
                popupCoordinator.OpenMoveConfirmForPolicy(request.ConfirmTitle, request.ConfirmSummary, () => ExecuteMotionRequest(request), request.ConfirmButtonLabel);
                return;
            }

            ExecuteMotionRequest(request);
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

        public bool HasUnsavedDraft()
        {
            return EvaluateUnsavedPointDraft();
        }

        public void DiscardDraftAndReturnToEasyMotion()
        {
            pointName = PendantV3LocalState.DefaultPointName;
            Array.Copy(previewTcpValues, tcpDraftValues, previewTcpValues.Length);
            Array.Copy(previewJointValues, jointDraftValues, previewJointValues.Length);
            motionKind = PendantV3LocalState.DefaultPointMotionKind;
            isPointNameInvalid = false;
            lastInvalidIndex = -1;
            PersistDraftState();
            visualizationOrchestrator?.ClearPreview();
            if (shellStateController != null)
            {
                var snapshot = shellStateController.GetStateSnapshot();
                var workTab = isDesktopVisible ? "TabEasyMotion" : snapshot.ActiveWorkTab;
                var tabletTab = isTabletVisible ? "BottomTabEasyMotion" : snapshot.ActiveTabletTab;
                shellStateController.SetDebugSelection(snapshot.ActiveNavSection, workTab, tabletTab);
            }

            ApplyAll();
            SetFeedback("입력 중이던 포인트 초안을 버리고 쉬운 조작으로 돌아갔다.");
        }

        private bool TryReadActivePanelValues(out double[] target, out string validationMessage)
        {
            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            var activeDraftValues = GetActiveDraftValues();
            target = new double[activeDraftValues.Length];
            validationMessage = "입력 검증 통과";
            lastInvalidIndex = -1;
            isPointNameInvalid = false;

            if (panel == null)
            {
                validationMessage = "포인트 이동 패널을 찾지 못했다.";
                return false;
            }

            var nextPointName = panel.PointNameInput?.value?.Trim();
            if (string.IsNullOrWhiteSpace(nextPointName))
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

            pointName = nextPointName;
            for (var index = 0; index < activeDraftValues.Length && index < target.Length; index++)
            {
                activeDraftValues[index] = (float)target[index];
            }

            PersistDraftState();
            return true;
        }

        private double[] BuildCurrentTargetValues()
        {
            var activeDraftValues = GetActiveDraftValues();
            var target = new double[activeDraftValues.Length];
            for (var index = 0; index < activeDraftValues.Length; index++)
            {
                target[index] = activeDraftValues[index];
            }

            return target;
        }

        private string BuildDeltaSummary(string pointName)
        {
            var activeDraftValues = GetActiveDraftValues();
            var previewValues = GetPreviewValuesForActiveMotion();
            if (previewValues.Length < 3)
            {
                return $"미리보기로 {pointName} 위치를 먼저 확인한 뒤 실제 이동을 보낸다.";
            }

            var dx = activeDraftValues[0] - previewValues[0];
            var dy = activeDraftValues[1] - previewValues[1];
            var dz = activeDraftValues[2] - previewValues[2];
            if (IsMoveJDispatchMode())
            {
                return $"미리보기 ΔJoint · J1 {dx:+0.0;-0.0;0.0} / J2 {dy:+0.0;-0.0;0.0} / J3 {dz:+0.0;-0.0;0.0}";
            }

            return $"미리보기 ΔTCP · X {dx:+0.0;-0.0;0.0} / Y {dy:+0.0;-0.0;0.0} / Z {dz:+0.0;-0.0;0.0}";
        }

        private string GetAxisLabel(int index)
        {
            var labels = IsMoveJDispatchMode() ? JointAxisLabels : TcpAxisLabels;
            return index >= 0 && index < labels.Length
                ? labels[index]
                : (IsMoveJDispatchMode() ? "Joint" : "TCP");
        }

        private PendantV3LocalState GetLocalState()
        {
            shellStateController ??= GetComponent<PendantV3ShellStateController>();
            return shellStateController != null
                ? shellStateController.GetStateSnapshot()
                : PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault());
        }

        private bool CanPreview() => connectionHomeController.CurrentPreviewState is not PendantV3PreviewState.Kind.Disconnected and not PendantV3PreviewState.Kind.AutoReconnect;
        private bool IsAnyPanelVisible() => isDesktopVisible || isTabletVisible;
        private bool IsMoveLDispatchMode() => motionKind == "MoveL";
        private bool IsMoveJDispatchMode() => motionKind == "MoveJ";
        private bool CanApply()
        {
            return connectionHomeController.CurrentPreviewState is not PendantV3PreviewState.Kind.Disconnected
                and not PendantV3PreviewState.Kind.AutoReconnect
                and not PendantV3PreviewState.Kind.Fault
                && connectionHomeController.ActualMoveAllowed;
        }

        private static float ParseValue(string rawValue)
        {
            return float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0f;
        }

        private void HandlePointNameChanged(string nextPointName)
        {
            pointName = string.IsNullOrWhiteSpace(nextPointName)
                ? PendantV3LocalState.DefaultPointName
                : nextPointName.Trim();
            isPointNameInvalid = false;
            PersistDraftState();
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
        }

        private void LoadDraftFromState()
        {
            var localState = GetLocalState();
            pointName = localState.PointName;
            motionKind = localState.PointMotionKind;
            if (!localState.HasPointDraft)
            {
                Array.Copy(previewTcpValues, tcpDraftValues, previewTcpValues.Length);
                Array.Copy(previewJointValues, jointDraftValues, previewJointValues.Length);
                pointName = PendantV3LocalState.DefaultPointName;
                return;
            }

            Array.Copy(localState.PointTcpDraftValues, tcpDraftValues, tcpDraftValues.Length);
            Array.Copy(localState.PointJointDraftValues, jointDraftValues, jointDraftValues.Length);
        }

        private void PersistDraftState()
        {
            var normalizedPointName = string.IsNullOrWhiteSpace(pointName)
                ? PendantV3LocalState.DefaultPointName
                : pointName.Trim();
            pointName = normalizedPointName;
            var hasPointDraft = EvaluateUnsavedPointDraft();
            if (shellStateController != null)
            {
                shellStateController.UpdatePointMoveDraft(normalizedPointName, motionKind, tcpDraftValues, jointDraftValues, hasPointDraft);
                return;
            }

            var localState = GetLocalState();
            localState.PointName = normalizedPointName;
            localState.PointMotionKind = motionKind;
            localState.PointTcpDraftValues = (float[])tcpDraftValues.Clone();
            localState.PointJointDraftValues = (float[])jointDraftValues.Clone();
            localState.HasPointDraft = hasPointDraft;
            LocalSettingsStore.Save(localState);
        }

        private bool EvaluateUnsavedPointDraft()
        {
            return !string.Equals(pointName, PendantV3LocalState.DefaultPointName, System.StringComparison.Ordinal)
                || !AreDraftValuesEqual(tcpDraftValues, previewTcpValues)
                || !AreDraftValuesEqual(jointDraftValues, previewJointValues);
        }

        private float[] GetActiveDraftValues()
        {
            return IsMoveJDispatchMode() ? jointDraftValues : tcpDraftValues;
        }

        private float[] GetPreviewValuesForActiveMotion()
        {
            return IsMoveJDispatchMode() ? previewJointValues : previewTcpValues;
        }

        private void ResetActiveDraftToPreview()
        {
            var previewValues = GetPreviewValuesForActiveMotion();
            var activeDraftValues = GetActiveDraftValues();
            Array.Copy(previewValues, activeDraftValues, previewValues.Length);
        }

        private bool TryBuildMotionRequest(out MotionRequest request, out string validationMessage)
        {
            request = default;
            if (!IsAnyPanelVisible())
            {
                validationMessage = "포인트 이동 패널이 열려 있을 때만 적용할 수 있다.";
                return false;
            }

            if (!CanApply())
            {
                validationMessage = connectionHomeController.ActualMoveAllowed
                    ? "연결 상태가 준비되지 않아 적용할 수 없다. 연결/에러 상태를 먼저 확인해라."
                    : connectionHomeController.ActualMoveBlockReason;
                return false;
            }

            if (!TryReadActivePanelValues(out var target, out validationMessage))
            {
                return false;
            }

            var state = GetLocalState();
            request = new MotionRequest(
                motionKind,
                pointName,
                target,
                Mathf.Clamp(state.SpeedPercent, 1, 100),
                IsMoveJDispatchMode()
                    ? $"{pointName} 관절 목표를 MoveJ로 전송할 준비가 됐다."
                    : $"{pointName} TCP 목표를 MoveL로 전송할 준비가 됐다.",
                IsMoveJDispatchMode()
                    ? $"관절 목표 J1 {target[0]:0.0} / J2 {target[1]:0.0} / J3 {target[2]:0.0} 를 speed {state.SpeedPercent}%로 보낸다."
                    : $"TCP 목표 X {target[0]:0.0} / Y {target[1]:0.0} / Z {target[2]:0.0} 를 speed {state.SpeedPercent}%로 보낸다.",
                IsMoveJDispatchMode() ? "MoveJ 실행" : "MoveL 실행");
            validationMessage = "적용 준비 완료";
            return true;
        }

        private void ExecuteMotionRequest(MotionRequest request)
        {
            var runtimeResult = EnsureMotionRuntime();
            if (!runtimeResult.IsSuccess)
            {
                SetFeedback(runtimeResult.Message);
                return;
            }

            var result = request.MotionKind == "MoveJ"
                ? motionRuntime.DispatchMoveJ(request.TargetValues, request.SpeedPercent)
                : motionRuntime.DispatchMoveL(request.TargetValues, request.SpeedPercent);
            if (result.IsSuccess)
            {
                visualizationOrchestrator?.SetRuntimePose(
                    request.MotionKind == "MoveJ" ? request.TargetValues : BuildCurrentJointFallback(),
                    request.MotionKind == "MoveL" ? request.TargetValues : BuildCurrentTcpFallback());
                visualizationOrchestrator?.ClearPreview();
                SetFeedback(request.MotionKind == "MoveJ"
                    ? $"[Dispatch] MoveJ 완료 · speed {request.SpeedPercent}% · J1 {request.TargetValues[0]:0.0} / J2 {request.TargetValues[1]:0.0} / J3 {request.TargetValues[2]:0.0}"
                    : $"[Dispatch] MoveL 완료 · speed {request.SpeedPercent}% · X {request.TargetValues[0]:0.0} / Y {request.TargetValues[1]:0.0} / Z {request.TargetValues[2]:0.0}");
                return;
            }

            SetFeedback($"[Dispatch] {request.MotionKind} 실패 · {result.Message}");
        }

        private static bool AreDraftValuesEqual(float[] left, float[] right)
        {
            for (var index = 0; index < left.Length && index < right.Length; index++)
            {
                if (Mathf.Abs(left[index] - right[index]) >= 0.0001f)
                {
                    return false;
                }
            }

            return true;
        }

        private void PublishVisualizationPreview(bool moveRequest)
        {
            if (visualizationOrchestrator == null)
            {
                return;
            }

            var target = BuildCurrentTargetValues();
            if (IsMoveJDispatchMode())
            {
                visualizationOrchestrator.PreviewJointPose(target, FindDominantAxisIndex(target, previewJointValues), $"{pointName} MoveJ", moveRequest);
                return;
            }

            visualizationOrchestrator.PreviewTcpTarget(target, FindDominantAxisIndex(target, BuildDoubleArray(previewTcpValues)), 1, activeCoordSystem, $"{pointName} MoveL", moveRequest);
        }

        private int FindDominantAxisIndex(double[] target, float[] preview)
        {
            var maxDelta = 0d;
            var maxIndex = 0;
            for (var index = 0; index < target.Length && index < preview.Length; index++)
            {
                var delta = System.Math.Abs(target[index] - preview[index]);
                if (delta > maxDelta)
                {
                    maxDelta = delta;
                    maxIndex = index;
                }
            }

            return maxIndex;
        }

        private int FindDominantAxisIndex(double[] target, double[] preview)
        {
            var maxDelta = 0d;
            var maxIndex = 0;
            for (var index = 0; index < target.Length && index < preview.Length; index++)
            {
                var delta = System.Math.Abs(target[index] - preview[index]);
                if (delta > maxDelta)
                {
                    maxDelta = delta;
                    maxIndex = index;
                }
            }

            return maxIndex;
        }

        private static double[] BuildDoubleArray(float[] source)
        {
            var result = new double[source.Length];
            for (var index = 0; index < source.Length; index++)
            {
                result[index] = source[index];
            }

            return result;
        }

        private double[] BuildCurrentJointFallback()
        {
            return BuildDoubleArray(previewJointValues);
        }

        private double[] BuildCurrentTcpFallback()
        {
            return BuildDoubleArray(previewTcpValues);
        }

        private readonly struct MotionRequest
        {
            public MotionRequest(string motionKind, string pointName, double[] targetValues, int speedPercent, string confirmTitle, string confirmSummary, string confirmButtonLabel)
            {
                MotionKind = motionKind;
                PointName = pointName;
                TargetValues = targetValues;
                SpeedPercent = speedPercent;
                ConfirmTitle = confirmTitle;
                ConfirmSummary = confirmSummary;
                ConfirmButtonLabel = confirmButtonLabel;
            }

            public string MotionKind { get; }
            public string PointName { get; }
            public double[] TargetValues { get; }
            public int SpeedPercent { get; }
            public string ConfirmTitle { get; }
            public string ConfirmSummary { get; }
            public string ConfirmButtonLabel { get; }
        }
    }
}
