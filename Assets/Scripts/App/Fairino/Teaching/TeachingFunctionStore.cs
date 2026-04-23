// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.IO;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// Unity teaching function 단위입니다. 제조사 프로그램이 아니라 저장 포인트 묶음입니다.
    /// </summary>
    [Serializable]
    public sealed class TeachingFunction
    {
        public string name;
        public string description;
        public TeachingFunctionStep[] steps = Array.Empty<TeachingFunctionStep>();
        public string created;
        public string updated;
    }

    /// <summary>
    /// Teaching function 안의 단일 참조 step입니다.
    /// </summary>
    [Serializable]
    public sealed class TeachingFunctionStep
    {
        public string kind = "PointRef";
        public string refName;
        public bool enabled = true;
        public string note;
    }

    /// <summary>
    /// TeachingFunction JSON 저장, 로드, 요약을 담당합니다.
    /// </summary>
    public sealed class TeachingFunctionStore
    {
        private const string SubFolder = "teaching-functions";
        private const string PointRefKind = "PointRef";

        public TeachingFunction CreateFromSequence(string functionName, WaypointSequence sequence)
        {
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                return null;
            }

            var safeName = NormalizeName(functionName);
            var steps = new TeachingFunctionStep[sequence.waypoints.Length];
            for (var index = 0; index < sequence.waypoints.Length; index++)
            {
                var point = sequence.waypoints[index];
                steps[index] = new TeachingFunctionStep
                {
                    kind = PointRefKind,
                    refName = point?.name ?? string.Empty,
                    enabled = true,
                    note = string.Empty
                };
            }

            var now = DateTime.Now.ToString("O");
            return new TeachingFunction
            {
                name = safeName,
                description = $"Created from {sequence.name}",
                steps = steps,
                created = now,
                updated = now
            };
        }

        public TeachingFunction CreateFromPointRefs(string functionName, string[] pointNames, string sourceName)
        {
            if (pointNames == null || pointNames.Length == 0)
            {
                return null;
            }

            var steps = new TeachingFunctionStep[pointNames.Length];
            for (var index = 0; index < pointNames.Length; index++)
            {
                steps[index] = new TeachingFunctionStep
                {
                    kind = PointRefKind,
                    refName = pointNames[index]?.Trim() ?? string.Empty,
                    enabled = true,
                    note = string.Empty
                };
            }

            var now = DateTime.Now.ToString("O");
            return new TeachingFunction
            {
                name = NormalizeName(functionName),
                description = $"Created from selected points in {sourceName}",
                steps = steps,
                created = now,
                updated = now
            };
        }

        public bool Save(TeachingFunction function)
        {
            if (function == null || string.IsNullOrWhiteSpace(function.name))
            {
                return false;
            }

            var dir = GetStorageDirectory();
            EnsureDirectory(dir);
            function.name = NormalizeName(function.name);
            function.updated = DateTime.Now.ToString("O");
            var path = Path.Combine(dir, SanitizeFileName(function.name) + ".json");
            File.WriteAllText(path, JsonUtility.ToJson(function, true));
            Debug.Log($"[TeachingFunctionStore] 저장 완료: {path}");
            return true;
        }

        public TeachingFunction Load(string functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                return null;
            }

            var path = Path.Combine(GetStorageDirectory(), SanitizeFileName(NormalizeName(functionName)) + ".json");
            if (!File.Exists(path))
            {
                return null;
            }

            return JsonUtility.FromJson<TeachingFunction>(File.ReadAllText(path));
        }

        public string[] LoadAllNames()
        {
            var dir = GetStorageDirectory();
            if (!Directory.Exists(dir))
            {
                return Array.Empty<string>();
            }

            var files = Directory.GetFiles(dir, "*.json");
            var names = new string[files.Length];
            for (var index = 0; index < files.Length; index++)
            {
                names[index] = Path.GetFileNameWithoutExtension(files[index]);
            }

            Array.Sort(names, StringComparer.OrdinalIgnoreCase);
            return names;
        }

        public bool Delete(string functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                return false;
            }

            var path = Path.Combine(GetStorageDirectory(), SanitizeFileName(NormalizeName(functionName)) + ".json");
            if (!File.Exists(path))
            {
                return false;
            }

            File.Delete(path);
            Debug.Log($"[TeachingFunctionStore] 삭제 완료: {path}");
            return true;
        }

        public int DeleteAll()
        {
            var names = LoadAllNames();
            var deleted = 0;
            for (var index = 0; index < names.Length; index++)
            {
                if (Delete(names[index]))
                {
                    deleted++;
                }
            }

            return deleted;
        }

        public TeachingFunction Duplicate(string sourceName, string requestedName = null)
        {
            var source = Load(sourceName);
            if (source == null)
            {
                return null;
            }

            var copy = CloneFunction(source);
            copy.name = BuildUniqueName(string.IsNullOrWhiteSpace(requestedName) ? $"{source.name}_COPY" : requestedName);
            var now = DateTime.Now.ToString("O");
            copy.created = now;
            copy.updated = now;
            return Save(copy) ? copy : null;
        }

        public bool Rename(string oldName, string newName)
        {
            var function = Load(oldName);
            if (function == null || string.IsNullOrWhiteSpace(newName))
            {
                return false;
            }

            var safeNewName = NormalizeName(newName);
            if (!string.Equals(oldName, safeNewName, StringComparison.OrdinalIgnoreCase) && Load(safeNewName) != null)
            {
                return false;
            }

            Delete(oldName);
            function.name = safeNewName;
            return Save(function);
        }

        public string BuildSummary()
        {
            var names = LoadAllNames();
            var parts = new string[names.Length];
            for (var index = 0; index < names.Length; index++)
            {
                var function = Load(names[index]);
                parts[index] = $"{index}:{names[index]}:{(function?.steps?.Length ?? 0)}";
            }

            return $"functions={names.Length}; list=[{string.Join(",", parts)}]";
        }

        public string BuildDetail(string functionName)
        {
            var function = Load(functionName);
            if (function == null)
            {
                return "function=none";
            }

            var steps = function.steps ?? Array.Empty<TeachingFunctionStep>();
            var parts = new string[steps.Length];
            for (var index = 0; index < steps.Length; index++)
            {
                var step = steps[index];
                parts[index] = $"{index}:{step?.kind}:{step?.refName}:{step?.enabled}";
            }

            return $"function={function.name}; steps={steps.Length}; refs=[{string.Join(",", parts)}]";
        }

        public string BuildUniqueName(string baseName)
        {
            var safeBase = NormalizeName(baseName);
            var candidate = safeBase;
            var suffix = 2;
            while (Load(candidate) != null)
            {
                candidate = $"{safeBase}_{suffix}";
                suffix++;
            }

            return candidate;
        }

        public static TeachingFunction CloneFunction(TeachingFunction function)
        {
            if (function == null)
            {
                return null;
            }

            var steps = function.steps ?? Array.Empty<TeachingFunctionStep>();
            var clonedSteps = new TeachingFunctionStep[steps.Length];
            for (var index = 0; index < steps.Length; index++)
            {
                var step = steps[index];
                clonedSteps[index] = new TeachingFunctionStep
                {
                    kind = string.IsNullOrWhiteSpace(step?.kind) ? PointRefKind : step.kind,
                    refName = step?.refName ?? string.Empty,
                    enabled = step == null || step.enabled,
                    note = step?.note ?? string.Empty
                };
            }

            return new TeachingFunction
            {
                name = function.name ?? string.Empty,
                description = function.description ?? string.Empty,
                steps = clonedSteps,
                created = function.created ?? string.Empty,
                updated = function.updated ?? string.Empty
            };
        }

        private static string NormalizeName(string functionName)
        {
            return string.IsNullOrWhiteSpace(functionName)
                ? "Function"
                : functionName.Trim();
        }

        private static string GetStorageDirectory()
        {
            return Path.Combine(Application.persistentDataPath, SubFolder);
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = name;
            for (var index = 0; index < invalid.Length; index++)
            {
                sanitized = sanitized.Replace(invalid[index], '_');
            }

            return sanitized;
        }
    }
}
