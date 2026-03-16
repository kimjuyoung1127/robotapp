// Folder: Editor/CliTools - unity-cli 커스텀 도구: 두 포즈 간 EE 위치 비교
using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using KineTutor3D.Kinematics;
using KineTutor3D.Math;
using KineTutor3D.Types;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 두 관절 각도 세트의 FK를 계산하여 EE 위치 차이를 비교합니다.
    /// </summary>
    [UnityCliTool(Description = "Compare two joint poses and compute EE position difference")]
    public static class PoseCompareTool
    {
        public class Parameters
        {
            [ToolParameter("Robot template: 2DOF_RR, SCARA_RV, FR5")]
            public string Template { get; set; }

            [ToolParameter("First joint angles in degrees (comma-separated)")]
            public string JointsA { get; set; }

            [ToolParameter("Second joint angles in degrees (comma-separated)")]
            public string JointsB { get; set; }
        }

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            string templateName = p.Get("template", "2DOF_RR");
            string jointsAStr = p.Get("joints_a", null);
            string jointsBStr = p.Get("joints_b", null);

            if (string.IsNullOrEmpty(jointsAStr) || string.IsNullOrEmpty(jointsBStr))
                return new ErrorResponse("Both joints_a and joints_b parameters are required.");

            var template = TemplateResolver.Resolve(templateName);
            if (template == null)
                return new ErrorResponse($"Unknown template: {templateName}. Available: {TemplateResolver.AvailableNames}");

            DHLink[] links = template.GetLinks();

            double[] anglesA = ParseAngles(jointsAStr, links.Length, out string errorA);
            if (errorA != null) return new ErrorResponse($"joints_a: {errorA}");

            double[] anglesB = ParseAngles(jointsBStr, links.Length, out string errorB);
            if (errorB != null) return new ErrorResponse($"joints_b: {errorB}");

            try
            {
                double[] radiansA = ToRadians(anglesA);
                double[] radiansB = ToRadians(anglesB);

                Mat4D matA = ForwardKinematics.ComputeEndEffectorTransform(links, radiansA);
                Mat4D matB = ForwardKinematics.ComputeEndEffectorTransform(links, radiansB);

                Vec3D posA = matA.ExtractPosition();
                Vec3D posB = matB.ExtractPosition();

                Vec3D delta = posA - posB;
                double distance = delta.Magnitude();

                return new SuccessResponse(
                    $"Pose comparison for {templateName}: distance = {distance:F6}.",
                    new
                    {
                        template = templateName,
                        pose_a = new { joints_deg = anglesA, ee_position = new[] { posA.X, posA.Y, posA.Z } },
                        pose_b = new { joints_deg = anglesB, ee_position = new[] { posB.X, posB.Y, posB.Z } },
                        distance,
                        delta = new { x = delta.X, y = delta.Y, z = delta.Z }
                    });
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"Pose comparison failed: {ex.Message}");
            }
        }

        private static double[] ParseAngles(string str, int expectedCount, out string error)
        {
            string[] parts = str.Split(',');
            if (parts.Length != expectedCount)
            {
                error = $"Expected {expectedCount} angles, got {parts.Length}.";
                return null;
            }

            double[] angles = new double[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!double.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out angles[i]))
                {
                    error = $"Invalid angle at index {i}: '{parts[i]}'.";
                    return null;
                }
                if (double.IsNaN(angles[i]) || double.IsInfinity(angles[i]))
                {
                    error = $"NaN/Infinity not allowed at index {i}.";
                    return null;
                }
            }
            error = null;
            return angles;
        }

        private static double[] ToRadians(double[] degrees)
        {
            double[] radians = new double[degrees.Length];
            for (int i = 0; i < degrees.Length; i++)
                radians[i] = degrees[i] * System.Math.PI / 180.0;
            return radians;
        }
    }
}
