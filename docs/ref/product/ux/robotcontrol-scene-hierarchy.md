# RobotControl 씬 계층 구조 설계

## Purpose
- `RobotControl.unity`를 기존 로직을 버리지 않고 재조립하기 위한 씬 계층 구조를 정의한다.
- authored-first 방식으로 씬 셸을 새로 세울 때, 어떤 오브젝트를 만들고 무엇을 유지할지 기준점으로 사용한다.

## Parent Doc
- [robotcontrol-soft-teaching-pad.md](./robotcontrol-soft-teaching-pad.md)
- [tablet-first-policy.md](./tablet-first-policy.md)
- [fairino-simmachine-screen-structure-draft.md](../robots/fairino-simmachine-screen-structure-draft.md)

## When To Read
- `RobotControl.unity` authored 레이아웃을 다시 짤 때
- 기존 패널을 쪼개고 새 패널로 재배치할 때
- Desktop/Tablet 공용 씬 구조를 먼저 고정할 때

## Last Updated
- 2026-03-31 (KST)

## 설계 원칙
- 기존 `App/Fairino` 런타임 로직은 최대한 유지한다.
- 씬은 `SceneBootstrap`, `RuntimeRoot`, `RobotControlShell` 3축으로 재구성한다.
- authored-first를 유지하고, 런타임 생성은 fallback 경로로만 쓴다.
- 3D와 UI를 같은 계층에 섞지 않는다.
- Desktop/Tablet은 같은 씬 계층을 공유하고, 레이아웃 전환만 다르게 한다.

## 유지할 것
- `RobotControlSceneCoordinator`
- `IFairinoRobotClient`
- `FairinoConnectionService`
- `LiveFairinoClient`
- `FairinoUrdfJointDriver`
- `FrameGizmoFactory`
- `EETrailRenderer`
- `DisplacementArrow`
- `PresetTransitionAnimator`

## 새로 정리할 것
- `Canvas/RobotControlShell` 내부 구조
- 큰 패널을 작은 역할 패널로 분해
- Desktop/Tablet 레이아웃 전환 계층
- 팝업과 도움말 전용 계층

## 권장 씬 계층

```text
RobotControl.unity
├── SceneBootstrap
│   ├── RobotControlSceneCoordinator
│   ├── RobotControlLayoutCoordinator
│   ├── RobotControlModeState
│   └── RobotControlPopupCoordinator
│
├── RuntimeRoot
│   ├── RobotRuntimeRoot
│   │   ├── RobotModelRoot
│   │   │   └── FR5_UrdfInstance
│   │   ├── JointDriverRoot
│   │   │   └── FairinoUrdfJointDriver
│   │   ├── KinematicsRoot
│   │   ├── PreviewRoot
│   │   │   ├── TargetGhostRoot
│   │   │   ├── PredictedPathRoot
│   │   │   ├── RiskHighlightRoot
│   │   │   └── PreviewTargetMarkerRoot
│   │   ├── OverlayRoot
│   │   │   ├── FrameGizmoRoot
│   │   │   ├── DisplacementArrowRoot
│   │   │   ├── EndEffectorTrailRoot
│   │   │   └── JointHandleRoot
│   │   └── RuntimeDiagnosticsRoot
│   └── SessionRoot
│       ├── WaypointSequenceRoot
│       ├── PresetAnimatorRoot
│       └── ReportBufferRoot
│
├── Main Camera
│   ├── OrbitCameraController
│   └── RobotControlCameraAnchor
│
├── Directional Light
│
├── Canvas
│   └── RobotControlShell
│       ├── SafeArea
│       │   ├── TopStatusBar
│       │   ├── LeftRail
│       │   │   ├── GlobalNavPanel
│       │   │   ├── WorkTabBar
│       │   │   └── WorkPanelHost
│       │   ├── CenterViewport
│       │   │   ├── ViewportHeaderOverlay
│       │   │   ├── ViewportFooterOverlay
│       │   │   └── ViewportHintLayer
│       │   ├── RightRail
│       │   │   ├── StatusSummaryPanel
│       │   │   ├── WhyItMovedPanel
│       │   │   ├── RecoveryGuidePanel
│       │   │   └── HelpPanel
│       │   ├── BottomSheets
│       │   │   ├── TabletWorkSheet
│       │   │   ├── TabletStatusSheet
│       │   │   └── TabletModuleSheet
│       │   └── Popups
│       │       ├── MoveConfirmDialog
│       │       ├── WarningDialog
│       │       ├── RecoveryDialog
│       │       ├── FirstRunGuideDialog
│       │       └── ToastAnchor
│       └── DebugOnly
│           ├── DiagnosticsDrawer
│           └── LayoutBoundsOverlay
│
└── EventSystem
```

## UI 하위 계층 상세

### TopStatusBar
- 목적: 제조사 `Control area + Status bar`를 Desktop/Tablet 공용 상단 바에 압축
- 포함:
  - `TxtRobotName`
  - `TxtConnectionState`
  - `TxtLiveMode`
  - `BtnServoEnable`
  - `BtnRun`
  - `BtnStop`
  - `BtnPauseResume`
  - `BtnSync`
  - `BtnResetError`
  - `TxtSpeed`
  - `TxtMode`
  - `TxtToolUser`
  - `TxtFault`

### LeftRail
- 목적: 현재 작업에 필요한 조작만 모아두는 작업 레일
- 포함:
  - `GlobalNavPanel`
  - `WorkTabBar`
  - `WorkPanelHost`

### WorkPanelHost
- 포함:
  - `EasyMotionPanel`
  - `TcpJogPanel`
  - `JointJogPanel`
  - `PointMovePanel`
  - `TeachingPanel`

### CenterViewport
- 목적: 3D를 화면의 주인공으로 유지
- 포함:
  - 축/고스트/궤적/위험 토글
  - 하단 보조 모듈 바
  - 3D 힌트 오버레이

### RightRail
- 목적: 조작보다 `이해`와 `복구`를 담당
- 포함:
  - `StatusSummaryPanel`
  - `WhyItMovedPanel`
  - `RecoveryGuidePanel`
  - `HelpPanel`

### BottomSheets
- 목적: Tablet 전용 레이아웃 전환
- 규칙:
  - Desktop에서는 숨김
  - Tablet에서는 `LeftRail`, `RightRail` 일부를 여기로 이동

## 기존 컴포넌트 매핑

| 기존 컴포넌트 | 권장 새 위치 | 처리 방식 |
|---|---|---|
| `FairinoConnectionPanel` | `TopStatusBar` + 일부 `StatusSummaryPanel` | 분해 후 재사용 |
| `FairinoJointControlPanel` | `EasyMotionPanel`, `JointJogPanel`, 일부 `TeachingPanel` | 분해 |
| `FairinoTcpControlPanel` | `TcpJogPanel`, `PointMovePanel` | 분해 |
| `FairinoStatePanel` | `StatusSummaryPanel` | 축소/재배치 |
| `FairinoWhyItMovedLabel` | `WhyItMovedPanel` | 승격 |
| `RobotControlDiagnosticsDrawer` | `DebugOnly/DiagnosticsDrawer` | 유지 |
| `FairinoMoveConfirmDialog` | `Popups/MoveConfirmDialog` | 유지 |
| `FairinoRobotControlViewBuilder` | `RobotControlShell` 바인더 | 점진 교체 |

## 새로 추가할 스크립트 제안
- `RobotControlLayoutCoordinator.cs`
- `RobotControlModeState.cs`
- `RobotControlPopupCoordinator.cs`
- `EasyMotionPanel.cs`
- `TcpJogPanel.cs`
- `JointJogPanel.cs`
- `PointMovePanel.cs`
- `StatusSummaryPanel.cs`
- `RecoveryGuidePanel.cs`
- `HelpPanel.cs`

## Desktop / Tablet 레이아웃 규칙

### Desktop
- `LeftRail + CenterViewport + RightRail + ViewportFooterOverlay`
- 진단 서랍은 우측 오버레이

### Tablet
- `TopStatusBar + CenterViewport + BottomSheets`
- `LeftRail` 일부는 `TabletWorkSheet`
- `RightRail` 일부는 `TabletStatusSheet`

## authored-first 규칙
- 씬에는 최종 이름으로 authored 오브젝트를 만든다.
- 런타임은 `TryBindExistingLayout(...)`로 authored 오브젝트를 먼저 찾는다.
- 빠진 것만 fallback 생성한다.

## 마이그레이션 순서
1. `RobotControlShell` 새 authored 계층 생성
2. `TopStatusBar`, `LeftRail`, `CenterViewport`, `RightRail` 빈 구조 먼저 고정
3. `TryBindExistingLayout(...)`를 새 구조까지 읽도록 확장
4. 기존 큰 패널 기능을 새 하위 패널로 이동
5. Tablet용 `BottomSheets` 추가
6. 구형 패널 잔여물 정리

## 구현 판단
- 현재는 full rewrite보다 `구조적 재조립`이 맞다.
- 씬 계층은 새로 설계하되, 연결/상태/모션/시각화 코어는 살린다.
