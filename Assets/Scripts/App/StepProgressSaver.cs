// Folder: App - application orchestration and runtime state.
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// 튜토리얼 방문/진행 상태를 로컬에 저장합니다.
    /// </summary>
    public static class StepProgressSaver
    {
        private const string HasVisitedKey = "KineTutor3D.HasVisited";
        private const string LastCompletedStepKey = "KineTutor3D.LastCompletedStep";
        private const string ReducedMotionKey = "KineTutor3D.ReducedMotion";

        public static bool HasVisited()
        {
            return PlayerPrefs.GetInt(HasVisitedKey, 0) == 1;
        }

        public static void MarkVisited()
        {
            PlayerPrefs.SetInt(HasVisitedKey, 1);
            PlayerPrefs.Save();
        }

        public static void SaveLastCompletedStep(int step)
        {
            PlayerPrefs.SetInt(LastCompletedStepKey, Mathf.Max(0, step));
            PlayerPrefs.Save();
        }

        public static int GetLastCompletedStep()
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(LastCompletedStepKey, 0));
        }

        public static int GetResumeStep(int defaultStep)
        {
            var resume = GetLastCompletedStep() + 1;
            return Mathf.Max(defaultStep, resume);
        }

        public static bool GetReducedMotion()
        {
            return PlayerPrefs.GetInt(ReducedMotionKey, 0) == 1;
        }

        public static void SetReducedMotion(bool enabled)
        {
            PlayerPrefs.SetInt(ReducedMotionKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}

