// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections.Generic;
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
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
            popupCoordinator ??= GetComponentInParent<PopupCoordinatorV3>();
            popupCoordinator ??= Object.FindFirstObjectByType<PopupCoordinatorV3>();
            root = document?.rootVisualElement;
            if (root == null || pointMoveTemplate == null || connectionHomeController == null || runtimeController == null)
            {
                return false;
            }

            workPanelBody = root.Q<VisualElement>("WorkPanelBody");
            bottomSheetBody = root.Q<VisualElement>("BottomSheetBody");
            pointMovePanelHost = root.Q<VisualElement>("PointMovePanelHost");
            pointMoveSheetHost = root.Q<VisualElement>("PointMoveSheetHost");
            if (pointMovePanelHost == null || pointMoveSheetHost == null)
            {
                isInitialized = false;
                return false;
            }

            if (!isInitialized
                || desktopPanel == null
                || tabletPanel == null
                || desktopPanel.BtnPointBulkFunction == null
                || desktopPanel.BtnPointFunctionCreate == null
                || pointMovePanelHost.childCount == 0
                || pointMoveSheetHost.childCount == 0)
            {
                desktopPanel = CreatePanel(pointMovePanelHost);
                tabletPanel = CreatePanel(pointMoveSheetHost);
            }

            var localState = GetLocalState();
            activeNavSection = localState.ActiveNavSection;
            activeCoordSystem = localState.CoordSystem;
            isDesktopVisible = ShouldShowDesktopPanel(localState.ActiveNavSection, localState.ActiveWorkTab);
            isTabletVisible = ShouldShowTabletPanel(localState.ActiveNavSection, localState.ActiveTabletTab);
            isInitialized = true;
            connectionHomeController.PreviewChanged -= ApplyPreview;
            connectionHomeController.PreviewChanged += ApplyPreview;
            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            return true;
            }
            finally
            {
                isInitializing = false;
            }
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

        private PanelElements CreatePanel(VisualElement host)
        {
            host.Clear();
            var tree = pointMoveTemplate.CloneTree();
            host.Add(tree);
            var panel = new PanelElements(tree);
            RegisterPanel(panel);
            return panel;
        }

        private void RegisterPanel(PanelElements panel)
        {
            ConfigureListViews(panel);
            RegisterClick(panel.BtnCoordBase, () => SetCoordSystem("Base"));
            RegisterClick(panel.BtnCoordTool, () => SetCoordSystem("Tool"));
            RegisterClick(panel.BtnCoordUser, () => SetCoordSystem("User"));
            RegisterClick(panel.BtnPointSubview, () => SetTeachingSubview(PointSubviewName));
            RegisterClick(panel.BtnSequenceSubview, () => SetTeachingSubview(SequenceSubviewName));
            RegisterClick(panel.BtnFunctionSubview, () => SetTeachingSubview(FunctionSubviewName));
            RegisterClick(panel.BtnMoveJ, () => SetMotionKind("MoveJ"));
            RegisterClick(panel.BtnMoveL, () => SetMotionKind("MoveL"));
            RegisterSearchField(panel.PointSearchInput, value =>
            {
                pointSearchText = value?.Trim() ?? string.Empty;
                ApplyAll();
            });
            RegisterSearchField(panel.FunctionSearchInput, value =>
            {
                functionSearchText = value?.Trim() ?? string.Empty;
                ApplyAll();
            });
            RegisterSearchField(panel.SequenceSearchInput, value =>
            {
                sequenceSearchText = value?.Trim() ?? string.Empty;
                ApplyAll();
            });
            RegisterClick(panel.BtnPointFilterAll, () => SetPointFilter(FilterAll));
            RegisterClick(panel.BtnPointFilterSelected, () => SetPointFilter(FilterSelected));
            RegisterClick(panel.BtnPointFilterMoveJ, () => SetPointFilter(FilterMoveJ));
            RegisterClick(panel.BtnPointFilterMoveL, () => SetPointFilter(FilterMoveL));
            RegisterClick(panel.BtnFunctionFilterAll, () => SetFunctionFilter(FilterAll));
            RegisterClick(panel.BtnFunctionFilterSelected, () => SetFunctionFilter(FilterSelected));
            RegisterClick(panel.BtnFunctionFilterMissing, () => SetFunctionFilter(FilterMissing));
            RegisterClick(panel.BtnSequenceFilterAll, () => SetSequenceFilter(FilterAll));
            RegisterClick(panel.BtnSequenceFilterSelected, () => SetSequenceFilter(FilterSelected));
            RegisterClick(panel.BtnSequenceFilterDeletable, () => SetSequenceFilter(FilterDeletable));
            RegisterClick(panel.BtnSequenceFilterProtected, () => SetSequenceFilter(FilterProtected));
            RegisterClick(panel.BtnRestore, RestoreFromPreview);
            RegisterClick(panel.BtnSave, SaveCurrentPoint);
            RegisterClick(panel.BtnRecall, () => RecallPoint(panel.PointNameInput?.value));
            RegisterClick(panel.BtnDelete, () => DeletePoint(panel.PointNameInput?.value));
            RegisterClick(panel.BtnRename, () => RenamePoint(recalledPoint?.name, panel.PointNameInput?.value));
            RegisterClick(panel.BtnDuplicate, DuplicateSelectedPoint);
            RegisterClick(panel.BtnPointRowActionsToggle, TogglePointRowActionsCollapsed);
            RegisterClick(panel.BtnPointBulkClear, ClearSelectedPoints);
            RegisterClick(panel.BtnPointBulkFunction, AddSelectedPointsToFunction);
            RegisterClick(panel.BtnPointBulkDelete, DeleteSelectedPoints);
            RegisterClick(panel.BtnPointFunctionClearSelection, ClearFunctionPointSelection);
            RegisterClick(panel.BtnPointFunctionCreate, CreateFunctionFromSequence);
            RegisterClick(panel.BtnSequenceRowActionsToggle, ToggleSequenceRowActionsCollapsed);
            RegisterClick(panel.BtnSequenceBulkClear, ClearSelectedSequences);
            RegisterClick(panel.BtnSequenceBulkDelete, DeleteSelectedSequences);
            RegisterClick(panel.BtnFunctionRowActionsToggle, ToggleFunctionRowActionsCollapsed);
            RegisterClick(panel.BtnFunctionBulkClear, ClearSelectedFunctions);
            RegisterClick(panel.BtnFunctionBulkDuplicate, DuplicateSelectedFunctions);
            RegisterClick(panel.BtnFunctionBulkDelete, DeleteSelectedFunctions);
            RegisterClick(panel.BtnFunctionDeleteAll, DeleteAllFunctions);
            RegisterClick(panel.BtnUp, () => MovePointInSequence(-1));
            RegisterClick(panel.BtnDown, () => MovePointInSequence(1));
            RegisterClick(panel.BtnOverwrite, OverwriteSelectedPointWithCurrentReadback);
            RegisterClick(panel.BtnSpeedSlow, () => SetSelectedSpeedPreset("slow"));
            RegisterClick(panel.BtnSpeedMedium, () => SetSelectedSpeedPreset("medium"));
            RegisterClick(panel.BtnSpeedFast, () => SetSelectedSpeedPreset("fast"));
            RegisterClick(panel.BtnTimingApply, ApplySelectedPointTiming);
            RegisterClick(panel.BtnLoop, ToggleTeachingLoop);
            RegisterClick(panel.BtnRunSequence, RunActiveSequence);
            RegisterClick(panel.BtnStepBack, StepTeachingBackward);
            RegisterClick(panel.BtnStepForward, StepTeachingForward);
            RegisterClick(panel.BtnStopSequence, StopTeachingSequence);
            RegisterClick(panel.BtnPathRecordStart, StartPathRecording);
            RegisterClick(panel.BtnPathRecordStop, StopPathRecording);
            RegisterClick(panel.BtnPathReplayOnce, PlayRecordedPathOnce);
            RegisterClick(panel.BtnPathReplayLoop, PlayRecordedPathLoop);
            RegisterClick(panel.BtnPathRecordDelete, DeleteRecordedPath);
            RegisterClick(panel.BtnBlockAddPoint, AddSelectedPointToBlockSequence);
            RegisterClick(panel.BtnBlockAddBundle, OpenBundlePickerModal);
            RegisterClick(panel.BtnBlockPreview, PreviewBlockSequence);
            RegisterClick(panel.BtnBlockRun, RunBlockSequence);
            RegisterClick(panel.BtnPointModalSpeedSlow, () => SetPointModalSpeedPreset("slow"));
            RegisterClick(panel.BtnPointModalSpeedMedium, () => SetPointModalSpeedPreset("medium"));
            RegisterClick(panel.BtnPointModalSpeedFast, () => SetPointModalSpeedPreset("fast"));
            RegisterClick(panel.BtnPointModalPrimary, ApplyPointActionModalPrimary);
            RegisterClick(panel.BtnPointModalOverwrite, () => ExecutePointModalEditAction(OverwriteSelectedPointWithCurrentReadback));
            RegisterClick(panel.BtnPointModalDuplicate, () => ExecutePointModalEditAction(DuplicateSelectedPoint));
            RegisterClick(panel.BtnPointModalDelete, () => ExecutePointModalEditAction(() => DeletePoint(recalledPoint?.name)));
            RegisterClick(panel.BtnPointModalClose, ClosePointActionModal);
            RegisterClick(panel.BtnBundlePickerConfirm, ConfirmBundlePickerSelection);
            RegisterClick(panel.BtnBundlePickerClose, CloseBundlePickerModal);
            RegisterClick(panel.BtnFunctionRename, RenameSelectedFunction);
            RegisterClick(panel.BtnFunctionDuplicate, DuplicateSelectedFunction);
            RegisterClick(panel.BtnFunctionDelete, DeleteSelectedFunction);
            RegisterClick(panel.BtnExport, ExportPoints);
            RegisterClick(panel.BtnCleanup, CleanupPoints);
            RegisterClick(panel.BtnPreview, PreviewMotionCandidate);
            RegisterClick(panel.BtnRunFromSelected, RunFromSelectedPoint);
            RegisterClick(panel.BtnApply, ApplyMotionCandidate);
            panel.DwellInput?.RegisterValueChangedCallback(evt => HandleDwellChanged(evt.newValue));
            for (var index = 0; index < panel.ValueInputs.Length; index++)
            {
                var capturedIndex = index;
                panel.ValueInputs[index].RegisterValueChangedCallback(evt => HandleValueChanged(capturedIndex, evt.newValue));
                panel.ValueInputs[index].RegisterCallback<FocusOutEvent>(_ => DispatchEditedPointCandidate());
            }
        }

        private static void RegisterClick(Button button, System.Action handler)
        {
            if (button == null || handler == null)
            {
                return;
            }

            button.clicked += handler;
        }

        private static void RegisterSearchField(TextField field, System.Action<string> handler)
        {
            if (field == null || handler == null)
            {
                return;
            }

            field.RegisterValueChangedCallback(evt => handler(evt.newValue));
        }

        private void ConfigureListViews(PanelElements panel)
        {
            ConfigureListView(panel.PointListView, pointListItems, 144, BindPointListRow);
            ConfigureListView(panel.FunctionListView, functionListItems, 112, BindFunctionListRow);
            ConfigureListView(panel.BlockSequenceListView, blockSequenceListItems, 72, BindBlockSequenceListRow);
            ConfigureListView(panel.SequenceListView, sequenceListItems, 112, BindSequenceListRow);
            ConfigureListView(panel.BundlePickerListView, bundlePickerListItems, 72, BindBundlePickerListRow);
        }

        private static void ConfigureListView<T>(
            ListView listView,
            List<T> source,
            float itemHeight,
            System.Action<VisualElement, int> bindItem)
        {
            if (listView == null)
            {
                return;
            }

            listView.itemsSource = source;
            listView.fixedItemHeight = itemHeight;
            listView.selectionType = SelectionType.None;
            listView.makeItem = MakeListRow;
            listView.bindItem = bindItem;
        }

        private static VisualElement MakeListRow()
        {
            return new VisualElement();
        }

        private void SetPointFilter(string filter)
        {
            pointFilter = NormalizePointFilter(filter);
            ApplyAll();
        }

        private void SetFunctionFilter(string filter)
        {
            functionFilter = NormalizeFunctionFilter(filter);
            ApplyAll();
        }

        private void SetSequenceFilter(string filter)
        {
            sequenceFilter = NormalizeSequenceFilter(filter);
            ApplyAll();
        }

        private void ApplyPreview(RobotControlV3RuntimeSnapshot data)
        {
            for (var index = 0; index < currentValues.Length && index < data.TcpValues.Length; index++)
            {
                currentValues[index] = ParseValue(data.TcpValues[index]);
            }

            isPointNameInvalid = false;
            ApplyAll();
        }

        private void ApplyAll()
        {
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
            ApplyVisibility();
        }

        private void ApplyPanel(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            panel.BtnCoordBase.EnableInClassList("rc-point-coord-button--active", activeCoordSystem == "Base");
            panel.BtnCoordTool.EnableInClassList("rc-point-coord-button--active", activeCoordSystem == "Tool");
            panel.BtnCoordUser.EnableInClassList("rc-point-coord-button--active", activeCoordSystem == "User");
            panel.BtnMoveJ.EnableInClassList("rc-point-motion-button--active", motionKind == "MoveJ");
            panel.BtnMoveL.EnableInClassList("rc-point-motion-button--active", motionKind == "MoveL");
            ApplySubviewVisibility(panel);
            var isTeachingSurface = activeNavSection == "NavPoints";
            if (panel.Title != null)
            {
                panel.Title.text = isTeachingSurface ? "저장 위치" : "저장 위치";
            }

            panel.Hint.text = isTeachingSurface
                ? "수동으로 맞춘 현재 위치를 저장하고, 저장 위치를 순서대로 실행하거나 묶음으로 재사용한다."
                : motionKind == "MoveL"
                    ? "직선으로 접근할 위치라면 먼저 미리보기로 방향을 확인한다."
                    : "관절 기준으로 접근할 위치라면 먼저 미리보기로 빠르게 확인한다.";
            SetHidden(panel.Hint, isTeachingSurface);
            panel.CoordSummary.text = $"좌표계: {activeCoordSystem} / 현재 TCP 기준으로 시작";
            panel.MotionSummary.text = motionKind == "MoveL"
                ? "이동 방식: 직선 이동 / 공구 경로를 직선으로 먼저 확인"
                : "이동 방식: 관절 이동 / 관절 중심으로 먼저 후보를 확인";
            panel.PreviewSummary.text = BuildDeltaSummary(panel.PointNameInput.value);
            panel.StoreSummary.text = BuildStoreSummary();
            if (panel.PointInventorySummary != null)
            {
                panel.PointInventorySummary.text = BuildPointInventorySummary();
            }

            ApplyListToolbarState(panel);
            if (panel.PathRecordSummary != null)
            {
                panel.PathRecordSummary.text = FormatPathRecordSummary(runtimeController?.GetTeachingPathRecordingSummaryForDebug());
            }

            ApplyLoopState(panel);
            RebuildPointList(panel);
            ApplySequencePanel(panel);
            ApplyPointDetail(panel);
            ApplyPointActionModal(panel);
            ApplyBundlePickerModal(panel);
            ApplyPointFunctionBuilder(panel);
            ApplyFunctionPanel(panel);
            panel.FeedbackSummary.text = ShouldShowFeedbackLine() ? CompactFeedback(lastFeedback) : string.Empty;
            var canPreview = CanPreview() && IsAnyPanelVisible();
            var canApply = CanApply() && IsAnyPanelVisible();
            var canEdit = !IsSequenceEditLocked();
            panel.BtnRestore.SetEnabled(canPreview);
            panel.BtnPreview.SetEnabled(canPreview);
            panel.BtnRunFromSelected.SetEnabled(canApply && recalledPoint != null && !IsSequenceEditLocked());
            panel.BtnApply.SetEnabled(canApply);
            panel.BtnSave.SetEnabled(canEdit);
            panel.BtnDelete.SetEnabled(canEdit && (recalledPoint != null || HasNamedPoint(panel.PointNameInput?.value)));
            panel.BtnRename.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnDuplicate.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnUp.SetEnabled(canEdit && CanMoveSelectedPoint(-1));
            panel.BtnDown.SetEnabled(canEdit && CanMoveSelectedPoint(1));
            panel.BtnOverwrite.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnTimingApply?.SetEnabled(canEdit && recalledPoint != null && !isDwellInvalid);
            panel.BtnPointRowActionsToggle?.SetEnabled(HasAnyPoint());
            if (panel.BtnPointRowActionsToggle != null)
            {
                panel.BtnPointRowActionsToggle.text = pointRowActionsCollapsed ? "버튼 펼치기" : "버튼 접기";
            }

            panel.BtnPointBulkClear?.SetEnabled(selectedPointNames.Count > 0);
            panel.BtnPointBulkFunction?.SetEnabled(canEdit && selectedPointNames.Count > 0);
            panel.BtnPointBulkDelete?.SetEnabled(canEdit && selectedPointNames.Count > 0);
            SetHidden(panel.PointBulkActions, selectedPointNames.Count == 0);
            panel.BtnPointFunctionClearSelection?.SetEnabled(canEdit && selectedFunctionPointNames.Count > 0);
            panel.BtnPointFunctionCreate?.SetEnabled(canEdit && HasAnyPoint());
            panel.BtnSequenceRowActionsToggle?.SetEnabled(BuildOrderedSequenceNames().Count > 0);
            if (panel.BtnSequenceRowActionsToggle != null)
            {
                panel.BtnSequenceRowActionsToggle.text = sequenceRowActionsCollapsed ? "버튼 펼치기" : "버튼 접기";
            }

            panel.BtnSequenceBulkClear?.SetEnabled(selectedSequenceNames.Count > 0);
            panel.BtnSequenceBulkDelete?.SetEnabled(canEdit && CountDeletableSelectedSequences() > 0);
            panel.BtnFunctionRowActionsToggle?.SetEnabled(runtimeController != null && runtimeController.GetTeachingFunctionNames().Length > 0);
            if (panel.BtnFunctionRowActionsToggle != null)
            {
                panel.BtnFunctionRowActionsToggle.text = functionRowActionsCollapsed ? "버튼 펼치기" : "버튼 접기";
            }

            panel.BtnFunctionBulkClear?.SetEnabled(selectedFunctionNames.Count > 0);
            panel.BtnFunctionBulkDuplicate?.SetEnabled(canEdit && selectedFunctionNames.Count > 0);
            panel.BtnFunctionBulkDelete?.SetEnabled(canEdit && selectedFunctionNames.Count > 0);
            panel.BtnFunctionDeleteAll?.SetEnabled(canEdit && runtimeController != null && runtimeController.GetTeachingFunctionNames().Length > 0);
            panel.BtnPointModalPrimary?.SetEnabled(recalledPoint != null && !IsSequenceEditLocked());
            panel.BtnPointModalOverwrite?.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnPointModalDuplicate?.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnPointModalDelete?.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnLoop.SetEnabled(HasAnyPoint() && !IsSequenceEditLocked());
            panel.BtnRunSequence.SetEnabled(canApply && HasAnyPoint());
            panel.BtnStepBack.SetEnabled(canApply && HasAnyPoint());
            panel.BtnStepForward.SetEnabled(canApply && HasAnyPoint());
            panel.BtnStopSequence.SetEnabled(runtimeController != null);
            panel.BtnPathRecordStart?.SetEnabled(canApply && !IsSequenceEditLocked());
            panel.BtnPathRecordStop?.SetEnabled(runtimeController != null);
            panel.BtnPathReplayOnce?.SetEnabled(canApply && !IsSequenceEditLocked());
            panel.BtnPathReplayLoop?.SetEnabled(canApply && !IsSequenceEditLocked());
            panel.BtnPathRecordDelete?.SetEnabled(canEdit && HasNamedSequence(RecordedPathSequenceName));
            panel.BtnBlockAddPoint?.SetEnabled(canEdit && recalledPoint != null);
            panel.BtnBlockAddBundle?.SetEnabled(canEdit && runtimeController != null && runtimeController.GetTeachingFunctionNames().Length > 0);
            panel.BtnBlockPreview?.SetEnabled(canApply);
            panel.BtnBlockRun?.SetEnabled(canApply && !IsSequenceEditLocked());
            panel.BtnExport.SetEnabled(HasAnyPoint());
            panel.BtnCleanup.SetEnabled(canEdit && HasAnyPoint());
            panel.BtnFunctionRename?.SetEnabled(canEdit && !string.IsNullOrWhiteSpace(selectedFunctionName));
            panel.BtnFunctionDuplicate?.SetEnabled(canEdit && !string.IsNullOrWhiteSpace(selectedFunctionName));
            panel.BtnFunctionDelete?.SetEnabled(canEdit && !string.IsNullOrWhiteSpace(selectedFunctionName));
            panel.BtnApply.text = IsMoveLDispatchMode()
                ? (CanApply() ? "적용" : "적용 (연결 대기)")
                : (CanApply() ? "적용 (관절 이동)" : "적용 (연결 대기)");
            panel.PointNameInput.EnableInClassList("rc-point-name-input--danger", isPointNameInvalid);

            for (var index = 0; index < panel.ValueInputs.Length && index < currentValues.Length; index++)
            {
                panel.ValueInputs[index].SetValueWithoutNotify(currentValues[index].ToString("0.0", CultureInfo.InvariantCulture));
                panel.ValueInputs[index].EnableInClassList("rc-point-cell-input--danger", index == lastInvalidIndex);
            }
        }

        private void SetTeachingSubview(string subviewName)
        {
            activeTeachingSubview = subviewName == SequenceSubviewName || subviewName == FunctionSubviewName
                ? subviewName
                : PointSubviewName;
            ApplyAll();
        }

        private void ApplySubviewVisibility(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            var isTeachingSurface = activeNavSection == "NavPoints";
            if (!isTeachingSurface)
            {
                SetHidden(panel.MotionRow, false);
                SetHidden(panel.SubviewTabs, true);
                SetHidden(panel.NameRow, true);
                SetHidden(panel.BtnPrimarySave, true);
                SetHidden(panel.PointSubview, false);
                SetHidden(panel.SequenceSubview, true);
                SetHidden(panel.FunctionSubview, true);
                SetHidden(panel.CoordRow, false);
                SetHidden(panel.CoordGrid, false);
                SetHidden(panel.PointListContainer, true);
                SetHidden(panel.PointEditSubview, false);
                SetHidden(panel.DetailCard, true);
                SetHidden(panel.PointEditActions, false);
                ApplyMoveTargetActionVisibility(panel);
                return;
            }

            var showPoint = activeTeachingSubview == PointSubviewName;
            var showSequence = activeTeachingSubview == SequenceSubviewName;
            var showFunction = activeTeachingSubview == FunctionSubviewName;
            SetHidden(panel.MotionRow, true);
            SetHidden(panel.SubviewTabs, false);
            SetHidden(panel.NameRow, !showPoint);
            SetHidden(panel.BtnPrimarySave, !showPoint);
            panel.BtnPointSubview?.EnableInClassList("rc-point-subview-tab--active", showPoint);
            panel.BtnSequenceSubview?.EnableInClassList("rc-point-subview-tab--active", showSequence);
            panel.BtnFunctionSubview?.EnableInClassList("rc-point-subview-tab--active", showFunction);
            panel.PointSubview?.EnableInClassList("rc-hidden", !showPoint);
            panel.SequenceSubview?.EnableInClassList("rc-hidden", !showSequence);
            panel.FunctionSubview?.EnableInClassList("rc-hidden", !showFunction);
            SetHidden(panel.CoordRow, true);
            SetHidden(panel.CoordGrid, true);
            SetHidden(panel.PointListContainer, false);
            panel.PointEditSubview?.EnableInClassList("rc-hidden", !showPoint);
            SetHidden(panel.DetailCard, false);
            panel.PointEditActions?.EnableInClassList("rc-hidden", !showPoint);
            ApplyTeachingActionVisibility(panel);
        }

        private static void ApplyMoveTargetActionVisibility(PanelElements panel)
        {
            SetHidden(panel.BtnRestore, false);
            SetHidden(panel.BtnPreview, false);
            SetHidden(panel.BtnApply, false);
            SetHidden(panel.BtnRecall, true);
            SetHidden(panel.BtnDelete, true);
            SetHidden(panel.BtnRename, true);
            SetHidden(panel.BtnDuplicate, true);
            SetHidden(panel.BtnUp, true);
            SetHidden(panel.BtnDown, true);
            SetHidden(panel.BtnOverwrite, true);
            SetHidden(panel.BtnExport, true);
            SetHidden(panel.BtnCleanup, true);
        }

        private static void ApplyTeachingActionVisibility(PanelElements panel)
        {
            SetHidden(panel.BtnRestore, false);
            SetHidden(panel.BtnPreview, false);
            SetHidden(panel.BtnApply, false);
            SetHidden(panel.BtnRecall, false);
            SetHidden(panel.BtnDelete, false);
            SetHidden(panel.BtnRename, false);
            SetHidden(panel.BtnDuplicate, false);
            SetHidden(panel.BtnUp, false);
            SetHidden(panel.BtnDown, false);
            SetHidden(panel.BtnOverwrite, false);
            SetHidden(panel.BtnExport, false);
            SetHidden(panel.BtnCleanup, false);
        }

        private void ApplyListToolbarState(PanelElements panel)
        {
            panel.PointSearchInput?.SetValueWithoutNotify(pointSearchText);
            panel.FunctionSearchInput?.SetValueWithoutNotify(functionSearchText);
            panel.SequenceSearchInput?.SetValueWithoutNotify(sequenceSearchText);
            if (panel.PointSelectionCount != null)
            {
                panel.PointSelectionCount.text = $"선택 {selectedPointNames.Count}개";
            }

            if (panel.FunctionSelectionCount != null)
            {
                panel.FunctionSelectionCount.text = $"선택 {selectedFunctionNames.Count}개";
            }

            if (panel.SequenceSelectionCount != null)
            {
                panel.SequenceSelectionCount.text = $"선택 {selectedSequenceNames.Count}개";
            }

            ApplyFilterButtonState(panel.BtnPointFilterAll, pointFilter == FilterAll);
            ApplyFilterButtonState(panel.BtnPointFilterSelected, pointFilter == FilterSelected);
            ApplyFilterButtonState(panel.BtnPointFilterMoveJ, pointFilter == FilterMoveJ);
            ApplyFilterButtonState(panel.BtnPointFilterMoveL, pointFilter == FilterMoveL);
            ApplyFilterButtonState(panel.BtnFunctionFilterAll, functionFilter == FilterAll);
            ApplyFilterButtonState(panel.BtnFunctionFilterSelected, functionFilter == FilterSelected);
            ApplyFilterButtonState(panel.BtnFunctionFilterMissing, functionFilter == FilterMissing);
            ApplyFilterButtonState(panel.BtnSequenceFilterAll, sequenceFilter == FilterAll);
            ApplyFilterButtonState(panel.BtnSequenceFilterSelected, sequenceFilter == FilterSelected);
            ApplyFilterButtonState(panel.BtnSequenceFilterDeletable, sequenceFilter == FilterDeletable);
            ApplyFilterButtonState(panel.BtnSequenceFilterProtected, sequenceFilter == FilterProtected);
        }

        private static void ApplyFilterButtonState(Button button, bool active)
        {
            button?.EnableInClassList("rc-point-filter-button--active", active);
        }

        private static void SetHidden(VisualElement element, bool hidden)
        {
            element?.EnableInClassList("rc-hidden", hidden);
        }

        private static bool IsHidden(VisualElement element)
        {
            return element == null || element.ClassListContains("rc-hidden");
        }

        private string ResolveSurfaceDebugName()
        {
            return activeNavSection == "NavPoints" ? "Teaching" : "MoveTarget";
        }

        private void RunActiveSequence()
        {
            if (runtimeController == null)
            {
                SetFeedback("시퀀스 실행 runtime을 찾지 못했다.");
                return;
            }

            runtimeController.ExecutePrimaryAction();
            SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
        }

        private void StepTeachingBackward()
        {
            if (runtimeController == null)
            {
                SetFeedback("이전/다음 실행 기능을 찾지 못했다.");
                return;
            }

            runtimeController.StepBackward();
            SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
        }

        private void StepTeachingForward()
        {
            if (runtimeController == null)
            {
                SetFeedback("이전/다음 실행 기능을 찾지 못했다.");
                return;
            }

            runtimeController.StepForward();
            SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
        }

        private void StopTeachingSequence()
        {
            if (runtimeController == null)
            {
                SetFeedback("Stop runtime을 찾지 못했다.");
                return;
            }

            var result = runtimeController.StopMotion();
            SetFeedback(result.Message);
        }

        private void StartPathRecording()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 새 경로 기록을 시작하지 않는다. Stop 후 다시 해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.StartTeachingPathRecording()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void StopPathRecording()
        {
            var result = runtimeController != null
                ? runtimeController.StopTeachingPathRecording()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void PlayRecordedPathOnce()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 기록 재생을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.PlayRecordedTeachingPathOnce()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void PlayRecordedPathLoop()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 기록 루프를 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.PlayRecordedTeachingPathLoop()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void DeleteRecordedPath()
        {
            SelectSequence(RecordedPathSequenceName);
            DeleteSelectedSequence();
        }

        private void AddSelectedPointToBlockSequence()
        {
            if (recalledPoint == null)
            {
                SetFeedback("작업 시퀀스에 넣을 포인트를 먼저 선택해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.AddTeachingBlockPoint(recalledPoint.name)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void AddSelectedBundleToBlockSequence()
        {
            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("작업 시퀀스에 넣을 묶음을 먼저 선택해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.AddTeachingBlockBundle(selectedFunctionName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void PreviewBlockSequence()
        {
            var result = runtimeController != null
                ? runtimeController.PreviewTeachingBlockSequence()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void RunBlockSequence()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 작업 시퀀스를 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteTeachingBlockSequenceDryRun()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void MoveBlockSequenceRow(int index, int direction)
        {
            var result = runtimeController != null
                ? runtimeController.MoveTeachingBlock(index, direction)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void DeleteBlockSequenceRow(int index)
        {
            var result = runtimeController != null
                ? runtimeController.DeleteTeachingBlock(index)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void SelectSequence(string sequenceName)
        {
            var safeName = string.IsNullOrWhiteSpace(sequenceName)
                ? PointSequenceName
                : sequenceName.Trim();
            selectedSequenceName = safeName;
            SetFeedback($"[Sequence] {GetSequenceDisplayName(safeName)} 선택");
            ApplyAll();
        }

        private void RunSelectedSequenceOnce()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 새 실행을 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 실행할 수 없다.");
                return;
            }

            if (popupCoordinator != null
                && runtimeController != null
                && runtimeController.ShouldRouteWaypointSequenceThroughLiveApproval(selectedSequenceName, loop: false))
            {
                SetFeedback(runtimeController.PrepareWaypointSequenceOperatorApproval(selectedSequenceName, loop: false));
                if (runtimeController.HasPendingWaypointSequenceOperatorApproval())
                {
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveJ"))
                    {
                        popupCoordinator.OpenRunConfirmForProduct();
                    }
                    else
                    {
                        SetFeedback(runtimeController.ExecutePendingWaypointSequenceOperatorCommand());
                    }
                }

                ApplyAll();
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteWaypointSequenceOnce(selectedSequenceName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void RunSelectedSequenceLoop()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 새 루프를 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 루프 실행할 수 없다.");
                return;
            }

            if (popupCoordinator != null
                && runtimeController != null
                && runtimeController.ShouldRouteWaypointSequenceThroughLiveApproval(selectedSequenceName, loop: true))
            {
                SetFeedback(runtimeController.PrepareWaypointSequenceOperatorApproval(selectedSequenceName, loop: true));
                if (runtimeController.HasPendingWaypointSequenceOperatorApproval())
                {
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveJ"))
                    {
                        popupCoordinator.OpenRunConfirmForProduct();
                    }
                    else
                    {
                        SetFeedback(runtimeController.ExecutePendingWaypointSequenceOperatorCommand());
                    }
                }

                ApplyAll();
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteWaypointSequenceLoop(selectedSequenceName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void DeleteSelectedSequence()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 실행 목록 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

            if (string.Equals(selectedSequenceName, PointSequenceName, System.StringComparison.OrdinalIgnoreCase))
            {
                SetFeedback("저장한 포인트 순서는 포인트 탭의 삭제/정리로 관리한다. 여기서는 기록한 경로와 별도 실행 목록만 삭제한다.");
                return;
            }

            if (!HasNamedSequence(selectedSequenceName))
            {
                SetFeedback($"{GetSequenceDisplayName(selectedSequenceName)} 실행 목록을 찾지 못했다.");
                selectedSequenceName = PointSequenceName;
                ApplyAll();
                return;
            }

            if (!IsPendingConfirmation("delete-sequence", selectedSequenceName))
            {
                SetPendingConfirmation("delete-sequence", selectedSequenceName);
                SetFeedback($"[Confirm] {GetSequenceDisplayName(selectedSequenceName)} 삭제 예정. 삭제를 한 번 더 누르면 기록/실행 목록이 지워진다.");
                return;
            }

            var deletedName = selectedSequenceName;
            var result = runtimeController != null
                ? runtimeController.DeleteWaypointSequence(deletedName)
                : WaypointStore.Delete(deletedName)
                    ? $"[Sequence] {deletedName} 삭제"
                    : $"[Sequence] {deletedName} 삭제 실패";
            selectedSequenceName = PointSequenceName;
            ClearPendingConfirmation();
            SetFeedback(result);
            ApplyAll();
        }

        private void ToggleSequenceRowActionsCollapsed()
        {
            sequenceRowActionsCollapsed = !sequenceRowActionsCollapsed;
            SetFeedback(sequenceRowActionsCollapsed ? "[Sequence] row 버튼 접기" : "[Sequence] row 버튼 펼치기");
            ApplyAll();
        }

        private void ToggleSequenceSelection(string sequenceName)
        {
            if (string.IsNullOrWhiteSpace(sequenceName))
            {
                return;
            }

            var safeName = sequenceName.Trim();
            if (selectedSequenceNames.Contains(safeName))
            {
                selectedSequenceNames.Remove(safeName);
            }
            else
            {
                selectedSequenceNames.Add(safeName);
            }

            ClearPendingConfirmation();
            ApplyAll();
        }

        private void ClearSelectedSequences()
        {
            selectedSequenceNames.Clear();
            ClearPendingConfirmation();
            SetFeedback("[Sequence] 선택 해제");
            ApplyAll();
        }

        private void DeleteSelectedSequences()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 실행 목록 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

            var deletable = BuildDeletableSelectedSequences();
            if (deletable.Count == 0)
            {
                SetFeedback("삭제 가능한 실행 목록을 먼저 선택해라. 저장한 포인트 순서는 보호된다.");
                return;
            }

            var confirmKey = string.Join("|", deletable);
            if (!IsPendingConfirmation("bulk-delete-sequence", confirmKey))
            {
                SetPendingConfirmation("bulk-delete-sequence", confirmKey);
                SetFeedback($"[Confirm] 실행 목록 {deletable.Count}개 삭제 예정. 선택 삭제를 한 번 더 눌러라.");
                return;
            }

            var deleted = 0;
            for (var index = 0; index < deletable.Count; index++)
            {
                var name = deletable[index];
                var result = runtimeController != null
                    ? runtimeController.DeleteWaypointSequence(name)
                    : WaypointStore.Delete(name)
                        ? $"[Sequence] {name} 삭제"
                        : $"[Sequence] {name} 삭제 실패";
                if (result.Contains("삭제") && !result.Contains("실패"))
                {
                    deleted++;
                }
            }

            selectedSequenceNames.Clear();
            selectedSequenceName = PointSequenceName;
            ClearPendingConfirmation();
            SetFeedback($"[Delete] 실행 목록 {deleted}개 삭제");
            ApplyAll();
        }

        private void CreateFunctionFromSequence()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 생성을 잠근다. Stop 후 다시 묶어라.");
                return;
            }

            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            var functionName = panel?.PointFunctionNameInput?.value?.Trim();
            if (string.IsNullOrWhiteSpace(functionName))
            {
                functionName = panel?.FunctionNameInput?.value?.Trim();
            }

            if (string.IsNullOrWhiteSpace(functionName))
            {
                SetFeedback("작업 묶음 이름을 먼저 넣어라.");
                return;
            }

            var sourcePointNames = BuildCreateFunctionSourcePointNames();
            var result = runtimeController != null
                ? sourcePointNames.Length > 0
                    ? runtimeController.CreateTeachingFunctionFromPoints(functionName, sourcePointNames)
                    : runtimeController.CreateTeachingFunctionFromSequence(functionName)
                : "runtime missing";
            selectedFunctionName = ExtractCreatedFunctionName(result, functionName);
            SetFunctionName(selectedFunctionName);
            SetFeedback(result);
            ApplyAll();
        }

        private void AddSelectedPointToFunction()
        {
            if (recalledPoint == null)
            {
                SetFeedback("묶음 후보에 넣을 포인트를 먼저 선택해라.");
                return;
            }

            if (!selectedFunctionPointNames.Contains(recalledPoint.name))
            {
                selectedFunctionPointNames.Add(recalledPoint.name);
            }

            SetFeedback($"[Bundle] 후보 추가 · {recalledPoint.name}");
            ApplyAll();
        }

        private void ClearFunctionPointSelection()
        {
            selectedFunctionPointNames.Clear();
            SetFeedback("[Bundle] 묶음 후보 초기화");
            ApplyAll();
        }

        private void TogglePointRowActionsCollapsed()
        {
            pointRowActionsCollapsed = !pointRowActionsCollapsed;
            SetFeedback(pointRowActionsCollapsed ? "[List] row 버튼 접기" : "[List] row 버튼 펼치기");
            ApplyAll();
        }

        private void TogglePointSelection(string pointName)
        {
            if (string.IsNullOrWhiteSpace(pointName))
            {
                return;
            }

            var safeName = pointName.Trim();
            if (selectedPointNames.Contains(safeName))
            {
                selectedPointNames.Remove(safeName);
            }
            else
            {
                selectedPointNames.Add(safeName);
            }

            ClearPendingConfirmation();
            ApplyAll();
        }

        private void ClearSelectedPoints()
        {
            selectedPointNames.Clear();
            ClearPendingConfirmation();
            SetFeedback("[Select] 선택 해제");
            ApplyAll();
        }

        private void AddSelectedPointsToFunction()
        {
            if (selectedPointNames.Count == 0)
            {
                SetFeedback("묶음에 추가할 포인트를 먼저 선택해라.");
                return;
            }

            var added = 0;
            for (var index = 0; index < selectedPointNames.Count; index++)
            {
                var pointName = selectedPointNames[index];
                if (!selectedFunctionPointNames.Contains(pointName) && HasNamedPoint(pointName))
                {
                    selectedFunctionPointNames.Add(pointName);
                    added++;
                }
            }

            SetFeedback($"[Bundle] 선택 {added}개 추가");
            ApplyAll();
        }

        private void ApplyBulkPointTiming()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 일괄 수정을 잠근다. Stop 후 다시 수정해라.");
                return;
            }

            if (selectedPointNames.Count == 0)
            {
                SetFeedback("속도를 바꿀 포인트를 먼저 선택해라.");
                return;
            }

            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("수정할 저장 포인트가 없다.");
                return;
            }

            var changed = 0;
            for (var index = 0; index < sequence.waypoints.Length; index++)
            {
                var waypoint = sequence.waypoints[index];
                if (waypoint == null || !selectedPointNames.Contains(waypoint.name))
                {
                    continue;
                }

                waypoint.speedPreset = NormalizeSpeedPreset(selectedSpeedPreset);
                waypoint.dwellSec = selectedDwellSec;
                sequence.waypoints[index] = waypoint;
                changed++;
            }

            if (changed == 0)
            {
                SetFeedback("선택된 포인트를 찾지 못했다.");
                return;
            }

            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("일괄 속도 저장 실패");
                return;
            }

            ClearPendingConfirmation();
            SetFeedback($"[Bulk] {changed}개 속도 {selectedSpeedPreset} 저장");
            ApplyAll();
        }

        private void DeleteSelectedPoints()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 일괄 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

            if (selectedPointNames.Count == 0)
            {
                SetFeedback("삭제할 포인트를 먼저 선택해라.");
                return;
            }

            var confirmKey = string.Join("|", selectedPointNames);
            if (!IsPendingConfirmation("bulk-delete", confirmKey))
            {
                SetPendingConfirmation("bulk-delete", confirmKey);
                SetFeedback($"[Confirm] 선택 {selectedPointNames.Count}개 삭제 예정. 선택 삭제를 한 번 더 눌러라.");
                return;
            }

            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("삭제할 저장 포인트가 없다.");
                return;
            }

            var remaining = new List<Waypoint>();
            var deleted = 0;
            for (var index = 0; index < sequence.waypoints.Length; index++)
            {
                var waypoint = sequence.waypoints[index];
                if (waypoint != null && selectedPointNames.Contains(waypoint.name))
                {
                    deleted++;
                    continue;
                }

                remaining.Add(waypoint);
            }

            sequence.waypoints = remaining.ToArray();
            if (sequence.waypoints.Length == 0)
            {
                WaypointStore.Delete(PointSequenceName);
            }
            else
            {
                WaypointStore.Save(sequence);
            }

            if (recalledPoint != null && selectedPointNames.Contains(recalledPoint.name))
            {
                recalledPoint = null;
            }

            selectedPointNames.Clear();
            ClearPendingConfirmation();
            SetFeedback($"[Delete] 선택 {deleted}개 삭제");
            ApplyAll();
        }

        private void RunSelectedFunction()
        {
            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("실행할 묶음을 먼저 선택해라.");
                return;
            }

            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 실행을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteTeachingFunctionOnceDryRun(selectedFunctionName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void RunSelectedFunctionFromSelectedPoint()
        {
            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("실행할 묶음을 먼저 선택해라.");
                return;
            }

            if (recalledPoint == null)
            {
                SetFeedback("묶음 안에서 시작할 포인트를 먼저 선택해라.");
                return;
            }

            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 선택 실행을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteTeachingFunctionFromPointDryRun(selectedFunctionName, recalledPoint.name)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void RenameSelectedFunction()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 이름 변경을 잠근다. Stop 후 다시 수정해라.");
                return;
            }

            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            var newName = panel?.FunctionNameInput?.value?.Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                newName = panel?.PointFunctionNameInput?.value?.Trim();
            }

            if (string.IsNullOrWhiteSpace(selectedFunctionName) || string.IsNullOrWhiteSpace(newName))
            {
                SetFeedback("선택된 묶음과 새 이름이 필요하다.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.RenameTeachingFunctionForDebug(selectedFunctionName, newName)
                : "runtime missing";
            selectedFunctionName = newName;
            SetFeedback(result);
            ApplyAll();
        }

        private void DuplicateSelectedFunction()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 복사를 잠근다. Stop 후 다시 복사해라.");
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("복사할 묶음을 먼저 선택해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.DuplicateTeachingFunctionForDebug(selectedFunctionName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void DeleteSelectedFunction()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("삭제할 묶음을 먼저 선택해라.");
                return;
            }

            var deletedName = selectedFunctionName;
            var result = runtimeController != null
                ? runtimeController.DeleteTeachingFunctionForDebug(deletedName)
                : "runtime missing";
            selectedFunctionName = string.Empty;
            SelectFirstExistingFunctionIfNeeded();
            SetFeedback(result);
            ApplyAll();
        }

        private void ToggleFunctionRowActionsCollapsed()
        {
            functionRowActionsCollapsed = !functionRowActionsCollapsed;
            SetFeedback(functionRowActionsCollapsed ? "[Function] row 버튼 접기" : "[Function] row 버튼 펼치기");
            ApplyAll();
        }

        private void ToggleFunctionSelection(string functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                return;
            }

            var safeName = functionName.Trim();
            if (selectedFunctionNames.Contains(safeName))
            {
                selectedFunctionNames.Remove(safeName);
            }
            else
            {
                selectedFunctionNames.Add(safeName);
            }

            ClearPendingConfirmation();
            ApplyAll();
        }

        private void ClearSelectedFunctions()
        {
            selectedFunctionNames.Clear();
            ClearPendingConfirmation();
            SetFeedback("[Function] 선택 해제");
            ApplyAll();
        }

        private void DuplicateSelectedFunctions()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 복사를 잠근다. Stop 후 다시 복사해라.");
                return;
            }

            if (selectedFunctionNames.Count == 0)
            {
                SetFeedback("복사할 묶음을 먼저 선택해라.");
                return;
            }

            var copied = 0;
            for (var index = 0; index < selectedFunctionNames.Count; index++)
            {
                var result = runtimeController != null
                    ? runtimeController.DuplicateTeachingFunctionForDebug(selectedFunctionNames[index])
                    : "runtime missing";
                if (result.Contains("복사"))
                {
                    copied++;
                }
            }

            SetFeedback($"[Bundle] 선택 {copied}개 복사");
            ApplyAll();
        }

        private void DeleteSelectedFunctions()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

            if (selectedFunctionNames.Count == 0)
            {
                SetFeedback("삭제할 묶음을 먼저 선택해라.");
                return;
            }

            var confirmKey = string.Join("|", selectedFunctionNames);
            if (!IsPendingConfirmation("bulk-delete-function", confirmKey))
            {
                SetPendingConfirmation("bulk-delete-function", confirmKey);
                SetFeedback($"[Confirm] 묶음 {selectedFunctionNames.Count}개 삭제 예정. 선택 삭제를 한 번 더 눌러라.");
                return;
            }

            var deleted = 0;
            for (var index = 0; index < selectedFunctionNames.Count; index++)
            {
                var result = runtimeController != null
                    ? runtimeController.DeleteTeachingFunctionForDebug(selectedFunctionNames[index])
                    : "runtime missing";
                if (result.Contains("삭제"))
                {
                    deleted++;
                }
            }

            if (selectedFunctionNames.Contains(selectedFunctionName))
            {
                selectedFunctionName = string.Empty;
                SelectFirstExistingFunctionIfNeeded();
            }

            selectedFunctionNames.Clear();
            ClearPendingConfirmation();
            SetFeedback($"[Delete] 묶음 {deleted}개 삭제");
            ApplyAll();
        }

        private void DeleteAllFunctions()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 전체 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

            var count = runtimeController != null ? runtimeController.GetTeachingFunctionNames().Length : 0;
            if (count == 0)
            {
                SetFeedback("삭제할 묶음이 없다.");
                return;
            }

            if (!IsPendingConfirmation("delete-all-functions", count.ToString(CultureInfo.InvariantCulture)))
            {
                SetPendingConfirmation("delete-all-functions", count.ToString(CultureInfo.InvariantCulture));
                SetFeedback($"[Confirm] 모든 묶음 {count}개 삭제 예정. 전체 삭제를 한 번 더 눌러라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.DeleteAllTeachingFunctionsForDebug()
                : "runtime missing";
            selectedFunctionName = string.Empty;
            selectedFunctionNames.Clear();
            selectedFunctionPointNames.Clear();
            ClearPendingConfirmation();
            SetFeedback(result);
            ApplyAll();
        }

        private void SelectFunction(string functionName)
        {
            selectedFunctionName = functionName?.Trim() ?? string.Empty;
            SetFunctionName(selectedFunctionName);
            SetFeedback(string.IsNullOrWhiteSpace(selectedFunctionName)
                ? "선택된 묶음이 없다."
                : $"[Bundle] {selectedFunctionName} 선택");
            ApplyAll();
        }

        private void SetFunctionName(string functionName)
        {
            var safeName = functionName?.Trim() ?? string.Empty;
            desktopPanel?.PointFunctionNameInput?.SetValueWithoutNotify(safeName);
            tabletPanel?.PointFunctionNameInput?.SetValueWithoutNotify(safeName);
            desktopPanel?.FunctionNameInput?.SetValueWithoutNotify(safeName);
            tabletPanel?.FunctionNameInput?.SetValueWithoutNotify(safeName);
        }

        private void ApplyPointFunctionBuilder(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            var sourcePointNames = ResolveFunctionSourcePointNames();
            var sourceLabel = ResolveFunctionSourceLabel(sourcePointNames);
            if (panel.PointFunctionBuildSummary != null)
            {
                panel.PointFunctionBuildSummary.text = sourcePointNames.Length == 0
                    ? "저장 위치가 없다. 먼저 위치를 저장해라."
                    : $"{sourceLabel} {sourcePointNames.Length}개를 바로 작업 묶음에 등록한다.";
            }

            if (panel.PointFunctionSelectionSummary != null)
            {
                panel.PointFunctionSelectionSummary.text = sourcePointNames.Length == 0
                    ? "선택 소스 없음"
                    : $"{sourceLabel}: {string.Join(" / ", sourcePointNames)}";
            }
        }

        private string[] ResolveFunctionSourcePointNames()
        {
            var source = new List<string>();
            AppendExistingPointNames(source, selectedFunctionPointNames);
            if (source.Count > 0)
            {
                return source.ToArray();
            }

            AppendExistingPointNames(source, selectedPointNames);
            if (source.Count > 0)
            {
                return source.ToArray();
            }

            var panel = ResolveActivePanel();
            var currentPointName = panel?.PointNameInput?.value?.Trim() ?? string.Empty;
            if (HasNamedPoint(currentPointName))
            {
                source.Add(currentPointName);
                return source.ToArray();
            }

            if (recalledPoint != null && HasNamedPoint(recalledPoint.name))
            {
                source.Add(recalledPoint.name);
                return source.ToArray();
            }

            var sequence = LoadPointSequenceIfExists();
            var waypoints = sequence?.waypoints ?? System.Array.Empty<Waypoint>();
            for (var index = 0; index < waypoints.Length; index++)
            {
                var pointName = waypoints[index]?.name;
                if (!string.IsNullOrWhiteSpace(pointName) && !source.Contains(pointName))
                {
                    source.Add(pointName);
                }
            }

            return source.ToArray();
        }

        private string[] BuildCreateFunctionSourcePointNames()
        {
            if (selectedFunctionPointNames.Count > 0)
            {
                return ResolveFunctionSourcePointNames();
            }

            if (recalledPoint != null && HasNamedPoint(recalledPoint.name))
            {
                return new[] { recalledPoint.name };
            }

            var panel = ResolveActivePanel();
            var currentPointName = panel?.PointNameInput?.value?.Trim() ?? string.Empty;
            if (HasNamedPoint(currentPointName))
            {
                return new[] { currentPointName };
            }

            if (selectedPointNames.Count > 0)
            {
                return ResolveFunctionSourcePointNames();
            }

            return ResolveFunctionSourcePointNames();
        }

        private string ResolveFunctionSourceLabel(string[] sourcePointNames)
        {
            if (sourcePointNames == null || sourcePointNames.Length == 0)
            {
                return "선택 포인트";
            }

            if (selectedFunctionPointNames.Count > 0)
            {
                return "묶음 후보";
            }

            if (selectedPointNames.Count > 0)
            {
                return "선택 포인트";
            }

            var panel = ResolveActivePanel();
            var currentPointName = panel?.PointNameInput?.value?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(currentPointName)
                && sourcePointNames.Length == 1
                && string.Equals(sourcePointNames[0], currentPointName, System.StringComparison.OrdinalIgnoreCase))
            {
                return "현재 포인트";
            }

            if (recalledPoint != null && sourcePointNames.Length == 1 && string.Equals(sourcePointNames[0], recalledPoint.name, System.StringComparison.OrdinalIgnoreCase))
            {
                return "현재 선택";
            }

            return "전체 포인트";
        }

        private void AppendExistingPointNames(List<string> target, List<string> names)
        {
            if (target == null || names == null)
            {
                return;
            }

            for (var index = 0; index < names.Count; index++)
            {
                var pointName = names[index];
                if (string.IsNullOrWhiteSpace(pointName) || !HasNamedPoint(pointName) || target.Contains(pointName))
                {
                    continue;
                }

                target.Add(pointName);
            }
        }

        private static string ExtractCreatedFunctionName(string result, string fallbackName)
        {
            const string marker = "[Function] ";
            const string endMarker = " 생성";
            if (string.IsNullOrWhiteSpace(result))
            {
                return fallbackName?.Trim() ?? string.Empty;
            }

            var start = result.IndexOf(marker, System.StringComparison.Ordinal);
            if (start < 0)
            {
                return fallbackName?.Trim() ?? string.Empty;
            }

            start += marker.Length;
            var end = result.IndexOf(endMarker, start, System.StringComparison.Ordinal);
            return end > start
                ? result.Substring(start, end - start).Trim()
                : fallbackName?.Trim() ?? string.Empty;
        }

        private void SelectFirstExistingFunctionIfNeeded()
        {
            if (runtimeController == null)
            {
                return;
            }

            var names = runtimeController.GetTeachingFunctionNames();
            if (names.Length == 0)
            {
                selectedFunctionName = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedFunctionName) || System.Array.IndexOf(names, selectedFunctionName) < 0)
            {
                selectedFunctionName = names[0];
                SetFunctionName(selectedFunctionName);
            }
        }

        private void ApplyFunctionPanel(PanelElements panel)
        {
            if (panel?.FunctionSummary == null)
            {
                return;
            }

            SelectFirstExistingFunctionIfNeeded();
            var functionNames = runtimeController != null
                ? runtimeController.GetTeachingFunctionNames()
                : System.Array.Empty<string>();
            panel.FunctionSummary.text = string.IsNullOrWhiteSpace(selectedFunctionName)
                ? $"작업 묶음 {functionNames.Length}개"
                : $"작업 묶음 {functionNames.Length}개 · 선택 {ShortDisplayName(selectedFunctionName)}";
            if (panel.FunctionInventorySummary != null)
            {
                panel.FunctionInventorySummary.text = BuildFunctionInventorySummary(functionNames);
            }

            panel.FunctionDetail.text = !string.IsNullOrWhiteSpace(selectedFunctionName) && runtimeController != null
                ? FormatFunctionDetailForUi(runtimeController.GetTeachingFunctionDetailForDebug(selectedFunctionName))
                : "묶음을 선택하면 포함된 저장 위치가 보인다.";
            RebuildFunctionList(panel);
        }

        private static string FormatFunctionDetailForUi(string rawDetail)
        {
            if (string.IsNullOrWhiteSpace(rawDetail) || rawDetail.Contains("function=none"))
            {
                return "묶음을 선택하면 포함된 저장 위치가 보인다.";
            }

            var name = ExtractDebugValue(rawDetail, "function=");
            var steps = ExtractDebugValue(rawDetail, "steps=");
            var missingCount = ExtractDebugValue(rawDetail, "missingCount=");
            var missing = ExtractDebugBracketValue(rawDetail, "missing=[");
            return missingCount == "0" || string.IsNullOrWhiteSpace(missingCount)
                ? $"{name} · {steps}개 위치 · 누락 없음"
                : $"{name} · {steps}개 위치 · 누락 {missingCount}: {missing}";
        }

        private string BuildFunctionInventorySummary(string[] functionNames)
        {
            functionNames ??= System.Array.Empty<string>();
            var selectedDetail = !string.IsNullOrWhiteSpace(selectedFunctionName) && runtimeController != null
                ? runtimeController.GetTeachingFunctionDetailForDebug(selectedFunctionName)
                : string.Empty;
            var selectedSteps = ExtractDebugValue(selectedDetail, "steps=");
            var selectedMissing = ExtractDebugValue(selectedDetail, "missingCount=");
            var detail = string.IsNullOrWhiteSpace(selectedFunctionName)
                ? "선택 없음"
                : $"{ShortDisplayName(selectedFunctionName)} 포함 {selectedSteps}개 · 누락 {selectedMissing}";
            return $"작업 묶음 {functionNames.Length}개 · 선택 {selectedFunctionNames.Count}개 · {detail}";
        }

        private static string FormatPathRecordSummary(string rawSummary)
        {
            if (string.IsNullOrWhiteSpace(rawSummary))
            {
                return "기록: 대기 / 샘플 0개";
            }

            var recording = ExtractDebugValue(rawSummary, "recording=");
            var samples = ExtractDebugValue(rawSummary, "samples=");
            var saved = ExtractDebugValue(rawSummary, "saved=");
            var runner = ExtractDebugValue(rawSummary, "runner=");
            var state = string.Equals(recording, "True", System.StringComparison.OrdinalIgnoreCase)
                ? "기록 중"
                : "대기";
            return $"기록: {state} / 샘플 {samples}개 / 저장 {saved}개 / 재생 {runner}";
        }

        private static string ExtractDebugValue(string raw, string key)
        {
            var start = raw.IndexOf(key, System.StringComparison.Ordinal);
            if (start < 0)
            {
                return string.Empty;
            }

            start += key.Length;
            var end = raw.IndexOf(';', start);
            return end > start
                ? raw.Substring(start, end - start).Trim()
                : raw.Substring(start).Trim();
        }

        private static string ExtractDebugBracketValue(string raw, string key)
        {
            var start = raw.IndexOf(key, System.StringComparison.Ordinal);
            if (start < 0)
            {
                return string.Empty;
            }

            start += key.Length;
            var end = raw.IndexOf(']', start);
            return end >= start
                ? raw.Substring(start, end - start).Trim()
                : raw.Substring(start).Trim();
        }

        private void RebuildFunctionList(PanelElements panel)
        {
            if (panel?.FunctionListView == null)
            {
                return;
            }

            functionListItems.Clear();
            var names = runtimeController != null
                ? runtimeController.GetTeachingFunctionNames()
                : System.Array.Empty<string>();
            for (var index = 0; index < names.Length; index++)
            {
                var functionName = names[index];
                if (ShouldShowFunctionRow(functionName))
                {
                    functionListItems.Add(functionName);
                }
            }

            RefreshListView(panel.FunctionListView);
        }

        private string BuildFunctionRowSummary(string functionName)
        {
            var detail = runtimeController != null
                ? runtimeController.GetTeachingFunctionDetailForDebug(functionName)
                : string.Empty;
            var steps = ExtractDebugValue(detail, "steps=");
            var missingCount = ExtractDebugValue(detail, "missingCount=");
            var missing = string.IsNullOrWhiteSpace(missingCount) || missingCount == "0"
                ? "누락 없음"
                : $"누락 {missingCount}";
            return $"{ShortDisplayName(functionName)} · {steps}개 · {missing}";
        }

        private void ToggleTeachingLoop()
        {
            if (runtimeController == null)
            {
                SetFeedback("반복 실행 상태를 바꿀 runtime을 찾지 못했다.");
                return;
            }

            var enabled = runtimeController.ToggleTeachingLoopEnabled();
            SetFeedback(enabled
                ? "[반복] 켜짐 · 실행을 누르면 저장 위치를 반복한다."
                : "[반복] 꺼짐 · 실행은 한 번만 진행한다.");
            ApplyAll();
        }

        private void RunFromSelectedPoint()
        {
            if (!IsAnyPanelVisible())
            {
                SetFeedback("저장 위치 패널이 열려 있을 때만 선택부터 실행할 수 있다.");
                return;
            }

            if (recalledPoint == null)
            {
                SetFeedback("선택부터 실행할 저장 위치를 먼저 선택해라.");
                return;
            }

            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 선택부터 실행을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 선택부터 실행할 수 없다.");
                return;
            }

            if (!EnsurePointLiveSyncGate("선택부터 실행"))
            {
                return;
            }

            var hasSavedJointTarget = TryGetSavedJointTarget(currentValuesToDouble(), recalledPoint.name, out var savedJointTarget);
            var canRouteSavedPointThroughLiveApproval = runtimeController != null
                && hasSavedJointTarget
                && runtimeController.ShouldRouteSavedPointMoveJOperatorThroughLiveApproval();
            if (canRouteSavedPointThroughLiveApproval)
            {
                if (!EnsurePopupCoordinatorAvailableForLiveApproval("선택부터 실행"))
                {
                    ApplyAll();
                    return;
                }

                PreviewSavedMoveJ(recalledPoint.name, savedJointTarget);
                SetFeedback(runtimeController.PrepareSavedPointMoveJOperatorApproval(recalledPoint.name, savedJointTarget));
                if (runtimeController.HasPendingSavedPointOperatorApproval())
                {
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveJ"))
                    {
                        popupCoordinator.OpenMoveConfirmForProduct();
                    }
                    else
                    {
                        SetFeedback(runtimeController.ExecutePendingSavedPointOperatorCommand());
                    }
                }

                ApplyAll();
                return;
            }

            if (popupCoordinator != null
                && runtimeController != null
                && runtimeController.ShouldRouteWaypointSequenceThroughLiveApproval(PointSequenceName, loop: false))
            {
                SetFeedback(runtimeController.PrepareWaypointSequenceOperatorApproval(PointSequenceName, loop: false, recalledPoint.name));
                if (runtimeController.HasPendingWaypointSequenceOperatorApproval())
                {
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveJ"))
                    {
                        popupCoordinator.OpenRunConfirmForProduct();
                    }
                    else
                    {
                        SetFeedback(runtimeController.ExecutePendingWaypointSequenceOperatorCommand());
                    }
                }

                ApplyAll();
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteTeachingSequenceFromPoint(recalledPoint.name)
                : "runtime missing";
            SetFeedback(result);
        }

        private void ApplyLoopState(PanelElements panel)
        {
            if (panel?.BtnLoop == null)
            {
                return;
            }

            var loopEnabled = runtimeController != null && runtimeController.IsTeachingLoopEnabled;
            var running = runtimeController != null && runtimeController.IsTeachingSequenceRunning;
            var snapshot = runtimeController != null ? runtimeController.CurrentSnapshot : null;
            panel.BtnLoop.text = loopEnabled ? "반복 ON" : "반복 OFF";
            panel.BtnLoop.EnableInClassList("rc-point-loop-button--active", loopEnabled);
            if (snapshot != null && snapshot.MixedLiveLoopRunning)
            {
                panel.LoopStatus.text = $"반복 실행: {snapshot.MixedLiveLoopCycleCount}사이클 · {snapshot.MixedLiveLoopTarget} · gripper {snapshot.MixedLiveLoopGripperIntent}";
                return;
            }

            panel.LoopStatus.text = loopEnabled
                ? running ? "반복 실행: 진행 중 · Stop으로 종료" : "반복 실행: 켜짐 · Run으로 시작"
                : "반복 실행: 꺼짐";
        }

        private void HandleDwellChanged(string rawValue)
        {
            if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                || double.IsNaN(parsed)
                || double.IsInfinity(parsed)
                || parsed < 0.0
                || parsed > 600.0)
            {
                isDwellInvalid = true;
                ApplyPanel(desktopPanel);
                ApplyPanel(tabletPanel);
                return;
            }

            selectedDwellSec = parsed;
            isDwellInvalid = false;
            ClearPendingConfirmation();
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
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

            pointMovePanelHost?.EnableInClassList("rc-hidden", !isDesktopVisible);
            pointMoveSheetHost?.EnableInClassList("rc-hidden", !isTabletVisible);
        }

        private static bool ShouldShowDesktopPanel(string activeNavSection, string activeWorkTab)
        {
            return activeNavSection == "NavPoints";
        }

        private static bool ShouldShowTabletPanel(string activeNavSection, string activeTabletTab)
        {
            return activeNavSection == "NavPoints" || activeTabletTab == "BottomTabPointMove";
        }

        private void SetCoordSystem(string coordSystem)
        {
            activeCoordSystem = coordSystem is "Tool" or "User" ? coordSystem : "Base";
            var shellState = GetComponent<PendantV3ShellStateController>();
            if (shellState != null)
            {
                shellState.SetCoordSystemSelection(activeCoordSystem);
                return;
            }

            var localState = GetLocalState();
            localState.CoordSystem = activeCoordSystem;
            LocalSettingsStore.Save(localState);
            ApplyAll();
        }

        private void SetMotionKind(string nextMotionKind)
        {
            motionKind = nextMotionKind == "MoveL" ? "MoveL" : "MoveJ";
            ApplyAll();
        }

        private void RestoreFromPreview()
        {
            if (!CanPreview() || !IsAnyPanelVisible())
            {
                SetFeedback("연결이 준비될 때까지 현재값 복원을 잠시 잠근다.");
                return;
            }

            isPointNameInvalid = false;
            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            SetFeedback("현재 preview TCP 값으로 다시 채웠다.");
        }

        private void HandleValueChanged(int index, string rawValue)
        {
            if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return;
            }

            currentValues[index] = parsed;
            recalledPoint = null;
            lastInvalidIndex = -1;
            isPointNameInvalid = false;
            ClearPendingConfirmation();
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
            PreviewEditedPointCandidate();
        }

        private void PreviewEditedPointCandidate()
        {
            if (!IsAnyPanelVisible() || !CanPreview())
            {
                return;
            }

            if (!TryReadActivePanelValues(out var _, out _))
            {
                return;
            }

            var pointName = (isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel)?.PointNameInput?.value ?? "Point";
            if (motionKind == "MoveL")
            {
                runtimeController?.PreviewTcpPose(currentValuesToDouble(), $"저장 위치 {pointName} 직선 이동 입력 미리보기");
                return;
            }

            if (TryGetSavedJointTarget(currentValuesToDouble(), pointName, out var savedJointTarget))
            {
                PreviewSavedMoveJ(pointName, savedJointTarget);
                return;
            }

            runtimeController?.PreviewPointMoveJ(currentValuesToDouble(), $"저장 위치 {pointName} 관절 이동 입력 미리보기");
        }

        private void DispatchEditedPointCandidate()
        {
            if (!IsAnyPanelVisible() || runtimeController == null)
            {
                return;
            }

            if (!TryReadActivePanelValues(out var target, out _))
            {
                return;
            }

            if (!runtimeController.CurrentSnapshot.DryRunEnabled)
            {
                PreviewEditedPointCandidate();
                return;
            }

            var pointName = (isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel)?.PointNameInput?.value ?? "Point";
            if (motionKind == "MoveL")
            {
                runtimeController.ApplyTcpPose(target, $"저장 위치 {pointName} 직선 이동 입력 적용");
                SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
                return;
            }

            var result = TryGetSavedJointTarget(target, pointName, out var savedJointTarget)
                ? ApplySavedMoveJ(pointName, savedJointTarget)
                : runtimeController.ApplyPointMoveJ(target, $"저장 위치 {pointName} 관절 이동 입력 적용");
            SetFeedback(result.IsSuccess ? runtimeController.CurrentSnapshot.LastFeedback : result.Message);
        }

        private void PreviewMotionCandidate()
        {
            if (!IsAnyPanelVisible())
            {
                SetFeedback("저장 위치 패널이 열려 있을 때만 미리보기를 실행한다.");
                return;
            }

            if (!CanPreview())
            {
                SetFeedback("연결 상태가 준비되지 않아 미리보기를 잠시 잠근다.");
                return;
            }

            if (!TryReadActivePanelValues(out var _, out var validationMessage))
            {
                SetFeedback(validationMessage);
                return;
            }

            var pointName = desktopPanel?.PointNameInput?.value ?? tabletPanel?.PointNameInput?.value ?? "Point";
            if (motionKind == "MoveL")
            {
                runtimeController?.PreviewTcpPose(currentValuesToDouble(), $"저장 위치 {pointName} 직선 이동 후보");
                SetFeedback($"[미리보기] 직선 이동 후보 · {pointName} · X {currentValues[0]:0.0} / Y {currentValues[1]:0.0} / Z {currentValues[2]:0.0}");
                return;
            }

            var result = TryGetSavedJointTarget(currentValuesToDouble(), pointName, out var savedJointTarget)
                ? PreviewSavedMoveJ(pointName, savedJointTarget)
                : runtimeController?.PreviewPointMoveJ(currentValuesToDouble(), $"저장 위치 {pointName} 관절 이동 후보")
                    ?? FairinoResult.Fail(-1, "관절 이동 준비 기능을 찾지 못했다.");
            SetFeedback(result.IsSuccess
                ? $"[미리보기] 관절 이동 후보 · {pointName} · {result.Message}"
                : result.Message);
        }

        private void ApplyMotionCandidate()
        {
            if (!IsAnyPanelVisible())
            {
                SetFeedback("저장 위치 패널이 열려 있을 때만 적용할 수 있다.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 적용할 수 없다. 연결/에러 상태를 먼저 확인해라.");
                return;
            }

            if (!TryReadActivePanelValues(out var target, out var validationMessage))
            {
                SetFeedback(validationMessage);
                return;
            }

            if (!EnsurePointLiveSyncGate("포인트 적용"))
            {
                return;
            }

            if (motionKind != "MoveL")
            {
                var pointName = (isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel)?.PointNameInput?.value ?? "Point";
                var hasSavedJointTarget = TryGetSavedJointTarget(target, pointName, out var savedJointTarget);
                var shouldRouteSavedPointThroughLiveApproval = runtimeController != null
                    && hasSavedJointTarget
                    && runtimeController.ShouldRouteSavedPointMoveJOperatorThroughLiveApproval();
                if (shouldRouteSavedPointThroughLiveApproval)
                {
                    if (!EnsurePopupCoordinatorAvailableForLiveApproval("포인트 적용"))
                    {
                        return;
                    }

                    var previewResult = PreviewSavedMoveJ(pointName, savedJointTarget);
                    if (!previewResult.IsSuccess)
                    {
                        SetFeedback(previewResult.Message);
                        return;
                    }

                    runtimeController.PrepareSavedPointMoveJOperatorApproval(pointName, savedJointTarget);
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveJ"))
                    {
                        popupCoordinator.OpenMoveConfirmForProduct();
                        SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
                    }
                    else
                    {
                        SetFeedback(runtimeController.ExecutePendingSavedPointOperatorCommand());
                    }
                    return;
                }

                if (runtimeController != null
                    && runtimeController.ShouldRouteMoveJOperatorThroughLiveApproval())
                {
                    if (!EnsurePopupCoordinatorAvailableForLiveApproval("포인트 적용"))
                    {
                        return;
                    }

                    var previewResult = runtimeController.PreviewPointMoveJ(target, $"저장 위치 {pointName} 관절 이동 후보");
                    if (!previewResult.IsSuccess)
                    {
                        SetFeedback(previewResult.Message);
                        return;
                    }

                    runtimeController.PrepareMoveJOperatorApprovalSession();
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveJ"))
                    {
                        popupCoordinator.OpenMoveConfirmForProduct();
                        SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
                    }
                    else
                    {
                        runtimeController.ExecutePreparedPreviewForProduct();
                        SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
                    }
                    return;
                }

                var moveJResult = hasSavedJointTarget
                    ? ApplySavedMoveJ(pointName, savedJointTarget)
                    : runtimeController?.ApplyPointMoveJ(target, "저장 위치 관절 이동 적용")
                        ?? FairinoResult.Fail(-1, "관절 이동 준비 기능을 찾지 못했다.");
                SetFeedback(moveJResult.IsSuccess
                    ? runtimeController.CurrentSnapshot.LastFeedback
                    : moveJResult.Message);
                return;
            }

            runtimeController?.ApplyTcpPose(target, "저장 위치 직선 이동 적용");
            SetFeedback(runtimeController != null ? runtimeController.CurrentSnapshot.LastFeedback : "[실행] 직선 이동 적용 요청");
            return;
        }

        private FairinoResult<RobotControlMotionRuntime> EnsureMotionRuntime()
        {
            var robotId = RobotSelectionBridge.GetSelectedRobotId();
            if (string.IsNullOrWhiteSpace(robotId))
            {
                motionRuntime = null;
                return FairinoResult<RobotControlMotionRuntime>.Fail(-1, "선택된 로봇이 없어서 PointMove runtime을 준비하지 못했다.");
            }

            if (motionRuntime != null && string.Equals(motionRuntime.RobotId, robotId, System.StringComparison.Ordinal))
            {
                return FairinoResult<RobotControlMotionRuntime>.Ok(motionRuntime, $"{robotId} runtime 재사용");
            }

            var createResult = RobotControlMotionRuntime.CreateFromSelection();
            if (!createResult.IsSuccess)
            {
                motionRuntime = null;
                return createResult;
            }

            motionRuntime = createResult.Value;
            return createResult;
        }

        private void SetFeedback(string message)
        {
            lastFeedback = string.IsNullOrWhiteSpace(message) ? "..." : message;
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
        }

        private bool ShouldShowFeedbackLine()
        {
            return !string.IsNullOrWhiteSpace(lastFeedback)
                && lastFeedback != "아직 실행한 명령이 없다."
                && (lastFeedback.Contains("[Sequence")
                    || lastFeedback.Contains("[Path")
                    || lastFeedback.Contains("[미리보기]")
                    || lastFeedback.Contains("프리뷰")
                    || lastFeedback.Contains("live")
                    || lastFeedback.Contains("[Confirm]")
                    || lastFeedback.Contains("[Delete]")
                    || lastFeedback.Contains("[Save]")
                    || lastFeedback.Contains("[Bulk]")
                    || lastFeedback.Contains("[Function]")
                    || lastFeedback.Contains("[Bundle]")
                    || lastFeedback.Contains("실패")
                    || lastFeedback.Contains("찾지 못했다")
                    || lastFeedback.Contains("먼저"));
        }

        private static string CompactFeedback(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            var trimmed = message.Trim();
            return trimmed.Length <= 90 ? trimmed : trimmed.Substring(0, 87) + "...";
        }

        private void SaveCurrentPoint()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("실행 중에는 저장 위치 편집을 잠근다. 정지 후 다시 저장해라.");
                return;
            }

            if (!IsAnyPanelVisible())
            {
                SetFeedback("저장 위치 패널이 열려 있을 때만 저장할 수 있다.");
                return;
            }

            if (!TryReadActivePointName(out var pointName, out var validationMessage))
            {
                SetFeedback(validationMessage);
                return;
            }

            if (!EnsurePointLiveSyncGate("포인트 저장"))
            {
                return;
            }

            var sequence = LoadPointSequenceIfExists() ?? WaypointStore.CreateEmpty(PointSequenceName);
            var existingIndex = FindWaypointIndex(sequence, pointName);
            var existingPoint = existingIndex >= 0 ? sequence.waypoints[existingIndex] : null;
            var waypoint = new Waypoint
            {
                name = pointName,
                jointsDeg = ReadCurrentSnapshotJoints(),
                tcpMm = ReadCurrentSnapshotTcp(),
                moveType = motionKind,
                speedPreset = NormalizeSpeedPreset(existingPoint?.speedPreset ?? "medium"),
                dwellSec = existingPoint?.dwellSec ?? 0.0
            };

            if (existingIndex >= 0 && !IsPendingConfirmation("save-overwrite", pointName))
            {
                SetPendingConfirmation("save-overwrite", pointName);
                SetFeedback($"[Confirm] {pointName} 이름이 이미 있다. 같은 이름으로 저장하려면 저장을 한 번 더 눌러 기존 위치를 덮어쓴다.");
                return;
            }

            ReplaceWaypoint(sequence, waypoint);
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("저장 위치 저장 실패");
                return;
            }

            ClearPendingConfirmation();
            recalledPoint = CloneWaypoint(waypoint);
            SetFeedback($"[저장] {pointName} 저장 · 관절값 포함");
        }

        private bool EnsurePointLiveSyncGate(string actionLabel)
        {
            if (runtimeController == null)
            {
                SetFeedback($"{actionLabel} 전에 runtime을 찾지 못했다.");
                return false;
            }

            if (runtimeController.CurrentSnapshot.DryRunEnabled)
            {
                return true;
            }

            var syncResult = runtimeController.SyncCurrentState();
            if (!syncResult.IsSuccess)
            {
                SetFeedback($"[{actionLabel}] 현재 위치 읽기 실패 · {syncResult.Message}");
                return false;
            }

            var evidenceSummary = runtimeController.RefreshLiveEvidenceForDebug();
            if (!runtimeController.HasStableLiveEvidenceForDebug())
            {
                SetFeedback($"[{actionLabel}] 최신 위치 sync 확인이 아직 안정되지 않았다. {evidenceSummary}");
                return false;
            }

            return true;
        }

        private void RecallPoint(string requestedName)
        {
            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("저장 위치가 없다.");
                return;
            }

            var pointName = string.IsNullOrWhiteSpace(requestedName)
                ? sequence.waypoints[0].name
                : requestedName.Trim();
            var waypoint = FindWaypoint(sequence, pointName) ?? sequence.waypoints[0];
            recalledPoint = CloneWaypoint(waypoint);
            SetPointName(recalledPoint.name);
            motionKind = recalledPoint.moveType == "MoveL" ? "MoveL" : "MoveJ";
            selectedSpeedPreset = NormalizeSpeedPreset(recalledPoint.speedPreset);
            selectedDwellSec = recalledPoint.dwellSec;
            isDwellInvalid = false;
            ClearPendingConfirmation();
            for (var index = 0; index < currentValues.Length && index < recalledPoint.tcpMm.Length; index++)
            {
                currentValues[index] = (float)recalledPoint.tcpMm[index];
            }

            SetFeedback($"[불러오기] {recalledPoint.name} · {ToMotionLabel(motionKind)}");
        }

        private void DeletePoint(string requestedName)
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("삭제할 저장 위치가 없다.");
                return;
            }

            var pointName = string.IsNullOrWhiteSpace(requestedName)
                ? recalledPoint?.name
                : requestedName.Trim();
            var index = FindWaypointIndex(sequence, pointName);
            if (index < 0)
            {
                SetFeedback($"{pointName} 포인트를 찾지 못했다.");
                return;
            }

            if (!IsPendingConfirmation("delete", pointName))
            {
                SetPendingConfirmation("delete", pointName);
                SetFeedback($"[Confirm] {pointName} 삭제 예정. 이 포인트는 순서 목록에서 제거된다. 삭제를 한 번 더 누르면 실행한다.");
                return;
            }

            var deletedName = sequence.waypoints[index].name;
            WaypointStore.RemoveAt(sequence, index);
            if (sequence.waypoints.Length == 0)
            {
                WaypointStore.Delete(PointSequenceName);
            }
            else
            {
                WaypointStore.Save(sequence);
            }

            if (recalledPoint != null && string.Equals(recalledPoint.name, deletedName, System.StringComparison.OrdinalIgnoreCase))
            {
                recalledPoint = null;
            }

            selectedPointNames.Remove(deletedName);
            ClearPendingConfirmation();
            SetFeedback($"[Delete] {deletedName} 삭제");
        }

        private void RenamePoint(string oldName, string newName)
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 이름 변경을 잠근다. Stop 후 다시 수정해라.");
                return;
            }

            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("이름을 바꿀 저장 포인트가 없다.");
                return;
            }

            var fromName = string.IsNullOrWhiteSpace(oldName) ? recalledPoint?.name : oldName.Trim();
            var toName = string.IsNullOrWhiteSpace(newName) ? string.Empty : newName.Trim();
            if (string.IsNullOrWhiteSpace(fromName) || string.IsNullOrWhiteSpace(toName))
            {
                SetFeedback("이름 변경은 선택된 포인트와 새 이름이 필요하다.");
                return;
            }

            var oldIndex = FindWaypointIndex(sequence, fromName);
            if (oldIndex < 0)
            {
                SetFeedback($"{fromName} 포인트를 찾지 못했다.");
                return;
            }

            var duplicateIndex = FindWaypointIndex(sequence, toName);
            if (duplicateIndex >= 0 && duplicateIndex != oldIndex)
            {
                SetFeedback($"{toName} 이름이 이미 있다.");
                return;
            }

            sequence.waypoints[oldIndex].name = toName;
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("포인트 이름 변경 저장 실패");
                return;
            }

            var selectedIndex = selectedPointNames.IndexOf(fromName);
            if (selectedIndex >= 0)
            {
                selectedPointNames[selectedIndex] = toName;
            }

            ClearPendingConfirmation();
            recalledPoint = CloneWaypoint(sequence.waypoints[oldIndex]);
            SetPointName(toName);
            SetFeedback($"[Rename] {fromName} -> {toName}");
        }

        private void DuplicateSelectedPoint()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 복사를 잠근다. Stop 후 다시 복사해라.");
                return;
            }

            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0 || recalledPoint == null)
            {
                SetFeedback("복사할 저장 포인트를 먼저 선택해라.");
                return;
            }

            var sourceIndex = FindWaypointIndex(sequence, recalledPoint.name);
            if (sourceIndex < 0)
            {
                SetFeedback($"{recalledPoint.name} 포인트를 찾지 못했다.");
                return;
            }

            var duplicate = CloneWaypoint(sequence.waypoints[sourceIndex]);
            duplicate.name = BuildUniqueDuplicateName(sequence, duplicate.name);
            InsertWaypointAfter(sequence, duplicate, sourceIndex);
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("포인트 복사 저장 실패");
                return;
            }

            ClearPendingConfirmation();
            recalledPoint = CloneWaypoint(duplicate);
            SetPointName(recalledPoint.name);
            for (var valueIndex = 0; valueIndex < currentValues.Length && valueIndex < recalledPoint.tcpMm.Length; valueIndex++)
            {
                currentValues[valueIndex] = (float)recalledPoint.tcpMm[valueIndex];
            }

            SetFeedback($"[Duplicate] {sequence.waypoints[sourceIndex].name} -> {recalledPoint.name}");
        }

        private void MovePointInSequence(int direction)
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 순서 변경을 잠근다. Stop 후 다시 이동해라.");
                return;
            }

            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("순서를 바꿀 저장 포인트가 없다.");
                return;
            }

            var index = FindWaypointIndex(sequence, recalledPoint?.name);
            var targetIndex = index + (direction < 0 ? -1 : 1);
            if (index < 0 || targetIndex < 0 || targetIndex >= sequence.waypoints.Length)
            {
                SetFeedback("이 방향으로 더 이동할 수 없다.");
                return;
            }

            var temp = sequence.waypoints[index];
            sequence.waypoints[index] = sequence.waypoints[targetIndex];
            sequence.waypoints[targetIndex] = temp;
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("포인트 순서 저장 실패");
                return;
            }

            ClearPendingConfirmation();
            recalledPoint = CloneWaypoint(sequence.waypoints[targetIndex]);
            SetFeedback($"[Order] {recalledPoint.name} {targetIndex + 1}번째로 이동");
        }

        private void OverwriteSelectedPointWithCurrentReadback()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 덮어쓰기를 잠근다. Stop 후 다시 덮어써라.");
                return;
            }

            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0 || recalledPoint == null)
            {
                SetFeedback("덮어쓸 저장 포인트를 먼저 선택해라.");
                return;
            }

            var index = FindWaypointIndex(sequence, recalledPoint.name);
            if (index < 0)
            {
                SetFeedback($"{recalledPoint.name} 포인트를 찾지 못했다.");
                return;
            }

            if (!IsPendingConfirmation("overwrite", recalledPoint.name))
            {
                SetPendingConfirmation("overwrite", recalledPoint.name);
                SetFeedback($"[Confirm] {recalledPoint.name} 현재 readback으로 덮어쓰기 예정. 이름/MoveType/speed/dwell은 유지하고 joints/TCP만 바뀐다. 덮어쓰기를 한 번 더 눌러라.");
                return;
            }

            var waypoint = sequence.waypoints[index];
            waypoint.jointsDeg = ReadCurrentSnapshotJoints();
            waypoint.tcpMm = ReadCurrentSnapshotTcp();
            sequence.waypoints[index] = waypoint;
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("포인트 덮어쓰기 저장 실패");
                return;
            }

            recalledPoint = CloneWaypoint(waypoint);
            for (var valueIndex = 0; valueIndex < currentValues.Length && valueIndex < recalledPoint.tcpMm.Length; valueIndex++)
            {
                currentValues[valueIndex] = (float)recalledPoint.tcpMm[valueIndex];
            }

            ClearPendingConfirmation();
            SetFeedback($"[Overwrite] {recalledPoint.name} 현재 readback으로 갱신");
        }

        private void ApplySelectedPointTiming()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 포인트 편집을 잠근다. Stop 후 다시 수정해라.");
                return;
            }

            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0 || recalledPoint == null)
            {
                SetFeedback("속도/대기를 수정할 포인트를 먼저 선택해라.");
                return;
            }

            if (isDwellInvalid || selectedDwellSec < 0.0 || selectedDwellSec > 600.0)
            {
                isDwellInvalid = true;
                ApplyAll();
                SetFeedback("대기 시간은 0~600초 사이 숫자로 넣어라.");
                return;
            }

            var index = FindWaypointIndex(sequence, recalledPoint.name);
            if (index < 0)
            {
                SetFeedback($"{recalledPoint.name} 포인트를 찾지 못했다.");
                return;
            }

            var waypoint = sequence.waypoints[index];
            waypoint.speedPreset = NormalizeSpeedPreset(selectedSpeedPreset);
            waypoint.dwellSec = selectedDwellSec;
            sequence.waypoints[index] = waypoint;
            if (!WaypointStore.Save(sequence))
            {
                SetFeedback("속도/대기 저장 실패");
                return;
            }

            ClearPendingConfirmation();
            recalledPoint = CloneWaypoint(waypoint);
            SetFeedback($"[Timing] {recalledPoint.name} · {recalledPoint.speedPreset} · dwell {recalledPoint.dwellSec:0.0}s");
        }

        private void ExportPoints()
        {
            var sequence = LoadPointSequenceIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                SetFeedback("내보낼 저장 포인트가 없다.");
                return;
            }

            var exportPath = System.IO.Path.Combine(WaypointStore.GetStoragePath(), $"{PointSequenceName}.export.json");
            if (!WaypointStore.ExportToFile(sequence, exportPath))
            {
                SetFeedback("포인트 내보내기 실패");
                return;
            }

            SetFeedback($"[Export] {sequence.waypoints.Length}개 -> {exportPath}");
        }

        private void CleanupPoints()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 정리를 잠근다. Stop 후 다시 정리해라.");
                return;
            }

            var sequence = LoadPointSequenceIfExists();
            var count = sequence?.waypoints?.Length ?? 0;
            if (count == 0)
            {
                SetFeedback("정리할 저장 포인트가 없다.");
                return;
            }

            WaypointStore.Delete(PointSequenceName);
            recalledPoint = null;
            selectedPointNames.Clear();
            SetFeedback($"[정리] 저장 위치 {count}개 정리");
        }

        private FairinoResult PreviewSavedMoveJ(string pointName, double[] savedJointTarget)
        {
            runtimeController?.PreviewJointAngles(savedJointTarget, $"저장 위치 {pointName} 저장된 관절 이동 후보");
            return FairinoResult.Ok("저장된 관절값 사용");
        }

        private FairinoResult ApplySavedMoveJ(string pointName, double[] savedJointTarget)
        {
            return runtimeController?.ApplyTeachingMoveJ(savedJointTarget, $"저장 위치 {pointName} 저장된 관절 이동 적용")
                ?? FairinoResult.Fail(-1, "관절 이동 준비 기능을 찾지 못했다.");
        }

        private bool EnsurePopupCoordinatorAvailableForLiveApproval(string actionLabel)
        {
            popupCoordinator ??= GetComponent<PopupCoordinatorV3>();
            popupCoordinator ??= GetComponentInParent<PopupCoordinatorV3>();
            popupCoordinator ??= Object.FindFirstObjectByType<PopupCoordinatorV3>();
            if (popupCoordinator != null)
            {
                return true;
            }

            SetFeedback($"[{actionLabel}] 실행 확인 팝업을 찾지 못했다. V3 화면을 다시 열고 시도해라.");
            return false;
        }

        private bool TryGetSavedJointTarget(double[] targetTcp, string pointName, out double[] savedJointTarget)
        {
            savedJointTarget = null;
            if (recalledPoint == null)
            {
                return false;
            }

            var safePointName = string.IsNullOrWhiteSpace(pointName)
                ? recalledPoint.name
                : pointName.Trim();
            if (!string.Equals(recalledPoint.name, safePointName, System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (recalledPoint.jointsDeg == null || recalledPoint.jointsDeg.Length < 6)
            {
                return false;
            }

            savedJointTarget = (double[])recalledPoint.jointsDeg.Clone();
            return true;
        }

        private bool TryReadActivePanelValues(out double[] target, out string validationMessage)
        {
            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            target = new double[currentValues.Length];
            validationMessage = "입력 검증 통과";
            lastInvalidIndex = -1;
            isPointNameInvalid = false;

            if (panel == null)
            {
                validationMessage = "저장 위치 패널을 찾지 못했다.";
                return false;
            }

            var pointName = panel.PointNameInput?.value?.Trim();
            if (string.IsNullOrWhiteSpace(pointName))
            {
                isPointNameInvalid = true;
                ApplyPanel(desktopPanel);
                ApplyPanel(tabletPanel);
                validationMessage = "위치 이름을 먼저 넣어라.";
                return false;
            }

            for (var index = 0; index < panel.ValueInputs.Length && index < target.Length; index++)
            {
                var rawValue = panel.ValueInputs[index].value;
                if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                    || double.IsNaN(parsed)
                    || double.IsInfinity(parsed))
                {
                    lastInvalidIndex = index;
                    ApplyPanel(desktopPanel);
                    ApplyPanel(tabletPanel);
                    validationMessage = $"{GetAxisLabel(index)} 값 형식을 확인해라.";
                    return false;
                }

                if (index >= 3 && System.Math.Abs(parsed) > 360.0)
                {
                    lastInvalidIndex = index;
                    ApplyPanel(desktopPanel);
                    ApplyPanel(tabletPanel);
                    validationMessage = $"{GetAxisLabel(index)} 는 -360°~360° 범위 안으로 넣어라.";
                    return false;
                }

                target[index] = parsed;
            }

            for (var index = 0; index < currentValues.Length && index < target.Length; index++)
            {
                currentValues[index] = (float)target[index];
            }

            return true;
        }

        private bool TryReadActivePointName(out string pointName, out string validationMessage)
        {
            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            pointName = panel?.PointNameInput?.value?.Trim() ?? string.Empty;
            validationMessage = "위치 이름 검증 통과";
            isPointNameInvalid = false;

            if (panel == null)
            {
                validationMessage = "저장 위치 패널을 찾지 못했다.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(pointName))
            {
                isPointNameInvalid = true;
                ApplyPanel(desktopPanel);
                ApplyPanel(tabletPanel);
                validationMessage = "위치 이름을 먼저 넣어라.";
                return false;
            }

            return true;
        }

        private string BuildDeltaSummary(string pointName)
        {
            var preview = connectionHomeController.CurrentPreviewDefinition;
            if (preview.TcpValues == null || preview.TcpValues.Length < 3)
            {
                return $"미리보기로 {pointName} 위치를 먼저 확인한 뒤 실제 이동을 보낸다.";
            }

            var dx = currentValues[0] - ParseValue(preview.TcpValues[0]);
            var dy = currentValues[1] - ParseValue(preview.TcpValues[1]);
            var dz = currentValues[2] - ParseValue(preview.TcpValues[2]);
            return $"미리보기 ΔTCP · X {dx:+0.0;-0.0;0.0} / Y {dy:+0.0;-0.0;0.0} / Z {dz:+0.0;-0.0;0.0}";
        }

        private string BuildStoreSummary()
        {
            var sequence = LoadPointSequenceIfExists();
            var count = sequence?.waypoints?.Length ?? 0;
            if (count == 0)
            {
                return "저장 위치: 없음";
            }

            var active = recalledPoint != null ? $" / 선택: {recalledPoint.name}" : string.Empty;
            return $"저장 위치: {count}개{active}";
        }

        private string BuildPointInventorySummary()
        {
            var sequence = LoadPointSequenceIfExists();
            var waypoints = sequence?.waypoints ?? System.Array.Empty<Waypoint>();
            var slow = 0;
            var medium = 0;
            var fast = 0;
            for (var index = 0; index < waypoints.Length; index++)
            {
                switch (NormalizeSpeedPreset(waypoints[index]?.speedPreset))
                {
                    case "slow":
                        slow++;
                        break;
                    case "fast":
                        fast++;
                        break;
                    default:
                        medium++;
                        break;
                }
            }

            var functionCount = runtimeController != null ? runtimeController.GetTeachingFunctionNames().Length : 0;
            return $"저장 위치 {waypoints.Length}개 · 묶음 {functionCount}개 · 느림 {slow} / 중간 {medium} / 빠름 {fast} · 선택 {selectedPointNames.Count}개";
        }
    }
}
