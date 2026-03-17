// Folder: Editor/CliTools - unity-cli 커스텀 도구: 관절 제한 조회
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using KineTutor3D.Types;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 로봇 템플릿의 관절 제한 범위를 JSON으로 반환합니다.
    /// </summary>
    [UnityCliTool(Description = "Query joint limits for a robot template")]
    public static class JointLimitTool
    {
        public class Parameters
        {
            [ToolParameter("Robot template: 2DOF_RR, SCARA_RV, FR5")]
            public string Template { get; set; }
        }

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            string templateName = p.Get("template", "2DOF_RR");

            var template = TemplateResolver.Resolve(templateName);
            if (template == null)
                return new ErrorResponse($"Unknown template: {templateName}. Available: {TemplateResolver.AvailableNames}");

            JointLimit[] limits = template.GetJointLimits();
            var rows = new List<object>();

            for (int i = 0; i < limits.Length; i++)
            {
                double minDeg = limits[i].Min * 180.0 / System.Math.PI;
                double maxDeg = limits[i].Max * 180.0 / System.Math.PI;
                rows.Add(new
                {
                    joint = i,
                    min_rad = limits[i].Min,
                    max_rad = limits[i].Max,
                    min_deg = minDeg,
                    max_deg = maxDeg,
                    range_deg = maxDeg - minDeg
                });
            }

            return new SuccessResponse(
                $"Joint limits for {template.Name} ({template.Dof} DOF).",
                new
                {
                    template = template.Name,
                    dof = template.Dof,
                    joints = rows
                });
        }
    }
}
