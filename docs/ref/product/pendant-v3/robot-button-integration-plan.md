# RobotControlV3 Button-to-Robot Integration Plan

## Purpose
- RobotControlV3의 모든 조작 버튼이 `UI -> Controller -> Runtime -> Mock/Live boundary -> RobotStage`까지 이어지는지 관리하는 기준 문서다.
- V2는 구조/검증 성공패턴 참고용이고, 실조작 기능 범위는 FAIRINO 공식 SDK 기능군과 `Assets/Scripts/App/Fairino` 계약을 기준으로 한다.
- 구현 전후에는 이 문서의 상태표와 `progress-checklist.md`를 비교한다.

## Last Updated
- 2026-04-21 (KST)

## References
- FAIRINO SDK Manual: https://www.fairino.us/cobots-manual/sdk-manual
- FAIR-INNOVATION GitHub: https://github.com/FAIR-INNOVATION
- Local V2/V3 migration: `docs/ref/product/pendant-v3/migration-strategy.md`
- Local progress SSOT: `docs/ref/product/pendant-v3/progress-checklist.md`

## Status Legend

| Status | Meaning |
|---|---|
| `wired` | 버튼 클릭이 runtime/mock/visual state 변화까지 이어짐 |
| `partial` | UI, popup, preview, scaffold 중 일부만 이어짐 |
| `stub` | 버튼/문구는 있으나 실제 기능은 simulate 또는 placeholder |
| `pending` | 버튼/패널 자체가 없거나 구현 예정 |
| `excluded` | 현재 V3 핵심 범위에서 의도적으로 제외 |

## Current Integration Matrix

| Area | Buttons | Current | Target | SDK / Runtime Basis | Next Phase |
|---|---|---|---|---|---|
| Navigation | `NavHome`, `NavMotion`, `NavPoints`, `NavStatus`, `NavHelp`, bottom tabs | `wired` | shell state, panel host, bottom sheet state 유지 | V3 shell state | maintain |
| I/O Navigation | `NavIo`, `BottomTabIo` | `wired` | live SDK method enable after safety gate | mock/live-gated peripheral facade | Phase 3 polish |
| Connect | `BtnConnect`, `BtnDisconnect`, primary/quick action | `wired` | connect/disconnect + top/status refresh | `FairinoConnectionService` | maintain |
| Servo / Sync / Reset / Stop | `BtnServoEnable`, `BtnSync`, `BtnResetError`, `BtnStop`, `BtnStopBottom` | `wired` | popup confirm 뒤 runtime action 유지 | `Enable`, `SyncCurrentState`, `ResetErrors`, `StopMotion` | Phase 1/2 verify |
| Run | `BtnRun`, `BtnRunBottom` | `partial` | pending command 또는 sequence가 있을 때만 실행 | `ExecutePrimaryAction`, future command queue | Phase 4 |
| Pause | `BtnPause` | `partial` | running sequence pause/resume only | `WaypointCycleRunner` | Phase 4 |
| DryRun | `BtnDryRun` | `wired` | UI chip/status refresh까지 검증 | `RobotControlV3RuntimeSnapshot.DryRunEnabled` | maintain |
| Undo / Redo | `BtnUndo`, `BtnRedo` | `partial` | preview history와 command history 분리 | preview history, future command history | Phase 4 |
| Step | `BtnStepBack`, `BtnStepForward` | `partial` | preview step과 program step 의미 분리 | preview undo/redo, future sequence step | Phase 4 |
| Coordinate / Increment | `BtnCoordSystem`, `BtnIncrement` | `wired` | all panels share same local state | `PendantV3LocalState` | maintain |
| Easy Presets | `BtnEasyHome`, `BtnEasyReady`, `BtnEasyFolded` | `wired` | preview/apply -> RobotStage/Mock state | `PreviewPreset`, `ApplyPreset` | Phase 1 verify |
| Easy Zero | `BtnEasyZero` | `wired` | `Home` alias 제거, Zero 독립 preset | FR5 pose presets | maintain |
| Gripper | `BtnGripperOpen`, `BtnGripperClose`, I/O gripper buttons | `wired` | attach PGEA prefab + live SDK command after safety gate | robottemplete `FR5EndEffectorAttachment.SetGripperOpen` + FAIRINO `SetGripperConfig/ActGripper/MoveGripper/GetGripper*` | Phase 3 polish |
| Joint Jog | J1~J6 slider/input/`J-`/`J+`, `BtnJointPreview`, `BtnJointApply`, `BtnJointRestore` | `wired` | all axes apply/preview verified against visual pose | `PreviewJointAngles`, `ApplyJointAngles`, MoveJ | Phase 1 verify |
| TCP Jog | `X/Y/Z/RX/RY/RZ -/+`, coord buttons, `BtnTcpPreview`, `BtnTcpApply` | `wired` | panel + overlay share one runtime path | `PreviewTcpPose`, `ApplyTcpPose`, MoveL | Phase 1 verify |
| Cartesian Overlay | `BtnArrow1~6Minus/Plus` | `wired` | same as TCP jog, no main-panel obstruction | `TcpJogController.AdjustAxis` | Phase 1 verify |
| Point MoveL | `BtnPointMoveL`, `BtnPointPreview`, `BtnPointApply` | `wired` | save/select/apply flow 추가 | `PreviewTcpPose`, `ApplyTcpPose`, `DispatchMoveL` | Phase 2 |
| Point MoveJ | `BtnPointMoveJ`, `BtnPointPreview`, `BtnPointApply` | `wired` | production IK policy + saved point list UX | saved joint target first, numerical XYZ IK fallback | Phase 2 polish |
| Point Save / Recall / Delete / Rename / Export / Cleanup | `BtnPointSave`, `BtnPointRecall`, `BtnPointDelete`, `BtnPointRename`, `BtnPointExport`, `BtnPointCleanup`, point list rows | `wired` | delete confirm + import UX | `WaypointStore` sequence `PendantV3Points` | Phase 2 polish |
| CoordStrip Mode | `BtnCoordModeJoint`, `BtnCoordModeTcp`, `BtnCoordModeBoth` | `wired` | actual Joint/TCP/Both 표시 전환 | `StatusCardController` mode state | maintain |
| Status Details | `BtnFaultDetail`, `BtnSafetyDetail` | `partial` | help route + context detail deep link | Help panel | Phase 2 polish |
| Safety Recovery | `BtnRecoveryPrimary`, fault overlay reset/close | `partial` | action policy + reset/close semantics 정리 | `ExecutePrimaryAction`, `ResetErrors` | Phase 2 |
| View Toolbar | `Base`, `Tool`, `Path`, `Ghost`, `Bound`, `Coll`, `Cam` | `partial` | frame/ghost/path/bound/collision real visual state | visualization helpers | Phase 5 |
| Program Blocks | none yet | `pending` | MoveJ/MoveL/MoveC/Wait/IO block queue | future program model | Phase 4/5 |
| Live SDK Gate | no UI gate yet | `pending` | SDK/robot version, speed, E-stop, confirm checklist | `LiveFairinoClient`, official SDK | Phase 6 |

## Phase Plan

### Phase 1: Core Meaning Lock
- `Zero`를 `Home` alias에서 분리하고 독립 preset으로 둔다.
- `CoordStrip` mode buttons를 실제 표시 모드 전환에 연결한다.
- `Run`, `Step`, `Undo/Redo` 라벨과 runtime feedback의 의미를 정리한다.
- Easy/Joint/TCP/Cartesian 대표 버튼은 Mock/RobotStage state 변화로 재검증한다.

### Phase 2: Point / Safety Completion
- Point store v1을 추가한다: save current, list, select, restore values, preview, apply.
- MoveJ는 joint target이 있는 point에서만 dispatch한다.
- Safety recovery/fault detail buttons의 정책을 명확히 한다.
- Status detail은 Help panel deep link 또는 detail card로 연결한다.

### Phase 3: I/O + Gripper
- `NavIo`/`BottomTabIo`에 I/O panel을 추가한다.
- Mock DO/ToolDO/Gripper state를 runtime snapshot에 포함한다.
- Live는 공식 SDK 대응 method 확인된 기능만 enable하고 미지원 기능은 disabled reason을 표시한다.

### Phase 4: Run / Step / History
- Pending command queue를 만든다.
- `Run`은 pending command 또는 sequence가 있을 때만 실행한다.
- `StepBack/StepForward`는 sequence step과 preview step을 분리한다.
- Undo/Redo는 preview history와 command history를 분리한다.

### Phase 5: Boundary / Collision / Path
- `Bound`는 실제 workspace boundary visual과 연결한다.
- `Coll`은 predicted path collision state를 읽는다.
- Collision danger에서는 apply/run을 silent execute하지 않는다.

### Phase 6: Live SDK Gate
- Live 이동 전 SDK version, robot software version, safety state, speed, E-stop, DryRun preview를 모두 확인한다.
- Mock e2e 전부 green 전에는 Live MoveJ/MoveL/IO/Gripper 실행 금지다.

## Verification Rules
- 각 Phase 시작 전 `unityctl check --type compile --json`.
- 모든 보조패널 검증에서 `GetAuxLayoutSummaryForDebug()` 기준 `HorizontalVisible=False`, `Clipped=0` 유지.
- 모든 조작 버튼은 전후 `GetMovementStateSummaryForDebug()` 또는 기존 summary method로 상태 변화가 확인되어야 한다.
- RobotStage는 `front / side / top / iso` 중 최소 2개 각도에서 visual 변화가 캡처되어야 한다.
- Live Phase 전까지 `LiveFairinoClient.MoveJ/MoveL`을 실제 호출하는 테스트는 금지한다.

## First Implementation Order
1. `[done]` `GetMovementStateSummaryForDebug()` 추가.
2. `[done]` `Zero` preset 분리.
3. `[done]` `CoordStrip` mode buttons 연결.
4. `[done]` Easy/Joint/TCP/Cartesian 대표 버튼 전후 state matrix 검증.
5. `[done]` Point MoveL DryRun preview/apply 연결 및 검증.
6. `[done]` Point MoveJ numerical XYZ IK 기반 preview/dry-run apply 1차 연결.
7. `[done]` Point 저장/호출에서 saved joint target을 MoveJ에 우선 사용.
8. `[done]` Point list/select/delete UX 최소 연결.
9. `[done]` Point rename/export and persistence cleanup policy.
10. `[done]` I/O/Gripper mock/live-gated state facade 설계 및 1차 연결.
11. `[done]` PGEA attached visual prefab 이관/연결.
12. `[done]` live SDK gripper capability/readback contract scaffold.
13. `[next]` 실기 gripper command safety gate + pendant/SDK readback comparison.

## 2026-04-21 Phase 1 Start Result
- `GetMovementStateSummaryForDebug()` callable 노출 확인.
- `PreviewEasyMotionForDebug("Zero")` 후 `pending=대기 명령: MoveJ`, `feedback=[Preview] Zero 프리셋`, `ghost=True`, `path=True` 확인.
- `SetCoordStripModeForDebug("Joint")`: `jointHidden=False`, `tcpHidden=True`
- `SetCoordStripModeForDebug("TCP")`: `jointHidden=True`, `tcpHidden=False`
- `SetCoordStripModeForDebug("Both")`: `jointHidden=False`, `tcpHidden=False`

## 2026-04-21 Phase 1 Verification Result
- Unity 재시작 후 `unityctl check --type compile --json` 통과. `script get-errors` 기준 error 0, warning 2.
- Easy `Zero`: `joints=[0.0,0.0,0.0,0.0,0.0,0.0]`, `pending=대기 명령: MoveJ`, `ghost=True`, `path=True`.
- Joint input/slider: J1 input `12.5`, J2 slider `-7.5`가 보조패널 row summary와 runtime `joints=[12.5,-7.5,...]`에 동시에 반영됨.
- TCP jog: `RZ+`가 `pending=대기 명령: MoveL`, `tcp=[...,95.0]`, `path=True`, `ghost=False`로 반영됨.
- Point MoveL: disconnected DryRun에서도 `canPreview=True`, `canApply=True`, preview는 `pending=대기 명령: MoveL`, apply는 `[DryRun Apply] 포인트 MoveL 적용`으로 반영됨.
- Point MoveJ: preview 버튼은 열려 있으나 아직 IK/joint target이 없어 실제 MoveJ dispatch가 아니다. Phase 2에서 joint target point와 IK 결과를 붙일 때까지 `partial`로 유지한다.
- Layout guard: `GetAuxLayoutSummaryForDebug()` 기준 visible layout에서 `horizontalVisible=False`, `clipped=0` 정책 유지. 도메인 리로드 직후 `NaN` summary가 나올 수 있으므로 씬 재오픈/부트스트랩 후 측정한다.

## 2026-04-21 Phase 2 MoveJ First Slice
- 자기리뷰 결과:
  - `PointMoveController`는 실기 클라이언트를 직접 만지지 않고 `RobotControlV3RuntimeController` App facade만 호출한다.
  - `RobotControlV3RuntimeController`가 RobotStage preview/path/ghost의 SSOT이므로 Point Move preview도 이 facade를 통해 일관되게 갱신한다.
  - Live 실기 이동은 기존 `ApplyJointAngles` / `RobotControlMotionRuntime.DispatchMoveJ` 경로를 재사용하고, Phase 6 전까지 실기 실행 테스트는 금지한다.
- 구현:
  - `PreviewPointMoveJ(tcpPose)` / `ApplyPointMoveJ(tcpPose)` 추가.
  - FK 기반 coordinate-descent numerical IK로 목표 XYZ에 가까운 6축 joint target을 찾는다.
  - 성공 시 MoveJ preview는 `previewJointAnglesDeg`를 사용하므로 `ghost=True`, `path=True`, `pending=대기 명령: MoveJ`가 된다.
  - 실패 시 `Point MoveJ IK 실패 · 위치 오차 ...mm` feedback으로 막는다.
- 한계:
  - 현재 IK는 XYZ 위치 근사용이다. RX/RY/RZ orientation 정합, 다중해 선택, 특이점/충돌/관절 여유 최적화는 아직 production IK가 아니다.
  - Point 저장/호출이 없으므로 saved joint target 우선 정책은 다음 단계로 남긴다.
- 검증:
  - `unityctl check --type compile --json`: pass.
  - `FR5PosePresetsTests`: 11 passed.
  - `Point MoveJ` debug preview: `pending=대기 명령: MoveJ`, `ghost=True`, `path=True`.
  - `Point MoveJ` debug apply: `[DryRun Apply] 포인트 MoveJ 적용`.

## 2026-04-21 Phase 2 Point Save / Recall
- 구현:
  - `PointMoveController`에 Save/Recall 버튼과 `PointStoreSummary`를 추가했다.
  - 저장소는 기존 `WaypointStore`를 재사용하고, V3 전용 sequence 이름은 `PendantV3Points`로 둔다.
  - 저장 시 `tcpMm`, `jointsDeg`, `moveType`을 같이 저장한다.
  - recall 후 MoveJ preview/apply는 numerical IK보다 saved `jointsDeg`를 우선 사용한다.
- 검증:
  - `P_SAVE` 저장 후 X 값을 변경하고 `RecallPointMoveForDebug("P_SAVE")`로 저장값 복원 확인.
  - recall 후 `PreviewPointMoveForDebug()` 결과 feedback에 `saved joint target 사용` 확인.
  - runtime summary: `pending=대기 명령: MoveJ`, `ghost=True`, `path=True`.
  - saved MoveJ apply 후 `[DryRun Apply] 포인트 MoveJ 적용` 확인.
- 남은 리스크:
  - 현재 UI는 이름 기반 1개 recall 흐름만 있다. 여러 포인트 목록 표시, 선택, 삭제, rename은 Phase 2 polish로 남긴다.
  - 저장 파일은 Unity `Application.persistentDataPath/waypoints/PendantV3Points.json`에 생성된다.

## 2026-04-21 Phase 2 Point List / Select / Delete
- 구현:
  - 저장 포인트 목록을 `PointListContainer`에 동적 row button으로 표시한다.
  - row 선택 또는 `BtnPointRecall`은 해당 point의 TCP/move type을 복원한다.
  - `BtnPointDelete`는 이름 기준으로 point를 삭제하고 active selection을 해제한다.
  - 리스트는 세로 full-width row로 고정해 보조패널 가로 스크롤을 다시 만들지 않는다.
- 검증:
  - `P_A`, `P_B` 저장 후 list summary가 `count=3; active=P_B; points=[P_SAVE:MoveJ,P_A:MoveJ,P_B:MoveL]`로 갱신됨.
  - `RecallPointMoveForDebug("P_A")` 후 `active=P_A`.
  - `DeletePointMoveForDebug("P_A")` 후 list summary가 `count=2; active=none; points=[P_SAVE:MoveJ,P_B:MoveL]`.
  - `RecallPointMoveForDebug("P_B")` 후 `motion=MoveL`, `name=P_B`.
  - CSS 보정 후 `GetAuxLayoutSummaryForDebug()`는 edit-mode bounds가 `NaN`이지만 `horizontalVisible=False`, `clipped=0`으로 회귀 없음.
- 남은 리스크:
  - rename/export, 저장 파일 정리 정책, 사용자 친화적인 empty-state/confirm delete는 후속이다.

## 2026-04-21 Phase 2 Point Rename / Export / Cleanup
- 구현:
  - `BtnPointRename`, `BtnPointExport`, `BtnPointCleanup` 추가.
  - Rename은 active/recalled point를 현재 입력 이름으로 변경한다.
  - Export는 `Application.persistentDataPath/waypoints/PendantV3Points.export.json`으로 현재 sequence를 내보낸다.
  - Cleanup은 `PendantV3Points.json` sequence 파일을 삭제하고 active selection을 해제한다.
- 검증:
  - `P_RENAME_A` 저장 후 `RenamePointMoveForDebug("P_RENAME_A","P_RENAMED")` 성공.
  - list summary: `active=P_RENAMED`, `P_RENAMED:MoveJ` 포함.
  - `ExportPointMoveForDebug()` 후 export file path feedback 확인.
  - `CleanupPointMoveForDebug()` 후 list summary `count=0; active=none; points=[]`.
- 남은 리스크:
  - delete/cleanup confirm popup과 import UX는 아직 없다.

## 2026-04-21 Phase 3 I/O + Gripper First Slice
- `C:\Users\ezen601\Desktop\Jason\robottemplete`에서 `git pull --ff-only` 실행: `Already up to date`.
- 참고한 성공 케이스:
  - `FR5EndEffectorAttachment.SetGripperOpen(float ratio)`: 3파트 PGEA gripper finger transform 제어.
  - `docs/END-EFFECTOR-PGEA10040.md`: ToolMount identity, visual alignment와 TCP frame 분리, SDK/ROS gripper command는 후속으로 명시.
- 구현:
  - `RobotControlPeripheralFacade`와 `RobotControlPeripheralState` 추가.
  - Mock/DryRun에서는 Gripper, DO0/DO1, ToolDO0/ToolDO1 상태를 갱신한다.
  - Live에서는 SDK contract가 열릴 때까지 `live blocked: I/O/Gripper SDK contract not enabled`로 차단한다.
  - `IoPanelController`를 추가해 `NavIo`/`BottomTabIo`에서 I/O + Gripper 보조패널을 표시한다.
  - `EasyMotionController`의 그리퍼 버튼도 같은 runtime facade를 탄다.
  - `FR5EndEffectorAttachment`를 추가해 PGEA attached prefab이 있을 때 finger visual open/close를 적용한다.
- 검증:
  - `GetPanelControllerSummary`: `io=1`, `io=[initialized=True; desktopVisible=True; tabletVisible=True...]`.
  - `SetGripperOpenForDebug(true)`: `Gripper: Open (1.00)`.
  - `SetToolDoForDebug(0,true)`: `ToolDO0 ON / ToolDO1 OFF`.
  - `SetRobotDoForDebug(1,true)`: `DO0 OFF / DO1 ON`.
  - `unityctl check --type compile --json`: pass.
- 남은 리스크:
  - 현재 `robotapp2` control prefab에 PGEA attached visual이 없으면 `SetGripperOpen`은 state만 바꾸고 visual은 no-op이다.
  - live SDK/ROS gripper command는 robottemplete에서도 미착수로 문서화돼 있어, 아직 실제 실기 명령은 보내지 않는다.

## Next Visual Step Lock
- PGEA visual 이관 완료.
- 가져온 기준:
  - `robottemplete/Assets/Runtime/Resources/EndEffectors/PGEA_100_40.prefab`
  - `robottemplete/Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control_PGEA10040.prefab`
  - `ToolMount -> PGEA_100_40` 구조
  - `FR5EndEffectorAttachment`의 `finger_left/finger_right` 참조와 `SetGripperOpen(float)` 동작
- 기준값:
  - `FAIRINO_FR5_Control_PGEA10040.prefab`의 nested override를 SSOT로 삼는다.
  - 단독 `EndEffectors/PGEA_100_40.prefab`의 기본 `TcpFrame=(0,0,0)`은 운용 기준이 아니다.
  - `ToolMount`: identity.
  - `PGEA_100_40`: local position `(0.003, 0.1676, 0.031)`, local rotation quaternion `(0, 0, -0.7169106, 0.69716513)`.
  - `TcpFrame`: local position `(-0.0677, 0, -0.0325)`, local rotation identity.
  - `PGEA-100-40_Model`: local z `-0.031`.
  - 이 `TcpFrame`은 사용자가 `robottemplete`에서 수동 미세조정한 닫힌 finger 기준 작업점으로 취급한다.
- 구현 결과:
  - `robotapp2` RobotStage control robot의 `wrist3_link/ToolMount` 아래에 PGEA visual을 런타임 부착한다.
  - `SetGripperOpenForDebug(true/false)`에서 snapshot과 `FR5EndEffectorAttachment` finger transform을 함께 갱신한다.
  - 런타임 부착 시 위 Control prefab 기준값을 강제 적용해, scene visual과 debug `TcpFrame`이 같은 기준을 보게 한다.
  - 실기 SDK calibration은 아직 pending이며, 다음 단계에서 live pendant/SDK readback과 비교해 이 값이 실제 작업 TCP와 맞는지 확정한다.
- 검증:
  - `SetGripperOpenForDebug(true)`: `gripper=Gripper: Open (1.00); gripperVisual=True`
  - `SetGripperOpenForDebug(false)`: `gripper=Gripper: Closed (0.00); gripperVisual=True`
  - `GetGripperVisualSummaryForDebug()` closed 기준: `tcpLocal=(-0.0677,0,-0.0325)`, `modelLocal=(0.0065,0.3256,-0.031)`, `cameraVisible=True`
  - `unityctl check --type compile --json`: pass

## 2026-04-21 Phase 3 Live SDK Gripper Contract
- 공식 문서 기준:
  - gripper 설정: `SetGripperConfig(company, device, softversion, bus)`.
  - gripper reset/activate: `ActGripper(index, action)`.
  - gripper 이동: `MoveGripper(index, pos, vel, force, max_time, block, type, rotNum, rotVel, rotTorque)`.
  - readback: `GetGripperMotionDone`, `GetGripperActivateStatus`, `GetGripperCurPosition`, `GetGripperCurSpeed`, `GetGripperCurCurrent`, `GetGripperVoltage`, `GetGripperTemp`.
- 로컬 PGEA 후보 프로필:
  - `company=4`, `device=0`, `soft=0`, `bus=2`, `index=2`.
  - Open 후보 command: `pos=100`, `vel=50`, `force=50`, `max=30000`, `block=True`.
  - Close 후보 command: `pos=0`, `vel=50`, `force=50`, `max=30000`, `block=True`.
- 구현:
  - `IFairinoRobotClient`에 gripper capability/readback/config/activate/move 계약 추가.
  - `LiveFairinoClient`는 reflection으로 위 공식 SDK 메서드 존재 여부를 probe하고 readback을 읽는다.
  - `RobotControlPeripheralFacade`는 mock 연결 시 공식 command path를 같이 시뮬레이션한다.
  - Live 연결에서는 안전 게이트 전까지 버튼 실행은 계속 blocked 상태로 유지한다.
- 검증:
  - `unityctl check --type compile --json`: pass.
  - `GetGripperSdkSummaryForDebug(true)` mock connected:
    - `configure=True`, `activate=True`, `move=True`, `pos=True`, `current=True`, `voltage=True`, `temp=True`.
    - 초기 readback `position=0`.
  - `SetGripperOpenForDebug(true)` 후:
    - visual `Gripper: Open (1.00)`.
    - SDK mock readback `activationMask=1`, `position=100`, `speed=50`, `current=50`.
- 다음 단계:
  - 실제 FR5 연결 후 `GetGripperSdkSummaryForDebug(true)`로 SDK method/readback만 먼저 확인한다.
  - 사용자가 현장 안전을 확인하기 전까지 `MoveGripper` live 실행은 열지 않는다.

## 2026-04-21 Robot-linked Button Simulation Audit
- 목적:
  - 실기기 테스트 전, 화면에 보이는 로봇 연동 버튼들이 Mock/RobotStage/SDK-comparison state에서 시뮬레이션되는지 확인한다.
  - 동일 runtime path를 공유하는 버튼은 묶어서 검증한다.
    - 예: `BtnStop` / `BtnStopBottom`
    - 예: `BtnTcpXPlus` / `BtnArrowXPlus`
- 자기리뷰:
  - `SDK gripper pos=100`을 visual finger open ratio로 바로 연결하면 템플릿에서 수동 조정한 닫힌 finger TCP 기준이 깨진다.
  - 따라서 live 방향성 검증 전까지 visual finger는 닫힌 기준으로 고정하고, SDK mock/readback은 `GripperSdkSummary`로 분리한다.
  - `PointMoveController`는 `NavMotion + TabPointMove`에서 열리는 계약이다. `NavPoints`는 shell navigation label일 수 있으나 PointMove panel visibility 기준이 아니다.
  - audit은 각 버튼당 두 관점 이상을 본다.
    - runtime movement summary
    - visual gripper/stage summary
    - SDK gripper summary
    - joint row summary
    - point list summary
    - layout summary
- 구현:
  - `RunRobotLinkedButtonSimulationAuditForDebug()` 추가.
  - `NudgeJointForDebug()` 추가.
  - gripper summary는 `Cmd Open/Close`와 `Visual Closed/Open`을 분리해서 표시한다.
- 검증 결과:
  - `unityctl check --type compile --json`: pass.
  - `RunRobotLinkedButtonSimulationAuditForDebug()`: `pass=74; fail=0`.
  - 포함 영역:
    - Connect / Servo / Sync / Stop / Pause / DryRun
    - Easy Home / Ready / Folded / Zero / Apply
    - Gripper Open / Close
    - Joint J1~J6 +/- / Preview / Apply / Restore
    - TCP X/Y/Z/RX/RY/RZ +/- / coord / preview / apply
    - Point MoveL/MoveJ / Preview / Apply / Save / Recall / Rename / Export / Delete / Cleanup
    - Robot DO0/DO1, ToolDO0/ToolDO1
    - Viewport Base/Tool/Trail/Ghost/Boundary/Collision/Camera
    - CoordStrip Joint/TCP/Both
- 실기 전 남은 조건:
  - `RunRobotLinkedButtonSimulationAuditForDebug()`가 계속 green이어야 한다.
  - 실제 FR5에서는 먼저 `GetGripperSdkSummaryForDebug(true)` readback만 수행한다.
  - pendant gripper direction과 SDK `pos=0/100` 의미가 확인되기 전까지 visual finger open/close를 live truth로 쓰지 않는다.

## 2026-04-21 Actual UI Click E2E
- 범위:
  - `DebugBridge` 직접 호출이 아니라 `unityctl uitk click`로 실제 UI Toolkit 버튼을 눌렀다.
  - 목적은 `ClickEvent -> UI controller -> RobotControlV3RuntimeController -> Mock/RobotStage state`가 이어지는지 확인하는 것이다.
- 발견:
  - `Button.clicked` 기반 동적 버튼은 runtime path가 있어도 `unityctl uitk click` 검증에서 실행되지 않는 경우가 있었다.
  - top bar `Servo / Run / Stop / Reset`은 안전 popup scaffold와 별개로 runtime 진입점이 명확해야 했다.
  - I/O 동적 버튼은 stable name이 없어 duplicate desktop/tablet 버튼 중 desktop locator 검증이 불안정했다.
- 수정:
  - `ConnectionHomeController` runtime 버튼들을 `RegisterCallback<ClickEvent>`로 바꾸고 `Servo / Run / Stop / Reset` handlers를 추가했다.
  - `EasyMotionController`, `JointJogController`, `IoPanelController`의 로봇 연동 동적 버튼도 `RegisterCallback<ClickEvent>`로 통일했다.
  - I/O 버튼에 stable name을 부여했다.
- 실제 클릭 결과:
  - `BtnConnect` 클릭 후 `connected=True`, `enabled=False`.
  - `BtnServoEnable` 클릭 후 `connected=True`, `enabled=True`.
  - `NavMotion -> TabPointMove -> BtnPointPreview` 클릭 후 `pending=대기 명령: MoveJ`, `ghost=True`, `path=True`.
  - `BtnPointApply` 클릭 후 `[DryRun Apply] 포인트 MoveJ 적용`, `pending=대기 중인 명령 없음`.
  - `NavIo -> BtnRobotDo0On` 클릭 후 `robotDo=DO0 ON / DO1 OFF`.
  - `BtnToolDo0On` 클릭 후 `toolDo=ToolDO0 ON / ToolDO1 OFF`.
  - `BtnIoGripperOpen` 클릭 후 SDK mock readback `position=100`.
  - `BtnIoGripperClose` 클릭 후 SDK mock readback `position=0`.
- 최종 게이트:
  - `unityctl check --type compile --json`: pass.
  - `console get-count`: error 0.
  - `RunRobotLinkedButtonSimulationAuditForDebug()`: `pass=74; fail=0`.
- 계약:
  - PointMove 버튼은 `NavMotion + TabPointMove`에서 활성화된다.
  - `NavIo`에서 Point 버튼이 disabled처럼 보이는 상태는 panel visibility 계약상 정상이다.

## Missing / Pending Tests Before Live Robot
- `P0 필수`: actual UI click full matrix.
  - 현재는 대표 actual click + 전체 runtime audit 조합이다.
  - 다음에는 74개 로봇 연동 버튼을 desktop locator 기준으로 자동 순회하고, 각 버튼마다 before/after summary를 저장한다.
- `P0 필수`: tablet/bottom sheet actual click matrix.
  - `BottomTabHome/Motion/Point/Io`, tablet sheet의 Easy/Joint/TCP/Point/I/O 버튼을 별도 locator로 검증한다.
  - desktop과 같은 runtime path를 타더라도 tablet hit target/visibility는 별도 UX 리스크다.
- `P0 필수`: popup confirm/cancel E2E.
  - `Servo`, `Run`, `Reset`, `Stop`, fault recovery popup의 open/cancel/confirm 경로를 실제 클릭으로 확인한다.
  - runtime handler가 동작해도 popup policy가 깨지면 pendant UX로는 실패다.
- `P0 필수`: RobotStage screenshot evidence.
  - Joint jog, TCP jog, Point preview/apply 후 `front / side / iso` 중 2개 이상에서 robot pose, ghost, predicted path가 보이는지 캡처한다.
- `P1 후속`: viewport toolbar actual click matrix.
  - `Base / Tool / Path / Ghost / Bound / Coll / Cam` 화면 클릭 후 stage flag, layout summary, clipped count를 함께 확인한다.
- `P1 후속`: Point MoveJ production behavior.
  - saved joint target 우선 경로와 numerical IK fallback을 분리해 테스트한다.
  - orientation, unreachable target, joint limit, singularity, collision guard는 아직 production gate가 아니다.
- `P1 후속`: Safety/fault state actual flow.
  - fault preview/forced fault 상태에서 recovery/detail/reset/close 버튼의 visual state와 runtime state를 검증한다.
- `P2 실기 게이트`: live SDK readback-only.
  - 실제 FR5에서는 먼저 `GetGripperSdkSummaryForDebug(true)`만 수행한다.
  - pendant readback과 SDK readback의 gripper activation, position, speed, current, voltage, temperature를 비교한다.
- `P2 실기 게이트`: live movement/IO command.
  - `MoveJ`, `MoveL`, DO/ToolDO, `MoveGripper` live 실행은 safety checklist, speed limit, E-stop, operator confirm, DryRun preview evidence가 모두 준비된 뒤에만 연다.
