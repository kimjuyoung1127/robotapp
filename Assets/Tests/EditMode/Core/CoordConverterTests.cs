// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// CoordConverter 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.Math;
using KineTutor3D.Visualization;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// Verifies robotics-to-Unity coordinate conversion rules.
    /// </summary>
    public class CoordConverterTests
    {
        [Test]
        public void ToUnityPosition_SwizzlesAxes_AsDocumented()
        {
            var converted = CoordConverter.ToUnityPosition(new Vec3D(1.0, 2.0, 3.0));

            Assert.That(converted.x, Is.EqualTo(1f));
            Assert.That(converted.y, Is.EqualTo(3f));
            Assert.That(converted.z, Is.EqualTo(2f));
        }

        [Test]
        public void ToUnityRotation_Identity_RemainsIdentity()
        {
            var rotation = CoordConverter.ToUnityRotation(Mat3D.Identity);

            Assert.That(Quaternion.Angle(rotation, Quaternion.identity), Is.LessThan(1e-4f));
        }

        [Test]
        public void ToUnityRotation_RoboticsZRotation_MapsToExpectedUnityBasis()
        {
            var rotation = CoordConverter.ToUnityRotation(new Mat3D(
                0.0, -1.0, 0.0,
                1.0, 0.0, 0.0,
                0.0, 0.0, 1.0));

            var unityRight = rotation * Vector3.right;
            var unityUp = rotation * Vector3.up;
            var unityForward = rotation * Vector3.forward;

            Assert.That(Vector3.Distance(unityRight, new Vector3(0f, 0f, 1f)), Is.LessThan(1e-4f));
            Assert.That(Vector3.Distance(unityUp, Vector3.up), Is.LessThan(1e-4f));
            Assert.That(Vector3.Distance(unityForward, new Vector3(-1f, 0f, 0f)), Is.LessThan(1e-4f));
        }
    }
}
