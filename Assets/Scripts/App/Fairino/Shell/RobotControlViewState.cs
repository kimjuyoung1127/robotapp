// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// RobotControlV2 UI가 공통으로 소비하는 단일 상태 원천입니다.
    /// </summary>
    public sealed class RobotControlViewState
    {
        public bool IsConnected { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsMockMode { get; set; } = true;

        public bool IsDragMode { get; set; }

        public bool IsTabletLayout { get; set; }

        public ControllerModeViewState ControllerMode { get; set; } = ControllerModeViewState.Mock;

        public int ToolId { get; set; }

        public int UserId { get; set; }

        public string FaultSummary { get; set; } = "문제 없음";

        public string SafetySummary { get; set; } = "미리보기 안전 확인 사용 중";

        public string SpeedPreset { get; set; } = "30%";

        public string SpeedPolicySummary { get; set; } = "저속 교육 안전 프리셋";

        public double[] CurrentJointValuesDeg { get; set; } = new double[] { 0d, -32d, 84d, 0d, 90d, 0d };

        public string CurrentTcpPose { get; set; } = "X -497 / Y -130 / Z 477 / RX 180 / RY 0 / RZ 90";

        public string PreviewTarget { get; set; } = "Ready 포즈";

        public PreviewRiskSummary PreviewRiskSummary { get; set; } = PreviewRiskSummary.CreateDefault();

        public string LastCommandSummary { get; set; } = "아직 실행한 명령이 없습니다";

        public RecoveryHintViewState LastRecoveryHint { get; set; } = RecoveryHintViewState.CreateDefault();

        public event Action Changed;

        public void NotifyChanged()
        {
            Changed?.Invoke();
        }

        public static RobotControlViewState CreateDefault()
        {
            return new RobotControlViewState();
        }

        public static bool AuthorActiveV2Scene()
        {
            var editorSceneManagerType = System.Type.GetType("UnityEditor.SceneManagement.EditorSceneManager, UnityEditor");
            if (editorSceneManagerType == null)
            {
                throw new System.InvalidOperationException("UnityEditor.SceneManagement.EditorSceneManager type not found.");
            }

            var getActiveScene = editorSceneManagerType.GetMethod("GetActiveScene", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var markSceneDirty = editorSceneManagerType.GetMethod("MarkSceneDirty", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var saveScene = editorSceneManagerType.GetMethod(
                "SaveScene",
                new[] { typeof(UnityEngine.SceneManagement.Scene), typeof(string), typeof(bool) });

            if (getActiveScene == null || markSceneDirty == null || saveScene == null)
            {
                throw new System.InvalidOperationException("Required EditorSceneManager methods were not found.");
            }

            var scene = (UnityEngine.SceneManagement.Scene)getActiveScene.Invoke(null, null);
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV2.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV2 scene must be active. Current: {scene.path}");
            }

            var sceneBootstrap = UnityEngine.GameObject.Find("SceneBootstrap") ?? UnityEngine.GameObject.Find("RobotControlCoordinator");
            if (sceneBootstrap == null)
            {
                sceneBootstrap = new UnityEngine.GameObject("SceneBootstrap");
            }

            sceneBootstrap.name = "SceneBootstrap";

            var oldCoordinator = sceneBootstrap.GetComponent<RobotControlSceneCoordinator>();
            if (oldCoordinator != null)
            {
                UnityEngine.Object.DestroyImmediate(oldCoordinator, true);
            }

            if (sceneBootstrap.GetComponent<RobotControlV2SceneCoordinator>() == null)
            {
                sceneBootstrap.AddComponent<RobotControlV2SceneCoordinator>();
            }

            if (sceneBootstrap.GetComponent<RobotControlLayoutCoordinator>() == null)
            {
                sceneBootstrap.AddComponent<RobotControlLayoutCoordinator>();
            }

            if (sceneBootstrap.GetComponent<RobotControlPopupCoordinator>() == null)
            {
                sceneBootstrap.AddComponent<RobotControlPopupCoordinator>();
            }

            var runtimeRoot = UnityEngine.GameObject.Find("RuntimeRoot") ?? new UnityEngine.GameObject("RuntimeRoot");
            var robotRuntimeRoot = UnityEngine.GameObject.Find("RobotRuntimeRoot") ?? UnityEngine.GameObject.Find("FR5_RuntimeRoot");
            if (robotRuntimeRoot != null)
            {
                robotRuntimeRoot.name = "RobotRuntimeRoot";
                robotRuntimeRoot.transform.SetParent(runtimeRoot.transform, false);
            }

            var sessionRoot = EnsureChild(runtimeRoot.transform, "SessionRoot");
            EnsureChild(sessionRoot, "WaypointSequenceRoot");
            EnsureChild(sessionRoot, "PresetAnimatorRoot");
            EnsureChild(sessionRoot, "ReportBufferRoot");

            if (robotRuntimeRoot != null)
            {
                var previewRoot = EnsureChild(robotRuntimeRoot.transform, "PreviewRoot");
                EnsureChild(previewRoot, "TargetGhostRoot");
                EnsureChild(previewRoot, "PredictedPathRoot");
                EnsureChild(previewRoot, "RiskHighlightRoot");
                EnsureChild(previewRoot, "PreviewTargetMarkerRoot");

                var overlayRoot = EnsureChild(robotRuntimeRoot.transform, "OverlayRoot");
                EnsureChild(overlayRoot, "FrameGizmoRoot");
                EnsureChild(overlayRoot, "DisplacementArrowRoot");
                EnsureChild(overlayRoot, "EndEffectorTrailRoot");
                EnsureChild(overlayRoot, "JointHandleRoot");

                EnsureChild(robotRuntimeRoot.transform, "RuntimeDiagnosticsRoot");
            }

            var mainCamera = UnityEngine.Camera.main != null ? UnityEngine.Camera.main.gameObject : UnityEngine.GameObject.Find("Main Camera");
            if (mainCamera != null)
            {
                EnsureChild(mainCamera.transform, "RobotControlCameraAnchor");
            }

            var canvas = UnityEngine.Object.FindFirstObjectByType<UnityEngine.Canvas>(UnityEngine.FindObjectsInactive.Include);
            if (canvas == null)
            {
                throw new UnityEngine.MissingReferenceException("RobotControlV2 scene is missing a Canvas.");
            }

            var shell = KineTutor3D.UI.RobotControlShell.EnsureV2Shell(canvas, null, "로봇 제어 V2", "모의 연결");
            shell.Bind(CreateDefault());

            markSceneDirty.Invoke(null, new object[] { scene });

            var assetDatabaseType = System.Type.GetType("UnityEditor.AssetDatabase, UnityEditor");
            var saveAssets = assetDatabaseType?.GetMethod("SaveAssets", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var refresh = assetDatabaseType?.GetMethod("Refresh", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, System.Type.EmptyTypes, null);

            var saved = (bool)saveScene.Invoke(null, new object[] { scene, "Assets/Scenes/RobotControlV2.unity", true });
            saveAssets?.Invoke(null, null);
            refresh?.Invoke(null, null);
            return saved;
        }

        private static UnityEngine.Transform EnsureChild(UnityEngine.Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child;
            }

            var go = new UnityEngine.GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }
    }
}
