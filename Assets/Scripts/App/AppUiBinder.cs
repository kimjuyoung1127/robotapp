// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI;
using KineTutor3D.Visualization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KineTutor3D.App
{
    internal sealed class AppUiBinder
    {
        public void AutoWire(ref ProgressiveDisclosureController disclosureController, ref InteractionGateController gateController, ref StepTutorPanel stepTutorPanel, ref StepNavigator stepNavigator, ref ToastNotificationController toastController, ref FocusZoneHighlighter focusHighlighter, ref Slider jointSlider1, ref Slider jointSlider2, ref DHTableEditor dhTableEditor, ref TemplateSelector templateSelector, ref MatrixDisplay matrixDisplay, ref JointInputRail jointInputRail, ref WhyItMovedPanel whyItMovedPanel, ref BeginnerLeftPanel beginnerLeftPanel, ref MathReadinessPanel mathReadinessPanel, ref TargetFeedbackPanel targetFeedbackPanel, ref RobotRenderer robotRenderer, ref EndEffectorTrail endEffectorTrail, ref TargetMarkerVisual targetMarkerVisual, ref MathVisualOrchestrator mathVisualOrchestrator, ref FKDiagramPanel fkDiagramPanel)
        {
            if (SceneCatalog.GetCurrentSceneId() == SceneId.MathReadiness)
            {
                AutoWireDedicatedMathReadiness(
                    ref disclosureController,
                    ref gateController,
                    ref toastController,
                    ref focusHighlighter,
                    ref jointSlider1,
                    ref jointSlider2,
                    ref mathReadinessPanel,
                    ref robotRenderer,
                    ref endEffectorTrail,
                    ref targetMarkerVisual,
                    ref mathVisualOrchestrator,
                    ref fkDiagramPanel);
                return;
            }

            disclosureController ??= Object.FindFirstObjectByType<ProgressiveDisclosureController>(FindObjectsInactive.Include);
            gateController ??= Object.FindFirstObjectByType<InteractionGateController>(FindObjectsInactive.Include);
            stepTutorPanel ??= Object.FindFirstObjectByType<StepTutorPanel>(FindObjectsInactive.Include);
            stepNavigator ??= Object.FindFirstObjectByType<StepNavigator>(FindObjectsInactive.Include);
            toastController ??= Object.FindFirstObjectByType<ToastNotificationController>(FindObjectsInactive.Include);
            focusHighlighter ??= Object.FindFirstObjectByType<FocusZoneHighlighter>(FindObjectsInactive.Include);
            dhTableEditor ??= Object.FindFirstObjectByType<DHTableEditor>(FindObjectsInactive.Include);
            templateSelector ??= Object.FindFirstObjectByType<TemplateSelector>(FindObjectsInactive.Include);
            matrixDisplay ??= Object.FindFirstObjectByType<MatrixDisplay>(FindObjectsInactive.Include);
            jointInputRail ??= Object.FindFirstObjectByType<JointInputRail>(FindObjectsInactive.Include);
            whyItMovedPanel ??= Object.FindFirstObjectByType<WhyItMovedPanel>(FindObjectsInactive.Include);
            beginnerLeftPanel ??= Object.FindFirstObjectByType<BeginnerLeftPanel>(FindObjectsInactive.Include);
            mathReadinessPanel ??= Object.FindFirstObjectByType<MathReadinessPanel>(FindObjectsInactive.Include);
            targetFeedbackPanel ??= Object.FindFirstObjectByType<TargetFeedbackPanel>(FindObjectsInactive.Include);
            robotRenderer ??= Object.FindFirstObjectByType<RobotRenderer>(FindObjectsInactive.Include);
            endEffectorTrail ??= Object.FindFirstObjectByType<EndEffectorTrail>(FindObjectsInactive.Include);
            targetMarkerVisual ??= Object.FindFirstObjectByType<TargetMarkerVisual>(FindObjectsInactive.Include);
            mathVisualOrchestrator ??= Object.FindFirstObjectByType<MathVisualOrchestrator>(FindObjectsInactive.Include);
            fkDiagramPanel ??= Object.FindFirstObjectByType<FKDiagramPanel>(FindObjectsInactive.Include);

            if (jointSlider1 == null)
            {
                var go = GameObject.Find("joint_slider_1");
                if (go != null)
                {
                    jointSlider1 = go.GetComponent<Slider>();
                }
            }

            if (jointSlider2 == null)
            {
                var go = GameObject.Find("joint_slider_2");
                if (go != null)
                {
                    jointSlider2 = go.GetComponent<Slider>();
                }
            }

            if (templateSelector == null)
            {
                var topBar = GameObject.Find("TopBar");
                if (topBar != null)
                {
                    templateSelector = topBar.GetComponent<TemplateSelector>() ?? topBar.AddComponent<TemplateSelector>();
                }
            }

            if (dhTableEditor == null)
            {
                var leftPanel = GameObject.Find("LeftPanel");
                if (leftPanel != null)
                {
                    dhTableEditor = leftPanel.GetComponent<DHTableEditor>() ?? leftPanel.AddComponent<DHTableEditor>();
                }
            }

            if (matrixDisplay == null)
            {
                var rightPanel = GameObject.Find("RightPanel");
                if (rightPanel != null)
                {
                    matrixDisplay = rightPanel.GetComponent<MatrixDisplay>() ?? rightPanel.AddComponent<MatrixDisplay>();
                }
            }

            if (jointInputRail == null)
            {
                var bottomBar = GameObject.Find("BottomBar");
                if (bottomBar != null)
                {
                    jointInputRail = bottomBar.GetComponent<JointInputRail>() ?? bottomBar.AddComponent<JointInputRail>();
                }
            }

            if (whyItMovedPanel == null)
            {
                var rightPanel = GameObject.Find("RightPanel");
                if (rightPanel != null)
                {
                    whyItMovedPanel = rightPanel.GetComponent<WhyItMovedPanel>() ?? rightPanel.AddComponent<WhyItMovedPanel>();
                }
            }

            if (beginnerLeftPanel == null)
            {
                var leftPanel = GameObject.Find("LeftPanel");
                if (leftPanel != null)
                {
                    beginnerLeftPanel = leftPanel.GetComponent<BeginnerLeftPanel>() ?? leftPanel.AddComponent<BeginnerLeftPanel>();
                }
            }

            if (mathReadinessPanel == null)
            {
                var mathReadinessRoot = GameObject.Find("MathReadinessRect");
                if (mathReadinessRoot != null)
                {
                    mathReadinessPanel = mathReadinessRoot.GetComponent<MathReadinessPanel>() ?? mathReadinessRoot.AddComponent<MathReadinessPanel>();
                }
                else
                {
                    var leftPanel = GameObject.Find("LeftPanel");
                    if (leftPanel != null)
                    {
                        mathReadinessPanel = leftPanel.GetComponent<MathReadinessPanel>() ?? leftPanel.AddComponent<MathReadinessPanel>();
                    }
                }
            }

            if (targetFeedbackPanel == null)
            {
                var rightPanel = GameObject.Find("RightPanel");
                if (rightPanel != null)
                {
                    targetFeedbackPanel = rightPanel.GetComponent<TargetFeedbackPanel>() ?? rightPanel.AddComponent<TargetFeedbackPanel>();
                }
            }

            if (endEffectorTrail == null)
            {
                var ee = GameObject.Find("Frame_EE");
                if (ee != null)
                {
                    endEffectorTrail = ee.GetComponent<EndEffectorTrail>() ?? ee.AddComponent<EndEffectorTrail>();
                }
            }

            if (targetMarkerVisual == null)
            {
                var robotRoot = GameObject.Find("RobotRoot");
                if (robotRoot != null)
                {
                    targetMarkerVisual = robotRoot.GetComponent<TargetMarkerVisual>() ?? robotRoot.AddComponent<TargetMarkerVisual>();
                }
            }

            if (mathVisualOrchestrator == null)
            {
                var robotRoot = GameObject.Find("RobotRoot");
                if (robotRoot != null)
                {
                    mathVisualOrchestrator = robotRoot.GetComponent<MathVisualOrchestrator>() ?? robotRoot.AddComponent<MathVisualOrchestrator>();
                }
            }
        }

        private static Slider FindFooterSlider(Transform jointCard, string primaryName, string legacyName = null)
        {
            if (jointCard == null)
            {
                return null;
            }

            var slider = jointCard.Find(primaryName)?.GetComponent<Slider>();
            if (slider != null || string.IsNullOrWhiteSpace(legacyName))
            {
                return slider;
            }

            return jointCard.Find(legacyName)?.GetComponent<Slider>();
        }

        private static void AutoWireDedicatedMathReadiness(
            ref ProgressiveDisclosureController disclosureController,
            ref InteractionGateController gateController,
            ref ToastNotificationController toastController,
            ref FocusZoneHighlighter focusHighlighter,
            ref Slider jointSlider1,
            ref Slider jointSlider2,
            ref MathReadinessPanel mathReadinessPanel,
            ref RobotRenderer robotRenderer,
            ref EndEffectorTrail endEffectorTrail,
            ref TargetMarkerVisual targetMarkerVisual,
            ref MathVisualOrchestrator mathVisualOrchestrator,
            ref FKDiagramPanel fkDiagramPanel)
        {
            disclosureController ??= Object.FindFirstObjectByType<ProgressiveDisclosureController>(FindObjectsInactive.Include);
            gateController ??= Object.FindFirstObjectByType<InteractionGateController>(FindObjectsInactive.Include);
            toastController ??= Object.FindFirstObjectByType<ToastNotificationController>(FindObjectsInactive.Include);
            focusHighlighter ??= Object.FindFirstObjectByType<FocusZoneHighlighter>(FindObjectsInactive.Include);
            robotRenderer ??= Object.FindFirstObjectByType<RobotRenderer>(FindObjectsInactive.Include);
            endEffectorTrail ??= Object.FindFirstObjectByType<EndEffectorTrail>(FindObjectsInactive.Include);
            targetMarkerVisual ??= Object.FindFirstObjectByType<TargetMarkerVisual>(FindObjectsInactive.Include);
            mathVisualOrchestrator ??= Object.FindFirstObjectByType<MathVisualOrchestrator>(FindObjectsInactive.Include);
            fkDiagramPanel ??= Object.FindFirstObjectByType<FKDiagramPanel>(FindObjectsInactive.Include);

            var pageRoot = GameObject.Find("MathReadinessRect");
            if (pageRoot == null)
            {
                pageRoot = GameObject.Find("MathReadinessPageRoot");
            }
            if (pageRoot != null)
            {
                mathReadinessPanel ??= pageRoot.GetComponent<MathReadinessPanel>() ?? pageRoot.AddComponent<MathReadinessPanel>();

                var footer = pageRoot.transform.Find("PageFooter");
                if (footer != null)
                {
                    var jointCard = footer.Find("JointControlCard");
                    if (jointCard != null)
                    {
                        jointSlider1 ??= FindFooterSlider(jointCard, "joint_slider_1");
                        jointSlider2 ??= FindFooterSlider(jointCard, "joint_slider_2", "legacy_joint_slider_2");
                    }
                }
            }
        }

        public void BindRuntimeControllers(AppController owner, TemplateSelector templateSelector, DHTableEditor dhTableEditor, MatrixDisplay matrixDisplay, JointInputRail jointInputRail, WhyItMovedPanel whyItMovedPanel, BeginnerLeftPanel beginnerLeftPanel, MathReadinessPanel mathReadinessPanel, TargetFeedbackPanel targetFeedbackPanel, EndEffectorTrail endEffectorTrail, TargetMarkerVisual targetMarkerVisual)
        {
            templateSelector?.Bind(owner);
            dhTableEditor?.Bind(owner);
            matrixDisplay?.Bind(owner);
            jointInputRail?.Bind(owner);
            whyItMovedPanel?.Bind(owner);
            beginnerLeftPanel?.Bind(owner);
            mathReadinessPanel?.Bind(owner);
            targetFeedbackPanel?.Bind(owner);
            endEffectorTrail?.Bind(owner);
            targetMarkerVisual?.Bind(owner);
        }

        public void BindSliderEvents(Slider jointSlider1, Slider jointSlider2, UnityAction<float> onSlider1Changed, UnityAction<float> onSlider2Changed, ref bool sliderListenersBound)
        {
            if (sliderListenersBound)
            {
                return;
            }

            if (jointSlider1 != null)
            {
                jointSlider1.onValueChanged.AddListener(onSlider1Changed);
            }

            if (jointSlider2 != null)
            {
                jointSlider2.onValueChanged.AddListener(onSlider2Changed);
            }

            sliderListenersBound = true;
        }

        public void UnbindSliderEvents(Slider jointSlider1, Slider jointSlider2, UnityAction<float> onSlider1Changed, UnityAction<float> onSlider2Changed, ref bool sliderListenersBound)
        {
            if (!sliderListenersBound)
            {
                return;
            }

            if (jointSlider1 != null)
            {
                jointSlider1.onValueChanged.RemoveListener(onSlider1Changed);
            }

            if (jointSlider2 != null)
            {
                jointSlider2.onValueChanged.RemoveListener(onSlider2Changed);
            }

            sliderListenersBound = false;
        }
    }
}
