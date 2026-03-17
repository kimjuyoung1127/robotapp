// Folder: Types - Domain value types; no UnityEngine references.
using System;
using System.Linq;

namespace KineTutor3D.Types
{
    /// <summary>
    /// 로봇 메타데이터를 표현하는 불변 구조체입니다.
    /// </summary>
    public readonly struct RobotMetadataInfo
    {
        public string RobotId { get; }
        public string DisplayName { get; }
        public int Dof { get; }
        public string RobotType { get; }
        public string Difficulty { get; }
        public string Convention { get; }
        public bool GuidedLessonSupported { get; }
        public bool SandboxSupported { get; }
        public bool InstructorRecommended { get; }
        public string Description { get; }
        public string[] SupportedLessons { get; }
        public string[] InputModes { get; }
        public string VisualizationLevel { get; }
        public string PreviewSourceRobotId { get; }
        public double[] ZeroPoseDeg { get; }
        public double[] HomePoseDeg { get; }
        public double[] DemoPoseDeg { get; }
        public string ImportSource { get; }
        public string EffectivePreviewRobotId => string.IsNullOrWhiteSpace(PreviewSourceRobotId) ? RobotId : PreviewSourceRobotId;

        public RobotMetadataInfo(
            string robotId,
            string displayName,
            int dof,
            string robotType,
            string difficulty,
            string convention = "DH-Standard",
            bool guidedLessonSupported = false,
            bool sandboxSupported = false,
            bool instructorRecommended = false,
            string description = "",
            string[] supportedLessons = null,
            string[] inputModes = null,
            string visualizationLevel = "Lesson",
            string previewSourceRobotId = "",
            double[] zeroPoseDeg = null,
            double[] homePoseDeg = null,
            double[] demoPoseDeg = null,
            string importSource = "")
        {
            if (string.IsNullOrWhiteSpace(robotId))
            {
                throw new ArgumentException("RobotId는 비어 있을 수 없습니다.", nameof(robotId));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("DisplayName은 비어 있을 수 없습니다.", nameof(displayName));
            }

            RobotId = robotId;
            DisplayName = displayName;
            Dof = dof;
            RobotType = robotType ?? string.Empty;
            Difficulty = difficulty ?? string.Empty;
            Convention = convention ?? "DH-Standard";
            GuidedLessonSupported = guidedLessonSupported;
            SandboxSupported = sandboxSupported;
            InstructorRecommended = instructorRecommended;
            Description = description ?? string.Empty;
            SupportedLessons = CloneOrEmpty(supportedLessons);
            InputModes = CloneOrEmpty(inputModes);
            VisualizationLevel = string.IsNullOrWhiteSpace(visualizationLevel) ? "Lesson" : visualizationLevel;
            PreviewSourceRobotId = string.IsNullOrWhiteSpace(previewSourceRobotId) ? RobotId : previewSourceRobotId;
            ZeroPoseDeg = CloneOrEmpty(zeroPoseDeg, dof);
            HomePoseDeg = CloneOrEmpty(homePoseDeg, dof);
            DemoPoseDeg = CloneOrEmpty(demoPoseDeg, dof);
            ImportSource = importSource ?? string.Empty;
        }

        private static string[] CloneOrEmpty(string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return Array.Empty<string>();
            }

            return values.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        }

        private static double[] CloneOrEmpty(double[] values, int dof)
        {
            if (values == null || values.Length == 0)
            {
                return new double[dof];
            }

            if (values.Length != dof)
            {
                throw new ArgumentException($"Pose 배열 길이는 DOF({dof})와 같아야 합니다.", nameof(values));
            }

            return (double[])values.Clone();
        }
    }
}
