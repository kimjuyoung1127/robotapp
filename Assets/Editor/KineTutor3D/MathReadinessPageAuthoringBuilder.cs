// Folder: Editor - Unity Editor authoring, QA, and packaging tools.
// MathReadiness authored 페이지 레이아웃을 구성하는 에디터 빌더입니다.
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using KineTutor3D.UI;

namespace KineTutor3D.Editor
{
    /// <summary>
    /// MathReadiness 전용 authored 페이지 UI를 씬에 재구성합니다.
    /// </summary>
    internal static class MathReadinessPageAuthoringBuilder
    {
        private const string MenuPath = "KineTutor3D/MathReadiness/Author Dedicated Page UI";

        [MenuItem(MenuPath, priority = 150)]
        public static void AuthorSceneUi()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.path.EndsWith("MathReadiness.unity", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[MathReadiness] 먼저 Assets/Scenes/MathReadiness.unity 씬을 열어주세요.");
                return;
            }

            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null)
            {
                Debug.LogError("[MathReadiness] Canvas를 찾지 못했습니다.");
                return;
            }

            DestroyLegacyShell(canvas.transform);
            DestroyExisting(canvas.transform, "MathReadinessRect");
            DestroyExisting(canvas.transform, "MathReadinessPageRoot");
            RemoveStrayPanels();

            var pageRoot = BuildPageRoot(canvas.transform);
            BuildTopBar(pageRoot);
            BuildViewportPanel(pageRoot);
            BuildMainContent(pageRoot);
            BuildFooter(pageRoot);

            var panel = pageRoot.GetComponent<MathReadinessPanel>();
            if (panel == null)
            {
                panel = pageRoot.gameObject.AddComponent<MathReadinessPanel>();
            }

            EditorUtility.SetDirty(pageRoot);
            EditorUtility.SetDirty(panel);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MathReadiness] Dedicated authored page UI를 생성하고 저장했습니다.");
        }

        private static RectTransform BuildPageRoot(Transform canvasRoot)
        {
            var root = UIComponentFactory.CreatePanel(canvasRoot, "MathReadinessRect", UIDesignTokens.Colors.SurfaceOverlay);
            for (var i = root.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(root.GetChild(i).gameObject);
            }

            Stretch(root, Vector2.zero, Vector2.one, new Vector2(18f, 18f), new Vector2(-18f, -18f));
            if (root.GetComponent<CanvasGroup>() == null)
            {
                root.gameObject.AddComponent<CanvasGroup>();
            }

            return root;
        }

        private static void BuildTopBar(RectTransform pageRoot)
        {
            var topBar = UIComponentFactory.CreatePanel(pageRoot, "PageTopBar", UIDesignTokens.Colors.SurfaceRaisedAlt);
            Stretch(topBar, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -72f), new Vector2(0f, 0f));

            var title = UIComponentFactory.CreateText(topBar, "PageTitleText", TypographyPreset.DisplayLg, UIDesignTokens.Colors.TextPrimary, "Math Readiness");
            title.fontSize = UIDesignTokens.Type.DisplayLg + 4;
            title.fontStyle = FontStyle.Bold;
            Anchor(title.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(360f, 36f), new Vector2(24f, 12f));

            var context = UIComponentFactory.CreateText(topBar, "PageContextText", TypographyPreset.Body, UIDesignTokens.Colors.TextSecondary, "Lesson 1/4");
            context.fontSize = UIDesignTokens.Type.HeadingSm;
            Anchor(context.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(240f, 24f), new Vector2(28f, -16f));

            CreateTopBarButton(topBar, "BackButton", "Back", new Vector2(-400f, 0f));
            CreateTopBarButton(topBar, "HomeButton", "Home", new Vector2(-290f, 0f));
            CreateTopBarButton(topBar, "LibraryButton", "Library", new Vector2(-170f, 0f));
            CreateTopBarButton(topBar, "SandboxButton", "Sandbox", new Vector2(-30f, 0f));
        }

        private static void BuildViewportPanel(RectTransform pageRoot)
        {
            var viewportPanel = UIComponentFactory.CreatePanel(pageRoot, "PageViewportPanel", UIDesignTokens.Colors.SurfaceRaised);
            Anchor(viewportPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(440f, 96f), new Vector2(20f, -104f));

            var title = UIComponentFactory.CreateText(viewportPanel, "ViewportPanelTitleText", TypographyPreset.HeadingLg, UIDesignTokens.Colors.TextPrimary, "먼저 화면을 보고 움직여 보세요");
            title.fontSize = UIDesignTokens.Type.DisplaySm;
            title.fontStyle = FontStyle.Bold;
            Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(396f, 30f), new Vector2(20f, -20f));

            var body = UIComponentFactory.CreateText(viewportPanel, "ViewportPanelBodyText", TypographyPreset.Body, UIDesignTokens.Colors.TextSecondary, "기준선과 끝점 위치를 보면서 수학 감각을 먼저 만들어 보세요.");
            body.fontSize = UIDesignTokens.Type.HeadingSm;
            Anchor(body.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(396f, 40f), new Vector2(20f, 18f));
        }

        private static void BuildMainContent(RectTransform pageRoot)
        {
            var mainContent = EnsureRectChild(pageRoot, "MainContent");
            Stretch(mainContent, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 196f), new Vector2(0f, -124f));

            var questionColumn = UIComponentFactory.CreatePanel(mainContent, "QuestionColumn", Color.clear);
            Stretch(questionColumn, new Vector2(0f, 0f), new Vector2(0.68f, 1f), new Vector2(0f, 0f), new Vector2(-12f, 0f));

            var supportColumn = UIComponentFactory.CreatePanel(mainContent, "SupportColumn", Color.clear);
            Stretch(supportColumn, new Vector2(0.70f, 0f), new Vector2(1f, 1f), new Vector2(12f, 0f), new Vector2(0f, 0f));

            BuildOverviewCard(questionColumn);
            BuildQuestionCard(questionColumn);
            BuildSupportColumn(supportColumn);
        }

        private static void BuildFooter(RectTransform pageRoot)
        {
            var footer = UIComponentFactory.CreatePanel(pageRoot, "PageFooter", UIDesignTokens.Colors.SurfaceRaisedAlt);
            Stretch(footer, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 168f));

            var jointCard = UIComponentFactory.CreatePanel(footer, "JointControlCard", UIDesignTokens.Colors.SurfaceCard);
            Anchor(jointCard, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(640f, 132f), new Vector2(20f, 0f));

            BuildJointRow(jointCard, 0, "J1", "joint_slider_1", "Joint1Input", "Joint1ValueText", new Vector2(20f, 26f));
            BuildJointRow(jointCard, 1, "J2", "joint_slider_2", "Joint2Input", "Joint2ValueText", new Vector2(20f, -34f));

            var stepTitle = UIComponentFactory.CreateText(footer, "FooterStepTitleText", TypographyPreset.HeadingLg, UIDesignTokens.Colors.TextPrimary, "Math Readiness");
            stepTitle.fontSize = UIDesignTokens.Type.DisplaySm;
            stepTitle.fontStyle = FontStyle.Bold;
            Anchor(stepTitle.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(360f, 28f), new Vector2(-28f, -20f));

            var stepProgress = UIComponentFactory.CreateText(footer, "FooterStepProgressText", TypographyPreset.Body, UIDesignTokens.Colors.TextSecondary, "Step 1/4");
            stepProgress.fontSize = UIDesignTokens.Type.HeadingSm;
            Anchor(stepProgress.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(360f, 24f), new Vector2(-28f, -52f));

            CreateFooterButton(footer, "BtnPrev", "Prev", new Vector2(-332f, 26f), false);
            CreateFooterButton(footer, "BtnSkip", "Skip", new Vector2(-212f, 26f), false);
            CreateFooterButton(footer, "BtnNext", "Next", new Vector2(-76f, 26f), true);
        }

        private static void DestroyLegacyShell(Transform canvasRoot)
        {
            var legacyNames = new[]
            {
                "LeftPanelRect",
                "BeginnerLeftRect",
                "RightPanelRect",
                "WhyItMovedRect",
                "TargetFeedbackRect",
                "BottomBarRect",
                "TopBarRect",
                "TopBar",
                "LeftPanel",
                "RightPanel",
                "BottomBar"
            };

            foreach (var name in legacyNames)
            {
                var target = canvasRoot.Find(name);
                if (target != null)
                {
                    UnityEngine.Object.DestroyImmediate(target.gameObject);
                }
            }
        }

        private static void DestroyExisting(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void RemoveStrayPanels()
        {
            foreach (var panel in UnityEngine.Object.FindObjectsByType<MathReadinessPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (panel != null)
                {
                    UnityEngine.Object.DestroyImmediate(panel);
                }
            }
        }

        private static void BuildOverviewCard(RectTransform questionColumn)
        {
            var overview = UIComponentFactory.CreatePanel(questionColumn, "OverviewCard", UIDesignTokens.Colors.SurfaceCard);
            Stretch(overview, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -190f), Vector2.zero);

            var stripe = EnsureImage(overview, "MRP_ConceptStripe", UIDesignTokens.Colors.ConceptOrange);
            Stretch((RectTransform)stripe.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -6f), Vector2.zero);

            CreateCardText(overview, "MRP_OverviewLabel", "현재 학습", UIDesignTokens.Type.HeadingSm, UIDesignTokens.Colors.TextMuted, new Vector2(20f, -18f), new Vector2(200f, 22f), FontStyle.Bold);
            CreateCardText(overview, "MRP_LessonTitle", "각도는 방향이다", UIDesignTokens.Type.DisplaySm + 2, UIDesignTokens.Colors.TextPrimary, new Vector2(20f, -48f), new Vector2(500f, 34f), FontStyle.Bold);
            CreateCardText(overview, "MRP_LessonGoal", "먼저 슬라이더를 움직여 팔이 향하는 방향을 직접 봅니다.", UIDesignTokens.Type.HeadingSm + 2, UIDesignTokens.Colors.TextPrimary, new Vector2(20f, -90f), new Vector2(520f, 40f), FontStyle.Bold);
            CreateCardText(overview, "MRP_Intro", "0°/90°/180° 기준선을 보고 목표 각도로 먼저 움직여 보세요.", UIDesignTokens.Type.HeadingSm, UIDesignTokens.Colors.TextSecondary, new Vector2(20f, -138f), new Vector2(520f, 30f), FontStyle.Normal);
        }

        private static void BuildQuestionCard(RectTransform questionColumn)
        {
            var questionCard = UIComponentFactory.CreatePanel(questionColumn, "QuestionCard", UIDesignTokens.Colors.SurfaceRaisedAlt);
            Stretch(questionCard, Vector2.zero, new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, -206f));

            CreateCardText(questionCard, "QuestionSectionLabel", "확인 질문", UIDesignTokens.Type.HeadingSm, UIDesignTokens.Colors.TextMuted, new Vector2(20f, -18f), new Vector2(220f, 24f), FontStyle.Bold);
            var progressBadge = UIComponentFactory.CreateBadge(questionCard, "MRP_ProgressBadge", "Q1/1", UIDesignTokens.Colors.AccentSecondary);
            Anchor(progressBadge, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(92f, UIDesignTokens.Size.BadgeHeight + 6f), new Vector2(-20f, -18f));

            var questionStack = EnsureRectChild(questionCard, "QuestionStack");
            Stretch(questionStack, Vector2.zero, Vector2.one, new Vector2(20f, 20f), new Vector2(-20f, -58f));
            var stackLayout = EnsureVerticalLayout(questionStack.gameObject, UIDesignTokens.Space.Md, false);
            stackLayout.childControlHeight = false;
            stackLayout.childForceExpandHeight = false;

            var warmupBlock = UIComponentFactory.CreatePanel(questionStack, "WarmupBlock", UIDesignTokens.Colors.SurfaceCard);
            EnsureLayoutElement(warmupBlock).preferredHeight = 214f;
            CreateCardText(warmupBlock, "MRP_WarmupLabel", "워밍업", UIDesignTokens.Type.HeadingSm, UIDesignTokens.Colors.TextMuted, new Vector2(16f, -14f), new Vector2(200f, 24f), FontStyle.Bold);
            CreateCardText(warmupBlock, "MRP_WarmupPrompt", "기준 각도를 먼저 살펴볼게요.", UIDesignTokens.Type.DisplaySm, UIDesignTokens.Colors.TextPrimary, new Vector2(16f, -44f), new Vector2(520f, 30f), FontStyle.Bold);
            CreateCardText(warmupBlock, "MRP_WarmupFeedback", string.Empty, UIDesignTokens.Type.Body, UIDesignTokens.Colors.TextMuted, new Vector2(16f, -170f), new Vector2(520f, 24f), FontStyle.Italic);
            CreateChoiceButton(warmupBlock, "BtnWarmupChoice_0", "오른쪽(0°)", new Vector2(16f, -86f));
            CreateChoiceButton(warmupBlock, "BtnWarmupChoice_1", "위쪽(90°)", new Vector2(16f, -126f));
            CreateChoiceButton(warmupBlock, "BtnWarmupChoice_2", "왼쪽(180°)", new Vector2(16f, -166f));

            var manipulationBlock = UIComponentFactory.CreatePanel(questionStack, "ManipulationBlock", UIDesignTokens.Colors.SurfaceCard);
            EnsureLayoutElement(manipulationBlock).preferredHeight = 104f;
            CreateCardText(manipulationBlock, "MRP_ManipulationInstruction", "J1 슬라이더를 90°로 옮겨보세요.", UIDesignTokens.Type.DisplaySm, UIDesignTokens.Colors.TextPrimary, new Vector2(16f, -18f), new Vector2(440f, 52f), FontStyle.Bold);
            var targetBadge = UIComponentFactory.CreateBadge(manipulationBlock, "MRP_TargetBadge", "목표: J1 90°", UIDesignTokens.Colors.AccentPrimary);
            Anchor(targetBadge, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(184f, 28f), new Vector2(-16f, -18f));

            var questionPrompt = UIComponentFactory.CreateText(questionStack, "MRP_QuestionPrompt", TypographyPreset.HeadingLg, UIDesignTokens.Colors.TextPrimary, "방금 팔이 향한 방향은 어디에 가장 가까웠나요?");
            questionPrompt.fontSize = UIDesignTokens.Type.DisplaySm;
            questionPrompt.fontStyle = FontStyle.Bold;
            EnsureLayoutElement(questionPrompt).preferredHeight = 36f;

            var answerButtons = EnsureRectChild(questionStack, "AnswerButtons");
            EnsureLayoutElement(answerButtons).preferredHeight = 170f;
            var answerLayout = EnsureVerticalLayout(answerButtons.gameObject, UIDesignTokens.Space.Sm, false);
            answerLayout.childControlHeight = false;
            CreateChoiceButton(answerButtons, "BtnReadinessChoice_0", "오른쪽(0°)", Vector2.zero, true);
            CreateChoiceButton(answerButtons, "BtnReadinessChoice_1", "위쪽(90°)", Vector2.zero, true);
            CreateChoiceButton(answerButtons, "BtnReadinessChoice_2", "왼쪽(180°)", Vector2.zero, true);

            var feedbackRow = EnsureRectChild(questionStack, "FeedbackRow");
            EnsureLayoutElement(feedbackRow).preferredHeight = 42f;
            var feedbackIcon = UIIconResolver.CreateIcon(feedbackRow, "MRP_FeedbackIcon", "icon-check", UIDesignTokens.Size.IconMd, UIDesignTokens.Colors.AccentSuccess);
            Anchor(feedbackIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(UIDesignTokens.Size.IconMd, UIDesignTokens.Size.IconMd), new Vector2(16f, 0f));
            feedbackIcon.gameObject.SetActive(false);
            var feedbackText = UIComponentFactory.CreateText(feedbackRow, "MRP_FeedbackText", TypographyPreset.Body, UIDesignTokens.Colors.TextSecondary, string.Empty);
            feedbackText.fontSize = UIDesignTokens.Type.HeadingSm;
            Anchor(feedbackText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(500f, 28f), new Vector2(52f, 0f));

            var adaptiveHint = UIComponentFactory.CreateText(questionStack, "MRP_AdaptiveHint", TypographyPreset.Body, UIDesignTokens.Colors.AccentWarning, string.Empty);
            adaptiveHint.fontSize = UIDesignTokens.Type.HeadingSm;
            adaptiveHint.fontStyle = FontStyle.Bold;
            EnsureLayoutElement(adaptiveHint).preferredHeight = 28f;
        }

        private static void BuildSupportColumn(RectTransform supportColumn)
        {
            var coachCard = UIComponentFactory.CreatePanel(supportColumn, "CoachCard", UIDesignTokens.Colors.SurfaceCard);
            Stretch(coachCard, new Vector2(0f, 0.45f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            CreateCardText(coachCard, "MRP_CommonMistake", "많이 헷갈리는 포인트", UIDesignTokens.Type.HeadingSm, UIDesignTokens.Colors.TextSecondary, new Vector2(16f, -18f), new Vector2(280f, 54f), FontStyle.Normal);
            var coachButton = UIComponentFactory.CreateSecondaryButton(coachCard, "BtnToggleCoachHint", "교사 힌트 보기", null, 224f);
            StyleButtonLabel(coachButton, UIDesignTokens.Type.HeadingSm);
            Anchor((RectTransform)coachButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(224f, UIDesignTokens.Size.ButtonHeightLg), new Vector2(16f, 18f));
            UIComponentFactory.AttachLeadingIcon(coachButton, "icon-help", UIDesignTokens.Size.IconSm);
            CreateCardText(coachCard, "MRP_CoachHintBody", "교사 힌트", UIDesignTokens.Type.HeadingSm, UIDesignTokens.Colors.TextMuted, new Vector2(16f, -118f), new Vector2(280f, 88f), FontStyle.Italic);

            var whyMoved = UIComponentFactory.CreatePanel(supportColumn, "WhyMovedSummaryCard", UIDesignTokens.Colors.SurfaceCard);
            Stretch(whyMoved, new Vector2(0f, 0f), new Vector2(1f, 0.41f), Vector2.zero, Vector2.zero);
            CreateCardText(whyMoved, "WhyMovedSummaryTitleText", "움직임 요약", UIDesignTokens.Type.HeadingLg, UIDesignTokens.Colors.TextPrimary, new Vector2(16f, -18f), new Vector2(280f, 24f), FontStyle.Bold);
            CreateCardText(whyMoved, "WhyMovedSummaryText", "관절을 움직여 방향 변화를 확인해 보세요.", UIDesignTokens.Type.HeadingSm, UIDesignTokens.Colors.TextSecondary, new Vector2(16f, -56f), new Vector2(280f, 120f), FontStyle.Normal);
        }

        private static void BuildJointRow(Transform parent, int index, string label, string sliderName, string inputName, string valueName, Vector2 anchoredPosition)
        {
            var jointLabel = UIComponentFactory.CreateText(parent, $"Joint{index + 1}LabelText", TypographyPreset.Body, UIDesignTokens.Colors.TextPrimary, label);
            jointLabel.fontSize = UIDesignTokens.Type.HeadingSm;
            jointLabel.fontStyle = FontStyle.Bold;
            Anchor(jointLabel.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 24f), new Vector2(18f, anchoredPosition.y));

            var slider = UIComponentFactory.CreateSlider(parent, sliderName, -180f, 180f);
            Anchor((RectTransform)slider.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(320f, UIDesignTokens.Size.SliderHeight), new Vector2(86f, anchoredPosition.y));

            var input = UIComponentFactory.CreateInputField(parent, inputName, "0.0");
            Anchor((RectTransform)input.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(92f, UIDesignTokens.Size.ButtonHeightLg), new Vector2(430f, anchoredPosition.y));

            var value = UIComponentFactory.CreateText(parent, valueName, TypographyPreset.Body, UIDesignTokens.Colors.TextSecondary, $"{label}: 0.0");
            value.fontSize = UIDesignTokens.Type.HeadingSm;
            Anchor(value.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(140f, 24f), new Vector2(532f, anchoredPosition.y));
        }

        private static void CreateTopBarButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var button = UIComponentFactory.CreateSecondaryButton(parent, name, label, null, 112f);
            StyleButtonLabel(button, UIDesignTokens.Type.HeadingSm);
            Anchor((RectTransform)button.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(112f, UIDesignTokens.Size.ButtonHeightLg), anchoredPosition);
        }

        private static void CreateFooterButton(Transform parent, string name, string label, Vector2 anchoredPosition, bool primary)
        {
            var button = primary
                ? UIComponentFactory.CreatePrimaryButton(parent, name, label, null, 112f)
                : UIComponentFactory.CreateSecondaryButton(parent, name, label, null, 96f);
            StyleButtonLabel(button, UIDesignTokens.Type.HeadingSm);
            var width = primary ? 112f : 96f;
            Anchor((RectTransform)button.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(width, UIDesignTokens.Size.ButtonHeightLg), anchoredPosition);
        }

        private static void CreateChoiceButton(Transform parent, string name, string label, Vector2 anchoredPosition, bool useLayout = false)
        {
            var button = UIComponentFactory.CreateSecondaryButton(parent, name, label, null, 0f);
            StyleButtonLabel(button, UIDesignTokens.Type.HeadingSm);
            var rect = (RectTransform)button.transform;
            if (useLayout)
            {
                EnsureLayoutElement(rect).preferredHeight = UIDesignTokens.Size.ButtonHeightLg + 6f;
            }
            else
            {
                Anchor(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(520f, UIDesignTokens.Size.ButtonHeightLg + 4f), anchoredPosition);
            }
        }

        private static Text CreateCardText(Transform parent, string name, string content, int fontSize, Color color, Vector2 anchoredPosition, Vector2 size, FontStyle style)
        {
            var text = UIComponentFactory.CreateText(parent, name, TypographyPreset.Body, color, content);
            text.fontSize = fontSize;
            text.fontStyle = style;
            Anchor(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), size, anchoredPosition);
            return text;
        }

        private static void StyleButtonLabel(Button button, int fontSize)
        {
            var label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.fontSize = fontSize;
                label.fontStyle = FontStyle.Bold;
            }
        }

        private static RectTransform EnsureRectChild(Transform parent, string name)
        {
            var existing = parent.Find(name) as RectTransform;
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static Image EnsureImage(Transform parent, string name, Color color)
        {
            var rect = EnsureRectChild(parent, name);
            var image = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static LayoutElement EnsureLayoutElement(Component target)
        {
            return target.GetComponent<LayoutElement>() ?? target.gameObject.AddComponent<LayoutElement>();
        }

        private static VerticalLayoutGroup EnsureVerticalLayout(GameObject target, float spacing, bool controlHeight)
        {
            var layout = target.GetComponent<VerticalLayoutGroup>() ?? target.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = controlHeight;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;
            return layout;
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void Anchor(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPosition)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;
        }
    }
}
