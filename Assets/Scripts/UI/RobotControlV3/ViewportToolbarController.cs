// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 2C-2 뷰포트 보조 UI(툴바/작업공간 경계/충돌 강조) 표시 scaffold를 관리합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ConnectionHomeController))]
    public sealed class ViewportToolbarController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset viewportToolbarTemplate;

        private VisualElement root;
        private VisualElement viewportHost;
        private VisualElement viewportToolbarHost;
        private ConnectionHomeController connectionHomeController;
        private PendantV3VisualizationOrchestrator visualizationOrchestrator;
        private ToolbarElements elements;
        private bool isInitialized;
        private bool showBaseFrame = true;
        private bool showToolFrame = true;
        private bool showTrail = true;
        private bool showGhost;
        private bool showWorkspaceBoundary;
        private bool showCollisionManual;
        private bool showCollisionAuto;
        private string collisionStatusText = string.Empty;
        private bool collisionWarning;
        private bool collisionDanger;
        private Coroutine initializeCoroutine;

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
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

        internal void RefreshFromBinder(PendantV3PreviewState.Definition data)
        {
            if (!isInitialized && !TryInitialize())
            {
                return;
            }

            ApplyPreview(data);
        }

        public string GetDebugSummary()
        {
            var collisionVisible = showCollisionManual || showCollisionAuto;
            var hostBoundary = viewportHost?.ClassListContains("rc-viewport-host--boundary") ?? false;
            var hostCollision = viewportHost?.ClassListContains("rc-viewport-host--collision") ?? false;
            return $"initialized={isInitialized}; base={showBaseFrame}; tool={showToolFrame}; trail={showTrail}; ghost={showGhost}; boundary={showWorkspaceBoundary}; collision={collisionVisible}; collisionAuto={showCollisionAuto}; hostBoundary={hostBoundary}; hostCollision={hostCollision}; status={collisionStatusText}";
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            visualizationOrchestrator ??= GetComponent<PendantV3VisualizationOrchestrator>();
            root = document?.rootVisualElement;
            if (root == null || viewportToolbarTemplate == null || connectionHomeController == null)
            {
                return false;
            }

            viewportHost = root.Q<VisualElement>("ViewportHost");
            viewportToolbarHost = root.Q<VisualElement>("ViewportToolbarHost");
            if (viewportHost == null || viewportToolbarHost == null)
            {
                isInitialized = false;
                return false;
            }

            if (elements == null || viewportToolbarHost.childCount == 0)
            {
                elements = CreateToolbar(viewportToolbarHost);
            }

            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            ApplyAll();
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

        private ToolbarElements CreateToolbar(VisualElement host)
        {
            host.Clear();
            var tree = viewportToolbarTemplate.CloneTree();
            host.Add(tree);
            var toolbar = new ToolbarElements(tree);
            RegisterClicks(toolbar);
            return toolbar;
        }

        private void RegisterClicks(ToolbarElements toolbar)
        {
            RegisterClick(toolbar.BtnBaseFrame, () => ToggleBaseFrame());
            RegisterClick(toolbar.BtnToolFrame, () => ToggleToolFrame());
            RegisterClick(toolbar.BtnTrail, () => ToggleTrail());
            RegisterClick(toolbar.BtnGhost, () => ToggleGhost());
            RegisterClick(toolbar.BtnBoundary, () => ToggleBoundary());
            RegisterClick(toolbar.BtnCollision, () => ToggleCollision());
            RegisterClick(toolbar.BtnCameraReset, OnCameraReset);
        }

        private static void RegisterClick(Button button, System.Action handler)
        {
            if (button == null || handler == null)
            {
                return;
            }

            button.RegisterCallback<ClickEvent>(_ => handler());
        }

        private void ToggleBaseFrame()
        {
            showBaseFrame = !showBaseFrame;
            ApplyAll();
        }

        private void ToggleToolFrame()
        {
            showToolFrame = !showToolFrame;
            ApplyAll();
        }

        private void ToggleTrail()
        {
            showTrail = !showTrail;
            ApplyAll();
        }

        private void ToggleGhost()
        {
            showGhost = !showGhost;
            ApplyAll();
        }

        private void ToggleBoundary()
        {
            showWorkspaceBoundary = !showWorkspaceBoundary;
            ApplyAll();
        }

        private void ToggleCollision()
        {
            if (showCollisionAuto)
            {
                return;
            }

            showCollisionManual = !showCollisionManual;
            ApplyAll();
        }

        private void OnCameraReset()
        {
            if (elements?.Hint != null)
            {
                elements.Hint.text = elements.CameraResetHintText?.text ?? string.Empty;
            }
        }

        private void ApplyPreview(PendantV3PreviewState.Definition data)
        {
            var preview = connectionHomeController.CurrentPreviewState;
            showCollisionAuto = preview == PendantV3PreviewState.Kind.Fault;
            collisionWarning = preview is PendantV3PreviewState.Kind.ConnectedUnsynced or PendantV3PreviewState.Kind.AutoReconnect;
            collisionDanger = showCollisionAuto;
            collisionStatusText = showCollisionAuto
                ? elements?.CollisionStatusDangerText?.text ?? string.Empty
                : collisionWarning
                    ? elements?.CollisionStatusWarningText?.text ?? string.Empty
                    : elements?.CollisionStatusSafeText?.text ?? string.Empty;

            if (showCollisionAuto)
            {
                showGhost = true;
            }

            if (elements?.Hint != null)
            {
                elements.Hint.text = data.ActionWhy;
            }

            ApplyAll();
        }

        private void ApplyAll()
        {
            if (elements == null)
            {
                return;
            }

            var collisionVisible = showCollisionManual || showCollisionAuto;
            SetButtonState(elements.BtnBaseFrame, showBaseFrame);
            SetButtonState(elements.BtnToolFrame, showToolFrame);
            SetButtonState(elements.BtnTrail, showTrail);
            SetButtonState(elements.BtnGhost, showGhost);
            SetButtonState(elements.BtnBoundary, showWorkspaceBoundary);
            SetButtonState(elements.BtnCollision, collisionVisible);

            elements.BtnBoundary.EnableInClassList("rc-workspace-toggle--active", showWorkspaceBoundary);
            elements.BtnCollision.EnableInClassList("rc-collision-toggle--active", collisionVisible);
            elements.BtnCollision.SetEnabled(!showCollisionAuto);

            elements.BtnBoundary.text = showWorkspaceBoundary
                ? elements.BoundaryButtonOnText?.text ?? string.Empty
                : elements.BoundaryButtonOffText?.text ?? string.Empty;
            elements.BtnCollision.text = collisionVisible
                ? elements.CollisionButtonOnText?.text ?? string.Empty
                : elements.CollisionButtonOffText?.text ?? string.Empty;

            elements.BoundaryStatus.text = showWorkspaceBoundary
                ? elements.BoundaryStatusOnText?.text ?? string.Empty
                : elements.BoundaryStatusOffText?.text ?? string.Empty;
            elements.CollisionStatus.text = collisionStatusText;
            elements.CollisionStatus.EnableInClassList("rc-viewport-toolbar-status-line--muted", !collisionWarning && !collisionDanger);
            elements.CollisionStatus.EnableInClassList("rc-viewport-toolbar-status-line--warning", collisionWarning && !collisionDanger);
            elements.CollisionStatus.EnableInClassList("rc-viewport-toolbar-status-line--danger", collisionDanger);

            viewportHost?.EnableInClassList("rc-viewport-host--boundary", showWorkspaceBoundary);
            viewportHost?.EnableInClassList("rc-viewport-host--collision", collisionVisible);
            visualizationOrchestrator?.SetToolbarState(showBaseFrame, showToolFrame, showTrail, showGhost, showWorkspaceBoundary, showCollisionManual);
        }

        private static void SetButtonState(Button button, bool active)
        {
            if (button == null)
            {
                return;
            }

            button.EnableInClassList("rc-viewport-toolbar-button--active", active);
        }

        private sealed class ToolbarElements
        {
            public ToolbarElements(VisualElement root)
            {
                BtnBaseFrame = root.Q<Button>("BtnViewportBaseFrame");
                BtnToolFrame = root.Q<Button>("BtnViewportToolFrame");
                BtnTrail = root.Q<Button>("BtnViewportTrail");
                BtnGhost = root.Q<Button>("BtnViewportGhost");
                BtnBoundary = root.Q<Button>("BtnViewportBoundary");
                BtnCollision = root.Q<Button>("BtnViewportCollision");
                BtnCameraReset = root.Q<Button>("BtnViewportCameraReset");
                BoundaryStatus = root.Q<Label>("ViewportBoundaryStatus");
                CollisionStatus = root.Q<Label>("ViewportCollisionStatus");
                Hint = root.Q<Label>("ViewportToolbarHint");
                BoundaryButtonOnText = root.Q<Label>("ViewportBoundaryButtonOnText");
                BoundaryButtonOffText = root.Q<Label>("ViewportBoundaryButtonOffText");
                BoundaryStatusOnText = root.Q<Label>("ViewportBoundaryStatusOnText");
                BoundaryStatusOffText = root.Q<Label>("ViewportBoundaryStatusOffText");
                CollisionButtonOnText = root.Q<Label>("ViewportCollisionButtonOnText");
                CollisionButtonOffText = root.Q<Label>("ViewportCollisionButtonOffText");
                CollisionStatusSafeText = root.Q<Label>("ViewportCollisionStatusSafeText");
                CollisionStatusWarningText = root.Q<Label>("ViewportCollisionStatusWarningText");
                CollisionStatusDangerText = root.Q<Label>("ViewportCollisionStatusDangerText");
                CameraResetHintText = root.Q<Label>("ViewportCameraResetHintText");
            }

            public Button BtnBaseFrame { get; }
            public Button BtnToolFrame { get; }
            public Button BtnTrail { get; }
            public Button BtnGhost { get; }
            public Button BtnBoundary { get; }
            public Button BtnCollision { get; }
            public Button BtnCameraReset { get; }
            public Label BoundaryStatus { get; }
            public Label CollisionStatus { get; }
            public Label Hint { get; }
            public Label BoundaryButtonOnText { get; }
            public Label BoundaryButtonOffText { get; }
            public Label BoundaryStatusOnText { get; }
            public Label BoundaryStatusOffText { get; }
            public Label CollisionButtonOnText { get; }
            public Label CollisionButtonOffText { get; }
            public Label CollisionStatusSafeText { get; }
            public Label CollisionStatusWarningText { get; }
            public Label CollisionStatusDangerText { get; }
            public Label CameraResetHintText { get; }
        }
    }
}
