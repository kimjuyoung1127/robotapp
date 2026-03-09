using System.Collections;
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 첫 실행 온보딩 모달과 안내 시퀀스를 제어합니다.
    /// </summary>
    public class OnboardingManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject welcomeModal;
        [SerializeField] private Button startLearningButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Text guideText;

        [Header("Spotlight")]
        [SerializeField] private SpotlightOverlay spotlightOverlay;
        [SerializeField] private RectTransform robotViewportTarget;
        [SerializeField] private RectTransform dhTableTarget;
        [SerializeField] private RectTransform dhCellTarget;

        [Header("Data")]
        [SerializeField] private OnboardingSequenceConfig sequenceConfig;

        private AppController appController;

        private void Awake()
        {
            AutoWire();
            HideInvalidWelcomeModal();
            EnsureViewportTargetLayout();
            LoadSequenceIfNeeded();
        }

        private void OnEnable()
        {
            AutoWire();
            HideInvalidWelcomeModal();
        }

        public void Initialize(AppController owner)
        {
            appController = owner;
            BindButtons();

            if (!StepProgressSaver.HasVisited() && CanPresentWelcomeModal())
            {
                SetModalVisible(true);
                return;
            }

            SetModalVisible(false);
            if (!StepProgressSaver.HasVisited())
            {
                StepProgressSaver.MarkVisited();
            }

            var resumeStep = Mathf.Clamp(StepProgressSaver.GetResumeStep(1), 1, appController.TotalSteps);
            appController.SetCurrentStep(resumeStep);
        }

        private void BindButtons()
        {
            if (startLearningButton != null)
            {
                startLearningButton.onClick.RemoveListener(OnStartLearningClicked);
                startLearningButton.onClick.AddListener(OnStartLearningClicked);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(OnSkipClicked);
                skipButton.onClick.AddListener(OnSkipClicked);
            }
        }

        private void OnStartLearningClicked()
        {
            StepProgressSaver.MarkVisited();
            SetModalVisible(false);
            appController?.SetCurrentStep(1);
            StartCoroutine(RunGuideSequence());
        }

        private void OnSkipClicked()
        {
            StepProgressSaver.MarkVisited();
            SetModalVisible(false);
            appController?.JumpToSandbox();
        }

        private void SetModalVisible(bool visible)
        {
            if (welcomeModal != null)
            {
                welcomeModal.SetActive(visible);
            }

            if (!visible && guideText != null)
            {
                guideText.text = string.Empty;
            }
        }

        private bool CanPresentWelcomeModal()
        {
            return welcomeModal != null &&
                   welcomeModal.transform is RectTransform &&
                   startLearningButton != null &&
                   skipButton != null &&
                   guideText != null;
        }

        private IEnumerator RunGuideSequence()
        {
            if (sequenceConfig == null || sequenceConfig.events == null)
            {
                yield break;
            }

            spotlightOverlay?.Show(0.6f);

            foreach (var evt in sequenceConfig.events)
            {
                if (evt == null)
                {
                    continue;
                }

                if (evt.delaySeconds > 0f)
                {
                    yield return new WaitForSecondsRealtime(evt.delaySeconds);
                }

                if (guideText != null)
                {
                    guideText.text = evt.messageKo;
                }

                var target = ResolveTarget(evt.target);
                if (target != null)
                {
                    spotlightOverlay?.Focus(target);
                }
                else
                {
                    spotlightOverlay?.ClearFocus();
                }
            }

            yield return new WaitForSecondsRealtime(0.3f);
            spotlightOverlay?.Hide();
        }

        private RectTransform ResolveTarget(OnboardingTarget target)
        {
            switch (target)
            {
                case OnboardingTarget.RobotViewport:
                    return robotViewportTarget;
                case OnboardingTarget.DHTable:
                    return dhTableTarget;
                case OnboardingTarget.DHCell:
                    return dhCellTarget;
                default:
                    return null;
            }
        }

        private void AutoWire()
        {
            spotlightOverlay ??= FindFirstObjectByType<SpotlightOverlay>(FindObjectsInactive.Include);

            if (welcomeModal == null)
            {
                var modal = GameObject.Find("WelcomeModal");
                if (modal != null)
                {
                    welcomeModal = modal;
                }
            }

            if (startLearningButton == null)
            {
                var go = GameObject.Find("BtnStartLearning");
                if (go != null) startLearningButton = go.GetComponent<Button>();
            }

            if (skipButton == null)
            {
                var go = GameObject.Find("BtnOnboardingSkip");
                if (go != null) skipButton = go.GetComponent<Button>();
            }

            if (guideText == null)
            {
                var go = GameObject.Find("OnboardingGuideText");
                if (go != null) guideText = go.GetComponent<Text>();
            }

            if (robotViewportTarget == null)
            {
                var go = GameObject.Find("ViewportTarget");
                if (go != null) robotViewportTarget = go.GetComponent<RectTransform>();
            }

            if (dhTableTarget == null)
            {
                var go = GameObject.Find("DHTableTarget");
                if (go != null) dhTableTarget = go.GetComponent<RectTransform>();
            }

            if (dhCellTarget == null)
            {
                var go = GameObject.Find("DHCellTarget");
                if (go != null) dhCellTarget = go.GetComponent<RectTransform>();
            }
        }

        private void HideInvalidWelcomeModal()
        {
            if (welcomeModal == null)
            {
                return;
            }

            if (welcomeModal.transform is RectTransform)
            {
                return;
            }

            welcomeModal.SetActive(false);

            if (startLearningButton != null)
            {
                startLearningButton.gameObject.SetActive(false);
            }

            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(false);
            }

            if (guideText != null)
            {
                guideText.gameObject.SetActive(false);
                guideText.text = string.Empty;
            }
        }

        private void EnsureViewportTargetLayout()
        {
            if (robotViewportTarget == null)
            {
                return;
            }

            var isDefaultPlaceholder =
                robotViewportTarget.anchorMin == new Vector2(0.5f, 0.5f) &&
                robotViewportTarget.anchorMax == new Vector2(0.5f, 0.5f) &&
                robotViewportTarget.sizeDelta == new Vector2(100f, 100f);

            if (!isDefaultPlaceholder)
            {
                return;
            }

            robotViewportTarget.anchorMin = new Vector2(0.22f, 0.18f);
            robotViewportTarget.anchorMax = new Vector2(0.8f, 0.82f);
            robotViewportTarget.offsetMin = new Vector2(12f, 12f);
            robotViewportTarget.offsetMax = new Vector2(-12f, -12f);
            robotViewportTarget.anchoredPosition = Vector2.zero;
            robotViewportTarget.sizeDelta = Vector2.zero;
            robotViewportTarget.pivot = new Vector2(0.5f, 0.5f);
        }

        private void LoadSequenceIfNeeded()
        {
            if (sequenceConfig != null)
            {
                return;
            }

            sequenceConfig = Resources.Load<OnboardingSequenceConfig>("Onboarding/OnboardingSequenceConfig");
            if (sequenceConfig != null)
            {
                return;
            }

            sequenceConfig = ScriptableObject.CreateInstance<OnboardingSequenceConfig>();
            sequenceConfig.events = new[]
            {
                new OnboardingStepEvent { delaySeconds = 0.5f, target = OnboardingTarget.RobotViewport, messageKo = "이것은 2자유도(2DOF) 로봇입니다." },
                new OnboardingStepEvent { delaySeconds = 3.0f, target = OnboardingTarget.DHTable, messageKo = "DH 테이블에는 각 관절의 파라미터가 정리되어 있습니다." },
                new OnboardingStepEvent { delaySeconds = 3.0f, target = OnboardingTarget.DHCell, messageKo = "셀 위에 마우스를 올려 해당 파라미터를 확인해보세요." },
                new OnboardingStepEvent { delaySeconds = 3.0f, target = OnboardingTarget.None, messageKo = "이제 직접 조작을 시작해보세요." }
            };
        }
    }
}
