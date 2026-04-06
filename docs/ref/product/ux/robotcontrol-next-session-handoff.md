# RobotControl Next Session Handoff

## Goal
- `RobotControlShell`부터 시작하는 V2 구현을 바로 시작한다.
- 첫 구현 목표는 `TopStatusBar`와 authored-first 셸 구조를 고정하는 것이다.
- 구현은 전체 SimMachine 복제가 아니라 `조작 + 상태 + preview + 안전 안내` 중심으로 진행한다.
- `RobotControlV2` 시안 색/레이아웃 기준은 이제 `UIDesignTokens.RobotControlV2`를 SSOT로 본다.
- 새 세션에서는 이 문서와 SSOT 링크만 읽고 폴더 생성부터 시작한다.

## Last Updated
- 2026-04-01 (KST)

## Current V2 Shell Status
- `RobotControlV2`는 이제 old `RobotControlSceneCoordinator` 누적 경로가 아니라 별도 V2 composition root를 사용한다.
- `SceneBootstrap`, `RuntimeRoot`, `RobotRuntimeRoot`, `RobotControlShell/SafeArea/...` authored 구조가 scene 기준선으로 고정됐다.
- old FR5 메인 패널(`FairinoConnectionPanel`, `FairinoJointControlPanel`, `FairinoTcpControlPanel`, `FairinoStatePanel`, `FairinoWhyItMovedLabel`)은 V2 scene 계층에서 제거됐다.
- old shell 잔재(`RobotControlOverlay`, `TabBar`, `Tab_0`, `Tab_1`)도 V2 scene에서 제거됐다.
- `ExecuteAlways` 기반 edit-mode hierarchy mutation은 제거했다. authored 저장은 `RobotControlV2AuthoringBuilder` / `RobotControlV2SceneAuthoringBridge`가 책임진다.
- authored 레이아웃 보호는 `play 시작 1회 authored-lock`으로만 수행한다. runtime 중 지속적인 layout rewrite는 허용하지 않는다.
- `Always Start From Onboarding`는 유지한다. V2 검증은 `Onboarding -> RobotLibrary -> RobotControlV2` 진입 흐름에서 수행한다.
- `DiagnosticsDrawer`는 우상단이 아니라 우하단 debug-only anchor로 이동했다.
- `WorkTabBar`는 old 한 줄 overflow 구조가 아니라 3열 grid 구조로 정규화했다.
- `TopStatusBar`, `WorkTabBar`, `EasyMotionPanel`은 compact 기준을 추가해 `16:9`와 `4:3`에서 모두 버티도록 조정했다.
- `TcpJogPanel`, `JointJogPanel`, `PointMovePanel`, `TeachingPanel`은 authored child 구조까지 메모리 기준으로 새 V2 구조가 올라오도록 정리했다.
- `TcpJogPanel`, `JointJogPanel`, `PointMovePanel`, `TeachingPanel`의 내부 섹션 rect는 이제 scene-authored 값을 우선한다. 패널 스크립트는 authored 구조가 있으면 bind-only로 동작하고, authoring bridge는 더 이상 네 패널 내부를 freeze 값으로 다시 덮지 않는다.
- `RobotControlShellBinder`는 이미 존재하는 authored 부모 rect를 다시 stretch/anchor 하지 않고, 새로 만든 루트에만 기본값을 준다.
- `MoveConfirmDialog`, `WarningDialog`, `RecoveryDialog`, `TabletWorkSheet`, `TabletStatusSheet`, `TabletModuleSheet`도 authored shell 하위 구조로 추가했다.

## SSOT
- [fairino-simmachine-ia-map.md](./fairino-simmachine-ia-map.md)
- [robotcontrol-scene-hierarchy.md](./robotcontrol-scene-hierarchy.md)
- [robotcontrol-implementation-bridge.md](./robotcontrol-implementation-bridge.md)
- [robotcontrol-v2-naming-ssot.md](./robotcontrol-v2-naming-ssot.md)
- [robotcontrol-scene-authoring-contract.md](./robotcontrol-scene-authoring-contract.md)
- [robotcontrol-soft-teaching-pad-v1-backlog.md](../roadmap/robotcontrol-soft-teaching-pad-v1-backlog.md)
- [robotcontrol-soft-teaching-pad.md](./robotcontrol-soft-teaching-pad.md)

## Branch Strategy
- 문서는 메인 기준선으로 유지한다.
- 구현은 `codex/robotcontrol-shell` 브랜치에서 진행한다.
- 각 Phase는 `구현 -> 자기리뷰 -> unityctl 검증 -> 커밋` 순서를 강제로 지킨다.
- 미구현 탭은 숨기지 않고 `비활성 표시` 정책을 유지한다.
- 문서 업데이트는 종료 신호가 아니다. 문서 기준선을 갱신한 뒤에는 같은 세션에서 바로 다음 구현/검증 단위로 이어간다.
- 문서 업데이트가 발생한 턴에는 최소 `다음 실행 단위 1개`를 바로 진행한다.
- `RobotControlV2` 시안 조정은 로컬 상수가 아니라 `UIDesignTokens.RobotControlV2`를 통해서만 수정한다.

## Folder Structure To Create First
- `Assets/Scripts/App/Fairino/Connection`
- `Assets/Scripts/App/Fairino/Motion`
- `Assets/Scripts/App/Fairino/Teaching`
- `Assets/Scripts/App/Fairino/Template`
- `Assets/Scripts/App/Fairino/Shell`
- `Assets/Scripts/UI/RobotControl/Shell`
- `Assets/Scripts/UI/RobotControl/Connection`
- `Assets/Scripts/UI/RobotControl/Motion`
- `Assets/Scripts/UI/RobotControl/Teaching`
- `Assets/Scripts/UI/RobotControl/Status`
- `Assets/Scripts/UI/RobotControl/Help`
- `Assets/Scripts/UI/RobotControl/Popups`
- `Assets/Scripts/UI/RobotControl/Diagnostics`
- `Assets/Scripts/Visualization/RobotControl/Drivers`
- `Assets/Scripts/Visualization/RobotControl/Preview`
- `Assets/Scripts/Visualization/RobotControl/Overlays`
- 위 각 폴더에 `CLAUDE.md`를 추가한다.

## Execution Order
- `폴더/CLAUDE.md`
- `RobotControlShell`
- `TopStatusBar`
- `State contract`
- `Mock binding`
- `EasyMotion`
- `TcpJog`
- `JointJog`
- `PointMove`
- `StatusSummary`
- `Program 최소`
- `3D preview`
- `Live binding`
- `Tablet`

### 구현 시작점
- 첫 구현 대상은 `RobotControlShell`과 `TopStatusBar`다.
- 기존 런타임 로직은 최대한 유지하고, UI 셸과 패널 구조를 먼저 재조립한다.
- `UI 먼저`는 더미 화면만 먼저 만든다는 뜻이 아니다. 셸을 고정한 뒤 바로 상태 계약을 만들고, Mock 연결로 먼저 닫힌 루프를 만든다.
- 기존 `RobotControl`의 로봇 조작 코어는 재사용 가치가 높지만, 제품 목표 기준으로는 완성 상태가 아니다. 따라서 V2는 단순 리스킨이 아니라 `코어 재사용 + 상위 구조 재조립`으로 본다.
- Program 최소 버전은 `Points + Motion Sequence + Preview`만 포함한다.
- Status 최소 버전은 `상태 요약 + 최근 이벤트 + 세션 리포트`만 포함한다.
- 현재 `RobotControlV2`는 old 화면이 아니라 최소 셸 + 시안 레이아웃 상태다. 다음 구현은 이 셸 위에 실제 authored 패널을 붙이는 방향으로 간다.

## Locked Architecture Decisions
- `RobotControlV2`는 기존 `RobotControlSceneCoordinator`에 계속 분기를 늘리는 방식으로 완성하지 않는다.
- V2는 별도 composition root를 둔다. 기존 coordinator는 old `RobotControl` 유지와 공용 코어 재사용 경계까지만 책임진다.
- 기존 `FairinoConnectionService`, `IFairinoRobotClient`, `RobotKinematicsFacade`, visualization 코어는 최대한 재사용한다.
- 기존 `FairinoConnectionPanel`, `FairinoJointControlPanel`, `FairinoTcpControlPanel`, `FairinoStatePanel`은 직접 확장하지 않고 분해/대체 대상으로 본다.
- V2 패널은 각자 계산한 임시 상태를 소유하지 않는다. 상태 원천은 V2 상태 계약 한 곳으로 모은다.

## Locked State Contract
- V2 UI가 공통으로 기대하는 최소 상태는 아래 항목으로 고정한다.
  - connection state
  - enable state
  - controller mode
  - drag state
  - tool id / user id
  - fault / safety summary
  - speed preset / speed policy summary
  - current joint values
  - current TCP pose
  - current preview target
  - preview risk summary
  - last command / last recovery hint
- 패널별 파생 표현은 위 상태를 소비해 만들고, 패널 내부에서 별도 truth를 만들지 않는다.

## Do Not Pull Into V1
- `Initial` 상세 설정
- `Application`
- SimMachine식 `Coding / Graphical / Node Graph`
- SimMachine식 `Status Query`
- 산업 공정 패키지
- 고급 안전/시스템 설정

## Validation
- 새 세션 첫 루프:
  - `unityctl status --project $project --wait --json`
  - `unityctl check --project $project --type compile --json`
  - `unityctl console get-entries --project $project --limit 50 --json`
- Phase 공통 검증:
  - `compile`
  - 관련 `EditMode`
  - 필요 시 `play start`
  - 필요 시 `scene snapshot`
  - 필요 시 `screenshot capture`
  - 필요 시 `console get-entries`로 `UnityEngine.Input` legacy 예외와 `SceneNavigationBar` 노이즈를 확인한다.

## Latest Verify Notes
- `unityctl check --type compile` 통과
- `scene snapshot` 기준으로 `TopStatusBar` 루트는 `LeftCluster` / `RightCluster`만 남고, old 직계 자식은 제거됨
- `scene snapshot` 기준으로 `EasyMotionPanel`은 `Header`, `PresetGridTop`, `PresetGridBottom`, `ActionRow`, `InfoCard` 구조만 남음
- `scene snapshot` 기준으로 `WorkTabBar`는 `GridLayoutGroup`만 남고 `HorizontalLayoutGroup`은 제거됨
- `DiagnosticsDrawer` anchor는 `anchor(1,0)`, `anchoredPosition(-24,24)`로 고정
- `Onboarding` 강제 시작은 유지된다. `RobotControlV2` 검증도 direct play가 아니라 `Onboarding -> RobotLibrary -> RobotControlV2` 진입 흐름 기준으로 본다.
- `AuthorOpenScene()` 실행 후 메모리 기준 panel child는 아래처럼 정리된다.
  - `tcp=[Header, CoordinateRow, IncrementCard, AxisGrid, ActionRow, InfoCard]`
  - `joint=[Header, SingleAxisCard, MultiAxisCard, SummaryCard]`
  - `point=[Header, TargetCard, PoseGrid, ActionRow]`
  - `teaching=[Header, QuickActionRow, PointListCard, TpdCard, SummaryCard]`
- `scene snapshot` 기준으로 `BottomSheets`와 popup 3종의 authored child 계층도 확인했다.
- `RobotControlV2.unity`에 네 패널의 2차 spacing polish rect 조정값을 저장했고, 이후 패딩/간격 조정은 scene에서 직접 수행하는 것을 기준선으로 본다.
- `RobotControlShellBinderTests` EditMode 회귀 테스트를 추가해 authored `LeftRail -> WorkPanelHost -> JointJogPanel` rect가 binder 실행 뒤에도 유지되는 것을 검증했다.

## Next Unit
- `Onboarding -> RobotLibrary -> RobotControlV2` 실제 진입 흐름에서 authored-lock 유지 여부 확인
- `16:9` / `4:3` GameView 시각 점검
- `TcpJogPanel`, `JointJogPanel`, `PointMovePanel`, `TeachingPanel` 탭별 3차 micro polish
- panel section rect를 scene에서 직접 만질 때는 `robotcontrol-scene-authoring-contract.md`를 따른다

## Assumptions
- 새 세션은 Plan 없이 바로 구현에 들어간다.
- 이 문서는 기존 SSOT를 반복 설명하지 않고 링크와 실행 순서만 담는다.
- 새 세션 구현자는 이 문서 기준으로 폴더 구조를 먼저 만들고, 이후 셸부터 순차 구현한다.
