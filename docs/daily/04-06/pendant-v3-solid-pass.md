# Pendant V3 SOLID Pass

## Date
- 2026-04-06 (KST)

## Summary
- Teaching Pendant V3 계획이 SOLID 원칙을 더 잘 지키도록 문서 보강을 추가했다.
- 핵심 보강 포인트는 `Binder 책임 상한`, `ViewState slice`, `Mock/Live 계약 일치`, `concrete 의존 억제`다.

## Updated Docs
- `docs/ref/product/pendant-v3/migration-strategy.md`
- `docs/ref/product/pendant-v3/AGENT-CONTRACT.md`

## Decisions Added
- `PendantV3Binder`는 배선만 담당하고 상태 계산을 떠안지 않는다.
- 패널은 전체 상태를 다 물지 않고 필요한 slice만 소비하는 방향을 우선한다.
- `MockFairinoClient`와 `LiveFairinoClient`는 `IFairinoRobotClient` 계약을 같은 의미로 유지해야 한다.
- UI 계층은 concrete 구현이 아니라 state contract / interface / facade에 의존해야 한다.
