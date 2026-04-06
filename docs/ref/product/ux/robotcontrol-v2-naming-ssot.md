# RobotControlV2 Naming SSOT

## Purpose
- `RobotControlV2`의 UI 이름, authored 오브젝트 이름, 코드 심볼 이름, 사용자 표시 라벨을 한 표로 잠근다.
- 리네이밍 작업이 `구조적 재조립` 원칙을 깨지 않도록, 무엇을 지금 바꾸고 무엇을 alias로 남길지 기준을 제공한다.

## Parent Doc
- [robotcontrol-scene-hierarchy.md](./robotcontrol-scene-hierarchy.md)
- [robotcontrol-implementation-bridge.md](./robotcontrol-implementation-bridge.md)
- [fairino-simmachine-ia-map.md](./fairino-simmachine-ia-map.md)
- [robotcontrol-next-session-handoff.md](./robotcontrol-next-session-handoff.md)

## When To Read
- `RobotControlV2` 패널/탭/상태 카드 이름을 새로 정할 때
- scene authored 오브젝트 이름과 코드 심볼 이름을 맞출 때
- 사용자 표시 라벨과 개발용 식별자를 분리하고 싶을 때

## Last Updated
- 2026-04-01 (KST)

## Naming Rules
1. 코드 심볼은 영어 PascalCase를 사용한다.
2. scene authored 오브젝트 이름은 `바인딩 대상 컴포넌트`에 한해 코드 심볼과 1:1 또는 명확한 suffix 대응으로 맞춘다.
3. 사용자 표시 라벨은 한국어를 기본으로 한다.
4. `SceneId`, scene asset 이름, navigation display name은 runtime 안정성을 위해 분리해 관리한다.
5. old 이름은 바로 삭제하지 않고 `deprecated alias`로만 남긴다.
6. 패널이 `Jog`, `Move`, `Guide`, `Summary`, `Dialog`, `Sheet` 중 무엇인지 suffix로 역할을 드러낸다.
7. `WhyItMoved`는 내부 호환 alias로만 유지하고, 사용자-facing 이름은 `움직임 해설`로 통일한다.
8. 이 문서는 `현재 코드 truth`와 `목표 canonical 이름`을 함께 적되, 둘을 같은 값으로 취급하지 않는다.

## Canonical Table

| Layer | Canonical ID | Authored Object | Current Code Symbol | Target Code Symbol | Display Label | Deprecated Alias | Status | Notes |
|---|---|---|---|---|---|---|---|---|
| Scene | `RobotControlV2` | `RobotControlV2` scene asset | `SceneId.RobotControlV2` | same | `로봇 제어 V2` | `Robot Control V2` | locked-current | scene file / enum / nav display를 분리해 관리 |
| Shell | `RobotControlShell` | `RobotControlShell` | `RobotControlShell` | same | `로봇 제어 셸` | `RobotControlOverlay` | locked-current | V2 authored root |
| Top bar | `TopStatusBar` | `TopStatusBar` | `TopStatusBar` | same | `상단 상태 바` | `TopBar` | locked-current | 상단 상태/제어 영역 |
| Left rail | `LeftRail` | `LeftRail` | binder path only | binder path only | `작업 레일` | - | locked-structure | 구조 노드, 컴포넌트 rename 대상 아님 |
| Tab bar | `WorkTabBar` | `WorkTabBar` | `RobotControlWorkTabBar` | same | `작업 탭 바` | - | locked-current | 코드 타입만 `RobotControl*` prefix 유지 |
| Tab | `EasyMotion` | `EasyMotionPanel` | `RobotControlWorkTab.EasyMotion` | same | `쉬운 조작` | - | locked-current | 현재 정합성 양호 |
| Tab | `TcpJog` | `TcpJogPanel` | `RobotControlWorkTab.TcpJog` | same | `TCP 조그` | `Tcp` | locked-current | enum 리네이밍 반영 완료 |
| Tab | `JointJog` | `JointJogPanel` | `RobotControlWorkTab.JointJog` | same | `관절 조그` | `Joint` | locked-current | enum 리네이밍 반영 완료 |
| Tab | `PointMove` | `PointMovePanel` | `RobotControlWorkTab.PointMove` | same | `포인트 이동` | `Point` | locked-current | enum 리네이밍 반영 완료 |
| Tab | `Teaching` | `TeachingPanel` | `RobotControlWorkTab.Teaching` | same | `티칭` | - | locked-current | teaching point / sequence 최소 버전 |
| Status | `StatusSummary` | `StatusSummaryPanel` | `StatusSummaryPanel` | same | `상태 요약` | - | locked-current | right rail summary |
| Status | `MotionExplainer` | `WhyItMovedPanel` | `RobotControlWhyItMovedPanel` | `RobotControlMotionExplainerPanel` | `움직임 해설` | `FairinoWhyItMovedLabel`, `WhyItMoved` | target-later | 표시명은 먼저, 코드명은 후속 패스에서 전환 |
| Help | `RecoveryGuide` | `RecoveryGuidePanel` | `RecoveryGuidePanel` | same | `복구 안내` | - | locked-current | recovery copy surface |
| Help | `Help` | `HelpPanel` | `HelpPanel` | same | `도움말` | - | locked-current | contextual help |
| Debug | `Diagnostics` | `DiagnosticsDrawer` | `RobotControlDiagnosticsDrawer` or placeholder | same | `진단` | - | locked-current | debug-only surface |
| Popup | `MoveConfirm` | `MoveConfirmDialog` | `MoveConfirmDialog` | same | `이동 확인` | `MOVE CONFIRM` | locked-current | popup title 한국어화 반영 완료 |
| Popup | `Warning` | `WarningDialog` | `WarningDialog` | same | `경고` | `WARNING` | locked-current | popup title 한국어화 반영 완료 |
| Popup | `Recovery` | `RecoveryDialog` | `RecoveryDialog` | same | `복구 안내` | `RECOVERY` | locked-current | popup title 한국어화 반영 완료 |
| Popup | `FirstRunGuide` | `FirstRunGuideDialog` | binder path only | binder path only | `처음 실행 안내` | `FIRST RUN` | locked-current | shell staging 안내 한국어화 반영 완료 |
| Tablet | `TabletWorkSheet` | `TabletWorkSheet` | binder path only | binder path only | `작업 시트` | - | locked-structure | tablet work surface |
| Tablet | `TabletStatusSheet` | `TabletStatusSheet` | binder path only | binder path only | `상태 시트` | - | locked-structure | tablet status surface |
| Tablet | `TabletModuleSheet` | `TabletModuleSheet` | binder path only | binder path only | `모듈 시트` | - | locked-structure | tablet module surface |
| Viewport | `CenterViewport` | `CenterViewport` | binder path only | binder path only | `작업 공간` | `WORKSPACE` | locked-current | 3D 영역 표시 라벨 한국어화 반영 완료 |
| Viewport | `ViewportHeaderOverlay` | `ViewportHeaderOverlay` | binder path only | binder path only | `뷰포트 상단 도구` | - | locked-structure | overlay structure |
| Viewport | `ViewportFooterOverlay` | `ViewportFooterOverlay` | binder path only | binder path only | `뷰포트 하단 모듈` | - | locked-structure | overlay structure |
| Viewport | `ViewportHintLayer` | `ViewportHintLayer` | binder path only | binder path only | `뷰포트 안내` | - | locked-structure | help overlay |
| State | `RobotControlViewState` | n/a | `RobotControlViewState` | same | `V2 상태 계약` | - | locked-current | 상태 원천 SSOT |

## Status Key
- `locked-current`: 현재 코드와 목표 이름이 사실상 같아서 바로 기준으로 써도 된다.
- `locked-structure`: 구조 노드 이름이며 코드 타입 rename 대상으로 보지 않는다.
- `target-next`: 다음 구현 패스에서 바로 맞춘다.
- `target-later`: 연관 범위가 넓어 별도 migration 패스로 옮긴다.

## Copy Rules
- 사용자-facing 문자열은 한국어 우선으로 쓴다.
- 코드/로그/enum 이름에는 한국어를 넣지 않는다.
- 상태값 텍스트도 한국어 기준으로 맞춘다.
  - `Connected` -> `연결됨`
  - `Not connected` -> `연결 안 됨`
  - `Mock` -> `모의 연결`
  - `Preview gate enabled` -> `미리보기 안전 확인 사용 중`
  - `Why It Moved` -> `움직임 해설`
- placeholder 영문 제목 (`MOVE CONFIRM`, `WARNING`, `RECOVERY`, `FIRST RUN`, `WORKSPACE`)은 남기지 않는다.

## Rollout Order
1. display label과 placeholder copy를 먼저 한국어로 통일한다.
2. `RobotControlWorkTab` enum을 `TcpJog`, `JointJog`, `PointMove`로 맞춘다.
3. `WhyItMoved` 계열 코드를 `MotionExplainer` 기준으로 단계적 정리한다.
4. binder path와 scene authored 오브젝트 이름이 표와 같은지 `scene snapshot`으로 검증한다.
5. deprecated alias는 검색 가능한 주석 또는 migration note로만 남긴다.

## Same-Turn Sync
- 이 문서를 바꾼 턴에는 최소 아래 중 하나를 같이 갱신한다.
  - `robotcontrol-next-session-handoff.md`
  - `robotcontrol-implementation-bridge.md`
  - `docs/daily/MM-DD/*`
- `target-next` 항목을 코드에 반영한 턴에는 아래를 같이 확인한다.
  - `SceneCatalog` 표시명
  - `RobotControlWorkTab` enum
  - 관련 panel title / popup title / placeholder copy
  - `scene snapshot` 기준 authored object path

## Current Drift Found
- `MotionExplainer`는 표시 라벨만 먼저 한국어로 맞췄고, 코드 심볼과 authored path 정책은 아직 후속 migration 범위다.
- old legacy 경로인 `FairinoWhyItMovedLabel`, `RobotControlDiagnosticsDrawer`, `FairinoConnectionPanel`에는 영어 표시 문자열이 일부 남아 있다.
- `target-later` 항목은 alias 제거 시점과 authored path 유지 여부를 아직 잠가야 한다.

## Self Review
- 역할 경계: 이름 SSOT는 문서에 잠그고, 상태/토큰/레이아웃 SSOT와 섞지 않았다.
- SSOT 정합성: 구조는 `robotcontrol-scene-hierarchy`, 기능 분류는 `fairino-simmachine-ia-map`, 상태 원천은 `RobotControlViewState`, 시각 토큰은 `UIDesignTokens.RobotControlV2`로 분리했다.
- 리네이밍 위험: `SceneId.RobotControlV2`, scene asset 이름, authored object path는 바로 흔들지 않고 alias와 단계적 전환 원칙을 뒀다.
- 구현 친화성: `Current Code Symbol`과 `Target Code Symbol`을 분리해, 현재 truth와 목표안을 같은 값처럼 읽지 않도록 정리했다.
- 문서 일관성: 구조 노드와 컴포넌트 rename 대상을 `status` 열로 구분해 bridge/handoff 문서와 충돌을 줄였다.
- 남은 리스크: `WhyItMoved`를 `MotionExplainer`로 바꿀 때 기존 검색 습관과 문서 링크가 깨질 수 있으므로, 한 번에 삭제하지 말고 alias 기간을 둬야 한다.
