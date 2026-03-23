// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.Templates;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 기존 FR5 RobotControl 동작을 그대로 유지하는 기본 템플릿 정의입니다.
    /// </summary>
    internal static class FR5RobotControlTemplateDefinition
    {
        public static RobotControlTemplateDefinition Create()
        {
            return new RobotControlTemplateDefinition
            {
                RobotId = "FAIRINO_FR5",
                DisplayName = "FAIRINO FR5",
                ControlPrefabResourcePath = "Robots/FAIRINO_FR5_Control",
                ShowroomPrefabResourcePath = "Robots/FAIRINO_FR5",
                JointCount = 6,
                ConfigResourceName = "LearningTabs/FAIRINO_FR5",
                RuntimeRootName = "FR5_RuntimeRoot",
                ControlRobotInstanceName = "FR5_UrdfInstance",
                PosePresetProvider = new RobotControlPosePresetProvider(
                    () => FR5PosePresets.Ready.JointAnglesDeg,
                    FR5PosePresets.UpdateCurrent),
                KinematicsFactory = () => new RobotKinematicsFacade(TemplateFAIRINO_FR5.Create()),
                ConnectionServiceFactory = translator => new FairinoConnectionService(translator),
                FallbackConfigFactory = () => new FairinoRobotConfig
                {
                    robotId = "FAIRINO_FR5",
                    displayName = "FAIRINO FR5",
                    defaultIp = "192.168.58.2",
                    defaultPort = 8080,
                    dof = 6,
                    jointLimits = new[]
                    {
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -175d, maxDeg = 175d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -265d, maxDeg = 85d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -162d, maxDeg = 162d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -265d, maxDeg = 85d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -175d, maxDeg = 175d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -360d, maxDeg = 360d }
                    },
                    speedPresets = new FairinoRobotConfig.SpeedPresetsBlock
                    {
                        slow = new FairinoRobotConfig.SpeedPreset { jointSpeedPercent = 10, accPercent = 20 },
                        medium = new FairinoRobotConfig.SpeedPreset { jointSpeedPercent = 30, accPercent = 50 },
                        fast = new FairinoRobotConfig.SpeedPreset { jointSpeedPercent = 60, accPercent = 80 }
                    }
                }
            };
        }
    }
}
