# code-doc-align

Task: KineTutor3D 코드-문서 정합성 확인
Schedule: daily 21:30 (Asia/Seoul)
DRY_RUN: false (true 설정 시 변경 없이 리포트만)

## 목표
- `Assets/Scripts/` 디렉토리 구조와 문서 보드/매트릭스 간 드리프트 감지
- 제한적 자동 수정 수행

## 파싱 대상
1. **Assets/Scripts/** — .cs 파일이 있는 하위 폴더 스캔 → `managed_modules`
2. **docs/status/PHASE-EXECUTION-BOARD.md** — module 열 파싱 → `board_modules`
3. **docs/status/SKILL-DOC-MATRIX.md** — target_module 열 파싱 → `matrix_modules`

## 비교 규칙

### 드리프트 정의
- `managed_modules`에 있지만 `board_modules`에 없음 = **코드 선행 드리프트**
- `board_modules`에 있지만 `managed_modules`에 없고 상태가 Ready가 아님 = **문서 선행 드리프트**
- `board_modules` ≠ `matrix_modules` = **보드-매트릭스 불일치**

### 자동 수정 우선순위
1. PHASE-EXECUTION-BOARD.md → SKILL-DOC-MATRIX.md 순으로 수정
2. 새 코드 모듈이 발견되면 BOARD에 `Ready` 상태로 행 추가
3. 새 문서(daily 로그 등)는 자동 생성하지 않음 → manual_required로 기록

## 프로세스

### 1. Lock 획득
```
Lock 파일: docs/status/.code-doc-align.lock
규칙: docs-nightly-organizer와 동일
```

### 2. 스캔
```
managed_modules = [Assets/Scripts/ 하위에서 .cs 파일이 1개 이상 있는 폴더명]
board_modules = [PHASE-EXECUTION-BOARD.md 테이블의 module 열 값]
matrix_modules = [SKILL-DOC-MATRIX.md 테이블의 target_module 열 값]
```

### 3. 비교 & 수정
```
for module in (managed_modules ∪ board_modules ∪ matrix_modules):
    if module in managed but not in board:
        → BOARD에 Ready 행 추가 (auto_fix)
    if module in board but not in matrix:
        → MATRIX에 행 추가 (auto_fix, skill은 "-"으로)
    if module in board and status != Ready but not in managed:
        → drift로 기록 (manual_required)
```

### 4. 리포트 생성
```
docs/status/INTEGRITY-REPORT.md 덮어쓰기:
- managed_modules 수
- drift_count
- auto_fix_count
- manual_required 수
- 상세 이슈 목록

docs/status/INTEGRITY-HISTORY.ndjson append:
{"timestamp":"ISO8601","drift":N,"auto_fix":N,"manual_required":N}
```

### 5. Lock 해제

## 출력
- `docs/status/INTEGRITY-REPORT.md` (덮어쓰기)
- `docs/status/INTEGRITY-HISTORY.ndjson` (append)
- `docs/status/PHASE-EXECUTION-BOARD.md` (자동 수정 시)
- `docs/status/SKILL-DOC-MATRIX.md` (자동 수정 시)
- `docs/status/.code-doc-align.lock` (COMPLETED)
