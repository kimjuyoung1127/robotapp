// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Net.Sockets;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO direct SDK 클라이언트를 readback-only로 감싸는 어댑터입니다.
    /// </summary>
    public sealed class DirectReadbackFairinoClient : FairinoReadbackOnlyClientBase
    {
        private readonly LiveFairinoClient inner;
        private readonly FairinoSdkCompatibilityReport report;
        private string sdkVersion;

        public DirectReadbackFairinoClient(LiveFairinoClient innerClient, FairinoSdkCompatibilityReport compatibilityReport)
        {
            inner = innerClient ?? new LiveFairinoClient();
            report = compatibilityReport ?? FairinoSdkCompatibilityProbe.Probe();
            sdkVersion = report.sdkVersion ?? string.Empty;
        }

        public override bool IsConnected => inner.IsConnected;
        public override string ClientMode => "direct";
        public override string SdkLoadStatus => report.sdkLoadStatus ?? string.Empty;
        public override string SdkVersion => sdkVersion;
        public override string SdkRuntime => report.sdkRuntime ?? string.Empty;

        public override FairinoResult Connect(string ip, int port)
        {
            if (!report.IsDirectUsable)
            {
                return FairinoResult.Fail(-81, string.IsNullOrWhiteSpace(report.message)
                    ? "SDK 로딩 실패: direct SDK를 사용할 수 없다. bridge 필요."
                    : report.message);
            }

            if (!CanOpenTcpPort(ip, port, 1500))
            {
                return FairinoResult.Fail(-83, $"FR5 연결 실패: {ip}:{port} 포트 확인 실패. 맥북 Ethernet IP 대역, 랜선, 컨트롤러 전원을 먼저 확인해라.");
            }

            var result = inner.Connect(ip, port);
            return result.IsSuccess
                ? FairinoResult.Ok($"FR5 연결됨 · SDK 확인 완료 · readback-only direct ({ip}:{port})")
                : result;
        }

        public override FairinoResult Disconnect()
        {
            return inner.Disconnect();
        }

        public override FairinoResult<FairinoRobotState> ReadState()
        {
            var result = inner.ReadState();
            return result.IsSuccess
                ? FairinoResult<FairinoRobotState>.Ok(result.Value, "현재 위치 읽기 완료 · 관절/TCP 값 수신 중")
                : result;
        }

        public override FairinoResult<FairinoVersionInfo> GetVersion()
        {
            var result = inner.GetVersion();
            if (result.IsSuccess)
            {
                sdkVersion = result.Value.SdkVersion;
            }

            return result;
        }

        public override FairinoResult<int> GetSafetyCode()
        {
            return inner.GetSafetyCode();
        }

        public override FairinoResult<int> GetRealtimeStateSamplePeriod()
        {
            return inner.GetRealtimeStateSamplePeriod();
        }

        public override FairinoResult<FairinoCoordContext> ReadCoordContext()
        {
            return inner.ReadCoordContext();
        }

        public override FairinoResult<FairinoControllerFault> ReadControllerFault()
        {
            return inner.ReadControllerFault();
        }

        private static bool CanOpenTcpPort(string ip, int port, int timeoutMs)
        {
            if (string.IsNullOrWhiteSpace(ip) || port <= 0)
            {
                return false;
            }

            try
            {
                using (var tcpClient = new TcpClient())
                {
                    var asyncResult = tcpClient.BeginConnect(ip, port, null, null);
                    var waitMs = timeoutMs < 250 ? 250 : timeoutMs;
                    var connected = asyncResult.AsyncWaitHandle.WaitOne(waitMs);
                    if (!connected)
                    {
                        return false;
                    }

                    tcpClient.EndConnect(asyncResult);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
