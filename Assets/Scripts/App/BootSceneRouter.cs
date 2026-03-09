// Folder: App - application orchestration and runtime state.
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// 泥?諛⑸Ц ?щ????곕씪 ?쒖옉 ?ъ쓣 寃곗젙?⑸땲??
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

