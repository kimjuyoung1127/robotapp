// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// V3 I/O와 그리퍼 명령을 mock/live 경계 뒤로 모읍니다.
    /// Live SDK 경로는 안전 게이트가 열릴 때까지 명시적으로 차단합니다.
    /// </summary>
    internal sealed class RobotControlPeripheralFacade
    {
        private readonly FairinoConnectionService connectionService;
        private readonly RobotControlPeripheralState state = new();

        public RobotControlPeripheralFacade(FairinoConnectionService connectionService)
        {
            this.connectionService = connectionService;
        }

        public RobotControlPeripheralState Snapshot => state.Clone();

        public FairinoResult SetGripperOpen(bool open, bool allowDryRun)
        {
            if (!CanSimulateOrMock(allowDryRun, out var blockReason))
            {
                state.LastPeripheralFeedback = blockReason;
                return FairinoResult.Fail(-60, blockReason);
            }

            state.GripperOpen = open;
            state.GripperOpenRatio = open ? 1f : 0f;
            state.LastPeripheralFeedback = open ? "[Mock Gripper] 열림" : "[Mock Gripper] 닫힘";
            return FairinoResult.Ok(state.LastPeripheralFeedback);
        }

        public FairinoResult SetRobotDigitalOutput(int channel, bool value, bool allowDryRun)
        {
            return SetOutput(state.RobotDigitalOutputs, channel, value, allowDryRun, "DO");
        }

        public FairinoResult SetToolDigitalOutput(int channel, bool value, bool allowDryRun)
        {
            return SetOutput(state.ToolDigitalOutputs, channel, value, allowDryRun, "ToolDO");
        }

        public void SetGripperVisualAttached(bool value)
        {
            state.GripperVisualAttached = value;
        }

        private FairinoResult SetOutput(bool[] outputs, int channel, bool value, bool allowDryRun, string label)
        {
            if (channel < 0 || channel >= outputs.Length)
            {
                var invalid = $"{label}{channel} 채널이 지원 범위를 벗어났다.";
                state.LastPeripheralFeedback = invalid;
                return FairinoResult.Fail(-61, invalid);
            }

            if (!CanSimulateOrMock(allowDryRun, out var blockReason))
            {
                state.LastPeripheralFeedback = blockReason;
                return FairinoResult.Fail(-60, blockReason);
            }

            outputs[channel] = value;
            state.LastPeripheralFeedback = $"[Mock I/O] {label}{channel}={(value ? "ON" : "OFF")}";
            return FairinoResult.Ok(state.LastPeripheralFeedback);
        }

        private bool CanSimulateOrMock(bool allowDryRun, out string reason)
        {
            if (connectionService == null)
            {
                reason = "peripheral blocked: connection service missing";
                return false;
            }

            if (connectionService.IsMockMode || allowDryRun)
            {
                reason = string.Empty;
                return true;
            }

            reason = "live blocked: I/O/Gripper SDK contract not enabled";
            return false;
        }
    }
}
