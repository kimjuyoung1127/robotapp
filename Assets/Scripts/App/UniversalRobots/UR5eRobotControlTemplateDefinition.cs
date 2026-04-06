// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using KineTutor3D.Templates;

namespace KineTutor3D.App.UniversalRobots
{
    /// <summary>
    /// Universal Robots UR5e RobotControl 씬 템플릿 정의입니다.
    /// </summary>
    internal static class UR5eRobotControlTemplateDefinition
    {
        public static RobotControlTemplateDefinition Create()
        {
            return new RobotControlTemplateDefinition
            {
                RobotId = "UR5e",
                DisplayName = "Universal Robots UR5e",
                ControlPrefabResourcePath = "Robots/UR5e/UR5e_Control",
                ShowroomPrefabResourcePath = "Robots/UR5e/UR5e",
                JointCount = 6,
                ConfigResourceName = "LearningTabs/UR5e",
                RuntimeRootName = "UR5e_RuntimeRoot",
                ControlRobotInstanceName = "UR5e_UrdfInstance",
                ConnectionTitleText = "UR5e Connection",
                TopBarModeText = "UR5e · Mock by default",
                BaseLinkName = "base_link",
                PosePresetOptionsFactory = CreatePosePresetOptions,
                PosePresetProvider = new RobotControlPosePresetProvider(
                    () => UR5ePosePresets.Home.JointAnglesDeg,
                    UR5ePosePresets.UpdateCurrent),
                KinematicsFactory = () => new RobotKinematicsFacade(TemplateUR5e.Create()),
                ConnectionServiceFactory = translator => new FairinoConnectionService(new MockUR5eClient(), translator),
                FallbackConfigFactory = () => new FairinoRobotConfig
                {
                    robotId = "UR5e",
                    displayName = "Universal Robots UR5e",
                    defaultIp = "192.168.1.100",
                    defaultPort = 30002,
                    dof = 6,
                    jointLimits = new[]
                    {
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -360d, maxDeg = 360d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -360d, maxDeg = 360d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -180d, maxDeg = 180d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -360d, maxDeg = 360d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -360d, maxDeg = 360d },
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

        private static RobotControlPosePresetOption[] CreatePosePresetOptions()
        {
            var presets = UR5ePosePresets.All;
            var options = new RobotControlPosePresetOption[presets.Length];
            for (var i = 0; i < presets.Length; i++)
            {
                options[i] = new RobotControlPosePresetOption(
                    presets[i].Name,
                    presets[i].Description,
                    presets[i].JointAnglesDeg);
            }

            return options;
        }
    }
}
