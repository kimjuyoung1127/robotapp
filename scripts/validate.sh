#!/bin/bash
# ──────────────────────────────────────────────────────────────
# KineTutor3D 통합 검증 스크립트
# dotnet build + unity-cli 컴파일 체크 + 코딩 규칙 검증
# 사용법: ./scripts/validate.sh
# ──────────────────────────────────────────────────────────────
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
PASS=0
FAIL=0

check() {
    local name="$1"
    shift
    echo -n "[$name] "
    if "$@" > /dev/null 2>&1; then
        echo "PASS"
        PASS=$((PASS + 1))
    else
        echo "FAIL"
        FAIL=$((FAIL + 1))
    fi
}

echo "═══════════════════════════════════════════"
echo "  KineTutor3D Validation Pipeline"
echo "═══════════════════════════════════════════"
echo ""

# Step 1: dotnet build (if available)
if command -v dotnet &> /dev/null; then
    echo "── Step 1: dotnet build ──"
    check "dotnet build" dotnet build "$PROJECT_DIR"
else
    echo "── Step 1: dotnet build (SKIPPED - dotnet not found) ──"
fi

echo ""

# Step 2: unity-cli compile check (if Editor running)
if command -v unity-cli &> /dev/null; then
    echo "── Step 2: unity-cli compile check ──"
    check "compile-check" unity-cli compile-check
    check "console-errors" unity-cli console-check --type error
else
    echo "── Step 2: unity-cli checks (SKIPPED - unity-cli not found) ──"
fi

echo ""

# Step 3: Coding rules (pre-commit-check.sh)
echo "── Step 3: Coding rules ──"
if [ -x "$SCRIPT_DIR/pre-commit-check.sh" ]; then
    check "pre-commit-check" "$SCRIPT_DIR/pre-commit-check.sh"
else
    echo "  pre-commit-check.sh not found or not executable — SKIPPED"
fi

echo ""

# Step 4: unity-cli EditMode tests (if available)
if command -v unity-cli &> /dev/null; then
    echo "── Step 4: EditMode tests via unity-cli ──"
    check "run-tests edit" unity-cli run-tests --mode edit
else
    echo "── Step 4: EditMode tests (SKIPPED - unity-cli not found) ──"
fi

echo ""
echo "═══════════════════════════════════════════"
echo "  Results: $PASS passed, $FAIL failed"
echo "═══════════════════════════════════════════"

if [ "$FAIL" -gt 0 ]; then
    exit 1
fi
