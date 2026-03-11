// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using KineTutor3D.App;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 2DOF joint slider 옆에 숫자 입력 필드를 제공하고 슬라이더와 동기화합니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class JointInputRail : MonoBehaviour
    {
        [SerializeField] private AppController appController;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Slider jointSlider1;
        [SerializeField] private Slider jointSlider2;
        [SerializeField] private InputField jointInput1;
        [SerializeField] private InputField jointInput2;
        [SerializeField] private bool railVisible = true;

        private bool inputListenersBound;
        private int focusedJoint = -2;
        private float lastSlider1Value = float.NaN;
        private float lastSlider2Value = float.NaN;

        public InputField JointInput1 => jointInput1;
        public InputField JointInput2 => jointInput2;
        public bool IsRailVisible => railVisible;

        private void Awake()
        {
            EnsurePresentation();
        }

        private void OnEnable()
        {
            EnsurePresentation();
            BindInputEvents();
            RefreshFromSliders(force: true);
            ApplyRailVisibility();
        }

        private void OnDisable()
        {
            UnbindInputEvents();
        }

        private void Update()
        {
            EnsurePresentation();
            RefreshFromSliders(force: false);
            UpdateFocusedJoint();
        }

        public void Bind(AppController owner)
        {
            appController = owner;
            EnsurePresentation();
            RefreshFromSliders(force: true);
        }

        public void SetRailVisible(bool visible)
        {
            railVisible = visible;
            ApplyRailVisibility();

            if (!visible)
            {
                focusedJoint = -1;
                appController?.ClearJointFocus();
            }
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            jointSlider1 ??= GameObject.Find("joint_slider_1")?.GetComponent<Slider>();
            jointSlider2 ??= GameObject.Find("joint_slider_2")?.GetComponent<Slider>();

            if (jointSlider1 != null)
            {
                jointInput1 = jointInput1 == null
                    ? EnsureInputField(jointSlider1.transform, "JointInputField", new Vector2(-58f, 14f))
                    : jointInput1;
            }

            if (jointSlider2 != null)
            {
                jointInput2 = jointInput2 == null
                    ? EnsureInputField(jointSlider2.transform, "JointInputField", new Vector2(-58f, 14f))
                    : jointInput2;
            }
        }

        private InputField EnsureInputField(Transform parent, string name, Vector2 anchoredPosition)
        {
            var existing = parent.Find(name) as RectTransform;
            RectTransform rect;
            InputField inputField;

            if (existing == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
                go.transform.SetParent(parent, false);
                rect = go.GetComponent<RectTransform>();
                inputField = go.GetComponent<InputField>();
            }
            else
            {
                rect = existing;
                inputField = existing.GetComponent<InputField>() ?? existing.gameObject.AddComponent<InputField>();
            }

            UiRuntimeStyle.Anchor(rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(68f, 24f), anchoredPosition);

            var background = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            background.color = UiRuntimeStyle.CardBackground;
            background.raycastTarget = true;

            var text = UiRuntimeStyle.EnsureText(rect, "Text", fallbackFont, 12, FontStyle.Normal, TextAnchor.MiddleCenter, UiRuntimeStyle.TextPrimary);
            UiRuntimeStyle.Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 4f), new Vector2(-6f, -4f));
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = true;

            var placeholder = UiRuntimeStyle.EnsureText(rect, "Placeholder", fallbackFont, 12, FontStyle.Italic, TextAnchor.MiddleCenter, UiRuntimeStyle.TextMuted);
            UiRuntimeStyle.Stretch(placeholder.rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 4f), new Vector2(-6f, -4f));
            placeholder.text = "deg";

            inputField.textComponent = text;
            inputField.placeholder = placeholder;
            inputField.lineType = InputField.LineType.SingleLine;
            inputField.contentType = InputField.ContentType.Standard;
            inputField.characterValidation = InputField.CharacterValidation.None;
            inputField.transition = Selectable.Transition.ColorTint;

            var unitText = UiRuntimeStyle.EnsureText(rect, "UnitLabel", fallbackFont, 10, FontStyle.Bold, TextAnchor.MiddleCenter, UiRuntimeStyle.AccentYellow);
            UiRuntimeStyle.Anchor(unitText.rectTransform, new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(26f, 18f), new Vector2(8f, 0f));
            unitText.text = "deg";

            return inputField;
        }

        private void BindInputEvents()
        {
            if (inputListenersBound)
            {
                return;
            }

            if (jointInput1 != null)
            {
                jointInput1.onEndEdit.AddListener(OnJoint1EndEdit);
            }

            if (jointInput2 != null)
            {
                jointInput2.onEndEdit.AddListener(OnJoint2EndEdit);
            }

            inputListenersBound = true;
        }

        private void UnbindInputEvents()
        {
            if (!inputListenersBound)
            {
                return;
            }

            if (jointInput1 != null)
            {
                jointInput1.onEndEdit.RemoveListener(OnJoint1EndEdit);
            }

            if (jointInput2 != null)
            {
                jointInput2.onEndEdit.RemoveListener(OnJoint2EndEdit);
            }

            inputListenersBound = false;
        }

        private void OnJoint1EndEdit(string raw)
        {
            ApplyRawValue(0, jointSlider1, jointInput1, raw);
        }

        private void OnJoint2EndEdit(string raw)
        {
            ApplyRawValue(1, jointSlider2, jointInput2, raw);
        }

        private void ApplyRawValue(int jointIndex, Slider slider, InputField inputField, string raw)
        {
            if (slider == null || inputField == null || appController == null)
            {
                return;
            }

            if (!JointInputValidator.TryParseDegrees(raw, slider.minValue, slider.maxValue, out var parsed, out _))
            {
                inputField.SetTextWithoutNotify(JointInputValidator.FormatDegrees(slider.value));
                return;
            }

            appController.SetJointAngleDegrees(jointIndex, parsed);
            inputField.SetTextWithoutNotify(JointInputValidator.FormatDegrees(parsed));
            slider.SetValueWithoutNotify(parsed);
        }

        private void RefreshFromSliders(bool force)
        {
            RefreshInputFromSlider(jointSlider1, jointInput1, ref lastSlider1Value, force);
            RefreshInputFromSlider(jointSlider2, jointInput2, ref lastSlider2Value, force);
        }

        private void RefreshInputFromSlider(Slider slider, InputField inputField, ref float lastValue, bool force)
        {
            if (slider == null || inputField == null)
            {
                return;
            }

            var isEditing = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.transform.IsChildOf(inputField.transform);
            if (!force && (Mathf.Approximately(slider.value, lastValue) || isEditing))
            {
                return;
            }

            inputField.SetTextWithoutNotify(JointInputValidator.FormatDegrees(slider.value));
            lastValue = slider.value;
        }

        private void UpdateFocusedJoint()
        {
            var current = ResolveFocusedJoint();
            if (current == focusedJoint)
            {
                return;
            }

            focusedJoint = current;
            if (focusedJoint >= 0)
            {
                appController?.RequestJointFocus(focusedJoint);
            }
            else
            {
                appController?.ClearJointFocus();
            }
        }

        private int ResolveFocusedJoint()
        {
            if (EventSystem.current?.currentSelectedGameObject == null)
            {
                return -1;
            }

            var selected = EventSystem.current.currentSelectedGameObject.transform;
            if (jointSlider1 != null && selected.IsChildOf(jointSlider1.transform))
            {
                return 0;
            }

            if (jointSlider2 != null && selected.IsChildOf(jointSlider2.transform))
            {
                return 1;
            }

            if (jointInput1 != null && selected.IsChildOf(jointInput1.transform))
            {
                return 0;
            }

            if (jointInput2 != null && selected.IsChildOf(jointInput2.transform))
            {
                return 1;
            }

            return -1;
        }

        private void ApplyRailVisibility()
        {
            if (jointInput1 != null)
            {
                jointInput1.gameObject.SetActive(railVisible);
            }

            if (jointInput2 != null)
            {
                jointInput2.gameObject.SetActive(railVisible);
            }
        }
    }
}
