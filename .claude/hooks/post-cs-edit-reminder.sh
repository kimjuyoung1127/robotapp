#!/bin/bash
# post-cs-edit-reminder.sh
# PostToolUse 훅 — .cs 파일 편집 후 코드 패턴 규칙 리마인더 출력.

input=$(cat)
file_path=$(echo "$input" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_input',{}).get('file_path',''))" 2>/dev/null)

# .cs 파일이 아니면 무시
if [[ "$file_path" != *.cs ]]; then
  exit 0
fi

echo "✓ C# 파일 수정됨: $(basename "$file_path") — UTF-8 BOM, camelCase 필드, Folder 헤더, BindListeners 패턴 준수 확인"
exit 0
