// Folder: PointMoveController - sequence and block list rendering for the PointMove panel.
// Serves saved sequence inventory, block sequence rows, and sequence selection summaries.
// Sequence execution actions themselves remain in the Sequence partial.
using System.Collections.Generic;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
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

            runtimeController?.EnsureHomePoint1LoopSequenceForProduct();
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

            if (string.Equals(sequenceName, "HomePoint1Loop", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Home ↔ Point1 mixed loop";
            }

            return string.IsNullOrWhiteSpace(sequenceName) ? "실행 목록" : sequenceName.Trim();
        }

        private static bool IsProtectedSequence(string sequenceName)
        {
            return string.Equals(sequenceName, PointSequenceName, System.StringComparison.OrdinalIgnoreCase);
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
    }
}
