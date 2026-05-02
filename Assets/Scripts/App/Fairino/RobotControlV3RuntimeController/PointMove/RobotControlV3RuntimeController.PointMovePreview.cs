// Folder: PointMove - point move preview/apply entry points and primary action dispatch.
using KineTutor3D.Math;

namespace KineTutor3D.App.Fairino
{
    // Handles point-move preview/apply entry points and the main V3 primary action branch for point motion.
    // Sequence execution and mixed-live loop logic stay in separate PointMove partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        public void ExecutePrimaryAction()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ReadyToJog && TryExecutePendingPreview())
            {
                return;
            }

            switch (snapshot.StatusKind)
            {
                case RobotControlV3RuntimeStatusKind.Disconnected:
                    ConnectAndSyncDefaultAsync();
                    break;
                case RobotControlV3RuntimeStatusKind.ConnectedServoOff:
                    if (IsReadbackOnlyLiveClient())
                    {
                        SyncCurrentStateAsync();
                    }
                    else
                    {
                        EnableServo();
                    }
                    break;
                case RobotControlV3RuntimeStatusKind.ConnectedUnsynced:
                    SyncCurrentStateAsync();
                    break;
                case RobotControlV3RuntimeStatusKind.Fault:
                    ResetErrors();
                    break;
                default:
                    if (!TryRunTeachingSequenceOnce())
                    {
                        PushFeedback("실행할 저장 포인트가 없다.");
                    }

                    RefreshSnapshot();
                    break;
            }
        }


        public string ExecutePreparedPreviewForDebug()
        {
            var executed = TryExecutePendingPreview();
            RefreshSnapshot();
            return $"executed={executed}; status={snapshot.StatusKind}; dryRun={snapshot.DryRunEnabled}; feedback={snapshot.LastFeedback}";
        }


        public void ExecutePreparedPreviewForProduct()
        {
            TryExecutePendingPreview();
            RefreshSnapshot();
        }


        private bool TryExecutePendingPreview()
        {
            if (!EnsureReadyForCommand("실행"))
            {
                return false;
            }

            if (previewUsesJointPose && previewJointAnglesDeg != null)
            {
                ApplyTinyMoveJ(previewJointAnglesDeg, "실행 버튼 tiny MoveJ");
                return true;
            }

            if (!previewUsesJointPose && previewTcpPose != null)
            {
                ApplyTcpPose(previewTcpPose, "실행 버튼 MoveL");
                return true;
            }

            return false;
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
