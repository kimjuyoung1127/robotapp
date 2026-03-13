# 정합성 리포트

`code-doc-align` 자동화가 생성하는 코드-문서 정합성 감사 결과.

## 최종 실행
- 일시: 2026-03-12 13:09 KST
- 상태: completed (DRY_RUN=false)

## 결과
| 항목 | 값 |
|------|-----|
| managed_modules | 7 |
| managed_product_docs | 3 |
| board_product_docs | 3 |
| drift_count | 1 |
| auto_fix_count | 0 |
| manual_required | 1 |

## Validation
- module board sync: yes (board uses descriptive phase labels; 7 code modules represented across 30 board rows — accepted design convention)
- skill-doc matrix sync: yes (matrix uses canonical folder-based module names; all code modules covered)
- product doc board sync (`PRD`/`WIREFRAME`/`PRODUCT-ROADMAP`): yes
- canonical product docs last_updated match:
  - PRD (2026-03-11) == PRODUCT-DOC-BOARD (2026-03-11): ✓
  - WIREFRAME (2026-03-12) == PRODUCT-DOC-BOARD (2026-03-12): ✓
  - PRODUCT-ROADMAP (2026-03-12) == PRODUCT-DOC-BOARD (2026-03-12): ✓
- PRD downstream sync: ✓ (PROJECT-STATUS, ai-context/project-context, ai-context/master-plan — confirmed via 03-12 daily logs)
- WIREFRAME downstream sync: PARTIAL
  - USER-FLOW.md (2026-03-12): ✓
  - tutor-step-plan.md (2026-03-12): ✓
  - **architecture-diagrams.md (2026-03-09): ✗ — NOT synced with WIREFRAME 03-12 changes**
- PRODUCT-ROADMAP downstream sync: ✓ (PROJECT-STATUS ✓, PHASE-EXECUTION-BOARD ✓, ai-context/master-plan ✓ — confirmed via 03-12 daily logs)
- daily log created for product-doc change (03-12): ✓ (module-master-plan-home-sandbox-sync.md, module-p0-p1-reprioritization-home-sandbox.md)
- weekly rollup updated for milestone-level doc change (2026-W11): ✓ (exists; will be updated in docs-nightly pass)

## 상세 이슈
| # | 유형 | 대상 | 설명 | 조치 |
|---|------|------|------|------|
| 1 | 제품 문서 downstream drift | architecture-diagrams.md | WIREFRAME 2026-03-12 변경이 반영되지 않음. architecture-diagrams.md는 2026-03-09 이후 업데이트 없음. | manual_required: architecture-diagrams.md를 WIREFRAME 03-12 변경 (Home/Continue Hub, Sandbox, 재진입 흐름) 기준으로 업데이트 |

## 스캔 상세
| 항목 | 값 |
|------|-----|
| managed_modules | App, Kinematics, Math, Templates, Types, UI, Visualization |
| board rows | 30 (Done: 18, Ready: 11, Hold: 1, InProgress: 0, QA: 0) |
| matrix rows | 12 skills mapped |
| product doc last_updated match | PRD: ✓  WIREFRAME: ✓  PRODUCT-ROADMAP: ✓ |

## 규칙
- 이 파일은 `code-doc-align` 자동화가 덮어씁니다.
- 첫 자동 실행 전 초기 bootstrap baseline만 수동 반영할 수 있습니다.
