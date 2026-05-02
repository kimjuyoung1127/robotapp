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
2. `RobotControlV3RuntimeController.ViewState.cs` - completed
   - `StatusSafety` bucket partial split
   - `SnapshotComposition`, `OperatorCopy`, `GateSummary`, `ModeSessionLabels`
   - `dotnet build` green
   - Unity asset import/refresh 완료
   - Unity compile check는 IPC recovery 후 재시도 필요
3. `PointMoveController.PointActions.cs` - completed
   - same-folder partial split
   - `PointCrud`, `PointMotionHelpers`
   - `PointActions.cs`는 thin anchor partial로 축소
   - `point detail / point modal / bundle picker modal`은 `ListsAndModals`에 남겨둠
   - `dotnet build` green
4. `RobotControlV3RuntimeController.LiveApproval.cs` - completed
   - same-folder partial split
   - `SessionMode`, `TokenLifecycle`, `CommandApproval`, `LoopApproval`
   - `LiveApproval.cs`는 thin anchor partial로 축소
   - broad veto 삭제 대신 후속 축소를 위한 구조 분리로 잠금
   - `dotnet build` green
5. `PointMoveController.ListsAndModals.cs` second pass - completed
   - same-folder partial split
   - `PointDetail`, `PointActionModal`, `BundlePickerModal`
   - `ListsAndModals.cs`는 thin anchor partial로 축소
   - list view 렌더링은 기존 분리 상태 유지
   - `dotnet build` green
6. `RobotControlV3RuntimeController.Teaching.cs` - completed
   - same-folder partial split
   - `TeachingRecording`, `TeachingSequenceRuntime`, `TeachingFunctionBlock`, `TeachingRunnerEvents`
   - `Teaching.cs`는 thin anchor partial로 축소
   - path recording, teaching runtime, function/block editing, runner events 경계를 분리
   - `dotnet build` green
7. `RobotControlV3RuntimeController.PointMove.cs` - completed
   - same-folder partial split
   - `PointMoveSequence`, `PointMoveHomeLoop`, `PointMoveMixedLive`, `PointMovePreview`
   - `PointMove.cs`는 thin anchor partial로 축소
   - named sequence, Home↔Point1 loop build, mixed live continuation, point preview/apply 경계를 분리
   - `dotnet build` green

## Notes

- `ConnectionHomeController`, `StageRuntime`, `ReadbackAsync`, `PointMoveController.Bootstrap`, `PointMoveController.Sequence`는 지금은 비교적 응집적이라고 봤다.
- 이번 잠금 기준 다음 active split은 `RobotControlV3RuntimeController.Helpers.cs`다.
