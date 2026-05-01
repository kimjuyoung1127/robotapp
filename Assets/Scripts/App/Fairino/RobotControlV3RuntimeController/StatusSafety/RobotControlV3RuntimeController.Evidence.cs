// Folder: StatusSafety - live evidence freshness, tool/user/coord truth, and readback probe helpers for diagnostics.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KineTutor3D.Math;
using KineTutor3D.UI.RobotControlV3;
using KineTutor3D.Visualization;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    // Handles latest-state/latest-drift evidence reads, session freshness, and probe summaries used by diagnostics.
    // Gate execution policy stays in LiveSafety and panel-specific operator paths stay in their panel partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string RefreshLiveEvidenceForDebug()
        {
            if (!initialized)
            {
                TryInitialize();
            }

            EnsureRuntimeHelpers();
            liveStateRecorder?.SetConnectionInfo(templateDefinition.RobotId, config.defaultIp);
            var result = connectionService.SyncCurrentState();
            if (!result.IsSuccess)
            {
                RefreshSnapshot();
                return $"[LiveEvidence] {result.Message}";
            }

            currentState = result.Value;
            hasCurrentPositionReadComplete = true;
            templateDefinition.PosePresetProvider?.UpdateCurrent(result.Value.JointPosDeg);
            if (liveStateRecorder == null)
            {
                RefreshSnapshot();
                return "[LiveEvidence] recorder missing";
            }

            liveStateRecorder.RecordState(result.Value);
            ApplyVisualState();
            RefreshSnapshot();
            return $"[LiveEvidence] recorded tool={result.Value.ToolId:00} user={result.Value.UserId:00} coord={snapshot.CoordSystem}";
        }


        public string GetLiveEvidenceGateSummaryForDebug()
        {
            var evidence = ResolveTinyMoveJEvidenceGateState();
            return $"connected={connectionService?.Client.IsConnected ?? false}; enabled={connectionService?.Client.IsEnabled ?? false}; tool={evidence.ToolId}; user={evidence.UserId}; coord={evidence.CoordSystem}; coordResolved={evidence.HasExplicitCoordSystem}; stateFresh={evidence.StateEvidenceFresh}; driftFresh={evidence.DriftEvidenceFresh}; driftPassed={evidence.DriftPassed}; sessionMatch={evidence.MatchesCurrentSession}; placeholder={IsPlaceholderLiveStateForDebug()}; stateReason={evidence.StateEvidenceReason}; driftReason={evidence.DriftEvidenceReason}";
        }


        public bool HasStableLiveEvidenceForDebug()
        {
            var evidence = ResolveTinyMoveJEvidenceGateState();
            return (connectionService?.Client.IsConnected ?? false)
                && evidence.ToolId > 0
                && evidence.UserId > 0
                && evidence.HasExplicitCoordSystem
                && evidence.StateEvidenceFresh
                && evidence.DriftEvidenceFresh
                && evidence.DriftPassed
                && evidence.MatchesCurrentSession
                && !IsPlaceholderLiveStateForDebug();
        }


        public void SetLivePollIntervalForDebug(float seconds)
        {
            connectionService?.SetPollInterval(seconds);
        }


        public void ResetLiveReadbackProbeForDebug()
        {
            liveReadbackProbeUpdateCount = 0;
            liveReadbackProbeFirstUpdateTime = -1d;
            liveReadbackProbeLastUpdateTime = -1d;
        }


        public void ForceNextReadFailuresForDebug(int count)
        {
            connectionService?.ForceNextReadFailuresForDebug(count, "forced debug read fail");
        }


        public string GetLiveReadbackProbeSummaryForDebug()
        {
            var elapsedSeconds = liveReadbackProbeUpdateCount > 1 && liveReadbackProbeFirstUpdateTime >= 0d && liveReadbackProbeLastUpdateTime >= liveReadbackProbeFirstUpdateTime
                ? liveReadbackProbeLastUpdateTime - liveReadbackProbeFirstUpdateTime
                : 0d;
            var observedHz = elapsedSeconds > 0d
                ? (liveReadbackProbeUpdateCount - 1) / elapsedSeconds
                : 0d;
            return $"poll={connectionService?.CurrentPollIntervalSeconds.ToString("0.###") ?? "-"}s; sampleMs={connectionService?.LastRealtimeStateSamplePeriodMs ?? 0}; count={liveReadbackProbeUpdateCount}; windowSec={elapsedSeconds:0.###}; hz={observedHz:0.##}; readErrors={connectionService?.ConsecutiveReadErrors ?? 0}; forcedReadFailures={connectionService?.ForcedReadFailuresRemaining ?? 0}";
        }


        private FairinoRobotState BuildDisplayStateForDrift()
        {
            var lockToLiveBaseline =
                currentLiveSessionMode != LiveCommandSessionMode.LiveControl
                && (previewUsesJointPose || previewTcpPose != null);

            return new FairinoRobotState(
                currentState.JointPosDeg,
                lockToLiveBaseline
                    ? CopyPoseArray(currentState.TcpPose)
                    : ComputeDisplayedTcpPose(),
                isRobotEnabled: connectionService?.Client.IsEnabled ?? false);
        }


        private void ApplyLiveDriftBlockedReason(string message)
        {
            RememberOperatorBlockedReason(message);
        }


        private TinyMoveJEvidenceGateState ResolveTinyMoveJEvidenceGateState()
        {
            var state = new TinyMoveJEvidenceGateState
            {
                ToolId = ResolveLiveToolId(),
                UserId = ResolveLiveUserId(),
                CoordSystem = ResolveLiveCoordSystem(),
            };

            var root = liveStateRecorder?.RootDirectory;
            if (string.IsNullOrWhiteSpace(root))
            {
                root = Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory(), "Artifacts", "live", "fr5");
            }

            state.StateFilePath = Path.Combine(root, "latest-state.json");
            state.DriftFilePath = Path.Combine(root, "latest-drift.json");

            state.StateEvidenceFresh = TryReadFreshJson(state.StateFilePath, out Fr5LiveStateRecord latestState, out var stateEvidenceReason);
            state.DriftEvidenceFresh = TryReadFreshJson(state.DriftFilePath, out Fr5LiveDriftRecord latestDrift, out var driftEvidenceReason);
            state.StateEvidenceReason = stateEvidenceReason;
            state.DriftEvidenceReason = driftEvidenceReason;

            state.LatestState = latestState;
            state.LatestDrift = latestDrift;
            if (!string.IsNullOrWhiteSpace(latestState?.coordSystem))
            {
                state.CoordSystem = latestState.coordSystem;
            }

            state.HasToolContext = state.ToolId > 0;
            state.HasUserContext = state.UserId > 0;
            state.HasExplicitCoordSystem = IsExplicitCoordSystem(state.CoordSystem);
            state.DriftPassed = latestDrift != null && string.Equals(latestDrift.severity, "ok", StringComparison.OrdinalIgnoreCase);
            if (!state.DriftPassed && string.IsNullOrWhiteSpace(state.DriftEvidenceReason))
            {
                state.DriftEvidenceReason = latestDrift == null
                    ? "latest-drift evidence missing"
                    : $"latest-drift severity {latestDrift.severity}";
            }

            var recorderSessionId = liveStateRecorder?.SessionId;
            state.MatchesCurrentSession = string.IsNullOrWhiteSpace(recorderSessionId)
                || (latestState != null && string.Equals(latestState.sessionId, recorderSessionId, StringComparison.Ordinal))
                    && (latestDrift != null && string.Equals(latestDrift.sessionId, recorderSessionId, StringComparison.Ordinal));

            if (!state.MatchesCurrentSession)
            {
                if (string.IsNullOrWhiteSpace(state.StateEvidenceReason))
                {
                    state.StateEvidenceReason = "latest-state session mismatch";
                }

                if (string.IsNullOrWhiteSpace(state.DriftEvidenceReason))
                {
                    state.DriftEvidenceReason = "latest-drift session mismatch";
                }
            }

            return state;
        }


        private int ResolveLiveToolId()
        {
            if (connectionService.LastState.ToolId > 0)
            {
                return connectionService.LastState.ToolId;
            }

            return connectionService.LastCoordContext.ToolId;
        }


        private int ResolveLiveUserId()
        {
            if (connectionService.LastState.UserId > 0)
            {
                return connectionService.LastState.UserId;
            }

            return connectionService.LastCoordContext.UserId;
        }


        private string ResolveLiveCoordSystem()
        {
            return string.IsNullOrWhiteSpace(snapshot.CoordSystem) ? string.Empty : snapshot.CoordSystem;
        }


        private static bool IsExplicitCoordSystem(string coordSystem)
        {
            return coordSystem is "Base" or "Tool" or "User";
        }


        private bool IsPlaceholderLiveStateForDebug()
        {
            return (connectionService?.Client.IsConnected ?? false)
                && ResolveLiveToolId() <= 0
                && ResolveLiveUserId() <= 0
                && IsAllZeroArray(currentState.JointPosDeg)
                && IsAllZeroArray(currentState.TcpPose);
        }


        private static bool IsAllZeroArray(double[] values)
        {
            if (values == null || values.Length == 0)
            {
                return true;
            }

            for (var index = 0; index < values.Length; index++)
            {
                if (System.Math.Abs(values[index]) > 0.0001d)
                {
                    return false;
                }
            }

            return true;
        }


        private static bool TryReadFreshJson<T>(string path, out T value, out string reason) where T : class
        {
            value = null;
            reason = string.Empty;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                reason = $"{Path.GetFileName(path)} missing";
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                value = JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                reason = $"{Path.GetFileName(path)} parse failed: {ex.GetType().Name}";
                return false;
            }

            if (value == null)
            {
                reason = $"{Path.GetFileName(path)} parse failed";
                return false;
            }

            var timestampUtc = value switch
            {
                Fr5LiveStateRecord stateRecord => stateRecord.timestampUtc,
                Fr5LiveDriftRecord driftRecord => driftRecord.timestampUtc,
                _ => string.Empty,
            };

            if (!DateTime.TryParse(timestampUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedUtc))
            {
                reason = $"{Path.GetFileName(path)} timestamp invalid";
                return false;
            }

            var ageSeconds = System.Math.Abs((DateTime.UtcNow - parsedUtc.ToUniversalTime()).TotalSeconds);
            if (ageSeconds > LiveEvidenceFreshnessWindowSeconds)
            {
                reason = $"{Path.GetFileName(path)} stale";
                return false;
            }

            return true;
        }


    }
}
