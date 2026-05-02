// Folder: PointMove - mixed live sequence continuation and arrival/gripper helpers.
using System.Collections;

namespace KineTutor3D.App.Fairino
{
    // Handles mixed live loop continuation, recovery from evidence, and arrival/gripper post-actions.
    // Sequence selection/load and point preview/apply entry points stay in other PointMove partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        private bool CanContinueMixedLiveSequence(bool loop, out string message)
        {
            message = string.Empty;
            if (connectionService == null || connectionService.IsMockMode || connectionService.Client == null || !connectionService.Client.IsConnected)
            {
                message = loop
                    ? "[Sequence Loop] 연결이 끊겨 mixed live 루프를 중단한다."
                    : "[Sequence Run] 연결이 끊겨 mixed live 실행을 중단한다.";
                return false;
            }

            if (TryResolveEvidenceBackedMixedLiveState(out var evidenceState))
            {
                HandleStateUpdated(evidenceState);
                RefreshLiveEvidenceForDebug();
                return true;
            }

            var sync = connectionService.SyncCurrentState();
            if (!sync.IsSuccess)
            {
                if (TryResolveEvidenceBackedMixedLiveState(sync.Message, out var recoveredState))
                {
                    HandleStateUpdated(recoveredState);
                    RefreshLiveEvidenceForDebug();
                    return true;
                }

                message = loop
                    ? $"[Sequence Loop] 현재 위치 읽기 실패로 루프를 중단한다. {sync.Message}"
                    : $"[Sequence Run] 현재 위치 읽기 실패로 실행을 중단한다. {sync.Message}";
                return false;
            }

            HandleStateUpdated(sync.Value);
            RefreshLiveEvidenceForDebug();
            if (!HasStableLiveEvidenceForDebug())
            {
                message = loop
                    ? "[Sequence Loop] latest-state/latest-drift evidence가 stale이라 루프를 중단한다."
                    : "[Sequence Run] latest-state/latest-drift evidence가 stale이라 실행을 중단한다.";
                return false;
            }

            if (!sync.Value.IsRobotEnabled)
            {
                message = loop
                    ? "[Sequence Loop] controller enabled=false라 루프를 중단한다."
                    : "[Sequence Run] controller enabled=false라 실행을 중단한다.";
                return false;
            }

            if (sync.Value.RobotMode != 0)
            {
                message = loop
                    ? $"[Sequence Loop] mode={sync.Value.RobotMode}라 다음 cycle 진입을 막고 종료한다."
                    : $"[Sequence Run] mode={sync.Value.RobotMode}라 실행을 중단한다.";
                return false;
            }

            if (sync.Value.ToolId <= 0 || sync.Value.UserId <= 0)
            {
                message = loop
                    ? "[Sequence Loop] tool/user truth가 없어 루프를 중단한다."
                    : "[Sequence Run] tool/user truth가 없어 실행을 중단한다.";
                return false;
            }

            if (sync.Value.MainErrorCode != 0 || sync.Value.SubErrorCode != 0 || sync.Value.IsSafetyStop)
            {
                message = loop
                    ? $"[Sequence Loop] fault={sync.Value.MainErrorCode}/{sync.Value.SubErrorCode} safety={sync.Value.IsSafetyStop}라 루프를 중단한다."
                    : $"[Sequence Run] fault={sync.Value.MainErrorCode}/{sync.Value.SubErrorCode} safety={sync.Value.IsSafetyStop}라 실행을 중단한다.";
                return false;
            }

            return true;
        }


        private bool TryResolveEvidenceBackedMixedLiveState(out FairinoRobotState recoveredState)
        {
            recoveredState = FairinoRobotState.Zero();
            var evidence = ResolveTinyMoveJEvidenceGateState();
            if (!evidence.MatchesCurrentSession
                || !evidence.StateEvidenceFresh
                || evidence.LatestState == null
                || !evidence.LatestState.connected
                || !evidence.LatestState.enabled
                || !evidence.LatestState.isRobotEnabled
                || evidence.LatestState.mode != 0)
            {
                return false;
            }

            if (connectionService == null)
            {
                return false;
            }

            var cachedState = connectionService.LastState;
            if (cachedState.JointPosDeg != null
                && cachedState.JointPosDeg.Length >= templateDefinition.JointCount
                && cachedState.TcpPose != null
                && cachedState.TcpPose.Length >= 6
                && cachedState.IsRobotEnabled
                && cachedState.RobotMode == 0)
            {
                recoveredState = cachedState;
                return true;
            }

            if (evidence.LatestState.jointsDeg == null
                || evidence.LatestState.jointsDeg.Length < templateDefinition.JointCount
                || evidence.LatestState.tcpMmDeg == null
                || evidence.LatestState.tcpMmDeg.Length < 6)
            {
                return false;
            }

            recoveredState = new FairinoRobotState(
                evidence.LatestState.jointsDeg,
                evidence.LatestState.tcpMmDeg,
                robotMode: evidence.LatestState.mode,
                mainErrorCode: 0,
                subErrorCode: 0,
                toolId: evidence.LatestState.toolId,
                userId: evidence.LatestState.userId,
                isRobotEnabled: evidence.LatestState.isRobotEnabled,
                isInDragTeach: evidence.LatestState.isInDragTeach);
            return true;
        }


        private bool TryResolveEvidenceBackedMixedLiveState(string readbackFailureMessage, out FairinoRobotState recoveredState)
        {
            recoveredState = FairinoRobotState.Zero();
            return !string.IsNullOrWhiteSpace(readbackFailureMessage)
                && readbackFailureMessage.Contains("비활성 상태", System.StringComparison.OrdinalIgnoreCase)
                && TryResolveEvidenceBackedMixedLiveState(out recoveredState);
        }


        private static bool TryResolveHomePoint1GripperAction(string pointName, out float positionPercent, out string intent)
        {
            positionPercent = 100f;
            intent = "없음";
            if (string.IsNullOrWhiteSpace(pointName))
            {
                return false;
            }

            var safeName = pointName.Trim();
            if (string.Equals(safeName, DefaultPoint1Name, System.StringComparison.OrdinalIgnoreCase))
            {
                positionPercent = 0f;
                intent = "close";
                return true;
            }

            if (string.Equals(safeName, DefaultHomePointName, System.StringComparison.OrdinalIgnoreCase))
            {
                positionPercent = 100f;
                intent = "open";
                return true;
            }

            return false;
        }


        private static bool SequenceSupportsTinyMoveJLive(WaypointSequence sequence)
        {
            return SequenceSupportsDirectLiveSequence(sequence);
        }


        private IEnumerator RunLiveWaypointSequence(
            string sequenceName,
            WaypointSequence sequence,
            int startIndex,
            bool restoreDryRun,
            bool loop,
            bool applyGripperPattern)
        {
            const float arrivalPollSeconds = 0.1f;
            const float arrivalTimeoutSeconds = 30f;
            const double arrivalThresholdDeg = 1.0d;

            var executed = 0;
            var cycle = 0;
            try
            {
                liveWaypointSequenceLooping = loop;
                liveWaypointSequenceCycleCount = 0;
                liveWaypointCurrentTargetName = string.Empty;
                liveWaypointCurrentGripperIntent = string.Empty;
                liveWaypointBlockedReason = string.Empty;
                if (loop)
                {
                    SetLiveSessionMode(LiveCommandSessionMode.LoopRunning);
                }

                while (true)
                {
                    cycle++;
                    liveWaypointSequenceCycleCount = cycle;
                    if (!CanContinueMixedLiveSequence(loop, out var blockedMessage))
                    {
                        liveWaypointBlockedReason = blockedMessage;
                        RememberOperatorBlockedReason(blockedMessage);
                        PushFeedback(blockedMessage);
                        RefreshSnapshot();
                        yield break;
                    }

                    for (var index = startIndex; index < sequence.waypoints.Length; index++)
                    {
                        var point = sequence.waypoints[index];
                        if (point == null)
                        {
                            continue;
                        }

                        liveWaypointCurrentTargetName = point.name ?? $"P{index + 1}";
                        liveWaypointCurrentGripperIntent = "이동 중";
                        teachingSequenceRuntime?.Select(index);
                        PreviewJointAngles(point.jointsDeg, $"Sequence {point.name} MoveJ preview");
                        yield return null;

                        liveLoopApprovalExecutionContext = loop || applyGripperPattern;
                        var result = ExecuteTeachingWaypoint(point);
                        liveLoopApprovalExecutionContext = false;
                        if (!result.IsSuccess)
                        {
                            PushFeedback($"[Sequence Run] {index + 1}/{sequence.waypoints.Length} 실패 · {result.Message}");
                            RefreshSnapshot();
                            yield break;
                        }

                        executed++;
                        var elapsed = 0f;
                        var arrived = false;
                        while (elapsed < arrivalTimeoutSeconds)
                        {
                            if (connectionService != null)
                            {
                                var sync = connectionService.SyncCurrentState();
                                if (sync.IsSuccess && sync.Value.JointPosDeg != null)
                                {
                                    HandleStateUpdated(sync.Value);
                                    if (sync.Value.MotionQueueLength <= 0
                                        && HasArrivedAtWaypoint(sync.Value.JointPosDeg, point.jointsDeg, arrivalThresholdDeg))
                                    {
                                        arrived = true;
                                        break;
                                    }
                                }
                                else if (TryResolveEvidenceBackedMixedLiveState(sync.Message, out var recoveredState)
                                         && recoveredState.JointPosDeg != null)
                                {
                                    HandleStateUpdated(recoveredState);
                                    if (recoveredState.MotionQueueLength <= 0
                                        && HasArrivedAtWaypoint(recoveredState.JointPosDeg, point.jointsDeg, arrivalThresholdDeg))
                                    {
                                        arrived = true;
                                        break;
                                    }
                                }
                            }

                            yield return new UnityEngine.WaitForSeconds(arrivalPollSeconds);
                            elapsed += arrivalPollSeconds;
                        }

                        if (!arrived)
                        {
                            PushFeedback($"[Sequence Run] {index + 1}/{sequence.waypoints.Length} 실패 · {point.name} 도달 타임아웃");
                            RefreshSnapshot();
                            yield break;
                        }

                        PushFeedback($"[Sequence Run] {index + 1}/{sequence.waypoints.Length} {point.name} 도달");
                        RefreshSnapshot();
                        yield return null;

                        if (applyGripperPattern
                            && TryResolveHomePoint1GripperAction(point.name, out var gripperPercent, out var gripperIntent))
                        {
                            liveWaypointCurrentGripperIntent = gripperIntent;
                            liveLoopApprovalExecutionContext = true;
                            var gripperResult = SetGripperPositionPercent(gripperPercent);
                            liveLoopApprovalExecutionContext = false;
                            if (!gripperResult.IsSuccess)
                            {
                                PushFeedback($"[Sequence Run] {point.name} {gripperIntent} 실패 · {gripperResult.Message}");
                                RefreshSnapshot();
                                yield break;
                            }

                            PushFeedback($"[Sequence Run] {point.name} 도달 후 gripper {gripperIntent}");
                            RefreshSnapshot();
                            yield return null;
                        }
                        else
                        {
                            liveWaypointCurrentGripperIntent = "없음";
                        }
                    }

                    if (!loop)
                    {
                        break;
                    }

                    startIndex = 0;
                }

                PushFeedback(loop
                    ? $"[Sequence Loop] {sequenceName} {cycle}사이클 완료 · {executed}개 포인트"
                    : $"[Sequence Run] {sequenceName} live 1회 실행 완료 · {executed}개 포인트");
                RefreshSnapshot();
            }
            finally
            {
                if (restoreDryRun && !snapshot.DryRunEnabled)
                {
                    snapshot.DryRunEnabled = true;
                    InvalidateLiveApprovalContext();
                }

                liveLoopApprovalExecutionContext = false;
                liveWaypointSequenceLooping = false;
                liveWaypointSequenceCycleCount = 0;
                liveWaypointCurrentTargetName = string.Empty;
                liveWaypointCurrentGripperIntent = string.Empty;
                liveWaypointBlockedReason = string.Empty;
                liveWaypointSequenceCoroutine = null;
                liveWaypointSequenceName = string.Empty;
                SetLiveSessionMode(LiveCommandSessionMode.LiveControl);
            }
        }


        private static bool HasArrivedAtWaypoint(double[] currentJointDeg, double[] targetJointDeg, double thresholdDeg)
        {
            if (currentJointDeg == null || targetJointDeg == null || currentJointDeg.Length < 6 || targetJointDeg.Length < 6)
            {
                return false;
            }

            for (var index = 0; index < 6; index++)
            {
                if (System.Math.Abs(currentJointDeg[index] - targetJointDeg[index]) > thresholdDeg)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
