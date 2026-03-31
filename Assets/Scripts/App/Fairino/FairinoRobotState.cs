// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO 로봇의 현재 상태를 표현하는 불변 구조체입니다.
    /// </summary>
    public readonly struct FairinoRobotState
    {
        /// <summary>
        /// 현재 관절 각도 (도 단위, 6개)입니다.
        /// </summary>
        public double[] JointPosDeg { get; }

        /// <summary>
        /// 현재 TCP 포즈 (X, Y, Z, Rx, Ry, Rz)입니다.
        /// </summary>
        public double[] TcpPose { get; }

        /// <summary>
        /// 컨트롤러 보고 robot mode 값입니다.
        /// </summary>
        public int RobotMode { get; }

        /// <summary>
        /// 모션 큐 길이입니다.
        /// </summary>
        public int MotionQueueLength { get; }

        /// <summary>
        /// safety code 값입니다.
        /// </summary>
        public int SafetyCode { get; }

        /// <summary>
        /// 실시간 상태 주기(ms)입니다.
        /// </summary>
        public int RealtimeStateSamplePeriodMs { get; }

        /// <summary>
        /// Emergency stop 상태 여부입니다.
        /// </summary>
        public bool IsEmergencyStop { get; }

        /// <summary>
        /// 충돌 감지 상태 여부입니다.
        /// </summary>
        public bool IsCollisionDetected { get; }

        /// <summary>
        /// 로봇 enable 상태 여부입니다.
        /// </summary>
        public bool IsRobotEnabled { get; }

        /// <summary>
        /// 로봇 상태를 생성합니다.
        /// </summary>
        public FairinoRobotState(
            double[] jointPosDeg,
            double[] tcpPose,
            int robotMode = 0,
            int motionQueueLength = 0,
            int safetyCode = 0,
            int realtimeStateSamplePeriodMs = 0,
            bool isEmergencyStop = false,
            bool isCollisionDetected = false,
            bool isRobotEnabled = false)
        {
            JointPosDeg = jointPosDeg != null
                ? (double[])jointPosDeg.Clone()
                : new double[6];

            TcpPose = tcpPose != null
                ? (double[])tcpPose.Clone()
                : new double[6];

            RobotMode = robotMode;
            MotionQueueLength = motionQueueLength;
            SafetyCode = safetyCode;
            RealtimeStateSamplePeriodMs = realtimeStateSamplePeriodMs;
            IsEmergencyStop = isEmergencyStop;
            IsCollisionDetected = isCollisionDetected;
            IsRobotEnabled = isRobotEnabled;
        }

        /// <summary>
        /// 6축 영점 상태를 반환합니다.
        /// </summary>
        public static FairinoRobotState Zero()
        {
            return new FairinoRobotState(new double[6], new double[6]);
        }
    }
}
