// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.App.Doosan;
using KineTutor3D.App.Fairino;
using KineTutor3D.App.Mecademic;
using KineTutor3D.App.UniversalRobots;
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// robotId를 기반으로 적절한 RobotControlTemplateDefinition을 생성하는 팩토리입니다.
    /// 새 로봇 추가 시 이 팩토리에 case를 추가하세요.
    /// </summary>
    internal static class RobotControlFactory
    {
        /// <summary>
        /// robotId에 맞는 RobotControlTemplateDefinition을 반환합니다.
        /// 알 수 없는 ID는 FR5로 폴백됩니다.
        /// </summary>
        public static RobotControlTemplateDefinition Create(string robotId)
        {
            switch (robotId)
            {
                case "FAIRINO_FR5":
                    return FR5RobotControlTemplateDefinition.Create();
                case "UR5e":
                    return UR5eRobotControlTemplateDefinition.Create();
                case "DOOSAN_M1013":
                    return DoosanM1013RobotControlTemplateDefinition.Create();
                case "MECA500":
                    return Meca500RobotControlTemplateDefinition.Create();
                default:
                    Debug.LogWarning($"[RobotControlFactory] Unknown robotId '{robotId}', falling back to FR5");
                    return FR5RobotControlTemplateDefinition.Create();
            }
        }
    }
}
