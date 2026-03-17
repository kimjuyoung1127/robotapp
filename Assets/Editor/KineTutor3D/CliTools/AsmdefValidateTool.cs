// Folder: Editor/CliTools - unity-cli 커스텀 도구: Assembly Definition 검증
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 프로젝트의 Assembly Definition 파일들의 참조 무결성을 검사합니다.
    /// </summary>
    [UnityCliTool(Description = "Validate assembly definition references: missing refs, circular deps")]
    public static class AsmdefValidateTool
    {
        public static object HandleCommand(JObject @params)
        {
            string[] guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");
            var asmdefInfos = new List<object>();
            var allNames = new HashSet<string>(StringComparer.Ordinal);
            var refGraph = new Dictionary<string, List<string>>();
            var missingRefs = new List<string>();

            // 1단계: 모든 asmdef 이름 수집
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string json = System.IO.File.ReadAllText(path);
                JObject asmdef;
                try { asmdef = JObject.Parse(json); }
                catch { continue; }

                string name = asmdef["name"]?.ToString();
                if (!string.IsNullOrEmpty(name))
                    allNames.Add(name);
            }

            // 2단계: 참조 검증 + 그래프 구축
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string json = System.IO.File.ReadAllText(path);
                JObject asmdef;
                try { asmdef = JObject.Parse(json); }
                catch { continue; }

                string name = asmdef["name"]?.ToString() ?? path;
                var refs = new List<string>();
                var refsArray = asmdef["references"] as JArray;
                if (refsArray != null)
                {
                    foreach (var r in refsArray)
                    {
                        string refName = r.ToString();
                        // GUID 참조 형식 처리: "GUID:xxx" → 스킵 (Unity 내부 해석)
                        if (refName.StartsWith("GUID:", StringComparison.Ordinal))
                            continue;
                        refs.Add(refName);
                        if (!allNames.Contains(refName))
                            missingRefs.Add($"{name} -> {refName}");
                    }
                }

                refGraph[name] = refs;

                var platforms = asmdef["includePlatforms"] as JArray;
                bool isEditor = false;
                if (platforms != null)
                {
                    foreach (var plat in platforms)
                    {
                        if (string.Equals(plat.ToString(), "Editor", StringComparison.OrdinalIgnoreCase))
                            isEditor = true;
                    }
                }

                asmdefInfos.Add(new
                {
                    name,
                    path,
                    reference_count = refs.Count,
                    editor_only = isEditor
                });
            }

            // 3단계: 순환 참조 탐지 (DFS)
            var circularRefs = DetectCycles(refGraph);

            bool valid = missingRefs.Count == 0 && circularRefs.Count == 0;

            return new SuccessResponse(
                valid
                    ? $"Asmdef validation passed: {guids.Length} assembly definitions."
                    : $"Asmdef validation: {missingRefs.Count} missing ref(s), {circularRefs.Count} cycle(s).",
                new
                {
                    valid,
                    asmdef_count = guids.Length,
                    asmdefs = asmdefInfos,
                    missing_refs = missingRefs.Count > 0 ? missingRefs : null,
                    circular_refs = circularRefs.Count > 0 ? circularRefs : null
                });
        }

        private static List<string> DetectCycles(Dictionary<string, List<string>> graph)
        {
            var cycles = new List<string>();
            var visited = new HashSet<string>();
            var inStack = new HashSet<string>();
            var path = new List<string>();

            foreach (string node in graph.Keys)
            {
                if (!visited.Contains(node))
                    Dfs(node, graph, visited, inStack, path, cycles);
            }
            return cycles;
        }

        private static void Dfs(string node, Dictionary<string, List<string>> graph,
            HashSet<string> visited, HashSet<string> inStack, List<string> path, List<string> cycles)
        {
            visited.Add(node);
            inStack.Add(node);
            path.Add(node);

            if (graph.TryGetValue(node, out var neighbors))
            {
                foreach (string neighbor in neighbors)
                {
                    if (!graph.ContainsKey(neighbor)) continue;

                    if (inStack.Contains(neighbor))
                    {
                        int idx = path.IndexOf(neighbor);
                        string cycle = string.Join(" -> ", path.GetRange(idx, path.Count - idx)) + " -> " + neighbor;
                        cycles.Add(cycle);
                    }
                    else if (!visited.Contains(neighbor))
                    {
                        Dfs(neighbor, graph, visited, inStack, path, cycles);
                    }
                }
            }

            path.RemoveAt(path.Count - 1);
            inStack.Remove(node);
        }
    }
}
