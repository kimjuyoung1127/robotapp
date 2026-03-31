// Folder: Tests/PlayMode - PlayMode smoke and scene flow tests.
// SceneFlowSmoke 흐름을 검증하는 PlayMode 테스트입니다.
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
        public IEnumerator Boot_Visited_RoutesToRobotLibrary()
        {
            StepProgressSaver.MarkVisited();

            yield return LoadScene("Boot");
            yield return WaitForActiveScene("RobotLibrary");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RobotLibrary"));
        }

        [UnityTest]
        public IEnumerator Onboarding_StartLearning_LoadsRobotLibrary_AndMarksVisited()
        {
            yield return LoadScene("Onboarding");

            var button = FindComponent<Button>("BtnStartLearning");
            Assert.That(button, Is.Not.Null, "BtnStartLearning을 찾지 못했습니다.");

            button.onClick.Invoke();
            yield return WaitForActiveScene("RobotLibrary");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RobotLibrary"));
            Assert.That(StepProgressSaver.HasVisited(), Is.True, "학습 시작 후 방문 기록이 저장되어야 합니다.");
        }

        [UnityTest]
        public IEnumerator Onboarding_Skip_LoadsRobotLibrary()
        {
            yield return LoadScene("Onboarding");

            var button = FindComponent<Button>("BtnOnboardingSkip");
            Assert.That(button, Is.Not.Null, "BtnOnboardingSkip을 찾지 못했습니다.");

            button.onClick.Invoke();
            yield return WaitForActiveScene("RobotLibrary");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RobotLibrary"));
        }

        [UnityTest]
        public IEnumerator Onboarding_DoesNotShowGlobalSceneNavigation()
        {
            yield return LoadScene("Onboarding");

            var toHome = FindComponent<Button>("NavHome");
            Assert.That(toHome == null || !toHome.gameObject.activeInHierarchy, Is.True, "Onboarding 씬에는 전역 SceneNavigationBar가 표시되지 않아야 합니다.");
        }

        [UnityTest]
        public IEnumerator Onboarding_Cards_AreChildrenOfModalSurface()
        {
            yield return LoadScene("Onboarding");

            var modalSurface = Find("ModalSurface");
            var cardRow = Find("CardRow");
            var start = Find("BtnStartLearning");
            var beginner = Find("BtnBeginner");
            var skip = Find("BtnOnboardingSkip");

            Assert.That(modalSurface, Is.Not.Null, "ModalSurface를 찾지 못했습니다.");
            Assert.That(cardRow, Is.Not.Null, "CardRow를 찾지 못했습니다.");
            Assert.That(start, Is.Not.Null, "BtnStartLearning을 찾지 못했습니다.");
            Assert.That(beginner, Is.Not.Null, "BtnBeginner를 찾지 못했습니다.");
            Assert.That(skip, Is.Not.Null, "BtnOnboardingSkip을 찾지 못했습니다.");
            Assert.That(start.transform.parent, Is.EqualTo(cardRow.transform), "학습 시작 버튼은 CardRow 안에 있어야 합니다.");
            Assert.That(beginner.transform.parent, Is.EqualTo(cardRow.transform), "초보자 버튼은 CardRow 안에 있어야 합니다.");
            Assert.That(skip.transform.parent, Is.EqualTo(modalSurface.transform), "건너뛰기 버튼은 ModalSurface 안에 있어야 합니다.");
            Assert.That(cardRow.transform.parent, Is.EqualTo(modalSurface.transform), "CardRow는 ModalSurface 안에 있어야 합니다.");

            var startRect = start.GetComponent<RectTransform>();
            var beginnerRect = beginner.GetComponent<RectTransform>();
            Assert.That(startRect, Is.Not.Null);
            Assert.That(beginnerRect, Is.Not.Null);
            Assert.That(startRect.sizeDelta, Is.EqualTo(beginnerRect.sizeDelta), "두 선택 카드는 같은 크기여야 합니다.");
        }

        [UnityTest]
        public IEnumerator RobotLibraryScene_ContainsCamera()
        {
            StepProgressSaver.MarkVisited();
            yield return LoadScene("RobotLibrary");

            Assert.That(Object.FindFirstObjectByType<Camera>(), Is.Not.Null, "RobotLibrary 씬은 최소 1개의 카메라를 가져야 합니다.");
        }

        [UnityTest]
        public IEnumerator SceneNavigator_CanLoadSandboxScene()
        {
            yield return LoadScene("RobotLibrary");

            SceneNavigator.Load(SceneId.Sandbox);
            yield return WaitForActiveScene("Sandbox");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Sandbox"));
        }

        [UnityTest]
        public IEnumerator SandboxScene_DoesNotContainActiveOnboardingPlaceholder()
        {
            yield return LoadScene("Sandbox");

            var canvas = Find("Canvas");
            var modal = Find("WelcomeModal");

            Assert.That(canvas, Is.Not.Null, "Canvas를 찾지 못했습니다.");
            Assert.That(canvas.GetComponent("OnboardingManager"), Is.Null, "Sandbox 씬은 OnboardingManager를 포함하면 안 됩니다.");
            Assert.That(modal == null || !modal.activeInHierarchy, Is.True, "Sandbox 씬에 활성 온보딩 placeholder가 있으면 안 됩니다.");
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
