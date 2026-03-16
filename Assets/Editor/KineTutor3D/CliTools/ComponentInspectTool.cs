// Folder: Editor/CliTools - unity-cli 커스텀 도구: 컴포넌트 속성 조회
using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 지정된 GameObject의 컴포넌트 속성을 리플렉션으로 직렬화하여 반환합니다.
    /// </summary>
    [UnityCliTool(Description = "Inspect component properties on a GameObject by path")]
    public static class ComponentInspectTool
    {
        public class Parameters
        {
            [ToolParameter("GameObject path (e.g. 'Canvas/TopBar')")]
            public string Path { get; set; }

            [ToolParameter("Component type name (e.g. 'Transform', 'MeshRenderer')", Required = false)]
            public string Component { get; set; }
        }

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            string path = p.Get("path", null);

            if (string.IsNullOrEmpty(path))
                return new ErrorResponse("'path' parameter is required.");

            var go = GameObject.Find(path);
            if (go == null)
                return new ErrorResponse($"GameObject not found: {path}");

            string componentName = p.Get("component", null);

            if (!string.IsNullOrEmpty(componentName))
            {
                var comp = FindComponent(go, componentName);
                if (comp == null)
                    return new ErrorResponse($"Component '{componentName}' not found on '{path}'.");

                return new SuccessResponse(
                    $"Component '{componentName}' on '{path}'.",
                    new { gameObject = path, component = componentName, properties = SerializeComponent(comp) });
            }

            var allComponents = new List<object>();
            foreach (var comp in go.GetComponents<Component>())
            {
                if (comp == null) continue;
                allComponents.Add(new
                {
                    type = comp.GetType().Name,
                    properties = SerializeComponent(comp)
                });
            }

            return new SuccessResponse(
                $"All components on '{path}'.",
                new { gameObject = path, component_count = allComponents.Count, components = allComponents });
        }

        private static Component FindComponent(GameObject go, string typeName)
        {
            foreach (var comp in go.GetComponents<Component>())
            {
                if (comp != null && string.Equals(comp.GetType().Name, typeName, StringComparison.OrdinalIgnoreCase))
                    return comp;
            }
            return null;
        }

        private static Dictionary<string, object> SerializeComponent(Component comp)
        {
            var dict = new Dictionary<string, object>();

            if (comp is Transform t)
            {
                dict["position"] = new[] { t.position.x, t.position.y, t.position.z };
                dict["rotation"] = new[] { t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w };
                dict["localScale"] = new[] { t.localScale.x, t.localScale.y, t.localScale.z };
                dict["localPosition"] = new[] { t.localPosition.x, t.localPosition.y, t.localPosition.z };
                return dict;
            }

            if (comp is MeshRenderer mr)
            {
                dict["enabled"] = mr.enabled;
                dict["bounds_center"] = new[] { mr.bounds.center.x, mr.bounds.center.y, mr.bounds.center.z };
                dict["bounds_size"] = new[] { mr.bounds.size.x, mr.bounds.size.y, mr.bounds.size.z };
                dict["material_count"] = mr.sharedMaterials.Length;
                return dict;
            }

            if (comp is MeshFilter mf && mf.sharedMesh != null)
            {
                dict["mesh_name"] = mf.sharedMesh.name;
                dict["vertex_count"] = mf.sharedMesh.vertexCount;
                dict["triangle_count"] = (int)(mf.sharedMesh.GetIndexCount(0) / 3);
                return dict;
            }

            var type = comp.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int count = 0;
            foreach (var prop in props)
            {
                if (count >= 20) break;
                if (!prop.CanRead) continue;
                try
                {
                    var val = prop.GetValue(comp);
                    dict[prop.Name] = val?.ToString() ?? "null";
                    count++;
                }
                catch
                {
                    // 리플렉션 접근 실패 시 무시
                }
            }

            return dict;
        }
    }
}
