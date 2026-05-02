// Folder: Shared - waypoint sequence and loop approval preparation/execution shared across V3 panels.
using System;

namespace KineTutor3D.App.Fairino
{
    // Owns sequence-specific live approval preparation, loop context, and pending sequence execution state.
    // Session/token bookkeeping remains in SessionMode and TokenLifecycle partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        public bool ShouldRouteWaypointSequenceThroughLiveApproval(string sequenceName, bool loop)
        {
            if (!ShouldUseSavedPointMoveJOperatorPath())
            {
                return false;
            }

            if (loop)
            {
                return CanBuildHomePoint1LoopSequence(out _, out _, out _);
            }

            return TryLoadLiveWaypointSequence(sequenceName, out var sequence, out _)
                && SequenceSupportsDirectLiveSequence(sequence);
        }


        public string PrepareWaypointSequenceOperatorApproval(string sequenceName, bool loop, string startPointName = "")
        {
            WaypointSequence sequence;
            string resolvedSequenceName;
            string loadMessage;
            if (loop)
            {
                if (!CanBuildHomePoint1LoopSequence(out sequence, out resolvedSequenceName, out loadMessage))
                {
                    ClearPendingWaypointSequenceOperatorCommandState();
                    PushFeedback(loadMessage);
                    RefreshSnapshot();
                    return loadMessage;
                }
            }
            else if (!TryLoadLiveWaypointSequence(sequenceName, out sequence, out loadMessage))
            {
                ClearPendingWaypointSequenceOperatorCommandState();
                PushFeedback(loadMessage);
                RefreshSnapshot();
                return loadMessage;
            }
            else
            {
                resolvedSequenceName = string.IsNullOrWhiteSpace(sequenceName)
                    ? TeachingPointStoreAdapter.DefaultSequenceName
                    : sequenceName.Trim();
            }

            if (!SequenceSupportsDirectLiveSequence(sequence))
            {
                ClearPendingWaypointSequenceOperatorCommandState();
                const string unsupportedMessage = "[Sequence] direct live는 MoveJ 저장 포인트만 지원한다.";
                PushFeedback(unsupportedMessage);
                RefreshSnapshot();
                return unsupportedMessage;
            }

            var safeStartPointName = string.IsNullOrWhiteSpace(startPointName)
                ? string.Empty
                : startPointName.Trim();
            if (!string.IsNullOrWhiteSpace(safeStartPointName)
                && FindWaypointIndex(sequence, safeStartPointName) < 0)
            {
                ClearPendingWaypointSequenceOperatorCommandState();
                var missingStartMessage = $"[Sequence] {safeStartPointName} 포인트를 찾지 못했다.";
                PushFeedback(missingStartMessage);
                RefreshSnapshot();
                return missingStartMessage;
            }

            hasPendingWaypointSequenceOperatorCommand = true;
            pendingWaypointSequenceName = resolvedSequenceName;
            pendingWaypointSequenceStartPointName = safeStartPointName;
            pendingWaypointSequenceRestoreDryRun = snapshot.DryRunEnabled;
            pendingWaypointSequenceLoop = loop;
            snapshot.LiveBlockedReason = string.Empty;
            SetLiveSessionMode(LiveCommandSessionMode.LiveControl);

            if (snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = false;
                InvalidateLiveApprovalContext();
            }

            RefreshSnapshot();
            return $"pendingSequenceApproval=True; sequence={pendingWaypointSequenceName}; count={sequence.waypoints.Length}; loop={pendingWaypointSequenceLoop}; dryRun={snapshot.DryRunEnabled}";
        }


        public string ExecutePendingWaypointSequenceOperatorCommand()
        {
            if (!hasPendingWaypointSequenceOperatorCommand)
            {
                return "pendingSequenceApproval=False";
            }

            var sequenceName = pendingWaypointSequenceName;
            var startPointName = pendingWaypointSequenceStartPointName;
            var restoreDryRun = pendingWaypointSequenceRestoreDryRun;
            var loop = pendingWaypointSequenceLoop;
            ClearPendingWaypointSequenceOperatorCommandState();
            if (loop || string.Equals(sequenceName, HomePoint1LoopSequenceName, StringComparison.OrdinalIgnoreCase))
            {
                BeginLiveLoopApprovalContext(sequenceName);
            }

            if (liveWaypointSequenceCoroutine != null)
            {
                var alreadyRunning = $"[Sequence Run] {liveWaypointSequenceName} 실행 중이다. Stop 후 다시 실행해라.";
                PushFeedback(alreadyRunning);
                RefreshSnapshot();
                return alreadyRunning;
            }

            WaypointSequence sequence;
            string loadMessage;
            if (string.Equals(sequenceName, HomePoint1LoopSequenceName, StringComparison.OrdinalIgnoreCase))
            {
                if (!CanBuildHomePoint1LoopSequence(out sequence, out _, out loadMessage))
                {
                    PushFeedback(loadMessage);
                    RefreshSnapshot();
                    return loadMessage;
                }
            }
            else if (!TryLoadLiveWaypointSequence(sequenceName, out sequence, out loadMessage))
            {
                PushFeedback(loadMessage);
                RefreshSnapshot();
                return loadMessage;
            }

            var startIndex = string.IsNullOrWhiteSpace(startPointName)
                ? 0
                : FindWaypointIndex(sequence, startPointName);
            if (startIndex < 0)
            {
                var startPointMissing = $"[Sequence Run] {startPointName} 포인트를 찾지 못했다.";
                PushFeedback(startPointMissing);
                RefreshSnapshot();
                return startPointMissing;
            }

            for (var index = startIndex; index < sequence.waypoints.Length; index++)
            {
                var point = sequence.waypoints[index];
                if (point == null)
                {
                    continue;
                }

                if (!string.Equals(point.moveType, "MoveJ", StringComparison.OrdinalIgnoreCase))
                {
                    var unsupported = $"[Sequence Run] {point.name} 실패 · direct live는 MoveJ만 지원한다.";
                    PushFeedback(unsupported);
                    RefreshSnapshot();
                    return unsupported;
                }
            }

            liveWaypointSequenceName = sequenceName;
            liveWaypointSequenceCoroutine = StartCoroutine(
                RunLiveWaypointSequence(
                    sequenceName,
                    sequence,
                    startIndex,
                    restoreDryRun,
                    loop,
                    applyGripperPattern: string.Equals(sequenceName, HomePoint1LoopSequenceName, StringComparison.OrdinalIgnoreCase)));
            PushFeedback(loop
                ? $"[Sequence Loop] {sequenceName} mixed live 루프 시작 · {sequence.waypoints.Length - startIndex}개 포인트"
                : $"[Sequence Run] {sequenceName} live 실행 시작 · {sequence.waypoints.Length - startIndex}개 포인트");
            RefreshSnapshot();
            return snapshot.LastFeedback;
        }


        private void ClearPendingWaypointSequenceOperatorCommandState()
        {
            hasPendingWaypointSequenceOperatorCommand = false;
            pendingWaypointSequenceName = string.Empty;
            pendingWaypointSequenceStartPointName = string.Empty;
            pendingWaypointSequenceRestoreDryRun = false;
            pendingWaypointSequenceLoop = false;
        }


        private void BeginLiveLoopApprovalContext(string sequenceName)
        {
            approvedLiveLoopContextKey = ResolveLiveLoopContextKey(sequenceName);
            approvedLiveLoopUntilUtc = DateTime.UtcNow.AddSeconds(90);
        }


        private bool HasActiveLiveLoopApproval()
        {
            return liveLoopApprovalExecutionContext
                && !string.IsNullOrWhiteSpace(approvedLiveLoopContextKey)
                && approvedLiveLoopUntilUtc > DateTime.UtcNow;
        }


        private void ClearLiveLoopApprovalContext()
        {
            approvedLiveLoopUntilUtc = DateTime.MinValue;
            approvedLiveLoopContextKey = string.Empty;
        }


        private static string ResolveLiveLoopContextKey(string sequenceName)
        {
            return string.IsNullOrWhiteSpace(sequenceName)
                ? HomePoint1LoopSequenceName
                : sequenceName.Trim();
        }


        private void CancelPendingWaypointSequenceOperatorCommand()
        {
            var restoreDryRun = pendingWaypointSequenceRestoreDryRun;
            ClearPendingWaypointSequenceOperatorCommandState();
            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
            }
        }
    }
}
