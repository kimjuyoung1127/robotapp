// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KineTutor3D.App
{
    /// <summary>
    /// 게임 씬의 메인 카메라 구도와 FOV를 한 곳에서 관리합니다.
    /// CameraCaptureTool로 캡처한 오버라이드가 있으면 하드코딩 프로필 대신 적용합니다.
    /// </summary>
    public static class SceneCameraDirector
    {
        private const string OverrideKeyPrefix = "KineTutor3D_CameraOverride_";

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

        [Serializable]
        private struct OverrideData
        {
            public float posX, posY, posZ;
            public float eulerX, eulerY, eulerZ;
            public float fov;
            public float nearClip, farClip;
            public float bgR, bgG, bgB;
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

            var camera = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
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
            if (camera == null)
            {
                return;
            }

#if UNITY_EDITOR
            string sceneName = SceneCatalog.GetSceneName(sceneId);
            if (!string.IsNullOrEmpty(sceneName) && TryGetOverride(sceneName, out var overrideProfile))
            {
                ApplyProfile(camera, overrideProfile);
                return;
            }
#endif

            if (!TryGetProfile(sceneId, out var profile))
            {
                return;
            }

            ApplyProfile(camera, profile);
        }

        private static void ApplyProfile(Camera camera, CameraProfile profile)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = profile.Background;
            camera.transform.position = profile.Position;
            camera.transform.rotation = Quaternion.Euler(profile.EulerAngles);
            camera.fieldOfView = profile.FieldOfView;
            camera.nearClipPlane = profile.NearClip;
            camera.farClipPlane = profile.FarClip;
        }

#if UNITY_EDITOR
        private static bool TryGetOverride(string sceneName, out CameraProfile profile)
        {
            string json = EditorPrefs.GetString(OverrideKeyPrefix + sceneName, "");
            if (string.IsNullOrEmpty(json))
            {
                profile = default;
                return false;
            }

            var data = JsonUtility.FromJson<OverrideData>(json);
            profile = new CameraProfile(
                new Vector3(data.posX, data.posY, data.posZ),
                new Vector3(data.eulerX, data.eulerY, data.eulerZ),
                data.fov,
                data.nearClip,
                data.farClip,
                new Color(data.bgR, data.bgG, data.bgB, 1f));
            return true;
        }

        /// <summary>
        /// 특정 씬의 카메라 오버라이드를 제거합니다.
        /// </summary>
        public static void ClearOverride(string sceneName)
        {
            EditorPrefs.DeleteKey(OverrideKeyPrefix + sceneName);
        }

        /// <summary>
        /// 특정 씬에 카메라 오버라이드가 존재하는지 확인합니다.
        /// </summary>
        public static bool HasOverride(string sceneName)
        {
            return EditorPrefs.HasKey(OverrideKeyPrefix + sceneName);
        }
#endif

        private static bool TryGetProfile(SceneId sceneId, out CameraProfile profile)
        {
            switch (sceneId)
            {
                case SceneId.Sandbox:
                    profile = new CameraProfile(
                        new Vector3(0f, 0.8f, -2.5f),
                        new Vector3(10f, 0f, 0f),
                        40f,
                        0.01f,
                        1000f,
                        new Color(0.10f, 0.10f, 0.18f, 1f));
                    return true;
                case SceneId.MathReadiness:
                    profile = new CameraProfile(
                        new Vector3(0f, 1.62f, -6.1f),
                        new Vector3(6f, 0f, 0f),
                        74f,
                        0.3f,
                        1000f,
                        new Color(0.10f, 0.10f, 0.18f, 1f));
                    return true;
                case SceneId.RobotControl:
                case SceneId.RobotControlV2:
                case SceneId.RobotControlV3:
                    profile = new CameraProfile(
                        new Vector3(-1.39f, 0.55f, -2.35f),
                        new Vector3(6.78f, 31.61f, 0f),
                        40f,
                        0.01f,
                        30f,
                        new Color(0.08f, 0.10f, 0.16f, 1f));
                    return true;
                case SceneId.Onboarding:
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
