// Folder: UI - Sandbox 액션 패널 뷰 빌더.
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    internal static class SandboxActionPanelViewBuilder
    {
        private const string HomeIcon = "icon-home";
        private const string ResetIcon = "icon-reset";
        private const string LibraryIcon = "icon-search";
        private const string PlayIcon = "icon-play";
        private const string BackIcon = "icon-back";
        private const string EditIcon = "icon-edit";

        public static RectTransform Build(
            SandboxActionPanel host,
            RectTransform panelRoot,
            Font fallbackFont,
            UnityEngine.Events.UnityAction onZeroPose,
            UnityEngine.Events.UnityAction onHomePose,
            UnityEngine.Events.UnityAction onDemoPose,
            UnityEngine.Events.UnityAction onResetPose,
            UnityEngine.Events.UnityAction onChangeRobot,
            UnityEngine.Events.UnityAction onBackToLibrary,
            UnityEngine.Events.UnityAction onOpenRobotLibrary)
        {
            panelRoot ??= UiRuntimeStyle.EnsureHostedRoot(host, "SandboxActionRect");
            var width = UILayoutProfile.IsTablet ? 252f : 308f;
            UiRuntimeStyle.Stretch(panelRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var content = UIComponentFactory.CreateCardPanel(panelRoot, "SandboxActionContent");
            UiRuntimeStyle.Stretch(
                content,
                Vector2.zero,
                Vector2.one,
                new Vector2(UIDesignTokens.Space.Sm, UIDesignTokens.Space.Sm),
                new Vector2(-UIDesignTokens.Space.Sm, -UIDesignTokens.Space.Sm));

            var title = UIComponentFactory.CreateText(content, "Title", TypographyPreset.HeadingSm, UIDesignTokens.Colors.TextPrimary, "Sandbox Actions", fallbackFont);
            UiRuntimeStyle.Anchor(
                title.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(width, 24f),
                new Vector2(UIDesignTokens.Space.Md, -UIDesignTokens.Space.Md));
            title.text = "Sandbox Actions";

            var primaryGroup = UIComponentFactory.CreateVStack(content, "PrimaryActions", UIDesignTokens.Space.Sm);
            UiRuntimeStyle.Anchor(
                (RectTransform)primaryGroup.transform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(width, 184f),
                new Vector2(UIDesignTokens.Space.Md, -56f));
            CreateWideButton(primaryGroup.transform, fallbackFont, "BtnZeroPose", "Zero Pose", onZeroPose, ResetIcon);
            CreateWideButton(primaryGroup.transform, fallbackFont, "BtnHomePose", "Home Pose", onHomePose, HomeIcon);
            CreateWideButton(primaryGroup.transform, fallbackFont, "BtnDemoPose", "Demo Pose", onDemoPose, PlayIcon);
            CreateWideButton(primaryGroup.transform, fallbackFont, "BtnResetPose", "Reset", onResetPose, ResetIcon);
            CreateWideButton(primaryGroup.transform, fallbackFont, "BtnChangeRobot", "Change Robot", onChangeRobot, EditIcon);

            var navDivider = UIComponentFactory.CreateDivider(content, "NavDivider");
            UiRuntimeStyle.Anchor(
                navDivider.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(width, 1f),
                new Vector2(UIDesignTokens.Space.Md, -306f));

            var navGroup = UIComponentFactory.CreateVStack(content, "NavigationActions", UIDesignTokens.Space.Sm);
            UiRuntimeStyle.Anchor(
                (RectTransform)navGroup.transform,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(width, 96f),
                new Vector2(UIDesignTokens.Space.Md, UIDesignTokens.Space.Md));
            CreateWideButton(navGroup.transform, fallbackFont, "BtnBackToLibrary", "← 로봇 목록", onBackToLibrary, BackIcon);
            CreateWideButton(navGroup.transform, fallbackFont, "BtnOpenRobotLibrary", "Robot Library", onOpenRobotLibrary, LibraryIcon);
            return panelRoot;
        }

        private static Button CreateWideButton(Transform parent, Font fallbackFont, string name, string label, UnityEngine.Events.UnityAction onClick, string iconName = null)
        {
            var width = UILayoutProfile.IsTablet ? 252f : 276f;
            var button = UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, width);
            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.preferredWidth = width;
            element.preferredHeight = UIDesignTokens.Size.ButtonHeightMd;
            if (!string.IsNullOrEmpty(iconName))
            {
                UIComponentFactory.AttachLeadingIcon(button, iconName);
            }
            else
            {
                var labelText = button.transform.Find("Label")?.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.alignment = TextAnchor.MiddleCenter;
                    labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    labelText.verticalOverflow = VerticalWrapMode.Truncate;
                }
            }
            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
            return button;
        }
    }
}
