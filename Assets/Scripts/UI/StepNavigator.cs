using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Prev/Next/Skip 버튼과 스텝 인디케이터를 제어합니다.
    /// </summary>
    public class StepNavigator : MonoBehaviour
    {
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

        private void Awake()
        {
            AutoWire();
            EnsureBottomBarPresentation();
        }

        private void Update()
        {
            UpdateSliderValueText(jointSlider1, slider1ValueText, "θ1");
            UpdateSliderValueText(jointSlider2, slider2ValueText, "θ2");
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
                stepIndicatorText.text = $"Step {currentStep}/{totalSteps}";
            }
        }

        public void SetNextInteractable(bool interactable)
        {
            if (nextButton != null) nextButton.interactable = interactable;
        }

        public void SetPreviousInteractable(bool interactable)
        {
            if (prevButton != null) prevButton.interactable = interactable;
        }

        public void SetSkipVisible(bool visible)
        {
            if (skipButton != null) skipButton.gameObject.SetActive(visible);
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

            if (prevButton == null)
            {
                var go = GameObject.Find("BtnPrev");
                if (go != null) prevButton = go.GetComponent<Button>();
            }

            if (nextButton == null)
            {
                var go = GameObject.Find("BtnNext");
                if (go != null) nextButton = go.GetComponent<Button>();
            }

            if (skipButton == null)
            {
                var go = GameObject.Find("BtnSkip");
                if (go != null) skipButton = go.GetComponent<Button>();
            }

            if (stepIndicatorText == null)
            {
                var go = GameObject.Find("StepIndicatorText");
                if (go != null) stepIndicatorText = go.GetComponent<Text>();
            }

            if (jointSlider1 == null)
            {
                var go = GameObject.Find("joint_slider_1");
                if (go != null) jointSlider1 = go.GetComponent<Slider>();
            }

            if (jointSlider2 == null)
            {
                var go = GameObject.Find("joint_slider_2");
                if (go != null) jointSlider2 = go.GetComponent<Slider>();
            }
        }

        private void EnsureBottomBarPresentation()
        {
            var rect = transform as RectTransform;
            if (rect != null)
            {
                UiRuntimeStyle.Stretch(rect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 16f), new Vector2(-16f, 132f));
            }

            if (bottomBarBackground == null)
            {
                bottomBarBackground = UiRuntimeStyle.EnsureImage(transform, "BottomBarBackground", UiRuntimeStyle.PanelBackgroundAlt);
            }

            UiRuntimeStyle.Stretch((RectTransform)bottomBarBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            if (jointSlider1 != null)
            {
                slider1ValueText = StyleSlider(jointSlider1, "Joint 1", new Vector2(210f, 0f));
            }

            if (jointSlider2 != null)
            {
                slider2ValueText = StyleSlider(jointSlider2, "Joint 2", new Vector2(560f, 0f));
            }

            if (prevButton != null)
            {
                StyleButton(prevButton, "Prev", new Vector2(-340f, 0f), UiRuntimeStyle.CardBackground);
            }

            if (skipButton != null)
            {
                StyleButton(skipButton, "Skip", new Vector2(-220f, 0f), UiRuntimeStyle.DangerMuted);
            }

            if (nextButton != null)
            {
                StyleButton(nextButton, "Next", new Vector2(-100f, 0f), UiRuntimeStyle.AccentBlue);
            }
        }

        private Text StyleSlider(Slider slider, string label, Vector2 anchoredPosition)
        {
            var sliderRect = slider.transform as RectTransform;
            UiRuntimeStyle.Anchor(sliderRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(300f, 72f), anchoredPosition);

            var rootImage = slider.GetComponent<Image>();
            if (rootImage == null)
            {
                rootImage = slider.gameObject.AddComponent<Image>();
            }

            rootImage.color = UiRuntimeStyle.CardBackground;

            var labelText = UiRuntimeStyle.EnsureText(slider.transform, "SliderLabel", fallbackFont, 14, FontStyle.Bold, TextAnchor.UpperLeft, UiRuntimeStyle.TextPrimary);
            UiRuntimeStyle.Anchor(labelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(120f, 20f), new Vector2(12f, -10f));
            labelText.text = label;

            var valueText = UiRuntimeStyle.EnsureText(slider.transform, "SliderValueText", fallbackFont, 13, FontStyle.Normal, TextAnchor.UpperRight, UiRuntimeStyle.AccentYellow);
            UiRuntimeStyle.Anchor(valueText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(84f, 20f), new Vector2(-12f, -10f));

            var track = UiRuntimeStyle.EnsureImage(slider.transform, "Track", new Color(0.07f, 0.08f, 0.12f, 0.95f));
            UiRuntimeStyle.Anchor(track.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(256f, 16f), new Vector2(0f, 16f));

            var fillArea = UiRuntimeStyle.EnsureRectChild(slider.transform, "Fill Area");
            UiRuntimeStyle.Stretch(fillArea, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(22f, 16f), new Vector2(-22f, 32f));
            var fill = UiRuntimeStyle.EnsureImage(fillArea, "Fill", UiRuntimeStyle.AccentBlue);
            UiRuntimeStyle.Stretch(fill.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var handleArea = UiRuntimeStyle.EnsureRectChild(slider.transform, "Handle Slide Area");
            UiRuntimeStyle.Stretch(handleArea, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 12f), new Vector2(-12f, 36f));
            var handle = UiRuntimeStyle.EnsureImage(handleArea, "Handle", UiRuntimeStyle.AccentYellow);
            UiRuntimeStyle.Anchor(handle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(18f, 18f), Vector2.zero);

            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;

            return valueText;
        }

        private void StyleButton(Button button, string label, Vector2 anchoredPosition, Color backgroundColor)
        {
            UiRuntimeStyle.Anchor(button.transform as RectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(96f, 42f), anchoredPosition);
            UiRuntimeStyle.EnsureButtonLabel(button, fallbackFont, label, backgroundColor);
        }

        private static void UpdateSliderValueText(Slider slider, Text valueText, string prefix)
        {
            if (slider == null || valueText == null)
            {
                return;
            }

            valueText.text = $"{prefix} {slider.value:0.0} deg";
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
