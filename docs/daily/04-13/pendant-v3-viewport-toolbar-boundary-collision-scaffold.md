# Pendant V3 Viewport Toolbar Boundary Collision Scaffold

## Date
- 2026-04-13 (KST)

## Summary
- `2C-2` 최소 스코프로 뷰포트 보조 UI scaffold를 붙였다.
- `ViewportToolbarController`를 추가해 `viewport-toolbar` 토글 상태와 `ViewportHost` 클래스(`boundary/collision`)를 연결했다.
- `ConnectionHomeController.PreviewChanged`를 구독해 preview 상태(`Ready/Unsynced/Fault`)에 따라 충돌 강조 상태 텍스트와 버튼 잠금을 반영한다.
- 실제 충돌 계산/경계 메시는 아직 연결하지 않고, UI 표시와 정책 경계만 먼저 잠갔다.

## Updated Files
- `Assets/UI/PendantV3/viewport-toolbar.uxml`
- `Assets/UI/PendantV3/viewport-toolbar.uss`
- `Assets/UI/PendantV3/workspace-boundary.uss`
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/UI/PendantV3/CLAUDE.md`
- `Assets/Scripts/UI/RobotControlV3/ViewportToolbarController.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3Document.cs`
- `Assets/Scripts/UI/RobotControlV3/CLAUDE.md`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `docs/ref/product/pendant-v3/progress-checklist.md`

## Verification
- `unityctl status --project C:\Users\ezen601\Desktop\Jason\robotapp2 --wait --json`
  - Ready
- `unityctl exec --project ... --code "KineTutor3D.EditorTools.PendantV3SceneBuilder.AuthorSceneSafe()" --json`
  - `saved=True`
  - `GetDocumentRootComponentSummary()` 기준 `ViewportToolbarController` 포함 확인
- `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - pass
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlMotionRuntimeTests --json`
  - `2 passed / 0 failed / 0 skipped`
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --filter RobotControlMotionRuntimeTests --json`
  - `0 passed / 0 failed / 0 skipped` (`total=0`, short-name filter 신뢰도 낮음)
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --json`
  - `439 passed / 18 failed / 0 skipped` (`total=457`, 기존 red 묶음 유지)
- play smoke (`SceneNavigator.LoadByName("RobotControlV3")` + shell `NavHome`)
  - 초기 상태
    - `BtnViewportBoundary=경계 OFF`
    - `ViewportCollisionStatus=충돌 예측: 안전`
    - `ViewportHost` class=`rc-viewport-host`
  - `BtnViewportBoundary` actual click
    - `BtnViewportBoundary=경계 ON` + class `rc-workspace-toggle--active`
    - `ViewportBoundaryStatus=작업공간 경계: 표시`
    - `ViewportHost` class에 `rc-viewport-host--boundary` 추가
  - `BtnPresetFault` actual click
    - `ViewportCollisionStatus=충돌 예측: 위험 구간 감지 (자동 강조)`
    - status class=`rc-viewport-toolbar-status-line--danger`
    - `BtnViewportCollision` disabled + `충돌 ON`
    - `ViewportHost` class에 `rc-viewport-host--collision` 추가
  - `BtnPresetReady` actual click
    - `ViewportCollisionStatus=충돌 예측: 안전`
    - `BtnViewportCollision` enabled + `충돌 OFF`
    - `ViewportHost` class에서 `rc-viewport-host--collision` 해제
- play console
  - gameplay 에러는 없고 `unityctl` IPC 재연결 로그만 반복 관측

## Self Review
- 역할 경계
  - `ViewportToolbarController`는 표시 상태/클래스 토글만 담당하고, 충돌 계산/로봇 경계 데이터 생성은 하지 않는다.
  - `UI`가 `Visualization` 책임을 먹지 않도록 `scaffold` 수준으로 제한했다.
- 스코프 경계
  - `2C-2`에서 필요한 host/토글/상태표시까지만 반영했고, 실 데이터 연동은 다음 단위로 분리했다.
- 남은 리스크
  - 경계/충돌은 아직 UI 클래스 토글 기반이라 실제 로봇 geometry 기반 위험 판정은 미연동이다.
  - toolbar 토글 state를 최종적으로 `RobotControlViewState` 단일 소스로 통합해야 한다.
