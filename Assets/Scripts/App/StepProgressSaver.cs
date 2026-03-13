// Folder: App - Application controllers and services; single UnityEngine entry point.
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
        private const string MathReadinessLastCompletedStepKey = "KineTutor3D.MathReadiness.LastCompletedStep";
        private const string PreKinematicsLastCompletedStepKey = "KineTutor3D.PreKinematics.LastCompletedStep";
        private const string CoreKinematicsLastCompletedStepKey = "KineTutor3D.CoreKinematics.LastCompletedStep";

        public const string MathReadinessTrack = "math_readiness";
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
            switch (NormalizeTrack(track))
            {
                case MathReadinessTrack:
                    return MathReadinessLastCompletedStepKey;
                case PreKinematicsTrack:
                    return PreKinematicsLastCompletedStepKey;
                default:
                    return CoreKinematicsLastCompletedStepKey;
            }
        }

        private static string NormalizeTrack(string track)
        {
            if (string.Equals(track, MathReadinessTrack, System.StringComparison.Ordinal))
            {
                return MathReadinessTrack;
            }

            return string.Equals(track, PreKinematicsTrack, System.StringComparison.Ordinal)
                ? PreKinematicsTrack
                : CoreKinematicsTrack;
        }
    }
}

