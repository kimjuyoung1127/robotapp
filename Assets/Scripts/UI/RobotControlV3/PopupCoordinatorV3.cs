// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 2D 팝업(확인/미저장) 최소 scaffold를 관리합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(PendantV3InputContract))]
    public sealed class PopupCoordinatorV3 : MonoBehaviour
    {
        private enum PopupKind
        {
            None,
            ServoConfirm,
            ResetConfirm,
            RunConfirm,
            MoveConfirm,
            Warning,
            Recovery,
            Unsaved,
            FirstRunGuide,
        }

        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset servoConfirmTemplate;
        [SerializeField] private VisualTreeAsset resetConfirmTemplate;
        [SerializeField] private VisualTreeAsset runConfirmTemplate;
        [SerializeField] private VisualTreeAsset moveConfirmTemplate;
        [SerializeField] private VisualTreeAsset warningDialogTemplate;
        [SerializeField] private VisualTreeAsset recoveryDialogTemplate;
        [SerializeField] private VisualTreeAsset unsavedConfirmTemplate;
        [SerializeField] private VisualTreeAsset firstRunGuideTemplate;

        private PendantV3InputContract inputContract;
        private ConnectionHomeController connectionHomeController;
        private PointMoveController pointMoveController;
        private PendantV3ShellStateController shellStateController;
        private VisualElement root;
        private Label popupCardTitle;
        private Label popupCardSummary;
        private VisualElement popupTemplateHost;
        private VisualElement faultOverlayHost;
        private Button popupCancelButton;
        private Button popupConfirmButton;
        private Button btnServoEnable;
        private Button btnResetError;
        private Button btnRunBottom;
        private Button btnStopBottom;
        private Button btnStepBack;
        private Button btnSync;
        private Button btnFaultOverlayReset;
        private Button btnFaultOverlayClose;
        private bool restoreFaultOverlayAfterPopup;
        private bool firstRunGuideChecked;
        private PopupKind currentPopupKind;
        private System.Action pendingConfirmAction;
        private System.Action pendingCancelAction;
        private bool isInitialized;
        private Coroutine initializeCoroutine;

        public event System.Action PopupStateChanged;

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
            return $"initialized={isInitialized}; popupOpen={popupOpen}; kind={currentPopupKind}; title={title}; confirm={confirmText}; templateChildren={hostChildren}";
        }

        public bool HasActivePopup => currentPopupKind != PopupKind.None;

        public string GetPopupContextSummary()
        {
            return HasActivePopup
                ? $"{popupCardTitle?.text ?? "팝업"} · {popupCardSummary?.text ?? string.Empty}"
                : string.Empty;
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            inputContract ??= GetComponent<PendantV3InputContract>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            pointMoveController ??= GetComponent<PointMoveController>();
            shellStateController ??= GetComponent<PendantV3ShellStateController>();
            root = document?.rootVisualElement;
            if (root == null
                || inputContract == null
                || servoConfirmTemplate == null
                || resetConfirmTemplate == null
                || runConfirmTemplate == null
                || moveConfirmTemplate == null
                || warningDialogTemplate == null
                || recoveryDialogTemplate == null
                || unsavedConfirmTemplate == null
                || firstRunGuideTemplate == null)
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
            btnResetError = root.Q<Button>("BtnResetError");
            btnRunBottom = root.Q<Button>("BtnRunBottom");
            btnStopBottom = root.Q<Button>("BtnStopBottom");
            btnStepBack = root.Q<Button>("BtnStepBack");
            btnSync = root.Q<Button>("BtnSync");
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
            TryOpenFirstRunGuide();
            return true;
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
            popupCancelButton.RegisterCallback<ClickEvent>(OnPopupCancelClicked);
            popupConfirmButton.RegisterCallback<ClickEvent>(OnPopupConfirmClicked);

            if (btnServoEnable != null)
            {
                btnServoEnable.clicked += OpenServoConfirm;
            }

            if (btnResetError != null)
            {
                btnResetError.clicked += OpenResetConfirm;
            }

            if (btnRunBottom != null)
            {
                btnRunBottom.clicked += OpenRunConfirm;
            }

            if (btnSync != null)
            {
                btnSync.clicked += ApplySyncPolicy;
            }

            if (btnStopBottom != null)
            {
                btnStopBottom.clicked += OpenWarningDialog;
            }

            if (btnStepBack != null)
            {
                btnStepBack.clicked += OpenUnsavedConfirm;
            }

            if (btnFaultOverlayReset != null)
            {
                btnFaultOverlayReset.clicked += OpenRecoveryDialog;
            }

            if (btnFaultOverlayClose != null)
            {
                btnFaultOverlayClose.clicked += OpenWarningDialog;
            }
        }

        private void UnbindButtons()
        {
            if (popupCancelButton != null)
            {
                popupCancelButton.UnregisterCallback<ClickEvent>(OnPopupCancelClicked);
            }

            if (popupConfirmButton != null)
            {
                popupConfirmButton.UnregisterCallback<ClickEvent>(OnPopupConfirmClicked);
            }

            if (btnServoEnable != null)
            {
                btnServoEnable.clicked -= OpenServoConfirm;
            }

            if (btnResetError != null)
            {
                btnResetError.clicked -= OpenResetConfirm;
            }

            if (btnRunBottom != null)
            {
                btnRunBottom.clicked -= OpenRunConfirm;
            }

            if (btnSync != null)
            {
                btnSync.clicked -= ApplySyncPolicy;
            }

            if (btnStopBottom != null)
            {
                btnStopBottom.clicked -= OpenWarningDialog;
            }

            if (btnStepBack != null)
            {
                btnStepBack.clicked -= OpenUnsavedConfirm;
            }

            if (btnFaultOverlayReset != null)
            {
                btnFaultOverlayReset.clicked -= OpenRecoveryDialog;
            }

            if (btnFaultOverlayClose != null)
            {
                btnFaultOverlayClose.clicked -= OpenWarningDialog;
            }
        }

        private void OpenServoConfirm()
        {
            if (connectionHomeController == null || connectionHomeController.CurrentPreviewState != PendantV3PreviewState.Kind.ConnectedServoOff)
            {
                return;
            }

            OpenPopup(PopupKind.ServoConfirm, servoConfirmTemplate, connectionHomeController.ApplyServoEnablePolicy);
        }

        private void OpenResetConfirm()
        {
            if (connectionHomeController == null || connectionHomeController.CurrentPreviewState != PendantV3PreviewState.Kind.Fault)
            {
                return;
            }

            OpenPopup(PopupKind.ResetConfirm, resetConfirmTemplate, connectionHomeController.ApplyResetErrorPolicy);
        }

        private void OpenRunConfirm()
        {
            if (connectionHomeController == null || !connectionHomeController.CurrentPreviewDefinition.RunEnabled)
            {
                return;
            }

            OpenPopup(PopupKind.RunConfirm, runConfirmTemplate, connectionHomeController.ApplyRunPolicy);
        }

        private void OpenMoveConfirm()
        {
            OpenPopup(PopupKind.MoveConfirm, moveConfirmTemplate, null);
        }

        private void OpenWarningDialog()
        {
            OpenPopup(PopupKind.Warning, warningDialogTemplate, null);
        }

        private void OpenRecoveryDialog()
        {
            OpenPopup(PopupKind.Recovery, recoveryDialogTemplate, null);
        }

        private void OpenUnsavedConfirm()
        {
            if (pointMoveController == null || !pointMoveController.HasUnsavedDraft())
            {
                return;
            }

            OpenPopup(PopupKind.Unsaved, unsavedConfirmTemplate, pointMoveController.DiscardDraftAndReturnToEasyMotion);
        }

        private void OpenFirstRunGuide()
        {
            OpenPopup(PopupKind.FirstRunGuide, firstRunGuideTemplate, null);
        }

        public void OpenMoveConfirmForPolicy(string title, string summary, System.Action confirmAction, string confirmLabel)
        {
            OpenPopup(PopupKind.MoveConfirm, moveConfirmTemplate, confirmAction, null, title, summary, confirmLabel);
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
                case "guide":
                    OpenFirstRunGuide();
                    break;
                default:
                    return $"popupKind={popupKind}; supported=servo,reset,run,move,warning,recovery,unsaved,guide";
            }

            return GetDebugSummary();
        }

        private void OpenPopup(
            PopupKind popupKind,
            VisualTreeAsset template,
            System.Action confirmAction,
            System.Action cancelAction = null,
            string titleOverride = null,
            string summaryOverride = null,
            string confirmLabelOverride = null)
        {
            if (template == null || popupTemplateHost == null)
            {
                return;
            }

            pendingConfirmAction = confirmAction;
            pendingCancelAction = cancelAction;
            currentPopupKind = popupKind;
            popupTemplateHost.Clear();
            var tree = template.CloneTree();
            popupTemplateHost.Add(tree);
            ApplyTemplateCopy(tree, titleOverride, summaryOverride, confirmLabelOverride);
            SetFaultOverlaySuppressed(true);
            inputContract.OpenPopupProbeForDebug();
            PopupStateChanged?.Invoke();
        }

        private void CloseActivePopup()
        {
            popupTemplateHost?.Clear();
            inputContract?.ClosePopupProbeForDebug();
            SetFaultOverlaySuppressed(false);
            pendingConfirmAction = null;
            pendingCancelAction = null;
            currentPopupKind = PopupKind.None;
            PopupStateChanged?.Invoke();
        }

        private void OnPopupCancelClicked(ClickEvent _)
        {
            pendingCancelAction?.Invoke();
            CloseActivePopup();
        }

        private void OnPopupConfirmClicked(ClickEvent _)
        {
            pendingConfirmAction?.Invoke();
            CloseActivePopup();
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

        private void ApplyTemplateCopy(VisualElement tree, string titleOverride, string summaryOverride, string confirmLabelOverride)
        {
            var metaTitle = tree.Q<Label>("PopupMetaTitle");
            var metaSummary = tree.Q<Label>("PopupMetaSummary");
            var metaConfirm = tree.Q<Label>("PopupMetaConfirm");
            var metaCancel = tree.Q<Label>("PopupMetaCancel");
            var metaDanger = tree.Q<Label>("PopupMetaDanger");

            popupCardTitle.text = string.IsNullOrWhiteSpace(titleOverride) ? metaTitle?.text ?? string.Empty : titleOverride;
            popupCardSummary.text = string.IsNullOrWhiteSpace(summaryOverride) ? metaSummary?.text ?? string.Empty : summaryOverride;
            popupCancelButton.text = metaCancel?.text ?? string.Empty;
            popupConfirmButton.text = string.IsNullOrWhiteSpace(confirmLabelOverride) ? metaConfirm?.text ?? string.Empty : confirmLabelOverride;

            var isDanger = bool.TryParse(metaDanger?.text, out var parsedDanger) && parsedDanger;
            popupConfirmButton.EnableInClassList("rc-popup-button--danger", isDanger);
            popupConfirmButton.EnableInClassList("rc-popup-button--primary", !isDanger);
        }

        private void ApplySyncPolicy()
        {
            connectionHomeController?.ApplySyncPolicy();
            PopupStateChanged?.Invoke();
        }

        private void TryOpenFirstRunGuide()
        {
            if (firstRunGuideChecked || !RobotControlEntryPolicy.ShouldShowFirstRunGuide())
            {
                return;
            }

            firstRunGuideChecked = true;
            RobotControlEntryPolicy.MarkFirstRunGuideShown();
            OpenFirstRunGuide();
        }
    }
}
