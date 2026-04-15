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
                FeedbackSummary = root.Q<Label>("PointFeedbackSummary");
                BtnRestore = root.Q<Button>("BtnPointRestore");
                BtnPreview = root.Q<Button>("BtnPointPreview");
                BtnApply = root.Q<Button>("BtnPointApply");
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
            public Label FeedbackSummary { get; }
            public Button BtnRestore { get; }
            public Button BtnPreview { get; }
            public Button BtnApply { get; }
            public TextField[] ValueInputs { get; }
        }
    }
}
