// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    internal readonly struct SnapshotLiteViewRefs
    {
        public SnapshotLiteViewRefs(RectTransform panelRoot, Text statusText)
        {
            PanelRoot = panelRoot;
            StatusText = statusText;
        }

        public RectTransform PanelRoot { get; }
        public Text StatusText { get; }
    }

    internal static class SnapshotLitePanelViewBuilder
    {
        public static SnapshotLiteViewRefs Build(
            SnapshotLitePanel host,
            RectTransform panelRoot,
            Font fallbackFont,
            UnityEngine.Events.UnityAction onSave,
            UnityEngine.Events.UnityAction onLoad,
            UnityEngine.Events.UnityAction onCompare)
        {
            panelRoot ??= UiRuntimeStyle.EnsureHostedRoot(host, "SnapshotLiteRect");
            var width = UILayoutProfile.IsTablet ? 292f : 348f;
            UiRuntimeStyle.Stretch(panelRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var content = UIComponentFactory.CreateCardPanel(panelRoot, "SnapshotContent");
            UiRuntimeStyle.Stretch(
                content,
                Vector2.zero,
                Vector2.one,
                new Vector2(UIDesignTokens.Space.Sm, UIDesignTokens.Space.Sm),
                new Vector2(-UIDesignTokens.Space.Sm, -UIDesignTokens.Space.Sm));

            var title = UIComponentFactory.CreateText(content, "Title", TypographyPreset.HeadingSm, UIDesignTokens.Colors.TextPrimary, "Snapshot Lite", fallbackFont);
            UiRuntimeStyle.Anchor(
                title.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(width, 24f),
                new Vector2(UIDesignTokens.Space.Md, -UIDesignTokens.Space.Md));

            var statusText = UIComponentFactory.CreateText(content, "Status", TypographyPreset.Caption, UIDesignTokens.Colors.TextSecondary, string.Empty, fallbackFont);
            statusText.fontStyle = FontStyle.Italic;
            UiRuntimeStyle.Anchor(
                statusText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(width, 64f),
                new Vector2(UIDesignTokens.Space.Md, -44f));

            var actionsRow = UIComponentFactory.CreateButtonRow(content, "ActionRow", UIDesignTokens.Space.Sm);
            UiRuntimeStyle.Anchor(
                actionsRow,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(width, 76f),
                new Vector2(UIDesignTokens.Space.Md, UIDesignTokens.Space.Md));
            var layout = actionsRow.gameObject.GetComponent<HorizontalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childControlWidth = true;

            var buttonWidth = UILayoutProfile.IsTablet ? 82f : 96f;
            CreateButton(actionsRow, fallbackFont, "BtnSaveSnapshot", "Save", onSave, buttonWidth);
            CreateButton(actionsRow, fallbackFont, "BtnLoadSnapshot", "Load", onLoad, buttonWidth);
            CreateButton(actionsRow, fallbackFont, "BtnCompareSnapshot", "Compare", onCompare, buttonWidth);

            return new SnapshotLiteViewRefs(panelRoot, statusText);
        }

        private static Button CreateButton(Transform parent, Font fallbackFont, string name, string label, UnityEngine.Events.UnityAction onClick, float width)
        {
            var button = UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, width);
            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.preferredWidth = width;
            element.preferredHeight = UIDesignTokens.Size.ButtonHeightMd;
            var labelText = button.transform.Find("Label")?.GetComponent<Text>();
            if (labelText != null)
            {
                labelText.alignment = TextAnchor.MiddleCenter;
                labelText.resizeTextForBestFit = true;
                labelText.resizeTextMinSize = UIDesignTokens.Type.Tiny;
                labelText.resizeTextMaxSize = UIDesignTokens.Type.Body;
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
