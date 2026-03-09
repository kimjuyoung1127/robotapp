// Folder: App - application orchestration and runtime state.
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KineTutor3D.App
{
    /// <summary>
    /// ?꾩뿭 ???꾪솚 吏꾩엯?먯쓣 ?쒓났?⑸땲??
    /// </summary>
    public static class SceneNavigator
    {
        public static void Load(SceneId target)
        {
            var sceneName = SceneCatalog.GetSceneName(target);
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError($"[SceneNavigator] 誘몃벑濡??ъ엯?덈떎: {target}");
                return;
            }

            LoadByName(sceneName);
        }

        public static void LoadByName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[SceneNavigator] sceneName??鍮꾩뼱 ?덉뒿?덈떎.");
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}

