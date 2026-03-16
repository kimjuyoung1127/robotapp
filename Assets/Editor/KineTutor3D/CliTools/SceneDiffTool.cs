// Folder: Editor/CliTools - unity-cli 커스텀 도구: 씬 간 GameObject 트리 비교
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 두 씬의 루트 GameObject 목록을 비교하여 차이를 보고합니다.
    /// </summary>
    [UnityCliTool(Description = "Compare root GameObjects between two scenes")]
    public static class SceneDiffTool
    {
        public class Parameters
        {
            [ToolParameter("First scene name (e.g. Boot)")]
            public string SceneA { get; set; }

            [ToolParameter("Second scene name (e.g. Home)")]
            public string SceneB { get; set; }
        }

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            string sceneA = p.Get("scene_a", "");
            string sceneB = p.Get("scene_b", "");

            if (string.IsNullOrEmpty(sceneA) || string.IsNullOrEmpty(sceneB))
                return new ErrorResponse("Both scene_a and scene_b parameters are required.");

            string currentScene = SceneManager.GetActiveScene().path;

            try
            {
                var namesA = GetRootNames(sceneA);
                if (namesA == null)
                    return new ErrorResponse($"Scene not found: {sceneA}");

                var namesB = GetRootNames(sceneB);
                if (namesB == null)
                    return new ErrorResponse($"Scene not found: {sceneB}");

                var setA = new HashSet<string>(namesA);
                var setB = new HashSet<string>(namesB);

                var common = new List<string>();
                var onlyInA = new List<string>();
                var onlyInB = new List<string>();

                foreach (string name in namesA)
                {
                    if (setB.Contains(name))
                        common.Add(name);
                    else
                        onlyInA.Add(name);
                }

                foreach (string name in namesB)
                {
                    if (!setA.Contains(name))
                        onlyInB.Add(name);
                }

                return new SuccessResponse(
                    $"Scene diff: {common.Count} common, {onlyInA.Count} only in {sceneA}, {onlyInB.Count} only in {sceneB}.",
                    new
                    {
                        scene_a = sceneA,
                        scene_b = sceneB,
                        root_count_a = namesA.Count,
                        root_count_b = namesB.Count,
                        common_count = common.Count,
                        only_in_a = onlyInA.Count > 0 ? onlyInA : null,
                        only_in_b = onlyInB.Count > 0 ? onlyInB : null,
                        common
                    });
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"Scene diff failed: {ex.Message}");
            }
            finally
            {
                if (!string.IsNullOrEmpty(currentScene))
                {
                    try { EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single); }
                    catch { /* 복원 실패 무시 */ }
                }
            }
        }

        private static List<string> GetRootNames(string sceneName)
        {
            string path = $"Assets/Scenes/{sceneName}.unity";
            if (!System.IO.File.Exists(
                System.IO.Path.Combine(Application.dataPath, "..", path)))
                return null;

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var names = new List<string>();
            foreach (var go in scene.GetRootGameObjects())
                names.Add(go.name);
            return names;
        }
    }
}
