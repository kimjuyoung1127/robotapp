// Folder: Editor/CliTools - unity-cli 커스텀 도구: 프리팹 무결성 검증
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 프리팹 에셋의 missing script, 메시 vertex 0 등 무결성을 검사합니다.
    /// </summary>
    [UnityCliTool(Description = "Validate prefab integrity: missing scripts, zero-vertex meshes")]
    public static class PrefabValidateTool
    {
        public class Parameters
        {
            [ToolParameter("Prefab asset path (e.g. 'Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab')")]
            public string Path { get; set; }
        }

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            string path = p.Get("path", null);

            if (string.IsNullOrEmpty(path))
                return new ErrorResponse("'path' parameter is required.");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                return new ErrorResponse($"Prefab not found at: {path}");

            int missingScripts = 0;
            int zeroVertexMeshes = 0;
            int totalObjects = 0;
            var issues = new List<string>();

            ValidateRecursive(prefab, ref missingScripts, ref zeroVertexMeshes, ref totalObjects, issues);

            bool valid = missingScripts == 0 && zeroVertexMeshes == 0;
            string status = valid
                ? $"Prefab valid: {totalObjects} objects checked."
                : $"Prefab has issues: {missingScripts} missing scripts, {zeroVertexMeshes} zero-vertex meshes.";

            return new SuccessResponse(status, new
            {
                valid,
                path,
                total_objects = totalObjects,
                missing_scripts = missingScripts,
                zero_vertex_meshes = zeroVertexMeshes,
                issues
            });
        }

        private static void ValidateRecursive(
            GameObject go,
            ref int missingScripts,
            ref int zeroVertexMeshes,
            ref int totalObjects,
            List<string> issues)
        {
            totalObjects++;

            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (missing > 0)
            {
                missingScripts += missing;
                issues.Add($"Missing script on '{go.name}' ({missing})");
            }

            var mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount == 0)
            {
                zeroVertexMeshes++;
                issues.Add($"Zero-vertex mesh on '{go.name}': {mf.sharedMesh.name}");
            }

            for (int i = 0; i < go.transform.childCount; i++)
            {
                ValidateRecursive(go.transform.GetChild(i).gameObject, ref missingScripts, ref zeroVertexMeshes, ref totalObjects, issues);
            }
        }
    }
}
