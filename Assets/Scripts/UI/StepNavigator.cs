// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Prev/Next/Skip 버튼과 스텝 인디케이터를 제어합니다.
    /// </summary>
    [ExecuteAlways]
    public class StepNavigator : MonoBehaviour
    {
        [SerializeField] private RectTransform bottomBarRoot;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Text stepIndicatorText;
        [SerializeField] private Slider jointSlider1;
        [SerializeField] private Slider jointSlider2;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Graphic bottomBarBackground;

        private AppController appController;
        private Text slider1ValueText;
        private Text slider2ValueText;
        private Text controlTitleText;
        private Text controlHintText;

        private void Awake()
        {
            AutoWire();
            EnsureBottomBarPresentation();
        }

        private void OnEnable()
        {
            AutoWire();
            EnsureBottomBarPresentation();
        }

        private void Update()
        {
            UpdateSliderValueText(jointSlider1, slider1ValueText, "J1");
            UpdateSliderValueText(jointSlider2, slider2ValueText, "J2");
        }

        public void Bind(AppController owner)
        {
            appController = owner;
            WireButtons();
        }

        public void UpdateStep(int currentStep, int totalSteps)
        {
            if (stepIndicatorText != null)
            {
                var config = appController != null ? appController.CurrentStepConfig : null;
                if (config != null && config.mathReadinessMode)
                {
                    stepIndicatorText.gameObject.SetActive(true);
                    stepIndicatorText.text = config.stepTitleKo;
                    stepIndicatorText.fontSize = UIDesignTokens.Type.Caption + 1;
                    stepIndicatorText.color = UIDesignTokens.Colors.TextMuted;
                }
                else
                {
                    stepIndicatorText.gameObject.SetActive(true);
                    stepIndicatorText.text = $"Step {currentStep}/{totalSteps}";
                    stepIndicatorText.fontSize = UIDesignTokens.Type.Body;
                    stepIndicatorText.color = UIDesignTokens.Colors.TextSecondary;
                }
            }

            ApplyBottomBarCopy();
        }

        public void SetNextInteractable(bool interactable)
        {
            if (nextButton != null)
            {
                nextButton.interactable = interactable;
            }
        }

        public void SetPreviousInteractable(bool interactable)
        {
            if (prevButton != null)
            {
                prevButton.interactable = interactable;
            }
        }

        public void SetSkipVisible(bool visible)
        {
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(visible);
            }
        }

        private void WireButtons()
        {
            if (prevButton != null)
            {
                prevButton.onClick.RemoveListener(OnPrevClicked);
                prevButton.onClick.AddListener(OnPrevClicked);
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(OnNextClicked);
                nextButton.onClick.AddListener(OnNextClicked);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(OnSkipClicked);
                skipButton.onClick.AddListener(OnSkipClicked);
            }
        }

        private void AutoWire()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);

            if (prevButton == null && canvas != null)
            {
                foreach (var button in canvas.GetComponentsInChildren<Button>(true))
                {
                    if (button.gameObject.name == "BtnPrev")
                    {
                        prevButton = button;
                        break;
                    }
                }
            }

            if (nextButton == null && canvas != null)
            {
                foreach (var button in canvas.GetComponentsInChildren<Button>(true))
                {
                    if (button.gameObject.name == "BtnNext")
                    {
                        nextButton = button;
                        break;
                    }
                }
            }

            if (skipButton == null && canvas != null)
            {
                foreach (var button in canvas.GetComponentsInChildren<Button>(true))
                {
                    if (button.gameObject.name == "BtnSkip")
                    {
                        skipButton = button;
                        break;
                    }
                }
            }

            if (stepIndicatorText == null && canvas != null)
            {
                foreach (var text in canvas.GetComponentsInChildren<Text>(true))
                {
                    if (text.gameObject.name == "StepIndicatorText")
                    {
                        stepIndicatorText = text;
                        break;
                    }
                }
            }

            if (jointSlider1 == null && canvas != null)
            {
                foreach (var slider in canvas.GetComponentsInChildren<Slider>(true))
                {
                    if (slider.gameObject.name == "joint_slider_1")
                    {
                        jointSlider1 = slider;
                        break;
                    }
                }
            }

            if (jointSlider2 == null && canvas != null)
            {
                foreach (var slider in canvas.GetComponentsInChildren<Slider>(true))
                {
                    if (slider.gameObject.name == "joint_slider_2")
                    {
                        jointSlider2 = slider;
                        break;
                    }
                }
            }
        }

        private void EnsureBottomBarPresentation()
        {
            bottomBarRoot ??= UiRuntimeStyle.EnsureHostedRoot(this, "BottomBarRect");
            UiRuntimeStyle.Stretch(bottomBarRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 16f), new Vector2(-16f, 128f));

            if (bottomBarBackground == null)
            {
                bottomBarBackground = UiRuntimeStyle.EnsureImage(bottomBarRoot, "BottomBarBackground", UIDesignTokens.Colors.SurfaceRaised);
            }
            else
            {
                UiRuntimeStyle.ReparentTo(bottomBarBackground, bottomBarRoot);
            }

            UiRuntimeStyle.Stretch((RectTransform)bottomBarBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            controlTitleText ??= UiRuntimeStyle.EnsureText(bottomBarRoot, "BottomBarTitleText", fallbackFont,
                UIDesignTokens.Type.Caption + 1, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(controlTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(160f, 18f), new Vector2(20f, -10f));

            controlHintText ??= UiRuntimeStyle.EnsureText(bottomBarRoot, "BottomBarHintText", fallbackFont,
                UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(controlHintText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(340f, 18f), new Vector2(20f, 10f));
            ApplyBottomBarCopy();

            if (jointSlider1 != null)
            {
                UiRuntimeStyle.ReparentTo(jointSlider1, bottomBarRoot);
                slider1ValueText = StyleSlider(jointSlider1, "Joint 1", new Vector2(659f, 229f), 308f);
            }

            if (jointSlider2 != null)
            {
                UiRuntimeStyle.ReparentTo(jointSlider2, bottomBarRoot);
                slider2ValueText = StyleSlider(jointSlider2, "Joint 2", new Vector2(999f, 229f), 256f);
            }

            if (prevButton != null)
            {
                UiRuntimeStyle.ReparentTo(prevButton, bottomBarRoot);
                StyleButton(prevButton, "Prev", new Vector2(-1587f, 473f), new Vector2(84f, 44f),
                    UIDesignTokens.Colors.SurfaceCard, UIDesignTokens.Colors.TextPrimary);
            }

            if (skipButton != null)
            {
                UiRuntimeStyle.ReparentTo(skipButton, bottomBarRoot);
                StyleButton(skipButton, "Skip", new Vector2(-1477f, 473f), new Vector2(72f, 44f),
                    new Color(UIDesignTokens.Colors.SurfaceCard.r, UIDesignTokens.Colors.SurfaceCard.g,
                        UIDesignTokens.Colors.SurfaceCard.b, 0.32f),
                    UIDesignTokens.Colors.TextMuted);
            }

            if (nextButton != null)
            {
                UiRuntimeStyle.ReparentTo(nextButton, bottomBarRoot);
                StyleButton(nextButton, "Next", new Vector2(-1347f, 473f), new Vector2(116f, 44f),
                    UIDesignTokens.Colors.AccentPrimary, UIDesignTokens.Colors.TextOnAccent);
            }
        }

        private Text StyleSlider(Slider slider, string label, Vector2 anchoredPosition, float width)
        {
            var sliderRect = slider.transform as RectTransform;
            UiRuntimeStyle.Anchor(sliderRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(width, 76f), anchoredPosition);

            var rootImage = slider.GetComponent<Image>();
            if (rootImage == null)
            {
                rootImage = slider.gameObject.AddComponent<Image>();
            }

            rootImage.color = UIDesignTokens.Colors.SurfaceCard;

            var labelText = UiRuntimeStyle.EnsureText(slider.transform, "SliderLabel", fallbackFont,
                UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(labelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(120f, 20f), new Vector2(16f, -10f));
            labelText.text = label;

            var valueText = UiRuntimeStyle.EnsureText(slider.transform, "SliderValueText", fallbackFont,
                UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperRight, UIDesignTokens.Colors.AccentSecondary);
            UiRuntimeStyle.Anchor(valueText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(96f, 20f), new Vector2(-16f, -10f));

            var track = UiRuntimeStyle.EnsureImage(slider.transform, "Track", UIDesignTokens.Colors.SliderTrack);
            UiRuntimeStyle.Anchor(track.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(width - 40f, 16f), new Vector2(0f, 16f));

            var fillArea = UiRuntimeStyle.EnsureRectChild(slider.transform, "Fill Area");
            UiRuntimeStyle.Stretch(fillArea, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(22f, 16f), new Vector2(-22f, 32f));
            var fill = UiRuntimeStyle.EnsureImage(fillArea, "Fill", UIDesignTokens.Colors.AccentPrimary);
            UiRuntimeStyle.Stretch(fill.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var handleArea = UiRuntimeStyle.EnsureRectChild(slider.transform, "Handle Slide Area");
            UiRuntimeStyle.Stretch(handleArea, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 12f), new Vector2(-12f, 36f));
            var handle = UiRuntimeStyle.EnsureImage(handleArea, "Handle", UIDesignTokens.Colors.AccentSecondary);
            UiRuntimeStyle.Anchor(handle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(18f, 18f), Vector2.zero);

            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;

            return valueText;
        }

        private void StyleButton(Button button, string label, Vector2 anchoredPosition, Vector2 size, Color backgroundColor, Color labelColor)
        {
            UiRuntimeStyle.Anchor(button.transform as RectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), size, anchoredPosition);
            var labelText = UiRuntimeStyle.EnsureButtonLabel(button, fallbackFont, label, backgroundColor);
            labelText.color = labelColor;
            button.colors = UIDesignTokens.ButtonColors(backgroundColor);
        }

        private static void UpdateSliderValueText(Slider slider, Text valueText, string prefix)
        {
            if (slider == null || valueText == null)
            {
                return;
            }

            valueText.text = $"{prefix} {slider.value:0.0} deg";
        }

        private void ApplyBottomBarCopy()
        {
            if (controlTitleText == null || controlHintText == null)
            {
                return;
            }

            var config = appController != null ? appController.CurrentStepConfig : null;
            if (config != null && config.mathReadinessMode)
            {
                controlTitleText.text = "관절 조작";
                controlHintText.text = "슬라이더를 움직이며 방향을 먼저 느껴보세요.";
                return;
            }

            if (config != null && config.beginnerMode)
            {
                controlTitleText.text = "움직임 조작";
                controlHintText.text = "관절 값을 바꾸며 링크와 끝점 변화를 관찰하세요.";
                return;
            }

            controlTitleText.text = "Joint Control";
            controlHintText.text = "Adjust the rail to explore the robot state.";
        }

        private void OnPrevClicked()
        {
            appController?.PreviousStep();
        }

        private void OnNextClicked()
        {
            appController?.NextStep();
        }

        private void OnSkipClicked()
        {
            appController?.SkipCurrentStep();
        }
    }
}
