# Pendant V3 User Feedback Pass

## Date
- 2026-04-06 (KST)

## Summary
- "1시간 사용한 초보자" 관점의 상상 피드백을 바탕으로 V3 문서에 UX 보강 규칙을 반영했다.
- 핵심 포인트는 `다음 행동 1개 강조`, `위험 버튼 결과 설명`, `현재/목표/경로 시각 구분`, `세션 요약`, `초보자 기본값`, `Mock 종료 조건 강화`다.

## Updated Docs
- `docs/ref/product/pendant-v3/feature-connection-status.md`
- `docs/ref/product/pendant-v3/feature-safety-controls.md`
- `docs/ref/product/pendant-v3/feature-3d-viewport.md`
- `docs/ref/product/pendant-v3/feature-history.md`
- `docs/ref/product/pendant-v3/feature-user-modes.md`
- `docs/ref/product/pendant-v3/implementation-plan.md`

## Decisions Added
- 연결 홈은 현재 상태별 `Primary Next Action` 하나만 강하게 보여준다.
- 위험 버튼은 실행 전 결과 설명을 필수로 제공한다.
- 3D 뷰포트는 `현재 로봇`, `목표 고스트`, `예상 경로`, `위험 구간`을 명확히 구분한다.
- 히스토리 패널은 로그 외에 `1시간 사용 요약 카드`를 제공한다.
- 첫 실행 기본값은 `초보자 모드`, 첫 1시간은 안전 보호 규칙을 유지한다.
- `3C` Mock 검증은 기능 동작뿐 아니라 신뢰감 있는 시각 구분과 의미 전달까지 종료 조건에 포함한다.
