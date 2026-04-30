#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "${SCRIPT_DIR}/../.." && pwd)"
UNITYCTL_BIN="${UNITYCTL_BIN:-unityctl}"

PASS_COUNT=0
FAIL_COUNT=0
SKIP_COUNT=0
COMPILE_RETRY_SECONDS="${COMPILE_RETRY_SECONDS:-20}"
TEST_RETRY_SECONDS="${TEST_RETRY_SECONDS:-10}"

print_header() {
  printf '\n'
  printf '============================================================\n'
  printf '%s\n' "$1"
  printf '============================================================\n'
}

print_section() {
  printf '\n'
  printf -- '--- %s ---\n' "$1"
}

note() {
  printf '[INFO] %s\n' "$*"
}

pass() {
  PASS_COUNT=$((PASS_COUNT + 1))
  printf '[PASS] %s\n' "$*"
}

fail() {
  FAIL_COUNT=$((FAIL_COUNT + 1))
  printf '[FAIL] %s\n' "$*"
}

skip() {
  SKIP_COUNT=$((SKIP_COUNT + 1))
  printf '[SKIP] %s\n' "$*"
}

run_step() {
  local name="$1"
  shift

  printf '  -> %s\n' "$name"
  local output=""
  if output="$("$@" 2>&1)"; then
    pass "$name"
    printf '%s\n' "$output"
    return 0
  fi

  fail "$name"
  printf '%s\n' "$output"
  return 1
}

run_unityctl_test() {
  local label="$1"
  local filter="$2"
  local output=""
  local ok=0

  for ((attempt = 1; attempt <= TEST_RETRY_SECONDS; attempt++)); do
    if output="$("$UNITYCTL_BIN" test --project "$PROJECT_DIR" --mode edit --filter "$filter" --json 2>&1)"; then
      ok=1
      break
    fi

    if rg -q 'IPC is not ready yet' <<<"$output"; then
      note "$label waiting for IPC readiness (${attempt}/${TEST_RETRY_SECONDS})"
      sleep 1
      continue
    fi

    break
  done

  if [[ "$ok" -eq 1 ]]; then
    pass "$label"
    printf '%s\n' "$output"
    return 0
  fi

  fail "$label"
  printf '%s\n' "$output"
  return 1
}

run_if_test_file_exists() {
  local label="$1"
  local filter="$2"
  local file_path="$3"

  if [[ -f "$file_path" ]]; then
    run_unityctl_test "$label" "$filter" || true
  else
    skip "$label (missing: $(basename "$file_path"))"
  fi
}

run_directory_tests() {
  local label="$1"
  local directory="$2"

  if [[ ! -d "$directory" ]]; then
    skip "$label (missing directory: $(basename "$directory"))"
    return 0
  fi

  local found=0
  while IFS= read -r test_file; do
    [[ -n "$test_file" ]] || continue
    local class_name
    class_name="$(basename "$test_file" .cs)"
    found=1
    run_unityctl_test "$label :: $class_name" "KineTutor3D.Tests.EditMode.${class_name}" || true
  done < <(find "$directory" -maxdepth 1 -type f -name '*Tests.cs' | sort)

  if [[ "$found" -eq 0 ]]; then
    skip "$label (no *Tests.cs files found)"
  fi
}

main() {
  if ! command -v "$UNITYCTL_BIN" >/dev/null 2>&1; then
    printf '[ERROR] unityctl not found: %s\n' "$UNITYCTL_BIN" >&2
    exit 127
  fi

  print_header "EditMode Test Matrix"
  note "Project: $PROJECT_DIR"
  note "UnityCtl: $UNITYCTL_BIN"
  note "This runner does not start or require Play Mode; it is safe to use while Unity is not playing."

  print_section "Unity State"
  local status_json
  status_json="$(mktemp "${TMPDIR:-/tmp}/run_editmode_matrix.XXXXXX.json")"
  if "$UNITYCTL_BIN" status --project "$PROJECT_DIR" --json >"$status_json" 2>/dev/null; then
    note "unityctl status available"
    if rg -q '"isPlaying"[[:space:]]*:[[:space:]]*true' "$status_json"; then
      note "Unity is currently in Play Mode."
    else
      note "Unity is not in Play Mode or play state is not reported; continuing with EditMode only."
    fi
  else
    skip "unityctl status unavailable; continuing with EditMode execution"
  fi
  rm -f "$status_json"

  print_section "Compile Check"
  local compile_output=""
  local compile_ok=0
  for ((attempt = 1; attempt <= COMPILE_RETRY_SECONDS; attempt++)); do
    if compile_output="$("$UNITYCTL_BIN" check --project "$PROJECT_DIR" --type compile --json 2>&1)"; then
      compile_ok=1
      break
    fi

    if rg -q 'IPC is not ready yet' <<<"$compile_output"; then
      note "compile check waiting for IPC readiness (${attempt}/${COMPILE_RETRY_SECONDS})"
      sleep 1
      continue
    fi

    break
  done

  if [[ "$compile_ok" -eq 1 ]]; then
    pass "compile check"
    printf '%s\n' "$compile_output"
  else
    fail "compile check"
    printf '%s\n' "$compile_output"
  fi

  print_section "Core EditMode"
  run_directory_tests "Core suite" "$PROJECT_DIR/Assets/Tests/EditMode/Core"

  print_section "Integration EditMode"
  run_directory_tests "Integration suite" "$PROJECT_DIR/Assets/Tests/EditMode/Integration"

  print_section "Validation EditMode"
  run_directory_tests "Validation suite" "$PROJECT_DIR/Assets/Tests/EditMode/Validation"

  print_section "FR5-Focused EditMode"
  run_if_test_file_exists \
    "Fr5LiveReadbackTests" \
    "KineTutor3D.Tests.EditMode.Fr5LiveReadbackTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Integration/Fr5LiveReadbackTests.cs"
  run_if_test_file_exists \
    "FairinoConnectionServiceTests" \
    "KineTutor3D.Tests.EditMode.FairinoConnectionServiceTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Integration/FairinoConnectionServiceTests.cs"
  run_if_test_file_exists \
    "FairinoErrorTranslatorTests" \
    "KineTutor3D.Tests.EditMode.FairinoErrorTranslatorTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Integration/FairinoErrorTranslatorTests.cs"
  run_if_test_file_exists \
    "FairinoRobotConfigTests" \
    "KineTutor3D.Tests.EditMode.FairinoRobotConfigTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Integration/FairinoRobotConfigTests.cs"
  run_if_test_file_exists \
    "FairinoRobotControlUxTests" \
    "KineTutor3D.Tests.EditMode.FairinoRobotControlUxTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Integration/FairinoRobotControlUxTests.cs"
  run_if_test_file_exists \
    "FR5KinematicsFacadeTests" \
    "KineTutor3D.Tests.EditMode.FR5KinematicsFacadeTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Integration/FR5KinematicsFacadeTests.cs"
  run_if_test_file_exists \
    "FR5KinematicsFacadeIntegrationTests" \
    "KineTutor3D.Tests.EditMode.FR5KinematicsFacadeIntegrationTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Integration/FR5KinematicsFacadeIntegrationTests.cs"
  run_if_test_file_exists \
    "RobotControlEntryPolicyTests" \
    "KineTutor3D.Tests.EditMode.RobotControlEntryPolicyTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Core/RobotControlEntryPolicyTests.cs"
  run_if_test_file_exists \
    "RobotControlMotionRuntimeTests" \
    "KineTutor3D.Tests.EditMode.RobotControlMotionRuntimeTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Core/RobotControlMotionRuntimeTests.cs"
  run_if_test_file_exists \
    "RobotControlSceneCoordinatorTests" \
    "KineTutor3D.Tests.EditMode.RobotControlSceneCoordinatorTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Integration/RobotControlSceneCoordinatorTests.cs"
  run_if_test_file_exists \
    "RobotControlShellBinderTests" \
    "KineTutor3D.Tests.EditMode.RobotControlShellBinderTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Integration/RobotControlShellBinderTests.cs"
  run_if_test_file_exists \
    "RobotControlV3GizmoBehaviorTests" \
    "KineTutor3D.Tests.EditMode.RobotControlV3GizmoBehaviorTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Integration/RobotControlV3GizmoBehaviorTests.cs"
  run_if_test_file_exists \
    "RobotControlV3HardcodingGuardTests" \
    "KineTutor3D.Tests.EditMode.RobotControlV3HardcodingGuardTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Validation/RobotControlV3HardcodingGuardTests.cs"
  run_if_test_file_exists \
    "PendantV3ConnectionSessionAdapterTests" \
    "KineTutor3D.Tests.EditMode.PendantV3ConnectionSessionAdapterTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Core/PendantV3ConnectionSessionAdapterTests.cs"
  run_if_test_file_exists \
    "PendantV3VisualizationOrchestratorTests" \
    "KineTutor3D.Tests.EditMode.PendantV3VisualizationOrchestratorTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Core/PendantV3VisualizationOrchestratorTests.cs"
  run_if_test_file_exists \
    "FR5TemplatePoseCatalogTests" \
    "KineTutor3D.Tests.EditMode.FR5TemplatePoseCatalogTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Core/FR5TemplatePoseCatalogTests.cs"
  run_if_test_file_exists \
    "FR5TemplateSlimManifestTests" \
    "KineTutor3D.Tests.EditMode.FR5TemplateSlimManifestTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Core/FR5TemplateSlimManifestTests.cs"
  run_if_test_file_exists \
    "FR5PosePresetsTests" \
    "KineTutor3D.Tests.EditMode.FR5PosePresetsTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Core/FR5PosePresetsTests.cs"
  run_if_test_file_exists \
    "LiveFairinoClientSdkTests" \
    "KineTutor3D.Tests.EditMode.LiveFairinoClientSdkTests" \
    "$PROJECT_DIR/Assets/Tests/EditMode/Validation/LiveFairinoClientSdkTests.cs"

  print_header "EditMode Test Matrix Summary"
  printf 'Passed : %d\n' "$PASS_COUNT"
  printf 'Failed : %d\n' "$FAIL_COUNT"
  printf 'Skipped: %d\n' "$SKIP_COUNT"

  if [[ "$FAIL_COUNT" -gt 0 ]]; then
    exit 1
  fi
}

main "$@"
