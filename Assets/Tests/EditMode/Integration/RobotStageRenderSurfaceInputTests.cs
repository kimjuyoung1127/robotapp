// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
using System.Reflection;
using KineTutor3D.App.Fairino;
using KineTutor3D.UI.RobotControlV3;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// RobotControlV3 메인 RobotStage 입력 연결을 검증합니다.
    /// </summary>
    public class RobotStageRenderSurfaceInputTests
    {
        [Test]
        public void EnsureRenderSurfaceElement_EnablesPointerPicking()
        {
            var host = new VisualElement();
            var surface = new Image
            {
                name = "RobotStageRenderSurface",
                pickingMode = PickingMode.Ignore
            };
            host.Add(surface);

            var go = new GameObject("RobotStageRenderSurfaceTest");
            go.SetActive(false);
            var component = go.AddComponent<RobotStageRenderSurface>();

            try
            {
                SetPrivateField(component, "robotStageHost", host);
                InvokePrivate(component, "EnsureRenderSurfaceElement");

                Assert.That(surface.pickingMode, Is.EqualTo(PickingMode.Position));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DragThreshold_DistinguishesClickFromOrbitDrag()
        {
            var method = typeof(RobotStageRenderSurface).GetMethod(
                "ExceedsDragThreshold",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.That(method, Is.Not.Null);
            Assert.That((bool)method.Invoke(null, new object[] { Vector2.zero, new Vector2(3f, 4f) }), Is.False);
            Assert.That((bool)method.Invoke(null, new object[] { Vector2.zero, new Vector2(6f, 0f) }), Is.True);
        }

        [Test]
        public void RobotStageRenderSurface_DefinesDragAndWheelHandlers()
        {
            Assert.That(
                typeof(RobotStageRenderSurface).GetMethod("OnRenderSurfacePointerMove", BindingFlags.NonPublic | BindingFlags.Instance),
                Is.Not.Null);
            Assert.That(
                typeof(RobotStageRenderSurface).GetMethod("OnRenderSurfacePointerUp", BindingFlags.NonPublic | BindingFlags.Instance),
                Is.Not.Null);
            Assert.That(
                typeof(RobotStageRenderSurface).GetMethod("OnRenderSurfaceWheel", BindingFlags.NonPublic | BindingFlags.Instance),
                Is.Not.Null);
        }

        [Test]
        public void StageCameraFacade_OrbitPanAndZoomMoveCameraWithoutResettingState()
        {
            var runtimeGo = new GameObject("RobotControlV3RuntimeControllerTest");
            runtimeGo.SetActive(false);
            var runtime = runtimeGo.AddComponent<RobotControlV3RuntimeController>();
            var pivotGo = new GameObject("StagePivot");
            var cameraGo = new GameObject("V3StageCamera", typeof(Camera));
            cameraGo.transform.SetParent(pivotGo.transform, false);
            var camera = cameraGo.GetComponent<Camera>();

            try
            {
                SetPrivateField(runtime, "stageCamera", camera);
                SetPrivateField(runtime, "stageCameraPivot", pivotGo.transform);
                SetPrivateField(runtime, "stageCameraFocusPoint", Vector3.zero);
                SetPrivateField(runtime, "stageCameraPanOffset", Vector3.zero);
                SetPrivateField(runtime, "stageCameraDistance", 2.4f);
                SetPrivateField(runtime, "stageCameraMinDistance", 0.5f);
                SetPrivateField(runtime, "stageCameraMaxDistance", 6f);
                SetPrivateField(runtime, "stageCameraYaw", 0f);
                SetPrivateField(runtime, "stageCameraPitch", 0f);
                SetPrivateField(runtime, "stageCameraStateValid", true);

                runtime.RefreshStageCameraView();
                var initialPosition = camera.transform.position;

                runtime.OrbitStageCamera(new Vector2(12f, -4f));
                var orbitPosition = camera.transform.position;
                runtime.PanStageCamera(new Vector2(20f, 10f));
                var panPosition = camera.transform.position;
                runtime.ZoomStageCamera(4f);
                var zoomDistance = GetPrivateField<float>(runtime, "stageCameraDistance");

                Assert.That(Vector3.Distance(initialPosition, orbitPosition), Is.GreaterThan(0.001f));
                Assert.That(Vector3.Distance(orbitPosition, panPosition), Is.GreaterThan(0.001f));
                Assert.That(zoomDistance, Is.GreaterThan(2.4f));
                Assert.That(GetPrivateField<bool>(runtime, "stageCameraUserAdjusted"), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(cameraGo);
                Object.DestroyImmediate(pivotGo);
                Object.DestroyImmediate(runtimeGo);
            }
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, null);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, fieldName);
            return (T)field.GetValue(target);
        }
    }
}
