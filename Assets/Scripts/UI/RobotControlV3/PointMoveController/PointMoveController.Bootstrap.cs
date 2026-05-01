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
    }
}
