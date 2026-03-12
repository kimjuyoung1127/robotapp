using System;
using KineTutor3D.Kinematics;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// TemplateSCARA_RV의 DH 파라미터와 FK 계산을 검증합니다.
    /// 기준값: docs/ref/test-reference-values.md Section 7.
    /// </summary>
    public class TemplateSCARA_RVTests
    {
        private RobotTemplate _template;

        [SetUp]
        public void SetUp()
        {
            _template = TemplateSCARA_RV.Create();
        }

        [Test]
        public void Create_Returns3DOF()
        {
            Assert.AreEqual(3, _template.Dof);
        }

        [Test]
        public void Create_Name_Matches()
        {
            Assert.AreEqual("SCARA_RV", _template.Name);
        }

        [Test]
        public void Create_Joint1_Revolute()
        {
            Assert.AreEqual(JointType.Revolute, _template.GetLink(0).JointType);
        }

        [Test]
        public void Create_Joint2_Revolute()
        {
            Assert.AreEqual(JointType.Revolute, _template.GetLink(1).JointType);
        }

        [Test]
        public void Create_Joint3_Prismatic()
        {
            Assert.AreEqual(JointType.Prismatic, _template.GetLink(2).JointType);
        }

        [Test]
        public void Create_Alpha2_PI()
        {
            Assert.AreEqual(Math.PI, _template.GetLink(1).Alpha, TestTolerances.Math);
        }

        [Test]
        public void Create_ArmLengths_05()
        {
            Assert.AreEqual(0.5, _template.GetLink(0).A, TestTolerances.Math);
            Assert.AreEqual(0.5, _template.GetLink(1).A, TestTolerances.Math);
        }

        // ── FK Case A: θ₁=0, θ₂=0, d₃=0 → EE = (1.0, 0.0, 0.0)
        [Test]
        public void FK_CaseA_ZeroZeroZero_Position()
        {
            var links = _template.GetLinks();
            var joints = new[] { 0.0, 0.0, 0.0 };

            var pose = ForwardKinematics.ComputePose(links, joints);

            Assert.AreEqual(1.0, pose.Position.X, TestTolerances.Position, "CaseA X");
            Assert.AreEqual(0.0, pose.Position.Y, TestTolerances.Position, "CaseA Y");
            Assert.AreEqual(0.0, pose.Position.Z, TestTolerances.Position, "CaseA Z");
        }

        // ── FK Case B: θ₁=π/2, θ₂=0, d₃=0.5 → EE = (0.0, 1.0, -0.5)
        [Test]
        public void FK_CaseB_PiHalf_Zero_Half_Position()
        {
            var links = _template.GetLinks();
            var joints = new[] { Math.PI / 2.0, 0.0, 0.5 };

            var pose = ForwardKinematics.ComputePose(links, joints);

            Assert.AreEqual(0.0, pose.Position.X, TestTolerances.Position, "CaseB X");
            Assert.AreEqual(1.0, pose.Position.Y, TestTolerances.Position, "CaseB Y");
            Assert.AreEqual(-0.5, pose.Position.Z, TestTolerances.Position, "CaseB Z");
        }

        // ── FK Case C: θ₁=0, θ₂=π/2, d₃=0.25 → EE = (0.5, 0.5, -0.25)
        [Test]
        public void FK_CaseC_Zero_PiHalf_Quarter_Position()
        {
            var links = _template.GetLinks();
            var joints = new[] { 0.0, Math.PI / 2.0, 0.25 };

            var pose = ForwardKinematics.ComputePose(links, joints);

            Assert.AreEqual(0.5, pose.Position.X, TestTolerances.Position, "CaseC X");
            Assert.AreEqual(0.5, pose.Position.Y, TestTolerances.Position, "CaseC Y");
            Assert.AreEqual(-0.25, pose.Position.Z, TestTolerances.Position, "CaseC Z");
        }

        // ── Joint limits
        [Test]
        public void JointLimits_Joint3_Prismatic_Range()
        {
            var limit = _template.GetJointLimit(2);
            Assert.AreEqual(0.0, limit.Min, TestTolerances.Math);
            Assert.AreEqual(1.0, limit.Max, TestTolerances.Math);
        }
    }
}
