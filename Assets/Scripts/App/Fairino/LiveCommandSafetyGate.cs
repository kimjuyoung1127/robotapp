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
            };

            if (request.ConnectionService == null || request.ConnectionService.Client == null)
            {
                result.Block("connection service missing");
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

            if (!request.ConnectionService.Client.IsEnabled)
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

                if (state.MainErrorCode != 0 || state.SubErrorCode != 0)
                {
                    result.Block($"fault active main={state.MainErrorCode} sub={state.SubErrorCode}");
                }

                if (state.MotionQueueLength > 0)
                {
                    result.Block($"motion queue not empty: {state.MotionQueueLength}");
                }
            }

            if (IsMotion(request.Kind))
            {
                if (!request.HasDryRunPreviewArtifact)
                {
                    result.Block("dry-run preview artifact missing");
                }

                if (!request.IsProductionIkSafe)
                {
                    result.Block("production IK guard not cleared");
                }

                if (!request.IsBoundaryDataReady || !request.IsTargetWithinBoundary)
                {
                    result.Block("boundary data missing or target outside workspace");
                }

                if (!request.IsCollisionDataReady || !request.IsPredictedPathCollisionFree)
                {
                    result.Block("collision data missing or predicted path unsafe");
                }
            }

            if (request.Kind == LiveCommandKind.MoveGripper && !request.HasGripperReadback)
            {
                result.Block("gripper readback missing");
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
        public int RequestedSpeedPercent { get; set; }
        public int SpeedCapPercent { get; set; } = LiveCommandSafetyGate.DefaultLiveSpeedCapPercent;
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
            return $"kind={Kind}; status={Status}; risk={RiskLevel}; speed={RequestedSpeedPercent}; cap={SpeedCapPercent}; blocks=[{string.Join(" | ", BlockReasons)}]; cleared=[{string.Join(" | ", ClearedReasons)}]; readback=[{ReadbackSummary}]";
        }
    }
}
