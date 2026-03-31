// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// SandboxViewBuilder가 생성한 UI 참조를 담는 구조체입니다.
    /// </summary>
    public struct SandboxViewRefs
    {
        public Canvas canvas;
        public Slider[] jointSliders;
        public Text infoLabel;
        public Toggle gizmoToggle;
        public Button clearTrailButton;
        public Button backToLibraryButton;
        public Button zeroPoseButton;
        public Button homePoseButton;
        public Button demoPoseButton;
        public Button readyPoseButton;
        public Button changeRobotButton;
        public Text robotNameLabel;
    }

    /// <summary>
    /// Sandbox 씬의 모든 UI를 프로그래밍 방식으로 생성하는 정적 빌더입니다.
    /// </summary>
    public static class SandboxViewBuilder
    {
        private const float TopBarHeight = 48f;
        private const float JointPanelWidth = 280f;
        private const float PresetPanelHeight = 56f;
        private const float InfoPanelWidth = 260f;
        private const float InfoPanelHeight = 36f;
        private const float SliderRowHeight = 40f;
        private const float PresetButtonWidth = 100f;

        /// <summary>
        /// Sandbox UI 전체를 생성하고 참조를 반환합니다.
        /// </summary>
        public static SandboxViewRefs Build(
            Transform parent,
            Font font,
            int jointCount,
            string robotDisplayName,
            int dof,
            JointLimit[] jointLimits)
        {
            var refs = new SandboxViewRefs();

            // Canvas는 이미 생성된 상태 (parent가 Canvas의 transform)
            refs.canvas = parent.GetComponentInParent<Canvas>();

            BuildTopBar(parent, font, robotDisplayName, dof, ref refs);
            BuildJointSliderPanel(parent, font, jointCount, jointLimits, ref refs);
            BuildPresetPanel(parent, font, ref refs);
            BuildInfoPanel(parent, font, ref refs);

            return refs;
        }

        private static void BuildTopBar(Transform parent, Font font, string displayName, int dof, ref SandboxViewRefs refs)
        {
            var topBar = UIComponentFactory.CreatePanel(parent, "SandboxTopBar", UIDesignTokens.Colors.TopBarBackground);
            UiRuntimeStyle.Stretch(topBar, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -TopBarHeight), new Vector2(0f, 0f));

            var hlg = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(12, 12, 4, 4);
            hlg.spacing = 12f;

            refs.robotNameLabel = UIComponentFactory.CreateText(
                topBar, "RobotNameLabel",
                TypographyPreset.HeadingSm,
                UIDesignTokens.Colors.TextPrimary,
                $"{displayName} \u00b7 {dof}DOF",
                font,
                TextAnchor.MiddleLeft);
            var nameLabelRect = refs.robotNameLabel.rectTransform;
            nameLabelRect.sizeDelta = new Vector2(300f, TopBarHeight);

            // Spacer
            var spacer = UiRuntimeStyle.EnsureRectChild(topBar, "Spacer");
            var spacerLayout = spacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.flexibleWidth = 1f;

            refs.gizmoToggle = UIComponentFactory.CreateToggle(topBar, "GizmoToggle", "Gizmo", font);
            var toggleRect = refs.gizmoToggle.transform as RectTransform;
            if (toggleRect != null)
            {
                var toggleLayout = toggleRect.gameObject.AddComponent<LayoutElement>();
                toggleLayout.preferredWidth = 90f;
            }

            refs.clearTrailButton = UIComponentFactory.CreateSecondaryButton(topBar, "BtnClearTrail", "Clear Trail", font, 100f);
            refs.backToLibraryButton = UIComponentFactory.CreateSecondaryButton(topBar, "BtnBackToLibrary", "Library", font, 80f);
        }

        private static void BuildJointSliderPanel(Transform parent, Font font, int jointCount, JointLimit[] jointLimits, ref SandboxViewRefs refs)
        {
            var panel = UIComponentFactory.CreatePanel(parent, "JointSliderPanel", UIDesignTokens.Colors.SurfaceRaised);
            UiRuntimeStyle.Stretch(panel,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0f, PresetPanelHeight),
                new Vector2(JointPanelWidth, -TopBarHeight));

            // ScrollRect for > 4 joints
            var scrollGo = new GameObject("JointScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(panel, false);
            var scrollRect = scrollGo.GetComponent<ScrollRect>();
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            UiRuntimeStyle.Stretch(scrollRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            var viewportRt = viewport.GetComponent<RectTransform>();
            UiRuntimeStyle.Stretch(viewportRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0f, 0f);

            var contentVlg = content.GetComponent<VerticalLayoutGroup>();
            contentVlg.childControlWidth = true;
            contentVlg.childControlHeight = false;
            contentVlg.childForceExpandWidth = true;
            contentVlg.childForceExpandHeight = false;
            contentVlg.padding = new RectOffset(8, 8, 8, 8);
            contentVlg.spacing = 4f;

            var contentFitter = content.GetComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            refs.jointSliders = new Slider[jointCount];

            for (var i = 0; i < jointCount; i++)
            {
                var row = new GameObject($"JointRow_{i}", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                row.transform.SetParent(content.transform, false);
                var rowLayout = row.GetComponent<LayoutElement>();
                rowLayout.preferredHeight = SliderRowHeight;
                rowLayout.flexibleWidth = 1f;

                var rowHlg = row.GetComponent<HorizontalLayoutGroup>();
                rowHlg.childControlWidth = false;
                rowHlg.childControlHeight = true;
                rowHlg.childForceExpandWidth = false;
                rowHlg.childForceExpandHeight = true;
                rowHlg.spacing = 8f;
                rowHlg.padding = new RectOffset(4, 4, 0, 0);

                var label = UIComponentFactory.CreateText(
                    row.transform, $"LabelJ{i}",
                    TypographyPreset.Caption,
                    UIDesignTokens.Colors.TextSecondary,
                    $"J{i + 1}",
                    font,
                    TextAnchor.MiddleLeft);
                var labelLayout = label.gameObject.AddComponent<LayoutElement>();
                labelLayout.preferredWidth = 30f;

                var minDeg = jointLimits != null && i < jointLimits.Length ? (float)jointLimits[i].Min * (180f / Mathf.PI) : -180f;
                var maxDeg = jointLimits != null && i < jointLimits.Length ? (float)jointLimits[i].Max * (180f / Mathf.PI) : 180f;

                // JointLimit은 라디안이므로 도 변환
                var slider = UIComponentFactory.CreateSlider(row.transform, $"Slider_J{i}", minDeg, maxDeg);
                slider.wholeNumbers = false;
                slider.value = 0f;

                var sliderLayout = slider.gameObject.AddComponent<LayoutElement>();
                sliderLayout.flexibleWidth = 1f;
                sliderLayout.preferredHeight = UIDesignTokens.Size.SliderHeight;

                refs.jointSliders[i] = slider;
            }
        }

        private static void BuildPresetPanel(Transform parent, Font font, ref SandboxViewRefs refs)
        {
            var panel = UIComponentFactory.CreatePanel(parent, "PresetPanel", UIDesignTokens.Colors.SurfaceRaised);
            UiRuntimeStyle.Stretch(panel,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, PresetPanelHeight));

            var hlg = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(12, 12, 6, 6);
            hlg.spacing = 8f;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            refs.zeroPoseButton = UIComponentFactory.CreatePrimaryButton(panel, "BtnZero", "Zero", font, PresetButtonWidth);
            refs.homePoseButton = UIComponentFactory.CreatePrimaryButton(panel, "BtnHome", "Home", font, PresetButtonWidth);
            refs.demoPoseButton = UIComponentFactory.CreatePrimaryButton(panel, "BtnDemo", "Demo", font, PresetButtonWidth);
            refs.readyPoseButton = UIComponentFactory.CreatePrimaryButton(panel, "BtnReady", "Ready", font, PresetButtonWidth);

            // Spacer
            var spacer = UiRuntimeStyle.EnsureRectChild(panel, "PresetSpacer");
            var spacerLayout = spacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.flexibleWidth = 1f;

            refs.changeRobotButton = UIComponentFactory.CreateSecondaryButton(panel, "BtnChangeRobot", "Change Robot", font, 130f);
        }

        private static void BuildInfoPanel(Transform parent, Font font, ref SandboxViewRefs refs)
        {
            var panel = UIComponentFactory.CreatePanel(parent, "InfoPanel", UIDesignTokens.Colors.SurfaceCard);
            UiRuntimeStyle.Anchor(
                panel,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(InfoPanelWidth, InfoPanelHeight),
                new Vector2(-InfoPanelWidth / 2f - 8f, PresetPanelHeight + 8f));

            refs.infoLabel = UIComponentFactory.CreateText(
                panel, "EEInfoLabel",
                TypographyPreset.Caption,
                UIDesignTokens.Colors.TextSecondary,
                "X: 0.0  Y: 0.0  Z: 0.0 mm",
                font,
                TextAnchor.MiddleCenter);
            UiRuntimeStyle.Stretch(refs.infoLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }
    }
}
