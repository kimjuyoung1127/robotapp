// Folder: PointMove - teaching path recording, recorded-path playback, and manual readback simulation helpers.
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    // Handles path recording and the small manual-readback debug bridge used by teaching flows.
    // Sequence execution, function/block editing, and runner events remain in the other Teaching partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string StartTeachingPathRecording()
        {
            if (!EnsureReadyForCommand("경로 기록 시작"))
            {
                return GetTeachingPathRecordingSummaryForDebug();
            }

            teachingPathRecorder ??= new TeachingPathRecorder();
            teachingPathRecorder.Start(Time.timeAsDouble);
            teachingPathRecorder.Capture(currentState, Time.timeAsDouble, force: true);
            recordedPathSequence = null;
            PushFeedback("[Path Record] 기록 시작 · 현재 자세부터 샘플링");
            RefreshSnapshot();
            return GetTeachingPathRecordingSummaryForDebug();
        }


        public string StopTeachingPathRecording()
        {
            if (!EnsureReadyForCommand("경로 기록 중지"))
            {
                return GetTeachingPathRecordingSummaryForDebug();
            }

            teachingPathRecorder ??= new TeachingPathRecorder();
            teachingPathRecorder.Capture(currentState, Time.timeAsDouble, force: true);
            teachingPathRecorder.Stop();
            recordedPathSequence = teachingPathRecorder.BuildSequence(RecordedPathSequenceName);
            var count = recordedPathSequence.waypoints?.Length ?? 0;
            if (count >= 2)
            {
                WaypointStore.Save(recordedPathSequence);
                PushFeedback($"[Path Record] 기록 저장 · {count}개 샘플 → {RecordedPathSequenceName}");
            }
            else
            {
                PushFeedback("[Path Record] 저장할 움직임이 부족하다. 최소 2개 자세가 필요함.");
            }

            RefreshSnapshot();
            return GetTeachingPathRecordingSummaryForDebug();
        }


        public string CaptureTeachingPathFrameForDebug()
        {
            if (!EnsureReadyForCommand("경로 샘플 캡처"))
            {
                return GetTeachingPathRecordingSummaryForDebug();
            }

            teachingPathRecorder ??= new TeachingPathRecorder();
            teachingPathRecorder.Capture(currentState, Time.timeAsDouble, force: true);
            RefreshSnapshot();
            return GetTeachingPathRecordingSummaryForDebug();
        }


        public string PlayRecordedTeachingPathOnce()
        {
            return PlayRecordedTeachingPath(loop: false);
        }


        public string PlayRecordedTeachingPathLoop()
        {
            return PlayRecordedTeachingPath(loop: true);
        }


        public string GetTeachingPathRecordingSummaryForDebug()
        {
            var recorder = teachingPathRecorder?.ToDebugSummary() ?? "recording=False; samples=0";
            var saved = ResolveRecordedPathSequence();
            var savedCount = saved?.waypoints?.Length ?? 0;
            var runnerState = waypointRunner != null ? waypointRunner.State.ToString() : "missing";
            return $"{recorder}; saved={savedCount}; runner={runnerState}; sequence={RecordedPathSequenceName}; feedback={snapshot.LastFeedback}";
        }


        public string SimulateManualReadbackForDebug(double[] jointsDeg, double[] tcpMm)
        {
            ForceInitialize();
            manualReadbackTeachingProbe ??= new ManualReadbackTeachingProbe(connectionService);
            var result = manualReadbackTeachingProbe.SimulateManualMove(jointsDeg, tcpMm);
            RefreshSnapshot();
            return result.IsSuccess
                ? $"manualReadback=True; {FormatRobotStateForDebug(result.Value)}; {GetDebugSummary()}"
                : $"manualReadback=False; error={result.Message}; {GetDebugSummary()}";
        }
    }
}
