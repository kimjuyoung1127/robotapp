// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// RobotControlV2 authored shell을 이름 기준으로 정규화하고 패널 컴포넌트를 바인딩합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RobotControlShellBinder : MonoBehaviour
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private TopStatusBar topStatusBar;
        [SerializeField] private RobotControlWorkTabBar workTabBar;
        [SerializeField] private EasyMotionPanel easyMotionPanel;
        [SerializeField] private TcpJogPanel tcpJogPanel;
        [SerializeField] private JointJogPanel jointJogPanel;
        [SerializeField] private PointMovePanel pointMovePanel;
        [SerializeField] private TeachingPanel teachingPanel;
        [SerializeField] private StatusSummaryPanel statusSummaryPanel;
        [SerializeField] private RobotControlWhyItMovedPanel whyItMovedPanel;
        [SerializeField] private RecoveryGuidePanel recoveryGuidePanel;
        [SerializeField] private HelpPanel helpPanel;
        [SerializeField] private DiagnosticsPlaceholderPanel diagnosticsPanel;

        public TopStatusBar TopStatusBar => topStatusBar;

        public RobotControlWorkTabBar WorkTabBar => workTabBar;

        public void RefreshAuthoring()
        {
            EnsureShellStructure();
        }

        public void Bind(RobotControlViewState state)
        {
            EnsureShellStructure();
            ApplyState(state);
        }

        public void ApplyLayoutMode(bool tabletLayout)
        {
            if (transform is not RectTransform root)
            {
                return;
            }

            var safeArea = EnsureChild(root, "SafeArea");
            var leftRail = EnsureChild(safeArea, "LeftRail");
            var rightRail = EnsureChild(safeArea, "RightRail");
            var bottomSheets = EnsureChild(safeArea, "BottomSheets");

            UiRuntimeStyle.SetCanvasVisible(leftRail.gameObject, !tabletLayout);
            UiRuntimeStyle.SetCanvasVisible(rightRail.gameObject, !tabletLayout);
            UiRuntimeStyle.SetCanvasVisible(bottomSheets.gameObject, tabletLayout);
        }

        private void ApplyState(RobotControlViewState state)
        {
            topStatusBar?.ApplyState(state);
            easyMotionPanel?.ApplyState(state);
            tcpJogPanel?.ApplyState(state);
            jointJogPanel?.ApplyState(state);
            pointMovePanel?.ApplyState(state);
            teachingPanel?.ApplyState(state);
            statusSummaryPanel?.ApplyState(state);
            whyItMovedPanel?.ApplyState(state);
            recoveryGuidePanel?.ApplyState(state);
            helpPanel?.ApplyState(state);
            workTabBar?.SelectTab(RobotControlWorkTab.EasyMotion);
            ApplyLayoutMode(state.IsTabletLayout);
        }

        public void EnsureShellStructure()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);

            if (transform is not RectTransform root)
            {
                return;
            }

            var safeArea = EnsureChild(root, "SafeArea");
            UiRuntimeStyle.Stretch(safeArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            EnsureRegionStyle(safeArea, UIDesignTokens.RobotControlV2.Colors.SafeArea);

            var topBarRoot = EnsureChild(safeArea, "TopStatusBar");
            UiRuntimeStyle.Stretch(topBarRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -88f), new Vector2(-16f, -16f));

            var leftRail = EnsureChild(safeArea, "LeftRail");
            UiRuntimeStyle.Stretch(leftRail, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(16f, 16f), new Vector2(UIDesignTokens.RobotControlV2.Size.LeftRailWidth, -112f));
            EnsureRegionStyle(leftRail, UIDesignTokens.RobotControlV2.Colors.LeftRail);
            NormalizeChildren(leftRail, "GlobalNavPanel", "WorkTabBar", "WorkPanelHost");
            var globalNav = EnsureChild(leftRail, "GlobalNavPanel");
            var workTabBarRoot = EnsureChild(leftRail, "WorkTabBar");
            var workPanelHost = EnsureChild(leftRail, "WorkPanelHost");
            UiRuntimeStyle.Anchor(globalNav, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(344f, 64f), new Vector2(0f, -12f));
            UiRuntimeStyle.Anchor(workTabBarRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(344f, 80f), new Vector2(0f, -84f));
            UiRuntimeStyle.Stretch(workPanelHost, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(12f, 12f), new Vector2(-12f, -176f));
            BuildGlobalNav(globalNav);
            NormalizeChildren(workPanelHost, "EasyMotionPanel", "TcpJogPanel", "JointJogPanel", "PointMovePanel", "TeachingPanel");

            var centerViewport = EnsureChild(safeArea, "CenterViewport");
            UiRuntimeStyle.Stretch(centerViewport, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(UIDesignTokens.RobotControlV2.Size.LeftRailWidth + 32f, 16f), new Vector2(-(UIDesignTokens.RobotControlV2.Size.RightRailWidth + 32f), -112f));
            EnsureRegionStyle(centerViewport, UIDesignTokens.RobotControlV2.Colors.CenterViewport);
            NormalizeChildren(centerViewport, "ViewportHeaderOverlay", "ViewportFooterOverlay", "ViewportHintLayer");
            var viewportHeader = EnsureChild(centerViewport, "ViewportHeaderOverlay");
            var viewportFooter = EnsureChild(centerViewport, "ViewportFooterOverlay");
            var viewportHint = EnsureChild(centerViewport, "ViewportHintLayer");
            UiRuntimeStyle.Anchor(viewportHeader, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(440f, 56f), new Vector2(20f, -20f));
            UiRuntimeStyle.Anchor(viewportFooter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(420f, 64f), new Vector2(0f, 20f));
            UiRuntimeStyle.Anchor(viewportHint, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 136f), new Vector2(20f, -92f));
            BuildViewportHeader(viewportHeader);
            BuildViewportFooter(viewportFooter);
            BuildViewportHint(viewportHint);

            var rightRail = EnsureChild(safeArea, "RightRail");
            UiRuntimeStyle.Stretch(rightRail, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-UIDesignTokens.RobotControlV2.Size.RightRailWidth, 16f), new Vector2(-16f, -112f));
            EnsureRegionStyle(rightRail, UIDesignTokens.RobotControlV2.Colors.RightRail);
            NormalizeChildren(rightRail, "StatusSummaryPanel", "WhyItMovedPanel", "RecoveryGuidePanel", "HelpPanel");
            AnchorStackCard(EnsureChild(rightRail, "StatusSummaryPanel"), 12f, 158f);
            AnchorStackCard(EnsureChild(rightRail, "WhyItMovedPanel"), 182f, 128f);
            AnchorStackCard(EnsureChild(rightRail, "RecoveryGuidePanel"), 322f, 162f);
            AnchorStackCard(EnsureChild(rightRail, "HelpPanel"), 496f, 144f);

            var bottomSheets = EnsureChild(safeArea, "BottomSheets");
            UiRuntimeStyle.Stretch(bottomSheets, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 16f), new Vector2(-16f, UIDesignTokens.RobotControlV2.Size.BottomSheetHeight));
            EnsureRegionStyle(bottomSheets, UIDesignTokens.RobotControlV2.Colors.BottomSheet);
            NormalizeChildren(bottomSheets, "TabletWorkSheet", "TabletStatusSheet", "TabletModuleSheet");
            BuildTabletWorkSheet(EnsureChild(bottomSheets, "TabletWorkSheet"));
            BuildTabletStatusSheet(EnsureChild(bottomSheets, "TabletStatusSheet"));
            BuildTabletModuleSheet(EnsureChild(bottomSheets, "TabletModuleSheet"));

            var popups = EnsureChild(safeArea, "Popups");
            UiRuntimeStyle.Stretch(popups, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            NormalizeChildren(popups, "MoveConfirmDialog", "WarningDialog", "RecoveryDialog", "FirstRunGuideDialog", "ToastAnchor");
            BuildPopup(EnsureChild(popups, "MoveConfirmDialog"), "MOVE CONFIRM", "Mock mode is active. Preview and confirm flow is staged only.", "실행 준비", "닫기", false, UIDesignTokens.RobotControlV2.Colors.Accent);
            BuildPopup(EnsureChild(popups, "WarningDialog"), "WARNING", "Preview gate enabled", "위험 보기", "취소", false, UIDesignTokens.RobotControlV2.Colors.Warning);
            BuildPopup(EnsureChild(popups, "RecoveryDialog"), "RECOVERY", "Bind shell state, choose a work tab, then preview before live motion.", "복구 안내", "닫기", false, UIDesignTokens.RobotControlV2.Colors.Success);
            BuildPopup(EnsureChild(popups, "FirstRunGuideDialog"), "FIRST RUN", "This scene is a V2 shell-only staging surface.", "확인", "나중에", true, UIDesignTokens.RobotControlV2.Colors.Accent);
            BuildToastAnchor(EnsureChild(popups, "ToastAnchor"));

            var debugOnly = EnsureChild(root, "DebugOnly");
            UiRuntimeStyle.Stretch(debugOnly, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            NormalizeChildren(debugOnly, "DiagnosticsDrawer", "LayoutBoundsOverlay");
            var diagnosticsRoot = EnsureChild(debugOnly, "DiagnosticsDrawer");
            var layoutBoundsOverlay = EnsureChild(debugOnly, "LayoutBoundsOverlay");
            UiRuntimeStyle.Anchor(diagnosticsRoot, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(280f, 86f), new Vector2(-24f, 24f));
            UiRuntimeStyle.Stretch(layoutBoundsOverlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            BuildLayoutBoundsOverlay(layoutBoundsOverlay);

            topStatusBar = topBarRoot.GetComponent<TopStatusBar>() ?? topBarRoot.gameObject.AddComponent<TopStatusBar>();
            topStatusBar.SetFallbackFont(fallbackFont);

            workTabBar = workTabBarRoot.GetComponent<RobotControlWorkTabBar>() ?? workTabBarRoot.gameObject.AddComponent<RobotControlWorkTabBar>();
            workTabBar.SetFallbackFont(fallbackFont);
            workTabBar.Bind(workPanelHost);

            easyMotionPanel = AttachPanel<EasyMotionPanel>(workPanelHost, "EasyMotionPanel");
            tcpJogPanel = AttachPanel<TcpJogPanel>(workPanelHost, "TcpJogPanel");
            jointJogPanel = AttachPanel<JointJogPanel>(workPanelHost, "JointJogPanel");
            pointMovePanel = AttachPanel<PointMovePanel>(workPanelHost, "PointMovePanel");
            teachingPanel = AttachPanel<TeachingPanel>(workPanelHost, "TeachingPanel");
            statusSummaryPanel = AttachPanel<StatusSummaryPanel>(rightRail, "StatusSummaryPanel");
            whyItMovedPanel = AttachPanel<RobotControlWhyItMovedPanel>(rightRail, "WhyItMovedPanel");
            recoveryGuidePanel = AttachPanel<RecoveryGuidePanel>(rightRail, "RecoveryGuidePanel");
            helpPanel = AttachPanel<HelpPanel>(rightRail, "HelpPanel");
            diagnosticsPanel = AttachPanel<DiagnosticsPlaceholderPanel>(debugOnly, "DiagnosticsDrawer");
        }

        private T AttachPanel<T>(RectTransform parent, string name) where T : Component
        {
            var target = EnsureChild(parent, name);
            return target.GetComponent<T>() ?? target.gameObject.AddComponent<T>();
        }

        private void BuildGlobalNav(RectTransform root)
        {
            var bg = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            bg.color = UIDesignTokens.RobotControlV2.Colors.Card;

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, 12, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Warning);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 20f), new Vector2(16f, -12f));
            title.text = "GLOBAL NAV";

            var hint = UiRuntimeStyle.EnsureText(root, "Hint", fallbackFont, 11, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(hint.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 20f), new Vector2(16f, -32f));
            hint.text = "Robot selection and global page navigation placeholder.";
        }

        private void BuildViewportHeader(RectTransform root)
        {
            var bg = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            bg.color = UIDesignTokens.RobotControlV2.Colors.Card;

            EnsureMiniButton(root, "BtnShowBaseAxes", "BASE", new Vector2(12f, -14f), true);
            EnsureMiniButton(root, "BtnShowToolAxes", "TOOL", new Vector2(92f, -14f), false);
            EnsureMiniButton(root, "BtnShowGhost", "GHOST", new Vector2(172f, -14f), true);
            EnsureMiniButton(root, "BtnShowPath", "PATH", new Vector2(252f, -14f), false);
            EnsureMiniButton(root, "BtnShowRisk", "RISK", new Vector2(332f, -14f), false);
        }

        private void BuildViewportFooter(RectTransform root)
        {
            var bg = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            bg.color = UIDesignTokens.RobotControlV2.Colors.Card;

            EnsureMiniButton(root, "BtnPointModule", "POINTS", new Vector2(12f, -14f), false, 94f);
            EnsureMiniButton(root, "BtnIOModule", "I/O", new Vector2(114f, -14f), false, 72f);
            EnsureMiniButton(root, "BtnTPDModule", "TPD", new Vector2(194f, -14f), false, 72f);
            EnsureMiniButton(root, "BtnDiagModule", "DIAGNOSTICS", new Vector2(274f, -14f), true, 132f);
        }

        private void BuildViewportHint(RectTransform root)
        {
            var bg = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            bg.color = UIDesignTokens.RobotControlV2.Colors.CardAlt;

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, 12, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 20f), new Vector2(16f, -16f));
            title.text = "WORKSPACE";

            var body = UiRuntimeStyle.EnsureText(root, "Body", fallbackFont, 12, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Anchor(body.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280f, 64f), new Vector2(16f, -42f));
            body.text = "Live robot, ghost target, predicted path, and risk overlay land here.";
        }

        private void BuildTabletWorkSheet(RectTransform root)
        {
            var bg = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            bg.color = UIDesignTokens.RobotControlV2.Colors.Card;
            EnsurePanelSprite(bg);

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, 12, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(240f, 20f), new Vector2(16f, -16f));
            title.text = "Tablet Work Sheet";

            var body = UiRuntimeStyle.EnsureText(root, "Body", fallbackFont, 11, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(body.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 20f), new Vector2(16f, -38f));
            body.text = "현재 작업 탭을 tablet 하단 시트로 축약합니다.";

            var chipRow = EnsureChild(root, "ChipRow");
            UiRuntimeStyle.Anchor(chipRow, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -74f), new Vector2(-16f, -106f));
            ConfigureHorizontalRow(chipRow, 8f);
            EnsureMiniSheetButton(chipRow, "BtnSheetEasy", "쉬운 조작");
            EnsureMiniSheetButton(chipRow, "BtnSheetTcp", "TCP");
            EnsureMiniSheetButton(chipRow, "BtnSheetJoint", "관절");
            EnsureMiniSheetButton(chipRow, "BtnSheetPoint", "포인트");

            var actionRow = EnsureChild(root, "ActionRow");
            UiRuntimeStyle.Anchor(actionRow, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -118f), new Vector2(-16f, -158f));
            ConfigureHorizontalRow(actionRow, 8f);
            EnsureSheetAction(actionRow, "BtnTabletPreview", "미리보기", UIDesignTokens.RobotControlV2.Colors.Success);
            EnsureSheetAction(actionRow, "BtnTabletMove", "실제 이동", UIDesignTokens.RobotControlV2.Colors.Danger);
        }

        private void BuildTabletStatusSheet(RectTransform root)
        {
            var bg = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            bg.color = UIDesignTokens.RobotControlV2.Colors.Card;
            EnsurePanelSprite(bg);

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, 12, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(240f, 20f), new Vector2(16f, -16f));
            title.text = "Tablet Status Sheet";

            var body = UiRuntimeStyle.EnsureText(root, "Body", fallbackFont, 11, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(body.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 20f), new Vector2(16f, -38f));
            body.text = "상태 / 복구 / 도움말을 한 시트로 압축합니다.";

            var badgeRow = EnsureChild(root, "BadgeRow");
            UiRuntimeStyle.Anchor(badgeRow, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -72f), new Vector2(-16f, -102f));
            ConfigureHorizontalRow(badgeRow, 8f);
            EnsureBadgeLikeText(badgeRow, "FaultBadge", "FAULT", UIDesignTokens.RobotControlV2.Colors.Warning, 92f);
            EnsureBadgeLikeText(badgeRow, "SafetyBadge", "SAFETY", UIDesignTokens.RobotControlV2.Colors.Success, 92f);
            EnsureBadgeLikeText(badgeRow, "ModeBadge", "MOCK", UIDesignTokens.RobotControlV2.Colors.Accent, 76f);

            var cardRow = EnsureChild(root, "CardRow");
            UiRuntimeStyle.Anchor(cardRow, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -114f), new Vector2(-16f, -182f));
            ConfigureHorizontalRow(cardRow, 8f);
            EnsureSheetAction(cardRow, "BtnRecoveryGuide", "복구 안내", UIDesignTokens.RobotControlV2.Colors.CardAlt);
            EnsureSheetAction(cardRow, "BtnHelpGuide", "도움말", UIDesignTokens.RobotControlV2.Colors.CardAlt);
        }

        private void BuildTabletModuleSheet(RectTransform root)
        {
            var bg = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            bg.color = UIDesignTokens.RobotControlV2.Colors.Card;
            EnsurePanelSprite(bg);

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, 12, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(240f, 20f), new Vector2(16f, -16f));
            title.text = "Tablet Module Sheet";

            var body = UiRuntimeStyle.EnsureText(root, "Body", fallbackFont, 11, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(body.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 20f), new Vector2(16f, -38f));
            body.text = "보조 모듈 진입 카드만 제공합니다.";

            var gridRoot = EnsureChild(root, "ModuleGrid");
            UiRuntimeStyle.Anchor(gridRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -72f), new Vector2(-16f, -184f));
            var grid = gridRoot.GetComponent<GridLayoutGroup>() ?? gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.spacing = new Vector2(8f, 8f);
            grid.padding = new RectOffset(0, 0, 0, 0);
            grid.cellSize = new Vector2(152f, 44f);

            EnsureSheetAction(gridRoot, "BtnModulePoints", "포인트", UIDesignTokens.RobotControlV2.Colors.CardAlt);
            EnsureSheetAction(gridRoot, "BtnModuleIO", "I/O", UIDesignTokens.RobotControlV2.Colors.CardAlt);
            EnsureSheetAction(gridRoot, "BtnModuleTPD", "TPD", UIDesignTokens.RobotControlV2.Colors.CardAlt);
            EnsureSheetAction(gridRoot, "BtnModuleDiag", "Diagnostics", UIDesignTokens.RobotControlV2.Colors.CardAlt);
        }

        private void BuildPopup(RectTransform root, string titleText, string bodyText, string primaryLabel, string secondaryLabel, bool visible, Color accentColor)
        {
            var bg = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.14f, 0.96f);
            EnsurePanelSprite(bg);
            UiRuntimeStyle.Anchor(root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(440f, 214f), new Vector2(0f, 0f));
            UiRuntimeStyle.SetCanvasVisible(root.gameObject, visible);

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, 14, FontStyle.Bold, TextAnchor.UpperLeft, accentColor);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 24f), new Vector2(20f, -18f));
            title.text = titleText;

            var body = UiRuntimeStyle.EnsureText(root, "Body", fallbackFont, 12, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Anchor(body.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(390f, 72f), new Vector2(20f, -52f));
            body.text = bodyText;

            var detail = UiRuntimeStyle.EnsureText(root, "Detail", fallbackFont, 11, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(detail.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(390f, 32f), new Vector2(20f, -122f));
            detail.text = "미리보기 / 위험 / 복구 안내는 이 surface에서 단계적으로 확인합니다.";

            var actionRow = EnsureChild(root, "ActionRow");
            UiRuntimeStyle.Anchor(actionRow, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(20f, 20f), new Vector2(-20f, 56f));
            ConfigureHorizontalRow(actionRow, 8f);
            EnsureSheetAction(actionRow, "BtnPrimary", primaryLabel, accentColor);
            EnsureSheetAction(actionRow, "BtnSecondary", secondaryLabel, UIDesignTokens.RobotControlV2.Colors.CardAlt);
        }

        private void BuildToastAnchor(RectTransform root)
        {
            root.gameObject.SetActive(true);
            UiRuntimeStyle.Anchor(root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(420f, 48f), new Vector2(0f, 24f));
        }

        private void BuildLayoutBoundsOverlay(RectTransform root)
        {
            var image = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.01f);
            image.raycastTarget = false;
        }

        private void ConfigureHorizontalRow(RectTransform row, float spacing)
        {
            var layout = row.GetComponent<HorizontalLayoutGroup>() ?? row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private void EnsureMiniSheetButton(Transform parent, string name, string label)
        {
            var button = parent.Find(name)?.GetComponent<Button>() ?? UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, 84f);
            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.preferredWidth = 84f;
            element.minWidth = 72f;
            element.preferredHeight = 28f;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = UIDesignTokens.RobotControlV2.Colors.CardAlt;
            }
        }

        private void EnsureSheetAction(Transform parent, string name, string label, Color color)
        {
            var button = parent.Find(name)?.GetComponent<Button>() ?? UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, 120f);
            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.preferredHeight = 36f;
            element.minHeight = 32f;
            element.flexibleWidth = 1f;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
            var text = button.transform.Find("Label")?.GetComponent<Text>();
            if (text != null)
            {
                text.fontSize = 11;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 9;
                text.resizeTextMaxSize = 11;
            }
        }

        private void EnsureBadgeLikeText(Transform parent, string name, string label, Color color, float width)
        {
            var root = parent.Find(name) as RectTransform ?? UiRuntimeStyle.EnsureRectChild(parent, name);
            var image = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            image.color = color;
            EnsurePanelSprite(image);
            var element = UiRuntimeStyle.EnsureLayoutElement(root);
            element.preferredWidth = width;
            element.minWidth = width;
            element.preferredHeight = 28f;
            var text = UiRuntimeStyle.EnsureText(root, "Label", fallbackFont, 11, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f));
            text.text = label;
        }

        private static void EnsurePanelSprite(Image image)
        {
            if (image == null || image.sprite != null)
            {
                return;
            }

            image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
        }

        private void EnsureMiniButton(RectTransform root, string name, string label, Vector2 pos, bool active, float width = 72f)
        {
            var button = root.Find(name)?.GetComponent<Button>() ?? UIComponentFactory.CreateSecondaryButton(root, name, label, fallbackFont, width);
            UiRuntimeStyle.Anchor((RectTransform)button.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(width, 28f), pos);

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = active ? UIDesignTokens.RobotControlV2.Colors.Accent : UIDesignTokens.RobotControlV2.Colors.CardAlt;
            }
        }

        private static void AnchorStackCard(RectTransform card, float topOffset, float height)
        {
            UiRuntimeStyle.Anchor(card, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(344f, height), new Vector2(0f, -topOffset));
        }

        private void EnsureRegionStyle(RectTransform rect, Color color)
        {
            var image = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            image.color = color;

            var borderTop = UiRuntimeStyle.EnsureImage(rect, "BorderTop", UIDesignTokens.RobotControlV2.Colors.Border);
            var borderRect = borderTop.rectTransform;
            borderRect.anchorMin = new Vector2(0f, 1f);
            borderRect.anchorMax = new Vector2(1f, 1f);
            borderRect.offsetMin = new Vector2(0f, -1f);
            borderRect.offsetMax = new Vector2(0f, 0f);
        }

        private static RectTransform EnsureChild(RectTransform parent, string name)
        {
            var existing = parent.Find(name) as RectTransform;
            return existing ?? UiRuntimeStyle.EnsureRectChild(parent, name);
        }

        private void NormalizeChildren(RectTransform parent, params string[] allowedNames)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                var keep = false;
                for (var j = 0; j < allowedNames.Length; j++)
                {
                    if (child.name == allowedNames[j])
                    {
                        keep = true;
                        break;
                    }
                }

                if (keep)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(child.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}
