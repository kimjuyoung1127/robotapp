// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// OrbitCameraController 동작을 검증하는 EditMode 테스트입니다.
using System.Reflection;
using KineTutor3D.Visualization;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// OrbitCameraController의 초기 궤도 계산이 첫 프레임에 시점을 반전시키지 않는지 검증합니다.
    /// </summary>
    public class OrbitCameraControllerTests
    {
        [Test]
        public void SetTarget_AndApplyOrbit_PreservesCurrentRobotControlView()
        {
            var targetGo = new GameObject("Target");
            var cameraGo = new GameObject("Main Camera");
            var controller = cameraGo.AddComponent<OrbitCameraController>();

            var expectedPosition = new Vector3(-1.39f, 0.63f, -2.35f);
            cameraGo.transform.position = expectedPosition;
            cameraGo.transform.LookAt(targetGo.transform);

            try
            {
                controller.SetTarget(targetGo.transform);

                var applyOrbit = typeof(OrbitCameraController).GetMethod(
                    "ApplyOrbit",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                Assert.That(applyOrbit, Is.Not.Null);
                applyOrbit.Invoke(controller, null);

                Assert.That(Vector3.Distance(cameraGo.transform.position, expectedPosition), Is.LessThan(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(cameraGo);
                Object.DestroyImmediate(targetGo);
            }
        }
    }
}
