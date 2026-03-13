# Release Gates

## Purpose
- 릴리스 기준과 플랜 변경 SOP를 정의한다.

## Parent Doc
- [PRODUCT-ROADMAP](../../PRODUCT-ROADMAP.md)

## When To Read
- milestone 검토, plan 변경, release readiness 판단 시

## Locked Decisions
- root canonical 문서와 downstream sync가 완료되어야 QA 이상으로 올린다
- plan 변경 처리 순서는 `leaf -> root canonical -> downstream -> board -> logs`
- 경쟁제품 분석은 scope 확장의 근거가 아니라 complexity guardrail의 근거로도 사용한다

## Open Questions
- 자동화가 weekly milestone까지 어디까지 강제할지

## Downstream Sync
- `docs/ref/PRODUCT-ROADMAP.md`
- `docs/status/PRODUCT-DOC-BOARD.md`

## Last Updated
- 2026-03-12 (KST)

## Release Gates
1. 문서 drift 0
2. Guided Lesson UX 계약 고정
3. 태블릿 기준 사용성 확보
4. 비공개 자료 비노출 정책 준수
5. 핵심 런타임 테스트 유지
6. 온보딩 skip과 재방문 흐름이 초보자 hostile하지 않게 `Home / Continue Hub` 기준으로 정리된다

## Scope Rejections
- `vendor lock-in` 금지
- `ROS-heavy dependency` 금지
- `factory-scale simulation scope creep` 금지
- `beginner-hostile UI` 금지

## Complexity Guardrails
- 산업용 전문가 툴처럼 메뉴와 설정이 먼저 보이는 구조로 확장하지 않는다.
- 강사용 기능은 lesson 제어와 시연을 돕는 수준으로 유지하고, 공장 운영 툴처럼 확대하지 않는다.
- Sandbox는 범용 시뮬레이터가 아니라 학습 목적의 실습 공간으로 유지한다.
- 온보딩 skip은 고급 step 직접 점프보다 hub/continue 선택 흐름을 우선한다.

## Plan Change SOP
- 세부 변경
  - leaf 문서만 수정
  - 잠금 결정 변화가 있으면 root 문서 갱신
  - daily 로그 작성
- 구조 변경
  - leaf -> root -> downstream -> board -> daily
- 전략 변경
  - foundation/ux/roadmap leaf -> root 3문서 -> status/context -> board -> daily -> 필요 시 weekly
