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
                Hint = root.Q<Label>("PointMoveHint");
                PointNameInput = root.Q<TextField>("PointNameInput");
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
                BtnRestore = root.Q<Button>("BtnPointRestore");
                BtnSave = root.Q<Button>("BtnPointSave");
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

            public Label Hint { get; }
            public TextField PointNameInput { get; }
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
            public Button BtnApply { get; }
            public Label[] AxisLabels { get; }
            public Label[] AxisUnits { get; }
            public TextField[] ValueInputs { get; }
        }
    }
}
