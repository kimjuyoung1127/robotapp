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
                BtnPointSubview = root.Q<Button>("BtnPointSubview");
                BtnSequenceSubview = root.Q<Button>("BtnSequenceSubview");
                BtnFunctionSubview = root.Q<Button>("BtnFunctionSubview");
                PointSubview = root.Q<VisualElement>("PointSubview");
                SequenceSubview = root.Q<VisualElement>("SequenceSubview");
                FunctionSubview = root.Q<VisualElement>("FunctionSubview");
                PointEditSubview = root.Q<VisualElement>("PointEditSubview");
                PointEditActions = root.Q<VisualElement>("PointEditActions");
                PointNameInput = root.Q<TextField>("PointNameInput");
                BtnPrimarySave = root.Q<Button>("BtnPointSave") ?? root.Q<Button>("BtnPointPrimarySave");
                BtnCoordBase = root.Q<Button>("BtnPointCoordBase");
                BtnCoordTool = root.Q<Button>("BtnPointCoordTool");
                BtnCoordUser = root.Q<Button>("BtnPointCoordUser");
                BtnMoveJ = root.Q<Button>("BtnPointMoveJ");
                BtnMoveL = root.Q<Button>("BtnPointMoveL");
                CoordSummary = root.Q<Label>("PointCoordSummary");
                MotionSummary = root.Q<Label>("PointMotionSummary");
                PreviewSummary = root.Q<Label>("PointPreviewSummary");
                StoreSummary = root.Q<Label>("PointStoreSummary");
                BtnLoop = root.Q<Button>("BtnPointLoop");
                BtnRunSequence = root.Q<Button>("BtnPointRunSequence");
                BtnStepBack = root.Q<Button>("BtnPointStepBack");
                BtnStepForward = root.Q<Button>("BtnPointStepForward");
                BtnStopSequence = root.Q<Button>("BtnPointStopSequence");
                LoopStatus = root.Q<Label>("PointLoopStatus");
                PointListContainer = root.Q<VisualElement>("PointListContainer");
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
                FunctionNameInput = root.Q<TextField>("FunctionNameInput");
                FunctionBuildSummary = root.Q<Label>("FunctionBuildSummary");
                FunctionSummary = root.Q<Label>("FunctionSummary");
                FunctionListContainer = root.Q<VisualElement>("FunctionListContainer");
                FunctionSelectionSummary = root.Q<Label>("FunctionSelectionSummary");
                FunctionDetail = root.Q<Label>("FunctionDetail");
                BtnFunctionAddPoint = root.Q<Button>("BtnFunctionAddPoint");
                BtnFunctionClearSelection = root.Q<Button>("BtnFunctionClearSelection");
                BtnFunctionCreate = root.Q<Button>("BtnFunctionCreate");
                BtnFunctionRun = root.Q<Button>("BtnFunctionRun");
                BtnFunctionRunFromSelected = root.Q<Button>("BtnFunctionRunFromSelected");
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
            public Button BtnPointSubview { get; }
            public Button BtnSequenceSubview { get; }
            public Button BtnFunctionSubview { get; }
            public VisualElement PointSubview { get; }
            public VisualElement SequenceSubview { get; }
            public VisualElement FunctionSubview { get; }
            public VisualElement PointEditSubview { get; }
            public VisualElement PointEditActions { get; }
            public TextField PointNameInput { get; }
            public Button BtnPrimarySave { get; }
            public Button BtnCoordBase { get; }
            public Button BtnCoordTool { get; }
            public Button BtnCoordUser { get; }
            public Button BtnMoveJ { get; }
            public Button BtnMoveL { get; }
            public Label CoordSummary { get; }
            public Label MotionSummary { get; }
            public Label PreviewSummary { get; }
            public Label StoreSummary { get; }
            public Button BtnLoop { get; }
            public Button BtnRunSequence { get; }
            public Button BtnStepBack { get; }
            public Button BtnStepForward { get; }
            public Button BtnStopSequence { get; }
            public Label LoopStatus { get; }
            public VisualElement PointListContainer { get; }
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
            public TextField FunctionNameInput { get; }
            public Label FunctionBuildSummary { get; }
            public Label FunctionSummary { get; }
            public VisualElement FunctionListContainer { get; }
            public Label FunctionSelectionSummary { get; }
            public Label FunctionDetail { get; }
            public Button BtnFunctionAddPoint { get; }
            public Button BtnFunctionClearSelection { get; }
            public Button BtnFunctionCreate { get; }
            public Button BtnFunctionRun { get; }
            public Button BtnFunctionRunFromSelected { get; }
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
