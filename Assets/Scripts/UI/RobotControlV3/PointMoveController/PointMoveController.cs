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
        private const string FilterAll = "All";
        private const string FilterSelected = "Selected";
        private const string FilterMoveJ = "MoveJ";
        private const string FilterMoveL = "MoveL";
        private const string FilterMissing = "Missing";
        private const string FilterDeletable = "Deletable";
        private const string FilterProtected = "Protected";

        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset pointMoveTemplate;

        private readonly float[] currentValues = new float[6];
        private readonly List<string> selectedFunctionPointNames = new();
        private readonly List<string> selectedPointNames = new();
        private readonly List<string> selectedSequenceNames = new();
        private readonly List<string> selectedFunctionNames = new();
        private readonly List<Waypoint> pointListItems = new();
        private readonly List<string> functionListItems = new();
        private readonly List<string> sequenceListItems = new();
        private readonly List<BlockSequenceListItem> blockSequenceListItems = new();
        private readonly List<string> bundlePickerListItems = new();

        private VisualElement root;
        private VisualElement workPanelBody;
        private VisualElement bottomSheetBody;
        private VisualElement pointMovePanelHost;
        private VisualElement pointMoveSheetHost;
        private ConnectionHomeController connectionHomeController;
        private RobotControlV3RuntimeController runtimeController;
        private PopupCoordinatorV3 popupCoordinator;
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
        private string bundlePickerSelectedName = string.Empty;
        private string pointSearchText = string.Empty;
        private string pointFilter = FilterAll;
        private string functionSearchText = string.Empty;
        private string functionFilter = FilterAll;
        private string sequenceSearchText = string.Empty;
        private string sequenceFilter = FilterAll;
        private string pointActionModalMode = string.Empty;
        private bool pointActionModalOpen;
        private bool bundlePickerModalOpen;
        private bool pointRowActionsCollapsed;
        private bool sequenceRowActionsCollapsed;
        private bool functionRowActionsCollapsed;
        private bool debugSequenceEditLocked;
        private bool isDesktopVisible;
        private bool isTabletVisible;
        private bool isInitialized;
        private bool isInitializing;
        private Coroutine initializeCoroutine;

        private readonly struct BlockSequenceListItem
        {
            public BlockSequenceListItem(TeachingSequenceBlock block, int index)
            {
                Block = block;
                Index = index;
            }

            public TeachingSequenceBlock Block { get; }
            public int Index { get; }
        }

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
            isInitializing = false;
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

        public string SetTeachingSubviewForDebug(string subviewName)
        {
            SetTeachingSubview(subviewName);
            return GetDebugSummary();
        }

        public void CollectButtonsForDebug(string buttonName, List<Button> buttons)
        {
            if (buttons == null)
            {
                return;
            }

            ForceInitialize();
            root?.Query<Button>(name: buttonName).ForEach(button => buttons.Add(button));
        }

        public void CollectButtonsForDebug(List<Button> buttons)
        {
            if (buttons == null)
            {
                return;
            }

            ForceInitialize();
            root?.Query<Button>().ForEach(button => buttons.Add(button));
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

        public string ToggleSequenceSelectionForDebug(string sequenceName)
        {
            ToggleSequenceSelection(sequenceName);
            return GetSequenceLibrarySummaryForDebug();
        }

        public string ToggleSequenceActionsForDebug()
        {
            ToggleSequenceRowActionsCollapsed();
            return GetDebugSummary();
        }

        public string DeleteSelectedSequencesForDebug()
        {
            DeleteSelectedSequences();
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

        public string ClearSelectedPointsForDebug()
        {
            selectedPointNames.Clear();
            ClearPendingConfirmation();
            ApplyAll();
            return GetPointListSummaryForDebug();
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

        public string ToggleFunctionSelectionForDebug(string functionName)
        {
            ToggleFunctionSelection(functionName);
            return GetFunctionCompactDebugSummary();
        }

        public string ToggleFunctionActionsForDebug()
        {
            ToggleFunctionRowActionsCollapsed();
            return GetDebugSummary();
        }

        public string DuplicateSelectedFunctionsForDebug()
        {
            DuplicateSelectedFunctions();
            return GetFunctionCompactDebugSummary();
        }

        public string DeleteSelectedFunctionsForDebug()
        {
            DeleteSelectedFunctions();
            return GetFunctionCompactDebugSummary();
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

        public string GetFunctionCompactDebugSummary()
        {
            var names = runtimeController != null
                ? runtimeController.GetTeachingFunctionNames()
                : System.Array.Empty<string>();
            var detail = !string.IsNullOrWhiteSpace(selectedFunctionName) && runtimeController != null
                ? runtimeController.GetTeachingFunctionDetailForDebug(selectedFunctionName)
                : "function=none";
            var steps = ExtractDebugValue(detail, "steps=");
            var missing = ExtractDebugValue(detail, "missingCount=");
            return $"selectedFunction={selectedFunctionName}; functions={names.Length}; visibleFunctions={functionListItems.Count}; functionFilter={functionFilter}; selectedFunctions={selectedFunctionNames.Count}; candidates={selectedFunctionPointNames.Count}; steps={steps}; missingCount={missing}; feedback={lastFeedback}";
        }

        public string GetFunctionSourceDebugSummary()
        {
            var sourcePointNames = ResolveFunctionSourcePointNames();
            return $"candidates={selectedFunctionPointNames.Count}; selectedPoints={selectedPointNames.Count}; recalled={recalledPoint?.name ?? "none"}; sourceLabel={ResolveFunctionSourceLabel(sourcePointNames)}; sourceCount={sourcePointNames.Length}; source=[{string.Join(",", sourcePointNames)}]";
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
            PreviewEditedPointCandidate();
            return GetDebugSummary();
        }

        public string GetDebugSummary()
        {
            var pointName = desktopPanel?.PointNameInput?.value ?? tabletPanel?.PointNameInput?.value ?? "Point";
            var runtimeRobot = motionRuntime?.RobotId ?? "none";
            var canPreviewAction = CanPreview() && IsAnyPanelVisible();
            var canApplyAction = CanApply() && IsAnyPanelVisible();
            var panel = desktopPanel ?? tabletPanel;
            return $"initialized={isInitialized}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}; surface={ResolveSurfaceDebugName()}; subview={activeTeachingSubview}; pointModalOpen={pointActionModalOpen}; pointModalMode={pointActionModalMode}; bundlePickerOpen={bundlePickerModalOpen}; bundlePickerSelected={bundlePickerSelectedName}; selectedPoints={selectedPointNames.Count}; selectedSequences={selectedSequenceNames.Count}; selectedFunctions={selectedFunctionNames.Count}; visiblePoints={pointListItems.Count}; visibleFunctions={functionListItems.Count}; visibleSequences={sequenceListItems.Count}; pointFilter={pointFilter}; functionFilter={functionFilter}; sequenceFilter={sequenceFilter}; rowActionsCollapsed={pointRowActionsCollapsed}; sequenceActionsCollapsed={sequenceRowActionsCollapsed}; functionActionsCollapsed={functionRowActionsCollapsed}; tabsHidden={IsHidden(panel?.SubviewTabs)}; motionRowHidden={IsHidden(panel?.MotionRow)}; coordGridHidden={IsHidden(panel?.CoordGrid)}; listHidden={IsHidden(panel?.PointListContainer)}; coord={activeCoordSystem}; motion={motionKind}; speed={selectedSpeedPreset}; dwell={selectedDwellSec:0.0}; editLocked={IsSequenceEditLocked()}; pendingConfirm={pendingConfirmKind}:{pendingConfirmName}; previewState={connectionHomeController.CurrentPreviewState}; canPreview={canPreviewAction}; canApply={canApplyAction}; runtimeRobot={runtimeRobot}; name={pointName}; x={currentValues[0]:0.0}; rz={currentValues[5]:0.0}; feedback={lastFeedback}";
        }

    }
}
