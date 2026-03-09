// Folder: App - application orchestration and runtime state.
using KineTutor3D.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KineTutor3D.App
{
    internal sealed class AppUiBinder
    {
        public void AutoWire(ref ProgressiveDisclosureController disclosureController, ref InteractionGateController gateController, ref StepTutorPanel stepTutorPanel, ref StepNavigator stepNavigator, ref ToastNotificationController toastController, ref FocusZoneHighlighter focusHighlighter, ref Slider jointSlider1, ref Slider jointSlider2, ref DHTableEditor dhTableEditor, ref TemplateSelector templateSelector, ref MatrixDisplay matrixDisplay)
        {
            disclosureController ??= Object.FindFirstObjectByType<ProgressiveDisclosureController>(FindObjectsInactive.Include);
            gateController ??= Object.FindFirstObjectByType<InteractionGateController>(FindObjectsInactive.Include);
            stepTutorPanel ??= Object.FindFirstObjectByType<StepTutorPanel>(FindObjectsInactive.Include);
            stepNavigator ??= Object.FindFirstObjectByType<StepNavigator>(FindObjectsInactive.Include);
            toastController ??= Object.FindFirstObjectByType<ToastNotificationController>(FindObjectsInactive.Include);
            focusHighlighter ??= Object.FindFirstObjectByType<FocusZoneHighlighter>(FindObjectsInactive.Include);
            dhTableEditor ??= Object.FindFirstObjectByType<DHTableEditor>(FindObjectsInactive.Include);
            templateSelector ??= Object.FindFirstObjectByType<TemplateSelector>(FindObjectsInactive.Include);
            matrixDisplay ??= Object.FindFirstObjectByType<MatrixDisplay>(FindObjectsInactive.Include);

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
        }

        public void BindRuntimeControllers(AppController owner, TemplateSelector templateSelector, DHTableEditor dhTableEditor, MatrixDisplay matrixDisplay)
        {
            templateSelector?.Bind(owner);
            dhTableEditor?.Bind(owner);
            matrixDisplay?.Bind(owner);
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
