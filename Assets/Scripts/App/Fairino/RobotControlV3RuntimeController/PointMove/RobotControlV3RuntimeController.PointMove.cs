// Folder: PointMove - saved point, loop, and sequence orchestration for the V3 point move panel.
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
    // Handles recorded path playback, waypoint sequence execution, and point/sequence live loop orchestration.
    // Joint/TCP low-level apply helpers stay in JointControl/TcpControl and approval state stays in Shared.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string ExecuteWaypointSequenceOnce(string sequenceName)
        {
            return PlayNamedWaypointSequence(sequenceName, loop: false);
        }


        public string ExecuteWaypointSequenceLoop(string sequenceName)
        {
            return PlayNamedWaypointSequence(sequenceName, loop: true);
        }


        public string DeleteWaypointSequence(string sequenceName)
        {
            if (string.IsNullOrWhiteSpace(sequenceName))
            {
                PushFeedback("[Sequence] 삭제할 실행 목록 이름이 비어 있다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            if (waypointRunner != null && waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                PushFeedback("[Sequence] 실행 중에는 실행 목록을 삭제하지 않는다. Stop 후 다시 삭제해라.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var safeName = sequenceName.Trim();
            var ok = WaypointStore.Delete(safeName);
            if (string.Equals(safeName, RecordedPathSequenceName, StringComparison.OrdinalIgnoreCase))
            {
                recordedPathSequence = null;
            }

            PushFeedback(ok ? $"[Sequence] {safeName} 삭제" : $"[Sequence] {safeName} 삭제 실패");
            RefreshSnapshot();
            return snapshot.LastFeedback;
        }


        private string PlayRecordedTeachingPath(bool loop)
        {
            if (!EnsureReadyForCommand(loop ? "기록 루프 재생" : "기록 재생"))
            {
                return GetTeachingPathRecordingSummaryForDebug();
            }

            var sequence = ResolveRecordedPathSequence();
            if (sequence?.waypoints == null || sequence.waypoints.Length < 2)
            {
                PushFeedback("[Path Replay] 재생할 기록 경로가 없다. 기록 시작 → 이동 → 기록 중지 순서로 먼저 저장해라.");
                RefreshSnapshot();
                return GetTeachingPathRecordingSummaryForDebug();
            }

            if (waypointRunner == null)
            {
                EnsureRuntimeHelpers();
            }

            if (waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                PushFeedback("[Path Replay] 이미 재생 중이다. Stop 후 다시 실행해라.");
                RefreshSnapshot();
                return GetTeachingPathRecordingSummaryForDebug();
            }

            if (loop)
            {
                waypointRunner.PlayLoop(sequence, dryRun: true);
                PushFeedback($"[Path Preview] 기록 경로 루프 프리뷰 시작 · {sequence.waypoints.Length}개 샘플");
            }
            else
            {
                waypointRunner.PlayOnce(sequence, dryRun: true);
                PushFeedback($"[Path Preview] 기록 경로 1회 프리뷰 · {sequence.waypoints.Length}개 샘플");
            }

            RefreshSnapshot();
            return GetTeachingPathRecordingSummaryForDebug();
        }


        private string PlayNamedWaypointSequence(string sequenceName, bool loop)
        {
            var commandName = loop ? "실행 목록 루프" : "실행 목록 재생";
            if (!EnsureReadyForCommand(commandName))
            {
                return snapshot.LastFeedback;
            }

            if (!TryLoadLiveWaypointSequence(sequenceName, out var sequence, out var loadMessage))
            {
                PushFeedback(loadMessage);
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var safeName = string.IsNullOrWhiteSpace(sequenceName)
                ? TeachingPointStoreAdapter.DefaultSequenceName
                : sequenceName.Trim();

            if (waypointRunner == null)
            {
                EnsureRuntimeHelpers();
            }

            if (waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                PushFeedback("[Sequence] 이미 실행 중이다. Stop 후 다시 실행해라.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            if (!snapshot.DryRunEnabled && connectionService != null && !connectionService.IsMockMode)
            {
                if (loop)
                {
                    var prepared = PrepareWaypointSequenceOperatorApproval(sequenceName, loop: true);
                    if (HasPendingWaypointSequenceOperatorApproval())
                    {
                        BeginLiveLoopApprovalContext(sequenceName);
                        return ExecutePendingWaypointSequenceOperatorCommand();
                    }

                    return prepared;
                }

                var executed = 0;
                for (var index = 0; index < sequence.waypoints.Length; index++)
                {
                    var result = ExecuteTeachingWaypoint(sequence.waypoints[index]);
                    if (!result.IsSuccess)
                    {
                        PushFeedback($"[Sequence Run] {safeName} {index + 1}/{sequence.waypoints.Length} 실패 · {result.Message}");
                        RefreshSnapshot();
                        return snapshot.LastFeedback;
                    }

                    executed++;
                }

                PushFeedback($"[Sequence Run] {safeName} live 1회 실행 완료 · {executed}개");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var dryRun = true;
            if (loop)
            {
                waypointRunner.PlayLoop(sequence, dryRun);
                PushFeedback($"[Sequence Preview] {safeName} 루프 프리뷰 시작 · {sequence.waypoints.Length}개");
            }
            else
            {
                waypointRunner.PlayOnce(sequence, dryRun);
                PushFeedback($"[Sequence Preview] {safeName} 1회 프리뷰 · {sequence.waypoints.Length}개");
            }

            RefreshSnapshot();
            return snapshot.LastFeedback;
        }


        private bool TryLoadLiveWaypointSequence(string sequenceName, out WaypointSequence sequence, out string message)
        {
            var safeName = string.IsNullOrWhiteSpace(sequenceName)
                ? TeachingPointStoreAdapter.DefaultSequenceName
                : sequenceName.Trim();
            sequence = string.Equals(safeName, RecordedPathSequenceName, StringComparison.OrdinalIgnoreCase)
                ? ResolveRecordedPathSequence()
                : string.Equals(safeName, HomePoint1LoopSequenceName, StringComparison.OrdinalIgnoreCase)
                    ? BuildOrRefreshHomePoint1LoopSequence()
                : WaypointStore.Load(safeName);
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                message = $"[Sequence] {safeName} 실행할 포인트가 없다.";
                return false;
            }

            message = string.Empty;
            return true;
        }


        private static string ResolveWaypointSequenceApprovalTargetKey(string sequenceName, WaypointSequence sequence)
        {
            var safeName = string.IsNullOrWhiteSpace(sequenceName)
                ? TeachingPointStoreAdapter.DefaultSequenceName
                : sequenceName.Trim();
            var count = sequence?.waypoints?.Length ?? 0;
            return $"SEQ:{safeName}:{count}:mixed-home-point1-close-open";
        }


        private static bool SequenceSupportsDirectLiveSequence(WaypointSequence sequence)
        {
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                return false;
            }

            for (var index = 0; index < sequence.waypoints.Length; index++)
            {
                var waypoint = sequence.waypoints[index];
                if (waypoint == null
                    || !string.Equals(waypoint.moveType, "MoveJ", StringComparison.OrdinalIgnoreCase)
                    || waypoint.jointsDeg == null
                    || waypoint.jointsDeg.Length < 6)
                {
                    return false;
                }
            }

            return true;
        }


        public string EnsureHomePoint1LoopSequenceForProduct()
        {
            var sequence = BuildOrRefreshHomePoint1LoopSequence();
            return sequence?.waypoints != null && sequence.waypoints.Length >= 2
                ? $"{sequence.name}:{sequence.waypoints.Length}"
                : string.Empty;
        }


        private bool CanBuildHomePoint1LoopSequence(out WaypointSequence sequence, out string resolvedSequenceName, out string message)
        {
            resolvedSequenceName = HomePoint1LoopSequenceName;
            sequence = BuildOrRefreshHomePoint1LoopSequence();
            if (sequence?.waypoints == null || sequence.waypoints.Length < 2)
            {
                message = $"[Sequence] {HomePoint1LoopSequenceName} 준비 실패 · Home와 {DefaultPoint1Name} 저장 포인트를 확인해라.";
                return false;
            }

            message = string.Empty;
            return true;
        }


        private WaypointSequence BuildOrRefreshHomePoint1LoopSequence()
        {
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var teachingSequence = teachingPointStoreAdapter.LoadOrCreate();
            if (teachingSequence == null)
            {
                return null;
            }

            var homePoint = FindWaypoint(teachingSequence, DefaultHomePointName);
            if (homePoint == null)
            {
                homePoint = TryCaptureHomeWaypoint(teachingSequence);
            }

            var point1 = FindWaypoint(teachingSequence, DefaultPoint1Name);
            if (homePoint == null || point1 == null)
            {
                return null;
            }

            var existingSequence = WaypointStore.Load(HomePoint1LoopSequenceName);
            var sequence = new WaypointSequence
            {
                name = HomePoint1LoopSequenceName,
                created = string.IsNullOrWhiteSpace(existingSequence?.created)
                    ? DateTime.UtcNow.ToString("O")
                    : existingSequence.created,
                waypoints = new[]
                {
                    CloneWaypointForSequence(homePoint, DefaultHomePointName),
                    CloneWaypointForSequence(point1, DefaultPoint1Name),
                },
            };
            if (AreWaypointSequencesEquivalent(existingSequence, sequence))
            {
                return existingSequence;
            }

            WaypointStore.Save(sequence);
            return sequence;
        }


        private Waypoint TryCaptureHomeWaypoint(WaypointSequence teachingSequence)
        {
            if (!hasCurrentPositionReadComplete
                || currentState.JointPosDeg == null
                || currentState.JointPosDeg.Length < 6
                || currentState.TcpPose == null
                || currentState.TcpPose.Length < 6)
            {
                return null;
            }

            var capturedHome = new Waypoint
            {
                name = DefaultHomePointName,
                jointsDeg = CopyJointArray(currentState.JointPosDeg),
                tcpMm = CopyPoseArray(currentState.TcpPose),
                moveType = "MoveJ",
                speedPreset = "medium",
                dwellSec = 0d,
            };

            var existing = teachingSequence.waypoints ?? Array.Empty<Waypoint>();
            var expanded = new Waypoint[existing.Length + 1];
            Array.Copy(existing, expanded, existing.Length);
            expanded[existing.Length] = capturedHome;
            teachingSequence.waypoints = expanded;
            teachingPointStoreAdapter?.Save(teachingSequence);
            return capturedHome;
        }


        private static Waypoint CloneWaypointForSequence(Waypoint waypoint, string fallbackName)
        {
            return new Waypoint
            {
                name = string.IsNullOrWhiteSpace(waypoint?.name) ? fallbackName : waypoint.name.Trim(),
                jointsDeg = waypoint?.jointsDeg != null ? (double[])waypoint.jointsDeg.Clone() : new double[6],
                tcpMm = waypoint?.tcpMm != null ? (double[])waypoint.tcpMm.Clone() : new double[6],
                moveType = string.Equals(waypoint?.moveType, "MoveL", StringComparison.OrdinalIgnoreCase) ? "MoveL" : "MoveJ",
                speedPreset = string.IsNullOrWhiteSpace(waypoint?.speedPreset) ? "medium" : waypoint.speedPreset,
                dwellSec = waypoint?.dwellSec ?? 0d,
            };
        }


        private static bool AreWaypointSequencesEquivalent(WaypointSequence left, WaypointSequence right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            if (!string.Equals(left.name, right.name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var leftWaypoints = left.waypoints ?? Array.Empty<Waypoint>();
            var rightWaypoints = right.waypoints ?? Array.Empty<Waypoint>();
            if (leftWaypoints.Length != rightWaypoints.Length)
            {
                return false;
            }

            for (var index = 0; index < leftWaypoints.Length; index++)
            {
                if (!AreWaypointsEquivalent(leftWaypoints[index], rightWaypoints[index]))
                {
                    return false;
                }
            }

            return true;
        }


        private static bool AreWaypointsEquivalent(Waypoint left, Waypoint right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            return string.Equals(left.name, right.name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(left.moveType, right.moveType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(left.speedPreset, right.speedPreset, StringComparison.OrdinalIgnoreCase)
                && System.Math.Abs(left.dwellSec - right.dwellSec) < 0.0001d
                && AreDoubleArraysEquivalent(left.jointsDeg, right.jointsDeg)
                && AreDoubleArraysEquivalent(left.tcpMm, right.tcpMm);
        }


        private static bool AreDoubleArraysEquivalent(double[] left, double[] right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (var index = 0; index < left.Length; index++)
            {
                if (System.Math.Abs(left[index] - right[index]) >= 0.0001d)
                {
                    return false;
                }
            }

            return true;
        }


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
                && readbackFailureMessage.Contains("비활성 상태", StringComparison.OrdinalIgnoreCase)
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
            if (string.Equals(safeName, DefaultPoint1Name, StringComparison.OrdinalIgnoreCase))
            {
                positionPercent = 0f;
                intent = "close";
                return true;
            }

            if (string.Equals(safeName, DefaultHomePointName, StringComparison.OrdinalIgnoreCase))
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


        private WaypointSequence ResolveRecordedPathSequence()
        {
            if (recordedPathSequence?.waypoints != null && recordedPathSequence.waypoints.Length > 0)
            {
                return recordedPathSequence;
            }

            if (!WaypointSequenceExists(RecordedPathSequenceName))
            {
                recordedPathSequence = null;
                return null;
            }

            recordedPathSequence = WaypointStore.Load(RecordedPathSequenceName);
            return recordedPathSequence;
        }


        private static bool WaypointSequenceExists(string sequenceName)
        {
            var names = WaypointStore.LoadAllNames();
            for (var index = 0; index < names.Length; index++)
            {
                if (string.Equals(names[index], sequenceName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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

                            yield return new WaitForSeconds(arrivalPollSeconds);
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


        public void ExecutePrimaryAction()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ReadyToJog && TryExecutePendingPreview())
            {
                return;
            }

            switch (snapshot.StatusKind)
            {
                case RobotControlV3RuntimeStatusKind.Disconnected:
                    ConnectAndSyncDefaultAsync();
                    break;
                case RobotControlV3RuntimeStatusKind.ConnectedServoOff:
                    if (IsReadbackOnlyLiveClient())
                    {
                        SyncCurrentStateAsync();
                    }
                    else
                    {
                        EnableServo();
                    }
                    break;
                case RobotControlV3RuntimeStatusKind.ConnectedUnsynced:
                    SyncCurrentStateAsync();
                    break;
                case RobotControlV3RuntimeStatusKind.Fault:
                    ResetErrors();
                    break;
                default:
                    if (!TryRunTeachingSequenceOnce())
                    {
                        PushFeedback("실행할 저장 포인트가 없다.");
                    }

                    RefreshSnapshot();
                    break;
            }
        }


        public string ExecutePreparedPreviewForDebug()
        {
            var executed = TryExecutePendingPreview();
            RefreshSnapshot();
            return $"executed={executed}; status={snapshot.StatusKind}; dryRun={snapshot.DryRunEnabled}; feedback={snapshot.LastFeedback}";
        }


        public void ExecutePreparedPreviewForProduct()
        {
            TryExecutePendingPreview();
            RefreshSnapshot();
        }


        private bool TryExecutePendingPreview()
        {
            if (!EnsureReadyForCommand("실행"))
            {
                return false;
            }

            if (previewUsesJointPose && previewJointAnglesDeg != null)
            {
                ApplyTinyMoveJ(previewJointAnglesDeg, "실행 버튼 tiny MoveJ");
                return true;
            }

            if (!previewUsesJointPose && previewTcpPose != null)
            {
                ApplyTcpPose(previewTcpPose, "실행 버튼 MoveL");
                return true;
            }

            return false;
        }


        public FairinoResult PreviewPointMoveJ(double[] tcpPose, string reason = "포인트 MoveJ 후보")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            var solveResult = TrySolvePointMoveJoints(tcpPose, out var jointTarget);
            if (!solveResult.IsSuccess)
            {
                PushFeedback(solveResult.Message);
                RefreshSnapshot();
                return solveResult;
            }

            previewJointAnglesDeg = jointTarget;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            InvalidateLiveApprovalContext();
            CapturePreparedMotionContext(LiveCommandKind.MoveJ, previewJointAnglesDeg, null, productionIkSafe: false, boundaryReady: false, collisionReady: false, reason);
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback($"[Preview] {reason}");
            RefreshSnapshot();
            return FairinoResult.Ok("Point MoveJ preview ready");
        }


        public FairinoResult ApplyPointMoveJ(double[] tcpPose, string reason = "포인트 MoveJ 적용")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            var solveResult = TrySolvePointMoveJoints(tcpPose, out var jointTarget);
            if (!solveResult.IsSuccess)
            {
                PushFeedback(solveResult.Message);
                RefreshSnapshot();
                return solveResult;
            }

            return ApplyJointAngles(jointTarget, reason, liveProductionIkEligible: false);
        }
    }
}
