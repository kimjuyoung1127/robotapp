// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    internal readonly struct HomeContinueHubViewRefs
    {
        public HomeContinueHubViewRefs(
            RectTransform root,
            Text contextText,
            Button continueButton,
            Button startGuidedLessonButton,
            Button startMathReadinessButton,
            Button openRobotLibraryButton,
            Button openSandboxButton)
        {
            Root = root;
            ContextText = contextText;
            ContinueButton = continueButton;
            StartGuidedLessonButton = startGuidedLessonButton;
            StartMathReadinessButton = startMathReadinessButton;
            OpenRobotLibraryButton = openRobotLibraryButton;
            OpenSandboxButton = openSandboxButton;
        }

        public RectTransform Root { get; }
        public Text ContextText { get; }
        public Button ContinueButton { get; }
        public Button StartGuidedLessonButton { get; }
        public Button StartMathReadinessButton { get; }
        public Button OpenRobotLibraryButton { get; }
        public Button OpenSandboxButton { get; }
    }

    internal static class HomeContinueHubViewBuilder
    {
        private const string ContinueIcon = "icon-play";
        private const string RobotLibraryIcon = "icon-search";
        private const string SandboxIcon = "icon-home";

        public static bool TryBindExisting(Canvas canvas, out HomeContinueHubViewRefs view)
        {
            view = default;
            if (canvas == null)
            {
                return false;
            }

            var root = canvas.transform.Find("HomeContinueHubRoot") as RectTransform;
            if (root == null)
            {
                return false;
            }

            var contextText = FindText(root, "ContextSummary");
            var continueButton = FindButton(root, "BtnContinueLatestContext");
            var startGuidedLessonButton = FindButton(root, "BtnStartGuidedLesson");
            var startMathReadinessButton = FindButton(root, "BtnStartMathReadiness");
            var openRobotLibraryButton = FindButton(root, "BtnOpenRobotLibrary");
            var openSandboxButton = FindButton(root, "BtnOpenSandbox");

            if (contextText == null || continueButton == null || startGuidedLessonButton == null ||
                startMathReadinessButton == null || openRobotLibraryButton == null || openSandboxButton == null)
            {
                return false;
            }

            view = new HomeContinueHubViewRefs(
                root,
                contextText,
                continueButton,
                startGuidedLessonButton,
                startMathReadinessButton,
                openRobotLibraryButton,
                openSandboxButton);
            return true;
        }

        public static Canvas EnsureCanvas(Canvas existingCanvas)
        {
            var canvas = existingCanvas ?? Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
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
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null)
            {
                eventSystem.AddComponent(inputModuleType);
            }
        }

        public static HomeContinueHubViewRefs Build(
            Canvas canvas,
            Font fallbackFont,
            UnityEngine.Events.UnityAction onContinue,
            UnityEngine.Events.UnityAction onStartGuidedLesson,
            UnityEngine.Events.UnityAction onStartMathReadiness,
            UnityEngine.Events.UnityAction onOpenRobotLibrary,
            UnityEngine.Events.UnityAction onOpenSandbox)
        {
            var root = UiRuntimeStyle.EnsureRectChild(canvas.transform, "HomeContinueHubRoot");
            UiRuntimeStyle.Stretch(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var background = UiRuntimeStyle.EnsureImage(root, "Backdrop", UIDesignTokens.Colors.SurfaceRaisedAlt);
            UiRuntimeStyle.Stretch((RectTransform)background.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var content = UIComponentFactory.CreateVStack(root, "HubContent", UILayoutProfile.DefaultSpacing);
            UiRuntimeStyle.Anchor((RectTransform)content.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(UILayoutProfile.IsTablet ? 520f : 620f, 620f), new Vector2(UIDesignTokens.Space.Xxl, -UIDesignTokens.Space.Xxl));

            var title = UIComponentFactory.CreateText(content.transform, "Title", TypographyPreset.DisplayLg, UIDesignTokens.Colors.TextPrimary, "Home / Continue Hub", fallbackFont);
            UiRuntimeStyle.EnsureLayoutElement(title).preferredHeight = 40f;

            var subtitle = UIComponentFactory.CreateText(content.transform, "Subtitle", TypographyPreset.HeadingSm, UIDesignTokens.Colors.TextSecondary, "이어하기 또는 새 시작 경로를 선택하세요.", fallbackFont);
            UiRuntimeStyle.EnsureLayoutElement(subtitle).preferredHeight = 44f;

            var contextCard = UIComponentFactory.CreateCardPanel(content.transform, "ContextCard");
            UiRuntimeStyle.EnsureLayoutElement(contextCard).preferredHeight = 72f;
            var contextText = UIComponentFactory.CreateText(contextCard, "ContextSummary", TypographyPreset.Body, UIDesignTokens.Colors.TextSecondary, string.Empty, fallbackFont);
            UiRuntimeStyle.Stretch(contextText.rectTransform, Vector2.zero, Vector2.one, new Vector2(UIDesignTokens.Space.Md, UIDesignTokens.Space.Md), new Vector2(-UIDesignTokens.Space.Md, -UIDesignTokens.Space.Md));
            contextText.fontStyle = FontStyle.Italic;

            var buttonWidth = UILayoutProfile.IsTablet ? 200f : 220f;
            var continueButton = CreateButton(content.transform, fallbackFont, "BtnContinueLatestContext", "이어하기", UIDesignTokens.Colors.AccentPrimary, onContinue, buttonWidth);
            UIComponentFactory.AttachLeadingIcon(continueButton, ContinueIcon);

            CreateButton(content.transform, fallbackFont, "BtnStartGuidedLesson", "새로 시작", UIDesignTokens.Colors.SurfaceCard, onStartGuidedLesson, buttonWidth);
            CreateButton(content.transform, fallbackFont, "BtnStartMathReadiness", "수학 기초 워밍업", UIDesignTokens.Colors.AccentSecondary, onStartMathReadiness, buttonWidth);

            var robotLibraryButton = CreateButton(content.transform, fallbackFont, "BtnOpenRobotLibrary", "로봇 선택", UIDesignTokens.Colors.SurfaceCard, onOpenRobotLibrary, buttonWidth);
            UIComponentFactory.AttachLeadingIcon(robotLibraryButton, RobotLibraryIcon);

            var sandboxButton = CreateButton(content.transform, fallbackFont, "BtnOpenSandbox", "샌드박스", UIDesignTokens.Colors.SurfaceCard, onOpenSandbox, buttonWidth);
            UIComponentFactory.AttachLeadingIcon(sandboxButton, SandboxIcon);

            CreateButton(content.transform, fallbackFont, "BtnOpenProgress", "Progress", UIDesignTokens.Colors.DangerMuted, null, buttonWidth, false);
            CreateButton(content.transform, fallbackFont, "BtnOpenSettings", "Settings", UIDesignTokens.Colors.DangerMuted, null, buttonWidth, false);

            return new HomeContinueHubViewRefs(
                root,
                contextText,
                continueButton,
                FindButton(root, "BtnStartGuidedLesson"),
                FindButton(root, "BtnStartMathReadiness"),
                FindButton(root, "BtnOpenRobotLibrary"),
                FindButton(root, "BtnOpenSandbox"));
        }

        private static Button FindButton(Transform parent, string name)
        {
            var target = parent.Find(name);
            return target != null ? target.GetComponent<Button>() : null;
        }

        private static Text FindText(Transform parent, string name)
        {
            var target = parent.Find(name);
            return target != null ? target.GetComponent<Text>() : null;
        }

        private static Button CreateButton(
            Transform parent,
            Font fallbackFont,
            string name,
            string label,
            Color color,
            UnityEngine.Events.UnityAction onClick,
            float width,
            bool interactable = true)
        {
            var button = color == UIDesignTokens.Colors.AccentPrimary
                ? UIComponentFactory.CreatePrimaryButton(parent, name, label, fallbackFont, width)
                : UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, width);

            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.preferredWidth = width;
            element.preferredHeight = UIDesignTokens.Size.ButtonHeightLg;
            button.interactable = interactable;
            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            return button;
        }
    }
}
