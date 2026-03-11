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
        private const float CardWidth = 280f;
        private const float CardHeight = 220f;
        private const float Padding = 16f;

        /// <summary>
        /// 카탈로그 항목으로 카드 UI를 생성합니다.
        /// </summary>
        public static RectTransform BuildCard(
            Transform parent,
            RobotCatalogEntry entry,
            Font font,
            Action onStartLesson,
            Action onViewDetails)
        {
            var metadata = entry.Metadata;
            var cardRoot = UiRuntimeStyle.EnsureRectChild(parent, "Card_" + metadata.RobotId);
            cardRoot.sizeDelta = new Vector2(CardWidth, CardHeight);

            var le = UiRuntimeStyle.EnsureLayoutElement(cardRoot);
            le.preferredWidth = CardWidth;
            le.preferredHeight = CardHeight;

            var bg = UiRuntimeStyle.EnsureImage(cardRoot, "CardBg", UiRuntimeStyle.CardBackground);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.raycastTarget = true;

            var nameText = UiRuntimeStyle.EnsureText(cardRoot, "RobotName", font, 18, FontStyle.Bold, TextAnchor.UpperLeft, UiRuntimeStyle.TextPrimary);
            UiRuntimeStyle.Anchor(nameText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(CardWidth - Padding * 2, 28f), new Vector2(Padding, -Padding));
            nameText.text = metadata.DisplayName;

            BuildBadgeRow(cardRoot, metadata, font);

            var descText = UiRuntimeStyle.EnsureText(cardRoot, "Description", font, 13, FontStyle.Normal, TextAnchor.UpperLeft, UiRuntimeStyle.TextSecondary);
            UiRuntimeStyle.Anchor(descText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(CardWidth - Padding * 2, 44f), new Vector2(Padding, -80f));
            descText.text = TruncateDescription(metadata.Description, 60);

            BuildCtaButton(cardRoot, entry, font, onStartLesson);
            BuildDetailButton(cardRoot, font, onViewDetails);

            return cardRoot;
        }

        private static void BuildBadgeRow(RectTransform parent, RobotMetadataInfo metadata, Font font)
        {
            var badgeRow = UiRuntimeStyle.EnsureRectChild(parent, "BadgeRow");
            UiRuntimeStyle.Anchor(badgeRow, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(CardWidth - Padding * 2, 24f), new Vector2(Padding, -48f));
            UiRuntimeStyle.EnsureHorizontalLayout(badgeRow.gameObject, 8f);

            BuildBadge(badgeRow, "DofBadge", $"{metadata.Dof}DOF", UiRuntimeStyle.AccentBlue, font);
            BuildBadge(badgeRow, "DiffBadge", metadata.Difficulty, DifficultyColor(metadata.Difficulty), font);
        }

        private static void BuildBadge(Transform parent, string name, string label, Color color, Font font)
        {
            var badgeRect = UiRuntimeStyle.EnsureRectChild(parent, name);
            var le = UiRuntimeStyle.EnsureLayoutElement(badgeRect);
            le.preferredWidth = 70f;
            le.preferredHeight = 22f;

            var badgeBg = UiRuntimeStyle.EnsureImage(badgeRect, "Bg", new Color(color.r, color.g, color.b, 0.25f));
            UiRuntimeStyle.Stretch((RectTransform)badgeBg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var badgeText = UiRuntimeStyle.EnsureText(badgeRect, "Label", font, 11, FontStyle.Bold, TextAnchor.MiddleCenter, color);
            UiRuntimeStyle.Stretch(badgeText.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 2f), new Vector2(-4f, -2f));
            badgeText.text = label;
        }

        private static void BuildCtaButton(RectTransform parent, RobotCatalogEntry entry, Font font, Action onStartLesson)
        {
            var btnRect = UiRuntimeStyle.EnsureRectChild(parent, "BtnCta");
            UiRuntimeStyle.Anchor(btnRect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(140f, 36f), new Vector2(Padding, Padding));

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

            bool lessonSupported = entry.Metadata.GuidedLessonSupported;
            string label = lessonSupported ? "학습 시작" : "Coming Soon";
            var bgColor = lessonSupported ? UiRuntimeStyle.AccentBlue : UiRuntimeStyle.CardBackground;

            UiRuntimeStyle.EnsureButtonLabel(button, font, label, bgColor);
            button.interactable = lessonSupported;

            if (lessonSupported && onStartLesson != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onStartLesson());
            }
        }

        private static void BuildDetailButton(RectTransform parent, Font font, Action onViewDetails)
        {
            var btnRect = UiRuntimeStyle.EnsureRectChild(parent, "BtnDetail");
            UiRuntimeStyle.Anchor(btnRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(80f, 36f), new Vector2(-Padding, Padding));

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

            UiRuntimeStyle.EnsureButtonLabel(button, font, "상세", UiRuntimeStyle.PanelBackgroundAlt);

            if (onViewDetails != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onViewDetails());
            }
        }

        private static Color DifficultyColor(string difficulty)
        {
            switch (difficulty)
            {
                case "Easy": return new Color(0.3f, 0.85f, 0.45f, 1f);
                case "Hard": return new Color(0.9f, 0.35f, 0.3f, 1f);
                default: return UiRuntimeStyle.AccentYellow;
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
    }
}
