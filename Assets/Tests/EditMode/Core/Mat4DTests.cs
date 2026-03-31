// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// Mat4D 동작을 검증하는 EditMode 테스트입니다.
using System;
using KineTutor3D.Math;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// Mat4D 연산/추출/역행렬 동작을 검증합니다.
    /// </summary>
    public class Mat4DTests
    {
        [Test]
        public void Identity_HasExpectedDiagonal()
        {
            var identity = Mat4D.Identity;
            Assert.AreEqual(1.0, identity[0, 0], TestTolerances.Math);
            Assert.AreEqual(1.0, identity[1, 1], TestTolerances.Math);
            Assert.AreEqual(1.0, identity[2, 2], TestTolerances.Math);
            Assert.AreEqual(1.0, identity[3, 3], TestTolerances.Math);
        }

        [Test]
        public void FromRowMajor_InvalidLength_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Mat4D.FromRowMajor(new double[15]));
        }

        [Test]
        public void Multiply_ComposesTranslations()
        {
            var tx = Mat4D.TranslateX(1.0);
            var tz = Mat4D.TranslateZ(2.0);
            var combined = tx * tz;

            Assert.AreEqual(1.0, combined[0, 3], TestTolerances.Math);
            Assert.AreEqual(2.0, combined[2, 3], TestTolerances.Math);
        }

        [Test]
        public void TransformPoint_And_Direction_WorkAsExpected()
        {
            var transform = Mat4D.TranslateX(2.0) * Mat4D.RotateZ(System.Math.PI / 2.0);
            var point = transform.TransformPoint(new Vec3D(1.0, 0.0, 0.0));
            var direction = transform.TransformDirection(new Vec3D(1.0, 0.0, 0.0));

            Assert.AreEqual(2.0, point.X, TestTolerances.Position);
            Assert.AreEqual(1.0, point.Y, TestTolerances.Position);
            Assert.AreEqual(0.0, point.Z, TestTolerances.Position);

            Assert.AreEqual(0.0, direction.X, TestTolerances.Rotation);
            Assert.AreEqual(1.0, direction.Y, TestTolerances.Rotation);
            Assert.AreEqual(0.0, direction.Z, TestTolerances.Rotation);
        }

        [Test]
        public void ExtractPosition_And_Rotation_WorkAsExpected()
        {
            var transform = Mat4D.TranslateX(3.0) * Mat4D.RotateZ(System.Math.PI / 2.0);
            var position = transform.ExtractPosition();
            var rotation = transform.ExtractRotation();

            Assert.AreEqual(3.0, position.X, TestTolerances.Math);
            Assert.AreEqual(0.0, position.Y, TestTolerances.Math);
            Assert.AreEqual(0.0, position.Z, TestTolerances.Math);
            Assert.AreEqual(0.0, rotation[0, 0], TestTolerances.Rotation);
            Assert.AreEqual(-1.0, rotation[0, 1], TestTolerances.Rotation);
            Assert.AreEqual(1.0, rotation[1, 0], TestTolerances.Rotation);
        }

        [Test]
        public void InverseHomogeneous_MultipliedByOriginal_ReturnsIdentity()
        {
            var transform = Mat4D.TranslateX(1.25) * Mat4D.RotateZ(System.Math.PI / 4.0) * Mat4D.TranslateZ(0.75);
            var inv = transform.InverseHomogeneous();
            var result = transform * inv;

            MatrixAssert.IsIdentity(result, TestTolerances.Math, "역행렬 검증 실패");
        }

        [Test]
        public void RotateX_And_RotateZ_CreateExpectedTransforms()
        {
            var rz = Mat4D.RotateZ(System.Math.PI / 2.0);
            var rx = Mat4D.RotateX(System.Math.PI / 2.0);

            var rzDirection = rz.TransformDirection(Vec3D.UnitX);
            var rxDirection = rx.TransformDirection(Vec3D.UnitY);

            Assert.AreEqual(0.0, rzDirection.X, TestTolerances.Rotation);
            Assert.AreEqual(1.0, rzDirection.Y, TestTolerances.Rotation);
            Assert.AreEqual(0.0, rzDirection.Z, TestTolerances.Rotation);

            Assert.AreEqual(0.0, rxDirection.X, TestTolerances.Rotation);
            Assert.AreEqual(0.0, rxDirection.Y, TestTolerances.Rotation);
            Assert.AreEqual(1.0, rxDirection.Z, TestTolerances.Rotation);
        }
    }
}
