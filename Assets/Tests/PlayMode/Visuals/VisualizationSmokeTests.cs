// Folder: Tests/PlayMode - PlayMode smoke and scene flow tests.
// VisualizationSmoke 흐름을 검증하는 PlayMode 테스트입니다.
using System.Collections;
using KineTutor3D.UI;
using KineTutor3D.Visualization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KineTutor3D.Tests.PlayMode
{
    /// <summary>
    /// Validates the 2DOF visualization pipeline in the main scene.
    /// </summary>
    public class VisualizationSmokeTests
    {
        [UnitySetUp]
        public IEnumerator Setup()
        {
            PlayerPrefs.DeleteAll();
            LogAssert.ignoreFailingMessages = true;
            yield return LoadMainScene();
        }

        [TearDown]
        public void TearDown()
        {
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator RobotRenderer_UsesCanonicalFrameObjects()
        {
            var renderer = Object.FindFirstObjectByType<RobotRenderer>();
            for (var i = 0; i < 60 && renderer == null; i++)
            {
                yield return null;
                renderer = Object.FindFirstObjectByType<RobotRenderer>();
            }

            Assert.That(renderer, Is.Not.Null, "RobotRenderer를 찾지 못했습니다.");
            Assert.That(GameObject.Find("RobotRoot"), Is.Not.Null, "RobotRoot가 필요합니다.");
            Assert.That(GameObject.Find("frame_0"), Is.Not.Null, "frame_0가 필요합니다.");
            Assert.That(GameObject.Find("frame_1"), Is.Not.Null, "frame_1가 필요합니다.");
            Assert.That(GameObject.Find("Frame_EE"), Is.Not.Null, "Frame_EE가 필요합니다.");
            Assert.That(GameObject.Find("WorldFrame"), Is.Null.Or.Property("activeSelf").False, "중복 WorldFrame은 비활성 상태여야 합니다.");
            Assert.That(GameObject.Find("Frame_1"), Is.Null.Or.Property("activeSelf").False, "중복 Frame_1은 비활성 상태여야 합니다.");

            Assert.That(GameObject.Find("frame_0").GetComponent<FrameGizmo>(), Is.Not.Null, "frame_0에 FrameGizmo가 필요합니다.");
            Assert.That(GameObject.Find("frame_1").GetComponent<FrameGizmo>(), Is.Not.Null, "frame_1에 FrameGizmo가 필요합니다.");
            Assert.That(GameObject.Find("Frame_EE").GetComponent<FrameGizmo>(), Is.Not.Null, "Frame_EE에 FrameGizmo가 필요합니다.");
        }

        [UnityTest]
        public IEnumerator EndEffectorFrame_FollowsFkReferenceCases()
        {
            var slider1 = FindSlider("joint_slider_1");
            var slider2 = FindSlider("joint_slider_2");
            var eeFrame = GameObject.Find("Frame_EE");

            Assert.That(slider1, Is.Not.Null, "joint_slider_1 Slider를 찾지 못했습니다.");
            Assert.That(slider2, Is.Not.Null, "joint_slider_2 Slider를 찾지 못했습니다.");
            Assert.That(eeFrame, Is.Not.Null, "Frame_EE 오브젝트를 찾지 못했습니다.");

            slider1.value = 0f;
            slider2.value = 0f;
            yield return null;

            Assert.That(Vector3.Distance(eeFrame.transform.position, new Vector3(2f, 0f, 0f)), Is.LessThan(1e-4f));

            slider1.value = 90f;
            slider2.value = 0f;
            yield return null;

            Assert.That(Vector3.Distance(eeFrame.transform.position, new Vector3(0f, 0f, 2f)), Is.LessThan(1e-4f));
        }

        [UnityTest]
        public IEnumerator CanonicalFrames_KeepTooltipTargets_AndHideLegacyMarkers()
        {
            var frame0 = GameObject.Find("frame_0");
            var frame1 = GameObject.Find("frame_1");

            Assert.That(frame0, Is.Not.Null, "frame_0를 찾지 못했습니다.");
            Assert.That(frame1, Is.Not.Null, "frame_1를 찾지 못했습니다.");

            var frame0Tooltip = frame0.GetComponent<TooltipTrigger3D>();
            var frame1Tooltip = frame1.GetComponent<TooltipTrigger3D>();
            var frame0Renderer = frame0.GetComponent<MeshRenderer>();
            var frame1Renderer = frame1.GetComponent<MeshRenderer>();

            Assert.That(frame0Tooltip, Is.Not.Null, "frame_0 TooltipTrigger3D가 필요합니다.");
            Assert.That(frame1Tooltip, Is.Not.Null, "frame_1 TooltipTrigger3D가 필요합니다.");
            Assert.That(frame0Tooltip.TargetId, Is.EqualTo("frame_0"));
            Assert.That(frame1Tooltip.TargetId, Is.EqualTo("frame_1"));
            Assert.That(frame0Renderer == null || frame0Renderer.enabled == false, Is.True, "frame_0 legacy marker는 숨겨져야 합니다.");
            Assert.That(frame1Renderer == null || frame1Renderer.enabled == false, Is.True, "frame_1 legacy marker는 숨겨져야 합니다.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator DonorMeshVisuals_ArePresent_AndFollowFk()
        {
            var link0 = GameObject.Find("Link0Visual");
            var link1 = GameObject.Find("Link1Visual");
            var ee = GameObject.Find("EndEffectorVisualMesh");
            var source = FindAny("ScaraDonorProbe");
            var slider1 = FindSlider("joint_slider_1");

            Assert.That(link0, Is.Not.Null, "Link0Visual이 필요합니다.");
            Assert.That(link1, Is.Not.Null, "Link1Visual이 필요합니다.");
            Assert.That(ee, Is.Not.Null, "EndEffectorVisualMesh가 필요합니다.");
            Assert.That(source, Is.Not.Null, "ScaraDonorProbe source가 필요합니다.");
            Assert.That(source.activeSelf, Is.False, "ScaraDonorProbe source는 숨겨져야 합니다.");

            var before = ee.transform.position;
            slider1.value = 90f;
            yield return null;
            var after = ee.transform.position;

            Assert.That(Vector3.Distance(before, after), Is.GreaterThan(0.5f), "FK 변경 후 donor EE visual이 이동해야 합니다.");
        }

        [UnityTest]
        public IEnumerator UrpPipeline_IsActive_AndDonorMaterialsAreNotErrorShaders()
        {
            yield return null;

            Assert.That(GraphicsSettings.currentRenderPipeline, Is.Not.Null, "URP render pipeline asset이 활성 상태여야 합니다.");

            foreach (var name in new[] { "BaseVisual", "Link0Visual", "Link1Visual", "EndEffectorVisualMesh" })
            {
                var go = GameObject.Find(name);
                Assert.That(go, Is.Not.Null, $"{name} 오브젝트를 찾지 못했습니다.");

                var renderer = go.GetComponent<MeshRenderer>();
                Assert.That(renderer, Is.Not.Null, $"{name} MeshRenderer가 필요합니다.");
                Assert.That(renderer.sharedMaterial, Is.Not.Null, $"{name} material이 비어 있으면 안 됩니다.");
                Assert.That(renderer.sharedMaterial.shader, Is.Not.Null, $"{name} shader가 비어 있으면 안 됩니다.");
                Assert.That(renderer.sharedMaterial.shader.name, Is.Not.EqualTo("Hidden/InternalErrorShader"), $"{name}가 에러 셰이더로 렌더링되고 있습니다.");
            }
        }

        [UnityTest]
        public IEnumerator Canvas_UsesOverlayHudMode()
        {
            var canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();

            Assert.That(canvas, Is.Not.Null, "Canvas is required.");
            Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay), "Canvas should be fixed as HUD overlay.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator RobotRenderer_UsesExplicitScaraDonorSources_AndNotPick()
        {
            var renderer = Object.FindFirstObjectByType<RobotRenderer>();

            for (var i = 0; i < 60 && renderer == null; i++)
            {
                yield return null;
                renderer = Object.FindFirstObjectByType<RobotRenderer>();
            }

            Assert.That(renderer, Is.Not.Null, "RobotRenderer瑜?李얠? 紐삵뻽?듬땲??");
            Assert.That(renderer.DonorBaseSource, Is.Not.Null, "Base donor source is required.");
            Assert.That(renderer.DonorLink0Source, Is.Not.Null, "Axis1 donor source is required.");
            Assert.That(renderer.DonorLink1Source, Is.Not.Null, "Axis2 donor source is required.");
            Assert.That(renderer.DonorEndEffectorSource, Is.Not.Null, "Gripper donor source is required.");
            Assert.That(renderer.DonorPickSource, Is.Not.Null, "Pick helper should still exist.");
            Assert.That(renderer.UsesPickAsEndEffectorSource, Is.False, "Pick must not be selected as the EE donor.");
        }

        [UnityTest]
        public IEnumerator DonorVisuals_HaveNonZeroBounds_AndMoveOnScreen()
        {
            var camera = Camera.main;
            var ee = GameObject.Find("EndEffectorVisualMesh");
            var slider1 = FindSlider("joint_slider_1");

            Assert.That(camera, Is.Not.Null, "Main Camera媛 ?꾩슂?⑸땲??");
            Assert.That(ee, Is.Not.Null, "EndEffectorVisualMesh媛 ?꾩슂?⑸땲??");
            Assert.That(slider1, Is.Not.Null, "joint_slider_1 Slider瑜?李얠? 紐삵뻽?듬땲??");

            var eeRenderer = ee.GetComponent<MeshRenderer>();
            Assert.That(eeRenderer, Is.Not.Null, "EndEffectorVisualMesh renderer is required.");
            Assert.That(eeRenderer.bounds.size.sqrMagnitude, Is.GreaterThan(1e-6f), "EndEffectorVisualMesh bounds should not be zero.");

            slider1.value = 0f;
            yield return null;
            var before = camera.WorldToScreenPoint(eeRenderer.bounds.center);

            slider1.value = 90f;
            yield return null;
            var after = camera.WorldToScreenPoint(eeRenderer.bounds.center);

            Assert.That(Vector2.Distance(new Vector2(before.x, before.y), new Vector2(after.x, after.y)), Is.GreaterThan(10f),
                "EE donor visual should move on screen when the first joint rotates.");
        }

        [UnityTest]
        public IEnumerator RobotAggregateBounds_StayInsideMainCameraFrustum()
        {
            var renderer = Object.FindFirstObjectByType<RobotRenderer>();
            var camera = Camera.main;

            for (var i = 0; i < 60 && (renderer == null || camera == null); i++)
            {
                yield return null;
                renderer = Object.FindFirstObjectByType<RobotRenderer>();
                camera = Camera.main;
            }

            Assert.That(renderer, Is.Not.Null, "RobotRenderer를 찾지 못했습니다.");
            Assert.That(camera, Is.Not.Null, "Main Camera가 필요합니다.");
            Assert.That(renderer.IsVisibleFrom(camera), Is.True, "RobotRoot aggregate bounds가 Main Camera frustum 안에 있어야 합니다.");
        }

        private static Slider FindSlider(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.GetComponent<Slider>() : null;
        }

        private static GameObject FindAny(string name)
        {
            var active = GameObject.Find(name);
            if (active != null)
            {
                return active;
            }

            foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.name == name)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static IEnumerator LoadMainScene()
        {
            var op = SceneManager.LoadSceneAsync("Sandbox", LoadSceneMode.Single);
            Assert.That(op, Is.Not.Null, "Sandbox 씬 로드를 시작하지 못했습니다.");
            while (!op.isDone)
            {
                yield return null;
            }

            yield return null;
        }
    }
}
