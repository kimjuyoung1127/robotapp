// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 스텝 제목/힌트/게이트 진행 텍스트를 표시합니다.
    /// </summary>
    [ExecuteAlways]
    public class StepTutorPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Text stepTitleText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text gateProgressText;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Graphic tutorPanelBackground;
        private TutorStepConfig currentConfig;

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
        }

        private void OnEnable()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureLayout();
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(visible);
            }
        }

        public void ApplyStep(TutorStepConfig config, int currentStep, int totalSteps, bool gateSatisfied, string gateProgress)
        {
            if (config == null)
            {
                return;
            }

            currentConfig = config;

            if (stepTitleText != null)
            {
                var prefix = config.mathReadinessMode
                    ? "Math"
                    : config.beginnerMode
                        ? "Lesson"
                        : "Step";
                stepTitleText.text = $"{prefix} {currentStep}/{totalSteps}: {config.stepTitleKo}";
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

            if (gateSatisfied)
            {
                gateProgressText.text = currentConfig != null && currentConfig.mathReadinessMode
                    ? "좋아요. 다음 감각 확인으로 갈 수 있어요."
                    : "Ready for the next step.";
            }
            else if (currentConfig != null && currentConfig.mathReadinessMode && string.IsNullOrWhiteSpace(gateProgress))
            {
                gateProgressText.text = "선택지를 고르고 슬라이더를 움직여 보세요.";
            }
            else
            {
                gateProgressText.text = gateProgress;
            }

            gateProgressText.color = gateSatisfied ? UIDesignTokens.Colors.AccentSecondary : UIDesignTokens.Colors.TextSecondary;
        }

        private void EnsureLayout()
        {
            panelRoot ??= UiRuntimeStyle.EnsureHostedRoot(this, "RightPanelRect");
            UiRuntimeStyle.Stretch(panelRoot, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-404f, 146f), new Vector2(-16f, -92f));

            if (tutorPanelBackground == null)
            {
                tutorPanelBackground = UiRuntimeStyle.EnsureImage(panelRoot, "RightPanelBackground", UIDesignTokens.Colors.SurfaceRaised);
            }
            else
            {
                UiRuntimeStyle.ReparentTo(tutorPanelBackground, panelRoot);
            }

            UiRuntimeStyle.Stretch((RectTransform)tutorPanelBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            stepTitleText ??= ResolveOrCreate("StepTitleText", UIDesignTokens.Type.DisplaySm, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary, new Vector2(20f, -18f), new Vector2(320f, 28f));
            objectiveText ??= ResolveOrCreate("StepObjectiveText", UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary, new Vector2(20f, -58f), new Vector2(332f, 60f));
            hintText ??= ResolveOrCreate("StepHintText", UIDesignTokens.Type.Body, FontStyle.Italic, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted, new Vector2(20f, -126f), new Vector2(332f, 50f));
            gateProgressText ??= ResolveOrCreate("GateProgressText", UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.AccentSecondary, new Vector2(20f, -182f), new Vector2(332f, 24f));
        }

        private Text ResolveOrCreate(string objectName, int fontSize, FontStyle fontStyle, TextAnchor anchor, Color color, Vector2 anchoredPosition, Vector2 size)
        {
            var found = panelRoot != null ? panelRoot.Find(objectName) : null;
            var text = found != null ? found.GetComponent<Text>() : null;
            if (text == null)
            {
                text = UiRuntimeStyle.EnsureText(panelRoot, objectName, fallbackFont, fontSize, fontStyle, anchor, color);
            }
            else
            {
                UiRuntimeStyle.ReparentTo(text, panelRoot);
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

