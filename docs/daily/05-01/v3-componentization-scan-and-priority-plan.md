# V3 Componentization Scan And Priority Plan

## Summary

- `RobotControlV3` 주변의 큰 파일을 다시 스캔했다.
- 기준은 `줄 수`보다 `책임 혼합도`였다.
- runtime과 UI 양쪽에서 가장 먼저 더 쪼갤 파일을 우선순위로 잠갔다.

## Current Truth

- runtime top mixed files:
  - `ViewState`
  - `LiveApproval`
  - `Teaching`
  - `PointMove`
  - `Helpers`
- UI top mixed files:
  - `PointMoveController.Functions`
  - `PointMoveController.PointActions`
  - `PointMoveController.ListsAndModals`
  - `PopupCoordinatorV3`
  - `EasyMotionController`

## Locked SSOT

- canonical reference:
  - `/Users/family/jason/FR5UNITY/robotapp/docs/ref/product/pendant-v3/v3-componentization-priority-plan.md`

## First Split Order

1. `PointMoveController.Functions.cs` - completed
   - same-folder partial split
   - `FunctionBuilder`, `FunctionSelection`, `FunctionBulkOps`, `FunctionRunLoop`, `FunctionView`
   - `dotnet build` green
   - Unity compile check pass
2. `RobotControlV3RuntimeController.ViewState.cs`
3. `PointMoveController.PointActions.cs`
4. `RobotControlV3RuntimeController.LiveApproval.cs`
5. `PointMoveController.ListsAndModals.cs` second pass

## Notes

- `ConnectionHomeController`, `StageRuntime`, `ReadbackAsync`, `PointMoveController.Bootstrap`, `PointMoveController.Sequence`는 지금은 비교적 응집적이라고 봤다.
- 이번 패스는 문서화만 했고 코드/정책은 바꾸지 않았다.
