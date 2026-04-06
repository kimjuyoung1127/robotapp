// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// RobotControlV2 팝업 루트의 기본 표시 상태와 안내 텍스트를 조율합니다.
    /// </summary>
    public sealed class RobotControlPopupCoordinator : MonoBehaviour
    {
        [SerializeField] private RobotControlShell shell;
        [SerializeField] private Button runButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button syncButton;
        [SerializeField] private Button resetErrorButton;
        [SerializeField] private Button easyPreviewButton;
        [SerializeField] private Button easyApplyButton;
        [SerializeField] private Button tcpPreviewButton;
        [SerializeField] private Button tcpMoveButton;
        [SerializeField] private Button pointCalculateButton;
        [SerializeField] private Button pointMoveButton;
        [SerializeField] private Button movePrimaryButton;
        [SerializeField] private Button moveSecondaryButton;
        [SerializeField] private Button warningPrimaryButton;
        [SerializeField] private Button warningSecondaryButton;
        [SerializeField] private Button recoveryPrimaryButton;
        [SerializeField] private Button recoverySecondaryButton;
        [SerializeField] private Button firstRunPrimaryButton;
        [SerializeField] private Button firstRunSecondaryButton;

        private RobotControlViewState currentState;
        private bool listenersBound;

        private void OnDisable()
        {
            UnbindListeners();
        }

        public void Bind(RobotControlShell boundShell)
        {
            UnbindListeners();
            shell = boundShell;
            CacheButtonReferences();
            BindListeners();
            ApplyState(RobotControlViewState.CreateDefault());
        }

        public void ApplyState(RobotControlViewState state)
        {
            if (shell == null)
            {
                return;
            }

            currentState = state ?? RobotControlViewState.CreateDefault();
            shell.SetPopupCopy(
                moveConfirmBody: currentState.IsMockMode
                    ? "모의 연결 모드입니다. 실제 이동 대신 미리보기와 확인 흐름을 먼저 점검합니다."
                    : "실기 연결 경로는 미리보기와 확인 흐름이 준비되기 전까지 보호 상태를 유지합니다.",
                warningBody: currentState.PreviewRiskSummary.Summary,
                recoveryBody: currentState.LastRecoveryHint.Body);
        }

        private void CacheButtonReferences()
        {
            if (shell == null)
            {
                return;
            }

            runButton = shell.FindButton("SafeArea/TopStatusBar/RightCluster/BtnRun");
            stopButton = shell.FindButton("SafeArea/TopStatusBar/RightCluster/BtnStop");
            syncButton = shell.FindButton("SafeArea/TopStatusBar/RightCluster/BtnSync");
            resetErrorButton = shell.FindButton("SafeArea/TopStatusBar/RightCluster/BtnResetError");

            easyPreviewButton = shell.FindButton("SafeArea/LeftRail/WorkPanelHost/EasyMotionPanel/ActionRow/BtnPreview");
            easyApplyButton = shell.FindButton("SafeArea/LeftRail/WorkPanelHost/EasyMotionPanel/ActionRow/BtnApply");
            tcpPreviewButton = shell.FindButton("SafeArea/LeftRail/WorkPanelHost/TcpJogPanel/ActionRow/BtnPreview");
            tcpMoveButton = shell.FindButton("SafeArea/LeftRail/WorkPanelHost/TcpJogPanel/ActionRow/BtnMove");
            pointCalculateButton = shell.FindButton("SafeArea/LeftRail/WorkPanelHost/PointMovePanel/ActionRow/BtnCalculate");
            pointMoveButton = shell.FindButton("SafeArea/LeftRail/WorkPanelHost/PointMovePanel/ActionRow/BtnMove");

            movePrimaryButton = shell.FindButton("SafeArea/Popups/MoveConfirmDialog/ActionRow/BtnPrimary");
            moveSecondaryButton = shell.FindButton("SafeArea/Popups/MoveConfirmDialog/ActionRow/BtnSecondary");
            warningPrimaryButton = shell.FindButton("SafeArea/Popups/WarningDialog/ActionRow/BtnPrimary");
            warningSecondaryButton = shell.FindButton("SafeArea/Popups/WarningDialog/ActionRow/BtnSecondary");
            recoveryPrimaryButton = shell.FindButton("SafeArea/Popups/RecoveryDialog/ActionRow/BtnPrimary");
            recoverySecondaryButton = shell.FindButton("SafeArea/Popups/RecoveryDialog/ActionRow/BtnSecondary");
            firstRunPrimaryButton = shell.FindButton("SafeArea/Popups/FirstRunGuideDialog/ActionRow/BtnPrimary");
            firstRunSecondaryButton = shell.FindButton("SafeArea/Popups/FirstRunGuideDialog/ActionRow/BtnSecondary");
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            runButton?.onClick.AddListener(HandleRunClicked);
            stopButton?.onClick.AddListener(HandleStopClicked);
            syncButton?.onClick.AddListener(HandleSyncClicked);
            resetErrorButton?.onClick.AddListener(HandleResetErrorClicked);
            easyPreviewButton?.onClick.AddListener(HandlePreviewClicked);
            easyApplyButton?.onClick.AddListener(HandleMoveClicked);
            tcpPreviewButton?.onClick.AddListener(HandlePreviewClicked);
            tcpMoveButton?.onClick.AddListener(HandleMoveClicked);
            pointCalculateButton?.onClick.AddListener(HandlePreviewClicked);
            pointMoveButton?.onClick.AddListener(HandleMoveClicked);

            movePrimaryButton?.onClick.AddListener(HideAllPopups);
            moveSecondaryButton?.onClick.AddListener(HideAllPopups);
            warningPrimaryButton?.onClick.AddListener(HideAllPopups);
            warningSecondaryButton?.onClick.AddListener(HideAllPopups);
            recoveryPrimaryButton?.onClick.AddListener(HideAllPopups);
            recoverySecondaryButton?.onClick.AddListener(HideAllPopups);
            firstRunPrimaryButton?.onClick.AddListener(HideAllPopups);
            firstRunSecondaryButton?.onClick.AddListener(HideAllPopups);

            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            runButton?.onClick.RemoveListener(HandleRunClicked);
            stopButton?.onClick.RemoveListener(HandleStopClicked);
            syncButton?.onClick.RemoveListener(HandleSyncClicked);
            resetErrorButton?.onClick.RemoveListener(HandleResetErrorClicked);
            easyPreviewButton?.onClick.RemoveListener(HandlePreviewClicked);
            easyApplyButton?.onClick.RemoveListener(HandleMoveClicked);
            tcpPreviewButton?.onClick.RemoveListener(HandlePreviewClicked);
            tcpMoveButton?.onClick.RemoveListener(HandleMoveClicked);
            pointCalculateButton?.onClick.RemoveListener(HandlePreviewClicked);
            pointMoveButton?.onClick.RemoveListener(HandleMoveClicked);

            movePrimaryButton?.onClick.RemoveListener(HideAllPopups);
            moveSecondaryButton?.onClick.RemoveListener(HideAllPopups);
            warningPrimaryButton?.onClick.RemoveListener(HideAllPopups);
            warningSecondaryButton?.onClick.RemoveListener(HideAllPopups);
            recoveryPrimaryButton?.onClick.RemoveListener(HideAllPopups);
            recoverySecondaryButton?.onClick.RemoveListener(HideAllPopups);
            firstRunPrimaryButton?.onClick.RemoveListener(HideAllPopups);
            firstRunSecondaryButton?.onClick.RemoveListener(HideAllPopups);

            listenersBound = false;
        }

        private void HandleRunClicked()
        {
            ApplyState(currentState ?? RobotControlViewState.CreateDefault());
            if (currentState != null && currentState.PreviewRiskSummary.HasBlockingRisk)
            {
                shell.SetPopupBody("WarningDialog", currentState.PreviewRiskSummary.Summary);
                shell.ShowPopup("WarningDialog");
                return;
            }

            shell.ShowPopup("MoveConfirmDialog");
        }

        private void HandleStopClicked()
        {
            shell.SetPopupBody("WarningDialog", "정지 버튼은 아직 셸 검증 단계의 fallback 동작입니다. 실기 연결 전에는 미리보기와 경고 흐름만 점검합니다.");
            shell.ShowPopup("WarningDialog");
        }

        private void HandleSyncClicked()
        {
            shell.SetPopupBody("RecoveryDialog", "동기화 버튼은 아직 mock shell 단계의 fallback입니다. 연결 상태와 작업 탭을 먼저 확인한 뒤 실제 Sync 경로를 연결합니다.");
            shell.ShowPopup("RecoveryDialog");
        }

        private void HandleResetErrorClicked()
        {
            shell.SetPopupBody("RecoveryDialog", currentState?.LastRecoveryHint?.Body ?? "오류 초기화 전에는 복구 안내와 상태 요약을 먼저 확인하세요.");
            shell.ShowPopup("RecoveryDialog");
        }

        private void HandlePreviewClicked()
        {
            shell.SetPopupBody("WarningDialog", currentState?.PreviewRiskSummary?.Summary ?? "미리보기 위험 요약이 준비되지 않았습니다. fallback 안내를 먼저 표시합니다.");
            shell.ShowPopup("WarningDialog");
        }

        private void HandleMoveClicked()
        {
            ApplyState(currentState ?? RobotControlViewState.CreateDefault());
            shell.ShowPopup("MoveConfirmDialog");
        }

        private void HideAllPopups()
        {
            shell?.HideAllPopups();
        }
    }
}
