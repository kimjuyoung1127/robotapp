// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 실기 연결을 readback-only로 잠그는 공통 클라이언트 베이스입니다.
    /// </summary>
    public abstract class FairinoReadbackOnlyClientBase : IFairinoRobotClient, IFairinoLiveClientDiagnostics
    {
        protected const string ReadbackOnlyMessage = "실기 이동 차단됨: 현재 FR5 Mac 연결은 readback-only라 Enable/Move/IO/Gripper 명령을 보내지 않는다.";

        public abstract bool IsConnected { get; }
        public virtual bool IsEnabled => false;
        public abstract string ClientMode { get; }
        public abstract string SdkLoadStatus { get; }
        public abstract string SdkVersion { get; }
        public abstract string SdkRuntime { get; }
        public bool IsReadbackOnly => true;

        public abstract FairinoResult Connect(string ip, int port);
        public abstract FairinoResult Disconnect();
        public abstract FairinoResult<FairinoRobotState> ReadState();
        public abstract FairinoResult<FairinoVersionInfo> GetVersion();

        public virtual FairinoResult Enable()
        {
            return Blocked();
        }

        public virtual FairinoResult Disable()
        {
            return Blocked();
        }

        public virtual FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent)
        {
            return Blocked();
        }

        public virtual FairinoResult ServoJ(double[] jointPosDeg)
        {
            return Blocked();
        }

        public virtual FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent)
        {
            return Blocked();
        }

        public virtual FairinoResult StopMotion()
        {
            return Blocked();
        }

        public virtual FairinoResult<int> GetSafetyCode()
        {
            return FairinoResult<int>.Fail(-80, "readback-only safety code를 아직 읽지 못했다.");
        }

        public virtual FairinoResult<int> GetRealtimeStateSamplePeriod()
        {
            return FairinoResult<int>.Fail(-80, "readback-only 상태 주기를 아직 읽지 못했다.");
        }

        public virtual FairinoResult SetRealtimeStateSamplePeriod(int periodMs)
        {
            return Blocked();
        }

        public virtual FairinoResult ClearMotionQueue()
        {
            return Blocked();
        }

        public virtual FairinoResult SetMode(int mode)
        {
            return Blocked();
        }

        public virtual FairinoResult SetReconnect(bool enable, int timeoutMs, int periodMs)
        {
            return Blocked();
        }

        public virtual FairinoResult ExitDragTeach()
        {
            return Blocked();
        }

        public virtual FairinoResult EnsureAutoMode()
        {
            return Blocked();
        }

        public virtual FairinoResult<FairinoCoordContext> ReadCoordContext()
        {
            return FairinoResult<FairinoCoordContext>.Ok(FairinoCoordContext.Default());
        }

        public virtual FairinoResult<FairinoControllerFault> ReadControllerFault()
        {
            return FairinoResult<FairinoControllerFault>.Ok(FairinoControllerFault.None());
        }

        public virtual FairinoResult ResetErrors()
        {
            return Blocked();
        }

        public virtual FairinoResult<FairinoGripperCapability> ProbeGripperCapability()
        {
            return FairinoResult<FairinoGripperCapability>.Ok(default(FairinoGripperCapability), "readback-only: gripper 실행 차단");
        }

        public virtual FairinoResult<FairinoGripperStatus> ReadGripperStatus()
        {
            return FairinoResult<FairinoGripperStatus>.Fail(-80, "readback-only gripper 상태 읽기를 아직 지원하지 않는다.");
        }

        public virtual FairinoResult ConfigureGripper(FairinoGripperProfile profile)
        {
            return Blocked();
        }

        public virtual FairinoResult ActivateGripper(FairinoGripperProfile profile, bool activate)
        {
            return Blocked();
        }

        public virtual FairinoResult MoveGripper(FairinoGripperCommand command)
        {
            return Blocked();
        }

        protected static FairinoResult Blocked()
        {
            return FairinoResult.Fail(-80, ReadbackOnlyMessage);
        }
    }
}
