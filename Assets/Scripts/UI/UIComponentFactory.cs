// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 재사용 가능한 복합 UI 위젯 빌더.
    /// 모든 위젯은 UIDesignTokens의 토큰만 사용하여 일관된 디자인을 보장합니다.
    /// </summary>
    public static class UIComponentFactory
    {
        // ── Panels ───────────────────────────────────────────────────────

        /// <summary>배경색이 있는 패널을 생성합니다.</summary>
        public static RectTransform CreatePanel(Transform parent, string name, Color bgColor, RectOffset padding = null)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);
            var bg = UiRuntimeStyle.EnsureImage(rect, "Bg", bgColor);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            if (padding != null)
            {
                var vlg = rect.GetComponent<VerticalLayoutGroup>();
                if (vlg == null)
                {
                    vlg = rect.gameObject.AddComponent<VerticalLayoutGroup>();
                }

                vlg.padding = padding;
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
            }

            return rect;
        }

        /// <summary>카드 스타일 패널을 생성합니다.</summary>
        public static RectTransform CreateCardPanel(Transform parent, string name)
        {
            return CreatePanel(parent, name, UIDesignTokens.Colors.SurfaceCard,
                new RectOffset(
                    (int)UIDesignTokens.Space.Md,
                    (int)UIDesignTokens.Space.Md,
                    (int)UIDesignTokens.Space.Md,
                    (int)UIDesignTokens.Space.Md));
        }

        // ── Text ─────────────────────────────────────────────────────────

        /// <summary>프리셋 기반 Legacy Text를 생성합니다.</summary>
        public static Text CreateText(
            Transform parent,
            string name,
            TypographyPreset preset,
            Color color,
            string content,
            Font font = null,
            TextAnchor anchor = TextAnchor.UpperLeft)
        {
            var text = UiRuntimeStyle.EnsureText(
                parent, name,
                UiRuntimeStyle.ResolveFont(font),
                UITypography.GetFontSize(preset),
                UITypography.GetLegacyStyle(preset),
                anchor, color);
            text.text = content;
            return text;
        }

        // ── Buttons ──────────────────────────────────────────────────────

        /// <summary>Primary (AccentPrimary) 버튼을 생성합니다.</summary>
        public static Button CreatePrimaryButton(Transform parent, string name, string label, Font font = null, float width = 180f)
        {
            return CreateStyledButton(parent, name, label, UIDesignTokens.Colors.AccentPrimary, font, width, UIDesignTokens.Size.ButtonHeightLg);
        }

        /// <summary>Secondary (SurfaceRaisedAlt) 버튼을 생성합니다.</summary>
        public static Button CreateSecondaryButton(Transform parent, string name, string label, Font font = null, float width = 180f)
        {
            return CreateStyledButton(parent, name, label, UIDesignTokens.Colors.SurfaceRaisedAlt, font, width, UIDesignTokens.Size.ButtonHeightMd);
        }

        /// <summary>Ghost (투명 배경) 버튼을 생성합니다.</summary>
        public static Button CreateGhostButton(Transform parent, string name, string label, Font font = null)
        {
            return CreateStyledButton(parent, name, label, Color.clear, font, 0f, UIDesignTokens.Size.ButtonHeightMd);
        }

        /// <summary>버튼 왼쪽에 leading icon을 부착합니다.</summary>
        public static Image AttachLeadingIcon(Button button, string iconName, float size = UIDesignTokens.Size.IconSm)
        {
            if (button == null)
            {
                return null;
            }

            var icon = UIIconResolver.CreateIcon(button.transform, "LeadingIcon", iconName, size, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(size, size), new Vector2(UIDesignTokens.Space.Md, 0f));

            var label = button.transform.Find("Label")?.GetComponent<Text>();
            if (label != null)
            {
                var rect = button.transform as RectTransform;
                var width = rect != null ? rect.sizeDelta.x : 0f;
                var compact = width > 0f && width <= 120f;
                label.alignment = compact ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
                label.horizontalOverflow = HorizontalWrapMode.Overflow;
                label.verticalOverflow = VerticalWrapMode.Truncate;
                label.resizeTextForBestFit = compact;
                label.resizeTextMinSize = UIDesignTokens.Type.Tiny;
                label.resizeTextMaxSize = compact ? UIDesignTokens.Type.Caption : UIDesignTokens.Type.HeadingSm;
                UiRuntimeStyle.Stretch(
                    label.rectTransform,
                    Vector2.zero,
                    Vector2.one,
                    compact ? new Vector2(UIDesignTokens.Space.Xs, 4f) : new Vector2(UIDesignTokens.Space.Xl, 4f),
                    new Vector2(-UIDesignTokens.Space.Sm, -4f));
            }

            return icon;
        }

        /// <summary>아이콘 전용 버튼을 생성합니다.</summary>
        public static Button CreateIconButton(Transform parent, string name, string iconName, float size = UIDesignTokens.Size.ButtonHeightMd)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);
            rect.sizeDelta = new Vector2(size, size);

            var image = rect.GetComponent<Image>();
            if (image == null)
            {
                image = rect.gameObject.AddComponent<Image>();
            }

            image.color = Color.clear;

            var button = rect.GetComponent<Button>();
            if (button == null)
            {
                button = rect.gameObject.AddComponent<Button>();
            }

            button.colors = UIDesignTokens.ButtonColors(UIDesignTokens.Colors.SurfaceRaisedAlt);

            UIIconResolver.CreateIcon(rect, "Icon", iconName, size * 0.6f, UIDesignTokens.Colors.TextPrimary);

            return button;
        }

        // ── Input ────────────────────────────────────────────────────────

        /// <summary>입력 필드를 생성합니다.</summary>
        public static InputField CreateInputField(Transform parent, string name, string placeholder, Font font = null)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);
            rect.sizeDelta = new Vector2(UIDesignTokens.Size.InputFieldWidth, UIDesignTokens.Size.ButtonHeightMd);

            var bg = UiRuntimeStyle.EnsureImage(rect, "Bg", UIDesignTokens.Colors.SurfaceInput);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var resolvedFont = UiRuntimeStyle.ResolveFont(font);
            var inputText = UiRuntimeStyle.EnsureText(rect, "Text", resolvedFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Stretch(inputText.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(UIDesignTokens.Space.Xs, UIDesignTokens.Space.Xxs),
                new Vector2(-UIDesignTokens.Space.Xs, -UIDesignTokens.Space.Xxs));
            inputText.raycastTarget = true;

            var placeholderText = UiRuntimeStyle.EnsureText(rect, "Placeholder", resolvedFont, UIDesignTokens.Type.Body, FontStyle.Italic, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Stretch(placeholderText.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(UIDesignTokens.Space.Xs, UIDesignTokens.Space.Xxs),
                new Vector2(-UIDesignTokens.Space.Xs, -UIDesignTokens.Space.Xxs));
            placeholderText.text = placeholder;

            var inputField = rect.GetComponent<InputField>();
            if (inputField == null)
            {
                inputField = rect.gameObject.AddComponent<InputField>();
            }

            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;

            return inputField;
        }

        // ── Slider ───────────────────────────────────────────────────────

        /// <summary>슬라이더를 생성합니다.</summary>
        public static Slider CreateSlider(Transform parent, string name, float min, float max)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);
            rect.sizeDelta = new Vector2(200f, UIDesignTokens.Size.SliderHeight);

            var bgImage = UiRuntimeStyle.EnsureImage(rect, "Background", UIDesignTokens.Colors.SliderTrack);
            UiRuntimeStyle.Stretch((RectTransform)bgImage.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var fillArea = UiRuntimeStyle.EnsureRectChild(rect, "FillArea");
            UiRuntimeStyle.Stretch(fillArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fill = UiRuntimeStyle.EnsureImage(fillArea, "Fill", UIDesignTokens.Colors.SliderFill);
            UiRuntimeStyle.Stretch((RectTransform)fill.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var handleArea = UiRuntimeStyle.EnsureRectChild(rect, "HandleSlideArea");
            UiRuntimeStyle.Stretch(handleArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var handle = UiRuntimeStyle.EnsureImage(handleArea, "Handle", UIDesignTokens.Colors.SliderHandle);
            handle.rectTransform.sizeDelta = new Vector2(UIDesignTokens.Space.Md, UIDesignTokens.Size.SliderHeight);

            var slider = rect.GetComponent<Slider>();
            if (slider == null)
            {
                slider = rect.gameObject.AddComponent<Slider>();
            }

            slider.fillRect = (RectTransform)fill.transform;
            slider.handleRect = handle.rectTransform;
            slider.minValue = min;
            slider.maxValue = max;

            return slider;
        }

        // ── Badge ────────────────────────────────────────────────────────

        /// <summary>라벨 뱃지를 생성합니다.</summary>
        public static RectTransform CreateBadge(Transform parent, string name, string label, Color color, Font font = null)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);
            var le = UiRuntimeStyle.EnsureLayoutElement(rect);
            le.preferredWidth = UIDesignTokens.Size.BadgeWidth;
            le.preferredHeight = UIDesignTokens.Size.BadgeHeight;

            var bg = UiRuntimeStyle.EnsureImage(rect, "Bg", new Color(color.r, color.g, color.b, 0.25f));
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var text = UiRuntimeStyle.EnsureText(rect, "Label", UiRuntimeStyle.ResolveFont(font),
                UIDesignTokens.Type.Tiny + 1, FontStyle.Bold, TextAnchor.MiddleCenter, color);
            UiRuntimeStyle.Stretch(text.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(UIDesignTokens.Space.Xxs, 2f),
                new Vector2(-UIDesignTokens.Space.Xxs, -2f));
            text.text = label;

            return rect;
        }

        // ── Layout ───────────────────────────────────────────────────────

        /// <summary>수직 레이아웃 그룹을 생성합니다.</summary>
        public static VerticalLayoutGroup CreateVStack(Transform parent, string name, float spacing = -1f)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);
            return UiRuntimeStyle.EnsureVerticalLayout(rect.gameObject, spacing < 0 ? UIDesignTokens.Space.Xs : spacing);
        }

        /// <summary>수평 레이아웃 그룹을 생성합니다.</summary>
        public static HorizontalLayoutGroup CreateHStack(Transform parent, string name, float spacing = -1f)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);
            return UiRuntimeStyle.EnsureHorizontalLayout(rect.gameObject, spacing < 0 ? UIDesignTokens.Space.Xs : spacing);
        }

        /// <summary>버튼 row 컨테이너를 생성합니다.</summary>
        public static RectTransform CreateButtonRow(Transform parent, string name, float spacing = -1f)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);
            var layout = UiRuntimeStyle.EnsureHorizontalLayout(rect.gameObject, spacing < 0 ? UIDesignTokens.Space.Xs : spacing);
            layout.childAlignment = TextAnchor.MiddleLeft;
            return rect;
        }

        /// <summary>스크롤뷰를 생성합니다.</summary>
        public static ScrollRect CreateScrollView(Transform parent, string name, bool vertical = true)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);

            var scrollRect = rect.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                scrollRect = rect.gameObject.AddComponent<ScrollRect>();
            }

            var viewport = UiRuntimeStyle.EnsureRectChild(rect, "Viewport");
            UiRuntimeStyle.Stretch(viewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var mask = viewport.GetComponent<Mask>();
            if (mask == null)
            {
                mask = viewport.gameObject.AddComponent<Mask>();
            }

            mask.showMaskGraphic = false;
            var maskImage = viewport.GetComponent<Image>();
            if (maskImage == null)
            {
                maskImage = viewport.gameObject.AddComponent<Image>();
            }

            var content = UiRuntimeStyle.EnsureRectChild(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);

            scrollRect.content = content;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = !vertical;
            scrollRect.vertical = vertical;

            return scrollRect;
        }

        // ── Decorators ───────────────────────────────────────────────────

        /// <summary>구분선을 생성합니다.</summary>
        public static Image CreateDivider(Transform parent, string name)
        {
            var image = UiRuntimeStyle.EnsureImage(parent, name, UIDesignTokens.Colors.BorderSoft);
            var le = UiRuntimeStyle.EnsureLayoutElement(image);
            le.preferredHeight = 1f;
            le.flexibleWidth = 1f;
            return image;
        }

        /// <summary>모달 배경 오버레이를 생성합니다.</summary>
        public static RectTransform CreateModalBackdrop(Transform parent, string name)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);
            UiRuntimeStyle.Stretch(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var bg = UiRuntimeStyle.EnsureImage(rect, "Overlay", UIDesignTokens.Colors.SurfaceOverlay);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.raycastTarget = true;

            return rect;
        }

        // ── Toggle ──────────────────────────────────────────────────────

        /// <summary>토글 위젯을 생성합니다.</summary>
        public static Toggle CreateToggle(Transform parent, string name, string label, Font font = null)
        {
            var existing = parent.Find(name)?.GetComponent<Toggle>();
            if (existing != null)
            {
                return existing;
            }

            var toggleGo = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            toggleGo.transform.SetParent(parent, false);
            var toggle = toggleGo.GetComponent<Toggle>();

            var background = UiRuntimeStyle.EnsureImage(toggleGo.transform, "Background", UIDesignTokens.Colors.SurfaceCard);
            UiRuntimeStyle.Anchor(background.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 20f), new Vector2(0f, 0f));

            var checkmark = UiRuntimeStyle.EnsureImage(background.transform, "Checkmark", UIDesignTokens.Colors.AccentPrimary);
            UiRuntimeStyle.Stretch(checkmark.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            toggle.targetGraphic = background;
            toggle.graphic = checkmark;

            var text = UiRuntimeStyle.EnsureText(toggleGo.transform, "Label", UiRuntimeStyle.ResolveFont(font), UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(text.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(160f, 20f), new Vector2(28f, 0f));
            text.text = label;

            return toggle;
        }

        // ── Internal ─────────────────────────────────────────────────────

        private static Button CreateStyledButton(
            Transform parent,
            string name,
            string label,
            Color bgColor,
            Font font,
            float width,
            float height)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);
            if (width > 0f)
            {
                rect.sizeDelta = new Vector2(width, height);
            }
            else
            {
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            }

            var button = rect.GetComponent<Button>();
            if (button == null)
            {
                var image = rect.GetComponent<Image>();
                if (image == null)
                {
                    rect.gameObject.AddComponent<Image>();
                }

                button = rect.gameObject.AddComponent<Button>();
            }

            UiRuntimeStyle.EnsureButtonLabel(button, UiRuntimeStyle.ResolveFont(font), label, bgColor);
            return button;
        }
    }
}
