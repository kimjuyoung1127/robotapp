// Folder: PointMoveController - shared list row helpers and common filter utilities for PointMove list views.
// Serves Point/Sequence/Function/Bundle list rendering without owning modal or storage behavior.
// Point detail, modal actions, and point CRUD live in sibling partials.
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
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
    }
}
