// Folder: App - scene/runtime guard for third-party robot controller components.
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KineTutor3D.App
{
    /// <summary>
    /// URDF Importer가 자동으로 붙이는 legacy Controller를 Input System 프로젝트에서 조기에 비활성화합니다.
    /// </summary>
    public static class UrdfLegacyControllerGuard
    {
        private const string LegacyControllerTypeName = "Unity.Robotics.UrdfImporter.Control.Controller";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            DisableControllersInAllLoadedScenes();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode _mode)
        {
            DisableControllersInScene(scene);
        }

        private static void DisableControllersInAllLoadedScenes()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                DisableControllersInScene(SceneManager.GetSceneAt(i));
            }
        }

        private static void DisableControllersInScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            var disabledCount = 0;
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null)
                {
                    continue;
                }

                var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
                for (var j = 0; j < behaviours.Length; j++)
                {
                    var behaviour = behaviours[j];
                    if (behaviour == null || !behaviour.enabled)
                    {
                        continue;
                    }

                    if (behaviour.GetType().FullName == LegacyControllerTypeName)
                    {
                        behaviour.enabled = false;
                        disabledCount++;
                    }
                }
            }

            if (disabledCount > 0)
            {
                Debug.Log($"[UrdfLegacyControllerGuard] Disabled {disabledCount} legacy URDF controller(s) in scene '{scene.name}'.");
            }
        }
    }
}
