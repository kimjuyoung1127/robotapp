// Folder: Tests/EditMode - OnboardingViewBuilder UI 생성 결과 검증
using KineTutor3D.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// OnboardingViewBuilder.Build()가 올바른 UI 구조를 생성하는지 검증합니다.
    /// </summary>
    public class OnboardingViewBuilderTests
    {
        private GameObject canvasGo;
        private RectTransform canvasRoot;

        [SetUp]
        public void SetUp()
        {
            canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasRoot = canvasGo.GetComponent<RectTransform>();
        }

        [TearDown]
        public void TearDown()
        {
            if (canvasGo != null) Object.DestroyImmediate(canvasGo);
        }

        [Test]
        public void Build_CreatesModalSurface()
        {
            var refs = OnboardingViewBuilder.Build(canvasRoot, null);

            Assert.That(refs.ModalRoot, Is.Not.Null);
            var surface = refs.ModalRoot.Find("ModalSurface");
            Assert.That(surface, Is.Not.Null);
        }

        [Test]
        public void Build_CreatesThreeButtons()
        {
            var refs = OnboardingViewBuilder.Build(canvasRoot, null);

            Assert.That(refs.BeginnerButton, Is.Not.Null);
            Assert.That(refs.StartLearningButton, Is.Not.Null);
            Assert.That(refs.SkipButton, Is.Not.Null);
        }

        [Test]
        public void Build_CreatesHeadlineAndBody()
        {
            var refs = OnboardingViewBuilder.Build(canvasRoot, null);

            Assert.That(refs.HeadlineText, Is.Not.Null);
            Assert.That(refs.HeadlineText.text, Is.EqualTo("KineTutor3D"));
            Assert.That(refs.BodyText, Is.Not.Null);
        }

        [Test]
        public void Build_CardRowContainsSelectionCards()
        {
            var refs = OnboardingViewBuilder.Build(canvasRoot, null);

            var surface = refs.ModalRoot.Find("ModalSurface");
            Assert.That(surface, Is.Not.Null);

            var cardRow = surface.Find("CardRow");
            Assert.That(cardRow, Is.Not.Null);
            Assert.That(cardRow.Find("BtnBeginner"), Is.Not.Null);
            Assert.That(cardRow.Find("BtnStartLearning"), Is.Not.Null);
        }

        [Test]
        public void Build_ScreenBgIsFirstSibling()
        {
            var refs = OnboardingViewBuilder.Build(canvasRoot, null);

            var screenBg = canvasRoot.Find("ScreenBg");
            Assert.That(screenBg, Is.Not.Null);
            Assert.That(screenBg.GetSiblingIndex(), Is.EqualTo(0));
        }

        [Test]
        public void TryBindExisting_ReusesSceneAuthoredOnboardingShell()
        {
            var builtRefs = OnboardingViewBuilder.Build(canvasRoot, null);

            var bound = OnboardingViewBuilder.TryBindExisting(canvasRoot, null, out var reboundRefs);

            Assert.That(bound, Is.True);
            Assert.That(reboundRefs.Root, Is.SameAs(builtRefs.Root));
            Assert.That(reboundRefs.ModalRoot, Is.SameAs(builtRefs.ModalRoot));
            Assert.That(reboundRefs.HeadlineText, Is.SameAs(builtRefs.HeadlineText));
            Assert.That(reboundRefs.BodyText, Is.SameAs(builtRefs.BodyText));
            Assert.That(reboundRefs.BeginnerButton, Is.SameAs(builtRefs.BeginnerButton));
            Assert.That(reboundRefs.StartLearningButton, Is.SameAs(builtRefs.StartLearningButton));
            Assert.That(reboundRefs.SkipButton, Is.SameAs(builtRefs.SkipButton));
        }

        [Test]
        public void TryBindExisting_PreservesAuthoredLayoutChanges()
        {
            var refs = OnboardingViewBuilder.Build(canvasRoot, null);

            refs.ModalRoot.anchoredPosition = new Vector2(123f, -45f);
            refs.BodyText.rectTransform.anchoredPosition = new Vector2(77f, -88f);
            refs.BodyText.text = "커스텀 문구";

            var bound = OnboardingViewBuilder.TryBindExisting(canvasRoot, null, out var reboundRefs);

            Assert.That(bound, Is.True);
            Assert.That(reboundRefs.ModalRoot.anchoredPosition, Is.EqualTo(new Vector2(123f, -45f)));
            Assert.That(reboundRefs.BodyText.rectTransform.anchoredPosition, Is.EqualTo(new Vector2(77f, -88f)));
            Assert.That(reboundRefs.BodyText.text, Is.EqualTo("커스텀 문구"));
        }
    }
}
