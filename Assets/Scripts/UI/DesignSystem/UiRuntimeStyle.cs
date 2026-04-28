// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KineTutor3D.UI
{
    internal static class UiRuntimeStyle
    {
        private static Sprite cachedDefaultSprite;

        // ── Legacy color aliases → UIDesignTokens (기존 시그니처 100% 유지) ──

        [Obsolete("Use UIDesignTokens.Colors.SurfaceBase instead")]
        public static readonly Color CanvasBackdrop = new Color(UIDesignTokens.Colors.SurfaceBase.r, UIDesignTokens.Colors.SurfaceBase.g, UIDesignTokens.Colors.SurfaceBase.b, 0f);

        [Obsolete("Use UIDesignTokens.Colors.SurfaceRaised instead")]
        public static readonly Color PanelBackground = UIDesignTokens.Colors.SurfaceRaised;

        [Obsolete("Use UIDesignTokens.Colors.SurfaceRaisedAlt instead")]
        public static readonly Color PanelBackgroundAlt = UIDesignTokens.Colors.SurfaceRaisedAlt;

        [Obsolete("Use UIDesignTokens.Colors.SurfaceCard instead")]
        public static readonly Color CardBackground = UIDesignTokens.Colors.SurfaceCard;

        [Obsolete("Use UIDesignTokens.Colors.AccentPrimary instead")]
        public static readonly Color AccentBlue = UIDesignTokens.Colors.AccentPrimary;

        [Obsolete("Use UIDesignTokens.Colors.AccentSecondary instead")]
        public static readonly Color AccentYellow = UIDesignTokens.Colors.AccentSecondary;

        [Obsolete("Use UIDesignTokens.Colors.BorderSoft instead")]
        public static readonly Color BorderSoft = UIDesignTokens.Colors.BorderSoft;

        [Obsolete("Use UIDesignTokens.Colors.TextPrimary instead")]
        public static readonly Color TextPrimary = UIDesignTokens.Colors.TextPrimary;

        [Obsolete("Use UIDesignTokens.Colors.TextSecondary instead")]
        public static readonly Color TextSecondary = UIDesignTokens.Colors.TextSecondary;

        [Obsolete("Use UIDesignTokens.Colors.TextMuted instead")]
        public static readonly Color TextMuted = UIDesignTokens.Colors.TextMuted;

        [Obsolete("Use UIDesignTokens.Colors.DangerMuted instead")]
        public static readonly Color DangerMuted = UIDesignTokens.Colors.DangerMuted;

        public static Font ResolveFont(Font fallback)
        {
            if (fallback != null)
            {
                return fallback;
            }

            var legacyRuntime = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (legacyRuntime != null)
            {
                return legacyRuntime;
            }

            try
            {
                return Font.CreateDynamicFontFromOSFont("Arial", 14);
            }
            catch
            {
                return null;
            }
        }

        public static RectTransform EnsureHostedRoot(Component host, string rootName)
        {
            if (host == null)
            {
                return null;
            }

            if (host.transform is RectTransform selfRect)
            {
                return selfRect;
            }

            var parentRect = host.transform.parent as RectTransform;
            if (parentRect == null)
            {
                return null;
            }

            var existing = parentRect.Find(rootName) as RectTransform;
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject(rootName, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parentRect, false);
            rect.SetSiblingIndex(host.transform.GetSiblingIndex());
            return rect;
        }

        public static RectTransform EnsureRectChild(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                return existing as RectTransform;
            }

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static Image EnsureImage(Transform parent, string name, Color color)
        {
            var rect = EnsureRectChild(parent, name);
            var image = rect.GetComponent<Image>();
            if (image == null)
            {
                image = rect.gameObject.AddComponent<Image>();
            }

            if (image.sprite == null)
            {
                image.sprite = ResolveDefaultSprite();
                image.type = Image.Type.Sliced;
            }

            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static T ReparentTo<T>(T component, Transform parent) where T : Component
        {
            if (component != null && parent != null && component.transform.parent != parent)
            {
                component.transform.SetParent(parent, false);
            }

            return component;
        }

        public static Text EnsureText(Transform parent, string name, Font font, int fontSize, FontStyle fontStyle, TextAnchor anchor, Color color)
        {
            var rect = EnsureRectChild(parent, name);
            var text = rect.GetComponent<Text>();
            if (text == null)
            {
                text = rect.gameObject.AddComponent<Text>();
            }

            text.font = ResolveFont(font);
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = true;
            text.raycastTarget = false;
            return text;
        }

        public static Outline EnsureOutline(Graphic graphic, Color color, Vector2 distance)
        {
            var outline = graphic.GetComponent<Outline>();
            if (outline == null)
            {
                outline = graphic.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = color;
            outline.effectDistance = distance;
            return outline;
        }

        public static LayoutElement EnsureLayoutElement(Component target)
        {
            var element = target.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = target.gameObject.AddComponent<LayoutElement>();
            }

            return element;
        }

        public static CanvasGroup EnsureCanvasGroup(GameObject target)
        {
            var group = target.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = target.AddComponent<CanvasGroup>();
            }

            return group;
        }

        public static void SetCanvasVisible(GameObject target, bool visible)
        {
            if (target == null)
            {
                return;
            }

            if (!target.activeSelf)
            {
                target.SetActive(true);
            }

            var group = EnsureCanvasGroup(target);
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        public static VerticalLayoutGroup EnsureVerticalLayout(GameObject target, float spacing, bool controlHeight = true)
        {
            var layout = target.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = target.AddComponent<VerticalLayoutGroup>();
            }

            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = controlHeight;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;
            return layout;
        }

        public static HorizontalLayoutGroup EnsureHorizontalLayout(GameObject target, float spacing)
        {
            var layout = target.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = target.AddComponent<HorizontalLayoutGroup>();
            }

            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleLeft;
            return layout;
        }

        public static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        public static void Anchor(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPosition)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;
        }

        public static Image EnsureButtonBackground(Button button, Color backgroundColor)
        {
            var image = button.GetComponent<Image>();
            if (image == null)
            {
                image = button.gameObject.AddComponent<Image>();
            }

            if (image.sprite == null)
            {
                image.sprite = ResolveDefaultSprite();
            }

            image.type = Image.Type.Sliced;
            image.color = backgroundColor;
            image.raycastTarget = true;
            button.targetGraphic = image;

            var colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = Color.Lerp(backgroundColor, UIDesignTokens.Colors.AccentPrimary, 0.25f);
            colors.pressedColor = Color.Lerp(backgroundColor, UIDesignTokens.Colors.AccentPrimary, 0.45f);
            colors.disabledColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.35f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            return image;
        }

        public static Text EnsureButtonLabel(Button button, Font font, string label, Color backgroundColor)
        {
            EnsureButtonBackground(button, backgroundColor);

            var labelText = EnsureText(button.transform, "Label", font, UIDesignTokens.Type.HeadingSm, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextPrimary);
            Stretch(labelText.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 4f), new Vector2(-10f, -4f));
            labelText.text = label;

            return labelText;
        }

        public static void ForceTextHierarchySize(Transform root, int fontSize)
        {
            if (root == null || fontSize <= 0)
            {
                return;
            }

            var legacyTexts = root.GetComponentsInChildren<Text>(true);
            for (var i = 0; i < legacyTexts.Length; i++)
            {
                var text = legacyTexts[i];
                if (text == null)
                {
                    continue;
                }

                text.fontSize = fontSize;
                if (text.resizeTextForBestFit)
                {
                    text.resizeTextMinSize = fontSize;
                    text.resizeTextMaxSize = fontSize;
                }
            }

            var tmpTexts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (var i = 0; i < tmpTexts.Length; i++)
            {
                var text = tmpTexts[i];
                if (text == null)
                {
                    continue;
                }

                text.enableAutoSizing = false;
                text.fontSize = fontSize;
                text.fontSizeMin = fontSize;
                text.fontSizeMax = fontSize;
            }
        }

        private static Sprite ResolveDefaultSprite()
        {
            if (cachedDefaultSprite != null)
            {
                return cachedDefaultSprite;
            }

#if UNITY_EDITOR
            cachedDefaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (cachedDefaultSprite != null)
            {
                return cachedDefaultSprite;
            }

            cachedDefaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            if (cachedDefaultSprite != null)
            {
                return cachedDefaultSprite;
            }
#endif

            cachedDefaultSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            if (cachedDefaultSprite != null)
            {
                return cachedDefaultSprite;
            }

            cachedDefaultSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            if (cachedDefaultSprite != null)
            {
                return cachedDefaultSprite;
            }

            var texture = Texture2D.whiteTexture;
            cachedDefaultSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            cachedDefaultSprite.name = "GeneratedDefaultUiSprite";
            return cachedDefaultSprite;
        }
    }
}

