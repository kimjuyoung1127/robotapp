// Editor-only: ensures Play Mode always starts from Onboarding.unity
// regardless of which scene is currently open in the editor.
using UnityEditor;
using UnityEditor.SceneManagement;

namespace KineTutor3D.Editor
{
    [InitializeOnLoad]
    internal static class BootScenePlayModeSetup
    {
        private const string StartScenePath = "Assets/Scenes/Onboarding.unity";
        private const string MenuPath = "KineTutor3D/Always Start From Onboarding";
        private const string PrefKey = "KineTutor3D.AlwaysStartFromOnboarding";

        static BootScenePlayModeSetup()
        {
            EditorApplication.delayCall += ApplyIfEnabled;
        }

        private static bool IsEnabled
        {
            get => EditorPrefs.GetBool(PrefKey, true);
            set => EditorPrefs.SetBool(PrefKey, value);
        }

        [MenuItem(MenuPath, priority = 200)]
        private static void ToggleAlwaysStartFromOnboarding()
        {
            IsEnabled = !IsEnabled;
            ApplyIfEnabled();
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleAlwaysStartFromOnboardingValidate()
        {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        private static void ApplyIfEnabled()
        {
            if (!IsEnabled)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            var startScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePath);
            if (startScene != null)
            {
                EditorSceneManager.playModeStartScene = startScene;
            }
        }
    }
}
