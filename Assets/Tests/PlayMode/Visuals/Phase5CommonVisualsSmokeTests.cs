// Folder: Tests/PlayMode - PlayMode smoke and scene flow tests.
// Phase5CommonVisualsSmoke 흐름을 검증하는 PlayMode 테스트입니다.
using System.Collections;
using KineTutor3D.App;
using KineTutor3D.UI;
using KineTutor3D.Visualization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KineTutor3D.Tests.PlayMode
{
    /// <summary>
    /// Phase 5 공통 입력/시각화 기반 컴포넌트의 런타임 연결을 검증합니다.
    /// </summary>
    public class Phase5CommonVisualsSmokeTests
    {
        [UnitySetUp]
        public IEnumerator Setup()
        {
            PlayerPrefs.DeleteAll();
            yield return LoadMainScene();
        }

        [UnityTest]
        public IEnumerator JointInputRail_SyncsWithSliderAndAppState()
        {
            var app = Object.FindFirstObjectByType<AppController>();
            Assert.That(app, Is.Not.Null, "AppController가 필요합니다.");

            app.SetCurrentStep(3);
            yield return new WaitForSecondsRealtime(0.35f);

            var rail = Object.FindFirstObjectByType<JointInputRail>();
            var slider1 = GameObject.Find("joint_slider_1")?.GetComponent<Slider>();

            Assert.That(rail, Is.Not.Null, "JointInputRail이 필요합니다.");
            Assert.That(slider1, Is.Not.Null, "joint_slider_1 Slider가 필요합니다.");
            Assert.That(rail.JointInput1, Is.Not.Null, "Joint 1 numeric input이 필요합니다.");

            slider1.value = 42f;
            yield return null;
            Assert.That(rail.JointInput1.text, Is.EqualTo("42.0"));

            rail.JointInput1.text = "15";
            rail.JointInput1.onEndEdit.Invoke("15");
            yield return null;

            Assert.That(slider1.value, Is.EqualTo(15f).Within(0.01f));
            Assert.That(app.ChangedJointIndex, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator RobotRenderer_HighlightJoint_CreatesToggleableRings()
        {
            var renderer = Object.FindFirstObjectByType<RobotRenderer>();
            Assert.That(renderer, Is.Not.Null, "RobotRenderer가 필요합니다.");

            renderer.HighlightJoint(0);
            yield return null;

            var ring0 = GameObject.Find("JointHighlightRing_0");
            Assert.That(ring0, Is.Not.Null, "JointHighlightRing_0가 필요합니다.");
            Assert.That(ring0.GetComponent<LineRenderer>().enabled, Is.True);

            renderer.ClearJointHighlight();
            yield return null;
            Assert.That(ring0.GetComponent<LineRenderer>().enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator StepWithoutJointHighlight_DoesNotReenableHighlightOnJointChange()
        {
            var app = Object.FindFirstObjectByType<AppController>();
            var renderer = Object.FindFirstObjectByType<RobotRenderer>();

            Assert.That(app, Is.Not.Null, "AppController가 필요합니다.");
            Assert.That(renderer, Is.Not.Null, "RobotRenderer가 필요합니다.");

            app.SetCurrentStep(1);
            yield return null;

            app.SetJointAngleDegrees(0, 18f);
            yield return null;

            var ring0 = GameObject.Find("JointHighlightRing_0");
            if (ring0 != null)
            {
                Assert.That(ring0.GetComponent<LineRenderer>().enabled, Is.False);
            }
        }

        [UnityTest]
        public IEnumerator TrailAndTargetMarker_ToggleIndependentVisibility()
        {
            var app = Object.FindFirstObjectByType<AppController>();
            var trail = Object.FindFirstObjectByType<EndEffectorTrail>();
            var marker = Object.FindFirstObjectByType<TargetMarkerVisual>();
            var slider1 = GameObject.Find("joint_slider_1")?.GetComponent<Slider>();

            Assert.That(app, Is.Not.Null, "AppController가 필요합니다.");
            Assert.That(trail, Is.Not.Null, "EndEffectorTrail이 필요합니다.");
            Assert.That(marker, Is.Not.Null, "TargetMarkerVisual이 필요합니다.");
            Assert.That(slider1, Is.Not.Null, "joint_slider_1 Slider가 필요합니다.");

            trail.SetTrailVisible(true);
            marker.SetMarkersVisible(true);
            yield return null;

            slider1.value = 25f;
            yield return null;
            slider1.value = 55f;
            yield return null;

            Assert.That(trail.PointCount, Is.GreaterThan(1), "trail은 FK 변화 후 점을 기록해야 합니다.");
            Assert.That(marker.TargetMarker, Is.Not.Null, "타깃 마커 오브젝트가 필요합니다.");
            Assert.That(marker.TargetMarker.activeSelf, Is.True, "타깃 마커는 visible 상태여야 합니다.");

            trail.SetTrailVisible(false);
            marker.SetMarkersVisible(false);
            yield return null;

            Assert.That(trail.PointCount, Is.EqualTo(0));
            Assert.That(marker.TargetMarker.activeSelf, Is.False);
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
