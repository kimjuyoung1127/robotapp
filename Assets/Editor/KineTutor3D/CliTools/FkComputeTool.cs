// Folder: Editor/CliTools - unity-cli 커스텀 도구: FK 계산
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
    /// 로봇 템플릿의 FK 파이프라인으로 관절 각도에서 엔드이펙터 포즈를 계산합니다.
    /// </summary>
    [UnityCliTool(Description = "Compute forward kinematics for a robot template")]
    public static class FkComputeTool
    {
        public class Parameters
        {
            [ToolParameter("Robot template: 2DOF_RR, SCARA_RV, FR5")]
            public string Template { get; set; }

            [ToolParameter("Comma-separated joint angles in degrees (e.g. '0,-45,0,-59,-92,-42')")]
            public string Joints { get; set; }
        }

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            string templateName = p.Get("template", "2DOF_RR");
            string jointsStr = p.Get("joints", null);

            if (string.IsNullOrEmpty(jointsStr))
                return new ErrorResponse("'joints' parameter is required (comma-separated degrees).");

            string[] parts = jointsStr.Split(',');
            double[] angles = new double[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!double.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out angles[i]))
                    return new ErrorResponse($"Invalid joint angle at index {i}: '{parts[i]}'");

                if (double.IsNaN(angles[i]) || double.IsInfinity(angles[i]))
                    return new ErrorResponse($"NaN/Infinity not allowed at index {i}.");
            }

            try
            {
                RobotTemplate template = TemplateResolver.Resolve(templateName);
                if (template == null)
                    return new ErrorResponse($"Unknown template: {templateName}. Available: {TemplateResolver.AvailableNames}");

                DHLink[] links = template.GetLinks();
                if (angles.Length != links.Length)
                    return new ErrorResponse($"Joint count mismatch: expected {links.Length}, got {angles.Length}.");

                double[] radians = new double[angles.Length];
                for (int i = 0; i < angles.Length; i++)
                    radians[i] = angles[i] * System.Math.PI / 180.0;

                Mat4D result = ForwardKinematics.ComputeEndEffectorTransform(links, radians);
                Vec3D pos = result.ExtractPosition();

                return new SuccessResponse(
                    $"FK computed for {templateName}.",
                    new
                    {
                        template = templateName,
                        joint_angles_deg = angles,
                        ee_position = new[] { pos.X, pos.Y, pos.Z },
                        transform_matrix = new[]
                        {
                            new[] { result[0,0], result[0,1], result[0,2], result[0,3] },
                            new[] { result[1,0], result[1,1], result[1,2], result[1,3] },
                            new[] { result[2,0], result[2,1], result[2,2], result[2,3] },
                            new[] { result[3,0], result[3,1], result[3,2], result[3,3] }
                        }
                    });
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"FK computation failed: {ex.Message}");
            }
        }

    }
}
