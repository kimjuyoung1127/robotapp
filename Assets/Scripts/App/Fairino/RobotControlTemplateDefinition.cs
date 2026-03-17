// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
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
        public RobotControlPosePresetProvider PosePresetProvider { get; set; }
        public Func<FR5KinematicsFacade> KinematicsFactory { get; set; }
        public Func<FairinoErrorTranslator, FairinoConnectionService> ConnectionServiceFactory { get; set; }
        public Func<FairinoRobotConfig> FallbackConfigFactory { get; set; }
    }
}
