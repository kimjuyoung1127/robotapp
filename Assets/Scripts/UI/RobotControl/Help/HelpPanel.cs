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
                    ? "차단 위험이 활성화되어 있습니다. 실제 이동 전에 미리보기와 복구 안내를 먼저 확인하세요."
                    : $"모의 연결 셸이 활성화되어 있습니다. 현재 TCP: {state.CurrentTcpPose}";
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

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 24f), new Vector2(16f, -16f));
            title.text = "도움말";

            bodyText = UiRuntimeStyle.EnsureText(root, "Body", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Anchor(bodyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 84f), new Vector2(16f, -48f));
            bodyText.text = "모의 연결 셸이 활성화되어 있습니다. 현재 TCP: X -497 / Y -130 / Z 477 / RX 180 / RY 0 / RZ 90";
            UiRuntimeStyle.ForceTextHierarchySize(root, UIDesignTokens.RobotControlV2.Type.UniformText);
        }
    }
}
