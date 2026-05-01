// Folder: PointMoveController - bundle picker list rendering for the PointMove panel.
// Serves the bundle selection modal row binding only.
// Bundle picker modal visibility and confirmation behavior remain in ListsAndModals.
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
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
    }
}
