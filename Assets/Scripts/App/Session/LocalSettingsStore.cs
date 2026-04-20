// Folder: App - Application controllers and services; single UnityEngine entry point.
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// Pendant V3 로컬 UI 상태를 PlayerPrefs JSON으로 저장합니다.
    /// </summary>
    public static class LocalSettingsStore
    {
        private const string LocalStateKey = "KineTutor3D.RobotControlV3.LocalStateJson";

        public static void Save(PendantV3LocalState state)
        {
            var normalized = PendantV3LocalState.DeepCopy(state);
            PlayerPrefs.SetString(LocalStateKey, JsonUtility.ToJson(normalized));
            PlayerPrefs.Save();
        }

        public static bool TryLoad(out PendantV3LocalState state)
        {
            var raw = PlayerPrefs.GetString(LocalStateKey, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                state = PendantV3LocalState.Default();
                return false;
            }

            try
            {
                state = PendantV3LocalState.DeepCopy(JsonUtility.FromJson<PendantV3LocalState>(raw));
                return true;
            }
            catch
            {
                state = PendantV3LocalState.Default();
                return false;
            }
        }

        public static PendantV3LocalState LoadOrDefault()
        {
            return TryLoad(out var state) ? state : PendantV3LocalState.Default();
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(LocalStateKey);
            PlayerPrefs.Save();
        }

        public static void ResetForFreshStart()
        {
            var current = LoadOrDefault();
            var reset = PendantV3LocalState.Default();
            reset.HasShownFirstRunGuide = current.HasShownFirstRunGuide;
            Save(reset);
        }
    }
}
