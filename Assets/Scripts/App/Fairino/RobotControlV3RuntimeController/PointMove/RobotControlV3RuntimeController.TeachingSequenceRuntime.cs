// Folder: PointMove - teaching point store selection, dry-run preview/apply, and teaching sequence runtime helpers.
using System;

namespace KineTutor3D.App.Fairino
{
    // Handles point-store loading, selected-point preview/execute, teaching loop state, and one-shot sequence stepping.
    // Function/block editing and runner-event binding remain in separate Teaching partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string GetTeachingPointStoreSummaryForDebug()
        {
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            return teachingPointStoreAdapter.BuildSummary();
        }


        public string LoadTeachingSequenceForDebug()
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            return $"{teachingSequenceRuntime.ToDebugSummary()}; {GetTeachingLoopSummaryForDebug()}";
        }


        public string GetTeachingLoopSummaryForDebug()
        {
            var runnerState = waypointRunner != null ? waypointRunner.State.ToString() : "missing";
            var runnerIndex = waypointRunner != null ? waypointRunner.CurrentIndex : -1;
            var runnerTotal = waypointRunner != null ? waypointRunner.TotalCount : 0;
            return $"loopEnabled={teachingLoopEnabled}; runnerState={runnerState}; runnerIndex={runnerIndex}; runnerTotal={runnerTotal}; isTeachingRunning={IsTeachingSequenceRunning}";
        }


        public bool SetTeachingLoopEnabled(bool enabled)
        {
            teachingLoopEnabled = enabled;
            PushFeedback(enabled ? "[Teaching Loop] 반복 실행 ON" : "[Teaching Loop] 반복 실행 OFF");
            RefreshSnapshot();
            return teachingLoopEnabled;
        }


        public bool ToggleTeachingLoopEnabled()
        {
            return SetTeachingLoopEnabled(!teachingLoopEnabled);
        }


        public string SelectTeachingPointForDebug(int index)
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Select(index);
            return teachingSequenceRuntime.ToDebugSummary();
        }


        public string PreviewSelectedTeachingPointForDebug()
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            var result = teachingSequenceRuntime.PreviewSelected(PreviewTeachingWaypoint);
            RefreshSnapshot();
            return $"{result.Message}; {teachingSequenceRuntime.ToDebugSummary()}; {GetDebugSummary()}";
        }


        public string ExecuteSelectedTeachingPointForDebug()
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            var result = teachingSequenceRuntime.ExecuteSelected(ExecuteTeachingWaypoint);
            RefreshSnapshot();
            return $"{result.Message}; {teachingSequenceRuntime.ToDebugSummary()}; {GetDebugSummary()}";
        }


        public string ExecuteTeachingSequenceFromPoint(string pointName)
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Teaching From] 실행할 저장 포인트가 없다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var startIndex = FindWaypointIndex(sequence, pointName);
            if (startIndex < 0)
            {
                PushFeedback($"[Teaching From] {pointName} 포인트를 찾지 못했다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            if (waypointRunner != null && waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                PushFeedback("[Teaching From] 실행 중인 반복이 있다. Stop 후 다시 실행해라.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var executed = 0;
            for (var index = startIndex; index < sequence.waypoints.Length; index++)
            {
                teachingSequenceRuntime.Select(index);
                var result = ExecuteTeachingWaypoint(sequence.waypoints[index]);
                if (!result.IsSuccess)
                {
                    PushFeedback($"[Teaching From] {index + 1}/{sequence.waypoints.Length} 실패 · {result.Message}");
                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                executed++;
            }

            PushFeedback($"[Teaching From] {startIndex + 1}/{sequence.waypoints.Length}부터 {executed}개 포인트 실행 완료");
            RefreshSnapshot();
            return snapshot.LastFeedback;
        }


        public string ExecuteTeachingSequenceFromPointForDebug(string pointName)
        {
            return ExecuteTeachingSequenceFromPoint(pointName);
        }


        private bool TryRunTeachingSequenceOnce()
        {
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            if (teachingSequenceRuntime.Count <= 0)
            {
                return false;
            }

            if (teachingLoopEnabled)
            {
                var sequence = teachingPointStoreAdapter.LoadIfExists();
                if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
                {
                    return false;
                }

                if (waypointRunner == null)
                {
                    EnsureRuntimeHelpers();
                }

                if (waypointRunner.State != WaypointCycleRunner.RunState.Idle)
                {
                    PushFeedback("[Teaching Loop] 이미 반복 실행 중이다. Stop 후 다시 실행해라.");
                    RefreshSnapshot();
                    return true;
                }

                waypointRunner.PlayLoop(sequence, snapshot.DryRunEnabled || connectionService == null || connectionService.IsMockMode);
                PushFeedback($"[Teaching Loop] {teachingSequenceRuntime.Count}개 포인트 반복 실행 시작");
                RefreshSnapshot();
                return true;
            }

            for (var index = 0; index < teachingSequenceRuntime.Count; index++)
            {
                teachingSequenceRuntime.Select(index);
                var result = teachingSequenceRuntime.ExecuteSelected(ExecuteTeachingWaypoint);
                if (!result.IsSuccess)
                {
                    PushFeedback($"[Teaching Run] {index + 1}/{teachingSequenceRuntime.Count} 실패 · {result.Message}");
                    RefreshSnapshot();
                    return true;
                }
            }

            PushFeedback($"[Teaching Run] {teachingSequenceRuntime.Count}개 포인트 실행 완료");
            RefreshSnapshot();
            return true;
        }


        private bool PreviewTeachingStep(int delta)
        {
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            if (teachingSequenceRuntime.Count <= 0)
            {
                return false;
            }

            if (delta >= 0)
            {
                teachingSequenceRuntime.SelectNext();
            }
            else
            {
                teachingSequenceRuntime.SelectPrevious();
            }

            var result = teachingSequenceRuntime.PreviewSelected(PreviewTeachingWaypoint);
            PushFeedback(result.IsSuccess
                ? $"[Teaching Step] {teachingSequenceRuntime.State.SelectedIndex + 1}/{teachingSequenceRuntime.Count} 미리보기"
                : result.Message);
            RefreshSnapshot();
            return true;
        }


        private FairinoResult PreviewTeachingWaypoint(Waypoint point)
        {
            if (point == null)
            {
                return FairinoResult.Fail(-94, "teaching point missing");
            }

            if (string.Equals(point.moveType, "MoveL", StringComparison.OrdinalIgnoreCase))
            {
                PreviewTcpPose(point.tcpMm, $"Teaching {point.name} MoveL preview");
                return FairinoResult.Ok($"preview MoveL {point.name}");
            }

            PreviewJointAngles(point.jointsDeg, $"Teaching {point.name} MoveJ preview");
            return FairinoResult.Ok($"preview MoveJ {point.name}");
        }


        private FairinoResult ExecuteTeachingWaypoint(Waypoint point)
        {
            if (point == null)
            {
                return FairinoResult.Fail(-95, "teaching point missing");
            }

            return string.Equals(point.moveType, "MoveL", StringComparison.OrdinalIgnoreCase)
                ? ApplyTcpPose(point.tcpMm, $"Teaching {point.name} MoveL")
                : ApplyTeachingMoveJ(point.jointsDeg, $"Teaching {point.name} MoveJ");
        }
    }
}
