// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// macOS direct SDK 실패 시 사용하는 readback-only HTTP bridge 클라이언트입니다.
    /// </summary>
    public sealed class FairinoBridgeClient : FairinoReadbackOnlyClientBase
    {
        private readonly string baseUrl;
        private FairinoRobotState lastState = FairinoRobotState.Zero();
        private string sdkVersion = string.Empty;
        private bool connected;
        private int consecutiveReadFailures;
        private DateTime nextReadAttemptUtc = DateTime.MinValue;

        public FairinoBridgeClient(string bridgeUrl)
        {
            baseUrl = NormalizeBaseUrl(bridgeUrl);
        }

        public override bool IsConnected => connected;
        public override string ClientMode => "bridge";
        public override string SdkLoadStatus => "bridge";
        public override string SdkVersion => sdkVersion;
        public override string SdkRuntime => baseUrl;

        public override FairinoResult Connect(string ip, int port)
        {
            var body = JsonUtility.ToJson(new BridgeConnectRequest { ip = ip, port = port });
            var response = Send<BridgeBasicResponse>("POST", "/connect", body, 3000);
            if (!response.result.IsSuccess)
            {
                connected = false;
                return response.result;
            }

            connected = true;
            consecutiveReadFailures = 0;
            nextReadAttemptUtc = DateTime.MinValue;
            GetVersion();
            return FairinoResult.Ok($"FR5 연결됨 · bridge readback-only ({ip}:{port})");
        }

        public override FairinoResult Disconnect()
        {
            if (!connected)
            {
                return FairinoResult.Ok("bridge 연결 없음");
            }

            var response = Send<BridgeBasicResponse>("POST", "/disconnect", "{}", 3000);
            connected = false;
            return response.result.IsSuccess ? FairinoResult.Ok("bridge 연결 해제") : response.result;
        }

        public override FairinoResult<FairinoRobotState> ReadState()
        {
            if (!connected)
            {
                return FairinoResult<FairinoRobotState>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            var now = DateTime.UtcNow;
            if (now < nextReadAttemptUtc)
            {
                return FairinoResult<FairinoRobotState>.Fail(-82, "bridge readback 재시도 대기 중");
            }

            var response = Send<BridgeStateResponse>("GET", "/state", null, 500);
            if (!response.result.IsSuccess)
            {
                RegisterReadFailure();
                return FairinoResult<FairinoRobotState>.Fail(response.result.ErrorCode, response.result.Message);
            }

            var payload = response.value.state ?? response.value;
            lastState = new FairinoRobotState(
                NormalizeArray(payload.jointsDeg, 6),
                NormalizeArray(payload.tcpMmDeg, 6),
                robotMode: payload.mode,
                motionQueueLength: payload.motionQueueLength,
                safetyCode: payload.safetyCode,
                realtimeStateSamplePeriodMs: payload.realtimeStateSamplePeriodMs,
                mainErrorCode: payload.mainErrorCode,
                subErrorCode: payload.subErrorCode,
                toolId: payload.toolId,
                userId: payload.userId,
                isEmergencyStop: payload.emergencyStop,
                isCollisionDetected: payload.collisionDetected,
                isRobotEnabled: payload.enabled,
                isInDragTeach: payload.inDragTeach,
                isSafetyStop: payload.safetyStop);
            consecutiveReadFailures = 0;
            nextReadAttemptUtc = DateTime.MinValue;
            return FairinoResult<FairinoRobotState>.Ok(lastState, "현재 위치 읽기 완료 · 관절/TCP 값 수신 중");
        }

        public override FairinoResult<FairinoVersionInfo> GetVersion()
        {
            var response = Send<BridgeVersionResponse>("GET", "/version", null, 3000);
            if (!response.result.IsSuccess)
            {
                return FairinoResult<FairinoVersionInfo>.Fail(response.result.ErrorCode, response.result.Message);
            }

            var payload = response.value.version ?? response.value;
            sdkVersion = payload.sdkVersion ?? string.Empty;
            return FairinoResult<FairinoVersionInfo>.Ok(new FairinoVersionInfo(
                payload.firmwareVersion,
                payload.sdkVersion,
                payload.softwareVersion,
                payload.controllerVersion,
                payload.hardwareVersion));
        }

        public override FairinoResult<int> GetSafetyCode()
        {
            return FairinoResult<int>.Ok(lastState.SafetyCode);
        }

        public override FairinoResult<int> GetRealtimeStateSamplePeriod()
        {
            return FairinoResult<int>.Ok(lastState.RealtimeStateSamplePeriodMs);
        }

        public override FairinoResult<FairinoCoordContext> ReadCoordContext()
        {
            return FairinoResult<FairinoCoordContext>.Ok(new FairinoCoordContext(lastState.ToolId, lastState.UserId, new double[6], new double[6]));
        }

        public override FairinoResult<FairinoControllerFault> ReadControllerFault()
        {
            return FairinoResult<FairinoControllerFault>.Ok(new FairinoControllerFault(lastState.MainErrorCode, lastState.SubErrorCode, lastState.IsSafetyStop));
        }

        private static string NormalizeBaseUrl(string bridgeUrl)
        {
            var value = string.IsNullOrWhiteSpace(bridgeUrl) ? "http://127.0.0.1:5055" : bridgeUrl.Trim();
            return value.EndsWith("/", StringComparison.Ordinal) ? value.TrimEnd('/') : value;
        }

        private BridgeResponse<T> Send<T>(string method, string path, string body, int timeoutMs) where T : BridgeBasicResponse, new()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(baseUrl + path);
                request.Method = method;
                request.Timeout = timeoutMs;
                request.ReadWriteTimeout = timeoutMs;
                request.ContentType = "application/json";
                if (!string.IsNullOrEmpty(body))
                {
                    var bytes = Encoding.UTF8.GetBytes(body);
                    request.ContentLength = bytes.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream ?? Stream.Null, Encoding.UTF8))
                {
                    var json = reader.ReadToEnd();
                    var value = string.IsNullOrWhiteSpace(json)
                        ? new T()
                        : JsonUtility.FromJson<T>(json);
                    var result = ToResult(value);
                    return new BridgeResponse<T>(result, value);
                }
            }
            catch (WebException ex)
            {
                var message = ReadWebExceptionMessage(ex);
                return new BridgeResponse<T>(FairinoResult.Fail(-82, $"FR5 연결 실패: bridge 응답 실패 · {message}"), new T());
            }
            catch (Exception ex)
            {
                return new BridgeResponse<T>(FairinoResult.Fail(-82, $"FR5 연결 실패: bridge 호출 실패 · {ex.Message}"), new T());
            }
        }

        private static FairinoResult ToResult(BridgeBasicResponse response)
        {
            if (response == null)
            {
                return FairinoResult.Fail(-82, "bridge 응답이 비어 있다.");
            }

            var code = response.errorCode;
            if (!response.ok && code == 0)
            {
                code = -82;
            }

            return code == 0
                ? FairinoResult.Ok(string.IsNullOrWhiteSpace(response.message) ? "bridge OK" : response.message)
                : FairinoResult.Fail(code, string.IsNullOrWhiteSpace(response.message) ? "bridge 실패" : response.message);
        }

        private static string ReadWebExceptionMessage(WebException ex)
        {
            try
            {
                using (var stream = ex.Response?.GetResponseStream())
                using (var reader = stream != null ? new StreamReader(stream, Encoding.UTF8) : null)
                {
                    var body = reader?.ReadToEnd();
                    return string.IsNullOrWhiteSpace(body) ? ex.Message : body;
                }
            }
            catch
            {
                return ex.Message;
            }
        }

        private static double[] NormalizeArray(double[] values, int length)
        {
            var result = new double[length];
            if (values == null)
            {
                return result;
            }

            var copyLength = values.Length < length ? values.Length : length;
            Array.Copy(values, result, copyLength);
            return result;
        }

        private void RegisterReadFailure()
        {
            consecutiveReadFailures++;
            var backoffSeconds = consecutiveReadFailures >= 4
                ? 5.0
                : 0.5 * consecutiveReadFailures;
            nextReadAttemptUtc = DateTime.UtcNow.AddSeconds(backoffSeconds);
        }

        private readonly struct BridgeResponse<T>
        {
            public BridgeResponse(FairinoResult result, T value)
            {
                this.result = result;
                this.value = value;
            }

            public readonly FairinoResult result;
            public readonly T value;
        }

        [Serializable]
        private sealed class BridgeConnectRequest
        {
            public string ip;
            public int port;
        }

        [Serializable]
        public class BridgeBasicResponse
        {
            public bool ok = true;
            public int errorCode;
            public string message;
        }

        [Serializable]
        public sealed class BridgeStateResponse : BridgeBasicResponse
        {
            public BridgeStateResponse state;
            public double[] jointsDeg;
            public double[] tcpMmDeg;
            public int mode;
            public int motionQueueLength;
            public int safetyCode;
            public int realtimeStateSamplePeriodMs;
            public int mainErrorCode;
            public int subErrorCode;
            public int toolId;
            public int userId;
            public bool connected;
            public bool enabled;
            public bool emergencyStop;
            public bool collisionDetected;
            public bool inDragTeach;
            public bool safetyStop;
        }

        [Serializable]
        public sealed class BridgeVersionResponse : BridgeBasicResponse
        {
            public BridgeVersionResponse version;
            public string firmwareVersion;
            public string sdkVersion;
            public string softwareVersion;
            public string controllerVersion;
            public string hardwareVersion;
        }
    }
}
