#!/usr/bin/env bash
# pre-commit-check.sh
# code-patterns.md §8-9 기준 기계적 검증 스크립트.
# git pre-commit hook 또는 수동으로 실행 가능.
#
# 사용법:
#   ./scripts/pre-commit-check.sh          # staged .cs 파일 검증
#   ./scripts/pre-commit-check.sh --all    # 전체 .cs 파일 검증

set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
NC='\033[0m'

errors=0
warnings=0

# ── 대상 파일 수집 ──────────────────────────────────────
files=()
if [[ "${1:-}" == "--all" ]]; then
    while IFS= read -r line; do files+=("$line"); done <<< "$(find Assets/Scripts Assets/Tests -name '*.cs' 2>/dev/null)"
else
    while IFS= read -r line; do [[ -n "$line" ]] && files+=("$line"); done <<< "$(git diff --cached --name-only --diff-filter=ACM -- '*.cs' 2>/dev/null)"
fi

if [[ ${#files[@]} -eq 0 ]]; then
    echo -e "${GREEN}[OK] 검증할 .cs 파일 없음${NC}"
    exit 0
fi

echo "=== pre-commit-check: ${#files[@]}개 .cs 파일 검증 ==="
echo ""

# ── 1. UTF-8 BOM 확인 (§8.1) ────────────────────────────
echo "--- [1/5] UTF-8 BOM 확인 (§8.1) ---"
for f in "${files[@]}"; do
    if [[ ! -f "$f" ]]; then continue; fi
    # BOM: EF BB BF (hex)
    head_bytes=$(xxd -p -l 3 "$f" 2>/dev/null || true)
    if [[ "$head_bytes" != "efbbbf" ]]; then
        echo -e "${RED}  FAIL: $f — UTF-8 BOM 없음${NC}"
        ((errors++)) || true
    fi
done

# ── 2. 1행 Folder 헤더 확인 (§9) ─────────────────────────
echo "--- [2/5] Folder 헤더 확인 (§9) ---"
for f in "${files[@]}"; do
    if [[ ! -f "$f" ]]; then continue; fi
    # BOM 이후 첫 줄 읽기
    first_line=$(sed -n '1s/^\xEF\xBB\xBF//;1p' "$f" 2>/dev/null || head -1 "$f")
    if [[ ! "$first_line" =~ ^"// Folder:" ]]; then
        echo -e "${RED}  FAIL: $f — 1행에 '// Folder:' 헤더 없음${NC}"
        ((errors++)) || true
    fi
done

# ── 3. 금지 네이밍 패턴 탐지 (§8.2) ─────────────────────
echo "--- [3/5] 네이밍 규칙 확인 (§8.2) ---"
for f in "${files[@]}"; do
    if [[ ! -f "$f" ]]; then continue; fi

    # _prefix 필드 (private int _count 등) — SerializeField 제외
    if grep -Pn '(?<!Serialize)(?:private|protected)\s+\w+\s+_\w+' "$f" 2>/dev/null | grep -v 'SerializeField' | head -3; then
        echo -e "${YELLOW}  WARN: $f — '_' 접두사 필드 발견 (§8.2 금지)${NC}"
        ((warnings++)) || true
    fi

    # m_ 헝가리안 필드
    if grep -Pn '\bm_\w+' "$f" 2>/dev/null | head -3; then
        echo -e "${RED}  FAIL: $f — 'm_' 헝가리안 네이밍 발견 (§8.2 금지)${NC}"
        ((errors++)) || true
    fi
done

# ── 4. 수명주기 패턴 확인 (§8.4-8.6) ────────────────────
echo "--- [4/5] 수명주기 패턴 확인 (§8.4-8.6) ---"
for f in "${files[@]}"; do
    if [[ ! -f "$f" ]]; then continue; fi

    # MonoBehaviour 파일에서 BindButtons 사용 시 경고 (BindListeners로 통일)
    if grep -q 'MonoBehaviour' "$f" 2>/dev/null; then
        if grep -Pn '\bBindButtons\b' "$f" 2>/dev/null | head -3; then
            echo -e "${YELLOW}  WARN: $f — 'BindButtons' → 'BindListeners'로 통일 필요 (§8.5)${NC}"
            ((warnings++)) || true
        fi
        if grep -Pn '\bbuttonsBound\b' "$f" 2>/dev/null | head -3; then
            echo -e "${YELLOW}  WARN: $f — 'buttonsBound' → 'listenersBound'로 통일 필요 (§8.6)${NC}"
            ((warnings++)) || true
        fi
    fi
done

# ── 5. Mojibake 탐지 ─────────────────────────────────────
echo "--- [5/5] Mojibake 탐지 ---"
for f in "${files[@]}"; do
    if [[ ! -f "$f" ]]; then continue; fi
    # EUC-KR mojibake 지표: 한글 자모가 깨진 형태
    # 예: 媛, ?꾩슂, ?⑸땲 등 — 3바이트 한글(0xE-범위) 중간에 ?가 섞이는 패턴
    if grep -Pn '[\x{B9E4}]|[\x{AF3C}]|\?\x{EA}|\?\x{EB}' "$f" 2>/dev/null | head -3; then
        echo -e "${RED}  FAIL: $f — mojibake 의심 패턴 발견 (EUC-KR?)${NC}"
        ((errors++)) || true
    # 대안: python 기반 인코딩 검출 (chardet 가능 시)
    elif python3 -c "
import sys
with open('$f', 'rb') as fh:
    raw = fh.read()
    try:
        raw.decode('utf-8')
    except UnicodeDecodeError:
        print(f'  {\"$f\"}: UTF-8 디코딩 실패')
        sys.exit(1)
" 2>/dev/null; then
        : # valid UTF-8
    else
        echo -e "${RED}  FAIL: $f — UTF-8 디코딩 실패${NC}"
        ((errors++)) || true
    fi
done

# ── 결과 ──────────────────────────────────────────────────
echo ""
echo "========================================="
if [[ $errors -gt 0 ]]; then
    echo -e "${RED}BLOCKED: $errors 에러, $warnings 경고${NC}"
    echo "code-patterns.md §8-9 규칙을 확인하세요."
    exit 1
elif [[ $warnings -gt 0 ]]; then
    echo -e "${YELLOW}PASSED (경고 $warnings 건): 커밋 가능하지만 수정 권장${NC}"
    exit 0
else
    echo -e "${GREEN}PASSED: 모든 검증 통과${NC}"
    exit 0
fi
