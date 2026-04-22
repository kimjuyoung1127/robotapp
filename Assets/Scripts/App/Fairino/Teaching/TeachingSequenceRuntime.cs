// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// PendantV3Points를 실행 가능한 선택/미리보기 단위로 다루는 최소 런타임입니다.
    /// </summary>
    public sealed class TeachingSequenceRuntime
    {
        private readonly TeachingPointStoreAdapter storeAdapter;
        private WaypointSequence sequence;
        private int selectedIndex = -1;
        private int runningIndex = -1;
        private bool isRunning;
        private bool isLooping;
        private string runMode = "Idle";
        private string feedback = "teaching sequence not loaded";

        public TeachingSequenceRuntime(TeachingPointStoreAdapter storeAdapter)
        {
            this.storeAdapter = storeAdapter ?? new TeachingPointStoreAdapter();
        }

        public TeachingSequenceState State => new(
            TeachingPointStoreAdapter.DefaultSequenceName,
            Count,
            selectedIndex,
            runningIndex,
            isRunning,
            isLooping,
            runMode,
            feedback);

        public int Count => sequence?.waypoints?.Length ?? 0;

        public Waypoint SelectedPoint
            => selectedIndex >= 0 && sequence?.waypoints != null && selectedIndex < sequence.waypoints.Length
                ? sequence.waypoints[selectedIndex]
                : null;

        public TeachingSequenceState Load()
        {
            sequence = storeAdapter.LoadIfExists();
            selectedIndex = Count > 0 ? System.Math.Max(0, System.Math.Min(selectedIndex, Count - 1)) : -1;
            runningIndex = -1;
            isRunning = false;
            isLooping = false;
            runMode = "Idle";
            feedback = Count > 0
                ? $"loaded {Count} teaching point(s)"
                : "no saved teaching points";
            return State;
        }

        public TeachingSequenceState Select(int index)
        {
            EnsureLoaded();
            if (Count == 0)
            {
                selectedIndex = -1;
                feedback = "저장된 포인트가 없다.";
                return State;
            }

            selectedIndex = System.Math.Max(0, System.Math.Min(index, Count - 1));
            feedback = $"selected {selectedIndex + 1}/{Count} {SelectedPoint?.name}";
            runMode = "Preview";
            return State;
        }

        public TeachingSequenceState SelectNext()
        {
            EnsureLoaded();
            return Select(selectedIndex < 0 ? 0 : selectedIndex + 1);
        }

        public TeachingSequenceState SelectPrevious()
        {
            EnsureLoaded();
            return Select(selectedIndex < 0 ? 0 : selectedIndex - 1);
        }

        public FairinoResult PreviewSelected(Func<Waypoint, FairinoResult> preview)
        {
            EnsureLoaded();
            var point = SelectedPoint;
            if (point == null)
            {
                feedback = "미리보기할 포인트가 없다.";
                return FairinoResult.Fail(-90, feedback);
            }

            var result = preview != null
                ? preview(point)
                : FairinoResult.Fail(-91, "preview callback missing");
            feedback = result.IsSuccess
                ? $"preview {selectedIndex + 1}/{Count} {point.name}"
                : result.Message;
            runMode = "Preview";
            return result;
        }

        public FairinoResult ExecuteSelected(Func<Waypoint, FairinoResult> execute)
        {
            EnsureLoaded();
            var point = SelectedPoint;
            if (point == null)
            {
                feedback = "실행할 포인트가 없다.";
                return FairinoResult.Fail(-92, feedback);
            }

            runningIndex = selectedIndex;
            isRunning = true;
            runMode = "Step";
            var result = execute != null
                ? execute(point)
                : FairinoResult.Fail(-93, "execute callback missing");
            isRunning = false;
            feedback = result.IsSuccess
                ? $"executed {selectedIndex + 1}/{Count} {point.name}"
                : result.Message;
            if (!result.IsSuccess)
            {
                runningIndex = selectedIndex;
                return result;
            }

            runningIndex = -1;
            return result;
        }

        public string BuildSelectedPointDetail()
        {
            var point = SelectedPoint;
            if (point == null)
            {
                return "selected=none";
            }

            return $"selected={selectedIndex}; name={point.name}; move={point.moveType}; speed={point.speedPreset}; dwell={point.dwellSec:0.###}; joints=[{Format(point.jointsDeg)}]; tcp=[{Format(point.tcpMm)}]";
        }

        public string ToDebugSummary()
        {
            return $"{State.ToDebugSummary()}; detail=[{BuildSelectedPointDetail()}]";
        }

        private void EnsureLoaded()
        {
            if (sequence == null)
            {
                Load();
            }
        }

        private static string Format(double[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            var formatted = new string[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                formatted[i] = values[i].ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            }

            return string.Join(",", formatted);
        }
    }
}
