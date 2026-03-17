// Folder: Editor/CliTools - unity-cli 커스텀 도구: 씬 계층 구조 조회
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 현재 씬의 GameObject 계층 구조를 JSON 트리로 반환합니다.
    /// </summary>
    [UnityCliTool(Description = "Dump scene GameObject hierarchy as a JSON tree")]
    public static class SceneHierarchyTool
    {
        public class Parameters
        {
            [ToolParameter("Root path to start from (optional, e.g. 'Canvas/TopBar')", Required = false)]
            public string Path { get; set; }

            [ToolParameter("Maximum depth to traverse (default 3)", Required = false)]
            public int Depth { get; set; }
        }

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            string path = p.Get("path", null);
            int maxDepth = p.GetInt("depth", 3) ?? 3;

            if (!string.IsNullOrEmpty(path))
            {
                var target = GameObject.Find(path);
                if (target == null)
                    return new ErrorResponse($"GameObject not found: {path}");

                var node = BuildNode(target, 0, maxDepth);
                return new SuccessResponse($"Hierarchy for '{path}'.", node);
            }

            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            var nodes = new List<object>();

            foreach (var root in roots)
            {
                nodes.Add(BuildNode(root, 0, maxDepth));
            }

            return new SuccessResponse(
                $"Scene '{scene.name}': {roots.Length} root objects.",
                new { scene = scene.name, root_count = roots.Length, hierarchy = nodes });
        }

        private static object BuildNode(GameObject go, int currentDepth, int maxDepth)
        {
            var components = new List<string>();
            foreach (var comp in go.GetComponents<Component>())
            {
                if (comp != null)
                    components.Add(comp.GetType().Name);
            }

            var children = new List<object>();
            if (currentDepth < maxDepth)
            {
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    children.Add(BuildNode(go.transform.GetChild(i).gameObject, currentDepth + 1, maxDepth));
                }
            }
            else if (go.transform.childCount > 0)
            {
                children = null;
            }

            return new
            {
                name = go.name,
                active = go.activeSelf,
                components,
                child_count = go.transform.childCount,
                children
            };
        }
    }
}
