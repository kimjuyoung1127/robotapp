// Folder: Editor/CliTools - unity-cli 커스텀 도구: 세션 컨텍스트 조회/검증
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 저장된 세션 컨텍스트와 학습 진행 상태를 조회합니다.
    /// </summary>
    [UnityCliTool(Description = "Inspect session context and learning progress state")]
    public static class SessionContextTool
    {
        public static object HandleCommand(JObject @params)
        {
            // 기본 상태
            bool hasVisited = PlayerPrefs.GetInt("KineTutor3D.HasVisited", 0) == 1;
            string currentTrack = PlayerPrefs.GetString("KineTutor3D.CurrentTrack", "");
            string selectedRobotId = PlayerPrefs.GetString("KineTutor3D.SelectedRobotId", "");
            string selectedMode = PlayerPrefs.GetString("KineTutor3D.SelectedMode", "");
            bool reducedMotion = PlayerPrefs.GetInt("KineTutor3D.ReducedMotion", 0) == 1;

            // 트랙별 진행도
            int mathStep = PlayerPrefs.GetInt("KineTutor3D.MathReadiness.LastCompletedStep", 0);
            int preKinStep = PlayerPrefs.GetInt("KineTutor3D.PreKinematics.LastCompletedStep", 0);
            int coreStep = PlayerPrefs.GetInt("KineTutor3D.CoreKinematics.LastCompletedStep", 0);

            // 세션 JSON 파싱
            string sessionJson = PlayerPrefs.GetString("KineTutor3D.SessionContextJson", "");
            object sessionData = null;
            bool sessionValid = false;

            if (!string.IsNullOrEmpty(sessionJson))
            {
                try
                {
                    var parsed = JObject.Parse(sessionJson);
                    sessionData = new
                    {
                        robot_id = parsed["RobotId"]?.ToString(),
                        entry_mode = parsed["EntryMode"]?.ToString(),
                        track = parsed["Track"]?.ToString(),
                        step = parsed["Step"]?.ToObject<int>(),
                        scene_name = parsed["SceneName"]?.ToString(),
                        preset_id = parsed["PresetId"]?.ToString()
                    };
                    sessionValid = !string.IsNullOrEmpty(parsed["EntryMode"]?.ToString());
                }
                catch
                {
                    sessionData = "PARSE_ERROR";
                }
            }

            // 예상 다음 씬 추론
            string expectedNextScene;
            if (!hasVisited)
                expectedNextScene = "Onboarding";
            else if (!string.IsNullOrEmpty(selectedMode) && selectedMode == "robot_control")
                expectedNextScene = "RobotControl";
            else if (!string.IsNullOrEmpty(selectedMode) && selectedMode == "sandbox")
                expectedNextScene = "Sandbox";
            else if (currentTrack == "math_readiness")
                expectedNextScene = "MathReadiness";
            else
                expectedNextScene = "RobotLibrary";

            return new SuccessResponse(
                $"Session: visited={hasVisited}, track={currentTrack}, robot={selectedRobotId}.",
                new
                {
                    user_state = new
                    {
                        has_visited = hasVisited,
                        current_track = currentTrack,
                        selected_robot_id = selectedRobotId,
                        selected_mode = selectedMode,
                        reduced_motion = reducedMotion
                    },
                    progress = new
                    {
                        math_readiness = mathStep,
                        pre_kinematics = preKinStep,
                        core_kinematics = coreStep
                    },
                    session_context = new
                    {
                        has_data = !string.IsNullOrEmpty(sessionJson),
                        valid = sessionValid,
                        data = sessionData
                    },
                    expected_next_scene = expectedNextScene
                });
        }
    }
}
