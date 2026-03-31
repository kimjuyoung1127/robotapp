// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// DHStandard 동작을 검증하는 EditMode 테스트입니다.
using System;
using KineTutor3D.Kinematics;
using KineTutor3D.Math;
using KineTutor3D.Types;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// 표준 DH 단일 링크 변환 행렬 계산을 검증합니다.
    /// </summary>
    public class DHStandardTests
    {
        [Test]
        public void ComputeA_IdentityCase_ReturnsIdentity()
        {
            var link = new DHLink(0.0, 0.0, 0.0, 0.0, JointType.Revolute);
            var actual = DHStandard.ComputeA(link, 0.0);
            MatrixAssert.IsIdentity(actual, TestTolerances.Math, "Identity 케이스 실패");
        }

        [Test]
        public void ComputeA_RevolutePiOver2_ReturnsExpectedMatrix()
        {
            var link = new DHLink(0.0, 0.0, 1.0, 0.0, JointType.Revolute);
            var actual = DHStandard.ComputeA(link, System.Math.PI / 2.0);

            var expected = new Mat4D(
                0.0, -1.0, 0.0, 0.0,
                1.0, 0.0, 0.0, 1.0,
                0.0, 0.0, 1.0, 0.0,
                0.0, 0.0, 0.0, 1.0);

            MatrixAssert.AreEqual(expected, actual, TestTolerances.Math, "Revolute pi/2 케이스 실패");
        }

        [Test]
        public void ComputeA_Prismatic_AppliesJointValueToD()
        {
            var link = new DHLink(0.0, 1.0, 0.0, 0.0, JointType.Prismatic);
            var actual = DHStandard.ComputeA(link, 0.5);

            var expected = new Mat4D(
                1.0, 0.0, 0.0, 0.0,
                0.0, 1.0, 0.0, 0.0,
                0.0, 0.0, 1.0, 1.5,
                0.0, 0.0, 0.0, 1.0);

            MatrixAssert.AreEqual(expected, actual, TestTolerances.Math, "Prismatic d 반영 실패");
        }

        [Test]
        public void ComputeA_NaNJointValue_ThrowsArgumentException()
        {
            var link = new DHLink(0.0, 0.0, 1.0, 0.0, JointType.Revolute);
            Assert.Throws<ArgumentException>(() => DHStandard.ComputeA(link, double.NaN));
        }
    }
}
