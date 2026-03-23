// Folder: Tests - PlayMode test that validates all scene transitions and essential objects.
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KineTutor3D.Tests.PlayMode
{
    /// <summary>
    /// 모든 씬을 순회하며 필수 오브젝트 존재를 검증하고,
    /// SceneNavigator를 통한 전체 씬 전환 경로를 테스트합니다.
    /// </summary>
    public class FullSceneTransitionTests
    {
        /// <summary>
        /// SceneNavigator.Load()로 이동 가능한 모든 씬입니다.
        /// </summary>
        private static readonly string[] AllScenes =
        {
            "Boot",
            "Onboarding",
            "MathReadiness",
            "RobotLibrary",
            "Sandbox",
            "RobotControl",
        };

        /// <summary>
        /// 네비게이션 바로 이동 가능한 씬과 SceneId 매핑입니다.
        /// </summary>
        private static readonly (string sceneName, KineTutor3D.App.SceneId sceneId)[] NavTargets =
        {
            ("RobotLibrary", KineTutor3D.App.SceneId.RobotLibrary),
            ("MathReadiness", KineTutor3D.App.SceneId.MathReadiness),
            ("Sandbox", KineTutor3D.App.SceneId.Sandbox),
            ("RobotControl", KineTutor3D.App.SceneId.RobotControl),
        };

        [SetUp]
        public void ResetPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator AllScenes_LoadWithoutError()
        {
            var failedScenes = new List<string>();

            foreach (var sceneName in AllScenes)
            {
                var loaded = false;
                var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

                if (operation == null)
                {
                    failedScenes.Add($"{sceneName}: LoadSceneAsync returned null");
                    continue;
                }

                for (var i = 0; i < 300 && !operation.isDone; i++)
                {
                    yield return null;
                }

                if (operation.isDone)
                {
                    loaded = true;
                    yield return null;
                }

                if (!loaded)
                {
                    failedScenes.Add($"{sceneName}: 로드 타임아웃");
                }
            }

            if (failedScenes.Count > 0)
            {
                Assert.Fail("씬 로드 실패:\n" + string.Join("\n", failedScenes));
            }
        }

        [UnityTest]
        public IEnumerator AllScenes_HaveCamera()
        {
            var missing = new List<string>();

            foreach (var sceneName in AllScenes)
            {
                yield return LoadScene(sceneName);

                var camera = Object.FindFirstObjectByType<Camera>();
                if (camera == null)
                {
                    missing.Add(sceneName);
                }
            }

            if (missing.Count > 0)
            {
                Assert.Fail("카메라가 없는 씬:\n" + string.Join("\n", missing));
            }
        }

        [UnityTest]
        public IEnumerator AllScenes_WithUI_HaveEventSystem()
        {
            var missing = new List<string>();

            foreach (var sceneName in AllScenes)
            {
                if (sceneName == "Boot") continue;

                yield return LoadScene(sceneName);

                var canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas == null) continue;

                var eventSystem = Object.FindFirstObjectByType<EventSystem>();
                if (eventSystem == null)
                {
                    missing.Add(sceneName);
                }
            }

            if (missing.Count > 0)
            {
                Assert.Fail("Canvas는 있지만 EventSystem이 없는 씬:\n" + string.Join("\n", missing));
            }
        }

        [UnityTest]
        public IEnumerator AllScenes_WithCanvas_HaveGraphicRaycaster()
        {
            var missing = new List<string>();

            foreach (var sceneName in AllScenes)
            {
                if (sceneName == "Boot") continue;

                yield return LoadScene(sceneName);

                var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (var canvas in canvases)
                {
                    if (!canvas.gameObject.activeInHierarchy) continue;

                    var raycaster = canvas.GetComponent<GraphicRaycaster>();
                    if (raycaster == null)
                    {
                        missing.Add($"[{sceneName}] {GetFullPath(canvas.gameObject)}");
                    }
                }
            }

            if (missing.Count > 0)
            {
                Debug.LogWarning(
                    $"GraphicRaycaster가 없는 Canvas {missing.Count}개 (경고):\n" +
                    string.Join("\n", missing));
            }
        }

        [UnityTest]
        public IEnumerator SceneNavigator_CanReachAllNavTargets_FromRobotLibrary()
        {
            KineTutor3D.App.StepProgressSaver.MarkVisited();

            yield return LoadScene("RobotLibrary");

            foreach (var (sceneName, sceneId) in NavTargets)
            {
                if (sceneName == "RobotLibrary") continue;

                KineTutor3D.App.SceneNavigator.Load(sceneId);
                yield return WaitForActiveScene(sceneName);

                Assert.That(
                    SceneManager.GetActiveScene().name,
                    Is.EqualTo(sceneName),
                    $"RobotLibrary → {sceneName} 전환 실패");

                // RobotLibrary로 돌아가기
                KineTutor3D.App.SceneNavigator.Load(KineTutor3D.App.SceneId.RobotLibrary);
                yield return WaitForActiveScene("RobotLibrary");
            }
        }

        [UnityTest]
        public IEnumerator SceneNavigator_RoundTrip_AllPairs()
        {
            var failedPairs = new List<string>();

            for (var i = 0; i < NavTargets.Length; i++)
            {
                for (var j = 0; j < NavTargets.Length; j++)
                {
                    if (i == j) continue;

                    var (fromName, _) = NavTargets[i];
                    var (toName, toId) = NavTargets[j];

                    yield return LoadScene(fromName);

                    KineTutor3D.App.SceneNavigator.Load(toId);

                    var arrived = false;
                    for (var k = 0; k < 120; k++)
                    {
                        if (SceneManager.GetActiveScene().name == toName)
                        {
                            arrived = true;
                            break;
                        }

                        yield return null;
                    }

                    if (!arrived)
                    {
                        failedPairs.Add($"{fromName} → {toName}");
                    }

                    yield return null;
                }
            }

            if (failedPairs.Count > 0)
            {
                Assert.Fail(
                    $"씬 전환 실패 {failedPairs.Count}건:\n" +
                    string.Join("\n", failedPairs));
            }
        }

        [UnityTest]
        public IEnumerator Boot_FirstVisit_ToOnboarding_ToRobotLibrary_ToSandbox_FullFlow()
        {
            yield return LoadScene("Boot");
            yield return WaitForActiveScene("Onboarding");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Onboarding"));

            var startBtn = FindComponent<Button>("BtnStartLearning");
            Assert.That(startBtn, Is.Not.Null, "BtnStartLearning을 찾지 못했습니다.");

            startBtn.onClick.Invoke();
            yield return WaitForActiveScene("RobotLibrary");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RobotLibrary"));

            KineTutor3D.App.SceneNavigator.Load(KineTutor3D.App.SceneId.Sandbox);
            yield return WaitForActiveScene("Sandbox");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Sandbox"));
        }

        [UnityTest]
        public IEnumerator AllNavigableScenes_HaveNoMissingScripts()
        {
            var issues = new List<string>();

            foreach (var sceneName in AllScenes)
            {
                if (sceneName == "Boot") continue;

                yield return LoadScene(sceneName);

                var roots = SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var root in roots)
                {
                    if (root == null) continue;
                    var transforms = root.GetComponentsInChildren<Transform>(true);
                    foreach (var t in transforms)
                    {
                        if (t == null) continue;
                        var components = t.GetComponents<Component>();
                        for (var ci = 0; ci < components.Length; ci++)
                        {
                            if (components[ci] == null)
                            {
                                issues.Add($"[{sceneName}] {GetFullPath(t.gameObject)} — component[{ci}] missing");
                            }
                        }
                    }
                }
            }

            if (issues.Count > 0)
            {
                Assert.Fail(
                    $"Missing script 발견 {issues.Count}건:\n" +
                    string.Join("\n", issues));
            }
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

            Assert.Fail(
                $"활성 씬이 {sceneName} 으로 전환되지 않았습니다. " +
                $"현재 씬: {SceneManager.GetActiveScene().name}");
        }

        private static GameObject Find(string name)
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                if (root == null) continue;
                var transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var t in transforms)
                {
                    if (t != null && t.name == name) return t.gameObject;
                }
            }

            return null;
        }

        private static T FindComponent<T>(string name) where T : Component
        {
            var go = Find(name);
            return go != null ? go.GetComponent<T>() : null;
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
    }
}
