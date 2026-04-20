// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 로봇 상세 정보 패널을 표시합니다.
    /// </summary>
    [ExecuteAlways]
    public class RobotDetailDrawer : MonoBehaviour
    {
        private RectTransform overlayRoot;
        private RectTransform panelRoot;
        private Text titleText;
        private Text descriptionText;
        private Text specsText;
        private Text modesText;
        private Button lessonButton;
        private Button sandboxButton;
        private Button closeButton;
        private Font font;
        private RobotCatalogEntry currentEntry;
        private bool isVisible;

        public bool IsVisible => isVisible;

        public void Initialize(RectTransform parent, Font fallbackFont)
        {
            font = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsurePanel(parent);
            Hide();
        }

        public void Show(RobotCatalogEntry entry)
        {
            currentEntry = entry;
            if (panelRoot == null || overlayRoot == null)
            {
                return;
            }

            if (entry == null || entry.LibraryInteractionMode == LibraryInteractionMode.SelectOnly)
            {
                Hide();
                return;
            }

            var m = entry.Metadata;
            titleText.text = m.DisplayName;
            descriptionText.text = m.Description;
            specsText.text = $"DOF: {m.Dof}\nType: {m.RobotType}\nDifficulty: {m.Difficulty}\nConvention: {m.Convention}\nPreview: {ResolvePreviewLabel(m.VisualizationLevel)}";

            var modes = "";
            modes += m.GuidedLessonSupported ? "Guided Lesson: O\n" : "Guided Lesson: X\n";
            modes += SupportsRobotControl(m) ? $"{GetRobotControlLabel()}: O\n" : $"{GetRobotControlLabel()}: X\n";
            modes += m.SandboxSupported ? "Sandbox: O\n" : "Sandbox: X\n";
            modes += m.InstructorRecommended ? "Instructor: O" : "Instructor: X";
            modesText.text = modes;

            bool hasTemplate = RobotCatalog.HasTemplate(m.RobotId);
            var robotControlSupported = SupportsRobotControl(m);
            lessonButton.interactable = hasTemplate && (m.GuidedLessonSupported || robotControlSupported);
            sandboxButton.interactable = hasTemplate && m.SandboxSupported;

            var lessonLabel = lessonButton.GetComponentInChildren<Text>();
            if (lessonLabel != null)
            {
                lessonLabel.text = m.GuidedLessonSupported
                    ? "학습 시작"
                    : robotControlSupported
                        ? GetRobotControlLabel()
                        : "Coming Soon";
            }

            var sandboxLabel = sandboxButton.GetComponentInChildren<Text>();
            if (sandboxLabel != null)
            {
                sandboxLabel.text = sandboxButton.interactable ? "샌드박스 열기" : "Sandbox N/A";
            }

            overlayRoot.gameObject.SetActive(true);
            overlayRoot.SetAsLastSibling();
            isVisible = true;
        }

        public void Hide()
        {
            if (overlayRoot != null)
            {
                overlayRoot.gameObject.SetActive(false);
            }

            isVisible = false;
            currentEntry = null;
        }

        private void EnsurePanel(RectTransform parent)
        {
            overlayRoot = UiRuntimeStyle.EnsureRectChild(parent, "DetailOverlay");
            UiRuntimeStyle.Stretch(overlayRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var overlayBg = UiRuntimeStyle.EnsureImage(overlayRoot, "DetailOverlayBg", UIDesignTokens.Colors.SurfaceOverlay);
            UiRuntimeStyle.Stretch((RectTransform)overlayBg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            overlayBg.raycastTarget = true;

            panelRoot = UiRuntimeStyle.EnsureRectChild(overlayRoot, "DetailPanel");
            var panelWidth = UILayoutProfile.IsTablet ? 360f : 420f;
            UiRuntimeStyle.Anchor(panelRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(panelWidth, 500f), Vector2.zero);

            var bg = UiRuntimeStyle.EnsureImage(panelRoot, "DetailBg", UIDesignTokens.Colors.SurfaceRaised);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            float y = -20f;

            titleText = UiRuntimeStyle.EnsureText(panelRoot, "DetailTitle", font, UIDesignTokens.Type.DisplaySm, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 32f), new Vector2(20f, y));
            y -= 44f;

            descriptionText = UiRuntimeStyle.EnsureText(panelRoot, "DetailDesc", font, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(descriptionText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 60f), new Vector2(20f, y));
            y -= 72f;

            var specLabel = UiRuntimeStyle.EnsureText(panelRoot, "SpecLabel", font, UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.AccentPrimary);
            UiRuntimeStyle.Anchor(specLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 20f), new Vector2(20f, y));
            specLabel.text = "Specifications";
            y -= 24f;

            specsText = UiRuntimeStyle.EnsureText(panelRoot, "DetailSpecs", font, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(specsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 80f), new Vector2(20f, y));
            y -= 92f;

            var modeLabel = UiRuntimeStyle.EnsureText(panelRoot, "ModeLabel", font, UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.AccentPrimary);
            UiRuntimeStyle.Anchor(modeLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 20f), new Vector2(20f, y));
            modeLabel.text = "Supported Modes";
            y -= 24f;

            modesText = UiRuntimeStyle.EnsureText(panelRoot, "DetailModes", font, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(modesText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 60f), new Vector2(20f, y));

            EnsureLessonButton();
            EnsureSandboxButton();
            EnsureCloseButton();
        }

        private void EnsureLessonButton()
        {
            var btnRect = UiRuntimeStyle.EnsureRectChild(panelRoot, "BtnDetailLesson");
            UiRuntimeStyle.Anchor(btnRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(200f, 40f), new Vector2(0f, 106f));

            var image = btnRect.GetComponent<Image>();
            if (image == null)
            {
                image = btnRect.gameObject.AddComponent<Image>();
            }

            lessonButton = btnRect.GetComponent<Button>();
            if (lessonButton == null)
            {
                lessonButton = btnRect.gameObject.AddComponent<Button>();
            }

            UiRuntimeStyle.EnsureButtonLabel(lessonButton, font, "학습 시작", UIDesignTokens.Colors.AccentPrimary);
            lessonButton.onClick.RemoveAllListeners();
            lessonButton.onClick.AddListener(OnLessonClicked);
        }

        private void EnsureSandboxButton()
        {
            var btnRect = UiRuntimeStyle.EnsureRectChild(panelRoot, "BtnDetailSandbox");
            UiRuntimeStyle.Anchor(btnRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(200f, 40f), new Vector2(0f, 60f));

            var image = btnRect.GetComponent<Image>();
            if (image == null)
            {
                image = btnRect.gameObject.AddComponent<Image>();
            }

            sandboxButton = btnRect.GetComponent<Button>();
            if (sandboxButton == null)
            {
                sandboxButton = btnRect.gameObject.AddComponent<Button>();
            }

            UiRuntimeStyle.EnsureButtonLabel(sandboxButton, font, "샌드박스 열기", UIDesignTokens.Colors.SurfaceRaisedAlt);
            sandboxButton.onClick.RemoveAllListeners();
            sandboxButton.onClick.AddListener(OnSandboxClicked);
        }

        private void EnsureCloseButton()
        {
            var btnRect = UiRuntimeStyle.EnsureRectChild(panelRoot, "BtnDetailClose");
            UiRuntimeStyle.Anchor(btnRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(120f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(0f, 16f));

            var image = btnRect.GetComponent<Image>();
            if (image == null)
            {
                image = btnRect.gameObject.AddComponent<Image>();
            }

            closeButton = btnRect.GetComponent<Button>();
            if (closeButton == null)
            {
                closeButton = btnRect.gameObject.AddComponent<Button>();
            }

            UiRuntimeStyle.EnsureButtonLabel(closeButton, font, "닫기", UIDesignTokens.Colors.SurfaceCard);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        private void OnLessonClicked()
        {
            if (currentEntry == null || !RobotCatalog.HasTemplate(currentEntry.Metadata.RobotId))
            {
                return;
            }

            if (SupportsRobotControl(currentEntry.Metadata) && !currentEntry.Metadata.GuidedLessonSupported)
            {
                var targetScene = RobotControlScenePreference.GetPreferredSceneId();
                RobotControlEntryPolicy.Apply(targetScene, RobotControlEntryPolicy.Intent.ResumeLastSession);
                RobotSelectionBridge.SetSelection(currentEntry.Metadata.RobotId, RobotSelectionBridge.RobotControlMode);
                SceneNavigator.Load(targetScene);
                return;
            }

            RobotSelectionBridge.SetSelection(currentEntry.Metadata.RobotId, RobotSelectionBridge.GuidedLessonMode);
            SceneNavigator.Load(SceneId.Sandbox);
        }

        private void OnSandboxClicked()
        {
            if (currentEntry == null || !RobotCatalog.HasTemplate(currentEntry.Metadata.RobotId) || !currentEntry.Metadata.SandboxSupported)
            {
                return;
            }

            RobotSelectionBridge.SetSelection(currentEntry.Metadata.RobotId, RobotSelectionBridge.SandboxMode);
            SceneNavigator.Load(SceneId.Sandbox);
        }

        private static string ResolvePreviewLabel(string visualizationLevel)
        {
            switch (visualizationLevel)
            {
                case "DonorMesh":
                    return "Real 3D donor";
                case "Lesson":
                    return "Teaching model";
                default:
                    return "Concept preview";
            }
        }

        private static bool SupportsRobotControl(RobotMetadataInfo metadata)
        {
            if (metadata.SupportedLessons == null)
            {
                return false;
            }

            for (var i = 0; i < metadata.SupportedLessons.Length; i++)
            {
                if (string.Equals(metadata.SupportedLessons[i], "RobotControl", System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetRobotControlLabel()
        {
            return RobotControlScenePreference.ShouldPreferV3()
                ? "Robot Control V3"
                : "Robot Control";
        }
    }
}
