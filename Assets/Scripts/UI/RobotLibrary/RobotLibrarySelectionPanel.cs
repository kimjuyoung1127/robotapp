// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using System.Collections.Generic;
using KineTutor3D.App;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Robot Library에서 선택된 로봇의 정보 표시와 진입 CTA를 제공합니다.
    /// </summary>
    public class RobotLibrarySelectionPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;

        private const float CompactWidthThreshold = 560f;
        private const float CompactHeightThreshold = 520f;

        private RectTransform panelRoot;
        private RectTransform contentRoot;
        private RectTransform headerBlock;
        private RectTransform primaryActionsStack;

        private Text titleText;
        private Text subtitleText;
        private Text descriptionText;
        private Text capabilityText;
        private Text helperText;
        private Button guidedLessonButton;
        private Button sandboxButton;
        private Button robotControlButton;

        private RobotCatalogEntry currentEntry;

        public event Action OnGuidedLessonRequested;
        public event Action OnSandboxRequested;
        public event Action OnRobotControlRequested;

        public void Initialize(RectTransform parent, Font font)
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(font);
            EnsurePresentation(parent);
            SetVisible(true);
        }

        public void ShowRobot(RobotCatalogEntry entry)
        {
            currentEntry = entry;
            EnsurePresentation(transform.parent as RectTransform);

            if (entry == null)
            {
                titleText.text = "Robot Preview";
                subtitleText.text = "Select a robot";
                descriptionText.text = "카드나 3D showroom에서 로봇을 선택하면 여기서 관찰과 직접 조작을 이어갈 수 있습니다.";
                capabilityText.text = "지원 모드가 여기 표시됩니다.";
                helperText.text = "먼저 로봇을 고른 뒤 원하는 화면으로 이동하세요.";
                RefreshActionButtons(null);
                return;
            }

            var metadata = entry.Metadata;

            titleText.text = metadata.DisplayName;
            subtitleText.text = $"{metadata.Dof} DOF · {metadata.RobotType} · {metadata.Difficulty}";
            descriptionText.text = metadata.Description;
            capabilityText.text = BuildCapabilitySummary(metadata);
            helperText.text = "지원되는 화면으로 직접 이동할 수 있습니다.";

            if (entry.LibraryInteractionMode == LibraryInteractionMode.SelectOnly)
            {
                helperText.text = "이 항목은 RobotControl 템플릿 구조를 보여주는 선택 전용 카드입니다.";
            }

            RefreshActionButtons(metadata);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void EnsurePresentation(RectTransform parent)
        {
            if (parent == null)
            {
                return;
            }

            panelRoot = transform as RectTransform;
            if (panelRoot == null)
            {
                panelRoot = gameObject.AddComponent<RectTransform>();
            }

            if (panelRoot.parent != parent)
            {
                panelRoot.SetParent(parent, false);
            }

            var background = panelRoot.GetComponent<Image>() ?? panelRoot.gameObject.AddComponent<Image>();
            background.color = UIDesignTokens.Colors.SurfaceRaised;

            if (NeedsRebuild())
            {
                ClearPanelChildren();
                BuildPresentation();
            }

            ApplyResponsiveLayout();
        }

        private bool NeedsRebuild()
        {
            return panelRoot == null
                || panelRoot.Find("Content") == null
                || panelRoot.Find("Title") != null
                || panelRoot.Find("Subtitle") != null
                || panelRoot.Find("Description") != null
                || panelRoot.Find("ScrollView") != null
                || titleText == null
                || subtitleText == null
                || descriptionText == null
                || capabilityText == null
                || helperText == null
                || guidedLessonButton == null
                || sandboxButton == null
                || robotControlButton == null;
        }

        private void BuildPresentation()
        {
            contentRoot = UiRuntimeStyle.EnsureRectChild(panelRoot, "Content");
            UiRuntimeStyle.Stretch(contentRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var contentLayout = UiRuntimeStyle.EnsureVerticalLayout(contentRoot.gameObject, UIDesignTokens.Space.Sm);
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandHeight = false;

            headerBlock = UiRuntimeStyle.EnsureRectChild(contentRoot, "HeaderBlock");
            var headerLayout = UiRuntimeStyle.EnsureVerticalLayout(headerBlock.gameObject, UIDesignTokens.Space.Xs);
            headerLayout.childAlignment = TextAnchor.UpperLeft;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandHeight = false;

            titleText = CreateTextBlock(headerBlock, "Title", UIDesignTokens.Type.DisplaySm, FontStyle.Bold, UIDesignTokens.Colors.TextPrimary, 30f);
            subtitleText = CreateTextBlock(headerBlock, "Subtitle", UIDesignTokens.Type.HeadingSm, FontStyle.Bold, UIDesignTokens.Colors.AccentSecondary, 22f);
            descriptionText = CreateTextBlock(headerBlock, "Description", UIDesignTokens.Type.Body + 1, FontStyle.Normal, UIDesignTokens.Colors.TextSecondary, 52f);
            capabilityText = CreateTextBlock(headerBlock, "CapabilityText", UIDesignTokens.Type.Body, FontStyle.Normal, UIDesignTokens.Colors.TextPrimary, 36f);
            helperText = CreateTextBlock(headerBlock, "HelperText", UIDesignTokens.Type.Body, FontStyle.Italic, UIDesignTokens.Colors.TextMuted, 36f);

            var divider = UIComponentFactory.CreateDivider(contentRoot, "Divider");
            divider.raycastTarget = false;

            primaryActionsStack = UiRuntimeStyle.EnsureRectChild(contentRoot, "PrimaryActionsStack");
            var actionLayout = UiRuntimeStyle.EnsureVerticalLayout(primaryActionsStack.gameObject, UIDesignTokens.Space.Xs);
            actionLayout.childAlignment = TextAnchor.UpperLeft;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandHeight = false;

            guidedLessonButton = UIComponentFactory.CreatePrimaryButton(primaryActionsStack, "BtnGuidedLesson", "Start Lesson", fallbackFont, 0f);
            sandboxButton = UIComponentFactory.CreateSecondaryButton(primaryActionsStack, "BtnOpenSandbox", "Open Sandbox", fallbackFont, 0f);
            robotControlButton = UIComponentFactory.CreateSecondaryButton(primaryActionsStack, "BtnRobotControl", GetRobotControlLabel(), fallbackFont, 0f);
            guidedLessonButton.onClick.RemoveAllListeners();
            guidedLessonButton.onClick.AddListener(() => OnGuidedLessonRequested?.Invoke());
            sandboxButton.onClick.RemoveAllListeners();
            sandboxButton.onClick.AddListener(() => OnSandboxRequested?.Invoke());
            robotControlButton.onClick.RemoveAllListeners();
            robotControlButton.onClick.AddListener(() => OnRobotControlRequested?.Invoke());
        }

        private void ApplyResponsiveLayout()
        {
            if (panelRoot == null || contentRoot == null)
            {
                return;
            }

            var panelWidth = panelRoot.rect.width > 1f ? panelRoot.rect.width : 640f;
            var panelHeight = panelRoot.rect.height > 1f ? panelRoot.rect.height : 320f;
            var compact = panelWidth < CompactWidthThreshold || panelHeight < CompactHeightThreshold;
            var padding = Mathf.RoundToInt(compact ? UIDesignTokens.Space.Sm : UIDesignTokens.Space.Md);

            var contentLayout = contentRoot.GetComponent<VerticalLayoutGroup>();
            if (contentLayout != null)
            {
                contentLayout.padding = new RectOffset(padding, padding, padding, padding);
                contentLayout.spacing = compact ? UIDesignTokens.Space.Xs : UIDesignTokens.Space.Sm;
            }

            titleText.fontSize = compact ? UIDesignTokens.Type.HeadingLg : UIDesignTokens.Type.DisplaySm;
            subtitleText.fontSize = compact ? UIDesignTokens.Type.Body : UIDesignTokens.Type.HeadingSm;
            descriptionText.fontSize = compact ? UIDesignTokens.Type.Caption + 1 : UIDesignTokens.Type.Body + 1;
            capabilityText.fontSize = compact ? UIDesignTokens.Type.Caption + 1 : UIDesignTokens.Type.Body;
            helperText.fontSize = compact ? UIDesignTokens.Type.Caption + 1 : UIDesignTokens.Type.Body;

            ApplyTextHeight(titleText, compact ? 24f : 30f);
            ApplyTextHeight(subtitleText, compact ? 20f : 24f);
            ApplyTextHeight(descriptionText, compact ? 42f : 52f);
            ApplyTextHeight(capabilityText, compact ? 28f : 36f);
            ApplyTextHeight(helperText, compact ? 28f : 36f);

            ConfigureButton(guidedLessonButton, UILayoutProfile.TouchTarget, 0f, true);
            ConfigureButton(sandboxButton, UILayoutProfile.TouchTarget, 0f, true);
            ConfigureButton(robotControlButton, UILayoutProfile.TouchTarget, 0f, true);
        }

        private Text CreateTextBlock(Transform parent, string name, int fontSize, FontStyle style, Color color, float minHeight)
        {
            var text = UiRuntimeStyle.EnsureText(parent, name, fallbackFont, fontSize, style, TextAnchor.UpperLeft, color);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var element = UiRuntimeStyle.EnsureLayoutElement(text);
            element.minHeight = minHeight;
            element.preferredHeight = minHeight;
            element.flexibleWidth = 1f;
            return text;
        }

        private static void ApplyTextHeight(Text text, float minHeight)
        {
            if (text == null)
            {
                return;
            }

            var element = UiRuntimeStyle.EnsureLayoutElement(text);
            element.minHeight = minHeight;
            element.preferredHeight = minHeight;
        }

        private static void ConfigureButton(Button button, float height, float width, bool fullWidth)
        {
            if (button == null)
            {
                return;
            }

            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.minHeight = height;
            element.preferredHeight = height;
            element.flexibleWidth = fullWidth ? 1f : 0f;
            if (!fullWidth)
            {
                element.minWidth = width;
                element.preferredWidth = width;
            }
        }

        private void ClearPanelChildren()
        {
            if (panelRoot == null)
            {
                return;
            }

            var staleChildren = new List<GameObject>();
            for (var i = panelRoot.childCount - 1; i >= 0; i--)
            {
                staleChildren.Add(panelRoot.GetChild(i).gameObject);
            }

            for (var i = 0; i < staleChildren.Count; i++)
            {
                SafeDestroy(staleChildren[i]);
            }

            contentRoot = null;
            headerBlock = null;
            primaryActionsStack = null;
            titleText = null;
            subtitleText = null;
            descriptionText = null;
            capabilityText = null;
            helperText = null;
            guidedLessonButton = null;
            sandboxButton = null;
            robotControlButton = null;
        }

        private void RefreshActionButtons(RobotMetadataInfo? metadata)
        {
            if (!metadata.HasValue)
            {
                guidedLessonButton.interactable = false;
                sandboxButton.interactable = false;
                robotControlButton.interactable = false;
                return;
            }

            var data = metadata.Value;
            if (RobotCatalog.TryGet(data.RobotId, out var entry) && entry.LibraryInteractionMode == LibraryInteractionMode.SelectOnly)
            {
                guidedLessonButton.interactable = false;
                sandboxButton.interactable = false;
                robotControlButton.interactable = false;
                return;
            }

            var hasTemplate = RobotCatalog.HasTemplate(data.RobotId);
            guidedLessonButton.interactable = hasTemplate && data.GuidedLessonSupported;
            sandboxButton.interactable = hasTemplate && data.SandboxSupported;
            robotControlButton.interactable = hasTemplate && SupportsRobotControl(data);
            var robotControlLabel = robotControlButton.GetComponentInChildren<Text>();
            if (robotControlLabel != null)
            {
                robotControlLabel.text = GetRobotControlLabel();
            }
        }

        private static string BuildCapabilitySummary(RobotMetadataInfo metadata)
        {
            var parts = new List<string>
            {
                $"Convention: {metadata.Convention}",
                $"Visualization: {metadata.VisualizationLevel}"
            };

            if (metadata.GuidedLessonSupported)
            {
                parts.Add("Guided Lesson");
            }

            if (metadata.SandboxSupported)
            {
                parts.Add("Sandbox");
            }

            if (SupportsRobotControl(metadata))
            {
                parts.Add(GetRobotControlLabel());
            }

            if (metadata.InstructorRecommended)
            {
                parts.Add("Instructor Recommended");
            }

            return string.Join("  |  ", parts);
        }

        private static bool SupportsRobotControl(RobotMetadataInfo metadata)
        {
            var lessons = metadata.SupportedLessons;
            if (lessons == null)
            {
                return false;
            }

            for (var i = 0; i < lessons.Length; i++)
            {
                if (string.Equals(lessons[i], "RobotControl", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetRobotControlLabel()
        {
            return RobotControlScenePreference.ShouldPreferV3()
                ? "Robot Control V3"
                : "Robot Control";
        }

        private static void SafeDestroy(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
