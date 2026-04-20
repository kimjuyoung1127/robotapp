# Pendant V3 Progress Checklist

## Purpose
- V3 티칭패드 구현 진행률을 한 문서에서 체크한다.
- 현재 완료/진행중/미착수 범위를 빠르게 확인한다.
- daily log와 달리 "지금 어디까지 왔는지"만 짧게 유지한다.

## Last Updated
- 2026-04-20 (KST)

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
| `2A-2` 상태/좌표 패널 | done | placeholder 정리 + context scroll 단일화 + desktop 텍스트 잘림 정리 완료 |
| `2B-1` 쉬운 조작 | in_progress | EasyMotion host/직행 기본상태 복구 완료, 패널 polish ongoing |
| `2B-2` 관절 조그 | in_progress | panel/controller/scene wiring + direct UITK tab-visible QA 완료, polish ongoing |
| `2B-3` TCP 조그 | in_progress | panel/controller/overlay wiring + direct UITK coord click QA 완료, polish ongoing |
| `2B-4` 포인트 이동 | done | MoveL/MoveJ dispatch 연결 + point draft 저장/버리기 + invalid-input/hidden-panel/empty-name guard 유지 확인 |
| `2C-1` 안전/진단 | in_progress | safety diagnostics panel/fault overlay scaffold + scene wiring + actual preview-state smoke 완료, action wiring/policy 연동은 후속 |
| `2C-2` 뷰포트 보조 UI | in_progress | viewport 전용 camera + render texture + overlay 분리까지 추가했고 clean render에서는 `RobotActual` 실노출 확인, overlay safe-area/no-fly-zone 마감은 후속 |
| `2D` 팝업/도움말 | in_progress | policy wiring + first-run guide 1회 노출 + help-panel popup/unsaved 연동 완료, actual button-path smoke 일부 후속 |
| `3A` binder / scene bootstrap | done | binder/coordinator scaffold + authoring/summary/play smoke + popup first-run serialization fix 완료 |
| `3A-1` context density quick relief | done | CoordStrip 접기/토글화 + UITK click smoke 완료 |
| `3A-2` status/safety rebalance | done | StatusCard 안전 요약 추가 + SafetyDiagnostics 정상 숨김 / fault 재노출 확인 |
| `3A-3` context panel tab split | done | 상태/좌표 탭 분리 + 우측 패널 scroll/overflow fix + visual smoke 완료 |
| `3B` 로컬 서비스 | in_progress | UI-local Undo/Redo 범위 고정 + point draft/local state 정리 + `FairinoConnectionService` 기반 reconnect adapter 연결 완료, live reconnect 실기 검증은 후속 |
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
- [x] `StatusCard / CoordStrip / ActionHint / WhyItMoved` 텍스트 잘림 visual smoke 확인
- [x] placeholder 잔여 텍스트 제거 최종 확인

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
- [x] `MoveJ` 실제 command dispatch 연결
- [x] `MoveJ`/`MoveL` 모드별 draft 분리 (`joint target` vs `TCP target`)
- [x] point draft 저장/버리기 + shell local state 연동
- [x] PointMove guard rail 5-lock 적용
  - 패널 비가시 상태에서 preview 실행 잠금
  - 패널 비가시 상태에서 apply 실행 잠금
  - 빈 포인트 이름 apply 잠금 + `PointNameInput` danger class
- [x] preview/apply 기준 raw input validation + ΔTCP summary 반영
- [x] 입력 validation / summary actual UI smoke 보강
- [x] `BottomTabPointMove` tablet smoke 확인
- [x] actual `BtnPointApply` enabled 상태 확인
- [x] debug bridge 기준 `MoveJ` dispatch smoke 확인
- [x] debug bridge 기준 `MoveL` dispatch smoke 재확인

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
- [x] 복구 버튼 action wiring 최소 policy 연결
- [x] fault close/reset 정책을 App 계층 policy와 연결

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
- [x] `PendantV3VisualizationState` / `PendantV3VisualizationOrchestrator` / `PendantV3VisualizationDriver` 추가
- [x] `SceneCameraDirector`에 `RobotControlV3` 카메라 프로필 추가
- [x] viewport summary 기준
  - `actualVisible=True`
  - `ghostVisible=False`
  - `cameraTarget=base_link`
  - `cameraFramed=True`
- [x] play game-view 기준 `ViewportHost` 안 `RobotActual` 실제 노출
  - clean render 기준 [v3-viewport-rendertexture-clean.png](C:/Users/ezen601/Desktop/Jason/robotapp2/Artifacts/v3-viewport-rendertexture-clean.png) 에서 중앙 뷰포트 내 실노출 확인
- [ ] overlay 포함 상태에서 toolbar no-fly-zone 100% 회피
  - 현재 [v3-viewport-safe-overlay-4.png](C:/Users/ezen601/Desktop/Jason/robotapp2/Artifacts/v3-viewport-safe-overlay-4.png) 기준으로는 툴바 뒤 일부 겹침이 남음
- [ ] viewport 툴바 compact/foldable 또는 레이아웃 재배치로 safe-area 최종 마감
- [ ] visualization 실데이터(경계 볼륨/충돌 세그먼트) 연동
- [ ] toolbar 토글을 `RobotControlViewState`/policy와 단일 소스로 통합

### `2D` 팝업/도움말 scaffold
- [x] `PopupCoordinatorV3.cs` 생성
- [x] `action-confirm.uxml` meta copy 분리
- [x] `action-reset-confirm.uxml` 생성
- [x] `action-run-confirm.uxml` 생성
- [x] `unsaved-confirm.uxml` meta copy 분리
- [x] `move-confirm.uxml` 생성
- [x] `warning-dialog.uxml` 생성
- [x] `recovery-dialog.uxml` 생성
- [x] popup copy literal을 runtime controller에서 asset 쪽으로 이동
- [x] hardcoding guard에 popup/viewport UI copy 검사 추가
- [x] popup policy wiring
  - `서보ON`
  - `오류초기화`
  - `실행`
  - `미저장 이탈`
  - `MoveJ/MoveL 실행 확인`
- [x] actual play popup smoke (`Escape/Enter`, focus trap, confirm/cancel) 1차 닫기
  - debug `warning` popup open -> title `정지 안내`
  - `BtnPopupConfirm` actual click -> `popupActive=False`, focus 복귀 확인
  - debug `move` / `recovery` popup open -> title/confirm text 확인
- [x] `help-panel.uxml` / `.uss` 생성
- [x] `HelpPanelController.cs` 생성
- [x] `WhyItMovedController.cs` 생성
- [x] `NavHelp` actual click -> help panel visible / work tab bar hidden
- [x] `WhyItMovedSummary` 별도 controller 전담으로 분리
- [x] `BottomTabHelp` tablet 진입 경로 authored 반영
- [x] `BottomTabHelp` actual tablet smoke
  - `HelpSheetHost` visible + childCount=1
  - `BottomSheetTitle=BottomSheet · 도움말`
  - `BottomTabTcpJog` 복귀 시 `BottomSheetTitle=BottomSheet · TCP`
- [x] help-panel 카피 1차 심화
  - preview state + coord/increment/speed 기반 안내 문구 보강
- [x] `first-run-guide` popup/도움말 연계
- [x] direct fresh-start actual first-run guide smoke
- [x] popup actual open 뒤 coordinator summary 확인 (`kind=FirstRunGuide`)
- [ ] help-panel 탭별 세분화 2차 polish

### `3B` 로컬 서비스
- [x] bottom `Undo/Redo`를 UI-local only 범위로 고정
  - `nav / work tab / tablet tab / coord / speed / increment / split ratio / point draft`
- [x] `FreshStart`는 UI-local state만 reset, `HasShownFirstRunGuide`는 유지
- [x] `ResumeLastSession`은 마지막 shell/point draft 상태 복원 유지
- [x] `PendantV3ConnectionSessionAdapter` / `PendantV3ConnectionSessionState` 추가
- [x] `ConnectionHome / StatusCard / SafetyDiagnostics / HelpPanel`이 같은 reconnect 상태를 읽도록 정리
- [x] debug bridge reconnect summary / lost / retry / failure / success trigger 추가
- [x] reconnect smoke
  - baseline `ConnectedServoOff`
  - lost trigger -> `AutoReconnect`
  - failure -> `Disconnected + reconnectFailed=True + 수동 연결 필요`
  - success -> `ConnectedServoOff` 복귀
- [ ] live hardware 기준 `OnConnectionLost` actual smoke
- [ ] motion history / pose replay는 이번 `3B` 범위에 넣지 않음

## Policy Checklist

- [x] Onboarding direct path -> `FreshStart`
- [x] RobotLibrary re-entry -> `ResumeLastSession`
- [x] RobotLibrary restore UX 플레이 검증

## Next Verification Loop

1. `RobotLibrary -> RobotControlV3` resume 경로에서 `first-run guide` 비노출 재확인
2. 같은 세션 direct re-entry에서 `first-run guide` 비재노출 actual smoke 재확인
3. `ViewportHost` overlay 포함 no-fly-zone 최종 회피
4. toolbar compact/foldable vs viewport 폭 재배치 중 하나로 잠금
5. live 장비 기준 `OnConnectionLost -> AutoReconnect -> success/failure` actual smoke
6. `2C-2` visualization 실데이터 연동 범위 잠금(경계/충돌 계산은 Visualization 소유 유지)
7. `help-panel / WhyItMoved` 카피 2차 심화
8. `3B` motion history slice 범위 분리 문서화

## Latest Test Result

- `unityctl check --type compile`: pass
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.LocalSettingsStoreTests`: `2 passed / 0 failed / 0 skipped`
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlEntryPolicyTests`: `3 passed / 0 failed / 0 skipped`
- `RobotControlMotionRuntimeTests`
  - 선택 로봇 없음 -> runtime 생성 실패: pass
  - `FAIRINO_FR5` 선택 -> mock `DispatchMoveL`: pass
  - `FAIRINO_FR5` 선택 -> mock `DispatchMoveJ`: pass
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlMotionRuntimeTests`: `3 passed / 0 failed / 0 skipped`
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.PendantV3ConnectionSessionAdapterTests`: `3 passed / 0 failed / 0 skipped`
- `dotnet build KineTutor3D.Runtime.csproj`: pass
- `V3 viewport / render texture`
  - clean render: [v3-viewport-rendertexture-clean.png](C:/Users/ezen601/Desktop/Jason/robotapp2/Artifacts/v3-viewport-rendertexture-clean.png)
  - safe framing clean: [v3-viewport-safe-framing.png](C:/Users/ezen601/Desktop/Jason/robotapp2/Artifacts/v3-viewport-safe-framing.png)
  - overlay split 1차: [v3-viewport-rendertexture-overlay.png](C:/Users/ezen601/Desktop/Jason/robotapp2/Artifacts/v3-viewport-rendertexture-overlay.png)
  - overlay safe-area 2차: [v3-viewport-safe-overlay-4.png](C:/Users/ezen601/Desktop/Jason/robotapp2/Artifacts/v3-viewport-safe-overlay-4.png)
  - 판정: `RobotActual`은 viewport 안 실노출 성공, toolbar 뒤 일부 겹침은 아직 남음
- `dotnet build KineTutor3D.Runtime.csproj`: pass
- `dotnet build KineTutor3D.Tests.EditMode.csproj`: pass
- `unityctl` Play 검증:
  - `Onboarding -> FR5 V3 바로 열기`: pass
  - direct path first-run guide actual open:
    - `PopupCoordinatorSummary`: `initialized=True`, `popupOpen=True`, `kind=FirstRunGuide`
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
  - `SetPointMoveMotionKindForDebug("MoveJ") -> ApplyPointMoveForDebug()`: pass
  - result: `[Dispatch] MoveJ 완료 · speed 30% · J1 0.0 / J2 -32.0 / J3 84.0`
  - reconnect adapter smoke
    - `GetConnectionSessionSummary()`: baseline `ConnectedServoOff`, `connected=True`, `enabled=False`
    - `TriggerConnectionLostForDebug()`: `AutoReconnect`, `reconnect=True`, `retryIn=3.0`
    - `CompleteReconnectForDebug(false)`: `Disconnected`, `reconnectFailed=True`, `수동 연결 필요`
    - `CompleteReconnectForDebug(true)`: `ConnectedServoOff` 복귀
    - `GetPanelControllerSummary()`: `Status/Safety/Home`가 같은 reconnect failure 문구 반영 확인
  - `PreviewPointMoveForDebug()`: pass
  - result: `[Preview] MoveJ 후보 · Point · J1 ...`
  - invalid-input smoke
    - `PointValueX="abc"` + actual `BtnPointApply` click -> `X 값 형식을 확인해라.` + `PointValueX` danger class 확인
    - `PointValueX="NaN"` + actual `BtnPointApply` click -> `X 값 형식을 확인해라.` + `PointValueX` danger class 확인
    - `PointValueRx="361"` + `ApplyPointMoveForDebug()` -> `RX 는 -360°~360° 범위 안으로 넣어라.` + `PointValueRx` danger class 확인
  - PointMove 5-lock smoke
    - hidden 상태 `PreviewPointMoveForDebug()` -> `포인트 이동 패널이 열려 있을 때만 미리보기를 실행한다.`
    - hidden 상태 `ApplyPointMoveForDebug()` -> `포인트 이동 패널이 열려 있을 때만 적용할 수 있다.`
    - visible + `PointNameInput=""` + actual `BtnPointApply` click -> `포인트 이름을 먼저 넣어라.` + `PointNameInput` danger class 확인
    - visible + `MoveJ/MoveL` 모두 `BtnPointApply` enabled 확인
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
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlV3HardcodingGuardTests`: `1 passed / 0 failed / 0 skipped`
- `unityctl check --type compile`: pass
- `AuthorSceneSafe()` + `GetPanelControllerSummary()`: pass
  - `coordinator=bootstrapped=True`
  - `binder=initialized=True; subscriptions=True`
- `CoordStrip` context density quick relief (`3A-1`)
  - `AuthorSceneSafe()` + `scene open RobotControlV3`: pass
  - `BtnCoordStripToggle` actual UITK click after `SceneNavigator.LoadByName("RobotControlV3")`: pass
  - toggle text `접기 -> 펼치기` 전환 확인
- `Status/Safety` rebalance (`3A-2`)
  - `StatusCard` summaryTitle=`정상 대기`, summaryBody 갱신 확인
  - `SafetyDiagnostics` normal 상태에서 `hostHidden=True` 확인
  - `BtnPresetFault` actual UITK click 후 `safety.hostHidden=False`, `overlayVisible=True`, `status.summaryTitle=Fault 복구 우선` 확인
  - `ConnectedServoOff` 복귀 후 `safety.hostHidden=True`, `status.summaryTitle=정상 대기` 재확인
- `ContextPanel` tab split + scroll fix (`3A-3`)
  - `GetPanelControllerSummary()`: `contextTabs mode=Status`, `mode=Coordinate` 전환 확인
  - `GetContextPanelScrollSummary()`: `viewportHeight=554.7`, `contentHeight=728.0~821.3`, bottom scroll offset 확인
  - `status-bottom-controlled-right.png`: `다음 행동 추천` 카드 본문 전체 노출 확인
  - `coordinate-bottom-controlled-verified-right.png`: `최근 조작 메모` 카드 제목/본문 노출 확인
- `play start` -> `console get-entries` -> `play stop`: pass
  - console 1건: `[unityctl] IPC connection error: Pipe closed before full message was read.`
- note: short-name 필터(`--filter RobotControlMotionRuntimeTests`)는 현재 `0 total`로 떨어져 신뢰도가 낮다.
- note: full EditMode 기준으로는 기존 red 묶음 외에 `MathReadinessPanelTests`/`OnboardingManagerTests`/`UIInventoryValidatorTests` 계열 실패가 같이 보였다.
- note: play 검증 콘솔에는 gameplay 에러 없이 `unityctl` IPC 재연결 로그만 반복 관측됐다.
- note: popup smoke 기준 `BtnPopupConfirm` actual click 뒤 `popupActive=False`, focus=`BtnPopupProbe` 복귀를 재확인했다.
- note: `NavHelp` actual click 기준 `HelpPanelHost` visible, `WorkTabBar` hidden, `WhyItMovedSummary` 갱신까지 확인했다.
- note: `BottomTabHelp` actual click 기준 `HelpSheetHost` visible, `BottomSheetTitle=BottomSheet · 도움말`, `BottomTabTcpJog` 복귀 시 `BottomSheetTitle=BottomSheet · TCP` 원복까지 확인했다.
- note: direct re-entry no-guide actual click은 play마다 onboarding button `globalObjectId`가 바뀌어서 같은 턴에 완전 자동화까지는 못 닫았다. 다만 entry policy test와 stored `firstRunGuide=True` 상태는 확인했다.
- note: reconnect smoke는 `RobotControlV3` active scene + debug bridge 경로로 닫았고, live 하드웨어 `OnConnectionLost` actual path는 아직 후속이다.
- note: V3 viewport는 summary상 `RobotActual/cameraTarget`이 정상처럼 보이지만, 실제 Game view 캡처에선 로봇이 비어 있어 렌더링 경로 불일치가 남아 있다.

## Source Docs

- [README.md](./README.md)
- [implementation-plan.md](./implementation-plan.md)
- [feature-jog-motion.md](./feature-jog-motion.md)
- [shell-layout.md](./shell-layout.md)
