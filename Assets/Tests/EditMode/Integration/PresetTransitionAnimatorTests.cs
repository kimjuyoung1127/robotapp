// Folder: Tests - EditMode unit tests; no scene required.
using NUnit.Framework;
using KineTutor3D.App.Fairino;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// PresetTransitionAnimator의 순수 수학 함수 (EaseInOutCubic, LerpDouble) 단위 테스트입니다.
    /// </summary>
    [TestFixture]
    public class PresetTransitionAnimatorTests
    {
        private const double Delta = 1e-10;

        [Test]
        public void EaseInOutCubic_AtZero_ReturnsZero()
        {
            Assert.AreEqual(0.0, PresetTransitionAnimator.EaseInOutCubic(0.0), Delta);
        }

        [Test]
        public void EaseInOutCubic_AtOne_ReturnsOne()
        {
            Assert.AreEqual(1.0, PresetTransitionAnimator.EaseInOutCubic(1.0), Delta);
        }

        [Test]
        public void EaseInOutCubic_AtHalf_ReturnsHalf()
        {
            Assert.AreEqual(0.5, PresetTransitionAnimator.EaseInOutCubic(0.5), Delta);
        }

        [Test]
        public void EaseInOutCubic_NearZero_IsSlower()
        {
            var result = PresetTransitionAnimator.EaseInOutCubic(0.1);
            Assert.Less(result, 0.1, "Near zero, eased value should be less than linear");
        }

        [Test]
        public void EaseInOutCubic_NearOne_IsSlower()
        {
            var result = PresetTransitionAnimator.EaseInOutCubic(0.9);
            Assert.Greater(result, 0.9, "Near one, eased value should be greater than linear");
        }

        [Test]
        public void EaseInOutCubic_BelowZero_ClampsToZero()
        {
            Assert.AreEqual(0.0, PresetTransitionAnimator.EaseInOutCubic(-0.5), Delta);
        }

        [Test]
        public void EaseInOutCubic_AboveOne_ClampsToOne()
        {
            Assert.AreEqual(1.0, PresetTransitionAnimator.EaseInOutCubic(1.5), Delta);
        }

        [Test]
        public void EaseInOutCubic_IsMonotonicallyIncreasing()
        {
            var prev = 0.0;
            for (var i = 1; i <= 100; i++)
            {
                var t = i / 100.0;
                var val = PresetTransitionAnimator.EaseInOutCubic(t);
                Assert.GreaterOrEqual(val, prev, $"Must be monotonically increasing at t={t}");
                prev = val;
            }
        }

        [Test]
        public void LerpDouble_AtZero_ReturnsA()
        {
            Assert.AreEqual(10.0, PresetTransitionAnimator.LerpDouble(10.0, 20.0, 0.0), Delta);
        }

        [Test]
        public void LerpDouble_AtOne_ReturnsB()
        {
            Assert.AreEqual(20.0, PresetTransitionAnimator.LerpDouble(10.0, 20.0, 1.0), Delta);
        }

        [Test]
        public void LerpDouble_AtHalf_ReturnsMidpoint()
        {
            Assert.AreEqual(15.0, PresetTransitionAnimator.LerpDouble(10.0, 20.0, 0.5), Delta);
        }

        [Test]
        public void LerpDouble_NegativeValues_InterpolatesCorrectly()
        {
            Assert.AreEqual(-5.0, PresetTransitionAnimator.LerpDouble(-10.0, 0.0, 0.5), Delta);
        }

        [Test]
        public void LerpDouble_LargeRange_InterpolatesCorrectly()
        {
            Assert.AreEqual(90.0, PresetTransitionAnimator.LerpDouble(-180.0, 180.0, 0.75), Delta);
        }
    }
}
