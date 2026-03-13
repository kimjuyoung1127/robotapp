// Folder: App - application orchestration and runtime state.
using KineTutor3D.Templates;
using UnityEngine;

namespace KineTutor3D.App
{
    internal readonly struct HomeContinueContextInfo
    {
        public HomeContinueContextInfo(string summary, bool canContinue)
        {
            Summary = summary;
            CanContinue = canContinue;
        }

        public string Summary { get; }
        public bool CanContinue { get; }
    }

    internal sealed class HomeContinueHubFlowService
    {
        private const string DefaultRobotId = "2DOF_RR";

        public HomeContinueContextInfo GetContextInfo()
        {
            if (!TryGetValidContext(out var data))
            {
                return new HomeContinueContextInfo(
                    "최근 이어하기 정보가 없습니다. 새로 시작하거나 로봇을 선택하세요.",
                    false);
            }

            var robotLabel = RobotCatalog.TryGet(data.RobotId, out var entry)
                ? entry.Metadata.DisplayName
                : data.RobotId;
            var modeLabel = string.Equals(data.EntryMode, RobotSelectionBridge.SandboxMode, System.StringComparison.Ordinal)
                ? "Sandbox"
                : "Guided Lesson";
            var trackLabel = FormatTrackSummary(data.Track, data.Step);

            return new HomeContinueContextInfo(
                $"최근 학습: {trackLabel} / {robotLabel} / {modeLabel}",
                true);
        }

        public bool ContinueLatestContext()
        {
            if (!TryGetValidContext(out var data))
            {
                return false;
            }

            StepProgressSaver.SetCurrentTrack(string.IsNullOrWhiteSpace(data.Track)
                ? StepProgressSaver.CoreKinematicsTrack
                : data.Track);

            if (!string.IsNullOrWhiteSpace(data.Track))
            {
                StepProgressSaver.SaveLastCompletedStep(data.Track, Mathf.Max(0, data.Step - 1));
            }

            RobotSelectionBridge.SetSelection(data.RobotId, data.EntryMode);
            SceneNavigator.Load(string.Equals(data.EntryMode, RobotSelectionBridge.SandboxMode, System.StringComparison.Ordinal)
                ? SceneId.Sandbox
                : ResolveLearningScene(data.Track));
            return true;
        }

        public void StartGuidedLesson()
        {
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.CoreKinematicsTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.CoreKinematicsTrack, 0);
            RobotSelectionBridge.SetSelection(ResolveStartRobotId(StepProgressSaver.CoreKinematicsTrack), RobotSelectionBridge.GuidedLessonMode);
            SceneNavigator.Load(SceneId.Main);
        }

        public void StartMathReadiness()
        {
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.MathReadinessTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.MathReadinessTrack, 0);
            RobotSelectionBridge.SetSelection(DefaultRobotId, RobotSelectionBridge.GuidedLessonMode);
            SceneNavigator.Load(SceneId.MathReadiness);
        }

        public void OpenRobotLibrary()
        {
            SceneNavigator.Load(SceneId.RobotLibrary);
        }

        public void OpenSandbox()
        {
            RobotSelectionBridge.SetSelection(ResolveFallbackRobotId(), RobotSelectionBridge.SandboxMode);
            SceneNavigator.Load(SceneId.Sandbox);
        }

        private static bool TryGetValidContext(out SessionContextData data)
        {
            return SessionContextStore.TryLoad(out data) && RobotCatalog.TryGet(data.RobotId, out _);
        }

        private static string ResolveFallbackRobotId()
        {
            if (SessionContextStore.TryLoad(out var data) && RobotCatalog.HasTemplate(data.RobotId))
            {
                return data.RobotId;
            }

            return DefaultRobotId;
        }

        private static string ResolveStartRobotId(string track)
        {
            if (string.Equals(track, StepProgressSaver.MathReadinessTrack, System.StringComparison.Ordinal) ||
                string.Equals(track, StepProgressSaver.PreKinematicsTrack, System.StringComparison.Ordinal))
            {
                return DefaultRobotId;
            }

            return ResolveFallbackRobotId();
        }

        private static string FormatTrackSummary(string track, int step)
        {
            if (string.Equals(track, StepProgressSaver.MathReadinessTrack, System.StringComparison.Ordinal))
            {
                return $"수학 기초 M{Mathf.Max(0, step - 1)}";
            }

            if (string.Equals(track, StepProgressSaver.PreKinematicsTrack, System.StringComparison.Ordinal))
            {
                return $"초보 Lesson L{Mathf.Max(0, step - 1)}";
            }

            return $"코어 Step {Mathf.Max(1, step)}";
        }

        private static SceneId ResolveLearningScene(string track)
        {
            return string.Equals(track, StepProgressSaver.MathReadinessTrack, System.StringComparison.Ordinal)
                ? SceneId.MathReadiness
                : SceneId.Main;
        }
    }
}
