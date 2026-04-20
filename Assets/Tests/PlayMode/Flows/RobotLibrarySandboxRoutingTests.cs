// Folder: Tests/PlayMode - PlayMode smoke and scene flow tests.
// RobotLibrarySandboxRouting 흐름을 검증하는 PlayMode 테스트입니다.
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
    /// Robot Library에서 Sandbox 지원 로봇이 각자의 Sandbox 화면으로 연결되는지 검증합니다.
    /// </summary>
    public class RobotLibrarySandboxRoutingTests
    {
        private static readonly SandboxExpectation[] SandboxCases =
        {
            new("2DOF_RR", "2DOF RR", 2),
            new("SCARA_RV", "SCARA Robot", 4),
            new("FAIRINO_FR5", "FAIRINO FR5", 6),
            new("UR5e", "Universal Robots UR5e", 6),
            new("DOOSAN_M1013", "Doosan M1013", 6),
            new("MECA500", "Mecademic Meca500", 6)
        };

        [SetUp]
        public void ResetPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            StepProgressSaver.MarkVisited();
        }

        [UnityTest]
        public IEnumerator RobotLibrary_SandboxCta_LoadsMatchingRobotSandbox_ForEverySandboxSupportedRobot()
        {
            foreach (var sandboxCase in SandboxCases)
            {
                yield return LoadScene("RobotLibrary");

                var manager = FindComponent<RobotLibraryManager>();
                Assert.That(manager, Is.Not.Null, "RobotLibraryManager를 찾지 못했습니다.");

                var entry = GetRobotCatalogEntry(manager, sandboxCase.RobotId);
                InvokePrivate(manager, "OnOpenSandbox", entry);
                yield return WaitForActiveScene("Sandbox");
                yield return null;
                yield return null;

                Assert.That(
                    RobotSelectionBridge.GetSelectedRobotId(),
                    Is.EqualTo(sandboxCase.RobotId),
                    $"{sandboxCase.RobotId} 선택값이 Sandbox 진입 후 유지되어야 합니다.");
                Assert.That(
                    RobotSelectionBridge.GetSelectedMode(),
                    Is.EqualTo(RobotSelectionBridge.SandboxMode),
                    $"{sandboxCase.RobotId} 진입 모드는 sandbox 여야 합니다.");

                var robotNameLabel = FindByName<Text>("RobotNameLabel");
                Assert.That(robotNameLabel, Is.Not.Null, "Sandbox RobotNameLabel을 찾지 못했습니다.");
                StringAssert.Contains(sandboxCase.DisplayName, robotNameLabel.text);
                StringAssert.Contains($"{sandboxCase.Dof}DOF", robotNameLabel.text);

                var jointSliders = FindAllByPrefix<Slider>("Slider_J");
                Assert.That(
                    jointSliders.Count,
                    Is.EqualTo(sandboxCase.Dof),
                    $"{sandboxCase.RobotId} Sandbox 슬라이더 수가 DOF와 일치해야 합니다.");
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
            for (var i = 0; i < 180; i++)
            {
                if (SceneManager.GetActiveScene().name == sceneName)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"활성 씬이 {sceneName} 으로 전환되지 않았습니다. 현재 씬: {SceneManager.GetActiveScene().name}");
        }

        private static void InvokePrivate(Component component, string methodName, params object[] args)
        {
            var method = component.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"{methodName} 메서드를 찾지 못했습니다.");
            method.Invoke(component, args);
        }

        private static object GetRobotCatalogEntry(Component component, string robotId)
        {
            var robotCatalogType = ResolveType("KineTutor3D.Templates.RobotCatalog");
            Assert.That(robotCatalogType, Is.Not.Null, "RobotCatalog 타입을 찾지 못했습니다.");

            var tryGet = robotCatalogType.GetMethod("TryGet", BindingFlags.Public | BindingFlags.Static);
            Assert.That(tryGet, Is.Not.Null, "RobotCatalog.TryGet 메서드를 찾지 못했습니다.");

            var args = new object[] { robotId, null };
            var found = (bool)tryGet.Invoke(null, args);
            Assert.That(found, Is.True, $"{robotId} 카탈로그 엔트리를 찾지 못했습니다.");
            Assert.That(args[1], Is.Not.Null, $"{robotId} 카탈로그 엔트리가 null 입니다.");
            return args[1];
        }

        private static System.Type ResolveType(string fullName)
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var type = assemblies[i].GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static T FindComponent<T>() where T : Component
        {
            return Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
        }

        private static T FindByName<T>(string name) where T : Component
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var components = roots[i].GetComponentsInChildren<T>(true);
                for (var j = 0; j < components.Length; j++)
                {
                    var component = components[j];
                    if (component != null && component.name == name)
                    {
                        return component;
                    }
                }
            }

            return null;
        }

        private static List<T> FindAllByPrefix<T>(string namePrefix) where T : Component
        {
            var results = new List<T>();
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var components = roots[i].GetComponentsInChildren<T>(true);
                for (var j = 0; j < components.Length; j++)
                {
                    var component = components[j];
                    if (component != null && component.name.StartsWith(namePrefix))
                    {
                        results.Add(component);
                    }
                }
            }

            return results;
        }

        private sealed class SandboxExpectation
        {
            public SandboxExpectation(string robotId, string displayName, int dof)
            {
                RobotId = robotId;
                DisplayName = displayName;
                Dof = dof;
            }

            public string RobotId { get; }
            public string DisplayName { get; }
            public int Dof { get; }
        }
    }
}

namespace KineTutor3D.Tests.PlayMode
{
    /// <summary>
    /// unityctl exec로 Robot Library → Sandbox 라우팅을 직접 확인하기 위한 경량 probe입니다.
    /// </summary>
    public static class RobotLibrarySandboxRouteProbe
    {
        public static string InvokeOpenSandbox(string robotId)
        {
            var manager = Object.FindFirstObjectByType<RobotLibraryManager>(FindObjectsInactive.Include);
            if (manager == null)
            {
                return "manager:null";
            }

            var robotCatalogType = ResolveType("KineTutor3D.Templates.RobotCatalog");
            if (robotCatalogType == null)
            {
                return "catalog:null";
            }

            var tryGet = robotCatalogType.GetMethod("TryGet", BindingFlags.Public | BindingFlags.Static);
            if (tryGet == null)
            {
                return "tryget:null";
            }

            var args = new object[] { robotId, null };
            var found = (bool)tryGet.Invoke(null, args);
            if (!found || args[1] == null)
            {
                return $"entry:not-found:{robotId}";
            }

            var onOpenSandbox = manager.GetType().GetMethod("OnOpenSandbox", BindingFlags.Instance | BindingFlags.NonPublic);
            if (onOpenSandbox == null)
            {
                return "method:null";
            }

            onOpenSandbox.Invoke(manager, new[] { args[1] });
            return $"invoked:{robotId}";
        }

        public static string InvokeOpenRobotControl(string robotId)
        {
            var manager = Object.FindFirstObjectByType<RobotLibraryManager>(FindObjectsInactive.Include);
            if (manager == null)
            {
                return "manager:null";
            }

            var robotCatalogType = ResolveType("KineTutor3D.Templates.RobotCatalog");
            if (robotCatalogType == null)
            {
                return "catalog:null";
            }

            var tryGet = robotCatalogType.GetMethod("TryGet", BindingFlags.Public | BindingFlags.Static);
            if (tryGet == null)
            {
                return "tryget:null";
            }

            var args = new object[] { robotId, null };
            var found = (bool)tryGet.Invoke(null, args);
            if (!found || args[1] == null)
            {
                return $"entry:not-found:{robotId}";
            }

            var onOpenRobotControl = manager.GetType().GetMethod("OnOpenRobotControl", BindingFlags.Instance | BindingFlags.NonPublic);
            if (onOpenRobotControl == null)
            {
                return "method:null";
            }

            onOpenRobotControl.Invoke(manager, new[] { args[1] });
            return $"invoked:{robotId}";
        }

        public static string CaptureCurrentState()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            var robotId = RobotSelectionBridge.GetSelectedRobotId();
            var mode = RobotSelectionBridge.GetSelectedMode();
            var robotNameLabel = FindByName<Text>("RobotNameLabel");
            var sliderCount = CountByPrefix<Slider>("Slider_J");
            var labelText = robotNameLabel != null ? robotNameLabel.text : "null";
            return $"scene={sceneName};robot={robotId};mode={mode};label={labelText};sliders={sliderCount}";
        }

        private static T FindByName<T>(string name) where T : Component
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var components = roots[i].GetComponentsInChildren<T>(true);
                for (var j = 0; j < components.Length; j++)
                {
                    var component = components[j];
                    if (component != null && component.name == name)
                    {
                        return component;
                    }
                }
            }

            return null;
        }

        private static int CountByPrefix<T>(string namePrefix) where T : Component
        {
            var count = 0;
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var components = roots[i].GetComponentsInChildren<T>(true);
                for (var j = 0; j < components.Length; j++)
                {
                    var component = components[j];
                    if (component != null && component.name.StartsWith(namePrefix))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static System.Type ResolveType(string fullName)
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var type = assemblies[i].GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
