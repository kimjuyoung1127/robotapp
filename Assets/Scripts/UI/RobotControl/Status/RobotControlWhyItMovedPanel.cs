// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// RobotControlV2 전용 Why It Moved 자리의 플레이스홀더 텍스트를 구성합니다.
    /// </summary>
    public sealed class RobotControlWhyItMovedPanel : MonoBehaviour, IVisibilityControllable
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
                bodyText.text = $"움직임 해설 자리 표시자\n최근 명령: {state.LastCommandSummary}";
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
            title.text = "움직임 해설";

            bodyText = UiRuntimeStyle.EnsureText(root, "Body", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Anchor(bodyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 64f), new Vector2(16f, -46f));
            bodyText.text = "움직임 해설 자리 표시자";
            UiRuntimeStyle.ForceTextHierarchySize(root, UIDesignTokens.RobotControlV2.Type.UniformText);
        }
    }
}
