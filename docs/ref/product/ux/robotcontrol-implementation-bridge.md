# RobotControl 구현 브리지

## Purpose
- [fairino-simmachine-ia-map.md](./fairino-simmachine-ia-map.md)의 정보구조와 [robotcontrol-scene-hierarchy.md](./robotcontrol-scene-hierarchy.md)의 씬 계층을 실제 구현 순서로 연결한다.
- 각 IA 항목이 씬의 어디로 가고, 어떤 기존 컴포넌트를 재사용하며, `unityctl`로 어떻게 검증할지 한 문서에서 보게 한다.

## Parent Doc
- [fairino-simmachine-ia-map.md](./fairino-simmachine-ia-map.md)
- [robotcontrol-scene-hierarchy.md](./robotcontrol-scene-hierarchy.md)
- [robotcontrol-v2-naming-ssot.md](./robotcontrol-v2-naming-ssot.md)
- [robotcontrol-scene-authoring-contract.md](./robotcontrol-scene-authoring-contract.md)
- [robotcontrol-soft-teaching-pad-v1-backlog.md](../roadmap/robotcontrol-soft-teaching-pad-v1-backlog.md)

## When To Read
- 실제 구현 착수 직전
- authored-first 계층을 어느 순서로 만들지 정할 때
- `unityctl` 검증 루프를 구현 단계별로 묶고 싶을 때

## Last Updated
- 2026-04-01 (KST)

## Current Bridge Status
- Phase 1 셸 고정은 V2 scene 기준으로 실질 완료 상태다.
- 현재 authored shell 기준선:
  - `TopStatusBar`
  - `LeftRail/GlobalNavPanel`
  - `LeftRail/WorkTabBar`
  - `LeftRail/WorkPanelHost/EasyMotionPanel`
  - `CenterViewport`
  - `RightRail/StatusSummaryPanel`
  - `RightRail/WhyItMovedPanel`
  - `RightRail/RecoveryGuidePanel`
  - `RightRail/HelpPanel`
  - `Popups`
  - `DebugOnly/DiagnosticsDrawer`
- old shell 잔재와 old FR5 panel authored 오브젝트는 V2 scene 계층에서 제거 완료로 본다.
- 현재 focus는 “shell 존재 여부”가 아니라 “Onboarding 경유 진입에서 authored-lock 유지 + compact layout polish”다.
- `TcpJogPanel`, `JointJogPanel`, `PointMovePanel`, `TeachingPanel`, popup 3종, `BottomSheets` authored child 구조는 구현 완료로 본다.
- 네 work panel은 이제 authored 내부 섹션 rect를 scene source of truth로 본다. bridge/builder는 패널 내부 rect를 더 이상 고정값으로 재배치하지 않고, 패널 스크립트는 authored 구조가 있으면 bind-only로 빠진다.
- `RobotControlShellBinder`는 authored 부모 rect를 덮지 않고 새로 만든 루트에만 기본 anchor/stretch를 준다.

## 사용 원칙
- 통합 IA는 `무엇을 만들지`
- 씬 계층 문서는 `어디에 둘지`
- 이름 기준 문서는 `무엇이라고 부를지`
- 이 문서는 `어떤 순서로 붙이고 검증할지`
- 기존 로직은 최대한 재사용하고, UI 셸과 패널 구조를 먼저 재조립한다
- `UI 먼저`는 shell-only mockup을 오래 끈다는 뜻이 아니다. 셸을 고정한 뒤 상태 계약을 만들고 Mock 연결로 먼저 루프를 닫는다.
- 기존 `RobotControl`의 MoveJ / MoveL / 연결 코어는 재사용하되, old panel composition은 그대로 들고 오지 않는다.
- Live 연결은 셸, 상태 계약, Mock 조작, preview 흐름이 먼저 닫힌 뒤 마지막에 붙인다.
- 이름 관련 해석이 헷갈리면 이 문서보다 먼저 `robotcontrol-v2-naming-ssot.md`의 `status` 열을 본다.

## 잠근 결정
- V2는 별도 composition root를 사용한다.
- old `RobotControlSceneCoordinator`는 기존 씬 유지와 공용 코어 재사용 경계까지만 책임진다.
- V2 패널은 각자 로봇 상태를 다시 계산하지 않는다.
- 상태 원천은 V2 상태 계약 하나로 모으고, 패널은 presenter/view model 형태로 그 상태를 소비한다.

## V2 상태 계약

### 공통 상태 필드
- `isConnected`
- `isEnabled`
- `controllerMode`
- `isDragMode`
- `toolId`
- `userId`
- `faultSummary`
- `safetySummary`
- `speedPreset`
- `speedPolicySummary`
- `currentJointValuesDeg`
- `currentTcpPose`
- `previewTarget`
- `previewRiskSummary`
- `lastCommandSummary`
- `lastRecoveryHint`

### 상태 계약 규칙
- 위 필드는 V2의 최소 공통 상태다. 패널별 계산은 허용하지만, 패널 내부에 별도 truth를 만들지 않는다.
- `TopStatusBar`, `Connection`, `Motion`, `Status`, `Help`, `Popups`, `Preview`는 모두 이 상태 계약을 기준으로 바인딩한다.
- `tool/user`, `fault/safety`, `current joints/TCP`의 원천은 기존 connection/runtime 코어를 재사용한다.
- preview 관련 상태만 V2 계층에서 새로 계산하고, 결과는 상태 계약으로 다시 모은다.

## 구현 매핑 표

| IA 항목 | 씬 위치 | 패널/오브젝트 이름 | 담당 스크립트 | 상태 | unityctl 검증 |
|---|---|---|---|---|---|
| 연결 홈 | `Canvas/RobotControlShell/SafeArea/TopStatusBar` | `TopStatusBar` | `RobotControlLayoutCoordinator`, 기존 `FairinoConnectionPanel` 분해 | 재사용+분해 | `scene snapshot`, `exec GameObject.Find`, `play start` |
| 상단 상태 | `TopStatusBar/RightStatusGroup` | `TxtSpeed`, `TxtMode`, `TxtToolUser`, `TxtFault` | `StatusSummaryPanel` 일부 + 기존 상태 바인딩 | 신규+재사용 | `play start`, `console get-entries` |
| 쉬운 조작 | `LeftRail/WorkPanelHost/EasyMotionPanel` | `BtnHome`, `BtnReady`, `BtnFolded`, `BtnZero` | `EasyMotionPanel` | 신규 | `ui snapshot`, `screenshot capture` |
| TCP 조그 | `LeftRail/WorkPanelHost/TcpJogPanel` | `TcpJogPanel` | `TcpJogPanel`, 기존 `FairinoTcpControlPanel` 분해 | 재사용+분해 | `play start`, `ui get`, `screenshot capture` |
| 단일축 조그 | `LeftRail/WorkPanelHost/JointJogPanel/SingleAxisJogSection` | `J1- / J1+ ...` | `JointJogPanel`, 기존 `FairinoJointControlPanel` 분해 | 재사용+분해 | `play start`, `console get-entries` |
| 다축 연계 | `LeftRail/WorkPanelHost/JointJogPanel/MultiAxisSection` | `JointSliderList` | `JointJogPanel` | 재사용+분해 | `play start`, `screenshot capture` |
| 포인트 이동 | `LeftRail/WorkPanelHost/PointMovePanel` | `PointMovePanel` | `PointMovePanel` | 신규 | `play start`, `ui get` |
| Program 최소 버전 | `LeftRail/WorkPanelHost/TeachingPanel` 또는 별도 탭 | `MotionSequencePanel` | 신규 `MotionSequencePanel` | 신규 | `play start`, `screenshot capture` |
| 3D 헤더 오버레이 | `CenterViewport/ViewportHeaderOverlay` | `BtnShowBaseAxes`, `BtnShowGhost` 등 | `RobotControlLayoutCoordinator` | 신규 | `scene snapshot`, `play start` |
| 3D 프리뷰 | `RuntimeRoot/RobotRuntimeRoot/PreviewRoot` | `TargetGhostRoot`, `PredictedPathRoot`, `RiskHighlightRoot` | 신규 preview 계층 + 기존 FK/visualization 코어 | 신규+재사용 | `play start`, `screenshot capture` |
| 하단 모듈 바 | `CenterViewport/ViewportFooterOverlay` | `BtnPointModule`, `BtnIOModule`, `BtnTPDModule` | `RobotControlLayoutCoordinator` | 신규 | `play start`, `screenshot capture` |
| 상태 요약 | `RightRail/StatusSummaryPanel` | `StatusSummaryPanel` | 신규 `StatusSummaryPanel`, 기존 `FairinoStatePanel` 축소 | 재사용+축소 | `play start`, `console get-entries` |
| Why It Moved | `RightRail/WhyItMovedPanel` | `WhyItMovedPanel` | 기존 `FairinoWhyItMovedLabel` 승격 | 재사용 | `play start`, `screenshot capture` |
| 복구 안내 | `RightRail/RecoveryGuidePanel` | `RecoveryGuidePanel` | 신규 | 신규 | `play start`, 오류 시나리오 |
| 도움말 | `RightRail/HelpPanel` | `HelpPanel` | 신규 | 신규 | `play start`, 상태 전환 확인 |
| 팝업 | `Popups/*` | `MoveConfirmDialog`, `WarningDialog`, `RecoveryDialog` | `RobotControlPopupCoordinator` + 기존 dialog 일부 | 신규+재사용 | `play start`, `screenshot capture` |
| Tablet 바텀시트 | `BottomSheets/*` | `TabletWorkSheet`, `TabletStatusSheet` | `RobotControlLayoutCoordinator` | 신규 | 해상도 변경 후 `screenshot capture` |

## 구현 순서

### Phase 1. 셸 고정
1. `RobotControlShell`
2. `SafeArea`
3. `TopStatusBar`
4. `LeftRail`
5. `CenterViewport`
6. `RightRail`
7. `Popups`

이 단계에서는 빈 authored 구조와 이름만 먼저 잠근다.

### Phase 1A. 셸 정리 / authored hygiene
1. old shell 잔재 제거
2. duplicated child 제거
3. `ExecuteAlways` 제거
4. authoring bridge / builder 경로로만 scene 저장
5. `DiagnosticsDrawer` debug-only anchor 고정
6. `WorkTabBar` overflow 방지 구조 고정

이 단계가 끝나면 scene snapshot 기준으로 old 잔재와 duplicated UI가 보이지 않아야 한다.

### Phase 2. 상태 계약 + Mock 바인딩
1. `RobotControlViewState`
2. `ConnectionSummaryState`
3. `MotionCommandState`
4. `PreviewState`
5. Mock 연결 경로

이 단계가 끝나면 실제 하드웨어 없이도 `연결 상태 -> Sync -> 쉬운 조작 -> 작은 MoveJ/MoveL`이 Mock 기준으로 닫혀야 한다.

### Phase 3. 필수 패널
1. `TopStatusBar`
2. `EasyMotionPanel`
3. `TcpJogPanel`
4. `JointJogPanel`
5. `PointMovePanel`
6. `StatusSummaryPanel`

이 단계가 끝나면 V1 핵심 조작이 뼈대 수준으로 성립해야 한다.

### Phase 3A. responsive polish
1. `TopStatusBar` compact width 규칙
2. `WorkTabBar` 3열 grid 규칙
3. `EasyMotionPanel` compact spacing / button height 규칙
4. `TcpJogPanel`, `JointJogPanel`, `PointMovePanel`, `TeachingPanel` compact 규칙 확장
5. `16:9` / `4:3` 공통 검증

이 단계가 끝나면 authored shell이 두 대표 비율에서 모두 읽히고, 버튼 overflow나 겹침이 없어야 한다.
현재 추가 메모:
- 2차 spacing polish는 scene rect 값으로 반영했다.
- 후속 조정은 bridge 숫자 수정보다 `RobotControlV2.unity` 직접 authoring을 우선한다.

### Phase 3B. authored-lock verification
1. `play 시작 1회 authored-lock` 훅 유지
2. `Onboarding -> RobotLibrary -> RobotControlV2` 실제 진입 경로에서 shell 위치 유지 확인
3. runtime 중 지속적인 layout rewrite 금지 확인

### Phase 4. 3D 차별화
1. `ViewportHeaderOverlay`
2. `TargetGhostRoot`
3. `PredictedPathRoot`
4. `RiskHighlightRoot`
5. `WhyItMovedPanel`

### Phase 5. UX 보강
1. `RecoveryGuidePanel`
2. `HelpPanel`
3. `MoveConfirmDialog`
4. `WarningDialog`
5. `RecoveryDialog`

### Phase 6. Live binding
1. `IFairinoRobotClient` live path attach
2. `Connect -> Enable -> Sync`
3. `small MoveJ`
4. `small MoveL`

이 단계에서야 실기 검증을 붙인다. `ServoJ` / `ServoCart`는 별도 단계 전까지 일반 V1 경로에 넣지 않는다.

### Phase 7. Tablet
1. `TabletWorkSheet`
2. `TabletStatusSheet`
3. `TabletModuleSheet`

## unityctl 활용 방식

### 1. 구조 확인
```powershell
$unityctl = 'C:\Users\ezen601\Desktop\Jason\unityctl\src\Unityctl.Cli\bin\Debug\net10.0\unityctl.exe'
$project = 'C:\Users\ezen601\Desktop\Jason\robotapp2'
& $unityctl scene open --project $project --name RobotControl
& $unityctl scene snapshot --project $project
```

### 2. authored-first 바인딩 확인
```powershell
& $unityctl exec --project $project --code "UnityEngine.Debug.Log(UnityEngine.GameObject.Find(\"Canvas/RobotControlShell/SafeArea/TopStatusBar\") != null)"
```

### 3. Play 검증
```powershell
& $unityctl play start --project $project
& $unityctl console get-entries --project $project
```

### 4. 시각 검증
```powershell
& $unityctl screenshot capture --project $project --output robotcontrol-ui.png
```

### 5. 회귀 검증
```powershell
& $unityctl check --project $project --type compile
& $unityctl test --project $project --mode edit
```

## 단계별 체크포인트

### Phase 1 Done
- `RobotControlShell` 하위 authored 오브젝트 이름이 고정됨
- `TryBindExistingLayout(...)` 확장 설계가 가능해짐
- old FR5 shell panel authored 오브젝트 제거
- old shell 잔재(`RobotControlOverlay`, `TabBar`) 제거
- duplicated child 제거
- `DiagnosticsDrawer`와 `TopStatusBar` 겹침 해소
- `WorkTabBar` red-X / overflow 원인 제거

### Phase 2 Done
- V2 패널이 기대하는 상태 구조가 고정됨
- Mock 기준 연결/상태/조작 루프가 닫힘

### Phase 3 Done
- 연결/상태/조작 핵심 플로우가 화면상 동작
- `연결 -> Sync -> Preview -> Move` 뼈대 성립

### Current Open Verify
- `Onboarding -> RobotLibrary -> RobotControlV2` 진입 흐름에서 authored-lock 유지 확인
- GameView `16:9` 실제 시각 점검
- GameView `4:3` 실제 시각 점검
- 탭별 단독 노출 기준 3차 micro polish
- scene 직접 수정 후 Play 진입에서도 authored 값이 유지되는지 수동 확인

### Phase 4 Done
- Unity다운 시각 차별화가 보이기 시작

### Phase 5 Done
- 초보자 친화 UX가 체감되기 시작

### Phase 6 Done
- Live 기준 `Connect -> Enable -> small MoveJ -> small MoveL` 검증이 시작됨

### Phase 7 Done
- Tablet에서도 같은 시나리오가 성립

## 결론
- 구현 순서는 `셸 -> 상태 계약/Mock -> 필수 패널 -> 3D 차별화 -> UX 보강 -> Live -> Tablet`이 가장 안전하다.
- `unityctl`은 구현 수단이 아니라 `구조/플레이/시각 회귀 검증 루프`로 쓰는 것이 가장 효율적이다.
