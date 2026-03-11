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
    public class WhyItMovedPanel : MonoBehaviour
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
        private bool panelVisible;

        private readonly WhyItMovedState state = new WhyItMovedState();

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureLayout();
        }

        private void OnEnable()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureLayout();
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
                    ? UiRuntimeStyle.AccentYellow
                    : UiRuntimeStyle.AccentBlue;
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

        private void SetNeutralState()
        {
            if (changedJointText != null) changedJointText.text = "관절을 움직여 보세요.";
            if (angleTransitionText != null) angleTransitionText.text = string.Empty;
            if (deltaText != null)
            {
                deltaText.text = string.Empty;
                deltaText.color = UiRuntimeStyle.TextMuted;
            }
            if (affectedLinksText != null) affectedLinksText.text = string.Empty;
            if (eeChangeText != null) eeChangeText.text = string.Empty;
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
                panelBackground = UiRuntimeStyle.EnsureImage(panelRoot, "WhyItMovedBackground", UiRuntimeStyle.CardBackground);
            }
            else
            {
                UiRuntimeStyle.ReparentTo(panelBackground, panelRoot);
            }
            UiRuntimeStyle.Stretch((RectTransform)panelBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Divider line at top
            if (dividerImage == null)
            {
                dividerImage = UiRuntimeStyle.EnsureImage(panelRoot, "WhyItMovedDivider", UiRuntimeStyle.BorderSoft);
            }
            UiRuntimeStyle.Stretch((RectTransform)dividerImage.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, -1f), new Vector2(-8f, 0f));

            float yOffset = -10f;

            changedJointText = EnsureField("WIM_JointLabel", 12, FontStyle.Bold, UiRuntimeStyle.TextSecondary, yOffset, 20f);
            yOffset -= 20f;

            angleTransitionText = EnsureField("WIM_AngleTransition", 14, FontStyle.Normal, UiRuntimeStyle.TextPrimary, yOffset, 20f);
            yOffset -= 22f;

            deltaText = EnsureField("WIM_Delta", 13, FontStyle.Normal, UiRuntimeStyle.AccentYellow, yOffset, 20f);
            yOffset -= 20f;

            affectedLinksText = EnsureField("WIM_AffectedLinks", 13, FontStyle.Normal, UiRuntimeStyle.TextPrimary, yOffset, 18f);
            yOffset -= 20f;

            eeChangeText = EnsureField("WIM_EEChange", 13, FontStyle.Normal, UiRuntimeStyle.TextPrimary, yOffset, 18f);
            yOffset -= 24f;

            plainLanguageText = EnsureField("WIM_PlainLanguage", 14, FontStyle.Italic, UiRuntimeStyle.TextPrimary, yOffset, 28f);

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
