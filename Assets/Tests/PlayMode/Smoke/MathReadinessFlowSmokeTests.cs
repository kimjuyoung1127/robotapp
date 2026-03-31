// Folder: Tests/PlayMode - PlayMode smoke and scene flow tests.
// MathReadinessFlowSmoke 흐름을 검증하는 PlayMode 테스트입니다.
using System.Collections;
using KineTutor3D.App;
using KineTutor3D.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KineTutor3D.Tests.PlayMode
{
    public class MathReadinessFlowSmokeTests
    {
        [UnitySetUp]
        public IEnumerator Setup()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Onboarding_Beginner_LoadsMathReadiness_AndSetsMathTrack()
        {
            yield return LoadScene("Onboarding");

            var button = Find<Button>("BtnBeginner");
            Assert.That(button, Is.Not.Null);

            button.onClick.Invoke();
            yield return WaitForActiveScene("MathReadiness");

            Assert.That(StepProgressSaver.GetCurrentTrack(), Is.EqualTo(StepProgressSaver.MathReadinessTrack));
            Assert.That(RobotSelectionBridge.GetSelectedRobotId(), Is.EqualTo("2DOF_RR"));
        }

        [UnityTest]
        public IEnumerator RobotLibrary_HasMathWarmupButton()
        {
            StepProgressSaver.MarkVisited();
            yield return LoadScene("RobotLibrary");

            var button = Find<Button>("BtnStartMathReadiness");
            Assert.That(button, Is.Not.Null);
            Assert.That(button.interactable, Is.True);
        }

        [UnityTest]
        public IEnumerator MathReadiness_M0_StartsInManipulationPhase()
        {
            yield return LoadMathReadinessScene();

            var next = Find<Button>("BtnNext");
            var readiness = Find<Button>("BtnReadinessChoice_0");
            var instruction = FindObject("MRP_ManipulationInstruction");
            var targetBadge = FindObject("MRP_TargetBadge");

            Assert.That(next, Is.Not.Null);
            Assert.That(readiness, Is.Not.Null);
            Assert.That(readiness.gameObject.activeSelf, Is.False, "조작 단계에서는 확인 질문 선택지가 보이면 안 됩니다.");
            Assert.That(instruction, Is.Not.Null);
            Assert.That(instruction.activeSelf, Is.True);
            Assert.That(targetBadge, Is.Not.Null);
            Assert.That(targetBadge.activeSelf, Is.True);
            Assert.That(next.interactable, Is.False);
        }

        [UnityTest]
        public IEnumerator MathReadiness_M0_SliderReach_ShowsQuestion()
        {
            yield return LoadMathReadinessScene();

            var slider = Find<Slider>("joint_slider_1");
            var readiness = Find<Button>("BtnReadinessChoice_1");
            Assert.That(slider, Is.Not.Null);
            Assert.That(readiness, Is.Not.Null);

            slider.value = 90f;
            yield return null;

            Assert.That(readiness.gameObject.activeSelf, Is.True, "목표 각도에 도달하면 확인 질문이 나타나야 합니다.");
        }

        [UnityTest]
        public IEnumerator MathReadiness_M0_WrongAnswer_ShowsSoftCorrection()
        {
            yield return LoadMathReadinessScene();

            var slider = Find<Slider>("joint_slider_1");
            Assert.That(slider, Is.Not.Null);

            slider.value = 90f;
            yield return null;

            Find<Button>("BtnReadinessChoice_0").onClick.Invoke();
            yield return null;

            var panel = Object.FindFirstObjectByType<MathReadinessPanel>();
            Assert.That(panel, Is.Not.Null);
            StringAssert.Contains("조금만 다시 볼게요", panel.CurrentFeedbackText);
        }

        [UnityTest]
        public IEnumerator MathReadiness_M0_CorrectAnswer_UnlocksNext()
        {
            yield return LoadMathReadinessScene();

            var correct = Find<Button>("BtnReadinessChoice_1");
            var next = Find<Button>("BtnNext");
            var slider = Find<Slider>("joint_slider_1");

            Assert.That(correct, Is.Not.Null);
            Assert.That(next, Is.Not.Null);
            Assert.That(slider, Is.Not.Null);

            slider.value = 90f;
            yield return null;
            correct.onClick.Invoke();
            yield return null;

            Assert.That(next.interactable, Is.True);
        }

        [UnityTest]
        public IEnumerator MathReadiness_FinalCorrectAnswer_DisablesQuestionChoices()
        {
            yield return LoadMathReadinessScene();

            var correct = Find<Button>("BtnReadinessChoice_1");
            var slider = Find<Slider>("joint_slider_1");

            Assert.That(slider, Is.Not.Null);
            slider.value = 90f;
            yield return null;
            correct.onClick.Invoke();
            yield return null;

            Assert.That(correct.interactable, Is.False, "최종 정답 후에는 같은 선택지를 다시 누를 수 없어야 합니다.");
        }

        [UnityTest]
        public IEnumerator MathReadiness_Rail_HidesUnusedJoints_WhenInteractiveJointCountIs1()
        {
            yield return LoadMathReadinessScene();

            var slider1 = Find<Slider>("joint_slider_1");
            var slider2 = Find<Slider>("joint_slider_2");

            Assert.That(slider1, Is.Not.Null);
            Assert.That(slider1.gameObject.activeInHierarchy, Is.True);
            Assert.That(slider2, Is.Not.Null);
            Assert.That(slider2.gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator MathReadiness_CoachHint_StartsCollapsed()
        {
            yield return LoadMathReadinessScene();

            var coachHint = FindObject("MRP_CoachHintBody");
            Assert.That(coachHint, Is.Not.Null);
            Assert.That(coachHint.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator MathReadiness_M0_TwoWrongAnswers_ShowsAdaptiveHint()
        {
            yield return LoadMathReadinessScene();

            var slider = Find<Slider>("joint_slider_1");
            Assert.That(slider, Is.Not.Null);
            slider.value = 90f;
            yield return null;

            // First wrong answer (index 0 is wrong for M0)
            Find<Button>("BtnReadinessChoice_0").onClick.Invoke();
            yield return null;

            // Second wrong answer (index 2 is wrong for M0)
            Find<Button>("BtnReadinessChoice_2").onClick.Invoke();
            yield return null;

            var panel = Object.FindFirstObjectByType<MathReadinessPanel>();
            Assert.That(panel, Is.Not.Null);
            Assert.That(panel.CurrentQuestionAttemptCount, Is.GreaterThanOrEqualTo(2));

            var adaptiveHint = FindObject("MRP_AdaptiveHint");
            Assert.That(adaptiveHint, Is.Not.Null);
            Assert.That(adaptiveHint.activeSelf, Is.True, "Adaptive hint should appear after 2 wrong answers");
        }

        [UnityTest]
        public IEnumerator MathReadiness_ProgressBadge_ShowsCorrectCount()
        {
            yield return LoadMathReadinessScene();

            var slider = Find<Slider>("joint_slider_1");
            Assert.That(slider, Is.Not.Null);
            slider.value = 90f;
            yield return null;

            var badge = FindObject("MRP_ProgressBadge");
            Assert.That(badge, Is.Not.Null);

            var badgeText = badge.GetComponentInChildren<Text>();
            Assert.That(badgeText, Is.Not.Null);
            StringAssert.Contains("Q1/", badgeText.text);
        }

        [UnityTest]
        public IEnumerator MathReadiness_ShowsAngleReferenceMarkers()
        {
            yield return LoadMathReadinessScene();

            var angleRef = FindObject("AngleReference_J0");
            Assert.That(angleRef, Is.Not.Null, "MathReadiness는 0°/90°/180° 기준선을 표시해야 합니다.");
        }

        [UnityTest]
        public IEnumerator MathReadiness_FinalStep_BridgesTo_PreKinematics()
        {
            yield return LoadMathReadinessScene();

            var app = Object.FindFirstObjectByType<AppController>();
            Assert.That(app, Is.Not.Null);

            app.SetCurrentStep(4);
            yield return null;
            app.NextStep();
            yield return WaitForActiveScene("Sandbox");

            var reloadedApp = Object.FindFirstObjectByType<AppController>();
            Assert.That(reloadedApp, Is.Not.Null);
            Assert.That(reloadedApp.CurrentTrack, Is.EqualTo(StepProgressSaver.PreKinematicsTrack));
            Assert.That(reloadedApp.CurrentStep, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator MathReadiness_M3_TwoJointTarget_AllowsAnyAdjustmentOrder()
        {
            yield return LoadMathReadinessScene();

            var app = Object.FindFirstObjectByType<AppController>();
            Assert.That(app, Is.Not.Null);
            app.SetCurrentStep(4);
            yield return null;

            var slider1 = Find<Slider>("joint_slider_1");
            var slider2 = Find<Slider>("joint_slider_2");
            var readiness = Find<Button>("BtnReadinessChoice_1");

            Assert.That(slider1, Is.Not.Null);
            Assert.That(slider2, Is.Not.Null);
            Assert.That(readiness, Is.Not.Null);

            slider2.value = -45f;
            yield return null;
            Assert.That(readiness.gameObject.activeSelf, Is.False, "보조 목표만 맞춘 상태에서는 질문이 열리면 안 됩니다.");

            slider1.value = 45f;
            yield return null;
            Assert.That(readiness.gameObject.activeSelf, Is.True, "두 목표를 모두 만족하면 질문이 열려야 합니다.");
        }

        private static IEnumerator LoadMathReadinessScene()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.MathReadinessTrack);
            RobotSelectionBridge.SetSelection("2DOF_RR", RobotSelectionBridge.GuidedLessonMode);
            yield return LoadScene("MathReadiness");
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            Assert.That(op, Is.Not.Null);
            while (!op.isDone)
            {
                yield return null;
            }

            yield return null;
        }

        private static IEnumerator WaitForActiveScene(string sceneName)
        {
            for (var i = 0; i < 120; i++)
            {
                if (SceneManager.GetActiveScene().name == sceneName)
                {
                    yield return null;
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"활성 씬이 {sceneName}으로 전환되지 않았습니다.");
        }

        private static T Find<T>(string objectName) where T : Component
        {
            var go = FindObject(objectName);
            return go != null ? go.GetComponent<T>() : null;
        }

        private static GameObject FindObject(string objectName)
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null)
                {
                    continue;
                }

                var transforms = root.GetComponentsInChildren<Transform>(true);
                for (var j = 0; j < transforms.Length; j++)
                {
                    var candidate = transforms[j];
                    if (candidate != null && candidate.name == objectName)
                    {
                        return candidate.gameObject;
                    }
                }
            }

            return null;
        }
    }
}
