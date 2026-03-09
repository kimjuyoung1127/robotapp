using System;
using System.Collections.Generic;
using KineTutor3D.App;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 로봇 템플릿 드롭다운을 렌더하고 선택을 AppController에 전달합니다.
    /// </summary>
    [ExecuteAlways]
    public class TemplateSelector : MonoBehaviour
    {
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Text titleText;
        [SerializeField] private Button glossaryButton;
        [SerializeField] private Graphic topBarBackground;

        private AppController appController;
        private readonly List<string> optionNames = new List<string>();
        private bool suppressCallback;

        private void OnEnable()
        {
            EnsureDropdown();
        }

        public int OptionCount => dropdown != null ? dropdown.options.Count : 0;

        public void Bind(AppController owner)
        {
            UnbindCurrent();
            appController = owner;

            EnsureDropdown();
            RebuildOptions();

            if (dropdown != null)
            {
                dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
                dropdown.onValueChanged.AddListener(OnDropdownChanged);
            }

            if (appController != null)
            {
                appController.OnTemplateChanged += HandleTemplateChanged;
            }
        }

        private void OnDestroy()
        {
            UnbindCurrent();

            if (dropdown != null)
            {
                dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
            }
        }

        public void SelectByIndex(int index)
        {
            if (dropdown == null || index < 0 || index >= optionNames.Count)
            {
                return;
            }

            dropdown.SetValueWithoutNotify(index);
            OnDropdownChanged(index);
        }

        private void HandleTemplateChanged(RobotTemplate template)
        {
            if (dropdown == null || template == null)
            {
                return;
            }

            var index = optionNames.FindIndex(name => string.Equals(name, template.Name, StringComparison.Ordinal));
            if (index < 0)
            {
                return;
            }

            suppressCallback = true;
            dropdown.SetValueWithoutNotify(index);
            suppressCallback = false;
        }

        private void OnDropdownChanged(int index)
        {
            if (suppressCallback || appController == null || index < 0 || index >= optionNames.Count)
            {
                return;
            }

            appController.SelectTemplateByName(optionNames[index]);
        }

        private void EnsureDropdown()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureTopBarSurface();

            if (dropdown == null)
            {
                var existing = GameObject.Find("TemplateSelectorDropdown");
                if (existing != null)
                {
                    dropdown = existing.GetComponent<Dropdown>();
                }
            }

            if (dropdown == null)
            {
                var root = new GameObject("TemplateSelectorDropdown", typeof(RectTransform), typeof(Image), typeof(Dropdown));
                root.transform.SetParent(transform, false);
                dropdown = root.GetComponent<Dropdown>();

                var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
                labelGo.transform.SetParent(root.transform, false);
                dropdown.captionText = labelGo.GetComponent<Text>();
            }

            var rect = dropdown.transform as RectTransform;
            UiRuntimeStyle.Anchor(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(224f, 36f), new Vector2(-74f, 0f));

            var image = dropdown.GetComponent<Image>();
            if (image == null)
            {
                image = dropdown.gameObject.AddComponent<Image>();
            }

            image.color = UiRuntimeStyle.CardBackground;
            dropdown.targetGraphic = image;

            var label = dropdown.captionText;
            if (label == null)
            {
                label = UiRuntimeStyle.EnsureText(dropdown.transform, "Label", fallbackFont, 14, FontStyle.Normal, TextAnchor.MiddleLeft, UiRuntimeStyle.TextPrimary);
                dropdown.captionText = label;
            }

            label.font = fallbackFont;
            label.fontSize = 14;
            label.color = UiRuntimeStyle.TextPrimary;
            label.alignment = TextAnchor.MiddleLeft;
            UiRuntimeStyle.Stretch(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 4f), new Vector2(-24f, -4f));
        }

        private void EnsureTopBarSurface()
        {
            var rect = transform as RectTransform;
            if (rect != null)
            {
                UiRuntimeStyle.Stretch(rect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -76f), new Vector2(-16f, -16f));
            }

            if (topBarBackground == null)
            {
                topBarBackground = UiRuntimeStyle.EnsureImage(transform, "TopBarBackground", UiRuntimeStyle.PanelBackgroundAlt);
            }

            UiRuntimeStyle.Stretch((RectTransform)topBarBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            titleText ??= GameObject.Find("TitleText")?.GetComponent<Text>();
            if (titleText == null)
            {
                titleText = UiRuntimeStyle.EnsureText(transform, "TitleText", fallbackFont, 24, FontStyle.Bold, TextAnchor.MiddleLeft, UiRuntimeStyle.TextPrimary);
            }

            UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(220f, 40f), new Vector2(26f, 0f));
            titleText.text = "KineTutor3D";
            UiRuntimeStyle.EnsureOutline(titleText, new Color(0f, 0f, 0f, 0.28f), new Vector2(1f, -1f));

            var stepIndicator = GameObject.Find("StepIndicatorText")?.GetComponent<Text>();
            if (stepIndicator != null)
            {
                stepIndicator.font = fallbackFont;
                stepIndicator.fontSize = 16;
                stepIndicator.color = UiRuntimeStyle.TextSecondary;
                stepIndicator.alignment = TextAnchor.MiddleLeft;
                UiRuntimeStyle.Anchor(stepIndicator.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(280f, 28f), new Vector2(250f, 0f));
            }

            glossaryButton ??= GameObject.Find("BtnGlossaryOpen")?.GetComponent<Button>();
            if (glossaryButton != null)
            {
                UiRuntimeStyle.Anchor(glossaryButton.transform as RectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(40f, 36f), new Vector2(-24f, 0f));
                UiRuntimeStyle.EnsureButtonLabel(glossaryButton, fallbackFont, "?", UiRuntimeStyle.AccentBlue);
            }
        }

        private void RebuildOptions()
        {
            optionNames.Clear();

            if (dropdown == null || appController == null)
            {
                return;
            }

            optionNames.AddRange(appController.GetAvailableTemplateNames());
            dropdown.ClearOptions();
            dropdown.AddOptions(optionNames);

            var selected = 0;
            if (appController.CurrentTemplate != null)
            {
                var index = optionNames.FindIndex(x => string.Equals(x, appController.CurrentTemplate.Name, StringComparison.Ordinal));
                if (index >= 0)
                {
                    selected = index;
                }
            }

            suppressCallback = true;
            dropdown.SetValueWithoutNotify(selected);
            suppressCallback = false;
        }

        private void UnbindCurrent()
        {
            if (appController != null)
            {
                appController.OnTemplateChanged -= HandleTemplateChanged;
                appController = null;
            }
        }
    }
}
