// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// RobotControlV2의 상단 상태/제어 바를 구성합니다.
    /// </summary>
    public sealed class TopStatusBar : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Graphic backgroundGraphic;
        [SerializeField] private Text titleText;
        [SerializeField] private Text modeText;
        [SerializeField] private Text connectionStateText;
        [SerializeField] private Text toolUserText;
        [SerializeField] private Text faultText;
        [SerializeField] private Text speedText;
        [SerializeField] private Button servoEnableButton;
        [SerializeField] private Button runButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button pauseResumeButton;
        [SerializeField] private Button syncButton;
        [SerializeField] private Button resetErrorButton;

        public Button ServoEnableButton => servoEnableButton;
        public Button RunButton => runButton;
        public Button StopButton => stopButton;
        public Button PauseResumeButton => pauseResumeButton;
        public Button SyncButton => syncButton;
        public Button ResetErrorButton => resetErrorButton;

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

        public void SetFallbackFont(Font font)
        {
            fallbackFont = font;
            EnsurePresentation();
        }

        public void SetTitleText(string value)
        {
            EnsurePresentation();
            if (titleText != null && !string.IsNullOrWhiteSpace(value))
            {
                titleText.text = value;
            }
        }

        public void SetModeText(string value)
        {
            EnsurePresentation();
            if (modeText != null)
            {
                modeText.text = string.IsNullOrWhiteSpace(value) ? "모의 연결" : value;
            }
        }

        public void SetConnectionStateText(string value)
        {
            EnsurePresentation();
            if (connectionStateText != null)
            {
                connectionStateText.text = string.IsNullOrWhiteSpace(value) ? "연결 안 됨" : value;
            }
        }

        public void SetSpeedText(string value)
        {
            EnsurePresentation();
            if (speedText != null)
            {
                speedText.text = string.IsNullOrWhiteSpace(value) ? "속도 30%" : value;
            }
        }

        public void ApplyState(RobotControlViewState state)
        {
            EnsurePresentation();

            SetModeText(FormatModeText(state.ControllerMode));
            SetConnectionStateText(state.IsConnected ? "연결됨" : "연결 안 됨");
            SetSpeedText(state.SpeedPreset);

            if (toolUserText != null)
            {
                toolUserText.text = $"Tool {state.ToolId:00} / User {state.UserId:00}";
            }

            if (faultText != null)
            {
                faultText.text = state.PreviewRiskSummary.HasBlockingRisk
                    ? $"주의 · {state.FaultSummary}"
                    : $"안전 · {state.FaultSummary}";
            }

            StyleButton(servoEnableButton, state.IsEnabled ? UIDesignTokens.RobotControlV2.Colors.Success : UIDesignTokens.RobotControlV2.Colors.TopBarSecondary);
            StyleButton(runButton, state.IsMockMode ? UIDesignTokens.RobotControlV2.Colors.Accent : UIDesignTokens.RobotControlV2.Colors.Warning);
            StyleButton(stopButton, UIDesignTokens.RobotControlV2.Colors.Danger);
            StyleButton(pauseResumeButton, state.IsDragMode ? UIDesignTokens.RobotControlV2.Colors.Warning : UIDesignTokens.RobotControlV2.Colors.TopBarSecondary);
            StyleButton(syncButton, state.IsConnected ? UIDesignTokens.RobotControlV2.Colors.Accent : UIDesignTokens.RobotControlV2.Colors.CardAlt);
            StyleButton(resetErrorButton, state.PreviewRiskSummary.HasBlockingRisk ? UIDesignTokens.RobotControlV2.Colors.Danger : UIDesignTokens.RobotControlV2.Colors.TopBarSecondary);
            UiRuntimeStyle.ForceTextHierarchySize(transform, UIDesignTokens.RobotControlV2.Type.UniformText);
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);

            if (transform is not RectTransform root)
            {
                return;
            }

            var barWidth = Mathf.Max(root.rect.width, 720f);
            var compact = barWidth < 1200f;
            NormalizeLegacyRootChildren(root);
            backgroundGraphic = UiRuntimeStyle.EnsureImage(root, "Background", UIDesignTokens.RobotControlV2.Colors.TopBarBackground);
            UiRuntimeStyle.Stretch((RectTransform)backgroundGraphic.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var rightClusterWidth = compact ? 460f : 600f;
            var leftClusterWidth = Mathf.Max(220f, barWidth - rightClusterWidth - (compact ? 48f : 72f));
            var leftCluster = EnsureSection(root, "LeftCluster", leftClusterWidth, compact ? 16f : 24f, false);
            var rightCluster = EnsureSection(root, "RightCluster", rightClusterWidth, compact ? 16f : 24f, true);
            rightCluster.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleRight;
            rightCluster.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;
            rightCluster.GetComponent<HorizontalLayoutGroup>().childControlWidth = false;

            titleText = EnsurePrimaryText(leftCluster, "Title", "로봇 제어 V2", compact ? 132f : 210f);
            modeText = EnsurePillText(leftCluster, "ModeText", "모의 연결", UIDesignTokens.RobotControlV2.Colors.Accent, compact ? 92f : 108f);
            connectionStateText = EnsurePillText(leftCluster, "ConnectionStateText", "연결 안 됨", UIDesignTokens.RobotControlV2.Colors.CardAlt, compact ? 88f : 112f);
            toolUserText = EnsureSecondaryText(leftCluster, "ToolUserText", "Tool 00 / User 00", compact ? 92f : 132f);
            faultText = EnsurePillText(leftCluster, "FaultText", "안전 · 문제 없음", UIDesignTokens.RobotControlV2.Colors.CardAlt, compact ? 120f : 152f);

            servoEnableButton = EnsureButton(rightCluster, "BtnServoEnable", "서보", compact ? 56f : 72f, compact);
            runButton = EnsureButton(rightCluster, "BtnRun", "시작", compact ? 56f : 72f, compact);
            stopButton = EnsureButton(rightCluster, "BtnStop", "정지", compact ? 56f : 72f, compact);
            pauseResumeButton = EnsureButton(rightCluster, "BtnPauseResume", "일시정지", compact ? 76f : 96f, compact);
            syncButton = EnsureButton(rightCluster, "BtnSync", "Sync", compact ? 56f : 72f, compact);
            resetErrorButton = EnsureButton(rightCluster, "BtnResetError", "오류 초기화", compact ? 90f : 112f, compact);
            speedText = EnsureSecondaryText(rightCluster, "SpeedText", "속도 30%", compact ? 64f : 88f, TextAnchor.MiddleRight);

            titleText.color = UIDesignTokens.RobotControlV2.Colors.TitleText;
            titleText.fontSize = UIDesignTokens.RobotControlV2.Type.UniformText;
            modeText.color = UIDesignTokens.RobotControlV2.Colors.Accent;
            connectionStateText.color = UIDesignTokens.RobotControlV2.Colors.MutedText;
            toolUserText.color = UIDesignTokens.RobotControlV2.Colors.MutedText;
            faultText.color = UIDesignTokens.RobotControlV2.Colors.Warning;
            speedText.color = UIDesignTokens.RobotControlV2.Colors.MutedText;

            StyleButton(servoEnableButton, UIDesignTokens.RobotControlV2.Colors.TopBarSecondary);
            StyleButton(runButton, UIDesignTokens.RobotControlV2.Colors.Accent);
            StyleButton(stopButton, UIDesignTokens.RobotControlV2.Colors.Danger);
            StyleButton(pauseResumeButton, UIDesignTokens.RobotControlV2.Colors.TopBarSecondary);
            StyleButton(syncButton, UIDesignTokens.RobotControlV2.Colors.Accent);
            StyleButton(resetErrorButton, UIDesignTokens.RobotControlV2.Colors.Danger);
            UiRuntimeStyle.ForceTextHierarchySize(root, UIDesignTokens.RobotControlV2.Type.UniformText);
        }

        private void NormalizeLegacyRootChildren(RectTransform root)
        {
            RemoveDirectChild(root, "Title");
            RemoveDirectChild(root, "ModeText");
            RemoveDirectChild(root, "ConnectionStateText");
            RemoveDirectChild(root, "ToolUserText");
            RemoveDirectChild(root, "FaultText");
            RemoveDirectChild(root, "BtnServoEnable");
            RemoveDirectChild(root, "BtnRun");
            RemoveDirectChild(root, "BtnStop");
            RemoveDirectChild(root, "BtnPauseResume");
            RemoveDirectChild(root, "BtnSync");
            RemoveDirectChild(root, "BtnResetError");
            RemoveDirectChild(root, "SpeedText");
        }

        private RectTransform EnsureSection(RectTransform root, string name, float width, float outerPadding, bool alignRight)
        {
            var section = root.Find(name) as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, name);
            if (alignRight)
            {
                section.anchorMin = new Vector2(1f, 0f);
                section.anchorMax = new Vector2(1f, 1f);
                section.offsetMin = new Vector2(-(width + outerPadding), 12f);
                section.offsetMax = new Vector2(-outerPadding, -12f);
            }
            else
            {
                section.anchorMin = new Vector2(0f, 0f);
                section.anchorMax = new Vector2(0f, 1f);
                section.offsetMin = new Vector2(outerPadding, 12f);
                section.offsetMax = new Vector2(outerPadding + width, -12f);
            }

            var layout = UiRuntimeStyle.EnsureHorizontalLayout(section.gameObject, UIDesignTokens.Space.Xs);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            return section;
        }

        private Text EnsurePrimaryText(Transform parent, string name, string defaultText, float preferredWidth)
        {
            var text = UiRuntimeStyle.EnsureText(parent, name, fallbackFont, UIDesignTokens.Type.HeadingLg, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            text.text = defaultText;
            var element = UiRuntimeStyle.EnsureLayoutElement(text);
            element.preferredWidth = preferredWidth;
            element.minWidth = preferredWidth;
            return text;
        }

        private Text EnsureSecondaryText(Transform parent, string name, string defaultText, float preferredWidth, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            var text = UiRuntimeStyle.EnsureText(parent, name, fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, anchor, UIDesignTokens.RobotControlV2.Colors.MutedText);
            text.text = defaultText;
            var element = UiRuntimeStyle.EnsureLayoutElement(text);
            element.preferredWidth = preferredWidth;
            element.minWidth = preferredWidth;
            return text;
        }

        private Text EnsurePillText(Transform parent, string name, string defaultText, Color backgroundColor, float preferredWidth)
        {
            var root = parent.Find(name) as RectTransform ?? UiRuntimeStyle.EnsureRectChild(parent, name);
            var image = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            image.color = backgroundColor;
            var element = UiRuntimeStyle.EnsureLayoutElement(root);
            element.preferredWidth = preferredWidth;
            element.minWidth = preferredWidth;
            element.preferredHeight = UIDesignTokens.Size.ButtonHeightSm;

            var text = UiRuntimeStyle.EnsureText(root, "Label", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f));
            text.text = defaultText;

            return text;
        }

        private Button EnsureButton(Transform parent, string name, string label, float width, bool compact)
        {
            var button = parent.Find(name)?.GetComponent<Button>();
            if (button == null)
            {
                button = UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, width);
            }

            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.preferredWidth = width;
            element.minWidth = width;
            element.preferredHeight = compact ? 24f : UIDesignTokens.Size.ButtonHeightSm;
            var labelText = button.transform.Find("Label")?.GetComponent<Text>();
            if (labelText != null)
            {
                labelText.fontSize = UIDesignTokens.RobotControlV2.Type.UniformText;
                labelText.resizeTextForBestFit = compact;
                labelText.resizeTextMinSize = UIDesignTokens.RobotControlV2.Type.UniformText;
                labelText.resizeTextMaxSize = UIDesignTokens.RobotControlV2.Type.UniformText;
            }
            return button;
        }

        private static void StyleButton(Button button, Color color)
        {
            if (button == null)
            {
                return;
            }

            var label = button.transform.Find("Label")?.GetComponent<Text>();
            if (label != null)
            {
                label.color = UIDesignTokens.RobotControlV2.Colors.TitleText;
                label.fontSize = UIDesignTokens.RobotControlV2.Type.UniformText;
            }

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }

            var colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.08f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.32f);
            button.colors = colors;
        }

        private static string FormatModeText(ControllerModeViewState mode)
        {
            return mode switch
            {
                ControllerModeViewState.Mock => "모의 연결",
                ControllerModeViewState.Manual => "수동",
                ControllerModeViewState.Auto => "자동",
                _ => "알 수 없음",
            };
        }

        private static void RemoveDirectChild(RectTransform parent, string childName)
        {
            RectTransform child = null;
            for (var i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) is RectTransform rect && rect.name == childName)
                {
                    child = rect;
                    break;
                }
            }

            if (child == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(child.gameObject);
            }
            else
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}
