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
    /// RobotStage orientation widget 생성과 표시 규칙을 검증합니다.
    /// </summary>
    public class RobotStageOrientationGizmoControllerTests
    {
        [Test]
        public void EnsureWidget_CreatesXYZBadges()
        {
            var host = new VisualElement { name = "RobotStageOrientationHost" };
            var root = new VisualElement();
            root.Add(host);
            var go = new GameObject("RobotStageOrientationGizmoControllerTest");
            go.SetActive(false);
            var controller = go.AddComponent<RobotStageOrientationGizmoController>();

            try
            {
                SetPrivateField(controller, "root", root);
                SetPrivateField(controller, "orientationHost", host);
                InvokePrivate(controller, "EnsureWidget");

                Assert.That(host.Q<Label>("RobotStageOrientationAxisX"), Is.Not.Null);
                Assert.That(host.Q<Label>("RobotStageOrientationAxisY"), Is.Not.Null);
                Assert.That(host.Q<Label>("RobotStageOrientationAxisZ"), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RefreshWidget_HidesWhenStageCameraMissing()
        {
            var host = new VisualElement { name = "RobotStageOrientationHost" };
            var root = new VisualElement();
            root.Add(host);
            var runtimeGo = new GameObject("RobotControlV3RuntimeControllerTest");
            runtimeGo.SetActive(false);
            var runtime = runtimeGo.AddComponent<RobotControlV3RuntimeController>();
            var go = new GameObject("RobotStageOrientationGizmoControllerTest");
            go.SetActive(false);
            var controller = go.AddComponent<RobotStageOrientationGizmoController>();

            try
            {
                SetPrivateField(controller, "runtimeController", runtime);
                SetPrivateField(controller, "root", root);
                SetPrivateField(controller, "orientationHost", host);
                InvokePrivate(controller, "EnsureWidget");
                SetPrivateField(controller, "initialized", true);

                InvokePrivate(controller, "RefreshWidget");

                var widget = host.Q<VisualElement>("RobotStageOrientationWidget");
                Assert.That(widget, Is.Not.Null);
                Assert.That(widget.style.display.value, Is.EqualTo(DisplayStyle.None));
            }
            finally
            {
                Object.DestroyImmediate(go);
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
    }
}
