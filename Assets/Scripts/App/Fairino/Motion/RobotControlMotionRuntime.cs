// Folder: App - Application controllers and services; single UnityEngine entry point.
using System.Threading;
using KineTutor3D.App;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// V3 UI가 소비하는 최소 motion runtime facade입니다.
    /// 선택된 로봇 기준 연결 준비와 MoveJ/MoveL dispatch 정책을 App 계층에 모읍니다.
    /// </summary>
    public sealed class RobotControlMotionRuntime
    {
        public const int TinyMoveJSpeedCapPercent = 10;
        public const double TinyMoveJMaxJointDeltaDeg = 5d;
        public const double TinyMoveJRangeToleranceDeg = 0.01d;
        private const int AutoModeRetryCount = 6;
        private const int AutoModeRetryDelayMs = 150;

        private readonly FairinoConnectionService connectionService;
        private readonly FairinoRobotConfig robotConfig;
        private readonly bool preferMotionCapableDirect;

        private RobotControlMotionRuntime(
            string robotId,
            RobotControlTemplateDefinition templateDefinition,
            FairinoRobotConfig config,
            FairinoConnectionService service,
            bool preferMotionCapableDirect)
        {
            RobotId = robotId;
            TemplateDefinition = templateDefinition;
            robotConfig = config;
            connectionService = service;
            this.preferMotionCapableDirect = preferMotionCapableDirect;
        }

        public string RobotId { get; }
        public FairinoConnectionService ConnectionService => connectionService;
        public bool HasDedicatedTinyMoveJLivePath =>
            preferMotionCapableDirect
            && !connectionService.IsMockMode
            && connectionService.Client is IFairinoLiveClientDiagnostics { IsReadbackOnly: false };

        internal RobotControlTemplateDefinition TemplateDefinition { get; }

        public static FairinoResult<RobotControlMotionRuntime> CreateFromSelection(
            bool preferMotionCapableDirect = false,
            FairinoConnectionService existingConnectionService = null)
        {
            var robotId = RobotSelectionBridge.GetSelectedRobotId();
            if (string.IsNullOrWhiteSpace(robotId))
            {
                return FairinoResult<RobotControlMotionRuntime>.Fail(-1, "선택된 로봇이 없어서 PointMove motion runtime을 만들 수 없다.");
            }

            var template = RobotControlFactory.Create(robotId);
            var config = FairinoRobotConfig.Load(template.ConfigResourceName) ?? template.FallbackConfigFactory?.Invoke();
            if (config == null)
            {
                return FairinoResult<RobotControlMotionRuntime>.Fail(-3, $"{robotId} 기본 연결 설정을 찾지 못했다.");
            }

            FairinoConnectionService service;
            if (preferMotionCapableDirect && existingConnectionService != null)
            {
                var siblingResult = existingConnectionService.CreateMotionSiblingSession();
                if (!siblingResult.IsSuccess)
                {
                    return FairinoResult<RobotControlMotionRuntime>.Fail(-2, $"{robotId} 기존 live 세션 재사용 실패: {siblingResult.Message}");
                }

                service = siblingResult.Value;
            }
            else
            {
                service = template.ConnectionServiceFactory?.Invoke(new FairinoErrorTranslator());
                if (service == null)
                {
                    return FairinoResult<RobotControlMotionRuntime>.Fail(-2, $"{robotId} 연결 서비스를 만들지 못했다.");
                }

                service.ApplyLiveDefaults(config.liveDefaults);
                if (preferMotionCapableDirect)
                {
                    service.SetMockMode(false, preferMotionCapableDirect: true);
                }
            }

            service.ApplyLiveDefaults(config.liveDefaults);

            return FairinoResult<RobotControlMotionRuntime>.Ok(
                new RobotControlMotionRuntime(robotId, template, config, service, preferMotionCapableDirect),
                $"{robotId} motion runtime 준비 완료");
        }

        public FairinoResult EnsureReady()
        {
            if (!connectionService.Client.IsConnected)
            {
                if (string.IsNullOrWhiteSpace(robotConfig.defaultIp) || robotConfig.defaultPort <= 0)
                {
                    return FairinoResult.Fail(-4, $"{RobotId} 연결 기본값이 비어 있어서 실기/mock 세션을 준비할 수 없다.");
                }

                var connectResult = connectionService.Connect(robotConfig.defaultIp, robotConfig.defaultPort);
                if (!connectResult.IsSuccess)
                {
                    return connectResult;
                }
            }

            if (!connectionService.Client.IsEnabled)
            {
                var enableResult = connectionService.Enable();
                if (!enableResult.IsSuccess)
                {
                    return enableResult;
                }
            }

            if (!connectionService.IsMockMode
                && connectionService.Client is IFairinoLiveClientDiagnostics { IsReadbackOnly: false })
            {
                var autoReadyResult = EnsureMotionSessionAutoModeReady();
                if (!autoReadyResult.IsSuccess)
                {
                    return autoReadyResult;
                }
            }
            else
            {
                var stateResult = connectionService.SyncCurrentState();
                if (!stateResult.IsSuccess)
                {
                    return new FairinoResult(stateResult.ErrorCode, stateResult.Message);
                }
            }

            return FairinoResult.Ok($"{RobotId} motion runtime ready");
        }

        public FairinoResult DispatchMoveL(double[] targetTcpPose, int requestedSpeedPercent)
        {
            if (targetTcpPose == null || targetTcpPose.Length < 6)
            {
                return FairinoResult.Fail(-5, "MoveL 대상 TCP pose가 비어 있다.");
            }

            var readyResult = EnsureReady();
            if (!readyResult.IsSuccess)
            {
                return readyResult;
            }

            var speedAcc = BuildSpeedAcc(requestedSpeedPercent);
            return connectionService.Client.MoveL(targetTcpPose, speedAcc.speed, speedAcc.acc);
        }

        public FairinoResult DispatchMoveJ(double[] targetJointPosDeg, int requestedSpeedPercent)
        {
            if (targetJointPosDeg == null || targetJointPosDeg.Length < TemplateDefinition.JointCount)
            {
                return FairinoResult.Fail(-6, "MoveJ 대상 joint 값이 부족하다.");
            }

            var readyResult = EnsureReady();
            if (!readyResult.IsSuccess)
            {
                return readyResult;
            }

            var speedAcc = BuildSpeedAcc(requestedSpeedPercent);
            return connectionService.Client.MoveJ(targetJointPosDeg, speedAcc.speed, speedAcc.acc);
        }

        public bool TryEvaluateTinyMoveJRange(double[] currentJointPosDeg, double[] targetJointPosDeg, out double maxJointDeltaDeg, out int maxJointDeltaIndex)
        {
            maxJointDeltaDeg = 0d;
            maxJointDeltaIndex = -1;
            if (currentJointPosDeg == null || targetJointPosDeg == null)
            {
                return false;
            }

            var length = System.Math.Min(System.Math.Min(currentJointPosDeg.Length, targetJointPosDeg.Length), TemplateDefinition.JointCount);
            if (length <= 0)
            {
                return false;
            }

            for (var index = 0; index < length; index++)
            {
                var delta = Mathf.Abs((float)(targetJointPosDeg[index] - currentJointPosDeg[index]));
                if (delta > maxJointDeltaDeg)
                {
                    maxJointDeltaDeg = delta;
                    maxJointDeltaIndex = index;
                }
            }

            return maxJointDeltaDeg <= TinyMoveJMaxJointDeltaDeg + TinyMoveJRangeToleranceDeg;
        }

        public FairinoResult DispatchTinyMoveJ(double[] currentJointPosDeg, double[] targetJointPosDeg, int requestedSpeedPercent)
        {
            if (!HasDedicatedTinyMoveJLivePath)
            {
                return FairinoResult.Fail(
                    -71,
                    $"tiny MoveJ live path가 아직 잠겨 있다. 환경 변수 {FairinoRobotClientFactory.TinyMoveJLiveEnvironmentVariable}=1 확인이 필요하다.");
            }

            if (targetJointPosDeg == null || targetJointPosDeg.Length < TemplateDefinition.JointCount)
            {
                return FairinoResult.Fail(-6, "tiny MoveJ 대상 joint 값이 부족하다.");
            }

            var readyResult = EnsureReady();
            if (!readyResult.IsSuccess)
            {
                return readyResult;
            }

            var liveBaseline = connectionService.LastState.JointPosDeg;
            var baseline = liveBaseline != null && liveBaseline.Length >= TemplateDefinition.JointCount
                ? liveBaseline
                : currentJointPosDeg;
            if (!TryEvaluateTinyMoveJRange(baseline, targetJointPosDeg, out var maxDelta, out var maxDeltaIndex))
            {
                var jointLabel = maxDeltaIndex >= 0 ? $"J{maxDeltaIndex + 1}" : "joint";
                return FairinoResult.Fail(
                    -72,
                    $"tiny MoveJ 범위를 넘었다. {jointLabel} delta {maxDelta:0.###}deg > {TinyMoveJMaxJointDeltaDeg:0.###}deg");
            }

            var speedAcc = BuildSpeedAcc(System.Math.Min(requestedSpeedPercent, TinyMoveJSpeedCapPercent));
            return connectionService.Client.MoveJ(targetJointPosDeg, speedAcc.speed, speedAcc.acc);
        }

        private (int speed, int acc) BuildSpeedAcc(int requestedSpeedPercent)
        {
            if (requestedSpeedPercent <= 0)
            {
                return robotConfig.GetMediumSpeedAcc();
            }

            var speed = Mathf.Clamp(requestedSpeedPercent, 1, 100);
            var acc = Mathf.Clamp(speed + 20, 1, 100);
            return (speed, acc);
        }

        private FairinoResult EnsureMotionSessionAutoModeReady()
        {
            FairinoResult lastFailure = FairinoResult.Ok();
            FairinoRobotState lastState = FairinoRobotState.Zero();
            var hasState = false;

            for (var attempt = 0; attempt < AutoModeRetryCount; attempt++)
            {
                if (attempt > 0)
                {
                    Thread.Sleep(AutoModeRetryDelayMs);
                }

                var dragResult = connectionService.Client.ExitDragTeach();
                if (!dragResult.IsSuccess)
                {
                    lastFailure = dragResult;
                }

                var modeResult = connectionService.Client.EnsureAutoMode();
                if (!modeResult.IsSuccess)
                {
                    lastFailure = modeResult;
                }

                var stateResult = connectionService.SyncCurrentState();
                if (!stateResult.IsSuccess)
                {
                    lastFailure = new FairinoResult(stateResult.ErrorCode, stateResult.Message);
                    continue;
                }

                lastState = stateResult.Value;
                hasState = true;
                if (!lastState.IsInDragTeach && lastState.RobotMode == 0)
                {
                    return FairinoResult.Ok($"{RobotId} motion runtime ready");
                }
            }

            if (hasState)
            {
                if (lastState.IsInDragTeach)
                {
                    return FairinoResult.Fail(-8, $"드래그 티칭이 아직 해제되지 않았습니다. mode={lastState.RobotMode}; enabled={lastState.IsRobotEnabled}");
                }

                if (lastState.RobotMode != 0)
                {
                    return FairinoResult.Fail(-7, $"motion 세션이 자동 모드로 올라오지 않았습니다. mode={lastState.RobotMode}; enabled={lastState.IsRobotEnabled}");
                }
            }

            return lastFailure.IsSuccess
                ? FairinoResult.Fail(-7, "motion 세션 자동 모드 재확인에 실패했습니다.")
                : lastFailure;
        }
    }
}
