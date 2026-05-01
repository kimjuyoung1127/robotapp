// Folder: PointMoveController - function execution entry points and teaching loop state actions.
// Serves dry-run function execution, run-from-selected, and loop toggle state for the PointMove function subview.
// Rendering and mutation helpers live in sibling partials.
namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
        private void RunSelectedFunction()
        {
            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("실행할 묶음을 먼저 선택해라.");
                return;
            }

            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 실행을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteTeachingFunctionOnceDryRun(selectedFunctionName)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void RunSelectedFunctionFromSelectedPoint()
        {
            if (string.IsNullOrWhiteSpace(selectedFunctionName))
            {
                SetFeedback("실행할 묶음을 먼저 선택해라.");
                return;
            }

            if (recalledPoint == null)
            {
                SetFeedback("묶음 안에서 시작할 포인트를 먼저 선택해라.");
                return;
            }

            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 선택 실행을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            var result = runtimeController != null
                ? runtimeController.ExecuteTeachingFunctionFromPointDryRun(selectedFunctionName, recalledPoint.name)
                : "runtime missing";
            SetFeedback(result);
            ApplyAll();
        }

        private void ToggleTeachingLoop()
        {
            if (runtimeController == null)
            {
                SetFeedback("반복 실행 상태를 바꿀 runtime을 찾지 못했다.");
                return;
            }

            var enabled = runtimeController.ToggleTeachingLoopEnabled();
            SetFeedback(enabled
                ? "[반복] 켜짐 · 실행을 누르면 저장 위치를 반복한다."
                : "[반복] 꺼짐 · 실행은 한 번만 진행한다.");
            ApplyAll();
        }

        private void RunFromSelectedPoint()
        {
            if (!IsAnyPanelVisible())
            {
                SetFeedback("저장 위치 패널이 열려 있을 때만 선택부터 실행할 수 있다.");
                return;
            }

            if (recalledPoint == null)
            {
                SetFeedback("선택부터 실행할 저장 위치를 먼저 선택해라.");
                return;
            }

            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 선택부터 실행을 새로 시작할 수 없다. Stop 후 다시 실행해라.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 선택부터 실행할 수 없다.");
                return;
            }

            if (!EnsurePointLiveSyncGate("선택부터 실행"))
            {
                return;
            }

            var hasSavedJointTarget = TryGetSavedJointTarget(currentValuesToDouble(), recalledPoint.name, out var savedJointTarget);
            var canRouteSavedPointThroughLiveApproval = runtimeController != null
                && hasSavedJointTarget
                && runtimeController.ShouldRouteSavedPointMoveJOperatorThroughLiveApproval();
            if (canRouteSavedPointThroughLiveApproval)
            {
                if (!EnsurePopupCoordinatorAvailableForLiveApproval("선택부터 실행"))
                {
                    ApplyAll();
                    return;
                }

                PreviewSavedMoveJ(recalledPoint.name, savedJointTarget);
                SetFeedback(runtimeController.PrepareSavedPointMoveJOperatorApproval(recalledPoint.name, savedJointTarget));
                if (runtimeController.HasPendingSavedPointOperatorApproval())
                {
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveJ"))
                    {
                        popupCoordinator.OpenMoveConfirmForProduct();
                    }
                    else
                    {
                        SetFeedback(runtimeController.ExecutePendingSavedPointOperatorCommand());
                    }
                }

                ApplyAll();
                return;
            }

            if (popupCoordinator != null
                && runtimeController != null
                && runtimeController.ShouldRouteWaypointSequenceThroughLiveApproval(PointSequenceName, loop: false))
            {
                SetFeedback(runtimeController.PrepareWaypointSequenceOperatorApproval(PointSequenceName, loop: false, recalledPoint.name));
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
                ? runtimeController.ExecuteTeachingSequenceFromPoint(recalledPoint.name)
                : "runtime missing";
            SetFeedback(result);
        }

        private void ApplyLoopState(PanelElements panel)
        {
            if (panel?.BtnLoop == null)
            {
                return;
            }

            var loopEnabled = runtimeController != null && runtimeController.IsTeachingLoopEnabled;
            var running = runtimeController != null && runtimeController.IsTeachingSequenceRunning;
            var snapshot = runtimeController != null ? runtimeController.CurrentSnapshot : null;
            panel.BtnLoop.text = loopEnabled ? "반복 ON" : "반복 OFF";
            panel.BtnLoop.EnableInClassList("rc-point-loop-button--active", loopEnabled);
            if (snapshot != null && snapshot.MixedLiveLoopRunning)
            {
                panel.LoopStatus.text = $"반복 실행: {snapshot.MixedLiveLoopCycleCount}사이클 · {snapshot.MixedLiveLoopTarget} · gripper {snapshot.MixedLiveLoopGripperIntent}";
                return;
            }

            panel.LoopStatus.text = loopEnabled
                ? running ? "반복 실행: 진행 중 · Stop으로 종료" : "반복 실행: 켜짐 · Run으로 시작"
                : "반복 실행: 꺼짐";
        }
    }
}
