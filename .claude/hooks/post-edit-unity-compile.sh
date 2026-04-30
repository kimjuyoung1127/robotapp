#!/bin/bash
# PostToolUse hook: Unity 관련 소스 편집 후 빠른 compile check를 자동 실행한다.

set -euo pipefail

INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_input',{}).get('file_path',''))" 2>/dev/null || true)

if [[ -z "${FILE_PATH:-}" ]]; then
  exit 0
fi

case "$FILE_PATH" in
  *.cs|*.uxml|*.uss|*.json)
    ;;
  *)
    exit 0
    ;;
esac

PROJECT_DIR="${CLAUDE_PROJECT_DIR:-/Users/family/jason/FR5UNITY/robotapp}"

if ! command -v unityctl >/dev/null 2>&1; then
  echo "unity compile hook skipped: unityctl not found" >&2
  exit 0
fi

echo "## Post-edit Unity compile check..." >&2
if unityctl check --project "$PROJECT_DIR" --type compile --json >/tmp/post-edit-unity-compile.json 2>/dev/null; then
  python3 - <<'PY' /tmp/post-edit-unity-compile.json >&2
import json,sys
path=sys.argv[1]
with open(path,'r',encoding='utf-8') as f:
    data=json.load(f)
msg=data.get('message','Compilation check finished')
print(f"unity compile hook: {msg}")
PY
else
  echo "unity compile hook: compile check failed" >&2
fi

exit 0
