# Pendant V3 Click Matrix Stabilization

## Context

- Pendant V3 point/bundle/library recut 이후 `RunFunctionActualClickMatrixForDebug()`가 새 UX 계약과 맞지 않아 locator/runtime failure를 냈다.
- 실패 원인은 기능 로직 하나가 아니라 디버그 클릭 경로가 여러 `UIDocument`, detached row button, `Button.clicked` / `ClickEvent` callback 차이를 같이 타는 문제였다.

## Implementation

- `RobotControlV3DebugBridge.ClickUiButton()`을 모든 `UIDocument`와 `PointMoveController` debug root 기준으로 버튼을 찾게 보강했다.
- detached된 과거 row button은 `panel == null` 기준으로 제외했다.
- row action button 생성 경로를 `button.clicked`로 통일했다.
- Function actual click matrix를 현재 UX에 맞춰 조정했다.
  - 포인트 subview에서 row `묶음 추가` 클릭은 action modal open 검증으로 본다.
  - 함수 생성/초기화/서브뷰 전환/rename/duplicate/delete는 각각 실제 버튼 1개당 1개 기대값으로 확인한다.
- `PointMoveController`에는 QA 전용 subview 고정/버튼 수집 helper를 추가했다.
- domain reload 뒤 `RobotControlV3RuntimeController`가 partial initialized 상태로 남아 NRE를 내지 않도록 초기화 guard를 보강했다.

## Validation

- `unityctl check --type compile`: pass
- `RobotStageOrientationGizmoControllerTests`: `2/2 PASS`
- `RobotStageRenderSurfaceInputTests`: `4/4 PASS`
- `RobotControlV3GizmoBehaviorTests`: `3/3 PASS`
- `RunTeachingBlockSequenceMatrixForDebug()`: `9/9 PASS`
- `RunFunctionActualClickMatrixForDebug()`: `7/7 PASS`
- `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
- `RunSequenceFunctionBulkManagementMatrixForDebug()`: `11/11 PASS`
- `RunBundleAddDeleteRunMatrixForDebug()`: `5/5 PASS`

## Notes

- `FunctionActualClickMatrix`는 04-23 이전의 `8/8` 계약이 아니라, 현재 point/bundle/library recut 후의 `7/7` 계약을 기준으로 한다.
- 실제 live command gate는 그대로 닫혀 있다. 이번 작업은 UI/debug click 안정화와 mock/runtime matrix 검증 범위다.
