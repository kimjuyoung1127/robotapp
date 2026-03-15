// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.IO;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 티칭 웨이포인트 단일 지점입니다.
    /// 관절각, TCP 좌표, 이동 타입, 속도, 대기 시간을 포함합니다.
    /// </summary>
    [Serializable]
    public sealed class Waypoint
    {
        public string name;
        public double[] jointsDeg = new double[6];
        public double[] tcpMm = new double[6];
        public string moveType = "MoveJ";
        public string speedPreset = "medium";
        public double dwellSec;
    }

    /// <summary>
    /// 웨이포인트 시퀀스입니다. 이름, 생성일, 웨이포인트 배열을 포함합니다.
    /// </summary>
    [Serializable]
    public sealed class WaypointSequence
    {
        public string name;
        public string created;
        public Waypoint[] waypoints = Array.Empty<Waypoint>();
    }

    /// <summary>
    /// 웨이포인트 시퀀스의 JSON 영속 저장, 로드, 익스포트, 임포트를 담당합니다.
    /// 저장 경로: Application.persistentDataPath/waypoints/{name}.json
    /// </summary>
    public static class WaypointStore
    {
        private const string SubFolder = "waypoints";

        /// <summary>
        /// 시퀀스를 JSON 파일로 저장합니다.
        /// </summary>
        public static bool Save(WaypointSequence sequence)
        {
            if (sequence == null || string.IsNullOrWhiteSpace(sequence.name))
            {
                Debug.LogWarning("[WaypointStore] 시퀀스 이름이 비어있습니다.");
                return false;
            }

            var dir = GetStorageDirectory();
            EnsureDirectory(dir);

            var filePath = Path.Combine(dir, SanitizeFileName(sequence.name) + ".json");
            var json = JsonUtility.ToJson(sequence, true);
            File.WriteAllText(filePath, json);
            Debug.Log($"[WaypointStore] 저장 완료: {filePath}");
            return true;
        }

        /// <summary>
        /// 이름으로 시퀀스를 로드합니다.
        /// </summary>
        public static WaypointSequence Load(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var filePath = Path.Combine(GetStorageDirectory(), SanitizeFileName(name) + ".json");
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[WaypointStore] 파일 없음: {filePath}");
                return null;
            }

            var json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<WaypointSequence>(json);
        }

        /// <summary>
        /// 저장된 모든 시퀀스 이름 목록을 반환합니다.
        /// </summary>
        public static string[] LoadAllNames()
        {
            var dir = GetStorageDirectory();
            if (!Directory.Exists(dir))
            {
                return Array.Empty<string>();
            }

            var files = Directory.GetFiles(dir, "*.json");
            var names = new string[files.Length];
            for (var i = 0; i < files.Length; i++)
            {
                names[i] = Path.GetFileNameWithoutExtension(files[i]);
            }

            return names;
        }

        /// <summary>
        /// 시퀀스를 삭제합니다.
        /// </summary>
        public static bool Delete(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var filePath = Path.Combine(GetStorageDirectory(), SanitizeFileName(name) + ".json");
            if (!File.Exists(filePath))
            {
                return false;
            }

            File.Delete(filePath);
            Debug.Log($"[WaypointStore] 삭제 완료: {filePath}");
            return true;
        }

        /// <summary>
        /// 시퀀스를 복제합니다.
        /// </summary>
        public static WaypointSequence Duplicate(string srcName, string newName)
        {
            var src = Load(srcName);
            if (src == null)
            {
                return null;
            }

            src.name = newName;
            src.created = DateTime.Now.ToString("O");
            Save(src);
            return src;
        }

        /// <summary>
        /// 시퀀스 이름을 변경합니다.
        /// </summary>
        public static bool Rename(string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
            {
                return false;
            }

            var seq = Load(oldName);
            if (seq == null)
            {
                return false;
            }

            seq.name = newName;
            Save(seq);
            Delete(oldName);
            return true;
        }

        /// <summary>
        /// 시퀀스를 외부 경로로 JSON 익스포트합니다.
        /// </summary>
        public static bool ExportToFile(WaypointSequence sequence, string filePath)
        {
            if (sequence == null || string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            var json = JsonUtility.ToJson(sequence, true);
            File.WriteAllText(filePath, json);
            Debug.Log($"[WaypointStore] 익스포트 완료: {filePath}");
            return true;
        }

        /// <summary>
        /// 외부 JSON 파일에서 시퀀스를 임포트합니다.
        /// </summary>
        public static WaypointSequence ImportFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Debug.LogWarning($"[WaypointStore] 임포트 파일 없음: {filePath}");
                return null;
            }

            var json = File.ReadAllText(filePath);
            var seq = JsonUtility.FromJson<WaypointSequence>(json);
            if (seq == null || seq.waypoints == null)
            {
                Debug.LogWarning("[WaypointStore] 임포트 JSON 파싱 실패");
                return null;
            }

            return seq;
        }

        /// <summary>
        /// 웨이포인트 저장 디렉토리 경로를 반환합니다.
        /// </summary>
        public static string GetStoragePath()
        {
            return GetStorageDirectory();
        }

        /// <summary>
        /// 새 빈 시퀀스를 생성합니다.
        /// </summary>
        public static WaypointSequence CreateEmpty(string name = "Untitled")
        {
            return new WaypointSequence
            {
                name = name,
                created = DateTime.Now.ToString("O"),
                waypoints = Array.Empty<Waypoint>()
            };
        }

        /// <summary>
        /// 시퀀스에 웨이포인트를 추가합니다.
        /// </summary>
        public static void AddWaypoint(WaypointSequence sequence, Waypoint waypoint)
        {
            if (sequence == null || waypoint == null)
            {
                return;
            }

            var existing = sequence.waypoints ?? Array.Empty<Waypoint>();
            var expanded = new Waypoint[existing.Length + 1];
            Array.Copy(existing, expanded, existing.Length);
            expanded[existing.Length] = waypoint;
            sequence.waypoints = expanded;
        }

        /// <summary>
        /// 시퀀스에서 마지막 웨이포인트를 제거합니다.
        /// </summary>
        public static bool RemoveLast(WaypointSequence sequence)
        {
            if (sequence == null || sequence.waypoints == null || sequence.waypoints.Length == 0)
            {
                return false;
            }

            var shrunk = new Waypoint[sequence.waypoints.Length - 1];
            Array.Copy(sequence.waypoints, shrunk, shrunk.Length);
            sequence.waypoints = shrunk;
            return true;
        }

        /// <summary>
        /// 시퀀스에서 특정 인덱스의 웨이포인트를 제거합니다.
        /// </summary>
        public static bool RemoveAt(WaypointSequence sequence, int index)
        {
            if (sequence == null || sequence.waypoints == null
                || index < 0 || index >= sequence.waypoints.Length)
            {
                return false;
            }

            var shrunk = new Waypoint[sequence.waypoints.Length - 1];
            if (index > 0)
            {
                Array.Copy(sequence.waypoints, 0, shrunk, 0, index);
            }

            if (index < sequence.waypoints.Length - 1)
            {
                Array.Copy(sequence.waypoints, index + 1, shrunk, index, sequence.waypoints.Length - index - 1);
            }

            sequence.waypoints = shrunk;
            return true;
        }

        /// <summary>
        /// 시퀀스의 모든 웨이포인트를 제거합니다.
        /// </summary>
        public static void ClearWaypoints(WaypointSequence sequence)
        {
            if (sequence != null)
            {
                sequence.waypoints = Array.Empty<Waypoint>();
            }
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
            for (var i = 0; i < invalid.Length; i++)
            {
                sanitized = sanitized.Replace(invalid[i], '_');
            }

            return sanitized;
        }
    }
}
