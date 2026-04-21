// Editor-only: ensures Play Mode always starts from Onboarding.unity
// regardless of which scene is currently open in the editor.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KineTutor3D.Editor
{
    [InitializeOnLoad]
    public static class BootScenePlayModeSetup
    {
        private const string StartScenePath = "Assets/Scenes/Onboarding.unity";
        private const string MenuPath = "KineTutor3D/Always Start From Onboarding";
        private const string PrefKey = "KineTutor3D.AlwaysStartFromOnboarding";

        static BootScenePlayModeSetup()
        {
            EditorApplication.delayCall += ApplyIfEnabled;
            EditorSceneManager.activeSceneChangedInEditMode += HandleActiveSceneChangedInEditMode;
            EditorSceneManager.sceneOpened += HandleSceneOpened;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
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

        public static string SetAlwaysStartFromOnboarding(bool enabled)
        {
            IsEnabled = enabled;
            ApplyIfEnabled();
            return GetDebugSummary();
        }

        public static string GetDebugSummary()
        {
            var startScene = EditorSceneManager.playModeStartScene != null
                ? AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene)
                : "null";
            return $"alwaysStart={IsEnabled}; playModeStartScene={startScene}";
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleAlwaysStartFromOnboardingValidate()
        {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        private static void ApplyIfEnabled()
        {
            if (Application.isBatchMode)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

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

        private static void HandleActiveSceneChangedInEditMode(UnityEngine.SceneManagement.Scene _oldScene, UnityEngine.SceneManagement.Scene _newScene)
        {
            ApplyIfEnabled();
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode || change == PlayModeStateChange.ExitingEditMode)
            {
                ApplyIfEnabled();
            }
        }

        private static void HandleSceneOpened(UnityEngine.SceneManagement.Scene _scene, OpenSceneMode _mode)
        {
            ApplyIfEnabled();
        }
    }
}
