#!/bin/bash
# cli-gap-analyzer.sh
# MCP 사용 로그를 분석하여 CLI 도구 후보를 리포트합니다.
#
# 사용법:
#   ./scripts/cli-gap-analyzer.sh              # 전체 리포트
#   ./scripts/cli-gap-analyzer.sh --top 5      # 상위 5개만
#   ./scripts/cli-gap-analyzer.sh --json       # JSON 출력
#   ./scripts/cli-gap-analyzer.sh --since 7d   # 최근 7일만

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
LOG_FILE="$PROJECT_DIR/logs/mcp-usage.jsonl"
KNOWN_CLI="$PROJECT_DIR/logs/known-cli-tools.txt"
REPORT_FILE="$PROJECT_DIR/logs/cli-gap-report.md"

TOP_N=0
JSON_MODE=false
SINCE_DAYS=0

# 인자 파싱
while [[ $# -gt 0 ]]; do
  case "$1" in
    --top)   TOP_N="$2"; shift 2 ;;
    --json)  JSON_MODE=true; shift ;;
    --since)
      DAYS="${2%d}"
      SINCE_DAYS="$DAYS"
      shift 2 ;;
    -h|--help)
      echo "Usage: $0 [--top N] [--json] [--since Nd]"
      echo "  --top N    Show top N candidates only"
      echo "  --json     Output as JSON"
      echo "  --since Nd Filter to last N days"
      exit 0 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

# 로그 파일 존재 확인
if [ ! -f "$LOG_FILE" ]; then
  echo "No MCP usage log found at: $LOG_FILE"
  echo "MCP fallback logger hook이 아직 데이터를 수집하지 않았습니다."
  echo ""
  echo "활성화 확인: .claude/settings.json의 PreToolUse/PostToolUse 훅"
  exit 0
fi

LINE_COUNT=$(wc -l < "$LOG_FILE" | tr -d ' ')
if [ "$LINE_COUNT" -eq 0 ]; then
  echo "MCP usage log is empty. MCP 도구가 아직 사용되지 않았습니다."
  exit 0
fi

# 날짜 필터 적용
if [ "$SINCE_DAYS" -gt 0 ]; then
  CUTOFF=$(date -u -d "$SINCE_DAYS days ago" +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || \
           date -u -v-"${SINCE_DAYS}d" +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || echo "")
  if [ -n "$CUTOFF" ]; then
    FILTERED=$(jq -c --arg cutoff "$CUTOFF" 'select(.timestamp >= $cutoff)' "$LOG_FILE")
  else
    FILTERED=$(cat "$LOG_FILE")
  fi
else
  FILTERED=$(cat "$LOG_FILE")
fi

FILTERED_COUNT=$(echo "$FILTERED" | grep -c '^{' 2>/dev/null || echo "0")
if [ "$FILTERED_COUNT" -eq 0 ]; then
  echo "필터 조건에 맞는 MCP 사용 기록이 없습니다."
  exit 0
fi

# === 분석 ===

# 1. CLI 대체 불가 (substitute=none)인 MCP 호출 빈도 집계
GAPS=$(echo "$FILTERED" | \
  jq -r 'select(.cli_substitute == "none" and .event == "PreToolUse") | "\(.server)__\(.operation)"' | \
  sort | uniq -c | sort -rn)

# 2. CLI 대체 가능했던 MCP 호출 (중복 사용)
DUPLICATES=$(echo "$FILTERED" | \
  jq -r 'select(.cli_substitute != "none" and .event == "PreToolUse") | "\(.server)__\(.operation) → \(.cli_substitute)"' | \
  sort | uniq -c | sort -rn)

# 3. 서버별 호출 빈도
SERVER_FREQ=$(echo "$FILTERED" | \
  jq -r 'select(.event == "PreToolUse") | .server' | \
  sort | uniq -c | sort -rn)

# 4. 파라미터 패턴 (자주 사용되는 파라미터 조합)
PARAM_PATTERNS=$(echo "$FILTERED" | \
  jq -r 'select(.event == "PreToolUse") | "\(.server)__\(.operation): \(.param_keys | join(","))"' | \
  sort | uniq -c | sort -rn)

# TOP_N 적용
if [ "$TOP_N" -gt 0 ]; then
  GAPS=$(echo "$GAPS" | head -n "$TOP_N")
fi

# === JSON 출력 모드 ===
if [ "$JSON_MODE" = true ]; then
  echo "$FILTERED" | \
    jq -s --argjson topN "$TOP_N" '
      [.[] | select(.cli_substitute == "none" and .event == "PreToolUse")]
      | group_by((.server) + "__" + (.operation))
      | map({
          mcp_tool: (.[0].server + "__" + .[0].operation),
          call_count: length,
          param_patterns: [.[].param_keys] | unique,
          first_seen: ([.[].timestamp] | sort | .[0]),
          last_seen: ([.[].timestamp] | sort | .[-1]),
          suggested_cli_name: (.[0].operation | gsub("_"; "-"))
        })
      | sort_by(-.call_count)
      | if $topN > 0 then .[:$topN] else . end
    '
  exit 0
fi

# === 마크다운 리포트 ===
{
  echo "# CLI Gap Analysis Report"
  echo ""
  echo "Generated: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
  echo "Log entries analyzed: $FILTERED_COUNT"
  echo ""

  echo "## 1. CLI 도구 후보 (MCP 대체 불가 — Gap)"
  echo ""
  if [ -n "$GAPS" ]; then
    echo "| 호출수 | MCP 도구 | 추천 CLI 이름 |"
    echo "|--------|----------|---------------|"
    echo "$GAPS" | while read -r count tool; do
      cli_name=$(echo "$tool" | sed 's/__/-/g' | tr '[:upper:]' '[:lower:]')
      echo "| $count | \`$tool\` | \`$cli_name\` |"
    done
  else
    echo "Gap 없음 — 모든 MCP 호출에 CLI 대체 도구가 존재합니다."
  fi
  echo ""

  echo "## 2. 중복 사용 (CLI 대체 가능했으나 MCP 사용)"
  echo ""
  if [ -n "$DUPLICATES" ]; then
    echo "| 호출수 | MCP 도구 → CLI 대체 |"
    echo "|--------|---------------------|"
    echo "$DUPLICATES" | while read -r count mapping; do
      echo "| $count | \`$mapping\` |"
    done
  else
    echo "중복 없음."
  fi
  echo ""

  echo "## 3. 서버별 호출 빈도"
  echo ""
  echo "| 호출수 | MCP 서버 |"
  echo "|--------|----------|"
  echo "$SERVER_FREQ" | while read -r count server; do
    echo "| $count | \`$server\` |"
  done
  echo ""

  echo "## 4. 파라미터 패턴"
  echo ""
  echo "| 호출수 | 도구: 파라미터 |"
  echo "|--------|---------------|"
  echo "$PARAM_PATTERNS" | head -20 | while read -r count pattern; do
    echo "| $count | \`$pattern\` |"
  done
  echo ""

  echo "## 5. 다음 행동"
  echo ""
  echo "Gap 도구 후보를 CLI로 구현하려면:"
  echo '```bash'
  echo './scripts/cli-tool-scaffold.sh --from-gap   # gap report 기반 자동 생성'
  echo './scripts/cli-tool-scaffold.sh <tool-name>  # 단일 도구 스캐폴드'
  echo '```'
} | tee "$REPORT_FILE"

echo ""
echo "Report saved to: $REPORT_FILE"
