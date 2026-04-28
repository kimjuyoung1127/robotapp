// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FR5 live readback 상태와 화면값 drift를 파일로 기록합니다.
    /// </summary>
    public sealed class Fr5LiveStateRecorder
    {
        public const double JointWarningDeg = 0.5;
        public const double JointDangerDeg = 2.0;
        public const double TcpWarningMm = 2.0;
        public const double TcpDangerMm = 10.0;
        public const double TcpRotWarningDeg = 1.0;
        public const double TcpRotDangerDeg = 5.0;

        private readonly FairinoConnectionService connectionService;
        private readonly Func<FairinoRobotState> displayStateProvider;
        private readonly Action<string> liveBlockedReasonSink;
        private readonly string rootDirectory;
        private readonly string sessionId;
        private string robotId = "FAIRINO_FR5";
        private string ip = string.Empty;
        private bool attached;

        public Fr5LiveStateRecorder(
            FairinoConnectionService service,
            Func<FairinoRobotState> screenStateProvider = null,
            Action<string> blockedReasonSink = null,
            string rootPath = null)
        {
            connectionService = service ?? throw new ArgumentNullException(nameof(service));
            displayStateProvider = screenStateProvider;
            liveBlockedReasonSink = blockedReasonSink;
            rootDirectory = string.IsNullOrWhiteSpace(rootPath) ? ResolveDefaultRootPath() : rootPath;
            sessionId = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        }

        public string SessionId => sessionId;
        public string RootDirectory => rootDirectory;

        public void SetConnectionInfo(string selectedRobotId, string ipAddress)
        {
            robotId = string.IsNullOrWhiteSpace(selectedRobotId) ? robotId : selectedRobotId;
            ip = ipAddress ?? string.Empty;
        }

        public void Attach()
        {
            if (attached)
            {
                return;
            }

            EnsureDirectories();
            connectionService.OnStateUpdated += HandleStateUpdated;
            connectionService.OnError += HandleError;
            connectionService.OnConnectionStateChanged += HandleConnectionChanged;
            connectionService.OnEnableStateChanged += HandleEnableChanged;
            connectionService.OnModeChanged += HandleModeChanged;
            attached = true;
            WriteEvent("recorder", 0, "FR5 live recorder attached");
        }

        public void Detach()
        {
            if (!attached)
            {
                return;
            }

            connectionService.OnStateUpdated -= HandleStateUpdated;
            connectionService.OnError -= HandleError;
            connectionService.OnConnectionStateChanged -= HandleConnectionChanged;
            connectionService.OnEnableStateChanged -= HandleEnableChanged;
            connectionService.OnModeChanged -= HandleModeChanged;
            attached = false;
            WriteEvent("recorder", 0, "FR5 live recorder detached");
        }

        public Fr5LiveDriftRecord RecordState(FairinoRobotState state)
        {
            var drift = CreateDriftRecord(state, displayStateProvider?.Invoke());
            try
            {
                EnsureDirectories();
                var stateRecord = CreateStateRecord(state);
                WriteLatest("latest-state.json", JsonUtility.ToJson(stateRecord, true));
                AppendSession("readback", JsonUtility.ToJson(stateRecord, false));
                WriteLatest("latest-drift.json", JsonUtility.ToJson(drift, true));
                liveBlockedReasonSink?.Invoke(drift.severity == "ok" ? string.Empty : drift.liveBlockedReason);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Fr5LiveStateRecorder] 기록 실패: {ex.Message}");
            }

            return drift;
        }

        private void HandleStateUpdated(FairinoRobotState state)
        {
            RecordState(state);
        }

        private void HandleError(FairinoResult result)
        {
            WriteEvent("error", result.ErrorCode, result.Message);
        }

        private void HandleConnectionChanged(bool connected)
        {
            WriteEvent("connection", connected ? 1 : 0, connected ? "FR5 연결됨" : "FR5 연결 해제");
        }

        private void HandleEnableChanged(bool enabled)
        {
            WriteEvent("enable", enabled ? 1 : 0, enabled ? "서보 ON" : "서보 OFF");
        }

        private void HandleModeChanged(bool isMockMode)
        {
            WriteEvent("mode", isMockMode ? 1 : 0, isMockMode ? "Mock 모드" : "Live 모드");
        }

        private Fr5LiveStateRecord CreateStateRecord(FairinoRobotState state)
        {
            var diagnostics = connectionService.Client as IFairinoLiveClientDiagnostics;
            return new Fr5LiveStateRecord
            {
                sessionId = sessionId,
                robotId = robotId,
                ip = ip,
                timestampUtc = DateTime.UtcNow.ToString("O"),
                connected = connectionService.Client?.IsConnected ?? false,
                enabled = connectionService.Client?.IsEnabled ?? false,
                mode = state.RobotMode,
                toolId = state.ToolId,
                userId = state.UserId,
                jointsDeg = Copy(state.JointPosDeg, 6),
                tcpMmDeg = Copy(state.TcpPose, 6),
                safety = BuildSafetySummary(state),
                fault = $"{state.MainErrorCode}/{state.SubErrorCode}",
                sdk = diagnostics?.SdkVersion ?? string.Empty,
                sdkLoadStatus = diagnostics?.SdkLoadStatus ?? string.Empty,
                sdkRuntime = diagnostics?.SdkRuntime ?? string.Empty,
                clientMode = diagnostics?.ClientMode ?? (connectionService.IsMockMode ? "mock" : "live"),
            };
        }

        private Fr5LiveDriftRecord CreateDriftRecord(FairinoRobotState liveState, FairinoRobotState? displayState)
        {
            var result = new Fr5LiveDriftRecord
            {
                sessionId = sessionId,
                timestampUtc = DateTime.UtcNow.ToString("O"),
                severity = "ok",
                liveBlockedReason = string.Empty,
            };

            if (!displayState.HasValue)
            {
                return result;
            }

            var screen = displayState.Value;
            result.maxJointDeg = MaxAbsDelta(liveState.JointPosDeg, screen.JointPosDeg, 6, 0);
            result.maxTcpMm = MaxAbsDelta(liveState.TcpPose, screen.TcpPose, 3, 0);
            result.maxTcpRotDeg = MaxAbsDelta(liveState.TcpPose, screen.TcpPose, 3, 3);

            if (result.maxJointDeg >= JointDangerDeg || result.maxTcpMm >= TcpDangerMm || result.maxTcpRotDeg >= TcpRotDangerDeg)
            {
                result.severity = "danger";
            }
            else if (result.maxJointDeg >= JointWarningDeg || result.maxTcpMm >= TcpWarningMm || result.maxTcpRotDeg >= TcpRotWarningDeg)
            {
                result.severity = "warning";
            }

            if (result.severity != "ok")
            {
                result.liveBlockedReason =
                    $"실제 위치와 화면 위치가 다름 · joint {result.maxJointDeg:0.###}deg / tcp {result.maxTcpMm:0.###}mm / rot {result.maxTcpRotDeg:0.###}deg · 실기 이동 차단됨";
            }

            return result;
        }

        private void WriteEvent(string kind, int code, string message)
        {
            try
            {
                EnsureDirectories();
                var record = new Fr5LiveEventRecord
                {
                    timestampUtc = DateTime.UtcNow.ToString("O"),
                    kind = kind,
                    code = code,
                    message = message ?? string.Empty,
                };
                AppendSession("events", JsonUtility.ToJson(record, false));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Fr5LiveStateRecorder] 이벤트 기록 실패: {ex.Message}");
            }
        }

        private void EnsureDirectories()
        {
            Directory.CreateDirectory(rootDirectory);
            Directory.CreateDirectory(Path.Combine(rootDirectory, "sessions"));
        }

        private void WriteLatest(string fileName, string json)
        {
            var path = Path.Combine(rootDirectory, fileName);
            var temp = path + ".tmp";
            File.WriteAllText(temp, json, Encoding.UTF8);
            if (!File.Exists(path))
            {
                File.Move(temp, path);
                return;
            }

            try
            {
                File.Replace(temp, path, null);
            }
            catch
            {
                File.Delete(path);
                File.Move(temp, path);
            }
        }

        private void AppendSession(string suffix, string json)
        {
            var path = Path.Combine(rootDirectory, "sessions", $"{sessionId}-{suffix}.ndjson");
            File.AppendAllText(path, json + Environment.NewLine, Encoding.UTF8);
        }

        private static string ResolveDefaultRootPath()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            return Path.Combine(projectRoot, "Artifacts", "live", "fr5");
        }

        private static string BuildSafetySummary(FairinoRobotState state)
        {
            if (state.IsEmergencyStop)
            {
                return "emergency-stop";
            }

            if (state.IsSafetyStop)
            {
                return "safety-stop";
            }

            return state.SafetyCode == 0 ? "normal" : $"code-{state.SafetyCode}";
        }

        private static double[] Copy(double[] source, int length)
        {
            var result = new double[length];
            if (source != null)
            {
                var copyLength = source.Length < length ? source.Length : length;
                Array.Copy(source, result, copyLength);
            }

            return result;
        }

        private static double MaxAbsDelta(double[] a, double[] b, int count, int offset)
        {
            var max = 0d;
            for (var i = 0; i < count; i++)
            {
                var index = i + offset;
                var left = a != null && index < a.Length ? a[index] : 0d;
                var right = b != null && index < b.Length ? b[index] : 0d;
                var delta = left - right;
                if (delta < 0d)
                {
                    delta = -delta;
                }

                if (delta > max)
                {
                    max = delta;
                }
            }

            return max;
        }
    }

    [Serializable]
    public sealed class Fr5LiveStateRecord
    {
        public string sessionId;
        public string robotId;
        public string ip;
        public string timestampUtc;
        public bool connected;
        public bool enabled;
        public int mode;
        public int toolId;
        public int userId;
        public double[] jointsDeg;
        public double[] tcpMmDeg;
        public string safety;
        public string fault;
        public string sdk;
        public string sdkLoadStatus;
        public string sdkRuntime;
        public string clientMode;
    }

    [Serializable]
    public sealed class Fr5LiveDriftRecord
    {
        public string sessionId;
        public string timestampUtc;
        public string severity;
        public double maxJointDeg;
        public double maxTcpMm;
        public double maxTcpRotDeg;
        public string liveBlockedReason;
    }

    [Serializable]
    public sealed class Fr5LiveEventRecord
    {
        public string timestampUtc;
        public string kind;
        public int code;
        public string message;
    }
}
