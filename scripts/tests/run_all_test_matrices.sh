#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "${SCRIPT_DIR}/../.." && pwd)"
RUN_EDIT=1
RUN_PLAY=1
RUN_FR5=1
RUN_FR5_LIVE=0
STOP_ON_FAIL=0

usage() {
  cat <<'EOF'
Usage: scripts/tests/run_all_test_matrices.sh [options]

Options:
  --project PATH        Override project root
  --edit-only           Run only the EditMode matrix
  --play-only           Run only the PlayMode matrix
  --fr5-only            Run only the FR5 check matrix
  --skip-edit           Skip the EditMode matrix
  --skip-play           Skip the PlayMode matrix
  --skip-fr5            Skip the FR5 check matrix
  --live                Let the FR5 matrix attempt connect + sync
  --stop-on-fail        Stop as soon as one matrix fails
  -h, --help            Show this help
EOF
}

log() {
  printf '%s\n' "$*"
}

section() {
  printf '\n============================================================\n'
  printf '%s\n' "$1"
  printf '============================================================\n'
}

parse_args() {
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --project)
        PROJECT_DIR="$2"
        shift 2
        ;;
      --edit-only)
        RUN_EDIT=1
        RUN_PLAY=0
        RUN_FR5=0
        shift
        ;;
      --play-only)
        RUN_EDIT=0
        RUN_PLAY=1
        RUN_FR5=0
        shift
        ;;
      --fr5-only)
        RUN_EDIT=0
        RUN_PLAY=0
        RUN_FR5=1
        shift
        ;;
      --skip-edit)
        RUN_EDIT=0
        shift
        ;;
      --skip-play)
        RUN_PLAY=0
        shift
        ;;
      --skip-fr5)
        RUN_FR5=0
        shift
        ;;
      --live)
        RUN_FR5_LIVE=1
        shift
        ;;
      --stop-on-fail)
        STOP_ON_FAIL=1
        shift
        ;;
      -h|--help)
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
}

run_matrix() {
  local label="$1"
  shift

  section "$label"
  if "$@"; then
    MATRIX_PASS=$((MATRIX_PASS + 1))
    log "[PASS] $label"
    return 0
  fi

  MATRIX_FAIL=$((MATRIX_FAIL + 1))
  log "[FAIL] $label"
  if [[ "$STOP_ON_FAIL" -eq 1 ]]; then
    exit 1
  fi
  return 0
}

main() {
  parse_args "$@"
  PROJECT_DIR="$(cd "$PROJECT_DIR" && pwd)"

  local edit_script="$PROJECT_DIR/scripts/tests/run_editmode_matrix.sh"
  local play_script="$PROJECT_DIR/scripts/tests/run_playmode_matrix.sh"
  local fr5_script="$PROJECT_DIR/scripts/tests/run_fr5_live_checks.sh"

  MATRIX_PASS=0
  MATRIX_FAIL=0

  log "Project: $PROJECT_DIR"

  if [[ "$RUN_EDIT" -eq 1 ]]; then
    run_matrix "EditMode Matrix" "$edit_script"
  fi

  if [[ "$RUN_PLAY" -eq 1 ]]; then
    run_matrix "PlayMode Matrix" "$play_script"
  fi

  if [[ "$RUN_FR5" -eq 1 ]]; then
    if [[ "$RUN_FR5_LIVE" -eq 1 ]]; then
      run_matrix "FR5 Live Matrix" "$fr5_script" --live
    else
      run_matrix "FR5 Live Matrix" "$fr5_script"
    fi
  fi

  section "Matrix Summary"
  printf 'Passed matrices: %d\n' "$MATRIX_PASS"
  printf 'Failed matrices: %d\n' "$MATRIX_FAIL"

  if [[ "$MATRIX_FAIL" -gt 0 ]]; then
    exit 1
  fi
}

main "$@"
