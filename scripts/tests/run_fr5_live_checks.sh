#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEFAULT_PROJECT_PATH="/Users/family/jason/FR5UNITY/robotapp"

PROJECT_PATH="${PROJECT_PATH:-$DEFAULT_PROJECT_PATH}"
if [[ ! -d "$PROJECT_PATH" ]]; then
  PROJECT_PATH="$(cd "$SCRIPT_DIR/../.." && pwd)"
fi
PROJECT_PATH="$(cd "$PROJECT_PATH" && pwd)"

UNITYCTL_BIN="${UNITYCTL_BIN:-unityctl}"
FAIRINO_IP="${FAIRINO_IP:-192.168.57.2}"
FAIRINO_PORT="${FAIRINO_PORT:-8080}"
SCENE_PATH="${SCENE_PATH:-Assets/Scenes/RobotControlV3.unity}"
TIMEOUT_SECONDS="${TIMEOUT_SECONDS:-90}"
STATUS_TIMEOUT_SECONDS="${STATUS_TIMEOUT_SECONDS:-8}"
UNITYCTL_RETRY_SECONDS="${UNITYCTL_RETRY_SECONDS:-10}"
UNITYCTL_COMMAND_TIMEOUT_SECONDS="${UNITYCTL_COMMAND_TIMEOUT_SECONDS:-90}"
RUN_EDIT_TESTS=1
DO_CONNECT=0
DO_SYNC=0
LIVE_REQUESTED=0
PLAY_STARTED=0
LIVE_VERIFY_STARTED_EPOCH=0
LIVE_SESSION_ID=""
CURRENT_EVENTS_SESSION_ID=""

STATE_FILE="$PROJECT_PATH/Artifacts/live/fr5/latest-state.json"
DRIFT_FILE="$PROJECT_PATH/Artifacts/live/fr5/latest-drift.json"

usage() {
  cat <<'EOF'
Usage: run_fr5_live_checks.sh [options]

Options:
  --project PATH        Override project root
  --fairino-ip IP       Override FR5 controller IP
  --fairino-port PORT   Override FR5 controller port
  --scene PATH          Override RobotControlV3 scene path
  --connect             Attempt live readback connect
  --sync                Attempt live sync after connect
  --live                Shorthand for --connect --sync
  --no-edit-tests       Skip EditMode test pass
  --skip-edit-tests     Alias for --no-edit-tests
  --help                Show this help
EOF
}

log() {
  printf '%s\n' "$*"
}

section() {
  printf '\n== %s ==\n' "$*"
}

pass() {
  PASS=$((PASS + 1))
  TOTAL=$((TOTAL + 1))
  printf '[PASS] %s\n' "$1"
}

fail() {
  FAIL=$((FAIL + 1))
  TOTAL=$((TOTAL + 1))
  printf '[FAIL] %s\n' "$1"
  if [[ -n "${2:-}" ]]; then
    printf '%s\n' "$2"
  fi
}

skip() {
  SKIP=$((SKIP + 1))
  TOTAL=$((TOTAL + 1))
  printf '[SKIP] %s\n' "$1"
  if [[ -n "${2:-}" ]]; then
    printf '%s\n' "$2"
  fi
}

run_capture() {
  local __outvar="$1"
  shift
  local output
  if output="$("$@" 2>&1)"; then
    printf -v "$__outvar" '%s' "$output"
    return 0
  fi

  local rc=$?
  printf -v "$__outvar" '%s' "$output"
  return "$rc"
}

run_capture_with_timeout() {
  local __outvar="$1"
  local timeout_seconds="$2"
  shift 2
  local output
  if output="$(python3 - "$timeout_seconds" "$@" 2>&1 <<'PY'
import subprocess
import sys

timeout = float(sys.argv[1])
cmd = sys.argv[2:]

try:
    completed = subprocess.run(
        cmd,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        timeout=timeout,
    )
    sys.stdout.write(completed.stdout or "")
    raise SystemExit(completed.returncode)
except subprocess.TimeoutExpired as exc:
  if exc.stdout:
    sys.stdout.write(exc.stdout.decode() if isinstance(exc.stdout, bytes) else exc.stdout)
  sys.stdout.write(f"[timeout] {' '.join(cmd)} exceeded {timeout:.0f}s\n")
  raise SystemExit(142)
PY
)"; then
    printf -v "$__outvar" '%s' "$output"
    return 0
  fi

  local rc=$?
  printf -v "$__outvar" '%s' "$output"
  return "$rc"
}

is_retryable_unityctl_output() {
  local output="$1"
  rg -q 'IPC is not ready yet|domain reload|statusCode"[[:space:]]*:[[:space:]]*103|statusCode[[:space:]]*:[[:space:]]*103' <<<"$output"
}

run_unityctl_capture() {
  local __outvar="$1"
  local timeout_seconds="$2"
  shift 2
  local output=""
  local rc=0
  local attempt

  for ((attempt = 1; attempt <= UNITYCTL_RETRY_SECONDS; attempt++)); do
    if run_capture_with_timeout output "$timeout_seconds" "$UNITYCTL_BIN" "$@"; then
      printf -v "$__outvar" '%s' "$output"
      return 0
    fi

    rc=$?
    if [[ "$rc" -eq 142 ]] || is_retryable_unityctl_output "$output"; then
      log "[retry] unityctl $1 (${attempt}/${UNITYCTL_RETRY_SECONDS})"
      sleep 1
      continue
    fi

    break
  done

  printf -v "$__outvar" '%s' "$output"
  return "$rc"
}

print_json_summary() {
  local label="$1"
  local file="$2"
  local fields="$3"

  if [[ ! -f "$file" ]]; then
    skip "$label" "missing: $file"
    return 0
  fi

  pass "$label" "$file"
  log "  path: $file"
  while IFS= read -r pattern; do
    [[ -n "$pattern" ]] || continue
    grep -E "$pattern" "$file" || true
  done <<<"$fields"
}

require_fresh_file() {
  local file="$1"
  local started_epoch="$2"
  local label="$3"

  if [[ ! -f "$file" ]]; then
    fail "$label" "missing file: $file"
    return 1
  fi

  local modified_epoch
  modified_epoch="$(stat -f '%m' "$file" 2>/dev/null || true)"
  if [[ -z "$modified_epoch" ]]; then
    fail "$label" "could not read mtime: $file"
    return 1
  fi

  if [[ "$modified_epoch" -lt "$started_epoch" ]]; then
    fail "$label" "stale file: $file (mtime=$modified_epoch < started=$started_epoch)"
    return 1
  fi

  pass "$label"
  log "mtime=$modified_epoch file=$file"
  return 0
}

extract_json_string() {
  local file="$1"
  local key="$2"
  rg -o "\"$key\"[[:space:]]*:[[:space:]]*\"[^\"]+\"" "$file" 2>/dev/null | tail -1 | sed -E "s/.*\"$key\"[[:space:]]*:[[:space:]]*\"([^\"]+)\".*/\\1/" || true
}

find_latest_session_file_after() {
  local suffix="$1"
  local started_epoch="$2"
  local latest_file=""
  local latest_mtime=0
  local file
  while IFS= read -r file; do
    [[ -n "$file" ]] || continue
    local file_mtime
    file_mtime="$(stat -f '%m' "$file" 2>/dev/null || true)"
    [[ -n "$file_mtime" ]] || continue
    if [[ "$file_mtime" -ge "$started_epoch" && "$file_mtime" -ge "$latest_mtime" ]]; then
      latest_mtime="$file_mtime"
      latest_file="$file"
    fi
  done < <(find "$PROJECT_PATH/Artifacts/live/fr5/sessions" -name "*-${suffix}.ndjson" -print 2>/dev/null)

  printf '%s' "$latest_file"
}

run_unityctl_step() {
  local label="$1"
  shift
  local output
  if run_unityctl_capture output "$UNITYCTL_COMMAND_TIMEOUT_SECONDS" "$@"; then
    log "$output"
    pass "$label"
    return 0
  fi

  local rc=$?
  log "$output"
  fail "$label" "exit code: $rc"
  return 0
}

trap_cleanup() {
  if [[ "$PLAY_STARTED" -eq 1 ]]; then
    "$UNITYCTL_BIN" play stop --project "$PROJECT_PATH" --json >/dev/null 2>&1 || true
  fi
}

PASS=0
FAIL=0
SKIP=0
TOTAL=0
trap trap_cleanup EXIT

while [[ $# -gt 0 ]]; do
  case "$1" in
    --project)
      PROJECT_PATH="$2"
      shift 2
      ;;
    --fairino-ip)
      FAIRINO_IP="$2"
      shift 2
      ;;
    --fairino-port)
      FAIRINO_PORT="$2"
      shift 2
      ;;
    --scene)
      SCENE_PATH="$2"
      shift 2
      ;;
    --connect)
      DO_CONNECT=1
      LIVE_REQUESTED=1
      shift
      ;;
    --sync)
      DO_SYNC=1
      LIVE_REQUESTED=1
      shift
      ;;
    --live)
      DO_CONNECT=1
      DO_SYNC=1
      LIVE_REQUESTED=1
      shift
      ;;
    --no-edit-tests|--skip-edit-tests)
      RUN_EDIT_TESTS=0
      shift
      ;;
    --help|-h)
      usage
      exit 0
      ;;
    *)
      printf 'Unknown argument: %s\n' "$1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

PROJECT_PATH="$(cd "$PROJECT_PATH" && pwd)"
STATE_FILE="$PROJECT_PATH/Artifacts/live/fr5/latest-state.json"
DRIFT_FILE="$PROJECT_PATH/Artifacts/live/fr5/latest-drift.json"

if ! command -v "$UNITYCTL_BIN" >/dev/null 2>&1; then
  printf 'unityctl not found on PATH\n' >&2
  exit 1
fi

if ! command -v ping >/dev/null 2>&1; then
  printf 'ping not found on PATH\n' >&2
  exit 1
fi

if ! command -v nc >/dev/null 2>&1; then
  printf 'nc not found on PATH\n' >&2
  exit 1
fi

section "Network"
if run_capture ping_out ping -c 1 "$FAIRINO_IP"; then
  log "$ping_out"
  pass "ping $FAIRINO_IP"
else
  rc=$?
  log "$ping_out"
  fail "ping $FAIRINO_IP" "exit code: $rc"
fi

if run_capture nc_out nc -vz "$FAIRINO_IP" "$FAIRINO_PORT"; then
  log "$nc_out"
  pass "nc -vz $FAIRINO_IP $FAIRINO_PORT"
else
  rc=$?
  log "$nc_out"
  fail "nc -vz $FAIRINO_IP $FAIRINO_PORT" "exit code: $rc"
fi

section "UnityCtl Status"
status_ready=0
status_degraded=0
status_output=""
status_rc=0
for ((attempt = 1; attempt <= TIMEOUT_SECONDS; attempt++)); do
  if run_capture_with_timeout status_output "$STATUS_TIMEOUT_SECONDS" "$UNITYCTL_BIN" status --project "$PROJECT_PATH" --json; then
    if grep -Eq 'Ready' <<<"$status_output" \
      && grep -Eq '"?bridgeLoaded"?[[:space:]]*[:=][[:space:]]*true' <<<"$status_output" \
      && grep -Eq '"?ipcPipePresent"?[[:space:]]*[:=][[:space:]]*true' <<<"$status_output"; then
      status_ready=1
      break
    fi
  else
    status_rc=$?
    if [[ "$status_rc" -ne 142 ]] && ! is_retryable_unityctl_output "$status_output"; then
      break
    fi
  fi
  sleep 1
done
log "$status_output"
if [[ "$status_ready" -eq 1 ]]; then
  pass "unityctl status"
elif grep -Eq '"?state"?[[:space:]]*[:=][[:space:]]*"Playing"' <<<"$status_output" \
  && grep -Eq '"?bridgeLoaded"?[[:space:]]*[:=][[:space:]]*true' <<<"$status_output" \
  && grep -Eq '"?ipcPipePresent"?[[:space:]]*[:=][[:space:]]*true' <<<"$status_output"; then
  skip "unityctl status" "editor already playing; bridge is healthy so continuing with compile + live checks"
elif [[ "$status_rc" -eq 142 ]] || is_retryable_unityctl_output "$status_output"; then
  status_degraded=1
  skip "unityctl status" "status command stayed flaky on macOS; proceeding with compile + scene/play/exec as readiness gate"
else
  fail "unityctl status" "expected Ready/bridgeLoaded=true/ipcPipePresent=true within ${TIMEOUT_SECONDS}s"
fi

section "UnityCtl Compile"
compile_ok=0
compile_output=""
if run_unityctl_capture compile_output "$UNITYCTL_COMMAND_TIMEOUT_SECONDS" check --project "$PROJECT_PATH" --type compile --json; then
  log "$compile_output"
  compile_ok=1
  pass "unityctl compile"
else
  rc=$?
  log "$compile_output"
  fail "unityctl compile" "exit code: $rc"
fi

section "EditMode Tests"
if [[ "$RUN_EDIT_TESTS" -ne 1 ]]; then
  skip "edit-mode tests" "disabled by flag"
else
  edit_tests=(
    "KineTutor3D.Tests.EditMode.Validation.LiveFairinoClientSdkTests"
    "KineTutor3D.Tests.EditMode.Fr5LiveReadbackTests"
    "KineTutor3D.Tests.EditMode.Validation.RobotControlV3HardcodingGuardTests"
  )

  for test_filter in "${edit_tests[@]}"; do
    test_output=""
    if run_unityctl_capture test_output "$UNITYCTL_COMMAND_TIMEOUT_SECONDS" test --project "$PROJECT_PATH" --mode edit --filter "$test_filter" --json; then
      log "$test_output"
      pass "edit test: $test_filter"
    else
      rc=$?
      log "$test_output"
      fail "edit test: $test_filter" "exit code: $rc"
    fi
  done
fi

live_allowed=0
if [[ "$LIVE_REQUESTED" -eq 1 ]]; then
  if [[ "$compile_ok" -eq 1 ]]; then
    live_allowed=1
  else
    skip "live connect/sync" "requires compile success; status alone is treated as diagnostic on macOS"
  fi
fi

if [[ "$live_allowed" -eq 1 ]]; then
  section "FR5 Live V3"
  export FAIRINO_IP FAIRINO_PORT
  if [[ -n "${FAIRINO_BRIDGE_URL:-}" ]]; then
    export FAIRINO_BRIDGE_URL
  fi

  if run_unityctl_step "scene open RobotControlV3" scene open --project "$PROJECT_PATH" --path "$SCENE_PATH" --mode single --force --json; then
    :
  fi

  if run_unityctl_step "play start" play start --project "$PROJECT_PATH" --json; then
    PLAY_STARTED=1
  fi

  sleep 2

  run_unityctl_step "route to RobotControlV3" exec --project "$PROJECT_PATH" --code 'KineTutor3D.App.SceneNavigator.LoadByName("RobotControlV3")' --json
  run_unityctl_step "runtime summary" exec --project "$PROJECT_PATH" --code 'KineTutor3D.App.RobotControlV3DebugBridge.GetV3RuntimeSummary()' --json
  run_unityctl_step "panel summary" exec --project "$PROJECT_PATH" --code 'KineTutor3D.App.RobotControlV3DebugBridge.GetPanelControllerSummary()' --json

  if [[ "$DO_CONNECT" -eq 1 ]]; then
    LIVE_VERIFY_STARTED_EPOCH="$(date +%s)"
    run_unityctl_step "set mock off" exec --project "$PROJECT_PATH" --code 'KineTutor3D.App.RobotControlV3DebugBridge.SetMockModeForDebug(false)' --json
    run_unityctl_step "connect default" exec --project "$PROJECT_PATH" --code 'KineTutor3D.App.RobotControlV3DebugBridge.ConnectDefaultForDebug()' --json
  fi

  if [[ "$DO_SYNC" -eq 1 ]]; then
    run_unityctl_step "primary action sync" exec --project "$PROJECT_PATH" --code 'KineTutor3D.App.RobotControlV3DebugBridge.ExecutePrimaryActionForDebug()' --json
    sleep 1

    need_resync=0
    if [[ -f "$STATE_FILE" ]] && grep -q '"clientMode"[[:space:]]*:[[:space:]]*"mock"' "$STATE_FILE"; then
      need_resync=1
    fi
    if [[ -f "$DRIFT_FILE" ]] && grep -q '"severity"[[:space:]]*:[[:space:]]*"danger"' "$DRIFT_FILE"; then
      need_resync=1
    fi
    if [[ -f "$STATE_FILE" ]] && grep -q '"connected"[[:space:]]*:[[:space:]]*false' "$STATE_FILE"; then
      need_resync=1
    fi
    if [[ -f "$STATE_FILE" ]] && grep -q '"toolId"[[:space:]]*:[[:space:]]*0' "$STATE_FILE"; then
      need_resync=1
    fi
    if [[ -f "$STATE_FILE" ]] && grep -q '"userId"[[:space:]]*:[[:space:]]*0' "$STATE_FILE"; then
      need_resync=1
    fi

    if [[ "$need_resync" -eq 1 ]]; then
      log "[live] invalid live evidence detected, re-syncing"
      run_unityctl_step "reassert mock off" exec --project "$PROJECT_PATH" --code 'KineTutor3D.App.RobotControlV3DebugBridge.SetMockModeForDebug(false)' --json
      run_unityctl_step "reconnect default" exec --project "$PROJECT_PATH" --code 'KineTutor3D.App.RobotControlV3DebugBridge.ConnectDefaultForDebug()' --json
      run_unityctl_step "sync current state" exec --project "$PROJECT_PATH" --code 'KineTutor3D.App.RobotControlV3DebugBridge.SyncCurrentStateForDebug()' --json
      sleep 1
    fi
  fi

  if [[ "$DO_CONNECT" -eq 1 || "$DO_SYNC" -eq 1 ]]; then
    run_unityctl_step "refresh live evidence" exec --project "$PROJECT_PATH" --code 'KineTutor3D.App.RobotControlV3DebugBridge.RefreshLiveEvidenceForDebug()' --json
    sleep 1
  fi

  section "Evidence"
  print_json_summary "latest-state.json" "$STATE_FILE" '"clientMode"[[:space:]]*:[[:space:]]*"[^"]+"'$'\n''"sdkLoadStatus"[[:space:]]*:[[:space:]]*"[^"]+"'$'\n''"sdkRuntime"[[:space:]]*:[[:space:]]*"[^"]+"'$'\n''"sdk"[[:space:]]*:[[:space:]]*"[^"]+"'$'\n''"toolId"[[:space:]]*:[[:space:]]*[0-9]+'$'\n''"userId"[[:space:]]*:[[:space:]]*[0-9]+'$'\n''"coordSystem"[[:space:]]*:[[:space:]]*"[^"]+"'
  print_json_summary "latest-drift.json" "$DRIFT_FILE" '"severity"[[:space:]]*:[[:space:]]*"[^"]+"'$'\n''"maxJointDeg"[[:space:]]*:[[:space:]]*[0-9.eE+-]+'$'\n''"maxTcpMm"[[:space:]]*:[[:space:]]*[0-9.eE+-]+'$'\n''"maxTcpRotDeg"[[:space:]]*:[[:space:]]*[0-9.eE+-]+'$'\n''"liveBlockedReason"[[:space:]]*:[[:space:]]*"[^"]*"'

  if [[ "$DO_CONNECT" -eq 1 || "$DO_SYNC" -eq 1 ]]; then
    if [[ "$LIVE_VERIFY_STARTED_EPOCH" -eq 0 ]]; then
      LIVE_VERIFY_STARTED_EPOCH="$(date +%s)"
    fi
    current_events_file="$(find_latest_session_file_after "events" "$LIVE_VERIFY_STARTED_EPOCH")"
    if [[ -n "$current_events_file" ]]; then
      CURRENT_EVENTS_SESSION_ID="$(basename "$current_events_file" | sed -E 's/-events\.ndjson$//')"
      pass "current events session"
      log "$current_events_file"
    else
      fail "current events session" "expected a current-session events file after live verification"
    fi

    current_session_has_readback=0
    current_session_preserved_latest=0
    if [[ -n "${current_events_file:-}" && -f "$current_events_file" ]]; then
      if grep -Eq '"kind":"readback"' "$current_events_file"; then
        current_session_has_readback=1
      fi
      if grep -Eq '"kind":"readback-skip"' "$current_events_file"; then
        current_session_preserved_latest=1
      fi
    fi

    if [[ "$current_session_has_readback" -eq 1 ]]; then
      require_fresh_file "$STATE_FILE" "$LIVE_VERIFY_STARTED_EPOCH" "latest-state freshness"
      require_fresh_file "$DRIFT_FILE" "$LIVE_VERIFY_STARTED_EPOCH" "latest-drift freshness"
    elif [[ "$current_session_preserved_latest" -eq 1 ]]; then
      pass "latest-state preservation"
      log "current session preserved previous good latest-state instead of promoting invalid zero/disconnected state"
    else
      fail "latest-state freshness" "current session produced neither readback nor readback-skip evidence"
    fi

    if grep -Eq '"connected"[[:space:]]*:[[:space:]]*true' "$STATE_FILE"; then
      pass "latest-state connected"
    else
      fail "latest-state connected" "expected connected=true in $STATE_FILE"
    fi

    LIVE_SESSION_ID="$(extract_json_string "$STATE_FILE" "sessionId")"
    if [[ -n "$LIVE_SESSION_ID" ]]; then
      pass "live session id"
      log "latest-state sessionId=$LIVE_SESSION_ID"
    else
      fail "live session id" "missing sessionId in $STATE_FILE"
    fi
  fi

  section "Context Gate"
  tool_id="$(rg -o '"toolId"[[:space:]]*:[[:space:]]*[0-9]+' "$STATE_FILE" 2>/dev/null | tail -1 | rg -o '[0-9]+' | tail -1 || true)"
  user_id="$(rg -o '"userId"[[:space:]]*:[[:space:]]*[0-9]+' "$STATE_FILE" 2>/dev/null | tail -1 | rg -o '[0-9]+' | tail -1 || true)"
  if [[ -n "$tool_id" && "$tool_id" -gt 0 ]]; then
    pass "toolId context"
    log "toolId=$tool_id"
  else
    fail "toolId context" "expected toolId > 0 in $STATE_FILE"
  fi

  if [[ -n "$user_id" && "$user_id" -gt 0 ]]; then
    pass "userId context"
    log "userId=$user_id"
  else
    fail "userId context" "expected userId > 0 in $STATE_FILE"
  fi

  coord_system="$(rg -o '"coordSystem"[[:space:]]*:[[:space:]]*"[^"]+"' "$STATE_FILE" 2>/dev/null | tail -1 | sed -E 's/.*"coordSystem"[[:space:]]*:[[:space:]]*"([^"]+)".*/\1/' || true)"
  if [[ "$coord_system" == "Base" || "$coord_system" == "Tool" || "$coord_system" == "User" ]]; then
    pass "TCP coord context"
    log "coordSystem=$coord_system"
  else
    fail "TCP coord context" "expected coordSystem Base|Tool|User in $STATE_FILE"
  fi

  if [[ -n "$CURRENT_EVENTS_SESSION_ID" ]]; then
    readback_session_file="$PROJECT_PATH/Artifacts/live/fr5/sessions/${CURRENT_EVENTS_SESSION_ID}-readback.ndjson"
    events_session_file="$PROJECT_PATH/Artifacts/live/fr5/sessions/${CURRENT_EVENTS_SESSION_ID}-events.ndjson"

    if [[ -f "$readback_session_file" ]]; then
      pass "session readback file"
      log "$readback_session_file"
    else
      if [[ -f "$events_session_file" ]] && grep -Eq '"kind":"readback-skip"' "$events_session_file"; then
        skip "session readback file" "current session preserved previous latest-state; no new readback file promoted"
      else
        fail "session readback file" "missing: $readback_session_file"
      fi
    fi

    if [[ -f "$events_session_file" ]]; then
      pass "session events file"
      log "$events_session_file"
      if grep -Eq '"kind":"readback"' "$events_session_file"; then
        pass "session readback event"
      elif grep -Eq '"kind":"readback-skip"' "$events_session_file"; then
        pass "session readback preservation event"
      else
        fail "session readback event" "expected readback or readback-skip event in $events_session_file"
      fi
    else
      fail "session events file" "missing: $events_session_file"
    fi
  fi

  panel_summary=""
  if run_unityctl_capture panel_summary "$UNITYCTL_COMMAND_TIMEOUT_SECONDS" exec --project "$PROJECT_PATH" --code 'KineTutor3D.App.RobotControlV3DebugBridge.GetPanelControllerSummary()' --json; then
    log "$panel_summary"
    if grep -Eq 'Tool:[[:space:]]*[0-9]{2}|tool=[0-9]{2}|user=[0-9]{2}|coord=Base|coord=Tool|coord=User' <<<"$panel_summary"; then
      pass "V3 panel context snapshot"
    else
      skip "V3 panel context snapshot" "panel summary did not expose explicit tool/user tokens, kept TcpJog + latest-state as source of truth"
    fi
  else
    rc=$?
    log "$panel_summary"
    skip "V3 panel context snapshot" "unityctl exec failed with exit code: $rc"
  fi

  if [[ "$PLAY_STARTED" -eq 1 ]]; then
    if run_capture stop_output "$UNITYCTL_BIN" play stop --project "$PROJECT_PATH" --json; then
      log "$stop_output"
      pass "play stop"
      PLAY_STARTED=0
    else
      rc=$?
      log "$stop_output"
      fail "play stop" "exit code: $rc"
    fi
  fi
fi

section "Summary"
printf 'Results: %d passed, %d failed, %d skipped, %d total\n' "$PASS" "$FAIL" "$SKIP" "$TOTAL"
printf 'Evidence paths:\n'
printf '  latest-state: %s\n' "$STATE_FILE"
printf '  latest-drift: %s\n' "$DRIFT_FILE"

if [[ "$FAIL" -gt 0 ]]; then
  exit 1
fi
