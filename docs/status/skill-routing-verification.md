# 스킬 라우팅 검증 리포트

> 검증일: 2026-03-11
> 목적: CLAUDE.md 진입점에서 모든 스킬/하위 문서까지 도달 가능한지 확인

## 1. 인프라 요약

| 항목 | 결과 |
|---|---|
| 스킬 SKILL.md | 13/13 존재 |
| 모듈 CLAUDE.md | 10/10 존재 |
| 전체 문서 참조 추적 | 114개 |
| 도달 가능 | 112개 (98.2%) |
| 누락 | 2개 |

## 2. 계층별 도달성

### Layer 1: 진입 문서 → 핵심 5개
| 문서 | 상태 |
|---|---|
| `docs/ref/architecture-mermaid.md` | OK |
| `docs/status/PRODUCT-DOC-BOARD.md` | OK |
| `docs/ref/PRD.md` | OK |
| `docs/ref/WIREFRAME.md` | OK |
| `docs/ref/PRODUCT-ROADMAP.md` | OK |

### Layer 2: 핵심 → 하위 문서 (product/)
- PRD.md 하위: 7/7 OK
- WIREFRAME.md 하위: 6/6 OK
- PRODUCT-ROADMAP.md 하위: 8/9 (1 MISSING)
- PRODUCT-DOC-BOARD.md 하위: 9/9 OK

### Layer 3: 스킬 내부 references/
- student-friendly-ux: 3/3 OK
- unity-official-docs: 2/2 OK
- debug-success-capture: 2/2 OK

### Layer 4: 모듈 CLAUDE.md
- 10/10 OK (내부 참조 2개도 모두 도달 가능)

### Layer 5: 기타 (ai-context, docs/ref, docs/status)
- 14/15 OK (1 MISSING)

## 3. 누락 파일

| # | 경로 | 참조 위치 | 심각도 |
|---|---|---|---|
| 1 | `docs/ref/product/roadmap/asset-sourcing-checklist.md` | PRODUCT-ROADMAP.md, CLAUDE.md Task Routing #15 | 중 |
| 2 | `docs/ref/asset-curation-map.md` | CLAUDE.md Source of Truth | 중 |

두 파일 모두 에셋 작업 전용 문서로, 일반 기능 개발 라우팅에는 영향 없음.

## 4. 라우팅 시뮬레이션 (7개 시나리오)

| # | 시나리오 | 스킬 | 의존 체인 | 문서 도달 | 결과 |
|---|---|---|---|---|---|
| 1 | Quaternion 타입 추가 | math-module-add | — | OK | PASS |
| 2 | SCARA 템플릿 추가 | robot-template-add | dh-algorithm-add + editmode-test-add | OK | PASS |
| 3 | Step 5 튜토리얼 | tutor-step-add | — | OK | PASS |
| 4 | 온보딩 UX 개선 | student-friendly-ux | tutor-step-add + scene-scaffold | OK | PASS |
| 5 | EditMode 테스트 추가 | editmode-test-add | — | OK | PASS |
| 6 | Phase 완료 문서 동기화 | sprint-docs-sync | — | OK | PASS |
| 7 | 로보틱스 자료 레슨 반영 | robotics-reference-to-lesson | student-friendly-ux + tutor-step-add | OK | PASS |

**전체 7/7 PASS**

## 5. 임베딩 도입 분석

### 현재 구조
키워드 기반 정적 라우팅 (O(1) lookup):
- Skill 인덱스: 13개 키워드 그룹
- Task Routing: 16개 작업 유형
- SKILL.md Trigger: 세부 매칭

### 비교

| 항목 | Dense | Sparse (BM25) | 현재 |
|---|---|---|---|
| 검색 속도 | 중간 | 빠름 | 가장 빠름 |
| 의미적 유사성 | 높음 | 낮음 | 없음 |
| 확장 임계점 | 100+ 문서 | 1000+ 문서 | ~50개 이하 |

### 결론
현재 규모(스킬 13개, 문서 ~55개)에서는 임베딩이 속도를 높이지 않음.
임베딩 도입 권장 시점: 문서 100개+, 비정형 자연어 질의, 한/영 동의어 매칭 필요 시.

## 6. 합치기 필요 여부

**합칠 필요 없음.**
- 계층 구조가 잘 작동하며 참조 체인이 끊기지 않음
- 스킬이 필요한 문서만 선택적 로딩
- 합치면 컨텍스트 윈도우 낭비 + 노이즈 증가
