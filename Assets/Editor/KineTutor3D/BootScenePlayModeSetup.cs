// Editor-only: ensures Play Mode always starts from Boot.unity
// regardless of which scene is currently open in the editor.
using UnityEditor;
using UnityEditor.SceneManagement;

namespace KineTutor3D.Editor
{
    [InitializeOnLoad]
    internal static class BootScenePlayModeSetup
    {
        private const string BootScenePath = "Assets/Scenes/Boot.unity";
        private const string MenuPath = "KineTutor3D/Always Start From Boot";
        private const string PrefKey = "KineTutor3D.AlwaysStartFromBoot";

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
        private static void ToggleAlwaysStartFromBoot()
        {
            IsEnabled = !IsEnabled;
            ApplyIfEnabled();
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleAlwaysStartFromBootValidate()
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

            var bootScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath);
            if (bootScene != null)
            {
                EditorSceneManager.playModeStartScene = bootScene;
            }
        }
    }
}
