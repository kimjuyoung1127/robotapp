// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 우측 레일의 도움말 패널을 구성합니다.
    /// </summary>
    public sealed class HelpPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Text bodyText;

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
                bodyText.text = state.PreviewRiskSummary.HasBlockingRisk
                    ? "Blocking risk is active. Review preview and recovery guidance before live motion."
                    : $"Mock shell active. Current TCP: {state.CurrentTcpPose}";
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
            bg.color = UIDesignTokens.RobotControlV2.Colors.CardAlt;

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, 14, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 24f), new Vector2(16f, -16f));
            title.text = "도움말";

            bodyText = UiRuntimeStyle.EnsureText(root, "Body", fallbackFont, 12, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Anchor(bodyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 84f), new Vector2(16f, -48f));
            bodyText.text = "Mock shell active. Current TCP: X -497 / Y -130 / Z 477 / RX 180 / RY 0 / RZ 90";
        }
    }
}
