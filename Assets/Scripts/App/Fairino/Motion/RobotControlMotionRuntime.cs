// Folder: App - Application controllers and services; single UnityEngine entry point.
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
        private readonly FairinoConnectionService connectionService;
        private readonly FairinoRobotConfig robotConfig;

        private RobotControlMotionRuntime(
            string robotId,
            RobotControlTemplateDefinition templateDefinition,
            FairinoRobotConfig config,
            FairinoConnectionService service)
        {
            RobotId = robotId;
            TemplateDefinition = templateDefinition;
            robotConfig = config;
            connectionService = service;
        }

        public string RobotId { get; }

        internal RobotControlTemplateDefinition TemplateDefinition { get; }

        public static FairinoResult<RobotControlMotionRuntime> CreateFromSelection()
        {
            var robotId = RobotSelectionBridge.GetSelectedRobotId();
            if (string.IsNullOrWhiteSpace(robotId))
            {
                return FairinoResult<RobotControlMotionRuntime>.Fail(-1, "선택된 로봇이 없어서 PointMove motion runtime을 만들 수 없다.");
            }

            var template = RobotControlFactory.Create(robotId);
            var service = template.ConnectionServiceFactory?.Invoke(new FairinoErrorTranslator());
            if (service == null)
            {
                return FairinoResult<RobotControlMotionRuntime>.Fail(-2, $"{robotId} 연결 서비스를 만들지 못했다.");
            }

            var config = FairinoRobotConfig.Load(template.ConfigResourceName) ?? template.FallbackConfigFactory?.Invoke();
            if (config == null)
            {
                return FairinoResult<RobotControlMotionRuntime>.Fail(-3, $"{robotId} 기본 연결 설정을 찾지 못했다.");
            }

            service.ApplyLiveDefaults(config.liveDefaults);
            return FairinoResult<RobotControlMotionRuntime>.Ok(
                new RobotControlMotionRuntime(robotId, template, config, service),
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
    }
}
