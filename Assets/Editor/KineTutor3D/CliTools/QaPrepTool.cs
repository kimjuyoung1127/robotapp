// Folder: Editor/CliTools - unity-cli 커스텀 도구: QA 시나리오 준비
using System;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// QA 테스트 시나리오별 PlayerPrefs를 설정합니다.
    /// </summary>
    [UnityCliTool(Description = "Prepare QA test scenarios by setting PlayerPrefs")]
    public static class QaPrepTool
    {
        public class Parameters
        {
            [ToolParameter("Scenario: first-time, returning, sandbox, robot-control, math-readiness")]
            public string Scenario { get; set; }
        }

        private const string HasVisitedKey = "KineTutor3D.HasVisited";
        private const string TrackKey = "KineTutor3D.CurrentTrack";
        private const string SelectedRobotIdKey = "KineTutor3D.SelectedRobotId";
        private const string SelectedModeKey = "KineTutor3D.SelectedMode";
        private const string SessionContextKey = "KineTutor3D.SessionContextJson";

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            string scenario = p.Get("scenario", "first-time");

            switch (scenario.ToLowerInvariant())
            {
                case "first-time":
                    ClearAllQaKeys();
                    return new SuccessResponse("QA: Reset to first-time user.",
                        new { scenario, next_scene = "Onboarding" });

                case "returning":
                    ClearAllQaKeys();
                    PlayerPrefs.SetInt(HasVisitedKey, 1);
                    PlayerPrefs.Save();
                    return new SuccessResponse("QA: Reset to returning user.",
                        new { scenario, next_scene = "RobotLibrary" });

                case "sandbox":
                    ClearAllQaKeys();
                    PlayerPrefs.SetInt(HasVisitedKey, 1);
                    PlayerPrefs.SetString(SelectedModeKey, "sandbox");
                    PlayerPrefs.SetString(SelectedRobotIdKey, "2DOF_RR");
                    PlayerPrefs.Save();
                    return new SuccessResponse("QA: Prepared sandbox scenario.",
                        new { scenario, robot = "2DOF_RR", mode = "sandbox" });

                case "robot-control":
                    ClearAllQaKeys();
                    PlayerPrefs.SetInt(HasVisitedKey, 1);
                    PlayerPrefs.SetString(SelectedModeKey, "robot_control");
                    PlayerPrefs.SetString(SelectedRobotIdKey, "FAIRINO_FR5");
                    PlayerPrefs.Save();
                    return new SuccessResponse("QA: Prepared robot-control scenario.",
                        new { scenario, robot = "FAIRINO_FR5", mode = "robot_control" });

                case "math-readiness":
                    ClearAllQaKeys();
                    PlayerPrefs.SetInt(HasVisitedKey, 1);
                    PlayerPrefs.SetString(TrackKey, "math_readiness");
                    PlayerPrefs.Save();
                    return new SuccessResponse("QA: Prepared math-readiness scenario.",
                        new { scenario, track = "math_readiness" });

                default:
                    return new ErrorResponse(
                        $"Unknown scenario: {scenario}. Available: first-time, returning, sandbox, robot-control, math-readiness");
            }
        }

        private static void ClearAllQaKeys()
        {
            PlayerPrefs.DeleteKey(HasVisitedKey);
            PlayerPrefs.DeleteKey(TrackKey);
            PlayerPrefs.DeleteKey(SelectedRobotIdKey);
            PlayerPrefs.DeleteKey(SelectedModeKey);
            PlayerPrefs.DeleteKey(SessionContextKey);
            PlayerPrefs.DeleteKey("KineTutor3D.MathReadiness.LastCompletedStep");
            PlayerPrefs.DeleteKey("KineTutor3D.PreKinematics.LastCompletedStep");
            PlayerPrefs.DeleteKey("KineTutor3D.CoreKinematics.LastCompletedStep");
            PlayerPrefs.DeleteKey("KineTutor3D.ReducedMotion");
            PlayerPrefs.Save();
        }
    }
}
