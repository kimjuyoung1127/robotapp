// Folder: PointMoveController - point motion preview/apply flow and feedback handling for the PointMove panel.
// Serves point move input editing, preview-only candidate generation, and live apply entry.
// Point CRUD storage and teaching/function orchestration are split into sibling partials.
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
        private void ApplyVisibility()
        {
            if (isDesktopVisible)
            {
                workPanelBody?.EnableInClassList("rc-hidden", false);
            }

            if (isTabletVisible)
            {
                bottomSheetBody?.EnableInClassList("rc-hidden", false);
            }

            pointMovePanelHost?.EnableInClassList("rc-hidden", !isDesktopVisible);
            pointMoveSheetHost?.EnableInClassList("rc-hidden", !isTabletVisible);
        }

        private static bool ShouldShowDesktopPanel(string activeNavSection, string activeWorkTab)
        {
            return activeNavSection == "NavPoints";
        }

        private static bool ShouldShowTabletPanel(string activeNavSection, string activeTabletTab)
        {
            return activeNavSection == "NavPoints" || activeTabletTab == "BottomTabPointMove";
        }

        private void SetCoordSystem(string coordSystem)
        {
            activeCoordSystem = coordSystem is "Tool" or "User" ? coordSystem : "Base";
            var shellState = GetComponent<PendantV3ShellStateController>();
            if (shellState != null)
            {
                shellState.SetCoordSystemSelection(activeCoordSystem);
                return;
            }

            var localState = GetLocalState();
            localState.CoordSystem = activeCoordSystem;
            LocalSettingsStore.Save(localState);
            ApplyAll();
        }

        private void SetMotionKind(string nextMotionKind)
        {
            motionKind = nextMotionKind == "MoveL" ? "MoveL" : "MoveJ";
            ApplyAll();
        }

        private void RestoreFromPreview()
        {
            if (!CanPreview() || !IsAnyPanelVisible())
            {
                SetFeedback("연결이 준비될 때까지 현재값 복원을 잠시 잠근다.");
                return;
            }

            isPointNameInvalid = false;
            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            SetFeedback("현재 preview TCP 값으로 다시 채웠다.");
        }

        private void HandleValueChanged(int index, string rawValue)
        {
            if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return;
            }

            currentValues[index] = parsed;
            recalledPoint = null;
            lastInvalidIndex = -1;
            isPointNameInvalid = false;
            ClearPendingConfirmation();
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
            PreviewEditedPointCandidate();
        }

        private void PreviewEditedPointCandidate()
        {
            if (!IsAnyPanelVisible() || !CanPreview())
            {
                return;
            }

            if (!TryReadActivePanelValues(out var _, out _))
            {
                return;
            }

            var pointName = (isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel)?.PointNameInput?.value ?? "Point";
            if (motionKind == "MoveL")
            {
                runtimeController?.PreviewTcpPose(currentValuesToDouble(), $"저장 위치 {pointName} 직선 이동 입력 미리보기");
                return;
            }

            if (TryGetSavedJointTarget(currentValuesToDouble(), pointName, out var savedJointTarget))
            {
                PreviewSavedMoveJ(pointName, savedJointTarget);
                return;
            }

            runtimeController?.PreviewPointMoveJ(currentValuesToDouble(), $"저장 위치 {pointName} 관절 이동 입력 미리보기");
        }

        private void DispatchEditedPointCandidate()
        {
            if (!IsAnyPanelVisible() || runtimeController == null)
            {
                return;
            }

            if (!TryReadActivePanelValues(out var target, out _))
            {
                return;
            }

            if (!runtimeController.CurrentSnapshot.DryRunEnabled)
            {
                PreviewEditedPointCandidate();
                return;
            }

            var pointName = (isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel)?.PointNameInput?.value ?? "Point";
            if (motionKind == "MoveL")
            {
                runtimeController.ApplyTcpPose(target, $"저장 위치 {pointName} 직선 이동 입력 적용");
                SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
                return;
            }

            var result = TryGetSavedJointTarget(target, pointName, out var savedJointTarget)
                ? ApplySavedMoveJ(pointName, savedJointTarget)
                : runtimeController.ApplyPointMoveJ(target, $"저장 위치 {pointName} 관절 이동 입력 적용");
            SetFeedback(result.IsSuccess ? runtimeController.CurrentSnapshot.LastFeedback : result.Message);
        }

        private void PreviewMotionCandidate()
        {
            if (!IsAnyPanelVisible())
            {
                SetFeedback("저장 위치 패널이 열려 있을 때만 미리보기를 실행한다.");
                return;
            }

            if (!CanPreview())
            {
                SetFeedback("연결 상태가 준비되지 않아 미리보기를 잠시 잠근다.");
                return;
            }

            if (!TryReadActivePanelValues(out var _, out var validationMessage))
            {
                SetFeedback(validationMessage);
                return;
            }

            var pointName = desktopPanel?.PointNameInput?.value ?? tabletPanel?.PointNameInput?.value ?? "Point";
            if (motionKind == "MoveL")
            {
                runtimeController?.PreviewTcpPose(currentValuesToDouble(), $"저장 위치 {pointName} 직선 이동 후보");
                SetFeedback($"[미리보기] 직선 이동 후보 · {pointName} · X {currentValues[0]:0.0} / Y {currentValues[1]:0.0} / Z {currentValues[2]:0.0}");
                return;
            }

            var result = TryGetSavedJointTarget(currentValuesToDouble(), pointName, out var savedJointTarget)
                ? PreviewSavedMoveJ(pointName, savedJointTarget)
                : runtimeController?.PreviewPointMoveJ(currentValuesToDouble(), $"저장 위치 {pointName} 관절 이동 후보")
                    ?? FairinoResult.Fail(-1, "관절 이동 준비 기능을 찾지 못했다.");
            SetFeedback(result.IsSuccess
                ? $"[미리보기] 관절 이동 후보 · {pointName} · {result.Message}"
                : result.Message);
        }

        private void ApplyMotionCandidate()
        {
            if (!IsAnyPanelVisible())
            {
                SetFeedback("저장 위치 패널이 열려 있을 때만 적용할 수 있다.");
                return;
            }

            if (!CanApply())
            {
                SetFeedback("연결 상태가 준비되지 않아 적용할 수 없다. 연결/에러 상태를 먼저 확인해라.");
                return;
            }

            if (!TryReadActivePanelValues(out var target, out var validationMessage))
            {
                SetFeedback(validationMessage);
                return;
            }

            if (!EnsurePointLiveSyncGate("포인트 적용"))
            {
                return;
            }

            if (motionKind != "MoveL")
            {
                var pointName = (isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel)?.PointNameInput?.value ?? "Point";
                var hasSavedJointTarget = TryGetSavedJointTarget(target, pointName, out var savedJointTarget);
                var shouldRouteSavedPointThroughLiveApproval = runtimeController != null
                    && hasSavedJointTarget
                    && runtimeController.ShouldRouteSavedPointMoveJOperatorThroughLiveApproval();
                if (shouldRouteSavedPointThroughLiveApproval)
                {
                    if (!EnsurePopupCoordinatorAvailableForLiveApproval("포인트 적용"))
                    {
                        return;
                    }

                    var previewResult = PreviewSavedMoveJ(pointName, savedJointTarget);
                    if (!previewResult.IsSuccess)
                    {
                        SetFeedback(previewResult.Message);
                        return;
                    }

                    runtimeController.PrepareSavedPointMoveJOperatorApproval(pointName, savedJointTarget);
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveJ"))
                    {
                        popupCoordinator.OpenMoveConfirmForProduct();
                        SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
                    }
                    else
                    {
                        SetFeedback(runtimeController.ExecutePendingSavedPointOperatorCommand());
                    }
                    return;
                }

                if (runtimeController != null
                    && runtimeController.ShouldRouteMoveJOperatorThroughLiveApproval())
                {
                    if (!EnsurePopupCoordinatorAvailableForLiveApproval("포인트 적용"))
                    {
                        return;
                    }

                    var previewResult = runtimeController.PreviewPointMoveJ(target, $"저장 위치 {pointName} 관절 이동 후보");
                    if (!previewResult.IsSuccess)
                    {
                        SetFeedback(previewResult.Message);
                        return;
                    }

                    runtimeController.PrepareMoveJOperatorApprovalSession();
                    if (runtimeController.ShouldRequireLiveApprovalPopupForProduct("MoveJ"))
                    {
                        popupCoordinator.OpenMoveConfirmForProduct();
                        SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
                    }
                    else
                    {
                        runtimeController.ExecutePreparedPreviewForProduct();
                        SetFeedback(runtimeController.CurrentSnapshot.LastFeedback);
                    }
                    return;
                }

                var moveJResult = hasSavedJointTarget
                    ? ApplySavedMoveJ(pointName, savedJointTarget)
                    : runtimeController?.ApplyPointMoveJ(target, "저장 위치 관절 이동 적용")
                        ?? FairinoResult.Fail(-1, "관절 이동 준비 기능을 찾지 못했다.");
                SetFeedback(moveJResult.IsSuccess
                    ? runtimeController.CurrentSnapshot.LastFeedback
                    : moveJResult.Message);
                return;
            }

            runtimeController?.ApplyTcpPose(target, "저장 위치 직선 이동 적용");
            SetFeedback(runtimeController != null ? runtimeController.CurrentSnapshot.LastFeedback : "[실행] 직선 이동 적용 요청");
            return;
        }

        private FairinoResult<RobotControlMotionRuntime> EnsureMotionRuntime()
        {
            var robotId = RobotSelectionBridge.GetSelectedRobotId();
            if (string.IsNullOrWhiteSpace(robotId))
            {
                motionRuntime = null;
                return FairinoResult<RobotControlMotionRuntime>.Fail(-1, "선택된 로봇이 없어서 PointMove runtime을 준비하지 못했다.");
            }

            if (motionRuntime != null && string.Equals(motionRuntime.RobotId, robotId, System.StringComparison.Ordinal))
            {
                return FairinoResult<RobotControlMotionRuntime>.Ok(motionRuntime, $"{robotId} runtime 재사용");
            }

            var createResult = RobotControlMotionRuntime.CreateFromSelection();
            if (!createResult.IsSuccess)
            {
                motionRuntime = null;
                return createResult;
            }

            motionRuntime = createResult.Value;
            return createResult;
        }

        private void SetFeedback(string message)
        {
            lastFeedback = string.IsNullOrWhiteSpace(message) ? "..." : message;
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
        }

        private bool ShouldShowFeedbackLine()
        {
            return !string.IsNullOrWhiteSpace(lastFeedback)
                && lastFeedback != "아직 실행한 명령이 없다."
                && (lastFeedback.Contains("[Sequence")
                    || lastFeedback.Contains("[Path")
                    || lastFeedback.Contains("[미리보기]")
                    || lastFeedback.Contains("프리뷰")
                    || lastFeedback.Contains("live")
                    || lastFeedback.Contains("[Confirm]")
                    || lastFeedback.Contains("[Delete]")
                    || lastFeedback.Contains("[Save]")
                    || lastFeedback.Contains("[Bulk]")
                    || lastFeedback.Contains("[Function]")
                    || lastFeedback.Contains("[Bundle]")
                    || lastFeedback.Contains("실패")
                    || lastFeedback.Contains("찾지 못했다")
                    || lastFeedback.Contains("먼저"));
        }

        private static string CompactFeedback(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            var trimmed = message.Trim();
            return trimmed.Length <= 90 ? trimmed : trimmed.Substring(0, 87) + "...";
        }

    }
}
