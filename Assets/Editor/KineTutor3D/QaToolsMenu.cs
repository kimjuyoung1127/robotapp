// Editor-only: QA helper tools for testing the full user flow.
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor
{
    internal static class QaToolsMenu
    {
        private const string HasVisitedKey = "KineTutor3D.HasVisited";
        private const string TrackKey = "KineTutor3D.CurrentTrack";
        private const string MathReadinessLastCompletedStepKey = "KineTutor3D.MathReadiness.LastCompletedStep";
        private const string PreKinematicsLastCompletedStepKey = "KineTutor3D.PreKinematics.LastCompletedStep";
        private const string CoreKinematicsLastCompletedStepKey = "KineTutor3D.CoreKinematics.LastCompletedStep";
        private const string SessionContextKey = "KineTutor3D.SessionContextJson";
        private const string ReducedMotionKey = "KineTutor3D.ReducedMotion";
        private const string SelectedRobotIdKey = "KineTutor3D.SelectedRobotId";
        private const string SelectedModeKey = "KineTutor3D.SelectedMode";

        private const string CoreTrack = "core_kinematics";
        private const string MathReadinessTrack = "math_readiness";
        private const string GuidedLessonMode = "guided_lesson";
        private const string SandboxMode = "sandbox";
        private const string DefaultRobotId = "2DOF_RR";

        [MenuItem("KineTutor3D/QA: Reset to First-Time User", priority = 100)]
        private static void ResetToFirstTimeUser()
        {
            ClearQaState();
            PlayerPrefs.Save();
            Debug.Log("[QA] PlayerPrefs cleared — next Play will start from Onboarding.");
        }

        [MenuItem("KineTutor3D/QA: Reset to Returning User (skip onboarding)", priority = 101)]
        private static void ResetToReturningUser()
        {
            PlayerPrefs.SetInt("KineTutor3D.HasVisited", 1);
            PlayerPrefs.DeleteKey("KineTutor3D.CurrentTrack");
            PlayerPrefs.DeleteKey("KineTutor3D.MathReadiness.LastCompletedStep");
            PlayerPrefs.DeleteKey("KineTutor3D.PreKinematics.LastCompletedStep");
            PlayerPrefs.DeleteKey("KineTutor3D.CoreKinematics.LastCompletedStep");
            PlayerPrefs.DeleteKey("KineTutor3D.SessionContextJson");
            PlayerPrefs.Save();
            Debug.Log("[QA] PlayerPrefs set to returning user — next Play will start from Home.");
        }

        [MenuItem("KineTutor3D/QA: Prep Home / Continue Hub", priority = 110)]
        private static void PrepHomeContinueHub()
        {
            ResetToReturningUser();
            Debug.Log("[QA] Home / Continue Hub 준비 완료 — Play 후 Home에서 시작합니다.");
        }

        [MenuItem("KineTutor3D/QA: Prep Guided Lesson (Core Step 1)", priority = 111)]
        private static void PrepGuidedLessonCore()
        {
            ClearQaState();
            PlayerPrefs.SetInt(HasVisitedKey, 1);
            PlayerPrefs.SetString(TrackKey, CoreTrack);
            PlayerPrefs.SetInt(CoreKinematicsLastCompletedStepKey, 0);
            PlayerPrefs.SetString(SelectedRobotIdKey, DefaultRobotId);
            PlayerPrefs.SetString(SelectedModeKey, GuidedLessonMode);
            PlayerPrefs.Save();
            Debug.Log("[QA] Guided Lesson Core 준비 완료 — Play 후 Home에서 '학습 시작'을 눌러 Main으로 진입하세요.");
        }

        [MenuItem("KineTutor3D/QA: Prep Math Readiness", priority = 112)]
        private static void PrepMathReadiness()
        {
            ClearQaState();
            PlayerPrefs.SetInt(HasVisitedKey, 1);
            PlayerPrefs.SetString(TrackKey, MathReadinessTrack);
            PlayerPrefs.SetInt(MathReadinessLastCompletedStepKey, 0);
            PlayerPrefs.SetString(SelectedRobotIdKey, DefaultRobotId);
            PlayerPrefs.SetString(SelectedModeKey, GuidedLessonMode);
            PlayerPrefs.Save();
            Debug.Log("[QA] Math Readiness 준비 완료 — Play 후 Home에서 '수학 기초 워밍업'을 눌러 Main으로 진입하세요.");
        }

        [MenuItem("KineTutor3D/QA: Prep Robot Library", priority = 113)]
        private static void PrepRobotLibrary()
        {
            ResetToReturningUser();
            PlayerPrefs.SetString(SelectedRobotIdKey, DefaultRobotId);
            PlayerPrefs.SetString(SelectedModeKey, GuidedLessonMode);
            PlayerPrefs.Save();
            Debug.Log("[QA] Robot Library 준비 완료 — Play 후 Home에서 '로봇 선택'을 눌러 Robot Library로 진입하세요.");
        }

        [MenuItem("KineTutor3D/QA: Prep Sandbox", priority = 114)]
        private static void PrepSandbox()
        {
            ClearQaState();
            PlayerPrefs.SetInt(HasVisitedKey, 1);
            PlayerPrefs.SetString(TrackKey, CoreTrack);
            PlayerPrefs.SetString(SelectedRobotIdKey, DefaultRobotId);
            PlayerPrefs.SetString(SelectedModeKey, SandboxMode);
            PlayerPrefs.Save();
            Debug.Log("[QA] Sandbox 준비 완료 — Play 후 Home에서 '샌드박스'를 눌러 Sandbox로 진입하세요.");
        }

        private static void ClearQaState()
        {
            PlayerPrefs.DeleteKey(HasVisitedKey);
            PlayerPrefs.DeleteKey(TrackKey);
            PlayerPrefs.DeleteKey(MathReadinessLastCompletedStepKey);
            PlayerPrefs.DeleteKey(PreKinematicsLastCompletedStepKey);
            PlayerPrefs.DeleteKey(CoreKinematicsLastCompletedStepKey);
            PlayerPrefs.DeleteKey(SessionContextKey);
            PlayerPrefs.DeleteKey(ReducedMotionKey);
            PlayerPrefs.DeleteKey(SelectedRobotIdKey);
            PlayerPrefs.DeleteKey(SelectedModeKey);
        }
    }
}
