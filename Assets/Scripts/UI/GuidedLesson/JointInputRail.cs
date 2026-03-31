// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections.Generic;
using KineTutor3D.App;
using KineTutor3D.Math;
using KineTutor3D.Types;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Joint slider와 numeric input을 통합 관리합니다. 2DOF 기본 레일을 유지하면서
    /// 추가 DOF가 있는 템플릿은 동적으로 row를 더 생성합니다.
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
        [SerializeField] private RectTransform extraRowsRoot;
        [SerializeField] private bool railVisible = true;
        [SerializeField] private int interactiveJointCount;

        private readonly List<Slider> extraSliders = new List<Slider>();
        private readonly List<InputField> extraInputs = new List<InputField>();
        private bool inputListenersBound;
        private bool suppressRuntimeCallbacks;
        private int focusedJoint = -2;
        private float lastSlider1Value = float.NaN;
        private float lastSlider2Value = float.NaN;

        public InputField JointInput1 => jointInput1;
        public InputField JointInput2 => jointInput2;
        public bool IsRailVisible => railVisible;

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
        }

        private void OnEnable()
        {
            EnsurePresentation();
            BindInputEvents();
            RebuildForCurrentTemplate();
            RefreshFromRuntime(force: true);
            ApplyRailVisibility();
        }

        private void OnDisable()
        {
            UnbindInputEvents();
            UnbindAppController();
        }

        private void Update()
        {
            EnsurePresentation();
            RefreshFromRuntime(force: false);
            UpdateFocusedJoint();
        }

        public void Bind(AppController owner)
        {
            UnbindAppController();
            appController = owner;

            if (appController != null)
            {
                appController.OnTemplateChanged += HandleTemplateChanged;
                appController.OnKinematicsUpdated += HandleKinematicsUpdated;
            }

            EnsurePresentation();
            RebuildForCurrentTemplate();
            RefreshFromRuntime(force: true);
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

        public void SetInteractiveJointCount(int count)
        {
            interactiveJointCount = Mathf.Max(0, count);
            ApplyRailVisibility();
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            jointSlider1 ??= FindSliderInHierarchy("joint_slider_1");
            jointSlider2 ??= FindSliderInHierarchy("joint_slider_2");

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

            if (extraRowsRoot == null)
            {
                var root = transform as RectTransform;
                extraRowsRoot = UiRuntimeStyle.EnsureRectChild(root != null ? root : transform, "ExtraJointRows");
                UiRuntimeStyle.EnsureVerticalLayout(extraRowsRoot.gameObject, UIDesignTokens.Space.Xs, false);
                UiRuntimeStyle.Anchor(extraRowsRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(316f, 112f), new Vector2(870f, -8f));
            }
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

        private void UnbindAppController()
        {
            if (appController == null)
            {
                return;
            }

            appController.OnTemplateChanged -= HandleTemplateChanged;
            appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
        }

        private void HandleTemplateChanged(RobotTemplate _)
        {
            RebuildForCurrentTemplate();
            RefreshFromRuntime(force: true);
        }

        private void HandleKinematicsUpdated(Mat4D _a1, Mat4D _a2, Mat4D _t02, TutorPose _pose)
        {
            RefreshFromRuntime(force: false);
        }

        private void RebuildForCurrentTemplate()
        {
            if (extraRowsRoot == null)
            {
                return;
            }

            for (int i = extraRowsRoot.childCount - 1; i >= 0; i--)
            {
                var child = extraRowsRoot.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }

            extraSliders.Clear();
            extraInputs.Clear();

            var dof = appController != null ? appController.CurrentDof : 0;
            for (int jointIndex = 2; jointIndex < dof; jointIndex++)
            {
                CreateExtraRow(jointIndex);
            }

            ApplyRailVisibility();
        }

        private void CreateExtraRow(int jointIndex)
        {
            var row = UiRuntimeStyle.EnsureRectChild(extraRowsRoot, $"JointRow_{jointIndex + 1}");
            var bg = row.GetComponent<Image>() ?? row.gameObject.AddComponent<Image>();
            bg.color = UIDesignTokens.Colors.SurfaceCard;
            UiRuntimeStyle.EnsureLayoutElement(row).preferredHeight = 52f;

            var label = UiRuntimeStyle.EnsureText(row, "Label", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(42f, 20f), new Vector2(24f, 0f));
            label.text = $"J{jointIndex + 1}";

            var sliderRect = UiRuntimeStyle.EnsureRectChild(row, "Slider");
            UiRuntimeStyle.Anchor(sliderRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(176f, UIDesignTokens.Size.SliderHeight), new Vector2(136f, 0f));
            var sliderImage = sliderRect.GetComponent<Image>() ?? sliderRect.gameObject.AddComponent<Image>();
            sliderImage.color = UIDesignTokens.Colors.SliderTrack;

            var slider = sliderRect.GetComponent<Slider>() ?? sliderRect.gameObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.wholeNumbers = false;

            var fillArea = UiRuntimeStyle.EnsureRectChild(sliderRect, "Fill Area");
            UiRuntimeStyle.Stretch(fillArea, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(8f, 8f), new Vector2(-8f, -8f));
            var fill = UiRuntimeStyle.EnsureImage(fillArea, "Fill", UIDesignTokens.Colors.AccentPrimary);
            UiRuntimeStyle.Stretch(fill.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            slider.fillRect = fill.rectTransform;

            var handleArea = UiRuntimeStyle.EnsureRectChild(sliderRect, "Handle Slide Area");
            UiRuntimeStyle.Stretch(handleArea, Vector2.zero, Vector2.one, new Vector2(8f, 6f), new Vector2(-8f, -6f));
            var handle = UiRuntimeStyle.EnsureImage(handleArea, "Handle", UIDesignTokens.Colors.AccentSecondary);
            UiRuntimeStyle.Anchor(handle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(16f, 16f), Vector2.zero);
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;

            var input = EnsureInputField(row, "JointInputField", new Vector2(-58f, 0f));

            var limit = appController != null ? appController.GetJointLimit(jointIndex) : new JointLimit(-Mathf.PI, Mathf.PI);
            slider.minValue = (float)(limit.Min * Mathf.Rad2Deg);
            slider.maxValue = (float)(limit.Max * Mathf.Rad2Deg);

            var capturedIndex = jointIndex;
            slider.onValueChanged.AddListener(value =>
            {
                if (suppressRuntimeCallbacks || appController == null)
                {
                    return;
                }

                appController.SetJointAngleDegrees(capturedIndex, value);
                appController.ReportInteraction(InteractionType.SliderChange, $"joint_slider_{capturedIndex + 1}");
                input.SetTextWithoutNotify(JointInputValidator.FormatDegrees(value));
            });

            input.onEndEdit.AddListener(raw =>
            {
                if (appController == null)
                {
                    return;
                }

                if (!JointInputValidator.TryParseDegrees(raw, slider.minValue, slider.maxValue, out var parsed, out _))
                {
                    input.SetTextWithoutNotify(JointInputValidator.FormatDegrees(slider.value));
                    return;
                }

                suppressRuntimeCallbacks = true;
                slider.SetValueWithoutNotify(parsed);
                suppressRuntimeCallbacks = false;
                appController.SetJointAngleDegrees(capturedIndex, parsed);
                appController.ReportInteraction(InteractionType.SliderChange, $"joint_slider_{capturedIndex + 1}");
                input.SetTextWithoutNotify(JointInputValidator.FormatDegrees(parsed));
            });

            extraSliders.Add(slider);
            extraInputs.Add(input);
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

            UiRuntimeStyle.Anchor(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(68f, 24f), anchoredPosition);

            var background = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            background.color = UIDesignTokens.Colors.SurfaceCard;
            background.raycastTarget = true;

            var text = UiRuntimeStyle.EnsureText(rect, "Text", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 4f), new Vector2(-6f, -4f));
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = true;

            var placeholder = UiRuntimeStyle.EnsureText(rect, "Placeholder", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Italic, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Stretch(placeholder.rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 4f), new Vector2(-6f, -4f));
            placeholder.text = "deg";

            inputField.textComponent = text;
            inputField.placeholder = placeholder;
            inputField.lineType = InputField.LineType.SingleLine;
            inputField.contentType = InputField.ContentType.Standard;
            inputField.characterValidation = InputField.CharacterValidation.None;
            inputField.transition = Selectable.Transition.ColorTint;

            var unitText = UiRuntimeStyle.EnsureText(rect, "UnitLabel", fallbackFont, UIDesignTokens.Type.Tiny, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.Colors.AccentSecondary);
            unitText.text = "deg";
            unitText.gameObject.SetActive(false);

            return inputField;
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

        private void RefreshFromRuntime(bool force)
        {
            RefreshInputFromSlider(jointSlider1, jointInput1, ref lastSlider1Value, force);
            RefreshInputFromSlider(jointSlider2, jointInput2, ref lastSlider2Value, force);

            if (appController == null)
            {
                return;
            }

            suppressRuntimeCallbacks = true;
            for (int i = 0; i < extraSliders.Count; i++)
            {
                var jointIndex = i + 2;
                var value = (float)appController.GetJointAngleDegrees(jointIndex);
                extraSliders[i].SetValueWithoutNotify(value);
                extraInputs[i].SetTextWithoutNotify(JointInputValidator.FormatDegrees(value));
            }
            suppressRuntimeCallbacks = false;
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

            for (int i = 0; i < extraSliders.Count; i++)
            {
                if (selected.IsChildOf(extraSliders[i].transform) || selected.IsChildOf(extraInputs[i].transform))
                {
                    return i + 2;
                }
            }

            return -1;
        }

        private void ApplyRailVisibility()
        {
            var allowJoint1 = interactiveJointCount == 0 || interactiveJointCount >= 1;
            var allowJoint2 = interactiveJointCount == 0 || interactiveJointCount >= 2;

            SetBaseJointVisible(jointSlider1, jointInput1, railVisible && allowJoint1);
            SetBaseJointVisible(jointSlider2, jointInput2, railVisible && allowJoint2);

            for (int i = 0; i < extraSliders.Count; i++)
            {
                var jointIndex = i + 3;
                var allow = interactiveJointCount == 0 || jointIndex <= interactiveJointCount;
                extraSliders[i].gameObject.SetActive(railVisible && allow);
                extraInputs[i].gameObject.SetActive(railVisible && allow);
                if (extraSliders[i].transform.parent != null)
                {
                    extraSliders[i].transform.parent.gameObject.SetActive(railVisible && allow);
                }
            }

            if (extraRowsRoot != null)
            {
                var anyActive = false;
                for (int i = 0; i < extraRowsRoot.childCount; i++)
                {
                    if (extraRowsRoot.GetChild(i).gameObject.activeSelf)
                    {
                        anyActive = true;
                        break;
                    }
                }

                extraRowsRoot.gameObject.SetActive(railVisible && anyActive);
            }
        }

        private static Slider FindSliderInHierarchy(string name)
        {
            var canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null)
            {
                return null;
            }

            foreach (var slider in canvas.GetComponentsInChildren<Slider>(true))
            {
                if (slider.gameObject.name == name)
                {
                    return slider;
                }
            }

            return null;
        }

        private static void SetBaseJointVisible(Slider slider, InputField inputField, bool visible)
        {
            if (slider != null)
            {
                slider.gameObject.SetActive(visible);
            }

            if (inputField != null)
            {
                inputField.gameObject.SetActive(visible);
            }
        }
    }
}
