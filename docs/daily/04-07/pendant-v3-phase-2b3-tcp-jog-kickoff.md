# Pendant V3 Phase 2B-3 TCP Jog Kickoff

## Date
- 2026-04-07 (KST)

## Summary
- `2B-3` TCP 조그 첫 슬라이스를 V3 셸에 추가했다.
- desktop/tablet 양쪽 패널 host와 `ViewportHost`용 3D 방향 조작 오버레이 host를 같이 연결했다.
- `Base / Tool / User` 좌표계 선택이 V3 shell local state와 같이 움직이도록 묶었다.
- `X/Y/Z/RX/RY/RZ` 증분 버튼과 3D 오버레이 버튼을 같은 controller에서 다루게 해, 어느 쪽에서 눌러도 같은 축 강조 상태를 쓰게 했다.

## Updated Files
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/UI/PendantV3/tcp-jog-panel.uxml`
- `Assets/UI/PendantV3/tcp-jog-panel.uss`
- `Assets/UI/PendantV3/cartesian-arrows-overlay.uxml`
- `Assets/UI/PendantV3/cartesian-arrows-overlay.uss`
- `Assets/UI/PendantV3/CLAUDE.md`
- `Assets/Scripts/UI/RobotControlV3/TcpJogController.cs`
- `Assets/Scripts/UI/RobotControlV3/TcpJogController.Elements.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3Document.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.State.cs`
- `Assets/Scripts/UI/RobotControlV3/CLAUDE.md`
- `Assets/Scripts/App/RobotControlV3DebugBridge.cs`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `Assets/Scenes/RobotControlV3.unity`
- `docs/ref/product/pendant-v3/README.md`
- `docs/ref/product/pendant-v3/progress-checklist.md`

## What Changed
- `pendant-v3.uxml`
  - `TcpJogPanelHost`, `TcpJogSheetHost`, `CartesianArrowsOverlayHost`를 추가했다.
  - 기존 `TabTcpJog`/`BottomTabTcpJog` 상태가 실제 host visibility까지 이어지게 준비했다.
- `tcp-jog-panel.uxml` / `.uss`
  - `Base / Tool / User` 좌표계 선택 row
  - `X/Y/Z` 직선 이동 row
  - `RX/RY/RZ` 회전 이동 row
  - 증분/속도/오버레이 요약 카드
  - `MoveL`용 액션 버튼 row
- `cartesian-arrows-overlay.uxml` / `.uss`
  - 뷰포트 우하단 고정 오버레이
  - XYZ와 RX/RY/RZ 축별 `- / +` 버튼
  - 현재 좌표계 badge
  - 마지막 조작 축 요약
- `TcpJogController`
  - desktop/tablet panel clone + viewport overlay clone을 모두 소유한다.
  - `ConnectionHomeController.PreviewChanged`를 구독해 초기 TCP 값을 받아온다.
  - 축 버튼 조작 시 panel row highlight와 overlay axis highlight를 같이 갱신한다.
  - 좌표계 선택 시 `PendantV3ShellStateController.SetCoordSystemSelection(...)` 경로로 shell state와 같이 업데이트한다.
- shell/bootstrap/debug wiring
  - `PendantV3Document` 초기화 루프에 `TcpJogController`를 추가했다.
  - `PendantV3SceneBuilder`가 새 template asset과 controller를 scene에 author 하도록 확장했다.
  - `RobotControlV3DebugBridge`에 `GetTcpJogControllerSummary`, `SetTcpCoordSystemForDebug`, `NudgeTcpAxisForDebug`를 추가했다.
  - `PendantV3ShellStateController`가 coord/increment/speed 변경 시 TCP panel도 같이 다시 그리도록 notify 범위를 넓혔다.

## Verification
- `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - pass
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --json`
  - `436 passed / 18 failed / 0 skipped`
  - 실패 18건은 기존 red 묶음 유지
- `unityctl exec --project ... --code "KineTutor3D.EditorTools.PendantV3SceneBuilder.AuthorSceneSafe()" --json`
  - `saved=True`
- `unityctl play start --project ... --json`
  - play start 확인
- `unityctl ui find --project ... --name 'BtnOpenRobotControlV3' --include-inactive --json`
  - 온보딩 CTA 존재 확인
- `unityctl ui click --project ... --id <BtnOpenRobotControlV3 globalObjectId> --mode play --json`
  - `Onboarding -> RobotControlV3` direct path 확인
- `unityctl exec --project ... --code "KineTutor3D.App.SceneCatalog.GetCurrentSceneId()" --json`
  - `7`
- `unityctl exec --project ... --code 'KineTutor3D.App.RobotControlV3DebugBridge.SetShellSelection("NavMotion", "TabTcpJog", "BottomTabTcpJog")' --json`
  - shell state를 TCP 탭으로 강제 전환
- `unityctl exec --project ... --code "KineTutor3D.App.RobotControlV3DebugBridge.GetPanelControllerSummary()" --json`
  - `tcp=[initialized=True; desktopVisible=True; tabletVisible=True; coord=Base; ... overlayHidden=False ...]`
- `unityctl exec --project ... --code 'KineTutor3D.App.RobotControlV3DebugBridge.SetTcpCoordSystemForDebug("Tool")' --json`
  - `coord=Tool`
- `unityctl exec --project ... --code 'KineTutor3D.App.RobotControlV3DebugBridge.NudgeTcpAxisForDebug("X", 1)' --json`
  - `activeAxis=X+`
  - `x=-492.0`
- `unityctl exec --project ... --code "KineTutor3D.App.RobotControlV3DebugBridge.GetShellControllerSummary()" --json`
  - `coord=Tool`
- `unityctl exec list-callables --project ... --filter 'RobotControlV3DebugBridge' --json`
  - `ClickVisualButtonForDebug` callable surface 노출 확인
- `unityctl exec invoke --project ... --type 'KineTutor3D.App.RobotControlV3DebugBridge' --method 'ClickVisualButtonForDebug' --args '["TabTcpJog"]' --json`
  - `button=TabTcpJog; found=True; action=work-tab`
- `unityctl exec invoke --project ... --type 'KineTutor3D.App.RobotControlV3DebugBridge' --method 'ClickVisualButtonForDebug' --args '["BtnTcpCoordTool"]' --json`
  - `button=BtnTcpCoordTool; found=True; action=coord`
- `unityctl uitk find --project ... --name 'TabTcpJog' --json`
  - UITK 탭 locator 검색 성공
- `unityctl uitk find --project ... --name 'BtnTcpCoordTool' --json`
  - desktop/tablet 쪽 TCP 좌표계 버튼 locator 검색 성공

## Self Review
- 역할 경계
  - UI Toolkit 패널/오버레이 구성과 shell local state만 건드렸고, 실제 로봇 이동 로직이나 Visualization 계산 계층은 건드리지 않았다.
- 구조
  - `TcpJogController`는 300줄 경계를 넘지 않게 `Elements` partial로 쪼갰다.
  - `PendantV3ShellStateController`는 새 panel을 알리는 최소 notify 확장만 넣었다.
- 남은 리스크
  - 현재 `2B-3`는 UI first slice라서 실제 MoveL/live robot 연결은 아직 없다.
  - `StatusCard/CoordStrip`의 좌표계 표시까지 shell local state를 완전히 따라가게 다듬는 후속 polish가 남아 있다.
  - debug bridge summary는 `ForceInitialize()` 경유라 preview base 값으로 다시 스냅될 수 있어, 조그 누적 상태를 정밀 추적하는 probe는 별도 개선 여지가 있다.
  - `unityctl uitk click`는 이번 로컬 재검증에서 play-mode gating 판정이 흔들려 아직 안정 smoke로 보긴 어렵다. 현재는 `uitk find` + `exec invoke` 경로가 더 신뢰된다.
