// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// Pendant V3가 소비하는 I/O와 그리퍼 주변장치 상태입니다.
    /// </summary>
    internal sealed class RobotControlPeripheralState
    {
        public bool GripperOpen { get; set; }
        public float GripperOpenRatio { get; set; }
        public bool GripperVisualAttached { get; set; }
        public bool[] RobotDigitalOutputs { get; } = new bool[2];
        public bool[] ToolDigitalOutputs { get; } = new bool[2];
        public string LastPeripheralFeedback { get; set; } = "주변장치 조작 전";

        public RobotControlPeripheralState Clone()
        {
            var clone = new RobotControlPeripheralState
            {
                GripperOpen = GripperOpen,
                GripperOpenRatio = GripperOpenRatio,
                GripperVisualAttached = GripperVisualAttached,
                LastPeripheralFeedback = LastPeripheralFeedback,
            };
            RobotDigitalOutputs.CopyTo(clone.RobotDigitalOutputs, 0);
            ToolDigitalOutputs.CopyTo(clone.ToolDigitalOutputs, 0);
            return clone;
        }
    }
}
