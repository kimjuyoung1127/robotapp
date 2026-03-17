// Folder: Editor/CliTools - unity-cli 커스텀 도구: FR5 로봇 연결 진단
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// FR5 로봇 연결 상태와 기구학 상태를 진단합니다 (Play Mode 전용).
    /// </summary>
    [UnityCliTool(Description = "Diagnose FR5 robot connection and kinematics state (Play Mode only)")]
    public static class FR5DiagnosticTool
    {
        public static object HandleCommand(JObject @params)
        {
            if (!EditorApplication.isPlaying)
            {
                return new SuccessResponse(
                    "FR5 diagnostic requires Play Mode.",
                    new { play_mode = false, hint = "Enter Play Mode with RobotControl scene to use this tool." });
            }

            // RobotControlSceneCoordinator를 통해 접근
            var coordinatorType = FindType("KineTutor3D.App.Fairino.RobotControlSceneCoordinator");
            if (coordinatorType == null)
                return new ErrorResponse("RobotControlSceneCoordinator type not found.");

            var coordinator = Object.FindFirstObjectByType(coordinatorType);
            if (coordinator == null)
                return new SuccessResponse("FR5 diagnostic: no coordinator in scene.",
                    new { play_mode = true, has_coordinator = false, hint = "Load RobotControl scene first." });

            // 리플렉션으로 private 필드 접근
            var connField = coordinatorType.GetField("connectionService", BindingFlags.NonPublic | BindingFlags.Instance);
            var facadeField = coordinatorType.GetField("kinematicsFacade", BindingFlags.NonPublic | BindingFlags.Instance);

            object connectionInfo = null;
            object kinematicsInfo = null;
            object stateInfo = null;

            if (connField != null)
            {
                var connService = connField.GetValue(coordinator);
                if (connService != null)
                {
                    var connType = connService.GetType();
                    bool isMock = (bool)(connType.GetProperty("IsMockMode")?.GetValue(connService) ?? true);
                    var lastState = connType.GetProperty("LastState")?.GetValue(connService);

                    connectionInfo = new { mock_mode = isMock, has_service = true };

                    if (lastState != null)
                    {
                        var stateType = lastState.GetType();
                        var jointPos = stateType.GetProperty("JointPosDeg")?.GetValue(lastState) as double[];
                        var tcpPose = stateType.GetProperty("TcpPose")?.GetValue(lastState) as double[];
                        stateInfo = new { joint_pos_deg = jointPos, tcp_pose = tcpPose };
                    }
                }
            }

            if (facadeField != null)
            {
                var facade = facadeField.GetValue(coordinator);
                if (facade != null)
                {
                    var facadeType = facade.GetType();
                    var jointRad = facadeType.GetProperty("JointValuesRad")?.GetValue(facade) as double[];
                    var eeProp = facadeType.GetProperty("EndEffectorTransform");
                    object eePos = null;

                    if (eeProp != null)
                    {
                        var eeMat = eeProp.GetValue(facade);
                        var extractMethod = eeMat?.GetType().GetMethod("ExtractPosition");
                        if (extractMethod != null)
                        {
                            var pos = extractMethod.Invoke(eeMat, null);
                            var posType = pos.GetType();
                            double x = (double)(posType.GetProperty("X")?.GetValue(pos) ?? 0);
                            double y = (double)(posType.GetProperty("Y")?.GetValue(pos) ?? 0);
                            double z = (double)(posType.GetProperty("Z")?.GetValue(pos) ?? 0);
                            eePos = new[] { x, y, z };
                        }
                    }

                    kinematicsInfo = new { joint_values_rad = jointRad, ee_position = eePos };
                }
            }

            return new SuccessResponse(
                "FR5 diagnostic retrieved.",
                new
                {
                    play_mode = true,
                    has_coordinator = true,
                    connection = connectionInfo,
                    last_state = stateInfo,
                    kinematics = kinematicsInfo
                });
        }

        private static System.Type FindType(string fullName)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }
    }
}
