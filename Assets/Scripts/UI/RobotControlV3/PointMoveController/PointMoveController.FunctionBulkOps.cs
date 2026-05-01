// Folder: PointMoveController - bulk point timing/delete and function rename/duplicate/delete helpers.
// Serves user-triggered mutation flows for selected points and selected functions.
// Selection state and function rendering live in sibling partials.
using System.Globalization;
using KineTutor3D.App.Fairino;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
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

            var remaining = new System.Collections.Generic.List<KineTutor3D.App.Fairino.Waypoint>();
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
    }
}
