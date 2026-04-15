// Folder: UI - HUD/view components only; no kinematics logic.
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
        private Button btnResetError;
        private Button btnRunBottom;
        private Button btnStopBottom;
        private Button btnStepBack;
        private Button btnFaultOverlayReset;
        private Button btnFaultOverlayClose;
        private bool restoreFaultOverlayAfterPopup;
        private bool isInitialized;
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
            return $"initialized={isInitialized}; popupOpen={popupOpen}; title={title}; confirm={confirmText}; templateChildren={hostChildren}";
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            inputContract ??= GetComponent<PendantV3InputContract>();
            root = document?.rootVisualElement;
            if (root == null
                || inputContract == null
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
                popupCancelButton.UnregisterCallback<ClickEvent>(OnPopupButtonClicked);
            }

            if (popupConfirmButton != null)
            {
                popupConfirmButton.UnregisterCallback<ClickEvent>(OnPopupButtonClicked);
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
            OpenPopup(servoConfirmTemplate);
        }

        private void OpenResetConfirm()
        {
            OpenPopup(resetConfirmTemplate);
        }

        private void OpenRunConfirm()
        {
            OpenPopup(runConfirmTemplate);
        }

        private void OpenMoveConfirm()
        {
            OpenPopup(moveConfirmTemplate);
        }

        private void OpenWarningDialog()
        {
            OpenPopup(warningDialogTemplate);
        }

        private void OpenRecoveryDialog()
        {
            OpenPopup(recoveryDialogTemplate);
        }

        private void OpenUnsavedConfirm()
        {
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
            SetFaultOverlaySuppressed(true);
            inputContract.OpenPopupProbeForDebug();
        }

        private void CloseActivePopup()
        {
            popupTemplateHost?.Clear();
            inputContract?.ClosePopupProbeForDebug();
            SetFaultOverlaySuppressed(false);
        }

        private void OnPopupButtonClicked(ClickEvent _)
        {
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
    }
}
