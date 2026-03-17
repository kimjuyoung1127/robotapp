// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using System.Collections.Generic;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Robot Library에서 선택된 로봇의 관찰/직접 조작/명시적 진입 CTA를 제공합니다.
    /// </summary>
    public class RobotLibrarySelectionPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;

        private RectTransform panelRoot;
        private Text titleText;
        private Text subtitleText;
        private Text descriptionText;
        private Text capabilityText;
        private Text helperText;
        private Text controlStateText;
        private Button guidedLessonButton;
        private Button sandboxButton;
        private Button robotControlButton;
        private Button resetPoseButton;
        private Button demoPoseButton;
        private readonly List<Slider> jointSliders = new List<Slider>();
        private readonly List<InputField> jointInputs = new List<InputField>();
        private readonly List<bool> jointIsRotational = new List<bool>();
        private RobotCatalogEntry currentEntry;
        private bool suppressCallbacks;

        public event Action OnGuidedLessonRequested;
        public event Action OnSandboxRequested;
        public event Action OnRobotControlRequested;
        public event Action<double[]> OnPosePreviewChanged;
        public event Action OnResetPoseRequested;
        public event Action OnDemoPoseRequested;

        public void Initialize(RectTransform parent, Font font)
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(font);
            EnsurePresentation(parent);
            SetVisible(true);
        }

        public void ShowRobot(RobotCatalogEntry entry, double[] poseDeg, float[] minLimits, float[] maxLimits, bool[] isRotational)
        {
            currentEntry = entry;
            EnsurePresentation(transform.parent as RectTransform);

            if (entry == null)
            {
                titleText.text = "Robot Preview";
                subtitleText.text = "Select a robot";
                descriptionText.text = "카드나 3D showroom에서 로봇을 선택하면 여기서 관찰과 직접 조작을 이어갈 수 있습니다.";
                capabilityText.text = "지원 모드가 여기 표시됩니다.";
                helperText.text = "먼저 고르고, 움직여 보고, 그 다음 원하는 화면으로 이동하세요.";
                controlStateText.text = "선택 대기 중";
                RebuildControls(Array.Empty<float>(), Array.Empty<float>(), Array.Empty<bool>());
                RefreshActionButtons(null);
                return;
            }

            var metadata = entry.Metadata;
            var controlCount = Mathf.Min(
                minLimits != null ? minLimits.Length : 0,
                Mathf.Min(maxLimits != null ? maxLimits.Length : 0, isRotational != null ? isRotational.Length : 0));
            titleText.text = metadata.DisplayName;
            subtitleText.text = $"{metadata.Dof} DOF · {metadata.RobotType} · {metadata.Difficulty}";
            descriptionText.text = metadata.Description;
            capabilityText.text = BuildCapabilitySummary(metadata);
            helperText.text = controlCount > 0
                ? "조인트를 움직여 충분히 관찰한 뒤 학습, 샌드박스, Robot Control 중 원하는 화면으로 이동하세요."
                : "이 로봇은 현재 라이브러리에서 관찰 중심으로 보여집니다. 지원되는 화면으로 직접 이동할 수 있습니다.";
            if (entry.LibraryInteractionMode == LibraryInteractionMode.SelectOnly)
            {
                helperText.text = "이 항목은 RobotControl 템플릿 구조를 보여주는 선택 전용 카드입니다.";
            }
            controlStateText.text = controlCount > 0
                ? $"Library Control Active · {controlCount} joints"
                : "Observation Only";

            RebuildControls(minLimits, maxLimits, isRotational);
            SetPoseWithoutNotify(poseDeg);
            RefreshActionButtons(metadata);
        }

        public void SetPoseWithoutNotify(double[] poseDeg)
        {
            suppressCallbacks = true;
            for (var i = 0; i < jointSliders.Count; i++)
            {
                var value = poseDeg != null && i < poseDeg.Length ? (float)poseDeg[i] : 0f;
                jointSliders[i].SetValueWithoutNotify(value);
                jointInputs[i].SetTextWithoutNotify(JointInputValidator.FormatDegrees(value));
            }

            suppressCallbacks = false;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void EnsurePresentation(RectTransform parent)
        {
            if (panelRoot != null)
            {
                RemoveLegacyControlsRoot();
                return;
            }

            if (parent == null)
            {
                return;
            }

            panelRoot = transform as RectTransform;
            if (panelRoot == null)
            {
                panelRoot = gameObject.AddComponent<RectTransform>();
            }

            panelRoot.SetParent(parent, false);
            var background = panelRoot.GetComponent<Image>() ?? panelRoot.gameObject.AddComponent<Image>();
            background.color = UIDesignTokens.Colors.SurfaceRaised;
            RemoveLegacyControlsRoot();

            if (TryBindExistingPresentation())
            {
                return;
            }

            titleText = UiRuntimeStyle.EnsureText(panelRoot, "Title", fallbackFont, UIDesignTokens.Type.HeadingLg, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, 24f), new Vector2(16f, -16f));

            subtitleText = UiRuntimeStyle.EnsureText(panelRoot, "Subtitle", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.AccentSecondary);
            UiRuntimeStyle.Anchor(subtitleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, 18f), new Vector2(16f, -42f));

            descriptionText = UiRuntimeStyle.EnsureText(panelRoot, "Description", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(descriptionText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, 42f), new Vector2(16f, -66f));

            capabilityText = UiRuntimeStyle.EnsureText(panelRoot, "CapabilityText", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(capabilityText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, 36f), new Vector2(16f, -112f));

            helperText = UiRuntimeStyle.EnsureText(panelRoot, "HelperText", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Italic, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(helperText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, 40f), new Vector2(16f, -148f));

            var divider = UiRuntimeStyle.EnsureImage(panelRoot, "Divider", UIDesignTokens.Colors.BorderSoft);
            UiRuntimeStyle.Anchor((RectTransform)divider.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, 1f), new Vector2(16f, -192f));

            controlStateText = UiRuntimeStyle.EnsureText(panelRoot, "ControlState", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.AccentPrimary);
            UiRuntimeStyle.Anchor(controlStateText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, 18f), new Vector2(16f, -210f));

            resetPoseButton = UIComponentFactory.CreateSecondaryButton(panelRoot, "BtnResetPose", "Reset", fallbackFont, 84f);
            UiRuntimeStyle.Anchor((RectTransform)resetPoseButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(84f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f, 96f));
            resetPoseButton.onClick.RemoveAllListeners();
            resetPoseButton.onClick.AddListener(() => OnResetPoseRequested?.Invoke());

            demoPoseButton = UIComponentFactory.CreateSecondaryButton(panelRoot, "BtnDemoPose", "Demo Pose", fallbackFont, 108f);
            UiRuntimeStyle.Anchor((RectTransform)demoPoseButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(108f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(108f, 96f));
            demoPoseButton.onClick.RemoveAllListeners();
            demoPoseButton.onClick.AddListener(() => OnDemoPoseRequested?.Invoke());

            guidedLessonButton = UIComponentFactory.CreatePrimaryButton(panelRoot, "BtnGuidedLesson", "Start Lesson", fallbackFont, 112f);
            UiRuntimeStyle.Anchor((RectTransform)guidedLessonButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(112f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(16f, 48f));
            guidedLessonButton.onClick.RemoveAllListeners();
            guidedLessonButton.onClick.AddListener(() => OnGuidedLessonRequested?.Invoke());

            sandboxButton = UIComponentFactory.CreateSecondaryButton(panelRoot, "BtnOpenSandbox", "Open Sandbox", fallbackFont, 112f);
            UiRuntimeStyle.Anchor((RectTransform)sandboxButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(112f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(138f, 48f));
            sandboxButton.onClick.RemoveAllListeners();
            sandboxButton.onClick.AddListener(() => OnSandboxRequested?.Invoke());

            robotControlButton = UIComponentFactory.CreateSecondaryButton(panelRoot, "BtnRobotControl", "Robot Control", fallbackFont, 112f);
            UiRuntimeStyle.Anchor((RectTransform)robotControlButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(112f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(260f, 48f));
            robotControlButton.onClick.RemoveAllListeners();
            robotControlButton.onClick.AddListener(() => OnRobotControlRequested?.Invoke());
        }

        private bool TryBindExistingPresentation()
        {
            titleText = panelRoot.Find("Title")?.GetComponent<Text>();
            subtitleText = panelRoot.Find("Subtitle")?.GetComponent<Text>();
            descriptionText = panelRoot.Find("Description")?.GetComponent<Text>();
            capabilityText = panelRoot.Find("CapabilityText")?.GetComponent<Text>();
            helperText = panelRoot.Find("HelperText")?.GetComponent<Text>();
            controlStateText = panelRoot.Find("ControlState")?.GetComponent<Text>();
            guidedLessonButton = panelRoot.Find("BtnGuidedLesson")?.GetComponent<Button>();
            sandboxButton = panelRoot.Find("BtnOpenSandbox")?.GetComponent<Button>();
            robotControlButton = panelRoot.Find("BtnRobotControl")?.GetComponent<Button>();
            resetPoseButton = panelRoot.Find("BtnResetPose")?.GetComponent<Button>();
            demoPoseButton = panelRoot.Find("BtnDemoPose")?.GetComponent<Button>();

            if (titleText == null
                || subtitleText == null
                || descriptionText == null
                || capabilityText == null
                || helperText == null
                || controlStateText == null
                || guidedLessonButton == null
                || sandboxButton == null
                || robotControlButton == null
                || resetPoseButton == null
                || demoPoseButton == null)
            {
                return false;
            }

            resetPoseButton.onClick.RemoveAllListeners();
            resetPoseButton.onClick.AddListener(() => OnResetPoseRequested?.Invoke());
            demoPoseButton.onClick.RemoveAllListeners();
            demoPoseButton.onClick.AddListener(() => OnDemoPoseRequested?.Invoke());
            guidedLessonButton.onClick.RemoveAllListeners();
            guidedLessonButton.onClick.AddListener(() => OnGuidedLessonRequested?.Invoke());
            sandboxButton.onClick.RemoveAllListeners();
            sandboxButton.onClick.AddListener(() => OnSandboxRequested?.Invoke());
            robotControlButton.onClick.RemoveAllListeners();
            robotControlButton.onClick.AddListener(() => OnRobotControlRequested?.Invoke());

            return true;
        }

        private void RebuildControls(float[] minLimits, float[] maxLimits, bool[] isRotational)
        {
            ClearControlRows();

            jointSliders.Clear();
            jointInputs.Clear();
            jointIsRotational.Clear();

            var controlCount = Mathf.Min(
                minLimits != null ? minLimits.Length : 0,
                Mathf.Min(maxLimits != null ? maxLimits.Length : 0, isRotational != null ? isRotational.Length : 0));
            resetPoseButton.gameObject.SetActive(false);
            demoPoseButton.gameObject.SetActive(false);
        }

        private void BuildControlRow(int jointIndex, float minLimit, float maxLimit, bool isRotational)
        {
            var row = UiRuntimeStyle.EnsureRectChild(panelRoot, $"JointRow_{jointIndex + 1}");
            UiRuntimeStyle.Anchor(row, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, 30f), new Vector2(16f, -(234f + jointIndex * 34f)));

            var rowBackground = row.GetComponent<Image>() ?? row.gameObject.AddComponent<Image>();
            rowBackground.color = UIDesignTokens.Colors.SurfaceCard;

            var label = UiRuntimeStyle.EnsureText(row, "Label", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(32f, 16f), new Vector2(10f, 0f));
            label.text = isRotational ? $"J{jointIndex + 1}" : $"L{jointIndex + 1}";

            var slider = UIComponentFactory.CreateSlider(row, "Slider", minLimit, maxLimit);
            UiRuntimeStyle.Anchor((RectTransform)slider.transform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-116f, UIDesignTokens.Size.SliderHeight), new Vector2(50f, 0f));

            var input = UIComponentFactory.CreateInputField(row, "Input", "0.0", fallbackFont);
            UiRuntimeStyle.Anchor((RectTransform)input.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(-8f, 0f));

            var capturedIndex = jointIndex;
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(value => HandleSliderChanged(capturedIndex, value));
            input.onEndEdit.RemoveAllListeners();
            input.onEndEdit.AddListener(raw => HandleInputCommitted(capturedIndex, raw));

            jointSliders.Add(slider);
            jointInputs.Add(input);
            jointIsRotational.Add(isRotational);
        }

        private void RemoveLegacyControlsRoot()
        {
            if (panelRoot == null)
            {
                return;
            }

            var legacy = panelRoot.Find("ControlsRoot");
            if (legacy != null)
            {
                legacy.SetParent(null, false);
                SafeDestroy(legacy.gameObject);
            }
        }

        private void ClearControlRows()
        {
            if (panelRoot == null)
            {
                return;
            }

            var staleRows = new List<GameObject>();
            for (var i = panelRoot.childCount - 1; i >= 0; i--)
            {
                var child = panelRoot.GetChild(i);
                if (child != null && child.name.StartsWith("JointRow_", StringComparison.Ordinal))
                {
                    staleRows.Add(child.gameObject);
                    child.SetParent(null, false);
                }
            }

            for (var i = 0; i < staleRows.Count; i++)
            {
                SafeDestroy(staleRows[i]);
            }
        }

        private void HandleSliderChanged(int jointIndex, float value)
        {
            if (suppressCallbacks || jointIndex < 0 || jointIndex >= jointInputs.Count)
            {
                return;
            }

            jointInputs[jointIndex].SetTextWithoutNotify(JointInputValidator.FormatDegrees(value));
            RaisePosePreviewChanged();
        }

        private void HandleInputCommitted(int jointIndex, string raw)
        {
            if (jointIndex < 0 || jointIndex >= jointSliders.Count)
            {
                return;
            }

            var slider = jointSliders[jointIndex];
            var input = jointInputs[jointIndex];
            if (!JointInputValidator.TryParseDegrees(raw, slider.minValue, slider.maxValue, out var parsed, out _))
            {
                input.SetTextWithoutNotify(JointInputValidator.FormatDegrees(slider.value));
                return;
            }

            slider.SetValueWithoutNotify(parsed);
            input.SetTextWithoutNotify(JointInputValidator.FormatDegrees(parsed));
            RaisePosePreviewChanged();
        }

        private void RaisePosePreviewChanged()
        {
            var values = new double[jointSliders.Count];
            for (var i = 0; i < jointSliders.Count; i++)
            {
                values[i] = jointSliders[i].value;
            }

            OnPosePreviewChanged?.Invoke(values);
        }

        private void RefreshActionButtons(RobotMetadataInfo? metadata)
        {
            if (!metadata.HasValue)
            {
                guidedLessonButton.interactable = false;
                sandboxButton.interactable = false;
                robotControlButton.interactable = false;
                return;
            }

            var data = metadata.Value;
            if (RobotCatalog.TryGet(data.RobotId, out var entry) && entry.LibraryInteractionMode == LibraryInteractionMode.SelectOnly)
            {
                guidedLessonButton.interactable = false;
                sandboxButton.interactable = false;
                robotControlButton.interactable = false;
                return;
            }

            var hasTemplate = RobotCatalog.HasTemplate(data.RobotId);
            guidedLessonButton.interactable = hasTemplate && data.GuidedLessonSupported;
            sandboxButton.interactable = hasTemplate && data.SandboxSupported;
            robotControlButton.interactable = hasTemplate && SupportsRobotControl(data);
        }

        private static string BuildCapabilitySummary(RobotMetadataInfo metadata)
        {
            var parts = new List<string>
            {
                $"Convention: {metadata.Convention}",
                $"Visualization: {metadata.VisualizationLevel}"
            };

            if (metadata.GuidedLessonSupported)
            {
                parts.Add("Guided Lesson");
            }

            if (metadata.SandboxSupported)
            {
                parts.Add("Sandbox");
            }

            if (SupportsRobotControl(metadata))
            {
                parts.Add("Robot Control");
            }

            if (metadata.InstructorRecommended)
            {
                parts.Add("Instructor Recommended");
            }

            return string.Join("  |  ", parts);
        }

        private static bool SupportsRobotControl(RobotMetadataInfo metadata)
        {
            var lessons = metadata.SupportedLessons;
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

        private static void SafeDestroy(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
