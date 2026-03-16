// Folder: Editor/CliTools - unity-cli 커스텀 도구: 템플릿 이름 해석 공용 헬퍼
using KineTutor3D.Templates;
using KineTutor3D.Types;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// 문자열 이름으로 RobotTemplate을 해석하는 공용 헬퍼입니다.
    /// </summary>
    public static class TemplateResolver
    {
        /// <summary>
        /// 지원되는 템플릿 별칭 목록입니다.
        /// </summary>
        public static readonly string[] SupportedNames =
        {
            "2DOF_RR", "2DOF", "SCARA_RV", "SCARA", "FR5", "FAIRINO_FR5"
        };

        /// <summary>
        /// 이름 또는 별칭으로 RobotTemplate을 생성합니다. 실패 시 null 반환.
        /// </summary>
        public static RobotTemplate Resolve(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            switch (name.ToUpperInvariant())
            {
                case "2DOF_RR":
                case "2DOF":
                    return Template2DOF_RR.Create();
                case "SCARA_RV":
                case "SCARA":
                    return TemplateSCARA_RV.Create();
                case "FR5":
                case "FAIRINO_FR5":
                    return TemplateFAIRINO_FR5.Create();
                default:
                    return null;
            }
        }

        /// <summary>
        /// 지원되는 템플릿 이름 목록을 쉼표로 이어 반환합니다.
        /// </summary>
        public static string AvailableNames => "2DOF_RR, SCARA_RV, FR5";
    }
}
