// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// RobotControl 씬의 기본 Canvas, 패널, 카메라, 조명 레이아웃을 생성합니다.
    /// </summary>
    public static class FairinoRobotControlViewBuilder
    {
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

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var go = new GameObject("EventSystem", typeof(EventSystem));
            var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null)
            {
                go.AddComponent(inputModuleType);
            }

            go.transform.SetParent(null, false);
        }

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

        public static void EnsureLayout(
            Canvas canvas,
            Font fallbackFont,
            out FairinoConnectionPanel connectionPanel,
            out FairinoJointControlPanel jointControlPanel,
            out FairinoStatePanel statePanel)
        {
            var root = canvas.transform as RectTransform;
            var shellRoot = UiRuntimeStyle.EnsureRectChild(root, "RobotControlShell");
            UiRuntimeStyle.Stretch(shellRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var overlay = UiRuntimeStyle.EnsureImage(shellRoot, "RobotControlOverlay", UIDesignTokens.Colors.SceneOverlayLight);
            UiRuntimeStyle.Stretch((RectTransform)overlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            BuildTopBar(shellRoot, fallbackFont);

            var connectionRoot = BuildPanelHost(shellRoot, "ConnectionPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(360f, 210f), new Vector2(16f, -90f));
            var jointRoot = BuildPanelHost(shellRoot, "JointControlPanel", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(420f, 460f), new Vector2(16f, 16f));
            var stateRoot = BuildPanelHost(shellRoot, "StatePanel", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(360f, 240f), new Vector2(-16f, 16f));

            connectionPanel = connectionRoot.GetComponent<FairinoConnectionPanel>() ?? connectionRoot.gameObject.AddComponent<FairinoConnectionPanel>();
            jointControlPanel = jointRoot.GetComponent<FairinoJointControlPanel>() ?? jointRoot.gameObject.AddComponent<FairinoJointControlPanel>();
            statePanel = stateRoot.GetComponent<FairinoStatePanel>() ?? stateRoot.gameObject.AddComponent<FairinoStatePanel>();
        }

        private static RectTransform BuildPanelHost(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 anchoredPosition)
        {
            var panelRoot = UiRuntimeStyle.EnsureRectChild(parent, name);
            UiRuntimeStyle.Anchor(panelRoot, anchor, pivot, size, anchoredPosition);
            var background = panelRoot.GetComponent<Image>() ?? panelRoot.gameObject.AddComponent<Image>();
            background.color = UIDesignTokens.Colors.SurfaceRaisedAlt;
            return panelRoot;
        }

        private static void BuildTopBar(RectTransform parent, Font fallbackFont)
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
            mode.text = "FR5 · Mock by default";

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
