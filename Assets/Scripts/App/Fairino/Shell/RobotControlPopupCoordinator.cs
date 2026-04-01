// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// RobotControlV2 팝업 루트의 기본 표시 상태와 안내 텍스트를 조율합니다.
    /// </summary>
    public sealed class RobotControlPopupCoordinator : MonoBehaviour
    {
        [SerializeField] private RobotControlShell shell;

        public void Bind(RobotControlShell boundShell)
        {
            shell = boundShell;
            ApplyState(RobotControlViewState.CreateDefault());
        }

        public void ApplyState(RobotControlViewState state)
        {
            if (shell == null)
            {
                return;
            }

            shell.SetPopupCopy(
                moveConfirmBody: state.IsMockMode
                    ? "Mock mode is active. Preview and confirm flow is staged only."
                    : "Live mode stays locked until preview and confirm are wired.",
                warningBody: state.PreviewRiskSummary.Summary,
                recoveryBody: state.LastRecoveryHint.Body);
        }
    }
}
