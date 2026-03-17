#!/bin/bash
# pre-commit-guard.sh
# PreToolUse 훅 — Bash(git commit) 실행 전 pre-commit-check.sh 자동 실행.
# staged .cs 파일이 있을 때만 검증하고, 에러 시 커밋을 차단합니다.

# stdin으로 전달되는 JSON에서 tool_input.command 읽기
input=$(cat)
command=$(echo "$input" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_input',{}).get('command',''))" 2>/dev/null)

# git commit 명령이 아니면 통과
if ! echo "$command" | grep -q 'git commit'; then
  exit 0
fi

# staged .cs 파일 확인
cs_files=$(git diff --cached --name-only --diff-filter=ACM -- '*.cs' 2>/dev/null)
if [[ -z "$cs_files" ]]; then
  exit 0
fi

# pre-commit-check.sh 실행
echo "## Pre-commit 자동 검증 실행 중..." >&2
SCRIPT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
if bash "$SCRIPT_DIR/scripts/pre-commit-check.sh" 2>&1; then
  echo "pre-commit 검증 통과" >&2
  exit 0
else
  echo "BLOCKED: pre-commit-check.sh 검증 실패. 코드 패턴 규칙을 수정한 후 다시 커밋하세요." >&2
  exit 1
fi
