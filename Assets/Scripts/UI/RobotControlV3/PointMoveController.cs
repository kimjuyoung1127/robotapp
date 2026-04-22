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
        private const string PointSequenceName = "PendantV3Points";

        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset pointMoveTemplate;

        private readonly float[] currentValues = new float[6];

        private VisualElement root;
        private VisualElement workPanelBody;
        private VisualElement bottomSheetBody;
        private VisualElement pointMovePanelHost;
        private VisualElement pointMoveSheetHost;
        private ConnectionHomeController connectionHomeController;
        private RobotControlV3RuntimeController runtimeController;
        private RobotControlMotionRuntime motionRuntime;
        private string lastFeedback = "아직 실행한 명령이 없다.";
        private Waypoint recalledPoint;
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

        public string SavePointForDebug()
        {
            SaveCurrentPoint();
            return GetDebugSummary();
        }

        public string RecallPointForDebug(string pointName)
        {
            RecallPoint(pointName);
            return GetDebugSummary();
        }

        public string DeletePointForDebug(string pointName)
        {
            DeletePoint(pointName);
            return GetDebugSummary();
        }

        public string GetPointListSummaryForDebug()
        {
            return BuildPointListDebugSummary();
        }

        public string RenamePointForDebug(string oldName, string newName)
        {
            RenamePoint(oldName, newName);
            return GetDebugSummary();
        }

        public string MovePointForDebug(string pointName, int direction)
        {
            RecallPoint(pointName);
            MovePointInSequence(direction);
            return GetDebugSummary();
        }

        public string OverwritePointWithReadbackForDebug(string pointName)
        {
            RecallPoint(pointName);
            OverwriteSelectedPointWithCurrentReadback();
            return GetDebugSummary();
        }

        public string ExportPointsForDebug()
        {
            ExportPoints();
            return GetDebugSummary();
        }

        public string CleanupPointsForDebug()
        {
            CleanupPoints();
            return GetDebugSummary();
        }

        public string SetPointNameForDebug(string pointName)
        {
            SetPointName(pointName);
            ApplyAll();
            return GetDebugSummary();
        }

        public string SetPointValueForDebug(string axisLabel, float value)
        {
            var index = AxisIndexFromLabel(axisLabel);
            currentValues[index] = value;
            recalledPoint = null;
            ApplyAll();
            return GetDebugSummary();
        }

        public string GetDebugSummary()
        {
            var pointName = desktopPanel?.PointNameInput?.value ?? tabletPanel?.PointNameInput?.value ?? "Point";
            var runtimeRobot = motionRuntime?.RobotId ?? "none";
            var canPreviewAction = CanPreview() && IsAnyPanelVisible();
            var canApplyAction = CanApply() && IsAnyPanelVisible();
            return $"initialized={isInitialized}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}; coord={activeCoordSystem}; motion={motionKind}; previewState={connectionHomeController.CurrentPreviewState}; canPreview={canPreviewAction}; canApply={canApplyAction}; runtimeRobot={runtimeRobot}; name={pointName}; x={currentValues[0]:0.0}; rz={currentValues[5]:0.0}; feedback={lastFeedback}";
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            root = document?.rootVisualElement;
            if (root == null || pointMoveTemplate == null || connectionHomeController == null || runtimeController == null)
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
            RegisterClick(panel.BtnSave, SaveCurrentPoint);
            RegisterClick(panel.BtnRecall, () => RecallPoint(panel.PointNameInput?.value));
            RegisterClick(panel.BtnDelete, () => DeletePoint(panel.PointNameInput?.value));
            RegisterClick(panel.BtnRename, () => RenamePoint(recalledPoint?.name, panel.PointNameInput?.value));
            RegisterClick(panel.BtnUp, () => MovePointInSequence(-1));
            RegisterClick(panel.BtnDown, () => MovePointInSequence(1));
            RegisterClick(panel.BtnOverwrite, OverwriteSelectedPointWithCurrentReadback);
            RegisterClick(panel.BtnExport, ExportPoints);
            RegisterClick(panel.BtnCleanup, CleanupPoints);
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

        private void ApplyPreview(RobotControlV3RuntimeSnapshot data)
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
            panel.StoreSummary.text = BuildStoreSummary();
            RebuildPointList(panel);
            panel.FeedbackSummary.text = lastFeedback;
            var canPreview = CanPreview() && IsAnyPanelVisible();
            var canApply = CanApply() && IsAnyPanelVisible();
            panel.BtnRestore.SetEnabled(canPreview);
            panel.BtnPreview.SetEnabled(canPreview);
            panel.BtnApply.SetEnabled(canApply);
            panel.BtnDelete.SetEnabled(recalledPoint != null || HasNamedPoint(panel.PointNameInput?.value));
            panel.BtnRename.SetEnabled(recalledPoint != null);
            panel.BtnUp.SetEnabled(CanMoveSelectedPoint(-1));
            panel.BtnDown.SetEnabled(CanMoveSelectedPoint(1));
            panel.BtnOverwrite.SetEnabled(recalledPoint != null);
            panel.BtnExport.SetEnabled(HasAnyPoint());
            panel.BtnCleanup.SetEnabled(HasAnyPoint());
            panel.BtnApply.text = IsMoveLDispatchMode()
                ? (CanApply() ? "적용" : "적용 (연결 대기)")
                : (CanApply() ? "적용 (MoveJ)" : "적용 (연결 대기)");
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
            recalledPoint = null;
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
                runtimeController?.PreviewTcpPose(currentValuesToDouble(), $"포인트 {pointName} MoveL 후보");
                SetFeedback($"[Preview] MoveL 후보 · {pointName} · X {currentValues[0]:0.0} / Y {currentValues[1]:0.0} / Z {currentValues[2]:0.0}");
                return;
            }

            var result = TryGetSavedJointTarget(currentValuesToDouble(), pointName, out var savedJointTarget)
                ? PreviewSavedMoveJ(pointName, savedJointTarget)
                : runtimeController?.PreviewPointMoveJ(currentValuesToDouble(), $"포인트 {pointName} MoveJ 후보")
                    ?? FairinoResult.Fail(-1, "Point MoveJ runtime을 찾지 못했다.");
            SetFeedback(result.IsSuccess
                ? $"[Preview] MoveJ 후보 · {pointName} · {result.Message}"
                : result.Message);
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
                var pointName = (isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel)?.PointNameInput?.value ?? "Point";
                var moveJResult = TryGetSavedJointTarget(target, pointName, out var savedJointTarget)
                    ? ApplySavedMoveJ(pointName, savedJointTarget)
                    : runtimeController?.ApplyPointMoveJ(target, "포인트 MoveJ 적용")
                        ?? FairinoResult.Fail(-1, "Point MoveJ runtime을 찾지 못했다.");
                SetFeedback(moveJResult.IsSuccess
                    ? runtimeController.CurrentSnapshot.LastFeedback
                    : moveJResult.Message);
                return;
            }

            runtimeController?.ApplyTcpPose(target, "포인트 MoveL 적용");
            SetFeedback(runtimeController != null ? runtimeController.CurrentSnapshot.LastFeedback : "[Dispatch] MoveL 적용 요청");
            return;
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

        private void SaveCurrentPoint()
        {
            if (!IsAnyPanelVisible())
            {
                SetFeedback("포인트 이동 패널이 열려 있을 때만 저장할 수 있다.");
                return;
            }

            if (!TryReadActivePanelValues(out var target, out var validationMessage))
            {
                SetFeedback(validationMessage);
                return;
            }

            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            var pointName = panel?.PointNameInput?.value?.Trim();
            var waypoint = new Waypoint
            {
                name = pointName,
                jointsDeg = ReadCurrentSnapshotJoints(),
                tcpMm = target,
                moveType = motionKind,
                speedPreset = "medium",
                dwellSec = 0.0
            };

            var sequence = LoadPointSequenceIfExists() ?? WaypointStore.CreateEmpty(PointSequenceName);
            ReplaceWaypoint(sequence, waypoint);
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("포인트 저장 실패");
                return;
            }

            recalledPoint = CloneWaypoint(waypoint);
            SetFeedback($"[Save] {pointName} 저장 · saved joint target 포함");
        }

        private void RecallPoint(string requestedName)
        {
            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("저장된 포인트가 없다.");
                return;
            }

            var pointName = string.IsNullOrWhiteSpace(requestedName)
                ? sequence.waypoints[0].name
                : requestedName.Trim();
            var waypoint = FindWaypoint(sequence, pointName) ?? sequence.waypoints[0];
            recalledPoint = CloneWaypoint(waypoint);
            SetPointName(recalledPoint.name);
            motionKind = recalledPoint.moveType == "MoveL" ? "MoveL" : "MoveJ";
            for (var index = 0; index < currentValues.Length && index < recalledPoint.tcpMm.Length; index++)
            {
                currentValues[index] = (float)recalledPoint.tcpMm[index];
            }

            SetFeedback($"[Recall] {recalledPoint.name} 불러옴 · {motionKind}");
        }

        private void DeletePoint(string requestedName)
        {
            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("삭제할 저장 포인트가 없다.");
                return;
            }

            var pointName = string.IsNullOrWhiteSpace(requestedName)
                ? recalledPoint?.name
                : requestedName.Trim();
            var index = FindWaypointIndex(sequence, pointName);
            if (index < 0)
            {
                SetFeedback($"{pointName} 포인트를 찾지 못했다.");
                return;
            }

            var deletedName = sequence.waypoints[index].name;
            WaypointStore.RemoveAt(sequence, index);
            if (sequence.waypoints.Length == 0)
            {
                WaypointStore.Delete(PointSequenceName);
            }
            else
            {
                WaypointStore.Save(sequence);
            }

            if (recalledPoint != null && string.Equals(recalledPoint.name, deletedName, System.StringComparison.OrdinalIgnoreCase))
            {
                recalledPoint = null;
            }

            SetFeedback($"[Delete] {deletedName} 삭제");
        }

        private void RenamePoint(string oldName, string newName)
        {
            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("이름을 바꿀 저장 포인트가 없다.");
                return;
            }

            var fromName = string.IsNullOrWhiteSpace(oldName) ? recalledPoint?.name : oldName.Trim();
            var toName = string.IsNullOrWhiteSpace(newName) ? string.Empty : newName.Trim();
            if (string.IsNullOrWhiteSpace(fromName) || string.IsNullOrWhiteSpace(toName))
            {
                SetFeedback("이름 변경은 선택된 포인트와 새 이름이 필요하다.");
                return;
            }

            var oldIndex = FindWaypointIndex(sequence, fromName);
            if (oldIndex < 0)
            {
                SetFeedback($"{fromName} 포인트를 찾지 못했다.");
                return;
            }

            var duplicateIndex = FindWaypointIndex(sequence, toName);
            if (duplicateIndex >= 0 && duplicateIndex != oldIndex)
            {
                SetFeedback($"{toName} 이름이 이미 있다.");
                return;
            }

            sequence.waypoints[oldIndex].name = toName;
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("포인트 이름 변경 저장 실패");
                return;
            }

            recalledPoint = CloneWaypoint(sequence.waypoints[oldIndex]);
            SetPointName(toName);
            SetFeedback($"[Rename] {fromName} -> {toName}");
        }

        private void MovePointInSequence(int direction)
        {
            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("순서를 바꿀 저장 포인트가 없다.");
                return;
            }

            var index = FindWaypointIndex(sequence, recalledPoint?.name);
            var targetIndex = index + (direction < 0 ? -1 : 1);
            if (index < 0 || targetIndex < 0 || targetIndex >= sequence.waypoints.Length)
            {
                SetFeedback("이 방향으로 더 이동할 수 없다.");
                return;
            }

            var temp = sequence.waypoints[index];
            sequence.waypoints[index] = sequence.waypoints[targetIndex];
            sequence.waypoints[targetIndex] = temp;
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("포인트 순서 저장 실패");
                return;
            }

            recalledPoint = CloneWaypoint(sequence.waypoints[targetIndex]);
            SetFeedback($"[Order] {recalledPoint.name} {targetIndex + 1}번째로 이동");
        }

        private void OverwriteSelectedPointWithCurrentReadback()
        {
            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0 || recalledPoint == null)
            {
                SetFeedback("덮어쓸 저장 포인트를 먼저 선택해라.");
                return;
            }

            var index = FindWaypointIndex(sequence, recalledPoint.name);
            if (index < 0)
            {
                SetFeedback($"{recalledPoint.name} 포인트를 찾지 못했다.");
                return;
            }

            var waypoint = sequence.waypoints[index];
            waypoint.jointsDeg = ReadCurrentSnapshotJoints();
            waypoint.tcpMm = ReadCurrentSnapshotTcp();
            sequence.waypoints[index] = waypoint;
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("포인트 덮어쓰기 저장 실패");
                return;
            }

            recalledPoint = CloneWaypoint(waypoint);
            for (var valueIndex = 0; valueIndex < currentValues.Length && valueIndex < recalledPoint.tcpMm.Length; valueIndex++)
            {
                currentValues[valueIndex] = (float)recalledPoint.tcpMm[valueIndex];
            }

            SetFeedback($"[Overwrite] {recalledPoint.name} 현재 readback으로 갱신");
        }

        private void ExportPoints()
        {
            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("내보낼 저장 포인트가 없다.");
                return;
            }

            var exportPath = System.IO.Path.Combine(WaypointStore.GetStoragePath(), $"{PointSequenceName}.export.json");
            if (!WaypointStore.ExportToFile(sequence, exportPath))
            {
                SetFeedback("포인트 내보내기 실패");
                return;
            }

            SetFeedback($"[Export] {sequence.waypoints.Length}개 -> {exportPath}");
        }

        private void CleanupPoints()
        {
            var sequence = LoadPointSequenceIfExists();
            var count = sequence?.waypoints?.Length ?? 0;
            if (count == 0)
            {
                SetFeedback("정리할 저장 포인트가 없다.");
                return;
            }

            WaypointStore.Delete(PointSequenceName);
            recalledPoint = null;
            SetFeedback($"[Cleanup] 저장 포인트 {count}개 정리");
        }

        private FairinoResult PreviewSavedMoveJ(string pointName, double[] savedJointTarget)
        {
            runtimeController?.PreviewJointAngles(savedJointTarget, $"포인트 {pointName} saved MoveJ 후보");
            return FairinoResult.Ok("saved joint target 사용");
        }

        private FairinoResult ApplySavedMoveJ(string pointName, double[] savedJointTarget)
        {
            return runtimeController?.ApplyJointAngles(savedJointTarget, $"포인트 {pointName} saved MoveJ 적용")
                ?? FairinoResult.Fail(-1, "Point MoveJ runtime을 찾지 못했다.");
        }

        private bool TryGetSavedJointTarget(double[] targetTcp, string pointName, out double[] savedJointTarget)
        {
            savedJointTarget = null;
            if (recalledPoint == null || string.IsNullOrWhiteSpace(pointName))
            {
                return false;
            }

            if (!string.Equals(recalledPoint.name, pointName.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (recalledPoint.jointsDeg == null || recalledPoint.jointsDeg.Length < 6 || recalledPoint.tcpMm == null || recalledPoint.tcpMm.Length < 6)
            {
                return false;
            }

            for (var index = 0; index < 6; index++)
            {
                if (System.Math.Abs(recalledPoint.tcpMm[index] - targetTcp[index]) > 0.05)
                {
                    return false;
                }
            }

            savedJointTarget = (double[])recalledPoint.jointsDeg.Clone();
            return true;
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

        private string BuildStoreSummary()
        {
            var sequence = LoadPointSequenceIfExists();
            var count = sequence?.waypoints?.Length ?? 0;
            if (count == 0)
            {
                return "저장된 포인트: 없음";
            }

            var active = recalledPoint != null ? $" / 선택: {recalledPoint.name}" : string.Empty;
            return $"저장된 포인트: {count}개{active}";
        }

        private void RebuildPointList(PanelElements panel)
        {
            if (panel?.PointListContainer == null)
            {
                return;
            }

            panel.PointListContainer.Clear();
            var sequence = LoadPointSequenceIfExists();
            var waypoints = sequence?.waypoints ?? System.Array.Empty<Waypoint>();
            var visibleCount = System.Math.Min(waypoints.Length, 6);
            for (var index = 0; index < visibleCount; index++)
            {
                var waypoint = waypoints[index];
                if (waypoint == null)
                {
                    continue;
                }

                var capturedName = waypoint.name;
                var button = new Button(() => RecallPoint(capturedName))
                {
                    text = $"{waypoint.name} · {waypoint.moveType}"
                };
                button.AddToClassList("rc-point-list-button");
                button.EnableInClassList(
                    "rc-point-list-button--active",
                    recalledPoint != null && string.Equals(recalledPoint.name, waypoint.name, System.StringComparison.OrdinalIgnoreCase));
                panel.PointListContainer.Add(button);
            }

            if (waypoints.Length > visibleCount)
            {
                var more = new Label($"+{waypoints.Length - visibleCount} more");
                more.AddToClassList("rc-panel-copy");
                more.AddToClassList("rc-panel-copy--compact");
                panel.PointListContainer.Add(more);
            }
        }

        private string BuildPointListDebugSummary()
        {
            var sequence = LoadPointSequenceIfExists();
            var waypoints = sequence?.waypoints ?? System.Array.Empty<Waypoint>();
            var names = new string[waypoints.Length];
            for (var index = 0; index < waypoints.Length; index++)
            {
                names[index] = $"{waypoints[index]?.name}:{waypoints[index]?.moveType}";
            }

            return $"count={waypoints.Length}; active={recalledPoint?.name ?? "none"}; points=[{string.Join(",", names)}]";
        }

        private bool HasNamedPoint(string pointName)
        {
            var sequence = LoadPointSequenceIfExists();
            return FindWaypointIndex(sequence, pointName) >= 0;
        }

        private bool HasAnyPoint()
        {
            var sequence = LoadPointSequenceIfExists();
            return sequence?.waypoints != null && sequence.waypoints.Length > 0;
        }

        private bool CanMoveSelectedPoint(int direction)
        {
            if (recalledPoint == null)
            {
                return false;
            }

            var sequence = LoadPointSequenceIfExists();
            var index = FindWaypointIndex(sequence, recalledPoint.name);
            var targetIndex = index + (direction < 0 ? -1 : 1);
            return index >= 0 && sequence?.waypoints != null && targetIndex >= 0 && targetIndex < sequence.waypoints.Length;
        }

        private static WaypointSequence LoadPointSequenceIfExists()
        {
            var names = WaypointStore.LoadAllNames();
            for (var index = 0; index < names.Length; index++)
            {
                if (string.Equals(names[index], PointSequenceName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return WaypointStore.Load(PointSequenceName);
                }
            }

            return null;
        }

        private void SetPointName(string pointName)
        {
            var safeName = string.IsNullOrWhiteSpace(pointName) ? "Point" : pointName.Trim();
            desktopPanel?.PointNameInput?.SetValueWithoutNotify(safeName);
            tabletPanel?.PointNameInput?.SetValueWithoutNotify(safeName);
        }

        private double[] ReadCurrentSnapshotJoints()
        {
            var values = runtimeController?.CurrentSnapshot.JointValues;
            var result = new double[6];
            for (var index = 0; index < result.Length; index++)
            {
                if (values == null || index >= values.Length || !double.TryParse(values[index], NumberStyles.Float, CultureInfo.InvariantCulture, out result[index]))
                {
                    result[index] = 0.0;
                }
            }

            return result;
        }

        private double[] ReadCurrentSnapshotTcp()
        {
            var values = runtimeController?.CurrentSnapshot.TcpValues;
            var result = new double[6];
            for (var index = 0; index < result.Length; index++)
            {
                if (values == null || index >= values.Length || !double.TryParse(values[index], NumberStyles.Float, CultureInfo.InvariantCulture, out result[index]))
                {
                    result[index] = 0.0;
                }
            }

            return result;
        }

        private static void ReplaceWaypoint(WaypointSequence sequence, Waypoint waypoint)
        {
            var waypoints = sequence.waypoints ?? System.Array.Empty<Waypoint>();
            for (var index = 0; index < waypoints.Length; index++)
            {
                if (string.Equals(waypoints[index]?.name, waypoint.name, System.StringComparison.OrdinalIgnoreCase))
                {
                    waypoints[index] = CloneWaypoint(waypoint);
                    sequence.waypoints = waypoints;
                    return;
                }
            }

            WaypointStore.AddWaypoint(sequence, CloneWaypoint(waypoint));
        }

        private static Waypoint FindWaypoint(WaypointSequence sequence, string pointName)
        {
            var index = FindWaypointIndex(sequence, pointName);
            return index >= 0 ? sequence.waypoints[index] : null;
        }

        private static int FindWaypointIndex(WaypointSequence sequence, string pointName)
        {
            if (sequence?.waypoints == null || string.IsNullOrWhiteSpace(pointName))
            {
                return -1;
            }

            var waypoints = sequence.waypoints;
            for (var index = 0; index < waypoints.Length; index++)
            {
                var waypoint = waypoints[index];
                if (waypoint != null && string.Equals(waypoint.name, pointName.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        private static Waypoint CloneWaypoint(Waypoint waypoint)
        {
            return new Waypoint
            {
                name = waypoint?.name ?? "Point",
                jointsDeg = waypoint?.jointsDeg != null ? (double[])waypoint.jointsDeg.Clone() : new double[6],
                tcpMm = waypoint?.tcpMm != null ? (double[])waypoint.tcpMm.Clone() : new double[6],
                moveType = waypoint?.moveType ?? "MoveJ",
                speedPreset = waypoint?.speedPreset ?? "medium",
                dwellSec = waypoint?.dwellSec ?? 0.0
            };
        }

        private static int AxisIndexFromLabel(string axisLabel)
        {
            return (axisLabel ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "X" => 0,
                "Y" => 1,
                "Z" => 2,
                "RX" => 3,
                "RY" => 4,
                _ => 5,
            };
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

        private bool CanPreview()
        {
            if (connectionHomeController.CurrentPreviewState == PendantV3PreviewState.Kind.AutoReconnect)
            {
                return false;
            }

            return connectionHomeController.CurrentPreviewState != PendantV3PreviewState.Kind.Disconnected
                || connectionHomeController.CurrentPreviewDefinition.DryRunEnabled;
        }

        private bool IsAnyPanelVisible() => isDesktopVisible || isTabletVisible;
        private bool IsMoveLDispatchMode() => motionKind == "MoveL";
        private bool CanApply()
        {
            if (connectionHomeController.CurrentPreviewState is PendantV3PreviewState.Kind.AutoReconnect or PendantV3PreviewState.Kind.Fault)
            {
                return false;
            }

            return connectionHomeController.CurrentPreviewState != PendantV3PreviewState.Kind.Disconnected
                || connectionHomeController.CurrentPreviewDefinition.DryRunEnabled;
        }

        private static float ParseValue(string rawValue)
        {
            return float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0f;
        }

        private double[] currentValuesToDouble()
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
