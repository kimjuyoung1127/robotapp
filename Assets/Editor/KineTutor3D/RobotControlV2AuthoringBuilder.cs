// Folder: Editor - Authoring and QA utilities for Unity scenes and tools.
using KineTutor3D.App.Fairino;
using KineTutor3D.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.EditorTools
{
    /// <summary>
    /// RobotControlV2 씬의 authored-first 셸 구조를 고정하는 에디터 유틸입니다.
    /// </summary>
    public static class RobotControlV2AuthoringBuilder
    {
        private const string ScenePath = "Assets/Scenes/RobotControlV2.unity";

        [MenuItem("KineTutor3D/RobotControl/Author V2 Shell")]
        public static void AuthorScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            var sceneBootstrap = GameObject.Find("SceneBootstrap") ?? GameObject.Find("RobotControlCoordinator");
            if (sceneBootstrap == null)
            {
                sceneBootstrap = new GameObject("SceneBootstrap");
            }

            sceneBootstrap.name = "SceneBootstrap";
            RemoveComponent<RobotControlSceneCoordinator>(sceneBootstrap);
            EnsureComponent<RobotControlV2SceneCoordinator>(sceneBootstrap);
            EnsureComponent<RobotControlLayoutCoordinator>(sceneBootstrap);
            EnsureComponent<RobotControlPopupCoordinator>(sceneBootstrap);

            var runtimeRoot = GameObject.Find("RuntimeRoot") ?? new GameObject("RuntimeRoot");
            var robotRuntimeRoot = GameObject.Find("RobotRuntimeRoot") ?? GameObject.Find("FR5_RuntimeRoot");
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

            var mainCamera = Camera.main != null ? Camera.main.gameObject : GameObject.Find("Main Camera");
            if (mainCamera != null)
            {
                EnsureChild(mainCamera.transform, "RobotControlCameraAnchor");
            }

            var canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null)
            {
                throw new MissingReferenceException("RobotControlV2 scene is missing a Canvas.");
            }

            var shell = RobotControlShell.EnsureV2Shell(canvas, null, "로봇 제어 V2", "Mock shell");
            shell.Bind(RobotControlViewState.CreateDefault());
            ForceRefreshShellPanels(shell);
            NormalizeSceneAuthoredShell(canvas.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[RobotControlV2AuthoringBuilder] Authored V2 shell saved.");
        }

        private static void RemoveComponent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component != null)
            {
                Object.DestroyImmediate(component, true);
            }
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            return target.GetComponent<T>() ?? target.AddComponent<T>();
        }

        private static Transform EnsureChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void ForceRefreshShellPanels(RobotControlShell shell)
        {
            if (shell == null)
            {
                return;
            }

            InvokeEnsurePresentation(shell.transform);
            var binder = shell.Binder;
            if (binder == null)
            {
                return;
            }

            binder.RefreshAuthoring();
            InvokeEnsurePresentation(binder.TopStatusBar != null ? binder.TopStatusBar.transform : null);
            InvokeEnsurePresentation(binder.WorkTabBar != null ? binder.WorkTabBar.transform : null);

            var workPanelHost = shell.transform.Find("SafeArea/LeftRail/WorkPanelHost");
            if (workPanelHost != null)
            {
                InvokeEnsurePresentation(workPanelHost.Find("EasyMotionPanel"));
                (workPanelHost.Find("TcpJogPanel")?.GetComponent<TcpJogPanel>())?.RefreshAuthoring();
                (workPanelHost.Find("JointJogPanel")?.GetComponent<JointJogPanel>())?.RefreshAuthoring();
                (workPanelHost.Find("PointMovePanel")?.GetComponent<PointMovePanel>())?.RefreshAuthoring();
                (workPanelHost.Find("TeachingPanel")?.GetComponent<TeachingPanel>())?.RefreshAuthoring();
            }

            var rightRail = shell.transform.Find("SafeArea/RightRail");
            if (rightRail != null)
            {
                foreach (var childName in new[] { "StatusSummaryPanel", "WhyItMovedPanel", "RecoveryGuidePanel", "HelpPanel" })
                {
                    InvokeEnsurePresentation(rightRail.Find(childName));
                }
            }

            InvokeEnsurePresentation(shell.transform.Find("DebugOnly/DiagnosticsDrawer"));
        }

        private static void InvokeEnsurePresentation(Transform target)
        {
            if (target == null)
            {
                return;
            }

            var behaviours = target.GetComponents<MonoBehaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                var method = behaviour.GetType().GetMethod("EnsurePresentation", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                method?.Invoke(behaviour, null);
            }
        }

        private static void NormalizeSceneAuthoredShell(Transform canvasRoot)
        {
            RemoveDirectChildren(
                canvasRoot.Find("RobotControlShell"),
                "RobotControlOverlay",
                "TabBar");

            RemoveDirectChildren(
                canvasRoot.Find("RobotControlShell/SafeArea/TopStatusBar"),
                "Title",
                "ModeText",
                "ConnectionStateText",
                "ToolUserText",
                "FaultText",
                "BtnServoEnable",
                "BtnRun",
                "BtnStop",
                "BtnPauseResume",
                "BtnSync",
                "BtnResetError",
                "SpeedText");

            RemoveDirectChildren(
                canvasRoot.Find("RobotControlShell/SafeArea/LeftRail/WorkPanelHost/EasyMotionPanel"),
                "Title",
                "Hint",
                "BtnHome",
                "BtnReady",
                "BtnFolded",
                "BtnZero",
                "BtnPreview",
                "BtnApply",
                "StateText");

            RemoveDirectChildren(
                canvasRoot.Find("RobotControlShell/SafeArea/LeftRail/WorkPanelHost/TcpJogPanel"),
                "Title", "Hint", "ChipBase", "ChipTool", "ChipWobj", "XRow", "YRow", "ZRow", "RXRow", "RYRow", "RZRow", "StateText");

            RemoveDirectChildren(
                canvasRoot.Find("RobotControlShell/SafeArea/LeftRail/WorkPanelHost/JointJogPanel"),
                "Title", "Hint", "J1Row", "J2Row", "J3Row", "J4Row", "J5Row", "J6Row", "JointSummary");

            RemoveDirectChildren(
                canvasRoot.Find("RobotControlShell/SafeArea/LeftRail/WorkPanelHost/PointMovePanel"),
                "Title", "Hint", "TargetText", "BtnCalculate", "BtnMove", "BtnRestore");

            RemoveDirectChildren(
                canvasRoot.Find("RobotControlShell/SafeArea/LeftRail/WorkPanelHost/TeachingPanel"),
                "Title", "Hint", "SummaryText");

            NormalizeWorkTabBar(canvasRoot.Find("RobotControlShell/SafeArea/LeftRail/WorkTabBar") as RectTransform);
            NormalizeEasyMotionPanel(canvasRoot.Find("RobotControlShell/SafeArea/LeftRail/WorkPanelHost/EasyMotionPanel") as RectTransform);
            NormalizeDiagnosticsDrawer(canvasRoot.Find("RobotControlShell/DebugOnly/DiagnosticsDrawer") as RectTransform);
        }

        private static void RemoveDirectChildren(Transform parent, params string[] childNames)
        {
            if (parent == null)
            {
                return;
            }

            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                for (var j = 0; j < childNames.Length; j++)
                {
                    if (child.name != childNames[j])
                    {
                        continue;
                    }

                    Object.DestroyImmediate(child.gameObject, true);
                    break;
                }
            }
        }

        private static void NormalizeDiagnosticsDrawer(RectTransform diagnosticsRoot)
        {
            if (diagnosticsRoot == null)
            {
                return;
            }

            diagnosticsRoot.anchorMin = new Vector2(1f, 0f);
            diagnosticsRoot.anchorMax = new Vector2(1f, 0f);
            diagnosticsRoot.pivot = new Vector2(1f, 0f);
            diagnosticsRoot.sizeDelta = new Vector2(280f, 86f);
            diagnosticsRoot.anchoredPosition = new Vector2(-24f, 24f);
            var image = diagnosticsRoot.GetComponent<Image>();
            if (image != null && image.sprite == null)
            {
                image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                image.type = Image.Type.Sliced;
            }
        }

        private static void NormalizeWorkTabBar(RectTransform workTabBarRoot)
        {
            if (workTabBarRoot == null)
            {
                return;
            }

            var image = workTabBarRoot.GetComponent<Image>() ?? workTabBarRoot.gameObject.AddComponent<Image>();
            if (image.sprite == null)
            {
                image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd")
                    ?? Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                image.type = Image.Type.Sliced;
            }

            var horizontal = workTabBarRoot.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null)
            {
                Object.DestroyImmediate(horizontal, true);
            }

            var grid = workTabBarRoot.GetComponent<GridLayoutGroup>() ?? workTabBarRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.spacing = new Vector2(8f, 8f);
            grid.cellSize = new Vector2(104f, 30f);
        }

        private static void NormalizeEasyMotionPanel(RectTransform easyMotionRoot)
        {
            if (easyMotionRoot == null)
            {
                return;
            }

            var image = easyMotionRoot.GetComponent<Image>() ?? easyMotionRoot.gameObject.AddComponent<Image>();
            if (image.sprite == null)
            {
                image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd")
                    ?? Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                image.type = Image.Type.Sliced;
            }
        }
    }
}
