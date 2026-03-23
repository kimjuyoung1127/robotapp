// Folder: App - Application controllers and services; single UnityEngine entry point.
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// 첫 방문 여부에 따라 시작 씬을 결정합니다.
    /// </summary>
    public class BootSceneRouter : MonoBehaviour
    {
        private void Awake()
        {
            EnsureFallbackCamera();
        }

        private void Start()
        {
            var target = StepProgressSaver.HasVisited() ? SceneId.RobotLibrary : SceneId.Onboarding;
            SceneNavigator.Load(target);
        }

        private static void EnsureFallbackCamera()
        {
            if (Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.tag = "MainCamera";
            SceneCameraDirector.ConfigureForCurrentScene(camera);
        }
    }
}
