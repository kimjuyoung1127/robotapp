// Folder: App - application orchestration and runtime state.
using System;
using UnityEngine;

namespace KineTutor3D.App
{
    [Serializable]
    public struct SessionContextData
    {
        public string RobotId;
        public string EntryMode;
        public string Track;
        public int Step;
        public string SceneName;
        public string PresetId;
    }

    /// <summary>
    /// 최근 학습/실습 세션의 재진입 컨텍스트를 저장합니다.
    /// </summary>
    public static class SessionContextStore
    {
        private const string ContextKey = "KineTutor3D.SessionContextJson";

        public static void Save(SessionContextData data)
        {
            PlayerPrefs.SetString(ContextKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static bool TryLoad(out SessionContextData data)
        {
            var raw = PlayerPrefs.GetString(ContextKey, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                data = default;
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<SessionContextData>(raw);
                return !string.IsNullOrWhiteSpace(data.EntryMode);
            }
            catch
            {
                data = default;
                return false;
            }
        }

        public static bool HasUsableContext()
        {
            return TryLoad(out var data) &&
                !string.IsNullOrWhiteSpace(data.RobotId) &&
                !string.IsNullOrWhiteSpace(data.EntryMode);
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(ContextKey);
            PlayerPrefs.Save();
        }
    }
}
