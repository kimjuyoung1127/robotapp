// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using KineTutor3D.Templates;

namespace KineTutor3D.App.Doosan
{
    /// <summary>
    /// Doosan M1013 RobotControl 씬 템플릿 정의입니다.
    /// </summary>
    internal static class DoosanM1013RobotControlTemplateDefinition
    {
        public static RobotControlTemplateDefinition Create()
        {
            return new RobotControlTemplateDefinition
            {
                RobotId = "DOOSAN_M1013",
                DisplayName = "Doosan M1013",
                ControlPrefabResourcePath = "Robots/DoosanM1013/DoosanM1013_Control",
                ShowroomPrefabResourcePath = "Robots/DoosanM1013/DoosanM1013",
                JointCount = 6,
                ConfigResourceName = "LearningTabs/DoosanM1013",
                RuntimeRootName = "DoosanM1013_RuntimeRoot",
                ControlRobotInstanceName = "DoosanM1013_UrdfInstance",
                ConnectionTitleText = "Doosan M1013 Connection",
                TopBarModeText = "Doosan M1013 · Mock by default",
                BaseLinkName = "base_link",
                PosePresetOptionsFactory = CreatePosePresetOptions,
                PosePresetProvider = new RobotControlPosePresetProvider(
                    () => DoosanM1013PosePresets.Home.JointAnglesDeg,
                    DoosanM1013PosePresets.UpdateCurrent),
                KinematicsFactory = () => new RobotKinematicsFacade(TemplateDoosanM1013.Create()),
                ConnectionServiceFactory = translator => new FairinoConnectionService(new MockDoosanClient(), translator),
                FallbackConfigFactory = () => new FairinoRobotConfig
                {
                    robotId = "DOOSAN_M1013",
                    displayName = "Doosan M1013",
                    defaultIp = "192.168.137.100",
                    defaultPort = 12345,
                    dof = 6,
                    jointLimits = new[]
                    {
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -150d, maxDeg = 150d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -150d, maxDeg = 150d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -150d, maxDeg = 150d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -150d, maxDeg = 150d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -150d, maxDeg = 150d },
                        new FairinoRobotConfig.JointLimitEntry { minDeg = -150d, maxDeg = 150d }
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
            var presets = DoosanM1013PosePresets.All;
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
