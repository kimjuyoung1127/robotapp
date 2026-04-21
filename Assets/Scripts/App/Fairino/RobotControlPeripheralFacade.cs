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
                var command = FairinoGripperCommand.ForOpen(open);
                state.LastGripperSdkSummary = BuildGripperSdkSummary(includeReadback: true);
                state.LastPeripheralFeedback = $"{blockReason}; 공식 MoveGripper 후보: {command}";
                return FairinoResult.Fail(-60, blockReason);
            }

            state.GripperOpen = open;
            state.GripperOpenRatio = 0f;
            state.LastPeripheralFeedback = open ? "[Mock Gripper] 열림" : "[Mock Gripper] 닫힘";
            SyncMockSdkGripper(open);
            return FairinoResult.Ok(state.LastPeripheralFeedback);
        }

        public string GetGripperSdkSummary(bool includeReadback)
        {
            state.LastGripperSdkSummary = BuildGripperSdkSummary(includeReadback);
            return state.LastGripperSdkSummary;
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

        private string BuildGripperSdkSummary(bool includeReadback)
        {
            if (connectionService == null || connectionService.Client == null)
            {
                return "sdkGripper=blocked; reason=connection service missing";
            }

            if (!connectionService.Client.IsConnected)
            {
                return "sdkGripper=blocked; reason=not connected";
            }

            var profile = FairinoGripperProfile.Pgea10040Default;
            var openCommand = FairinoGripperCommand.ForOpen(true);
            var closeCommand = FairinoGripperCommand.ForOpen(false);
            var capability = connectionService.ProbeGripperCapability();
            if (!capability.IsSuccess)
            {
                return $"sdkGripper=probeFailed; code={capability.ErrorCode}; message={capability.Message}; profile={profile}; open={openCommand}; close={closeCommand}";
            }

            var summary = $"sdkGripper=probeOk; capability=({capability.Value}); profile=({profile}); open=({openCommand}); close=({closeCommand})";
            if (!includeReadback)
            {
                return summary;
            }

            var status = connectionService.ReadGripperStatus();
            return status.IsSuccess
                ? $"{summary}; readback=({status.Value})"
                : $"{summary}; readbackFailed=code {status.ErrorCode}: {status.Message}";
        }

        private void SyncMockSdkGripper(bool open)
        {
            if (connectionService == null || !connectionService.IsMockMode || connectionService.Client == null || !connectionService.Client.IsConnected)
            {
                return;
            }

            var command = FairinoGripperCommand.ForOpen(open);
            connectionService.ConfigureGripper(command.Profile);
            connectionService.ActivateGripper(command.Profile, activate: true);
            connectionService.MoveGripper(command);
            state.LastGripperSdkSummary = BuildGripperSdkSummary(includeReadback: true);
        }
    }
}
