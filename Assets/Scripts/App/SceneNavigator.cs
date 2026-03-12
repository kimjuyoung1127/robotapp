// Folder: App - Application controllers and services; single UnityEngine entry point.
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KineTutor3D.App
{
    /// <summary>
    /// 씬 전환 진입점을 제공합니다.
    /// </summary>
    public static class SceneNavigator
    {
        public static void Load(SceneId target)
        {
            var sceneName = SceneCatalog.GetSceneName(target);
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError($"[SceneNavigator] 미등록 씬입니다: {target}");
                return;
            }

            LoadByName(sceneName);
        }

        public static void LoadByName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[SceneNavigator] sceneName이 비어 있습니다.");
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
