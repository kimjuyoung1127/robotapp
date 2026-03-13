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
        public void ConfigureForScene_Main_AppliesZoomedOutProfile()
        {
            var go = new GameObject("Main Camera", typeof(Camera));
            var camera = go.GetComponent<Camera>();

            SceneCameraDirector.ConfigureForScene(SceneId.Main, camera);

            Assert.That(camera.transform.position, Is.EqualTo(new Vector3(0f, 1.8f, -7.4f)));
            Assert.That(camera.fieldOfView, Is.EqualTo(82f));
            Assert.That(camera.nearClipPlane, Is.EqualTo(0.3f));
            Assert.That(camera.farClipPlane, Is.EqualTo(1000f));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ConfigureForScene_Sandbox_AppliesSameGameplayProfile()
        {
            var go = new GameObject("Main Camera", typeof(Camera));
            var camera = go.GetComponent<Camera>();

            SceneCameraDirector.ConfigureForScene(SceneId.Sandbox, camera);

            Assert.That(camera.transform.position, Is.EqualTo(new Vector3(0f, 1.8f, -7.4f)));
            Assert.That(camera.fieldOfView, Is.EqualTo(82f));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ConfigureForScene_MathReadiness_AppliesSameGameplayProfile()
        {
            var go = new GameObject("Main Camera", typeof(Camera));
            var camera = go.GetComponent<Camera>();

            SceneCameraDirector.ConfigureForScene(SceneId.MathReadiness, camera);

            Assert.That(camera.transform.position, Is.EqualTo(new Vector3(0f, 1.8f, -7.4f)));
            Assert.That(camera.fieldOfView, Is.EqualTo(82f));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ConfigureForScene_RobotControl_AppliesDedicatedProfile()
        {
            var go = new GameObject("Main Camera", typeof(Camera));
            var camera = go.GetComponent<Camera>();

            SceneCameraDirector.ConfigureForScene(SceneId.RobotControl, camera);

            Assert.That(camera.transform.position, Is.EqualTo(new Vector3(2.95f, 2.15f, -6.9f)));
            Assert.That(camera.fieldOfView, Is.EqualTo(64f));
            Assert.That(camera.nearClipPlane, Is.EqualTo(0.05f));
            Assert.That(camera.farClipPlane, Is.EqualTo(30f));

            Object.DestroyImmediate(go);
        }
    }
}
