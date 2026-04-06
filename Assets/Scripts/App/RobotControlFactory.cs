// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.App.Doosan;
using KineTutor3D.App.Fairino;
using KineTutor3D.App.Mecademic;
using KineTutor3D.App.UniversalRobots;
using KineTutor3D.Templates;
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
                case "2DOF_RR":
                    return Create2DofTemplateDefinition();
                case "SCARA_RV":
                    return CreateScaraTemplateDefinition();
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

        private static RobotControlTemplateDefinition Create2DofTemplateDefinition()
        {
            var currentPose = new double[Template2DOF_RR.Create().Dof];

            return new RobotControlTemplateDefinition
            {
                RobotId = Template2DOF_RR.Name,
                DisplayName = "2DOF RR",
                ControlPrefabResourcePath = string.Empty,
                ShowroomPrefabResourcePath = string.Empty,
                JointCount = 2,
                ConfigResourceName = string.Empty,
                RuntimeRootName = "SandboxRuntimeRoot",
                ControlRobotInstanceName = "SandboxRobot",
                ConnectionTitleText = "Simulation Connection",
                TopBarModeText = "2DOF RR · Mock by default",
                PosePresetOptionsFactory = () => new[]
                {
                    new RobotControlPosePresetOption("Ready", "기본 준비 자세", new[] { 0d, 0d })
                },
                PosePresetProvider = new RobotControlPosePresetProvider(
                    readyPoseFactory: () => new double[] { 0d, 0d },
                    updateCurrentPose: pose => CopyPose(pose, currentPose)),
                KinematicsFactory = () => new RobotKinematicsFacade(Template2DOF_RR.Create())
            };
        }

        private static RobotControlTemplateDefinition CreateScaraTemplateDefinition()
        {
            var currentPose = new double[TemplateSCARA_RV.Create().Dof];

            return new RobotControlTemplateDefinition
            {
                RobotId = TemplateSCARA_RV.Name,
                DisplayName = "SCARA Robot",
                ControlPrefabResourcePath = "Robots/ScaraRobot",
                ShowroomPrefabResourcePath = "Robots/ScaraRobot",
                JointCount = 4,
                ConfigResourceName = string.Empty,
                RuntimeRootName = "SandboxRuntimeRoot",
                ControlRobotInstanceName = "SandboxRobot",
                ConnectionTitleText = "SCARA Connection",
                TopBarModeText = "SCARA · Mock by default",
                PosePresetOptionsFactory = () => new[]
                {
                    new RobotControlPosePresetOption("Ready", "기본 준비 자세", new[] { 0d, 25d, 0d, 0d })
                },
                PosePresetProvider = new RobotControlPosePresetProvider(
                    readyPoseFactory: () => new double[] { 0d, 25d, 0d, 0d },
                    updateCurrentPose: pose => CopyPose(pose, currentPose)),
                KinematicsFactory = () => new RobotKinematicsFacade(TemplateSCARA_RV.Create())
            };
        }

        private static void CopyPose(double[] source, double[] target)
        {
            if (source == null || target == null)
            {
                return;
            }

            var count = Mathf.Min(source.Length, target.Length);
            for (var i = 0; i < count; i++)
            {
                target[i] = source[i];
            }
        }
    }
}
