// Folder: PointMoveController - function list rendering and row filtering for the PointMove panel.
// Serves function inventory rows and function selection visuals.
// Function execution/build logic remains in the Functions partial.
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
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

        private bool FunctionHasMissingPoint(string functionName)
        {
            var detail = runtimeController != null
                ? runtimeController.GetTeachingFunctionDetailForDebug(functionName)
                : string.Empty;
            var missingCount = ExtractDebugValue(detail, "missingCount=");
            return !string.IsNullOrWhiteSpace(missingCount) && missingCount != "0";
        }
    }
}
