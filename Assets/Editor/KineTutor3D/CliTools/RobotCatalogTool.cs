// Folder: Editor/CliTools - unity-cli 커스텀 도구: 로봇 카탈로그 조회
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using KineTutor3D.Templates;
using KineTutor3D.Types;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// RobotCatalog에 등록된 로봇 목록을 반환합니다.
    /// </summary>
    [UnityCliTool(Description = "List all registered robots from the RobotCatalog")]
    public static class RobotCatalogTool
    {
        public static object HandleCommand(JObject @params)
        {
            var entries = RobotCatalog.GetAll();
            var robots = new List<object>();

            foreach (var entry in entries)
            {
                var meta = entry.Metadata;
                robots.Add(new
                {
                    id = meta.RobotId,
                    name = meta.DisplayName,
                    dof = meta.Dof,
                    type = meta.RobotType,
                    difficulty = meta.Difficulty,
                    has_template = entry.TemplateFactory != null
                });
            }

            return new SuccessResponse(
                $"Found {robots.Count} robot(s) in catalog.",
                new { count = robots.Count, robots });
        }
    }
}
