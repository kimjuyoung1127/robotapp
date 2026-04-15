// Folder: App - Application controllers and services; single UnityEngine entry point.
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// RobotControl 진입 씬을 테스트 목적에 맞게 선택합니다.
    /// </summary>
    public static class RobotControlScenePreference
    {
        private const string PreferV3Key = "KineTutor3D.RobotControl.PreferV3";

        public static SceneId GetPreferredSceneId()
        {
            return ShouldPreferV3() ? SceneId.RobotControlV3 : SceneId.RobotControl;
        }

        public static bool ShouldPreferV3()
        {
            if (PlayerPrefs.HasKey(PreferV3Key))
            {
                return PlayerPrefs.GetInt(PreferV3Key, 0) != 0;
            }

            return Application.isEditor;
        }

        public static void SetPreferV3(bool value)
        {
            PlayerPrefs.SetInt(PreferV3Key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static string GetDebugSummary()
        {
            return $"preferV3={ShouldPreferV3()}; scene={GetPreferredSceneId()}";
        }
    }
}
