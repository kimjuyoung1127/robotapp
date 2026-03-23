// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.App.Fairino;
using KineTutor3D.Templates;

namespace KineTutor3D.App.Mecademic
{
    /// <summary>
    /// Mecademic Meca500 RobotControl 씬 템플릿 정의입니다.
    /// </summary>
    internal static class Meca500RobotControlTemplateDefinition
    {
        public static RobotControlTemplateDefinition Create()
        {
            return new RobotControlTemplateDefinition
            {
                RobotId = "MECA500",
                DisplayName = "Mecademic Meca500",
                ControlPrefabResourcePath = "Robots/Meca500/Meca500_Control",
                ShowroomPrefabResourcePath = "Robots/Meca500/Meca500",
                JointCount = 6,
                ConfigResourceName = "LearningTabs/Meca500",
                RuntimeRootName = "Meca500_RuntimeRoot",
                ControlRobotInstanceName = "Meca500_UrdfInstance",
                PosePresetProvider = new RobotControlPosePresetProvider(
                    () => Meca500PosePresets.Home.JointAnglesDeg,
                    Meca500PosePresets.UpdateCurrent),
                KinematicsFactory = () => new RobotKinematicsFacade(TemplateMeca500.Create()),
                ConnectionServiceFactory = translator => new FairinoConnectionService(new MockMecademicClient(), translator),
                FallbackConfigFactory = () => new FairinoRobotConfig
                {
                    robotId = "MECA500",
                    displayName = "Mecademic Meca500",
                    defaultIp = "192.168.0.100",
                    defaultPort = 10000,
                    dof = 6,
                    jointLimits = new[]
                    {
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -175d, maxDeg = 175d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -70d,  maxDeg = 90d  },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -135d, maxDeg = 70d  },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -170d, maxDeg = 170d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -115d, maxDeg = 115d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -180d, maxDeg = 180d }
                    },
                    speedPresets = new FairinoRobotConfig.SpeedPresetsBlock
                    {
                        slow   = new FairinoRobotConfig.SpeedPreset { jointSpeedPercent = 10, accPercent = 20 },
                        medium = new FairinoRobotConfig.SpeedPreset { jointSpeedPercent = 30, accPercent = 50 },
                        fast   = new FairinoRobotConfig.SpeedPreset { jointSpeedPercent = 60, accPercent = 80 }
                    }
                }
            };
        }
    }
}
