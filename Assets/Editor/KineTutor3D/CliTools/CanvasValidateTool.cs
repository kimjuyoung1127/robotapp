// Folder: Editor/CliTools - unity-cli 커스텀 도구: Canvas UI 무결성 검증
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 현재 씬의 Canvas 구성 무결성을 검사합니다 (EventSystem, GraphicRaycaster 등).
    /// </summary>
    [UnityCliTool(Description = "Validate Canvas setup: EventSystem, GraphicRaycaster, sorting order")]
    public static class CanvasValidateTool
    {
        public static object HandleCommand(JObject @params)
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var issues = new List<string>();
            var canvasInfos = new List<object>();

            // EventSystem 존재 확인
            var eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null && canvases.Length > 0)
                issues.Add("EventSystem이 씬에 없습니다. UI 입력이 동작하지 않습니다.");

            // 정렬 순서 중복 감지용
            var sortingOrders = new Dictionary<int, List<string>>();

            foreach (var canvas in canvases)
            {
                bool hasRaycaster = canvas.GetComponent<GraphicRaycaster>() != null;
                string path = GetGameObjectPath(canvas.gameObject);

                if (!hasRaycaster && canvas.renderMode != RenderMode.WorldSpace)
                    issues.Add($"Canvas '{path}'에 GraphicRaycaster가 없습니다.");

                int order = canvas.sortingOrder;
                if (!sortingOrders.ContainsKey(order))
                    sortingOrders[order] = new List<string>();
                sortingOrders[order].Add(path);

                canvasInfos.Add(new
                {
                    name = canvas.gameObject.name,
                    path,
                    render_mode = canvas.renderMode.ToString(),
                    sorting_order = order,
                    has_raycaster = hasRaycaster,
                    enabled = canvas.enabled
                });
            }

            // 정렬 순서 충돌 보고
            foreach (var kvp in sortingOrders)
            {
                if (kvp.Value.Count > 1)
                    issues.Add($"sortingOrder {kvp.Key} 중복: {string.Join(", ", kvp.Value)}");
            }

            bool valid = issues.Count == 0;

            return new SuccessResponse(
                valid
                    ? $"Canvas validation passed: {canvases.Length} canvas(es) found."
                    : $"Canvas validation: {issues.Count} issue(s) found.",
                new
                {
                    valid,
                    canvas_count = canvases.Length,
                    has_event_system = eventSystem != null,
                    canvases = canvasInfos,
                    issues = issues.Count > 0 ? issues : null
                });
        }

        private static string GetGameObjectPath(GameObject go)
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
