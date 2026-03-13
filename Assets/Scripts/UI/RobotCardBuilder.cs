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
        private static readonly float CardWidth = UIDesignTokens.Size.CardWidth;
        private static readonly float CardHeight = UIDesignTokens.Size.CardHeight;
        private static readonly float Padding = UIDesignTokens.Space.Md;

        /// <summary>
        /// 카탈로그 항목으로 카드 UI를 생성합니다.
        /// </summary>
        public static RectTransform BuildCard(
            Transform parent,
            RobotCatalogEntry entry,
            Font font,
            Action onStartLesson,
            Action onOpenSandbox,
            Action onOpenRobotControl,
            Action onViewDetails)
        {
            var metadata = entry.Metadata;
            var cardRoot = UiRuntimeStyle.EnsureRectChild(parent, "Card_" + metadata.RobotId);
            cardRoot.sizeDelta = new Vector2(CardWidth, CardHeight);

            var le = UiRuntimeStyle.EnsureLayoutElement(cardRoot);
            le.preferredWidth = CardWidth;
            le.preferredHeight = CardHeight;

            var bg = UiRuntimeStyle.EnsureImage(cardRoot, "CardBg", UIDesignTokens.Colors.SurfaceCard);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.raycastTarget = true;

            var cardButton = cardRoot.GetComponent<Button>();
            if (cardButton == null)
            {
                cardButton = cardRoot.gameObject.AddComponent<Button>();
            }

            cardButton.transition = Selectable.Transition.ColorTint;
            cardButton.colors = UIDesignTokens.ButtonColors(UIDesignTokens.Colors.SurfaceCard);
            cardButton.onClick.RemoveAllListeners();
            if (onViewDetails != null)
            {
                cardButton.onClick.AddListener(() => onViewDetails());
            }

            var nameText = UiRuntimeStyle.EnsureText(cardRoot, "RobotName", font, UIDesignTokens.Type.HeadingLg, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(nameText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(CardWidth - Padding * 2, 28f), new Vector2(Padding, -Padding));
            nameText.text = metadata.DisplayName;

            BuildBadgeRow(cardRoot, metadata, font);

            var descText = UiRuntimeStyle.EnsureText(cardRoot, "Description", font, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(descText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(CardWidth - Padding * 2, 44f), new Vector2(Padding, -80f));
            descText.text = TruncateDescription(metadata.Description, 60);

            BuildCtaButton(cardRoot, entry, font, onStartLesson, onOpenSandbox, onOpenRobotControl);
            BuildDetailButton(cardRoot, font, onViewDetails);

            return cardRoot;
        }

        private static void BuildBadgeRow(RectTransform parent, RobotMetadataInfo metadata, Font font)
        {
            var badgeRow = UiRuntimeStyle.EnsureRectChild(parent, "BadgeRow");
            UiRuntimeStyle.Anchor(badgeRow, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(CardWidth - Padding * 2, 24f), new Vector2(Padding, -48f));
            UiRuntimeStyle.EnsureHorizontalLayout(badgeRow.gameObject, 8f);

            BuildBadge(badgeRow, "DofBadge", $"{metadata.Dof}DOF", UIDesignTokens.Colors.AccentPrimary, font);
            BuildBadge(badgeRow, "DiffBadge", metadata.Difficulty, DifficultyColor(metadata.Difficulty), font);
            BuildBadge(badgeRow, "PreviewBadge", GetPreviewLabel(metadata.VisualizationLevel), PreviewColor(metadata.VisualizationLevel), font);
        }

        private static void BuildBadge(Transform parent, string name, string label, Color color, Font font)
        {
            var badgeRect = UiRuntimeStyle.EnsureRectChild(parent, name);
            var le = UiRuntimeStyle.EnsureLayoutElement(badgeRect);
            le.preferredWidth = 70f;
            le.preferredHeight = 22f;

            var badgeBg = UiRuntimeStyle.EnsureImage(badgeRect, "Bg", new Color(color.r, color.g, color.b, 0.25f));
            UiRuntimeStyle.Stretch((RectTransform)badgeBg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var badgeText = UiRuntimeStyle.EnsureText(badgeRect, "Label", font, UIDesignTokens.Type.Tiny, FontStyle.Bold, TextAnchor.MiddleCenter, color);
            UiRuntimeStyle.Stretch(badgeText.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 2f), new Vector2(-4f, -2f));
            badgeText.text = label;
        }

        private static void BuildCtaButton(RectTransform parent, RobotCatalogEntry entry, Font font, Action onStartLesson, Action onOpenSandbox, Action onOpenRobotControl)
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
            bool sandboxSupported = entry.Metadata.SandboxSupported;
            bool robotControlSupported = SupportsRobotControl(entry);
            bool interactable = lessonSupported || robotControlSupported || sandboxSupported;
            string label = lessonSupported
                ? "학습 시작"
                : robotControlSupported
                    ? "Robot Control"
                    : sandboxSupported
                        ? "샌드박스"
                        : "Coming Soon";
            var bgColor = interactable ? UIDesignTokens.Colors.AccentPrimary : UIDesignTokens.Colors.SurfaceCard;

            UiRuntimeStyle.EnsureButtonLabel(button, font, label, bgColor);
            button.interactable = interactable;

            button.onClick.RemoveAllListeners();
            if (lessonSupported && onStartLesson != null)
            {
                button.onClick.AddListener(() => onStartLesson());
            }
            else if (robotControlSupported && onOpenRobotControl != null)
            {
                button.onClick.AddListener(() => onOpenRobotControl());
            }
            else if (sandboxSupported && onOpenSandbox != null)
            {
                button.onClick.AddListener(() => onOpenSandbox());
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

            UiRuntimeStyle.EnsureButtonLabel(button, font, "상세", UIDesignTokens.Colors.SurfaceRaisedAlt);

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

        private static bool SupportsRobotControl(RobotCatalogEntry entry)
        {
            if (entry == null)
            {
                return false;
            }

            var lessons = entry.Metadata.SupportedLessons;
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
    }
}
