// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.Types;
using UnityEngine.SceneManagement;

namespace KineTutor3D.App
{
    /// <summary>
    /// 현재 앱 상태를 이어하기용 세션 컨텍스트로 저장합니다.
    /// </summary>
    internal sealed class AppSessionContextService
    {
        public void SaveCurrent(RobotTemplate currentTemplate, bool sandboxMode, string track, int step)
        {
            var robotId = currentTemplate != null ? currentTemplate.Name : RobotSelectionBridge.GetSelectedRobotId();
            if (string.IsNullOrWhiteSpace(robotId))
            {
                return;
            }

            SessionContextStore.Save(new SessionContextData
            {
                RobotId = robotId,
                EntryMode = sandboxMode ? RobotSelectionBridge.SandboxMode : RobotSelectionBridge.GuidedLessonMode,
                Track = string.IsNullOrWhiteSpace(track) ? StepProgressSaver.CoreKinematicsTrack : track,
                Step = step,
                SceneName = SceneManager.GetActiveScene().name,
                PresetId = string.Empty
            });
        }
    }
}
