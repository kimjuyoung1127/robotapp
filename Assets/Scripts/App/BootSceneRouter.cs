// Folder: App - Application controllers and services; single UnityEngine entry point.
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// 첫 방문 여부에 따라 시작 씬을 결정합니다.
    /// </summary>
    public class BootSceneRouter : MonoBehaviour
    {
        private void Start()
        {
            var target = StepProgressSaver.HasVisited() ? SceneId.Main : SceneId.Onboarding;
            SceneNavigator.Load(target);
        }
    }
}
