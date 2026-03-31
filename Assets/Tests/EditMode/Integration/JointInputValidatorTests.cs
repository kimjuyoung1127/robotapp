// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// JointInputValidator 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.UI;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// Joint numeric input validator의 파싱/범위 검증을 확인합니다.
    /// </summary>
    public class JointInputValidatorTests
    {
        [Test]
        public void TryParseDegrees_ValidValueWithinRange_ReturnsTrue()
        {
            var ok = JointInputValidator.TryParseDegrees("45.5", -180f, 180f, out var value, out var error);

            Assert.That(ok, Is.True);
            Assert.That(value, Is.EqualTo(45.5f).Within(0.001f));
            Assert.That(error, Is.Empty);
        }

        [Test]
        public void TryParseDegrees_NaNOrInfinity_ReturnsFalse()
        {
            Assert.That(JointInputValidator.TryParseDegrees("NaN", -180f, 180f, out _, out _), Is.False);
            Assert.That(JointInputValidator.TryParseDegrees("Infinity", -180f, 180f, out _, out _), Is.False);
        }

        [Test]
        public void TryParseDegrees_OutOfRange_ReturnsFalse()
        {
            var ok = JointInputValidator.TryParseDegrees("270", -180f, 180f, out _, out var error);

            Assert.That(ok, Is.False);
            Assert.That(error, Does.Contain("between"));
        }
    }
}
