// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections;
using System.Collections.Generic;
using KineTutor3D.App;
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
        private VisualElement workPanel;
        private VisualElement viewportHost;
        private VisualElement splitHandle;
        private VisualElement bottomSheet;
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
        private Button undoButton;
        private Button redoButton;
        private Slider speedSlider;
        private ConnectionHomeController connectionHomeController;
        private EasyMotionController easyMotionController;
        private JointJogController jointJogController;
        private TcpJogController tcpJogController;
        private PointMoveController pointMoveController;
        private readonly Stack<PendantV3LocalState> undoHistory = new();
        private readonly Stack<PendantV3LocalState> redoHistory = new();

        private Coroutine saveCoroutine;
        private bool hasPendingSave;
        private PendantV3LocalState state;
        private int dragPointerId = -1;
        private float dragStartX;
        private float dragStartRatio;
        private PendantV3LocalState dragStartState;
        private bool isApplyingHistory;

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
            return $"{normalized.ToDebugSummary()}; undo={undoHistory.Count}; redo={redoHistory.Count}";
        }

        public PendantV3LocalState GetStateSnapshot()
        {
            return PendantV3LocalState.DeepCopy(state);
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

            mainSplit = root.Q<VisualElement>("MainSplit");
            workPanel = root.Q<VisualElement>("WorkPanel");
            viewportHost = root.Q<VisualElement>("ViewportHost");
            splitHandle = root.Q<VisualElement>("MainSplitHandle");
            bottomSheet = root.Q<VisualElement>("BottomSheet");
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
            undoButton = root.Q<Button>("BtnUndo");
            redoButton = root.Q<Button>("BtnRedo");
            speedSlider = root.Q<Slider>("SpeedSlider");
            connectionHomeController = GetComponent<ConnectionHomeController>();
            easyMotionController = GetComponent<EasyMotionController>();
            jointJogController = GetComponent<JointJogController>();
            tcpJogController = GetComponent<TcpJogController>();
            pointMoveController = GetComponent<PointMoveController>();
        }

        public void SetDebugSelection(string navSection, string workTab, string tabletTab)
        {
            var nextState = PendantV3LocalState.DeepCopy(state);
            nextState.ActiveNavSection = string.IsNullOrWhiteSpace(navSection) ? nextState.ActiveNavSection : navSection;
            nextState.ActiveWorkTab = string.IsNullOrWhiteSpace(workTab) ? nextState.ActiveWorkTab : workTab;
            nextState.ActiveTabletTab = string.IsNullOrWhiteSpace(tabletTab) ? nextState.ActiveTabletTab : tabletTab;
            CommitStateChange(nextState);
        }

        public void SetCoordSystemSelection(string coordSystem)
        {
            var nextState = PendantV3LocalState.DeepCopy(state);
            nextState.CoordSystem = string.IsNullOrWhiteSpace(coordSystem) ? nextState.CoordSystem : coordSystem;
            CommitStateChange(nextState);
        }

        public void UpdatePointMoveDraft(
            string pointName,
            string motionKind,
            float[] tcpDraftValues,
            float[] jointDraftValues,
            bool hasPointDraft)
        {
            var nextState = PendantV3LocalState.DeepCopy(state);
            nextState.PointName = pointName;
            nextState.PointMotionKind = motionKind;
            nextState.PointTcpDraftValues = tcpDraftValues;
            nextState.PointJointDraftValues = jointDraftValues;
            nextState.HasPointDraft = hasPointDraft;
            CommitStateChange(nextState);
        }

        private void BindListeners()
        {
            RegisterButtons(navButtons, OnNavClicked);
            RegisterButtons(workTabButtons, OnWorkTabClicked);
            RegisterButtons(bottomTabButtons, OnBottomTabClicked);
            coordSystemButton?.RegisterCallback<ClickEvent>(OnCoordSystemClicked);
            incrementButton?.RegisterCallback<ClickEvent>(OnIncrementClicked);
            sheetToggleButton?.RegisterCallback<ClickEvent>(OnSheetToggleClicked);
            undoButton?.RegisterCallback<ClickEvent>(OnUndoClicked);
            redoButton?.RegisterCallback<ClickEvent>(OnRedoClicked);
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
            undoButton?.UnregisterCallback<ClickEvent>(OnUndoClicked);
            redoButton?.UnregisterCallback<ClickEvent>(OnRedoClicked);
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

        private void OnNavClicked(ClickEvent evt)
        {
            if (evt.currentTarget is not Button button)
            {
                return;
            }

            var nextState = PendantV3LocalState.DeepCopy(state);
            nextState.ActiveNavSection = button.name;
            CommitStateChange(nextState);
        }

        private void OnWorkTabClicked(ClickEvent evt)
        {
            if (evt.currentTarget is not Button button)
            {
                return;
            }

            var nextState = PendantV3LocalState.DeepCopy(state);
            nextState.ActiveWorkTab = button.name;
            CommitStateChange(nextState);
        }

        private void OnBottomTabClicked(ClickEvent evt)
        {
            if (evt.currentTarget is not Button button)
            {
                return;
            }

            var nextState = PendantV3LocalState.DeepCopy(state);
            nextState.ActiveTabletTab = button.name;
            CommitStateChange(nextState);
        }

        private void OnCoordSystemClicked(ClickEvent evt)
        {
            var nextState = PendantV3LocalState.DeepCopy(state);
            nextState.CoordSystem = CoordSystems[(ResolveIndex(CoordSystems, state.CoordSystem) + 1) % CoordSystems.Length];
            CommitStateChange(nextState);
        }

        private void OnIncrementClicked(ClickEvent evt)
        {
            var nextState = PendantV3LocalState.DeepCopy(state);
            nextState.JogIncrement = Increments[(ResolveIndex(Increments, state.JogIncrement) + 1) % Increments.Length];
            CommitStateChange(nextState);
        }

        private void OnSheetToggleClicked(ClickEvent evt)
        {
            var nextState = PendantV3LocalState.DeepCopy(state);
            nextState.IsTabletSheetExpanded = !nextState.IsTabletSheetExpanded;
            CommitStateChange(nextState, emitSnapshot: false);
        }

        private void OnSpeedChanged(ChangeEvent<float> evt)
        {
            var nextState = PendantV3LocalState.DeepCopy(state);
            nextState.SpeedPercent = Mathf.RoundToInt(evt.newValue);
            CommitStateChange(nextState);
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
            dragStartState = PendantV3LocalState.DeepCopy(state);
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
            UpdateUndoRedoButtons();
        }

        private void OnSplitPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != dragPointerId)
            {
                return;
            }

            if (!PendantV3LocalState.AreEquivalent(dragStartState, state))
            {
                undoHistory.Push(PendantV3LocalState.DeepCopy(dragStartState));
                redoHistory.Clear();
                state = PendantV3LocalState.DeepCopy(state);
                QueueSave();
                EmitStateSnapshotChanged();
                UpdateUndoRedoButtons();
            }

            ReleaseSplitDrag();
        }

        private void OnSplitPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            ReleaseSplitDrag();
        }

        private void OnMainSplitGeometryChanged(GeometryChangedEvent evt)
        {
            ApplySplitRatio();
        }

        private void OnUndoClicked(ClickEvent evt)
        {
            UndoLastState();
        }

        private void OnRedoClicked(ClickEvent evt)
        {
            RedoLastState();
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

        private void CommitStateChange(PendantV3LocalState nextState, bool emitSnapshot = true)
        {
            var normalizedNext = PendantV3LocalState.DeepCopy(nextState);
            if (PendantV3LocalState.AreEquivalent(state, normalizedNext))
            {
                UpdateUndoRedoButtons();
                return;
            }

            if (!isApplyingHistory)
            {
                undoHistory.Push(PendantV3LocalState.DeepCopy(state));
                redoHistory.Clear();
            }

            state = normalizedNext;
            ApplyState();
            QueueSave();
            if (emitSnapshot)
            {
                EmitStateSnapshotChanged();
            }
            UpdateUndoRedoButtons();
        }

        private void UndoLastState()
        {
            if (undoHistory.Count == 0)
            {
                UpdateUndoRedoButtons();
                return;
            }

            isApplyingHistory = true;
            redoHistory.Push(PendantV3LocalState.DeepCopy(state));
            state = PendantV3LocalState.DeepCopy(undoHistory.Pop());
            isApplyingHistory = false;
            ApplyState();
            QueueSave();
            EmitStateSnapshotChanged();
            UpdateUndoRedoButtons();
        }

        private void RedoLastState()
        {
            if (redoHistory.Count == 0)
            {
                UpdateUndoRedoButtons();
                return;
            }

            isApplyingHistory = true;
            undoHistory.Push(PendantV3LocalState.DeepCopy(state));
            state = PendantV3LocalState.DeepCopy(redoHistory.Pop());
            isApplyingHistory = false;
            ApplyState();
            QueueSave();
            EmitStateSnapshotChanged();
            UpdateUndoRedoButtons();
        }

        private void UpdateUndoRedoButtons()
        {
            undoButton?.SetEnabled(undoHistory.Count > 0);
            redoButton?.SetEnabled(redoHistory.Count > 0);
        }

        private void EmitStateSnapshotChanged()
        {
            StateSnapshotChanged?.Invoke(PendantV3LocalState.Normalize(state));
        }

    }
}
