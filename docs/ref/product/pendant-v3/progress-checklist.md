# Pendant V3 Progress Checklist

## Purpose
- V3 티칭패드 구현 진행률을 한 문서에서 체크한다.
- 현재 완료/진행중/미착수 범위를 빠르게 확인한다.
- daily log와 달리 "지금 어디까지 왔는지"만 짧게 유지한다.

## Last Updated
- 2026-04-14 (KST)

## Current Phase Snapshot

| Slice | Status | Notes |
|------|--------|------|
| `0A` 인프라 자산 | done | PanelSettings/TextSettings/SpriteAtlas/UIDocument |
| `0B` 루트 셸 | done | `pendant-v3.uxml` + `pendant-v3.uss` + V3 씬 |
| `0C` 입력/포커스 계약 | done | popup probe + 입력 계약 기본형 |
| `1A` Desktop 셸 | done | 기본 5영역 desktop 셸 |
| `1B` Tablet 셸 | done | tablet class + bottom sheet 구조 |
| `1C` 로컬 상태 | done | `PendantV3LocalState` + `LocalSettingsStore` |
| `2A-1` 연결 홈 | done | ConnectionHome 시안 + preview 상태 |
| `2A-2` 상태/좌표 패널 | in_progress | StatusCard/CoordStrip 완료, desktop polish ongoing |
| `2B-1` 쉬운 조작 | in_progress | EasyMotion host/직행 기본상태 복구 완료, 패널 polish ongoing |
| `2B-2` 관절 조그 | in_progress | panel/controller/scene wiring + direct UITK tab-visible QA 완료, polish ongoing |
| `2B-3` TCP 조그 | in_progress | panel/controller/overlay wiring + direct UITK coord click QA 완료, polish ongoing |
| `2B-4` 포인트 이동 | in_progress | 최소 scaffold + App motion runtime facade + desktop/tablet actual `BtnPointApply` MoveL dispatch 확인 완료, invalid-input smoke 완료, MoveJ hold UX lock 완료 / MoveJ dispatch pending |
| `2C-1` 안전/진단 | in_progress | safety diagnostics panel/fault overlay scaffold + scene wiring + actual preview-state smoke 완료, action wiring/policy 연동은 후속 |
| `2C-2` 뷰포트 보조 UI | in_progress | viewport toolbar + workspace boundary/collision visual scaffold 완료, visualization 실데이터/정책 연동은 후속 |
| `2D` 팝업/도움말 | pending | 미착수 |
| `3A` binder / scene bootstrap | pending | 미착수 |
| `3B` 로컬 서비스 | pending | Undo/Redo, autoreconnect 미착수 |
| `3C` mock e2e | pending | 미착수 |
| `4` V2 vs V3 평가 | pending | 미착수 |

## Done Checklist

- [x] V3 전용 `RobotControlV3.unity` 씬 생성
- [x] `PendantV3PanelSettings.asset` + `PendantV3TextSettings.asset`
- [x] `pendant-v3.uxml` / `pendant-v3.uss` 루트 셸
- [x] desktop/tablet layout class 전환
- [x] `PendantV3LocalState` / `LocalSettingsStore`
- [x] Connection Home 패널
- [x] StatusCard / CoordStrip 패널
- [x] EasyMotion 패널
- [x] Onboarding -> `FR5 V3 바로 열기` direct path
- [x] authored scene `previewMode=Desktop` 기본값 고정
- [x] direct path는 fresh, library re-entry는 resume 정책 고정

## In Progress Checklist

### `2A-2` / `2B-1` desktop polish
- [x] desktop에서 tablet class 오판정 제거
- [x] direct path stale local state 제거
- [x] WorkPanel / BottomSheet 헤더 슬롯 추가
- [x] 우측 `ActionHint` / `WhyItMoved` placeholder 교체
- [ ] `StatusCard / CoordStrip / ActionHint` 시각 완성도 최종 확인
- [ ] placeholder 잔여 텍스트 제거 최종 확인

### `2B-2` 관절 조그 kickoff
- [x] `joint-jog-panel.uxml` 생성
- [x] `joint-jog-panel.uss` 생성
- [x] `JointJogController.cs` 생성
- [x] desktop/tablet host 연결
- [x] SceneBuilder serialized template 연결
- [x] direct path 이후 debug shell selection으로 `TabJointJog` 상태 반영 확인
- [x] 관절 입력 `FocusIn -> SelectAll()` 확인
- [x] `uitk click` 기준 `TabJointJog` desktop visible 반영 확인

### `2B-3` TCP 조그 kickoff
- [x] `tcp-jog-panel.uxml` 생성
- [x] `tcp-jog-panel.uss` 생성
- [x] `cartesian-arrows-overlay.uxml` 생성
- [x] `cartesian-arrows-overlay.uss` 생성
- [x] `TcpJogController.cs` 생성
- [x] desktop/tablet host + viewport overlay host 연결
- [x] SceneBuilder serialized template 연결
- [x] debug shell selection으로 `TabTcpJog` 상태 반영 확인
- [x] `Tool` 좌표계 전환이 shell local state에 반영되는 것 확인
- [x] `X+` 조그 debug path로 overlay highlight + 값 증분 응답 확인
- [x] `exec invoke` 기준 `TabTcpJog` / `BtnTcpCoordTool` 클릭 smoke 확인
- [x] `uitk click` 기준 `TabTcpJog` / `BtnTcpCoordTool` 실제 클릭으로 `coord=Tool` 반영 확인

### `2B-4` 포인트 이동 scaffold
- [x] `point-move-panel.uxml` 생성
- [x] `point-move-panel.uss` 생성
- [x] `PointMoveController.cs` 생성
- [x] desktop/tablet host 연결
- [x] SceneBuilder serialized template 연결
- [x] `AuthorSceneSafe()` 기준 `PointMoveController` authored root 반영 확인
- [x] `uitk click` 기준 `TabPointMove` desktop visible 반영 확인
- [x] `MoveL` mock command dispatch debug 경로 연결
- [x] actual `BtnPointApply` click 기준 `MoveL` dispatch feedback 반영 확인
- [x] `PointMoveController -> RobotControlMotionRuntime` facade로 connect/enable/move 정책 분리
- [ ] `MoveJ` 실제 command dispatch 연결
- [x] `MoveJ` 보류 UX lock (`MoveJ` 상태에서 `BtnPointApply` 비활성 + 문구 명시)
- [x] PointMove guard rail 5-lock 적용
  - `MoveJ` 상태 apply 비활성 + `적용 (MoveJ 준비중)` 문구
  - `MoveL` 상태만 apply 활성
  - 패널 비가시 상태에서 preview 실행 잠금
  - 패널 비가시 상태에서 apply 실행 잠금
  - 빈 포인트 이름 apply 잠금 + `PointNameInput` danger class
- [x] preview/apply 기준 raw input validation + ΔTCP summary 반영
- [x] 입력 validation / summary actual UI smoke 보강
- [x] `BottomTabPointMove` tablet smoke 확인
- [x] actual `BtnPointApply` enabled 상태 확인

### `2C-1` 안전/진단 scaffold
- [x] `safety-diagnostics-panel.uxml` / `.uss` 생성
- [x] `fault-overlay.uxml` / `.uss` 생성
- [x] `SafetyDiagnosticsController.cs` 생성
- [x] shell host 연결 (`SafetyDiagnosticsHost`, `FaultOverlayHost`)
- [x] `PendantV3SceneBuilder` serialized template 연결
- [x] `ConnectionHomeController.PreviewChanged` 구독 기반 상태 반영
- [x] actual preview preset smoke
  - `Ready`: safe banner + fault overlay hidden
  - `Unsynced`: warning banner + fault overlay hidden
  - `Fault`: danger banner + fault overlay visible + fault summary 텍스트 확인
- [ ] 복구 버튼 action wiring (현재는 enable/label 상태 반영까지만)
- [ ] fault close/reset 정책을 App 계층 policy와 연결

### `2C-2` 뷰포트 보조 UI scaffold
- [x] `viewport-toolbar.uxml` / `.uss` 생성
- [x] `workspace-boundary.uss` 생성
- [x] `ViewportToolbarController.cs` 생성
- [x] shell host 연결 (`ViewportToolbarHost`)
- [x] `PendantV3SceneBuilder` serialized template 연결
- [x] `ViewportHost` boundary/collision 클래스 토글 scaffold 연결
- [x] preview 상태 기반 collision 위험 강조(ready/unsynced/fault) 반영
- [x] actual play smoke
  - `BtnViewportBoundary` click -> `경계 ON` + `작업공간 경계: 표시` + `ViewportHost`에 `rc-viewport-host--boundary`
  - `BtnPresetFault` click -> `충돌 예측: 위험 구간 감지 (자동 강조)` + `BtnViewportCollision` disabled `충돌 ON` + `ViewportHost`에 `rc-viewport-host--collision`
  - `BtnPresetReady` click -> `충돌 예측: 안전` + `BtnViewportCollision` enabled `충돌 OFF` + collision class 해제
- [ ] visualization 실데이터(경계 볼륨/충돌 세그먼트) 연동
- [ ] toolbar 토글을 `RobotControlViewState`/policy와 단일 소스로 통합

## Policy Checklist

- [x] Onboarding direct path -> `FreshStart`
- [x] RobotLibrary re-entry -> `ResumeLastSession`
- [x] RobotLibrary restore UX 플레이 검증

## Next Verification Loop

1. `unityctl check --type compile`
2. `Play -> Onboarding -> FR5 V3 바로 열기`
3. desktop 우측 `StatusCard / CoordStrip / ActionHint` 시각 완성도 최종 확인
4. placeholder 잔여 텍스트 제거 최종 확인
5. `PointMove` motion/apply 문구와 상호작용 polish
6. `PointMove` MoveJ hold UX (`Apply` disable + 라벨) 회귀
7. 필요 시 `RobotLibrary -> RobotControlV3` resume 회귀만 짧게 재확인
8. `2C-2` scaffold 이후 visualization 실데이터 연동 범위 잠금(경계/충돌 계산은 Visualization 소유 유지)
9. `2D` 팝업/도움말 최소 착수 전 위험 버튼/확인 팝업 스코프 재잠금

## Latest Test Result

- `unityctl check --type compile`: pass
- `RobotControlMotionRuntimeTests`
  - 선택 로봇 없음 -> runtime 생성 실패: pass
  - `FAIRINO_FR5` 선택 -> mock `DispatchMoveL`: pass
- `unityctl` Play 검증:
  - `Onboarding -> FR5 V3 바로 열기`: pass
  - `TabJointJog` actual UITK click -> desktop visible: pass
  - `TabTcpJog` actual UITK click -> desktop visible: pass
  - `BtnTcpCoordTool` actual UITK click -> `coord=Tool`: pass
  - `RobotLibrary -> InvokeOpenRobotControl("FAIRINO_FR5") -> RobotControlV3`: pass
  - `ResumeLastSession` 복원: `work=TabTcpJog`, `tablet=BottomTabTcpJog`, `coord=Tool`
  - `TabPointMove` actual UITK click -> `PointMoveController desktopVisible=True`: pass
  - `SetPointMoveMotionKindForDebug("MoveL") -> ApplyPointMoveForDebug()`: pass
  - result: `[Dispatch] MoveL 완료 · speed 30% · X -497.0 / Y -130.0 / Z 477.0`
  - actual `BtnPointMoveL` -> `BtnPointApply` click: pass
  - actual feedback: `[Dispatch] MoveL 완료 · speed 30% · X -497.0 / Y -130.0 / Z 477.0`
  - `BottomTabPointMove` -> `BtnPointMoveL` -> `BtnPointApply` click: pass
  - tablet feedback: `[Dispatch] MoveL 완료 · speed 30% · X -497.0 / Y -130.0 / Z 477.0`
  - `PreviewPointMoveForDebug()`: pass
  - result: `[Preview] MoveJ 후보 · 현재는 IK 연결 전`
  - invalid-input smoke
    - `PointValueX="abc"` + actual `BtnPointApply` click -> `X 값 형식을 확인해라.` + `PointValueX` danger class 확인
    - `PointValueX="NaN"` + actual `BtnPointApply` click -> `X 값 형식을 확인해라.` + `PointValueX` danger class 확인
    - `PointValueRx="361"` + `ApplyPointMoveForDebug()` -> `RX 는 -360°~360° 범위 안으로 넣어라.` + `PointValueRx` danger class 확인
  - MoveJ hold UX smoke
    - `SetPointMoveMotionKindForDebug("MoveJ")` 상태에서 actual `BtnPointApply` click 시도 -> `disabled in hierarchy` 확인
    - `BtnPointApply` text: `적용 (MoveJ 준비중)`
  - PointMove 5-lock smoke
    - hidden 상태 `PreviewPointMoveForDebug()` -> `포인트 이동 패널이 열려 있을 때만 미리보기를 실행한다.`
    - hidden 상태 `ApplyPointMoveForDebug()` -> `포인트 이동 패널이 열려 있을 때만 적용할 수 있다.`
    - visible + `PointNameInput=""` + actual `BtnPointApply` click -> `포인트 이름을 먼저 넣어라.` + `PointNameInput` danger class 확인
    - visible + `MoveL` + `BtnPointApply` enabled + text `적용` 확인
  - MoveL dispatch UX smoke
    - `SetPointMoveMotionKindForDebug("MoveL")` 상태에서 `BtnPointApply` text: `적용`, enabled 확인
  - actual `BtnPointMoveL` -> `BtnPointApply` click (FR5 selection) -> `[Dispatch] MoveL 완료 · speed 30% · X -497.0 / Y -130.0 / Z 477.0`
  - safety diagnostics scaffold smoke (`2C-1`)
    - `AuthorSceneSafe()` 후 play + `SceneNavigator.LoadByName("RobotControlV3")`: pass
    - shell selection `NavHome` 기준 preset 클릭으로 상태 전환:
      - `BtnPresetUnsynced`: `SafetyBannerText=안전 상태: 주의 · 동기화/재연결 확인`, banner class=`rc-safety-banner--warning`, `FaultOverlayHost` hidden 유지
      - `BtnPresetFault`: `SafetyBannerText=안전 상태: Fault 감지 · 조작 잠금`, banner class=`rc-safety-banner--danger`, `FaultOverlayHost` visible, `FaultOverlaySummary=코드 F203 · Safety 정지`
      - `BtnPresetReady`: banner class=`rc-safety-banner--safe`, `SafetyBannerText=안전 상태: 정상`, `FaultOverlayHost` hidden 복귀
  - viewport helper scaffold smoke (`2C-2`)
    - initial: `BtnViewportBoundary=경계 OFF`, `ViewportCollisionStatus=충돌 예측: 안전`, `ViewportHost` 기본 class 유지
    - `BtnViewportBoundary` click: `경계 ON`, `작업공간 경계: 표시`, `ViewportHost` class=`rc-viewport-host--boundary`
    - `BtnPresetFault` click: `ViewportCollisionStatus=충돌 예측: 위험 구간 감지 (자동 강조)`, status class=`rc-viewport-toolbar-status-line--danger`, `BtnViewportCollision` disabled + `충돌 ON`, `ViewportHost` class에 `rc-viewport-host--collision`
    - `BtnPresetReady` click: `ViewportCollisionStatus=충돌 예측: 안전`, `BtnViewportCollision` enabled + `충돌 OFF`, `ViewportHost`에서 `rc-viewport-host--collision` 해제
- `unityctl test --mode edit`: `439 passed / 18 failed / 0 skipped` (`total=457`)
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlMotionRuntimeTests`: `2 passed / 0 failed / 0 skipped`
- note: short-name 필터(`--filter RobotControlMotionRuntimeTests`)는 현재 `0 total`로 떨어져 신뢰도가 낮다.
- note: full EditMode 기준으로는 기존 red 묶음 외에 `MathReadinessPanelTests`/`OnboardingManagerTests`/`UIInventoryValidatorTests` 계열 실패가 같이 보였다.
- note: play 검증 콘솔에는 gameplay 에러 없이 `unityctl` IPC 재연결 로그만 반복 관측됐다.

## Source Docs

- [README.md](./README.md)
- [implementation-plan.md](./implementation-plan.md)
- [feature-jog-motion.md](./feature-jog-motion.md)
- [shell-layout.md](./shell-layout.md)
