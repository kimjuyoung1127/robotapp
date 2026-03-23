// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.Templates;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FR5 전용 KinematicsFacade 생성 팩토리입니다.
    /// RobotKinematicsFacade를 FR5 템플릿으로 초기화합니다.
    /// 하위 호환성을 위해 유지되며, 새 코드에는 RobotKinematicsFacade를 직접 사용하세요.
    /// </summary>
    public static class FR5KinematicsFacade
    {
        /// <summary>
        /// FR5 템플릿으로 초기화된 RobotKinematicsFacade를 생성합니다.
        /// </summary>
        public static RobotKinematicsFacade Create()
        {
            return new RobotKinematicsFacade(TemplateFAIRINO_FR5.Create());
        }
    }
}
