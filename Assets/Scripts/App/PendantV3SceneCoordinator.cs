// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.App.Fairino;
using KineTutor3D.UI.RobotControlV3;
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// Pendant V3 씬 진입 시 document, binder, 모션/팝업 초기화 순서를 고정합니다.
    /// </summary>
    [DefaultExecutionOrder(-900)]
    [RequireComponent(typeof(PendantV3Document))]
    public sealed class PendantV3SceneCoordinator : MonoBehaviour
    {
        [SerializeField] private PendantV3Document document;
        [SerializeField] private PendantV3Binder binder;
        [SerializeField] private ContextPanelTabController contextPanelTabController;
        [SerializeField] private PendantV3ConnectionSessionAdapter connectionSessionAdapter;
        [SerializeField] private PendantV3VisualizationOrchestrator visualizationOrchestrator;
        [SerializeField] private Visualization.PendantV3VisualizationDriver visualizationDriver;
        [SerializeField] private ConnectionHomeController connectionHomeController;
        [SerializeField] private EasyMotionController easyMotionController;
        [SerializeField] private JointJogController jointJogController;
        [SerializeField] private TcpJogController tcpJogController;
        [SerializeField] private PointMoveController pointMoveController;
        [SerializeField] private PopupCoordinatorV3 popupCoordinator;

        private bool isBootstrapped;
        private Coroutine bootstrapCoroutine;

        private void OnEnable()
        {
            TryBootstrap();
            bootstrapCoroutine ??= StartCoroutine(BootstrapWhenReady());
        }

        private void OnDisable()
        {
            if (bootstrapCoroutine != null)
            {
                StopCoroutine(bootstrapCoroutine);
                bootstrapCoroutine = null;
            }

            isBootstrapped = false;
        }

        public bool ForceBootstrap()
        {
            return TryBootstrap();
        }

        public string GetDebugSummary()
        {
            return $"bootstrapped={isBootstrapped}; documentReady={document != null && document.IsReadyForSceneBootstrap()}; binder={(binder != null)}; contextTabs={(contextPanelTabController != null)}; session={(connectionSessionAdapter != null)}; viz={(visualizationOrchestrator != null && visualizationDriver != null)}; home={(connectionHomeController != null)}; motion={(easyMotionController != null && jointJogController != null && tcpJogController != null && pointMoveController != null)}; popup={(popupCoordinator != null)}";
        }

        private System.Collections.IEnumerator BootstrapWhenReady()
        {
            for (var frame = 0; frame < 30 && !isBootstrapped; frame++)
            {
                TryBootstrap();
                if (isBootstrapped)
                {
                    break;
                }

                yield return null;
            }

            bootstrapCoroutine = null;
        }

        private bool TryBootstrap()
        {
            document ??= GetComponent<PendantV3Document>();
            binder ??= GetComponent<PendantV3Binder>();
            contextPanelTabController ??= GetComponent<ContextPanelTabController>();
            connectionSessionAdapter ??= GetComponent<PendantV3ConnectionSessionAdapter>();
            visualizationOrchestrator ??= GetComponent<PendantV3VisualizationOrchestrator>();
            visualizationDriver ??= GetComponent<Visualization.PendantV3VisualizationDriver>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            easyMotionController ??= GetComponent<EasyMotionController>();
            jointJogController ??= GetComponent<JointJogController>();
            tcpJogController ??= GetComponent<TcpJogController>();
            pointMoveController ??= GetComponent<PointMoveController>();
            popupCoordinator ??= GetComponent<PopupCoordinatorV3>();
            if (contextPanelTabController == null)
            {
                contextPanelTabController = gameObject.AddComponent<ContextPanelTabController>();
            }

            if (document == null || !document.IsReadyForSceneBootstrap())
            {
                return false;
            }

            var homeReady = connectionHomeController == null || connectionHomeController.ForceInitialize();
            var sessionReady = connectionSessionAdapter == null || connectionSessionAdapter.ForceInitialize();
            var visualizationReady = visualizationOrchestrator == null || visualizationOrchestrator.ForceInitialize();
            var visualizationDriverReady = visualizationDriver == null || visualizationDriver.ForceInitialize();
            var binderReady = binder == null || binder.ForceInitialize();
            var contextTabsReady = contextPanelTabController == null || contextPanelTabController.ForceInitialize();
            var easyReady = easyMotionController == null || easyMotionController.ForceInitialize();
            var jointReady = jointJogController == null || jointJogController.ForceInitialize();
            var tcpReady = tcpJogController == null || tcpJogController.ForceInitialize();
            var pointReady = pointMoveController == null || pointMoveController.ForceInitialize();
            var popupReady = popupCoordinator == null || popupCoordinator.ForceInitialize();

            isBootstrapped = homeReady && sessionReady && visualizationReady && visualizationDriverReady && binderReady && contextTabsReady && easyReady && jointReady && tcpReady && pointReady && popupReady;
            return isBootstrapped;
        }
    }
}
