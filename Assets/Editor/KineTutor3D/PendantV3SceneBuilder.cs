// Folder: Editor - Authoring and QA utilities for Unity scenes and tools.
using System.Collections.Generic;
using KineTutor3D.Editor.CliTools;
using KineTutor3D.UI.RobotControlV3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace KineTutor3D.EditorTools
{
    /// <summary>
    /// RobotControlV3 최소 authored 씬을 보장하는 에디터 유틸입니다.
    /// </summary>
    public static class PendantV3SceneBuilder
    {
        private const string MenuPath = "KineTutor3D/RobotControl/Author V3 Shell";
        private const string PopupOpenMenuPath = "KineTutor3D/RobotControl/V3 Popup Probe/Open";
        private const string PopupCloseMenuPath = "KineTutor3D/RobotControl/V3 Popup Probe/Close";
        private const string ScenePath = "Assets/Scenes/RobotControlV3.unity";
        private const string PanelSettingsPath = "Assets/UI/PendantV3/PanelSettings/PendantV3PanelSettings.asset";
        private const string RootVisualTreePath = "Assets/UI/PendantV3/pendant-v3.uxml";

        [MenuItem(MenuPath, priority = 172)]
        public static void AuthorSceneMenu()
        {
            var saved = AuthorScene();
            Debug.Log("[PendantV3SceneBuilder] RobotControlV3 scene authored: " + saved);
        }

        [MenuItem(PopupOpenMenuPath, priority = 173)]
        public static void OpenPopupProbeMenu()
        {
            var summary = OpenPopupProbe();
            Debug.Log("[PendantV3SceneBuilder] " + summary);
        }

        [MenuItem(PopupCloseMenuPath, priority = 174)]
        public static void ClosePopupProbeMenu()
        {
            var summary = ClosePopupProbe();
            Debug.Log("[PendantV3SceneBuilder] " + summary);
        }

        public static string AuthorSceneSafe()
        {
            try
            {
                var saved = AuthorScene();
                var absoluteScenePath = System.IO.Path.GetFullPath(ScenePath);
                var exists = System.IO.File.Exists(absoluteScenePath);
                return $"saved={saved}; exists={exists}; path={absoluteScenePath}";
            }
            catch (System.Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string GetInputContractSummary()
        {
            return GetInputContract().GetDebugStateSummary();
        }

        public static string OpenPopupProbe()
        {
            var contract = GetInputContract();
            contract.OpenPopupProbeForDebug();
            return contract.GetDebugStateSummary();
        }

        public static string ClosePopupProbe()
        {
            var contract = GetInputContract();
            contract.ClosePopupProbeForDebug();
            return contract.GetDebugStateSummary();
        }

        public static bool AuthorScene()
        {
            PendantV3BootstrapTool.EnsurePhase0Assets();

            var scene = EnsureSceneOpen();
            EnsureSceneGameObjects();
            EnsureBuildSettingsScene();

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return EditorSceneManager.SaveScene(scene, ScenePath, true);
        }

        private static Scene EnsureSceneOpen()
        {
            var absoluteScenePath = System.IO.Path.GetFullPath(ScenePath);
            if (System.IO.File.Exists(absoluteScenePath))
            {
                return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static void EnsureSceneGameObjects()
        {
            EnsureMainCamera();
            EnsureEventSystem();
            EnsureSceneBootstrap();
            EnsureDocumentRoot();
        }

        private static void EnsureMainCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                camera = Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
            }

            var cameraObject = camera != null ? camera.gameObject : new GameObject("Main Camera");
            cameraObject.name = "Main Camera";
            camera = cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.08f, 0.11f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 1.35f, -2.8f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            var eventSystemObject = eventSystem != null ? eventSystem.gameObject : new GameObject("EventSystem");
            eventSystemObject.name = "EventSystem";
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            var inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            }

            var standaloneModule = eventSystemObject.GetComponent<StandaloneInputModule>();
            if (standaloneModule != null)
            {
                Object.DestroyImmediate(standaloneModule, true);
            }

            if (inputModule != null)
            {
                inputModule.moveRepeatDelay = 0.35f;
                inputModule.moveRepeatRate = 0.08f;
            }
        }

        private static void EnsureSceneBootstrap()
        {
            var sceneBootstrap = GameObject.Find("SceneBootstrap") ?? new GameObject("SceneBootstrap");
            sceneBootstrap.name = "SceneBootstrap";
        }

        private static void EnsureDocumentRoot()
        {
            var documentObject = GameObject.Find("PendantV3Root") ?? new GameObject("PendantV3Root");
            documentObject.name = "PendantV3Root";

            var uiDocument = documentObject.GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = documentObject.AddComponent<UIDocument>();
            }

            var documentBridge = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.PendantV3Document>();
            if (documentBridge == null)
            {
                documentBridge = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.PendantV3Document>();
            }

            var inputContract = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.PendantV3InputContract>();
            if (inputContract == null)
            {
                inputContract = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.PendantV3InputContract>();
            }

            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RootVisualTreePath);
            if (panelSettings == null)
            {
                throw new MissingReferenceException($"PanelSettings not found: {PanelSettingsPath}");
            }

            if (visualTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {RootVisualTreePath}");
            }

            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = visualTree;
            documentBridge.enabled = true;
            inputContract.enabled = true;
        }

        private static void EnsureBuildSettingsScene()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var index = scenes.FindIndex(scene => scene.path == ScenePath);
            if (index >= 0)
            {
                scenes[index].enabled = true;
            }
            else
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static PendantV3InputContract GetInputContract()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var contract = Object.FindFirstObjectByType<PendantV3InputContract>(FindObjectsInactive.Include);
            if (contract == null)
            {
                throw new MissingReferenceException("PendantV3InputContract not found in RobotControlV3 scene.");
            }

            return contract;
        }
    }
}
