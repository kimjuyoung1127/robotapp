// Folder: PointMoveController - sequence, path recording, and teaching flow actions for the PointMove panel.
// Serves the Point/Sequence subviews and keeps sequence execution UI separate from point CRUD.
// Motion candidate preview/apply and point CRUD live sync helpers are owned by adjacent partials.
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
        private void RunActiveSequence()
        {
            if (runtimeController == null)
            {
                SetFeedback("시퀀스 실행 runtime을 찾지 못했다.");
                return;
            }

            runtimeController.ExecutePrimaryAction();
            SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
        }

        private void StepTeachingBackward()
        {
            if (runtimeController == null)
            {
                SetFeedback("이전/다음 실행 기능을 찾지 못했다.");
                return;
            }

            runtimeController.StepBackward();
            SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
        }

        private void StepTeachingForward()
        {
            if (runtimeController == null)
            {
                SetFeedback("이전/다음 실행 기능을 찾지 못했다.");
                return;
            }

            runtimeController.StepForward();
            SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
        }

        private void StopTeachingSequence()
        {
            if (runtimeController == null)
            {
                SetFeedback("Stop runtime을 찾지 못했다.");
                return;
            }

            var result = runtimeController.StopMotion();
            SetFeedback(result.Message);
        }

        private void StartPathRecording()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 새 경로 기록을 시작하지 않는다. Stop 후 다시 해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.StartTeachingPathRecording()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void StopPathRecording()
        {
            var result = runtimeController != null
                ? runtimeController.StopTeachingPathRecording()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void PlayRecordedPathOnce()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 기록 재생을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.PlayRecordedTeachingPathOnce()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void PlayRecordedPathLoop()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 기록 루프를 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.PlayRecordedTeachingPathLoop()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void DeleteRecordedPath()
        {
            SelectSequence(RecordedPathSequenceName);
            DeleteSelectedSequence();
        }

        private void AddSelectedPointToBlockSequence()
        {
            if (recalledPoint == null)
            {
                SetFeedback("작업 시퀀스에 넣을 포인트를 먼저 선택해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.AddTeachingBlockPoint(recalledPoint.name)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void AddSelectedBundleToBlockSequence()
        {
            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("작업 시퀀스에 넣을 묶음을 먼저 선택해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.AddTeachingBlockBundle(selectedFunctionName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void PreviewBlockSequence()
        {
            var result = runtimeController != null
                ? runtimeController.PreviewTeachingBlockSequence()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void RunBlockSequence()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 작업 시퀀스를 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteTeachingBlockSequenceDryRun()
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void MoveBlockSequenceRow(int index, int direction)
        {
            var result = runtimeController != null
                ? runtimeController.MoveTeachingBlock(index, direction)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void DeleteBlockSequenceRow(int index)
        {
            var result = runtimeController != null
                ? runtimeController.DeleteTeachingBlock(index)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void SelectSequence(string sequenceName)
        {
            var safeName = string.IsNullOrWhiteSpace(sequenceName)
                ? PointSequenceName
                : sequenceName.Trim();
            selectedSequenceName = safeName;
            SetFeedback($"[Sequence] {GetSequenceDisplayName(safeName)} 선택");
            ApplyAll();
        }

        private void RunSelectedSequenceOnce()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 새 실행을 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 실행할 수 없다.");
                return;
            }

            if (popupCoordinator != null
                && runtimeController != null
                && runtimeController.ShouldRouteWaypointSequenceThroughLiveApproval(selectedSequenceName, loop: false))
            {
                SetFeedback(runtimeController.PrepareWaypointSequenceOperatorApproval(selectedSequenceName, loop: false));
                if (runtimeController.HasPendingWaypointSequenceOperatorApproval())
                {
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveJ"))
                    {
                        popupCoordinator.OpenRunConfirmForProduct();
                    }
                    else
                    {
                        SetFeedback(runtimeController.ExecutePendingWaypointSequenceOperatorCommand());
                    }
                }

                ApplyAll();
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteWaypointSequenceOnce(selectedSequenceName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void RunSelectedSequenceLoop()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 새 루프를 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 루프 실행할 수 없다.");
                return;
            }

            if (popupCoordinator != null
                && runtimeController != null
                && runtimeController.ShouldRouteWaypointSequenceThroughLiveApproval(selectedSequenceName, loop: true))
            {
                SetFeedback(runtimeController.PrepareWaypointSequenceOperatorApproval(selectedSequenceName, loop: true));
                if (runtimeController.HasPendingWaypointSequenceOperatorApproval())
                {
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveJ"))
                    {
                        popupCoordinator.OpenRunConfirmForProduct();
                    }
                    else
                    {
                        SetFeedback(runtimeController.ExecutePendingWaypointSequenceOperatorCommand());
                    }
                }

                ApplyAll();
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteWaypointSequenceLoop(selectedSequenceName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void DeleteSelectedSequence()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 실행 목록 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

            if (string.Equals(selectedSequenceName, PointSequenceName, System.StringComparison.OrdinalIgnoreCase))
            {
                SetFeedback("저장한 포인트 순서는 포인트 탭의 삭제/정리로 관리한다. 여기서는 기록한 경로와 별도 실행 목록만 삭제한다.");
                return;
            }

            if (!HasNamedSequence(selectedSequenceName))
            {
                SetFeedback($"{GetSequenceDisplayName(selectedSequenceName)} 실행 목록을 찾지 못했다.");
                selectedSequenceName = PointSequenceName;
                ApplyAll();
                return;
            }

            if (!IsPendingConfirmation("delete-sequence", selectedSequenceName))
            {
                SetPendingConfirmation("delete-sequence", selectedSequenceName);
                SetFeedback($"[Confirm] {GetSequenceDisplayName(selectedSequenceName)} 삭제 예정. 삭제를 한 번 더 누르면 기록/실행 목록이 지워진다.");
                return;
            }

            var deletedName = selectedSequenceName;
            var result = runtimeController != null
                ? runtimeController.DeleteWaypointSequence(deletedName)
                : WaypointStore.Delete(deletedName)
                    ? $"[Sequence] {deletedName} 삭제"
                    : $"[Sequence] {deletedName} 삭제 실패";
            selectedSequenceName = PointSequenceName;
            ClearPendingConfirmation();
            SetFeedback(result);
            ApplyAll();
        }

        private void ToggleSequenceRowActionsCollapsed()
        {
            sequenceRowActionsCollapsed = !sequenceRowActionsCollapsed;
            SetFeedback(sequenceRowActionsCollapsed ? "[Sequence] row 버튼 접기" : "[Sequence] row 버튼 펼치기");
            ApplyAll();
        }

        private void ToggleSequenceSelection(string sequenceName)
        {
            if (string.IsNullOrWhiteSpace(sequenceName))
            {
                return;
            }

            var safeName = sequenceName.Trim();
            if (selectedSequenceNames.Contains(safeName))
            {
                selectedSequenceNames.Remove(safeName);
            }
            else
            {
                selectedSequenceNames.Add(safeName);
            }

            ClearPendingConfirmation();
            ApplyAll();
        }

        private void ClearSelectedSequences()
        {
            selectedSequenceNames.Clear();
            ClearPendingConfirmation();
            SetFeedback("[Sequence] 선택 해제");
            ApplyAll();
        }

        private void DeleteSelectedSequences()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 실행 목록 삭제를 잠근다. Stop 후 다시 삭제해라.");
                return;
            }

            var deletable = BuildDeletableSelectedSequences();
            if (deletable.Count == 0)
            {
                SetFeedback("삭제 가능한 실행 목록을 먼저 선택해라. 저장한 포인트 순서는 보호된다.");
                return;
            }

            var confirmKey = string.Join("|", deletable);
            if (!IsPendingConfirmation("bulk-delete-sequence", confirmKey))
            {
                SetPendingConfirmation("bulk-delete-sequence", confirmKey);
                SetFeedback($"[Confirm] 실행 목록 {deletable.Count}개 삭제 예정. 선택 삭제를 한 번 더 눌러라.");
                return;
            }

            var deleted = 0;
            for (var index = 0; index < deletable.Count; index++)
            {
                var name = deletable[index];
                var result = runtimeController != null
                    ? runtimeController.DeleteWaypointSequence(name)
                    : WaypointStore.Delete(name)
                        ? $"[Sequence] {name} 삭제"
                        : $"[Sequence] {name} 삭제 실패";
                if (result.Contains("삭제") && !result.Contains("실패"))
                {
                    deleted++;
                }
            }

            selectedSequenceNames.Clear();
            selectedSequenceName = PointSequenceName;
            ClearPendingConfirmation();
            SetFeedback($"[Delete] 실행 목록 {deleted}개 삭제");
            ApplyAll();
        }

    }
}
