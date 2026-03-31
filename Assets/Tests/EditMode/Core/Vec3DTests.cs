// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// Vec3D 동작을 검증하는 EditMode 테스트입니다.
using System;
using KineTutor3D.Math;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// Vec3D 기본 연산과 가드를 검증합니다.
    /// </summary>
    public class Vec3DTests
    {
        [Test]
        public void Constructor_ValidValues_AssignsComponents()
        {
            var v = new Vec3D(1.0, 2.0, 3.0);
            Assert.AreEqual(1.0, v.X, TestTolerances.Math);
            Assert.AreEqual(2.0, v.Y, TestTolerances.Math);
            Assert.AreEqual(3.0, v.Z, TestTolerances.Math);
        }

        [Test]
        public void Constructor_NaN_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Vec3D(double.NaN, 0.0, 0.0));
        }

        [Test]
        public void Constructor_Infinity_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Vec3D(0.0, double.PositiveInfinity, 0.0));
        }

        [Test]
        public void Add_Subtract_Multiply_WorkAsExpected()
        {
            var a = new Vec3D(1.0, 2.0, 3.0);
            var b = new Vec3D(4.0, 5.0, 6.0);

            var sum = a + b;
            var diff = b - a;
            var scaled = 2.0 * a;

            Assert.AreEqual(5.0, sum.X, TestTolerances.Math);
            Assert.AreEqual(7.0, sum.Y, TestTolerances.Math);
            Assert.AreEqual(9.0, sum.Z, TestTolerances.Math);
            Assert.AreEqual(3.0, diff.X, TestTolerances.Math);
            Assert.AreEqual(3.0, diff.Y, TestTolerances.Math);
            Assert.AreEqual(3.0, diff.Z, TestTolerances.Math);
            Assert.AreEqual(2.0, scaled.X, TestTolerances.Math);
            Assert.AreEqual(4.0, scaled.Y, TestTolerances.Math);
            Assert.AreEqual(6.0, scaled.Z, TestTolerances.Math);
        }

        [Test]
        public void Dot_And_Cross_ReturnExpectedValues()
        {
            var a = new Vec3D(1.0, 2.0, 3.0);
            var b = new Vec3D(4.0, 5.0, 6.0);

            Assert.AreEqual(32.0, a.Dot(b), TestTolerances.Math);

            var cross = a.Cross(b);
            Assert.AreEqual(-3.0, cross.X, TestTolerances.Math);
            Assert.AreEqual(6.0, cross.Y, TestTolerances.Math);
            Assert.AreEqual(-3.0, cross.Z, TestTolerances.Math);
        }

        [Test]
        public void Magnitude_And_Normalized_ReturnExpectedValues()
        {
            var v = new Vec3D(3.0, 4.0, 0.0);
            Assert.AreEqual(5.0, v.Magnitude(), TestTolerances.Math);

            var n = v.Normalized();
            Assert.AreEqual(0.6, n.X, TestTolerances.Math);
            Assert.AreEqual(0.8, n.Y, TestTolerances.Math);
            Assert.AreEqual(0.0, n.Z, TestTolerances.Math);
        }

        [Test]
        public void Equals_UsesTolerance()
        {
            var a = new Vec3D(1.0, 2.0, 3.0);
            var b = new Vec3D(1.0 + 1e-11, 2.0, 3.0);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }
    }
}
