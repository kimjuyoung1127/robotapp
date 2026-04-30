// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO live smoke 점검을 direct/bridge 공통 경로로 실행합니다.
    /// </summary>
    public static class FairinoLiveSmokeRunner
    {
        public const string DefaultIp = "192.168.57.2";
        public const int DefaultPort = 8080;

        public static string RunFromEnvironment()
        {
            return Run(ResolveIpFromEnvironment(), ResolvePortFromEnvironment());
        }

        public static string Run(string ip, int port, IFairinoRobotClient client = null)
        {
            var smokeClient = client ?? FairinoRobotClientFactory.CreateLive(new FairinoErrorTranslator());
            var diagnostics = smokeClient as IFairinoLiveClientDiagnostics;
            var clientMode = diagnostics?.ClientMode ?? "live";
            var sdkLoadStatus = diagnostics?.SdkLoadStatus ?? "-";
            var sdkRuntime = diagnostics?.SdkRuntime ?? "-";

            var connect = smokeClient.Connect(ip, port);
            if (!connect.IsSuccess)
            {
                return $"[FAIRINO LIVE SMOKE] CONNECT_FAIL ip={ip} port={port} client={clientMode} sdkLoadStatus={sdkLoadStatus} sdkRuntime={sdkRuntime} code={connect.ErrorCode} msg={connect.Message}";
            }

            try
            {
                var version = smokeClient.GetVersion();
                var state = smokeClient.ReadState();

                var versionText = version.IsSuccess
                    ? $"fw={version.Value.FirmwareVersion} sdk={version.Value.SdkVersion}"
                    : $"version_fail code={version.ErrorCode} msg={version.Message}";

                var stateText = state.IsSuccess
                    ? $"joints=[{string.Join(", ", state.Value.JointPosDeg)}] tcp=[{string.Join(", ", state.Value.TcpPose)}]"
                    : $"state_fail code={state.ErrorCode} msg={state.Message}";

                return $"[FAIRINO LIVE SMOKE] CONNECT_OK ip={ip} port={port} client={clientMode} sdkLoadStatus={sdkLoadStatus} sdkRuntime={sdkRuntime} {versionText} {stateText}";
            }
            finally
            {
                smokeClient.Disconnect();
            }
        }

        private static string ResolveIpFromEnvironment()
        {
            var ip = Environment.GetEnvironmentVariable("FAIRINO_IP");
            return string.IsNullOrWhiteSpace(ip) ? DefaultIp : ip.Trim();
        }

        private static int ResolvePortFromEnvironment()
        {
            var portRaw = Environment.GetEnvironmentVariable("FAIRINO_PORT");
            return !string.IsNullOrWhiteSpace(portRaw) && int.TryParse(portRaw, out var parsedPort)
                ? parsedPort
                : DefaultPort;
        }
    }
}
