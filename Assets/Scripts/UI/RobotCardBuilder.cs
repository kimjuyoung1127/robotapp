// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Robot Library 카드 UI를 생성합니다.
    /// </summary>
    internal static class RobotCardBuilder
    {
        private static readonly float DefaultCardWidth = UIDesignTokens.Size.CardWidth;
        private static readonly float DefaultCardHeight = UIDesignTokens.Size.CardHeight;
        private static readonly float Padding = UIDesignTokens.Space.Md;

        /// <summary>
        /// 카탈로그 항목으로 카드 UI를 생성합니다.
        /// </summary>
        public static RectTransform BuildCard(
            Transform parent,
            RobotCatalogEntry entry,
            Font font,
            bool isSelected,
            Action onSelect,
            Action onViewDetails,
            float cardWidth,
            float cardHeight)
        {
            var metadata = entry.Metadata;
            float widthScale = Mathf.Clamp(cardWidth / DefaultCardWidth, 1f, 1.85f);
            float heightScale = Mathf.Clamp(cardHeight / DefaultCardHeight, 0.9f, 1.15f);
            float titleHeight = Mathf.Round(28f * heightScale);
            float badgeTop = Mathf.Round(48f * heightScale);
            float descriptionTop = Mathf.Round(80f * heightScale);
            float descriptionHeight = Mathf.Round(44f * heightScale);
            float modeBottom = Mathf.Round(56f * heightScale);
            float modeHeight = Mathf.Round(30f * heightScale);
            float selectButtonWidth = Mathf.Round(140f * widthScale);
            float detailButtonWidth = Mathf.Round(80f * widthScale);
            float buttonHeight = Mathf.Round(36f * heightScale);
            float badgeWidth = Mathf.Round(70f * Mathf.Clamp(widthScale, 1f, 1.35f));
            float badgeHeight = Mathf.Round(22f * heightScale);
            int titleFontSize = widthScale >= 1.35f ? UIDesignTokens.Type.DisplaySm : UIDesignTokens.Type.HeadingLg;
            int bodyFontSize = widthScale >= 1.45f ? UIDesignTokens.Type.HeadingSm : UIDesignTokens.Type.Body;
            int badgeFontSize = widthScale >= 1.45f ? UIDesignTokens.Type.Caption : UIDesignTokens.Type.Tiny;
            var cardRoot = UiRuntimeStyle.EnsureRectChild(parent, "Card_" + metadata.RobotId);
            cardRoot.sizeDelta = new Vector2(cardWidth, cardHeight);

            var le = UiRuntimeStyle.EnsureLayoutElement(cardRoot);
            le.preferredWidth = cardWidth;
            le.preferredHeight = cardHeight;

            RemoveLegacyCardInteractions(cardRoot);

            var backgroundColor = isSelected
                ? Color.Lerp(UIDesignTokens.Colors.SurfaceCard, UIDesignTokens.Colors.AccentPrimary, 0.22f)
                : UIDesignTokens.Colors.SurfaceCard;
            var bg = UiRuntimeStyle.EnsureImage(cardRoot, "CardBg", backgroundColor);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.raycastTarget = true;

            BuildBodySelectButton(cardRoot, font, onSelect, cardWidth, buttonHeight);

            var nameText = UiRuntimeStyle.EnsureText(cardRoot, "RobotName", font, titleFontSize, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(nameText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(cardWidth - Padding * 2f, titleHeight), new Vector2(Padding, -Padding));
            nameText.text = metadata.DisplayName;

            BuildBadgeRow(cardRoot, metadata, font, cardWidth, badgeTop, badgeHeight, badgeWidth, badgeFontSize);

            var descText = UiRuntimeStyle.EnsureText(cardRoot, "Description", font, bodyFontSize, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(descText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(cardWidth - Padding * 2f, descriptionHeight), new Vector2(Padding, -descriptionTop));
            descText.text = TruncateDescription(metadata.Description, widthScale >= 1.45f ? 90 : 60);

            BuildModeSummary(cardRoot, entry, font, cardWidth, modeBottom, modeHeight, bodyFontSize);
            BuildSelectButton(cardRoot, font, isSelected, onSelect, buttonHeight, selectButtonWidth, bodyFontSize);
            BuildDetailButton(cardRoot, entry, font, onViewDetails, buttonHeight, detailButtonWidth, bodyFontSize);

            return cardRoot;
        }

        private static void BuildBodySelectButton(RectTransform parent, Font font, Action onSelect, float cardWidth, float buttonHeight)
        {
            var btnRect = UiRuntimeStyle.EnsureRectChild(parent, "BtnCardBody");
            UiRuntimeStyle.Stretch(btnRect, Vector2.zero, Vector2.one, new Vector2(Padding, Padding + buttonHeight + 12f), new Vector2(-Padding, -Padding));
            btnRect.SetSiblingIndex(1);

            var image = btnRect.GetComponent<Image>();
            if (image == null)
            {
                image = btnRect.gameObject.AddComponent<Image>();
            }

            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;

            var button = btnRect.GetComponent<Button>();
            if (button == null)
            {
                button = btnRect.gameObject.AddComponent<Button>();
            }

            button.transition = Selectable.Transition.ColorTint;
            button.colors = UIDesignTokens.ButtonColors(new Color(0f, 0f, 0f, 0f));
            button.onClick.RemoveAllListeners();
            if (onSelect != null)
            {
                button.onClick.AddListener(() => onSelect());
            }
        }

        private static void BuildBadgeRow(RectTransform parent, RobotMetadataInfo metadata, Font font, float cardWidth, float topOffset, float badgeHeight, float badgeWidth, int badgeFontSize)
        {
            var badgeRow = UiRuntimeStyle.EnsureRectChild(parent, "BadgeRow");
            UiRuntimeStyle.Anchor(badgeRow, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(cardWidth - Padding * 2f, badgeHeight), new Vector2(Padding, -topOffset));
            UiRuntimeStyle.EnsureHorizontalLayout(badgeRow.gameObject, 8f);

            BuildBadge(badgeRow, "DofBadge", $"{metadata.Dof}DOF", UIDesignTokens.Colors.AccentPrimary, font, badgeWidth, badgeHeight, badgeFontSize);
            BuildBadge(badgeRow, "DiffBadge", metadata.Difficulty, DifficultyColor(metadata.Difficulty), font, badgeWidth, badgeHeight, badgeFontSize);
            BuildBadge(badgeRow, "PreviewBadge", GetPreviewLabel(metadata.VisualizationLevel), PreviewColor(metadata.VisualizationLevel), font, badgeWidth, badgeHeight, badgeFontSize);
        }

        private static void BuildBadge(Transform parent, string name, string label, Color color, Font font, float width, float height, int fontSize)
        {
            var badgeRect = UiRuntimeStyle.EnsureRectChild(parent, name);
            var le = UiRuntimeStyle.EnsureLayoutElement(badgeRect);
            le.preferredWidth = width;
            le.preferredHeight = height;

            var badgeBg = UiRuntimeStyle.EnsureImage(badgeRect, "Bg", new Color(color.r, color.g, color.b, 0.25f));
            UiRuntimeStyle.Stretch((RectTransform)badgeBg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var badgeText = UiRuntimeStyle.EnsureText(badgeRect, "Label", font, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter, color);
            UiRuntimeStyle.Stretch(badgeText.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 2f), new Vector2(-4f, -2f));
            badgeText.text = label;
        }

        private static void BuildModeSummary(RectTransform parent, RobotCatalogEntry entry, Font font, float cardWidth, float bottomOffset, float modeHeight, int fontSize)
        {
            var modeText = UiRuntimeStyle.EnsureText(parent, "ModeSummary", font, fontSize, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(modeText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(-Padding * 2f, modeHeight), new Vector2(Padding, bottomOffset));
            modeText.text = BuildModeLabel(entry);
        }

        private static void BuildSelectButton(RectTransform parent, Font font, bool isSelected, Action onSelect, float buttonHeight, float buttonWidth, int fontSize)
        {
            var btnRect = UiRuntimeStyle.EnsureRectChild(parent, "BtnSelect");
            UiRuntimeStyle.Anchor(btnRect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(buttonWidth, buttonHeight), new Vector2(Padding, Padding));

            var image = btnRect.GetComponent<Image>();
            if (image == null)
            {
                image = btnRect.gameObject.AddComponent<Image>();
            }

            var button = btnRect.GetComponent<Button>();
            if (button == null)
            {
                button = btnRect.gameObject.AddComponent<Button>();
            }

            var backgroundColor = isSelected ? UIDesignTokens.Colors.AccentPrimary : UIDesignTokens.Colors.SurfaceRaisedAlt;
            var label = UiRuntimeStyle.EnsureButtonLabel(button, font, isSelected ? "선택됨" : "살펴보기", backgroundColor);
            label.fontSize = fontSize;
            button.interactable = !isSelected;
            button.onClick.RemoveAllListeners();
            if (!isSelected && onSelect != null)
            {
                button.onClick.AddListener(() => onSelect());
            }
        }

        private static void BuildDetailButton(RectTransform parent, RobotCatalogEntry entry, Font font, Action onViewDetails, float buttonHeight, float buttonWidth, int fontSize)
        {
            var btnRect = UiRuntimeStyle.EnsureRectChild(parent, "BtnDetail");
            UiRuntimeStyle.Anchor(btnRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(buttonWidth, buttonHeight), new Vector2(-Padding, Padding));
            bool allowDetails = entry != null && entry.LibraryInteractionMode != LibraryInteractionMode.SelectOnly;
            btnRect.gameObject.SetActive(allowDetails);
            if (!allowDetails)
            {
                return;
            }

            var image = btnRect.GetComponent<Image>();
            if (image == null)
            {
                image = btnRect.gameObject.AddComponent<Image>();
            }

            var button = btnRect.GetComponent<Button>();
            if (button == null)
            {
                button = btnRect.gameObject.AddComponent<Button>();
            }

            var label = UiRuntimeStyle.EnsureButtonLabel(button, font, "상세", UIDesignTokens.Colors.SurfaceRaisedAlt);
            label.fontSize = fontSize;

            if (onViewDetails != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onViewDetails());
            }
        }

        private static Color DifficultyColor(string difficulty)
        {
            return UIDesignTokens.GetDifficultyColor(difficulty);
        }

        private static string GetPreviewLabel(string visualizationLevel)
        {
            switch (visualizationLevel)
            {
                case "DonorMesh":
                    return "REAL 3D";
                case "Lesson":
                    return "TEACH";
                default:
                    return "CONCEPT";
            }
        }

        private static Color PreviewColor(string visualizationLevel)
        {
            switch (visualizationLevel)
            {
                case "DonorMesh":
                    return UIDesignTokens.Colors.AccentSuccess;
                case "Lesson":
                    return UIDesignTokens.Colors.AccentSecondary;
                default:
                    return UIDesignTokens.Colors.TextMuted;
            }
        }

        private static string TruncateDescription(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text ?? string.Empty;
            }

            return text.Substring(0, maxLength - 3) + "...";
        }

        private static string BuildModeLabel(RobotCatalogEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            var metadata = entry.Metadata;
            var modes = string.Empty;
            modes += metadata.GuidedLessonSupported ? "Lesson" : "Observe";
            if (metadata.SandboxSupported)
            {
                modes += " · Sandbox";
            }

            if (SupportsRobotControl(metadata))
            {
                modes += " · Control";
            }

            if (metadata.InstructorRecommended)
            {
                modes += " · Instructor";
            }

            if (entry.LibraryInteractionMode == LibraryInteractionMode.SelectOnly)
            {
                modes += " · Select";
            }

            return modes;
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

        private static void RemoveLegacyCardInteractions(RectTransform cardRoot)
        {
            var rootButton = cardRoot.GetComponent<Button>();
            if (rootButton != null)
            {
                DestroyComponentImmediate(rootButton);
            }

            var legacyCta = cardRoot.Find("BtnCta");
            if (legacyCta != null)
            {
                DestroyObjectImmediate(legacyCta.gameObject);
            }
        }

        private static void DestroyComponentImmediate(Component component)
        {
            if (component == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(component);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(component);
            }
        }

        private static void DestroyObjectImmediate(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}
