// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    public enum LiveCommandKind
    {
        ReadbackOnly,
        MoveJ,
        MoveL,
        RobotDo,
        ToolDo,
        MoveGripper,
    }

    public enum LiveCommandSessionMode
    {
        LiveControl,
        ReadbackOnly,
        GripperOnly,
        TinyMoveJOnly,
    }

    public enum LiveCommandGateStatus
    {
        Allowed,
        Blocked,
        ReadbackOnly,
        RequiresConfirm,
    }

    public enum LiveCommandRiskLevel
    {
        Low,
        Medium,
        High,
        Critical,
    }

    public sealed class LiveCommandSafetyGate
    {
        public const int DefaultLiveSpeedCapPercent = 10;

        public LiveCommandSafetyGateResult Evaluate(LiveCommandSafetyGateRequest request)
        {
            var result = new LiveCommandSafetyGateResult
            {
                Kind = request.Kind,
                RequestedSpeedPercent = request.RequestedSpeedPercent,
                SpeedCapPercent = request.SpeedCapPercent > 0 ? request.SpeedCapPercent : DefaultLiveSpeedCapPercent,
                RiskLevel = ResolveRisk(request.Kind),
                ToolId = request.ToolId,
                UserId = request.UserId,
                CoordSystem = request.CoordSystem,
                HasResolvedCoordSystem = request.HasResolvedCoordSystem,
                HasFreshLatestState = request.HasFreshLatestState,
                HasFreshLatestDrift = request.HasFreshLatestDrift,
                IsDriftWithinThreshold = request.IsDriftWithinThreshold,
                LatestStateTimestampUtc = request.LatestStateTimestampUtc,
                LatestDriftTimestampUtc = request.LatestDriftTimestampUtc,
            };

            if (!IsCommandAllowedForSession(request.Kind, request.SessionMode))
            {
                result.Block($"session mode {request.SessionMode} does not allow {request.Kind}");
                return result;
            }

            if (request.ConnectionService == null || request.ConnectionService.Client == null)
            {
                result.Block("connection service missing");
                return result;
            }

            var usingReadbackOnlyClient = IsReadbackOnlyClient(request.ConnectionService.Client);
            var allowTinyMoveJOverride =
                request.Kind == LiveCommandKind.MoveJ
                && request.HasDedicatedTinyMoveJMotionPath
                && request.AllowReadbackOnlyMotionPathOverride
                && request.SessionMode == LiveCommandSessionMode.TinyMoveJOnly;
            var allowGripperOverride =
                request.Kind == LiveCommandKind.MoveGripper
                && request.AllowReadbackOnlyGripperPathOverride
                && request.SessionMode == LiveCommandSessionMode.GripperOnly;

            if (request.Kind != LiveCommandKind.ReadbackOnly && usingReadbackOnlyClient && !allowTinyMoveJOverride && !allowGripperOverride)
            {
                result.Status = LiveCommandGateStatus.ReadbackOnly;
                result.BlockReasons.Add("live client is readback-only");
                result.ClearedReasons.Add("actual motion/IO/gripper commands remain locked on macOS live readback");
                return result;
            }

            if (request.Kind == LiveCommandKind.ReadbackOnly)
            {
                result.Status = request.ConnectionService.Client.IsConnected
                    ? LiveCommandGateStatus.ReadbackOnly
                    : LiveCommandGateStatus.Blocked;
                if (result.Status == LiveCommandGateStatus.Blocked)
                {
                    result.BlockReasons.Add("readback requires connection");
                }

                return result;
            }

            if (request.AllowDryRun || (request.ConnectionService.IsMockMode && !request.TreatMockAsLiveForDebug))
            {
                result.Status = LiveCommandGateStatus.Allowed;
                result.ClearedReasons.Add(request.AllowDryRun ? "dry-run simulation" : "mock client");
                return result;
            }

            if (!request.ConnectionService.Client.IsConnected)
            {
                result.Block("not connected");
            }

            if (!allowTinyMoveJOverride
                && request.Kind != LiveCommandKind.MoveGripper
                && !request.ConnectionService.Client.IsEnabled)
            {
                result.Block("servo disabled");
            }

            if (request.RequestedSpeedPercent > result.SpeedCapPercent)
            {
                result.Block($"speed {request.RequestedSpeedPercent}% exceeds cap {result.SpeedCapPercent}%");
            }

            var stateResult = request.ConnectionService.SyncCurrentState();
            if (!stateResult.IsSuccess)
            {
                result.Block($"state readback failed: {stateResult.Message}");
            }
            else
            {
                var state = stateResult.Value;
                result.ReadbackSummary = BuildStateSummary(state);
                if (state.IsEmergencyStop)
                {
                    result.Block("emergency stop active");
                }

                if (state.IsSafetyStop)
                {
                    result.Block("safety stop active");
                }

                if (state.IsCollisionDetected)
                {
                    result.Block("controller collision flag active");
                }

                if (!state.IsRobotEnabled)
                {
                    result.Block("servo disabled");
                }

                if (state.MainErrorCode != 0 || state.SubErrorCode != 0)
                {
                    result.Block($"fault active main={state.MainErrorCode} sub={state.SubErrorCode}");
                }

                if (state.MotionQueueLength > 0)
                {
                    result.Block($"motion queue not empty: {state.MotionQueueLength}");
                }
            }

            if (request.ToolId <= 0)
            {
                result.Block("toolId missing");
            }

            if (request.UserId <= 0)
            {
                result.Block("userId missing");
            }

            if (!request.HasResolvedCoordSystem || string.IsNullOrWhiteSpace(request.CoordSystem))
            {
                result.Block("coordSystem unresolved");
            }

            if (!request.HasFreshLatestState)
            {
                result.Block("latest-state freshness failed");
            }

            if (!request.HasFreshLatestDrift)
            {
                result.Block("latest-drift freshness failed");
            }

            if (!request.IsDriftWithinThreshold)
            {
                result.Block("drift threshold failed");
            }

            if (IsMotion(request.Kind))
            {
                if (!request.HasMatchingPreparedTarget)
                {
                    result.Block("prepared target mismatch");
                }

                if (request.Kind == LiveCommandKind.MoveJ && !request.IsWithinTinyMoveRange)
                {
                    result.Block("tiny MoveJ range exceeded");
                }

                if (!request.HasDryRunPreviewArtifact)
                {
                    result.Block("dry-run preview artifact missing");
                }

                if (!request.IsProductionIkSafe)
                {
                    result.Block("production IK guard not cleared");
                }

                if (!request.HasDedicatedTinyMoveJMotionPath
                    && (!request.IsBoundaryDataReady || !request.IsTargetWithinBoundary))
                {
                    result.Block("boundary data missing or target outside workspace");
                }

                if (!request.HasDedicatedTinyMoveJMotionPath
                    && (!request.IsCollisionDataReady || !request.IsPredictedPathCollisionFree))
                {
                    result.Block("collision data missing or predicted path unsafe");
                }
            }

            if (request.Kind == LiveCommandKind.MoveGripper && !request.HasGripperReadback)
            {
                result.Block("gripper readback missing");
            }

            if (!request.HasMatchingApprovalContext)
            {
                result.Block("operator approval target mismatch");
            }

            if (result.BlockReasons.Count > 0)
            {
                result.Status = LiveCommandGateStatus.Blocked;
                return result;
            }

            if (!request.OperatorConfirmed)
            {
                result.Status = LiveCommandGateStatus.RequiresConfirm;
                result.BlockReasons.Add("operator confirm token required");
                return result;
            }

            result.Status = LiveCommandGateStatus.Allowed;
            result.ClearedReasons.Add($"tool/user/coord resolved: tool={request.ToolId}; user={request.UserId}; coord={request.CoordSystem}");
            result.ClearedReasons.Add("latest-state freshness ok");
            result.ClearedReasons.Add("latest-drift freshness ok");
            result.ClearedReasons.Add("drift within threshold");
            if (request.HasDedicatedTinyMoveJMotionPath)
            {
                result.ClearedReasons.Add("tiny MoveJ dedicated live path enabled");
                result.ClearedReasons.Add("tiny MoveJ range guard within 2.0deg");
            }
            else if (request.Kind == LiveCommandKind.MoveGripper && request.AllowReadbackOnlyGripperPathOverride)
            {
                result.ClearedReasons.Add("gripper-only live path enabled");
            }

            result.ClearedReasons.Add("operator confirm token accepted");
            result.ClearedReasons.Add("live preflight readback clear");
            return result;
        }

        public string WriteAudit(LiveCommandSafetyGateResult result, string label)
        {
            var project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            var artifactDir = Path.Combine(project, "Artifacts");
            Directory.CreateDirectory(artifactDir);
            var safeLabel = string.IsNullOrWhiteSpace(label) ? "live-command-gate" : label.Replace(' ', '-');
            var path = Path.Combine(artifactDir, $"robotcontrolv3-{safeLabel}.txt");
            File.WriteAllText(path, result.ToSummary(), Encoding.UTF8);
            return path;
        }

        private static LiveCommandRiskLevel ResolveRisk(LiveCommandKind kind)
        {
            return kind switch
            {
                LiveCommandKind.ReadbackOnly => LiveCommandRiskLevel.Low,
                LiveCommandKind.RobotDo or LiveCommandKind.ToolDo => LiveCommandRiskLevel.Medium,
                LiveCommandKind.MoveGripper => LiveCommandRiskLevel.High,
                _ => LiveCommandRiskLevel.Critical,
            };
        }

        private static bool IsMotion(LiveCommandKind kind)
        {
            return kind is LiveCommandKind.MoveJ or LiveCommandKind.MoveL;
        }

        private static bool IsCommandAllowedForSession(LiveCommandKind kind, LiveCommandSessionMode sessionMode)
        {
            return sessionMode switch
            {
                LiveCommandSessionMode.LiveControl => true,
                LiveCommandSessionMode.ReadbackOnly => kind == LiveCommandKind.ReadbackOnly,
                LiveCommandSessionMode.GripperOnly => kind is LiveCommandKind.ReadbackOnly or LiveCommandKind.MoveGripper,
                LiveCommandSessionMode.TinyMoveJOnly => kind is LiveCommandKind.ReadbackOnly or LiveCommandKind.MoveJ,
                _ => kind == LiveCommandKind.ReadbackOnly,
            };
        }

        private static bool IsReadbackOnlyClient(IFairinoRobotClient client)
        {
            return client is IFairinoLiveClientDiagnostics { IsReadbackOnly: true };
        }

        private static string BuildStateSummary(FairinoRobotState state)
        {
            return $"mode={state.RobotMode}; enabled={state.IsRobotEnabled}; queue={state.MotionQueueLength}; safety={state.SafetyCode}; fault={state.MainErrorCode}/{state.SubErrorCode}; eStop={state.IsEmergencyStop}; collision={state.IsCollisionDetected}; tool={state.ToolId}; user={state.UserId}";
        }
    }

    public sealed class LiveCommandSafetyGateRequest
    {
        public LiveCommandKind Kind { get; set; }
        public FairinoConnectionService ConnectionService { get; set; }
        public bool AllowDryRun { get; set; }
        public bool OperatorConfirmed { get; set; }
        public bool HasMatchingPreparedTarget { get; set; } = true;
        public bool HasMatchingApprovalContext { get; set; } = true;
        public LiveCommandSessionMode SessionMode { get; set; } = LiveCommandSessionMode.LiveControl;
        public bool AllowReadbackOnlyMotionPathOverride { get; set; }
        public bool AllowReadbackOnlyGripperPathOverride { get; set; }
        public bool HasDedicatedTinyMoveJMotionPath { get; set; }
        public bool IsWithinTinyMoveRange { get; set; } = true;
        public int RequestedSpeedPercent { get; set; }
        public int SpeedCapPercent { get; set; } = LiveCommandSafetyGate.DefaultLiveSpeedCapPercent;
        public int ToolId { get; set; } = 1;
        public int UserId { get; set; } = 1;
        public string CoordSystem { get; set; } = "Base";
        public bool HasResolvedCoordSystem { get; set; } = true;
        public bool HasFreshLatestState { get; set; } = true;
        public bool HasFreshLatestDrift { get; set; } = true;
        public bool IsDriftWithinThreshold { get; set; } = true;
        public string LatestStateTimestampUtc { get; set; } = string.Empty;
        public string LatestDriftTimestampUtc { get; set; } = string.Empty;
        public bool HasDryRunPreviewArtifact { get; set; }
        public bool IsProductionIkSafe { get; set; }
        public bool IsBoundaryDataReady { get; set; }
        public bool IsTargetWithinBoundary { get; set; }
        public bool IsCollisionDataReady { get; set; }
        public bool IsPredictedPathCollisionFree { get; set; }
        public bool HasGripperReadback { get; set; }
        public bool TreatMockAsLiveForDebug { get; set; }
    }

    public sealed class LiveCommandSafetyGateResult
    {
        public LiveCommandKind Kind { get; set; }
        public LiveCommandGateStatus Status { get; set; } = LiveCommandGateStatus.Allowed;
        public LiveCommandRiskLevel RiskLevel { get; set; }
        public int RequestedSpeedPercent { get; set; }
        public int SpeedCapPercent { get; set; }
        public int ToolId { get; set; }
        public int UserId { get; set; }
        public string CoordSystem { get; set; } = string.Empty;
        public bool HasResolvedCoordSystem { get; set; }
        public bool HasFreshLatestState { get; set; }
        public bool HasFreshLatestDrift { get; set; }
        public bool IsDriftWithinThreshold { get; set; }
        public string LatestStateTimestampUtc { get; set; } = string.Empty;
        public string LatestDriftTimestampUtc { get; set; } = string.Empty;
        public string ReadbackSummary { get; set; } = string.Empty;
        public List<string> BlockReasons { get; } = new();
        public List<string> ClearedReasons { get; } = new();
        public bool CanExecuteLive => Status == LiveCommandGateStatus.Allowed;

        public void Block(string reason)
        {
            Status = LiveCommandGateStatus.Blocked;
            if (!string.IsNullOrWhiteSpace(reason))
            {
                BlockReasons.Add(reason);
            }
        }

        public string ToSummary()
        {
            return $"kind={Kind}; status={Status}; risk={RiskLevel}; speed={RequestedSpeedPercent}; cap={SpeedCapPercent}; tool={ToolId}; user={UserId}; coord={CoordSystem}; coordResolved={HasResolvedCoordSystem}; latestStateFresh={HasFreshLatestState}; latestDriftFresh={HasFreshLatestDrift}; driftWithinThreshold={IsDriftWithinThreshold}; latestStateUtc={LatestStateTimestampUtc}; latestDriftUtc={LatestDriftTimestampUtc}; blocks=[{string.Join(" | ", BlockReasons)}]; cleared=[{string.Join(" | ", ClearedReasons)}]; readback=[{ReadbackSummary}]";
        }
    }
}
