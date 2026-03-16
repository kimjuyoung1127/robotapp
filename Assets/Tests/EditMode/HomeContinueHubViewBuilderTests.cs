using KineTutor3D.UI;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.Tests.EditMode
{
    public class HomeContinueHubViewBuilderTests
    {
        private GameObject _canvasRoot;
        private Canvas _canvas;
        private Font _font;

        [SetUp]
        public void SetUp()
        {
            _canvasRoot = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas = _canvasRoot.GetComponent<Canvas>();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_canvasRoot);
        }

        [Test]
        public void Build_CreatesMathReadinessButton()
        {
            InvokeBuild();

            Assert.IsNotNull(_canvasRoot.transform.Find("HomeContinueHubRoot/HubContent/BtnStartMathReadiness"));
        }

        [Test]
        public void Build_ContinueButton_HasLeadingIcon()
        {
            var refs = InvokeBuild();
            var continueButton = GetProperty<Button>(refs, "ContinueButton");

            Assert.IsNotNull(continueButton);
            Assert.IsNotNull(continueButton.transform.Find("LeadingIcon"));
        }

        [Test]
        public void TryBindExisting_ReusesSceneAuthoredHomeShell()
        {
            var builtRefs = InvokeBuild();
            var type = typeof(HomeContinueHubController).Assembly.GetType("KineTutor3D.UI.HomeContinueHubViewBuilder");
            var method = type.GetMethod("TryBindExisting", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var args = new object[] { _canvas, null };

            var result = (bool)method.Invoke(null, args);
            var reboundRefs = args[1];

            Assert.That(result, Is.True);
            Assert.That(GetProperty<RectTransform>(reboundRefs, "Root"), Is.SameAs(GetProperty<RectTransform>(builtRefs, "Root")));
            Assert.That(GetProperty<Text>(reboundRefs, "ContextText"), Is.SameAs(GetProperty<Text>(builtRefs, "ContextText")));
            Assert.That(GetProperty<Button>(reboundRefs, "ContinueButton"), Is.SameAs(GetProperty<Button>(builtRefs, "ContinueButton")));
            Assert.That(GetProperty<Button>(reboundRefs, "OpenRobotLibraryButton"), Is.SameAs(GetProperty<Button>(builtRefs, "OpenRobotLibraryButton")));
            Assert.That(GetProperty<Button>(reboundRefs, "OpenSandboxButton"), Is.SameAs(GetProperty<Button>(builtRefs, "OpenSandboxButton")));
        }

        private object InvokeBuild()
        {
            var type = typeof(HomeContinueHubController).Assembly.GetType("KineTutor3D.UI.HomeContinueHubViewBuilder");
            var method = type.GetMethod("Build", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return method.Invoke(null, new object[] { _canvas, _font, null, null, null, null, null });
        }

        private static T GetProperty<T>(object value, string propertyName) where T : class
        {
            return value.GetType().GetProperty(propertyName)?.GetValue(value) as T;
        }
    }
}
