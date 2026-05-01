// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UIElements;
using KineTutor3D.App.Fairino;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 2D 팝업(확인/미저장) 최소 scaffold를 관리합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(PendantV3InputContract))]
    public sealed class PopupCoordinatorV3 : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset servoConfirmTemplate;
        [SerializeField] private VisualTreeAsset resetConfirmTemplate;
        [SerializeField] private VisualTreeAsset runConfirmTemplate;
        [SerializeField] private VisualTreeAsset moveConfirmTemplate;
        [SerializeField] private VisualTreeAsset warningDialogTemplate;
        [SerializeField] private VisualTreeAsset recoveryDialogTemplate;
        [SerializeField] private VisualTreeAsset unsavedConfirmTemplate;

        private PendantV3InputContract inputContract;
        private VisualElement root;
        private Label popupCardTitle;
        private Label popupCardSummary;
        private VisualElement popupTemplateHost;
        private VisualElement faultOverlayHost;
        private Button popupCancelButton;
        private Button popupConfirmButton;
        private Button btnServoEnable;
        private Button btnRun;
        private Button btnStop;
        private Button btnResetError;
        private Button btnRunBottom;
        private Button btnStopBottom;
        private Button btnStepBack;
        private Button btnFaultOverlayReset;
        private Button btnFaultOverlayClose;
        private EventCallback<ClickEvent> servoClickCallback;
        private EventCallback<ClickEvent> resetClickCallback;
        private EventCallback<ClickEvent> runClickCallback;
        private EventCallback<ClickEvent> stopClickCallback;
        private EventCallback<ClickEvent> unsavedClickCallback;
        private EventCallback<ClickEvent> recoveryClickCallback;
        private RobotControlV3RuntimeController runtimeController;
        private string activePopupKind = string.Empty;
        private string pendingLiveApprovalToken = string.Empty;
        private string pendingLiveApprovalSummary = string.Empty;
        private bool restoreFaultOverlayAfterPopup;
        private bool isInitialized;
        private bool isInitializing;
        private Coroutine initializeCoroutine;

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
            UnbindButtons();
            if (initializeCoroutine != null)
            {
                StopCoroutine(initializeCoroutine);
                initializeCoroutine = null;
            }

            isInitialized = false;
            isInitializing = false;
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            var title = popupCardTitle?.text ?? "missing";
            var confirmText = popupConfirmButton?.text ?? "missing";
            var hostChildren = popupTemplateHost?.childCount ?? -1;
            var popupOpen = popupTemplateHost != null && popupTemplateHost.childCount > 0;
            return $"initialized={isInitialized}; popupOpen={popupOpen}; title={title}; confirm={confirmText}; templateChildren={hostChildren}; liveApproval=[{pendingLiveApprovalSummary}]";
        }

        private bool TryInitialize()
        {
            if (isInitialized)
            {
                return true;
            }

            if (isInitializing)
            {
                return false;
            }

            isInitializing = true;
            try
            {
            document ??= GetComponent<UIDocument>();
            inputContract ??= GetComponent<PendantV3InputContract>();
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            root = document?.rootVisualElement;
            if (root == null
                || inputContract == null
                || runtimeController == null
                || !runtimeController.ForceInitialize()
                || servoConfirmTemplate == null
                || resetConfirmTemplate == null
                || runConfirmTemplate == null
                || moveConfirmTemplate == null
                || warningDialogTemplate == null
                || recoveryDialogTemplate == null
                || unsavedConfirmTemplate == null)
            {
                return false;
            }

            popupCardTitle = root.Q<Label>("PopupCardTitle");
            popupCardSummary = root.Q<Label>("PopupCardSummary");
            popupTemplateHost = root.Q<VisualElement>("PopupTemplateHost");
            faultOverlayHost = root.Q<VisualElement>("FaultOverlayHost");
            popupCancelButton = root.Q<Button>("BtnPopupCancel");
            popupConfirmButton = root.Q<Button>("BtnPopupConfirm");
            btnServoEnable = root.Q<Button>("BtnServoEnable");
            btnRun = root.Q<Button>("BtnRun");
            btnStop = root.Q<Button>("BtnStop");
            btnResetError = root.Q<Button>("BtnResetError");
            btnRunBottom = root.Q<Button>("BtnRunBottom");
            btnStopBottom = root.Q<Button>("BtnStopBottom");
            btnStepBack = root.Q<Button>("BtnStepBack");
            btnFaultOverlayReset = root.Q<Button>("BtnFaultOverlayReset");
            btnFaultOverlayClose = root.Q<Button>("BtnFaultOverlayClose");

            if (popupCardTitle == null || popupCardSummary == null || popupTemplateHost == null || popupCancelButton == null || popupConfirmButton == null)
            {
                isInitialized = false;
                return false;
            }

            UnbindButtons();
            BindButtons();
            isInitialized = true;
            return true;
            }
            finally
            {
                isInitializing = false;
            }
        }

        private System.Collections.IEnumerator WaitForInitialize()
        {
            for (var frame = 0; frame < 30 && !isInitialized; frame++)
            {
                TryInitialize();
                if (isInitialized)
                {
                    break;
                }

                yield return null;
            }

            initializeCoroutine = null;
        }

        private void BindButtons()
        {
            popupCancelButton.RegisterCallback<ClickEvent>(OnPopupButtonClicked);
            popupConfirmButton.RegisterCallback<ClickEvent>(OnPopupButtonClicked);
            servoClickCallback ??= _ => OpenServoConfirm();
            resetClickCallback ??= _ => OpenResetConfirm();
            runClickCallback ??= _ => OpenRunConfirm();
            stopClickCallback ??= _ => OpenWarningDialog();
            unsavedClickCallback ??= _ => OpenUnsavedConfirm();
            recoveryClickCallback ??= _ => OpenRecoveryDialog();

            if (btnServoEnable != null)
            {
                btnServoEnable.clicked += OpenServoConfirm;
                btnServoEnable.RegisterCallback<ClickEvent>(servoClickCallback);
            }

            if (btnResetError != null)
            {
                btnResetError.clicked += OpenResetConfirm;
                btnResetError.RegisterCallback<ClickEvent>(resetClickCallback);
            }

            if (btnRun != null)
            {
                btnRun.clicked += OpenRunConfirm;
                btnRun.RegisterCallback<ClickEvent>(runClickCallback);
            }

            if (btnStop != null)
            {
                btnStop.clicked += OpenWarningDialog;
                btnStop.RegisterCallback<ClickEvent>(stopClickCallback);
            }

            if (btnRunBottom != null)
            {
                btnRunBottom.clicked += OpenRunConfirm;
                btnRunBottom.RegisterCallback<ClickEvent>(runClickCallback);
            }

            if (btnStopBottom != null)
            {
                btnStopBottom.clicked += OpenWarningDialog;
                btnStopBottom.RegisterCallback<ClickEvent>(stopClickCallback);
            }

            if (btnStepBack != null)
            {
                btnStepBack.clicked += OpenUnsavedConfirm;
                btnStepBack.RegisterCallback<ClickEvent>(unsavedClickCallback);
            }

            if (btnFaultOverlayReset != null)
            {
                btnFaultOverlayReset.clicked += OpenRecoveryDialog;
                btnFaultOverlayReset.RegisterCallback<ClickEvent>(recoveryClickCallback);
            }

            if (btnFaultOverlayClose != null)
            {
                btnFaultOverlayClose.clicked += OpenWarningDialog;
                btnFaultOverlayClose.RegisterCallback<ClickEvent>(stopClickCallback);
            }
        }

        private void UnbindButtons()
        {
            if (popupCancelButton != null)
            {
                popupCancelButton.UnregisterCallback<ClickEvent>(OnPopupButtonClicked);
            }

            if (popupConfirmButton != null)
            {
                popupConfirmButton.UnregisterCallback<ClickEvent>(OnPopupButtonClicked);
            }

            if (btnServoEnable != null)
            {
                btnServoEnable.clicked -= OpenServoConfirm;
                if (servoClickCallback != null)
                {
                    btnServoEnable.UnregisterCallback<ClickEvent>(servoClickCallback);
                }
            }

            if (btnResetError != null)
            {
                btnResetError.clicked -= OpenResetConfirm;
                if (resetClickCallback != null)
                {
                    btnResetError.UnregisterCallback<ClickEvent>(resetClickCallback);
                }
            }

            if (btnRun != null)
            {
                btnRun.clicked -= OpenRunConfirm;
                if (runClickCallback != null)
                {
                    btnRun.UnregisterCallback<ClickEvent>(runClickCallback);
                }
            }

            if (btnStop != null)
            {
                btnStop.clicked -= OpenWarningDialog;
                if (stopClickCallback != null)
                {
                    btnStop.UnregisterCallback<ClickEvent>(stopClickCallback);
                }
            }

            if (btnRunBottom != null)
            {
                btnRunBottom.clicked -= OpenRunConfirm;
                if (runClickCallback != null)
                {
                    btnRunBottom.UnregisterCallback<ClickEvent>(runClickCallback);
                }
            }

            if (btnStopBottom != null)
            {
                btnStopBottom.clicked -= OpenWarningDialog;
                if (stopClickCallback != null)
                {
                    btnStopBottom.UnregisterCallback<ClickEvent>(stopClickCallback);
                }
            }

            if (btnStepBack != null)
            {
                btnStepBack.clicked -= OpenUnsavedConfirm;
                if (unsavedClickCallback != null)
                {
                    btnStepBack.UnregisterCallback<ClickEvent>(unsavedClickCallback);
                }
            }

            if (btnFaultOverlayReset != null)
            {
                btnFaultOverlayReset.clicked -= OpenRecoveryDialog;
                if (recoveryClickCallback != null)
                {
                    btnFaultOverlayReset.UnregisterCallback<ClickEvent>(recoveryClickCallback);
                }
            }

            if (btnFaultOverlayClose != null)
            {
                btnFaultOverlayClose.clicked -= OpenWarningDialog;
                if (stopClickCallback != null)
                {
                    btnFaultOverlayClose.UnregisterCallback<ClickEvent>(stopClickCallback);
                }
            }
        }

        private void OpenServoConfirm()
        {
            activePopupKind = "servo";
            OpenPopup(servoConfirmTemplate);
        }

        private void OpenResetConfirm()
        {
            activePopupKind = "reset";
            OpenPopup(resetConfirmTemplate);
        }

        private void OpenRunConfirm()
        {
            activePopupKind = "run";
            OpenPopup(runConfirmTemplate);
        }

        private void OpenMoveConfirm()
        {
            activePopupKind = "move";
            OpenPopup(moveConfirmTemplate);
        }

        public void OpenMoveConfirmForProduct()
        {
            if (!isInitialized && !TryInitialize())
            {
                return;
            }

            OpenMoveConfirm();
        }

        public void OpenRunConfirmForProduct()
        {
            if (!isInitialized && !TryInitialize())
            {
                return;
            }

            OpenRunConfirm();
        }

        private void OpenWarningDialog()
        {
            activePopupKind = "warning";
            OpenPopup(warningDialogTemplate);
        }

        private void OpenRecoveryDialog()
        {
            activePopupKind = "recovery";
            OpenPopup(recoveryDialogTemplate);
        }

        private void OpenUnsavedConfirm()
        {
            activePopupKind = "unsaved";
            OpenPopup(unsavedConfirmTemplate);
        }

        public string OpenPopupForDebug(string popupKind)
        {
            if (!isInitialized && !TryInitialize())
            {
                return GetDebugSummary();
            }

            switch (popupKind)
            {
                case "servo":
                    OpenServoConfirm();
                    break;
                case "reset":
                    OpenResetConfirm();
                    break;
                case "run":
                    OpenRunConfirm();
                    break;
                case "move":
                    OpenMoveConfirm();
                    break;
                case "warning":
                    OpenWarningDialog();
                    break;
                case "recovery":
                    OpenRecoveryDialog();
                    break;
                case "unsaved":
                    OpenUnsavedConfirm();
                    break;
                default:
                    return $"popupKind={popupKind}; supported=servo,reset,run,move,warning,recovery,unsaved";
            }

            return GetDebugSummary();
        }

        public string ConfirmActivePopupForDebug()
        {
            if (!isInitialized && !TryInitialize())
            {
                return GetDebugSummary();
            }

            if (popupTemplateHost == null || popupTemplateHost.childCount == 0)
            {
                return GetDebugSummary();
            }

            ExecuteConfirmedAction();
            CloseActivePopup();
            return GetDebugSummary();
        }

        public string CancelActivePopupForDebug()
        {
            if (!isInitialized && !TryInitialize())
            {
                return GetDebugSummary();
            }

            if (popupTemplateHost == null || popupTemplateHost.childCount == 0)
            {
                return GetDebugSummary();
            }

            runtimeController?.CancelLiveCommandApprovalForProduct();
            CloseActivePopup();
            return GetDebugSummary();
        }

        private void OpenPopup(VisualTreeAsset template)
        {
            if (template == null || popupTemplateHost == null)
            {
                return;
            }

            popupTemplateHost.Clear();
            var tree = template.CloneTree();
            popupTemplateHost.Add(tree);
            ApplyTemplateCopy(tree);
            ApplyLiveApprovalToken(tree);
            SetFaultOverlaySuppressed(true);
            inputContract.OpenPopupProbeForDebug();
        }

        private void CloseActivePopup()
        {
            popupTemplateHost?.Clear();
            inputContract?.ClosePopupProbeForDebug();
            SetFaultOverlaySuppressed(false);
            pendingLiveApprovalToken = string.Empty;
            pendingLiveApprovalSummary = string.Empty;
            activePopupKind = string.Empty;
        }

        private void OnPopupButtonClicked(ClickEvent evt)
        {
            var confirmed = evt.currentTarget == popupConfirmButton;
            if (confirmed)
            {
                ExecuteConfirmedAction();
            }
            else
            {
                runtimeController?.CancelLiveCommandApprovalForProduct();
            }

            CloseActivePopup();
        }

        private void ExecuteConfirmedAction()
        {
            switch (activePopupKind)
            {
                case "servo":
                    runtimeController?.EnableServo();
                    break;
                case "reset":
                case "recovery":
                    runtimeController?.ResetErrors();
                    break;
                case "run":
                case "move":
                    if (ConfirmLiveApprovalIfNeeded())
                    {
                        var commandKind = runtimeController?.ResolvePendingLiveCommandKindForProduct();
                        if (string.Equals(commandKind, "MoveGripper", System.StringComparison.Ordinal))
                        {
                            runtimeController?.ExecutePendingGripperOperatorCommand();
                        }
                        else if (runtimeController != null && runtimeController.HasPendingSavedPointOperatorApproval())
                        {
                            runtimeController.ExecutePendingSavedPointOperatorCommand();
                        }
                        else if (runtimeController != null && runtimeController.HasPendingWaypointSequenceOperatorApproval())
                        {
                            runtimeController.ExecutePendingWaypointSequenceOperatorCommand();
                        }
                        else
                        {
                            runtimeController?.ExecutePreparedPreviewForProduct();
                        }
                    }
                    break;
                case "warning":
                    runtimeController?.StopMotion();
                    break;
                case "unsaved":
                    runtimeController?.StepBackward();
                    break;
            }
        }

        private void SetFaultOverlaySuppressed(bool suppressed)
        {
            if (faultOverlayHost == null)
            {
                return;
            }

            if (suppressed)
            {
                restoreFaultOverlayAfterPopup = !faultOverlayHost.ClassListContains("rc-hidden");
                if (restoreFaultOverlayAfterPopup)
                {
                    faultOverlayHost.EnableInClassList("rc-hidden", true);
                }

                return;
            }

            if (!restoreFaultOverlayAfterPopup)
            {
                return;
            }

            faultOverlayHost.EnableInClassList("rc-hidden", false);
            restoreFaultOverlayAfterPopup = false;
        }

        private void ApplyTemplateCopy(VisualElement tree)
        {
            var metaTitle = tree.Q<Label>("PopupMetaTitle");
            var metaSummary = tree.Q<Label>("PopupMetaSummary");
            var metaConfirm = tree.Q<Label>("PopupMetaConfirm");
            var metaCancel = tree.Q<Label>("PopupMetaCancel");
            var metaDanger = tree.Q<Label>("PopupMetaDanger");

            popupCardTitle.text = metaTitle?.text ?? string.Empty;
            popupCardSummary.text = metaSummary?.text ?? string.Empty;
            popupCancelButton.text = metaCancel?.text ?? string.Empty;
            popupConfirmButton.text = metaConfirm?.text ?? string.Empty;

            var isDanger = bool.TryParse(metaDanger?.text, out var parsedDanger) && parsedDanger;
            popupConfirmButton.EnableInClassList("rc-popup-button--danger", isDanger);
            popupConfirmButton.EnableInClassList("rc-popup-button--primary", !isDanger);
        }

        private void ApplyLiveApprovalToken(VisualElement tree)
        {
            pendingLiveApprovalToken = string.Empty;
            pendingLiveApprovalSummary = string.Empty;
            if (runtimeController == null || !IsLiveApprovalPopup())
            {
                return;
            }

            var commandKind = runtimeController.ResolvePendingLiveCommandKindForProduct();
            pendingLiveApprovalSummary = runtimeController.BeginLiveCommandApprovalForProduct(commandKind);
            pendingLiveApprovalToken = ExtractToken(pendingLiveApprovalSummary);
            var body = tree.Q<VisualElement>("ActionRunConfirmBody") ?? tree.Q<VisualElement>("MoveConfirmBody") ?? tree;
            popupCardSummary.text = BuildApprovalTargetHeadline(commandKind, pendingLiveApprovalSummary);

            var metaBlock = new VisualElement
            {
                name = "LiveApprovalMetaBlock",
            };
            metaBlock.AddToClassList("rc-popup-template-meta");

            metaBlock.Add(BuildPopupLine($"승인 대상: {BuildApprovalTargetLabel(commandKind)}"));
            metaBlock.Add(BuildPopupLine($"확인 요약: {FormatApprovalLeadSummary(pendingLiveApprovalSummary)}"));
            body.Insert(0, metaBlock);
        }

        private bool ConfirmLiveApprovalIfNeeded()
        {
            if (runtimeController == null || !IsLiveApprovalPopup())
            {
                return true;
            }

            if (runtimeController.TryConfirmLiveCommandApprovalForProduct(pendingLiveApprovalToken, out var summary))
            {
                pendingLiveApprovalSummary = summary;
                return true;
            }

            pendingLiveApprovalSummary = summary;
            return false;
        }

        private static string ExtractToken(string summary)
        {
            const string tokenNeedle = "token=";
            var tokenStart = summary.IndexOf(tokenNeedle, System.StringComparison.Ordinal);
            if (tokenStart < 0)
            {
                return string.Empty;
            }

            tokenStart += tokenNeedle.Length;
            var tokenEnd = summary.IndexOf(';', tokenStart);
            return tokenEnd >= tokenStart
                ? summary.Substring(tokenStart, tokenEnd - tokenStart)
                : summary.Substring(tokenStart);
        }

        private bool IsLiveApprovalPopup()
        {
            return activePopupKind == "run" || activePopupKind == "move";
        }

        private static string FormatApprovalSummary(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return "토큰 없음";
            }

            return summary
                .Replace("approvalRequired=True", "승인 필요")
                .Replace("approvalRequired=False", "승인 생략")
                .Replace("kind=", "명령=")
                .Replace("token=", "토큰=")
                .Replace("expires=", "만료=")
                .Replace("reason=dry-run", "DryRun");
        }

        private static string FormatApprovalLeadSummary(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return "승인 상태 확인 중";
            }

            return summary
                .Replace("approvalRequired=True", "승인 필요")
                .Replace("approvalRequired=False", "승인 생략")
                .Replace("kind=", "명령=")
                .Replace("expires=", "만료=")
                .Replace("reason=dry-run", "DryRun");
        }

        private static string BuildApprovalTargetLabel(string commandKind)
        {
            return "이번 연결의 실기 live session";
        }

        private static string BuildApprovalTargetHeadline(string commandKind, string summary)
        {
            return $"승인 대상: {BuildApprovalTargetLabel(commandKind)} · {FormatApprovalLeadSummary(summary)}";
        }

        private static Label BuildPopupLine(string text)
        {
            var label = new Label(text)
            {
                name = "LiveApprovalLine",
            };
            label.AddToClassList("rc-popup-template-line");
            return label;
        }
    }
}
