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
                ConnectionTitleText = "FR5 Connection",
                TopBarModeText = "FR5 · Mock by default",
                BaseLinkName = "base_link",
                PosePresetOptionsFactory = CreatePosePresetOptions,
                PosePresetProvider = new RobotControlPosePresetProvider(
                    () => FR5PosePresets.Ready.JointAnglesDeg,
                    FR5PosePresets.UpdateCurrent),
                KinematicsFactory = () => new RobotKinematicsFacade(TemplateFAIRINO_FR5.Create()),
                ConnectionServiceFactory = translator => new FairinoConnectionService(translator),
                FallbackConfigFactory = () => new FairinoRobotConfig
                {
                    robotId = "FAIRINO_FR5",
                    displayName = "FAIRINO FR5",
                    defaultIp = "192.168.57.2",
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
                    },
                    gripperDefaults = new FairinoRobotConfig.GripperDefaultsBlock
                    {
                        profile = new FairinoRobotConfig.GripperProfileEntry
                        {
                            company = 2,
                            device = 4,
                            softVersion = 0,
                            bus = 0,
                            index = 1,
                        },
                        calibration = new FairinoRobotConfig.GripperCalibrationEntry
                        {
                            closedRawPercent = 0,
                            openRawPercent = 100,
                            objectStopRawPercent = 70,
                            closedVisualInputOpenRatio = 0.6f,
                            openVisualInputOpenRatio = 1f,
                        }
                    }
                }
            };
        }

        private static RobotControlPosePresetOption[] CreatePosePresetOptions()
        {
            var presets = FR5PosePresets.All;
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
