// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 표시 패널의 preview/shell snapshot 바인딩을 단일 진입점으로 묶습니다.
    /// </summary>
    [RequireComponent(typeof(ConnectionHomeController))]
    [RequireComponent(typeof(PendantV3ShellStateController))]
    public sealed class PendantV3Binder : MonoBehaviour
    {
        [SerializeField] private ConnectionHomeController connectionHomeController;
        [SerializeField] private PendantV3ShellStateController shellStateController;
        [SerializeField] private StatusCardController statusCardController;
        [SerializeField] private SafetyDiagnosticsController safetyDiagnosticsController;
        [SerializeField] private ViewportToolbarController viewportToolbarController;
        [SerializeField] private ViewportAuxInfoController viewportAuxInfoController;
        [SerializeField] private HelpPanelController helpPanelController;
        [SerializeField] private WhyItMovedController whyItMovedController;

        private bool isInitialized;
        private bool isInitializing;
        private bool subscriptionsBound;
        private Coroutine initializeCoroutine;
        private RobotControlV3RuntimeSnapshot lastPreviewDefinition;
        private PendantV3LocalState lastShellState;
        private bool isRefreshingPanels;
        private bool refreshQueued;

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
            UnbindSubscriptions();
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
            return $"initialized={isInitialized}; subscriptions={subscriptionsBound}; preview={connectionHomeController?.CurrentPreviewState.ToString() ?? "missing"}; nav={lastShellState.ActiveNavSection}; work={lastShellState.ActiveWorkTab}; tablet={lastShellState.ActiveTabletTab}";
        }

        public string RefreshFromSourcesForDebug()
        {
            if (!ForceInitialize())
            {
                return GetDebugSummary();
            }

            RefreshFromSources();
            return GetDebugSummary();
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
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            shellStateController ??= GetComponent<PendantV3ShellStateController>();
            statusCardController ??= GetComponent<StatusCardController>();
            safetyDiagnosticsController ??= GetComponent<SafetyDiagnosticsController>();
            viewportToolbarController ??= GetComponent<ViewportToolbarController>();
            viewportAuxInfoController ??= GetComponent<ViewportAuxInfoController>();
            helpPanelController ??= GetComponent<HelpPanelController>();
            whyItMovedController ??= GetComponent<WhyItMovedController>();

            try
            {
                if (connectionHomeController == null || shellStateController == null)
                {
                    return false;
                }

                var statusReady = statusCardController == null || statusCardController.ForceInitialize();
                var safetyReady = safetyDiagnosticsController == null || safetyDiagnosticsController.ForceInitialize();
                var toolbarReady = viewportToolbarController == null || viewportToolbarController.ForceInitialize();
                var auxReady = viewportAuxInfoController == null || viewportAuxInfoController.ForceInitialize();
                var helpReady = helpPanelController == null || helpPanelController.ForceInitialize();
                var whyReady = whyItMovedController == null || whyItMovedController.ForceInitialize();
                if (!statusReady || !safetyReady || !toolbarReady || !auxReady || !helpReady || !whyReady)
                {
                    isInitialized = false;
                    return false;
                }

                BindSubscriptions();
                isInitialized = true;
                RefreshFromSources();
                return true;
            }
            finally
            {
                isInitializing = false;
            }
        }

        private void BindSubscriptions()
        {
            if (subscriptionsBound)
            {
                return;
            }

            connectionHomeController.PreviewChanged += HandlePreviewChanged;
            shellStateController.StateSnapshotChanged += HandleShellStateChanged;
            subscriptionsBound = true;
        }

        private void UnbindSubscriptions()
        {
            if (!subscriptionsBound)
            {
                return;
            }

            if (connectionHomeController != null)
            {
                connectionHomeController.PreviewChanged -= HandlePreviewChanged;
            }

            if (shellStateController != null)
            {
                shellStateController.StateSnapshotChanged -= HandleShellStateChanged;
            }

            subscriptionsBound = false;
        }

        private void HandlePreviewChanged(RobotControlV3RuntimeSnapshot data)
        {
            lastPreviewDefinition = data?.Clone();
            lastShellState = shellStateController != null
                ? shellStateController.GetStateSnapshot()
                : lastShellState;
            RefreshDisplayPanels(lastPreviewDefinition, lastShellState);
        }

        private void HandleShellStateChanged(PendantV3LocalState shellState)
        {
            lastShellState = PendantV3LocalState.Normalize(shellState);
            lastPreviewDefinition = connectionHomeController.CurrentPreviewDefinition?.Clone();
            RefreshDisplayPanels(lastPreviewDefinition, lastShellState);
        }

        private void RefreshFromSources()
        {
            lastPreviewDefinition = connectionHomeController.CurrentPreviewDefinition?.Clone();
            lastShellState = shellStateController.GetStateSnapshot();
            RefreshDisplayPanels(lastPreviewDefinition, lastShellState);
        }

        private void RefreshDisplayPanels(RobotControlV3RuntimeSnapshot previewDefinition, PendantV3LocalState shellState)
        {
            if (previewDefinition == null)
            {
                return;
            }

            if (isRefreshingPanels)
            {
                refreshQueued = true;
                lastPreviewDefinition = previewDefinition.Clone();
                lastShellState = shellState;
                return;
            }

            isRefreshingPanels = true;
            try
            {
                do
                {
                    refreshQueued = false;
                    var currentPreview = lastPreviewDefinition?.Clone() ?? previewDefinition.Clone();
                    var currentShellState = lastShellState;

                    statusCardController?.RefreshFromBinder(currentPreview);
                    safetyDiagnosticsController?.RefreshFromBinder(currentPreview);
                    viewportToolbarController?.RefreshFromBinder(currentPreview);
                    viewportAuxInfoController?.RefreshFromBinder(currentPreview, currentShellState);
                    helpPanelController?.RefreshFromBinder(currentPreview, currentShellState);
                    whyItMovedController?.RefreshFromBinder(currentPreview);
                }
                while (refreshQueued);
            }
            finally
            {
                isRefreshingPanels = false;
            }
        }
    }
}
