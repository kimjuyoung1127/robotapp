// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO 로봇 통신 클라이언트 인터페이스입니다.
    /// Mock과 Live 구현을 교체할 수 있도록 추상화합니다.
    /// </summary>
    public interface IFairinoRobotClient
    {
        /// <summary>
        /// 현재 연결 상태입니다.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 로봇 활성화(서보 ON) 상태입니다.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// 로봇에 연결합니다.
        /// </summary>
        FairinoResult Connect(string ip, int port);

        /// <summary>
        /// 연결을 해제합니다.
        /// </summary>
        FairinoResult Disconnect();

        /// <summary>
        /// 로봇을 활성화(서보 ON)합니다.
        /// </summary>
        FairinoResult Enable();

        /// <summary>
        /// 로봇을 비활성화(서보 OFF)합니다.
        /// </summary>
        FairinoResult Disable();

        /// <summary>
        /// 관절 공간 이동 명령을 전송합니다 (MoveJ).
        /// </summary>
        /// <param name="jointPosDeg">6축 목표 각도 (도 단위).</param>
        /// <param name="speedPercent">속도 비율 (0~100).</param>
        /// <param name="accPercent">가속도 비율 (0~100).</param>
        FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent);

        /// <summary>
        /// 서보 관절 이동 명령을 전송합니다 (ServoJ).
        /// </summary>
        /// <param name="jointPosDeg">6축 목표 각도 (도 단위).</param>
        FairinoResult ServoJ(double[] jointPosDeg);

        /// <summary>
        /// 현재 로봇 상태를 읽어옵니다.
        /// </summary>
        FairinoResult<FairinoRobotState> ReadState();

        /// <summary>
        /// 직교 공간 직선 이동 명령을 전송합니다 (MoveL).
        /// </summary>
        /// <param name="tcpPose">목표 TCP 포즈 [X, Y, Z, Rx, Ry, Rz].</param>
        /// <param name="speedPercent">속도 비율 (0~100).</param>
        /// <param name="accPercent">가속도 비율 (0~100).</param>
        FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent);

        /// <summary>
        /// 모든 동작을 즉시 정지합니다.
        /// </summary>
        FairinoResult StopMotion();

        /// <summary>
        /// 로봇 버전 정보를 읽어옵니다.
        /// </summary>
        FairinoResult<FairinoVersionInfo> GetVersion();
    }
}
