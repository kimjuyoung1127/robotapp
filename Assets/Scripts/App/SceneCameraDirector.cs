// Folder: App - Application controllers and services; single UnityEngine entry point.
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KineTutor3D.App
{
    /// <summary>
    /// 게임 씬의 메인 카메라 구도와 FOV를 한 곳에서 관리합니다.
    /// </summary>
    public static class SceneCameraDirector
    {
        private readonly struct CameraProfile
        {
            public CameraProfile(Vector3 position, Vector3 eulerAngles, float fieldOfView, float nearClip, float farClip, Color background)
            {
                Position = position;
                EulerAngles = eulerAngles;
                FieldOfView = fieldOfView;
                NearClip = nearClip;
                FarClip = farClip;
                Background = background;
            }

            public Vector3 Position { get; }
            public Vector3 EulerAngles { get; }
            public float FieldOfView { get; }
            public float NearClip { get; }
            public float FarClip { get; }
            public Color Background { get; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneHook()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode _)
        {
            if (!SceneCatalog.TryGetSceneId(scene.name, out var sceneId))
            {
                return;
            }

            var camera = Camera.main ?? Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
            ConfigureForScene(sceneId, camera);
        }

        public static void ConfigureForCurrentScene(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            ConfigureForScene(SceneCatalog.GetCurrentSceneId(), camera);
        }

        public static void ConfigureForScene(SceneId sceneId, Camera camera)
        {
            if (camera == null || !TryGetProfile(sceneId, out var profile))
            {
                return;
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = profile.Background;
            camera.transform.position = profile.Position;
            camera.transform.rotation = Quaternion.Euler(profile.EulerAngles);
            camera.fieldOfView = profile.FieldOfView;
            camera.nearClipPlane = profile.NearClip;
            camera.farClipPlane = profile.FarClip;
        }

        private static bool TryGetProfile(SceneId sceneId, out CameraProfile profile)
        {
            switch (sceneId)
            {
                case SceneId.Main:
                case SceneId.MathReadiness:
                case SceneId.Sandbox:
                    profile = new CameraProfile(
                        new Vector3(0f, 1.8f, -7.4f),
                        new Vector3(7f, 0f, 0f),
                        82f,
                        0.3f,
                        1000f,
                        new Color(0.10f, 0.10f, 0.18f, 1f));
                    return true;
                case SceneId.RobotControl:
                    profile = new CameraProfile(
                        new Vector3(1.0f, 0.75f, 1.0f),
                        new Vector3(22f, 215f, 0f),
                        40f,
                        0.01f,
                        30f,
                        new Color(0.08f, 0.10f, 0.16f, 1f));
                    return true;
                case SceneId.Onboarding:
                case SceneId.Home:
                    profile = new CameraProfile(
                        new Vector3(0f, 0f, -10f),
                        Vector3.zero,
                        60f,
                        0.3f,
                        1000f,
                        new Color(0.19f, 0.30f, 0.47f, 1f));
                    return true;
                default:
                    profile = default;
                    return false;
            }
        }
    }
}
