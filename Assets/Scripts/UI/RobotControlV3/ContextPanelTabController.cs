// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 오른쪽 컬럼을 상태/좌표 탭으로 분리합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ContextPanelTabController : MonoBehaviour
    {
        private enum ContextTabMode
        {
            Status,
            Coordinate
        }

        [SerializeField] private UIDocument document;

        private VisualElement root;
        private Button btnStatusTab;
        private Button btnCoordinateTab;
        private VisualElement coordStripHost;
        private VisualElement statusCardHost;
        private VisualElement safetyDiagnosticsHost;
        private VisualElement actionHintCard;
        private WhyItMovedController whyItMovedController;
        private SafetyDiagnosticsController safetyDiagnosticsController;
        private ContextTabMode activeMode = ContextTabMode.Status;
        private bool isInitialized;
        private Coroutine initializeCoroutine;
        private EventCallback<ClickEvent> statusTabCallback;
        private EventCallback<ClickEvent> coordinateTabCallback;

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
            UnbindListeners();
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
            var statusVisible = !(statusCardHost?.ClassListContains("rc-hidden") ?? true);
            var coordVisible = !(coordStripHost?.ClassListContains("rc-hidden") ?? true);
            var actionVisible = !(actionHintCard?.ClassListContains("rc-hidden") ?? true);
            return $"initialized={isInitialized}; mode={activeMode}; statusVisible={statusVisible}; coordVisible={coordVisible}; actionVisible={actionVisible}";
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
            document ??= GetComponent<UIDocument>();
            root = document?.rootVisualElement;
            if (root == null)
            {
                return false;
            }

            btnStatusTab = root.Q<Button>("BtnContextTabStatus");
            btnCoordinateTab = root.Q<Button>("BtnContextTabCoordinate");
            coordStripHost = root.Q<VisualElement>("CoordStripHost");
            statusCardHost = root.Q<VisualElement>("StatusCardHost");
            safetyDiagnosticsHost = root.Q<VisualElement>("SafetyDiagnosticsHost");
            actionHintCard = root.Q<VisualElement>("ActionHint");
            whyItMovedController ??= GetComponent<WhyItMovedController>();
            safetyDiagnosticsController ??= GetComponent<SafetyDiagnosticsController>();

            if (btnStatusTab == null || btnCoordinateTab == null || coordStripHost == null || statusCardHost == null || actionHintCard == null)
            {
                isInitialized = false;
                return false;
            }

            BindListeners();
            ApplyMode();
            isInitialized = true;
            return true;
        }

        private void BindListeners()
        {
            statusTabCallback ??= _ => SetMode(ContextTabMode.Status);
            coordinateTabCallback ??= _ => SetMode(ContextTabMode.Coordinate);
            btnStatusTab.UnregisterCallback(statusTabCallback);
            btnCoordinateTab.UnregisterCallback(coordinateTabCallback);
            btnStatusTab.RegisterCallback(statusTabCallback);
            btnCoordinateTab.RegisterCallback(coordinateTabCallback);
        }

        private void UnbindListeners()
        {
            if (btnStatusTab != null && statusTabCallback != null)
            {
                btnStatusTab.UnregisterCallback(statusTabCallback);
            }

            if (btnCoordinateTab != null && coordinateTabCallback != null)
            {
                btnCoordinateTab.UnregisterCallback(coordinateTabCallback);
            }
        }

        private void SetMode(ContextTabMode mode)
        {
            activeMode = mode;
            ApplyMode();
        }

        private void ApplyMode()
        {
            var isStatusMode = activeMode == ContextTabMode.Status;
            btnStatusTab?.EnableInClassList("rc-context-tab--active", isStatusMode);
            btnCoordinateTab?.EnableInClassList("rc-context-tab--active", !isStatusMode);

            statusCardHost?.EnableInClassList("rc-hidden", !isStatusMode);
            coordStripHost?.EnableInClassList("rc-hidden", isStatusMode);
            actionHintCard?.EnableInClassList("rc-hidden", !isStatusMode);
        }
    }
}
