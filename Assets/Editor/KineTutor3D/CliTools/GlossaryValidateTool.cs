// Folder: Editor/CliTools - unity-cli 커스텀 도구: Glossary 데이터베이스 검증
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// Glossary 데이터베이스와 용어 에셋의 존재/참조 무결성을 검증합니다.
    /// </summary>
    [UnityCliTool(Description = "Validate Glossary database and term assets")]
    public static class GlossaryValidateTool
    {
        public static object HandleCommand(JObject @params)
        {
            string glossaryDir = "Assets/Runtime/Resources/Glossary";
            var issues = new List<string>();

            // GlossaryDatabase 존재 확인
            string dbPath = Path.Combine(glossaryDir, "GlossaryDatabase.asset");
            var dbAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(dbPath);
            bool dbExists = dbAsset != null;

            if (!dbExists)
            {
                issues.Add("GlossaryDatabase.asset 로드 실패");
                return new SuccessResponse(
                    "Glossary validation: database missing.",
                    new { valid = false, database_exists = false, issues });
            }

            // 용어 에셋 수집 (GlossaryDatabase 제외)
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { glossaryDir });
            var termInfos = new List<object>();
            int termCount = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);

                if (fileName == "GlossaryDatabase") continue;

                termCount++;
                var termAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                if (termAsset == null)
                {
                    issues.Add($"{fileName}: 에셋 로드 실패");
                    termInfos.Add(new { name = fileName, found = false });
                    continue;
                }

                // 리플렉션으로 주요 필드 확인
                var type = termAsset.GetType();
                string term = GetStringField(termAsset, type, "term") ?? GetStringField(termAsset, type, "Term") ?? fileName;
                string definition = GetStringField(termAsset, type, "definition") ?? GetStringField(termAsset, type, "Definition");

                bool hasDef = !string.IsNullOrEmpty(definition);
                if (!hasDef)
                    issues.Add($"{fileName}: definition 필드가 비어있습니다.");

                termInfos.Add(new
                {
                    name = fileName,
                    found = true,
                    term,
                    has_definition = hasDef,
                    asset_type = type.Name
                });
            }

            // Database 내부 참조 카운트 (리플렉션)
            int dbRefCount = 0;
            var dbType = dbAsset.GetType();
            var entriesField = dbType.GetField("entries",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (entriesField != null && entriesField.FieldType.IsArray)
            {
                var arr = entriesField.GetValue(dbAsset) as System.Array;
                if (arr != null) dbRefCount = arr.Length;
            }

            bool valid = issues.Count == 0;

            return new SuccessResponse(
                valid
                    ? $"Glossary validation passed: {termCount} terms."
                    : $"Glossary: {issues.Count} issue(s).",
                new
                {
                    valid,
                    database_exists = true,
                    database_reference_count = dbRefCount,
                    term_asset_count = termCount,
                    terms = termInfos,
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
