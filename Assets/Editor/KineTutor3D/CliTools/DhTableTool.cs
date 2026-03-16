// Folder: Editor/CliTools - unity-cli 커스텀 도구: DH 파라미터 테이블 덤프
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using KineTutor3D.Types;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 로봇 템플릿의 DH 파라미터 테이블을 JSON으로 반환합니다.
    /// </summary>
    [UnityCliTool(Description = "Dump DH parameter table for a robot template")]
    public static class DhTableTool
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

            DHLink[] links = template.GetLinks();
            var rows = new List<object>();

            for (int i = 0; i < links.Length; i++)
            {
                var link = links[i];
                rows.Add(new
                {
                    joint = i,
                    theta_rad = link.Theta,
                    theta_deg = link.Theta * 180.0 / System.Math.PI,
                    d = link.D,
                    a = link.A,
                    alpha_rad = link.Alpha,
                    alpha_deg = link.Alpha * 180.0 / System.Math.PI,
                    joint_type = link.JointType.ToString()
                });
            }

            return new SuccessResponse(
                $"DH table for {template.Name} ({template.Dof} DOF).",
                new
                {
                    template = template.Name,
                    dof = template.Dof,
                    links = rows
                });
        }
    }
}
