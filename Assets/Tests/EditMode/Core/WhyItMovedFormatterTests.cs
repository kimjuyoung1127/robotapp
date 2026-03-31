// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// WhyItMovedFormatter 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using KineTutor3D.Math;
using KineTutor3D.UI;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// WhyItMovedFormatter의 텍스트 포매팅을 검증합니다.
    /// </summary>
    public class WhyItMovedFormatterTests
    {
        [Test]
        public void FormatDeltaText_PositiveValue_IncludesPlusSign()
        {
            var result = WhyItMovedFormatter.FormatDeltaText(15.0);

            Assert.That(result, Is.EqualTo("+15.000 deg"));
        }

        [Test]
        public void FormatDeltaText_NegativeValue_IncludesMinusSign()
        {
            var result = WhyItMovedFormatter.FormatDeltaText(-15.0);

            Assert.That(result, Is.EqualTo("-15.000 deg"));
        }

        [Test]
        public void FormatDeltaText_Zero_IncludesPlusSign()
        {
            var result = WhyItMovedFormatter.FormatDeltaText(0.0);

            Assert.That(result, Is.EqualTo("+0.000 deg"));
        }

        [Test]
        public void FormatEEChange_NonZeroDisplacement_FormatsXYZ()
        {
            var displacement = new Vec3D(0.12, 0.08, 0.0);

            var result = WhyItMovedFormatter.FormatEEChange(displacement);

            Assert.That(result, Does.Contain("x: +0.120"));
            Assert.That(result, Does.Contain("y: +0.080"));
            Assert.That(result, Does.Contain("z: +0.000"));
        }

        [Test]
        public void FormatEEChange_NegativeDisplacement_ShowsMinusSign()
        {
            var displacement = new Vec3D(-0.5, -0.3, 0.1);

            var result = WhyItMovedFormatter.FormatEEChange(displacement);

            Assert.That(result, Does.Contain("x: -0.500"));
            Assert.That(result, Does.Contain("y: -0.300"));
            Assert.That(result, Does.Contain("z: +0.100"));
        }

        [Test]
        public void FormatAngleTransition_ConvertsToDegrees()
        {
            var prevRad = 30.0 * System.Math.PI / 180.0;
            var currRad = 45.0 * System.Math.PI / 180.0;

            var result = WhyItMovedFormatter.FormatAngleTransition(prevRad, currRad);

            Assert.That(result, Does.Contain("30.000"));
            Assert.That(result, Does.Contain("45.000"));
            Assert.That(result, Does.Contain("->"));
            Assert.That(result, Does.Contain("deg"));
        }

        [Test]
        public void FormatAffectedLinks_MultipleLinks_JoinsWithComma()
        {
            var links = new[] { "Link0", "Link1", "EE" };

            var result = WhyItMovedFormatter.FormatAffectedLinks(links);

            Assert.That(result, Is.EqualTo("Link0, Link1, EE"));
        }

        [Test]
        public void FormatAffectedLinks_EmptyArray_ReturnsDash()
        {
            var result = WhyItMovedFormatter.FormatAffectedLinks(System.Array.Empty<string>());

            Assert.That(result, Is.EqualTo("-"));
        }

        [Test]
        public void FormatAffectedLinks_Null_ReturnsDash()
        {
            var result = WhyItMovedFormatter.FormatAffectedLinks(null);

            Assert.That(result, Is.EqualTo("-"));
        }

        [Test]
        public void FormatPlainLanguage_MeaningfulJointChange_ReturnsKoreanDescription()
        {
            var state = new WhyItMovedState();
            var prevJoints = new[] { DegreesToRadians(30.0), 0.0 };
            var currJoints = new[] { DegreesToRadians(45.0), 0.0 };
            var prevEE = new Vec3D(1.0, 0.0, 0.0);
            var currEE = new Vec3D(1.1, 0.1, 0.0);

            state.Compute(RuntimeUpdateCause.JointAngleChange, 0, prevJoints, currJoints, prevEE, currEE, 2);
            var result = WhyItMovedFormatter.FormatPlainLanguage(state);

            Assert.That(result, Does.Contain("관절1"));
            Assert.That(result, Does.Contain("15.000"));
            Assert.That(result, Does.Contain("움직였어요"));
        }

        [Test]
        public void FormatPlainLanguage_NotMeaningful_ReturnsEmpty()
        {
            var state = new WhyItMovedState();
            var joints = new[] { 0.0, 0.0 };

            state.Compute(RuntimeUpdateCause.TemplateApply, -1, joints, joints, Vec3D.Zero, Vec3D.Zero, 2);
            var result = WhyItMovedFormatter.FormatPlainLanguage(state);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FormatPlainLanguage_NullState_ReturnsEmpty()
        {
            var result = WhyItMovedFormatter.FormatPlainLanguage(null);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void IsDeltaPositive_PositiveValue_ReturnsTrue()
        {
            Assert.That(WhyItMovedFormatter.IsDeltaPositive(15.0), Is.True);
        }

        [Test]
        public void IsDeltaPositive_NegativeValue_ReturnsFalse()
        {
            Assert.That(WhyItMovedFormatter.IsDeltaPositive(-15.0), Is.False);
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (System.Math.PI / 180.0);
        }
    }
}
