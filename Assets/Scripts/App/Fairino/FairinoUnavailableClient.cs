// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// SDK direct 경로가 준비되지 않았을 때 명확한 실패 메시지를 반환하는 클라이언트입니다.
    /// </summary>
    public sealed class FairinoUnavailableClient : FairinoReadbackOnlyClientBase
    {
        private readonly FairinoSdkCompatibilityReport report;

        public FairinoUnavailableClient(FairinoSdkCompatibilityReport compatibilityReport)
        {
            report = compatibilityReport ?? FairinoSdkCompatibilityProbe.Probe();
        }

        public override bool IsConnected => false;
        public override string ClientMode => "direct-unavailable";
        public override string SdkLoadStatus => report.sdkLoadStatus ?? "sdk-load-failed";
        public override string SdkVersion => report.sdkVersion ?? string.Empty;
        public override string SdkRuntime => report.sdkRuntime ?? string.Empty;

        public override FairinoResult Connect(string ip, int port)
        {
            var message = string.IsNullOrWhiteSpace(report.message)
                ? "SDK 로딩 실패: macOS direct 확인 실패, bridge 필요. 현재 readback-only 유지."
                : $"{report.message} 현재 readback-only 유지.";
            return FairinoResult.Fail(-81, message);
        }

        public override FairinoResult Disconnect()
        {
            return FairinoResult.Ok("연결 없음");
        }

        public override FairinoResult<FairinoRobotState> ReadState()
        {
            return FairinoResult<FairinoRobotState>.Fail(-81, "현재 위치 읽기 실패: SDK 로딩 실패, bridge 필요.");
        }

        public override FairinoResult<FairinoVersionInfo> GetVersion()
        {
            return FairinoResult<FairinoVersionInfo>.Fail(-81, "SDK 확인 실패: bridge 필요.");
        }
    }
}
