// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// UIComponentFactory 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// UIComponentFactory 출력 계층 및 터치 타깃 검증.
    /// </summary>
    public class UIComponentFactoryTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestRoot", typeof(RectTransform));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void CreatePanel_ReturnsNonNull()
        {
            var panel = UIComponentFactory.CreatePanel(_root.transform, "TestPanel", UIDesignTokens.Colors.SurfaceCard);
            Assert.IsNotNull(panel);
        }

        [Test]
        public void CreatePanel_HasBackground()
        {
            var panel = UIComponentFactory.CreatePanel(_root.transform, "TestPanel", UIDesignTokens.Colors.SurfaceCard);
            var bg = panel.Find("Bg");
            Assert.IsNotNull(bg);
            var image = bg.GetComponent<Image>();
            Assert.IsNotNull(image);
        }

        [Test]
        public void CreateCardPanel_UsesSurfaceCardColor()
        {
            var card = UIComponentFactory.CreateCardPanel(_root.transform, "TestCard");
            var bg = card.Find("Bg");
            Assert.IsNotNull(bg);
            var image = bg.GetComponent<Image>();
            Assert.AreEqual(UIDesignTokens.Colors.SurfaceCard, image.color);
        }

        [Test]
        public void CreatePrimaryButton_HeightMeetsTouchTarget()
        {
            var button = UIComponentFactory.CreatePrimaryButton(_root.transform, "TestBtn", "Test");
            var rect = button.GetComponent<RectTransform>();
            Assert.GreaterOrEqual(rect.sizeDelta.y, UIDesignTokens.Size.TouchTargetMin);
        }

        [Test]
        public void CreateSecondaryButton_ReturnsButton()
        {
            var button = UIComponentFactory.CreateSecondaryButton(_root.transform, "TestBtn2", "Test");
            Assert.IsNotNull(button);
            Assert.IsNotNull(button.GetComponent<RectTransform>());
        }

        [Test]
        public void CreateText_ReturnsTextWithPreset()
        {
            var text = UIComponentFactory.CreateText(
                _root.transform, "TestText",
                TypographyPreset.HeadingLg,
                UIDesignTokens.Colors.TextPrimary,
                "Hello");
            Assert.IsNotNull(text);
            Assert.AreEqual(UIDesignTokens.Type.HeadingLg, text.fontSize);
            Assert.AreEqual("Hello", text.text);
        }

        [Test]
        public void CreateBadge_HasLabelChild()
        {
            var badge = UIComponentFactory.CreateBadge(
                _root.transform, "TestBadge", "Easy",
                UIDesignTokens.Colors.DifficultyEasy);
            var label = badge.Find("Label");
            Assert.IsNotNull(label);
            var text = label.GetComponent<Text>();
            Assert.IsNotNull(text);
            Assert.AreEqual("Easy", text.text);
        }

        [Test]
        public void CreateVStack_HasVerticalLayoutGroup()
        {
            var vlg = UIComponentFactory.CreateVStack(_root.transform, "TestVStack");
            Assert.IsNotNull(vlg);
            Assert.AreEqual(UIDesignTokens.Space.Xs, vlg.spacing);
        }

        [Test]
        public void CreateHStack_HasHorizontalLayoutGroup()
        {
            var hlg = UIComponentFactory.CreateHStack(_root.transform, "TestHStack");
            Assert.IsNotNull(hlg);
            Assert.AreEqual(UIDesignTokens.Space.Xs, hlg.spacing);
        }

        [Test]
        public void CreateDivider_HasImage()
        {
            var divider = UIComponentFactory.CreateDivider(_root.transform, "TestDivider");
            Assert.IsNotNull(divider);
            Assert.AreEqual(UIDesignTokens.Colors.BorderSoft, divider.color);
        }

        [Test]
        public void CreateModalBackdrop_HasOverlay()
        {
            var backdrop = UIComponentFactory.CreateModalBackdrop(_root.transform, "TestBackdrop");
            Assert.IsNotNull(backdrop);
            var overlay = backdrop.Find("Overlay");
            Assert.IsNotNull(overlay);
        }

        [Test]
        public void CreateSlider_HasMinMax()
        {
            var slider = UIComponentFactory.CreateSlider(_root.transform, "TestSlider", -180f, 180f);
            Assert.IsNotNull(slider);
            Assert.AreEqual(-180f, slider.minValue);
            Assert.AreEqual(180f, slider.maxValue);
        }

        [Test]
        public void CreateInputField_HasPlaceholder()
        {
            var input = UIComponentFactory.CreateInputField(_root.transform, "TestInput", "Enter value...");
            Assert.IsNotNull(input);
            Assert.IsNotNull(input.placeholder);
        }

        [Test]
        public void AttachLeadingIcon_CreatesIconChild()
        {
            var button = UIComponentFactory.CreateSecondaryButton(_root.transform, "TestIconBtn", "Search");
            var icon = UIComponentFactory.AttachLeadingIcon(button, "icon-search");

            Assert.IsNotNull(icon);
            Assert.IsNotNull(button.transform.Find("LeadingIcon"));
        }

        [Test]
        public void CreateButtonRow_HasHorizontalLayoutGroup()
        {
            var row = UIComponentFactory.CreateButtonRow(_root.transform, "ButtonRow");

            Assert.IsNotNull(row);
            Assert.IsNotNull(row.GetComponent<HorizontalLayoutGroup>());
        }

        // ── UILayoutProfile ──────────────────────────────────────────────

        [Test]
        public void LayoutProfile_LeftPanelWidth_Positive()
        {
            Assert.Greater(UILayoutProfile.LeftPanelWidth, 0f);
        }

        [Test]
        public void LayoutProfile_RightPanelWidth_Positive()
        {
            Assert.Greater(UILayoutProfile.RightPanelWidth, 0f);
        }

        [Test]
        public void LayoutProfile_TouchTarget_MeetsTouchTargetMin()
        {
            Assert.GreaterOrEqual(UILayoutProfile.TouchTarget, UIDesignTokens.Size.TouchTargetMin);
        }
    }
}
