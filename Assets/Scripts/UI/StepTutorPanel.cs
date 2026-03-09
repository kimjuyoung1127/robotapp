using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 스텝 목표/힌트/게이트 진행 텍스트를 표시합니다.
    /// </summary>
    public class StepTutorPanel : MonoBehaviour
    {
        [SerializeField] private Text stepTitleText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text gateProgressText;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Graphic tutorPanelBackground;

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureLayout();
        }

        public void ApplyStep(TutorStepConfig config, int currentStep, int totalSteps, bool gateSatisfied, string gateProgress)
        {
            if (config == null)
            {
                return;
            }

            if (stepTitleText != null)
            {
                stepTitleText.text = $"Step {currentStep}/{totalSteps}: {config.stepTitleKo}";
            }

            if (objectiveText != null)
            {
                objectiveText.text = config.objectiveKo;
            }

            if (hintText != null)
            {
                hintText.text = config.hintKo;
            }

            UpdateGateState(gateSatisfied, gateProgress);
        }

        public void UpdateGateState(bool gateSatisfied, string gateProgress)
        {
            if (gateProgressText == null)
            {
                return;
            }

            gateProgressText.text = gateSatisfied ? "Ready for the next step." : gateProgress;
            gateProgressText.color = gateSatisfied ? UiRuntimeStyle.AccentYellow : UiRuntimeStyle.TextSecondary;
        }

        private void EnsureLayout()
        {
            var rect = transform as RectTransform;
            if (rect != null)
            {
                UiRuntimeStyle.Stretch(rect, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-404f, 146f), new Vector2(-16f, -92f));
            }

            if (tutorPanelBackground == null)
            {
                tutorPanelBackground = UiRuntimeStyle.EnsureImage(transform, "RightPanelBackground", UiRuntimeStyle.PanelBackground);
            }

            UiRuntimeStyle.Stretch((RectTransform)tutorPanelBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            stepTitleText ??= ResolveOrCreate("StepTitleText", 20, FontStyle.Bold, TextAnchor.UpperLeft, UiRuntimeStyle.TextPrimary, new Vector2(20f, -18f), new Vector2(320f, 28f));
            objectiveText ??= ResolveOrCreate("StepObjectiveText", 14, FontStyle.Normal, TextAnchor.UpperLeft, UiRuntimeStyle.TextSecondary, new Vector2(20f, -58f), new Vector2(332f, 60f));
            hintText ??= ResolveOrCreate("StepHintText", 13, FontStyle.Italic, TextAnchor.UpperLeft, UiRuntimeStyle.TextMuted, new Vector2(20f, -126f), new Vector2(332f, 50f));
            gateProgressText ??= ResolveOrCreate("GateProgressText", 13, FontStyle.Bold, TextAnchor.UpperLeft, UiRuntimeStyle.AccentYellow, new Vector2(20f, -182f), new Vector2(332f, 24f));
        }

        private Text ResolveOrCreate(string objectName, int fontSize, FontStyle fontStyle, TextAnchor anchor, Color color, Vector2 anchoredPosition, Vector2 size)
        {
            var text = GameObject.Find(objectName)?.GetComponent<Text>();
            if (text == null)
            {
                text = UiRuntimeStyle.EnsureText(transform, objectName, fallbackFont, fontSize, fontStyle, anchor, color);
            }

            text.font = fallbackFont;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = color;
            text.alignment = anchor;
            UiRuntimeStyle.Anchor(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), size, anchoredPosition);
            return text;
        }
    }
}
