// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 쉬운 조작 패널 시안을 desktop/tablet host에 주입합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ConnectionHomeController))]
    public sealed class EasyMotionController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset easyMotionTemplate;

        private VisualElement root;
        private VisualElement workPanelBody;
        private VisualElement bottomSheetBody;
        private VisualElement easyMotionPanelHost;
        private VisualElement easyMotionSheetHost;
        private ConnectionHomeController connectionHomeController;
        private RobotControlV3RuntimeController runtimeController;
        private PopupCoordinatorV3 popupCoordinator;
        private EasyMotionGripperLiveControl gripperControl;
        private string selectedPresetName = "Ready";

        private PanelElements desktopPanel;
        private PanelElements tabletPanel;
        private bool isDesktopVisible;
        private bool isTabletVisible;
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
            if (isInitialized && connectionHomeController != null)
            {
                connectionHomeController.PreviewChanged -= ApplyPreview;
            }

            if (initializeCoroutine != null)
            {
                StopCoroutine(initializeCoroutine);
                initializeCoroutine = null;
            }

            isInitialized = false;
            isInitializing = false;
        }

        public string GetDebugSummary()
        {
            var liveRoot = document != null ? document.rootVisualElement : null;
            var livePanelHost = liveRoot?.Q<VisualElement>("EasyMotionPanelHost");
            var liveSheetHost = liveRoot?.Q<VisualElement>("EasyMotionSheetHost");
            var panelChildren = easyMotionPanelHost?.childCount ?? -1;
            var sheetChildren = easyMotionSheetHost?.childCount ?? -1;
            var panelHidden = easyMotionPanelHost?.ClassListContains("rc-hidden") ?? false;
            var sheetHidden = easyMotionSheetHost?.ClassListContains("rc-hidden") ?? false;
            var templateName = easyMotionTemplate != null ? easyMotionTemplate.name : "null";
            return $"initialized={isInitialized}; template={templateName}; document={(document != null)}; liveRoot={(liveRoot != null)}; livePanelHost={(livePanelHost != null)}; liveSheetHost={(liveSheetHost != null)}; panelHost={(easyMotionPanelHost != null)}; panelChildren={panelChildren}; panelHidden={panelHidden}; sheetHost={(easyMotionSheetHost != null)}; sheetChildren={sheetChildren}; sheetHidden={sheetHidden}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}";
        }

        public void SetShellState(string activeNavSection, string activeWorkTab, string activeTabletTab)
        {
            isDesktopVisible = activeNavSection == "NavMotion" && activeWorkTab == "TabEasyMotion";
            isTabletVisible = activeNavSection == "NavMotion" && activeTabletTab == "BottomTabEasyMotion";
            if (!isInitialized)
            {
                TryInitialize();
            }

            ApplyVisibility();
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string SetGripperSliderForDebug(int positionPercent)
        {
            if (!TryInitialize())
            {
                return "Easy Motion 초기화 실패";
            }

            var summary = gripperControl?.SetDraftForDebug(positionPercent, apply: true) ?? "Easy Motion 그리퍼 조작부를 찾지 못했다";
            return $"legacySlider=removed; {summary}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}";
        }

        public string SetGripperInputForDebug(float positionPercent, bool apply = false)
        {
            if (!TryInitialize())
            {
                return "Easy Motion 초기화 실패";
            }

            var summary = gripperControl?.SetDraftForDebug(positionPercent, apply) ?? "Easy Motion 그리퍼 입력창을 찾지 못했다";
            return $"{summary}; applied={apply}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}";
        }

        public string GetGripperInputStateForDebug()
        {
            if (!TryInitialize())
            {
                return "Easy Motion 초기화 실패";
            }

            var summary = gripperControl?.GetDebugState() ?? "Easy Motion 그리퍼 입력 상태를 찾지 못했다";
            return $"{summary}; desktopVisible={isDesktopVisible}; tabletVisible={isTabletVisible}";
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
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            popupCoordinator ??= GetComponent<PopupCoordinatorV3>();
            root = document?.rootVisualElement;
            if (root == null || easyMotionTemplate == null || connectionHomeController == null || runtimeController == null)
            {
                return false;
            }

            gripperControl ??= new EasyMotionGripperLiveControl(runtimeController, popupCoordinator);
            CacheShellElements();
            if (easyMotionPanelHost == null || easyMotionSheetHost == null)
            {
                isInitialized = false;
                return false;
            }

            if (desktopPanel == null || tabletPanel == null || easyMotionPanelHost.childCount == 0 || easyMotionSheetHost.childCount == 0)
            {
                BuildPanels();
            }

            ApplyShellStateSnapshot();
            isInitialized = true;
            connectionHomeController.PreviewChanged -= ApplyPreview;
            connectionHomeController.PreviewChanged += ApplyPreview;
            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            ApplyVisibility();
            return true;
            }
            finally
            {
                isInitializing = false;
            }
        }

        private void ApplyShellStateSnapshot()
        {
            var shellStateController = GetComponent<PendantV3ShellStateController>();
            var localState = shellStateController != null
                ? shellStateController.GetStateSnapshot()
                : PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault());
            isDesktopVisible = localState.ActiveNavSection == "NavMotion" && localState.ActiveWorkTab == "TabEasyMotion";
            isTabletVisible = localState.ActiveNavSection == "NavMotion" && localState.ActiveTabletTab == "BottomTabEasyMotion";
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

        private void CacheShellElements()
        {
            workPanelBody = root.Q<VisualElement>("WorkPanelBody");
            bottomSheetBody = root.Q<VisualElement>("BottomSheetBody");
            easyMotionPanelHost = root.Q<VisualElement>("EasyMotionPanelHost");
            easyMotionSheetHost = root.Q<VisualElement>("EasyMotionSheetHost");
        }

        private void BuildPanels()
        {
            desktopPanel = CreatePanel(easyMotionPanelHost);
            tabletPanel = CreatePanel(easyMotionSheetHost);
        }

        private PanelElements CreatePanel(VisualElement host)
        {
            if (host == null)
            {
                return null;
            }

            host.Clear();
            var tree = easyMotionTemplate.CloneTree();
            host.Add(tree);
            var panel = new PanelElements(tree, gripperControl);
            RegisterPanel(panel);
            return panel;
        }

        private void RegisterPanel(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            RegisterClick(panel.BtnEasyHome, () => SelectPresetAndPreview("Home"));
            RegisterClick(panel.BtnEasyReady, () => SelectPresetAndPreview("Ready"));
            RegisterClick(panel.BtnEasyFolded, () => SelectPresetAndPreview("Folded"));
            RegisterClick(panel.BtnEasyZero, () => SelectPresetAndPreview("Zero"));
            RegisterClick(panel.BtnEasyPreview, PreviewSelectedPreset);
            RegisterClick(panel.BtnEasyApply, ApplySelectedPreset);
        }

        private static void RegisterClick(Button button, System.Action handler)
        {
            if (button == null || handler == null)
            {
                return;
            }

            button.clicked += handler;
        }

        private void ApplyPreview(RobotControlV3RuntimeSnapshot data)
        {
            gripperControl?.UpdateSnapshot(data);
            ApplyPanel(desktopPanel, data, connectionHomeController.CurrentPreviewState);
            ApplyPanel(tabletPanel, data, connectionHomeController.CurrentPreviewState);
        }

        private void ApplyPanel(PanelElements panel, RobotControlV3RuntimeSnapshot data, PendantV3PreviewState.Kind state)
        {
            if (panel == null)
            {
                return;
            }

            panel.EasyStateSummary.text = $"{data.StatusConnection} · {data.StatusTool} · {data.StatusUser}";
            var liveGripperWriteEnabled = !data.DryRunEnabled
                && data.MotionGateReady
                && data.CurrentSessionMode is "live-control" or "gripper-only";
            panel.EasyModeBadge.text = liveGripperWriteEnabled
                ? "그리퍼 live 가능"
                : data.CurrentSessionMode == "readback-only"
                    ? "그리퍼 읽기 전용"
                    : "화면 프리뷰";
            panel.EasyDryRunLabel.text = liveGripperWriteEnabled
                ? $"실기 쓰기 가능 · {data.GripperSummary}"
                : data.DryRunEnabled
                    ? $"미리보기 전용 · {data.GripperSummary}"
                    : $"읽기 전용 · {data.GripperSummary}";
            panel.EasyActionHint.text = liveGripperWriteEnabled
                ? $"{data.ActionWhy} / {data.PeripheralFeedback}"
                : data.DryRunEnabled
                    ? "현재는 화면 프리뷰만 갱신한다. 실제 그리퍼 쓰기는 열리지 않았다."
                    : "현재 세션은 읽기 전용이라 입력값만 유지한다. 실제 그리퍼 쓰기는 잠겨 있다.";

            var canPreview = state != PendantV3PreviewState.Kind.AutoReconnect;
            var canApply = data.DryRunEnabled || state == PendantV3PreviewState.Kind.ReadyToJog;
            var canGrip = data.StatusKind is RobotControlV3RuntimeStatusKind.ConnectedServoOff
                or RobotControlV3RuntimeStatusKind.ConnectedUnsynced
                or RobotControlV3RuntimeStatusKind.ReadyToJog
                or RobotControlV3RuntimeStatusKind.Fault;
            var canPreset = canPreview;

            panel.BtnEasyHome.SetEnabled(canPreset);
            panel.BtnEasyReady.SetEnabled(canPreset);
            panel.BtnEasyFolded.SetEnabled(canPreset);
            panel.BtnEasyZero.SetEnabled(canPreset);
            panel.BtnEasyPreview.SetEnabled(canPreview);
            panel.BtnEasyApply.SetEnabled(canApply);
            gripperControl?.RefreshView(panel.GripperView, canGrip);
        }

        private void SelectPresetAndPreview(string presetName)
        {
            selectedPresetName = string.IsNullOrWhiteSpace(presetName) ? "Ready" : presetName;
            runtimeController?.PreviewPreset(selectedPresetName);
        }

        private void PreviewSelectedPreset()
        {
            runtimeController?.PreviewPreset(selectedPresetName);
        }

        private void ApplySelectedPreset()
        {
            runtimeController?.ApplyPreset(selectedPresetName);
        }

        private void ApplyVisibility()
        {
            if (isDesktopVisible)
            {
                workPanelBody?.EnableInClassList("rc-hidden", false);
            }

            if (isTabletVisible)
            {
                bottomSheetBody?.EnableInClassList("rc-hidden", false);
            }

            easyMotionPanelHost?.EnableInClassList("rc-hidden", !isDesktopVisible);
            easyMotionSheetHost?.EnableInClassList("rc-hidden", !isTabletVisible);
        }

        private sealed class PanelElements
        {
            public PanelElements(VisualElement root, EasyMotionGripperLiveControl liveControl)
            {
                EasyModeBadge = root.Q<Label>("EasyModeBadge");
                EasyStateSummary = root.Q<Label>("EasyStateSummary");
                EasyDryRunLabel = root.Q<Label>("EasyDryRunLabel");
                EasyActionHint = root.Q<Label>("EasyActionHint");
                EasyGripperControlHost = root.Q<VisualElement>("EasyGripperControlHost");
                BtnEasyHome = root.Q<Button>("BtnEasyHome");
                BtnEasyReady = root.Q<Button>("BtnEasyReady");
                BtnEasyFolded = root.Q<Button>("BtnEasyFolded");
                BtnEasyZero = root.Q<Button>("BtnEasyZero");
                BtnEasyPreview = root.Q<Button>("BtnEasyPreview");
                BtnEasyApply = root.Q<Button>("BtnEasyApply");
                GripperView = liveControl?.Attach(EasyGripperControlHost);
            }

            public Label EasyModeBadge { get; }
            public Label EasyStateSummary { get; }
            public Label EasyDryRunLabel { get; }
            public Label EasyActionHint { get; }
            public VisualElement EasyGripperControlHost { get; }
            public Button BtnEasyHome { get; }
            public Button BtnEasyReady { get; }
            public Button BtnEasyFolded { get; }
            public Button BtnEasyZero { get; }
            public Button BtnEasyPreview { get; }
            public Button BtnEasyApply { get; }
            public EasyMotionGripperLiveControl.ViewHandle GripperView { get; }
        }
    }

    internal sealed class EasyMotionGripperLiveControl
    {
        private readonly RobotControlV3RuntimeController runtimeController;
        private readonly PopupCoordinatorV3 popupCoordinator;
        private readonly System.Collections.Generic.List<ViewHandle> views = new();
        private float requestedPercent = 100f;
        private float actualPercent = 100f;
        private float pendingDraftPercent = 100f;
        private int rawPercent = 100;
        private bool hasPendingDraft;
        private bool hasReliableReadback = true;
        private bool canIssueLiveApply;
        private bool isReadbackOnlyLive;
        private bool isPreviewOnlyMode = true;
        private string lastApplyResult = "아직 적용 전";

        public EasyMotionGripperLiveControl(RobotControlV3RuntimeController runtimeController, PopupCoordinatorV3 popupCoordinator)
        {
            this.runtimeController = runtimeController;
            this.popupCoordinator = popupCoordinator;
        }

        public ViewHandle Attach(VisualElement host)
        {
            if (host == null)
            {
                return null;
            }

            var view = new ViewHandle(host);
            views.Add(view);
            Bind(view);
            RefreshView(view, true);
            return view;
        }

        public void UpdateSnapshot(RobotControlV3RuntimeSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            canIssueLiveApply = runtimeController != null
                ? runtimeController.CanIssueLiveGripperOperatorWrite()
                : CanIssueLiveApply(snapshot);
            isReadbackOnlyLive = !snapshot.DryRunEnabled && snapshot.CurrentSessionMode == "readback-only";
            isPreviewOnlyMode = !canIssueLiveApply;
            if (canIssueLiveApply)
            {
                requestedPercent = NormalizePercent(snapshot.GripperCommandedPositionPercent, requestedPercent);
            }

            actualPercent = NormalizePercent(snapshot.GripperActualPositionPercent, actualPercent);
            rawPercent = Mathf.Clamp(snapshot.GripperRawActualPositionPercent, 0, 100);
            hasReliableReadback = snapshot.HasReliableGripperReadback;
            if (!hasPendingDraft)
            {
                pendingDraftPercent = requestedPercent;
            }

            RefreshAllViews();
        }

        public void RefreshView(ViewHandle view, bool enabled)
        {
            if (view == null)
            {
                return;
            }

            view.Enabled = enabled;
            view.ActualLabel.text = hasReliableReadback
                ? $"현재 실제값 {FormatPercent(actualPercent)}%"
                : isPreviewOnlyMode
                    ? "현재 실제값 없음 · 화면 프리뷰만 유지"
                    : "현재 실제값 확인 안 됨";
            view.CommandLabel.text = canIssueLiveApply
                ? $"현재 요청값 {FormatPercent(requestedPercent)}%"
                : isReadbackOnlyLive
                    ? $"읽기 전용 기준값 {FormatPercent(requestedPercent)}%"
                    : $"프리뷰 요청값 {FormatPercent(requestedPercent)}%";
            view.RawLabel.text = $"SDK raw {rawPercent}%";
            view.DraftLabel.text = hasPendingDraft
                ? $"입력 대기 {FormatPercent(pendingDraftPercent)}% · 자동 덮어쓰기 잠금"
                : isPreviewOnlyMode
                    ? "입력값 유지 중 · 실제 이동은 아직 잠김"
                    : "입력값 동기화됨";
            view.ResultLabel.text = $"마지막 적용 {lastApplyResult}";
            if (!view.IsEditingInput)
            {
                var visibleInput = hasPendingDraft ? pendingDraftPercent : requestedPercent;
                view.PositionInput.SetValueWithoutNotify(visibleInput);
            }

            view.PreviewButton.text = "미리보기 적용";
            view.LiveButton.text = canIssueLiveApply ? "실제 이동" : "실제 이동 잠김";
            view.SetEnabled(enabled);
        }

        public string SetDraftForDebug(float positionPercent, bool apply)
        {
            var clamped = NormalizePercent(positionPercent, pendingDraftPercent);
            if (apply)
            {
                ApplyRequestedPosition(clamped);
            }
            else
            {
                pendingDraftPercent = clamped;
                hasPendingDraft = true;
                if (isPreviewOnlyMode)
                {
                    requestedPercent = clamped;
                }
                RefreshAllViews();
            }

            return GetDebugState();
        }

        public string GetDebugState()
        {
            return $"requested={FormatPercent(requestedPercent)}; actual={FormatPercent(actualPercent)}; raw={rawPercent}; draft={FormatPercent(pendingDraftPercent)}; pending={hasPendingDraft}; readback={hasReliableReadback}; lastApply={lastApplyResult}";
        }

        private void Bind(ViewHandle view)
        {
            view.PositionInput.RegisterValueChangedCallback(evt =>
            {
                pendingDraftPercent = NormalizePercent(evt.newValue, pendingDraftPercent);
                hasPendingDraft = true;
                if (isPreviewOnlyMode)
                {
                    requestedPercent = pendingDraftPercent;
                }
                RefreshAllViews();
            });
            view.PositionInput.RegisterCallback<FocusInEvent>(_ =>
            {
                view.IsEditingInput = true;
                view.PositionInput.SelectAll();
            });
            view.PositionInput.RegisterCallback<FocusOutEvent>(_ =>
            {
                pendingDraftPercent = NormalizePercent(view.PositionInput.value, pendingDraftPercent);
                hasPendingDraft = true;
                if (isPreviewOnlyMode)
                {
                    requestedPercent = pendingDraftPercent;
                }
                view.IsEditingInput = false;
                RefreshAllViews();
            });
            view.PreviewButton.clicked += () => ApplyPreviewOnly(pendingDraftPercent);
            view.LiveButton.clicked += () => ApplyRequestedPosition(pendingDraftPercent);
            view.Preset100Button.clicked += () => SetPresetDraft(100f);
            view.Preset50Button.clicked += () => SetPresetDraft(50f);
            view.Preset0Button.clicked += () => SetPresetDraft(0f);
        }

        private void ApplyRequestedPosition(float positionPercent)
        {
            var clamped = NormalizePercent(positionPercent, pendingDraftPercent);
            pendingDraftPercent = clamped;
            hasPendingDraft = true;
            var runtimeSnapshot = runtimeController?.CurrentSnapshot;
            var definitelyPreviewOnly = runtimeSnapshot == null
                || runtimeSnapshot.DryRunEnabled
                || runtimeSnapshot.CurrentSessionMode == "readback-only"
                || runtimeSnapshot.StatusKind == RobotControlV3RuntimeStatusKind.Disconnected
                || runtimeSnapshot.StatusKind == RobotControlV3RuntimeStatusKind.AutoReconnect
                || (runtimeSnapshot.StatusConnection?.Contains("미연결") ?? false)
                || (runtimeSnapshot.ConnectionChip?.Contains("미연결") ?? false)
                || !runtimeSnapshot.MotionGateReady;
            canIssueLiveApply = !definitelyPreviewOnly
                && runtimeController != null
                && runtimeController.CanIssueLiveGripperOperatorWrite();
            isPreviewOnlyMode = !canIssueLiveApply;

            if (!canIssueLiveApply)
            {
                requestedPercent = clamped;
                hasPendingDraft = false;
                pendingDraftPercent = clamped;
                lastApplyResult = isReadbackOnlyLive
                    ? "읽기 전용 세션이라 실제 그리퍼는 움직이지 않고 입력값만 유지했다."
                    : "화면 프리뷰만 갱신했다. 실제 그리퍼 쓰기는 아직 잠겨 있다.";
                RefreshAllViews();
                return;
            }

            if (runtimeController == null)
            {
                lastApplyResult = "런타임을 찾지 못했다.";
                RefreshAllViews();
                return;
            }

            if (popupCoordinator != null && runtimeController.ShouldRouteGripperOperatorThroughLiveApproval())
            {
                lastApplyResult = runtimeController.PrepareGripperOperatorApproval(clamped);
                if (runtimeController.HasPendingGripperOperatorApproval())
                {
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveGripper"))
                    {
                        popupCoordinator.OpenMoveConfirmForProduct();
                    }
                    else
                    {
                        lastApplyResult = runtimeController.ExecutePendingGripperOperatorCommand();
                    }
                }

                RefreshAllViews();
                return;
            }

            var result = runtimeController.SetGripperPositionPercentFromOperator(clamped);
            lastApplyResult = string.IsNullOrWhiteSpace(result.Message) ? "적용 결과 없음" : result.Message;
            if (result.IsSuccess)
            {
                requestedPercent = clamped;
                hasPendingDraft = false;
                pendingDraftPercent = clamped;
            }

            RefreshAllViews();
        }

        private void ApplyPreviewOnly(float positionPercent)
        {
            var clamped = NormalizePercent(positionPercent, pendingDraftPercent);
            pendingDraftPercent = clamped;
            requestedPercent = clamped;
            hasPendingDraft = false;
            lastApplyResult = $"프리뷰 요청값 {FormatPercent(clamped)}%로 갱신";
            RefreshAllViews();
        }

        private void SetPresetDraft(float positionPercent)
        {
            var clamped = NormalizePercent(positionPercent, pendingDraftPercent);
            pendingDraftPercent = clamped;
            hasPendingDraft = true;
            if (isPreviewOnlyMode)
            {
                requestedPercent = clamped;
            }

            lastApplyResult = $"빠른 선택 {FormatPercent(clamped)}%";
            RefreshAllViews();
        }

        private void RefreshAllViews()
        {
            for (var i = 0; i < views.Count; i++)
            {
                RefreshView(views[i], views[i].Enabled);
            }
        }

        private static float NormalizePercent(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                value = fallback;
            }

            return Mathf.Clamp(value, 0f, 100f);
        }

        private static string FormatPercent(float value)
        {
            return NormalizePercent(value, 0f).ToString("0.##");
        }

        private static bool CanIssueLiveApply(RobotControlV3RuntimeSnapshot snapshot)
        {
            if (snapshot == null || snapshot.DryRunEnabled)
            {
                return false;
            }

            return snapshot.StatusKind != RobotControlV3RuntimeStatusKind.Disconnected
                && snapshot.StatusKind != RobotControlV3RuntimeStatusKind.AutoReconnect
                && snapshot.MotionGateReady
                && snapshot.CurrentSessionMode is "live-control" or "gripper-only";
        }

        internal sealed class ViewHandle
        {
            public ViewHandle(VisualElement host)
            {
                host.Clear();

                ActualLabel = CreateLabel("EasyGripperActual", "rc-easy-gripper-status");
                CommandLabel = CreateLabel("EasyGripperRequested", "rc-easy-gripper-status");
                RawLabel = CreateLabel("EasyGripperRaw", "rc-easy-gripper-meta");
                DraftLabel = CreateLabel("EasyGripperDraft", "rc-easy-gripper-meta");
                ResultLabel = CreateLabel("EasyGripperResult", "rc-easy-gripper-meta");

                PositionInput = new FloatField("입력 %")
                {
                    name = "EasyGripperPositionInput",
                    value = 100f,
                    isDelayed = false,
                };
                PositionInput.AddToClassList("rc-easy-number-field");

                PreviewButton = new Button
                {
                    name = "BtnEasyGripperPreviewApply",
                    text = "미리보기 적용",
                };
                PreviewButton.AddToClassList("rc-easy-action-button");

                LiveButton = new Button
                {
                    name = "BtnEasyGripperLiveApply",
                    text = "실제 이동",
                };
                LiveButton.AddToClassList("rc-easy-action-button");

                Preset100Button = CreateQuickButton("BtnEasyGripper100", "100");
                Preset50Button = CreateQuickButton("BtnEasyGripper50", "50");
                Preset0Button = CreateQuickButton("BtnEasyGripper0", "0");

                host.Add(ActualLabel);
                host.Add(CommandLabel);
                host.Add(RawLabel);
                host.Add(DraftLabel);

                var inputRow = new VisualElement();
                inputRow.AddToClassList("rc-easy-control-row");
                inputRow.Add(PositionInput);
                inputRow.Add(PreviewButton);
                inputRow.Add(LiveButton);
                host.Add(inputRow);

                var quickRow = new VisualElement();
                quickRow.AddToClassList("rc-easy-control-row");
                quickRow.Add(Preset100Button);
                quickRow.Add(Preset50Button);
                quickRow.Add(Preset0Button);
                host.Add(quickRow);

                host.Add(ResultLabel);
            }

            public Label ActualLabel { get; }
            public Label CommandLabel { get; }
            public Label RawLabel { get; }
            public Label DraftLabel { get; }
            public Label ResultLabel { get; }
            public FloatField PositionInput { get; }
            public Button PreviewButton { get; }
            public Button LiveButton { get; }
            public Button Preset100Button { get; }
            public Button Preset50Button { get; }
            public Button Preset0Button { get; }
            public bool Enabled { get; set; }
            public bool IsEditingInput { get; set; }

            public void SetEnabled(bool enabled)
            {
                PositionInput.SetEnabled(enabled);
                PreviewButton.SetEnabled(enabled);
                LiveButton.SetEnabled(enabled);
                Preset100Button.SetEnabled(enabled);
                Preset50Button.SetEnabled(enabled);
                Preset0Button.SetEnabled(enabled);
            }

            private static Label CreateLabel(string name, string className)
            {
                var label = new Label
                {
                    name = name,
                };
                label.AddToClassList(className);
                return label;
            }

            private static Button CreateQuickButton(string name, string text)
            {
                var button = new Button
                {
                    name = name,
                    text = text,
                };
                button.AddToClassList("rc-easy-action-button");
                button.AddToClassList("rc-easy-quick-button");
                return button;
            }
        }
    }
}
