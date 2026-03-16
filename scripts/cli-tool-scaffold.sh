#!/bin/bash
# cli-tool-scaffold.sh
# CLI 도구 보일러플레이트를 자동 생성합니다.
#
# 사용법:
#   ./scripts/cli-tool-scaffold.sh <tool-name> [--desc "설명"]     # 단일 도구 생성
#   ./scripts/cli-tool-scaffold.sh --from-gap                      # gap report에서 일괄 생성
#   ./scripts/cli-tool-scaffold.sh --list-gaps                     # gap 후보 목록만 출력
#   ./scripts/cli-tool-scaffold.sh --dry-run <tool-name>           # 미리보기 (파일 미생성)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
CLI_DIR="$PROJECT_DIR/Assets/Editor/KineTutor3D/CliTools"
TEST_DIR="$PROJECT_DIR/Assets/Tests/EditMode/CliTools"
LOG_FILE="$PROJECT_DIR/logs/mcp-usage.jsonl"
KNOWN_CLI="$PROJECT_DIR/.claude/known-cli-tools.txt"
INTEGRATION_TEST="$PROJECT_DIR/scripts/cli-integration-test.sh"

DRY_RUN=false
FROM_GAP=false
LIST_GAPS=false
TOOL_NAME=""
DESCRIPTION=""

# 인자 파싱
while [[ $# -gt 0 ]]; do
  case "$1" in
    --from-gap)   FROM_GAP=true; shift ;;
    --list-gaps)  LIST_GAPS=true; shift ;;
    --dry-run)    DRY_RUN=true; shift ;;
    --desc)       DESCRIPTION="$2"; shift 2 ;;
    -h|--help)
      echo "Usage: $0 <tool-name> [--desc \"description\"] [--dry-run]"
      echo "       $0 --from-gap    # Generate from gap analysis"
      echo "       $0 --list-gaps   # Show gap candidates"
      echo ""
      echo "Examples:"
      echo "  $0 mesh-inspect --desc \"메시 버텍스/폴리곤 수 검사\""
      echo "  $0 --from-gap"
      exit 0 ;;
    -*) echo "Unknown option: $1"; exit 1 ;;
    *)  TOOL_NAME="$1"; shift ;;
  esac
done

# === 유틸리티 함수 ===

# kebab-case → PascalCase 변환 (예: mesh-inspect → MeshInspect)
to_pascal_case() {
  echo "$1" | sed 's/-/ /g' | awk '{for(i=1;i<=NF;i++) $i=toupper(substr($i,1,1)) tolower(substr($i,2))}1' | tr -d ' '
}

# kebab-case → camelCase 변환
to_camel_case() {
  local pascal
  pascal=$(to_pascal_case "$1")
  echo "${pascal,}"
}

# C# 파일 생성
generate_tool() {
  local name="$1"
  local desc="${2:-$name 관련 검증/조회를 수행합니다}"
  local pascal_name
  pascal_name=$(to_pascal_case "$name")
  local class_name="${pascal_name}Tool"
  local file_path="$CLI_DIR/${class_name}.cs"

  if [ -f "$file_path" ]; then
    echo "  SKIP: $file_path already exists"
    return 1
  fi

  local content
  content=$(cat <<CSHARP
\xEF\xBB\xBF// Folder: Editor/CliTools - unity-cli 커스텀 도구: ${desc}
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// ${desc}
    /// </summary>
    [UnityCliTool(Description = "${desc}")]
    public static class ${class_name}
    {
        /// <summary>도구 파라미터 정의.</summary>
        public static readonly JArray ToolParams = new JArray
        {
            new JObject
            {
                ["name"] = "verbose",
                ["type"] = "boolean",
                ["description"] = "상세 출력 포함 여부",
                ["required"] = false
            }
        };

        /// <summary>명령을 처리합니다.</summary>
        public static object HandleCommand(JObject @params)
        {
            bool verbose = @params?["verbose"]?.Value<bool>() ?? false;

            var result = new JObject
            {
                ["status"] = "ok"
            };

            // TODO: 구현 — MCP gap에서 자동 생성된 스캐폴드입니다.
            // 이 도구의 원래 MCP 호출 패턴:
            //   서버: (gap report 참조)
            //   동작: ${name}
            // 아래에 Unity Editor API를 사용한 구현을 추가하세요.

            if (verbose)
            {
                result["details"] = "verbose output not yet implemented";
            }

            return new SuccessResponse("${class_name} executed.", result);
        }
    }
}
CSHARP
  )

  if [ "$DRY_RUN" = true ]; then
    echo "=== DRY RUN: $file_path ==="
    printf '%b' "$content"
    echo ""
    echo "=== END ==="
    return 0
  fi

  # BOM 포함 파일 생성
  printf '%b' "$content" > "$file_path"
  echo "  CREATED: $file_path"

  # known-cli-tools.txt에 등록
  if [ -f "$KNOWN_CLI" ]; then
    if ! grep -qx "$name" "$KNOWN_CLI"; then
      echo "$name" >> "$KNOWN_CLI"
      echo "  REGISTERED: $name → known-cli-tools.txt"
    fi
  fi

  # cli-integration-test.sh에 항목 추가
  if [ -f "$INTEGRATION_TEST" ]; then
    if ! grep -q "\"$name\"" "$INTEGRATION_TEST"; then
      # 마지막 테스트 항목 뒤에 추가
      sed -i "/^# === END ===/i\\run_test \"$name\" '{}'\\n" "$INTEGRATION_TEST" 2>/dev/null || true
    fi
  fi

  return 0
}

# === --list-gaps 모드 ===
if [ "$LIST_GAPS" = true ]; then
  if [ ! -f "$LOG_FILE" ]; then
    echo "No MCP usage log found. Hook이 아직 데이터를 수집하지 않았습니다."
    exit 0
  fi

  echo "=== CLI 도구 후보 (MCP Gap) ==="
  echo ""
  jq -r 'select(.cli_substitute == "none" and .event == "PreToolUse") | "\(.server)__\(.operation)"' "$LOG_FILE" | \
    sort | uniq -c | sort -rn | \
    while read -r count tool; do
      cli_name=$(echo "$tool" | sed 's/__/-/g' | tr '[:upper:]' '[:lower:]')
      echo "  $count회  $tool  →  추천: $cli_name"
    done

  if [ "$(jq -r 'select(.cli_substitute == "none" and .event == "PreToolUse")' "$LOG_FILE" | wc -l)" -eq 0 ]; then
    echo "  Gap 없음 — MCP 로그에 대체 불가 호출이 없습니다."
  fi
  exit 0
fi

# === --from-gap 모드 ===
if [ "$FROM_GAP" = true ]; then
  if [ ! -f "$LOG_FILE" ]; then
    echo "No MCP usage log found. Hook이 아직 데이터를 수집하지 않았습니다."
    exit 0
  fi

  echo "=== Gap 기반 CLI 도구 일괄 생성 ==="
  echo ""

  CANDIDATES=$(jq -r 'select(.cli_substitute == "none" and .event == "PreToolUse") | "\(.server)__\(.operation)"' "$LOG_FILE" | \
    sort | uniq -c | sort -rn | awk '{print $2}')

  if [ -z "$CANDIDATES" ]; then
    echo "Gap 없음 — 생성할 도구가 없습니다."
    exit 0
  fi

  CREATED=0
  SKIPPED=0

  while IFS= read -r tool; do
    cli_name=$(echo "$tool" | sed 's/__/-/g' | tr '[:upper:]' '[:lower:]')
    server=$(echo "$tool" | sed 's/__.*$//')
    operation=$(echo "$tool" | sed "s/^${server}__//")
    desc="${server} ${operation} 기능을 CLI로 제공합니다"

    echo "Processing: $tool → $cli_name"
    if generate_tool "$cli_name" "$desc"; then
      CREATED=$((CREATED + 1))
    else
      SKIPPED=$((SKIPPED + 1))
    fi
  done <<< "$CANDIDATES"

  echo ""
  echo "완료: ${CREATED}개 생성, ${SKIPPED}개 스킵"
  echo ""
  echo "다음 단계:"
  echo "  1. 생성된 파일의 TODO 주석을 구현으로 교체"
  echo "  2. EditMode 테스트 추가"
  echo "  3. BOM 확인: od -A n -t x1 -N 3 <file>"
  exit 0
fi

# === 단일 도구 생성 모드 ===
if [ -z "$TOOL_NAME" ]; then
  echo "Error: tool name required"
  echo "Usage: $0 <tool-name> [--desc \"description\"]"
  echo "       $0 --from-gap"
  exit 1
fi

echo "=== CLI Tool Scaffold: $TOOL_NAME ==="
echo ""

if [ -z "$DESCRIPTION" ]; then
  DESCRIPTION="$TOOL_NAME 관련 검증/조회를 수행합니다"
fi

if generate_tool "$TOOL_NAME" "$DESCRIPTION"; then
  echo ""
  echo "다음 단계:"
  echo "  1. $CLI_DIR/$(to_pascal_case "$TOOL_NAME")Tool.cs 의 TODO 구현"
  echo "  2. EditMode 테스트 추가"
  echo "  3. BOM 확인: od -A n -t x1 -N 3 $CLI_DIR/$(to_pascal_case "$TOOL_NAME")Tool.cs"
fi
