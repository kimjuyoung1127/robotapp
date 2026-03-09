using System.Collections;
using KineTutor3D.App;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KineTutor3D.Tests.PlayMode
{
    /// <summary>
    /// Boot, Onboarding, Main 씬 분기와 전역 네비게이션을 검증합니다.
    /// </summary>
    public class SceneFlowSmokeTests
    {
        [SetUp]
        public void ResetPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator Boot_FirstVisit_RoutesToOnboarding()
        {
            yield return LoadScene("Boot");
            yield return WaitForActiveScene("Onboarding");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Onboarding"));
        }

        [UnityTest]
        public IEnumerator Boot_Visited_RoutesToMain()
        {
            StepProgressSaver.MarkVisited();

            yield return LoadScene("Boot");
            yield return WaitForActiveScene("Main");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Main"));
        }

        [UnityTest]
        public IEnumerator Onboarding_StartLearning_LoadsMain_AndMarksVisited()
        {
            yield return LoadScene("Onboarding");

            var button = FindComponent<Button>("BtnStartLearning");
            Assert.That(button, Is.Not.Null, "BtnStartLearning을 찾지 못했습니다.");

            button.onClick.Invoke();
            yield return WaitForActiveScene("Main");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Main"));
            Assert.That(StepProgressSaver.HasVisited(), Is.True, "학습 시작 후 방문 기록이 저장되어야 합니다.");
        }

        [UnityTest]
        public IEnumerator Onboarding_Skip_LoadsMain()
        {
            yield return LoadScene("Onboarding");

            var button = FindComponent<Button>("BtnOnboardingSkip");
            Assert.That(button, Is.Not.Null, "BtnOnboardingSkip을 찾지 못했습니다.");

            button.onClick.Invoke();
            yield return WaitForActiveScene("Main");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Main"));
        }

        [UnityTest]
        public IEnumerator GlobalNavigation_CanMoveBetweenOnboarding_And_Main()
        {
            yield return LoadScene("Onboarding");

            var toMain = FindComponent<Button>("NavMain");
            Assert.That(toMain, Is.Not.Null, "Onboarding 씬에서 NavMain 버튼을 찾지 못했습니다.");

            toMain.onClick.Invoke();
            yield return WaitForActiveScene("Main");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Main"));

            var toOnboarding = FindComponent<Button>("NavOnboarding");
            Assert.That(toOnboarding, Is.Not.Null, "Main 씬에서 NavOnboarding 버튼을 찾지 못했습니다.");

            SceneNavigator.Load(SceneId.Onboarding);
            yield return WaitForActiveScene("Onboarding");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Onboarding"));
        }

        [UnityTest]
        public IEnumerator MainScene_DoesNotContainActiveOnboardingPlaceholder()
        {
            yield return LoadScene("Main");

            var canvas = Find("Canvas");
            var modal = Find("WelcomeModal");

            Assert.That(canvas, Is.Not.Null, "Canvas를 찾지 못했습니다.");
            Assert.That(canvas.GetComponent("OnboardingManager"), Is.Null, "Main 씬은 OnboardingManager를 포함하면 안 됩니다.");
            Assert.That(modal == null || !modal.activeInHierarchy, Is.True, "Main 씬에 활성 온보딩 placeholder가 있으면 안 됩니다.");
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            Assert.That(operation, Is.Not.Null, $"{sceneName} 씬 로드를 시작하지 못했습니다.");

            while (!operation.isDone)
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

            Assert.Fail($"활성 씬이 {sceneName} 으로 전환되지 않았습니다. 현재 씬: {SceneManager.GetActiveScene().name}");
        }

        private static GameObject Find(string name)
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
                    if (candidate != null && candidate.name == name)
                    {
                        return candidate.gameObject;
                    }
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
