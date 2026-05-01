// Folder: StatusSafety - centralized live safety evaluation helpers isolated for later diagnostics-first reduction.
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
    // Handles current live gate evaluation, target fingerprinting, and dedicated motion/gripper preflight checks.
    // This 1st pass preserves current veto behavior; later phases can narrow authority without losing diagnostics.
    public sealed partial class RobotControlV3RuntimeController
    {
        private LiveCommandSafetyGateResult EvaluateLiveCommandSafety(
            LiveCommandKind kind,
            int requestedSpeedPercent,
            bool productionIkSafe,
            bool boundaryReady,
            bool collisionReady,
            bool hasGripperReadback,
            string approvalTargetKey = "",
            bool hasMatchingPreparedTarget = true,
            bool allowReadbackOnlyMotionPathOverride = false,
            bool hasDedicatedTinyMoveJMotionPath = false,
            bool isWithinTinyMoveRange = true,
            FairinoConnectionService gateConnectionService = null)
        {
            var approvalState = ConsumeLiveCommandApproval(kind, approvalTargetKey);
            return EvaluateLiveCommandSafetyCore(
                kind,
                requestedSpeedPercent,
                productionIkSafe,
                boundaryReady,
                collisionReady,
                hasGripperReadback,
                approvalState,
                hasMatchingPreparedTarget,
                allowReadbackOnlyMotionPathOverride,
                hasDedicatedTinyMoveJMotionPath,
                isWithinTinyMoveRange,
                gateConnectionService);
        }


        private LiveCommandSafetyGateResult EvaluateLiveCommandSafetyPreview(
            LiveCommandKind kind,
            int requestedSpeedPercent,
            bool productionIkSafe,
            bool boundaryReady,
            bool collisionReady,
            bool hasGripperReadback,
            bool allowReadbackOnlyMotionPathOverride = false,
            bool hasDedicatedTinyMoveJMotionPath = false,
            bool isWithinTinyMoveRange = true)
        {
            var preparedMotion = kind switch
            {
                LiveCommandKind.MoveJ => ResolvePreparedMotionContext(
                    kind,
                    previewUsesJointPose ? previewJointAnglesDeg : null,
                    null,
                    productionIkSafe),
                LiveCommandKind.MoveL => ResolvePreparedMotionContext(
                    kind,
                    null,
                    !previewUsesJointPose ? previewTcpPose : null,
                    productionIkSafe),
                _ => null,
            };

            return EvaluateLiveCommandSafetyCore(
                kind,
                requestedSpeedPercent,
                preparedMotion?.IsProductionIkSafe ?? productionIkSafe,
                preparedMotion?.IsBoundaryReady ?? boundaryReady,
                preparedMotion?.IsCollisionReady ?? collisionReady,
                hasGripperReadback,
                HasActiveLiveCommandSessionApproval() ? LiveCommandApprovalState.SessionActive : LiveCommandApprovalState.None,
                preparedMotion?.HasPreviewArtifact ?? true,
                allowReadbackOnlyMotionPathOverride,
                hasDedicatedTinyMoveJMotionPath,
                isWithinTinyMoveRange,
                gateConnectionService: null);
        }


        private LiveCommandSafetyGateResult EvaluateLiveCommandSafetyCore(
            LiveCommandKind kind,
            int requestedSpeedPercent,
            bool productionIkSafe,
            bool boundaryReady,
            bool collisionReady,
            bool hasGripperReadback,
            LiveCommandApprovalState approvalState,
            bool hasMatchingPreparedTarget,
            bool allowReadbackOnlyMotionPathOverride,
            bool hasDedicatedTinyMoveJMotionPath,
            bool isWithinTinyMoveRange,
            FairinoConnectionService gateConnectionService)
        {
            liveCommandSafetyGate ??= new LiveCommandSafetyGate();
            var effectiveConnectionService = gateConnectionService ?? connectionService;
            var dedicatedTinyMoveJOverride =
                kind == LiveCommandKind.MoveJ
                && hasDedicatedTinyMoveJMotionPath
                && allowReadbackOnlyMotionPathOverride;
            var dedicatedGripperOverride =
                kind == LiveCommandKind.MoveGripper
                && currentLiveSessionMode == LiveCommandSessionMode.GripperOnly
                && HasDedicatedLiveGripperSmokePathConfigured();
            var loopApprovalActive = liveLoopApprovalExecutionContext || HasActiveLiveLoopApproval();
            var sessionApprovalActive = approvalState == LiveCommandApprovalState.SessionActive || HasActiveLiveCommandSessionApproval();
            if (kind != LiveCommandKind.ReadbackOnly
                && IsReadbackOnlyLiveClient(effectiveConnectionService)
                && !dedicatedTinyMoveJOverride
                && !dedicatedGripperOverride)
            {
                var readbackOnlyResult = new LiveCommandSafetyGateResult
                {
                    Kind = kind,
                    RequestedSpeedPercent = requestedSpeedPercent,
                    SpeedCapPercent = LiveCommandSafetyGate.DefaultLiveSpeedCapPercent,
                    RiskLevel = kind switch
                    {
                        LiveCommandKind.RobotDo or LiveCommandKind.ToolDo => LiveCommandRiskLevel.Medium,
                        LiveCommandKind.MoveGripper => LiveCommandRiskLevel.High,
                        _ => LiveCommandRiskLevel.Critical,
                    },
                    Status = LiveCommandGateStatus.ReadbackOnly,
                };

                var stateResult = effectiveConnectionService.SyncCurrentState();
                if (stateResult.IsSuccess)
                {
                    var state = stateResult.Value;
                    readbackOnlyResult.ReadbackSummary =
                        $"mode={state.RobotMode}; enabled={state.IsRobotEnabled}; queue={state.MotionQueueLength}; safety={state.SafetyCode}; fault={state.MainErrorCode}/{state.SubErrorCode}; eStop={state.IsEmergencyStop}; collision={state.IsCollisionDetected}; tool={state.ToolId}; user={state.UserId}";
                }

                readbackOnlyResult.BlockReasons.Add("live client is readback-only");
                readbackOnlyResult.ClearedReasons.Add("actual motion/IO/gripper commands remain locked on macOS live readback");
                return readbackOnlyResult;
            }

            var hasPreview = previewJointAnglesDeg != null || previewTcpPose != null;
            var evidence = ResolveTinyMoveJEvidenceGateState();
            var allowStateReadbackFallback = liveLoopApprovalExecutionContext
                || currentLiveSessionMode == LiveCommandSessionMode.LoopRunning;
            var fallbackState = default(FairinoRobotState);
            var hasFallbackState = allowStateReadbackFallback
                && TryResolveEvidenceBackedMixedLiveState(out fallbackState);
            var request = new LiveCommandSafetyGateRequest
            {
                Kind = kind,
                ConnectionService = effectiveConnectionService,
                AllowDryRun = snapshot.DryRunEnabled,
                OperatorConfirmed = approvalState == LiveCommandApprovalState.Consumed || sessionApprovalActive || loopApprovalActive,
                HasMatchingPreparedTarget = hasMatchingPreparedTarget,
                HasMatchingApprovalContext = approvalState != LiveCommandApprovalState.TargetMismatch || sessionApprovalActive || loopApprovalActive,
                SessionMode = currentLiveSessionMode,
                AllowReadbackOnlyMotionPathOverride = allowReadbackOnlyMotionPathOverride,
                AllowReadbackOnlyGripperPathOverride = dedicatedGripperOverride,
                HasDedicatedTinyMoveJMotionPath = hasDedicatedTinyMoveJMotionPath,
                IsWithinTinyMoveRange = isWithinTinyMoveRange,
                RequestedSpeedPercent = requestedSpeedPercent,
                SpeedCapPercent = LiveCommandSafetyGate.DefaultLiveSpeedCapPercent,
                ToolId = evidence.ToolId,
                UserId = evidence.UserId,
                CoordSystem = evidence.CoordSystem,
                HasResolvedCoordSystem = evidence.HasExplicitCoordSystem,
                HasFreshLatestState = evidence.StateEvidenceFresh && evidence.MatchesCurrentSession,
                HasFreshLatestDrift = evidence.DriftEvidenceFresh && evidence.MatchesCurrentSession,
                IsDriftWithinThreshold = evidence.DriftPassed && evidence.MatchesCurrentSession,
                LatestStateTimestampUtc = evidence.LatestState?.timestampUtc ?? string.Empty,
                LatestDriftTimestampUtc = evidence.LatestDrift?.timestampUtc ?? string.Empty,
                HasDryRunPreviewArtifact = hasPreview,
                IsProductionIkSafe = productionIkSafe,
                IsBoundaryDataReady = boundaryReady,
                IsTargetWithinBoundary = boundaryReady,
                IsCollisionDataReady = collisionReady,
                IsPredictedPathCollisionFree = collisionReady,
                HasGripperReadback = hasGripperReadback,
                AllowStateReadbackFailureFallback = allowStateReadbackFallback,
                HasFallbackState = hasFallbackState,
                FallbackState = fallbackState,
                StateReadbackFallbackReason = hasFallbackState ? "fresh mixed-live evidence" : string.Empty,
            };

            return liveCommandSafetyGate.Evaluate(request);
        }


        private FairinoResult BlockLiveCommand(LiveCommandSafetyGateResult gate, string auditLabel)
        {
            liveCommandSafetyGate ??= new LiveCommandSafetyGate();
            var artifactPath = liveCommandSafetyGate.WriteAudit(gate, auditLabel);
            var message = $"[Live Gate] {gate.Status}: {string.Join(" / ", gate.BlockReasons)} · artifact={artifactPath}";
            PushFeedback(message);
            RememberOperatorBlockedReason(message);
            RefreshSnapshot();
            return FairinoResult.Fail(-70, message);
        }


        private FairinoResult PreflightLiveGripperOperatorPath(bool allowWarmup)
        {
            if (!ShouldUseLiveGripperOperatorPath())
            {
                return FairinoResult.Ok("gripper preflight bypassed");
            }

            var siblingResult = connectionService.CreateMotionSiblingSession();
            if (!siblingResult.IsSuccess || siblingResult.Value == null)
            {
                return FairinoResult.Fail(
                    siblingResult.ErrorCode != 0 ? siblingResult.ErrorCode : -86,
                    $"그리퍼 준비 안 됨 · live gripper 세션을 만들지 못했다. {siblingResult.Message}");
            }

            var liveService = siblingResult.Value;
            var profile = config?.GetGripperProfile() ?? FairinoGripperProfile.Pgea10040Default;
            var statusResult = liveService.ReadGripperStatus();
            if (!statusResult.IsSuccess)
            {
                return FairinoResult.Fail(
                    statusResult.ErrorCode != 0 ? statusResult.ErrorCode : -86,
                    $"그리퍼 준비 안 됨 · 상태 읽기 실패: {statusResult.Message}");
            }

            if (RobotControlPeripheralFacade.IsGripperActivationReadyForProfile(statusResult.Value, profile)
                || RobotControlPeripheralFacade.IsGripperDiscreteFieldReady(statusResult.Value))
            {
                return FairinoResult.Ok(
                    RobotControlPeripheralFacade.IsGripperActivationReadyForProfile(statusResult.Value, profile)
                        ? "gripper ready"
                        : "gripper field-ready");
            }

            if (allowWarmup && !liveGripperWarmupAttemptedThisConnection)
            {
                liveGripperWarmupAttemptedThisConnection = true;
                var warmup = RobotControlPeripheralFacade.TryWarmUpLiveGripper(liveService, profile);
                var finalStatus = liveService.ReadGripperStatus();
                if (finalStatus.IsSuccess
                    && (RobotControlPeripheralFacade.IsGripperActivationReadyForProfile(finalStatus.Value, profile)
                        || RobotControlPeripheralFacade.IsGripperDiscreteFieldReady(finalStatus.Value)))
                {
                    return FairinoResult.Ok(
                        RobotControlPeripheralFacade.IsGripperActivationReadyForProfile(finalStatus.Value, profile)
                            ? (warmup.IsSuccess
                                ? "[Live Gripper] warm-up 1회 완료"
                                : "[Live Gripper] warm-up 후 activation ready 확인")
                            : "[Live Gripper] warm-up 후 field-ready 확인");
                }

                if (finalStatus.IsSuccess)
                {
                    return FairinoResult.Fail(
                        warmup.ErrorCode != 0 ? warmup.ErrorCode : -86,
                        RobotControlPeripheralFacade.BuildOperatorGripperNotReadyMessage(finalStatus.Value, true));
                }

                return FairinoResult.Fail(
                    warmup.ErrorCode != 0 ? warmup.ErrorCode : -86,
                    $"그리퍼 준비 안 됨 · warm-up 후 상태 읽기 실패: {finalStatus.Message}");
            }

            return FairinoResult.Fail(
                -86,
                RobotControlPeripheralFacade.BuildOperatorGripperNotReadyMessage(statusResult.Value, false));
        }


        private PreparedLiveMotionContext ResolvePreparedMotionContext(
            LiveCommandKind kind,
            double[] jointTarget,
            double[] tcpTarget,
            bool productionIkSafeFallback)
        {
            var targetKey = BuildMotionTargetKey(kind, jointTarget, tcpTarget);
            var contextMatches = !string.IsNullOrWhiteSpace(targetKey)
                && preparedLiveMotionContext.Kind == kind
                && string.Equals(preparedLiveMotionContext.TargetKey, targetKey, StringComparison.Ordinal);

            if (contextMatches)
            {
                return preparedLiveMotionContext;
            }

            return new PreparedLiveMotionContext
            {
                Kind = kind,
                TargetKey = targetKey,
                HasPreviewArtifact = false,
                IsProductionIkSafe = productionIkSafeFallback,
                IsBoundaryReady = false,
                IsCollisionReady = false,
                Source = contextMatches ? preparedLiveMotionContext.Source : "mismatch",
            };
        }


        private bool HasDedicatedTinyMoveJLivePathConfigured()
        {
            if (!FairinoRobotClientFactory.IsTinyMoveJLiveEnabled())
            {
                return false;
            }

            var client = FairinoRobotClientFactory.CreateLive(new FairinoErrorTranslator(), preferMotionCapableDirect: true);
            return client is IFairinoLiveClientDiagnostics { IsReadbackOnly: false };
        }


        private bool HasDedicatedLiveGripperSmokePathConfigured()
        {
            if (FairinoRobotClientFactory.IsLiveGripperSmokeEnabled())
            {
                return true;
            }

            return currentLiveSessionMode == LiveCommandSessionMode.GripperOnly
                && connectionService != null
                && !connectionService.IsMockMode
                && connectionService.Client.IsConnected
                && IsReadbackOnlyLiveClient();
        }


        private bool TryEvaluateTinyMoveJRange(double[] targetJointAnglesDeg, out double maxJointDeltaDeg, out int maxJointDeltaIndex)
        {
            maxJointDeltaDeg = 0d;
            maxJointDeltaIndex = -1;
            if (targetJointAnglesDeg == null || targetJointAnglesDeg.Length == 0)
            {
                return false;
            }

            var liveJointBaseline = connectionService?.LastState.JointPosDeg;
            if (liveJointBaseline == null || liveJointBaseline.Length == 0)
            {
                liveJointBaseline = currentState.JointPosDeg;
            }

            if (liveJointBaseline == null || liveJointBaseline.Length == 0)
            {
                return false;
            }

            var jointCount = templateDefinition?.JointCount ?? System.Math.Min(liveJointBaseline.Length, targetJointAnglesDeg.Length);
            var length = System.Math.Min(System.Math.Min(liveJointBaseline.Length, targetJointAnglesDeg.Length), jointCount);
            if (length <= 0)
            {
                return false;
            }

            for (var index = 0; index < length; index++)
            {
                var delta = System.Math.Abs(targetJointAnglesDeg[index] - liveJointBaseline[index]);
                if (delta > maxJointDeltaDeg)
                {
                    maxJointDeltaDeg = delta;
                    maxJointDeltaIndex = index;
                }
            }

            return maxJointDeltaDeg <= RobotControlMotionRuntime.TinyMoveJMaxJointDeltaDeg
                + RobotControlMotionRuntime.TinyMoveJRangeToleranceDeg;
        }


        private string BuildMotionTargetKey(LiveCommandKind kind, double[] jointTarget, double[] tcpTarget)
        {
            var coord = string.IsNullOrWhiteSpace(snapshot.CoordSystem) ? "Base" : snapshot.CoordSystem;
            var session = liveStateRecorder?.SessionId;
            if (string.IsNullOrWhiteSpace(session))
            {
                session = ResolveTinyMoveJEvidenceGateState().LatestState?.sessionId ?? "no-session";
            }

            var speed = ResolveRequestedSpeedPercent();
            if (jointTarget != null)
            {
                return $"{kind}|speed={speed}|coord={coord}|session={session}|joint={FormatTargetValues(jointTarget)}";
            }

            if (tcpTarget != null)
            {
                return $"{kind}|speed={speed}|coord={coord}|session={session}|tcp={FormatTargetValues(tcpTarget)}";
            }

            return string.Empty;
        }


        private static string FormatTargetValues(double[] values)
        {
            if (values == null || values.Length == 0)
            {
                return string.Empty;
            }

            var formatted = new string[values.Length];
            for (var index = 0; index < values.Length; index++)
            {
                formatted[index] = values[index].ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
            }

            return string.Join(",", formatted);
        }

    }
}
