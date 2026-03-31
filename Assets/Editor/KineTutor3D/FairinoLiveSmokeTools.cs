// Folder: Editor - Unity Editor authoring, QA, and packaging tools.
// FAIRINO 라이브 연결 스모크 점검용 에디터 도구입니다.
#if UNITY_EDITOR
using System;
using KineTutor3D.App.Fairino;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor
{
    public static class FairinoLiveSmokeTools
    {
        private const string DefaultIp = "192.168.58.2";
        private const int DefaultPort = 8080;

        [MenuItem("KineTutor3D/RobotControl/Run FAIRINO Live Smoke Test", priority = 170)]
        public static void RunMenu()
        {
            Debug.Log(RunSmoke());
        }

        public static string RunSmoke()
        {
            var ip = Environment.GetEnvironmentVariable("FAIRINO_IP");
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = DefaultIp;
            }

            var port = DefaultPort;
            var portRaw = Environment.GetEnvironmentVariable("FAIRINO_PORT");
            if (!string.IsNullOrWhiteSpace(portRaw) && int.TryParse(portRaw, out var parsedPort))
            {
                port = parsedPort;
            }

            var client = new LiveFairinoClient(new FairinoErrorTranslator());
            var connect = client.Connect(ip, port);
            if (!connect.IsSuccess)
            {
                return $"[FAIRINO LIVE SMOKE] CONNECT_FAIL ip={ip} port={port} code={connect.ErrorCode} msg={connect.Message}";
            }

            try
            {
                var version = client.GetVersion();
                var state = client.ReadState();

                var versionText = version.IsSuccess
                    ? $"fw={version.Value.FirmwareVersion} sdk={version.Value.SdkVersion}"
                    : $"version_fail code={version.ErrorCode} msg={version.Message}";

                var stateText = state.IsSuccess
                    ? $"joints=[{string.Join(", ", state.Value.JointPosDeg)}] tcp=[{string.Join(", ", state.Value.TcpPose)}]"
                    : $"state_fail code={state.ErrorCode} msg={state.Message}";

                return $"[FAIRINO LIVE SMOKE] CONNECT_OK ip={ip} port={port} {versionText} {stateText}";
            }
            finally
            {
                client.Disconnect();
            }
        }
    }
}
#endif
