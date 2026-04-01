// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 우측 레일의 상태 요약 카드를 구성합니다.
    /// </summary>
    public sealed class StatusSummaryPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Text connectionValue;
        [SerializeField] private Text modeValue;
        [SerializeField] private Text faultValue;
        [SerializeField] private Text safetyValue;
        [SerializeField] private Text toolUserValue;
        [SerializeField] private Text speedValue;

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
            if (connectionValue != null) connectionValue.text = state.IsConnected ? "Connected" : "Not connected";
            if (modeValue != null) modeValue.text = state.ControllerMode.ToString();
            if (faultValue != null) faultValue.text = state.FaultSummary;
            if (safetyValue != null) safetyValue.text = state.SafetySummary;
            if (toolUserValue != null) toolUserValue.text = $"Tool {state.ToolId:00} / User {state.UserId:00}";
            if (speedValue != null) speedValue.text = $"{state.SpeedPreset} · {state.SpeedPolicySummary}";
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
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(240f, 24f), new Vector2(16f, -16f));
            title.text = "상태 요약";

            connectionValue = EnsureRow(root, "Connection", 0, "Not connected");
            modeValue = EnsureRow(root, "Mode", 1, "Mock");
            faultValue = EnsureRow(root, "Fault", 2, "None");
            safetyValue = EnsureRow(root, "Safety", 3, "Preview gate enabled");
            toolUserValue = EnsureRow(root, "ToolUser", 4, "Tool 00 / User 00");
            speedValue = EnsureRow(root, "Speed", 5, "30% · Slow teaching-safe preset");
        }

        private Text EnsureRow(RectTransform root, string label, int index, string value)
        {
            var y = -52f - (index * 26f);
            var labelText = UiRuntimeStyle.EnsureText(root, $"{label}Label", fallbackFont, 12, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(labelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(88f, 20f), new Vector2(16f, y));
            labelText.text = label;

            var valueText = UiRuntimeStyle.EnsureText(root, $"{label}Value", fallbackFont, 12, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Anchor(valueText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(208f, 20f), new Vector2(108f, y));
            valueText.text = value;
            return valueText;
        }
    }
}
