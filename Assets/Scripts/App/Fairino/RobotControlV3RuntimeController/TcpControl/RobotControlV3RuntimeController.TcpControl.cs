// Folder: TcpControl - TCP preview/apply entry points for V3 TCP jog surfaces.
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
    // Handles TCP preview/apply and shared cartesian preview rules used by point-move surfaces.
    // Joint-specific preview/apply stays in JointControl and panel orchestration stays outside this partial.
    public sealed partial class RobotControlV3RuntimeController
    {
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
            if (!ShouldPreserveLiveApprovalContextForSequencePreview())
            {
                InvalidateLiveApprovalContext();
            }
            CapturePreparedMotionContext(LiveCommandKind.MoveL, null, previewTcpPose, productionIkSafe: previewTcpVisualJointAnglesDeg != null, boundaryReady: false, collisionReady: false, reason);
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback($"[Preview] {reason}");
            RefreshSnapshot();
        }


        private bool ShouldPreserveLiveApprovalContextForSequencePreview()
        {
            return liveLoopApprovalExecutionContext
                || currentLiveSessionMode == LiveCommandSessionMode.LoopRunning;
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

    }
}
