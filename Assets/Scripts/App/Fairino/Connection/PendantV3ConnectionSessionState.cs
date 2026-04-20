// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// Pendant V3가 소비하는 연결/재연결 세션 상태 스냅샷입니다.
    /// </summary>
    public readonly struct PendantV3ConnectionSessionState : IEquatable<PendantV3ConnectionSessionState>
    {
        public PendantV3ConnectionSessionState(
            PendantV3ConnectionDisplayKind displayKind,
            bool isConnected,
            bool isEnabled,
            bool isMockMode,
            bool isLiveArmActive,
            bool actualMoveAllowed,
            bool hasSynced,
            int toolId,
            int userId,
            int safetyCode,
            string connectionSummary,
            string modeSummary,
            string servoSummary,
            string motionSummary,
            string liveArmSummary,
            string actualMoveBlockReason,
            string safetySummary,
            string faultSummary,
            string ipAddress,
            int reconnectAttempt,
            int reconnectAttemptMax,
            float reconnectSecondsUntilRetry,
            bool reconnectActive,
            bool reconnectFailed,
            string reconnectFailureSummary,
            string lastErrorSummary)
        {
            DisplayKind = displayKind;
            IsConnected = isConnected;
            IsEnabled = isEnabled;
            IsMockMode = isMockMode;
            IsLiveArmActive = isLiveArmActive;
            ActualMoveAllowed = actualMoveAllowed;
            HasSynced = hasSynced;
            ToolId = toolId;
            UserId = userId;
            SafetyCode = safetyCode;
            ConnectionSummary = connectionSummary ?? string.Empty;
            ModeSummary = modeSummary ?? string.Empty;
            ServoSummary = servoSummary ?? string.Empty;
            MotionSummary = motionSummary ?? string.Empty;
            LiveArmSummary = liveArmSummary ?? string.Empty;
            ActualMoveBlockReason = actualMoveBlockReason ?? string.Empty;
            SafetySummary = safetySummary ?? string.Empty;
            FaultSummary = faultSummary ?? string.Empty;
            IpAddress = ipAddress ?? string.Empty;
            ReconnectAttempt = reconnectAttempt;
            ReconnectAttemptMax = reconnectAttemptMax;
            ReconnectSecondsUntilRetry = reconnectSecondsUntilRetry;
            ReconnectActive = reconnectActive;
            ReconnectFailed = reconnectFailed;
            ReconnectFailureSummary = reconnectFailureSummary ?? string.Empty;
            LastErrorSummary = lastErrorSummary ?? string.Empty;
        }

        public PendantV3ConnectionDisplayKind DisplayKind { get; }
        public bool IsConnected { get; }
        public bool IsEnabled { get; }
        public bool IsMockMode { get; }
        public bool IsLiveArmActive { get; }
        public bool ActualMoveAllowed { get; }
        public bool HasSynced { get; }
        public int ToolId { get; }
        public int UserId { get; }
        public int SafetyCode { get; }
        public string ConnectionSummary { get; }
        public string ModeSummary { get; }
        public string ServoSummary { get; }
        public string MotionSummary { get; }
        public string LiveArmSummary { get; }
        public string ActualMoveBlockReason { get; }
        public string SafetySummary { get; }
        public string FaultSummary { get; }
        public string IpAddress { get; }
        public int ReconnectAttempt { get; }
        public int ReconnectAttemptMax { get; }
        public float ReconnectSecondsUntilRetry { get; }
        public bool ReconnectActive { get; }
        public bool ReconnectFailed { get; }
        public string ReconnectFailureSummary { get; }
        public string LastErrorSummary { get; }

        public string ToDebugSummary()
        {
            return $"kind={DisplayKind}; connected={IsConnected}; enabled={IsEnabled}; mock={IsMockMode}; liveArm={IsLiveArmActive}; actualMove={ActualMoveAllowed}; synced={HasSynced}; tool={ToolId}; user={UserId}; safetyCode={SafetyCode}; reconnect={ReconnectActive}; attempt={ReconnectAttempt}/{ReconnectAttemptMax}; retryIn={ReconnectSecondsUntilRetry:0.0}; reconnectFailed={ReconnectFailed}; fault={FaultSummary}; safety={SafetySummary}; armSummary={LiveArmSummary}; block={ActualMoveBlockReason}; error={LastErrorSummary}";
        }

        public bool Equals(PendantV3ConnectionSessionState other)
        {
            return DisplayKind == other.DisplayKind
                && IsConnected == other.IsConnected
                && IsEnabled == other.IsEnabled
                && IsMockMode == other.IsMockMode
                && IsLiveArmActive == other.IsLiveArmActive
                && ActualMoveAllowed == other.ActualMoveAllowed
                && HasSynced == other.HasSynced
                && ToolId == other.ToolId
                && UserId == other.UserId
                && SafetyCode == other.SafetyCode
                && string.Equals(ConnectionSummary, other.ConnectionSummary, StringComparison.Ordinal)
                && string.Equals(ModeSummary, other.ModeSummary, StringComparison.Ordinal)
                && string.Equals(ServoSummary, other.ServoSummary, StringComparison.Ordinal)
                && string.Equals(MotionSummary, other.MotionSummary, StringComparison.Ordinal)
                && string.Equals(LiveArmSummary, other.LiveArmSummary, StringComparison.Ordinal)
                && string.Equals(ActualMoveBlockReason, other.ActualMoveBlockReason, StringComparison.Ordinal)
                && string.Equals(SafetySummary, other.SafetySummary, StringComparison.Ordinal)
                && string.Equals(FaultSummary, other.FaultSummary, StringComparison.Ordinal)
                && string.Equals(IpAddress, other.IpAddress, StringComparison.Ordinal)
                && ReconnectAttempt == other.ReconnectAttempt
                && ReconnectAttemptMax == other.ReconnectAttemptMax
                && System.Math.Abs(ReconnectSecondsUntilRetry - other.ReconnectSecondsUntilRetry) < 0.0001f
                && ReconnectActive == other.ReconnectActive
                && ReconnectFailed == other.ReconnectFailed
                && string.Equals(ReconnectFailureSummary, other.ReconnectFailureSummary, StringComparison.Ordinal)
                && string.Equals(LastErrorSummary, other.LastErrorSummary, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is PendantV3ConnectionSessionState other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(DisplayKind);
            hash.Add(IsConnected);
            hash.Add(IsEnabled);
            hash.Add(IsMockMode);
            hash.Add(IsLiveArmActive);
            hash.Add(ActualMoveAllowed);
            hash.Add(HasSynced);
            hash.Add(ToolId);
            hash.Add(UserId);
            hash.Add(SafetyCode);
            hash.Add(ConnectionSummary);
            hash.Add(ModeSummary);
            hash.Add(ServoSummary);
            hash.Add(MotionSummary);
            hash.Add(LiveArmSummary);
            hash.Add(ActualMoveBlockReason);
            hash.Add(SafetySummary);
            hash.Add(FaultSummary);
            hash.Add(IpAddress);
            hash.Add(ReconnectAttempt);
            hash.Add(ReconnectAttemptMax);
            hash.Add(System.Math.Round(ReconnectSecondsUntilRetry, 4));
            hash.Add(ReconnectActive);
            hash.Add(ReconnectFailed);
            hash.Add(ReconnectFailureSummary);
            hash.Add(LastErrorSummary);
            return hash.ToHashCode();
        }

        public static PendantV3ConnectionSessionState DefaultDisconnected()
        {
            return new PendantV3ConnectionSessionState(
                PendantV3ConnectionDisplayKind.Disconnected,
                false,
                false,
                true,
                false,
                false,
                false,
                0,
                0,
                0,
                "미연결",
                "--",
                "--",
                "대기",
                "Disarmed",
                "연결부터 다시 확인해라.",
                "--",
                "없음",
                string.Empty,
                0,
                10,
                0f,
                false,
                false,
                string.Empty,
                string.Empty);
        }
    }

    public enum PendantV3ConnectionDisplayKind
    {
        Disconnected,
        ConnectedServoOff,
        ConnectedUnsynced,
        ReadyToJog,
        Fault,
        AutoReconnect,
    }
}
