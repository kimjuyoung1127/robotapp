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
        private VisualElement viewportToolbarHost;
        private VisualElement coordStripHost;
        private VisualElement statusCardHost;
        private VisualElement safetyDiagnosticsHost;
        private VisualElement actionHintCard;
        private VisualElement whyItMovedCard;
        private ContextTabMode activeMode = ContextTabMode.Status;
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
            UnbindListeners();
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
            var statusVisible = !(statusCardHost?.ClassListContains("rc-hidden") ?? true);
            var coordVisible = !(coordStripHost?.ClassListContains("rc-hidden") ?? true);
            var toolbarVisible = !(viewportToolbarHost?.ClassListContains("rc-hidden") ?? true);
            var safetyVisible = !(safetyDiagnosticsHost?.ClassListContains("rc-hidden") ?? true);
            var actionVisible = !(actionHintCard?.ClassListContains("rc-hidden") ?? true);
            var whyVisible = !(whyItMovedCard?.ClassListContains("rc-hidden") ?? true);
            return $"initialized={isInitialized}; mode={activeMode}; statusVisible={statusVisible}; coordVisible={coordVisible}; toolbarVisible={toolbarVisible}; safetyVisible={safetyVisible}; actionVisible={actionVisible}; whyVisible={whyVisible}";
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
            try
            {
            document ??= GetComponent<UIDocument>();
            root = document?.rootVisualElement;
            if (root == null)
            {
                return false;
            }

            btnStatusTab = root.Q<Button>("BtnContextTabStatus");
            btnCoordinateTab = root.Q<Button>("BtnContextTabCoordinate");
            viewportToolbarHost = root.Q<VisualElement>("ViewportToolbarHost");
            coordStripHost = root.Q<VisualElement>("CoordStripHost");
            statusCardHost = root.Q<VisualElement>("StatusCardHost");
            safetyDiagnosticsHost = root.Q<VisualElement>("SafetyDiagnosticsHost");
            actionHintCard = root.Q<VisualElement>("ActionHint");
            whyItMovedCard = root.Q<VisualElement>("WhyItMoved");

            if (btnStatusTab == null
                || btnCoordinateTab == null
                || viewportToolbarHost == null
                || coordStripHost == null
                || statusCardHost == null
                || safetyDiagnosticsHost == null
                || actionHintCard == null
                || whyItMovedCard == null)
            {
                isInitialized = false;
                return false;
            }

            BindListeners();
            ApplyMode();
            isInitialized = true;
            return true;
            }
            finally
            {
                isInitializing = false;
            }
        }

        private void BindListeners()
        {
            btnStatusTab.clicked -= HandleStatusTabClicked;
            btnCoordinateTab.clicked -= HandleCoordinateTabClicked;
            btnStatusTab.clicked += HandleStatusTabClicked;
            btnCoordinateTab.clicked += HandleCoordinateTabClicked;
        }

        private void UnbindListeners()
        {
            if (btnStatusTab != null)
            {
                btnStatusTab.clicked -= HandleStatusTabClicked;
            }

            if (btnCoordinateTab != null)
            {
                btnCoordinateTab.clicked -= HandleCoordinateTabClicked;
            }
        }

        private void HandleStatusTabClicked()
        {
            SetMode(ContextTabMode.Status);
        }

        private void HandleCoordinateTabClicked()
        {
            SetMode(ContextTabMode.Coordinate);
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

            viewportToolbarHost?.EnableInClassList("rc-hidden", isStatusMode);
            statusCardHost?.EnableInClassList("rc-hidden", !isStatusMode);
            coordStripHost?.EnableInClassList("rc-hidden", isStatusMode);
            safetyDiagnosticsHost?.EnableInClassList("rc-hidden", !isStatusMode);
            actionHintCard?.EnableInClassList("rc-hidden", !isStatusMode);
            whyItMovedCard?.EnableInClassList("rc-hidden", !isStatusMode);
        }
    }
}
