#!/usr/bin/env bash
set -euo pipefail

PROJECT_PATH="${PROJECT_PATH:-/Users/family/jason/FR5UNITY/robotapp}"
UNITYCTL_BIN="${UNITYCTL_BIN:-unityctl}"

run_exec() {
  "$UNITYCTL_BIN" exec --project "$PROJECT_PATH" --code "$1" --json
}

echo "== Restart / Live Sync =="
"/Users/family/.codex/skills/unity-fr5-restart-live/scripts/restart_v3_live_loop.sh" --connect --sync

echo "== Post-Restart Compile =="
"$UNITYCTL_BIN" check --project "$PROJECT_PATH" --type compile --json

echo "== Normal 33ms Probe =="
run_exec 'KineTutor3D.App.RobotControlV3DebugBridge.SetLivePollIntervalForDebug(0.033)'
run_exec 'KineTutor3D.App.RobotControlV3DebugBridge.ResetLiveReadbackProbeForDebug()'
sleep 2
run_exec 'KineTutor3D.App.RobotControlV3DebugBridge.GetLiveReadbackProbeSummaryForDebug()'

echo "== Forced Error Fallback =="
run_exec 'KineTutor3D.App.RobotControlV3DebugBridge.SetLivePollIntervalForDebug(0.033)'
run_exec 'KineTutor3D.App.RobotControlV3DebugBridge.ForceNextReadFailuresForDebug(2)'
sleep 1
run_exec 'KineTutor3D.App.RobotControlV3DebugBridge.GetLiveReadbackProbeSummaryForDebug()'
sleep 1
run_exec 'KineTutor3D.App.RobotControlV3DebugBridge.GetLiveReadbackProbeSummaryForDebug()'

echo "== Gate / Evidence =="
run_exec 'KineTutor3D.App.RobotControlV3DebugBridge.GetTinyMoveJGateSummaryForDebug()'
run_exec 'KineTutor3D.App.RobotControlV3DebugBridge.RefreshLiveEvidenceForDebug()'
cat "$PROJECT_PATH/Artifacts/live/fr5/latest-state.json"
cat "$PROJECT_PATH/Artifacts/live/fr5/latest-drift.json"
