// Folder: App - Application controllers and services; single UnityEngine entry point.
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// 씬 간 로봇 선택 상태를 전달합니다. PlayerPrefs 기반입니다.
    /// </summary>
    public static class RobotSelectionBridge
    {
        private const string RobotIdKey = "KineTutor3D.SelectedRobotId";
        private const string ModeKey = "KineTutor3D.SelectedMode";
        public const string GuidedLessonMode = "guided_lesson";
        public const string SandboxMode = "sandbox";
        public const string RobotControlMode = "robot_control";

        public static void SetSelectedRobot(string robotId)
        {
            PlayerPrefs.SetString(RobotIdKey, robotId ?? string.Empty);
            PlayerPrefs.Save();
        }

        public static void SetSelection(string robotId, string mode)
        {
            PlayerPrefs.SetString(RobotIdKey, robotId ?? string.Empty);
            PlayerPrefs.SetString(ModeKey, mode ?? string.Empty);
            PlayerPrefs.Save();
        }

        public static string GetSelectedRobotId()
        {
            return PlayerPrefs.GetString(RobotIdKey, string.Empty);
        }

        public static void SetSelectedMode(string mode)
        {
            PlayerPrefs.SetString(ModeKey, mode ?? string.Empty);
            PlayerPrefs.Save();
        }

        public static string GetSelectedMode()
        {
            return PlayerPrefs.GetString(ModeKey, string.Empty);
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(RobotIdKey);
            PlayerPrefs.DeleteKey(ModeKey);
            PlayerPrefs.Save();
        }
    }
}
