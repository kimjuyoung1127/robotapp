// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI.RobotControlV3;
using KineTutor3D.App.Fairino;
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
        [SerializeField] private ConnectionHomeController connectionHomeController;
        [SerializeField] private RobotControlV3RuntimeController runtimeController;
        [SerializeField] private RobotStageRenderSurface robotStageRenderSurface;
        [SerializeField] private RobotStageOrientationGizmoController robotStageOrientationGizmoController;
        [SerializeField] private ViewportAuxInfoController viewportAuxInfoController;
        [SerializeField] private ContextPanelTabController contextPanelTabController;
        [SerializeField] private EasyMotionController easyMotionController;
        [SerializeField] private JointJogController jointJogController;
        [SerializeField] private TcpJogController tcpJogController;
        [SerializeField] private PointMoveController pointMoveController;
        [SerializeField] private IoPanelController ioPanelController;
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
            return $"bootstrapped={isBootstrapped}; documentReady={document != null && document.IsReadyForSceneBootstrap()}; binder={(binder != null)}; home={(connectionHomeController != null)}; contextTabs={(contextPanelTabController != null)}; motion={(easyMotionController != null && jointJogController != null && tcpJogController != null && pointMoveController != null)}; io={(ioPanelController != null)}; popup={(popupCoordinator != null)}";
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
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>() ?? gameObject.AddComponent<RobotControlV3RuntimeController>();
            robotStageRenderSurface ??= GetComponent<RobotStageRenderSurface>() ?? gameObject.AddComponent<RobotStageRenderSurface>();
            robotStageOrientationGizmoController ??= GetComponent<RobotStageOrientationGizmoController>() ?? gameObject.AddComponent<RobotStageOrientationGizmoController>();
            viewportAuxInfoController ??= GetComponent<ViewportAuxInfoController>() ?? gameObject.AddComponent<ViewportAuxInfoController>();
            contextPanelTabController ??= GetComponent<ContextPanelTabController>() ?? gameObject.AddComponent<ContextPanelTabController>();
            easyMotionController ??= GetComponent<EasyMotionController>();
            jointJogController ??= GetComponent<JointJogController>();
            tcpJogController ??= GetComponent<TcpJogController>();
            pointMoveController ??= GetComponent<PointMoveController>();
            ioPanelController ??= GetComponent<IoPanelController>() ?? gameObject.AddComponent<IoPanelController>();
            popupCoordinator ??= GetComponent<PopupCoordinatorV3>();

            if (document == null || !document.IsReadyForSceneBootstrap())
            {
                return false;
            }

            var runtimeReady = true;
            var renderReady = robotStageRenderSurface == null || robotStageRenderSurface.ForceInitialize();
            var orientationReady = robotStageOrientationGizmoController == null || robotStageOrientationGizmoController.ForceInitialize();
            var auxInfoReady = viewportAuxInfoController == null || viewportAuxInfoController.ForceInitialize();
            var contextTabsReady = contextPanelTabController == null || contextPanelTabController.ForceInitialize();
            var homeReady = connectionHomeController == null || connectionHomeController.ForceInitialize();
            var binderReady = binder == null || binder.ForceInitialize();
            var easyReady = easyMotionController == null || easyMotionController.ForceInitialize();
            var jointReady = jointJogController == null || jointJogController.ForceInitialize();
            var tcpReady = tcpJogController == null || tcpJogController.ForceInitialize();
            var pointReady = pointMoveController == null || pointMoveController.ForceInitialize();
            var ioReady = ioPanelController == null || ioPanelController.ForceInitialize();
            var popupReady = popupCoordinator == null || popupCoordinator.ForceInitialize();

            isBootstrapped = runtimeReady && renderReady && orientationReady && auxInfoReady && contextTabsReady && homeReady && binderReady && easyReady && jointReady && tcpReady && pointReady && ioReady && popupReady;
            return isBootstrapped;
        }
    }
}
