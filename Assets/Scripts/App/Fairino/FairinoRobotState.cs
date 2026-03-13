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
        /// 로봇 상태를 생성합니다.
        /// </summary>
        public FairinoRobotState(double[] jointPosDeg, double[] tcpPose)
        {
            JointPosDeg = jointPosDeg != null
                ? (double[])jointPosDeg.Clone()
                : new double[6];

            TcpPose = tcpPose != null
                ? (double[])tcpPose.Clone()
                : new double[6];
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
