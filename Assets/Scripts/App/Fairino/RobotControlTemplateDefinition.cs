// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// RobotControl UI에서 사용하는 공통 포즈 프리셋 항목입니다.
    /// </summary>
    public readonly struct RobotControlPosePresetOption
    {
        public RobotControlPosePresetOption(string name, string description, double[] jointAnglesDeg)
        {
            Name = name ?? string.Empty;
            Description = description ?? string.Empty;
            JointAnglesDeg = jointAnglesDeg != null
                ? (double[])jointAnglesDeg.Clone()
                : Array.Empty<double>();
        }

        public string Name { get; }

        public string Description { get; }

        public double[] JointAnglesDeg { get; }
    }

    /// <summary>
    /// RobotControl 씬이 소비하는 로봇별 정의입니다.
    /// </summary>
    internal sealed class RobotControlTemplateDefinition
    {
        public string RobotId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ControlPrefabResourcePath { get; set; } = string.Empty;
        public string ShowroomPrefabResourcePath { get; set; } = string.Empty;
        public int JointCount { get; set; }
        public string ConfigResourceName { get; set; } = string.Empty;
        public string RuntimeRootName { get; set; } = string.Empty;
        public string ControlRobotInstanceName { get; set; } = string.Empty;
        public string ConnectionTitleText { get; set; } = string.Empty;
        public string TopBarModeText { get; set; } = string.Empty;
        public string BaseLinkName { get; set; } = "base_link";
        public Func<RobotControlPosePresetOption[]> PosePresetOptionsFactory { get; set; }
        public RobotControlPosePresetProvider PosePresetProvider { get; set; }
        public Func<RobotKinematicsFacade> KinematicsFactory { get; set; }
        public Func<FairinoErrorTranslator, FairinoConnectionService> ConnectionServiceFactory { get; set; }
        public Func<FairinoRobotConfig> FallbackConfigFactory { get; set; }
    }
}
