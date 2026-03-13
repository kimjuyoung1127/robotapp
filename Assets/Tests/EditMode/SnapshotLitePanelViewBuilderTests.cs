using KineTutor3D.UI;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.Tests.EditMode
{
    public class SnapshotLitePanelViewBuilderTests
    {
        private GameObject _host;
        private SnapshotLitePanel _panel;
        private Font _font;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("SnapshotLiteHost", typeof(RectTransform));
            _panel = _host.AddComponent<SnapshotLitePanel>();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void Build_CreatesThreeActionButtons()
        {
            var refs = InvokeBuild();
            var panelRoot = GetProperty<RectTransform>(refs, "PanelRoot");

            Assert.IsNotNull(panelRoot);
            Assert.IsNotNull(panelRoot.Find("SnapshotContent/ActionRow/BtnSaveSnapshot"));
            Assert.IsNotNull(panelRoot.Find("SnapshotContent/ActionRow/BtnLoadSnapshot"));
            Assert.IsNotNull(panelRoot.Find("SnapshotContent/ActionRow/BtnCompareSnapshot"));
        }

        [Test]
        public void Build_StatusText_UsesCaptionSize()
        {
            var refs = InvokeBuild();
            var statusText = GetProperty<Text>(refs, "StatusText");

            Assert.IsNotNull(statusText);
            Assert.AreEqual(UIDesignTokens.Type.Caption, statusText.fontSize);
        }

        private object InvokeBuild()
        {
            var type = typeof(SnapshotLitePanel).Assembly.GetType("KineTutor3D.UI.SnapshotLitePanelViewBuilder");
            var method = type.GetMethod("Build", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return method.Invoke(null, new object[] { _panel, null, _font, null, null, null });
        }

        private static T GetProperty<T>(object value, string propertyName) where T : class
        {
            return value.GetType().GetProperty(propertyName)?.GetValue(value) as T;
        }
    }
}
