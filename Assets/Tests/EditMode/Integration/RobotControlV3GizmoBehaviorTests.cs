// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
using System.Reflection;
using KineTutor3D.App.Fairino;
using KineTutor3D.Visualization;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// RobotControlV3 기즈모 표시 규칙을 검증합니다.
    /// </summary>
    public class RobotControlV3GizmoBehaviorTests
    {
        [Test]
        public void ApplyBaseAndToolFrameState_UsesDedicatedGizmos_AndHidesLegacyFactory()
        {
            var runtimeGo = new GameObject("RobotControlV3RuntimeControllerTest");
            runtimeGo.SetActive(false);
            var runtime = runtimeGo.AddComponent<RobotControlV3RuntimeController>();
            var controlRoot = new GameObject("RobotRoot");
            var baseLink = new GameObject("base_link");
            var toolMount = new GameObject("ToolMount");
            var factoryHost = new GameObject("FrameGizmos");
            var baseGizmoGo = new GameObject("BaseFrameGizmo");
            var toolGizmoGo = new GameObject("ToolFrameGizmo");
            var legacyFactory = factoryHost.AddComponent<FrameGizmoFactory>();
            var baseGizmo = baseGizmoGo.AddComponent<FrameGizmo>();
            var toolGizmo = toolGizmoGo.AddComponent<FrameGizmo>();

            try
            {
                baseLink.transform.SetParent(controlRoot.transform, false);
                toolMount.transform.SetParent(controlRoot.transform, false);
                baseLink.transform.position = new Vector3(0f, 0.2f, 0f);
                toolMount.transform.position = new Vector3(0.4f, 0.8f, 0f);
                legacyFactory.SetVisible(true);

                SetPrivateField(runtime, "controlRobotInstance", controlRoot);
                SetPrivateField(runtime, "runtimeRoot", controlRoot.transform);
                SetPrivateField(runtime, "frameGizmoFactory", legacyFactory);
                SetPrivateField(runtime, "baseFrameGizmo", baseGizmo);
                SetPrivateField(runtime, "toolFrameGizmo", toolGizmo);
                SetPrivateField(runtime, "showBaseFrame", true);
                SetPrivateField(runtime, "showToolFrame", false);

                InvokePrivate(runtime, "ApplyBaseAndToolFrameState");

                Assert.That(legacyFactory.IsVisible, Is.False);
                Assert.That(HasAnyVisibleLine(baseGizmoGo), Is.True);
                Assert.That(HasAnyVisibleLine(toolGizmoGo), Is.False);
                Assert.That(baseGizmoGo.transform.position, Is.EqualTo(baseLink.transform.position));
            }
            finally
            {
                Object.DestroyImmediate(toolGizmoGo);
                Object.DestroyImmediate(baseGizmoGo);
                Object.DestroyImmediate(factoryHost);
                Object.DestroyImmediate(controlRoot);
                Object.DestroyImmediate(runtimeGo);
            }
        }

        [Test]
        public void SelectedLinkHighlighter_SelectAndClear_TracksSelectedRenderer()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "upperarm_link_0";
            var highlighter = root.AddComponent<SelectedLinkHighlighter>();
            var renderer = root.GetComponent<Renderer>();
            var originalMaterials = renderer.sharedMaterials;

            try
            {
                highlighter.Select(root.transform);

                Assert.That(highlighter.SelectedTarget, Is.SameAs(root.transform));
                Assert.That(renderer.sharedMaterials[0].name, Does.Contain("_Selected"));

                highlighter.Clear();

                Assert.That(highlighter.SelectedTarget, Is.Null);
                Assert.That(renderer.sharedMaterials, Is.EqualTo(originalMaterials));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PulseJointHighlight_ShowsOnlyRequestedRing()
        {
            var runtimeGo = new GameObject("RobotControlV3RuntimeControllerTest");
            runtimeGo.SetActive(false);
            var runtime = runtimeGo.AddComponent<RobotControlV3RuntimeController>();
            var ring0 = new GameObject("Ring0").AddComponent<JointHighlightRing>();
            var ring1 = new GameObject("Ring1").AddComponent<JointHighlightRing>();
            var ring2 = new GameObject("Ring2").AddComponent<JointHighlightRing>();

            try
            {
                SetPrivateField(runtime, "jointHighlightRings", new[] { ring0, ring1, ring2 });

                runtime.PulseJointHighlight(1);

                Assert.That(ring0.IsVisible, Is.False);
                Assert.That(ring1.IsVisible, Is.True);
                Assert.That(ring2.IsVisible, Is.False);

                runtime.ClearJointHighlight();

                Assert.That(ring0.IsVisible, Is.False);
                Assert.That(ring1.IsVisible, Is.False);
                Assert.That(ring2.IsVisible, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(ring2.gameObject);
                Object.DestroyImmediate(ring1.gameObject);
                Object.DestroyImmediate(ring0.gameObject);
                Object.DestroyImmediate(runtimeGo);
            }
        }

        private static bool HasAnyVisibleLine(GameObject target)
        {
            var renderers = target.GetComponentsInChildren<LineRenderer>(true);
            for (var index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] != null && renderers[index].enabled)
                {
                    return true;
                }
            }

            return false;
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
