// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 온보딩 화면 빌드 결과 참조.
    /// </summary>
    internal readonly struct OnboardingViewRefs
    {
        public OnboardingViewRefs(
            RectTransform root,
            RectTransform modalRoot,
            Text headlineText,
            Text bodyText,
            Button beginnerButton,
            Button startLearningButton,
            Button skipButton)
        {
            Root = root;
            ModalRoot = modalRoot;
            HeadlineText = headlineText;
            BodyText = bodyText;
            BeginnerButton = beginnerButton;
            StartLearningButton = startLearningButton;
            SkipButton = skipButton;
        }

        public RectTransform Root { get; }
        public RectTransform ModalRoot { get; }
        public Text HeadlineText { get; }
        public Text BodyText { get; }
        public Button BeginnerButton { get; }
        public Button StartLearningButton { get; }
        public Button SkipButton { get; }
    }

    /// <summary>
    /// 온보딩 화면 UI를 UIComponentFactory/UIDesignTokens 기반으로 빌드합니다.
    /// </summary>
    internal static class OnboardingViewBuilder
    {
        private static readonly Vector2 ModalSize = new(720f, 500f);
        private static readonly Vector2 CardSize = new(290f, 230f);
        private const float CardSpacing = 24f;

        public static bool TryBindExisting(RectTransform canvasRoot, Font font, out OnboardingViewRefs refs)
        {
            refs = default;
            if (canvasRoot == null)
            {
                return false;
            }

            font = UiRuntimeStyle.ResolveFont(font);

            var screenBg = canvasRoot.Find("ScreenBg") as RectTransform;
            var modalRoot = canvasRoot.Find("WelcomeModal") as RectTransform;
            var surface = modalRoot?.Find("ModalSurface") as RectTransform;
            var headline = surface?.Find("HeadlineText")?.GetComponent<Text>();
            var body = surface?.Find("BodyText")?.GetComponent<Text>();
            var beginnerButton = surface?.Find("CardRow/BtnBeginner")?.GetComponent<Button>();
            var startLearningButton = surface?.Find("CardRow/BtnStartLearning")?.GetComponent<Button>();
            var skipButton = surface?.Find("BtnOnboardingSkip")?.GetComponent<Button>();
            var cardRow = surface?.Find("CardRow") as RectTransform;

            if (screenBg == null
                || modalRoot == null
                || surface == null
                || cardRow == null
                || headline == null
                || body == null
                || beginnerButton == null
                || startLearningButton == null
                || skipButton == null)
            {
                return false;
            }

            NormalizeExistingShell(canvasRoot, screenBg, modalRoot, surface, cardRow, body, beginnerButton, startLearningButton, skipButton);

            var bodyLabel = body;
            bodyLabel.text = "어떤 수준에서 시작할까요?";

            headline.text = "KineTutor3D";
            headline.gameObject.SetActive(false);

            var skipLabel = skipButton.transform.Find("Label")?.GetComponent<Text>();
            if (skipLabel != null)
            {
                skipLabel.color = UIDesignTokens.Colors.TextMuted;
            }

            screenBg.SetAsFirstSibling();
            modalRoot.SetAsLastSibling();

            refs = new OnboardingViewRefs(canvasRoot, modalRoot, headline, body, beginnerButton, startLearningButton, skipButton);
            return true;
        }

        private static void NormalizeExistingShell(
            RectTransform canvasRoot,
            RectTransform screenBg,
            RectTransform modalRoot,
            RectTransform surface,
            RectTransform cardRow,
            Text body,
            Button beginnerButton,
            Button startLearningButton,
            Button skipButton)
        {
            UiRuntimeStyle.Stretch(screenBg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UiRuntimeStyle.Anchor(modalRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), ModalSize, Vector2.zero);
            UiRuntimeStyle.Stretch(surface, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var surfaceBg = surface.Find("Bg");
            if (surfaceBg != null)
            {
                ((RectTransform)surfaceBg).SetAsFirstSibling();
                UiRuntimeStyle.Stretch((RectTransform)surfaceBg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }

            UiRuntimeStyle.Anchor(body.rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(540f, 40f), new Vector2(0f, -56f));

            var hstack = cardRow.GetComponent<HorizontalLayoutGroup>() ?? cardRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hstack.spacing = CardSpacing;
            hstack.childAlignment = TextAnchor.MiddleCenter;
            hstack.childControlWidth = false;
            hstack.childControlHeight = false;
            hstack.childForceExpandWidth = false;
            hstack.childForceExpandHeight = false;

            UiRuntimeStyle.Anchor(cardRow,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(620f, 240f), new Vector2(0f, -10f));

            NormalizeExistingCard(beginnerButton.transform as RectTransform, UIDesignTokens.Colors.AccentSecondary);
            NormalizeExistingCard(startLearningButton.transform as RectTransform, UIDesignTokens.Colors.AccentPrimary);

            var skipRect = skipButton.transform as RectTransform;
            if (skipRect != null)
            {
                UiRuntimeStyle.Anchor(skipRect,
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(140f, 36f), new Vector2(0f, 16f));
            }

            screenBg.SetAsFirstSibling();
            modalRoot.SetAsLastSibling();
        }

        private static void NormalizeExistingCard(RectTransform cardRect, Color accent)
        {
            if (cardRect == null)
            {
                return;
            }

            cardRect.sizeDelta = CardSize;
            var layout = cardRect.GetComponent<LayoutElement>() ?? cardRect.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = CardSize.x;
            layout.minHeight = CardSize.y;
            layout.preferredWidth = CardSize.x;
            layout.preferredHeight = CardSize.y;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;

            var hitArea = cardRect.GetComponent<Image>() ?? cardRect.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;

            var button = cardRect.GetComponent<Button>();
            if (button != null)
            {
                button.targetGraphic = hitArea;
                button.colors = UIDesignTokens.ButtonColors(UIDesignTokens.Colors.SurfaceCard);
            }

            var bg = cardRect.Find("CardBg") as RectTransform;
            if (bg != null)
            {
                UiRuntimeStyle.Stretch(bg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var bgImage = bg.Find("Bg") as RectTransform;
                if (bgImage != null)
                {
                    UiRuntimeStyle.Stretch(bgImage, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                }
            }

            var stripe = cardRect.Find("AccentStripe") as RectTransform;
            if (stripe != null)
            {
                UiRuntimeStyle.Anchor(stripe,
                    new Vector2(0f, 1f), new Vector2(1f, 1f),
                    new Vector2(0f, 4f), Vector2.zero);
            }

            var stripeBg = stripe?.Find("Stripe") as RectTransform;
            if (stripeBg != null)
            {
                UiRuntimeStyle.Stretch(stripeBg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var stripeImage = stripeBg.GetComponent<Image>();
                if (stripeImage != null)
                {
                    stripeImage.color = accent;
                }
            }

            var icon = cardRect.Find("CardIcon")?.GetComponent<Image>();
            if (icon != null)
            {
                UiRuntimeStyle.Anchor(icon.rectTransform,
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(40f, 40f), new Vector2(0f, -UIDesignTokens.Space.Lg));
                icon.color = accent;
            }

            var title = cardRect.Find("CardTitle")?.GetComponent<Text>();
            if (title != null)
            {
                UiRuntimeStyle.Anchor(title.rectTransform,
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(250f, 30f), new Vector2(0f, -78f));
            }

            var desc = cardRect.Find("CardDesc")?.GetComponent<Text>();
            if (desc != null)
            {
                UiRuntimeStyle.Anchor(desc.rectTransform,
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(240f, 70f), new Vector2(0f, -116f));
            }

            var hover = cardRect.Find("HoverLabel")?.GetComponent<Text>();
            if (hover != null)
            {
                UiRuntimeStyle.Anchor(hover.rectTransform,
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(200f, 24f), new Vector2(0f, UIDesignTokens.Space.Sm));
                hover.color = accent;
            }
        }

        public static OnboardingViewRefs Build(RectTransform canvasRoot, Font font)
        {
            font = UiRuntimeStyle.ResolveFont(font);

            // Screen background
            var screenBg = UIComponentFactory.CreatePanel(canvasRoot, "ScreenBg", UIDesignTokens.Colors.SurfaceBase);
            UiRuntimeStyle.Stretch(screenBg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            screenBg.SetAsFirstSibling();

            // Modal root
            var modalRoot = UiRuntimeStyle.EnsureRectChild(canvasRoot, "WelcomeModal");
            UiRuntimeStyle.Anchor(modalRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), ModalSize, Vector2.zero);
            modalRoot.SetAsLastSibling();

            // Modal surface
            var surface = UIComponentFactory.CreatePanel(modalRoot, "ModalSurface", UIDesignTokens.Colors.SurfaceRaised);
            UiRuntimeStyle.Stretch(surface, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var surfaceBg = surface.Find("Bg");
            if (surfaceBg != null) surfaceBg.SetAsFirstSibling();

            // Headline — TopBar에 앱 이름이 이미 있으므로 숨김 처리
            var headline = UIComponentFactory.CreateText(surface, "HeadlineText",
                TypographyPreset.HeadingLg, UIDesignTokens.Colors.TextPrimary, "KineTutor3D", font);
            UiRuntimeStyle.Anchor(headline.rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(320f, 40f), new Vector2(0f, -20f));
            headline.gameObject.SetActive(false);

            // Body
            var body = UIComponentFactory.CreateText(surface, "BodyText",
                TypographyPreset.HeadingLg, UIDesignTokens.Colors.TextPrimary,
                "\uc5b4\ub5a4 \uc218\uc900\uc5d0\uc11c \uc2dc\uc791\ud560\uae4c\uc694?", font);
            UiRuntimeStyle.Anchor(body.rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(540f, 40f), new Vector2(0f, -56f));

            // Card row
            var hstack = UIComponentFactory.CreateHStack(surface, "CardRow", CardSpacing);
            hstack.childAlignment = TextAnchor.MiddleCenter;
            hstack.childControlWidth = false;
            hstack.childControlHeight = false;
            var cardRow = (RectTransform)hstack.transform;
            UiRuntimeStyle.Anchor(cardRow,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(620f, 240f), new Vector2(0f, -10f));

            // Selection cards
            var beginnerBtn = BuildSelectionCard(cardRow, "BtnBeginner",
                "icon-user", "\ucc98\uc74c\uc774\uc5d0\uc694",
                "\uac10\uac01\ubd80\ud130 \uc2dc\uc791\ud574\uc694.\n\ub85c\ubd07 \ud314\uc744 \uc6c0\uc9c1\uc5ec\ubcf4\uba70\n\uc9c1\uad00\uc744 \uc313\uc544\uac11\ub2c8\ub2e4.",
                UIDesignTokens.Colors.AccentSecondary, font);

            var startBtn = BuildSelectionCard(cardRow, "BtnStartLearning",
                "icon-play", "\uc54c\uace0 \uc788\uc5b4\uc694",
                "DH \ud30c\ub77c\ubbf8\ud130\uc640 \ud589\ub82c\ub85c\n\ubc14\ub85c \ub4e4\uc5b4\uac11\ub2c8\ub2e4.\n\uae30\uad6c\ud559 \ud559\uc2b5\uc744 \uc2dc\uc791\ud558\uc138\uc694.",
                UIDesignTokens.Colors.AccentPrimary, font);

            // Skip button
            var skipBtn = UIComponentFactory.CreateGhostButton(surface, "BtnOnboardingSkip",
                "\ub458\ub7ec\ubcf4\uae30 \u2192");
            var skipRect = skipBtn.transform as RectTransform;
            if (skipRect != null)
            {
                UiRuntimeStyle.Anchor(skipRect,
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(140f, 36f), new Vector2(0f, 16f));
            }

            var skipLabel = skipBtn.transform.Find("Label")?.GetComponent<Text>();
            if (skipLabel != null)
            {
                skipLabel.color = UIDesignTokens.Colors.TextMuted;
            }

            return new OnboardingViewRefs(canvasRoot, modalRoot, headline, body,
                beginnerBtn, startBtn, skipBtn);
        }

        private static Button BuildSelectionCard(Transform parent, string name,
            string iconName, string titleText, string descText, Color accent, Font font)
        {
            var cardRect = UiRuntimeStyle.EnsureRectChild(parent, name);
            cardRect.sizeDelta = CardSize;
            var layout = cardRect.GetComponent<LayoutElement>() ?? cardRect.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = CardSize.x;
            layout.minHeight = CardSize.y;
            layout.preferredWidth = CardSize.x;
            layout.preferredHeight = CardSize.y;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;

            // Hit area
            var hitArea = cardRect.GetComponent<Image>() ?? cardRect.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;

            // Card background
            var bg = UIComponentFactory.CreatePanel(cardRect, "CardBg", UIDesignTokens.Colors.SurfaceCard);
            UiRuntimeStyle.Stretch(bg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Accent stripe
            var stripeRect = UiRuntimeStyle.EnsureRectChild(cardRect, "AccentStripe");
            var stripeImg = UiRuntimeStyle.EnsureImage(stripeRect, "Stripe", accent);
            UiRuntimeStyle.Anchor((RectTransform)stripeImg.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, 4f), Vector2.zero);

            // Icon
            var icon = UIIconResolver.CreateIcon(cardRect, "CardIcon", iconName,
                UIDesignTokens.Size.IconLg + 8f, accent);
            if (icon != null)
            {
                UiRuntimeStyle.Anchor(icon.rectTransform,
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(40f, 40f), new Vector2(0f, -UIDesignTokens.Space.Lg));
            }

            // Title
            var title = UIComponentFactory.CreateText(cardRect, "CardTitle",
                TypographyPreset.HeadingLg, UIDesignTokens.Colors.TextPrimary, titleText, font);
            UiRuntimeStyle.Anchor(title.rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(250f, 30f), new Vector2(0f, -78f));

            // Description
            var desc = UIComponentFactory.CreateText(cardRect, "CardDesc",
                TypographyPreset.Body, UIDesignTokens.Colors.TextSecondary, descText, font);
            UiRuntimeStyle.Anchor(desc.rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(240f, 70f), new Vector2(0f, -116f));

            // Button
            var button = cardRect.GetComponent<Button>() ?? cardRect.gameObject.AddComponent<Button>();
            button.targetGraphic = hitArea;
            button.colors = UIDesignTokens.ButtonColors(UIDesignTokens.Colors.SurfaceCard);

            // Hover label
            var hover = UIComponentFactory.CreateText(cardRect, "HoverLabel",
                TypographyPreset.Caption, accent, "\uc120\ud0dd\ud558\uae30 \u2192", font);
            UiRuntimeStyle.Anchor(hover.rectTransform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(200f, 24f), new Vector2(0f, UIDesignTokens.Space.Sm));

            return button;
        }
    }
}
