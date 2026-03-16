// Folder: Editor/CliTools - unity-cli 커스텀 도구: TutorStep 에셋 검증
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// TutorSteps 에셋(S01~S08)의 존재 및 필드 무결성을 검증합니다.
    /// </summary>
    [UnityCliTool(Description = "Validate TutorStep assets (S01-S08) for completeness")]
    public static class TutorStepValidateTool
    {
        private static readonly string[] expectedSteps =
        {
            "S01", "S02", "S03", "S04", "S05", "S06", "S07", "S08"
        };

        public static object HandleCommand(JObject @params)
        {
            var stepInfos = new List<object>();
            var issues = new List<string>();

            foreach (string stepId in expectedSteps)
            {
                string path = $"Assets/Runtime/Resources/TutorSteps/{stepId}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset == null)
                {
                    issues.Add($"{stepId}: 에셋 로드 실패 ({path})");
                    stepInfos.Add(new { id = stepId, found = false, valid = false });
                    continue;
                }

                // 리플렉션으로 주요 필드 검증
                var type = asset.GetType();
                string title = GetStringField(asset, type, "title") ?? GetStringField(asset, type, "Title");
                string description = GetStringField(asset, type, "description") ?? GetStringField(asset, type, "Description");
                string stepName = GetStringField(asset, type, "stepName") ?? GetStringField(asset, type, "StepName");

                bool hasTitle = !string.IsNullOrEmpty(title);
                bool hasDesc = !string.IsNullOrEmpty(description);

                if (!hasTitle)
                    issues.Add($"{stepId}: title 필드가 비어있습니다.");

                stepInfos.Add(new
                {
                    id = stepId,
                    found = true,
                    valid = hasTitle,
                    asset_type = type.Name,
                    title = title ?? "(empty)",
                    has_description = hasDesc,
                    step_name = stepName
                });
            }

            bool valid = issues.Count == 0;

            return new SuccessResponse(
                valid
                    ? $"TutorSteps validation passed: {expectedSteps.Length} steps."
                    : $"TutorSteps: {issues.Count} issue(s).",
                new
                {
                    valid,
                    total_expected = expectedSteps.Length,
                    steps = stepInfos,
                    issues = issues.Count > 0 ? issues : null
                });
        }

        private static string GetStringField(object obj, System.Type type, string fieldName)
        {
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(string))
                return field.GetValue(obj) as string;

            var prop = type.GetProperty(fieldName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (prop != null && prop.PropertyType == typeof(string))
                return prop.GetValue(obj) as string;

            return null;
        }
    }
}
