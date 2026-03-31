// Folder: UI - HUD/view components only; no kinematics logic.
namespace KineTutor3D.UI
{
    /// <summary>
    /// RobotControl 진단 서랍에서 표시할 hand input 상태 스냅샷입니다.
    /// </summary>
    public sealed class HandInputDiagnosticsState
    {
        public string Mode { get; private set; } = "Idle";
        public bool IsConfigured { get; private set; }
        public bool IsListening { get; private set; }
        public int ListenPort { get; private set; }
        public string AllowedSenderIp { get; private set; } = "Any";
        public string SenderIp { get; private set; } = string.Empty;
        public float TimeoutSeconds { get; private set; }
        public int Sequence { get; private set; }
        public bool IsTracked { get; private set; }
        public float HandX { get; private set; }
        public float HandY { get; private set; }
        public float Pinch { get; private set; }
        public string SourceId { get; private set; } = string.Empty;

        public void SetUnconfigured()
        {
            Mode = "Unconfigured";
            IsConfigured = false;
            IsListening = false;
            ListenPort = 0;
            AllowedSenderIp = "Any";
            SenderIp = string.Empty;
            TimeoutSeconds = 0f;
            Sequence = 0;
            IsTracked = false;
            HandX = 0f;
            HandY = 0f;
            Pinch = 0f;
            SourceId = string.Empty;
        }

        public void SetListening(int listenPort, string allowedSenderIp, bool isListening)
        {
            Mode = "Listening";
            IsConfigured = true;
            IsListening = isListening;
            ListenPort = listenPort;
            AllowedSenderIp = string.IsNullOrWhiteSpace(allowedSenderIp) ? "Any" : allowedSenderIp;
            SenderIp = string.Empty;
            TimeoutSeconds = 0f;
            Sequence = 0;
            IsTracked = false;
            HandX = 0f;
            HandY = 0f;
            Pinch = 0f;
            SourceId = string.Empty;
        }

        public void SetFresh(
            int listenPort,
            string allowedSenderIp,
            float timeoutSeconds,
            string senderIp,
            int sequence,
            bool tracked,
            float handX,
            float handY,
            float pinch,
            string sourceId)
        {
            Mode = "Fresh";
            IsConfigured = true;
            IsListening = true;
            ListenPort = listenPort;
            AllowedSenderIp = string.IsNullOrWhiteSpace(allowedSenderIp) ? "Any" : allowedSenderIp;
            TimeoutSeconds = timeoutSeconds;
            SenderIp = senderIp ?? string.Empty;
            Sequence = sequence;
            IsTracked = tracked;
            HandX = handX;
            HandY = handY;
            Pinch = pinch;
            SourceId = sourceId ?? string.Empty;
        }

        public void SetStale(
            int listenPort,
            string allowedSenderIp,
            float timeoutSeconds,
            string senderIp,
            int sequence,
            bool tracked,
            float handX,
            float handY,
            float pinch,
            string sourceId)
        {
            Mode = "Stale";
            IsConfigured = true;
            IsListening = true;
            ListenPort = listenPort;
            AllowedSenderIp = string.IsNullOrWhiteSpace(allowedSenderIp) ? "Any" : allowedSenderIp;
            TimeoutSeconds = timeoutSeconds;
            SenderIp = senderIp ?? string.Empty;
            Sequence = sequence;
            IsTracked = tracked;
            HandX = handX;
            HandY = handY;
            Pinch = pinch;
            SourceId = sourceId ?? string.Empty;
        }
    }
}
