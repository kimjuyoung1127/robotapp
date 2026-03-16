// Folder: Editor/CliTools - unity-cli 커스텀 도구: 카메라 위치 캡처/저장/적용
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// Play Mode 카메라 위치를 캡처하고 Edit Mode에서 적용할 수 있는 도구입니다.
    /// </summary>
    [UnityCliTool(Description = "Capture, save, list, apply, and delete camera position snapshots")]
    public static class CameraCaptureTool
    {
        private const string PrefsKeyPrefix = "KineTutor3D_CameraSnapshot_";
        private const string PrefsIndexKey = "KineTutor3D_CameraSnapshotIndex";

        /// <summary>도구 파라미터 정의.</summary>
        public static readonly JArray ToolParams = new JArray
        {
            new JObject
            {
                ["name"] = "action",
                ["type"] = "string",
                ["description"] = "수행할 동작: capture, current, list, apply, delete (기본: capture)",
                ["required"] = false
            },
            new JObject
            {
                ["name"] = "name",
                ["type"] = "string",
                ["description"] = "스냅샷 이름 (capture/apply/delete 시 사용)",
                ["required"] = false
            },
            new JObject
            {
                ["name"] = "scene",
                ["type"] = "string",
                ["description"] = "apply 시 대상 씬 ID (생략 시 캡처 당시 씬 사용)",
                ["required"] = false
            }
        };

        /// <summary>명령을 처리합니다.</summary>
        public static object HandleCommand(JObject @params)
        {
            string action = @params?["action"]?.Value<string>() ?? "capture";
            string snapshotName = @params?["name"]?.Value<string>() ?? "";
            string sceneOverride = @params?["scene"]?.Value<string>() ?? "";

            switch (action.ToLowerInvariant())
            {
                case "capture": return HandleCapture(snapshotName);
                case "current": return HandleCurrent();
                case "list":    return HandleList();
                case "apply":   return HandleApply(snapshotName, sceneOverride);
                case "delete":  return HandleDelete(snapshotName);
                default:
                    return new ErrorResponse($"Unknown action: {action}. Use capture, current, list, apply, or delete.");
            }
        }

        private static object HandleCapture(string name)
        {
            var camera = GetMainCamera();
            if (camera == null)
                return new ErrorResponse("Main Camera not found.");

            if (string.IsNullOrWhiteSpace(name))
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                string timestamp = DateTime.UtcNow.ToString("HHmmss");
                name = $"{sceneName}_{timestamp}";
            }

            var snapshot = new CameraSnapshot
            {
                name = name,
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                posX = camera.transform.position.x,
                posY = camera.transform.position.y,
                posZ = camera.transform.position.z,
                eulerX = camera.transform.eulerAngles.x,
                eulerY = camera.transform.eulerAngles.y,
                eulerZ = camera.transform.eulerAngles.z,
                fov = camera.fieldOfView,
                nearClip = camera.nearClipPlane,
                farClip = camera.farClipPlane,
                bgR = camera.backgroundColor.r,
                bgG = camera.backgroundColor.g,
                bgB = camera.backgroundColor.b,
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                playMode = EditorApplication.isPlaying
            };

            string json = JsonUtility.ToJson(snapshot);
            EditorPrefs.SetString(PrefsKeyPrefix + name, json);
            AddToIndex(name);

            var result = SnapshotToJObject(snapshot);
            result["saved"] = true;
            return new SuccessResponse($"Camera snapshot '{name}' captured.", result);
        }

        private static object HandleCurrent()
        {
            var camera = GetMainCamera();
            if (camera == null)
                return new ErrorResponse("Main Camera not found.");

            var result = new JObject
            {
                ["scene"] = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                ["playMode"] = EditorApplication.isPlaying,
                ["position"] = new JObject
                {
                    ["x"] = RoundTo(camera.transform.position.x),
                    ["y"] = RoundTo(camera.transform.position.y),
                    ["z"] = RoundTo(camera.transform.position.z)
                },
                ["eulerAngles"] = new JObject
                {
                    ["x"] = RoundTo(camera.transform.eulerAngles.x),
                    ["y"] = RoundTo(camera.transform.eulerAngles.y),
                    ["z"] = RoundTo(camera.transform.eulerAngles.z)
                },
                ["fov"] = RoundTo(camera.fieldOfView),
                ["nearClip"] = RoundTo(camera.nearClipPlane),
                ["farClip"] = RoundTo(camera.farClipPlane),
                ["backgroundColor"] = new JObject
                {
                    ["r"] = RoundTo(camera.backgroundColor.r),
                    ["g"] = RoundTo(camera.backgroundColor.g),
                    ["b"] = RoundTo(camera.backgroundColor.b)
                }
            };

            return new SuccessResponse("Current camera state.", result);
        }

        private static object HandleList()
        {
            var names = GetIndex();
            var snapshots = new JArray();

            foreach (string name in names)
            {
                string json = EditorPrefs.GetString(PrefsKeyPrefix + name, "");
                if (string.IsNullOrEmpty(json)) continue;

                var snapshot = JsonUtility.FromJson<CameraSnapshot>(json);
                var entry = new JObject
                {
                    ["name"] = snapshot.name,
                    ["scene"] = snapshot.scene,
                    ["timestamp"] = snapshot.timestamp,
                    ["playMode"] = snapshot.playMode,
                    ["position"] = $"({RoundTo(snapshot.posX)}, {RoundTo(snapshot.posY)}, {RoundTo(snapshot.posZ)})",
                    ["fov"] = RoundTo(snapshot.fov)
                };
                snapshots.Add(entry);
            }

            var result = new JObject
            {
                ["count"] = snapshots.Count,
                ["snapshots"] = snapshots
            };

            return new SuccessResponse($"{snapshots.Count} snapshot(s) found.", result);
        }

        private static object HandleApply(string name, string sceneOverride)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new ErrorResponse("Snapshot name required. Use --name <name>.");

            string json = EditorPrefs.GetString(PrefsKeyPrefix + name, "");
            if (string.IsNullOrEmpty(json))
                return new ErrorResponse($"Snapshot '{name}' not found.");

            var snapshot = JsonUtility.FromJson<CameraSnapshot>(json);

            // SceneCameraDirector 오버라이드 등록
            string targetScene = string.IsNullOrWhiteSpace(sceneOverride) ? snapshot.scene : sceneOverride;
            string overrideJson = JsonUtility.ToJson(snapshot);
            EditorPrefs.SetString("KineTutor3D_CameraOverride_" + targetScene, overrideJson);

            // 현재 카메라에도 즉시 적용
            var camera = GetMainCamera();
            if (camera != null)
            {
                camera.transform.position = new Vector3(snapshot.posX, snapshot.posY, snapshot.posZ);
                camera.transform.rotation = Quaternion.Euler(snapshot.eulerX, snapshot.eulerY, snapshot.eulerZ);
                camera.fieldOfView = snapshot.fov;
                camera.nearClipPlane = snapshot.nearClip;
                camera.farClipPlane = snapshot.farClip;
                camera.backgroundColor = new Color(snapshot.bgR, snapshot.bgG, snapshot.bgB, 1f);
            }

            var result = SnapshotToJObject(snapshot);
            result["appliedTo"] = targetScene;
            result["cameraUpdated"] = camera != null;
            return new SuccessResponse($"Snapshot '{name}' applied to scene '{targetScene}'.", result);
        }

        private static object HandleDelete(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new ErrorResponse("Snapshot name required. Use --name <name>.");

            if (!EditorPrefs.HasKey(PrefsKeyPrefix + name))
                return new ErrorResponse($"Snapshot '{name}' not found.");

            EditorPrefs.DeleteKey(PrefsKeyPrefix + name);
            RemoveFromIndex(name);

            // 관련 오버라이드도 정리
            string json = EditorPrefs.GetString(PrefsKeyPrefix + name, "");
            if (!string.IsNullOrEmpty(json))
            {
                var snapshot = JsonUtility.FromJson<CameraSnapshot>(json);
                EditorPrefs.DeleteKey("KineTutor3D_CameraOverride_" + snapshot.scene);
            }

            return new SuccessResponse($"Snapshot '{name}' deleted.", new JObject { ["deleted"] = name });
        }

        // ── 헬퍼 ──

        private static Camera GetMainCamera()
        {
            return Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
        }

        private static float RoundTo(float value, int digits = 4)
        {
            return (float)System.Math.Round(value, digits);
        }

        private static JObject SnapshotToJObject(CameraSnapshot s)
        {
            return new JObject
            {
                ["name"] = s.name,
                ["scene"] = s.scene,
                ["timestamp"] = s.timestamp,
                ["position"] = new JObject { ["x"] = RoundTo(s.posX), ["y"] = RoundTo(s.posY), ["z"] = RoundTo(s.posZ) },
                ["eulerAngles"] = new JObject { ["x"] = RoundTo(s.eulerX), ["y"] = RoundTo(s.eulerY), ["z"] = RoundTo(s.eulerZ) },
                ["fov"] = RoundTo(s.fov),
                ["nearClip"] = RoundTo(s.nearClip),
                ["farClip"] = RoundTo(s.farClip),
                ["backgroundColor"] = new JObject { ["r"] = RoundTo(s.bgR), ["g"] = RoundTo(s.bgG), ["b"] = RoundTo(s.bgB) }
            };
        }

        // ── 인덱스 관리 (EditorPrefs) ──

        private static List<string> GetIndex()
        {
            string raw = EditorPrefs.GetString(PrefsIndexKey, "");
            if (string.IsNullOrEmpty(raw)) return new List<string>();
            return new List<string>(raw.Split('|'));
        }

        private static void AddToIndex(string name)
        {
            var index = GetIndex();
            if (!index.Contains(name)) index.Add(name);
            EditorPrefs.SetString(PrefsIndexKey, string.Join("|", index));
        }

        private static void RemoveFromIndex(string name)
        {
            var index = GetIndex();
            index.Remove(name);
            EditorPrefs.SetString(PrefsIndexKey, string.Join("|", index));
        }

        // ── 직렬화 구조체 ──

        [Serializable]
        private struct CameraSnapshot
        {
            public string name;
            public string scene;
            public float posX, posY, posZ;
            public float eulerX, eulerY, eulerZ;
            public float fov;
            public float nearClip, farClip;
            public float bgR, bgG, bgB;
            public string timestamp;
            public bool playMode;
        }
    }
}
