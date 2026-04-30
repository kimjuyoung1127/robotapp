#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
UNITYCTL_BIN="${UNITYCTL:-${UNITYCTL_EXE:-}}"
GROUP_FILTERS=()
SUITE_FILTERS=()
LIST_ONLY=0
SHOW_HELP=0
STRICT_STATUS=1
STOP_ON_FAIL=0
TEST_RETRY_SECONDS="${TEST_RETRY_SECONDS:-10}"
STATUS_TIMEOUT_SECONDS="${STATUS_TIMEOUT_SECONDS:-8}"
TEST_TIMEOUT_SECONDS="${TEST_TIMEOUT_SECONDS:-90}"

source "$SCRIPT_DIR/playmode_suites.sh"

usage() {
    cat <<'EOF'
Usage:
  scripts/tests/run_playmode_matrix.sh [options]

Options:
  --project PATH        Unity project root (default: repo root)
  --unityctl PATH       unityctl executable path
  --group NAME          Run one group: smoke, flows, visuals, or all
                        Can be repeated. Default: all groups
  --filter GLOB         Filter suite class names by shell glob
                        Can be repeated. Example: --filter '*SmokeTests'
  --list                Print the selected matrix and exit
  --no-status           Skip the unityctl status precheck
  --stop-on-fail        Stop after the first failing suite
  -h, --help            Show this help

Notes:
  - This runner executes grouped PlayMode suites via unityctl test --mode play.
  - The --filter option applies to suite class names, not to individual UnityTest names.
EOF
}

die() {
    printf '[ERROR] %s\n' "$*" >&2
    exit 1
}

info() {
    printf '[INFO] %s\n' "$*"
}

warn() {
    printf '[WARN] %s\n' "$*" >&2
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

is_failed_test_result() {
    local output="$1"
    rg -q '"success"[[:space:]]*:[[:space:]]*false|"failed"[[:space:]]*:[[:space:]]*[1-9]|statusCode"[[:space:]]*:[[:space:]]*504|statusCode[[:space:]]*:[[:space:]]*504' <<<"$output"
}

resolve_unityctl() {
    if [[ -n "$UNITYCTL_BIN" ]]; then
        printf '%s\n' "$UNITYCTL_BIN"
        return 0
    fi

    if command -v unityctl >/dev/null 2>&1; then
        command -v unityctl
        return 0
    fi

    return 1
}

append_group() {
    local group
    group="$(to_lower "$1")"
    case "$group" in
        all)
            while IFS= read -r item; do
                [[ -n "$item" ]] && GROUP_FILTERS+=("$item")
            done < <(playmode_all_groups)
            ;;
        smoke|flows|visuals)
            GROUP_FILTERS+=("$group")
            ;;
        *)
            die "Unknown group: $group"
            ;;
    esac
}

parse_args() {
    while [[ $# -gt 0 ]]; do
        case "$1" in
            --project)
                [[ $# -ge 2 ]] || die "--project requires a path"
                PROJECT_DIR="$2"
                shift 2
                ;;
            --unityctl)
                [[ $# -ge 2 ]] || die "--unityctl requires a path"
                UNITYCTL_BIN="$2"
                shift 2
                ;;
            --group)
                [[ $# -ge 2 ]] || die "--group requires a name"
                append_group "$2"
                shift 2
                ;;
            --filter)
                [[ $# -ge 2 ]] || die "--filter requires a glob"
                SUITE_FILTERS+=("$2")
                shift 2
                ;;
            --list)
                LIST_ONLY=1
                shift
                ;;
            --no-status)
                STRICT_STATUS=0
                shift
                ;;
            --stop-on-fail)
                STOP_ON_FAIL=1
                shift
                ;;
            -h|--help)
                SHOW_HELP=1
                shift
                ;;
            *)
                die "Unknown argument: $1"
                ;;
        esac
    done
}

suite_matches_filters() {
    local suite="$1"

    if [[ ${#SUITE_FILTERS[@]} -eq 0 ]]; then
        return 0
    fi

    local pattern
    for pattern in "${SUITE_FILTERS[@]}"; do
        if [[ "$suite" == $pattern ]]; then
            return 0
        fi
    done

    return 1
}

build_suite_list() {
    local group suite
    local selected=()

    for group in "${GROUP_FILTERS[@]}"; do
        while IFS= read -r suite; do
            [[ -n "$suite" ]] || continue
            if suite_matches_filters "$suite"; then
                selected+=("$suite")
            fi
        done < <(playmode_group_suites "$group")
    done

    printf '%s\n' "${selected[@]}"
}

run_status_precheck() {
    local status_output

    info "Precheck: unityctl status"
    if ! run_capture_with_timeout status_output "$STATUS_TIMEOUT_SECONDS" "$UNITYCTL_BIN" status --project "$PROJECT_DIR" --json; then
        printf '%s\n' "$status_output" >&2
        warn "unityctl status precheck failed; continuing to PlayMode tests"
        return 0
    fi

    printf '%s\n' "$status_output" | sed 's/^/  /'
}

run_suite() {
    local suite="$1"
    local group_label="$2"
    local cmd_output status=0
    local attempt

    printf '\n=== [%s] %s ===\n' "$group_label" "$suite"
    for ((attempt = 1; attempt <= TEST_RETRY_SECONDS; attempt++)); do
        if run_capture_with_timeout cmd_output "$TEST_TIMEOUT_SECONDS" "$UNITYCTL_BIN" test --project "$PROJECT_DIR" --mode play --filter "$suite" --json; then
            if is_failed_test_result "$cmd_output"; then
                status=1
            else
                status=0
            fi
            break
        fi

        status=$?
        if [[ $status -eq 142 ]] || is_retryable_unityctl_output "$cmd_output"; then
            info "$suite waiting for IPC/domain reload/timeout (${attempt}/${TEST_RETRY_SECONDS})"
            sleep 1
            continue
        fi

        break
    done

    printf '%s\n' "$cmd_output" | sed 's/^/  /'

    if [[ $status -eq 0 ]]; then
        printf '[PASS] %s\n' "$suite"
    else
        printf '[FAIL] %s\n' "$suite" >&2
    fi

    return "$status"
}

main() {
    parse_args "$@"

    if [[ $SHOW_HELP -eq 1 ]]; then
        usage
        exit 0
    fi

    if [[ -z "${GROUP_FILTERS[*]:-}" ]]; then
        while IFS= read -r group; do
            [[ -n "$group" ]] && GROUP_FILTERS+=("$group")
        done < <(playmode_all_groups)
    fi

    [[ -d "$PROJECT_DIR/Assets" ]] || die "Project root is missing Assets: $PROJECT_DIR"
    [[ -d "$PROJECT_DIR/ProjectSettings" ]] || die "Project root is missing ProjectSettings: $PROJECT_DIR"

    UNITYCTL_BIN="$(resolve_unityctl)" || die "unityctl not found. Set UNITYCTL or put unityctl on PATH."

    if [[ $LIST_ONLY -eq 1 ]]; then
        printf 'PlayMode matrix for %s\n' "$PROJECT_DIR"
        while IFS= read -r group; do
            [[ -n "$group" ]] || continue
            printf '\n[%s]\n' "$(playmode_group_label "$group")"
            while IFS= read -r suite; do
                [[ -n "$suite" ]] && printf '%s\n' "$suite"
            done < <(playmode_group_suites "$group" | while IFS= read -r suite; do
                if suite_matches_filters "$suite"; then
                    printf '%s\n' "$suite"
                fi
            done)
        done < <(printf '%s\n' "${GROUP_FILTERS[@]}")
        exit 0
    fi

    if [[ $STRICT_STATUS -eq 1 ]]; then
        run_status_precheck
    fi

    suites_file="$(mktemp)"
    trap 'rm -f "$suites_file"' EXIT
    build_suite_list > "$suites_file"

    if [[ ! -s "$suites_file" ]]; then
        warn "No PlayMode suites matched the selected groups and filters."
        exit 0
    fi

    local total=0 passed=0 failed=0
    local suite group label status
    while IFS= read -r suite; do
        [[ -n "$suite" ]] || continue
        total=$((total + 1))

        group="unknown"
        case "$suite" in
            KineTutor3D.Tests.PlayMode.*)
                case "$suite" in
                    KineTutor3D.Tests.PlayMode.AllButtonsSmokeTests|KineTutor3D.Tests.PlayMode.MathReadinessFlowSmokeTests)
                        group="Smoke"
                        ;;
                    KineTutor3D.Tests.PlayMode.FullSceneTransitionTests|KineTutor3D.Tests.PlayMode.RobotLibrarySandboxRoutingTests|KineTutor3D.Tests.PlayMode.SceneFlowSmokeTests|KineTutor3D.Tests.PlayMode.UxFlowSmokeTests)
                        group="Flows"
                        ;;
                    KineTutor3D.Tests.PlayMode.Phase5CommonVisualsSmokeTests|KineTutor3D.Tests.PlayMode.UIPanelDesignSystemSmokeTests|KineTutor3D.Tests.PlayMode.VisualizationSmokeTests)
                        group="Visuals"
                        ;;
                esac
                ;;
        esac

        if run_suite "$suite" "$group"; then
            passed=$((passed + 1))
        else
            failed=$((failed + 1))
            if [[ $STOP_ON_FAIL -eq 1 ]]; then
                break
            fi
        fi
    done < "$suites_file"

    printf '\n=== Summary ===\n'
    printf 'Project: %s\n' "$PROJECT_DIR"
    printf 'UnityCtl: %s\n' "$UNITYCTL_BIN"
    printf 'Suites:  %d total, %d passed, %d failed\n' "$total" "$passed" "$failed"

    if [[ $failed -gt 0 ]]; then
        exit 1
    fi
}

main "$@"
