// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.App.Fairino;

namespace KineTutor3D.App.Doosan
{
    /// <summary>
    /// 오프라인 테스트용 Doosan M1013 클라이언트 Mock 구현입니다.
    /// SDK 없이도 UI와 로직을 검증할 수 있습니다.
    /// </summary>
    public sealed class MockDoosanClient : IFairinoRobotClient
    {
        private double[] currentJointPosDeg = { 0, 0, 0, 0, 0, 0 };
        private double[] currentTcpPose = new double[6];
        private int currentMode;
        private int stateSamplePeriodMs = 100;

        /// <summary>
        /// 현재 연결 상태입니다.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// 로봇 활성화(서보 ON) 상태입니다.
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// 로봇에 연결합니다 (Mock: 항상 성공).
        /// </summary>
        public FairinoResult Connect(string ip, int port)
        {
            if (string.IsNullOrEmpty(ip))
            {
                return FairinoResult.Fail(-1, "IP 주소가 비어 있습니다.");
            }

            IsConnected = true;
            return FairinoResult.Ok("Mock Doosan M1013 연결 성공");
        }

        /// <summary>
        /// 연결을 해제합니다.
        /// </summary>
        public FairinoResult Disconnect()
        {
            IsConnected = false;
            IsEnabled = false;
            return FairinoResult.Ok("Mock Doosan M1013 연결 해제");
        }

        /// <summary>
        /// 로봇을 활성화합니다 (Mock: 연결 상태면 성공).
        /// </summary>
        public FairinoResult Enable()
        {
            if (!IsConnected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            IsEnabled = true;
            return FairinoResult.Ok("Mock Doosan M1013 서보 ON");
        }

        /// <summary>
        /// 로봇을 비활성화합니다.
        /// </summary>
        public FairinoResult Disable()
        {
            IsEnabled = false;
            return FairinoResult.Ok("Mock Doosan M1013 서보 OFF");
        }

        /// <summary>
        /// 관절 이동 명령입니다 (Mock: 즉시 목표 위치로 이동).
        /// </summary>
        public FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent)
        {
            if (!IsConnected) return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            if (!IsEnabled) return FairinoResult.Fail(-2, "로봇이 비활성 상태입니다.");
            if (jointPosDeg == null || jointPosDeg.Length != 6)
                return FairinoResult.Fail(-3, "6축 관절 값이 필요합니다.");

            Array.Copy(jointPosDeg, currentJointPosDeg, 6);
            return FairinoResult.Ok("Mock Doosan M1013 MoveJ 완료");
        }

        /// <summary>
        /// 서보 관절 이동 명령입니다 (Mock: 즉시 목표 위치로 이동).
        /// </summary>
        public FairinoResult ServoJ(double[] jointPosDeg)
        {
            if (!IsConnected) return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            if (!IsEnabled) return FairinoResult.Fail(-2, "로봇이 비활성 상태입니다.");
            if (jointPosDeg == null || jointPosDeg.Length != 6)
                return FairinoResult.Fail(-3, "6축 관절 값이 필요합니다.");

            Array.Copy(jointPosDeg, currentJointPosDeg, 6);
            return FairinoResult.Ok("Mock Doosan M1013 ServoJ 완료");
        }

        /// <summary>
        /// 현재 로봇 상태를 반환합니다 (Mock: 내부 상태 반환).
        /// </summary>
        public FairinoResult<FairinoRobotState> ReadState()
        {
            if (!IsConnected)
            {
                return FairinoResult<FairinoRobotState>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            var state = new FairinoRobotState(
                currentJointPosDeg,
                currentTcpPose,
                robotMode: currentMode,
                motionQueueLength: 0,
                safetyCode: 0,
                realtimeStateSamplePeriodMs: stateSamplePeriodMs,
                isEmergencyStop: false,
                isCollisionDetected: false,
                isRobotEnabled: IsEnabled);
            return FairinoResult<FairinoRobotState>.Ok(state, "Mock Doosan M1013 상태 읽기 성공");
        }

        /// <summary>
        /// 직교 공간 직선 이동 명령입니다 (Mock: 즉시 목표 포즈로 이동).
        /// </summary>
        public FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent)
        {
            if (!IsConnected) return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            if (!IsEnabled) return FairinoResult.Fail(-2, "로봇이 비활성 상태입니다.");
            if (tcpPose == null || tcpPose.Length != 6)
                return FairinoResult.Fail(-3, "6축 TCP 포즈가 필요합니다.");

            Array.Copy(tcpPose, currentTcpPose, 6);
            return FairinoResult.Ok("Mock Doosan M1013 MoveL 완료");
        }

        /// <summary>
        /// 모든 동작을 정지합니다 (Mock: 즉시 성공).
        /// </summary>
        public FairinoResult StopMotion()
        {
            return FairinoResult.Ok("Mock Doosan M1013 비상정지 완료");
        }

        /// <summary>
        /// 버전 정보를 반환합니다 (Mock: 고정값).
        /// </summary>
        public FairinoResult<FairinoVersionInfo> GetVersion()
        {
            if (!IsConnected)
            {
                return FairinoResult<FairinoVersionInfo>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            var version = new FairinoVersionInfo("Mock-DoosanM1013-1.0.0", "SDK-Mock-1.0");
            return FairinoResult<FairinoVersionInfo>.Ok(version);
        }

        public FairinoResult<int> GetSafetyCode()
        {
            return !IsConnected
                ? FairinoResult<int>.Fail(-1, "연결되지 않은 상태입니다.")
                : FairinoResult<int>.Ok(0);
        }

        public FairinoResult<int> GetRealtimeStateSamplePeriod()
        {
            return !IsConnected
                ? FairinoResult<int>.Fail(-1, "연결되지 않은 상태입니다.")
                : FairinoResult<int>.Ok(stateSamplePeriodMs);
        }

        public FairinoResult SetRealtimeStateSamplePeriod(int periodMs)
        {
            if (!IsConnected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            stateSamplePeriodMs = System.Math.Max(10, periodMs);
            return FairinoResult.Ok("Mock Doosan M1013 상태 주기 설정 완료");
        }

        public FairinoResult ClearMotionQueue()
        {
            return !IsConnected
                ? FairinoResult.Fail(-1, "연결되지 않은 상태입니다.")
                : FairinoResult.Ok("Mock Doosan M1013 모션 큐 비움");
        }

        public FairinoResult SetMode(int mode)
        {
            if (!IsConnected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            currentMode = mode;
            return FairinoResult.Ok($"Mock Doosan M1013 모드 전환: {mode}");
        }
    }
}
