using System;
using System.Linq;
using KineTutor3D.UI;
using KineTutor3D.UI.Data;
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// 튜토리얼 스텝 전환과 UI 학습 상태 동기화를 담당합니다.
    /// </summary>
    public class AppController : MonoBehaviour
    {
        [Header("Step Data")]
        [SerializeField] private TutorStepConfig[] stepConfigs = Array.Empty<TutorStepConfig>();

        [Header("UI References")]
        [SerializeField] private ProgressiveDisclosureController disclosureController;
        [SerializeField] private InteractionGateController gateController;
        [SerializeField] private StepTutorPanel stepTutorPanel;
        [SerializeField] private StepNavigator stepNavigator;
        [SerializeField] private OnboardingManager onboardingManager;
        [SerializeField] private ToastNotificationController toastController;
        [SerializeField] private FocusZoneHighlighter focusHighlighter;

        private int currentStepIndex;

        public event Action<int, TutorStepConfig> OnStepChanged;
        public event Action<InteractionType, string> OnInteractionEvent;

        public int CurrentStep => currentStepIndex + 1;
        public int TotalSteps => stepConfigs?.Length ?? 0;

        private void Awake()
        {
            AutoWireReferences();
            LoadStepConfigsIfNeeded();

            if (gateController != null)
            {
                gateController.GateStateChanged += HandleGateStateChanged;
            }
        }

        private void Start()
        {
            if (TotalSteps <= 0)
            {
                Debug.LogWarning("[AppController] Step config is empty.");
                return;
            }

            stepNavigator?.Bind(this);

            if (onboardingManager != null)
            {
                onboardingManager.Initialize(this);
                return;
            }

            SetCurrentStep(Mathf.Clamp(StepProgressSaver.GetResumeStep(1), 1, TotalSteps));
        }

        private void OnDestroy()
        {
            if (gateController != null)
            {
                gateController.GateStateChanged -= HandleGateStateChanged;
            }
        }

        public void SetCurrentStep(int oneBasedStep)
        {
            if (TotalSteps <= 0)
            {
                return;
            }

            currentStepIndex = Mathf.Clamp(oneBasedStep - 1, 0, TotalSteps - 1);
            var config = stepConfigs[currentStepIndex];

            disclosureController?.ApplyStep(config);
            focusHighlighter?.ApplyFocus(config.focusTarget, config.focusHighlightColor);
            gateController?.LoadStep(config, CurrentStep);
            stepTutorPanel?.ApplyStep(config, CurrentStep, TotalSteps, gateController == null || gateController.IsGateSatisfied, gateController?.GetProgressText() ?? string.Empty);

            if (stepNavigator != null)
            {
                stepNavigator.UpdateStep(CurrentStep, TotalSteps);
                stepNavigator.SetPreviousInteractable(CurrentStep > 1);
                stepNavigator.SetSkipVisible(CurrentStep < TotalSteps);
                stepNavigator.SetNextInteractable(gateController == null || gateController.IsGateSatisfied || CurrentStep >= TotalSteps);
            }

            OnStepChanged?.Invoke(CurrentStep, config);
        }

        public void NextStep()
        {
            if (CurrentStep >= TotalSteps)
            {
                return;
            }

            StepProgressSaver.SaveLastCompletedStep(CurrentStep);
            SetCurrentStep(CurrentStep + 1);
        }

        public void PreviousStep()
        {
            SetCurrentStep(CurrentStep - 1);
        }

        public void SkipCurrentStep()
        {
            if (CurrentStep >= TotalSteps)
            {
                return;
            }

            gateController?.SkipCurrentGate();
            NextStep();
        }

        public void JumpToSandbox()
        {
            SetCurrentStep(TotalSteps);
        }

        public void ReportInteraction(InteractionType interactionType, string targetId)
        {
            OnInteractionEvent?.Invoke(interactionType, targetId);
            gateController?.RegisterInteraction(interactionType, targetId);
            stepTutorPanel?.UpdateGateState(gateController == null || gateController.IsGateSatisfied, gateController?.GetProgressText() ?? string.Empty);
        }

        private void HandleGateStateChanged(bool gateSatisfied, string completionMessage)
        {
            if (stepNavigator != null)
            {
                stepNavigator.SetNextInteractable(gateSatisfied || CurrentStep >= TotalSteps);
            }

            stepTutorPanel?.UpdateGateState(gateSatisfied, gateController?.GetProgressText() ?? string.Empty);

            if (gateSatisfied && !string.IsNullOrWhiteSpace(completionMessage))
            {
                toastController?.ShowSuccess(completionMessage, 5f);
            }
        }

        private void AutoWireReferences()
        {
            disclosureController ??= FindFirstObjectByType<ProgressiveDisclosureController>(FindObjectsInactive.Include);
            gateController ??= FindFirstObjectByType<InteractionGateController>(FindObjectsInactive.Include);
            stepTutorPanel ??= FindFirstObjectByType<StepTutorPanel>(FindObjectsInactive.Include);
            stepNavigator ??= FindFirstObjectByType<StepNavigator>(FindObjectsInactive.Include);
            onboardingManager ??= FindFirstObjectByType<OnboardingManager>(FindObjectsInactive.Include);
            toastController ??= FindFirstObjectByType<ToastNotificationController>(FindObjectsInactive.Include);
            focusHighlighter ??= FindFirstObjectByType<FocusZoneHighlighter>(FindObjectsInactive.Include);
        }

        private void LoadStepConfigsIfNeeded()
        {
            if (stepConfigs != null && stepConfigs.Length > 0)
            {
                return;
            }

            var loaded = Resources.LoadAll<TutorStepConfig>("TutorSteps");
            if (loaded != null && loaded.Length > 0)
            {
                stepConfigs = loaded.OrderBy(x => x.name).ToArray();
                return;
            }

            stepConfigs = TutorStepRuntimeFactory.CreateDefaults();
        }
    }
}
