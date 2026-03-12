# 정합성 리포트

`code-doc-align` 자동화가 생성하는 코드-문서 정합성 감사 결과.

## 최종 실행
- 일시: 2026-03-12 KST
- 상태: completed (sprint-docs-sync Phase 5G)

## 결과
| 항목 | 값 |
|------|-----|
| managed_modules | 7 |
| managed_product_docs | 3 |
| board_product_docs | 3 |
| drift_count | 0 |
| auto_fix_count | 0 |
| manual_required | 0 |

## Validation
- module board sync: yes (board uses descriptive phase labels; 7 code modules represented across 17 board rows — accepted design convention)
- skill-doc matrix sync: yes (matrix uses canonical folder-based module names; all code modules covered)
- product doc board sync (`PRD`/`WIREFRAME`/`PRODUCT-ROADMAP`): yes
- canonical product docs last_updated (2026-03-12) == PRODUCT-DOC-BOARD last_updated (2026-03-12): yes
- canonical product docs downstream sync (`PROJECT-STATUS`/`ai-context`/`ref`): yes
- daily log created for product-doc change (03-12): yes (scara-template-and-docs-completion.md)
- weekly rollup updated for milestone-level doc change (2026-W11): yes
- skill routing verification: 13/13 skills, 114/114 documents reachable (100%)

## 상세 이슈
- none (이전 누락 문서 2개 생성 완료: asset-sourcing-checklist.md, asset-curation-map.md)

## 스캔 상세
| 항목 | 값 |
|------|-----|
| managed_modules | App, Kinematics, Math, Templates, Types, UI, Visualization |
| board rows | 17 (Done: 13, Hold: 1, Ready: 3) |
| matrix rows | 13 skills mapped |
| product doc last_updated match | PRD: ✓  WIREFRAME: ✓  PRODUCT-ROADMAP: ✓ |
| test results | EditMode 107/107, PlayMode 30/30 |

## 규칙
- 이 파일은 `code-doc-align` 자동화가 덮어씁니다.
- 첫 자동 실행 전 초기 bootstrap baseline만 수동 반영할 수 있습니다.
