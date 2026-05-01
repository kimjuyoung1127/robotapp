// Folder: StatusSafety - operator-facing safety summaries and blocked-reason helpers for V3 diagnostics surfaces.
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
    // Handles gate summary/debug surfaces and safety-facing runtime wording used by V3 status panels.
    // Hard live-command gating stays in LiveSafety and panel-specific execution remains outside this partial.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string GetLiveCommandSafetyGateSummaryForDebug(string commandKind)
        {
            var kind = ParseLiveCommandKind(commandKind);
            var result = EvaluateLiveCommandSafetyPreview(
                kind,
                ResolveRequestedSpeedPercent(),
                productionIkSafe: true,
                boundaryReady: false,
                collisionReady: false,
                hasGripperReadback: kind == LiveCommandKind.MoveGripper);
            return result.ToSummary();
        }


        public string GetTinyMoveJGateSummaryForDebug()
        {
            var dedicatedTinyMoveJPath = HasDedicatedTinyMoveJLivePathConfigured();
            var withinTinyRange = TryEvaluateTinyMoveJRange(
                previewUsesJointPose ? previewJointAnglesDeg : null,
                out _,
                out _);
            var gate = EvaluateLiveCommandSafetyPreview(
                LiveCommandKind.MoveJ,
                ResolveRequestedSpeedPercent(),
                productionIkSafe: true,
                boundaryReady: false,
                collisionReady: false,
                hasGripperReadback: false,
                allowReadbackOnlyMotionPathOverride: dedicatedTinyMoveJPath,
                hasDedicatedTinyMoveJMotionPath: dedicatedTinyMoveJPath,
                isWithinTinyMoveRange: withinTinyRange);
            return $"status={gate.Status}; ready={gate.CanExecuteLive}; risk={gate.RiskLevel}; blocks={string.Join(" | ", gate.BlockReasons)}; cleared={string.Join(" | ", gate.ClearedReasons)}; readback={gate.ReadbackSummary}";
        }


        private static string FormatRobotStateForDebug(FairinoRobotState state)
        {
            return $"joints=[{string.Join(",", FormatValues(state.JointPosDeg, "0.0"))}]; tcp=[{string.Join(",", FormatValues(state.TcpPose, "0.0"))}]; enabled={state.IsRobotEnabled}; mode={state.RobotMode}; fault={state.MainErrorCode}/{state.SubErrorCode}";
        }

    }
}
