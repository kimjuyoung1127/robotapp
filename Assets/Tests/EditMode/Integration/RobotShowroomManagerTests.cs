// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// RobotShowroomManager 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.UI;
using KineTutor3D.Visualization;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class RobotShowroomManagerTests
    {
        private GameObject _host;
        private RobotShowroomManager _manager;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("ShowroomManagerHost");
            _manager = _host.AddComponent<RobotShowroomManager>();
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
        public void Configure_UsesCenteredDefaultHero_AndCreatesVisiblePods()
        {
            var context = new RobotShowroomContext(
                robotIds: new[] { "2DOF_RR", "SCARA_RV", "FAIRINO_FR5" },
                maxVisiblePods: 3,
                showLabels: true,
                showCtaButtons: false,
                allowOrbit: false);

            _manager.Configure(context);

            Assert.That(_manager.GetCurrentHeroId(), Is.EqualTo("SCARA_RV"));
            Assert.That(_host.transform.Find("ShowroomPodContainer/Pod_2DOF_RR"), Is.Not.Null);
            Assert.That(_host.transform.Find("ShowroomPodContainer/Pod_SCARA_RV"), Is.Not.Null);
            Assert.That(_host.transform.Find("ShowroomPodContainer/Pod_FAIRINO_FR5"), Is.Not.Null);
            Assert.That(_host.transform.Find("ShowroomPodContainer/Pod_SCARA_RV").localPosition.x, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void SelectRobot_RaisesSelectionEvent_ForAnotherRobot()
        {
            var context = new RobotShowroomContext(
                robotIds: new[] { "2DOF_RR", "SCARA_RV", "FAIRINO_FR5", "FANUC_CRX10" },
                maxVisiblePods: 3,
                heroRobotId: "2DOF_RR");
            string selectedRobotId = null;
            _manager.OnRobotSelected += id => selectedRobotId = id;

            _manager.Configure(context);
            _manager.SelectRobot("FAIRINO_FR5");

            Assert.That(selectedRobotId, Is.EqualTo("FAIRINO_FR5"));
            Assert.That(_manager.GetCurrentHeroId(), Is.EqualTo("FAIRINO_FR5"));
            Assert.That(_host.transform.Find("ShowroomPodContainer/Pod_FAIRINO_FR5"), Is.Not.Null);
            Assert.That(_host.transform.Find("ShowroomPodContainer/Pod_SCARA_RV"), Is.Not.Null);
            Assert.That(_host.transform.Find("ShowroomPodContainer/Pod_2DOF_RR"), Is.Not.Null);
            Assert.That(_host.transform.Find("ShowroomPodContainer/Pod_FANUC_CRX10"), Is.Null);
        }

        [Test]
        public void PreviousPage_ReturnsToCenteredHeroOnFirstPage()
        {
            var context = new RobotShowroomContext(
                robotIds: new[] { "2DOF_RR", "SCARA_RV", "FAIRINO_FR5", "FANUC_CRX10", "IGUS_REBEL" },
                maxVisiblePods: 3,
                showLabels: false,
                showCtaButtons: false,
                allowOrbit: false);

            _manager.Configure(context);
            _manager.NextPage();

            Assert.That(_manager.GetCurrentHeroId(), Is.EqualTo("FANUC_CRX10"));

            _manager.PreviousPage();

            Assert.That(_manager.GetCurrentHeroId(), Is.EqualTo("SCARA_RV"));
            Assert.That(_host.transform.Find("ShowroomPodContainer/Pod_2DOF_RR"), Is.Not.Null);
            Assert.That(_host.transform.Find("ShowroomPodContainer/Pod_SCARA_RV"), Is.Not.Null);
            Assert.That(_host.transform.Find("ShowroomPodContainer/Pod_FAIRINO_FR5"), Is.Not.Null);
        }
    }
}
