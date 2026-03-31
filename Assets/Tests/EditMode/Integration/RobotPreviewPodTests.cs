// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// RobotPreviewPod 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.Types;
using KineTutor3D.Visualization;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class RobotPreviewPodTests
    {
        private GameObject _host;
        private RobotPreviewPod _pod;
        private GameObject _meshRoot;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("PreviewPodHost");
            _pod = _host.AddComponent<RobotPreviewPod>();
            _meshRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _meshRoot.name = "MeshRoot";
        }

        [TearDown]
        public void TearDown()
        {
            if (_meshRoot != null)
            {
                Object.DestroyImmediate(_meshRoot);
            }

            if (_host != null)
            {
                Object.DestroyImmediate(_host);
            }
        }

        [Test]
        public void Initialize_WithLabel_CreatesPedestalAndNameLabel()
        {
            var metadata = new RobotMetadataInfo("TEST_BOT", "Test Bot", 2, "RR", "Easy");

            _pod.Initialize(metadata, _meshRoot, showLabel: true);

            Assert.That(_host.transform.Find("Pedestal"), Is.Not.Null);
            Assert.That(_host.transform.Find("NameLabel"), Is.Not.Null);
            Assert.That(_pod.RobotId, Is.EqualTo("TEST_BOT"));
        }

        [Test]
        public void SetSelected_ChangesScale()
        {
            var metadata = new RobotMetadataInfo("TEST_BOT", "Test Bot", 2, "RR", "Easy");
            _pod.Initialize(metadata, _meshRoot, showLabel: false);

            _pod.SetSelected(true);

            Assert.That(_pod.transform.localScale.x, Is.EqualTo(1.24f).Within(0.001f));
            Assert.That(_pod.IsSelected, Is.True);
        }
    }
}
