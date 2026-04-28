// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
        private sealed class PanelElements
        {
            public PanelElements(VisualElement root)
            {
                Title = root.Q<Label>("PointPanelTitle");
                Hint = root.Q<Label>("PointMoveHint");
                MotionRow = root.Q<VisualElement>(className: "rc-point-motion-row");
                SubviewTabs = root.Q<VisualElement>(className: "rc-point-subview-tabs");
                BtnPointSubview = root.Q<Button>("BtnPointSubview");
                BtnSequenceSubview = root.Q<Button>("BtnSequenceSubview");
                BtnFunctionSubview = root.Q<Button>("BtnFunctionSubview");
                PointSubview = root.Q<VisualElement>("PointSubview");
                SequenceSubview = root.Q<VisualElement>("SequenceSubview");
                FunctionSubview = root.Q<VisualElement>("FunctionSubview");
                PointEditSubview = root.Q<VisualElement>("PointEditSubview");
                PointEditActions = root.Q<VisualElement>("PointEditActions");
                DetailCard = root.Q<VisualElement>("PointDetailCard");
                NameRow = root.Q<VisualElement>(className: "rc-point-name-row");
                PointNameInput = root.Q<TextField>("PointNameInput");
                BtnPrimarySave = root.Q<Button>("BtnPointSave") ?? root.Q<Button>("BtnPointPrimarySave");
                CoordRow = root.Q<VisualElement>(className: "rc-point-coord-row");
                CoordGrid = root.Q<VisualElement>(className: "rc-point-grid");
                BtnCoordBase = root.Q<Button>("BtnPointCoordBase");
                BtnCoordTool = root.Q<Button>("BtnPointCoordTool");
                BtnCoordUser = root.Q<Button>("BtnPointCoordUser");
                BtnMoveJ = root.Q<Button>("BtnPointMoveJ");
                BtnMoveL = root.Q<Button>("BtnPointMoveL");
                CoordSummary = root.Q<Label>("PointCoordSummary");
                MotionSummary = root.Q<Label>("PointMotionSummary");
                PreviewSummary = root.Q<Label>("PointPreviewSummary");
                StoreSummary = root.Q<Label>("PointStoreSummary");
                PointInventorySummary = root.Q<Label>("PointInventorySummary");
                PointSearchInput = root.Q<TextField>("PointSearchInput");
                PointSelectionCount = root.Q<Label>("PointSelectionCount");
                BtnPointFilterAll = root.Q<Button>("BtnPointFilterAll");
                BtnPointFilterSelected = root.Q<Button>("BtnPointFilterSelected");
                BtnPointFilterMoveJ = root.Q<Button>("BtnPointFilterMoveJ");
                BtnPointFilterMoveL = root.Q<Button>("BtnPointFilterMoveL");
                PointBulkActions = root.Q<VisualElement>("PointBulkActions");
                BtnPointRowActionsToggle = root.Q<Button>("BtnPointRowActionsToggle");
                BtnPointBulkClear = root.Q<Button>("BtnPointBulkClear");
                BtnPointBulkSpeed = root.Q<Button>("BtnPointBulkSpeed");
                BtnPointBulkFunction = root.Q<Button>("BtnPointBulkFunction");
                BtnPointBulkDelete = root.Q<Button>("BtnPointBulkDelete");
                PointFunctionBuildSummary = root.Q<Label>("PointFunctionBuildSummary");
                PointFunctionNameInput = root.Q<TextField>("PointFunctionNameInput");
                PointFunctionSelectionSummary = root.Q<Label>("PointFunctionSelectionSummary");
                BtnPointFunctionClearSelection = root.Q<Button>("BtnPointFunctionClearSelection");
                BtnPointFunctionCreate = root.Q<Button>("BtnPointFunctionCreate");
                BtnLoop = root.Q<Button>("BtnPointLoop");
                BtnRunSequence = root.Q<Button>("BtnPointRunSequence");
                BtnStepBack = root.Q<Button>("BtnPointStepBack");
                BtnStepForward = root.Q<Button>("BtnPointStepForward");
                BtnStopSequence = root.Q<Button>("BtnPointStopSequence");
                LoopStatus = root.Q<Label>("PointLoopStatus");
                BtnPathRecordStart = root.Q<Button>("BtnPathRecordStart");
                BtnPathRecordStop = root.Q<Button>("BtnPathRecordStop");
                BtnPathReplayOnce = root.Q<Button>("BtnPathReplayOnce");
                BtnPathReplayLoop = root.Q<Button>("BtnPathReplayLoop");
                BtnPathRecordDelete = root.Q<Button>("BtnPathRecordDelete");
                PathRecordSummary = root.Q<Label>("PathRecordSummary");
                BlockSequenceSummary = root.Q<Label>("BlockSequenceSummary");
                BlockSequenceListView = root.Q<ListView>("BlockSequenceListContainer");
                BlockSequenceListContainer = BlockSequenceListView ?? root.Q<VisualElement>("BlockSequenceListContainer");
                BtnBlockAddPoint = root.Q<Button>("BtnBlockAddPoint");
                BtnBlockAddBundle = root.Q<Button>("BtnBlockAddBundle");
                BtnBlockPreview = root.Q<Button>("BtnBlockPreview");
                BtnBlockRun = root.Q<Button>("BtnBlockRun");
                SequenceLibrarySummary = root.Q<Label>("SequenceLibrarySummary");
                SelectedSequenceDetail = root.Q<Label>("SelectedSequenceDetail");
                SequenceInventorySummary = root.Q<Label>("SequenceInventorySummary");
                SequenceSearchInput = root.Q<TextField>("SequenceSearchInput");
                SequenceSelectionCount = root.Q<Label>("SequenceSelectionCount");
                BtnSequenceFilterAll = root.Q<Button>("BtnSequenceFilterAll");
                BtnSequenceFilterSelected = root.Q<Button>("BtnSequenceFilterSelected");
                BtnSequenceFilterDeletable = root.Q<Button>("BtnSequenceFilterDeletable");
                BtnSequenceFilterProtected = root.Q<Button>("BtnSequenceFilterProtected");
                SequenceBulkActions = root.Q<VisualElement>("SequenceBulkActions");
                BtnSequenceRowActionsToggle = root.Q<Button>("BtnSequenceRowActionsToggle");
                BtnSequenceBulkClear = root.Q<Button>("BtnSequenceBulkClear");
                BtnSequenceBulkDelete = root.Q<Button>("BtnSequenceBulkDelete");
                SequenceListView = root.Q<ListView>("SequenceListContainer");
                SequenceListContainer = SequenceListView ?? root.Q<VisualElement>("SequenceListContainer");
                PointListView = root.Q<ListView>("PointListContainer");
                PointListContainer = PointListView ?? root.Q<VisualElement>("PointListContainer");
                DetailTitle = root.Q<Label>("PointDetailTitle");
                DetailMeta = root.Q<Label>("PointDetailMeta");
                DetailJoints = root.Q<Label>("PointDetailJoints");
                DetailTcp = root.Q<Label>("PointDetailTcp");
                BtnSpeedSlow = root.Q<Button>("BtnPointSpeedSlow");
                BtnSpeedMedium = root.Q<Button>("BtnPointSpeedMedium");
                BtnSpeedFast = root.Q<Button>("BtnPointSpeedFast");
                DwellInput = root.Q<TextField>("PointDwellInput");
                BtnTimingApply = root.Q<Button>("BtnPointTimingApply");
                FeedbackSummary = root.Q<Label>("PointFeedbackSummary");
                PointActionModal = root.Q<VisualElement>("PointActionModal");
                PointActionModalTitle = root.Q<Label>("PointActionModalTitle");
                PointActionModalSummary = root.Q<Label>("PointActionModalSummary");
                PointActionModalPose = root.Q<Label>("PointActionModalPose");
                PointActionModalNameInput = root.Q<TextField>("PointActionModalNameInput");
                PointActionModalDwellInput = root.Q<TextField>("PointActionModalDwellInput");
                BtnPointModalSpeedSlow = root.Q<Button>("BtnPointModalSpeedSlow");
                BtnPointModalSpeedMedium = root.Q<Button>("BtnPointModalSpeedMedium");
                BtnPointModalSpeedFast = root.Q<Button>("BtnPointModalSpeedFast");
                BtnPointModalPrimary = root.Q<Button>("BtnPointModalPrimary");
                BtnPointModalOverwrite = root.Q<Button>("BtnPointModalOverwrite");
                BtnPointModalDuplicate = root.Q<Button>("BtnPointModalDuplicate");
                BtnPointModalDelete = root.Q<Button>("BtnPointModalDelete");
                BtnPointModalClose = root.Q<Button>("BtnPointModalClose");
                BundlePickerModal = root.Q<VisualElement>("BundlePickerModal");
                BundlePickerSummary = root.Q<Label>("BundlePickerSummary");
                BundlePickerListView = root.Q<ListView>("BundlePickerListContainer");
                BundlePickerListContainer = BundlePickerListView ?? root.Q<VisualElement>("BundlePickerListContainer");
                BtnBundlePickerConfirm = root.Q<Button>("BtnBundlePickerConfirm");
                BtnBundlePickerClose = root.Q<Button>("BtnBundlePickerClose");
                FunctionNameInput = root.Q<TextField>("FunctionNameInput");
                FunctionSummary = root.Q<Label>("FunctionSummary");
                FunctionInventorySummary = root.Q<Label>("FunctionInventorySummary");
                FunctionSearchInput = root.Q<TextField>("FunctionSearchInput");
                FunctionSelectionCount = root.Q<Label>("FunctionSelectionCount");
                BtnFunctionFilterAll = root.Q<Button>("BtnFunctionFilterAll");
                BtnFunctionFilterSelected = root.Q<Button>("BtnFunctionFilterSelected");
                BtnFunctionFilterMissing = root.Q<Button>("BtnFunctionFilterMissing");
                FunctionBulkActions = root.Q<VisualElement>("FunctionBulkActions");
                FunctionListView = root.Q<ListView>("FunctionListContainer");
                FunctionListContainer = FunctionListView ?? root.Q<VisualElement>("FunctionListContainer");
                FunctionDetail = root.Q<Label>("FunctionDetail");
                BtnFunctionRowActionsToggle = root.Q<Button>("BtnFunctionRowActionsToggle");
                BtnFunctionBulkClear = root.Q<Button>("BtnFunctionBulkClear");
                BtnFunctionBulkDuplicate = root.Q<Button>("BtnFunctionBulkDuplicate");
                BtnFunctionBulkDelete = root.Q<Button>("BtnFunctionBulkDelete");
                BtnFunctionDeleteAll = root.Q<Button>("BtnFunctionDeleteAll");
                BtnFunctionRename = root.Q<Button>("BtnFunctionRename");
                BtnFunctionDuplicate = root.Q<Button>("BtnFunctionDuplicate");
                BtnFunctionDelete = root.Q<Button>("BtnFunctionDelete");
                BtnRestore = root.Q<Button>("BtnPointRestore");
                BtnSave = BtnPrimarySave;
                BtnRecall = root.Q<Button>("BtnPointRecall");
                BtnDelete = root.Q<Button>("BtnPointDelete");
                BtnRename = root.Q<Button>("BtnPointRename");
                BtnDuplicate = root.Q<Button>("BtnPointDuplicate");
                BtnUp = root.Q<Button>("BtnPointUp");
                BtnDown = root.Q<Button>("BtnPointDown");
                BtnOverwrite = root.Q<Button>("BtnPointOverwrite");
                BtnExport = root.Q<Button>("BtnPointExport");
                BtnCleanup = root.Q<Button>("BtnPointCleanup");
                BtnPreview = root.Q<Button>("BtnPointPreview");
                BtnRunFromSelected = root.Q<Button>("BtnPointRunFromSelected");
                BtnApply = root.Q<Button>("BtnPointApply");
                AxisLabels = new[]
                {
                    root.Q<Label>("PointLabel1"),
                    root.Q<Label>("PointLabel2"),
                    root.Q<Label>("PointLabel3"),
                    root.Q<Label>("PointLabel4"),
                    root.Q<Label>("PointLabel5"),
                    root.Q<Label>("PointLabel6"),
                };
                AxisUnits = new[]
                {
                    root.Q<Label>("PointUnit1"),
                    root.Q<Label>("PointUnit2"),
                    root.Q<Label>("PointUnit3"),
                    root.Q<Label>("PointUnit4"),
                    root.Q<Label>("PointUnit5"),
                    root.Q<Label>("PointUnit6"),
                };
                ValueInputs = new[]
                {
                    root.Q<TextField>("PointValueX"),
                    root.Q<TextField>("PointValueY"),
                    root.Q<TextField>("PointValueZ"),
                    root.Q<TextField>("PointValueRx"),
                    root.Q<TextField>("PointValueRy"),
                    root.Q<TextField>("PointValueRz"),
                };
            }

            public Label Title { get; }
            public Label Hint { get; }
            public VisualElement MotionRow { get; }
            public VisualElement SubviewTabs { get; }
            public Button BtnPointSubview { get; }
            public Button BtnSequenceSubview { get; }
            public Button BtnFunctionSubview { get; }
            public VisualElement PointSubview { get; }
            public VisualElement SequenceSubview { get; }
            public VisualElement FunctionSubview { get; }
            public VisualElement PointEditSubview { get; }
            public VisualElement PointEditActions { get; }
            public VisualElement DetailCard { get; }
            public VisualElement NameRow { get; }
            public TextField PointNameInput { get; }
            public Button BtnPrimarySave { get; }
            public VisualElement CoordRow { get; }
            public VisualElement CoordGrid { get; }
            public Button BtnCoordBase { get; }
            public Button BtnCoordTool { get; }
            public Button BtnCoordUser { get; }
            public Button BtnMoveJ { get; }
            public Button BtnMoveL { get; }
            public Label CoordSummary { get; }
            public Label MotionSummary { get; }
            public Label PreviewSummary { get; }
            public Label StoreSummary { get; }
            public Label PointInventorySummary { get; }
            public TextField PointSearchInput { get; }
            public Label PointSelectionCount { get; }
            public Button BtnPointFilterAll { get; }
            public Button BtnPointFilterSelected { get; }
            public Button BtnPointFilterMoveJ { get; }
            public Button BtnPointFilterMoveL { get; }
            public VisualElement PointBulkActions { get; }
            public Button BtnPointRowActionsToggle { get; }
            public Button BtnPointBulkClear { get; }
            public Button BtnPointBulkSpeed { get; }
            public Button BtnPointBulkFunction { get; }
            public Button BtnPointBulkDelete { get; }
            public Label PointFunctionBuildSummary { get; }
            public TextField PointFunctionNameInput { get; }
            public Label PointFunctionSelectionSummary { get; }
            public Button BtnPointFunctionClearSelection { get; }
            public Button BtnPointFunctionCreate { get; }
            public Button BtnLoop { get; }
            public Button BtnRunSequence { get; }
            public Button BtnStepBack { get; }
            public Button BtnStepForward { get; }
            public Button BtnStopSequence { get; }
            public Label LoopStatus { get; }
            public Button BtnPathRecordStart { get; }
            public Button BtnPathRecordStop { get; }
            public Button BtnPathReplayOnce { get; }
            public Button BtnPathReplayLoop { get; }
            public Button BtnPathRecordDelete { get; }
            public Label PathRecordSummary { get; }
            public Label BlockSequenceSummary { get; }
            public VisualElement BlockSequenceListContainer { get; }
            public ListView BlockSequenceListView { get; }
            public Button BtnBlockAddPoint { get; }
            public Button BtnBlockAddBundle { get; }
            public Button BtnBlockPreview { get; }
            public Button BtnBlockRun { get; }
            public Label SequenceLibrarySummary { get; }
            public Label SelectedSequenceDetail { get; }
            public Label SequenceInventorySummary { get; }
            public TextField SequenceSearchInput { get; }
            public Label SequenceSelectionCount { get; }
            public Button BtnSequenceFilterAll { get; }
            public Button BtnSequenceFilterSelected { get; }
            public Button BtnSequenceFilterDeletable { get; }
            public Button BtnSequenceFilterProtected { get; }
            public VisualElement SequenceBulkActions { get; }
            public Button BtnSequenceRowActionsToggle { get; }
            public Button BtnSequenceBulkClear { get; }
            public Button BtnSequenceBulkDelete { get; }
            public VisualElement SequenceListContainer { get; }
            public ListView SequenceListView { get; }
            public VisualElement PointListContainer { get; }
            public ListView PointListView { get; }
            public Label DetailTitle { get; }
            public Label DetailMeta { get; }
            public Label DetailJoints { get; }
            public Label DetailTcp { get; }
            public Button BtnSpeedSlow { get; }
            public Button BtnSpeedMedium { get; }
            public Button BtnSpeedFast { get; }
            public TextField DwellInput { get; }
            public Button BtnTimingApply { get; }
            public Label FeedbackSummary { get; }
            public VisualElement PointActionModal { get; }
            public Label PointActionModalTitle { get; }
            public Label PointActionModalSummary { get; }
            public Label PointActionModalPose { get; }
            public TextField PointActionModalNameInput { get; }
            public TextField PointActionModalDwellInput { get; }
            public Button BtnPointModalSpeedSlow { get; }
            public Button BtnPointModalSpeedMedium { get; }
            public Button BtnPointModalSpeedFast { get; }
            public Button BtnPointModalPrimary { get; }
            public Button BtnPointModalOverwrite { get; }
            public Button BtnPointModalDuplicate { get; }
            public Button BtnPointModalDelete { get; }
            public Button BtnPointModalClose { get; }
            public VisualElement BundlePickerModal { get; }
            public Label BundlePickerSummary { get; }
            public VisualElement BundlePickerListContainer { get; }
            public ListView BundlePickerListView { get; }
            public Button BtnBundlePickerConfirm { get; }
            public Button BtnBundlePickerClose { get; }
            public TextField FunctionNameInput { get; }
            public Label FunctionSummary { get; }
            public Label FunctionInventorySummary { get; }
            public TextField FunctionSearchInput { get; }
            public Label FunctionSelectionCount { get; }
            public Button BtnFunctionFilterAll { get; }
            public Button BtnFunctionFilterSelected { get; }
            public Button BtnFunctionFilterMissing { get; }
            public VisualElement FunctionBulkActions { get; }
            public VisualElement FunctionListContainer { get; }
            public ListView FunctionListView { get; }
            public Label FunctionDetail { get; }
            public Button BtnFunctionRowActionsToggle { get; }
            public Button BtnFunctionBulkClear { get; }
            public Button BtnFunctionBulkDuplicate { get; }
            public Button BtnFunctionBulkDelete { get; }
            public Button BtnFunctionDeleteAll { get; }
            public Button BtnFunctionRename { get; }
            public Button BtnFunctionDuplicate { get; }
            public Button BtnFunctionDelete { get; }
            public Button BtnRestore { get; }
            public Button BtnSave { get; }
            public Button BtnRecall { get; }
            public Button BtnDelete { get; }
            public Button BtnRename { get; }
            public Button BtnDuplicate { get; }
            public Button BtnUp { get; }
            public Button BtnDown { get; }
            public Button BtnOverwrite { get; }
            public Button BtnExport { get; }
            public Button BtnCleanup { get; }
            public Button BtnPreview { get; }
            public Button BtnRunFromSelected { get; }
            public Button BtnApply { get; }
            public Label[] AxisLabels { get; }
            public Label[] AxisUnits { get; }
            public TextField[] ValueInputs { get; }
        }
    }
}
