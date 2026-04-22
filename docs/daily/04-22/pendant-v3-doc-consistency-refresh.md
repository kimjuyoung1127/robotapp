# Pendant V3 Doc Consistency Refresh

## Summary

- Pendant V3 active SSOT 문서의 stale 상태 문구를 최신 handoff 기준으로 맞췄다.
- 기준 handoff 커밋은 `853d6c5 Document RobotControl V3 next session handoff`로 정리했다.
- 다음 구현 우선순위는 `Operator live confirm UX -> Boundary/Collision real data -> Point MoveJ production IK policy -> Program Run/Step queue`로 고정했다.

## Updated Docs

- `docs/ref/product/pendant-v3/README.md`
- `docs/ref/product/pendant-v3/progress-checklist.md`
- `docs/ref/product/pendant-v3/robot-button-integration-plan.md`
- `docs/ref/product/pendant-v3/feature-points-teaching.md`
- `docs/ref/product/pendant-v3/shell-layout.md`
- `docs/ref/product/pendant-v3/implementation-plan.md`

## Consistency Fixes

- `Live SDK Gate`를 `pending`에서 `partial`로 정리하고, live command safety gate scaffold는 완료지만 product operator confirm UI는 pending으로 분리했다.
- `Mock e2e 완료 전까지 live 금지` 문구를 현재 조건에 맞게 갱신했다.
- 포인트 저장/호출은 SSOT 미정이 아니라 `WaypointStore` 기반 v1 wired 상태로 정리했다.
- 포인트 저장 경로를 `Application.persistentDataPath/waypoints/PendantV3Points.json`로 통일했다.
- 보조/오른쪽 패널 layout acceptance 기준을 `scrollShare>=0.88`로 통일했다.

## Remaining Product Gaps

- operator confirm product UX
- boundary/collision real data
- Point MoveJ production IK policy
- Program Run/Step queue
