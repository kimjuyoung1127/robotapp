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
    public class TemplateSelector : MonoBehaviour
    {
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private Font fallbackFont;

        private AppController appController;
        private readonly List<string> optionNames = new List<string>();
        private bool suppressCallback;

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
            if (fallbackFont == null)
            {
                fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            if (dropdown == null)
            {
                var existing = GameObject.Find("TemplateSelectorDropdown");
                if (existing != null)
                {
                    dropdown = existing.GetComponent<Dropdown>();
                }
            }

            if (dropdown != null)
            {
                return;
            }

            var root = new GameObject("TemplateSelectorDropdown", typeof(RectTransform), typeof(Image), typeof(Dropdown));
            root.transform.SetParent(transform, false);

            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(220f, 30f);
            rect.anchoredPosition = new Vector2(-16f, 0f);

            var image = root.GetComponent<Image>();
            image.color = new Color(0.20f, 0.20f, 0.30f, 0.95f);

            dropdown = root.GetComponent<Dropdown>();
            dropdown.targetGraphic = image;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(root.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 2f);
            labelRect.offsetMax = new Vector2(-22f, -2f);

            var label = labelGo.GetComponent<Text>();
            label.font = fallbackFont;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            dropdown.captionText = label;
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
