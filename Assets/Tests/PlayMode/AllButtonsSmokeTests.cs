// Folder: Tests - PlayMode smoke test that invokes every Button in every navigable scene.
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KineTutor3D.Tests.PlayMode
{
    /// <summary>
    /// 모든 네비게이션 가능한 씬을 순회하며, 각 씬에 존재하는 Button 컴포넌트를
    /// 자동으로 수집하고 onClick.Invoke()를 호출하여 예외가 발생하지 않는지 검증합니다.
    /// </summary>
    public class AllButtonsSmokeTests
    {
        /// <summary>
        /// 네비게이션 가능한 씬 목록입니다. SceneCatalog.GetNavigableEntries()와 동기화합니다.
        /// </summary>
        private static readonly string[] NavigableScenes =
        {
            "MathReadiness",
            "RobotLibrary",
            "Sandbox",
            "RobotControl",
        };

        /// <summary>
        /// 클릭 시 씬 전환을 유발하므로 스모크 테스트에서 제외해야 하는 버튼 이름입니다.
        /// </summary>
        private static readonly HashSet<string> ExcludedButtons = new()
        {
            // 네비게이션 바 버튼 — 클릭 시 씬이 전환되어 테스트 컨텍스트가 사라집니다
            "NavHome",
            "NavMain",
            "NavMathReadiness",
            "NavRobotLibrary",
            "NavSandbox",
            "NavRobotControl",
            // 온보딩/부트 전환 버튼
            "BtnStartLearning",
            "BtnBeginner",
            "BtnOnboardingSkip",
            // Home 씬 전환 버튼
            "BtnContinueLatestContext",
            "BtnStartMathReadiness",
            // Robot Library → 다른 씬 CTA
            "BtnRobotControl",
            "BtnOpenSandbox",
        };

        [UnityTest]
        public IEnumerator AllNavigableScenes_AllButtons_InvokeWithoutException()
        {
            var totalButtons = 0;
            var testedButtons = 0;
            var skippedButtons = 0;
            var failedButtons = new List<string>();

            foreach (var sceneName in NavigableScenes)
            {
                yield return LoadScene(sceneName);

                var buttons = CollectAllButtons();
                totalButtons += buttons.Count;

                foreach (var button in buttons)
                {
                    if (button == null) continue;

                    var buttonName = GetFullPath(button.gameObject);

                    if (ExcludedButtons.Contains(button.gameObject.name))
                    {
                        skippedButtons++;
                        continue;
                    }

                    try
                    {
                        button.onClick.Invoke();
                        testedButtons++;
                    }
                    catch (System.Exception ex)
                    {
                        failedButtons.Add($"[{sceneName}] {buttonName}: {ex.Message}");
                        testedButtons++;
                    }

                    yield return null;
                }
            }

            Debug.Log(
                $"[AllButtonsSmokeTest] 완료 — 전체: {totalButtons}, " +
                $"테스트: {testedButtons}, 제외: {skippedButtons}, " +
                $"실패: {failedButtons.Count}");

            if (failedButtons.Count > 0)
            {
                Assert.Fail(
                    $"버튼 클릭 실패 {failedButtons.Count}건:\n" +
                    string.Join("\n", failedButtons));
            }
        }

        [UnityTest]
        public IEnumerator AllNavigableScenes_AllButtons_AreInteractable_OrExplicitlyDisabled()
        {
            var nonInteractable = new List<string>();

            foreach (var sceneName in NavigableScenes)
            {
                yield return LoadScene(sceneName);

                var buttons = CollectAllButtons();

                foreach (var button in buttons)
                {
                    if (button == null) continue;
                    if (!button.gameObject.activeInHierarchy) continue;

                    if (!button.interactable)
                    {
                        nonInteractable.Add($"[{sceneName}] {GetFullPath(button.gameObject)}");
                    }
                }
            }

            Debug.Log(
                $"[AllButtonsSmokeTest] 비활성 버튼 {nonInteractable.Count}개 발견 " +
                $"(경고만, 실패 아님):\n" +
                string.Join("\n", nonInteractable));
        }

        [UnityTest]
        public IEnumerator OnboardingScene_AllButtons_InvokeWithoutException_ExceptNavigation()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            yield return LoadScene("Onboarding");

            var buttons = CollectAllButtons();
            var failedButtons = new List<string>();

            foreach (var button in buttons)
            {
                if (button == null) continue;
                if (ExcludedButtons.Contains(button.gameObject.name)) continue;

                try
                {
                    button.onClick.Invoke();
                }
                catch (System.Exception ex)
                {
                    failedButtons.Add($"[Onboarding] {GetFullPath(button.gameObject)}: {ex.Message}");
                }

                yield return null;
            }

            if (failedButtons.Count > 0)
            {
                Assert.Fail(
                    $"Onboarding 버튼 클릭 실패 {failedButtons.Count}건:\n" +
                    string.Join("\n", failedButtons));
            }
        }

        private static List<Button> CollectAllButtons()
        {
            var result = new List<Button>();
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                if (root == null) continue;
                var buttons = root.GetComponentsInChildren<Button>(true);
                result.AddRange(buttons);
            }

            return result;
        }

        private static string GetFullPath(GameObject go)
        {
            var path = go.name;
            var parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
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
            yield return null;
        }
    }
}
