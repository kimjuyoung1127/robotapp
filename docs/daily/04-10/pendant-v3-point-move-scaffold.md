# Pendant V3 Point Move Scaffold

## Date
- 2026-04-10 (KST)

## Summary
- `2B-4` 포인트 이동을 다시 여는 최소 scaffold를 붙였다.
- `PointMoveController`, `point-move-panel.uxml/.uss`, shell host, scene authoring wiring까지 연결했다.
- 실제 플레이에서 `TabPointMove` 클릭 후 desktop panel visible까지 확인했다.
- 후속으로 actual `BtnPointApply` 클릭에서 `MoveL` mock dispatch feedback까지 확인했다.
- tablet path에서도 `BottomTabPointMove -> BtnPointApply` 기준 `MoveL` feedback까지 확인했다.
- preview/apply 기준 raw input validation과 ΔTCP summary도 붙였다.
- 실기 연동성 고려로 `PointMoveController`가 직접 `FairinoConnectionService`를 조립하던 경로를 `RobotControlMotionRuntime` facade로 옮겼다.

## Updated Files
- `Assets/UI/PendantV3/point-move-panel.uxml`
- `Assets/UI/PendantV3/point-move-panel.uss`
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/CLAUDE.md`
- `Assets/Scripts/UI/RobotControlV3/PointMoveController.cs`
- `Assets/Scripts/UI/RobotControlV3/PointMoveController.Elements.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3Document.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.State.cs`
- `Assets/Scripts/UI/RobotControlV3/CLAUDE.md`
- `Assets/Scripts/App/RobotControlV3DebugBridge.cs`
- `Assets/Scripts/App/Fairino/Motion/RobotControlMotionRuntime.cs`
- `Assets/Scripts/App/Fairino/CLAUDE.md`
- `Assets/Scripts/App/Fairino/Motion/CLAUDE.md`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `Assets/Tests/EditMode/Core/RobotControlMotionRuntimeTests.cs`
- `docs/ref/product/pendant-v3/progress-checklist.md`

## What Changed
- 새 `PointMove` panel template를 추가했다.
  - 포인트 이름
  - Base/Tool/User 좌표계 선택
  - X/Y/Z/RX/RY/RZ 직접 입력
  - `MoveJ / MoveL` 후보 토글
  - restore / preview / apply 버튼
- `PointMoveController`
  - `TabPointMove` / `BottomTabPointMove`에 따라 desktop/tablet host visibility를 제어한다.
  - preview TCP 값을 기본 입력값으로 채운다.
  - shell local state의 좌표계를 같이 읽고 반영한다.
  - 실기/mock 준비와 `MoveL` dispatch는 `RobotControlMotionRuntime`에 위임하고, UI는 panel state와 validation만 들고 있게 정리했다.
- `RobotControlMotionRuntime`
  - `RobotSelectionBridge -> RobotControlFactory -> FairinoRobotConfig -> FairinoConnectionService` 순서로 선택 로봇 기준 motion session을 만든다.
  - `EnsureReady()`에서 `Connect -> Enable` 준비를 한 군데로 모은다.
  - `DispatchMoveL()` / `DispatchMoveJ()`가 speed/acc 정책과 실제 client 호출을 감싼다.
- authoring/bootstrap
  - `PendantV3Document` 의존 초기화 루프에 `PointMoveController`를 추가했다.
  - `PendantV3SceneBuilder`가 `PointMoveController`와 template reference를 authored scene에 붙이도록 확장했다.
  - `RobotControlV3DebugBridge`에 `GetPointMoveControllerSummary()`를 추가했다.

## Verification
- `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - pass
- `unityctl exec --project ... --code 'KineTutor3D.EditorTools.PendantV3SceneBuilder.AuthorSceneSafe()' --json`
  - `saved=True`
- `unityctl exec --project ... --code 'KineTutor3D.EditorTools.PendantV3SceneBuilder.GetDocumentRootComponentSummary()' --json`
  - `PointMoveController` 포함 확인
- `play start`
- `exec SceneNavigator.LoadByName("RobotControlV3")`
  - `SceneId = 7`
- `uitk click TabPointMove`
  - pass
- `exec KineTutor3D.App.RobotControlV3DebugBridge.GetPointMoveControllerSummary()`
  - `desktopVisible=True`
  - `coord=Base`
  - `motion=MoveJ`
  - `name=Waypoint-1`
- `uitk click BtnPointMoveL`
  - pass
- `uitk click BtnPointApply`
  - pass
- `exec KineTutor3D.App.RobotControlV3DebugBridge.GetPointMoveControllerSummary()`
  - `feedback=[Dispatch] MoveL 완료 · speed 30% · X -497.0 / Y -130.0 / Z 477.0`
- `SetShellSelection("NavMotion", "TabEasyMotion", "BottomTabPointMove")`
  - `tabletVisible=True`
- tablet `uitk click BtnPointMoveL`
  - pass
- tablet `uitk click BtnPointApply`
  - pass
- tablet summary
  - `feedback=[Dispatch] MoveL 완료 · speed 30% · X -497.0 / Y -130.0 / Z 477.0`
- `exec KineTutor3D.App.RobotControlV3DebugBridge.PreviewPointMoveForDebug()`
  - `feedback=[Preview] MoveJ 후보 · 현재는 IK 연결 전이라 관절 후보 계산만 후속으로 붙일 예정.`
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --filter RobotControlMotionRuntimeTests --json`
  - filter runner 불안정으로 직접 매칭은 신뢰도가 낮았고, 대신 full EditMode에서 새 테스트 포함 compile baseline을 확인했다.
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --json`
  - `447 passed / 10 failed / 0 skipped`
  - total이 `455 -> 457`로 증가해 `RobotControlMotionRuntimeTests` 2건이 runner에 포함된 것까지 확인했다.

## Revalidation (2026-04-13, KST)
- `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - pass
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --json`
  - `439 passed / 18 failed / 0 skipped` (`total=457`)
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --filter RobotControlMotionRuntimeTests --json`
  - `0 passed / 0 failed / 0 skipped` (`total=0`)
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlMotionRuntimeTests --json`
  - `2 passed / 0 failed / 0 skipped` (`total=2`)
- 해석
  - short-name filter는 현재 러너 매칭 신뢰도가 낮다.
  - FQCN filter 기준으로 `RobotControlMotionRuntimeTests` 2건은 정상 통과를 재확인했다.

## PointMove Invalid-Input Smoke (2026-04-13, KST)
- Play + `SceneNavigator.LoadByName("RobotControlV3")` + shell selection `NavMotion/TabPointMove` 기준으로 검증.
- case A: `PointValueX="abc"` + actual `BtnPointApply` click
  - feedback: `X 값 형식을 확인해라.`
  - UI class: `PointValueX`에 `rc-point-cell-input--danger` 확인
- case B: `PointValueX="NaN"` + actual `BtnPointApply` click
  - feedback: `X 값 형식을 확인해라.`
  - UI class: `PointValueX`에 `rc-point-cell-input--danger` 확인
- case C: `PointValueRx="361"`
  - `ApplyPointMoveForDebug()` 기준 feedback: `RX 는 -360°~360° 범위 안으로 넣어라.`
  - UI class: `PointValueRx`에 `rc-point-cell-input--danger` 확인
- regression: `BtnPointMoveL` -> actual `BtnPointApply` click
  - feedback: `[Dispatch] MoveL 완료 · speed 30% · X -497.0 / Y -130.0 / Z 477.0`

## Remaining Follow-up
- `MoveJ` 실제 바인딩
- point 입력값 변경 시 actual UI invalid-input smoke 보강

## Self Review
- 역할 경계
  - 이번 추가 범위는 `UI/RobotControlV3` panel scaffold, shell host wiring, debug summary, scene authoring wiring까지만 건드렸다.
  - 실제 로봇 통신, IK 계산, motion execution policy는 아직 연결하지 않아 `App/Fairino` 경계로 새 책임이 새지 않았다.
- 구조
  - `PointMoveController`는 panel state/visibility/validation에 집중하고, 실기/mock motion 준비는 `RobotControlMotionRuntime`으로 밀어 역할 경계를 더 명확히 했다.
  - shell state 연동은 기존 `JointJog/TcpJog` 패턴을 그대로 따라가 재사용 흐름을 유지했다.
- 남은 리스크
  - `MoveL`은 runtime facade 경유 mock dispatch까지 붙었지만 `MoveJ`는 IK 연결 전이라 아직 보류 상태다.
  - 입력 validation은 preview/apply 기준으로 들어갔지만 실제 UI invalid 입력 smoke는 아직 남아 있다.
  - 좌표계별 hint 정교화는 아직 남아 있어 `2B-4`를 done으로 올리기엔 이르다.
  - `BottomTabPointMove` 실제 클릭 smoke는 닫혔고, restore 정책 검증은 후속으로 닫아야 한다.
