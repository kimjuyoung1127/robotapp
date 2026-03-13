// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using KineTutor3D.App;
using KineTutor3D.Math;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using KineTutor3D.UI.Data;
using KineTutor3D.Visualization;
using UnityEngine;
using UnityEngine.UI;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Main 씬의 탭형 학습 쉘을 구성하고 3D/2D/텍스트 패널을 동기화합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainLearningShellController : MonoBehaviour
    {
        private const string OverviewTabId = "overview";
        private const string MotionTabId = "motion";
        private const string ForwardKinematicsTabId = "fk";

        [SerializeField] private AppController appController;
        [SerializeField] private Canvas canvasRoot;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private RobotRenderer robotRenderer;
        [SerializeField] private MathVisualOrchestrator mathVisualOrchestrator;

        private RectTransform shellRoot;
        private RectTransform headerRoot;
        private RectTransform rightDockRoot;
        private RectTransform contentCard;
        private RectTransform motionBarRoot;
        private RectTransform motionControlsRoot;
        private RectTransform overviewGroupRoot;
        private RectTransform overviewLegendRoot;
        private RectTransform overviewCardsRoot;
        private RectTransform overviewCalloutsRoot;
        private RectTransform motionGroupRoot;
        private RectTransform motionAnglesRoot;
        private RectTransform motionMetricsRoot;
        private RectTransform fkGroupRoot;
        private RectTransform fkAdvancedRoot;
        private RectTransform fkAdvancedCardsRoot;

        private Text productTitleText;
        private Text robotTitleText;
        private Text robotMetaText;
        private Button backButton;
        private Button overviewTabButton;
        private Button motionTabButton;
        private Button fkTabButton;

        private Text overviewHeadlineText;
        private Text overviewSummaryText;
        private Text overviewBodyText;
        private Button overviewCtaButton;

        private Text motionHeadlineText;
        private Text motionBodyText;
        private Text motionResultTitleText;
        private Text motionOverlaySummaryText;

        private Text fkHeadlineText;
        private Text fkBodyText;
        private Text fkCompactTitleText;
        private Text fkMatrixText;
        private Text fkPoseText;
        private Text fkFormulaText;
        private Text fkDhSummaryText;
        private Text fkCompactMatrixText;
        private Button fkAdvancedToggleButton;

        private FKDiagramPanel fkDiagramPanel;
        private MainLearningTabsDocument document;
        private RobotMetadataInfo currentMetadata;
        private string currentRobotId = string.Empty;
        private string currentTabId = OverviewTabId;
        private bool advancedVisible;
        private bool listenersBound;
        private bool suppressMotionCallbacks;
        private readonly List<MotionControlRefs> motionControls = new List<MotionControlRefs>();
        private readonly List<MetricBinding> metricBindings = new List<MetricBinding>();

        private sealed class MotionControlRefs
        {
            public int JointIndex;
            public Slider Slider;
            public InputField InputField;
            public Text ValueText;
        }

        private sealed class MetricBinding
        {
            public string MetricId;
            public Text ValueText;
        }

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
        }

        private void OnEnable()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsurePresentation();
        }

        private void OnDisable()
        {
            UnbindListeners();
            HideMathVisuals();
        }

        /// <summary>
        /// AppController와 학습 쉘을 연결합니다.
        /// </summary>
        public void Bind(AppController owner)
        {
            appController = owner;
            ResolveDependencies();
            EnsurePresentation();
            LoadDocument();
            RebuildMotionControls();
            BindListeners();
            RefreshAll();
        }

        /// <summary>
        /// 학습 쉘을 전면에 표시합니다.
        /// </summary>
        public void ShowShell()
        {
            ResolveDependencies();
            EnsurePresentation();
            HideLegacyUi();
            LoadDocument();
            if (shellRoot != null)
            {
                shellRoot.gameObject.SetActive(true);
                shellRoot.SetAsLastSibling();
            }

            ApplyTab(currentTabId);
            RefreshAll();
        }

        private void BindListeners()
        {
            if (listenersBound || appController == null)
            {
                return;
            }

            appController.OnKinematicsUpdated += HandleKinematicsUpdated;
            appController.OnTemplateChanged += HandleTemplateChanged;
            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound || appController == null)
            {
                return;
            }

            appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
            appController.OnTemplateChanged -= HandleTemplateChanged;
            listenersBound = false;
        }

        private void HandleKinematicsUpdated(Mat4D _a1, Mat4D _a2, Mat4D _t02, TutorPose _pose)
        {
            RefreshAll();
        }

        private void HandleTemplateChanged(RobotTemplate _template)
        {
            LoadDocument();
            RebuildMotionControls();
            RefreshAll();
        }

        private void ResolveDependencies()
        {
            canvasRoot ??= GetComponent<Canvas>() ?? FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            robotRenderer ??= FindFirstObjectByType<RobotRenderer>(FindObjectsInactive.Include);
            mathVisualOrchestrator ??= FindFirstObjectByType<MathVisualOrchestrator>(FindObjectsInactive.Include);
        }

        private void EnsurePresentation()
        {
            if (canvasRoot == null)
            {
                return;
            }

            shellRoot = UiRuntimeStyle.EnsureRectChild(canvasRoot.transform, "MainLearningShellRoot");
            UiRuntimeStyle.Stretch(shellRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            BuildHeader();
            BuildRightDock();
            BuildMotionBar();
            EnsureDiagramPanel();
        }

        private void BuildHeader()
        {
            headerRoot = UIComponentFactory.CreatePanel(shellRoot, "MainShellHeader", new Color(0.05f, 0.06f, 0.10f, 0.88f));
            UiRuntimeStyle.Stretch(headerRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -76f), new Vector2(-16f, -16f));

            productTitleText = UIComponentFactory.CreateText(headerRoot, "ShellProductTitle", TypographyPreset.DisplaySm, UIDesignTokens.Colors.TextPrimary, "KineTutor3D", fallbackFont);
            UiRuntimeStyle.Anchor(productTitleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(220f, 28f), new Vector2(24f, 12f));

            robotTitleText = UIComponentFactory.CreateText(headerRoot, "ShellRobotTitle", TypographyPreset.HeadingLg, UIDesignTokens.Colors.AccentPrimary, string.Empty, fallbackFont);
            UiRuntimeStyle.Anchor(robotTitleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(320f, 24f), new Vector2(24f, -14f));

            robotMetaText = UIComponentFactory.CreateText(headerRoot, "ShellRobotMeta", TypographyPreset.Caption, UIDesignTokens.Colors.TextMuted, string.Empty, fallbackFont);
            UiRuntimeStyle.Anchor(robotMetaText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(260f, 20f), new Vector2(-40f, 0f));

            backButton = UIComponentFactory.CreateSecondaryButton(headerRoot, "BtnShellBackToLibrary", "Robot Library", fallbackFont, 132f);
            UiRuntimeStyle.Anchor((RectTransform)backButton.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(132f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(-24f, 0f));
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackToLibrary);

            var tabRow = UIComponentFactory.CreateButtonRow(headerRoot, "ShellTabRow", UIDesignTokens.Space.Xs);
            UiRuntimeStyle.Anchor(tabRow, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(390f, 36f), new Vector2(-174f, 0f));

            overviewTabButton = UIComponentFactory.CreateSecondaryButton(tabRow, "BtnShellTabOverview", "개요", fallbackFont, 118f);
            motionTabButton = UIComponentFactory.CreateSecondaryButton(tabRow, "BtnShellTabMotion", "움직임", fallbackFont, 118f);
            fkTabButton = UIComponentFactory.CreateSecondaryButton(tabRow, "BtnShellTabFk", "FK", fallbackFont, 118f);

            overviewTabButton.onClick.RemoveAllListeners();
            overviewTabButton.onClick.AddListener(() => ApplyTab(OverviewTabId));
            motionTabButton.onClick.RemoveAllListeners();
            motionTabButton.onClick.AddListener(() => ApplyTab(MotionTabId));
            fkTabButton.onClick.RemoveAllListeners();
            fkTabButton.onClick.AddListener(() => ApplyTab(ForwardKinematicsTabId));
        }

        private void BuildRightDock()
        {
            var dockWidth = UILayoutProfile.RightPanelWidth + UIDesignTokens.Space.Xs;
            rightDockRoot = UIComponentFactory.CreatePanel(shellRoot, "MainShellRightDock", new Color(0.08f, 0.09f, 0.14f, 0.82f));
            UiRuntimeStyle.Stretch(
                rightDockRoot,
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(-dockWidth, 16f),
                new Vector2(-16f, -96f));

            var diagramCard = UIComponentFactory.CreatePanel(rightDockRoot, "MainShellDiagramCard", UIDesignTokens.Colors.SurfaceCard);
            UiRuntimeStyle.Anchor(
                diagramCard,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(dockWidth - UIDesignTokens.Space.Md, 264f),
                new Vector2(UIDesignTokens.Space.Xs, -UIDesignTokens.Space.Xs));

            var diagramTitle = UIComponentFactory.CreateText(diagramCard, "MainShellDiagramTitle", TypographyPreset.HeadingSm, UIDesignTokens.Colors.TextPrimary, "2D FK 다이어그램", fallbackFont);
            UiRuntimeStyle.Anchor(diagramTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 22f), new Vector2(16f, -14f));

            var diagramSubtitle = UIComponentFactory.CreateText(diagramCard, "MainShellDiagramSubtitle", TypographyPreset.Caption, UIDesignTokens.Colors.TextMuted, "3D와 동일한 관절 상태를 평면도로 함께 보여줍니다.", fallbackFont);
            UiRuntimeStyle.Anchor(diagramSubtitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(dockWidth - 64f, 18f), new Vector2(16f, -38f));

            var diagramHost = UiRuntimeStyle.EnsureRectChild(diagramCard, "MainShellDiagramHost");
            UiRuntimeStyle.Stretch(diagramHost, Vector2.zero, Vector2.one, new Vector2(16f, 16f), new Vector2(-16f, -68f));

            contentCard = UIComponentFactory.CreatePanel(rightDockRoot, "MainShellContentCard", UIDesignTokens.Colors.SurfaceCard);
            UiRuntimeStyle.Stretch(
                contentCard,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(UIDesignTokens.Space.Xs, 0f),
                new Vector2(-UIDesignTokens.Space.Xs, -280f));

            overviewGroupRoot = CreateScrollGroup(contentCard, "OverviewGroup");
            motionGroupRoot = CreateScrollGroup(contentCard, "MotionGroup");
            fkGroupRoot = CreateScrollGroup(contentCard, "FkGroup");

            BuildOverviewGroup();
            BuildMotionGroup();
            BuildForwardKinematicsGroup();
        }

        private void BuildOverviewGroup()
        {
            overviewHeadlineText = CreateSectionTitle(overviewGroupRoot, "OverviewHeadline");
            overviewSummaryText = CreateBodyText(overviewGroupRoot, "OverviewSummary", FontStyle.Bold, UIDesignTokens.Colors.TextPrimary);
            overviewBodyText = CreateBodyText(overviewGroupRoot, "OverviewBody", FontStyle.Normal, UIDesignTokens.Colors.TextSecondary);

            overviewLegendRoot = UiRuntimeStyle.EnsureRectChild(overviewGroupRoot, "OverviewLegendRoot");
            UiRuntimeStyle.EnsureHorizontalLayout(overviewLegendRoot.gameObject, UIDesignTokens.Space.Xs);
            UiRuntimeStyle.EnsureLayoutElement(overviewLegendRoot).preferredHeight = 34f;

            overviewCardsRoot = UiRuntimeStyle.EnsureRectChild(overviewGroupRoot, "OverviewCardsRoot");
            UiRuntimeStyle.EnsureVerticalLayout(overviewCardsRoot.gameObject, UIDesignTokens.Space.Xs, false);

            overviewCalloutsRoot = UiRuntimeStyle.EnsureRectChild(overviewGroupRoot, "OverviewCalloutsRoot");
            UiRuntimeStyle.EnsureVerticalLayout(overviewCalloutsRoot.gameObject, UIDesignTokens.Space.Xs, false);

            overviewCtaButton = UIComponentFactory.CreatePrimaryButton(overviewGroupRoot, "BtnOverviewMoveNow", "움직여보기", fallbackFont, 180f);
            overviewCtaButton.onClick.RemoveAllListeners();
            overviewCtaButton.onClick.AddListener(() => ApplyTab(MotionTabId));
        }

        private void BuildMotionGroup()
        {
            motionHeadlineText = CreateSectionTitle(motionGroupRoot, "MotionHeadline");
            motionBodyText = CreateBodyText(motionGroupRoot, "MotionBody", FontStyle.Normal, UIDesignTokens.Colors.TextSecondary);

            motionAnglesRoot = UiRuntimeStyle.EnsureRectChild(motionGroupRoot, "MotionAnglesRoot");
            UiRuntimeStyle.EnsureHorizontalLayout(motionAnglesRoot.gameObject, UIDesignTokens.Space.Xs);
            UiRuntimeStyle.EnsureLayoutElement(motionAnglesRoot).preferredHeight = 28f;

            motionResultTitleText = UIComponentFactory.CreateText(motionGroupRoot, "MotionResultTitle", TypographyPreset.HeadingSm, UIDesignTokens.Colors.TextPrimary, string.Empty, fallbackFont);
            motionMetricsRoot = UiRuntimeStyle.EnsureRectChild(motionGroupRoot, "MotionMetricsRoot");
            UiRuntimeStyle.EnsureVerticalLayout(motionMetricsRoot.gameObject, UIDesignTokens.Space.Xs, false);

            motionOverlaySummaryText = CreateBodyText(motionGroupRoot, "MotionOverlaySummary", FontStyle.Italic, UIDesignTokens.Colors.TextMuted);
        }

        private void BuildForwardKinematicsGroup()
        {
            fkHeadlineText = CreateSectionTitle(fkGroupRoot, "FkHeadline");
            fkBodyText = CreateBodyText(fkGroupRoot, "FkBody", FontStyle.Normal, UIDesignTokens.Colors.TextSecondary);
            fkCompactTitleText = UIComponentFactory.CreateText(fkGroupRoot, "FkCompactTitle", TypographyPreset.HeadingSm, UIDesignTokens.Colors.TextPrimary, string.Empty, fallbackFont);
            fkMatrixText = CreateMonospaceText(fkGroupRoot, "FkMatrixText", Vector2.zero, Vector2.zero);
            UiRuntimeStyle.EnsureLayoutElement(fkMatrixText).preferredHeight = 112f;
            fkPoseText = CreateBodyText(fkGroupRoot, "FkPoseText", FontStyle.Bold, UIDesignTokens.Colors.AccentSecondary);
            fkFormulaText = CreateMonospaceText(fkGroupRoot, "FkFormulaText", Vector2.zero, Vector2.zero);
            UiRuntimeStyle.EnsureLayoutElement(fkFormulaText).preferredHeight = 96f;

            fkAdvancedToggleButton = UIComponentFactory.CreateSecondaryButton(fkGroupRoot, "BtnFkAdvanced", "자세히 보기", fallbackFont, 160f);
            fkAdvancedToggleButton.onClick.RemoveAllListeners();
            fkAdvancedToggleButton.onClick.AddListener(ToggleAdvancedSection);

            fkAdvancedRoot = UIComponentFactory.CreatePanel(fkGroupRoot, "FkAdvancedRoot", UIDesignTokens.Colors.SurfaceCard);
            UiRuntimeStyle.EnsureLayoutElement(fkAdvancedRoot).preferredHeight = 380f;
            fkAdvancedCardsRoot = UiRuntimeStyle.EnsureRectChild(fkAdvancedRoot, "FkAdvancedCardsRoot");
            UiRuntimeStyle.Stretch(fkAdvancedCardsRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -16f), new Vector2(-16f, -168f));
            UiRuntimeStyle.EnsureVerticalLayout(fkAdvancedCardsRoot.gameObject, UIDesignTokens.Space.Xs, false);

            fkDhSummaryText = CreateMonospaceText(fkAdvancedRoot, "FkDhSummaryText", new Vector2(16f, -224f), new Vector2(-16f, -96f));
            fkCompactMatrixText = CreateMonospaceText(fkAdvancedRoot, "FkCompactMatrixText", new Vector2(16f, -332f), new Vector2(-16f, -16f));
        }

        private void BuildMotionBar()
        {
            motionBarRoot = UIComponentFactory.CreatePanel(shellRoot, "MainShellMotionBar", new Color(0.07f, 0.08f, 0.12f, 0.90f));
            UiRuntimeStyle.Stretch(
                motionBarRoot,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(16f, 16f),
                new Vector2(-(UILayoutProfile.RightPanelWidth + UIDesignTokens.Space.Xxl), 224f));

            var motionTitle = UIComponentFactory.CreateText(motionBarRoot, "MotionBarTitle", TypographyPreset.Caption, UIDesignTokens.Colors.TextMuted, "JOINT CONTROL", fallbackFont);
            motionTitle.fontStyle = FontStyle.Bold;
            UiRuntimeStyle.Anchor(motionTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180f, 16f), new Vector2(16f, -12f));

            var motionHint = UIComponentFactory.CreateText(motionBarRoot, "MotionBarHint", TypographyPreset.Caption, UIDesignTokens.Colors.TextSecondary, "슬라이더를 조작하면 3D와 2D 결과가 동시에 갱신됩니다.", fallbackFont);
            UiRuntimeStyle.Anchor(motionHint.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, 18f), new Vector2(16f, -32f));

            var scroll = UIComponentFactory.CreateScrollView(motionBarRoot, "MotionControlsScroll", true);
            UiRuntimeStyle.Stretch((RectTransform)scroll.transform, Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -56f));
            motionControlsRoot = scroll.content;
            UiRuntimeStyle.EnsureVerticalLayout(motionControlsRoot.gameObject, UIDesignTokens.Space.Xs, false);
            motionControlsRoot.anchorMin = new Vector2(0f, 1f);
            motionControlsRoot.anchorMax = new Vector2(1f, 1f);
            motionControlsRoot.pivot = new Vector2(0.5f, 1f);
        }

        private void EnsureDiagramPanel()
        {
            var host = shellRoot.Find("MainShellRightDock/MainShellDiagramCard/MainShellDiagramHost") as RectTransform;
            if (host == null)
            {
                return;
            }

            var diagramGo = host.Find("DiagramRuntime")?.gameObject;
            if (diagramGo == null)
            {
                diagramGo = new GameObject("DiagramRuntime", typeof(RectTransform));
                diagramGo.transform.SetParent(host, false);
            }

            var diagramRect = (RectTransform)diagramGo.transform;
            UiRuntimeStyle.Stretch(diagramRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            fkDiagramPanel = diagramGo.GetComponent<FKDiagramPanel>();
            if (fkDiagramPanel == null)
            {
                fkDiagramPanel = diagramGo.AddComponent<FKDiagramPanel>();
            }
        }

        private RectTransform CreateScrollGroup(Transform parent, string name)
        {
            var scroll = UIComponentFactory.CreateScrollView(parent, name, true);
            UiRuntimeStyle.Stretch((RectTransform)scroll.transform, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));
            var content = scroll.content;
            UiRuntimeStyle.EnsureVerticalLayout(content.gameObject, UIDesignTokens.Space.Sm, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            return content;
        }

        private Text CreateSectionTitle(Transform parent, string name)
        {
            return UIComponentFactory.CreateText(parent, name, TypographyPreset.HeadingLg, UIDesignTokens.Colors.TextPrimary, string.Empty, fallbackFont);
        }

        private Text CreateBodyText(Transform parent, string name, FontStyle fontStyle, Color color)
        {
            var text = UIComponentFactory.CreateText(parent, name, TypographyPreset.Body, color, string.Empty, fallbackFont);
            text.fontStyle = fontStyle;
            return text;
        }

        private Text CreateMonospaceText(Transform parent, string name, Vector2 offsetMin, Vector2 offsetMax)
        {
            var text = UiRuntimeStyle.EnsureText(parent, name, fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            if (offsetMin != Vector2.zero || offsetMax != Vector2.zero)
            {
                UiRuntimeStyle.Stretch(text.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), offsetMin, offsetMax);
            }

            return text;
        }

        private void LoadDocument()
        {
            if (appController == null)
            {
                return;
            }

            currentRobotId = !string.IsNullOrWhiteSpace(appController.CurrentTemplate?.Name)
                ? appController.CurrentTemplate.Name
                : RobotSelectionBridge.GetSelectedRobotId();
            if (string.IsNullOrWhiteSpace(currentRobotId))
            {
                currentRobotId = "2DOF_RR";
            }

            currentMetadata = RobotCatalog.TryGet(currentRobotId, out var entry)
                ? entry.Metadata
                : new RobotMetadataInfo(currentRobotId, currentRobotId, Mathf.Max(2, appController.CurrentDof), "Robot", "Medium");

            document = MainLearningTabsLoader.LoadForRobot(currentRobotId, currentMetadata);
            ApplyDocumentToUi();
        }

        private void ApplyDocumentToUi()
        {
            if (document == null)
            {
                return;
            }

            productTitleText.text = "KineTutor3D";
            robotTitleText.text = currentMetadata.DisplayName;
            robotMetaText.text = $"DOF {currentMetadata.Dof} · {currentMetadata.RobotType} · {currentMetadata.Difficulty}";

            UpdateTabButtonLabel(overviewTabButton, document.GetTab(OverviewTabId)?.labelKo ?? "개요", string.Equals(currentTabId, OverviewTabId, StringComparison.Ordinal));
            UpdateTabButtonLabel(motionTabButton, document.GetTab(MotionTabId)?.labelKo ?? "움직임", string.Equals(currentTabId, MotionTabId, StringComparison.Ordinal));
            UpdateTabButtonLabel(fkTabButton, document.GetTab(ForwardKinematicsTabId)?.labelKo ?? "FK", string.Equals(currentTabId, ForwardKinematicsTabId, StringComparison.Ordinal));

            var overview = document.GetTab(OverviewTabId);
            overviewHeadlineText.text = overview?.headlineKo ?? document.displayTitle;
            overviewSummaryText.text = document.heroSummary;
            overviewBodyText.text = overview?.bodyKo ?? string.Empty;
            UpdateButtonLabel(overviewCtaButton, string.IsNullOrWhiteSpace(document.motion.primaryCtaLabelKo) ? "움직여보기" : document.motion.primaryCtaLabelKo);

            RebuildLegend(document.legend);
            RebuildInfoCards(overviewCardsRoot, overview?.cards, false);
            RebuildCallouts(overviewCalloutsRoot, overview?.callouts);

            var motion = document.GetTab(MotionTabId);
            motionHeadlineText.text = motion?.headlineKo ?? "직접 움직여 보기";
            motionBodyText.text = motion?.bodyKo ?? string.Empty;
            motionResultTitleText.text = string.IsNullOrWhiteSpace(document.motion.resultTitleKo) ? "실시간 결과" : document.motion.resultTitleKo;
            motionOverlaySummaryText.text = document.motion.overlaySummaryKo;
            RebuildMetricRows();

            var fk = document.GetTab(ForwardKinematicsTabId);
            fkHeadlineText.text = fk?.headlineKo ?? "Forward Kinematics";
            fkBodyText.text = fk?.bodyKo ?? string.Empty;
            fkCompactTitleText.text = string.IsNullOrWhiteSpace(document.forwardKinematics.compactTitleKo) ? "현재 누적 변환" : document.forwardKinematics.compactTitleKo;
            fkFormulaText.text = BuildFormulaBlock(document.forwardKinematics.formulaLinesKo);
            UpdateButtonLabel(fkAdvancedToggleButton, string.IsNullOrWhiteSpace(document.forwardKinematics.advancedDrawerLabelKo) ? "자세히 보기" : document.forwardKinematics.advancedDrawerLabelKo);
            RebuildInfoCards(fkAdvancedCardsRoot, document.forwardKinematics.advancedCards, true);
        }

        private void RebuildLegend(LegendItemDto[] legendItems)
        {
            ClearChildren(overviewLegendRoot);

            if (legendItems == null)
            {
                return;
            }

            for (var i = 0; i < legendItems.Length; i++)
            {
                var item = legendItems[i];
                if (item == null)
                {
                    continue;
                }

                var badge = UIComponentFactory.CreateBadge(overviewLegendRoot, $"Legend_{i}", item.labelKo, ResolveAccentColor(item.accentKey), fallbackFont);
                var layout = badge.GetComponent<LayoutElement>();
                if (layout != null)
                {
                    layout.preferredWidth = Mathf.Max(86f, 40f + (item.labelKo?.Length ?? 0) * 9f);
                }
            }
        }

        private void RebuildInfoCards(RectTransform root, InfoCardDto[] cards, bool compact)
        {
            ClearChildren(root);

            if (cards == null)
            {
                return;
            }

            for (var i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                if (card == null)
                {
                    continue;
                }

                var panel = UIComponentFactory.CreatePanel(root, $"InfoCard_{i}", UIDesignTokens.Colors.SurfaceCard);
                UiRuntimeStyle.EnsureLayoutElement(panel).preferredHeight = compact ? 92f : 112f;

                var tag = UIComponentFactory.CreateText(panel, "Tag", TypographyPreset.Caption, UIDesignTokens.Colors.AccentPrimary, card.tagKo, fallbackFont);
                tag.fontStyle = FontStyle.Bold;
                UiRuntimeStyle.Anchor(tag.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 18f), new Vector2(12f, -12f));

                var title = UIComponentFactory.CreateText(panel, "Title", TypographyPreset.HeadingSm, UIDesignTokens.Colors.TextPrimary, card.titleKo, fallbackFont);
                UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(260f, 22f), new Vector2(12f, -34f));

                var body = UIComponentFactory.CreateText(panel, "Body", TypographyPreset.Body, UIDesignTokens.Colors.TextSecondary, card.bodyKo, fallbackFont);
                UiRuntimeStyle.Stretch(body.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 48f), new Vector2(-12f, -12f));
            }
        }

        private void RebuildCallouts(RectTransform root, CalloutDto[] callouts)
        {
            ClearChildren(root);

            if (callouts == null)
            {
                return;
            }

            for (var i = 0; i < callouts.Length; i++)
            {
                var callout = callouts[i];
                if (callout == null)
                {
                    continue;
                }

                var color = string.Equals(callout.tone, "accent", StringComparison.Ordinal)
                    ? UIDesignTokens.Colors.AccentPrimary
                    : UIDesignTokens.Colors.SurfaceCard;
                var panel = UIComponentFactory.CreatePanel(root, $"Callout_{i}", new Color(color.r, color.g, color.b, 0.16f));
                UiRuntimeStyle.EnsureLayoutElement(panel).preferredHeight = 84f;

                var title = UIComponentFactory.CreateText(panel, "Title", TypographyPreset.Caption, ResolveAccentColor(callout.tone), callout.titleKo, fallbackFont);
                title.fontStyle = FontStyle.Bold;
                UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280f, 18f), new Vector2(12f, -12f));

                var body = UIComponentFactory.CreateText(panel, "Body", TypographyPreset.Body, UIDesignTokens.Colors.TextSecondary, callout.bodyKo, fallbackFont);
                UiRuntimeStyle.Stretch(body.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 36f), new Vector2(-12f, -12f));
            }
        }

        private void RebuildMetricRows()
        {
            ClearChildren(motionMetricsRoot);
            metricBindings.Clear();

            var metrics = document?.motion?.metrics;
            if (metrics == null)
            {
                return;
            }

            for (var i = 0; i < metrics.Length; i++)
            {
                var metric = metrics[i];
                if (metric == null)
                {
                    continue;
                }

                var row = UIComponentFactory.CreatePanel(motionMetricsRoot, $"MetricRow_{i}", UIDesignTokens.Colors.SurfaceCard);
                UiRuntimeStyle.EnsureLayoutElement(row).preferredHeight = 42f;

                var label = UIComponentFactory.CreateText(row, "Label", TypographyPreset.Body, UIDesignTokens.Colors.TextSecondary, metric.labelKo, fallbackFont);
                UiRuntimeStyle.Anchor(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120f, 20f), new Vector2(12f, 0f));

                var value = UIComponentFactory.CreateText(row, "Value", TypographyPreset.Body, UIDesignTokens.Colors.TextPrimary, "--", fallbackFont, TextAnchor.MiddleRight);
                value.fontStyle = FontStyle.Bold;
                UiRuntimeStyle.Anchor(value.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(180f, 20f), new Vector2(-12f, 0f));

                metricBindings.Add(new MetricBinding
                {
                    MetricId = metric.id,
                    ValueText = value
                });
            }
        }

        private void RebuildMotionControls()
        {
            ClearChildren(motionControlsRoot);
            motionControls.Clear();

            if (appController == null)
            {
                return;
            }

            var dof = Mathf.Max(1, appController.CurrentDof);
            for (var jointIndex = 0; jointIndex < dof; jointIndex++)
            {
                var row = UIComponentFactory.CreatePanel(motionControlsRoot, $"MotionRow_{jointIndex + 1}", UIDesignTokens.Colors.SurfaceCard);
                UiRuntimeStyle.EnsureLayoutElement(row).preferredHeight = 60f;

                var label = UIComponentFactory.CreateText(row, "JointLabel", TypographyPreset.Body, ResolveJointColor(jointIndex), $"J{jointIndex + 1}", fallbackFont);
                label.fontStyle = FontStyle.Bold;
                UiRuntimeStyle.Anchor(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36f, 22f), new Vector2(16f, 0f));

                var valueText = UIComponentFactory.CreateText(row, "ValueText", TypographyPreset.Caption, UIDesignTokens.Colors.TextMuted, "0.0 deg", fallbackFont, TextAnchor.MiddleRight);
                UiRuntimeStyle.Anchor(valueText.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(88f, 20f), new Vector2(-88f, 14f));

                var slider = UIComponentFactory.CreateSlider(row, "JointSlider", -180f, 180f);
                UiRuntimeStyle.Anchor((RectTransform)slider.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(300f, 28f), new Vector2(96f, 0f));

                var inputField = UIComponentFactory.CreateInputField(row, "JointInput", "deg", fallbackFont);
                UiRuntimeStyle.Anchor((RectTransform)inputField.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(72f, 28f), new Vector2(-12f, 0f));

                var capturedIndex = jointIndex;
                slider.onValueChanged.AddListener(value => OnMotionSliderChanged(capturedIndex, value));
                inputField.onEndEdit.AddListener(value => OnMotionInputEndEdit(capturedIndex, value));

                motionControls.Add(new MotionControlRefs
                {
                    JointIndex = jointIndex,
                    Slider = slider,
                    InputField = inputField,
                    ValueText = valueText
                });
            }
        }

        private void OnMotionSliderChanged(int jointIndex, float value)
        {
            if (suppressMotionCallbacks || appController == null)
            {
                return;
            }

            appController.SetJointAngleDegrees(jointIndex, value);
        }

        private void OnMotionInputEndEdit(int jointIndex, string rawValue)
        {
            if (appController == null || jointIndex < 0 || jointIndex >= motionControls.Count)
            {
                return;
            }

            var refs = motionControls[jointIndex];
            if (!JointInputValidator.TryParseDegrees(rawValue, refs.Slider.minValue, refs.Slider.maxValue, out var parsed, out _))
            {
                refs.InputField.SetTextWithoutNotify(JointInputValidator.FormatDegrees(refs.Slider.value));
                return;
            }

            appController.SetJointAngleDegrees(jointIndex, parsed);
        }

        private void ApplyTab(string tabId)
        {
            currentTabId = string.IsNullOrWhiteSpace(tabId) ? OverviewTabId : tabId;

            SetGroupVisible(overviewGroupRoot, string.Equals(currentTabId, OverviewTabId, StringComparison.Ordinal));
            SetGroupVisible(motionGroupRoot, string.Equals(currentTabId, MotionTabId, StringComparison.Ordinal));
            SetGroupVisible(fkGroupRoot, string.Equals(currentTabId, ForwardKinematicsTabId, StringComparison.Ordinal));

            if (motionBarRoot != null)
            {
                motionBarRoot.gameObject.SetActive(string.Equals(currentTabId, MotionTabId, StringComparison.Ordinal));
            }

            if (fkAdvancedRoot != null)
            {
                fkAdvancedRoot.gameObject.SetActive(string.Equals(currentTabId, ForwardKinematicsTabId, StringComparison.Ordinal) && advancedVisible);
            }

            UpdateTabButtonLabel(overviewTabButton, document?.GetTab(OverviewTabId)?.labelKo ?? "개요", string.Equals(currentTabId, OverviewTabId, StringComparison.Ordinal));
            UpdateTabButtonLabel(motionTabButton, document?.GetTab(MotionTabId)?.labelKo ?? "움직임", string.Equals(currentTabId, MotionTabId, StringComparison.Ordinal));
            UpdateTabButtonLabel(fkTabButton, document?.GetTab(ForwardKinematicsTabId)?.labelKo ?? "FK", string.Equals(currentTabId, ForwardKinematicsTabId, StringComparison.Ordinal));

            UpdateVisualMode();
        }

        private static void SetGroupVisible(RectTransform contentRoot, bool visible)
        {
            if (contentRoot == null)
            {
                return;
            }

            var host = contentRoot.parent != null ? contentRoot.parent.parent : null;
            if (host != null)
            {
                host.gameObject.SetActive(visible);
            }
            else
            {
                contentRoot.gameObject.SetActive(visible);
            }
        }

        private void ToggleAdvancedSection()
        {
            advancedVisible = !advancedVisible;
            if (fkAdvancedRoot != null)
            {
                fkAdvancedRoot.gameObject.SetActive(string.Equals(currentTabId, ForwardKinematicsTabId, StringComparison.Ordinal) && advancedVisible);
            }
        }

        private void RefreshAll()
        {
            if (appController == null || document == null)
            {
                return;
            }

            RefreshMotionControlsFromRuntime();
            RefreshMetricValues();
            RefreshForwardKinematics();
            RefreshDiagram();
            UpdateVisualMode();
        }

        private void RefreshMotionControlsFromRuntime()
        {
            suppressMotionCallbacks = true;

            ClearChildren(motionAnglesRoot);
            var joints = appController.CurrentJointValuesRad;
            var cumulativeDeg = 0d;

            for (var i = 0; i < motionControls.Count; i++)
            {
                var refs = motionControls[i];
                var limit = appController.GetJointLimit(refs.JointIndex);
                refs.Slider.minValue = (float)(limit.Min * Mathf.Rad2Deg);
                refs.Slider.maxValue = (float)(limit.Max * Mathf.Rad2Deg);

                var degrees = (float)appController.GetJointAngleDegrees(refs.JointIndex);
                refs.Slider.SetValueWithoutNotify(degrees);
                refs.InputField.SetTextWithoutNotify(JointInputValidator.FormatDegrees(degrees));
                refs.ValueText.text = $"{degrees:0.0} deg";
                refs.ValueText.color = ResolveJointColor(refs.JointIndex);

                cumulativeDeg += joints.Length > refs.JointIndex ? joints[refs.JointIndex] * Mathf.Rad2Deg : 0d;
                UIComponentFactory.CreateBadge(motionAnglesRoot, $"AngleBadge_{i}", $"φ{refs.JointIndex + 1} {cumulativeDeg:0.0}°", ResolveJointColor(refs.JointIndex), fallbackFont);
            }

            suppressMotionCallbacks = false;
        }

        private void RefreshMetricValues()
        {
            var endEffectorPosition = appController.CurrentEndEffectorTransform.ExtractPosition();
            var jointSumDeg = GetCumulativeAngleDegrees(appController.CurrentJointValuesRad);
            var linkLengths = BuildLinkLengthsSummary(appController.CurrentLinks);

            for (var i = 0; i < metricBindings.Count; i++)
            {
                var binding = metricBindings[i];
                if (binding == null || binding.ValueText == null)
                {
                    continue;
                }

                switch (binding.MetricId)
                {
                    case "ee_x":
                        binding.ValueText.text = $"{endEffectorPosition.X.ToString("0.000", CultureInfo.InvariantCulture)} m";
                        break;
                    case "ee_y":
                        binding.ValueText.text = $"{endEffectorPosition.Y.ToString("0.000", CultureInfo.InvariantCulture)} m";
                        break;
                    case "ee_theta":
                        binding.ValueText.text = $"{jointSumDeg.ToString("0.0", CultureInfo.InvariantCulture)} deg";
                        break;
                    case "link_lengths":
                        binding.ValueText.text = linkLengths;
                        break;
                    default:
                        binding.ValueText.text = "--";
                        break;
                }
            }
        }

        private void RefreshForwardKinematics()
        {
            fkMatrixText.text = FormatMatrix(appController.CurrentEndEffectorTransform);
            fkPoseText.text = BuildPoseSummary();
            fkDhSummaryText.text = BuildDhSummary();
            fkCompactMatrixText.text = BuildCompactMatrixSummary();
        }

        private void RefreshDiagram()
        {
            if (fkDiagramPanel == null || appController == null)
            {
                return;
            }

            fkDiagramPanel.SetVisible(true);
            fkDiagramPanel.Refresh(appController.CurrentLinks, appController.CurrentJointValuesRad);
        }

        private void UpdateVisualMode()
        {
            if (!string.Equals(currentTabId, MotionTabId, StringComparison.Ordinal))
            {
                HideMathVisuals();
                return;
            }

            if (!SupportsRichVisuals())
            {
                HideMathVisuals();
                return;
            }

            ResolveDependencies();
            if (robotRenderer == null)
            {
                return;
            }

            mathVisualOrchestrator ??= FindFirstObjectByType<MathVisualOrchestrator>(FindObjectsInactive.Include);
            if (mathVisualOrchestrator == null)
            {
                mathVisualOrchestrator = robotRenderer.gameObject.AddComponent<MathVisualOrchestrator>();
            }

            InitializeMathVisualsIfNeeded();
            mathVisualOrchestrator.SetVisible(true);
            mathVisualOrchestrator.ApplyMathPreset(MathReadinessContent.TwoLinkComposition);
            mathVisualOrchestrator.UpdateFromJointAngles(appController.CurrentJointValuesRad);
        }

        private void InitializeMathVisualsIfNeeded()
        {
            if (mathVisualOrchestrator == null || robotRenderer == null)
            {
                return;
            }

            var baseAnchor = robotRenderer.transform.Find("VisualRoot/BaseVisual");
            var link0Anchor = baseAnchor != null ? baseAnchor.Find("Link0Visual") : null;
            var link1Anchor = link0Anchor != null ? link0Anchor.Find("Link1Visual") : null;
            var eeAnchor = robotRenderer.transform.Find("Frame_EE");

            if (baseAnchor != null && link0Anchor != null)
            {
                mathVisualOrchestrator.Initialize(baseAnchor, link0Anchor, link1Anchor, eeAnchor);
            }
        }

        private void HideMathVisuals()
        {
            if (mathVisualOrchestrator == null)
            {
                return;
            }

            mathVisualOrchestrator.HideAll();
            mathVisualOrchestrator.SetVisible(false);
        }

        private bool SupportsRichVisuals()
        {
            return string.Equals(currentRobotId, "2DOF_RR", StringComparison.Ordinal);
        }

        private void HideLegacyUi()
        {
            if (canvasRoot == null)
            {
                return;
            }

            var legacyNames = new[]
            {
                "TopBar",
                "LeftPanel",
                "RightPanel",
                "BottomBar",
                "GlossaryPanel",
                "TopBarRect",
                "LeftPanelRect",
                "RightPanelRect",
                "BottomBarRect",
                "WhyItMovedRect",
                "BeginnerLeftRect",
                "MathReadinessRect",
                "TargetFeedbackRect",
                "FocusLeftHighlight",
                "FocusRightHighlight",
                "FocusBottomHighlight",
                "FocusViewportHighlight"
            };

            for (var i = 0; i < legacyNames.Length; i++)
            {
                var target = FindCanvasChild(legacyNames[i]);
                if (target != null)
                {
                    target.gameObject.SetActive(false);
                }
            }
        }

        private void BackToLibrary()
        {
            SceneNavigator.Load(SceneId.RobotLibrary);
        }

        private Transform FindCanvasChild(string objectName)
        {
            if (canvasRoot == null)
            {
                return null;
            }

            var transforms = canvasRoot.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                if (string.Equals(transforms[i].name, objectName, StringComparison.Ordinal))
                {
                    return transforms[i];
                }
            }

            return null;
        }

        private static void ClearChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(child);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(child);
                }
            }
        }

        private void UpdateButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
            }
        }

        private void UpdateTabButtonLabel(Button button, string label, bool selected)
        {
            if (button == null)
            {
                return;
            }

            var background = selected ? UIDesignTokens.Colors.AccentPrimary : UIDesignTokens.Colors.SurfaceCard;
            var labelText = UiRuntimeStyle.EnsureButtonLabel(button, fallbackFont, label, background);
            labelText.color = selected ? UIDesignTokens.Colors.TextOnAccent : UIDesignTokens.Colors.TextPrimary;
        }

        private static Color ResolveAccentColor(string accentKey)
        {
            switch (accentKey)
            {
                case "joint1": return UIDesignTokens.Colors.DiagramLink1;
                case "joint2": return UIDesignTokens.Colors.DiagramLink2;
                case "joint3": return UIDesignTokens.Colors.DiagramLink3;
                case "joint4": return UIDesignTokens.Colors.DiagramLink4;
                case "joint5": return UIDesignTokens.Colors.DiagramLink5;
                case "joint6": return UIDesignTokens.Colors.DiagramLink6;
                case "ee": return UIDesignTokens.Colors.DiagramEE;
                case "accent": return UIDesignTokens.Colors.AccentPrimary;
                default: return UIDesignTokens.Colors.TextSecondary;
            }
        }

        private static Color ResolveJointColor(int jointIndex)
        {
            switch (jointIndex)
            {
                case 0: return UIDesignTokens.Colors.DiagramLink1;
                case 1: return UIDesignTokens.Colors.DiagramLink2;
                case 2: return UIDesignTokens.Colors.DiagramLink3;
                case 3: return UIDesignTokens.Colors.DiagramLink4;
                case 4: return UIDesignTokens.Colors.DiagramLink5;
                default: return UIDesignTokens.Colors.DiagramLink6;
            }
        }

        private static double GetCumulativeAngleDegrees(double[] valuesRad)
        {
            if (valuesRad == null)
            {
                return 0d;
            }

            var sum = 0d;
            for (var i = 0; i < valuesRad.Length; i++)
            {
                sum += valuesRad[i] * Mathf.Rad2Deg;
            }

            return sum;
        }

        private static string BuildFormulaBlock(string[] lines)
        {
            if (lines == null || lines.Length == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < lines.Length; i++)
            {
                builder.AppendLine(lines[i]);
            }

            return builder.ToString().TrimEnd();
        }

        private string BuildPoseSummary()
        {
            var position = appController.CurrentEndEffectorTransform.ExtractPosition();
            var orientation = GetCumulativeAngleDegrees(appController.CurrentJointValuesRad);
            return $"EE: ({position.X.ToString("0.000", CultureInfo.InvariantCulture)}, {position.Y.ToString("0.000", CultureInfo.InvariantCulture)}) · θ {orientation.ToString("0.0", CultureInfo.InvariantCulture)}°";
        }

        private static string BuildLinkLengthsSummary(DHLink[] links)
        {
            if (links == null || links.Length == 0)
            {
                return "--";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < links.Length; i++)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append($"L{i + 1} {links[i].A.ToString("0.00", CultureInfo.InvariantCulture)}");
            }

            return builder.ToString();
        }

        private string BuildDhSummary()
        {
            var links = appController.CurrentLinks;
            if (links == null || links.Length == 0)
            {
                return "DH 데이터 없음";
            }

            var builder = new StringBuilder();
            builder.AppendLine("DH summary");
            for (var i = 0; i < links.Length; i++)
            {
                builder.Append("L").Append(i + 1)
                    .Append(": a=").Append(links[i].A.ToString("0.00", CultureInfo.InvariantCulture))
                    .Append(" d=").Append(links[i].D.ToString("0.00", CultureInfo.InvariantCulture))
                    .Append(" α=").Append(links[i].Alpha.ToString("0.00", CultureInfo.InvariantCulture))
                    .AppendLine();
            }

            return builder.ToString().TrimEnd();
        }

        private string BuildCompactMatrixSummary()
        {
            var builder = new StringBuilder();
            builder.AppendLine("A1");
            builder.AppendLine(FormatMatrix(appController.CurrentA1));
            builder.AppendLine();
            builder.AppendLine("T02");
            builder.Append(FormatMatrix(appController.CurrentT02));
            return builder.ToString();
        }

        private static string FormatMatrix(Mat4D matrix)
        {
            var builder = new StringBuilder();
            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                    builder.Append(matrix[row, col].ToString("0.00", CultureInfo.InvariantCulture).PadLeft(6));
                    if (col < 3)
                    {
                        builder.Append(" ");
                    }
                }

                if (row < 3)
                {
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }
    }
}
