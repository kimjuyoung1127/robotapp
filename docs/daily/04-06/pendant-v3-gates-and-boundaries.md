# Pendant V3 Gates And Boundaries

## Date
- 2026-04-06 (KST)

## Summary
- `pendant-v3` 문서에서 아직 열려 있던 핵심 결정 3가지를 추가로 잠갔다.
- 대상은 `채택/폐기 게이트`, `3D viewport 경계`, `Mock 종단 검증 종료 조건`이다.

## Updated Docs
- `docs/ref/product/pendant-v3/migration-strategy.md`
- `docs/ref/product/pendant-v3/feature-3d-viewport.md`
- `docs/ref/product/pendant-v3/implementation-plan.md`

## Locked Decisions

### 1. 채택 / 폐기 게이트
- V3가 총점만 높아도 성능과 입력 안정성이 무너지면 채택하지 않도록 강제 탈락 조건을 추가했다.
- `반응형 레이아웃`, `데이터 바인딩`, `스타일 유지보수` 중 최소 2개 이상 우세해야 채택 가능하도록 잠갔다.

### 2. 3D viewport 경계
- V3는 `UI Toolkit 셸 + 기존 Visualization 3D` 하이브리드 구조를 유지한다.
- `ViewportHost`만 3D 입력을 받는 영역으로 고정했고, 나머지 V3 UI는 3D 입력 관통 금지로 잠갔다.
- World Space UI Toolkit은 채택 전 주 경로에 넣지 않도록 금지했다.

### 3. Mock 검증 종료 조건
- `3C`는 단순 “동작해 보임”이 아니라 3회 연속 재현, 콘솔 에러/입력 경합/포커스 실패 0건, 핵심 스크린샷 확보까지 만족해야 종료되도록 잠갔다.

## Why
- V3의 남은 큰 리스크는 레이아웃보다도 `채택 기준의 주관성`, `UI/3D 경계 흔들림`, `Mock 검증 종료 조건 부재`였다.
- 이번 잠금으로 초반 구현 단위와 최종 평가 단위가 더 일치하게 됐다.
