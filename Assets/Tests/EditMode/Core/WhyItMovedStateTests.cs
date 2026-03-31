// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// WhyItMovedState 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using KineTutor3D.Math;
using KineTutor3D.UI;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// WhyItMovedState의 Compute 로직을 검증합니다.
    /// </summary>
    public class WhyItMovedStateTests
    {
        private const double Tolerance = 1e-6;

        [Test]
        public void Compute_JointAngleChange_CalculatesDeltaDeg()
        {
            var state = new WhyItMovedState();
            var prevJoints = new[] { DegreesToRadians(30.0), 0.0 };
            var currJoints = new[] { DegreesToRadians(45.0), 0.0 };
            var prevEE = new Vec3D(1.0, 0.0, 0.0);
            var currEE = new Vec3D(1.1, 0.1, 0.0);

            state.Compute(RuntimeUpdateCause.JointAngleChange, 0, prevJoints, currJoints, prevEE, currEE, 2);

            Assert.That(state.DeltaDeg, Is.EqualTo(15.0).Within(Tolerance));
            Assert.That(state.ChangedJointIndex, Is.EqualTo(0));
            Assert.That(state.IsMeaningfulChange, Is.True);
            Assert.That(state.UpdateCause, Is.EqualTo(RuntimeUpdateCause.JointAngleChange));
        }

        [Test]
        public void Compute_NegativeDelta_ReportsNegativeDeg()
        {
            var state = new WhyItMovedState();
            var prevJoints = new[] { DegreesToRadians(45.0), 0.0 };
            var currJoints = new[] { DegreesToRadians(30.0), 0.0 };
            var prevEE = new Vec3D(1.0, 0.0, 0.0);
            var currEE = new Vec3D(0.9, -0.1, 0.0);

            state.Compute(RuntimeUpdateCause.JointAngleChange, 0, prevJoints, currJoints, prevEE, currEE, 2);

            Assert.That(state.DeltaDeg, Is.EqualTo(-15.0).Within(Tolerance));
            Assert.That(state.IsMeaningfulChange, Is.True);
        }

        [Test]
        public void Compute_TinyChange_IsNotMeaningful()
        {
            var state = new WhyItMovedState();
            var angle = DegreesToRadians(45.0);
            var tinyDelta = DegreesToRadians(0.005);
            var prevJoints = new[] { angle, 0.0 };
            var currJoints = new[] { angle + tinyDelta, 0.0 };
            var ee = new Vec3D(1.0, 0.0, 0.0);

            state.Compute(RuntimeUpdateCause.JointAngleChange, 0, prevJoints, currJoints, ee, ee, 2);

            Assert.That(state.IsMeaningfulChange, Is.False);
        }

        [Test]
        public void Compute_TemplateApply_IsNotMeaningful()
        {
            var state = new WhyItMovedState();
            var prevJoints = new[] { 0.0, 0.0 };
            var currJoints = new[] { 0.0, 0.0 };
            var ee = Vec3D.Zero;

            state.Compute(RuntimeUpdateCause.TemplateApply, -1, prevJoints, currJoints, ee, ee, 2);

            Assert.That(state.IsMeaningfulChange, Is.False);
            Assert.That(state.UpdateCause, Is.EqualTo(RuntimeUpdateCause.TemplateApply));
        }

        [Test]
        public void Compute_DhParameterEdit_IsNotMeaningful()
        {
            var state = new WhyItMovedState();
            var prevJoints = new[] { 0.0, 0.0 };
            var currJoints = new[] { 0.0, 0.0 };
            var ee = Vec3D.Zero;

            state.Compute(RuntimeUpdateCause.DhParameterEdit, 0, prevJoints, currJoints, ee, ee, 2);

            Assert.That(state.IsMeaningfulChange, Is.False);
        }

        [Test]
        public void Compute_EEDisplacement_CalculatedCorrectly()
        {
            var state = new WhyItMovedState();
            var prevJoints = new[] { 0.0, 0.0 };
            var currJoints = new[] { DegreesToRadians(30.0), 0.0 };
            var prevEE = new Vec3D(1.0, 0.0, 0.0);
            var currEE = new Vec3D(1.3, 0.4, 0.0);

            state.Compute(RuntimeUpdateCause.JointAngleChange, 0, prevJoints, currJoints, prevEE, currEE, 2);

            Assert.That(state.EEDisplacement.X, Is.EqualTo(0.3).Within(Tolerance));
            Assert.That(state.EEDisplacement.Y, Is.EqualTo(0.4).Within(Tolerance));
            Assert.That(state.EEDistanceMoved, Is.EqualTo(0.5).Within(Tolerance));
        }

        [Test]
        public void Compute_AffectedLinks_FromJoint0_IncludesAllLinks()
        {
            var state = new WhyItMovedState();
            var prevJoints = new[] { 0.0, 0.0 };
            var currJoints = new[] { DegreesToRadians(10.0), 0.0 };
            var ee = Vec3D.Zero;

            state.Compute(RuntimeUpdateCause.JointAngleChange, 0, prevJoints, currJoints, ee, ee, 2);

            Assert.That(state.AffectedLinkNames, Is.EqualTo(new[] { "Link0", "Link1", "EE" }));
        }

        [Test]
        public void Compute_AffectedLinks_FromJoint1_ExcludesLink0()
        {
            var state = new WhyItMovedState();
            var prevJoints = new[] { 0.0, 0.0 };
            var currJoints = new[] { 0.0, DegreesToRadians(10.0) };
            var ee = Vec3D.Zero;

            state.Compute(RuntimeUpdateCause.JointAngleChange, 1, prevJoints, currJoints, ee, ee, 2);

            Assert.That(state.AffectedLinkNames, Is.EqualTo(new[] { "Link1", "EE" }));
        }

        [Test]
        public void Compute_NullPreviousJoints_IsNotMeaningful()
        {
            var state = new WhyItMovedState();
            var currJoints = new[] { DegreesToRadians(45.0), 0.0 };
            var ee = Vec3D.Zero;

            state.Compute(RuntimeUpdateCause.JointAngleChange, 0, null, currJoints, ee, ee, 2);

            Assert.That(state.IsMeaningfulChange, Is.False);
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (System.Math.PI / 180.0);
        }
    }
}
