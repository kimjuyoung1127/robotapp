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

        private AppController appController;

        private void Awake()
        {
            AutoWire();
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
