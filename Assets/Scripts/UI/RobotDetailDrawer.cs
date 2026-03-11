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
        private RectTransform panelRoot;
        private Text titleText;
        private Text descriptionText;
        private Text specsText;
        private Text modesText;
        private Button ctaButton;
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
            if (panelRoot == null)
            {
                return;
            }

            var m = entry.Metadata;
            titleText.text = m.DisplayName;
            descriptionText.text = m.Description;
            specsText.text = $"DOF: {m.Dof}\nType: {m.RobotType}\nDifficulty: {m.Difficulty}\nConvention: {m.Convention}";

            var modes = "";
            modes += m.GuidedLessonSupported ? "Guided Lesson: O\n" : "Guided Lesson: X\n";
            modes += m.SandboxSupported ? "Sandbox: O\n" : "Sandbox: X\n";
            modes += m.InstructorRecommended ? "Instructor: O" : "Instructor: X";
            modesText.text = modes;

            bool hasTemplate = RobotCatalog.HasTemplate(m.RobotId);
            ctaButton.interactable = hasTemplate;
            var ctaLabel = ctaButton.GetComponentInChildren<Text>();
            if (ctaLabel != null)
            {
                ctaLabel.text = hasTemplate ? "학습 시작" : "Coming Soon";
            }

            panelRoot.gameObject.SetActive(true);
            isVisible = true;
        }

        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(false);
            }

            isVisible = false;
            currentEntry = null;
        }

        private void EnsurePanel(RectTransform parent)
        {
            panelRoot = UiRuntimeStyle.EnsureRectChild(parent, "DetailPanel");
            UiRuntimeStyle.Anchor(panelRoot, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(340f, 500f), new Vector2(-10f, 0f));

            var bg = UiRuntimeStyle.EnsureImage(panelRoot, "DetailBg", UiRuntimeStyle.PanelBackground);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            float y = -20f;

            titleText = UiRuntimeStyle.EnsureText(panelRoot, "DetailTitle", font, 22, FontStyle.Bold, TextAnchor.UpperLeft, UiRuntimeStyle.TextPrimary);
            UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 32f), new Vector2(20f, y));
            y -= 44f;

            descriptionText = UiRuntimeStyle.EnsureText(panelRoot, "DetailDesc", font, 14, FontStyle.Normal, TextAnchor.UpperLeft, UiRuntimeStyle.TextSecondary);
            UiRuntimeStyle.Anchor(descriptionText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 60f), new Vector2(20f, y));
            y -= 72f;

            var specLabel = UiRuntimeStyle.EnsureText(panelRoot, "SpecLabel", font, 14, FontStyle.Bold, TextAnchor.UpperLeft, UiRuntimeStyle.AccentBlue);
            UiRuntimeStyle.Anchor(specLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 20f), new Vector2(20f, y));
            specLabel.text = "Specifications";
            y -= 24f;

            specsText = UiRuntimeStyle.EnsureText(panelRoot, "DetailSpecs", font, 13, FontStyle.Normal, TextAnchor.UpperLeft, UiRuntimeStyle.TextSecondary);
            UiRuntimeStyle.Anchor(specsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 80f), new Vector2(20f, y));
            y -= 92f;

            var modeLabel = UiRuntimeStyle.EnsureText(panelRoot, "ModeLabel", font, 14, FontStyle.Bold, TextAnchor.UpperLeft, UiRuntimeStyle.AccentBlue);
            UiRuntimeStyle.Anchor(modeLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 20f), new Vector2(20f, y));
            modeLabel.text = "Supported Modes";
            y -= 24f;

            modesText = UiRuntimeStyle.EnsureText(panelRoot, "DetailModes", font, 13, FontStyle.Normal, TextAnchor.UpperLeft, UiRuntimeStyle.TextSecondary);
            UiRuntimeStyle.Anchor(modesText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 60f), new Vector2(20f, y));

            EnsureCtaButton();
            EnsureCloseButton();
        }

        private void EnsureCtaButton()
        {
            var btnRect = UiRuntimeStyle.EnsureRectChild(panelRoot, "BtnDetailCta");
            UiRuntimeStyle.Anchor(btnRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(200f, 40f), new Vector2(0f, 60f));

            var image = btnRect.GetComponent<Image>();
            if (image == null)
            {
                image = btnRect.gameObject.AddComponent<Image>();
            }

            ctaButton = btnRect.GetComponent<Button>();
            if (ctaButton == null)
            {
                ctaButton = btnRect.gameObject.AddComponent<Button>();
            }

            UiRuntimeStyle.EnsureButtonLabel(ctaButton, font, "학습 시작", UiRuntimeStyle.AccentBlue);
            ctaButton.onClick.RemoveAllListeners();
            ctaButton.onClick.AddListener(OnCtaClicked);
        }

        private void EnsureCloseButton()
        {
            var btnRect = UiRuntimeStyle.EnsureRectChild(panelRoot, "BtnDetailClose");
            UiRuntimeStyle.Anchor(btnRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(120f, 36f), new Vector2(0f, 16f));

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

            UiRuntimeStyle.EnsureButtonLabel(closeButton, font, "닫기", UiRuntimeStyle.CardBackground);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        private void OnCtaClicked()
        {
            if (currentEntry == null || !RobotCatalog.HasTemplate(currentEntry.Metadata.RobotId))
            {
                return;
            }

            RobotSelectionBridge.SetSelectedRobot(currentEntry.Metadata.RobotId);
            SceneNavigator.Load(SceneId.Main);
        }
    }
}
