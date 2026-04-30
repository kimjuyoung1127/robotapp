// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    public sealed partial class RobotControlV3RuntimeController
    {
        public void ToggleDryRun()
        {
            snapshot.DryRunEnabled = !snapshot.DryRunEnabled;
            InvalidateLiveApprovalContext();
            PushFeedback(snapshot.DryRunEnabled ? "[DryRun] ON" : "[DryRun] OFF");
            RefreshSnapshot();
        }

        public void SetCoordSystem(string coordSystem)
        {
            snapshot.CoordSystem = coordSystem is "Tool" or "User" ? coordSystem : "Base";
            RefreshSnapshot();
        }

        public void UndoPreview()
        {
            if (undoJointHistory.Count == 0)
            {
                PushFeedback("Undo 할 이력이 없다.");
                RefreshSnapshot();
                return;
            }

            redoJointHistory.Push(CopyJointArray(previewJointAnglesDeg ?? currentState.JointPosDeg));
            previewJointAnglesDeg = undoJointHistory.Pop();
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            InvalidateLiveApprovalContext();
            CapturePreparedMotionContext(LiveCommandKind.MoveJ, previewJointAnglesDeg, null, productionIkSafe: true, boundaryReady: false, collisionReady: false, "Undo");
            ApplyVisualState();
            PushFeedback("[Undo] 이전 관절 프리뷰 복원");
            RefreshSnapshot();
        }

        public void RedoPreview()
        {
            if (redoJointHistory.Count == 0)
            {
                PushFeedback("Redo 할 이력이 없다.");
                RefreshSnapshot();
                return;
            }

            undoJointHistory.Push(CopyJointArray(previewJointAnglesDeg ?? currentState.JointPosDeg));
            previewJointAnglesDeg = redoJointHistory.Pop();
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            InvalidateLiveApprovalContext();
            CapturePreparedMotionContext(LiveCommandKind.MoveJ, previewJointAnglesDeg, null, productionIkSafe: true, boundaryReady: false, collisionReady: false, "Redo");
            ApplyVisualState();
            PushFeedback("[Redo] 다음 관절 프리뷰 복원");
            RefreshSnapshot();
        }

        public void PreviewPreset(string presetName)
        {
            if (!EnsureReadyForCommand("프리셋 미리보기"))
            {
                return;
            }

            var preset = ResolvePreset(presetName);
            if (!preset.HasValue)
            {
                PushFeedback($"{presetName} 프리셋을 찾지 못했다.");
                RefreshSnapshot();
                return;
            }

            var presetValue = preset.Value;
            previewJointAnglesDeg = presetValue.JointAnglesDeg;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            InvalidateLiveApprovalContext();
            CapturePreparedMotionContext(LiveCommandKind.MoveJ, previewJointAnglesDeg, null, productionIkSafe: true, boundaryReady: false, collisionReady: false, $"Preset:{presetValue.Name}");
            RecordUndo(previewJointAnglesDeg);
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback($"[Preview] {presetValue.Name} 프리셋");
            RefreshSnapshot();
        }

        public FairinoResult ApplyPreset(string presetName)
        {
            if (!EnsureReadyForCommand("프리셋 적용"))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            var preset = ResolvePreset(presetName);
            if (!preset.HasValue)
            {
                var fail = FairinoResult.Fail(-31, $"{presetName} 프리셋을 찾지 못했다.");
                PushFeedback(fail.Message);
                RefreshSnapshot();
                return fail;
            }

            var presetValue = preset.Value;
            return ApplyJointAngles(presetValue.JointAnglesDeg, $"{presetValue.Name} 프리셋");
        }

        public void PreviewJointAngles(double[] jointAnglesDeg, string reason = "관절 프리뷰")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return;
            }

            previewJointAnglesDeg = CopyJointArray(jointAnglesDeg);
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            InvalidateLiveApprovalContext();
            CapturePreparedMotionContext(LiveCommandKind.MoveJ, previewJointAnglesDeg, null, productionIkSafe: true, boundaryReady: false, collisionReady: false, reason);
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback($"[Preview] {reason}");
            RefreshSnapshot();
        }

        public void RestoreJointPreview()
        {
            if (!EnsureReadyForCommand("관절 복원"))
            {
                return;
            }

            previewJointAnglesDeg = CopyJointArray(currentState.JointPosDeg);
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            InvalidateLiveApprovalContext();
            CapturePreparedMotionContext(LiveCommandKind.MoveJ, previewJointAnglesDeg, null, productionIkSafe: true, boundaryReady: false, collisionReady: false, "Restore");
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback("[Restore] 현재 관절값으로 복원");
            RefreshSnapshot();
        }

        public FairinoResult ApplyJointAngles(double[] jointAnglesDeg, string reason = "관절 적용", bool liveProductionIkEligible = true)
        {
            if (!EnsureReadyForCommand(reason))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            if (jointAnglesDeg == null || jointAnglesDeg.Length < templateDefinition.JointCount)
            {
                var invalid = FairinoResult.Fail(-32, "관절 적용 값이 부족하다.");
                PushFeedback(invalid.Message);
                RefreshSnapshot();
                return invalid;
            }

            RecordUndo(currentState.JointPosDeg);
            if (snapshot.DryRunEnabled)
            {
                currentState = new FairinoRobotState(jointAnglesDeg, ComputeTcpPoseFromJoints(jointAnglesDeg), isRobotEnabled: connectionService.Client.IsEnabled);
                templateDefinition.PosePresetProvider?.UpdateCurrent(jointAnglesDeg);
                previewJointAnglesDeg = null;
                previewTcpPose = null;
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
                ClearPreparedMotionContext();
                InvalidateLiveApprovalContext();
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback($"[DryRun Apply] {reason}");
                RefreshSnapshot();
                return FairinoResult.Ok("DryRun 적용");
            }

            var preparedMotion = ResolvePreparedMotionContext(
                LiveCommandKind.MoveJ,
                jointAnglesDeg,
                null,
                liveProductionIkEligible);
            var gate = EvaluateLiveCommandSafety(
                LiveCommandKind.MoveJ,
                ResolveRequestedSpeedPercent(),
                preparedMotion.IsProductionIkSafe && liveProductionIkEligible,
                boundaryReady: preparedMotion.IsBoundaryReady,
                collisionReady: preparedMotion.IsCollisionReady,
                hasGripperReadback: false,
                approvalTargetKey: preparedMotion.TargetKey,
                hasMatchingPreparedTarget: preparedMotion.HasPreviewArtifact);
            if (!gate.CanExecuteLive)
            {
                return BlockLiveCommand(gate, "live-movej-blocked");
            }

            var runtime = RobotControlMotionRuntime.CreateFromSelection();
            if (!runtime.IsSuccess)
            {
                PushFeedback(runtime.Message);
                RefreshSnapshot();
                return new FairinoResult(runtime.ErrorCode, runtime.Message);
            }

            var result = runtime.Value.DispatchMoveJ(jointAnglesDeg, ResolveRequestedSpeedPercent());
            if (result.IsSuccess)
            {
                currentState = new FairinoRobotState(jointAnglesDeg, ComputeTcpPoseFromJoints(jointAnglesDeg), isRobotEnabled: connectionService.Client.IsEnabled);
                templateDefinition.PosePresetProvider?.UpdateCurrent(jointAnglesDeg);
                previewJointAnglesDeg = null;
                previewTcpPose = null;
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
                ClearPreparedMotionContext();
                InvalidateLiveApprovalContext();
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback($"[Dispatch] MoveJ 완료 · {reason}");
            }
            else
            {
                PushFeedback(result.Message);
            }

            ResetLiveSessionModeAfterLiveAttempt(LiveCommandKind.MoveJ, result);
            RefreshSnapshot();
            return result;
        }

        public FairinoResult ApplyTeachingMoveJ(double[] jointAnglesDeg, string reason = "Teaching MoveJ")
        {
            var canUseTinyMoveJ = !snapshot.DryRunEnabled
                && connectionService != null
                && !connectionService.IsMockMode
                && HasDedicatedTinyMoveJLivePathConfigured()
                && TryEvaluateTinyMoveJRange(jointAnglesDeg, out _, out _);
            if (canUseTinyMoveJ)
            {
                previewJointAnglesDeg = CopyJointArray(jointAnglesDeg);
                previewTcpPose = null;
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = true;
                CapturePreparedMotionContext(
                    LiveCommandKind.MoveJ,
                    previewJointAnglesDeg,
                    null,
                    productionIkSafe: true,
                    boundaryReady: false,
                    collisionReady: false,
                    reason);
            }

            return canUseTinyMoveJ
                ? ApplyTinyMoveJ(jointAnglesDeg, reason)
                : ApplyJointAngles(jointAnglesDeg, reason);
        }

        private FairinoResult ApplyTinyMoveJ(double[] jointAnglesDeg, string reason = "tiny MoveJ 적용")
        {
            if (snapshot.DryRunEnabled || connectionService == null || connectionService.IsMockMode)
            {
                return ApplyJointAngles(jointAnglesDeg, reason);
            }

            if (!EnsureReadyForCommand(reason))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            if (jointAnglesDeg == null || jointAnglesDeg.Length < templateDefinition.JointCount)
            {
                var invalid = FairinoResult.Fail(-32, "tiny MoveJ 대상 관절 값이 부족하다.");
                PushFeedback(invalid.Message);
                RefreshSnapshot();
                return invalid;
            }

            var preparedMotion = ResolvePreparedMotionContext(
                LiveCommandKind.MoveJ,
                jointAnglesDeg,
                null,
                productionIkSafeFallback: true);
            var dedicatedTinyMoveJPath = HasDedicatedTinyMoveJLivePathConfigured();
            var withinTinyRange = TryEvaluateTinyMoveJRange(jointAnglesDeg, out _, out _);
            if (dedicatedTinyMoveJPath
                && withinTinyRange
                && currentLiveSessionMode == LiveCommandSessionMode.LiveControl)
            {
                SetLiveSessionMode(LiveCommandSessionMode.TinyMoveJOnly);
            }

            var gate = EvaluateLiveCommandSafety(
                LiveCommandKind.MoveJ,
                ResolveRequestedSpeedPercent(),
                productionIkSafe: preparedMotion.IsProductionIkSafe,
                boundaryReady: preparedMotion.IsBoundaryReady,
                collisionReady: preparedMotion.IsCollisionReady,
                hasGripperReadback: false,
                approvalTargetKey: preparedMotion.TargetKey,
                hasMatchingPreparedTarget: preparedMotion.HasPreviewArtifact,
                allowReadbackOnlyMotionPathOverride: dedicatedTinyMoveJPath,
                hasDedicatedTinyMoveJMotionPath: dedicatedTinyMoveJPath,
                isWithinTinyMoveRange: withinTinyRange);
            if (!gate.CanExecuteLive)
            {
                return BlockLiveCommand(gate, "live-tiny-movej-blocked");
            }

            var runtime = RobotControlMotionRuntime.CreateFromSelection(
                preferMotionCapableDirect: true,
                existingConnectionService: connectionService);
            if (!runtime.IsSuccess)
            {
                PushFeedback(runtime.Message);
                RefreshSnapshot();
                return new FairinoResult(runtime.ErrorCode, runtime.Message);
            }

            var liveJointBaseline = connectionService.LastState.JointPosDeg;
            var result = runtime.Value.DispatchTinyMoveJ(liveJointBaseline, jointAnglesDeg, ResolveRequestedSpeedPercent());
            if (result.IsSuccess)
            {
                currentState = new FairinoRobotState(jointAnglesDeg, ComputeTcpPoseFromJoints(jointAnglesDeg), isRobotEnabled: true);
                connectionService.SeedLastState(currentState);
                templateDefinition.PosePresetProvider?.UpdateCurrent(jointAnglesDeg);
                previewJointAnglesDeg = null;
                previewTcpPose = null;
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
                ClearPreparedMotionContext();
                InvalidateLiveApprovalContext();
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback($"[Dispatch] tiny MoveJ 완료 · {reason}");
            }
            else
            {
                PushFeedback(result.Message);
            }

            ResetLiveSessionModeAfterLiveAttempt(LiveCommandKind.MoveJ, result);
            RefreshSnapshot();
            return result;
        }

        public void PreviewTcpPose(double[] tcpPose, string reason = "TCP 프리뷰")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return;
            }

            previewTcpPose = CopyPoseArray(tcpPose);
            previewTcpVisualJointAnglesDeg = TrySolvePointMoveJoints(tcpPose, out var jointTarget).IsSuccess
                ? jointTarget
                : null;
            previewUsesJointPose = false;
            InvalidateLiveApprovalContext();
            CapturePreparedMotionContext(LiveCommandKind.MoveL, null, previewTcpPose, productionIkSafe: previewTcpVisualJointAnglesDeg != null, boundaryReady: false, collisionReady: false, reason);
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback($"[Preview] {reason}");
            RefreshSnapshot();
        }

        public FairinoResult ApplyTcpPose(double[] tcpPose, string reason = "TCP 적용")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            if (tcpPose == null || tcpPose.Length < 6)
            {
                var invalid = FairinoResult.Fail(-33, "TCP 적용 값이 부족하다.");
                PushFeedback(invalid.Message);
                RefreshSnapshot();
                return invalid;
            }

            if (snapshot.DryRunEnabled)
            {
                RecordUndo(currentState.JointPosDeg);
                var solveResult = TrySolvePointMoveJoints(tcpPose, out var visualJointTarget);
                if (solveResult.IsSuccess)
                {
                    currentState = new FairinoRobotState(visualJointTarget, CopyPoseArray(tcpPose), isRobotEnabled: connectionService.Client.IsEnabled);
                    templateDefinition.PosePresetProvider?.UpdateCurrent(visualJointTarget);
                    previewJointAnglesDeg = null;
                    previewTcpPose = null;
                    previewTcpVisualJointAnglesDeg = null;
                    previewUsesJointPose = false;
                    ClearPreparedMotionContext();
                    InvalidateLiveApprovalContext();
                    requestStageRefocus = true;
                    ApplyVisualState();
                    PushFeedback($"[DryRun Apply] {reason} · visual IK");
                    RefreshSnapshot();
                    return FairinoResult.Ok("DryRun TCP 적용");
                }

                previewTcpPose = CopyPoseArray(tcpPose);
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
                CapturePreparedMotionContext(LiveCommandKind.MoveL, null, previewTcpPose, productionIkSafe: false, boundaryReady: false, collisionReady: false, $"{reason}:marker-only");
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback($"[DryRun Apply] {reason} · 시각 IK 실패, 목표 마커만 표시");
                RefreshSnapshot();
                return FairinoResult.Ok("DryRun TCP 적용");
            }

            var preparedMotion = ResolvePreparedMotionContext(
                LiveCommandKind.MoveL,
                null,
                tcpPose,
                productionIkSafeFallback: previewTcpVisualJointAnglesDeg != null);
            var gate = EvaluateLiveCommandSafety(
                LiveCommandKind.MoveL,
                ResolveRequestedSpeedPercent(),
                productionIkSafe: preparedMotion.IsProductionIkSafe,
                boundaryReady: preparedMotion.IsBoundaryReady,
                collisionReady: preparedMotion.IsCollisionReady,
                hasGripperReadback: false,
                approvalTargetKey: preparedMotion.TargetKey,
                hasMatchingPreparedTarget: preparedMotion.HasPreviewArtifact);
            if (!gate.CanExecuteLive)
            {
                return BlockLiveCommand(gate, "live-movel-blocked");
            }

            var runtime = RobotControlMotionRuntime.CreateFromSelection();
            if (!runtime.IsSuccess)
            {
                PushFeedback(runtime.Message);
                RefreshSnapshot();
                return new FairinoResult(runtime.ErrorCode, runtime.Message);
            }

            var result = runtime.Value.DispatchMoveL(tcpPose, ResolveRequestedSpeedPercent());
            if (result.IsSuccess)
            {
                previewTcpPose = CopyPoseArray(tcpPose);
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
                ClearPreparedMotionContext();
                InvalidateLiveApprovalContext();
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback($"[Dispatch] MoveL 완료 · {reason}");
            }
            else
            {
                PushFeedback(result.Message);
            }

            ResetLiveSessionModeAfterLiveAttempt(LiveCommandKind.MoveL, result);
            RefreshSnapshot();
            return result;
        }

        public FairinoResult PreviewPointMoveJ(double[] tcpPose, string reason = "포인트 MoveJ 후보")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            var solveResult = TrySolvePointMoveJoints(tcpPose, out var jointTarget);
            if (!solveResult.IsSuccess)
            {
                PushFeedback(solveResult.Message);
                RefreshSnapshot();
                return solveResult;
            }

            previewJointAnglesDeg = jointTarget;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            InvalidateLiveApprovalContext();
            CapturePreparedMotionContext(LiveCommandKind.MoveJ, previewJointAnglesDeg, null, productionIkSafe: false, boundaryReady: false, collisionReady: false, reason);
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback($"[Preview] {reason}");
            RefreshSnapshot();
            return FairinoResult.Ok("Point MoveJ preview ready");
        }

        public FairinoResult ApplyPointMoveJ(double[] tcpPose, string reason = "포인트 MoveJ 적용")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            var solveResult = TrySolvePointMoveJoints(tcpPose, out var jointTarget);
            if (!solveResult.IsSuccess)
            {
                PushFeedback(solveResult.Message);
                RefreshSnapshot();
                return solveResult;
            }

            return ApplyJointAngles(jointTarget, reason, liveProductionIkEligible: false);
        }
    }
}
