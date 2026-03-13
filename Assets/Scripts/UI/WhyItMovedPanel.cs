// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.Math;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.UI
{
    /// <summary>
    /// "Why It Moved" 설명 패널 — 관절 변화 시 무엇이 왜 움직였는지 보여줍니다.
    /// RightPanel 하단(StepTutorPanel 아래)에 배치됩니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class WhyItMovedPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private AppController appController;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Font fallbackFont;

        private Text changedJointText;
        private Text angleTransitionText;
        private Text deltaText;
        private Text affectedLinksText;
        private Text eeChangeText;
        private Text plainLanguageText;
        private Image panelBackground;
        private Image dividerImage;
        private Image directionArrowIcon;
        private bool panelVisible;

        private readonly WhyItMovedState state = new WhyItMovedState();

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
        }

        private void OnEnable()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureLayout();
            SetVisible(false);
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        /// <summary>
        /// AppController에 이벤트를 바인딩합니다.
        /// </summary>
        public void Bind(AppController owner)
        {
            Unbind();
            appController = owner;

            if (appController != null)
            {
                appController.OnKinematicsUpdated += HandleKinematicsUpdated;
                appController.OnStepChanged += HandleStepChanged;
            }

            EnsureLayout();
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            panelVisible = visible;
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(visible);
            }
        }

        private void HandleKinematicsUpdated(Mat4D _a1, Mat4D _a2, Mat4D _t02, TutorPose _pose)
        {
            if (!panelVisible || appController == null)
            {
                return;
            }

            var cause = appController.LastUpdateCause;
            var changedJoint = appController.ChangedJointIndex;
            var prevJoints = appController.PreviousJointValuesRad;
            var currJoints = appController.CurrentJointValuesRad;
            var prevEE = appController.PreviousEndEffectorTransform.ExtractPosition();
            var currEE = appController.CurrentEndEffectorTransform.ExtractPosition();
            var jointCount = currJoints?.Length ?? 0;

            state.Compute(cause, changedJoint, prevJoints, currJoints, prevEE, currEE, jointCount);
            Refresh();
        }

        private void HandleStepChanged(int _step, TutorStepConfig _config)
        {
            // Reset display on step change
            ClearDisplay();
        }

        private void Refresh()
        {
            if (appController != null && appController.CurrentStepConfig != null && appController.CurrentStepConfig.mathReadinessMode)
            {
                RefreshMathReadiness();
                return;
            }

            if (state.UpdateCause != RuntimeUpdateCause.JointAngleChange || !state.IsMeaningfulChange)
            {
                SetNeutralState();
                return;
            }

            if (changedJointText != null)
            {
                changedJointText.text = $"변경된 관절: J{state.ChangedJointIndex + 1}";
            }

            if (angleTransitionText != null)
            {
                angleTransitionText.text = WhyItMovedFormatter.FormatAngleTransition(state.PreviousValueRad, state.CurrentValueRad);
            }

            if (deltaText != null)
            {
                deltaText.text = $"변화량: {WhyItMovedFormatter.FormatDeltaText(state.DeltaDeg)}";
                deltaText.color = WhyItMovedFormatter.IsDeltaPositive(state.DeltaDeg)
                    ? UIDesignTokens.Colors.AccentSecondary
                    : UIDesignTokens.Colors.AccentPrimary;
            }

            if (affectedLinksText != null)
            {
                affectedLinksText.text = $"영향: {WhyItMovedFormatter.FormatAffectedLinks(state.AffectedLinkNames)}";
            }

            if (eeChangeText != null)
            {
                eeChangeText.text = $"끝점: {WhyItMovedFormatter.FormatEEChange(state.EEDisplacement)}";
            }

            if (plainLanguageText != null)
            {
                plainLanguageText.text = WhyItMovedFormatter.FormatPlainLanguage(state);
            }
        }

        private void RefreshMathReadiness()
        {
            if (state.UpdateCause != RuntimeUpdateCause.JointAngleChange || !state.IsMeaningfulChange)
            {
                SetNeutralState();
                return;
            }

            if (changedJointText != null)
            {
                changedJointText.text = $"움직인 관절: J{state.ChangedJointIndex + 1}";
            }

            if (angleTransitionText != null)
            {
                angleTransitionText.text = WhyItMovedFormatter.FormatAngleTransition(state.PreviousValueRad, state.CurrentValueRad);
            }

            if (deltaText != null)
            {
                deltaText.text = $"각도 변화: {WhyItMovedFormatter.FormatDeltaText(state.DeltaDeg)}";
                deltaText.color = WhyItMovedFormatter.IsDeltaPositive(state.DeltaDeg)
                    ? UIDesignTokens.Colors.AccentSecondary
                    : UIDesignTokens.Colors.AccentPrimary;
            }

            if (affectedLinksText != null)
            {
                affectedLinksText.text = string.Empty;
            }

            if (eeChangeText != null)
            {
                eeChangeText.text = $"끝점 이동: {MathReadinessFormatter.FormatDirection(state.EEDisplacement)}";
            }

            // C2: Direction arrow icon for math readiness mode
            if (directionArrowIcon != null)
            {
                var iconName = MathReadinessFormatter.GetDirectionIconName(state.EEDisplacement);
                UIIconResolver.SetIcon(directionArrowIcon, iconName, UIDesignTokens.Colors.AccentSecondary);
                directionArrowIcon.gameObject.SetActive(true);
            }

            if (plainLanguageText != null)
            {
                plainLanguageText.text = MathReadinessFormatter.FormatWhyItMoved(state);
            }
        }

        private void SetNeutralState()
        {
            if (changedJointText != null) changedJointText.text = "관절을 움직여 보세요.";
            if (angleTransitionText != null) angleTransitionText.text = string.Empty;
            if (deltaText != null)
            {
                deltaText.text = string.Empty;
                deltaText.color = UIDesignTokens.Colors.TextMuted;
            }
            if (affectedLinksText != null) affectedLinksText.text = string.Empty;
            if (eeChangeText != null) eeChangeText.text = string.Empty;
            if (directionArrowIcon != null) directionArrowIcon.gameObject.SetActive(false);
            if (plainLanguageText != null) plainLanguageText.text = string.Empty;
        }

        private void ClearDisplay()
        {
            SetNeutralState();
        }

        private void EnsureLayout()
        {
            panelRoot ??= UiRuntimeStyle.EnsureHostedRoot(this, "WhyItMovedRect");
            // Position below StepTutorPanel in RightPanel area
            UiRuntimeStyle.Stretch(panelRoot,
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-404f, 16f), new Vector2(-16f, 138f));

            if (panelBackground == null)
            {
                panelBackground = UiRuntimeStyle.EnsureImage(panelRoot, "WhyItMovedBackground", UIDesignTokens.Colors.SurfaceCard);
            }
            else
            {
                UiRuntimeStyle.ReparentTo(panelBackground, panelRoot);
            }
            UiRuntimeStyle.Stretch((RectTransform)panelBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Divider line at top
            if (dividerImage == null)
            {
                dividerImage = UiRuntimeStyle.EnsureImage(panelRoot, "WhyItMovedDivider", UIDesignTokens.Colors.BorderSoft);
            }
            UiRuntimeStyle.Stretch((RectTransform)dividerImage.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, -1f), new Vector2(-8f, 0f));

            float yOffset = -10f;

            changedJointText = EnsureField("WIM_JointLabel", 12, FontStyle.Bold, UIDesignTokens.Colors.TextSecondary, yOffset, 20f);
            yOffset -= 20f;

            angleTransitionText = EnsureField("WIM_AngleTransition", 14, FontStyle.Normal, UIDesignTokens.Colors.TextPrimary, yOffset, 20f);
            yOffset -= 22f;

            deltaText = EnsureField("WIM_Delta", 13, FontStyle.Normal, UIDesignTokens.Colors.AccentSecondary, yOffset, 20f);
            yOffset -= 20f;

            affectedLinksText = EnsureField("WIM_AffectedLinks", 13, FontStyle.Normal, UIDesignTokens.Colors.TextPrimary, yOffset, 18f);
            yOffset -= 20f;

            eeChangeText = EnsureField("WIM_EEChange", 13, FontStyle.Normal, UIDesignTokens.Colors.TextPrimary, yOffset, 18f);

            // C2: Direction arrow icon next to EE change
            directionArrowIcon = UIIconResolver.CreateIcon(panelRoot, "WIM_DirectionArrow", "icon-arrow-right", UIDesignTokens.Size.IconMd, UIDesignTokens.Colors.AccentSecondary);
            UiRuntimeStyle.Anchor(directionArrowIcon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(UIDesignTokens.Size.IconMd, UIDesignTokens.Size.IconMd), new Vector2(340f, yOffset));
            directionArrowIcon.gameObject.SetActive(false);

            yOffset -= 24f;

            plainLanguageText = EnsureField("WIM_PlainLanguage", 14, FontStyle.Italic, UIDesignTokens.Colors.TextPrimary, yOffset, 28f);

            SetNeutralState();
        }

        private Text EnsureField(string objectName, int fontSize, FontStyle fontStyle, Color color, float yPos, float height)
        {
            var text = UiRuntimeStyle.EnsureText(panelRoot, objectName, fallbackFont, fontSize, fontStyle, TextAnchor.UpperLeft, color);
            UiRuntimeStyle.Anchor(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(352f, height), new Vector2(16f, yPos));
            return text;
        }

        private void Unbind()
        {
            if (appController != null)
            {
                appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
                appController.OnStepChanged -= HandleStepChanged;
            }
        }
    }
}
