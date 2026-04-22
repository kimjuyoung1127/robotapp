// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 실기기 수동 이동 readback을 Mock 상태 갱신으로 재현합니다.
    /// </summary>
    public sealed class ManualReadbackTeachingProbe
    {
        private readonly FairinoConnectionService connectionService;

        public ManualReadbackTeachingProbe(FairinoConnectionService connectionService)
        {
            this.connectionService = connectionService;
        }

        public FairinoResult<FairinoRobotState> SimulateManualMove(double[] jointsDeg, double[] tcpMm)
        {
            if (connectionService == null)
            {
                return FairinoResult<FairinoRobotState>.Fail(-80, "connection service missing");
            }

            return connectionService.SimulateExternalReadbackForDebug(jointsDeg, tcpMm);
        }
    }
}
