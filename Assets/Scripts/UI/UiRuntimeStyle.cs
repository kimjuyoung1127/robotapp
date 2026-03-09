using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    internal static class UiRuntimeStyle
    {
        public static readonly Color CanvasBackdrop = new Color(0.08f, 0.09f, 0.14f, 0.00f);
        public static readonly Color PanelBackground = new Color(0.10f, 0.11f, 0.17f, 0.92f);
        public static readonly Color PanelBackgroundAlt = new Color(0.13f, 0.14f, 0.22f, 0.94f);
        public static readonly Color CardBackground = new Color(0.16f, 0.18f, 0.28f, 0.95f);
        public static readonly Color AccentBlue = new Color(0.29f, 0.56f, 0.85f, 1f);
        public static readonly Color AccentYellow = new Color(0.95f, 0.77f, 0.15f, 1f);
        public static readonly Color BorderSoft = new Color(0.29f, 0.56f, 0.85f, 0.18f);
        public static readonly Color TextPrimary = new Color(0.92f, 0.93f, 0.96f, 1f);
        public static readonly Color TextSecondary = new Color(0.72f, 0.76f, 0.84f, 1f);
        public static readonly Color TextMuted = new Color(0.55f, 0.60f, 0.72f, 1f);
        public static readonly Color DangerMuted = new Color(0.42f, 0.18f, 0.15f, 0.92f);

        public static Font ResolveFont(Font fallback)
        {
            if (fallback != null)
            {
                return fallback;
            }

            var arial = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (arial != null)
            {
                return arial;
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

        public static Text EnsureButtonLabel(Button button, Font font, string label, Color backgroundColor)
        {
            var image = button.GetComponent<Image>();
            if (image == null)
            {
                image = button.gameObject.AddComponent<Image>();
            }

            image.color = backgroundColor;

            var labelText = EnsureText(button.transform, "Label", font, 15, FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);
            Stretch(labelText.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 4f), new Vector2(-10f, -4f));
            labelText.text = label;

            var colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = Color.Lerp(backgroundColor, AccentBlue, 0.25f);
            colors.pressedColor = Color.Lerp(backgroundColor, AccentBlue, 0.45f);
            colors.disabledColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.35f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            return labelText;
        }
    }
}
