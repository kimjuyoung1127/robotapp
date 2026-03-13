using KineTutor3D.UI;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class SandboxActionPanelViewBuilderTests
    {
        private GameObject _host;
        private SandboxActionPanel _panel;
        private Font _font;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("SandboxHost", typeof(RectTransform));
            _panel = _host.AddComponent<SandboxActionPanel>();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void Build_CreatesPrimaryAndNavigationGroups()
        {
            var root = InvokeBuild();

            Assert.IsNotNull(root.Find("SandboxActionContent/PrimaryActions"));
            Assert.IsNotNull(root.Find("SandboxActionContent/NavigationActions"));
        }

        [Test]
        public void Build_CreatesExpectedButtons()
        {
            var root = InvokeBuild();

            Assert.IsNotNull(root.Find("SandboxActionContent/PrimaryActions/BtnZeroPose"));
            Assert.IsNotNull(root.Find("SandboxActionContent/PrimaryActions/BtnResetPose"));
            Assert.IsNotNull(root.Find("SandboxActionContent/NavigationActions/BtnBackHome"));
            Assert.IsNotNull(root.Find("SandboxActionContent/NavigationActions/BtnOpenRobotLibrary"));
        }

        private RectTransform InvokeBuild()
        {
            var type = typeof(SandboxActionPanel).Assembly.GetType("KineTutor3D.UI.SandboxActionPanelViewBuilder");
            var method = type.GetMethod("Build", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return (RectTransform)method.Invoke(null, new object[] { _panel, null, _font, null, null, null, null, null, null });
        }
    }
}
