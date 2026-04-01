// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// RobotControlV2 씬의 UI 셸과 상태 계약을 조립하는 V2 전용 composition root입니다.
    /// </summary>
    public sealed class RobotControlV2SceneCoordinator : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private RobotControlShell shell;
        [SerializeField] private RobotControlLayoutCoordinator layoutCoordinator;
        [SerializeField] private RobotControlPopupCoordinator popupCoordinator;

        private RobotControlViewState viewState;

        private void Awake()
        {
            EnsureSceneServices();
            EnsureShell();
            EnsureState();
            BindCoordinators();
            ApplyState();
        }

        private void EnsureSceneServices()
        {
            FairinoRobotControlViewBuilder.EnsureEventSystem();
            canvas = FairinoRobotControlViewBuilder.EnsureCanvas(canvas, fallbackFont);
            FairinoRobotControlViewBuilder.EnsureCamera();
            FairinoRobotControlViewBuilder.EnsureLight();
        }

        private void EnsureShell()
        {
            shell = RobotControlShell.EnsureV2Shell(canvas, fallbackFont, "로봇 제어 V2", "Mock shell");
        }

        private void EnsureState()
        {
            viewState ??= RobotControlViewState.CreateDefault();
            viewState.Changed -= HandleStateChanged;
            viewState.Changed += HandleStateChanged;
        }

        private void BindCoordinators()
        {
            layoutCoordinator = GetComponent<RobotControlLayoutCoordinator>() ?? gameObject.AddComponent<RobotControlLayoutCoordinator>();
            popupCoordinator = GetComponent<RobotControlPopupCoordinator>() ?? gameObject.AddComponent<RobotControlPopupCoordinator>();

            layoutCoordinator.Bind(shell);
            popupCoordinator.Bind(shell);
        }

        private void ApplyState()
        {
            if (shell == null || viewState == null)
            {
                return;
            }

            shell.Bind(viewState);
            layoutCoordinator?.SetTabletLayout(viewState.IsTabletLayout);
            popupCoordinator?.ApplyState(viewState);
        }

        private void HandleStateChanged()
        {
            ApplyState();
        }
    }
}
