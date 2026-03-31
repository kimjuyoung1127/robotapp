// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// FK 동작을 검증하는 EditMode 테스트입니다.
using System;
using KineTutor3D.Kinematics;
using KineTutor3D.Types;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// FK 누적 행렬과 포즈 계산을 검증합니다.
    /// </summary>
    public class FKTests
    {
        [Test]
        public void ComputeAll_IdentitySingleLink_ReturnsIdentity()
        {
            var links = new[]
            {
                new DHLink(0.0, 0.0, 0.0, 0.0, JointType.Revolute)
            };
            var joints = new[] { 0.0 };

            var all = ForwardKinematics.ComputeAll(links, joints);

            Assert.AreEqual(1, all.Length);
            MatrixAssert.IsIdentity(all[0], TestTolerances.Math, "Identity 체인 실패");
        }

        [Test]
        public void ComputePose_2DofRR_ZeroZero_PositionMatchesReference()
        {
            var links = CreateTwoDofRrLinks();
            var joints = new[] { 0.0, 0.0 };

            var pose = ForwardKinematics.ComputePose(links, joints);

            Assert.AreEqual(2.0, pose.Position.X, TestTolerances.Position);
            Assert.AreEqual(0.0, pose.Position.Y, TestTolerances.Position);
            Assert.AreEqual(0.0, pose.Position.Z, TestTolerances.Position);
        }

        [Test]
        public void ComputePose_2DofRR_PiOver2Zero_PositionMatchesReference()
        {
            var links = CreateTwoDofRrLinks();
            var joints = new[] { System.Math.PI / 2.0, 0.0 };

            var pose = ForwardKinematics.ComputePose(links, joints);

            Assert.AreEqual(0.0, pose.Position.X, TestTolerances.Position);
            Assert.AreEqual(2.0, pose.Position.Y, TestTolerances.Position);
            Assert.AreEqual(0.0, pose.Position.Z, TestTolerances.Position);
        }

        [Test]
        public void ComputePose_2DofRR_ZeroPiOver2_PositionMatchesReference()
        {
            var links = CreateTwoDofRrLinks();
            var joints = new[] { 0.0, System.Math.PI / 2.0 };

            var pose = ForwardKinematics.ComputePose(links, joints);

            Assert.AreEqual(1.0, pose.Position.X, TestTolerances.Position);
            Assert.AreEqual(1.0, pose.Position.Y, TestTolerances.Position);
            Assert.AreEqual(0.0, pose.Position.Z, TestTolerances.Position);
        }

        [Test]
        public void ComputePose_2DofRR_PiOver4PiOver4_PositionMatchesReference()
        {
            var links = CreateTwoDofRrLinks();
            var joints = new[] { System.Math.PI / 4.0, System.Math.PI / 4.0 };

            var pose = ForwardKinematics.ComputePose(links, joints);
            var root2Over2 = System.Math.Sqrt(2.0) / 2.0;

            Assert.AreEqual(root2Over2, pose.Position.X, TestTolerances.Position);
            Assert.AreEqual(1.0 + root2Over2, pose.Position.Y, TestTolerances.Position);
            Assert.AreEqual(0.0, pose.Position.Z, TestTolerances.Position);
        }

        [Test]
        public void ComputePose_ScaraCase_PositionMatchesReference()
        {
            var links = new[]
            {
                new DHLink(0.0, 0.0, 0.5, 0.0, JointType.Revolute),
                new DHLink(0.0, 0.0, 0.5, System.Math.PI, JointType.Revolute),
                new DHLink(0.0, 0.0, 0.0, 0.0, JointType.Prismatic)
            };
            var joints = new[] { System.Math.PI / 2.0, 0.0, 0.5 };

            var pose = ForwardKinematics.ComputePose(links, joints);

            Assert.AreEqual(0.0, pose.Position.X, TestTolerances.Position);
            Assert.AreEqual(1.0, pose.Position.Y, TestTolerances.Position);
            Assert.AreEqual(-0.5, pose.Position.Z, TestTolerances.Position);
        }

        [Test]
        public void ComputeAll_LengthMismatch_ThrowsArgumentException()
        {
            var links = CreateTwoDofRrLinks();
            var joints = new[] { 0.0 };
            Assert.Throws<ArgumentException>(() => ForwardKinematics.ComputeAll(links, joints));
        }

        [Test]
        public void ComputeAll_NaNJointValue_ThrowsArgumentException()
        {
            var links = CreateTwoDofRrLinks();
            var joints = new[] { 0.0, double.NaN };
            Assert.Throws<ArgumentException>(() => ForwardKinematics.ComputeAll(links, joints));
        }

        private static DHLink[] CreateTwoDofRrLinks()
        {
            return new[]
            {
                new DHLink(0.0, 0.0, 1.0, 0.0, JointType.Revolute),
                new DHLink(0.0, 0.0, 1.0, 0.0, JointType.Revolute)
            };
        }
    }
}
