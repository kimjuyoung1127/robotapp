using KineTutor3D.App;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// RobotSelectionBridge의 Set/Get/Clear 라운드트립을 검증합니다.
    /// </summary>
    public class RobotSelectionBridgeTests
    {
        [SetUp]
        public void SetUp()
        {
            RobotSelectionBridge.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            RobotSelectionBridge.Clear();
        }

        [Test]
        public void SetGet_RobotId_Roundtrip()
        {
            RobotSelectionBridge.SetSelectedRobot("2DOF_RR");
            Assert.AreEqual("2DOF_RR", RobotSelectionBridge.GetSelectedRobotId());
        }

        [Test]
        public void SetGet_Mode_Roundtrip()
        {
            RobotSelectionBridge.SetSelectedMode("guided");
            Assert.AreEqual("guided", RobotSelectionBridge.GetSelectedMode());
        }

        [Test]
        public void GetSelectedRobotId_Default_Empty()
        {
            Assert.AreEqual(string.Empty, RobotSelectionBridge.GetSelectedRobotId());
        }

        [Test]
        public void Clear_RemovesBoth()
        {
            RobotSelectionBridge.SetSelectedRobot("SCARA_RV");
            RobotSelectionBridge.SetSelectedMode("sandbox");
            RobotSelectionBridge.Clear();

            Assert.AreEqual(string.Empty, RobotSelectionBridge.GetSelectedRobotId());
            Assert.AreEqual(string.Empty, RobotSelectionBridge.GetSelectedMode());
        }
    }
}
