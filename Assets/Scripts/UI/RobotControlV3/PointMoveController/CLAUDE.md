# PointMoveController Folder Guide

이 폴더는 `PointMoveController`의 UI orchestration을 책임진다.

## 역할
- Point/Sequence/Function 패널 초기화와 UI wiring
- point preview/apply/save/delete/rename 같은 operator flow
- sequence/function list, modal, detail editor 갱신
- `RobotControlV3RuntimeController`와 `PopupCoordinatorV3`를 호출하는 얇은 UI backend

## 주 소비자
- `PointMoveController`
- `PendantV3ShellStateController`
- `PointMove` 관련 UITK 패널

## 넣지 말 것
- broad live robot policy
- 공용 status/safety taxonomy
- unrelated joint/tcp panel orchestration

## 파일 네이밍 규칙
- `PointMoveController.<Role>.cs` 형식을 유지한다.
- 새 partial 파일 첫 줄에는 `// Folder: PointMoveController - ...` 역할 주석을 둔다.

## 현재 파일 인덱스
- `PointMoveController.cs`: 루트 필드/상수/public debug surface
- `PointMoveController.Bootstrap.cs`: 초기화, 패널 생성, 공통 view wiring
- `PointMoveController.Elements.cs`: 패널 element 바인딩
- `PointMoveController.ListsAndModals.cs`: point detail/modal anchor partial
- `PointMoveController.PointDetail.cs`: selected point detail rendering and shared point/detail helper utilities
- `PointMoveController.PointActionModal.cs`: point action modal visibility, copy, and primary edit/run actions
- `PointMoveController.BundlePickerModal.cs`: bundle picker modal visibility and confirm flow
- `PointMoveController.ListViewShared.cs`: 공용 list row helper와 filter helper
- `PointMoveController.PointListView.cs`: point list 렌더링과 row binding
- `PointMoveController.SequenceListView.cs`: sequence/block list 렌더링과 row binding
- `PointMoveController.FunctionListView.cs`: function list 렌더링과 row binding
- `PointMoveController.BundlePickerListView.cs`: bundle picker list row binding
- `PointMoveController.Sequence.cs`: sequence/path/block teaching 실행 흐름
- `PointMoveController.Functions.cs`: function-domain anchor partial
- `PointMoveController.FunctionBuilder.cs`: function source resolution과 builder panel copy
- `PointMoveController.FunctionSelection.cs`: point/function 선택 상태와 row collapse
- `PointMoveController.FunctionBulkOps.cs`: point/function bulk mutation helpers
- `PointMoveController.FunctionRunLoop.cs`: function dry-run 실행과 teaching loop action
- `PointMoveController.FunctionView.cs`: function summary/detail/list/loop text rendering
- `PointMoveController.Motion.cs`: point motion preview/apply와 feedback
- `PointMoveController.PointActions.cs`: point action anchor partial
- `PointMoveController.PointCrud.cs`: point CRUD/timing/export/order/inventory helpers
- `PointMoveController.PointMotionHelpers.cs`: point live sync, validation, popup, saved-joint helper logic
