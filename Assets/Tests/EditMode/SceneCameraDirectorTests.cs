using KineTutor3D.App;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// SceneCameraDirector의 씬별 카메라 프로필 적용을 검증합니다.
    /// </summary>
    public class SceneCameraDirectorTests
    {
        [Test]
        public void ConfigureForScene_Sandbox_AppliesZoomedOutProfile()
        {
            var go = new GameObject("Main Camera", typeof(Camera));
            var camera = go.GetComponent<Camera>();

            SceneCameraDirector.ConfigureForScene(SceneId.Sandbox, camera);

            Assert.That(camera.transform.position, Is.EqualTo(new Vector3(0f, 1.8f, -7.4f)));
            Assert.That(camera.fieldOfView, Is.EqualTo(82f));
            Assert.That(camera.nearClipPlane, Is.EqualTo(0.3f));
            Assert.That(camera.farClipPlane, Is.EqualTo(1000f));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ConfigureForScene_MathReadiness_AppliesCloserReadinessProfile()
        {
            var go = new GameObject("Main Camera", typeof(Camera));
            var camera = go.GetComponent<Camera>();

            SceneCameraDirector.ConfigureForScene(SceneId.MathReadiness, camera);

            Assert.That(camera.transform.position, Is.EqualTo(new Vector3(0f, 1.62f, -6.1f)));
            Assert.That(camera.transform.eulerAngles, Is.EqualTo(new Vector3(6f, 0f, 0f)));
            Assert.That(camera.fieldOfView, Is.EqualTo(74f));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ConfigureForScene_RobotControl_AppliesDedicatedProfile()
        {
            var go = new GameObject("Main Camera", typeof(Camera));
            var camera = go.GetComponent<Camera>();

            SceneCameraDirector.ConfigureForScene(SceneId.RobotControl, camera);

            Assert.That(camera.transform.position, Is.EqualTo(new Vector3(-1.39f, 0.55f, -2.35f)));
            Assert.That(camera.fieldOfView, Is.EqualTo(40f));
            Assert.That(camera.nearClipPlane, Is.EqualTo(0.01f));
            Assert.That(camera.farClipPlane, Is.EqualTo(30f));

            Object.DestroyImmediate(go);
        }
    }
}
