// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App
{
    /// <summary>
    /// RobotControl 진입 맥락에 따라 로컬 UI 상태 reset/restore 정책을 적용합니다.
    /// </summary>
    public static class RobotControlEntryPolicy
    {
        private static bool pendingDirectFreshStartGuide;

        public enum Intent
        {
            FreshStart,
            ResumeLastSession,
        }

        public static void Apply(SceneId targetSceneId, Intent intent)
        {
            if (targetSceneId != SceneId.RobotControlV3)
            {
                return;
            }

            if (intent == Intent.FreshStart)
            {
                LocalSettingsStore.ResetForFreshStart();
                pendingDirectFreshStartGuide = !LocalSettingsStore.LoadOrDefault().HasShownFirstRunGuide;
                return;
            }

            pendingDirectFreshStartGuide = false;
        }

        public static bool ShouldShowFirstRunGuide()
        {
            return pendingDirectFreshStartGuide;
        }

        public static void MarkFirstRunGuideShown()
        {
            var state = LocalSettingsStore.LoadOrDefault();
            state.HasShownFirstRunGuide = true;
            LocalSettingsStore.Save(state);
            pendingDirectFreshStartGuide = false;
        }
    }
}
