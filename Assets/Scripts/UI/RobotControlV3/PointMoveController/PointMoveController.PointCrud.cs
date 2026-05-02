// Folder: PointMoveController - point CRUD, timing, export, and inventory helpers for the PointMove panel.
// Serves point save/recall/delete/rename/duplicate/order workflows and stored-point summaries.
// Saved-point live preview/apply helpers live in PointMotionHelpers.
using System.Collections.Generic;
using KineTutor3D.App.Fairino;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
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
