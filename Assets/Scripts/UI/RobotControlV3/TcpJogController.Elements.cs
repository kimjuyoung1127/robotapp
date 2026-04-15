// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class TcpJogController
    {
        private static readonly AxisSpec[] AxisSpecs =
        {
            new("X", -1500f, 1500f),
            new("Y", -1500f, 1500f),
            new("Z", -1500f, 1500f),
            new("RX", -360f, 360f),
            new("RY", -360f, 360f),
            new("RZ", -360f, 360f),
        };

        private readonly struct AxisSpec
        {
            public AxisSpec(string label, float minValue, float maxValue)
            {
                Label = label;
                MinValue = minValue;
                MaxValue = maxValue;
            }

            public string Label { get; }
            public float MinValue { get; }
            public float MaxValue { get; }
        }

        private sealed class PanelElements
        {
            public PanelElements(VisualElement root)
            {
                BtnCoordBase = root.Q<Button>("BtnTcpCoordBase");
                BtnCoordTool = root.Q<Button>("BtnTcpCoordTool");
                BtnCoordUser = root.Q<Button>("BtnTcpCoordUser");
                Hint = root.Q<Label>("TcpJogHint");
                IncrementSummary = root.Q<Label>("TcpIncrementSummary");
                SpeedSummary = root.Q<Label>("TcpSpeedSummary");
                OverlaySummary = root.Q<Label>("TcpOverlaySummary");
                BtnPreview = root.Q<Button>("BtnTcpPreview");
                BtnApply = root.Q<Button>("BtnTcpApply");
                Rows = new[]
                {
                    new TcpRowElements(root, 1),
                    new TcpRowElements(root, 2),
                    new TcpRowElements(root, 3),
                    new TcpRowElements(root, 4),
                    new TcpRowElements(root, 5),
                    new TcpRowElements(root, 6),
                };
            }

            public Button BtnCoordBase { get; }
            public Button BtnCoordTool { get; }
            public Button BtnCoordUser { get; }
            public Label Hint { get; }
            public Label IncrementSummary { get; }
            public Label SpeedSummary { get; }
            public Label OverlaySummary { get; }
            public Button BtnPreview { get; }
            public Button BtnApply { get; }
            public TcpRowElements[] Rows { get; }
        }

        private sealed class TcpRowElements
        {
            public TcpRowElements(VisualElement root, int axisNumber)
            {
                Root = root.Q<VisualElement>($"TcpRow{axisNumber}");
                MinusButton = root.Q<Button>($"BtnTcp{axisNumber}Minus");
                PlusButton = root.Q<Button>($"BtnTcp{axisNumber}Plus");
                Value = root.Q<Label>($"TcpValue{axisNumber}");
            }

            public VisualElement Root { get; }
            public Button MinusButton { get; }
            public Button PlusButton { get; }
            public Label Value { get; }
        }

        private sealed class OverlayElements
        {
            public OverlayElements(VisualElement root)
            {
                CoordBadge = root.Q<Label>("CartesianCoordBadge");
                Hint = root.Q<Label>("CartesianOverlayHint");
                Summary = root.Q<Label>("CartesianOverlaySummary");
                Axes = new[]
                {
                    new OverlayAxisElements(root, 1),
                    new OverlayAxisElements(root, 2),
                    new OverlayAxisElements(root, 3),
                    new OverlayAxisElements(root, 4),
                    new OverlayAxisElements(root, 5),
                    new OverlayAxisElements(root, 6),
                };
            }

            public Label CoordBadge { get; }
            public Label Hint { get; }
            public Label Summary { get; }
            public OverlayAxisElements[] Axes { get; }
        }

        private sealed class OverlayAxisElements
        {
            public OverlayAxisElements(VisualElement root, int axisNumber)
            {
                Root = root.Q<VisualElement>($"CartesianAxis{axisNumber}");
                Value = root.Q<Label>($"CartesianAxisValue{axisNumber}");
                MinusButton = root.Q<Button>($"BtnArrow{axisNumber}Minus");
                PlusButton = root.Q<Button>($"BtnArrow{axisNumber}Plus");
            }

            public VisualElement Root { get; }
            public Label Value { get; }
            public Button MinusButton { get; }
            public Button PlusButton { get; }
        }
    }
}
