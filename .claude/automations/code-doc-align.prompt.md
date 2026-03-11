# code-doc-align

Task: KineTutor3D 코드-문서 정합성 확인
Schedule: 평일 13:00 (Asia/Seoul)
DRY_RUN: false (true 설정 시 변경 없이 리포트만)

## 목표
- `Assets/Scripts/` 디렉토리 구조와 문서 보드/매트릭스 간 드리프트 감지
- canonical product docs 3종과 `PRODUCT-DOC-BOARD` 간 드리프트 감지
- 제한적 자동 수정 수행

## 파싱 대상
1. **Assets/Scripts/** — .cs 파일이 있는 하위 폴더 스캔 → `managed_modules`
2. **docs/status/PHASE-EXECUTION-BOARD.md** — module 열 파싱 → `board_modules`
3. **docs/status/SKILL-DOC-MATRIX.md** — target_module 열 파싱 → `matrix_modules`
4. **docs/ref/** — canonical product docs 집합 확인 → `canonical_product_docs`
5. **docs/status/PRODUCT-DOC-BOARD.md** — doc_id 열 파싱 → `board_product_docs`

### Product Doc ID 매핑
- `prd` <-> `docs/ref/PRD.md`
- `wireframe` <-> `docs/ref/WIREFRAME.md`
- `product-roadmap` <-> `docs/ref/PRODUCT-ROADMAP.md`

## 비교 규칙

### 드리프트 정의
- `managed_modules`에 있지만 `board_modules`에 없음 = **코드 선행 드리프트**
- `board_modules`에 있지만 `managed_modules`에 없고 상태가 Ready가 아님 = **문서 선행 드리프트**
- `board_modules` ≠ `matrix_modules` = **보드-매트릭스 불일치**
- `canonical_product_docs` ≠ `board_product_docs` = **제품 문서 보드 불일치**
- product doc 파일의 `Last Updated` 날짜 ≠ `PRODUCT-DOC-BOARD.last_updated` = **제품 문서 버전 드리프트**
- canonical product doc 변경이 downstream sync 문서에 반영되지 않음 = **제품 문서 downstream drift**

### 자동 수정 우선순위
1. PHASE-EXECUTION-BOARD.md → SKILL-DOC-MATRIX.md 순으로 수정
2. 새 코드 모듈이 발견되면 BOARD에 `Ready` 상태로 행 추가
3. canonical product docs 누락 시 `PRODUCT-DOC-BOARD.md`에 `Ready` 행 추가
4. 새 문서(daily 로그 등)는 자동 생성하지 않음 → manual_required로 기록

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
canonical_product_docs = [`prd`, `wireframe`, `product-roadmap` 파일 존재 여부]
board_product_docs = [PRODUCT-DOC-BOARD.md 테이블의 doc_id 열 값]
product_doc_last_updated = [각 canonical product doc의 `Last Updated` 날짜]
board_last_updated = [PRODUCT-DOC-BOARD.md 테이블의 last_updated 열 값]
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

for product_doc in (canonical_product_docs ∪ board_product_docs):
    if product_doc in canonical but not in board:
        → PRODUCT-DOC-BOARD에 Ready 행 추가 (auto_fix)
    if product_doc in board but not in canonical:
        → drift로 기록 (manual_required)
    if product_doc_last_updated != board_last_updated:
        → drift로 기록 (manual_required)

downstream sync 확인:
- `prd` 변경 -> `PROJECT-STATUS`, `ai-context/project-context`, `ai-context/master-plan`
- `wireframe` 변경 -> `USER-FLOW`, `tutor-step-plan`, 필요 시 `architecture-diagrams`
- `product-roadmap` 변경 -> `PROJECT-STATUS`, `PHASE-EXECUTION-BOARD`, `ai-context/master-plan`
```

### 4. 리포트 생성
```
docs/status/INTEGRITY-REPORT.md 덮어쓰기:
- managed_modules 수
- managed_product_docs 수
- board_product_docs 수
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
