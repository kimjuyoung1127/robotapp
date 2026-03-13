// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    internal static class DHTableViewBuilder
    {
        public static void CreateHeaderRow(RectTransform tableRoot, Font fallbackFont)
        {
            var header = new GameObject("DHHeaderRow", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            header.transform.SetParent(tableRoot, false);
            header.GetComponent<Image>().color = UIDesignTokens.Colors.SurfaceCard;
            header.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 34f);
            var layout = UiRuntimeStyle.EnsureHorizontalLayout(header, 6f);
            layout.padding = new RectOffset(10, 10, 6, 6);
            CreateHeaderLabel(header.transform, fallbackFont, "Joint", 64f);
            CreateHeaderLabel(header.transform, fallbackFont, "theta", 70f);
            CreateHeaderLabel(header.transform, fallbackFont, "d", 58f);
            CreateHeaderLabel(header.transform, fallbackFont, "a", 58f);
            CreateHeaderLabel(header.transform, fallbackFont, "alpha", 70f);
        }

        public static DHTableRowRefs CreateDataRow(RectTransform tableRoot, Font fallbackFont, int rowIndex, Action<string> onDChanged, Action<string> onAChanged, Action<string> onAlphaChanged)
        {
            var row = new GameObject($"DHRow_{rowIndex}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(tableRoot, false);
            row.GetComponent<Image>().color = UIDesignTokens.Colors.SurfaceRaisedAlt;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 38f);
            var layout = UiRuntimeStyle.EnsureHorizontalLayout(row, 6f);
            layout.padding = new RectOffset(10, 10, 6, 6);

            var refs = new DHTableRowRefs { Index = rowIndex };
            refs.JointType = CreateReadOnlyText(row.transform, fallbackFont, $"JointType_{rowIndex}", 64f);
            refs.Theta = CreateInput(row.transform, fallbackFont, $"ThetaInput_{rowIndex}", false, 70f);
            refs.D = CreateInput(row.transform, fallbackFont, $"DInput_{rowIndex}", true, 58f);
            refs.A = CreateInput(row.transform, fallbackFont, $"AInput_{rowIndex}", true, 58f);
            refs.Alpha = CreateInput(row.transform, fallbackFont, $"AlphaInput_{rowIndex}", true, 70f);
            refs.D.onEndEdit.AddListener(value => onDChanged?.Invoke(value));
            refs.A.onEndEdit.AddListener(value => onAChanged?.Invoke(value));
            refs.Alpha.onEndEdit.AddListener(value => onAlphaChanged?.Invoke(value));
            return refs;
        }

        private static Text CreateHeaderLabel(Transform parent, Font fallbackFont, string text, float width)
        {
            var label = UiRuntimeStyle.EnsureText(parent, $"Header_{text}", fallbackFont, 13, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextSecondary);
            var element = UiRuntimeStyle.EnsureLayoutElement(label);
            element.minWidth = width;
            element.preferredWidth = width;
            label.text = text;
            return label;
        }

        private static Text CreateReadOnlyText(Transform parent, Font fallbackFont, string name, float width)
        {
            var text = UiRuntimeStyle.EnsureText(parent, name, fallbackFont, 12, FontStyle.Normal, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextPrimary);
            var element = UiRuntimeStyle.EnsureLayoutElement(text);
            element.minWidth = width;
            element.preferredWidth = width;
            text.text = "-";
            return text;
        }

        private static InputField CreateInput(Transform parent, Font fallbackFont, string name, bool interactable, float width)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            root.transform.SetParent(parent, false);
            var input = root.GetComponent<InputField>();
            var element = UiRuntimeStyle.EnsureLayoutElement(input);
            element.minWidth = width;
            element.preferredWidth = width;
            var image = root.GetComponent<Image>();
            image.color = interactable ? UIDesignTokens.Colors.SurfaceRaisedAlt : UIDesignTokens.Colors.SurfaceInput;
            input.interactable = interactable;
            input.contentType = InputField.ContentType.DecimalNumber;
            input.lineType = InputField.LineType.SingleLine;
            var text = UiRuntimeStyle.EnsureText(root.transform, "Text", fallbackFont, 12, FontStyle.Normal, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 1f), new Vector2(-4f, -1f));
            input.textComponent = text;
            input.SetTextWithoutNotify("0");
            return input;
        }
    }
}
