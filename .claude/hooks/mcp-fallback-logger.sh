#!/bin/bash
# mcp-fallback-logger.sh
# PreToolUse / PostToolUse 훅 — MCP 도구 호출을 JSONL로 기록.
# CLI 도구로 대체 가능한 패턴을 축적하여 gap analysis에 활용.
#
# stdin: Claude Code hook JSON (tool_name, tool_input, session_id 등)
# stdout: 없음 (로깅만 수행)
# exit 0: 도구 실행 허용

set -euo pipefail

LOG_DIR="${CLAUDE_PROJECT_DIR:-.}/logs"
LOG_FILE="$LOG_DIR/mcp-usage.jsonl"
KNOWN_CLI_FILE="${CLAUDE_PROJECT_DIR:-.}/.claude/known-cli-tools.txt"

mkdir -p "$LOG_DIR"

# stdin에서 hook JSON 읽기
INPUT=$(cat)

TOOL_NAME=$(echo "$INPUT" | jq -r '.tool_name // empty')
HOOK_EVENT=$(echo "$INPUT" | jq -r '.hook_event_name // empty')
SESSION_ID=$(echo "$INPUT" | jq -r '.session_id // empty')

# MCP 도구가 아니면 무시
if [[ ! "$TOOL_NAME" =~ ^mcp__ ]]; then
  exit 0
fi

# mcp__<server>__<tool> 패턴 파싱
IFS='_' read -ra PARTS <<< "${TOOL_NAME#mcp__}"
# PARTS 재구성: mcp__ 제거 후 __ 기준 분리
SERVER=$(echo "$TOOL_NAME" | sed 's/^mcp__//' | sed 's/__.*$//')
OPERATION=$(echo "$TOOL_NAME" | sed "s/^mcp__${SERVER}__//")

TIMESTAMP=$(date -u +%Y-%m-%dT%H:%M:%SZ)

# tool_input에서 핵심 파라미터 추출 (민감 데이터 제외)
PARAMS=$(echo "$INPUT" | jq -c '.tool_input // {}' 2>/dev/null || echo '{}')
# 파라미터 키만 추출 (값은 로깅하지 않음 — 보안)
PARAM_KEYS=$(echo "$PARAMS" | jq -c 'keys' 2>/dev/null || echo '[]')

# 기존 CLI 도구로 대체 가능한지 체크
CLI_SUBSTITUTE="none"
if [ -f "$KNOWN_CLI_FILE" ]; then
  # 간단한 키워드 매칭: MCP 서버/동작명이 CLI 도구명과 겹치는지
  while IFS= read -r cli_tool; do
    cli_lower=$(echo "$cli_tool" | tr '[:upper:]' '[:lower:]')
    op_lower=$(echo "$OPERATION" | tr '[:upper:]' '[:lower:]')
    server_lower=$(echo "$SERVER" | tr '[:upper:]' '[:lower:]')
    if [[ "$op_lower" == *"$cli_lower"* ]] || [[ "$cli_lower" == *"$op_lower"* ]] || \
       [[ "$cli_lower" == *"$server_lower"* ]]; then
      CLI_SUBSTITUTE="$cli_tool"
      break
    fi
  done < "$KNOWN_CLI_FILE"
fi

# JSONL 엔트리 작성
jq -n -c \
  --arg ts "$TIMESTAMP" \
  --arg event "$HOOK_EVENT" \
  --arg tool "$TOOL_NAME" \
  --arg server "$SERVER" \
  --arg op "$OPERATION" \
  --argjson paramKeys "$PARAM_KEYS" \
  --arg session "$SESSION_ID" \
  --arg substitute "$CLI_SUBSTITUTE" \
  '{
    timestamp: $ts,
    event: $event,
    tool: $tool,
    server: $server,
    operation: $op,
    param_keys: $paramKeys,
    session_id: $session,
    cli_substitute: $substitute
  }' >> "$LOG_FILE"

exit 0
