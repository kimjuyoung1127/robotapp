// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 우측 레일의 복구 안내 패널을 구성합니다.
    /// </summary>
    public sealed class RecoveryGuidePanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text actionText;

        private void Awake()
        {
            EnsurePresentation();
        }

        private void OnEnable()
        {
            EnsurePresentation();
        }

        public void Bind(RobotControlViewState state)
        {
            ApplyState(state);
        }

        public void ApplyState(RobotControlViewState state)
        {
            EnsurePresentation();
            if (bodyText != null)
            {
                bodyText.text = state.LastRecoveryHint.Body;
            }

            if (actionText != null)
            {
                actionText.text = state.LastRecoveryHint.ActionLabel;
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            if (transform is not RectTransform root)
            {
                return;
            }

            var bg = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            bg.color = UIDesignTokens.RobotControlV2.Colors.Card;

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Warning);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(240f, 24f), new Vector2(16f, -16f));
            title.text = "복구 안내";

            bodyText = UiRuntimeStyle.EnsureText(root, "Body", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Anchor(bodyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 72f), new Vector2(16f, -48f));
            bodyText.text = "Bind shell state, choose a work tab, then preview before live motion.";

            actionText = UiRuntimeStyle.EnsureText(root, "Action", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(actionText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 24f), new Vector2(16f, -126f));
            actionText.text = "Preview first";
            UiRuntimeStyle.ForceTextHierarchySize(root, UIDesignTokens.RobotControlV2.Type.UniformText);
        }
    }
}
