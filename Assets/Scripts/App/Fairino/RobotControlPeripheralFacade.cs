// Folder: App - Application controllers and services; single UnityEngine entry point.
using System.Diagnostics;
using System.Threading;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// V3 I/O와 그리퍼 명령을 mock/live 경계 뒤로 모읍니다.
    /// Live SDK 경로는 안전 게이트가 열릴 때까지 명시적으로 차단합니다.
    /// </summary>
    internal sealed class RobotControlPeripheralFacade
    {
        private enum GripperActivationSequence
        {
            ActivateOnly,
            ResetThenActivate,
            ConfigureThenActivate,
            ConfigureResetActivate,
        }

        private const int GripperActivationPollTimeoutMs = 5000;
        private const int GripperActivationPollIntervalMs = 200;

        private readonly FairinoConnectionService connectionService;
        private readonly RobotControlPeripheralState state = new();
        private readonly GripperCalibrationProfile gripperCalibration;
        private readonly FairinoGripperProfile gripperProfile;

        public RobotControlPeripheralFacade(FairinoConnectionService connectionService, FairinoRobotConfig config = null)
        {
            this.connectionService = connectionService;
            gripperCalibration = config?.GetGripperCalibration() ?? GripperCalibrationProfile.Pgea10040Observed;
            gripperProfile = config?.GetGripperProfile() ?? FairinoGripperProfile.Pgea10040Default;
        }

        public RobotControlPeripheralState Snapshot => state.Clone();

        public FairinoResult SetGripperOpen(bool open, bool allowDryRun)
        {
            return SetGripperPosition(open ? 100f : 0f, allowDryRun, objectDetected: false, objectStopPercent: 0);
        }

        public FairinoResult SetGripperPosition(int positionPercent, bool allowDryRun, bool objectDetected, int objectStopPercent, FairinoConnectionService liveWriteService = null)
        {
            return SetGripperPosition((float)positionPercent, allowDryRun, objectDetected, objectStopPercent, liveWriteService);
        }

        public FairinoResult SetGripperPosition(float positionPercent, bool allowDryRun, bool objectDetected, int objectStopPercent, FairinoConnectionService liveWriteService = null)
        {
            var commandedUser = ClampPercent(positionPercent);
            var commandedRaw = gripperCalibration.UserToRawPercent(commandedUser);
            var effectiveConnectionService = liveWriteService ?? connectionService;
            if (!CanSimulateOrMock(effectiveConnectionService, allowDryRun, out var blockReason))
            {
                var command = FairinoGripperCommand.ForPosition(commandedRaw);
                state.LastGripperSdkSummary = BuildGripperSdkSummary(includeReadback: true, effectiveConnectionService);
                state.LastPeripheralFeedback = $"{blockReason}; 공식 MoveGripper 후보: {command}; ui={commandedUser}%";
                return FairinoResult.Fail(-60, blockReason);
            }

            if (!allowDryRun && effectiveConnectionService != null && !effectiveConnectionService.IsMockMode)
            {
                return ExecuteLiveGripperCommand(effectiveConnectionService, commandedUser, commandedRaw);
            }

            var stopRaw = objectDetected ? ClampPercent(objectStopPercent) : 0;
            var actualRaw = objectDetected && commandedRaw < stopRaw ? stopRaw : commandedRaw;
            var actualUser = gripperCalibration.RawToUserPercent(actualRaw);
            var stopUser = objectDetected ? gripperCalibration.RawToUserPercent(stopRaw) : 0f;
            state.GripperCommandedPositionPercent = commandedUser;
            state.GripperActualPositionPercent = actualUser;
            state.GripperRawCommandedPositionPercent = commandedRaw;
            state.GripperRawActualPositionPercent = actualRaw;
            state.GripperSpeedPercent = 50;
            state.GripperForcePercent = 50;
            state.GripperOpen = actualUser >= 50f;
            state.GripperOpenRatio = gripperCalibration.UserToVisualOpenRatio(actualUser);
            state.GripperObjectDetected = objectDetected;
            state.GripperObjectStopPercent = stopUser;
            state.GripperRawObjectStopPercent = stopRaw;
            state.GripperHoldingObject = objectDetected && commandedRaw < stopRaw;
            state.HasReliableGripperReadback = true;
            state.LastGripperReadbackNote = string.Empty;
            state.LastPeripheralFeedback = BuildGripperFeedback(commandedUser, actualUser, commandedRaw, actualRaw, objectDetected, stopRaw);
            SyncMockSdkGripper(actualRaw, state.GripperSpeedPercent, state.GripperForcePercent);
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

            if (!CanSimulateOrMock(connectionService, allowDryRun, out var blockReason))
            {
                state.LastPeripheralFeedback = blockReason;
                return FairinoResult.Fail(-60, blockReason);
            }

            outputs[channel] = value;
            state.LastPeripheralFeedback = $"[Mock I/O] {label}{channel}={(value ? "ON" : "OFF")}";
            return FairinoResult.Ok(state.LastPeripheralFeedback);
        }

        private bool CanSimulateOrMock(FairinoConnectionService effectiveConnectionService, bool allowDryRun, out string reason)
        {
            if (effectiveConnectionService == null)
            {
                reason = "peripheral blocked: connection service missing";
                return false;
            }

            if (effectiveConnectionService.IsMockMode || allowDryRun)
            {
                reason = string.Empty;
                return true;
            }

            if (!ReferenceEquals(effectiveConnectionService, connectionService))
            {
                reason = string.Empty;
                return true;
            }

            reason = "live blocked: I/O/Gripper SDK contract not enabled";
            return false;
        }

        private FairinoResult ExecuteLiveGripperCommand(FairinoConnectionService liveService, float commandedUser, int commandedRaw)
        {
            var capability = liveService.ProbeGripperCapability();
            if (!capability.IsSuccess)
            {
                state.LastGripperSdkSummary = BuildGripperSdkSummary(includeReadback: false, liveService);
                state.LastPeripheralFeedback = capability.Message;
                return new FairinoResult(capability.ErrorCode, capability.Message);
            }

            if (!capability.Value.CanUseLiveGripper)
            {
                state.LastGripperSdkSummary = BuildGripperSdkSummary(includeReadback: false, liveService);
                state.LastPeripheralFeedback = "live gripper capability missing";
                return FairinoResult.Fail(-62, state.LastPeripheralFeedback);
            }

            var command = new FairinoGripperCommand(gripperProfile, commandedRaw, 20, 20, 30000, blocking: true);
            var prepare = TryEnsureLiveGripperReady(liveService, command.Profile);
            if (!prepare.IsSuccess)
            {
                state.LastGripperSdkSummary = BuildGripperSdkSummary(includeReadback: true, liveService);
                state.LastPeripheralFeedback = prepare.Message;
                return prepare;
            }

            var move = liveService.MoveGripper(command);
            if (!move.IsSuccess)
            {
                state.LastGripperSdkSummary = BuildGripperSdkSummary(includeReadback: true, liveService);
                state.LastPeripheralFeedback = move.Message;
                return move;
            }

            var status = liveService.ReadGripperStatus();
            var hasReliableReadback = status.IsSuccess
                && status.Value.MotionFault == 0
                && status.Value.MotionDone != 0
                && status.Value.PositionFault == 0
                && gripperCalibration.IsWithinObservedRawRange(ClampPercent(status.Value.PositionPercent));
            var actualRaw = hasReliableReadback
                ? ClampPercent(status.Value.PositionPercent)
                : commandedRaw;
            var actualUser = gripperCalibration.RawToUserPercent(actualRaw);
            state.GripperCommandedPositionPercent = commandedUser;
            state.GripperActualPositionPercent = actualUser;
            state.GripperRawCommandedPositionPercent = commandedRaw;
            state.GripperRawActualPositionPercent = actualRaw;
            state.GripperSpeedPercent = status.IsSuccess && status.Value.SpeedFault == 0
                ? ClampPercent(status.Value.SpeedPercent)
                : command.SpeedPercent;
            state.GripperForcePercent = command.ForcePercent;
            state.GripperOpen = actualUser >= 50f;
            state.GripperOpenRatio = gripperCalibration.UserToVisualOpenRatio(actualUser);
            state.GripperObjectDetected = false;
            state.GripperObjectStopPercent = 0;
            state.GripperRawObjectStopPercent = 0;
            state.GripperHoldingObject = false;
            state.HasReliableGripperReadback = hasReliableReadback;
            state.LastGripperReadbackNote = status.IsSuccess
                ? BuildReadbackReliabilityNote(status.Value, hasReliableReadback)
                : $"readback pending: {status.Message}";
            state.LastGripperSdkSummary = BuildGripperSdkSummary(includeReadback: true, liveService);
            state.LastPeripheralFeedback = status.IsSuccess
                ? hasReliableReadback
                    ? $"[Live Gripper] 요청 {FormatPercent(commandedUser)}% -> readback {FormatPercent(actualUser)}% (raw {commandedRaw}%->{actualRaw}%)"
                    : $"[Live Gripper] 요청 {FormatPercent(commandedUser)}% 전송 완료 · readback 확인 안 됨 ({state.LastGripperReadbackNote})"
                : $"[Live Gripper] 요청 {FormatPercent(commandedUser)}% 전송 완료 · gripper readback pending ({status.Message})";
            return FairinoResult.Ok(state.LastPeripheralFeedback);
        }

        internal static string ProbeLiveGripperProfileForDebug(FairinoConnectionService liveService, FairinoGripperProfile profile)
        {
            if (liveService == null)
            {
                return $"profile=({profile}); FAIL(connection service missing)";
            }

            var capability = liveService.ProbeGripperCapability();
            var config = liveService.ReadGripperConfig();
            var initialStatus = liveService.ReadGripperStatus();
            var prepare = TryEnsureLiveGripperReady(liveService, profile);
            var finalStatus = liveService.ReadGripperStatus();
            var configSummary = config.IsSuccess
                ? config.Value.ToString()
                : $"FAIL({config.ErrorCode}:{config.Message})";
            var capabilitySummary = capability.IsSuccess
                ? capability.Value.ToString()
                : $"FAIL({capability.ErrorCode}:{capability.Message})";
            var initialSummary = initialStatus.IsSuccess
                ? initialStatus.Value.ToString()
                : $"FAIL({initialStatus.ErrorCode}:{initialStatus.Message})";
            var finalSummary = finalStatus.IsSuccess
                ? finalStatus.Value.ToString()
                : $"FAIL({finalStatus.ErrorCode}:{finalStatus.Message})";
            return $"profile=({profile}); capability=({capabilitySummary}); sdkConfig=({configSummary}); initial=({initialSummary}); prepare={(prepare.IsSuccess ? "OK" : $"FAIL({prepare.ErrorCode}:{prepare.Message})")}; final=({finalSummary})";
        }

        internal static string ProbeLiveGripperActivationSequencesForDebug(FairinoConnectionService liveService, FairinoGripperProfile profile)
        {
            if (liveService == null)
            {
                return $"profile=({profile}); FAIL(connection service missing)";
            }

            var lines = new System.Text.StringBuilder();
            lines.Append($"profile=({profile})");
            AppendActivationSequenceResult(lines, "activate-only", RunActivationSequenceForDebug(liveService, profile, GripperActivationSequence.ActivateOnly));
            AppendActivationSequenceResult(lines, "reset-activate", RunActivationSequenceForDebug(liveService, profile, GripperActivationSequence.ResetThenActivate));
            AppendActivationSequenceResult(lines, "configure-activate", RunActivationSequenceForDebug(liveService, profile, GripperActivationSequence.ConfigureThenActivate));
            AppendActivationSequenceResult(lines, "configure-reset-activate", RunActivationSequenceForDebug(liveService, profile, GripperActivationSequence.ConfigureResetActivate));
            return lines.ToString();
        }

        internal static FairinoResult TryWarmUpLiveGripper(FairinoConnectionService liveService, FairinoGripperProfile profile)
        {
            return TryEnsureLiveGripperReady(liveService, profile);
        }

        internal static bool IsGripperActivationReadyForProfile(FairinoGripperStatus status, FairinoGripperProfile profile)
        {
            return IsGripperActivationReady(status, profile);
        }

        internal static string BuildOperatorGripperNotReadyMessage(FairinoGripperStatus status, bool warmupAttempted)
        {
            var prefix = warmupAttempted
                ? "그리퍼 준비 안 됨 · warm-up 1회 후에도 activation ready가 아니다."
                : "그리퍼 준비 안 됨 · activation ready가 아직 아니다.";
            return $"{prefix} activationMask={status.ActivationMask}; activationFault={status.ActivationFault}; motionFault={status.MotionFault}; done={status.MotionDone}; positionFault={status.PositionFault}; position={status.PositionPercent}";
        }

        private static FairinoResult TryEnsureLiveGripperReady(FairinoConnectionService liveService, FairinoGripperProfile profile)
        {
            var initialStatus = liveService.ReadGripperStatus();
            if (initialStatus.IsSuccess && IsGripperActivationReady(initialStatus.Value, profile))
            {
                return FairinoResult.Ok("gripper already active");
            }

            var configure = liveService.ConfigureGripper(profile);
            if (!configure.IsSuccess)
            {
                return configure;
            }

            var reset = liveService.ActivateGripper(profile, activate: false);
            if (!reset.IsSuccess)
            {
                return reset;
            }

            var activate = liveService.ActivateGripper(profile, activate: true);
            if (!activate.IsSuccess)
            {
                return activate;
            }

            var wait = WaitForGripperActivation(liveService, profile);
            if (!wait.IsSuccess)
            {
                return wait;
            }

            return FairinoResult.Ok("gripper ready");
        }

        private static string RunActivationSequenceForDebug(
            FairinoConnectionService liveService,
            FairinoGripperProfile profile,
            GripperActivationSequence sequence)
        {
            var initialStatus = liveService.ReadGripperStatus();
            var configureResult = FairinoResult.Ok("skipped");
            var resetResult = FairinoResult.Ok("skipped");
            var activateResult = FairinoResult.Ok("skipped");

            switch (sequence)
            {
                case GripperActivationSequence.ActivateOnly:
                    activateResult = liveService.ActivateGripper(profile, activate: true);
                    break;
                case GripperActivationSequence.ResetThenActivate:
                    resetResult = liveService.ActivateGripper(profile, activate: false);
                    if (resetResult.IsSuccess)
                    {
                        activateResult = liveService.ActivateGripper(profile, activate: true);
                    }
                    break;
                case GripperActivationSequence.ConfigureThenActivate:
                    configureResult = liveService.ConfigureGripper(profile);
                    if (configureResult.IsSuccess)
                    {
                        activateResult = liveService.ActivateGripper(profile, activate: true);
                    }
                    break;
                case GripperActivationSequence.ConfigureResetActivate:
                    configureResult = liveService.ConfigureGripper(profile);
                    if (configureResult.IsSuccess)
                    {
                        resetResult = liveService.ActivateGripper(profile, activate: false);
                    }
                    if (configureResult.IsSuccess && resetResult.IsSuccess)
                    {
                        activateResult = liveService.ActivateGripper(profile, activate: true);
                    }
                    break;
            }

            var waitResult = activateResult.IsSuccess
                ? WaitForGripperActivation(liveService, profile)
                : FairinoResult.Fail(activateResult.ErrorCode, activateResult.Message);
            var finalStatus = liveService.ReadGripperStatus();
            return $"initial=({FormatStatus(initialStatus)}); configure={FormatResult(configureResult)}; reset={FormatResult(resetResult)}; activate={FormatResult(activateResult)}; wait={FormatResult(waitResult)}; final=({FormatStatus(finalStatus)})";
        }

        private static FairinoResult WaitForGripperActivation(FairinoConnectionService liveService, FairinoGripperProfile profile)
        {
            var watch = Stopwatch.StartNew();
            FairinoResult<FairinoGripperStatus> lastStatus = default;
            while (watch.ElapsedMilliseconds < GripperActivationPollTimeoutMs)
            {
                lastStatus = liveService.ReadGripperStatus();
                if (lastStatus.IsSuccess && IsGripperActivationReady(lastStatus.Value, profile))
                {
                    return FairinoResult.Ok("gripper activation confirmed");
                }

                Thread.Sleep(GripperActivationPollIntervalMs);
            }

            var detail = lastStatus.IsSuccess
                ? lastStatus.Value.ToString()
                : lastStatus.Message;
            return FairinoResult.Fail(-63, $"gripper activation not ready: {detail}");
        }

        private static bool IsGripperActivationReady(FairinoGripperStatus status, FairinoGripperProfile profile)
        {
            if (status.ActivationFault != 0)
            {
                return false;
            }

            var zeroBasedBit = profile.Index >= 0 && profile.Index < 31
                ? 1 << profile.Index
                : 0;
            var oneBasedBit = profile.Index > 0 && profile.Index <= 31
                ? 1 << (profile.Index - 1)
                : 0;
            return (zeroBasedBit != 0 && (status.ActivationMask & zeroBasedBit) != 0)
                || (oneBasedBit != 0 && (status.ActivationMask & oneBasedBit) != 0);
        }

        private static void AppendActivationSequenceResult(System.Text.StringBuilder lines, string label, string result)
        {
            lines.AppendLine();
            lines.Append(label);
            lines.Append(": ");
            lines.Append(result);
        }

        private static string FormatResult(FairinoResult result)
        {
            return result.IsSuccess
                ? $"OK({result.Message})"
                : $"FAIL({result.ErrorCode}:{result.Message})";
        }

        private static string FormatStatus(FairinoResult<FairinoGripperStatus> status)
        {
            return status.IsSuccess
                ? status.Value.ToString()
                : $"FAIL({status.ErrorCode}:{status.Message})";
        }

        private string BuildGripperSdkSummary(bool includeReadback, FairinoConnectionService serviceOverride = null)
        {
            var effectiveConnectionService = serviceOverride ?? connectionService;
            if (effectiveConnectionService == null || effectiveConnectionService.Client == null)
            {
                return "sdkGripper=blocked; reason=connection service missing";
            }

            if (!effectiveConnectionService.Client.IsConnected)
            {
                return "sdkGripper=blocked; reason=not connected";
            }

            var profile = gripperProfile;
            var openCommand = FairinoGripperCommand.ForOpen(true);
            var closeCommand = new FairinoGripperCommand(profile, gripperCalibration.UserToRawPercent(0), 50, 50, 30000, blocking: true);
            var capability = effectiveConnectionService.ProbeGripperCapability();
            if (!capability.IsSuccess)
            {
                return $"sdkGripper=probeFailed; code={capability.ErrorCode}; message={capability.Message}; profile={profile}; calibration=({gripperCalibration}); open={openCommand}; close={closeCommand}";
            }

            var summary = $"sdkGripper=probeOk; capability=({capability.Value}); profile=({profile}); calibration=({gripperCalibration}); open=({openCommand}); close=({closeCommand})";
            if (!includeReadback)
            {
                return summary;
            }

            var status = effectiveConnectionService.ReadGripperStatus();
            return status.IsSuccess
                ? $"{summary}; readback=({status.Value})"
                : $"{summary}; readbackFailed=code {status.ErrorCode}: {status.Message}";
        }

        private void SyncMockSdkGripper(int positionPercent, int speedPercent, int forcePercent)
        {
            if (connectionService == null || !connectionService.IsMockMode || connectionService.Client == null || !connectionService.Client.IsConnected)
            {
                return;
            }

            var command = new FairinoGripperCommand(gripperProfile, positionPercent, speedPercent, forcePercent, 30000, blocking: true);
            connectionService.ConfigureGripper(command.Profile);
            connectionService.ActivateGripper(command.Profile, activate: true);
            connectionService.MoveGripper(command);
            state.LastGripperSdkSummary = BuildGripperSdkSummary(includeReadback: true);
        }

        private static string BuildReadbackReliabilityNote(FairinoGripperStatus status, bool hasReliableReadback)
        {
            if (hasReliableReadback)
            {
                return "readback ok";
            }

            return $"motionFault={status.MotionFault}; done={status.MotionDone}; positionFault={status.PositionFault}; raw={status.PositionPercent}";
        }

        private static string BuildGripperFeedback(float commanded, float actual, int rawCommanded, int rawActual, bool objectDetected, int rawStopPercent)
        {
            if (objectDetected && rawCommanded < rawStopPercent)
            {
                return $"[Mock Gripper] 물체 감지 · 요청 {FormatPercent(commanded)}% -> 안전 정지 {FormatPercent(actual)}% · 잡은 상태 (raw {rawCommanded}%->{rawActual}%)";
            }

            if (actual >= 99f)
            {
                return "[Mock Gripper] 완전 열림 100%";
            }

            if (actual <= 1f)
            {
                return $"[Mock Gripper] 완전 닫힘 0% (raw {rawActual}%)";
            }

            return $"[Mock Gripper] 위치 {FormatPercent(actual)}% (raw {rawActual}%)";
        }

        private static int ClampPercent(int value)
        {
            return value < 0 ? 0 : value > 100 ? 100 : value;
        }

        private static float ClampPercent(float value)
        {
            return value < 0f ? 0f : value > 100f ? 100f : value;
        }

        private static string FormatPercent(float value)
        {
            return value.ToString("0.##");
        }
    }
}
