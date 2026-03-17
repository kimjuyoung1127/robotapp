// Folder: Editor/CliTools - unity-cli 커스텀 도구: LearningTabs JSON 검증
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 로봇별 LearningTabs JSON 파일의 구조를 검증하고 요약합니다.
    /// </summary>
    [UnityCliTool(Description = "Validate and summarize LearningTabs JSON files")]
    public static class LearningTabsTool
    {
        public class Parameters
        {
            [ToolParameter("Robot ID (e.g. FAIRINO_FR5) or 'all'", Required = false)]
            public string RobotId { get; set; }
        }

        private static readonly string[] allRobotIds =
        {
            "2DOF_RR", "SCARA_RV", "FAIRINO_FR5", "FANUC_CRX10", "IGUS_REBEL", "GENERIC_6DOF"
        };

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            string robotId = p.Get("robot_id", "all");
            string tabsDir = Path.Combine(Application.dataPath, "Runtime", "Resources", "LearningTabs");

            string[] targetIds;
            if (string.Equals(robotId, "all", System.StringComparison.OrdinalIgnoreCase))
                targetIds = allRobotIds;
            else
                targetIds = new[] { robotId };

            var results = new List<object>();
            var issues = new List<string>();

            foreach (string id in targetIds)
            {
                string filePath = Path.Combine(tabsDir, id + ".json");
                if (!File.Exists(filePath))
                {
                    issues.Add($"{id}: JSON 파일 누락");
                    results.Add(new { robot_id = id, found = false, valid = false });
                    continue;
                }

                try
                {
                    string json = File.ReadAllText(filePath);
                    var doc = JObject.Parse(json);

                    string displayTitle = doc["displayTitle"]?.ToString();
                    string heroSummary = doc["heroSummary"]?.ToString();
                    var tabs = doc["tabs"] as JArray;

                    bool hasTitle = !string.IsNullOrEmpty(displayTitle);
                    bool hasTabs = tabs != null && tabs.Count > 0;

                    if (!hasTitle) issues.Add($"{id}: displayTitle 필드 누락");
                    if (!hasTabs) issues.Add($"{id}: tabs 배열 비어있음");

                    var tabSummaries = new List<object>();
                    if (tabs != null)
                    {
                        foreach (var tab in tabs)
                        {
                            string tabId = tab["id"]?.ToString();
                            string labelKo = tab["labelKo"]?.ToString();
                            var cards = tab["cards"] as JArray;

                            if (string.IsNullOrEmpty(tabId))
                                issues.Add($"{id}: tab에 id 필드 누락");

                            tabSummaries.Add(new
                            {
                                id = tabId,
                                label = labelKo,
                                card_count = cards?.Count ?? 0
                            });
                        }
                    }

                    results.Add(new
                    {
                        robot_id = id,
                        found = true,
                        valid = hasTitle && hasTabs,
                        display_title = displayTitle,
                        tab_count = tabs?.Count ?? 0,
                        tabs = tabSummaries
                    });
                }
                catch (System.Exception ex)
                {
                    issues.Add($"{id}: JSON 파싱 실패 — {ex.Message}");
                    results.Add(new { robot_id = id, found = true, valid = false, parse_error = ex.Message });
                }
            }

            bool valid = issues.Count == 0;

            return new SuccessResponse(
                valid
                    ? $"LearningTabs validation passed: {targetIds.Length} file(s)."
                    : $"LearningTabs: {issues.Count} issue(s).",
                new
                {
                    valid,
                    file_count = targetIds.Length,
                    results,
                    issues = issues.Count > 0 ? issues : null
                });
        }
    }
}
