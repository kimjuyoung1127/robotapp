// Folder: Shared - session mode and lightweight live-approval routing summaries shared across V3 panels.
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    // Tracks current live session mode and exposes the simplest product-facing routing truth.
    // Token issuance/confirmation stays in TokenLifecycle, while pending command execution stays in CommandApproval/LoopApproval.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string SetLiveSessionModeForDebug(string sessionMode)
        {
            SetLiveSessionMode(ParseLiveCommandSessionMode(sessionMode));
            PushFeedback($"[Live Session] {BuildLiveSessionModeDisplay(currentLiveSessionMode)}");
            RefreshSnapshot();
            return $"sessionMode={currentLiveSessionMode}; summary={BuildLiveSessionModeSummary(currentLiveSessionMode)}";
        }


        public string GetLiveSessionModeSummaryForDebug()
        {
            return $"sessionMode={currentLiveSessionMode}; summary={BuildLiveSessionModeSummary(currentLiveSessionMode)}";
        }


        internal void SetLiveSessionMode(LiveCommandSessionMode mode)
        {
            if (currentLiveSessionMode == mode)
            {
                return;
            }

            currentLiveSessionMode = mode;
            InvalidateLiveApprovalContext();
        }


        public bool HasActiveLiveSessionApprovalForProduct()
        {
            return HasActiveLiveCommandSessionApproval();
        }


        public string ResolvePendingLiveCommandKindForProduct()
        {
            if (hasPendingGripperOperatorCommand)
            {
                return LiveCommandKind.MoveGripper.ToString();
            }

            if (hasPendingSavedPointOperatorCommand)
            {
                return LiveCommandKind.MoveJ.ToString();
            }

            if (hasPendingWaypointSequenceOperatorCommand)
            {
                return LiveCommandKind.MoveJ.ToString();
            }

            if (previewUsesJointPose && previewJointAnglesDeg != null)
            {
                return LiveCommandKind.MoveJ.ToString();
            }

            if (!previewUsesJointPose && previewTcpPose != null)
            {
                return LiveCommandKind.MoveL.ToString();
            }

            return LiveCommandKind.ReadbackOnly.ToString();
        }


        public bool ShouldRouteGripperOperatorThroughLiveApproval()
        {
            return ShouldUseLiveGripperOperatorPath();
        }


        public bool CanIssueLiveGripperOperatorWrite()
        {
            return ShouldUseLiveGripperOperatorPath() && snapshot.MotionGateReady;
        }


        public bool HasPendingGripperOperatorApproval()
        {
            return hasPendingGripperOperatorCommand;
        }


        public bool HasPendingWaypointSequenceOperatorApproval()
        {
            return hasPendingWaypointSequenceOperatorCommand;
        }


        public bool HasPendingSavedPointOperatorApproval()
        {
            return hasPendingSavedPointOperatorCommand;
        }


        public bool ShouldRouteMoveJOperatorThroughLiveApproval()
        {
            return ShouldUseLiveMoveJOperatorPath();
        }


        public bool ShouldRouteSavedPointMoveJOperatorThroughLiveApproval()
        {
            return ShouldUseSavedPointMoveJOperatorPath();
        }


        public string PrepareMoveJOperatorApprovalSession()
        {
            if (!ShouldUseLiveMoveJOperatorPath())
            {
                return "moveJOperatorApproval=False; reason=live operator path disabled";
            }

            snapshot.LiveBlockedReason = string.Empty;
            SetLiveSessionMode(LiveCommandSessionMode.LiveControl);
            if (snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = false;
                InvalidateLiveApprovalContext();
            }

            RefreshSnapshot();
            return $"moveJOperatorApproval=True; session={currentLiveSessionMode}; dryRun={snapshot.DryRunEnabled}";
        }
    }
}
