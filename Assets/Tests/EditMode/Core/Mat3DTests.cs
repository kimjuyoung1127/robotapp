// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// Mat3D 동작을 검증하는 EditMode 테스트입니다.
using System;
using KineTutor3D.Math;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// Mat3D 생성/인덱서/전치/동등성 동작을 검증합니다.
    /// </summary>
    public class Mat3DTests
    {
        [Test]
        public void Identity_HasExpectedDiagonal()
        {
            var identity = Mat3D.Identity;
            Assert.AreEqual(1.0, identity[0, 0], TestTolerances.Math);
            Assert.AreEqual(1.0, identity[1, 1], TestTolerances.Math);
            Assert.AreEqual(1.0, identity[2, 2], TestTolerances.Math);
            Assert.AreEqual(0.0, identity[0, 1], TestTolerances.Math);
        }

        [Test]
        public void FromRowMajor_InvalidLength_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Mat3D.FromRowMajor(new double[8]));
        }

        [Test]
        public void Constructor_NaN_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new Mat3D(double.NaN, 0, 0, 0, 1, 0, 0, 0, 1));
        }

        [Test]
        public void Transpose_ReturnsExpectedMatrix()
        {
            var m = Mat3D.FromRowMajor(new[]
            {
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0
            });

            var t = m.Transpose();

            Assert.AreEqual(1.0, t[0, 0], TestTolerances.Math);
            Assert.AreEqual(4.0, t[0, 1], TestTolerances.Math);
            Assert.AreEqual(7.0, t[0, 2], TestTolerances.Math);
            Assert.AreEqual(2.0, t[1, 0], TestTolerances.Math);
            Assert.AreEqual(6.0, t[2, 1], TestTolerances.Math);
        }

        [Test]
        public void Equals_UsesTolerance()
        {
            var a = Mat3D.Identity;
            var b = new Mat3D(
                1.0 + 1e-11, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }
    }
}
