// Folder: Shared - pending gripper and saved-point live command preparation/execution shared across V3 panels.
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    // Prepares and executes non-loop live operator commands after routing/session state is set.
    // Token issuance and confirmation remain in TokenLifecycle, and sequence loop execution stays in LoopApproval.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string PrepareSavedPointMoveJOperatorApproval(string pointName, double[] jointAnglesDeg)
        {
            if (!ShouldUseSavedPointMoveJOperatorPath())
            {
                return "savedPointMoveJOperatorApproval=False; reason=live operator path disabled";
            }

            if (jointAnglesDeg == null || jointAnglesDeg.Length < templateDefinition.JointCount)
            {
                return "savedPointMoveJOperatorApproval=False; reason=target missing";
            }

            hasPendingSavedPointOperatorCommand = true;
            pendingSavedPointOperatorName = string.IsNullOrWhiteSpace(pointName) ? "Point" : pointName.Trim();
            pendingSavedPointOperatorJointTarget = CopyJointArray(jointAnglesDeg);
            pendingSavedPointOperatorTargetKey = BuildMotionTargetKey(LiveCommandKind.MoveJ, pendingSavedPointOperatorJointTarget, null);
            pendingSavedPointOperatorRestoreDryRun = snapshot.DryRunEnabled;
            snapshot.LiveBlockedReason = string.Empty;
            SetLiveSessionMode(LiveCommandSessionMode.LiveControl);
            if (snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = false;
                InvalidateLiveApprovalContext();
            }

            RefreshSnapshot();
            return $"pendingSavedPointApproval=True; point={pendingSavedPointOperatorName}; dryRun={snapshot.DryRunEnabled}";
        }


        public string PrepareGripperOperatorApproval(float positionPercent)
        {
            var clamped = Mathf.Clamp(positionPercent, 0f, 100f);
            var preflight = PreflightLiveGripperOperatorPath(allowWarmup: true);
            if (!preflight.IsSuccess)
            {
                ClearPendingGripperOperatorCommandState();
                RememberOperatorBlockedReason(preflight.Message);
                PushFeedback(preflight.Message);
                RefreshSnapshot();
                return preflight.Message;
            }

            hasPendingGripperOperatorCommand = true;
            pendingGripperOperatorPercent = clamped;
            pendingGripperOperatorRestoreDryRun = snapshot.DryRunEnabled;
            SetLiveSessionMode(LiveCommandSessionMode.LiveControl);
            snapshot.LiveBlockedReason = string.Empty;

            if (snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = false;
                InvalidateLiveApprovalContext();
            }

            RefreshSnapshot();
            return $"pendingGripperApproval=True; percent={clamped:0.##}; dryRun={snapshot.DryRunEnabled}";
        }


        public string ExecutePendingGripperOperatorCommand()
        {
            if (!hasPendingGripperOperatorCommand)
            {
                return "pendingGripperApproval=False";
            }

            var clamped = pendingGripperOperatorPercent;
            var restoreDryRun = pendingGripperOperatorRestoreDryRun;
            ClearPendingGripperOperatorCommandState();
            var result = SetGripperPositionPercent(clamped);

            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
                PushFeedback($"{result.Message} · DryRun으로 다시 잠갔다.");
                RefreshSnapshot();
            }

            return result.Message;
        }


        public string ExecutePendingSavedPointOperatorCommand()
        {
            if (!hasPendingSavedPointOperatorCommand || pendingSavedPointOperatorJointTarget == null)
            {
                return "pendingSavedPointApproval=False";
            }

            var pointName = pendingSavedPointOperatorName;
            var jointTarget = CopyJointArray(pendingSavedPointOperatorJointTarget);
            var restoreDryRun = pendingSavedPointOperatorRestoreDryRun;
            ClearPendingSavedPointOperatorCommandState();
            var result = ApplyTeachingMoveJ(jointTarget, $"저장 위치 {pointName} 저장된 관절 이동 적용");

            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
                PushFeedback($"{result.Message} · DryRun으로 다시 잠갔다.");
                RefreshSnapshot();
            }

            return result.Message;
        }


        private string ResolvePreparedMotionTargetKey(LiveCommandKind kind)
        {
            return preparedLiveMotionContext.Kind == kind
                ? preparedLiveMotionContext.TargetKey ?? string.Empty
                : string.Empty;
        }


        private void CapturePreparedMotionContext(
            LiveCommandKind kind,
            double[] jointTarget,
            double[] tcpTarget,
            bool productionIkSafe,
            bool boundaryReady,
            bool collisionReady,
            string source)
        {
            preparedLiveMotionContext = new PreparedLiveMotionContext
            {
                Kind = kind,
                TargetKey = BuildMotionTargetKey(kind, jointTarget, tcpTarget),
                HasPreviewArtifact = jointTarget != null || tcpTarget != null,
                IsProductionIkSafe = productionIkSafe,
                IsBoundaryReady = boundaryReady,
                IsCollisionReady = collisionReady,
                Source = source ?? string.Empty,
            };
        }


        private void ClearPreparedMotionContext()
        {
            preparedLiveMotionContext = new PreparedLiveMotionContext();
        }


        private void CancelPendingGripperOperatorCommand()
        {
            var restoreDryRun = pendingGripperOperatorRestoreDryRun;
            ClearPendingGripperOperatorCommandState();
            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
            }
        }


        private void CancelPendingSavedPointOperatorCommand()
        {
            var restoreDryRun = pendingSavedPointOperatorRestoreDryRun;
            ClearPendingSavedPointOperatorCommandState();
            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
            }
        }


        private void ClearPendingGripperOperatorCommandState()
        {
            hasPendingGripperOperatorCommand = false;
            pendingGripperOperatorPercent = 100f;
            pendingGripperOperatorRestoreDryRun = false;
        }


        private void ClearPendingSavedPointOperatorCommandState()
        {
            hasPendingSavedPointOperatorCommand = false;
            pendingSavedPointOperatorName = string.Empty;
            pendingSavedPointOperatorJointTarget = null;
            pendingSavedPointOperatorTargetKey = string.Empty;
            pendingSavedPointOperatorRestoreDryRun = false;
        }
    }
}
