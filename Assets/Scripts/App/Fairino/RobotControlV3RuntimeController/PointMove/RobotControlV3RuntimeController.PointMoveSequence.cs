// Folder: PointMove - named sequence orchestration and recorded path playback for the V3 point move panel.
using System;

namespace KineTutor3D.App.Fairino
{
    // Handles saved waypoint sequence load/run/delete and recorded path preview entry points.
    // Home-loop building and mixed-live recovery stay in dedicated PointMove partials.
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
    }
}
