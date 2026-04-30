#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$PROJECT_ROOT"

PATTERN='ReadbackOnly|ready=False|coordSystem=Base|tool=1|user=1|dry-run simulation'

TARGETS=(
  "Assets/UI/PendantV3"
  "Assets/Scripts/UI/RobotControlV3"
  "Assets/Scripts/App/Fairino/RobotControlV3RuntimeSnapshot.cs"
  "Assets/Scripts/UI/RobotControlV3/PendantV3PreviewState.cs"
)

if rg -n -S "$PATTERN" "${TARGETS[@]}"; then
  echo "FAIL: 운영자 UI 경로에 금지 상태문구 토큰이 남아 있습니다." >&2
  exit 1
fi

echo "PASS: 운영자 UI 경로에서 금지 상태문구 토큰을 찾지 못했습니다."
