using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KineTutor3D.Tests.PlayMode
{
    /// <summary>
    /// 학생 친화 UX의 핵심 런타임 흐름을 검증하는 PlayMode 스모크 테스트입니다.
    /// </summary>
    public class UxFlowSmokeTests
    {
        [UnitySetUp]
        public IEnumerator Setup()
        {
            PlayerPrefs.DeleteAll();
            yield return LoadMainScene();
        }

        [UnityTest]
        public IEnumerator Onboarding_FirstRun_ShowsModal_And_StartEntersStep1()
        {
            Component app = null;
            for (var i = 0; i < 60 && app == null; i++)
            {
                var appGo = Find("AppController");
                app = appGo != null ? appGo.GetComponent("AppController") : null;
                if (app == null) yield return null;
            }
            Assert.That(app, Is.Not.Null, "AppController를 찾지 못했습니다.");

            var modal = Find("WelcomeModal");
            Assert.That(modal, Is.Not.Null, "WelcomeModal 오브젝트를 찾지 못했습니다.");
            Assert.That(modal.activeSelf, Is.True, "첫 실행 시 WelcomeModal은 활성 상태여야 합니다.");

            var startButton = FindComponent<Button>("BtnStartLearning");
            Assert.That(startButton, Is.Not.Null, "BtnStartLearning Button 컴포넌트를 찾지 못했습니다.");
            startButton.onClick.Invoke();
            yield return null;

            Assert.That(GetCurrentStep(app), Is.EqualTo(1), "학습 시작 클릭 후 Step 1로 진입해야 합니다.");
            Assert.That(modal.activeSelf, Is.False, "학습 시작 후 WelcomeModal은 비활성 상태여야 합니다.");
        }

        [UnityTest]
        public IEnumerator Gate_Step1_LocksNext_ThenUnlocks_AfterRequiredInteractions()
        {
            yield return ReloadWithProgress(1, 0);

            Component app = null;
            for (var i = 0; i < 60 && app == null; i++)
            {
                var appGo = Find("AppController");
                app = appGo != null ? appGo.GetComponent("AppController") : null;
                if (app == null) yield return null;
            }

            var nextButton = FindComponent<Button>("BtnNext");
            Assert.That(app, Is.Not.Null, "AppController를 찾지 못했습니다.");
            Assert.That(nextButton, Is.Not.Null, "BtnNext Button 컴포넌트를 찾지 못했습니다.");

            InvokeMethod(app, "SetCurrentStep", 1);
            yield return null;
            Assert.That(nextButton.interactable, Is.False, "Step 1 게이트 충족 전 Next 버튼은 비활성 상태여야 합니다.");

            InvokeReportInteraction(app, "Hover", "dh_theta_header");
            InvokeReportInteraction(app, "Hover", "dh_alpha_header");
            yield return null;

            Assert.That(nextButton.interactable, Is.True, "Step 1 게이트 충족 후 Next 버튼은 활성 상태여야 합니다.");
        }

        [UnityTest]
        public IEnumerator SkipButton_ForcesCurrentStepPass_AndMovesForward()
        {
            yield return ReloadWithProgress(1, 0);

            Component app = null;
            for (var i = 0; i < 60 && app == null; i++)
            {
                var appGo = Find("AppController");
                app = appGo != null ? appGo.GetComponent("AppController") : null;
                if (app == null) yield return null;
            }

            var skipButton = FindComponent<Button>("BtnSkip");
            Assert.That(app, Is.Not.Null, "AppController를 찾지 못했습니다.");
            Assert.That(skipButton, Is.Not.Null, "BtnSkip Button 컴포넌트를 찾지 못했습니다.");

            InvokeMethod(app, "SetCurrentStep", 1);
            yield return null;
            skipButton.onClick.Invoke();
            yield return null;

            Assert.That(GetCurrentStep(app), Is.EqualTo(2), "Skip 클릭 시 다음 스텝으로 이동해야 합니다.");
        }

        [UnityTest]
        public IEnumerator StepVisibility_MatchesMatrix_ForAllSteps()
        {
            yield return ReloadWithProgress(1, 0);

            Component app = null;
            for (var i = 0; i < 60 && app == null; i++)
            {
                var appGo = Find("AppController");
                app = appGo != null ? appGo.GetComponent("AppController") : null;
                if (app == null) yield return null;
            }

            var leftPanel = Find("LeftPanel");
            var rightPanel = Find("RightPanel");
            var bottomBar = Find("BottomBar");

            Assert.That(app, Is.Not.Null, "AppController를 찾지 못했습니다.");
            Assert.That(leftPanel, Is.Not.Null, "LeftPanel 오브젝트를 찾지 못했습니다.");
            Assert.That(rightPanel, Is.Not.Null, "RightPanel 오브젝트를 찾지 못했습니다.");
            Assert.That(bottomBar, Is.Not.Null, "BottomBar 오브젝트를 찾지 못했습니다.");

            var expectLeft = new[] { true, false, true, true, true, true, true, true };
            var expectRight = new[] { false, true, false, false, true, true, true, true };
            var expectBottom = new[] { false, false, true, false, true, true, true, true };

            for (var i = 0; i < 8; i++)
            {
                InvokeMethod(app, "SetCurrentStep", i + 1);
                yield return new WaitForSecondsRealtime(0.35f);

                Assert.That(leftPanel.activeSelf, Is.EqualTo(expectLeft[i]), $"Step {i + 1} LeftPanel 상태 불일치");
                Assert.That(rightPanel.activeSelf, Is.EqualTo(expectRight[i]), $"Step {i + 1} RightPanel 상태 불일치");
                Assert.That(bottomBar.activeSelf, Is.EqualTo(expectBottom[i]), $"Step {i + 1} BottomBar 상태 불일치");
            }
        }

        [UnityTest]
        public IEnumerator Tooltip_And_Glossary_OpenClose_Work()
        {
            yield return ReloadWithProgress(1, 0);

            Component app = null;
            for (var i = 0; i < 60 && app == null; i++)
            {
                var appGo = Find("AppController");
                app = appGo != null ? appGo.GetComponent("AppController") : null;
                if (app == null) yield return null;
            }

            var triggerGo = Find("DHTableTarget");
            var trigger = triggerGo != null ? triggerGo.GetComponent("TooltipTriggerUI") : null;
            var glossaryPanel = Find("GlossaryPanel");
            var openButton = FindComponent<Button>("BtnGlossaryOpen");
            var closeButton = FindComponent<Button>("BtnGlossaryClose");

            Assert.That(app, Is.Not.Null, "AppController를 찾지 못했습니다.");
            Assert.That(EventSystem.current, Is.Not.Null, "EventSystem이 필요합니다.");
            Assert.That(trigger, Is.Not.Null, "DHTableTarget TooltipTriggerUI를 찾지 못했습니다.");
            Assert.That(openButton, Is.Not.Null, "BtnGlossaryOpen Button 컴포넌트를 찾지 못했습니다.");
            Assert.That(closeButton, Is.Not.Null, "BtnGlossaryClose Button 컴포넌트를 찾지 못했습니다.");

            InvokeMethod(trigger, "OnPointerEnter", new PointerEventData(EventSystem.current));
            Assert.That(GameObject.Find("TooltipRoot"), Is.Not.Null, "툴팁 표시 후 TooltipRoot는 활성 상태여야 합니다.");

            InvokeMethod(trigger, "OnPointerExit", new PointerEventData(EventSystem.current));
            Assert.That(GameObject.Find("TooltipRoot"), Is.Null, "툴팁 숨김 후 TooltipRoot는 비활성 상태여야 합니다.");

            openButton.onClick.Invoke();
            yield return null;
            Assert.That(glossaryPanel.activeSelf, Is.True, "용어 사전 열기 후 GlossaryPanel은 활성 상태여야 합니다.");

            closeButton.onClick.Invoke();
            yield return null;
            Assert.That(glossaryPanel.activeSelf, Is.False, "용어 사전 닫기 후 GlossaryPanel은 비활성 상태여야 합니다.");
        }

        [UnityTest]
        public IEnumerator SliderDrivenFk_ZeroZero_MatchesReferencePosition()
        {
            yield return ReloadWithProgress(1, 0);

            Component app = null;
            for (var i = 0; i < 60 && app == null; i++)
            {
                var appGo = Find("AppController");
                app = appGo != null ? appGo.GetComponent("AppController") : null;
                if (app == null) yield return null;
            }

            var slider1 = FindComponent<Slider>("joint_slider_1");
            var slider2 = FindComponent<Slider>("joint_slider_2");

            Assert.That(app, Is.Not.Null, "AppController를 찾지 못했습니다.");
            Assert.That(slider1, Is.Not.Null, "joint_slider_1 Slider 컴포넌트를 찾지 못했습니다.");
            Assert.That(slider2, Is.Not.Null, "joint_slider_2 Slider 컴포넌트를 찾지 못했습니다.");

            slider1.value = 0f;
            slider2.value = 0f;
            yield return null;

            var pos = GetCurrentEndEffectorPosition(app);
            Assert.That(pos.x, Is.EqualTo(2.0).Within(1e-4));
            Assert.That(pos.y, Is.EqualTo(0.0).Within(1e-4));
            Assert.That(pos.z, Is.EqualTo(0.0).Within(1e-4));
        }

        [UnityTest]
        public IEnumerator SliderDrivenFk_PiOver2Zero_MatchesReferencePosition()
        {
            yield return ReloadWithProgress(1, 0);

            Component app = null;
            for (var i = 0; i < 60 && app == null; i++)
            {
                var appGo = Find("AppController");
                app = appGo != null ? appGo.GetComponent("AppController") : null;
                if (app == null) yield return null;
            }

            var slider1 = FindComponent<Slider>("joint_slider_1");
            var slider2 = FindComponent<Slider>("joint_slider_2");

            Assert.That(app, Is.Not.Null, "AppController를 찾지 못했습니다.");
            Assert.That(slider1, Is.Not.Null, "joint_slider_1 Slider 컴포넌트를 찾지 못했습니다.");
            Assert.That(slider2, Is.Not.Null, "joint_slider_2 Slider 컴포넌트를 찾지 못했습니다.");

            slider1.value = 90f;
            slider2.value = 0f;
            yield return null;

            var pos = GetCurrentEndEffectorPosition(app);
            Assert.That(pos.x, Is.EqualTo(0.0).Within(1e-4));
            Assert.That(pos.y, Is.EqualTo(2.0).Within(1e-4));
            Assert.That(pos.z, Is.EqualTo(0.0).Within(1e-4));
        }

        private static IEnumerator ReloadWithProgress(int hasVisited, int lastCompletedStep)
        {
            PlayerPrefs.SetInt("KineTutor3D.HasVisited", hasVisited);
            PlayerPrefs.SetInt("KineTutor3D.LastCompletedStep", lastCompletedStep);
            PlayerPrefs.Save();
            yield return LoadMainScene();
        }

        private static IEnumerator LoadMainScene()
        {
            var op = SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
            Assert.That(op, Is.Not.Null, "Main 씬 로드를 시작하지 못했습니다.");
            while (!op.isDone)
            {
                yield return null;
            }

            yield return null;
        }

        private static int GetCurrentStep(Component app)
        {
            var prop = app.GetType().GetProperty("CurrentStep", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(prop, Is.Not.Null, "CurrentStep 프로퍼티를 찾지 못했습니다.");
            return (int)prop.GetValue(app);
        }

        private static (double x, double y, double z) GetCurrentEndEffectorPosition(Component app)
        {
            var poseProp = app.GetType().GetProperty("CurrentEndEffectorPose", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(poseProp, Is.Not.Null, "CurrentEndEffectorPose 프로퍼티를 찾지 못했습니다.");

            var pose = poseProp.GetValue(app);
            Assert.That(pose, Is.Not.Null, "CurrentEndEffectorPose 값이 비어 있습니다.");

            var positionProp = pose.GetType().GetProperty("Position", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(positionProp, Is.Not.Null, "Pose.Position 프로퍼티를 찾지 못했습니다.");

            var position = positionProp.GetValue(pose);
            Assert.That(position, Is.Not.Null, "Pose.Position 값이 비어 있습니다.");

            var positionType = position.GetType();
            var x = (double)positionType.GetProperty("X", BindingFlags.Public | BindingFlags.Instance).GetValue(position);
            var y = (double)positionType.GetProperty("Y", BindingFlags.Public | BindingFlags.Instance).GetValue(position);
            var z = (double)positionType.GetProperty("Z", BindingFlags.Public | BindingFlags.Instance).GetValue(position);
            return (x, y, z);
        }

        private static void InvokeReportInteraction(Component app, string interactionTypeName, string targetId)
        {
            var appType = app.GetType();
            var enumType = appType.Assembly.GetType("KineTutor3D.UI.Data.InteractionType");
            Assert.That(enumType, Is.Not.Null, "InteractionType enum을 찾지 못했습니다.");

            var enumValue = Enum.Parse(enumType, interactionTypeName);
            var method = appType.GetMethod("ReportInteraction", new[] { enumType, typeof(string) });
            Assert.That(method, Is.Not.Null, "ReportInteraction 메서드를 찾지 못했습니다.");
            method.Invoke(app, new object[] { enumValue, targetId });
        }

        private static void InvokeMethod(Component component, string methodName, params object[] args)
        {
            var method = component.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, $"메서드 {methodName} 를 찾지 못했습니다.");
            method.Invoke(component, args);
        }

        private static GameObject Find(string name)
        {
            var active = GameObject.Find(name);
            if (active != null)
            {
                return active;
            }

            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            for (var i = 0; i < all.Length; i++)
            {
                if (all[i].name == name)
                {
                    return all[i];
                }
            }

            return null;
        }

        private static T FindComponent<T>(string name) where T : Component
        {
            var go = Find(name);
            return go != null ? go.GetComponent<T>() : null;
        }
    }
}
