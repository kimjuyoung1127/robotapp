// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections.Generic;
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
        private const string RecordedPathSequenceName = "PendantV3RecordedPath";
        private const string PointSubviewName = "Point";
        private const string SequenceSubviewName = "Sequence";
        private const string FunctionSubviewName = "Function";
        private const string PointModalPreviewMode = "Preview";
        private const string PointModalRunMode = "Run";
        private const string PointModalEditMode = "Edit";
        private const string PointModalFunctionMode = "Function";

        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset pointMoveTemplate;

        private readonly float[] currentValues = new float[6];
        private readonly List<string> selectedFunctionPointNames = new();

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
        private string activeNavSection = PendantV3LocalState.DefaultNavSection;
        private string activeTeachingSubview = PointSubviewName;
        private string activeCoordSystem = PendantV3LocalState.DefaultCoordSystem;
        private string motionKind = "MoveJ";
        private string selectedSpeedPreset = "medium";
        private double selectedDwellSec;
        private bool isDwellInvalid;
        private string pendingConfirmKind = string.Empty;
        private string pendingConfirmName = string.Empty;
        private string selectedSequenceName = PointSequenceName;
        private string selectedFunctionName = string.Empty;
        private string pointActionModalMode = string.Empty;
        private bool pointActionModalOpen;
        private bool debugSequenceEditLocked;
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
            isDesktopVisible = ShouldShowDesktopPanel(activeNavSection, activeWorkTab);
            isTabletVisible = ShouldShowTabletPanel(activeNavSection, activeTabletTab);
            this.activeNavSection = activeNavSection;
            activeCoordSystem = GetLocalState().CoordSystem;
            if (activeNavSection != "NavPoints")
            {
                activeTeachingSubview = PointSubviewName;
            }

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

        public string GetSequenceLibrarySummaryForDebug()
        {
            return BuildSequenceLibraryDebugSummary();
        }

        public string SelectSequenceForDebug(string sequenceName)
        {
            SelectSequence(sequenceName);
            return GetSequenceLibrarySummaryForDebug();
        }

        public string RunSelectedSequenceOnceForDebug()
        {
            RunSelectedSequenceOnce();
            return GetSequenceLibrarySummaryForDebug();
        }

        public string RunSelectedSequenceLoopForDebug()
        {
            RunSelectedSequenceLoop();
            return GetSequenceLibrarySummaryForDebug();
        }

        public string DeleteSelectedSequenceForDebug()
        {
            DeleteSelectedSequence();
            return GetSequenceLibrarySummaryForDebug();
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

        public string DuplicatePointForDebug(string pointName)
        {
            RecallPoint(pointName);
            DuplicateSelectedPoint();
            return GetDebugSummary();
        }

        public string GetSelectedPointDetailForDebug()
        {
            return BuildPointDetailDebugSummary();
        }

        public string GetPointActionModalSummaryForDebug()
        {
            return BuildPointActionModalDebugSummary();
        }

        public string SetPointTimingForDebug(string speedPreset, double dwellSec)
        {
            SetSelectedSpeedPreset(speedPreset);
            selectedDwellSec = dwellSec;
            isDwellInvalid = false;
            ApplyAll();
            return GetDebugSummary();
        }

        public string ApplyPointTimingForDebug()
        {
            ApplySelectedPointTiming();
            return GetSelectedPointDetailForDebug();
        }

        public string SetSequenceEditLockedForDebug(bool locked)
        {
            debugSequenceEditLocked = locked;
            ApplyAll();
            return GetDebugSummary();
        }

        public string ToggleLoopForDebug()
        {
            ToggleTeachingLoop();
            return GetDebugSummary();
        }

        public string RunFromSelectedForDebug(string pointName)
        {
            RecallPoint(pointName);
            RunFromSelectedPoint();
            return GetDebugSummary();
        }

        public string CreateFunctionForDebug(string functionName)
        {
            SetFunctionName(functionName);
            CreateFunctionFromSequence();
            return GetFunctionDebugSummary();
        }

        public string SelectFunctionForDebug(string functionName)
        {
            SelectFunction(functionName);
            return GetFunctionDebugSummary();
        }

        public string SetFunctionNameForDebug(string functionName)
        {
            SetFunctionName(functionName);
            ApplyAll();
            return GetFunctionDebugSummary();
        }

        public string RenameFunctionForDebug(string functionName)
        {
            SetFunctionName(functionName);
            RenameSelectedFunction();
            return GetFunctionDebugSummary();
        }

        public string DuplicateFunctionForDebug()
        {
            DuplicateSelectedFunction();
            return GetFunctionDebugSummary();
        }

        public string DeleteFunctionForDebug()
        {
            DeleteSelectedFunction();
            return GetFunctionDebugSummary();
        }

        public string RunFunctionForDebug()
        {
            RunSelectedFunction();
            return GetFunctionDebugSummary();
        }

        public string AddSelectedPointToFunctionForDebug(string pointName)
        {
            RecallPoint(pointName);
            AddSelectedPointToFunction();
            return GetFunctionDebugSummary();
        }

        public string ClearFunctionPointSelectionForDebug()
        {
            ClearFunctionPointSelection();
            return GetFunctionDebugSummary();
        }

        public string RunFunctionFromSelectedForDebug(string pointName)
        {
            RecallPoint(pointName);
            RunSelectedFunctionFromSelectedPoint();
            return GetFunctionDebugSummary();
        }

        public string GetFunctionDebugSummary()
        {
            var summary = runtimeController != null ? runtimeController.GetTeachingFunctionSummaryForDebug() : "functions=missing";
            var detail = !string.IsNullOrWhiteSpace(selectedFunctionName) && runtimeController != null
                ? runtimeController.GetTeachingFunctionDetailForDebug(selectedFunctionName)
                : "function=none";
            return $"selectedFunction={selectedFunctionName}; {summary}; {detail}; feedback={lastFeedback}";
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
            ClearPendingConfirmation();
            SetPointName(pointName);
            ApplyAll();
            return GetDebugSummary();
        }

        public string SetPointValueForDebug(string axisLabel, float value)
        {
            var index = AxisIndexFromLabel(axisLabel);
            currentValues[index] = value;
            recalledPoint = null;
            ClearPendingConfirmation();
            ApplyAll();
            return GetDebugSummary();
        }

        public string GetDebugSummary()
        {
            var pointName = desktopPanel?.PointNameInput?.value ?? tabletPanel?.PointNameInput?.value ?? "Point";
            var runtimeRobot = motionRuntime?.RobotId ?? "none";
            var canPreviewAction = CanPreview() && IsAnyPanelVisible();
            var canApplyAction = CanApply() && IsAnyPanelVisible();
            var panel = desktopPanel ?? tabletPanel;
            return $"initialized={isInitialized}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}; surface={ResolveSurfaceDebugName()}; subview={activeTeachingSubview}; pointModalOpen={pointActionModalOpen}; pointModalMode={pointActionModalMode}; tabsHidden={IsHidden(panel?.SubviewTabs)}; motionRowHidden={IsHidden(panel?.MotionRow)}; coordGridHidden={IsHidden(panel?.CoordGrid)}; listHidden={IsHidden(panel?.PointListContainer)}; coord={activeCoordSystem}; motion={motionKind}; speed={selectedSpeedPreset}; dwell={selectedDwellSec:0.0}; editLocked={IsSequenceEditLocked()}; pendingConfirm={pendingConfirmKind}:{pendingConfirmName}; previewState={connectionHomeController.CurrentPreviewState}; canPreview={canPreviewAction}; canApply={canApplyAction}; runtimeRobot={runtimeRobot}; name={pointName}; x={currentValues[0]:0.0}; rz={currentValues[5]:0.0}; feedback={lastFeedback}";
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
            activeNavSection = localState.ActiveNavSection;
            activeCoordSystem = localState.CoordSystem;
            isDesktopVisible = ShouldShowDesktopPanel(localState.ActiveNavSection, localState.ActiveWorkTab);
            isTabletVisible = ShouldShowTabletPanel(localState.ActiveNavSection, localState.ActiveTabletTab);
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
            RegisterClick(panel.BtnPointSubview, () => SetTeachingSubview(PointSubviewName));
            RegisterClick(panel.BtnSequenceSubview, () => SetTeachingSubview(SequenceSubviewName));
            RegisterClick(panel.BtnFunctionSubview, () => SetTeachingSubview(FunctionSubviewName));
            RegisterClick(panel.BtnMoveJ, () => SetMotionKind("MoveJ"));
            RegisterClick(panel.BtnMoveL, () => SetMotionKind("MoveL"));
            RegisterClick(panel.BtnRestore, RestoreFromPreview);
            RegisterClick(panel.BtnSave, SaveCurrentPoint);
            RegisterClick(panel.BtnRecall, () => RecallPoint(panel.PointNameInput?.value));
            RegisterClick(panel.BtnDelete, () => DeletePoint(panel.PointNameInput?.value));
            RegisterClick(panel.BtnRename, () => RenamePoint(recalledPoint?.name, panel.PointNameInput?.value));
            RegisterClick(panel.BtnDuplicate, DuplicateSelectedPoint);
            RegisterClick(panel.BtnUp, () => MovePointInSequence(-1));
            RegisterClick(panel.BtnDown, () => MovePointInSequence(1));
            RegisterClick(panel.BtnOverwrite, OverwriteSelectedPointWithCurrentReadback);
            RegisterClick(panel.BtnSpeedSlow, () => SetSelectedSpeedPreset("slow"));
            RegisterClick(panel.BtnSpeedMedium, () => SetSelectedSpeedPreset("medium"));
            RegisterClick(panel.BtnSpeedFast, () => SetSelectedSpeedPreset("fast"));
            RegisterClick(panel.BtnTimingApply, ApplySelectedPointTiming);
            RegisterClick(panel.BtnLoop, ToggleTeachingLoop);
            RegisterClick(panel.BtnRunSequence, RunActiveSequence);
            RegisterClick(panel.BtnStepBack, StepTeachingBackward);
            RegisterClick(panel.BtnStepForward, StepTeachingForward);
            RegisterClick(panel.BtnStopSequence, StopTeachingSequence);
            RegisterClick(panel.BtnPathRecordStart, StartPathRecording);
            RegisterClick(panel.BtnPathRecordStop, StopPathRecording);
            RegisterClick(panel.BtnPathReplayOnce, PlayRecordedPathOnce);
            RegisterClick(panel.BtnPathReplayLoop, PlayRecordedPathLoop);
            RegisterClick(panel.BtnPathRecordDelete, DeleteRecordedPath);
            RegisterClick(panel.BtnPointModalSpeedSlow, () => SetPointModalSpeedPreset("slow"));
            RegisterClick(panel.BtnPointModalSpeedMedium, () => SetPointModalSpeedPreset("medium"));
            RegisterClick(panel.BtnPointModalSpeedFast, () => SetPointModalSpeedPreset("fast"));
            RegisterClick(panel.BtnPointModalPrimary, ApplyPointActionModalPrimary);
            RegisterClick(panel.BtnPointModalOverwrite, () => ExecutePointModalEditAction(OverwriteSelectedPointWithCurrentReadback));
            RegisterClick(panel.BtnPointModalDuplicate, () => ExecutePointModalEditAction(DuplicateSelectedPoint));
            RegisterClick(panel.BtnPointModalDelete, () => ExecutePointModalEditAction(() => DeletePoint(recalledPoint?.name)));
            RegisterClick(panel.BtnPointModalClose, ClosePointActionModal);
            RegisterClick(panel.BtnFunctionAddPoint, AddSelectedPointToFunction);
            RegisterClick(panel.BtnFunctionClearSelection, ClearFunctionPointSelection);
            RegisterClick(panel.BtnFunctionCreate, CreateFunctionFromSequence);
            RegisterClick(panel.BtnFunctionRun, RunSelectedFunction);
            RegisterClick(panel.BtnFunctionRunFromSelected, RunSelectedFunctionFromSelectedPoint);
            RegisterClick(panel.BtnFunctionRename, RenameSelectedFunction);
            RegisterClick(panel.BtnFunctionDuplicate, DuplicateSelectedFunction);
            RegisterClick(panel.BtnFunctionDelete, DeleteSelectedFunction);
            RegisterClick(panel.BtnExport, ExportPoints);
            RegisterClick(panel.BtnCleanup, CleanupPoints);
            RegisterClick(panel.BtnPreview, PreviewMotionCandidate);
            RegisterClick(panel.BtnRunFromSelected, RunFromSelectedPoint);
            RegisterClick(panel.BtnApply, ApplyMotionCandidate);
            panel.DwellInput?.RegisterValueChangedCallback(evt => HandleDwellChanged(evt.newValue));
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
            ApplySubviewVisibility(panel);
            var isTeachingSurface = activeNavSection == "NavPoints";
            if (panel.Title != null)
            {
                panel.Title.text = isTeachingSurface ? "티칭 포인트" : "포인트 이동";
            }

            panel.Hint.text = isTeachingSurface
                ? "수동으로 맞춘 현재 위치를 저장하고, 저장 포인트를 순서대로 실행하거나 함수로 묶는다."
                : motionKind == "MoveL"
                    ? "직선 접근이 필요할 때는 MoveL 후보로 보고, 먼저 미리보기로 궤적 감각을 확인한다."
                    : "관절 기준으로 먼저 접근해도 되는 위치라면 MoveJ 후보로 빠르게 확인한다.";
            panel.CoordSummary.text = $"좌표계: {activeCoordSystem} / 현재 TCP 기준으로 시작";
            panel.MotionSummary.text = motionKind == "MoveL"
                ? "이동 방식: MoveL / 공구 경로를 직선으로 먼저 확인"
                : "이동 방식: MoveJ / 관절 중심으로 먼저 후보를 확인";
            panel.PreviewSummary.text = BuildDeltaSummary(panel.PointNameInput.value);
            panel.StoreSummary.text = BuildStoreSummary();
            if (panel.PathRecordSummary != null)
            {
                panel.PathRecordSummary.text = FormatPathRecordSummary(runtimeController?.GetTeachingPathRecordingSummaryForDebug());
            }

            ApplyLoopState(panel);
            RebuildPointList(panel);
            ApplySequencePanel(panel);
            ApplyPointDetail(panel);
            ApplyPointActionModal(panel);
            ApplyFunctionPanel(panel);
            panel.FeedbackSummary.text = lastFeedback;
            var canPreview = CanPreview() && IsAnyPanelVisible();
            var canApply = CanApply() && IsAnyPanelVisible();
            var canEdit = !IsSequenceEditLocked();
            panel.BtnRestore.SetEnabled(canPreview);
            panel.BtnPreview.SetEnabled(canPreview);
            panel.BtnRunFromSelected.SetEnabled(canApply && recalledPoint != null && !IsSequenceEditLocked());
            panel.BtnApply.SetEnabled(canApply);
            panel.BtnSave.SetEnabled(canEdit);
            panel.BtnDelete.SetEnabled(canEdit && (recalledPoint != null || HasNamedPoint(panel.PointNameInput?.value)));
            panel.BtnRename.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnDuplicate.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnUp.SetEnabled(canEdit && CanMoveSelectedPoint(-1));
            panel.BtnDown.SetEnabled(canEdit && CanMoveSelectedPoint(1));
            panel.BtnOverwrite.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnTimingApply.SetEnabled(canEdit && recalledPoint != null && !isDwellInvalid);
            panel.BtnPointModalPrimary?.SetEnabled(recalledPoint != null && !IsSequenceEditLocked());
            panel.BtnPointModalOverwrite?.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnPointModalDuplicate?.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnPointModalDelete?.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnLoop.SetEnabled(HasAnyPoint() && !IsSequenceEditLocked());
            panel.BtnRunSequence.SetEnabled(canApply && HasAnyPoint());
            panel.BtnStepBack.SetEnabled(canApply && HasAnyPoint());
            panel.BtnStepForward.SetEnabled(canApply && HasAnyPoint());
            panel.BtnStopSequence.SetEnabled(runtimeController != null);
            panel.BtnPathRecordStart?.SetEnabled(canApply && !IsSequenceEditLocked());
            panel.BtnPathRecordStop?.SetEnabled(runtimeController != null);
            panel.BtnPathReplayOnce?.SetEnabled(canApply && !IsSequenceEditLocked());
            panel.BtnPathReplayLoop?.SetEnabled(canApply && !IsSequenceEditLocked());
            panel.BtnPathRecordDelete?.SetEnabled(canEdit && HasNamedSequence(RecordedPathSequenceName));
            panel.BtnExport.SetEnabled(HasAnyPoint());
            panel.BtnCleanup.SetEnabled(canEdit && HasAnyPoint());
            panel.BtnFunctionCreate.SetEnabled(canEdit && HasAnyPoint());
            panel.BtnFunctionRun.SetEnabled(canApply && !string.IsNullOrWhiteSpace(selectedFunctionName));
            panel.BtnFunctionAddPoint.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnFunctionClearSelection.SetEnabled(canEdit && selectedFunctionPointNames.Count > 0);
            panel.BtnFunctionRunFromSelected.SetEnabled(canApply && recalledPoint != null && !string.IsNullOrWhiteSpace(selectedFunctionName));
            panel.BtnFunctionRename.SetEnabled(canEdit && !string.IsNullOrWhiteSpace(selectedFunctionName));
            panel.BtnFunctionDuplicate.SetEnabled(canEdit && !string.IsNullOrWhiteSpace(selectedFunctionName));
            panel.BtnFunctionDelete.SetEnabled(canEdit && !string.IsNullOrWhiteSpace(selectedFunctionName));
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

        private void SetTeachingSubview(string subviewName)
        {
            activeTeachingSubview = subviewName == SequenceSubviewName || subviewName == FunctionSubviewName
                ? subviewName
                : PointSubviewName;
            ApplyAll();
        }

        private void ApplySubviewVisibility(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            var isTeachingSurface = activeNavSection == "NavPoints";
            if (!isTeachingSurface)
            {
                SetHidden(panel.MotionRow, false);
                SetHidden(panel.SubviewTabs, true);
                SetHidden(panel.NameRow, true);
                SetHidden(panel.BtnPrimarySave, true);
                SetHidden(panel.PointSubview, false);
                SetHidden(panel.SequenceSubview, true);
                SetHidden(panel.FunctionSubview, true);
                SetHidden(panel.CoordRow, false);
                SetHidden(panel.CoordGrid, false);
                SetHidden(panel.PointListContainer, true);
                SetHidden(panel.PointEditSubview, false);
                SetHidden(panel.DetailCard, true);
                SetHidden(panel.PointEditActions, false);
                ApplyMoveTargetActionVisibility(panel);
                return;
            }

            var showPoint = activeTeachingSubview == PointSubviewName;
            var showSequence = activeTeachingSubview == SequenceSubviewName;
            var showFunction = activeTeachingSubview == FunctionSubviewName;
            SetHidden(panel.MotionRow, true);
            SetHidden(panel.SubviewTabs, false);
            SetHidden(panel.NameRow, false);
            SetHidden(panel.BtnPrimarySave, false);
            panel.BtnPointSubview?.EnableInClassList("rc-point-subview-tab--active", showPoint);
            panel.BtnSequenceSubview?.EnableInClassList("rc-point-subview-tab--active", showSequence);
            panel.BtnFunctionSubview?.EnableInClassList("rc-point-subview-tab--active", showFunction);
            panel.PointSubview?.EnableInClassList("rc-hidden", !showPoint);
            panel.SequenceSubview?.EnableInClassList("rc-hidden", !showSequence);
            panel.FunctionSubview?.EnableInClassList("rc-hidden", !showFunction);
            SetHidden(panel.CoordRow, true);
            SetHidden(panel.CoordGrid, true);
            SetHidden(panel.PointListContainer, false);
            panel.PointEditSubview?.EnableInClassList("rc-hidden", !showPoint);
            SetHidden(panel.DetailCard, false);
            panel.PointEditActions?.EnableInClassList("rc-hidden", !showPoint);
            ApplyTeachingActionVisibility(panel);
        }

        private static void ApplyMoveTargetActionVisibility(PanelElements panel)
        {
            SetHidden(panel.BtnRestore, false);
            SetHidden(panel.BtnPreview, false);
            SetHidden(panel.BtnApply, false);
            SetHidden(panel.BtnRecall, true);
            SetHidden(panel.BtnDelete, true);
            SetHidden(panel.BtnRename, true);
            SetHidden(panel.BtnDuplicate, true);
            SetHidden(panel.BtnUp, true);
            SetHidden(panel.BtnDown, true);
            SetHidden(panel.BtnOverwrite, true);
            SetHidden(panel.BtnExport, true);
            SetHidden(panel.BtnCleanup, true);
        }

        private static void ApplyTeachingActionVisibility(PanelElements panel)
        {
            SetHidden(panel.BtnRestore, false);
            SetHidden(panel.BtnPreview, false);
            SetHidden(panel.BtnApply, false);
            SetHidden(panel.BtnRecall, false);
            SetHidden(panel.BtnDelete, false);
            SetHidden(panel.BtnRename, false);
            SetHidden(panel.BtnDuplicate, false);
            SetHidden(panel.BtnUp, false);
            SetHidden(panel.BtnDown, false);
            SetHidden(panel.BtnOverwrite, false);
            SetHidden(panel.BtnExport, false);
            SetHidden(panel.BtnCleanup, false);
        }

        private static void SetHidden(VisualElement element, bool hidden)
        {
            element?.EnableInClassList("rc-hidden", hidden);
        }

        private static bool IsHidden(VisualElement element)
        {
            return element == null || element.ClassListContains("rc-hidden");
        }

        private string ResolveSurfaceDebugName()
        {
            return activeNavSection == "NavPoints" ? "Teaching" : "MoveTarget";
        }

        private void RunActiveSequence()
        {
            if (runtimeController == null)
            {
                SetFeedback("시퀀스 실행 runtime을 찾지 못했다.");
                return;
            }

            runtimeController.ExecutePrimaryAction();
            SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
        }

        private void StepTeachingBackward()
        {
            if (runtimeController == null)
            {
                SetFeedback("Step 실행 runtime을 찾지 못했다.");
                return;
            }

            runtimeController.StepBackward();
            SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
        }

        private void StepTeachingForward()
        {
            if (runtimeController == null)
            {
                SetFeedback("Step 실행 runtime을 찾지 못했다.");
                return;
            }

            runtimeController.StepForward();
            SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
        }

        private void StopTeachingSequence()
        {
            if (runtimeController == null)
            {
                SetFeedback("Stop runtime을 찾지 못했다.");
                return;
            }

            var result = runtimeController.StopMotion();
            SetFeedback(result.Message);
        }

        private void StartPathRecording()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 새 경로 기록을 시작하지 않는다. Stop 후 다시 해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.StartTeachingPathRecording()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void StopPathRecording()
        {
            var result = runtimeController != null
                ? runtimeController.StopTeachingPathRecording()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void PlayRecordedPathOnce()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 기록 재생을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.PlayRecordedTeachingPathOnce()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void PlayRecordedPathLoop()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 기록 루프를 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.PlayRecordedTeachingPathLoop()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void DeleteRecordedPath()
        {
            SelectSequence(RecordedPathSequenceName);
            DeleteSelectedSequence();
        }

        private void SelectSequence(string sequenceName)
        {
            var safeName = string.IsNullOrWhiteSpace(sequenceName)
                ? PointSequenceName
                : sequenceName.Trim();
            selectedSequenceName = safeName;
            SetFeedback($"[Sequence] {GetSequenceDisplayName(safeName)} 선택");
            ApplyAll();
        }

        private void RunSelectedSequenceOnce()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 새 실행을 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 실행할 수 없다.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteWaypointSequenceOnce(selectedSequenceName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void RunSelectedSequenceLoop()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 새 루프를 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 루프 실행할 수 없다.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteWaypointSequenceLoop(selectedSequenceName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void DeleteSelectedSequence()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 실행 목록 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

            if (string.Equals(selectedSequenceName, PointSequenceName, System.StringComparison.OrdinalIgnoreCase))
            {
                SetFeedback("저장한 포인트 순서는 포인트 탭의 삭제/정리로 관리한다. 여기서는 기록한 경로와 별도 실행 목록만 삭제한다.");
                return;
            }

            if (!HasNamedSequence(selectedSequenceName))
            {
                SetFeedback($"{GetSequenceDisplayName(selectedSequenceName)} 실행 목록을 찾지 못했다.");
                selectedSequenceName = PointSequenceName;
                ApplyAll();
                return;
            }

            if (!IsPendingConfirmation("delete-sequence", selectedSequenceName))
            {
                SetPendingConfirmation("delete-sequence", selectedSequenceName);
                SetFeedback($"[Confirm] {GetSequenceDisplayName(selectedSequenceName)} 삭제 예정. 삭제를 한 번 더 누르면 기록/실행 목록이 지워진다.");
                return;
            }

            var deletedName = selectedSequenceName;
            var result = runtimeController != null
                ? runtimeController.DeleteWaypointSequence(deletedName)
                : WaypointStore.Delete(deletedName)
                    ? $"[Sequence] {deletedName} 삭제"
                    : $"[Sequence] {deletedName} 삭제 실패";
            selectedSequenceName = PointSequenceName;
            ClearPendingConfirmation();
            SetFeedback(result);
            ApplyAll();
        }

        private void CreateFunctionFromSequence()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 함수 생성을 잠근다. Stop 후 다시 묶어라.");
                return;
            }

            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            var functionName = panel?.FunctionNameInput?.value?.Trim();
            if (string.IsNullOrWhiteSpace(functionName))
            {
                SetFeedback("함수 이름을 먼저 넣어라.");
                return;
            }

            var result = runtimeController != null
                ? selectedFunctionPointNames.Count > 0
                    ? runtimeController.CreateTeachingFunctionFromPoints(functionName, selectedFunctionPointNames.ToArray())
                    : runtimeController.CreateTeachingFunctionFromSequence(functionName)
                : "runtime missing";
            selectedFunctionName = ExtractCreatedFunctionName(result, functionName);
            SetFunctionName(selectedFunctionName);
            SetFeedback(result);
            ApplyAll();
        }

        private void AddSelectedPointToFunction()
        {
            if (recalledPoint == null)
            {
                SetFeedback("함수 후보에 넣을 포인트를 먼저 선택해라.");
                return;
            }

            if (!selectedFunctionPointNames.Contains(recalledPoint.name))
            {
                selectedFunctionPointNames.Add(recalledPoint.name);
            }

            SetFeedback($"[Function] 후보 추가 · {recalledPoint.name}");
            ApplyAll();
        }

        private void ClearFunctionPointSelection()
        {
            selectedFunctionPointNames.Clear();
            SetFeedback("[Function] 함수 후보 초기화 · 전체 저장 포인트 기준으로 돌아감");
            ApplyAll();
        }

        private void RunSelectedFunction()
        {
            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("실행할 함수를 먼저 선택해라.");
                return;
            }

            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 함수 실행을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteTeachingFunctionOnceDryRun(selectedFunctionName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void RunSelectedFunctionFromSelectedPoint()
        {
            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("실행할 함수를 먼저 선택해라.");
                return;
            }

            if (recalledPoint == null)
            {
                SetFeedback("함수 안에서 시작할 포인트를 먼저 선택해라.");
                return;
            }

            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 함수 선택 실행을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteTeachingFunctionFromPointDryRun(selectedFunctionName, recalledPoint.name)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void RenameSelectedFunction()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 함수 이름 변경을 잠근다. Stop 후 다시 수정해라.");
                return;
            }

            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            var newName = panel?.FunctionNameInput?.value?.Trim();
            if (string.IsNullOrWhiteSpace(selectedFunctionName) || string.IsNullOrWhiteSpace(newName))
            {
                SetFeedback("선택된 함수와 새 이름이 필요하다.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.RenameTeachingFunctionForDebug(selectedFunctionName, newName)
                : "runtime missing";
            selectedFunctionName = newName;
            SetFeedback(result);
            ApplyAll();
        }

        private void DuplicateSelectedFunction()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 함수 복사를 잠근다. Stop 후 다시 복사해라.");
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("복사할 함수를 먼저 선택해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.DuplicateTeachingFunctionForDebug(selectedFunctionName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void DeleteSelectedFunction()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 함수 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("삭제할 함수를 먼저 선택해라.");
                return;
            }

            var deletedName = selectedFunctionName;
            var result = runtimeController != null
                ? runtimeController.DeleteTeachingFunctionForDebug(deletedName)
                : "runtime missing";
            selectedFunctionName = string.Empty;
            SelectFirstExistingFunctionIfNeeded();
            SetFeedback(result);
            ApplyAll();
        }

        private void SelectFunction(string functionName)
        {
            selectedFunctionName = functionName?.Trim() ?? string.Empty;
            SetFunctionName(selectedFunctionName);
            SetFeedback(string.IsNullOrWhiteSpace(selectedFunctionName)
                ? "선택된 함수가 없다."
                : $"[Function] {selectedFunctionName} 선택");
            ApplyAll();
        }

        private void SetFunctionName(string functionName)
        {
            var safeName = functionName?.Trim() ?? string.Empty;
            desktopPanel?.FunctionNameInput?.SetValueWithoutNotify(safeName);
            tabletPanel?.FunctionNameInput?.SetValueWithoutNotify(safeName);
        }

        private static string ExtractCreatedFunctionName(string result, string fallbackName)
        {
            const string marker = "[Function] ";
            const string endMarker = " 생성";
            if (string.IsNullOrWhiteSpace(result))
            {
                return fallbackName?.Trim() ?? string.Empty;
            }

            var start = result.IndexOf(marker, System.StringComparison.Ordinal);
            if (start < 0)
            {
                return fallbackName?.Trim() ?? string.Empty;
            }

            start += marker.Length;
            var end = result.IndexOf(endMarker, start, System.StringComparison.Ordinal);
            return end > start
                ? result.Substring(start, end - start).Trim()
                : fallbackName?.Trim() ?? string.Empty;
        }

        private void SelectFirstExistingFunctionIfNeeded()
        {
            if (runtimeController == null)
            {
                return;
            }

            var names = runtimeController.GetTeachingFunctionNames();
            if (names.Length == 0)
            {
                selectedFunctionName = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedFunctionName) || System.Array.IndexOf(names, selectedFunctionName) < 0)
            {
                selectedFunctionName = names[0];
                SetFunctionName(selectedFunctionName);
            }
        }

        private void ApplyFunctionPanel(PanelElements panel)
        {
            if (panel?.FunctionSummary == null)
            {
                return;
            }

            SelectFirstExistingFunctionIfNeeded();
            var functionNames = runtimeController != null
                ? runtimeController.GetTeachingFunctionNames()
                : System.Array.Empty<string>();
            if (panel.FunctionBuildSummary != null)
            {
                panel.FunctionBuildSummary.text = selectedFunctionPointNames.Count == 0
                    ? "선택한 포인트 0개 → 포인트 row의 후보 버튼으로 추가"
                    : $"선택한 포인트 {selectedFunctionPointNames.Count}개 → 함수 만들기";
            }

            panel.FunctionSummary.text = string.IsNullOrWhiteSpace(selectedFunctionName)
                ? $"함수: {functionNames.Length}개"
                : $"함수: {functionNames.Length}개 / 선택: {selectedFunctionName}";
            panel.FunctionSelectionSummary.text = selectedFunctionPointNames.Count == 0
                ? "함수 후보: 전체 저장 포인트"
                : $"함수 후보: {string.Join(" / ", selectedFunctionPointNames)}";
            panel.FunctionDetail.text = !string.IsNullOrWhiteSpace(selectedFunctionName) && runtimeController != null
                ? FormatFunctionDetailForUi(runtimeController.GetTeachingFunctionDetailForDebug(selectedFunctionName))
                : "함수를 선택하면 참조 포인트가 보인다.";
            RebuildFunctionList(panel);
        }

        private static string FormatFunctionDetailForUi(string rawDetail)
        {
            if (string.IsNullOrWhiteSpace(rawDetail) || rawDetail.Contains("function=none"))
            {
                return "함수를 선택하면 참조 포인트가 보인다.";
            }

            var name = ExtractDebugValue(rawDetail, "function=");
            var steps = ExtractDebugValue(rawDetail, "steps=");
            var missingCount = ExtractDebugValue(rawDetail, "missingCount=");
            var missing = ExtractDebugBracketValue(rawDetail, "missing=[");
            return missingCount == "0" || string.IsNullOrWhiteSpace(missingCount)
                ? $"{name} · {steps}개 포인트 · 누락 없음"
                : $"{name} · {steps}개 포인트 · 누락 {missingCount}: {missing}";
        }

        private static string FormatPathRecordSummary(string rawSummary)
        {
            if (string.IsNullOrWhiteSpace(rawSummary))
            {
                return "기록: 대기 / 샘플 0개";
            }

            var recording = ExtractDebugValue(rawSummary, "recording=");
            var samples = ExtractDebugValue(rawSummary, "samples=");
            var saved = ExtractDebugValue(rawSummary, "saved=");
            var runner = ExtractDebugValue(rawSummary, "runner=");
            var state = string.Equals(recording, "True", System.StringComparison.OrdinalIgnoreCase)
                ? "기록 중"
                : "대기";
            return $"기록: {state} / 샘플 {samples}개 / 저장 {saved}개 / 재생 {runner}";
        }

        private static string ExtractDebugValue(string raw, string key)
        {
            var start = raw.IndexOf(key, System.StringComparison.Ordinal);
            if (start < 0)
            {
                return string.Empty;
            }

            start += key.Length;
            var end = raw.IndexOf(';', start);
            return end > start
                ? raw.Substring(start, end - start).Trim()
                : raw.Substring(start).Trim();
        }

        private static string ExtractDebugBracketValue(string raw, string key)
        {
            var start = raw.IndexOf(key, System.StringComparison.Ordinal);
            if (start < 0)
            {
                return string.Empty;
            }

            start += key.Length;
            var end = raw.IndexOf(']', start);
            return end >= start
                ? raw.Substring(start, end - start).Trim()
                : raw.Substring(start).Trim();
        }

        private void RebuildFunctionList(PanelElements panel)
        {
            if (panel?.FunctionListContainer == null)
            {
                return;
            }

            panel.FunctionListContainer.Clear();
            var names = runtimeController != null
                ? runtimeController.GetTeachingFunctionNames()
                : System.Array.Empty<string>();
            var visibleCount = System.Math.Min(names.Length, 5);
            for (var index = 0; index < visibleCount; index++)
            {
                var functionName = names[index];
                var button = new Button(() => SelectFunction(functionName))
                {
                    text = ShortDisplayName(functionName)
                };
                button.AddToClassList("rc-point-list-button");
                button.EnableInClassList(
                    "rc-point-list-button--active",
                    string.Equals(selectedFunctionName, functionName, System.StringComparison.OrdinalIgnoreCase));
                panel.FunctionListContainer.Add(button);
            }
        }

        private void ToggleTeachingLoop()
        {
            if (runtimeController == null)
            {
                SetFeedback("반복 실행 상태를 바꿀 runtime을 찾지 못했다.");
                return;
            }

            var enabled = runtimeController.ToggleTeachingLoopEnabled();
            SetFeedback(enabled
                ? "[Loop] 반복 실행 ON · Run을 누르면 저장 포인트를 반복한다."
                : "[Loop] 반복 실행 OFF · Run은 한 번만 실행한다.");
            ApplyAll();
        }

        private void RunFromSelectedPoint()
        {
            if (!IsAnyPanelVisible())
            {
                SetFeedback("포인트 이동 패널이 열려 있을 때만 선택부터 실행할 수 있다.");
                return;
            }

            if (recalledPoint == null)
            {
                SetFeedback("선택부터 실행할 포인트를 먼저 선택해라.");
                return;
            }

            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 선택부터 실행을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 선택부터 실행할 수 없다.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteTeachingSequenceFromPoint(recalledPoint.name)
                : "runtime missing";
            SetFeedback(result);
        }

        private void ApplyLoopState(PanelElements panel)
        {
            if (panel?.BtnLoop == null)
            {
                return;
            }

            var loopEnabled = runtimeController != null && runtimeController.IsTeachingLoopEnabled;
            var running = runtimeController != null && runtimeController.IsTeachingSequenceRunning;
            panel.BtnLoop.text = loopEnabled ? "반복 ON" : "반복 OFF";
            panel.BtnLoop.EnableInClassList("rc-point-loop-button--active", loopEnabled);
            panel.LoopStatus.text = loopEnabled
                ? running ? "반복 실행: 진행 중 · Stop으로 종료" : "반복 실행: 켜짐 · Run으로 시작"
                : "반복 실행: 꺼짐";
        }

        private void HandleDwellChanged(string rawValue)
        {
            if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                || double.IsNaN(parsed)
                || double.IsInfinity(parsed)
                || parsed < 0.0
                || parsed > 600.0)
            {
                isDwellInvalid = true;
                ApplyPanel(desktopPanel);
                ApplyPanel(tabletPanel);
                return;
            }

            selectedDwellSec = parsed;
            isDwellInvalid = false;
            ClearPendingConfirmation();
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
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

        private static bool ShouldShowDesktopPanel(string activeNavSection, string activeWorkTab)
        {
            return activeNavSection == "NavPoints" ||
                activeNavSection == "NavMotion" && activeWorkTab == "TabPointMove";
        }

        private static bool ShouldShowTabletPanel(string activeNavSection, string activeTabletTab)
        {
            return activeNavSection == "NavPoints" || activeTabletTab == "BottomTabPointMove";
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
            ClearPendingConfirmation();
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
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 포인트 저장/편집을 잠근다. Stop 후 다시 저장해라.");
                return;
            }

            if (!IsAnyPanelVisible())
            {
                SetFeedback("포인트 이동 패널이 열려 있을 때만 저장할 수 있다.");
                return;
            }

            if (!TryReadActivePointName(out var pointName, out var validationMessage))
            {
                SetFeedback(validationMessage);
                return;
            }

            var sequence = LoadPointSequenceIfExists() ?? WaypointStore.CreateEmpty(PointSequenceName);
            var existingIndex = FindWaypointIndex(sequence, pointName);
            var existingPoint = existingIndex >= 0 ? sequence.waypoints[existingIndex] : null;
            var waypoint = new Waypoint
            {
                name = pointName,
                jointsDeg = ReadCurrentSnapshotJoints(),
                tcpMm = ReadCurrentSnapshotTcp(),
                moveType = motionKind,
                speedPreset = NormalizeSpeedPreset(existingPoint?.speedPreset ?? "medium"),
                dwellSec = existingPoint?.dwellSec ?? 0.0
            };

            if (existingIndex >= 0 && !IsPendingConfirmation("save-overwrite", pointName))
            {
                SetPendingConfirmation("save-overwrite", pointName);
                SetFeedback($"[Confirm] {pointName} 이름이 이미 있다. 같은 이름으로 저장하려면 저장을 한 번 더 눌러 기존 위치를 덮어쓴다.");
                return;
            }

            ReplaceWaypoint(sequence, waypoint);
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("포인트 저장 실패");
                return;
            }

            ClearPendingConfirmation();
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
            selectedSpeedPreset = NormalizeSpeedPreset(recalledPoint.speedPreset);
            selectedDwellSec = recalledPoint.dwellSec;
            isDwellInvalid = false;
            ClearPendingConfirmation();
            for (var index = 0; index < currentValues.Length && index < recalledPoint.tcpMm.Length; index++)
            {
                currentValues[index] = (float)recalledPoint.tcpMm[index];
            }

            SetFeedback($"[Recall] {recalledPoint.name} 불러옴 · {motionKind}");
        }

        private void DeletePoint(string requestedName)
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

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

            if (!IsPendingConfirmation("delete", pointName))
            {
                SetPendingConfirmation("delete", pointName);
                SetFeedback($"[Confirm] {pointName} 삭제 예정. 이 포인트는 순서 목록에서 제거된다. 삭제를 한 번 더 누르면 실행한다.");
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

            ClearPendingConfirmation();
            SetFeedback($"[Delete] {deletedName} 삭제");
        }

        private void RenamePoint(string oldName, string newName)
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 이름 변경을 잠근다. Stop 후 다시 수정해라.");
                return;
            }

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

            ClearPendingConfirmation();
            recalledPoint = CloneWaypoint(sequence.waypoints[oldIndex]);
            SetPointName(toName);
            SetFeedback($"[Rename] {fromName} -> {toName}");
        }

        private void DuplicateSelectedPoint()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 복사를 잠근다. Stop 후 다시 복사해라.");
                return;
            }

            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0 || recalledPoint == null)
            {
                SetFeedback("복사할 저장 포인트를 먼저 선택해라.");
                return;
            }

            var sourceIndex = FindWaypointIndex(sequence, recalledPoint.name);
            if (sourceIndex < 0)
            {
                SetFeedback($"{recalledPoint.name} 포인트를 찾지 못했다.");
                return;
            }

            var duplicate = CloneWaypoint(sequence.waypoints[sourceIndex]);
            duplicate.name = BuildUniqueDuplicateName(sequence, duplicate.name);
            InsertWaypointAfter(sequence, duplicate, sourceIndex);
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("포인트 복사 저장 실패");
                return;
            }

            ClearPendingConfirmation();
            recalledPoint = CloneWaypoint(duplicate);
            SetPointName(recalledPoint.name);
            for (var valueIndex = 0; valueIndex < currentValues.Length && valueIndex < recalledPoint.tcpMm.Length; valueIndex++)
            {
                currentValues[valueIndex] = (float)recalledPoint.tcpMm[valueIndex];
            }

            SetFeedback($"[Duplicate] {sequence.waypoints[sourceIndex].name} -> {recalledPoint.name}");
        }

        private void MovePointInSequence(int direction)
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 순서 변경을 잠근다. Stop 후 다시 이동해라.");
                return;
            }

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

            ClearPendingConfirmation();
            recalledPoint = CloneWaypoint(sequence.waypoints[targetIndex]);
            SetFeedback($"[Order] {recalledPoint.name} {targetIndex + 1}번째로 이동");
        }

        private void OverwriteSelectedPointWithCurrentReadback()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 덮어쓰기를 잠근다. Stop 후 다시 덮어써라.");
                return;
            }

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

            if (!IsPendingConfirmation("overwrite", recalledPoint.name))
            {
                SetPendingConfirmation("overwrite", recalledPoint.name);
                SetFeedback($"[Confirm] {recalledPoint.name} 현재 readback으로 덮어쓰기 예정. 이름/MoveType/speed/dwell은 유지하고 joints/TCP만 바뀐다. 덮어쓰기를 한 번 더 눌러라.");
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

            ClearPendingConfirmation();
            SetFeedback($"[Overwrite] {recalledPoint.name} 현재 readback으로 갱신");
        }

        private void ApplySelectedPointTiming()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 포인트 편집을 잠근다. Stop 후 다시 수정해라.");
                return;
            }

            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0 || recalledPoint == null)
            {
                SetFeedback("속도/대기를 수정할 포인트를 먼저 선택해라.");
                return;
            }

            if (isDwellInvalid || selectedDwellSec < 0.0 || selectedDwellSec > 600.0)
            {
                isDwellInvalid = true;
                ApplyAll();
                SetFeedback("대기 시간은 0~600초 사이 숫자로 넣어라.");
                return;
            }

            var index = FindWaypointIndex(sequence, recalledPoint.name);
            if (index < 0)
            {
                SetFeedback($"{recalledPoint.name} 포인트를 찾지 못했다.");
                return;
            }

            var waypoint = sequence.waypoints[index];
            waypoint.speedPreset = NormalizeSpeedPreset(selectedSpeedPreset);
            waypoint.dwellSec = selectedDwellSec;
            sequence.waypoints[index] = waypoint;
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("속도/대기 저장 실패");
                return;
            }

            ClearPendingConfirmation();
            recalledPoint = CloneWaypoint(waypoint);
            SetFeedback($"[Timing] {recalledPoint.name} · {recalledPoint.speedPreset} · dwell {recalledPoint.dwellSec:0.0}s");
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
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 정리를 잠근다. Stop 후 다시 정리해라.");
                return;
            }

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

        private bool TryReadActivePointName(out string pointName, out string validationMessage)
        {
            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            pointName = panel?.PointNameInput?.value?.Trim() ?? string.Empty;
            validationMessage = "포인트 이름 검증 통과";
            isPointNameInvalid = false;

            if (panel == null)
            {
                validationMessage = "포인트 이동 패널을 찾지 못했다.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(pointName))
            {
                isPointNameInvalid = true;
                ApplyPanel(desktopPanel);
                ApplyPanel(tabletPanel);
                validationMessage = "포인트 이름을 먼저 넣어라.";
                return false;
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
                var row = new VisualElement();
                row.AddToClassList("rc-point-row");
                row.EnableInClassList(
                    "rc-point-row--active",
                    recalledPoint != null && string.Equals(recalledPoint.name, waypoint.name, System.StringComparison.OrdinalIgnoreCase));
                row.RegisterCallback<ClickEvent>(_ => RecallPoint(capturedName));

                var summary = new Label(BuildPointRowSummary(waypoint));
                summary.AddToClassList("rc-point-row-summary");
                row.Add(summary);

                var actions = new VisualElement();
                actions.AddToClassList("rc-point-row-actions");
                actions.Add(CreatePointRowButton("BtnPointRowMove", "실행", () => MovePointRow(capturedName)));
                actions.Add(CreatePointRowButton("BtnPointRowPreview", "미리보기", () => PreviewPointRow(capturedName)));
                actions.Add(CreatePointRowButton("BtnPointRowEdit", "편집", () => EditPointRow(capturedName)));
                actions.Add(CreatePointRowButton("BtnPointRowFunctionCandidate", "함수 추가", () => AddPointRowToFunction(capturedName)));
                row.Add(actions);
                panel.PointListContainer.Add(row);
            }

            if (waypoints.Length > visibleCount)
            {
                var more = new Label($"+{waypoints.Length - visibleCount} more");
                more.AddToClassList("rc-panel-copy");
                more.AddToClassList("rc-panel-copy--compact");
                panel.PointListContainer.Add(more);
            }
        }

        private void ApplySequencePanel(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            if (!HasNamedSequence(selectedSequenceName))
            {
                selectedSequenceName = PointSequenceName;
            }

            if (panel.SequenceLibrarySummary != null)
            {
                panel.SequenceLibrarySummary.text = BuildSequenceLibraryUiSummary();
            }

            if (panel.SelectedSequenceDetail != null)
            {
                panel.SelectedSequenceDetail.text = BuildSelectedSequenceDetail();
            }

            RebuildSequenceList(panel);
        }

        private void RebuildSequenceList(PanelElements panel)
        {
            if (panel?.SequenceListContainer == null)
            {
                return;
            }

            panel.SequenceListContainer.Clear();
            var names = BuildOrderedSequenceNames();
            var visibleCount = System.Math.Min(names.Count, 6);
            for (var index = 0; index < visibleCount; index++)
            {
                var sequenceName = names[index];
                var row = new VisualElement();
                row.AddToClassList("rc-point-row");
                row.EnableInClassList(
                    "rc-point-row--active",
                    string.Equals(selectedSequenceName, sequenceName, System.StringComparison.OrdinalIgnoreCase));

                var summary = new Label(BuildSequenceRowSummary(sequenceName));
                summary.AddToClassList("rc-point-row-summary");
                row.Add(summary);

                var actions = new VisualElement();
                actions.AddToClassList("rc-point-row-actions");
                actions.Add(CreatePointRowButton("BtnSequenceRowSelect", "선택", () => SelectSequence(sequenceName)));
                actions.Add(CreatePointRowButton("BtnSequenceRowRun", "재생", () =>
                {
                    SelectSequence(sequenceName);
                    RunSelectedSequenceOnce();
                }));
                actions.Add(CreatePointRowButton("BtnSequenceRowLoop", "루프", () =>
                {
                    SelectSequence(sequenceName);
                    RunSelectedSequenceLoop();
                }));

                var deleteButton = CreatePointRowButton("BtnSequenceRowDelete", "삭제", () =>
                {
                    SelectSequence(sequenceName);
                    DeleteSelectedSequence();
                });
                deleteButton.SetEnabled(!string.Equals(sequenceName, PointSequenceName, System.StringComparison.OrdinalIgnoreCase));
                actions.Add(deleteButton);
                row.Add(actions);
                panel.SequenceListContainer.Add(row);
            }

            if (names.Count == 0)
            {
                var empty = new Label("실행 목록 없음 · 포인트를 저장하거나 경로를 기록하면 여기에 뜬다.");
                empty.AddToClassList("rc-panel-copy");
                empty.AddToClassList("rc-panel-copy--compact");
                panel.SequenceListContainer.Add(empty);
            }
        }

        private string BuildSequenceLibraryUiSummary()
        {
            var pointCount = CountSequenceWaypoints(PointSequenceName);
            var recordedCount = CountSequenceWaypoints(RecordedPathSequenceName);
            var names = WaypointStore.LoadAllNames();
            var otherCount = 0;
            for (var index = 0; index < names.Length; index++)
            {
                if (!string.Equals(names[index], PointSequenceName, System.StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(names[index], RecordedPathSequenceName, System.StringComparison.OrdinalIgnoreCase))
                {
                    otherCount++;
                }
            }

            return $"저장한 포인트 순서 {pointCount}개 / 기록한 경로 {recordedCount}개 / 기타 {otherCount}개";
        }

        private string BuildSelectedSequenceDetail()
        {
            var sequence = LoadSequenceIfExists(selectedSequenceName);
            var count = sequence?.waypoints?.Length ?? 0;
            return $"{GetSequenceDisplayName(selectedSequenceName)} · {count}개 포인트";
        }

        private string BuildSequenceRowSummary(string sequenceName)
        {
            var sequence = LoadSequenceIfExists(sequenceName);
            var count = sequence?.waypoints?.Length ?? 0;
            var first = count > 0 ? sequence.waypoints[0]?.name : "-";
            var last = count > 1 ? sequence.waypoints[count - 1]?.name : first;
            return $"{GetSequenceDisplayName(sequenceName)} · {count}개 · {ShortDisplayName(first)} → {ShortDisplayName(last)}";
        }

        private string BuildSequenceLibraryDebugSummary()
        {
            var names = BuildOrderedSequenceNames();
            var parts = new string[names.Count];
            for (var index = 0; index < names.Count; index++)
            {
                parts[index] = $"{names[index]}:{CountSequenceWaypoints(names[index])}";
            }

            return $"selectedSequence={selectedSequenceName}; pointCount={CountSequenceWaypoints(PointSequenceName)}; recordedPathCount={CountSequenceWaypoints(RecordedPathSequenceName)}; {BuildSequenceLibraryUiSummary()}; sequences=[{string.Join(",", parts)}]; feedback={lastFeedback}";
        }

        private static List<string> BuildOrderedSequenceNames()
        {
            var result = new List<string>();
            if (HasNamedSequence(PointSequenceName))
            {
                result.Add(PointSequenceName);
            }

            if (HasNamedSequence(RecordedPathSequenceName))
            {
                result.Add(RecordedPathSequenceName);
            }

            var names = WaypointStore.LoadAllNames();
            System.Array.Sort(names, System.StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < names.Length; index++)
            {
                var name = names[index];
                if (string.Equals(name, PointSequenceName, System.StringComparison.OrdinalIgnoreCase)
                    || string.Equals(name, RecordedPathSequenceName, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(name);
            }

            return result;
        }

        private static string GetSequenceDisplayName(string sequenceName)
        {
            if (string.Equals(sequenceName, PointSequenceName, System.StringComparison.OrdinalIgnoreCase))
            {
                return "저장한 포인트 순서";
            }

            if (string.Equals(sequenceName, RecordedPathSequenceName, System.StringComparison.OrdinalIgnoreCase))
            {
                return "기록한 경로";
            }

            return string.IsNullOrWhiteSpace(sequenceName) ? "실행 목록" : sequenceName.Trim();
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

        private static string BuildPointRowSummary(Waypoint waypoint)
        {
            var speed = string.IsNullOrWhiteSpace(waypoint.speedPreset) ? "medium" : waypoint.speedPreset;
            var dwell = waypoint.dwellSec.ToString("0.0", CultureInfo.InvariantCulture);
            var tcp = waypoint.tcpMm != null && waypoint.tcpMm.Length >= 3
                ? $"TCP {waypoint.tcpMm[0]:0.#}/{waypoint.tcpMm[1]:0.#}/{waypoint.tcpMm[2]:0.#}"
                : "TCP -";
            var joint = waypoint.jointsDeg != null && waypoint.jointsDeg.Length > 0
                ? $"J1 {waypoint.jointsDeg[0]:0.#}"
                : "J -";
            return $"{ShortDisplayName(waypoint.name)} · {waypoint.moveType} · {speed} · dwell {dwell}s · {tcp} · {joint}";
        }

        private static string ShortDisplayName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "-";
            }

            var trimmed = value.Trim();
            return trimmed.Length <= 18
                ? trimmed
                : trimmed.Substring(0, 15) + "...";
        }

        private static Button CreatePointRowButton(string name, string text, System.Action clicked)
        {
            var button = new Button
            {
                name = name,
                text = text
            };
            button.RegisterCallback<ClickEvent>(_ => clicked?.Invoke());
            button.AddToClassList("rc-point-row-button");
            return button;
        }

        private void MovePointRow(string pointName)
        {
            OpenPointActionModal(pointName, PointModalRunMode);
        }

        private void PreviewPointRow(string pointName)
        {
            OpenPointActionModal(pointName, PointModalPreviewMode);
        }

        private void EditPointRow(string pointName)
        {
            OpenPointActionModal(pointName, PointModalEditMode);
        }

        private void AddPointRowToFunction(string pointName)
        {
            OpenPointActionModal(pointName, PointModalFunctionMode);
        }

        private void ApplyPointDetail(PanelElements panel)
        {
            if (panel?.DetailTitle == null)
            {
                return;
            }

            if (recalledPoint == null)
            {
                panel.DetailTitle.text = "선택된 포인트 없음";
                panel.DetailMeta.text = "포인트를 선택하면 이동 방식과 속도/대기 시간이 보인다.";
                panel.DetailJoints.text = "J: -";
                panel.DetailTcp.text = "TCP: -";
                return;
            }

            panel.DetailTitle.text = recalledPoint.name;
            panel.DetailMeta.text = $"{NormalizeMoveType(recalledPoint.moveType)} · {selectedSpeedPreset} · dwell {selectedDwellSec:0.0}s";
            panel.DetailJoints.text = $"J: {FormatVector(recalledPoint.jointsDeg, "0.0")}";
            panel.DetailTcp.text = $"TCP: {FormatTcp(recalledPoint.tcpMm)}";
            panel.BtnSpeedSlow.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "slow");
            panel.BtnSpeedMedium.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "medium");
            panel.BtnSpeedFast.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "fast");
            panel.DwellInput.SetValueWithoutNotify(selectedDwellSec.ToString("0.0", CultureInfo.InvariantCulture));
            panel.DwellInput.EnableInClassList("rc-point-dwell-input--danger", isDwellInvalid);
        }

        private void ApplyPointActionModal(PanelElements panel)
        {
            if (panel?.PointActionModal == null)
            {
                return;
            }

            var show = pointActionModalOpen && activeNavSection == "NavPoints" && recalledPoint != null;
            SetHidden(panel.PointActionModal, !show);
            if (!show)
            {
                return;
            }

            var isEditMode = pointActionModalMode == PointModalEditMode;
            panel.PointActionModalTitle.text = BuildPointModalTitle();
            panel.PointActionModalSummary.text = BuildPointModalSummary();
            panel.PointActionModalPose.text = $"J: {FormatVector(recalledPoint.jointsDeg, "0.0")} / TCP: {FormatTcp(recalledPoint.tcpMm)}";
            panel.PointActionModalNameInput?.SetValueWithoutNotify(recalledPoint.name ?? string.Empty);
            panel.PointActionModalDwellInput?.SetValueWithoutNotify(selectedDwellSec.ToString("0.0", CultureInfo.InvariantCulture));
            panel.PointActionModalNameInput?.SetEnabled(isEditMode);
            panel.PointActionModalDwellInput?.SetEnabled(isEditMode);
            panel.PointActionModalNameInput?.EnableInClassList("rc-point-modal-readonly", !isEditMode);
            panel.PointActionModalDwellInput?.EnableInClassList("rc-point-modal-readonly", !isEditMode);
            panel.BtnPointModalSpeedSlow?.SetEnabled(isEditMode);
            panel.BtnPointModalSpeedMedium?.SetEnabled(isEditMode);
            panel.BtnPointModalSpeedFast?.SetEnabled(isEditMode);
            panel.BtnPointModalSpeedSlow?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "slow");
            panel.BtnPointModalSpeedMedium?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "medium");
            panel.BtnPointModalSpeedFast?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "fast");
            panel.BtnPointModalPrimary.text = BuildPointModalPrimaryText();
            SetHidden(panel.BtnPointModalOverwrite, !isEditMode);
            SetHidden(panel.BtnPointModalDuplicate, !isEditMode);
            SetHidden(panel.BtnPointModalDelete, !isEditMode);
        }

        private string BuildPointModalTitle()
        {
            return pointActionModalMode switch
            {
                PointModalPreviewMode => "미리보기 확인",
                PointModalRunMode => "포인트 실행",
                PointModalEditMode => "포인트 편집",
                PointModalFunctionMode => "함수에 추가",
                _ => "포인트 작업",
            };
        }

        private string BuildPointModalSummary()
        {
            var name = recalledPoint?.name ?? "-";
            return pointActionModalMode switch
            {
                PointModalPreviewMode => $"{name} 위치를 ghost/path로 먼저 확인한다.",
                PointModalRunMode => $"{name} 위치로 이동한다. DryRun/Mock에서 먼저 움직임을 확인한다.",
                PointModalEditMode => $"{name} 이름, 속도, 대기 시간을 여기서 바로 수정한다.",
                PointModalFunctionMode => $"{name} 포인트를 함수 만들기 후보에 추가한다.",
                _ => $"{name} 포인트 작업을 선택한다.",
            };
        }

        private string BuildPointModalPrimaryText()
        {
            return pointActionModalMode switch
            {
                PointModalPreviewMode => "미리보기 실행",
                PointModalRunMode => "실행",
                PointModalEditMode => "저장",
                PointModalFunctionMode => "함수에 추가",
                _ => "확인",
            };
        }

        private string BuildPointActionModalDebugSummary()
        {
            return $"modalOpen={pointActionModalOpen}; mode={pointActionModalMode}; point={recalledPoint?.name ?? "none"}; speed={selectedSpeedPreset}; dwell={selectedDwellSec:0.0}; feedback={lastFeedback}";
        }

        private void OpenPointActionModal(string pointName, string mode)
        {
            RecallPoint(pointName);
            if (recalledPoint == null)
            {
                return;
            }

            pointActionModalMode = NormalizePointModalMode(mode);
            pointActionModalOpen = true;
            SetFeedback($"[{BuildPointModalTitle()}] {recalledPoint.name}");
            ApplyAll();
        }

        private void ClosePointActionModal()
        {
            pointActionModalOpen = false;
            pointActionModalMode = string.Empty;
            ApplyAll();
        }

        private static string NormalizePointModalMode(string mode)
        {
            return mode switch
            {
                PointModalRunMode => PointModalRunMode,
                PointModalEditMode => PointModalEditMode,
                PointModalFunctionMode => PointModalFunctionMode,
                _ => PointModalPreviewMode,
            };
        }

        private void ApplyPointActionModalPrimary()
        {
            if (!pointActionModalOpen || recalledPoint == null)
            {
                SetFeedback("작업할 포인트를 먼저 선택해라.");
                return;
            }

            switch (pointActionModalMode)
            {
                case PointModalRunMode:
                    ApplyMotionCandidate();
                    break;
                case PointModalEditMode:
                    ApplyPointModalEdits();
                    break;
                case PointModalFunctionMode:
                    AddSelectedPointToFunction();
                    break;
                default:
                    PreviewMotionCandidate();
                    break;
            }
        }

        private void ApplyPointModalEdits()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 포인트 편집을 잠근다. Stop 후 다시 수정해라.");
                return;
            }

            var panel = ResolveActivePanel();
            var nextName = panel?.PointActionModalNameInput?.value?.Trim() ?? string.Empty;
            var nextDwellRaw = panel?.PointActionModalDwellInput?.value ?? "0";
            if (string.IsNullOrWhiteSpace(nextName))
            {
                SetFeedback("포인트 이름을 먼저 넣어라.");
                return;
            }

            if (!double.TryParse(nextDwellRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out var nextDwell)
                || double.IsNaN(nextDwell)
                || double.IsInfinity(nextDwell)
                || nextDwell < 0.0
                || nextDwell > 600.0)
            {
                SetFeedback("대기 시간은 0~600초 사이 숫자로 넣어라.");
                return;
            }

            var originalName = recalledPoint.name;
            selectedDwellSec = nextDwell;
            isDwellInvalid = false;
            if (!string.Equals(originalName, nextName, System.StringComparison.OrdinalIgnoreCase))
            {
                if (HasNamedPoint(nextName))
                {
                    SetFeedback($"{nextName} 이름이 이미 있다.");
                    return;
                }

                RenamePoint(originalName, nextName);
            }

            ApplySelectedPointTiming();
            pointActionModalOpen = recalledPoint != null;
            pointActionModalMode = PointModalEditMode;
            ApplyAll();
        }

        private void SetPointModalSpeedPreset(string speedPreset)
        {
            if (pointActionModalMode != PointModalEditMode)
            {
                return;
            }

            selectedSpeedPreset = NormalizeSpeedPreset(speedPreset);
            ClearPendingConfirmation();
            ApplyAll();
        }

        private void ExecutePointModalEditAction(System.Action action)
        {
            if (pointActionModalMode != PointModalEditMode || recalledPoint == null)
            {
                SetFeedback("편집할 포인트를 먼저 선택해라.");
                return;
            }

            action?.Invoke();
            pointActionModalOpen = recalledPoint != null;
            pointActionModalMode = pointActionModalOpen ? PointModalEditMode : string.Empty;
            ApplyAll();
        }

        private string BuildPointDetailDebugSummary()
        {
            if (recalledPoint == null)
            {
                return "detail=none";
            }

            return $"detail={recalledPoint.name}; moveType={NormalizeMoveType(recalledPoint.moveType)}; speed={NormalizeSpeedPreset(recalledPoint.speedPreset)}; dwell={recalledPoint.dwellSec:0.0}; joints=[{FormatVector(recalledPoint.jointsDeg, "0.0")}]; tcp=[{FormatTcp(recalledPoint.tcpMm)}]";
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
            return LoadSequenceIfExists(PointSequenceName);
        }

        private static WaypointSequence LoadSequenceIfExists(string sequenceName)
        {
            if (string.IsNullOrWhiteSpace(sequenceName))
            {
                return null;
            }

            var names = WaypointStore.LoadAllNames();
            for (var index = 0; index < names.Length; index++)
            {
                if (string.Equals(names[index], sequenceName.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return WaypointStore.Load(names[index]);
                }
            }

            return null;
        }

        private static bool HasNamedSequence(string sequenceName)
        {
            return LoadSequenceIfExists(sequenceName) != null;
        }

        private static int CountSequenceWaypoints(string sequenceName)
        {
            return LoadSequenceIfExists(sequenceName)?.waypoints?.Length ?? 0;
        }

        private void SetPointName(string pointName)
        {
            var safeName = pointName?.Trim() ?? string.Empty;
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

        private void SetSelectedSpeedPreset(string speedPreset)
        {
            selectedSpeedPreset = NormalizeSpeedPreset(speedPreset);
            ClearPendingConfirmation();
            ApplyAll();
        }

        private bool IsSequenceEditLocked()
        {
            return debugSequenceEditLocked || (runtimeController != null && runtimeController.IsTeachingSequenceRunning);
        }

        private void SetPendingConfirmation(string kind, string pointName)
        {
            pendingConfirmKind = kind ?? string.Empty;
            pendingConfirmName = pointName?.Trim() ?? string.Empty;
        }

        private bool IsPendingConfirmation(string kind, string pointName)
        {
            return string.Equals(pendingConfirmKind, kind, System.StringComparison.Ordinal)
                && string.Equals(pendingConfirmName, pointName?.Trim() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
        }

        private void ClearPendingConfirmation()
        {
            pendingConfirmKind = string.Empty;
            pendingConfirmName = string.Empty;
        }

        private static void InsertWaypointAfter(WaypointSequence sequence, Waypoint waypoint, int sourceIndex)
        {
            var existing = sequence.waypoints ?? System.Array.Empty<Waypoint>();
            var insertIndex = Mathf.Clamp(sourceIndex + 1, 0, existing.Length);
            var expanded = new Waypoint[existing.Length + 1];
            if (insertIndex > 0)
            {
                System.Array.Copy(existing, 0, expanded, 0, insertIndex);
            }

            expanded[insertIndex] = CloneWaypoint(waypoint);
            if (insertIndex < existing.Length)
            {
                System.Array.Copy(existing, insertIndex, expanded, insertIndex + 1, existing.Length - insertIndex);
            }

            sequence.waypoints = expanded;
        }

        private static string BuildUniqueDuplicateName(WaypointSequence sequence, string sourceName)
        {
            var safeSource = string.IsNullOrWhiteSpace(sourceName) ? "Point" : sourceName.Trim();
            var baseName = $"{safeSource}_COPY";
            var candidate = baseName;
            var suffix = 2;
            while (FindWaypointIndex(sequence, candidate) >= 0)
            {
                candidate = $"{baseName}_{suffix}";
                suffix++;
            }

            return candidate;
        }

        private static string FormatVector(double[] values, string format)
        {
            if (values == null || values.Length == 0)
            {
                return "-";
            }

            var count = System.Math.Min(values.Length, 6);
            var parts = new string[count];
            for (var index = 0; index < count; index++)
            {
                parts[index] = values[index].ToString(format, CultureInfo.InvariantCulture);
            }

            return string.Join(" / ", parts);
        }

        private static string FormatTcp(double[] values)
        {
            if (values == null || values.Length < 6)
            {
                return "-";
            }

            return $"X {values[0]:0.0} / Y {values[1]:0.0} / Z {values[2]:0.0} / RX {values[3]:0.0} / RY {values[4]:0.0} / RZ {values[5]:0.0}";
        }

        private static string NormalizeMoveType(string value)
        {
            return string.Equals(value, "MoveL", System.StringComparison.OrdinalIgnoreCase) ? "MoveL" : "MoveJ";
        }

        private static string NormalizeSpeedPreset(string value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "slow" => "slow",
                "fast" => "fast",
                _ => "medium",
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
        private PanelElements ResolveActivePanel() => isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
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
