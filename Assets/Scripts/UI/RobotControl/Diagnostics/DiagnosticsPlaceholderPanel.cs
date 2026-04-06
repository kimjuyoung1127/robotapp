// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// DebugOnly 루트 아래의 진단 플레이스홀더를 구성합니다.
    /// </summary>
    public sealed class DiagnosticsPlaceholderPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;

        private void Awake()
        {
            EnsurePresentation();
        }

        private void OnEnable()
        {
            EnsurePresentation();
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
            bg.color = new Color(0f, 0f, 0f, 0.18f);

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Warning);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180f, 20f), new Vector2(16f, -16f));
            title.text = "디버그 전용";

            var body = UiRuntimeStyle.EnsureText(root, "Body", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(body.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(260f, 36f), new Vector2(16f, -40f));
            body.text = "진단 서랍과 레이아웃 경계 오버레이가 이 영역에 표시됩니다.";
            UiRuntimeStyle.ForceTextHierarchySize(root, UIDesignTokens.RobotControlV2.Type.UniformText);
        }
    }
}
