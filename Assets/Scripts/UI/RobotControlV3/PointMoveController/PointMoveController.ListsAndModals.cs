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
        private void RebuildPointList(PanelElements panel)
        {
            if (panel?.PointListView == null)
            {
                return;
            }

            pointListItems.Clear();
            var sequence = LoadPointSequenceIfExists();
            var waypoints = sequence?.waypoints ?? System.Array.Empty<Waypoint>();
            for (var index = 0; index < waypoints.Length; index++)
            {
                var waypoint = waypoints[index];
                if (waypoint != null && ShouldShowPointRow(waypoint))
                {
                    pointListItems.Add(waypoint);
                }
            }

            RefreshListView(panel.PointListView);
        }

        private void ApplySequencePanel(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            if (!HasNamedSequence(selectedSequenceName))
            {
                selectedSequenceName = PointSequenceName;
            }

            if (panel.SequenceLibrarySummary != null)
            {
                panel.SequenceLibrarySummary.text = BuildSequenceLibraryUiSummary();
            }

            if (panel.SequenceInventorySummary != null)
            {
                panel.SequenceInventorySummary.text = BuildSequenceInventorySummary();
            }

            if (panel.SelectedSequenceDetail != null)
            {
                panel.SelectedSequenceDetail.text = BuildSelectedSequenceDetail();
            }

            if (panel.BlockSequenceSummary != null)
            {
                panel.BlockSequenceSummary.text = FormatBlockSequenceSummary(runtimeController?.GetTeachingBlockSequenceSummaryForDebug());
            }

            RebuildBlockSequenceList(panel);
            RebuildSequenceList(panel);
        }

        private void RebuildBlockSequenceList(PanelElements panel)
        {
            if (panel?.BlockSequenceListView == null)
            {
                return;
            }

            blockSequenceListItems.Clear();
            var store = new TeachingBlockSequenceStore();
            var blocks = store.LoadOrCreate().blocks ?? System.Array.Empty<TeachingSequenceBlock>();
            for (var index = 0; index < blocks.Length; index++)
            {
                var block = blocks[index];
                if (block != null)
                {
                    blockSequenceListItems.Add(new BlockSequenceListItem(block, index));
                }
            }

            RefreshListView(panel.BlockSequenceListView);
        }

        private static string BuildBlockRowSummary(TeachingSequenceBlock block, int index)
        {
            var label = string.Equals(block.kind, TeachingSequenceBlock.BundleRefKind, System.StringComparison.OrdinalIgnoreCase)
                ? "묶음"
                : "저장 위치";
            return $"{index + 1}. {label} · {ShortDisplayName(block.refName)}";
        }

        private static string FormatBlockSequenceSummary(string rawSummary)
        {
            if (string.IsNullOrWhiteSpace(rawSummary))
            {
                return "블록 0개 · 펼친 저장 위치 0개";
            }

            var blocks = ExtractDebugValue(rawSummary, "blocks=");
            var expanded = ExtractDebugValue(rawSummary, "expanded=");
            var runner = ExtractDebugValue(rawSummary, "runner=");
            return $"블록 {blocks}개 · 펼친 저장 위치 {expanded}개 · 재생 {runner}";
        }

        private void RebuildSequenceList(PanelElements panel)
        {
            if (panel?.SequenceListView == null)
            {
                return;
            }

            sequenceListItems.Clear();
            var names = BuildOrderedSequenceNames();
            for (var index = 0; index < names.Count; index++)
            {
                var sequenceName = names[index];
                if (ShouldShowSequenceRow(sequenceName))
                {
                    sequenceListItems.Add(sequenceName);
                }
            }

            RefreshListView(panel.SequenceListView);
        }

        private string BuildSequenceLibraryUiSummary()
        {
            var pointCount = CountSequenceWaypoints(PointSequenceName);
            var recordedCount = CountSequenceWaypoints(RecordedPathSequenceName);
            var names = WaypointStore.LoadAllNames();
            var otherCount = 0;
            for (var index = 0; index < names.Length; index++)
            {
                if (!string.Equals(names[index], PointSequenceName, System.StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(names[index], RecordedPathSequenceName, System.StringComparison.OrdinalIgnoreCase))
                {
                    otherCount++;
                }
            }

            return $"저장한 위치 순서 {pointCount}개 / 기록한 경로 {recordedCount}개 / 기타 {otherCount}개";
        }

        private string BuildSequenceInventorySummary()
        {
            var names = BuildOrderedSequenceNames();
            var deletable = 0;
            var totalWaypoints = 0;
            for (var index = 0; index < names.Count; index++)
            {
                if (!string.Equals(names[index], PointSequenceName, System.StringComparison.OrdinalIgnoreCase))
                {
                    deletable++;
                }

                totalWaypoints += CountSequenceWaypoints(names[index]);
            }

            return $"실행 목록 {names.Count}개 · 삭제 가능 {deletable}개 · 총 위치 {totalWaypoints}개 · 선택 {selectedSequenceNames.Count}개";
        }

        private string BuildSelectedSequenceDetail()
        {
            var sequence = LoadSequenceIfExists(selectedSequenceName);
            var count = sequence?.waypoints?.Length ?? 0;
            return $"{GetSequenceDisplayName(selectedSequenceName)} · {count}개 위치";
        }

        private string BuildSequenceRowSummary(string sequenceName)
        {
            var sequence = LoadSequenceIfExists(sequenceName);
            var count = sequence?.waypoints?.Length ?? 0;
            var first = count > 0 ? sequence.waypoints[0]?.name : "-";
            var last = count > 1 ? sequence.waypoints[count - 1]?.name : first;
            return $"{GetSequenceDisplayName(sequenceName)} · {count}개 · {ShortDisplayName(first)} → {ShortDisplayName(last)}";
        }

        private string BuildSequenceLibraryDebugSummary()
        {
            var names = BuildOrderedSequenceNames();
            var parts = new string[names.Count];
            for (var index = 0; index < names.Count; index++)
            {
                parts[index] = $"{names[index]}:{CountSequenceWaypoints(names[index])}";
            }

            return $"selectedSequence={selectedSequenceName}; selectedSequences={selectedSequenceNames.Count}; visibleSequences={sequenceListItems.Count}; sequenceFilter={sequenceFilter}; collapsed={sequenceRowActionsCollapsed}; pointCount={CountSequenceWaypoints(PointSequenceName)}; recordedPathCount={CountSequenceWaypoints(RecordedPathSequenceName)}; {BuildSequenceLibraryUiSummary()}; inventory=[{BuildSequenceInventorySummary()}]; sequences=[{string.Join(",", parts)}]; feedback={lastFeedback}";
        }

        private int CountDeletableSelectedSequences()
        {
            return BuildDeletableSelectedSequences().Count;
        }

        private List<string> BuildDeletableSelectedSequences()
        {
            var result = new List<string>();
            for (var index = 0; index < selectedSequenceNames.Count; index++)
            {
                var name = selectedSequenceNames[index];
                if (!string.Equals(name, PointSequenceName, System.StringComparison.OrdinalIgnoreCase)
                    && HasNamedSequence(name))
                {
                    result.Add(name);
                }
            }

            return result;
        }

        private static List<string> BuildOrderedSequenceNames()
        {
            var result = new List<string>();
            if (HasNamedSequence(PointSequenceName))
            {
                result.Add(PointSequenceName);
            }

            if (HasNamedSequence(RecordedPathSequenceName))
            {
                result.Add(RecordedPathSequenceName);
            }

            var names = WaypointStore.LoadAllNames();
            System.Array.Sort(names, System.StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < names.Length; index++)
            {
                var name = names[index];
                if (string.Equals(name, PointSequenceName, System.StringComparison.OrdinalIgnoreCase)
                    || string.Equals(name, RecordedPathSequenceName, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(name);
            }

            return result;
        }

        private static string GetSequenceDisplayName(string sequenceName)
        {
            if (string.Equals(sequenceName, PointSequenceName, System.StringComparison.OrdinalIgnoreCase))
            {
                return "저장한 포인트 순서";
            }

            if (string.Equals(sequenceName, RecordedPathSequenceName, System.StringComparison.OrdinalIgnoreCase))
            {
                return "기록한 경로";
            }

            return string.IsNullOrWhiteSpace(sequenceName) ? "실행 목록" : sequenceName.Trim();
        }

        private static bool IsProtectedSequence(string sequenceName)
        {
            return string.Equals(sequenceName, PointSequenceName, System.StringComparison.OrdinalIgnoreCase);
        }

        private string BuildPointListDebugSummary()
        {
            var sequence = LoadPointSequenceIfExists();
            var waypoints = sequence?.waypoints ?? System.Array.Empty<Waypoint>();
            var names = new string[waypoints.Length];
            for (var index = 0; index < waypoints.Length; index++)
            {
                names[index] = $"{waypoints[index]?.name}:{waypoints[index]?.moveType}";
            }

            return $"count={waypoints.Length}; visiblePoints={pointListItems.Count}; pointFilter={pointFilter}; active={recalledPoint?.name ?? "none"}; selected={selectedPointNames.Count}; collapsed={pointRowActionsCollapsed}; inventory=[{BuildPointInventorySummary()}]; points=[{string.Join(",", names)}]";
        }

        private static string BuildPointRowSummary(Waypoint waypoint)
        {
            return ShortDisplayName(waypoint?.name);
        }

        private static string ShortDisplayName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "-";
            }

            var trimmed = value.Trim();
            return trimmed.Length <= 18
                ? trimmed
                : trimmed.Substring(0, 15) + "...";
        }

        private static Button CreatePointRowButton(string name, string text, System.Action clicked)
        {
            var button = new Button
            {
                name = name,
                text = text
            };
            button.clicked += clicked;
            button.AddToClassList("rc-point-row-button");
            return button;
        }

        private void BindPointListRow(VisualElement row, int index)
        {
            PrepareListRow(row);
            if (index < 0 || index >= pointListItems.Count || pointListItems[index] == null)
            {
                AddEmptyRow(row, "저장 위치 없음");
                return;
            }

            var waypoint = pointListItems[index];
            var capturedName = waypoint.name;
            row.EnableInClassList(
                "rc-point-row--active",
                recalledPoint != null && string.Equals(recalledPoint.name, waypoint.name, System.StringComparison.OrdinalIgnoreCase));
            row.EnableInClassList("rc-point-row--selected", selectedPointNames.Contains(capturedName));

            AddRowSummary(row, BuildPointRowSummary(waypoint));
            var actions = AddRowActions(row);
            actions.Add(CreatePointRowButton(
                "BtnPointRowSelect",
                selectedPointNames.Contains(capturedName) ? "선택됨" : "선택",
                () => TogglePointSelection(capturedName)));
            actions.Add(CreatePointRowButton("BtnPointRowRecall", "불러오기", () => RecallPoint(capturedName)));
            if (!pointRowActionsCollapsed)
            {
                actions.Add(CreatePointRowButton("BtnPointRowMove", "실행", () => MovePointRow(capturedName)));
                actions.Add(CreatePointRowButton("BtnPointRowPreview", "미리보기", () => PreviewPointRow(capturedName)));
                actions.Add(CreatePointRowButton("BtnPointRowEdit", "편집", () => EditPointRow(capturedName)));
                actions.Add(CreatePointRowButton("BtnPointRowFunctionCandidate", "묶음 추가", () => AddPointRowToFunction(capturedName)));
            }
        }

        private void BindFunctionListRow(VisualElement row, int index)
        {
            PrepareListRow(row);
            if (index < 0 || index >= functionListItems.Count)
            {
                AddEmptyRow(row, "묶음 없음");
                return;
            }

            var functionName = functionListItems[index];
            row.EnableInClassList(
                "rc-point-row--active",
                string.Equals(selectedFunctionName, functionName, System.StringComparison.OrdinalIgnoreCase));
            row.EnableInClassList("rc-point-row--selected", selectedFunctionNames.Contains(functionName));

            AddRowSummary(row, BuildFunctionRowSummary(functionName));
            var actions = AddRowActions(row);
            actions.Add(CreatePointRowButton(
                "BtnFunctionRowSelect",
                selectedFunctionNames.Contains(functionName) ? "선택됨" : "선택",
                () => ToggleFunctionSelection(functionName)));
            actions.Add(CreatePointRowButton("BtnFunctionRowOpen", "열기", () => SelectFunction(functionName)));
            if (!functionRowActionsCollapsed)
            {
                actions.Add(CreatePointRowButton("BtnFunctionRowDuplicate", "복사", () =>
                {
                    SelectFunction(functionName);
                    DuplicateSelectedFunction();
                }));
                actions.Add(CreatePointRowButton("BtnFunctionRowDelete", "삭제", () =>
                {
                    SelectFunction(functionName);
                    DeleteSelectedFunction();
                }));
            }
        }

        private void BindBlockSequenceListRow(VisualElement row, int index)
        {
            PrepareListRow(row);
            if (index < 0 || index >= blockSequenceListItems.Count || blockSequenceListItems[index].Block == null)
            {
                AddEmptyRow(row, "작업 순서 없음");
                return;
            }

            var item = blockSequenceListItems[index];
            var capturedIndex = item.Index;
            AddRowSummary(row, BuildBlockRowSummary(item.Block, capturedIndex));
            var actions = AddRowActions(row);
            actions.Add(CreatePointRowButton("BtnBlockMoveUp", "위", () => MoveBlockSequenceRow(capturedIndex, -1)));
            actions.Add(CreatePointRowButton("BtnBlockMoveDown", "아래", () => MoveBlockSequenceRow(capturedIndex, 1)));
            actions.Add(CreatePointRowButton("BtnBlockDelete", "삭제", () => DeleteBlockSequenceRow(capturedIndex)));
        }

        private void BindSequenceListRow(VisualElement row, int index)
        {
            PrepareListRow(row);
            if (index < 0 || index >= sequenceListItems.Count)
            {
                AddEmptyRow(row, "실행 목록 없음");
                return;
            }

            var sequenceName = sequenceListItems[index];
            row.EnableInClassList(
                "rc-point-row--active",
                string.Equals(selectedSequenceName, sequenceName, System.StringComparison.OrdinalIgnoreCase));
            row.EnableInClassList("rc-point-row--selected", selectedSequenceNames.Contains(sequenceName));

            AddRowSummary(row, BuildSequenceRowSummary(sequenceName));
            var actions = AddRowActions(row);
            actions.Add(CreatePointRowButton(
                "BtnSequenceRowMultiSelect",
                selectedSequenceNames.Contains(sequenceName) ? "선택됨" : "선택",
                () => ToggleSequenceSelection(sequenceName)));
            actions.Add(CreatePointRowButton("BtnSequenceRowSelect", "열기", () => SelectSequence(sequenceName)));
            if (!sequenceRowActionsCollapsed)
            {
                actions.Add(CreatePointRowButton("BtnSequenceRowRun", "재생", () =>
                {
                    SelectSequence(sequenceName);
                    RunSelectedSequenceOnce();
                }));
                actions.Add(CreatePointRowButton("BtnSequenceRowLoop", "루프", () =>
                {
                    SelectSequence(sequenceName);
                    RunSelectedSequenceLoop();
                }));

                var deleteButton = CreatePointRowButton("BtnSequenceRowDelete", "삭제", () =>
                {
                    SelectSequence(sequenceName);
                    DeleteSelectedSequence();
                });
                deleteButton.SetEnabled(!IsProtectedSequence(sequenceName));
                actions.Add(deleteButton);
            }
        }

        private void BindBundlePickerListRow(VisualElement row, int index)
        {
            PrepareListRow(row);
            if (index < 0 || index >= bundlePickerListItems.Count)
            {
                AddEmptyRow(row, "묶음 없음");
                return;
            }

            var functionName = bundlePickerListItems[index];
            row.EnableInClassList(
                "rc-point-row--active",
                string.Equals(bundlePickerSelectedName, functionName, System.StringComparison.OrdinalIgnoreCase));
            AddRowSummary(row, BuildFunctionRowSummary(functionName));
            var actions = AddRowActions(row);
            actions.Add(CreatePointRowButton(
                "BtnBundlePickerRowSelect",
                string.Equals(bundlePickerSelectedName, functionName, System.StringComparison.OrdinalIgnoreCase) ? "선택됨" : "선택",
                () =>
                {
                    bundlePickerSelectedName = functionName;
                    ApplyAll();
                }));
        }

        private static void PrepareListRow(VisualElement row)
        {
            row.Clear();
            row.ClearClassList();
            row.AddToClassList("rc-point-row");
        }

        private static void AddRowSummary(VisualElement row, string text)
        {
            var summary = new Label(text);
            summary.AddToClassList("rc-point-row-summary");
            row.Add(summary);
        }

        private static VisualElement AddRowActions(VisualElement row)
        {
            var actions = new VisualElement();
            actions.AddToClassList("rc-point-row-actions");
            row.Add(actions);
            return actions;
        }

        private static void AddEmptyRow(VisualElement row, string text)
        {
            AddRowSummary(row, text);
        }

        private static void RefreshListView(ListView listView)
        {
            listView?.Rebuild();
        }

        private bool ShouldShowPointRow(Waypoint waypoint)
        {
            if (waypoint == null || !MatchesSearch(waypoint.name, pointSearchText))
            {
                return false;
            }

            return pointFilter switch
            {
                FilterSelected => selectedPointNames.Contains(waypoint.name),
                FilterMoveJ => NormalizeMoveType(waypoint.moveType) == "MoveJ",
                FilterMoveL => NormalizeMoveType(waypoint.moveType) == "MoveL",
                _ => true,
            };
        }

        private bool ShouldShowFunctionRow(string functionName)
        {
            if (!MatchesSearch(functionName, functionSearchText))
            {
                return false;
            }

            return functionFilter switch
            {
                FilterSelected => selectedFunctionNames.Contains(functionName),
                FilterMissing => FunctionHasMissingPoint(functionName),
                _ => true,
            };
        }

        private bool ShouldShowSequenceRow(string sequenceName)
        {
            if (!MatchesSearch(sequenceName, sequenceSearchText) && !MatchesSearch(GetSequenceDisplayName(sequenceName), sequenceSearchText))
            {
                return false;
            }

            return sequenceFilter switch
            {
                FilterSelected => selectedSequenceNames.Contains(sequenceName),
                FilterDeletable => !IsProtectedSequence(sequenceName),
                FilterProtected => IsProtectedSequence(sequenceName),
                _ => true,
            };
        }

        private bool FunctionHasMissingPoint(string functionName)
        {
            var detail = runtimeController != null
                ? runtimeController.GetTeachingFunctionDetailForDebug(functionName)
                : string.Empty;
            var missingCount = ExtractDebugValue(detail, "missingCount=");
            return !string.IsNullOrWhiteSpace(missingCount) && missingCount != "0";
        }

        private static bool MatchesSearch(string value, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            return (value ?? string.Empty).IndexOf(searchText.Trim(), System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string NormalizePointFilter(string filter)
        {
            return filter switch
            {
                FilterSelected => FilterSelected,
                FilterMoveJ => FilterMoveJ,
                FilterMoveL => FilterMoveL,
                _ => FilterAll,
            };
        }

        private static string NormalizeFunctionFilter(string filter)
        {
            return filter switch
            {
                FilterSelected => FilterSelected,
                FilterMissing => FilterMissing,
                _ => FilterAll,
            };
        }

        private static string NormalizeSequenceFilter(string filter)
        {
            return filter switch
            {
                FilterSelected => FilterSelected,
                FilterDeletable => FilterDeletable,
                FilterProtected => FilterProtected,
                _ => FilterAll,
            };
        }

        private void MovePointRow(string pointName)
        {
            OpenPointActionModal(pointName, PointModalRunMode);
        }

        private void PreviewPointRow(string pointName)
        {
            OpenPointActionModal(pointName, PointModalPreviewMode);
        }

        private void EditPointRow(string pointName)
        {
            OpenPointActionModal(pointName, PointModalEditMode);
        }

        private void AddPointRowToFunction(string pointName)
        {
            OpenPointActionModal(pointName, PointModalFunctionMode);
        }

        private void ApplyPointDetail(PanelElements panel)
        {
            if (panel?.DetailTitle == null)
            {
                return;
            }

            if (recalledPoint == null)
            {
                panel.DetailTitle.text = "선택된 저장 위치 없음";
                panel.DetailMeta.text = "저장 위치를 선택하면 이동 방식과 저장된 속도/대기 시간이 보인다.";
                panel.DetailJoints.text = "J: -";
                panel.DetailTcp.text = "TCP: -";
                SetHidden(panel.PointEditActions, true);
                return;
            }

            SetHidden(panel.PointEditActions, false);
            panel.DetailTitle.text = recalledPoint.name;
            panel.DetailMeta.text = $"{ToMotionLabel(NormalizeMoveType(recalledPoint.moveType))} · {ToSpeedLabel(selectedSpeedPreset)} · 대기 {selectedDwellSec:0.0}초";
            panel.DetailJoints.text = $"J: {FormatVector(recalledPoint.jointsDeg, "0.0")}";
            panel.DetailTcp.text = $"TCP: {FormatTcp(recalledPoint.tcpMm)}";
            panel.BtnSpeedSlow?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "slow");
            panel.BtnSpeedMedium?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "medium");
            panel.BtnSpeedFast?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "fast");
            panel.DwellInput?.SetValueWithoutNotify(selectedDwellSec.ToString("0.0", CultureInfo.InvariantCulture));
            panel.DwellInput?.EnableInClassList("rc-point-dwell-input--danger", isDwellInvalid);
        }

        private void ApplyPointActionModal(PanelElements panel)
        {
            if (panel?.PointActionModal == null)
            {
                return;
            }

            var show = pointActionModalOpen && activeNavSection == "NavPoints" && recalledPoint != null;
            SetHidden(panel.PointActionModal, !show);
            if (!show)
            {
                return;
            }

            var isEditMode = pointActionModalMode == PointModalEditMode;
            panel.PointActionModalTitle.text = BuildPointModalTitle();
            panel.PointActionModalSummary.text = BuildPointModalSummary();
            panel.PointActionModalPose.text = $"J: {FormatVector(recalledPoint.jointsDeg, "0.0")} / TCP: {FormatTcp(recalledPoint.tcpMm)}";
            panel.PointActionModalNameInput?.SetValueWithoutNotify(recalledPoint.name ?? string.Empty);
            panel.PointActionModalDwellInput?.SetValueWithoutNotify(selectedDwellSec.ToString("0.0", CultureInfo.InvariantCulture));
            panel.PointActionModalNameInput?.SetEnabled(isEditMode);
            panel.PointActionModalDwellInput?.SetEnabled(isEditMode);
            panel.PointActionModalNameInput?.EnableInClassList("rc-point-modal-readonly", !isEditMode);
            panel.PointActionModalDwellInput?.EnableInClassList("rc-point-modal-readonly", !isEditMode);
            panel.BtnPointModalSpeedSlow?.SetEnabled(isEditMode);
            panel.BtnPointModalSpeedMedium?.SetEnabled(isEditMode);
            panel.BtnPointModalSpeedFast?.SetEnabled(isEditMode);
            panel.BtnPointModalSpeedSlow?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "slow");
            panel.BtnPointModalSpeedMedium?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "medium");
            panel.BtnPointModalSpeedFast?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "fast");
            panel.BtnPointModalPrimary.text = BuildPointModalPrimaryText();
            SetHidden(panel.BtnPointModalOverwrite, !isEditMode);
            SetHidden(panel.BtnPointModalDuplicate, !isEditMode);
            SetHidden(panel.BtnPointModalDelete, !isEditMode);
        }

        private string BuildPointModalTitle()
        {
            return pointActionModalMode switch
            {
                PointModalPreviewMode => "미리보기 확인",
                PointModalRunMode => "저장 위치 실행",
                PointModalEditMode => "저장 위치 편집",
                PointModalFunctionMode => "묶음에 추가",
                _ => "저장 위치 작업",
            };
        }

        private string BuildPointModalSummary()
        {
            var name = recalledPoint?.name ?? "-";
            return pointActionModalMode switch
            {
                PointModalPreviewMode => $"{name} 위치를 화면에서 먼저 확인한다.",
                PointModalRunMode => $"{name} 위치로 이동한다. 미리보기에서 먼저 움직임을 확인한다.",
                PointModalEditMode => $"{name} 이름, 속도, 대기 시간을 여기서 바로 수정한다.",
                PointModalFunctionMode => $"{name} 위치를 묶음 만들기 후보에 추가한다.",
                _ => $"{name} 저장 위치 작업을 선택한다.",
            };
        }

        private string BuildPointModalPrimaryText()
        {
            return pointActionModalMode switch
            {
                PointModalPreviewMode => "미리보기 실행",
                PointModalRunMode => "실행",
                PointModalEditMode => "저장",
                PointModalFunctionMode => "묶음에 추가",
                _ => "확인",
            };
        }

        private string BuildPointActionModalDebugSummary()
        {
            return $"modalOpen={pointActionModalOpen}; mode={pointActionModalMode}; point={recalledPoint?.name ?? "none"}; speed={selectedSpeedPreset}; dwell={selectedDwellSec:0.0}; feedback={lastFeedback}";
        }

        private void OpenPointActionModal(string pointName, string mode)
        {
            RecallPoint(pointName);
            if (recalledPoint == null)
            {
                return;
            }

            pointActionModalMode = NormalizePointModalMode(mode);
            pointActionModalOpen = true;
            SetFeedback($"[{BuildPointModalTitle()}] {recalledPoint.name}");
            ApplyAll();
        }

        private void ClosePointActionModal()
        {
            pointActionModalOpen = false;
            pointActionModalMode = string.Empty;
            ApplyAll();
        }

        private void OpenBundlePickerModal()
        {
            if (runtimeController == null || runtimeController.GetTeachingFunctionNames().Length == 0)
            {
                SetFeedback("시퀀스에 넣을 묶음이 없다. 포인트 탭에서 함수부터 등록해라.");
                return;
            }

            bundlePickerSelectedName = string.IsNullOrWhiteSpace(selectedFunctionName)
                ? runtimeController.GetTeachingFunctionNames()[0]
                : selectedFunctionName;
            bundlePickerModalOpen = true;
            ApplyAll();
        }

        private void CloseBundlePickerModal()
        {
            bundlePickerModalOpen = false;
            ApplyAll();
        }

        private void ConfirmBundlePickerSelection()
        {
            if (string.IsNullOrWhiteSpace(bundlePickerSelectedName))
            {
                SetFeedback("작업 시퀀스에 넣을 묶음을 먼저 골라라.");
                return;
            }

            selectedFunctionName = bundlePickerSelectedName;
            AddSelectedBundleToBlockSequence();
            bundlePickerModalOpen = false;
            ApplyAll();
        }

        private void ApplyBundlePickerModal(PanelElements panel)
        {
            if (panel?.BundlePickerModal == null)
            {
                return;
            }

            SetHidden(panel.BundlePickerModal, !bundlePickerModalOpen);
            if (!bundlePickerModalOpen)
            {
                return;
            }

            var functionNames = runtimeController != null
                ? runtimeController.GetTeachingFunctionNames()
                : System.Array.Empty<string>();
            if (panel.BundlePickerSummary != null)
            {
                panel.BundlePickerSummary.text = functionNames.Length == 0
                    ? "등록된 묶음이 없다."
                    : string.IsNullOrWhiteSpace(bundlePickerSelectedName)
                        ? $"묶음 {functionNames.Length}개 · 선택 없음"
                        : $"묶음 {functionNames.Length}개 · 선택 {ShortDisplayName(bundlePickerSelectedName)}";
            }

            if (panel.BundlePickerListView != null)
            {
                bundlePickerListItems.Clear();
                for (var index = 0; index < functionNames.Length; index++)
                {
                    bundlePickerListItems.Add(functionNames[index]);
                }

                RefreshListView(panel.BundlePickerListView);
            }

            panel.BtnBundlePickerConfirm?.SetEnabled(!string.IsNullOrWhiteSpace(bundlePickerSelectedName));
        }

        private static string NormalizePointModalMode(string mode)
        {
            return mode switch
            {
                PointModalRunMode => PointModalRunMode,
                PointModalEditMode => PointModalEditMode,
                PointModalFunctionMode => PointModalFunctionMode,
                _ => PointModalPreviewMode,
            };
        }

        private void ApplyPointActionModalPrimary()
        {
            if (!pointActionModalOpen || recalledPoint == null)
            {
                SetFeedback("작업할 포인트를 먼저 선택해라.");
                return;
            }

            switch (pointActionModalMode)
            {
                case PointModalRunMode:
                    ApplyMotionCandidate();
                    break;
                case PointModalEditMode:
                    ApplyPointModalEdits();
                    break;
                case PointModalFunctionMode:
                    AddSelectedPointToFunction();
                    break;
                default:
                    PreviewMotionCandidate();
                    break;
            }
        }

        private void ApplyPointModalEdits()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 포인트 편집을 잠근다. Stop 후 다시 수정해라.");
                return;
            }

            var panel = ResolveActivePanel();
            var nextName = panel?.PointActionModalNameInput?.value?.Trim() ?? string.Empty;
            var nextDwellRaw = panel?.PointActionModalDwellInput?.value ?? "0";
            if (string.IsNullOrWhiteSpace(nextName))
            {
                SetFeedback("위치 이름을 먼저 넣어라.");
                return;
            }

            if (!double.TryParse(nextDwellRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out var nextDwell)
                || double.IsNaN(nextDwell)
                || double.IsInfinity(nextDwell)
                || nextDwell < 0.0
                || nextDwell > 600.0)
            {
                SetFeedback("대기 시간은 0~600초 사이 숫자로 넣어라.");
                return;
            }

            var originalName = recalledPoint.name;
            selectedDwellSec = nextDwell;
            isDwellInvalid = false;
            if (!string.Equals(originalName, nextName, System.StringComparison.OrdinalIgnoreCase))
            {
                if (HasNamedPoint(nextName))
                {
                    SetFeedback($"{nextName} 이름이 이미 있다.");
                    return;
                }

                RenamePoint(originalName, nextName);
            }

            ApplySelectedPointTiming();
            pointActionModalOpen = recalledPoint != null;
            pointActionModalMode = PointModalEditMode;
            ApplyAll();
        }

        private void SetPointModalSpeedPreset(string speedPreset)
        {
            if (pointActionModalMode != PointModalEditMode)
            {
                return;
            }

            selectedSpeedPreset = NormalizeSpeedPreset(speedPreset);
            ClearPendingConfirmation();
            ApplyAll();
        }

        private void ExecutePointModalEditAction(System.Action action)
        {
            if (pointActionModalMode != PointModalEditMode || recalledPoint == null)
            {
                SetFeedback("편집할 포인트를 먼저 선택해라.");
                return;
            }

            action?.Invoke();
            pointActionModalOpen = recalledPoint != null;
            pointActionModalMode = pointActionModalOpen ? PointModalEditMode : string.Empty;
            ApplyAll();
        }

        private string BuildPointDetailDebugSummary()
        {
            if (recalledPoint == null)
            {
                return "detail=none";
            }

            return $"detail={recalledPoint.name}; moveType={NormalizeMoveType(recalledPoint.moveType)}; speed={NormalizeSpeedPreset(recalledPoint.speedPreset)}; dwell={recalledPoint.dwellSec:0.0}; joints=[{FormatVector(recalledPoint.jointsDeg, "0.0")}]; tcp=[{FormatTcp(recalledPoint.tcpMm)}]";
        }

        private bool HasNamedPoint(string pointName)
        {
            var sequence = LoadPointSequenceIfExists();
            return FindWaypointIndex(sequence, pointName) >= 0;
        }

        private bool HasAnyPoint()
        {
            var sequence = LoadPointSequenceIfExists();
            return sequence?.waypoints != null && sequence.waypoints.Length > 0;
        }

        private bool CanMoveSelectedPoint(int direction)
        {
            if (recalledPoint == null)
            {
                return false;
            }

            var sequence = LoadPointSequenceIfExists();
            var index = FindWaypointIndex(sequence, recalledPoint.name);
            var targetIndex = index + (direction < 0 ? -1 : 1);
            return index >= 0 && sequence?.waypoints != null && targetIndex >= 0 && targetIndex < sequence.waypoints.Length;
        }

        private static WaypointSequence LoadPointSequenceIfExists()
        {
            return LoadSequenceIfExists(PointSequenceName);
        }

        private static WaypointSequence LoadSequenceIfExists(string sequenceName)
        {
            if (string.IsNullOrWhiteSpace(sequenceName))
            {
                return null;
            }

            var names = WaypointStore.LoadAllNames();
            for (var index = 0; index < names.Length; index++)
            {
                if (string.Equals(names[index], sequenceName.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return WaypointStore.Load(names[index]);
                }
            }

            return null;
        }

        private static bool HasNamedSequence(string sequenceName)
        {
            return LoadSequenceIfExists(sequenceName) != null;
        }

        private static int CountSequenceWaypoints(string sequenceName)
        {
            return LoadSequenceIfExists(sequenceName)?.waypoints?.Length ?? 0;
        }

        private void SetPointName(string pointName)
        {
            var safeName = pointName?.Trim() ?? string.Empty;
            desktopPanel?.PointNameInput?.SetValueWithoutNotify(safeName);
            tabletPanel?.PointNameInput?.SetValueWithoutNotify(safeName);
        }

        private double[] ReadCurrentSnapshotJoints()
        {
            var values = runtimeController?.CurrentSnapshot.JointValues;
            var result = new double[6];
            for (var index = 0; index < result.Length; index++)
            {
                if (values == null || index >= values.Length || !double.TryParse(values[index], NumberStyles.Float, CultureInfo.InvariantCulture, out result[index]))
                {
                    result[index] = 0.0;
                }
            }

            return result;
        }

        private double[] ReadCurrentSnapshotTcp()
        {
            var values = runtimeController?.CurrentSnapshot.TcpValues;
            var result = new double[6];
            for (var index = 0; index < result.Length; index++)
            {
                if (values == null || index >= values.Length || !double.TryParse(values[index], NumberStyles.Float, CultureInfo.InvariantCulture, out result[index]))
                {
                    result[index] = 0.0;
                }
            }

            return result;
        }

        private static void ReplaceWaypoint(WaypointSequence sequence, Waypoint waypoint)
        {
            var waypoints = sequence.waypoints ?? System.Array.Empty<Waypoint>();
            for (var index = 0; index < waypoints.Length; index++)
            {
                if (string.Equals(waypoints[index]?.name, waypoint.name, System.StringComparison.OrdinalIgnoreCase))
                {
                    waypoints[index] = CloneWaypoint(waypoint);
                    sequence.waypoints = waypoints;
                    return;
                }
            }

            WaypointStore.AddWaypoint(sequence, CloneWaypoint(waypoint));
        }

        private static Waypoint FindWaypoint(WaypointSequence sequence, string pointName)
        {
            var index = FindWaypointIndex(sequence, pointName);
            return index >= 0 ? sequence.waypoints[index] : null;
        }

        private static int FindWaypointIndex(WaypointSequence sequence, string pointName)
        {
            if (sequence?.waypoints == null || string.IsNullOrWhiteSpace(pointName))
            {
                return -1;
            }

            var waypoints = sequence.waypoints;
            for (var index = 0; index < waypoints.Length; index++)
            {
                var waypoint = waypoints[index];
                if (waypoint != null && string.Equals(waypoint.name, pointName.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        private static Waypoint CloneWaypoint(Waypoint waypoint)
        {
            return new Waypoint
            {
                name = waypoint?.name ?? "Point",
                jointsDeg = waypoint?.jointsDeg != null ? (double[])waypoint.jointsDeg.Clone() : new double[6],
                tcpMm = waypoint?.tcpMm != null ? (double[])waypoint.tcpMm.Clone() : new double[6],
                moveType = waypoint?.moveType ?? "MoveJ",
                speedPreset = waypoint?.speedPreset ?? "medium",
                dwellSec = waypoint?.dwellSec ?? 0.0
            };
        }

        private void SetSelectedSpeedPreset(string speedPreset)
        {
            selectedSpeedPreset = NormalizeSpeedPreset(speedPreset);
            ClearPendingConfirmation();
            ApplyAll();
        }

        private bool IsSequenceEditLocked()
        {
            return debugSequenceEditLocked || (runtimeController != null && runtimeController.IsTeachingSequenceRunning);
        }

        private void SetPendingConfirmation(string kind, string pointName)
        {
            pendingConfirmKind = kind ?? string.Empty;
            pendingConfirmName = pointName?.Trim() ?? string.Empty;
        }

        private bool IsPendingConfirmation(string kind, string pointName)
        {
            return string.Equals(pendingConfirmKind, kind, System.StringComparison.Ordinal)
                && string.Equals(pendingConfirmName, pointName?.Trim() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
        }

        private void ClearPendingConfirmation()
        {
            pendingConfirmKind = string.Empty;
            pendingConfirmName = string.Empty;
        }

        private static void InsertWaypointAfter(WaypointSequence sequence, Waypoint waypoint, int sourceIndex)
        {
            var existing = sequence.waypoints ?? System.Array.Empty<Waypoint>();
            var insertIndex = Mathf.Clamp(sourceIndex + 1, 0, existing.Length);
            var expanded = new Waypoint[existing.Length + 1];
            if (insertIndex > 0)
            {
                System.Array.Copy(existing, 0, expanded, 0, insertIndex);
            }

            expanded[insertIndex] = CloneWaypoint(waypoint);
            if (insertIndex < existing.Length)
            {
                System.Array.Copy(existing, insertIndex, expanded, insertIndex + 1, existing.Length - insertIndex);
            }

            sequence.waypoints = expanded;
        }

        private static string BuildUniqueDuplicateName(WaypointSequence sequence, string sourceName)
        {
            var safeSource = string.IsNullOrWhiteSpace(sourceName) ? "Point" : sourceName.Trim();
            var baseName = $"{safeSource}_COPY";
            var candidate = baseName;
            var suffix = 2;
            while (FindWaypointIndex(sequence, candidate) >= 0)
            {
                candidate = $"{baseName}_{suffix}";
                suffix++;
            }

            return candidate;
        }

        private static string FormatVector(double[] values, string format)
        {
            if (values == null || values.Length == 0)
            {
                return "-";
            }

            var count = System.Math.Min(values.Length, 6);
            var parts = new string[count];
            for (var index = 0; index < count; index++)
            {
                parts[index] = values[index].ToString(format, CultureInfo.InvariantCulture);
            }

            return string.Join(" / ", parts);
        }

        private static string FormatTcp(double[] values)
        {
            if (values == null || values.Length < 6)
            {
                return "-";
            }

            return $"X {values[0]:0.0} / Y {values[1]:0.0} / Z {values[2]:0.0} / RX {values[3]:0.0} / RY {values[4]:0.0} / RZ {values[5]:0.0}";
        }

        private static string NormalizeMoveType(string value)
        {
            return string.Equals(value, "MoveL", System.StringComparison.OrdinalIgnoreCase) ? "MoveL" : "MoveJ";
        }

        private static string ToMotionLabel(string moveType)
        {
            return string.Equals(moveType, "MoveL", System.StringComparison.OrdinalIgnoreCase)
                ? "직선 이동"
                : "관절 이동";
        }

        private static string ToSpeedLabel(string speedPreset)
        {
            return NormalizeSpeedPreset(speedPreset) switch
            {
                "slow" => "느림",
                "fast" => "빠름",
                _ => "중간",
            };
        }

        private static string NormalizeSpeedPreset(string value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "slow" => "slow",
                "fast" => "fast",
                _ => "medium",
            };
        }

        private static int AxisIndexFromLabel(string axisLabel)
        {
            return (axisLabel ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "X" => 0,
                "Y" => 1,
                "Z" => 2,
                "RX" => 3,
                "RY" => 4,
                _ => 5,
            };
        }

        private static string GetAxisLabel(int index)
        {
            return index switch
            {
                0 => "X",
                1 => "Y",
                2 => "Z",
                3 => "RX",
                4 => "RY",
                _ => "RZ",
            };
        }

        private PendantV3LocalState GetLocalState()
        {
            var shellState = GetComponent<PendantV3ShellStateController>();
            return shellState != null
                ? shellState.GetStateSnapshot()
                : PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault());
        }

        private bool CanPreview()
        {
            if (connectionHomeController.CurrentPreviewState == PendantV3PreviewState.Kind.AutoReconnect)
            {
                return false;
            }

            return connectionHomeController.CurrentPreviewState != PendantV3PreviewState.Kind.Disconnected
                || connectionHomeController.CurrentPreviewDefinition.DryRunEnabled;
        }

        private bool IsAnyPanelVisible() => isDesktopVisible || isTabletVisible;
        private PanelElements ResolveActivePanel() => isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
        private bool IsMoveLDispatchMode() => motionKind == "MoveL";
        private bool CanApply()
        {
            if (connectionHomeController.CurrentPreviewState is PendantV3PreviewState.Kind.AutoReconnect or PendantV3PreviewState.Kind.Fault)
            {
                return false;
            }

            return connectionHomeController.CurrentPreviewState != PendantV3PreviewState.Kind.Disconnected
                || connectionHomeController.CurrentPreviewDefinition.DryRunEnabled;
        }

        private static float ParseValue(string rawValue)
        {
            return float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0f;
        }

        private double[] currentValuesToDouble()
        {
            var result = new double[currentValues.Length];
            for (var i = 0; i < currentValues.Length; i++)
            {
                result[i] = currentValues[i];
            }

            return result;
        }
    }
}
