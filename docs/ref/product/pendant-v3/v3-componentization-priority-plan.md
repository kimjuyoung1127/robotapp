# Pendant V3 Componentization Priority Plan

## Purpose

- `RobotControlV3` 주변의 큰 파일을 줄 수가 아니라 **책임 혼합도** 기준으로 다시 본다.
- 다음 분리 대상을 한 번에 정하고, 새 세션 에이전트도 같은 순서로 따라오게 한다.
- 이미 성공한 패턴인 `panel-first partial`, `같은 폴더 안 책임별 파일 추가`, `move methods first` 원칙을 유지한다.

## Scan Rule

- `1000+`줄은 강한 경고 신호로 본다.
- 최종 분리 판단은 아래 둘을 같이 본다.
  - 한 파일에 서로 다른 사용자 행동, 상태, 렌더링, 실행, 문구 생성이 같이 있는지
  - 수정할 때 파일 여러 구간을 동시에 만지게 되는지
- 즉 `줄 수`보다 `책임 혼합도`를 우선한다.

## Current High-Mix Runtime Files

1. `ViewState`
   - File: `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/RobotControlV3RuntimeController.ViewState.cs`
   - Mixed concerns:
     - snapshot composition
     - gate interpretation
     - operator copy
     - mode/session summary
     - tool/user/coord formatting
   - Natural split:
     - `SnapshotComposition`
     - `OperatorCopy`
     - `GateSummary`
     - `ModeSessionLabels`

2. `LiveApproval`
   - File: `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/Shared/RobotControlV3RuntimeController.LiveApproval.cs`
   - Mixed concerns:
     - session mode
     - approval token lifecycle
     - pending command state
     - gripper/joint/point/sequence approval entry
     - live loop approval
   - Natural split:
     - `SessionMode`
     - `TokenLifecycle`
     - `PerCommandPreflight`
     - `LoopApproval`

3. `Teaching`
   - File: `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/PointMove/RobotControlV3RuntimeController.Teaching.cs`
   - Mixed concerns:
     - recording
     - teaching sequence runtime
     - function/block editing
     - runner events

4. `PointMove`
   - File: `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/PointMove/RobotControlV3RuntimeController.PointMove.cs`
   - Mixed concerns:
     - sequence execution
     - playback
     - prepared target helpers
     - mixed-live recovery
     - point preview/apply

5. `Helpers`
   - File: `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/RobotControlV3RuntimeController.Helpers.cs`
   - Mixed concerns:
     - motion/IK helpers
     - geometry utilities
     - visual stabilization
     - snapshot adjuncts

## Current High-Mix UI Files

1. `PointMoveController.Functions`
   - File: `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/UI/RobotControlV3/PointMoveController/PointMoveController.Functions.cs`
   - Mixed concerns:
     - function builder
     - selection/clear
     - bulk delete
     - bulk timing
     - run/loop
     - summary/debug

2. `PointMoveController.ListsAndModals`
   - File: `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/UI/RobotControlV3/PointMoveController/PointMoveController.ListsAndModals.cs`
   - Current role after first split:
     - point detail
     - point modal
     - bundle picker modal
   - Next natural split:
     - `PointDetail`
     - `PointActionModal`
     - `BundlePickerModal`

3. `PopupCoordinatorV3`
   - File: `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/UI/RobotControlV3/PopupCoordinatorV3.cs`
   - Mixed concerns:
     - popup shell
     - approval adapter
     - action dispatch
     - overlay/fault suppression

4. `EasyMotionController`
   - File: `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/UI/RobotControlV3/EasyMotionController.cs`
   - Mixed concerns:
     - panel shell
     - preset preview
     - gripper control
     - draft/input state
     - quick buttons
     - debug harness

## Comparatively Cohesive Files

- `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/UI/RobotControlV3/ConnectionHomeController.cs`
- `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/UI/RobotControlV3/PointMoveController/PointMoveController.Bootstrap.cs`
- `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/UI/RobotControlV3/PointMoveController/PointMoveController.Sequence.cs`
- `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/Stage/RobotControlV3RuntimeController.StageRuntime.cs`
- `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/RobotControlV3RuntimeController.ReadbackAsync.cs`

## Priority Plan

### P0 - Next Split First

1. `RobotControlV3RuntimeController.ViewState.cs`
   - Why second:
     - highest runtime-side responsibility mixing
     - broad impact on operator wording and diagnostics
     - now isolated enough to split inside `StatusSafety`
   - Split target:
     - `SnapshotComposition`
     - `OperatorCopy`
     - `GateSummary`
     - `ModeSessionLabels`

## Recently Completed

1. `PointMoveController.Functions.cs`
   - Completed as a same-folder partial split inside `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/UI/RobotControlV3/PointMoveController`
   - Result:
     - `PointMoveController.Functions.cs` -> thin anchor partial
     - `PointMoveController.FunctionBuilder.cs`
     - `PointMoveController.FunctionSelection.cs`
     - `PointMoveController.FunctionBulkOps.cs`
     - `PointMoveController.FunctionRunLoop.cs`
     - `PointMoveController.FunctionView.cs`
   - Validation:
     - `dotnet build /Users/family/jason/FR5UNITY/robotapp/robotapp.slnx`
     - `unityctl check --project /Users/family/jason/FR5UNITY/robotapp --type compile --json`

2. `RobotControlV3RuntimeController.ViewState.cs`
   - Completed as a `StatusSafety` bucket split inside `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/StatusSafety`
   - Result:
     - `RobotControlV3RuntimeController.ViewState.cs` -> `ApplyVisualState` 중심 축소
     - `RobotControlV3RuntimeController.SnapshotComposition.cs`
     - `RobotControlV3RuntimeController.OperatorCopy.cs`
     - `RobotControlV3RuntimeController.GateSummary.cs`
     - `RobotControlV3RuntimeController.ModeSessionLabels.cs`
   - Validation:
     - `dotnet build /Users/family/jason/FR5UNITY/robotapp/robotapp.slnx`
     - Unity asset import/refresh 완료
     - Unity compile check는 IPC recovery 후 재시도 필요

3. `PointMoveController.PointActions.cs`
   - Completed as a same-folder partial split inside `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/UI/RobotControlV3/PointMoveController`
   - Result:
     - `PointMoveController.PointActions.cs` -> thin anchor partial
     - `PointMoveController.PointCrud.cs`
     - `PointMoveController.PointMotionHelpers.cs`
   - Scope note:
     - `point detail`, `point modal`, `bundle picker modal`은 아직 `PointMoveController.ListsAndModals.cs`에 남겨두고 2차 분리 대상으로 유지
   - Validation:
     - `dotnet build /Users/family/jason/FR5UNITY/robotapp/robotapp.slnx`

4. `RobotControlV3RuntimeController.LiveApproval.cs`
   - Completed as a same-folder partial split inside `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/Shared`
   - Result:
     - `RobotControlV3RuntimeController.LiveApproval.cs` -> thin anchor partial
     - `RobotControlV3RuntimeController.SessionMode.cs`
     - `RobotControlV3RuntimeController.TokenLifecycle.cs`
     - `RobotControlV3RuntimeController.CommandApproval.cs`
     - `RobotControlV3RuntimeController.LoopApproval.cs`
   - Scope note:
     - 이번 패스는 `LiveApproval` 삭제가 아니라, 후속 `diagnostics-first` 축소를 위한 구조 분리다.
     - broad veto 정책은 아직 유지하고, 세션/토큰/명령/loop 경계만 분리했다.
   - Validation:
     - `dotnet build /Users/family/jason/FR5UNITY/robotapp/robotapp.slnx`

5. `PointMoveController.ListsAndModals.cs`
   - Completed as a same-folder partial split inside `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/UI/RobotControlV3/PointMoveController`
   - Result:
     - `PointMoveController.ListsAndModals.cs` -> thin anchor partial
     - `PointMoveController.PointDetail.cs`
     - `PointMoveController.PointActionModal.cs`
     - `PointMoveController.BundlePickerModal.cs`
   - Scope note:
     - list rendering은 이미 `PointListView / SequenceListView / FunctionListView / BundlePickerListView`로 분리된 상태를 유지한다.
     - 이번 패스는 detail/modal만 분리했다.
   - Validation:
     - `dotnet build /Users/family/jason/FR5UNITY/robotapp/robotapp.slnx`

### P1 - Immediate Follow-Up

3. `RobotControlV3RuntimeController.Teaching.cs`

### P2 - After P0/P1 Stabilize

4. `RobotControlV3RuntimeController.PointMove.cs`

5. `RobotControlV3RuntimeController.Helpers.cs`

6. `PopupCoordinatorV3.cs`

7. `EasyMotionController.cs`

## Safe Execution Rules

- keep the existing folder; do not create a `Bootstrap`-named subfolder
- move methods first; do not mix policy changes in the same pass
- keep one responsibility per new file name
- update the local `CLAUDE.md` file index whenever a split lands
- validate each pass with:
  - Unity compile check
  - the smallest relevant smoke only

## Suggested Execution Order

1. `RobotControlV3RuntimeController.Helpers.cs`
2. `PopupCoordinatorV3.cs`
3. `EasyMotionController.cs`
4. `ConnectionHomeController.cs`

Update:
- `PointMoveController.PointActions.cs` split is complete.
- `RobotControlV3RuntimeController.LiveApproval.cs` split is also complete.
- `PointMoveController.ListsAndModals.cs` second pass is also complete.
- `RobotControlV3RuntimeController.Teaching.cs` split is also complete.
- `RobotControlV3RuntimeController.PointMove.cs` split is also complete.
- the next active starting point is `RobotControlV3RuntimeController.Helpers.cs`.

This is the current recommended order unless a live robot blocker forces a different path.
