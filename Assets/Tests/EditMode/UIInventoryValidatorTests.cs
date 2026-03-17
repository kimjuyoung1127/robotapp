// Folder: Tests - EditMode validator that scans all scene assets for UI component inventory and missing references.
#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// 모든 씬 에셋을 EditMode에서 열어 UI 컴포넌트(Button, Slider, Toggle, InputField)를
    /// 전수 조사하고 missing reference, missing script, EventSystem 부재 등을 검증합니다.
    /// </summary>
    [TestFixture]
    public class UIInventoryValidatorTests
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/Onboarding.unity",
            "Assets/Scenes/Home.unity",
            "Assets/Scenes/Main.unity",
            "Assets/Scenes/MathReadiness.unity",
            "Assets/Scenes/RobotLibrary.unity",
            "Assets/Scenes/Sandbox.unity",
            "Assets/Scenes/RobotControl.unity",
        };

        [Test]
        public void AllScenes_NoMissingScripts()
        {
            var issues = new List<string>();

            foreach (var scenePath in ScenePaths)
            {
                if (!System.IO.File.Exists(scenePath)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();

                foreach (var root in roots)
                {
                    var transforms = root.GetComponentsInChildren<Transform>(true);
                    foreach (var t in transforms)
                    {
                        if (t == null) continue;
                        var components = t.GetComponents<Component>();
                        for (var i = 0; i < components.Length; i++)
                        {
                            if (components[i] == null)
                            {
                                issues.Add($"[{scene.name}] {GetFullPath(t.gameObject)} — component[{i}] missing script");
                            }
                        }
                    }
                }
            }

            if (issues.Count > 0)
            {
                Assert.Fail($"Missing script 발견 {issues.Count}건:\n" + string.Join("\n", issues));
            }
        }

        [Test]
        public void AllScenes_ButtonsHaveOnClickListenerOrName()
        {
            var orphanButtons = new List<string>();

            foreach (var scenePath in ScenePaths)
            {
                if (!System.IO.File.Exists(scenePath)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();

                foreach (var root in roots)
                {
                    var buttons = root.GetComponentsInChildren<Button>(true);
                    foreach (var button in buttons)
                    {
                        if (button == null) continue;

                        var hasSerializedListeners = button.onClick.GetPersistentEventCount() > 0;
                        var hasRecognizedName = button.gameObject.name.StartsWith("Btn");

                        if (!hasSerializedListeners && !hasRecognizedName)
                        {
                            orphanButtons.Add(
                                $"[{scene.name}] {GetFullPath(button.gameObject)} — " +
                                $"onClick 리스너 0개, Btn 접두사 없음");
                        }
                    }
                }
            }

            if (orphanButtons.Count > 0)
            {
                Debug.LogWarning(
                    $"리스너/이름 미확인 버튼 {orphanButtons.Count}개 (경고):\n" +
                    string.Join("\n", orphanButtons));
            }
        }

        [Test]
        public void AllScenes_WithCanvas_HaveEventSystem()
        {
            var missing = new List<string>();

            foreach (var scenePath in ScenePaths)
            {
                if (!System.IO.File.Exists(scenePath)) continue;
                if (scenePath.Contains("Boot")) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();

                var hasCanvas = false;
                var hasEventSystem = false;

                foreach (var root in roots)
                {
                    if (root.GetComponentInChildren<Canvas>(true) != null) hasCanvas = true;
                    if (root.GetComponentInChildren<EventSystem>(true) != null) hasEventSystem = true;
                }

                if (hasCanvas && !hasEventSystem)
                {
                    missing.Add(scene.name);
                }
            }

            if (missing.Count > 0)
            {
                Assert.Fail("Canvas는 있지만 EventSystem이 없는 씬:\n" + string.Join("\n", missing));
            }
        }

        [Test]
        public void AllScenes_UIComponentInventory_LogsSummary()
        {
            var summary = new List<string>();
            var totalButtons = 0;
            var totalSliders = 0;
            var totalToggles = 0;
            var totalInputFields = 0;

            foreach (var scenePath in ScenePaths)
            {
                if (!System.IO.File.Exists(scenePath)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();

                var buttons = 0;
                var sliders = 0;
                var toggles = 0;
                var inputFields = 0;

                foreach (var root in roots)
                {
                    buttons += root.GetComponentsInChildren<Button>(true).Length;
                    sliders += root.GetComponentsInChildren<Slider>(true).Length;
                    toggles += root.GetComponentsInChildren<Toggle>(true).Length;
                    inputFields += root.GetComponentsInChildren<InputField>(true).Length;
                }

                totalButtons += buttons;
                totalSliders += sliders;
                totalToggles += toggles;
                totalInputFields += inputFields;

                summary.Add(
                    $"| {scene.name,-16} | {buttons,7} | {sliders,7} | {toggles,7} | {inputFields,11} |");
            }

            var header =
                "| Scene            | Buttons | Sliders | Toggles | InputFields |\n" +
                "|------------------|---------|---------|---------|-------------|";
            var footer =
                $"| **TOTAL**        | **{totalButtons}** | **{totalSliders}** | **{totalToggles}** | **{totalInputFields}** |";

            var report = $"\n[UIInventoryValidator] UI 컴포넌트 전수조사 결과:\n{header}\n" +
                         string.Join("\n", summary) + $"\n{footer}";

            Debug.Log(report);

            Assert.Pass(report);
        }

        [Test]
        public void AllScenes_SlidersHaveValidRange()
        {
            var issues = new List<string>();

            foreach (var scenePath in ScenePaths)
            {
                if (!System.IO.File.Exists(scenePath)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();

                foreach (var root in roots)
                {
                    var sliders = root.GetComponentsInChildren<Slider>(true);
                    foreach (var slider in sliders)
                    {
                        if (slider == null) continue;

                        if (slider.minValue >= slider.maxValue)
                        {
                            issues.Add(
                                $"[{scene.name}] {GetFullPath(slider.gameObject)} — " +
                                $"min({slider.minValue}) >= max({slider.maxValue})");
                        }
                    }
                }
            }

            if (issues.Count > 0)
            {
                Assert.Fail($"잘못된 Slider 범위 {issues.Count}건:\n" + string.Join("\n", issues));
            }
        }

        [Test]
        public void AllScenes_ActiveButtonsHaveRaycastTarget()
        {
            var issues = new List<string>();

            foreach (var scenePath in ScenePaths)
            {
                if (!System.IO.File.Exists(scenePath)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();

                foreach (var root in roots)
                {
                    var buttons = root.GetComponentsInChildren<Button>(true);
                    foreach (var button in buttons)
                    {
                        if (button == null) continue;
                        if (!button.gameObject.activeInHierarchy) continue;

                        var graphic = button.GetComponent<Graphic>();
                        if (graphic != null && !graphic.raycastTarget)
                        {
                            issues.Add(
                                $"[{scene.name}] {GetFullPath(button.gameObject)} — " +
                                $"raycastTarget=false (버튼 클릭 불가)");
                        }
                    }
                }
            }

            if (issues.Count > 0)
            {
                Assert.Fail(
                    $"raycastTarget이 꺼진 활성 버튼 {issues.Count}건:\n" +
                    string.Join("\n", issues));
            }
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
#endif
