// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FR5 Live 클라이언트 생성 정책을 한 곳에 모읍니다.
    /// </summary>
    public static class FairinoRobotClientFactory
    {
        public const string BridgeUrlEnvironmentVariable = "FAIRINO_BRIDGE_URL";

        public static IFairinoRobotClient CreateLive(FairinoErrorTranslator translator = null)
        {
            var bridgeUrl = Environment.GetEnvironmentVariable(BridgeUrlEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(bridgeUrl))
            {
                return new FairinoBridgeClient(bridgeUrl.Trim());
            }

            var report = FairinoSdkCompatibilityProbe.Probe();
            if (!report.IsDirectUsable)
            {
                return new FairinoUnavailableClient(report);
            }

            return new DirectReadbackFairinoClient(new LiveFairinoClient(translator), report);
        }
    }
}
