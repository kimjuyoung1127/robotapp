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
        private const string ConnectionHomePath = "Assets/UI/PendantV3/connection-home.uxml";
        private const string CoordStripPath = "Assets/UI/PendantV3/coord-strip.uxml";
        private const string StatusCardPath = "Assets/UI/PendantV3/status-card.uxml";
        private const string EasyMotionPath = "Assets/UI/PendantV3/easy-motion-panel.uxml";
        private const string JointJogPath = "Assets/UI/PendantV3/joint-jog-panel.uxml";
        private const string TcpJogPath = "Assets/UI/PendantV3/tcp-jog-panel.uxml";
        private const string PointMovePath = "Assets/UI/PendantV3/point-move-panel.uxml";
        private const string SafetyDiagnosticsPath = "Assets/UI/PendantV3/safety-diagnostics-panel.uxml";
        private const string FaultOverlayPath = "Assets/UI/PendantV3/fault-overlay.uxml";
        private const string CartesianArrowsOverlayPath = "Assets/UI/PendantV3/cartesian-arrows-overlay.uxml";
        private const string ViewportToolbarPath = "Assets/UI/PendantV3/viewport-toolbar.uxml";
        private const string HelpPanelPath = "Assets/UI/PendantV3/help-panel.uxml";
        private const string ActionServoConfirmPath = "Assets/UI/PendantV3/popups/action-confirm.uxml";
        private const string ActionResetConfirmPath = "Assets/UI/PendantV3/popups/action-reset-confirm.uxml";
        private const string ActionRunConfirmPath = "Assets/UI/PendantV3/popups/action-run-confirm.uxml";
        private const string MoveConfirmPath = "Assets/UI/PendantV3/popups/move-confirm.uxml";
        private const string WarningDialogPath = "Assets/UI/PendantV3/popups/warning-dialog.uxml";
        private const string RecoveryDialogPath = "Assets/UI/PendantV3/popups/recovery-dialog.uxml";
        private const string UnsavedConfirmPath = "Assets/UI/PendantV3/popups/unsaved-confirm.uxml";
        private const string FirstRunGuidePath = "Assets/UI/PendantV3/popups/first-run-guide.uxml";

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

        public static string GetDocumentRootComponentSummary()
        {
            var documentObject = GameObject.Find("PendantV3Root");
            if (documentObject == null)
            {
                return "PendantV3Root missing";
            }

            var components = documentObject.GetComponents<Component>();
            var names = new List<string>();
            foreach (var component in components)
            {
                names.Add(component == null ? "MissingComponent" : component.GetType().Name);
            }

            return string.Join(", ", names);
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

            var layoutController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.PendantV3LayoutController>();
            if (layoutController == null)
            {
                layoutController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.PendantV3LayoutController>();
            }

            var shellStateController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.PendantV3ShellStateController>();
            if (shellStateController == null)
            {
                shellStateController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.PendantV3ShellStateController>();
            }

            var sceneCoordinator = documentObject.GetComponent<KineTutor3D.App.PendantV3SceneCoordinator>();
            if (sceneCoordinator == null)
            {
                sceneCoordinator = documentObject.AddComponent<KineTutor3D.App.PendantV3SceneCoordinator>();
            }

            var runtimeController = documentObject.GetComponent<KineTutor3D.App.Fairino.RobotControlV3RuntimeController>();
            if (runtimeController == null)
            {
                runtimeController = documentObject.AddComponent<KineTutor3D.App.Fairino.RobotControlV3RuntimeController>();
            }

            var robotStageRenderSurface = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.RobotStageRenderSurface>();
            if (robotStageRenderSurface == null)
            {
                robotStageRenderSurface = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.RobotStageRenderSurface>();
            }

            var connectionHomeController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.ConnectionHomeController>();
            if (connectionHomeController == null)
            {
                connectionHomeController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.ConnectionHomeController>();
            }

            var binder = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.PendantV3Binder>();
            if (binder == null)
            {
                binder = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.PendantV3Binder>();
            }

            var easyMotionController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.EasyMotionController>();
            if (easyMotionController == null)
            {
                easyMotionController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.EasyMotionController>();
            }

            var jointJogController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.JointJogController>();
            if (jointJogController == null)
            {
                jointJogController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.JointJogController>();
            }

            var tcpJogController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.TcpJogController>();
            if (tcpJogController == null)
            {
                tcpJogController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.TcpJogController>();
            }

            var pointMoveController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.PointMoveController>();
            if (pointMoveController == null)
            {
                pointMoveController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.PointMoveController>();
            }

            var ioPanelController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.IoPanelController>();
            if (ioPanelController == null)
            {
                ioPanelController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.IoPanelController>();
            }

            var safetyDiagnosticsController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.SafetyDiagnosticsController>();
            if (safetyDiagnosticsController == null)
            {
                safetyDiagnosticsController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.SafetyDiagnosticsController>();
            }

            var viewportToolbarController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.ViewportToolbarController>();
            if (viewportToolbarController == null)
            {
                viewportToolbarController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.ViewportToolbarController>();
            }

            var viewportAuxInfoController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.ViewportAuxInfoController>();
            if (viewportAuxInfoController == null)
            {
                viewportAuxInfoController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.ViewportAuxInfoController>();
            }

            var helpPanelController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.HelpPanelController>();
            if (helpPanelController == null)
            {
                helpPanelController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.HelpPanelController>();
            }

            var whyItMovedController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.WhyItMovedController>();
            if (whyItMovedController == null)
            {
                whyItMovedController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.WhyItMovedController>();
            }

            var popupCoordinator = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.PopupCoordinatorV3>();
            if (popupCoordinator == null)
            {
                popupCoordinator = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.PopupCoordinatorV3>();
            }

            var statusCardController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.StatusCardController>();
            if (statusCardController == null)
            {
                statusCardController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.StatusCardController>();
            }

            var navRailController = documentObject.GetComponent<KineTutor3D.UI.RobotControlV3.NavRailController>();
            if (navRailController == null)
            {
                navRailController = documentObject.AddComponent<KineTutor3D.UI.RobotControlV3.NavRailController>();
            }

            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RootVisualTreePath);
            var connectionHomeTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ConnectionHomePath);
            var coordStripTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CoordStripPath);
            var statusCardTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(StatusCardPath);
            var easyMotionTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(EasyMotionPath);
            var jointJogTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(JointJogPath);
            var tcpJogTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TcpJogPath);
            var pointMoveTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PointMovePath);
            var safetyDiagnosticsTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SafetyDiagnosticsPath);
            var faultOverlayTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(FaultOverlayPath);
            var cartesianOverlayTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CartesianArrowsOverlayPath);
            var viewportToolbarTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ViewportToolbarPath);
            var helpPanelTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HelpPanelPath);
            var actionServoConfirmTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ActionServoConfirmPath);
            var actionResetConfirmTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ActionResetConfirmPath);
            var actionRunConfirmTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ActionRunConfirmPath);
            var moveConfirmTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MoveConfirmPath);
            var warningDialogTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(WarningDialogPath);
            var recoveryDialogTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RecoveryDialogPath);
            var unsavedConfirmTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UnsavedConfirmPath);
            var firstRunGuideTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(FirstRunGuidePath);
            if (panelSettings == null)
            {
                throw new MissingReferenceException($"PanelSettings not found: {PanelSettingsPath}");
            }

            if (visualTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {RootVisualTreePath}");
            }

            if (connectionHomeTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {ConnectionHomePath}");
            }

            if (coordStripTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {CoordStripPath}");
            }

            if (statusCardTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {StatusCardPath}");
            }

            if (easyMotionTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {EasyMotionPath}");
            }

            if (jointJogTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {JointJogPath}");
            }

            if (tcpJogTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {TcpJogPath}");
            }

            if (pointMoveTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {PointMovePath}");
            }

            if (safetyDiagnosticsTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {SafetyDiagnosticsPath}");
            }

            if (faultOverlayTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {FaultOverlayPath}");
            }

            if (cartesianOverlayTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {CartesianArrowsOverlayPath}");
            }

            if (viewportToolbarTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {ViewportToolbarPath}");
            }

            if (helpPanelTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {HelpPanelPath}");
            }

            if (actionServoConfirmTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {ActionServoConfirmPath}");
            }

            if (actionResetConfirmTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {ActionResetConfirmPath}");
            }

            if (actionRunConfirmTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {ActionRunConfirmPath}");
            }

            if (moveConfirmTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {MoveConfirmPath}");
            }

            if (warningDialogTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {WarningDialogPath}");
            }

            if (recoveryDialogTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {RecoveryDialogPath}");
            }

            if (unsavedConfirmTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {UnsavedConfirmPath}");
            }

            if (firstRunGuideTree == null)
            {
                throw new MissingReferenceException($"VisualTreeAsset not found: {FirstRunGuidePath}");
            }

            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = visualTree;
            SetObjectReference(documentBridge, "panelSettings", panelSettings);
            SetObjectReference(documentBridge, "rootVisualTree", visualTree);
            SetObjectReference(connectionHomeController, "connectionHomeTemplate", connectionHomeTree);
            SetObjectReference(easyMotionController, "easyMotionTemplate", easyMotionTree);
            SetObjectReference(jointJogController, "jointJogTemplate", jointJogTree);
            SetObjectReference(tcpJogController, "tcpJogTemplate", tcpJogTree);
            SetObjectReference(tcpJogController, "cartesianArrowsOverlayTemplate", cartesianOverlayTree);
            SetObjectReference(pointMoveController, "pointMoveTemplate", pointMoveTree);
            SetObjectReference(safetyDiagnosticsController, "safetyDiagnosticsTemplate", safetyDiagnosticsTree);
            SetObjectReference(safetyDiagnosticsController, "faultOverlayTemplate", faultOverlayTree);
            SetObjectReference(viewportToolbarController, "viewportToolbarTemplate", viewportToolbarTree);
            SetObjectReference(helpPanelController, "helpPanelTemplate", helpPanelTree);
            SetObjectReference(popupCoordinator, "servoConfirmTemplate", actionServoConfirmTree);
            SetObjectReference(popupCoordinator, "resetConfirmTemplate", actionResetConfirmTree);
            SetObjectReference(popupCoordinator, "runConfirmTemplate", actionRunConfirmTree);
            SetObjectReference(popupCoordinator, "moveConfirmTemplate", moveConfirmTree);
            SetObjectReference(popupCoordinator, "warningDialogTemplate", warningDialogTree);
            SetObjectReference(popupCoordinator, "recoveryDialogTemplate", recoveryDialogTree);
            SetObjectReference(popupCoordinator, "unsavedConfirmTemplate", unsavedConfirmTree);
            SetObjectReference(popupCoordinator, "firstRunGuideTemplate", firstRunGuideTree);
            SetObjectReference(statusCardController, "coordStripTemplate", coordStripTree);
            SetObjectReference(statusCardController, "statusCardTemplate", statusCardTree);
            SetIntValue(layoutController, "previewMode", (int)PendantV3LayoutController.PreviewMode.Desktop);
            SetFloatValue(layoutController, "tabletBreakpoint", 1366f);
            documentBridge.enabled = true;
            inputContract.enabled = true;
            layoutController.enabled = true;
            shellStateController.enabled = true;
            sceneCoordinator.enabled = true;
            connectionHomeController.enabled = true;
            binder.enabled = true;
            easyMotionController.enabled = true;
            jointJogController.enabled = true;
            tcpJogController.enabled = true;
            pointMoveController.enabled = true;
            safetyDiagnosticsController.enabled = true;
            viewportToolbarController.enabled = true;
            viewportAuxInfoController.enabled = true;
            helpPanelController.enabled = true;
            whyItMovedController.enabled = true;
            popupCoordinator.enabled = true;
            statusCardController.enabled = true;
            navRailController.enabled = true;
        }

        private static void SetObjectReference(Object target, string propertyName, Object reference)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new MissingReferenceException($"Serialized property not found: {propertyName}");
            }

            property.objectReferenceValue = reference;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetIntValue(Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new MissingReferenceException($"Serialized property not found: {propertyName}");
            }

            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloatValue(Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new MissingReferenceException($"Serialized property not found: {propertyName}");
            }

            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
