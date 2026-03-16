#!/bin/bash
# ──────────────────────────────────────────────────────────────
# unity-cli 커스텀 도구 통합 테스트
# 전제: Unity Editor가 열려있고 unity-cli connector가 활성화된 상태
# 사용법: ./scripts/cli-integration-test.sh
# ──────────────────────────────────────────────────────────────
set -euo pipefail

PASS=0
FAIL=0
TOTAL=0

run_test() {
    local name="$1"
    shift
    TOTAL=$((TOTAL + 1))
    echo -n "  [$TOTAL] $name ... "
    if output=$("$@" 2>&1); then
        echo "PASS"
        PASS=$((PASS + 1))
    else
        echo "FAIL"
        echo "    Output: $output"
        FAIL=$((FAIL + 1))
    fi
}

echo "═══════════════════════════════════════════"
echo "  unity-cli Custom Tool Integration Tests"
echo "═══════════════════════════════════════════"
echo ""

# Tier 1: CI/QA Essentials
echo "── Tier 1: CI/QA Essentials ──"
run_test "compile-check"        unity-cli compile-check
run_test "console-check error"  unity-cli console-check --type error
run_test "console-check all"    unity-cli console-check --type all
run_test "scene-validate Boot"  unity-cli scene-validate --name Boot
run_test "scene-validate all"   unity-cli scene-validate --name all

echo ""

# Tier 2: MCP Replacement
echo "── Tier 2: MCP Replacement ──"
run_test "scene-hierarchy"      unity-cli scene-hierarchy --depth 2
run_test "prefab-validate FR5"  unity-cli prefab-validate --path "Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab"

echo ""

# Tier 3: KineTutor3D Specific
echo "── Tier 3: KineTutor3D Specific ──"
run_test "robot-catalog"        unity-cli robot-catalog
run_test "fk-compute 2DOF"      unity-cli fk-compute --template 2DOF_RR --joints "45,30"
run_test "fk-compute FR5"       unity-cli fk-compute --template FR5 --joints "0,-45,0,-59,-92,-42"
run_test "qa-prep first-time"   unity-cli qa-prep --scenario first-time
run_test "qa-prep returning"    unity-cli qa-prep --scenario returning
run_test "dh-table FR5"         unity-cli dh-table --template FR5
run_test "joint-limit 2DOF"     unity-cli joint-limit --template 2DOF_RR
run_test "build-settings"       unity-cli build-settings
run_test "canvas-validate"      unity-cli canvas-validate
run_test "asmdef-validate"      unity-cli asmdef-validate
run_test "playerprefs-inspect" unity-cli playerprefs-inspect
run_test "resource-validate"   unity-cli resource-validate
run_test "session-context"     unity-cli session-context
run_test "tutorstep-validate"  unity-cli tutorstep-validate
run_test "glossary-validate"   unity-cli glossary-validate
run_test "fr5-diagnostic"      unity-cli fr5-diagnostic
run_test "asset-size"          unity-cli asset-size --top 5
run_test "scene-diff"          unity-cli scene-diff --scene_a Boot --scene_b Home
run_test "pose-compare"        unity-cli pose-compare --template 2DOF_RR --joints_a "0,0" --joints_b "45,30"
run_test "learning-tabs"       unity-cli learning-tabs --robot_id 2DOF_RR
run_test "camera-capture"      unity-cli camera-capture --action current

echo ""
echo "═══════════════════════════════════════════"
echo "  Results: $PASS passed, $FAIL failed, $TOTAL total"
echo "═══════════════════════════════════════════"

if [ "$FAIL" -gt 0 ]; then
    exit 1
fi
