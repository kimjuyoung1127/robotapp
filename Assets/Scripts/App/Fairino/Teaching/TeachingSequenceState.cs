// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// Pendant V3 teaching sequence의 현재 실행/선택 상태입니다.
    /// </summary>
    public readonly struct TeachingSequenceState
    {
        public TeachingSequenceState(
            string sequenceName,
            int pointCount,
            int selectedIndex,
            int runningIndex,
            bool isRunning,
            bool isLooping,
            string runMode,
            string feedback)
        {
            SequenceName = sequenceName ?? string.Empty;
            PointCount = pointCount;
            SelectedIndex = selectedIndex;
            RunningIndex = runningIndex;
            IsRunning = isRunning;
            IsLooping = isLooping;
            RunMode = runMode ?? "Idle";
            Feedback = feedback ?? string.Empty;
        }

        public string SequenceName { get; }
        public int PointCount { get; }
        public int SelectedIndex { get; }
        public int RunningIndex { get; }
        public bool IsRunning { get; }
        public bool IsLooping { get; }
        public string RunMode { get; }
        public string Feedback { get; }

        public string ToDebugSummary()
        {
            return $"sequence={SequenceName}; count={PointCount}; selected={SelectedIndex}; running={RunningIndex}; isRunning={IsRunning}; loop={IsLooping}; mode={RunMode}; feedback={Feedback}";
        }
    }
}
