// Folder: PointMoveController - function and point selection state helpers for the function subview.
// Serves row-action collapse, selected-name bookkeeping, and active function name propagation.
// Builder, bulk operations, and run/loop execution live in sibling partials.
namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
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
    }
}
