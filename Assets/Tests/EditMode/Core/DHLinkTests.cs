// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// DHLink 동작을 검증하는 EditMode 테스트입니다.
using System;
using KineTutor3D.Types;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// DHLink 입력 검증과 값 보존 동작을 검증합니다.
    /// </summary>
    public class DHLinkTests
    {
        [Test]
        public void Constructor_ValidValues_AssignsFields()
        {
            var link = new DHLink(0.5, 0.2, 1.0, -0.1, JointType.Revolute);

            Assert.AreEqual(0.5, link.Theta, TestTolerances.Math);
            Assert.AreEqual(0.2, link.D, TestTolerances.Math);
            Assert.AreEqual(1.0, link.A, TestTolerances.Math);
            Assert.AreEqual(-0.1, link.Alpha, TestTolerances.Math);
            Assert.AreEqual(JointType.Revolute, link.JointType);
        }

        [Test]
        public void Constructor_DefaultJointType_IsRevolute()
        {
            var link = new DHLink(0.1, 0.2, 0.3, 0.4);
            Assert.AreEqual(JointType.Revolute, link.JointType);
        }

        [Test]
        public void Constructor_NaN_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new DHLink(double.NaN, 0.0, 0.0, 0.0, JointType.Revolute));
        }

        [Test]
        public void Constructor_Infinity_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new DHLink(0.0, 0.0, double.PositiveInfinity, 0.0, JointType.Prismatic));
        }
    }
}
