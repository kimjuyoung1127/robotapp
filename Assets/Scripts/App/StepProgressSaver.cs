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
        private const string ReducedMotionKey = "KineTutor3D.ReducedMotion";
        private const string TrackKey = "KineTutor3D.CurrentTrack";
        private const string PreKinematicsLastCompletedStepKey = "KineTutor3D.PreKinematics.LastCompletedStep";
        private const string CoreKinematicsLastCompletedStepKey = "KineTutor3D.CoreKinematics.LastCompletedStep";

        public const string PreKinematicsTrack = "pre_kinematics";
        public const string CoreKinematicsTrack = "core_kinematics";

        public static bool HasVisited()
        {
            return PlayerPrefs.GetInt(HasVisitedKey, 0) == 1;
        }

        public static void MarkVisited()
        {
            PlayerPrefs.SetInt(HasVisitedKey, 1);
            PlayerPrefs.Save();
        }

        public static string GetCurrentTrack()
        {
            return NormalizeTrack(PlayerPrefs.GetString(TrackKey, CoreKinematicsTrack));
        }

        public static void SetCurrentTrack(string track)
        {
            PlayerPrefs.SetString(TrackKey, NormalizeTrack(track));
            PlayerPrefs.Save();
        }

        public static void SaveLastCompletedStep(int step)
        {
            SaveLastCompletedStep(CoreKinematicsTrack, step);
        }

        public static int GetLastCompletedStep()
        {
            return GetLastCompletedStep(CoreKinematicsTrack);
        }

        public static void SaveLastCompletedStep(string track, int step)
        {
            PlayerPrefs.SetInt(ResolveLastCompletedStepKey(track), Mathf.Max(0, step));
            PlayerPrefs.Save();
        }

        public static int GetLastCompletedStep(string track)
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(ResolveLastCompletedStepKey(track), 0));
        }

        public static int GetResumeStep(int defaultStep)
        {
            return GetResumeStep(CoreKinematicsTrack, defaultStep);
        }

        public static int GetResumeStep(string track, int defaultStep)
        {
            var resume = GetLastCompletedStep(track) + 1;
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

        private static string ResolveLastCompletedStepKey(string track)
        {
            return NormalizeTrack(track) == PreKinematicsTrack
                ? PreKinematicsLastCompletedStepKey
                : CoreKinematicsLastCompletedStepKey;
        }

        private static string NormalizeTrack(string track)
        {
            return string.Equals(track, PreKinematicsTrack, System.StringComparison.Ordinal)
                ? PreKinematicsTrack
                : CoreKinematicsTrack;
        }
    }
}

