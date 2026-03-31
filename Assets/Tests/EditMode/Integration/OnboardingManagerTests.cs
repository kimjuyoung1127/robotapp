// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// OnboardingManager 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.UI;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.Tests.EditMode
{
    public class OnboardingManagerTests
    {
        private GameObject _canvasRoot;
        private OnboardingManager _manager;
        private SceneNavigationBar _sceneNavigationBar;

        [SetUp]
        public void SetUp()
        {
            _canvasRoot = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            var navHost = new GameObject("TopBar");
            navHost.transform.SetParent(_canvasRoot.transform, false);
            _sceneNavigationBar = navHost.AddComponent<SceneNavigationBar>();

            _manager = _canvasRoot.AddComponent<OnboardingManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvasRoot != null)
            {
                Object.DestroyImmediate(_canvasRoot);
            }
        }

        [Test]
        public void EnsurePresentation_UsesChildSceneNavigationBar()
        {
            InvokeEnsurePresentation();

            Assert.That(_sceneNavigationBar.enabled, Is.True);
            Assert.That(_canvasRoot.transform.Find("TopBarRect"), Is.Not.Null);
            Assert.That(_canvasRoot.transform.Find("TopBarRect").gameObject.activeSelf, Is.True);
        }

        [Test]
        public void EnsurePresentation_BringsTopBarInFrontOfWelcomeModal()
        {
            InvokeEnsurePresentation();

            var topBarRect = _canvasRoot.transform.Find("TopBarRect");
            var welcomeModal = _canvasRoot.transform.Find("WelcomeModal");

            Assert.That(topBarRect, Is.Not.Null);
            Assert.That(welcomeModal, Is.Not.Null);
            Assert.That(topBarRect.GetSiblingIndex(), Is.GreaterThan(welcomeModal.GetSiblingIndex()));
        }

        [Test]
        public void EnsurePresentation_ShowsOnboardingGlobalNavigationStrip()
        {
            InvokeEnsurePresentation();

            var topBarRect = _canvasRoot.transform.Find("TopBarRect");
            Assert.That(topBarRect, Is.Not.Null);
            Assert.That(topBarRect.gameObject.activeSelf, Is.True);
            Assert.That(_sceneNavigationBar.enabled, Is.True);
        }

        private void InvokeEnsurePresentation()
        {
            var method = typeof(OnboardingManager).GetMethod("EnsurePresentation", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(_manager, null);
        }
    }
}
