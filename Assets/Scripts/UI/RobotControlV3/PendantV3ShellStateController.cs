// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections;
using System.Collections.Generic;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 셸의 탭/레이아웃 로컬 상태를 유지하고 저장합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed partial class PendantV3ShellStateController : MonoBehaviour
    {
        private static readonly string[] CoordSystems = { "Base", "Tool", "User" };
        private static readonly int[] Increments = { 1, 5, 10 };

        [SerializeField] private UIDocument document;

        internal event System.Action<PendantV3LocalState> StateSnapshotChanged;

        private readonly List<Button> navButtons = new();
        private readonly List<Button> workTabButtons = new();
        private readonly List<Button> bottomTabButtons = new();

        private VisualElement root;
        private VisualElement mainSplit;
        private VisualElement workTabBar;
        private VisualElement workPanel;
        private VisualElement viewportHost;
        private ScrollView viewportPanelScroll;
        private Label workPanelDebugBadge;
        private VisualElement splitHandle;
        private VisualElement bottomSheet;
        private VisualElement bottomTabBar;
        private VisualElement bottomSheetContent;
        private Label speedLabel;
        private Label coordSystemLabel;
        private Label workPanelTitle;
        private Label workPanelSummary;
        private Label bottomSheetTitle;
        private Label bottomSheetSummary;
        private Label speedValueLabel;
        private Button coordSystemButton;
        private Button incrementButton;
        private Button sheetToggleButton;
        private Button dryRunButton;
        private Button undoButton;
        private Button redoButton;
        private Button stepBackButton;
        private Button stepForwardButton;
        private Slider speedSlider;
        private ConnectionHomeController connectionHomeController;
        private RobotControlV3RuntimeController runtimeController;
        private HelpPanelController helpPanelController;
        private EasyMotionController easyMotionController;
        private JointJogController jointJogController;
        private TcpJogController tcpJogController;
        private PointMoveController pointMoveController;
        private IoPanelController ioPanelController;

        private Coroutine saveCoroutine;
        private bool hasPendingSave;
        private PendantV3LocalState state;
        private int dragPointerId = -1;
        private float dragStartX;
        private float dragStartRatio;

        private void OnEnable()
        {
            document ??= GetComponent<UIDocument>();
            root = document?.rootVisualElement;
            if (root == null)
            {
                return;
            }

            CacheElements();
            BindListeners();
            state = LocalSettingsStore.LoadOrDefault();
            ApplyState();
            EmitStateSnapshotChanged();
        }

        private void OnDisable()
        {
            UnbindListeners();
            if (saveCoroutine != null)
            {
                StopCoroutine(saveCoroutine);
                saveCoroutine = null;
            }

            if (hasPendingSave)
            {
                LocalSettingsStore.Save(state);
                hasPendingSave = false;
            }
        }

        public string GetDebugSummary()
        {
            var normalized = PendantV3LocalState.Normalize(state);
            var workPanelHighlighted = workPanel?.ClassListContains("rc-work-panel--debug-highlight") ?? false;
            var workPanelBadgeVisible = !(workPanelDebugBadge?.ClassListContains("rc-hidden") ?? true);
            return $"{normalized.ToDebugSummary()}; {BuildTabVisibilitySummary()}; workPanelHighlight={workPanelHighlighted}; workPanelBadgeVisible={workPanelBadgeVisible}";
        }

        public PendantV3LocalState GetStateSnapshot()
        {
            return PendantV3LocalState.Normalize(state);
        }

        private void CacheElements()
        {
            navButtons.Clear();
            workTabButtons.Clear();
            bottomTabButtons.Clear();

            AddButton(navButtons, "NavHome");
            AddButton(navButtons, "NavMotion");
            AddButton(navButtons, "NavPoints");
            AddButton(navButtons, "NavIo");
            AddButton(navButtons, "NavStatus");
            AddButton(navButtons, "NavHelp");
            AddButton(workTabButtons, "TabEasyMotion");
            AddButton(workTabButtons, "TabJointJog");
            AddButton(workTabButtons, "TabTcpJog");
            AddButton(workTabButtons, "TabPointMove");
            AddButton(bottomTabButtons, "BottomTabEasyMotion");
            AddButton(bottomTabButtons, "BottomTabJointJog");
            AddButton(bottomTabButtons, "BottomTabTcpJog");
            AddButton(bottomTabButtons, "BottomTabPointMove");
            AddButton(bottomTabButtons, "BottomTabIo");
            AddButton(bottomTabButtons, "BottomTabStatus");
            AddButton(bottomTabButtons, "BottomTabHelp");

            workTabBar = root.Q<VisualElement>("WorkTabBar");
            mainSplit = root.Q<VisualElement>("MainSplit");
            workPanel = root.Q<VisualElement>("WorkPanel");
            viewportHost = root.Q<VisualElement>("ViewportHost");
            viewportPanelScroll = root.Q<ScrollView>("ViewportPanelScroll");
            ConfigureSharedScrollViews();
            workPanelDebugBadge = root.Q<Label>("WorkPanelDebugBadge");
            splitHandle = root.Q<VisualElement>("MainSplitHandle");
            bottomSheet = root.Q<VisualElement>("BottomSheet");
            bottomTabBar = root.Q<VisualElement>("BottomTabBar");
            bottomSheetContent = root.Q<VisualElement>("BottomSheetContent");
            speedLabel = root.Q<Label>("SpeedLabel");
            coordSystemLabel = root.Q<Label>("CoordSystemLabel");
            workPanelTitle = root.Q<Label>("WorkPanelTitle");
            workPanelSummary = root.Q<Label>("WorkPanelSummary");
            bottomSheetTitle = root.Q<Label>("BottomSheetTitle");
            bottomSheetSummary = root.Q<Label>("BottomSheetSummary");
            speedValueLabel = root.Q<Label>("SpeedValueLabel");
            coordSystemButton = root.Q<Button>("BtnCoordSystem");
            incrementButton = root.Q<Button>("BtnIncrement");
            sheetToggleButton = root.Q<Button>("BtnSheetToggle");
            dryRunButton = root.Q<Button>("BtnDryRun");
            undoButton = root.Q<Button>("BtnUndo");
            redoButton = root.Q<Button>("BtnRedo");
            stepBackButton = root.Q<Button>("BtnStepBack");
            stepForwardButton = root.Q<Button>("BtnStepForward");
            speedSlider = root.Q<Slider>("SpeedSlider");
            connectionHomeController = GetComponent<ConnectionHomeController>();
            runtimeController = GetComponent<RobotControlV3RuntimeController>();
            helpPanelController = GetComponent<HelpPanelController>();
            easyMotionController = GetComponent<EasyMotionController>();
            jointJogController = GetComponent<JointJogController>();
            tcpJogController = GetComponent<TcpJogController>();
            pointMoveController = GetComponent<PointMoveController>();
            ioPanelController = GetComponent<IoPanelController>();
        }

        private void ConfigureSharedScrollViews()
        {
            root.Query<ScrollView>(className: "rc-shared-scroll").ForEach(scrollView =>
            {
                scrollView.mode = ScrollViewMode.Vertical;
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
                scrollView.style.overflow = Overflow.Hidden;
                scrollView.contentContainer.style.minWidth = 0;
                scrollView.contentContainer.style.maxWidth = Length.Percent(100);
            });
        }

        public void SetDebugSelection(string navSection, string workTab, string tabletTab)
        {
            state.ActiveNavSection = string.IsNullOrWhiteSpace(navSection) ? state.ActiveNavSection : navSection;
            state.ActiveWorkTab = string.IsNullOrWhiteSpace(workTab) ? state.ActiveWorkTab : workTab;
            state.ActiveTabletTab = string.IsNullOrWhiteSpace(tabletTab) ? state.ActiveTabletTab : tabletTab;
            state = PendantV3LocalState.Normalize(state);
            LocalSettingsStore.Save(state);
            ApplyState();
            EmitStateSnapshotChanged();
        }

        public void SetCoordSystemSelection(string coordSystem)
        {
            state.CoordSystem = string.IsNullOrWhiteSpace(coordSystem) ? state.CoordSystem : coordSystem;
            state = PendantV3LocalState.Normalize(state);
            ApplyCoordSystemState();
            runtimeController?.SetCoordSystem(state.CoordSystem);
            QueueSave();
            EmitStateSnapshotChanged();
        }

        private void BindListeners()
        {
            RegisterButtons(navButtons, OnNavClicked);
            RegisterButtons(workTabButtons, OnWorkTabClicked);
            RegisterButtons(bottomTabButtons, OnBottomTabClicked);
            coordSystemButton?.RegisterCallback<ClickEvent>(OnCoordSystemClicked);
            incrementButton?.RegisterCallback<ClickEvent>(OnIncrementClicked);
            sheetToggleButton?.RegisterCallback<ClickEvent>(OnSheetToggleClicked);
            dryRunButton?.RegisterCallback<ClickEvent>(OnDryRunClicked);
            undoButton?.RegisterCallback<ClickEvent>(OnUndoClicked);
            redoButton?.RegisterCallback<ClickEvent>(OnRedoClicked);
            stepBackButton?.RegisterCallback<ClickEvent>(OnStepBackClicked);
            stepForwardButton?.RegisterCallback<ClickEvent>(OnStepForwardClicked);
            speedSlider?.RegisterValueChangedCallback(OnSpeedChanged);
            splitHandle?.RegisterCallback<PointerDownEvent>(OnSplitPointerDown);
            splitHandle?.RegisterCallback<PointerMoveEvent>(OnSplitPointerMove);
            splitHandle?.RegisterCallback<PointerUpEvent>(OnSplitPointerUp);
            splitHandle?.RegisterCallback<PointerCaptureOutEvent>(OnSplitPointerCaptureOut);
            mainSplit?.RegisterCallback<GeometryChangedEvent>(OnMainSplitGeometryChanged);
        }

        private void UnbindListeners()
        {
            RegisterButtons(navButtons, OnNavClicked, register: false);
            RegisterButtons(workTabButtons, OnWorkTabClicked, register: false);
            RegisterButtons(bottomTabButtons, OnBottomTabClicked, register: false);
            coordSystemButton?.UnregisterCallback<ClickEvent>(OnCoordSystemClicked);
            incrementButton?.UnregisterCallback<ClickEvent>(OnIncrementClicked);
            sheetToggleButton?.UnregisterCallback<ClickEvent>(OnSheetToggleClicked);
            dryRunButton?.UnregisterCallback<ClickEvent>(OnDryRunClicked);
            undoButton?.UnregisterCallback<ClickEvent>(OnUndoClicked);
            redoButton?.UnregisterCallback<ClickEvent>(OnRedoClicked);
            stepBackButton?.UnregisterCallback<ClickEvent>(OnStepBackClicked);
            stepForwardButton?.UnregisterCallback<ClickEvent>(OnStepForwardClicked);
            speedSlider?.UnregisterValueChangedCallback(OnSpeedChanged);
            splitHandle?.UnregisterCallback<PointerDownEvent>(OnSplitPointerDown);
            splitHandle?.UnregisterCallback<PointerMoveEvent>(OnSplitPointerMove);
            splitHandle?.UnregisterCallback<PointerUpEvent>(OnSplitPointerUp);
            splitHandle?.UnregisterCallback<PointerCaptureOutEvent>(OnSplitPointerCaptureOut);
            mainSplit?.UnregisterCallback<GeometryChangedEvent>(OnMainSplitGeometryChanged);
        }

        private void AddButton(List<Button> target, string name)
        {
            var button = root.Q<Button>(name);
            if (button != null)
            {
                target.Add(button);
            }
        }

        private static void RegisterButtons(IEnumerable<Button> buttons, EventCallback<ClickEvent> callback, bool register = true)
        {
            foreach (var button in buttons)
            {
                if (register)
                {
                    button.RegisterCallback<ClickEvent>(callback);
                }
                else
                {
                    button.UnregisterCallback<ClickEvent>(callback);
                }
            }
        }

        private string BuildTabVisibilitySummary()
        {
            return $"workTabs={CountVisibleTabs(workTabButtons, workTabBar)}/{workTabButtons.Count}; workTabDetail=[{BuildTabDetail(workTabButtons, workTabBar)}]; bottomTabs={CountVisibleTabs(bottomTabButtons, bottomTabBar)}/{bottomTabButtons.Count}; bottomTabDetail=[{BuildTabDetail(bottomTabButtons, bottomTabBar)}]";
        }

        private static int CountVisibleTabs(IReadOnlyList<Button> buttons, VisualElement parent)
        {
            var count = 0;
            for (var index = 0; index < buttons.Count; index++)
            {
                if (IsTabVisible(buttons[index], parent))
                {
                    count++;
                }
            }

            return count;
        }

        private static string BuildTabDetail(IReadOnlyList<Button> buttons, VisualElement parent)
        {
            var parts = new string[buttons.Count];
            for (var index = 0; index < buttons.Count; index++)
            {
                var button = buttons[index];
                parts[index] = $"{button.name}:{(IsTabVisible(button, parent) ? "visible" : "hidden")}:clipped={IsTabClipped(button, parent)}";
            }

            return string.Join(",", parts);
        }

        private static bool IsTabVisible(Button button, VisualElement parent)
        {
            if (button == null || parent == null)
            {
                return false;
            }

            return button.resolvedStyle.display != DisplayStyle.None
                && button.resolvedStyle.visibility != Visibility.Hidden
                && parent.resolvedStyle.display != DisplayStyle.None
                && !IsTabClipped(button, parent);
        }

        private static bool IsTabClipped(Button button, VisualElement parent)
        {
            if (button == null || parent == null)
            {
                return true;
            }

            var buttonBounds = button.worldBound;
            var parentBounds = parent.worldBound;
            if (buttonBounds.width <= 1f || buttonBounds.height <= 1f || parentBounds.width <= 1f || parentBounds.height <= 1f)
            {
                return false;
            }

            const float tolerance = 0.5f;
            return buttonBounds.xMin < parentBounds.xMin - tolerance
                || buttonBounds.xMax > parentBounds.xMax + tolerance
                || buttonBounds.yMin < parentBounds.yMin - tolerance
                || buttonBounds.yMax > parentBounds.yMax + tolerance;
        }

        private void OnNavClicked(ClickEvent evt)
        {
            if (evt.currentTarget is not Button button)
            {
                return;
            }

            state.ActiveNavSection = button.name;
            ApplyNavState();
            QueueSave();
            EmitStateSnapshotChanged();
        }

        private void OnWorkTabClicked(ClickEvent evt)
        {
            if (evt.currentTarget is not Button button)
            {
                return;
            }

            state.ActiveWorkTab = button.name;
            ApplyWorkTabState();
            QueueSave();
            EmitStateSnapshotChanged();
        }

        private void OnBottomTabClicked(ClickEvent evt)
        {
            if (evt.currentTarget is not Button button)
            {
                return;
            }

            state.ActiveTabletTab = button.name;
            ApplyBottomTabState();
            QueueSave();
            EmitStateSnapshotChanged();
        }

        private void OnCoordSystemClicked(ClickEvent evt)
        {
            state.CoordSystem = CoordSystems[(ResolveIndex(CoordSystems, state.CoordSystem) + 1) % CoordSystems.Length];
            ApplyCoordSystemState();
            runtimeController?.SetCoordSystem(state.CoordSystem);
            QueueSave();
            EmitStateSnapshotChanged();
        }

        private void OnIncrementClicked(ClickEvent evt)
        {
            state.JogIncrement = Increments[(ResolveIndex(Increments, state.JogIncrement) + 1) % Increments.Length];
            ApplyIncrementState();
            QueueSave();
            EmitStateSnapshotChanged();
        }

        private void OnSheetToggleClicked(ClickEvent evt)
        {
            state.IsTabletSheetExpanded = !state.IsTabletSheetExpanded;
            ApplyBottomSheetState();
            QueueSave();
        }

        private void OnDryRunClicked(ClickEvent evt)
        {
            runtimeController?.ToggleDryRun();
            ApplyBottomBarState();
        }

        private void OnUndoClicked(ClickEvent evt)
        {
            runtimeController?.UndoPreview();
        }

        private void OnRedoClicked(ClickEvent evt)
        {
            runtimeController?.RedoPreview();
        }

        private void OnStepBackClicked(ClickEvent evt)
        {
            runtimeController?.StepBackward();
        }

        private void OnStepForwardClicked(ClickEvent evt)
        {
            runtimeController?.StepForward();
        }

        private void OnSpeedChanged(ChangeEvent<float> evt)
        {
            state.SpeedPercent = Mathf.RoundToInt(evt.newValue);
            ApplySpeedState();
            QueueSave();
            EmitStateSnapshotChanged();
        }

        private void OnSplitPointerDown(PointerDownEvent evt)
        {
            if (splitHandle == null)
            {
                return;
            }

            dragPointerId = evt.pointerId;
            dragStartX = evt.position.x;
            dragStartRatio = state.DesktopSplitRatio;
            splitHandle.CapturePointer(dragPointerId);
            splitHandle.EnableInClassList("rc-split-handle--dragging", true);
            evt.StopPropagation();
        }

        private void OnSplitPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != dragPointerId || mainSplit == null)
            {
                return;
            }

            var width = Mathf.Max(1f, mainSplit.worldBound.width);
            var deltaRatio = (evt.position.x - dragStartX) / width;
            state.DesktopSplitRatio = Mathf.Clamp(dragStartRatio + deltaRatio, PendantV3LocalState.MinSplitRatio, PendantV3LocalState.MaxSplitRatio);
            ApplySplitRatio();
        }

        private void OnSplitPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != dragPointerId)
            {
                return;
            }

            ReleaseSplitDrag();
            QueueSave();
        }

        private void OnSplitPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            ReleaseSplitDrag();
        }

        private void OnMainSplitGeometryChanged(GeometryChangedEvent evt)
        {
            ApplySplitRatio();
        }

        private void ReleaseSplitDrag()
        {
            if (splitHandle != null && dragPointerId >= 0 && splitHandle.HasPointerCapture(dragPointerId))
            {
                splitHandle.ReleasePointer(dragPointerId);
            }

            dragPointerId = -1;
            splitHandle?.EnableInClassList("rc-split-handle--dragging", false);
        }

        private void EmitStateSnapshotChanged()
        {
            StateSnapshotChanged?.Invoke(PendantV3LocalState.Normalize(state));
        }

    }
}
