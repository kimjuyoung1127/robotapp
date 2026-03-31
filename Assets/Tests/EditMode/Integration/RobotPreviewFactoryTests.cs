// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// RobotPreviewFactory 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.Templates;
using KineTutor3D.Visualization;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class RobotPreviewFactoryTests
    {
        private GameObject _host;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("RobotPreviewFactoryHost");
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null)
            {
                Object.DestroyImmediate(_host);
            }
        }

        [Test]
        public void CreatePod_FairinoFr5_CreatesArticulatedPreview()
        {
            const string robotId = "FAIRINO_FR5";
            Assert.That(RobotCatalog.TryGet(robotId, out var entry), Is.True, $"Robot '{robotId}' should exist in catalog.");

            var pod = RobotPreviewFactory.CreatePod(_host.transform, entry, showLabel: true);

            Assert.That(pod, Is.Not.Null);
            var podRoot = _host.transform.Find($"Pod_{robotId}");
            Assert.That(podRoot, Is.Not.Null);
            Assert.That(podRoot.Find("UnknownPreview"), Is.Null);
            Assert.That(podRoot.GetComponentsInChildren<Transform>(true), Has.Some.Matches<Transform>(t => t.name == "JointPivot0"));
            Assert.That(podRoot.GetComponentsInChildren<Transform>(true), Has.Some.Matches<Transform>(t => t.name == "JointPivot1"));
            Assert.That(podRoot.GetComponentsInChildren<Transform>(true), Has.Some.Matches<Transform>(t => t.name == "JointPivot2"));
        }

        [Test]
        public void CreatePod_TemplateFr5_ReusesFr5PreviewShape()
        {
            const string robotId = "FAIRINO_FR5_TEMPLATE";
            Assert.That(RobotCatalog.TryGet(robotId, out var entry), Is.True, $"Robot '{robotId}' should exist in catalog.");

            var pod = RobotPreviewFactory.CreatePod(_host.transform, entry, showLabel: true);

            Assert.That(pod, Is.Not.Null);
            var podRoot = _host.transform.Find($"Pod_{robotId}");
            Assert.That(podRoot, Is.Not.Null);
            Assert.That(podRoot.GetComponentsInChildren<Transform>(true), Has.Some.Matches<Transform>(t => t.name == "JointPivot0"));
            Assert.That(podRoot.GetComponentsInChildren<Transform>(true), Has.Some.Matches<Transform>(t => t.name == "JointPivot1"));
        }

        [TestCase("FANUC_CRX10")]
        [TestCase("IGUS_REBEL")]
        public void CreatePod_RealDonorRobots_CreateVisiblePreview(string robotId)
        {
            Assert.That(RobotCatalog.TryGet(robotId, out var entry), Is.True, $"Robot '{robotId}' should exist in catalog.");

            var pod = RobotPreviewFactory.CreatePod(_host.transform, entry, showLabel: true);

            Assert.That(pod, Is.Not.Null);
            var podRoot = _host.transform.Find($"Pod_{robotId}");
            Assert.That(podRoot, Is.Not.Null);
            Assert.That(podRoot.GetComponentsInChildren<MeshRenderer>(true).Length, Is.GreaterThan(0));
        }
    }
}
