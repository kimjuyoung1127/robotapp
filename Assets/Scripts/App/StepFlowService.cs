// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI;
using KineTutor3D.UI.Data;
using UnityEngine;

namespace KineTutor3D.App
{
    internal sealed class StepFlowService
    {
        public int ApplyStep(int oneBasedStep, TutorStepConfig[] stepConfigs, ProgressiveDisclosureController disclosureController, InteractionGateController gateController, StepTutorPanel stepTutorPanel, StepNavigator stepNavigator, FocusZoneHighlighter focusHighlighter)
        {
            if (stepConfigs == null || stepConfigs.Length == 0)
            {
                return 0;
            }

            var currentIndex = Mathf.Clamp(oneBasedStep - 1, 0, stepConfigs.Length - 1);
            var currentStep = currentIndex + 1;
            var config = stepConfigs[currentIndex];

            disclosureController?.ApplyStep(config);
            var highlightColor = config.focusHighlightColor == default ? UI.UIDesignTokens.Colors.AccentPrimary : config.focusHighlightColor;
            focusHighlighter?.ApplyFocus(config.focusTarget, highlightColor);
            gateController?.LoadStep(config, currentStep);
            stepTutorPanel?.ApplyStep(config, currentStep, stepConfigs.Length, gateController == null || gateController.IsGateSatisfied, gateController?.GetProgressText() ?? string.Empty);

            if (stepNavigator != null)
            {
                stepNavigator.UpdateStep(currentStep, stepConfigs.Length);
                stepNavigator.SetPreviousInteractable(currentStep > 1);
                stepNavigator.SetSkipVisible(currentStep < stepConfigs.Length);
                stepNavigator.SetNextInteractable(gateController == null || gateController.IsGateSatisfied || currentStep >= stepConfigs.Length);
            }

            return currentIndex;
        }
    }
}
