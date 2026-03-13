// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 온보딩 전용 씬에서 환영 화면과 학습 트랙 선택 카드를 제공합니다.
    /// </summary>
    [ExecuteAlways]
    public class OnboardingManager : MonoBehaviour
    {
        private const string DefaultRobotId = "2DOF_RR";

        [SerializeField] private RectTransform canvasRoot;
        [SerializeField] private RectTransform modalRoot;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Button startLearningButton;
        [SerializeField] private Button beginnerButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Text headlineText;
        [SerializeField] private Text bodyText;
        [SerializeField] private SceneNavigationBar sceneNavigationBar;
        [SerializeField] private bool enableGlobalDebugNavigation = true;

        [Header("Layout")]
        [SerializeField] private Vector2 modalSize = new(720f, 500f);
        [SerializeField] private Vector2 headlineSize = new(620f, 48f);
        [SerializeField] private Vector2 headlinePosition = new(0f, -24f);
        [SerializeField] private Vector2 bodySize = new(540f, 32f);
        [SerializeField] private Vector2 bodyPosition = new(0f, -76f);
        [SerializeField] private Vector2 cardRowSize = new(620f, 240f);
        [SerializeField] private Vector2 cardRowPosition = new(0f, -10f);
        [SerializeField] private Vector2 onboardingCardSize = new(290f, 230f);
        [SerializeField] private float onboardingCardSpacing = 24f;
        [SerializeField] private Vector2 skipButtonSize = new(140f, 36f);
        [SerializeField] private Vector2 skipButtonPosition = new(0f, 16f);

        private bool listenersBound;

        private void Awake()
        {
            EnsurePresentation();
            if (Application.isPlaying)
            {
                BindListeners();
            }
        }

        private void OnEnable()
        {
            EnsurePresentation();
            if (Application.isPlaying)
            {
                UnbindListeners();
                BindListeners();
            }
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                EnsurePresentation();
            }
        }

        public void BeginLearning()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.CoreKinematicsTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.CoreKinematicsTrack, 0);
            SessionContextStore.Clear();
            SceneNavigator.Load(SceneId.Home);
        }

        public void BeginAsBeginner()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.MathReadinessTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.MathReadinessTrack, 0);
            SessionContextStore.Clear();
            RobotSelectionBridge.SetSelection(DefaultRobotId, RobotSelectionBridge.GuidedLessonMode);
            SceneNavigator.Load(SceneId.Main);
        }

        public void SkipToMain()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.CoreKinematicsTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.CoreKinematicsTrack, 0);
            SessionContextStore.Clear();
            SceneNavigator.Load(SceneId.Home);
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            canvasRoot ??= transform as RectTransform;

            if (canvasRoot == null)
            {
                return;
            }

            canvasRoot.gameObject.SetActive(true);
            sceneNavigationBar ??= GetComponent<SceneNavigationBar>();
            if (sceneNavigationBar != null)
            {
                sceneNavigationBar.enabled = enableGlobalDebugNavigation;
            }

            UiRuntimeStyle.EnsureImage(canvasRoot, "ScreenBg", UIDesignTokens.Colors.SurfaceBase);
            var screenBg = canvasRoot.Find("ScreenBg") as RectTransform;
            if (screenBg != null)
            {
                screenBg.SetAsFirstSibling();
                UiRuntimeStyle.Stretch(screenBg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }

            modalRoot ??= UiRuntimeStyle.EnsureRectChild(canvasRoot, "WelcomeModal");
            UiRuntimeStyle.Anchor(modalRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), modalSize, Vector2.zero);
            modalRoot.SetAsLastSibling();

            var modalImage = UiRuntimeStyle.EnsureImage(modalRoot, "ModalSurface", UIDesignTokens.Colors.SurfaceRaised);
            var surface = (RectTransform)modalImage.transform;
            UiRuntimeStyle.Stretch(surface, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            MoveIntoSurface(surface, "HeadlineText");
            MoveIntoSurface(surface, "BodyText");
            MoveIntoSurface(surface, "CardRow");
            MoveIntoSurface(surface, "BtnOnboardingSkip");
            RemoveLegacyDirectChild(surface, "BtnStartLearning");
            RemoveLegacyDirectChild(surface, "BtnBeginner");

            var title = UiRuntimeStyle.EnsureText(surface, "HeadlineText", fallbackFont,
                UIDesignTokens.Type.DisplayLg, FontStyle.Bold, TextAnchor.MiddleCenter,
                UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                headlineSize, headlinePosition);
            title.text = "KineTutor3D";
            headlineText = title;

            var body = UiRuntimeStyle.EnsureText(surface, "BodyText", fallbackFont,
                UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.MiddleCenter,
                UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                bodySize, bodyPosition);
            body.text = "어떤 수준에서 시작할까요?";
            bodyText = body;

            var cardRow = UiRuntimeStyle.EnsureRectChild(surface, "CardRow");
            UiRuntimeStyle.Anchor(cardRow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                cardRowSize, cardRowPosition);
            var hLayout = cardRow.GetComponent<HorizontalLayoutGroup>();
            if (hLayout == null)
            {
                hLayout = cardRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            hLayout.spacing = onboardingCardSpacing;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;

            beginnerButton = BuildSelectionCard(cardRow, "BtnBeginner",
                "icon-user",
                "처음이에요",
                "감각부터 시작해요.\n로봇 팔을 움직여보며\n직관을 쌓아갑니다.",
                UIDesignTokens.Colors.AccentSecondary,
                onboardingCardSize);

            startLearningButton = BuildSelectionCard(cardRow, "BtnStartLearning",
                "icon-play",
                "알고 있어요",
                "DH 파라미터와 행렬로\n바로 들어갑니다.\n기구학 학습을 시작하세요.",
                UIDesignTokens.Colors.AccentPrimary,
                onboardingCardSize);

            skipButton = UIComponentFactory.CreateGhostButton(surface, "BtnOnboardingSkip", "둘러보기 →");
            var skipRect = skipButton.transform as RectTransform;
            if (skipRect != null)
            {
                UiRuntimeStyle.Anchor(skipRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    skipButtonSize, skipButtonPosition);
            }

            var skipLabel = skipButton.transform.Find("Label")?.GetComponent<Text>();
            if (skipLabel != null)
            {
                skipLabel.color = UIDesignTokens.Colors.TextMuted;
            }
        }

        private Button BuildSelectionCard(Transform parent, string name,
            string iconName, string titleText, string descriptionText, Color accentColor, Vector2 cardSize)
        {
            var cardRect = UiRuntimeStyle.EnsureRectChild(parent, name);
            cardRect.sizeDelta = cardSize;

            var cardHitArea = cardRect.GetComponent<Image>();
            if (cardHitArea == null)
            {
                cardHitArea = cardRect.gameObject.AddComponent<Image>();
            }

            cardHitArea.color = Color.clear;
            cardHitArea.raycastTarget = true;

            var bg = UiRuntimeStyle.EnsureImage(cardRect, "CardBg", UIDesignTokens.Colors.SurfaceCard);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var stripe = UiRuntimeStyle.EnsureImage(cardRect, "AccentStripe", accentColor);
            UiRuntimeStyle.Anchor((RectTransform)stripe.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, 4f), new Vector2(0f, 0f));

            var icon = UIIconResolver.CreateIcon(cardRect, "CardIcon", iconName,
                UIDesignTokens.Size.IconLg + 8f, accentColor);
            if (icon != null)
            {
                UiRuntimeStyle.Anchor(icon.rectTransform,
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(40f, 40f), new Vector2(0f, -UIDesignTokens.Space.Lg));
            }

            var cardTitle = UiRuntimeStyle.EnsureText(cardRect, "CardTitle", fallbackFont,
                UIDesignTokens.Type.HeadingLg, FontStyle.Bold, TextAnchor.MiddleCenter,
                UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(cardTitle.rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(250f, 30f), new Vector2(0f, -78f));
            cardTitle.text = titleText;

            var cardDesc = UiRuntimeStyle.EnsureText(cardRect, "CardDesc", fallbackFont,
                UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperCenter,
                UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(cardDesc.rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(240f, 70f), new Vector2(0f, -116f));
            cardDesc.text = descriptionText;

            var button = cardRect.GetComponent<Button>();
            if (button == null)
            {
                button = cardRect.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = cardHitArea;
            button.colors = UIDesignTokens.ButtonColors(UIDesignTokens.Colors.SurfaceCard);

            var hoverLabel = UiRuntimeStyle.EnsureText(cardRect, "HoverLabel", fallbackFont,
                UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleCenter,
                accentColor);
            UiRuntimeStyle.Anchor(hoverLabel.rectTransform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(200f, 24f), new Vector2(0f, UIDesignTokens.Space.Sm));
            hoverLabel.text = "선택하기 →";

            return button;
        }

        private void MoveIntoSurface(RectTransform modalSurfaceRect, string childName)
        {
            if (modalSurfaceRect == null || modalRoot == null)
            {
                return;
            }

            var directChild = modalRoot.Find(childName) as RectTransform;
            if (directChild != null && directChild.parent != modalSurfaceRect)
            {
                directChild.SetParent(modalSurfaceRect, false);
            }
        }

        private void RemoveLegacyDirectChild(RectTransform parent, string childName)
        {
            if (parent == null)
            {
                return;
            }

            var directChild = parent.Find(childName);
            if (directChild == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(directChild.gameObject);
            }
            else
            {
                DestroyImmediate(directChild.gameObject);
            }
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            if (startLearningButton != null)
            {
                startLearningButton.onClick.RemoveListener(BeginLearning);
                startLearningButton.onClick.AddListener(BeginLearning);
            }

            if (beginnerButton != null)
            {
                beginnerButton.onClick.RemoveListener(BeginAsBeginner);
                beginnerButton.onClick.AddListener(BeginAsBeginner);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(SkipToMain);
                skipButton.onClick.AddListener(SkipToMain);
            }

            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            if (startLearningButton != null)
            {
                startLearningButton.onClick.RemoveListener(BeginLearning);
            }

            if (beginnerButton != null)
            {
                beginnerButton.onClick.RemoveListener(BeginAsBeginner);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(SkipToMain);
            }

            listenersBound = false;
        }
    }
}
