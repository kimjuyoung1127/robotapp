// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 모든 씬에서 공통으로 사용하는 상단 씬 전환 바입니다.
    /// </summary>
    public class SceneNavigationBar : MonoBehaviour, IVisibilityControllable
    {
        private const float NavButtonWidth = 96f;
        private const float NavButtonHeight = 36f;
        private const float NavButtonSpacing = 10f;

        [SerializeField] private RectTransform topBarRoot;
        [SerializeField] private RectTransform buttonContainer;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Graphic topBarBackground;
        [SerializeField] private bool createTitleIfMissing = true;
        [SerializeField] private bool hideOnOnboarding = true;
        [SerializeField] private AppController appController;
        [SerializeField] private Text titleText;
        [SerializeField] private Text contextText;
        private bool? onboardingHideOverride;

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
        }

        private void OnEnable()
        {
            appController ??= FindFirstObjectByType<AppController>(FindObjectsInactive.Include);
            if (appController != null)
            {
                appController.OnStepChanged += HandleStepChanged;
            }
            EnsurePresentation();
        }

        private void OnDisable()
        {
            if (appController != null)
            {
                appController.OnStepChanged -= HandleStepChanged;
            }
        }

        public void SetHideOnOnboarding(bool hide)
        {
            onboardingHideOverride = hide;
            EnsurePresentation();
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            topBarRoot ??= ResolveTopBarRoot();
            var entries = SceneCatalog.GetNavigableEntries();

            if (topBarRoot == null)
            {
                return;
            }

            var currentScene = SceneCatalog.GetCurrentSceneId();
            var hideOnOnboardingValue = onboardingHideOverride ?? hideOnOnboarding;
            var shouldHide = hideOnOnboardingValue && currentScene == SceneId.Onboarding;
            topBarRoot.gameObject.SetActive(!shouldHide);
            if (shouldHide)
            {
                return;
            }

            topBarRoot.SetAsLastSibling();

            UiRuntimeStyle.Stretch(topBarRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -76f), new Vector2(-16f, -16f));

            if (topBarBackground == null)
            {
                topBarBackground = UiRuntimeStyle.EnsureImage(topBarRoot, "TopBarBackground", UIDesignTokens.Colors.SurfaceRaisedAlt);
            }
            else
            {
                UiRuntimeStyle.ReparentTo(topBarBackground, topBarRoot);
            }

            UiRuntimeStyle.Stretch((RectTransform)topBarBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var topBarContentParent = topBarBackground.transform;

            if (createTitleIfMissing)
            {
                var existing = topBarContentParent.Find("TitleText");
                if (existing == null)
                {
                    existing = topBarRoot.Find("TitleText");
                }
                titleText = existing != null ? existing.GetComponent<Text>() : titleText;
                if (titleText == null)
                {
                    titleText = UiRuntimeStyle.EnsureText(topBarContentParent, "TitleText", fallbackFont, UIDesignTokens.Type.DisplaySm, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextPrimary);
                }
                UiRuntimeStyle.ReparentTo(titleText, topBarContentParent);
                UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(150f, 40f), new Vector2(26f, 0f));
                titleText.text = "KineTutor3D";
            }

            contextText ??= topBarContentParent.Find("TopBarContextText")?.GetComponent<Text>();
            if (contextText == null)
            {
                contextText = topBarRoot.Find("TopBarContextText")?.GetComponent<Text>();
            }
            if (contextText == null)
            {
                contextText = UiRuntimeStyle.EnsureText(topBarContentParent, "TopBarContextText", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextSecondary);
            }
            UiRuntimeStyle.ReparentTo(contextText, topBarContentParent);
            UiRuntimeStyle.Anchor(contextText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(360f, 22f), new Vector2(250f, 10f));

            buttonContainer ??= topBarContentParent.Find("SceneNavButtons") as RectTransform;
            if (buttonContainer == null)
            {
                buttonContainer = topBarRoot.Find("SceneNavButtons") as RectTransform;
            }
            if (buttonContainer == null)
            {
                buttonContainer = UiRuntimeStyle.EnsureRectChild(topBarContentParent, "SceneNavButtons");
            }
            UiRuntimeStyle.ReparentTo(buttonContainer, topBarContentParent);
            var navWidth = (entries.Count * NavButtonWidth) + (Mathf.Max(0, entries.Count - 1) * NavButtonSpacing);
            UiRuntimeStyle.Anchor(buttonContainer, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(navWidth, NavButtonHeight), new Vector2(-26f, 0f));

            buttonContainer.gameObject.SetActive(true);
            RebuildButtons();
            ApplyCompactLearningMode(currentScene);
        }

        private RectTransform ResolveTopBarRoot()
        {
            if (transform is RectTransform selfRect && selfRect.GetComponent<Canvas>() != null)
            {
                return UiRuntimeStyle.EnsureRectChild(selfRect, "TopBarRect");
            }

            return UiRuntimeStyle.EnsureHostedRoot(this, "TopBarRect");
        }

        private void RebuildButtons()
        {
            if (buttonContainer == null)
            {
                return;
            }

            var entries = SceneCatalog.GetNavigableEntries();
            var currentScene = SceneCatalog.GetCurrentSceneId();
            ClearButtonContainer();

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var button = ResolveOrCreateButton(i, entry, entry.Id == currentScene);
                var targetScene = entry.Id;
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SceneNavigator.Load(targetScene));
            }
        }

        private void ApplyCompactLearningMode(SceneId currentScene)
        {
            var compactLearning = currentScene == SceneId.MathReadiness &&
                appController != null &&
                appController.CurrentStepConfig != null;
            var stepDetailText = topBarRoot != null ? topBarRoot.Find("StepIndicatorText")?.GetComponent<Text>() : null;

            if (!compactLearning)
            {
                if (contextText != null)
                {
                    contextText.text = string.Empty;
                    contextText.gameObject.SetActive(false);
                }

                if (buttonContainer != null)
                {
                    buttonContainer.gameObject.SetActive(true);
                }

                if (titleText != null)
                {
                    UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(150f, 40f), new Vector2(26f, 0f));
                }

                if (stepDetailText != null)
                {
                    stepDetailText.gameObject.SetActive(true);
                    stepDetailText.font = fallbackFont;
                    stepDetailText.fontSize = UIDesignTokens.Type.Body;
                    stepDetailText.color = UIDesignTokens.Colors.TextSecondary;
                    stepDetailText.alignment = TextAnchor.MiddleLeft;
                    UiRuntimeStyle.Anchor(stepDetailText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(280f, 24f), new Vector2(250f, 0f));
                }

                return;
            }

            if (buttonContainer != null)
            {
                buttonContainer.gameObject.SetActive(false);
            }

            if (titleText != null)
            {
                UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(220f, 28f), new Vector2(26f, 10f));
                titleText.text = "KineTutor3D";
            }

            if (contextText != null)
            {
                contextText.gameObject.SetActive(true);
                var config = appController.CurrentStepConfig;
                var label = config.mathReadinessMode
                    ? "수학 기초 워밍업"
                    : config.beginnerMode
                        ? "Pre-Kinematics"
                        : "Guided Lesson";
                contextText.text = $"{label} · {appController.CurrentStep}/{appController.TotalSteps}";
                contextText.font = fallbackFont;
                contextText.fontSize = UIDesignTokens.Type.Body;
                contextText.color = UIDesignTokens.Colors.TextSecondary;
                contextText.alignment = TextAnchor.MiddleLeft;
                UiRuntimeStyle.Anchor(contextText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(360f, 22f), new Vector2(250f, 10f));
            }

            if (stepDetailText != null)
            {
                var config = appController.CurrentStepConfig;
                stepDetailText.gameObject.SetActive(true);
                stepDetailText.font = fallbackFont;
                stepDetailText.fontSize = UIDesignTokens.Type.Caption + 1;
                stepDetailText.color = UIDesignTokens.Colors.TextMuted;
                stepDetailText.alignment = TextAnchor.MiddleLeft;
                stepDetailText.text = config != null ? config.stepTitleKo : string.Empty;
                UiRuntimeStyle.Anchor(stepDetailText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(420f, 20f), new Vector2(250f, -11f));
            }
        }

        private void HandleStepChanged(int _step, UI.Data.TutorStepConfig _config)
        {
            EnsurePresentation();
        }

        private Button ResolveOrCreateButton(int index, SceneEntry entry, bool isCurrentScene)
        {
            var sanitizedName = entry.DisplayName.Replace(" ", string.Empty);
            var expectedName = $"Nav{sanitizedName}";
            var go = new GameObject(expectedName, typeof(RectTransform), typeof(Image), typeof(Button));
            var button = go.GetComponent<Button>();

            if (button.transform.parent != buttonContainer)
            {
                button.transform.SetParent(buttonContainer, false);
            }

            button.gameObject.SetActive(true);
            var background = isCurrentScene
                ? UIDesignTokens.Colors.NavCurrentScene
                : UIDesignTokens.Colors.AccentPrimary;
            UiRuntimeStyle.EnsureButtonLabel(button, fallbackFont, entry.DisplayName, background);
            UiRuntimeStyle.Anchor(button.transform as RectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(NavButtonWidth, NavButtonHeight), new Vector2(index * (NavButtonWidth + NavButtonSpacing), 0f));
            var layout = UiRuntimeStyle.EnsureLayoutElement(button);
            layout.preferredWidth = NavButtonWidth;
            layout.preferredHeight = NavButtonHeight;
            layout.minWidth = 104f;
            return button;
        }

        private void ClearButtonContainer()
        {
            if (buttonContainer == null)
            {
                return;
            }

            for (var i = buttonContainer.childCount - 1; i >= 0; i--)
            {
                var child = buttonContainer.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    child.gameObject.SetActive(false);
                    child.SetParent(null, false);
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 네비게이션 바 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (topBarRoot != null) topBarRoot.gameObject.SetActive(visible);
        }

    }
}

