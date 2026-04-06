// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 루트에 desktop/tablet 클래스 상태를 적용합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class PendantV3LayoutController : MonoBehaviour
    {
        private UIDocument document;

        private void OnEnable()
        {
            document ??= GetComponent<UIDocument>();
            var root = document?.rootVisualElement;
            if (root == null)
            {
                return;
            }

            root.EnableInClassList("rc-root--desktop", true);
            root.EnableInClassList("rc-root--tablet", false);
        }
    }
}
