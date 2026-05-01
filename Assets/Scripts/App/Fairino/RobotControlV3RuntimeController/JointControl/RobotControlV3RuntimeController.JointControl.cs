// Folder: JointControl - joint preview/apply and tiny-joint approval entry points for V3 joint control.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KineTutor3D.Math;
using KineTutor3D.UI.RobotControlV3;
using KineTutor3D.Visualization;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    // Handles joint preview/apply, preset preview/apply, and tiny-joint/live MoveJ operator entry paths.
    // Shared approval/session helpers live in Shared and broad safety evaluation stays in StatusSafety.
    public sealed partial class RobotControlV3RuntimeController
    {
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
            if (!ShouldPreserveLiveApprovalContextForSequencePreview())
            {
                InvalidateLiveApprovalContext();
            }
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
            var preferUnifiedLiveMoveJ = liveLoopApprovalExecutionContext
                || currentLiveSessionMode == LiveCommandSessionMode.LoopRunning
                || HasActiveLiveSessionApprovalForProduct();
            if (preferUnifiedLiveMoveJ)
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
                    boundaryReady: true,
                    collisionReady: true,
                    $"{reason}:mixed-live-sequence");
            }

            var canUseTinyMoveJ = !snapshot.DryRunEnabled
                && !preferUnifiedLiveMoveJ
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
                SetLiveSessionMode(LiveCommandSessionMode.LiveControl);
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

    }
}
