// Folder: PointMoveController - point list rendering and point row view bindings for the PointMove panel.
// Serves the saved point list surface and point-row selection/preview/run affordances.
// Modal/detail rendering and point CRUD storage remain in sibling partials.
using KineTutor3D.App.Fairino;
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
    }
}
