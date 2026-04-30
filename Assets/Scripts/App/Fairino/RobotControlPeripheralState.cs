// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// Pendant V3가 소비하는 I/O와 그리퍼 주변장치 상태입니다.
    /// </summary>
    internal sealed class RobotControlPeripheralState
    {
        public bool GripperOpen { get; set; } = true;
        public float GripperOpenRatio { get; set; } = 1f;
        public float GripperCommandedPositionPercent { get; set; } = 100f;
        public float GripperActualPositionPercent { get; set; } = 100f;
        public int GripperRawCommandedPositionPercent { get; set; } = 100;
        public int GripperRawActualPositionPercent { get; set; } = 100;
        public int GripperSpeedPercent { get; set; } = 50;
        public int GripperForcePercent { get; set; } = 50;
        public bool GripperObjectDetected { get; set; }
        public bool GripperHoldingObject { get; set; }
        public bool HasReliableGripperReadback { get; set; } = true;
        public float GripperObjectStopPercent { get; set; }
        public int GripperRawObjectStopPercent { get; set; }
        public bool GripperVisualAttached { get; set; }
        public bool[] RobotDigitalOutputs { get; } = new bool[2];
        public bool[] ToolDigitalOutputs { get; } = new bool[2];
        public string LastPeripheralFeedback { get; set; } = "주변장치 조작 전";
        public string LastGripperSdkSummary { get; set; } = "SDK gripper 비교 전";
        public string LastGripperReadbackNote { get; set; } = string.Empty;

        public RobotControlPeripheralState Clone()
        {
            var clone = new RobotControlPeripheralState
            {
                GripperOpen = GripperOpen,
                GripperOpenRatio = GripperOpenRatio,
                GripperCommandedPositionPercent = GripperCommandedPositionPercent,
                GripperActualPositionPercent = GripperActualPositionPercent,
                GripperRawCommandedPositionPercent = GripperRawCommandedPositionPercent,
                GripperRawActualPositionPercent = GripperRawActualPositionPercent,
                GripperSpeedPercent = GripperSpeedPercent,
                GripperForcePercent = GripperForcePercent,
                GripperObjectDetected = GripperObjectDetected,
                GripperHoldingObject = GripperHoldingObject,
                HasReliableGripperReadback = HasReliableGripperReadback,
                GripperObjectStopPercent = GripperObjectStopPercent,
                GripperRawObjectStopPercent = GripperRawObjectStopPercent,
                GripperVisualAttached = GripperVisualAttached,
                LastPeripheralFeedback = LastPeripheralFeedback,
                LastGripperSdkSummary = LastGripperSdkSummary,
                LastGripperReadbackNote = LastGripperReadbackNote,
            };
            RobotDigitalOutputs.CopyTo(clone.RobotDigitalOutputs, 0);
            ToolDigitalOutputs.CopyTo(clone.ToolDigitalOutputs, 0);
            return clone;
        }
    }
}
