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
        /// 메인 에러 코드입니다.
        /// </summary>
        public int MainErrorCode { get; }

        /// <summary>
        /// 서브 에러 코드입니다.
        /// </summary>
        public int SubErrorCode { get; }

        /// <summary>
        /// 현재 활성 tool ID입니다.
        /// </summary>
        public int ToolId { get; }

        /// <summary>
        /// 현재 활성 user/workobject ID입니다.
        /// </summary>
        public int UserId { get; }

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
        /// Drag teach 상태 여부입니다.
        /// </summary>
        public bool IsInDragTeach { get; }

        /// <summary>
        /// Safety stop 상태 여부입니다.
        /// </summary>
        public bool IsSafetyStop { get; }

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
            int mainErrorCode = 0,
            int subErrorCode = 0,
            int toolId = 0,
            int userId = 0,
            bool isEmergencyStop = false,
            bool isCollisionDetected = false,
            bool isRobotEnabled = false,
            bool isInDragTeach = false,
            bool isSafetyStop = false)
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
            MainErrorCode = mainErrorCode;
            SubErrorCode = subErrorCode;
            ToolId = toolId;
            UserId = userId;
            IsEmergencyStop = isEmergencyStop;
            IsCollisionDetected = isCollisionDetected;
            IsRobotEnabled = isRobotEnabled;
            IsInDragTeach = isInDragTeach;
            IsSafetyStop = isSafetyStop;
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
