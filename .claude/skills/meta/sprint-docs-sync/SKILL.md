---
name: sprint-docs-sync
description: "문서 동기화 — 문서 동기화, phase 완료, 스프린트 완료, 일괄 동기화, docs sync"
---

## Trigger
Phase 완료, 주요 마일스톤, 또는 문서 동기화 요청 시.

## Input Context
- 완료된 Phase/마일스톤
- 변경 요약

## Read First
1. `docs/status/PROJECT-STATUS.md`
2. `docs/status/PRODUCT-DOC-BOARD.md`
3. `docs/status/PHASE-EXECUTION-BOARD.md`
4. `docs/status/SKILL-DOC-MATRIX.md`
5. `docs/status/INTEGRITY-REPORT.md`
6. `docs/status/INTEGRITY-HISTORY.ndjson`
7. `docs/ref/PRD.md`
8. `docs/ref/WIREFRAME.md`
9. `docs/ref/PRODUCT-ROADMAP.md`
10. `docs/ref/product/content/open-robotics-reference-pack.md`
11. `docs/ref/product/content/llm-teaching-strategy.md`
12. `docs/ref/product/roadmap/mobile-release-checklist.md`
13. `ai-context/master-plan.md`
14. `ai-context/project-context.md`
15. `Assets/Scripts/` 디렉토리 목록 (Source of Truth)

## Do
1. `PROJECT-STATUS.md` 업데이트: 현재 Phase, 완료 항목, 다음 작업
2. `PRODUCT-DOC-BOARD.md` 업데이트: canonical product docs 상태와 downstream sync 최신화
3. `PHASE-EXECUTION-BOARD.md` 업데이트: 모듈 상태 (Ready/InProgress/QA/Done)
4. `SKILL-DOC-MATRIX.md` 업데이트: 새로 추가된 코드 경로와 canonical product docs required_docs 반영
5. `INTEGRITY-REPORT.md` 업데이트: 새 검증 항목 + product doc drift 결과
6. `INTEGRITY-HISTORY.ndjson`에 append
7. `master-plan.md` 업데이트: 최근 완료 + 다음 우선순위
8. `project-context.md` 업데이트: 현재 상태
9. 제품 문서 변경 시 `docs/daily/MM-DD/` 로그 작성, 마일스톤 변경이면 `docs/weekly/` 롤업도 갱신
10. 제로 드리프트 확인: `managed_modules == board_modules == matrix_modules`
11. product doc 제로 드리프트 확인: `canonical_product_docs == board_product_docs`
12. reference pack, llm strategy, mobile release checklist가 canonical leaf 문서 집합에 포함되는지 확인

## Do Not
1. NDJSON 파일 덮어쓰기 금지 (append-only)
2. Source of Truth(Assets/Scripts/) 확인 없이 문서만 업데이트 금지
3. 드리프트 발견 시 무시하고 진행 금지

## Validation
- [ ] PROJECT-STATUS.md 최신 상태
- [ ] PRODUCT-DOC-BOARD.md canonical docs 상태 정확
- [ ] PHASE-EXECUTION-BOARD.md 모든 모듈 상태 정확
- [ ] SKILL-DOC-MATRIX.md 코드 경로 유효
- [ ] INTEGRITY-HISTORY.ndjson 새 항목 append됨
- [ ] master-plan.md 업데이트됨
- [ ] project-context.md 업데이트됨
- [ ] managed_modules == board_modules == matrix_modules (제로 드리프트)
- [ ] canonical_product_docs == board_product_docs (제로 드리프트)
- [ ] 제품 문서 변경에 대한 daily/weekly sync 확인
- [ ] reference pack / llm strategy / mobile checklist sync 확인

## Output Template
```
[sprint-docs-sync 완료]
- Phase: {PhaseN} → {상태}
- 업데이트된 문서: {N}개
- 드리프트: {0 또는 발견된 수}
- 자동 수정: {N}건
- 수동 필요: {N}건
- 제로 드리프트: {확인/미확인}
```
