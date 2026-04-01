// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// RobotControlV2의 Desktop/Tablet 레이아웃 표시 상태를 조율합니다.
    /// </summary>
    public sealed class RobotControlLayoutCoordinator : MonoBehaviour
    {
        [SerializeField] private bool tabletLayout;
        [SerializeField] private RobotControlShell shell;

        public bool IsTabletLayout => tabletLayout;

        public void Bind(RobotControlShell boundShell)
        {
            shell = boundShell;
            ApplyCurrentLayout();
        }

        public void SetTabletLayout(bool enabled)
        {
            tabletLayout = enabled;
            ApplyCurrentLayout();
        }

        public void ApplyCurrentLayout()
        {
            if (shell == null)
            {
                return;
            }

            shell.ApplyLayoutMode(tabletLayout);
        }
    }
}
