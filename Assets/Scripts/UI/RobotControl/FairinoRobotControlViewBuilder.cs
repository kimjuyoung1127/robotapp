// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// RobotControl 씬의 기본 Canvas, 패널, 카메라, 조명 레이아웃을 생성합니다.
    /// 2탭 바(Joint Control / TCP Control)를 지원하며, State 패널은 우측 상시 표시입니다.
    /// </summary>
    public static class FairinoRobotControlViewBuilder
    {
        private const float TabBarHeight = 40f;

        /// <summary>
        /// 씬에 이미 authored된 RobotControl UI를 찾아 바인딩합니다.
        /// </summary>
        public static bool TryBindExistingLayout(
            Canvas canvas,
            out FairinoConnectionPanel connectionPanel,
            out FairinoJointControlPanel jointControlPanel,
            out FairinoStatePanel statePanel,
            out FairinoTcpControlPanel tcpPanel,
            out RobotControlDiagnosticsDrawer diagnosticsDrawer,
            out FairinoWhyItMovedLabel whyItMovedLabel,
            out FairinoMoveConfirmDialog moveConfirmDialog,
            out Button diagnosticsButton,
            out Toggle gizmoToggle,
            out Button clearTrailButton)
        {
            connectionPanel = null;
            jointControlPanel = null;
            statePanel = null;
            tcpPanel = null;
            diagnosticsDrawer = null;
            whyItMovedLabel = null;
            moveConfirmDialog = null;
            diagnosticsButton = null;
            gizmoToggle = null;
            clearTrailButton = null;

            if (canvas == null)
            {
                return false;
            }

            var shellRoot = canvas.transform.Find("RobotControlShell");
            if (shellRoot == null)
            {
                return false;
            }

            connectionPanel = shellRoot.Find("ConnectionPanel")?.GetComponent<FairinoConnectionPanel>();
            jointControlPanel = shellRoot.Find("JointControlPanel")?.GetComponent<FairinoJointControlPanel>();
            statePanel = shellRoot.Find("StatePanel")?.GetComponent<FairinoStatePanel>();
            tcpPanel = shellRoot.Find("TcpControlPanel")?.GetComponent<FairinoTcpControlPanel>();
            diagnosticsDrawer = shellRoot.Find("DiagnosticsDrawer")?.GetComponent<RobotControlDiagnosticsDrawer>();
            whyItMovedLabel = shellRoot.Find("WhyItMovedLabel")?.GetComponent<FairinoWhyItMovedLabel>();
            moveConfirmDialog = shellRoot.Find("MoveConfirmDialog")?.GetComponent<FairinoMoveConfirmDialog>();
            diagnosticsButton = shellRoot.Find("TopBar/BtnDiagnostics")?.GetComponent<Button>();
            gizmoToggle = shellRoot.Find("TopBar/GizmoToggle")?.GetComponent<Toggle>();
            clearTrailButton = shellRoot.Find("TopBar/BtnClearTrail")?.GetComponent<Button>();

            return connectionPanel != null
                && jointControlPanel != null
                && statePanel != null
                && tcpPanel != null
                && diagnosticsDrawer != null
                && whyItMovedLabel != null
                && moveConfirmDialog != null
                && diagnosticsButton != null
                && gizmoToggle != null
                && clearTrailButton != null;
        }

        /// <summary>
        /// Canvas를 확인하거나 생성합니다.
        /// </summary>
        public static Canvas EnsureCanvas(Canvas canvas, Font fallbackFont)
        {
            if (canvas != null)
            {
                return canvas;
            }

            canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas != null)
            {
                return canvas;
            }

            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        /// <summary>
        /// EventSystem을 확인하거나 생성합니다.
        /// </summary>
        public static void EnsureEventSystem()
        {
            var existing = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (existing != null)
            {
                // 기존 EventSystem에 InputModule이 없으면 추가
                if (existing.GetComponent<InputSystemUIInputModule>() == null
                    && existing.GetComponent<BaseInputModule>() == null)
                {
                    var module = existing.gameObject.AddComponent<InputSystemUIInputModule>();
                    module.AssignDefaultActions();
                    Debug.Log("[EventSystem] Added InputSystemUIInputModule to existing EventSystem.");
                }

                return;
            }

            var go = new GameObject("EventSystem", typeof(EventSystem));
            var inputModule = go.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
            go.transform.SetParent(null, false);
            Debug.Log("[EventSystem] Created new EventSystem with InputSystemUIInputModule.");
        }

        /// <summary>
        /// 메인 카메라를 확인하거나 생성합니다.
        /// </summary>
        public static Camera EnsureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camera = cameraGo.GetComponent<Camera>();
                camera.tag = "MainCamera";
            }
            SceneCameraDirector.ConfigureForCurrentScene(camera);
            return camera;
        }

        /// <summary>
        /// 방향 조명을 확인하거나 생성합니다.
        /// </summary>
        public static Light EnsureLight()
        {
            var light = Object.FindFirstObjectByType<Light>(FindObjectsInactive.Include);
            if (light == null)
            {
                var lightGo = new GameObject("Directional Light", typeof(Light));
                light = lightGo.GetComponent<Light>();
            }

            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.transform.rotation = Quaternion.Euler(40f, -32f, 0f);
            return light;
        }

        /// <summary>
        /// 2탭 바와 전체 패널 레이아웃을 생성합니다.
        /// </summary>
        public static void EnsureLayout(
            Canvas canvas,
            Font fallbackFont,
            out FairinoConnectionPanel connectionPanel,
            out FairinoJointControlPanel jointControlPanel,
            out FairinoStatePanel statePanel,
            out FairinoTcpControlPanel tcpPanel,
            out RobotControlDiagnosticsDrawer diagnosticsDrawer,
            out FairinoWhyItMovedLabel whyItMovedLabel,
            out FairinoMoveConfirmDialog moveConfirmDialog,
            out Button diagnosticsButton,
            out Toggle gizmoToggle,
            out Button clearTrailButton)
        {
            EnsureLayout(
                canvas,
                fallbackFont,
                null,
                out connectionPanel,
                out jointControlPanel,
                out statePanel,
                out tcpPanel,
                out diagnosticsDrawer,
                out whyItMovedLabel,
                out moveConfirmDialog,
                out diagnosticsButton,
                out gizmoToggle,
                out clearTrailButton);
        }

        /// <summary>
        /// 2탭 바와 전체 패널 레이아웃을 생성합니다.
        /// </summary>
        public static void EnsureLayout(
            Canvas canvas,
            Font fallbackFont,
            string topBarModeText,
            out FairinoConnectionPanel connectionPanel,
            out FairinoJointControlPanel jointControlPanel,
            out FairinoStatePanel statePanel,
            out FairinoTcpControlPanel tcpPanel,
            out RobotControlDiagnosticsDrawer diagnosticsDrawer,
            out FairinoWhyItMovedLabel whyItMovedLabel,
            out FairinoMoveConfirmDialog moveConfirmDialog,
            out Button diagnosticsButton,
            out Toggle gizmoToggle,
            out Button clearTrailButton)
        {
            var root = canvas.transform as RectTransform;
            var shellRoot = UiRuntimeStyle.EnsureRectChild(root, "RobotControlShell");
            UiRuntimeStyle.Stretch(shellRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var overlay = UiRuntimeStyle.EnsureImage(shellRoot, "RobotControlOverlay", UIDesignTokens.Colors.SceneOverlayLight);
            overlay.raycastTarget = false;
            UiRuntimeStyle.Stretch((RectTransform)overlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            BuildTopBar(shellRoot, fallbackFont, topBarModeText, out diagnosticsButton, out gizmoToggle, out clearTrailButton);

            // Connection panel — compact horizontal layout below TopBar
            var connectionRoot = BuildPanelHost(shellRoot, "ConnectionPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, 140f), new Vector2(16f, -78f));

            // Tab bar — 2 tabs (Joint/TCP), below ConnectionPanel
            var tabBar = BuildTabBar(shellRoot, fallbackFont);

            // Left content panels — below tab bar, top-anchored
            var jointRoot = BuildPanelHost(shellRoot, "JointControlPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, 804f), new Vector2(16f, -270f));
            var tcpRoot = BuildPanelHost(shellRoot, "TcpControlPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, 664f), new Vector2(16f, -270f));

            // Right panels — state (always visible) + why it moved
            var stateRoot = BuildPanelHost(shellRoot, "StatePanel", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(360f, 280f), new Vector2(-16f, 80f));
            var whyRoot = BuildPanelHost(shellRoot, "WhyItMovedLabel", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(360f, 116f), new Vector2(-16f, 16f));

            connectionPanel = connectionRoot.GetComponent<FairinoConnectionPanel>() ?? connectionRoot.gameObject.AddComponent<FairinoConnectionPanel>();
            jointControlPanel = jointRoot.GetComponent<FairinoJointControlPanel>() ?? jointRoot.gameObject.AddComponent<FairinoJointControlPanel>();
            statePanel = stateRoot.GetComponent<FairinoStatePanel>() ?? stateRoot.gameObject.AddComponent<FairinoStatePanel>();
            tcpPanel = tcpRoot.GetComponent<FairinoTcpControlPanel>() ?? tcpRoot.gameObject.AddComponent<FairinoTcpControlPanel>();
            whyItMovedLabel = whyRoot.GetComponent<FairinoWhyItMovedLabel>() ?? whyRoot.gameObject.AddComponent<FairinoWhyItMovedLabel>();

            var diagnosticsRoot = UiRuntimeStyle.EnsureRectChild(shellRoot, "DiagnosticsDrawer");
            UiRuntimeStyle.Stretch(diagnosticsRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            diagnosticsDrawer = diagnosticsRoot.GetComponent<RobotControlDiagnosticsDrawer>() ?? diagnosticsRoot.gameObject.AddComponent<RobotControlDiagnosticsDrawer>();
            diagnosticsRoot.gameObject.SetActive(false);

            // TCP panel starts hidden (Joint Control is default tab)
            tcpRoot.gameObject.SetActive(false);

            // Move confirm dialog — overlay, starts hidden
            var dialogRoot = BuildPanelHost(shellRoot, "MoveConfirmDialog", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1920f, 1080f), Vector2.zero);
            dialogRoot.GetComponent<Image>().color = Color.clear;
            moveConfirmDialog = dialogRoot.GetComponent<FairinoMoveConfirmDialog>() ?? dialogRoot.gameObject.AddComponent<FairinoMoveConfirmDialog>();
            dialogRoot.gameObject.SetActive(false);

            // Wire tab switching (Joint ↔ TCP only, State is always visible on right)
            WireTabButtons(tabBar, jointRoot.gameObject, tcpRoot.gameObject);
        }

        private static RectTransform BuildTabBar(RectTransform parent, Font fallbackFont)
        {
            var tabBar = UiRuntimeStyle.EnsureRectChild(parent, "TabBar");
            UiRuntimeStyle.Anchor(tabBar, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, TabBarHeight), new Vector2(16f, -224f));

            var tabBg = tabBar.GetComponent<Image>() ?? tabBar.gameObject.AddComponent<Image>();
            tabBg.color = UIDesignTokens.Colors.SurfaceCard;

            var tabNames = new[] { "Joint Control", "TCP Control" };
            const float tabWidth = 210f;

            for (var i = 0; i < tabNames.Length; i++)
            {
                var btn = tabBar.Find($"Tab_{i}")?.GetComponent<Button>();
                if (btn == null)
                {
                    btn = UIComponentFactory.CreateGhostButton(tabBar, $"Tab_{i}", tabNames[i], fallbackFont);
                }

                UiRuntimeStyle.Anchor((RectTransform)btn.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(tabWidth, TabBarHeight), new Vector2(i * tabWidth, 0f));
                var label = btn.GetComponentInChildren<Text>();
                if (label != null) label.text = tabNames[i];
            }

            // 이전 3탭 레이아웃에서 남은 탭 제거
            var staleTab = tabBar.Find("Tab_2");
            if (staleTab != null) Object.Destroy(staleTab.gameObject);

            return tabBar;
        }

        private static void WireTabButtons(RectTransform tabBar, GameObject jointPanel, GameObject tcpPanel)
        {
            var panels = new[] { jointPanel, tcpPanel };

            for (var i = 0; i < 2; i++)
            {
                var btn = tabBar.Find($"Tab_{i}")?.GetComponent<Button>();
                if (btn == null) continue;

                var capturedIndex = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    for (var p = 0; p < panels.Length; p++)
                    {
                        if (panels[p] != null)
                        {
                            panels[p].SetActive(p == capturedIndex);
                        }
                    }
                });
            }
        }

        private static RectTransform BuildPanelHost(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 anchoredPosition)
        {
            var panelRoot = UiRuntimeStyle.EnsureRectChild(parent, name);
            UiRuntimeStyle.Anchor(panelRoot, anchor, pivot, size, anchoredPosition);
            var background = panelRoot.GetComponent<Image>() ?? panelRoot.gameObject.AddComponent<Image>();
            background.color = UIDesignTokens.Colors.SurfaceRaisedAlt;
            return panelRoot;
        }

        public static void ApplyTopBarModeText(Canvas canvas, string topBarModeText)
        {
            if (canvas == null || string.IsNullOrWhiteSpace(topBarModeText))
            {
                return;
            }

            var mode = canvas.transform.Find("RobotControlShell/TopBar/ModeText")?.GetComponent<Text>();
            if (mode != null)
            {
                mode.text = topBarModeText;
            }
        }

        private static void BuildTopBar(RectTransform parent, Font fallbackFont, string topBarModeText, out Button diagnosticsButton, out Toggle gizmoToggle, out Button clearTrailButton)
        {
            var topBar = UiRuntimeStyle.EnsureRectChild(parent, "TopBar");
            UiRuntimeStyle.Stretch(topBar, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -72f), new Vector2(-16f, -16f));

            var bg = UiRuntimeStyle.EnsureImage(topBar, "TopBarBg", UIDesignTokens.Colors.TopBarBackground);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var title = UiRuntimeStyle.EnsureText(topBar, "Title", fallbackFont, UIDesignTokens.Type.DisplaySm, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(240f, 28f), new Vector2(24f, 0f));
            title.text = "Robot Control";

            var mode = UiRuntimeStyle.EnsureText(topBar, "ModeText", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.AccentSecondary);
            UiRuntimeStyle.Anchor(mode.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(240f, 20f), new Vector2(260f, 0f));
            mode.text = string.IsNullOrWhiteSpace(topBarModeText) ? "Robot · Mock by default" : topBarModeText;

            // 기즈모 토글
            gizmoToggle = topBar.Find("GizmoToggle")?.GetComponent<Toggle>();
            if (gizmoToggle == null)
            {
                gizmoToggle = UIComponentFactory.CreateToggle(topBar, "GizmoToggle", "Gizmos", fallbackFont);
            }

            UiRuntimeStyle.Anchor((RectTransform)gizmoToggle.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(100f, 24f), new Vector2(-290f, 0f));

            // 트레일 Clear 버튼
            clearTrailButton = topBar.Find("BtnClearTrail")?.GetComponent<Button>();
            if (clearTrailButton == null)
            {
                clearTrailButton = UIComponentFactory.CreateGhostButton(topBar, "BtnClearTrail", "Clear Trail", fallbackFont);
            }

            UiRuntimeStyle.Anchor((RectTransform)clearTrailButton.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(92f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(-180f, 0f));

            diagnosticsButton = topBar.Find("BtnDiagnostics")?.GetComponent<Button>();
            if (diagnosticsButton == null)
            {
                diagnosticsButton = UIComponentFactory.CreateSecondaryButton(topBar, "BtnDiagnostics", "Details", fallbackFont, 84f);
            }

            UiRuntimeStyle.Anchor((RectTransform)diagnosticsButton.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(84f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(-392f, 0f));

            // Back 버튼
            var backButton = topBar.Find("BtnBackToLibrary")?.GetComponent<Button>();
            if (backButton == null)
            {
                backButton = UIComponentFactory.CreateSecondaryButton(topBar, "BtnBackToLibrary", "Robot Library", fallbackFont, 132f);
            }

            UiRuntimeStyle.Anchor((RectTransform)backButton.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(132f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(-24f, 0f));
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => SceneNavigator.Load(SceneId.RobotLibrary));
        }
    }
}
