# Pendant V3 Progress Checklist

## Purpose
- V3 티칭패드 구현 진행률을 한 문서에서 체크한다.
- 현재 완료/진행중/미착수 범위를 빠르게 확인한다.
- daily log와 달리 "지금 어디까지 왔는지"만 짧게 유지한다.

## Last Updated
- 2026-04-22 (KST)

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
| `2A-2` 상태/좌표 패널 | done | StatusCard/CoordStrip + detail routing + actual click matrix 통과 |
| `2B-1` 쉬운 조작 | done | EasyMotion + Zero preset + actual click matrix 통과 |
| `2B-2` 관절 조그 | done | Joint jog preview/apply/restore + actual click matrix 통과 |
| `2B-3` TCP 조그 | done | TCP jog + Cartesian arrows + actual click matrix 통과 |
| `2B-4` 포인트 이동 | in_progress | MoveL/MoveJ preview/apply, save/recall/list/delete/rename/export/cleanup 연결 완료. Numerical IK live는 safety gate에서 차단 |
| `2C-1` 안전/진단 | done | safety/fault actual flow `5/5 PASS`, fault overlay popup route 확인 |
| `2C-2` 뷰포트 보조 UI | in_progress | toolbar/frame/path/ghost/bound/coll/cam actual click matrix 통과. 실데이터 boundary/collision은 후속 |
| `2D` 팝업/도움말 | done | popup confirm/cancel E2E `10/10 PASS`, status detail help routing 확인 |
| `3A` binder / scene bootstrap | done | binder/coordinator scaffold + authoring/summary/play smoke 완료 |
| `3A-1` context density quick relief | done | CoordStrip 접기/토글화 + UITK click smoke 완료 |
| `3A-2` status/safety rebalance | done | StatusCard 안전 요약 추가 + SafetyDiagnostics 정상 숨김 / fault 재노출 확인 |
| `3A-3` context panel tab split | done | 상태/좌표 탭 분리 + 우측 패널 scroll/overflow fix + visual smoke 완료 |
| `3B` 로컬 서비스 | in_progress | Undo/Redo/Step 기본 preview history는 연결, product live confirm token 완료, manual readback simulation gate `6/6 PASS`, `PendantV3Points` sequence execution 후속 |
| `3C` mock e2e | done | Desktop actual click `95/95 PASS`, tablet/bottom representative `16/16 PASS`, popup/safety/point/live-readback/live-command gate artifacts 생성 |
| `4` V2 vs V3 평가 | pending | 미착수 |

## 2026-04-20 Viewport Note

- 오늘 viewport 관련 실험은 **채택 안 함**으로 잠근다.
- 현재 기준선은 `8549b09`이며, 이 기준선 자체에 `MainSplitHandle + ViewportHost` 별도 패널 구조가 이미 포함돼 있다.
- 즉 오늘 화면에서 계속 보이던 별도 `ViewportHost`는 "오늘 수정 잔재"가 아니라 **현재 기준선 원구조**다.
- 오늘 시도한 `ViewportHost 내장`, `RT/오버레이 분리`, `툴바 패널 재배치`, `2패널 분리` 실험은 모두 rollback 대상으로 간주한다.
- 다음 세션에서는 구현 전에 먼저 아래를 문서로 잠근 뒤 시작한다.
  - 로봇을 **어느 패널에 표시할지** 1회 확정
  - `ViewportHost`를 유지할지 제거할지 1회 확정
  - `Base축 / Tool축 / 궤적` 패널이 어느 레벨에서 분리될지 1회 확정

## 2026-04-20 Display Lock

- 현재 하이라이트된 `WorkPanel`을 **로봇 표시 핵심 패널**로 확정한다.
- `ViewportHost`는 메인 로봇 표시 패널이 아니다. 1차 구현에서는 보조/유틸 영역으로만 취급한다.
- `WorkPanel` 내부는 `RobotStage` 단일 책임으로 잠근다.
  - `RobotStage`: 로봇 메시 + 프레임 + 고스트 + 트레일 + 바닥 격자 + 선택 XYZ 기즈모
- 현재 선택 탭의 조작 UI는 `ViewportHost`의 보조 작업 패널로 이동한다.
- `TCP 3D 화살표`, 특히 `Z / RX / RY / RZ` 조작은 로봇을 가리지 않게 `ViewportHost` 보조 패널에서만 노출한다.
- `ViewportHost`는 상단 보조 툴바 + 공용 `ScrollView` 구조로 유지한다.
- `관절 / 쉬운조작 / TCP / 포인트 이동`은 모두 같은 `RobotStage`를 공유하고, 보조 패널 쪽 내용만 교체한다.
- 다음 구현 단위는 `고스트 / predicted path / 경계 / 충돌`을 실제 시각 데이터와 연결하는 것이다.

## 2026-04-21 Aux Compact Lock

- 가로 스크롤은 복구하지 않는다.
- 보조패널/오른쪽 패널 버튼 잘림은 내부 요소 compact/wrap으로 해결한다.
- `ViewportPanelScroll`과 `ContextPanelScroll`은 세로 전용으로 유지한다.
- TCP/Cartesian 조작행은 `축+값+단위`와 `- / +`를 2줄로 분리했다.
- Joint 조작행은 `J축+입력+값`, `슬라이더`, `- / +` 버튼 행으로 분리했다.
- Point/Easy/Coord/Status/Safety 카드도 `min-width: 0`, `max-width: 100%`, wrap/compact 기준으로 맞췄다.
- 재시작 후 `GetAuxLayoutSummaryForDebug()` 기준 `horizontalVisible=False`, `clipped=0`, `scrollShare>=0.88`을 확인했다.

## 2026-04-21 Button Integration SSOT

- `robot-button-integration-plan.md`를 버튼-로봇 연동 기준 문서로 추가했다.
- 모든 V3 조작 버튼은 `wired / partial / stub / pending / excluded` 상태로 비교한다.
- 현재 high-priority gap은 `Program Run/Step queue`, `Point MoveJ production IK policy`, `Boundary/Collision warning-only future`이다.
- 실기기 연동 전 필수 gate는 `manual readback -> RobotStage -> 값 표시 -> 포인트 저장 -> DryRun replay` Unity/Mock 시뮬레이션이다.
- Teaching sequence v1 잠금: 저장은 readback 기준, `Step▶/Step◀`는 preview only, `Run`은 pending preview 우선 후 sequence run, 순서 변경은 위/아래 버튼, point name은 unique key로 본다.
- `GetMovementStateSummaryForDebug()`, `Zero preset`, `CoordStrip mode`는 1차 연결 완료했다.
- Easy/Joint/TCP/Cartesian 대표 전후 state matrix와 Point MoveL DryRun preview/apply까지 확인했다.
- Joint preview target이 runtime snapshot `JointValues`로 전달되게 수정해서 보조패널 row와 로봇 preview 상태가 같은 값을 본다.
- Point MoveL은 disconnected DryRun에서도 preview/apply 가능하도록 게이트를 맞췄다.
- Point MoveJ는 FK 기반 numerical XYZ IK로 preview/dry-run apply 1차 경로를 연결했다.
- Point 저장/호출은 `WaypointStore` 기반으로 연결했고, recall된 point의 saved joint target을 MoveJ에 우선 사용한다.
- Point list/select/delete 최소 UX를 연결했다.
- Point rename/export/persistence cleanup을 연결했다.
- I/O/Gripper mock/live-gated state facade 1차 연결을 완료했다.
- PGEA attached visual prefab 이관/연결을 완료했다.
- live SDK gripper capability/readback scaffold를 연결했다.
- Desktop actual UI click matrix `95/95 PASS`.
- Tablet/bottom representative actual click matrix `16/16 PASS`.
- Popup confirm/cancel E2E `10/10 PASS`.
- Safety/Fault actual flow `5/5 PASS`.
- Point MoveJ production guard matrix `6/6 PASS`.
- RobotStage screenshot evidence 3장 생성.
- Live SDK readback gate 생성: `readbackOk=True`, live command는 operator safety confirm 전까지 차단.
- Live command safety gate matrix `12/12 PASS`.
- Product live confirm token matrix `4/4 PASS`.
- `Run/Move` 확인 팝업에서 DryRun은 승인 생략, non-DryRun은 1회성 token 표시 후 확인 시 live gate 승인으로 승격한다.
- Manual readback teaching simulation matrix `6/6 PASS`.
- Mock readback이 `FairinoConnectionService.OnStateUpdated` 경로를 타고 RobotStage/좌표/포인트 저장/recall에 반영되는 것을 확인했다.
- live motion은 manual readback simulation, product confirm, production IK policy가 준비될 때까지 gate에서 차단한다.
- Live 실기 이동은 Phase 6 전까지 금지한다.

## Next Session Handoff

- 현재 브랜치: `codex/robotcontrol-v3-toolkit`
- 최신 커밋 기준:
  - `853d6c5 Document RobotControl V3 next session handoff`
- 첫 확인 명령:
  - `unityctl status --project C:\Users\ezen601\Desktop\Jason\robotapp2 --wait --json`
  - `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - direct V3 QA가 필요하면 `Always Start From Onboarding=false`로 잠깐 끄고 `Assets/Scenes/RobotControlV3.unity`에서 Play 후 반드시 원복한다.
- 바로 재실행할 핵심 matrix:
  - `RunLiveCommandSafetyGateMatrixForDebug()` -> `12/12 PASS`
  - `RunActualUiClickMatrixForDebug()` -> `95/95 PASS`
  - `RunTabletBottomActualClickMatrixForDebug()` -> `16/16 PASS`
  - `RunPopupConfirmCancelE2EForDebug()` -> `10/10 PASS`
  - `RunSafetyFaultActualFlowForDebug()` -> `5/5 PASS`
- 다음 구현 우선순위:
  - `Program Run/Step queue`: `PendantV3Points`를 실행 가능한 teaching sequence로 승격한다.
  - `Point MoveJ production IK policy`: saved joint target 외 numerical IK fallback은 계속 live 금지한다.
  - `Boundary/Collision`: 지금은 hard gate가 아니라 warning/future로 둔다.
- 절대 금지:
  - 실제 FR5 `MoveJ / MoveL / DO / ToolDO / MoveGripper`를 manual readback simulation, operator safety confirm UX, production IK policy 없이 열지 않는다.
  - live command를 열기 전에 `RunLiveSdkReadbackGateForDebug()` readback-only부터 수행한다.

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
- [x] EasyMotion 보조패널 compact layout에서 horizontal scroll/clipping 0건 확인
- [x] `BtnEasyZero`를 `Home` alias에서 분리하고 `Zero` preset preview 경로 확인
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
- [x] 보조패널 compact row (`J축+입력+값` / `슬라이더` / `- +`) 전환
- [x] `GetAuxLayoutSummaryForDebug()` 기준 `viewportHorizontalVisible=False`, `viewportClipped=0` 확인

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
- [x] TCP 조그 row를 `축+값+단위` / `- +` 2줄 compact 구조로 전환
- [x] Cartesian overlay row도 같은 compact 규칙으로 전환
- [x] `GetAuxLayoutSummaryForDebug()` 기준 `viewportHorizontalVisible=False`, `viewportClipped=0` 확인

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
- [x] `MoveJ` 보류 UX lock은 해제됨. 현재는 saved joint target 우선 + numerical XYZ IK fallback으로 preview/apply 가능
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
- [x] PointMove 보조/오른쪽 패널 compact layout에서 `viewportClipped=0`, `contextClipped=0` 확인

### Button integration Phase 1
- [x] `robot-button-integration-plan.md` 생성
- [x] `GetMovementStateSummaryForDebug()` 추가
- [x] `Zero` 독립 preset 추가 + preview smoke 확인
- [x] `CoordStrip` `Joint / TCP / Both` 실제 표시 모드 전환 연결
- [x] Easy/Joint/TCP/Cartesian 대표 전후 state matrix 검증
- [x] Joint preview target이 보조패널 row summary와 runtime summary에 동시에 반영되는지 확인
- [x] Point MoveL DryRun preview/apply 연결 및 검증
- [x] Point MoveJ numerical XYZ IK 기반 preview/dry-run apply 1차 연결
- [x] Point 저장/호출 + saved joint target 기반 MoveJ 우선 정책
- [x] Point list/select/delete UX 최소 연결
- [x] Point rename/export/persistence cleanup
- [ ] Production IK policy (orientation, 다중해, singularity, collision guard)
  - 현재 guard matrix에서는 `product-pending`으로 명시하고 실기 이동 gate에서 제외한다.

### Button integration Phase 3
- [x] `robottemplete` 최신 확인 (`git pull --ff-only`: already up to date)
- [x] `FR5EndEffectorAttachment.SetGripperOpen(float)` 성공 패턴 스캔
- [x] `RobotControlPeripheralFacade` / `RobotControlPeripheralState` 추가
- [x] `NavIo` / `BottomTabIo` I/O 보조패널 host 연결
- [x] `IoPanelController` mock I/O + gripper panel 연결
- [x] EasyMotion 그리퍼 버튼을 runtime peripheral facade로 변경
- [x] Gripper open/close, DO, ToolDO debug state 검증
- [x] PGEA attached visual prefab 이관/연결
- [x] live SDK gripper capability/readback scaffold
- [x] live command safety gate scaffold (`RunLiveCommandSafetyGateMatrixForDebug`: `12/12 PASS`)
- [x] product operator confirm token UX (`RunProductLiveConfirmTokenMatrixForDebug`: `4/4 PASS`)
- [ ] live SDK/ROS command real-device readback comparison

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
- [x] 복구 버튼 action wiring smoke (`RunSafetyFaultActualFlowForDebug`: `5/5 PASS`)
- [x] fault close/reset popup route 확인
- [ ] 실제 컨트롤러 fault 주입/readback 기반 policy 연결

### `2C-2` 뷰포트 보조 UI scaffold
- [x] `viewport-toolbar.uxml` / `.uss` 생성
- [x] `workspace-boundary.uss` 생성
- [x] `ViewportToolbarController.cs` 생성
- [x] shell host 연결 (`ViewportToolbarHost`)
- [x] `PendantV3SceneBuilder` serialized template 연결
- [x] `ViewportHost` boundary/collision 클래스 토글 scaffold 연결
- [x] preview 상태 기반 collision 위험 강조(ready/unsynced/fault) 반영
- [x] toolbar label compact화 (`Base / Tool / Path / Ghost / Bound / Coll / Cam`)
- [x] toolbar status/hint 기본 숨김 + scroll 본문 우선 유지
- [x] actual play smoke
  - `BtnViewportBoundary` click -> `경계 ON` + `작업공간 경계: 표시` + `ViewportHost`에 `rc-viewport-host--boundary`
  - `BtnPresetFault` click -> `충돌 예측: 위험 구간 감지 (자동 강조)` + `BtnViewportCollision` disabled `충돌 ON` + `ViewportHost`에 `rc-viewport-host--collision`
  - `BtnPresetReady` click -> `충돌 예측: 안전` + `BtnViewportCollision` enabled `충돌 OFF` + collision class 해제
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
- [x] actual play popup smoke (`Escape/Enter`, focus trap, confirm/cancel) 1차 닫기
  - debug `warning` popup open -> title `정지 안내`
  - `BtnPopupConfirm` actual click -> `popupActive=False`, focus 복귀 확인
  - debug `move` / `recovery` popup open -> title/confirm text 확인
- [x] `help-panel.uxml` / `.uss` 생성
- [x] popup confirm/cancel E2E (`RunPopupConfirmCancelE2EForDebug`: `10/10 PASS`)
- [x] status fault/safety detail help routing actual flow 확인
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
- [ ] `first-run-guide` popup/도움말 연계
- [ ] help-panel 탭별 세분화 2차 polish

## Policy Checklist

- [x] Onboarding direct path -> `FreshStart`
- [x] RobotLibrary re-entry -> `ResumeLastSession`
- [x] RobotLibrary restore UX 플레이 검증

## Next Verification Loop

1. `unityctl status --project C:\Users\ezen601\Desktop\Jason\robotapp2 --wait --json`
2. `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
3. 핵심 matrix 재실행
   - `RunLiveCommandSafetyGateMatrixForDebug()` -> `12/12 PASS`
   - `RunActualUiClickMatrixForDebug()` -> `95/95 PASS`
   - `RunTabletBottomActualClickMatrixForDebug()` -> `16/16 PASS`
   - `RunPopupConfirmCancelE2EForDebug()` -> `10/10 PASS`
   - `RunSafetyFaultActualFlowForDebug()` -> `5/5 PASS`
4. `play` 시작 규칙 확인
   - 기본값 `Always Start From Onboarding = true`
   - direct V3 검증이 필요하면 QA용으로만 일시 해제하고, 종료 후 복구
5. 다음 구현은 handoff 우선순위를 따른다.
   - `Program Run/Step queue`
   - `Point MoveJ production IK policy`
   - `Boundary/Collision warning-only future`
6. live command는 operator confirm UX와 production IK policy 없이 열지 않는다.

## Latest Test Result

- 2026-04-21 추가 확인
  - `unityctl check --type compile --json`: pass
  - Unity 재시작 후 `exec list-callables`에서 `GetAuxLayoutSummaryForDebug` 노출 확인
  - `rg 'mode="Horizontal"|ScrollViewMode\.Horizontal|AlwaysVisible|Bound Off|Coll Off|고스트' Assets/UI/PendantV3 Assets/Scripts -S`: V3 현행 보조패널 기준 horizontal 복구 흔적 없음
  - `SetShellSelection("NavMotion","TabTcpJog","BottomTabTcpJog")` 후 `GetAuxLayoutSummaryForDebug`: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextHorizontalVisible=False`, `contextClipped=0`
  - `SetShellSelection("NavMotion","TabJointJog","BottomTabJointJog")` 후 `GetAuxLayoutSummaryForDebug`: `viewportHorizontalVisible=False`, `viewportClipped=0`
  - `SetShellSelection("NavMotion","TabPointMove","BottomTabPointMove")` 후 `GetAuxLayoutSummaryForDebug`: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextClipped=0`
  - `SetShellSelection("NavMotion","TabEasyMotion","BottomTabEasyMotion")` 후 `GetAuxLayoutSummaryForDebug`: `viewportHorizontalVisible=False`, `viewportClipped=0`
  - screenshot: `Artifacts/robotcontrolv3-compact-no-clipping-v25.png`
  - 자기리뷰: App 브리지는 QA/debug helper만 추가, UI 레이아웃은 UXML/USS 책임 안에서 처리, 메인패널/보조패널 역할 경계 유지
  - `GetMovementStateSummaryForDebug`: callable 노출 확인
  - `PreviewEasyMotionForDebug("Zero")`: `pending=대기 명령: MoveJ`, `feedback=[Preview] Zero 프리셋`, `ghost=True`, `path=True`
  - `SetCoordStripModeForDebug("Joint")`: `jointHidden=False`, `tcpHidden=True`
  - `SetCoordStripModeForDebug("TCP")`: `jointHidden=True`, `tcpHidden=False`
  - `SetCoordStripModeForDebug("Both")`: `jointHidden=False`, `tcpHidden=False`

- 2026-04-20 추가 확인
  - `git reset --hard 8549b09` 후 기준선 복귀 확인
  - `git status --short`: clean 기준선 확인
  - `unityctl check --type compile --json`: pass
  - `Always Start From Onboarding`가 켜져 있으면 play 시작 시 현재 편집 씬이 아니라 `Onboarding`부터 시작하는 것 확인
  - QA용으로 `Always Start From Onboarding`를 잠깐 끄고 `RobotControlV3.unity`를 직접 열면 `SceneId=7`에서 direct play 가능함을 확인
  - QA 종료 후 `Always Start From Onboarding = true`로 복구
  - 오늘 viewport 재배치 실험은 모두 rollback 처리하고 기준선 유지
  - 시행착오 요약
    - `ViewportHost`가 오늘 실험으로 생긴 줄 알고 reset을 반복했지만, 실제론 `8549b09` 이전 baseline에도 이미 존재했음
    - play 중 stale runtime 화면 때문에 "원복이 안 됐다"는 착시가 있었음
    - `RobotControlV3DebugBridge`가 baseline과 안 맞아 임시 컴파일 오류를 만들었고, 이건 세션 중 정리함

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

## Source Docs

- [README.md](./README.md)
- [implementation-plan.md](./implementation-plan.md)
- [robot-button-integration-plan.md](./robot-button-integration-plan.md)
- [feature-jog-motion.md](./feature-jog-motion.md)
- [shell-layout.md](./shell-layout.md)
