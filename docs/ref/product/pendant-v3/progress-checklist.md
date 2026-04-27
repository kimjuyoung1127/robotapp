# Pendant V3 Progress Checklist

## Purpose
- V3 티칭패드 구현 진행률을 한 문서에서 체크한다.
- 현재 완료/진행중/미착수 범위를 빠르게 확인한다.
- daily log와 달리 "지금 어디까지 왔는지"만 짧게 유지한다.

## Last Updated
- 2026-04-27 (KST)

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
| `3B` 로컬 서비스 | in_progress | Product live confirm token 완료, manual readback `6/6 PASS`, sequence runtime/run-step/order-overwrite/detail/duplicate/timing/confirm/edit-lock/loop/run-from-selected/function-v1-polish `33/33 PASS` |
| `3C` mock e2e | done | Desktop actual click baseline `110/110 PASS`, function actual click current contract `7/7 PASS`, tablet/bottom representative `16/16 PASS`, popup/safety/point/live-readback/live-command gate artifacts 생성 |
| `4` V2 vs V3 평가 | pending | 미착수 |

## 2026-04-20 Viewport Note

- 오늘 viewport 관련 실험은 **채택 안 함**으로 잠근다.
- 현재 기준선은 `8549b09`이며, 이 기준선 자체에 `MainSplitHandle + ViewportHost` 별도 패널 구조가 이미 포함돼 있다.
- 즉 오늘 화면에서 계속 보이던 별도 `ViewportHost`는 "오늘 수정 잔재"가 아니라 **현재 기준선 원구조**다.
- 오늘 시도한 `ViewportHost 내장`, `RT/오버레이 분리`, `2패널 분리` 실험은 모두 rollback 대상으로 간주한다.
- 다음 세션에서는 구현 전에 먼저 아래를 문서로 잠근 뒤 시작한다.
  - 로봇을 **어느 패널에 표시할지** 1회 확정
  - `ViewportHost`를 유지할지 제거할지 1회 확정
  - 시각화 토글 카드가 어느 패널에 들어갈지 1회 확정

## 2026-04-20 Display Lock

- 현재 하이라이트된 `WorkPanel`을 **로봇 표시 핵심 패널**로 확정한다.
- `ViewportHost`는 메인 로봇 표시 패널이 아니다. 1차 구현에서는 보조/유틸 영역으로만 취급한다.
- `WorkPanel` 내부는 `RobotStage` 단일 책임으로 잠근다.
  - `RobotStage`: 로봇 메시 + 프레임 + 고스트 + 트레일 + 바닥 격자 + 선택 XYZ 기즈모
- 현재 선택 탭의 조작 UI는 `ViewportHost`의 보조 작업 패널로 이동한다.
- `NavMotion`의 조작 모드는 상단 독립 탭이 아니라 `ControlDockHost` 내부 `기본 / 관절 / TCP / 좌표` subtab으로 표시한다.
- `NavPoints` active 상태에서는 조작 subtab을 숨기고 `포인트 / 시퀀스 / 함수` 내부 subview만 표시한다.
- `TCP 3D 화살표`, 특히 `Z / RX / RY / RZ` 조작은 로봇을 가리지 않게 `ViewportHost` 보조 패널에서만 노출한다.
- `ViewportHost`는 조작/설명 중심 공용 `ScrollView` 구조로 유지한다.
- `기본 / 관절 / TCP / 좌표`는 모두 같은 `RobotStage`를 공유하고, 보조 패널 쪽 내용만 교체한다.
- 다음 구현 단위는 `고스트 / predicted path / 경계 / 충돌`을 실제 시각 데이터와 연결하는 것이다.

## 2026-04-21 Aux Compact Lock

- 가로 스크롤은 복구하지 않는다.
- 보조패널/오른쪽 패널 버튼 잘림은 내부 요소 compact/wrap으로 해결한다.
- `ViewportPanelScroll`과 `ContextPanelScroll`은 세로 전용으로 유지한다.
- TCP/Cartesian 조작행은 `축+값+단위`와 `- / +`를 2줄로 분리했다.
- Joint 조작행은 `J축+입력+값`, `슬라이더`, `- / +` 버튼 행으로 분리했다.
- Point/Easy/Coord/Status/Safety 카드도 `min-width: 0`, `max-width: 100%`, wrap/compact 기준으로 맞췄다.
- 재시작 후 `GetAuxLayoutSummaryForDebug()` 기준 `horizontalVisible=False`, `clipped=0`, `scrollShare>=0.88`을 확인했다.

## 2026-04-22 Motion Subtabs Aux Panel Lock

- `WorkTabBar`는 런타임에서 `ControlDockHost` 첫 줄로 이동한다.
- `NavMotion` active 상태에서만 `기본 / 관절 / TCP / 좌표`를 표시한다.
- `NavPoints` active 상태에서는 조작 subtab을 `display: none`으로 숨긴다.
- 보조패널 스크롤 순서는 `ControlDockHost -> CartesianArrowsOverlayHost -> ViewportDescriptionSection`로 잠근다.
- 실제 조작 UI와 TCP 3D 방향 조작을 먼저 보여주고, 설명 카드는 그 아래로 둔다.
- 시각화 토글 카드는 `ContextPanelTabBar` 아래, `CoordStrip` 위로 이동한다.
- Desktop 폭 우선순위는 `메인패널 > 보조패널 > 컨텐츠패널`로 잠근다.
- `ViewportHost` 최소 폭은 360px, `ContextPanel` 기준 폭은 320px이다.
- `기본`은 기존 EasyMotion, `좌표`는 기존 PointMove 직접 좌표 이동 경로다.
- `포인트` 좌측 탭은 티칭 포인트 저장/목록/시퀀스/함수 전용으로 유지한다.
- 검증:
  - `RunAuxPanelOrderMatrixForDebug()` -> `2/2 PASS`
  - `GetPanelWidthHierarchySummaryForDebug()` -> `main>aux>context`
  - `RunMotionTabExposureMatrixForDebug()` -> `6/6 PASS`
  - `RunActualUiClickMatrixForDebug()` -> `113/113 PASS`
  - `RunRobotLinkedButtonSimulationAuditForDebug()` -> `74/74 PASS`

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
- Desktop actual UI click matrix `98/98 PASS`.
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
- Teaching sequence runtime matrix `6/6 PASS`.
- `PendantV3Points`를 load/select/preview/execute 할 수 있는 runtime adapter를 연결했다.
- Teaching sequence runtime matrix 확장 `11/11 PASS`.
- `Run`은 pending preview 우선 후 saved sequence 실행, `Step▶/Step◀`는 saved point preview-only로 연결했다.
- Point up/down order editing과 current readback overwrite를 연결했다.
- live motion은 manual readback simulation, product confirm, production IK policy가 준비될 때까지 gate에서 차단한다.
- Live 실기 이동은 Phase 6 전까지 금지한다.

## 2026-04-22 Teaching Sequence SSOT Reconciliation

- `teaching-sequence-execution-plan.md`의 Current State를 실제 구현 기준으로 재분류했다.
- 완료로 잠근 범위:
  - manual readback simulation: `RunManualReadbackTeachingMatrixForDebug()` -> `6/6 PASS`
  - sequence load/select/preview/execute runtime: `RunTeachingSequenceMatrixForDebug()` -> `11/11 PASS`
  - `Run` fallback: pending preview 우선 후 saved `PendantV3Points` RunOnce
  - `Step▶/Step◀`: saved point selection + preview-only
  - `Stop`: sequence runner/motion stop path
  - point order editing: `위로` / `아래로`
  - current readback overwrite: name/moveType/speed/dwell 보존, joints/TCP만 갱신
  - actual UI click matrix: point reorder/overwrite buttons 포함 `98/98 PASS`
- 아직 미구현으로 잠근 범위:
  - duplicate point
  - duplicate-name overwrite confirmation
  - visible point detail UI for joints/TCP/moveType/speed/dwell
  - speed/dwell editing
  - delete/overwrite consequence confirmation copy
  - `선택 지점부터 실행`
  - visible loop toggle/status
  - execution-time edit lock in visible UI
  - function/group model
  - IO/gripper sequence blocks
  - stable point IDs if v1 name keys become limiting
- 다음 실행 우선순위는 `Phase C2 - Easy Editing`이다.

## 2026-04-22 Phase C2 Easy Editing Start

- PointMove selected point detail UI를 추가했다.
  - name
  - move type
  - speed preset
  - dwell
  - saved joints
  - saved TCP
- `복사` 버튼을 추가했다.
  - 선택 포인트를 바로 다음 순서에 복제한다.
  - 이름은 `원본_COPY`, 충돌 시 `원본_COPY_2`로 고유화한다.
  - 일반 save의 사용자 입력 이름 정책은 그대로 유지한다.
- 저장 정책을 SSOT와 다시 맞췄다.
  - normal point save는 포인트 이름만 검증한다.
  - dirty coordinate input은 저장값에 섞이지 않고, 저장 차단 조건도 아니다.
  - saved joints/TCP는 계속 current readback snapshot에서만 온다.
- QA helper를 추가했다.
  - `DuplicatePointMoveForDebug(string pointName)`
  - `GetPointMoveDetailForDebug()`
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingSequenceMatrixForDebug()`: `13/13 PASS`
  - `RunActualUiClickMatrixForDebug()`: `99/99 PASS`
  - `GetAuxLayoutSummaryForDebug()` on PointMove: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextHorizontalVisible=False`, `contextClipped=0`
- 콘솔에는 기존 unityctl IPC pipe 로그가 남았고, 기능 matrix failure는 없다.
- 다음 C2 잔여 범위:
  - speed/dwell editing
  - delete/overwrite consequence confirmation copy
  - duplicate-name overwrite confirmation
  - execution-time edit lock in visible UI

## 2026-04-22 Phase C2 Easy Editing Complete

- Speed/dwell editing을 추가했다.
  - `slow / medium / fast` 선택
  - dwell seconds 입력
  - 선택 포인트에 speed/dwell 저장
- 위험 편집은 2-click 확인으로 잠갔다.
  - delete: 첫 클릭은 "순서 목록에서 제거" 안내, 두 번째 클릭이 실제 삭제
  - overwrite: 첫 클릭은 "joints/TCP만 readback으로 교체, name/moveType/speed/dwell 유지" 안내, 두 번째 클릭이 실제 덮어쓰기
  - duplicate-name save: 첫 저장은 기존 이름 덮어쓰기 안내, 두 번째 저장이 기존 포인트를 같은 순서에서 교체
- 실행 중 편집 lock을 추가했다.
  - save/delete/rename/duplicate/reorder/overwrite/cleanup/timing edit 잠금
  - 현재 RunOnce는 동기 실행이라 짧게 끝나지만, loop/async 실행 전 UI 정책을 먼저 잠갔다.
- Runtime은 `IsTeachingSequenceRunning`만 노출하고, 편집 UX 판단은 `PointMoveController`가 담당한다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingSequenceMatrixForDebug()`: `20/20 PASS`
  - `RunActualUiClickMatrixForDebug()`: `103/103 PASS`
  - `GetAuxLayoutSummaryForDebug()` on PointMove: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextHorizontalVisible=False`, `contextClipped=0`
- 콘솔에는 기존 unityctl IPC pipe 로그가 남았고, 기능 matrix failure는 없다.
- 다음 우선순위는 `Phase D - Loop Mode` 또는 `Run From Selected` 중 하나다.

## 2026-04-22 Phase D Loop Mode

- Point/Teaching 패널에 `반복 ON/OFF` 토글과 loop status를 추가했다.
- BottomBar에는 새 loop 버튼을 넣지 않았다.
- `Run` 동작은 기존 우선순위를 유지한다.
  - pending preview가 있으면 pending preview 실행
  - pending preview가 없고 loop mode OFF면 saved sequence RunOnce
  - pending preview가 없고 loop mode ON이면 `WaypointCycleRunner.PlayLoop`
- `Stop`은 loop runner를 정지하고 runner state를 idle로 돌린다.
- Runtime loop state surface:
  - `IsTeachingLoopEnabled`
  - `IsTeachingSequenceRunning`
  - `GetTeachingLoopSummaryForDebug()`
- Runner frame update를 V3 RobotStage/snapshot 쪽으로 다시 연결했다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingSequenceMatrixForDebug()`: `24/24 PASS`
  - `RunActualUiClickMatrixForDebug()`: `104/104 PASS`
  - `GetAuxLayoutSummaryForDebug()` on PointMove: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextHorizontalVisible=False`, `contextClipped=0`
- 콘솔에는 기존 unityctl IPC pipe 로그가 남았고, 기능 matrix failure는 없다.
- 다음 우선순위는 `Run From Selected`다.

## 2026-04-22 Run From Selected

- Point/Teaching 패널에 `선택부터` 버튼을 추가했다.
- 선택된 포인트부터 `PendantV3Points` 끝까지 1회 실행한다.
- Loop mode가 ON이어도 `선택부터`는 명시적 one-shot run으로 처리한다.
- 실행 중인 loop runner가 있으면 Stop 후 다시 실행하라는 feedback을 낸다.
- BottomBar에는 새 버튼을 추가하지 않았다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingSequenceMatrixForDebug()`: `26/26 PASS`
  - `RunActualUiClickMatrixForDebug()`: `105/105 PASS`
  - `GetAuxLayoutSummaryForDebug()` on PointMove: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextHorizontalVisible=False`, `contextClipped=0`
- 콘솔에는 기존 unityctl IPC pipe 로그가 남았고, 기능 matrix failure는 없다.
- 다음 우선순위는 `Phase E - Function / Group Planning`이다.

## 2026-04-22 Phase E Function / Group Planning

- `TeachingFunction` 개념을 문서로 잠갔다.
- Function은 제조사 program/Lua/script가 아니라 Unity teaching routine이다.
- Function은 `NavPoints` 내부 `함수` subview에 둔다.
- 새 left-nav `Program` 탭은 추가하지 않는다.
- V1 function step은 `PointRef`만 사용한다.
  - `FunctionRef`는 future composition으로만 문서화한다.
  - IO/gripper steps, variables, IF/ELSE/LOOP blocks는 제외한다.
- V1 reference는 point name 기반이다.
  - stable point ID는 name 기반 workflow가 한계에 닿을 때 후속으로 검토한다.
- 첫 구현 단위:
  - selected points로 함수 생성
  - function list
  - ordered point refs detail
  - rename / duplicate / delete function
  - function RunOnce DryRun
- 첫 예시 함수:
  - `HomeReturn`
  - `Pick`
  - `Place`
  - `Inspect`
- 검증:
  - `git diff --check`: pass
  - `unityctl check --type compile`: pass
- 다음 우선순위는 Function v1 scaffold다.

## 2026-04-22 Function v1 Scaffold

- `TeachingFunctionStore`를 추가했다.
- Function 저장 위치:
  - `Application.persistentDataPath/teaching-functions`
- Function UI는 Point/Teaching 패널 안의 compact `함수` 카드로 추가했다.
- 지원 기능:
  - function store
  - create from current ordered `PendantV3Points`
  - list
  - detail with ordered point refs
  - rename
  - duplicate
  - delete
  - RunOnce DryRun
- 멀티 선택 포인트 UI가 아직 없어서, v1 create는 현재 저장된 포인트 순서 전체를 함수로 묶는다.
- 제조사 Program/Lua/script 실행은 계속 제외다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingSequenceMatrixForDebug()`: `31/31 PASS`
  - `RunActualUiClickMatrixForDebug()`: `110/110 PASS`
  - `GetAuxLayoutSummaryForDebug()` on PointMove: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextHorizontalVisible=False`, `contextClipped=0`
- 콘솔에는 기존 unityctl IPC pipe 로그와 store 저장/삭제 로그만 있고, 기능 matrix failure는 없다.
- 다음 우선순위는 function v1 polish 또는 IO/gripper sequence block 설계다.

## 2026-04-22 Function v1 Polish

- Function create-from-selected-points UI를 추가했다.
  - `선택 추가`
  - `선택 초기화`
  - 후보 summary
- Function RunFromSelected를 추가했다.
  - `함수 선택부터`
  - 선택된 포인트 ref가 함수 안에 없으면 명확한 feedback을 낸다.
- Missing-ref warning을 함수 detail에 추가했다.
  - `missingCount`
  - missing ref names
- Aux panel 화면용 function copy를 raw debug list에서 짧은 요약으로 바꿨다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingSequenceMatrixForDebug()`: `33/33 PASS`
  - `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
  - `GetAuxLayoutSummaryForDebug()` on PointMove: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextHorizontalVisible=False`, `contextClipped=0`
- Screenshot evidence:
  - `Artifacts/pendant-v3-function-polish-final.png`
  - `Artifacts/pendant-v3-function-polish-action-buttons.png`
- 참고:
  - 전체 `RunActualUiClickMatrixForDebug()`는 케이스가 길어져 unityctl IPC 30초 응답 제한에 걸릴 수 있다.
  - 그래서 function 버튼 actual click은 `RunFunctionActualClickMatrixForDebug()`로 분리했다.
- 다음 우선순위는 missing-ref repair UX 또는 IO/gripper sequence block 설계다.

## 2026-04-22 NavPoints Teaching Panel Lock

- SSOT 재확인: 티칭 포인트/시퀀스/함수 UX는 `NavPoints`에서 바로 열려야 한다.
- 구현 보정:
  - `NavPoints` 선택 시 `PointMoveController` desktop aux panel을 표시한다.
  - `NavMotion > TabPointMove`는 좌표 직접 이동 호환 경로로 유지한다.
  - `NavPoints` active 상태에서는 조작 내부 탭을 숨기고 셸/패널 제목/도움말/보조 설명을 `티칭 포인트` 기준으로 표시한다.
- 회귀 보호:
  - `RunTeachingSequenceMatrixForDebug()`에 `navpoints-opens-teaching-point-panel` 케이스를 추가했다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
  - `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
  - `GetAuxLayoutSummaryForDebug()` on NavPoints: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextHorizontalVisible=False`, `contextClipped=0`
- Screenshot evidence:
  - `Artifacts/pendant-v3-navpoints-teaching-panel.png`
- 안정화:
  - stale/short `previewTcpPose`가 target marker를 인덱싱해 직접 씬 재진입 matrix를 깨는 문제를 방어했다.

## 2026-04-22 Input Text + Teaching Actual Click Motion QA

- Text input visibility:
  - V3 TextField 내부 입력 텍스트를 검정색으로 고정했다.
  - 적용 범위는 `PendantV3` root 아래 `unity-base-text-field__input` / `unity-text-input`이다.
  - Screenshot evidence: `Artifacts/pendant-v3-input-text-black.png`
- Teaching actual click motion QA:
  - `GetStagePoseSignatureForDebug()`를 추가했다.
  - `RunTeachingActualClickMotionMatrixForDebug()`를 추가했다.
  - 실제 UI Toolkit `ClickEvent` 후 `status/pending/feedback/joints/tcp/ghost/path/visual` signature가 바뀌는지 검증한다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingActualClickMotionMatrixForDebug()`: `6/6 PASS`
  - `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
  - `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
  - `RunActualUiClickMatrixForDebug()`: `113/113 PASS`
  - `RunRobotLinkedButtonSimulationAuditForDebug()`: `74/74 PASS`
  - `RunStageScreenshotEvidenceForDebug()`: `ready-front`, `ready-side`, `tcp-iso` screenshots generated
  - `RunPointMoveJProductionGuardMatrixForDebug()`: `6/6 PASS`
- 테스트 스킬:
  - `unity-ui-mcp-guard`와 `unity-cli-qa-validation`을 참고했다.
  - 이 저장소의 현행 SSOT는 `unityctl`이므로 새 스킬 생성 없이 기존 성공 패턴을 `RobotControlV3DebugBridge` matrix로 확장했다.

## 2026-04-22 Teaching Subview UX Reorganization

- `NavPoints` 내부를 `포인트 / 시퀀스 / 함수` segmented subview로 분리했다.
- 상단 primary action을 `현재 위치 저장`으로 승격했다.
- 포인트 row에 바로 `이동`, `미리보기`, `수정`, `후보` 액션을 추가했다.
- 실행 관련 버튼을 `시퀀스` subview 안의 실행 band로 모았다.
  - `Run`
  - `Step◀`
  - `Step▶`
  - `Stop`
  - `선택부터`
  - `반복 ON/OFF`
- Function flow를 `선택한 포인트 N개 → 함수 만들기`로 표시했다.
- 긴 point/function 이름은 row/list 표시에서 줄이고 detail/debug data에는 원본을 유지한다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingSubviewActualClickMatrixForDebug()`: `10/10 PASS`
  - `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
  - `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
  - `RunTeachingActualClickMotionMatrixForDebug()`: `6/6 PASS`
  - `GetAuxLayoutSummaryForDebug()`: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextHorizontalVisible=False`, `contextClipped=0`
- Screenshot evidence:
  - `Artifacts/pendant-v3-subview-points.png`
  - `Artifacts/pendant-v3-subview-sequence.png`
  - `Artifacts/pendant-v3-subview-function.png`
- 참고:
  - 전체 `RunActualUiClickMatrixForDebug()`는 현재 케이스 수가 커져 unityctl IPC 30초 제한에 걸릴 수 있다.
  - 이번 UX 변경 회귀는 subview/function/teaching motion split matrix로 닫았다.

## 2026-04-22 Sequence Library Linkage

- `시퀀스` 탭에서 `포인트` 탭 저장 결과를 바로 볼 수 있게 했다.
  - `PendantV3Points` -> `저장한 포인트 순서`
  - `PendantV3RecordedPath` -> `기록한 경로`
  - 기타 `WaypointStore` sequence -> 실행 목록 row
- sequence row에 `선택`, `재생`, `루프`, `삭제`를 추가했다.
- `기록 삭제` 버튼을 추가해 기록된 루프/경로를 시퀀스 탭에서 직접 지울 수 있게 했다.
- `PendantV3Points` 삭제는 실수 방지를 위해 포인트 탭의 개별 삭제/정리 confirmation으로 유지한다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunSequenceLibraryMatrixForDebug()`: `11/11 PASS`
  - `RunTeachingSubviewActualClickMatrixForDebug()`: `16/16 PASS`
  - `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
  - `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
  - `RunTeachingPathRecordingLoopMatrixForDebug()`: pass

## 2026-04-23 Point Row Action Modal

- 포인트 row 버튼 문구를 사용자 작업 기준으로 바꿨다.
  - `이동` -> `실행`
  - `수정` -> `편집`
  - `후보` -> `함수 추가`
- row 버튼은 바로 아래 detail 카드로 스크롤을 요구하지 않고, 먼저 포인트 작업 모달을 연다.
- `편집` 모달에서 이름, 속도, dwell을 저장하고, 덮어쓰기/복사/삭제도 같은 문맥에서 실행할 수 있게 했다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingSubviewActualClickMatrixForDebug()`: `16/16 PASS`
  - `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
  - `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
- Runtime text recheck:
  - source에는 old labels `이동 / 수정 / 후보`가 남아 있지 않다.
  - refreshed Play runtime UITK 기준 `BtnPointRowMove=실행`, `BtnPointRowEdit=편집`, `BtnPointRowFunctionCandidate=함수 추가`.
  - `RunTeachingSubviewActualClickMatrixForDebug()`: `19/19 PASS`
  - old labels가 보이면 active Play UI tree가 stale 상태이므로 Play restart 또는 RobotControlV3 scene re-entry가 필요하다.

## 2026-04-23 Point List Management

- 포인트 탭 상단에 inventory summary를 추가했다.
  - 포인트 개수
  - 함수 개수
  - 속도별 개수: 느림 / 중간 / 빠름
  - 현재 선택 개수
- 포인트 row에 `선택` 버튼을 추가하고 다중선택 상태를 row highlight로 표시한다.
- 일괄 작업을 추가했다.
  - 선택 해제
  - 선택 속도 저장
  - 선택 함수 추가
  - 선택 삭제
- `버튼 접기 / 버튼 펼치기`로 row action 버튼을 줄여 긴 목록 스캔을 쉽게 했다.
- 기존 장황한 보조 메시지는 숨기고, confirmation/save/delete/bulk/error 중심의 짧은 feedback만 남겼다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingSubviewActualClickMatrixForDebug()`: `25/25 PASS`
  - `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
  - `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`

## 2026-04-23 Sequence And Function List Management

- 포인트 탭과 같은 목록 관리 패턴을 `시퀀스`와 `함수` 탭에도 확장했다.
- 시퀀스 탭:
  - 실행 목록 개수, 삭제 가능 개수, 총 포인트 개수, 선택 개수를 표시한다.
  - row 다중선택을 지원한다.
  - `PendantV3Points`는 보호하고, recorded/named sequence만 일괄 삭제한다.
  - `버튼 접기 / 펼치기`로 row action을 줄인다.
- 함수 탭:
  - 함수 개수, 선택 개수, 선택 함수의 참조/누락 수를 표시한다.
  - row 다중선택을 지원한다.
  - 선택 함수 일괄 복사/삭제를 지원한다.
  - `버튼 접기 / 펼치기`로 row action을 줄인다.
- QA 안정화를 위해 full subview actual-click matrix에 모두 밀어 넣지 않고 focused matrix를 추가했다.
  - `RunSequenceFunctionBulkManagementMatrixForDebug()`
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunSequenceFunctionBulkManagementMatrixForDebug()`: `11/11 PASS`
  - `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
  - `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`

## 2026-04-23 Point / Bundle / Sequence Redefinition

- 사용자 용어를 `함수`에서 `묶음`으로 바꿨다.
  - 내부 `TeachingFunction` 타입명과 JSON 호환성은 유지한다.
  - UI 탭 순서는 `포인트 / 묶음 / 시퀀스`다.
- `시퀀스` 탭에 `작업 시퀀스` 영역을 추가했다.
  - `PointRef` 블록과 `BundleRef` 블록을 한 줄짜리 실행 블록으로 표시한다.
  - 저장소는 `PendantV3Blocks`다.
  - `포인트 추가`, `묶음 추가`, `위/아래`, `삭제`, `미리보기`, `실행`을 지원한다.
- 실행 정책:
  - `PointRef`는 저장 포인트 1개로 펼친다.
  - `BundleRef`는 묶음 내부 포인트 참조들로 펼친다.
  - v1 실행은 Unity/Mock DryRun 경로이며 live gate를 우회하지 않는다.
- 검증 결과:
  - `unityctl check --type compile`: pass
  - `RunTeachingBlockSequenceMatrixForDebug()`: `9/9 PASS`
  - `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
  - `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
  - `RunSequenceFunctionBulkManagementMatrixForDebug()`: `11/11 PASS`
  - UITK text/order check: `포인트 / 묶음 / 시퀀스`

## 2026-04-23 Bundle Add/Delete/Run Verification

- 묶음 후보 추가 완료 feedback이 사용자에게 보이도록 `[Bundle]` feedback을 표시 대상에 포함했다.
- 묶음 탭에 `전체 삭제` 버튼을 추가했다.
  - confirmation 2회 클릭 후 모든 묶음 JSON을 삭제한다.
  - 삭제 후 선택 묶음/선택 목록/후보 목록을 비운다.
- 검증 결과:
  - `RunBundleAddDeleteRunMatrixForDebug()`: `5/5 PASS`
  - 전체 묶음 삭제 후 `functions=0` 확인
  - 포인트 2개를 묶음 후보로 추가하면 `candidates=1`, `candidates=2`로 증가 확인
  - 사용자 완료 피드백 확인: `[Bundle] 후보 추가 · BUNDLE_POINT_A/B`
  - 새 묶음 `BUNDLE_AFTER_DELETE` 생성 확인
  - 생성된 묶음 DryRun 실행 확인
  - 파일 저장소에는 `BUNDLE_AFTER_DELETE.json`만 남는 것 확인

## Next Session Handoff

- 현재 브랜치: `codex/robotcontrol-v3-toolkit`
- 최신 구현 커밋 기준:
  - `522dcbe Add Pendant V3 run from selected`
- 첫 확인 명령:
  - `unityctl status --project C:\Users\ezen601\Desktop\Jason\robotapp2 --wait --json`
  - `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - direct V3 QA가 필요하면 `Always Start From Onboarding=false`로 잠깐 끄고 `Assets/Scenes/RobotControlV3.unity`에서 Play 후 반드시 원복한다.
- 바로 재실행할 핵심 matrix:
  - `RunLiveCommandSafetyGateMatrixForDebug()` -> `12/12 PASS`
  - `RunFunctionActualClickMatrixForDebug()` -> `8/8 PASS`
  - `RunTabletBottomActualClickMatrixForDebug()` -> `16/16 PASS`
  - `RunPopupConfirmCancelE2EForDebug()` -> `10/10 PASS`
  - `RunSafetyFaultActualFlowForDebug()` -> `5/5 PASS`
- 다음 구현 우선순위:
  - `Function v1 polish`: create-from-selected-points UI, missing-ref warning polish, function RunFromSelected.
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
- [x] toolbar label compact화 (`Path / Ghost / Bound / Coll / Cam`)
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
   - `RunActualUiClickMatrixForDebug()` -> `98/98 PASS`
   - `RunTabletBottomActualClickMatrixForDebug()` -> `16/16 PASS`
   - `RunPopupConfirmCancelE2EForDebug()` -> `10/10 PASS`
   - `RunSafetyFaultActualFlowForDebug()` -> `5/5 PASS`
4. `play` 시작 규칙 확인
   - 기본값 `Always Start From Onboarding = true`
   - direct V3 검증이 필요하면 QA용으로만 일시 해제하고, 종료 후 복구
5. 다음 구현은 handoff 우선순위를 따른다.
   - `Easy point editing`
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
- note: `NavHelp` actual click 기준 `HelpPanelHost` visible, 조작 내부 탭 hidden, `WhyItMovedSummary` 갱신까지 확인했다.
- note: `BottomTabHelp` actual click 기준 `HelpSheetHost` visible, `BottomSheetTitle=BottomSheet · 도움말`, `BottomTabTcpJog` 복귀 시 `BottomSheetTitle=BottomSheet · TCP` 원복까지 확인했다.

## 2026-04-23 Point / Bundle / Sequence Role Re-cut

- 사용자 피드백 기준으로 `묶음` 탭 역할을 다시 잘랐다.
- `포인트` 탭이 이제 묶음 후보 선택과 `함수 라이브러리 저장`을 담당한다.
- `묶음` 탭은 생성/실행 탭이 아니라 저장된 함수 라이브러리 관리 탭으로 정리했다.
- `시퀀스` 탭의 `묶음 추가`는 `묶음 라이브러리 modal`로 바꿨다.
- `현재 위치 저장`은 `포인트` 탭에만 남기고, 기존 detail 카드의 속도/대기 조절 UI는 제거했다.
- 포인트별 속도/대기 수정은 `편집 modal` 중심으로 정리했다.
- 묶음 후보가 비어 있을 때는 `현재 보고 있는 포인트 1개`를 함수 등록 소스로 우선 사용하게 바꿨다.
- 구현 메모:
  - `point-move-panel.uxml`
  - `PointMoveController.cs`
  - `PointMoveController.Elements.cs`
  - `RobotControlV3DebugBridge.cs`
- 검증:
  - `unityctl check --type compile`: pass
- actual click smoke:
  - `BtnPointSubview` click: pass
  - `현재 포인트 1개` 기준 `CreateTeachingFunctionForDebug("ONE_PICK_TRUE")` -> `steps=1`: pass
  - `BtnSequenceSubview` click: pass
  - `BtnBlockAddBundle` click -> `bundlePickerOpen=True`: pass
  - `BtnBundlePickerConfirm` click -> `blockSequence=PendantV3Blocks; blocks=1; list=[0:BundleRef:ONE_PICK_TRUE:True]`: pass
- note:
  - Play 중 현재 저장소에 남아 있던 이전 포인트/묶음 상태 때문에 검증용 이름이 `LIB_B` 대신 `1` 같은 기존 포인트로 읽힌 케이스가 있었고, 최종 smoke는 새 fallback 규칙과 actual-click 기준으로 다시 닫았다.

## 2026-04-27 Click Matrix Stabilization

- `RunFunctionActualClickMatrixForDebug()`를 현재 point/bundle/library recut UX 계약 기준으로 다시 맞췄다.
- 디버그 클릭 탐색을 안정화했다.
  - 여러 `UIDocument`를 모두 검색한다.
  - `PointMoveController` debug root도 fallback으로 검색한다.
  - detached row button은 제외한다.
  - row action button은 `button.clicked` 경로로 통일한다.
- domain reload 뒤 partial initialized runtime 상태가 남아 NRE를 내지 않도록 `RobotControlV3RuntimeController.TryInitialize()` guard를 보강했다.
- 검증:
  - `unityctl check --type compile`: pass
  - `RobotStageOrientationGizmoControllerTests`: `2/2 PASS`
  - `RobotStageRenderSurfaceInputTests`: `4/4 PASS`
  - `RobotControlV3GizmoBehaviorTests`: `3/3 PASS`
  - `RunTeachingBlockSequenceMatrixForDebug()`: `9/9 PASS`
  - `RunFunctionActualClickMatrixForDebug()`: `7/7 PASS`
  - `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
  - `RunSequenceFunctionBulkManagementMatrixForDebug()`: `11/11 PASS`
  - `RunBundleAddDeleteRunMatrixForDebug()`: `5/5 PASS`

## Source Docs

- [README.md](./README.md)
- [implementation-plan.md](./implementation-plan.md)
- [robot-button-integration-plan.md](./robot-button-integration-plan.md)
- [feature-jog-motion.md](./feature-jog-motion.md)
- [shell-layout.md](./shell-layout.md)
