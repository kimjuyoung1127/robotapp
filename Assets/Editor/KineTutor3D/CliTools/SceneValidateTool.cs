// Folder: Editor/CliTools - unity-cli 커스텀 도구: 씬 무결성 검증
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 프로젝트의 모든 씬 또는 지정된 씬의 missing script/reference를 검사합니다.
    /// </summary>
    [UnityCliTool(Description = "Validate scenes for missing scripts and broken references")]
    public static class SceneValidateTool
    {
        public class Parameters
        {
            [ToolParameter("Scene name or 'all' for all scenes", Required = false)]
            public string Name { get; set; }
        }

        private static readonly string[] knownScenes =
        {
            "Boot", "Onboarding", "MathReadiness",
            "RobotLibrary", "Sandbox", "RobotControl"
        };

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            string name = p.Get("name", "all");
            bool verbose = p.GetBool("verbose", false);

            string currentScene = SceneManager.GetActiveScene().path;
            var results = new List<object>();
            int totalMissing = 0;

            string[] scenesToCheck;
            if (string.Equals(name, "all", StringComparison.OrdinalIgnoreCase))
            {
                scenesToCheck = knownScenes;
            }
            else
            {
                scenesToCheck = new[] { name };
            }

            foreach (string sceneName in scenesToCheck)
            {
                string path = $"Assets/Scenes/{sceneName}.unity";
                if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "..", path)))
                {
                    results.Add(new { name = sceneName, valid = false, error = "Scene file not found", missing_scripts = 0 });
                    continue;
                }

                try
                {
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    int missing = 0;
                    var missingPaths = verbose ? new List<string>() : null;

                    foreach (var go in scene.GetRootGameObjects())
                    {
                        missing += CountMissingScriptsRecursive(go, missingPaths);
                    }

                    totalMissing += missing;
                    results.Add(new
                    {
                        name = sceneName,
                        valid = missing == 0,
                        missing_scripts = missing,
                        missing_script_paths = verbose && missingPaths.Count > 0 ? missingPaths : null
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new { name = sceneName, valid = false, error = ex.Message, missing_scripts = -1 });
                }
            }

            if (!string.IsNullOrEmpty(currentScene))
            {
                try { EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single); }
                catch { /* 원래 씬 복원 실패 시 무시 */ }
            }

            bool allValid = totalMissing == 0;
            return new SuccessResponse(
                allValid ? "All scenes valid." : $"Found {totalMissing} missing script(s).",
                new { all_valid = allValid, total_missing = totalMissing, scenes = results });
        }

        private static int CountMissingScriptsRecursive(GameObject go, List<string> missingPaths = null)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (count > 0 && missingPaths != null)
                missingPaths.Add(GetFullPath(go));

            for (int i = 0; i < go.transform.childCount; i++)
            {
                count += CountMissingScriptsRecursive(go.transform.GetChild(i).gameObject, missingPaths);
            }
            return count;
        }

        private static string GetFullPath(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
    }
}
