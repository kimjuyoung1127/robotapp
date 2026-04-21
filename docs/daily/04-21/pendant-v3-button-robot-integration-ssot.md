# Pendant V3 Button-Robot Integration SSOT

## Summary
- `robot-button-integration-plan.md`를 새 기준 문서로 추가했다.
- 목적은 V3의 모든 버튼이 로봇/Mock/RobotStage와 어디까지 연결됐는지 한 표에서 비교하는 것이다.
- 앞으로 구현은 이 문서를 먼저 보고, 버튼 상태를 `wired / partial / stub / pending / excluded`로 갱신하면서 진행한다.

## 기준
- V2 성공패턴은 구조, 상태관리, 검증 루프 참고용이다.
- 실제 기능 범위는 FAIRINO 공식 SDK 기능군과 `Assets/Scripts/App/Fairino` 계약을 기준으로 한다.
- V3 레이아웃 잠금은 계속 유지한다.
  - 메인 `RobotStage`는 로봇 표시 전용
  - 조작 버튼은 보조패널/하단바/오른쪽 패널
  - 가로 스크롤 금지

## High Priority Gaps
- `I/O`: `NavIo`, `BottomTabIo` 버튼은 있으나 실제 I/O panel/runtime path 없음.
- `Run / Step`: program queue/step 실행 미구현.
- `Point MoveJ`: numerical XYZ IK preview/apply 1차는 연결됐지만, saved joint target과 production IK 정책은 아직 필요하다.
- `Point 저장/호출`: save/list/select/apply 플로우 없음.
- `Gripper`: 버튼은 있으나 현재 simulate feedback 수준.
- `Boundary / Collision`: toolbar scaffold만 있고 실데이터 미연동.
- `CoordStrip Joint/TCP/Both`: 실제 표시 모드 전환 핸들러 연결 완료.
- `Zero`: Easy Zero 독립 preset 연결 완료.

## Next Order
1. `[done]` `GetMovementStateSummaryForDebug()` 추가.
2. `[done]` Easy `Zero` 독립 preset 분리.
3. `[done]` `CoordStrip` mode buttons 실제 연결.
4. `[done]` Easy/Joint/TCP/Cartesian 대표 전후 state matrix 검증.
5. `[done]` Point MoveL DryRun preview/apply 연결.
6. `[done]` Point MoveJ numerical XYZ IK 기반 preview/dry-run apply 1차 연결.
7. `[done]` Point 저장/호출 + saved joint target 우선 MoveJ.
8. `[done]` Point list/select/delete UX 최소 연결.
9. `[done]` Point rename/export/persistence cleanup.
10. `[done]` I/O/Gripper mock/live-gated state facade 1차 연결.
11. `[done]` PGEA attached visual prefab 이관/연결.
12. `[next]` live SDK/ROS command contract.
13. `[done]` FAIRINO live SDK gripper capability/readback scaffold.
14. `[next]` 실기 연결 후 pendant/SDK gripper readback 비교.

## Phase 1 Start Verification
- Unity 재시작 후 `RobotControlV3DebugBridge` callable 목록에 `GetMovementStateSummaryForDebug`, `SetCoordStripModeForDebug` 노출 확인.
- `PreviewEasyMotionForDebug("Zero")` 후 `pending=대기 명령: MoveJ`, `feedback=[Preview] Zero 프리셋`, `ghost=True`, `path=True`.
- `SetCoordStripModeForDebug("Joint")` -> `jointHidden=False`, `tcpHidden=True`.
- `SetCoordStripModeForDebug("TCP")` -> `jointHidden=True`, `tcpHidden=False`.
- `SetCoordStripModeForDebug("Both")` -> `jointHidden=False`, `tcpHidden=False`.

## Phase 1 Follow-up Verification
- `RobotControlV3RuntimeController.RefreshSnapshot()`이 joint preview target을 snapshot `JointValues`로 내보내게 수정했다.
- 이 수정으로 `JointJogController` row summary와 runtime summary가 서로 밀리지 않는다.
- J1 input `12.5`, J2 slider `-7.5` 검증 결과 row summary와 `GetMovementStateSummaryForDebug()`의 `joints`가 일치했다.
- `PointMoveController`는 disconnected DryRun에서도 preview/apply를 허용하도록 맞췄다.
- MoveL point preview/apply는 runtime `MoveL` path와 `[DryRun Apply]` feedback까지 확인했다.
- MoveJ point preview는 아직 IK 기반 MoveJ가 아니므로 Phase 2 gap으로 남긴다.

## Phase 2 MoveJ First Slice
- 자기리뷰:
  - UI는 실기 클라이언트를 직접 호출하지 않는다.
  - Point Move preview/apply는 RobotStage SSOT인 `RobotControlV3RuntimeController` App facade를 호출한다.
  - 이 경로는 `ApplyJointAngles` / `ApplyTcpPose`를 재사용하므로 live/mock boundary가 분산되지 않는다.
- 구현:
  - `RobotControlV3RuntimeController.PreviewPointMoveJ()`와 `ApplyPointMoveJ()` 추가.
  - 목표 TCP XYZ를 FK 기반 numerical IK로 joint target에 근사한다.
  - MoveJ preview는 `pending=대기 명령: MoveJ`, `ghost=True`, `path=True`로 표시된다.
  - MoveJ apply는 DryRun에서 `[DryRun Apply] 포인트 MoveJ 적용`으로 닫힌다.
- 검증:
  - `unityctl check --type compile --json`: pass.
  - `FR5PosePresetsTests`: 11 passed.
  - RobotControlV3 direct scene debug에서 Point MoveJ preview/apply 확인.
- 남은 리스크:
  - 현재 IK는 XYZ 근사용이다. RX/RY/RZ orientation, 다중해, singularity, collision, 상용 수준의 teaching pendant IK 정책은 후속이다.
  - Point 저장/호출이 없어서 saved joint target 우선 MoveJ는 아직 미구현이다.

## Phase 2 Point Save / Recall
- 기존 `WaypointStore`를 재사용해서 V3 PointMove 전용 sequence `PendantV3Points`를 저장한다.
- 저장 버튼/디버그 경로는 현재 point name, TCP, runtime joint snapshot, move type을 함께 저장한다.
- 불러오기 버튼/디버그 경로는 이름으로 point를 찾아 TCP 입력값과 move type을 복원한다.
- recall된 point가 현재 point name/TCP와 일치하면 MoveJ preview/apply는 numerical IK 대신 saved `jointsDeg`를 우선 사용한다.
- 검증 결과:
  - `P_SAVE` 저장 후 X 값을 변경하고 recall하면 저장된 X/TCP 값으로 복원됨.
  - recall 후 MoveJ preview feedback: `saved joint target 사용`.
  - runtime summary: `pending=대기 명령: MoveJ`, `ghost=True`, `path=True`.
  - MoveJ apply: `[DryRun Apply] 포인트 MoveJ 적용`.
- 남은 리스크:
  - 현재 UI는 이름 기반 save/recall 최소형이다.
  - 여러 point 목록 표시, 선택, 삭제, rename, 저장 파일 정리 정책은 다음 polish 범위다.

## Phase 2 Point List / Select / Delete
- `PointListContainer`를 추가하고 저장 point를 동적 row button으로 표시한다.
- row 선택과 `BtnPointRecall`은 같은 recall 경로를 탄다.
- `BtnPointDelete`는 이름 기준으로 저장 point를 삭제하고 active selection을 해제한다.
- 리스트는 세로 full-width row로 고정해서 보조패널 가로 스크롤을 복구하지 않는다.
- 검증 결과:
  - `P_A`, `P_B` 저장 후 list summary가 `P_SAVE`, `P_A`, `P_B`를 표시.
  - `P_A` recall 후 `active=P_A`.
  - `P_A` delete 후 list summary에서 `P_A` 제거.
  - `P_B` recall 후 `motion=MoveL`, `name=P_B`.
  - CSS 보정 후 `GetAuxLayoutSummaryForDebug()`는 edit-mode bounds가 `NaN`이나 `horizontalVisible=False`, `clipped=0`.
- 남은 리스크:
  - rename/export, delete confirm, 저장 파일 정리 정책은 후속이다.

## Phase 2 Point Rename / Export / Cleanup
- `BtnPointRename`, `BtnPointExport`, `BtnPointCleanup`을 추가했다.
- Rename은 active/recalled point 이름을 현재 입력 이름으로 변경한다.
- Export는 `Application.persistentDataPath/waypoints/PendantV3Points.export.json`으로 내보낸다.
- Cleanup은 `PendantV3Points.json` sequence 파일을 삭제하고 active selection을 해제한다.
- 검증 결과:
  - `P_RENAME_A -> P_RENAMED` rename 성공.
  - list summary가 `active=P_RENAMED`, `P_RENAMED:MoveJ`를 표시.
  - export feedback에 `.export.json` 경로 표시.
  - cleanup 후 list summary `count=0; active=none; points=[]`.
- 남은 리스크:
  - delete/cleanup confirm popup과 import UX는 아직 없다.

## Phase 3 I/O + Gripper First Slice
- `C:\Users\ezen601\Desktop\Jason\robottemplete`에서 `git pull --ff-only` 실행 결과 `Already up to date`.
- 성공 케이스로 가져온 핵심:
  - `FR5EndEffectorAttachment.SetGripperOpen(float ratio)`.
  - 3파트 PGEA finger transform open/close.
  - visual alignment와 TCP frame 분리 원칙.
- 구현:
  - `RobotControlPeripheralFacade`와 `RobotControlPeripheralState` 추가.
  - `IoPanelController` 추가.
  - `IoPanelHost`, `IoSheetHost` 추가.
  - `NavIo`, `BottomTabIo`에서 I/O 패널 표시.
  - EasyMotion gripper 버튼도 같은 runtime facade 사용.
  - Live는 아직 안전하게 unsupported reason으로 차단.
- 검증:
  - `GetPanelControllerSummary`: `io=1`.
  - `SetGripperOpenForDebug(true)`: `Gripper: Open (1.00)`.
  - `SetToolDoForDebug(0,true)`: `ToolDO0 ON`.
  - `SetRobotDoForDebug(1,true)`: `DO1 ON`.
  - `unityctl check --type compile --json`: pass.
- 남은 리스크:
  - PGEA attached prefab이 현재 `robotapp2` control prefab에 없으면 visual finger open/close는 no-op이다.
  - 실제 live SDK/ROS gripper command는 robottemplete에서도 미착수라 아직 실기 명령을 보내지 않는다.

## Next Visual Lock
- PGEA attached visual 이관 완료.
- `robottemplete`의 PGEA 성공 구조를 기준으로 `robotapp2` RobotStage control robot에 visual을 런타임 부착한다.
- `robottemplete` 비교 결과:
  - 운용 기준은 `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control_PGEA10040.prefab`의 nested override다.
  - 단독 `Assets/Runtime/Resources/EndEffectors/PGEA_100_40.prefab`는 기본 `TcpFrame=(0,0,0)`을 갖고 있어 수동 튜닝 기준으로 쓰지 않는다.
  - Control override 기준값은 `PGEA_100_40 local=(0.003,0.1676,0.031)`, rotation quaternion `(0,0,-0.7169106,0.69716513)`.
  - `TcpFrame local=(-0.0677,0,-0.0325)`, `PGEA-100-40_Model.z=-0.031`.
  - 이 값은 사용자가 템플릿 프로젝트에서 수동 미세조정한 닫힌 finger 기준 작업점으로 취급한다.
- 구현 결과:
  - `robotapp2` 런타임 부착 시 위 Control override 값을 강제 적용한다.
  - 그리퍼 본체/핑거가 흰 로봇과 묻히지 않도록 런타임 고대비 material을 적용한다.
  - `GetGripperVisualSummaryForDebug()`는 `tcpLocal` / `modelLocal`을 함께 출력한다.
- 검증 결과:
  - `SetGripperOpenForDebug(true)`: `gripper=Gripper: Open (1.00); gripperVisual=True`
  - `SetGripperOpenForDebug(false)`: `gripper=Gripper: Closed (0.00); gripperVisual=True`
  - `GetGripperVisualSummaryForDebug()` closed: `tcpLocal=(-0.0677,0,-0.0325)`, `modelLocal=(0.0065,0.3256,-0.031)`, `cameraVisible=True`.
  - stage camera screenshot: `Artifacts/robotcontrolv3-gripper-template-tcp.png`.
  - `unityctl check --type compile --json`: pass
- 다음 단계:
  - FAIRINO 공식 SDK/pendant 문서와 `LiveFairinoClient` 구현을 비교한다.
  - 실기 gripper open/close, tool coordinate readback, `GetActualTCPPose`를 기준으로 current TCP가 닫힌 finger 작업점과 일치하는지 확정한다.
  - 실기 SDK 확인 전에는 live gripper/move command를 자동 실행하지 않는다.

## Phase 3 Live SDK Gripper Contract
- 공식 문서 비교:
  - `SetGripperConfig(company, device, softversion, bus)`로 gripper vendor/device/bus를 설정한다.
  - `ActGripper(index, action)`으로 reset/activate를 수행한다.
  - `MoveGripper(index, pos, vel, force, max_time, block, type, rotNum, rotVel, rotTorque)`가 실제 gripper 위치 명령이다.
  - `GetGripperMotionDone`, `GetGripperActivateStatus`, `GetGripperCurPosition`, `GetGripperCurSpeed`, `GetGripperCurCurrent`, `GetGripperVoltage`, `GetGripperTemp`를 readback 비교 기준으로 둔다.
- 구현:
  - `FairinoGripperProfile`, `FairinoGripperCommand`, `FairinoGripperCapability`, `FairinoGripperStatus` 추가.
  - `IFairinoRobotClient`에 gripper SDK contract 추가.
  - `LiveFairinoClient`는 공식 SDK method명을 reflection으로 probe/readback/wrap한다.
  - `MockFairinoClient`는 같은 contract를 시뮬레이션한다.
  - UR5e/Doosan/Meca mock은 gripper unsupported로 명시 반환한다.
  - `RobotControlPeripheralFacade`는 mock 연결 시 visual open/close와 SDK mock readback을 함께 갱신한다.
  - Live 실행은 아직 차단한다. 현재 단계는 실기 명령 전 method/readback 비교 scaffold다.
- 검증:
  - `unityctl check --type compile --json`: pass.
  - `ConnectDefaultForDebug()` mock connected.
  - `GetGripperSdkSummaryForDebug(true)`:
    - `capability=(configure=True; activate=True; move=True; motion=True; active=True; pos=True; speed=True; current=True; voltage=True; temp=True)`.
    - 초기 `position=0`.
  - `SetGripperOpenForDebug(true)`:
    - visual `Gripper: Open (1.00)`.
    - mock SDK readback `activationMask=1`, `position=100`, `speed=50`, `current=50`.
- 다음 실기 기준:
  - 실제 FR5 연결 후 먼저 `GetGripperSdkSummaryForDebug(true)`만 실행한다.
  - pendant에서 gripper configuration/activation 상태를 확인하고 앱 readback과 비교한다.
  - 현장 확인 전에는 `MoveGripper` live 실행을 열지 않는다.

## Robot-linked Button Simulation Audit
- 요청 목적:
  - 실기기 테스트 전, 화면에 보이는 로봇 연동 버튼이 Mock/RobotStage에서 시뮬레이션되는지 확인한다.
  - 버튼당 최소 2개 관점으로 확인한다.
- 자기리뷰:
  - 이전 핑거 깨짐 원인은 SDK gripper position과 visual finger transform을 너무 빨리 결합한 것이다.
  - 실기에서 `pos=0/100` 방향성이 확인되기 전까지 visual finger는 닫힌 TCP 기준에 고정한다.
  - SDK 상태는 `GripperSdkSummary`로 따로 비교한다.
  - 포인트 패널 visibility 계약은 `NavMotion + TabPointMove`다. `NavPoints`로 audit하면 저장 버튼이 닫힌 패널 상태로 판단된다.
- 코드 보강:
  - `RobotControlV3DebugBridge.RunRobotLinkedButtonSimulationAuditForDebug()` 추가.
  - `JointJogController.NudgeJointForDebug()` 추가.
  - `GetGripperVisualSummaryForDebug()`에 `fingerLeft` / `fingerRight` local position 추가.
  - `GripperSummary`를 `Cmd`와 `Visual`로 분리.
- 검증:
  - `unityctl check --type compile --json`: pass.
  - `script get-errors`: error 0.
  - `RunRobotLinkedButtonSimulationAuditForDebug()`: `RobotLinkedButtonAudit pass=74; fail=0`.
- 포함한 로봇 연동 버튼군:
  - `BtnConnect`, `BtnServoEnable`, `BtnSync`, `BtnStop`, `BtnStopBottom`, `BtnPause`, `BtnDryRun`.
  - `BtnEasyHome`, `BtnEasyReady`, `BtnEasyFolded`, `BtnEasyZero`, `BtnEasyApply`.
  - `BtnGripperOpen`, `BtnGripperClose`.
  - `BtnJoint1~6Plus`, `BtnJoint1~6Minus`, `BtnJointPreview`, `BtnJointApply`, `BtnJointRestore`.
  - `BtnTcp/BtnArrow X/Y/Z/RX/RY/RZ +/-`, `BtnTcpCoordBase/Tool/User`, `BtnTcpPreview`, `BtnTcpApply`.
  - `BtnPointMoveL`, `BtnPointMoveJ`, `BtnPointPreview`, `BtnPointApply`, `BtnPointSave`, `BtnPointRecall`, `BtnPointRename`, `BtnPointExport`, `BtnPointDelete`, `BtnPointCleanup`.
  - `DO0/DO1 ON/OFF`, `TDO0/TDO1 ON/OFF`.
  - `BtnViewportBaseFrame`, `BtnViewportToolFrame`, `BtnViewportTrail`, `BtnViewportGhost`, `BtnViewportBoundary`, `BtnViewportCollision`, `BtnViewportCameraReset`.
  - `BtnCoordModeJoint`, `BtnCoordModeTcp`, `BtnCoordModeBoth`.
- 해석:
  - 동일 runtime path를 공유하는 버튼은 묶어서 확인했다.
    - 예: `BtnStop` / `BtnStopBottom`
    - 예: `BtnTcpXPlus` / `BtnArrowXPlus`
  - 현재 목표인 “실기기 전 시뮬레이션 가능 여부”는 통과다.
  - 실기 테스트 전 추가 조건은 pendant gripper direction, SDK readback, current tool coordinate 비교다.

## Actual UI Click E2E Follow-up
- 이전 `RobotLinkedButtonAudit pass=74; fail=0`은 `DebugBridge -> runtime` 경로 검증이다.
- 추가로 `unityctl uitk click`로 화면 버튼을 실제 클릭해 `UI Toolkit ClickEvent -> controller -> runtime` 경로를 검증했다.
- 실제 클릭에서 찾은 차이:
  - 일부 동적 버튼이 `Button.clicked`만 사용해서 `unityctl uitk click`의 `ClickEvent` 검증에서 runtime handler가 실행되지 않았다.
  - 상단 `Servo / Run / Stop / Reset`은 runtime 직접 진입점이 부족했다.
  - I/O 동적 버튼은 이름이 없어 desktop/tablet 중복 locator를 안정적으로 고르기 어려웠다.
- 수정:
  - `ConnectionHomeController` 상단 버튼과 home panel 버튼을 `RegisterCallback<ClickEvent>` 기반으로 묶었다.
  - `EasyMotionController`, `JointJogController`, `IoPanelController` 동적 로봇 연동 버튼도 `RegisterCallback<ClickEvent>`로 통일했다.
  - I/O 버튼에 `BtnIoGripperOpen/Close`, `BtnRobotDo0/1On/Off`, `BtnToolDo0/1On/Off` 이름을 부여했다.
- 실제 클릭 확인:
  - `BtnConnect`: `connected=True`, `enabled=False`.
  - `BtnServoEnable`: `connected=True`, `enabled=True`.
  - `BtnPointPreview`: `pending=대기 명령: MoveJ`, `ghost=True`, `path=True`.
  - `BtnPointApply`: `[DryRun Apply] 포인트 MoveJ 적용`, `pending=대기 중인 명령 없음`, `ghost=False`, `path=False`.
  - `BtnRobotDo0On`: `robotDo=DO0 ON / DO1 OFF`.
  - `BtnToolDo0On`: `toolDo=ToolDO0 ON / ToolDO1 OFF`.
  - `BtnIoGripperOpen`: `Cmd Open`, SDK mock readback `position=100`.
  - `BtnIoGripperClose`: `Cmd Close`, SDK mock readback `position=0`.
- 재검증:
  - `unityctl check --type compile --json`: pass.
  - `console get-count`: error 0.
  - `RunRobotLinkedButtonSimulationAuditForDebug()`: `RobotLinkedButtonAudit pass=74; fail=0`.
- 주의:
  - Point 버튼은 `NavMotion + TabPointMove`에서 enabled가 되는 것이 현재 계약이다.
  - `NavIo` 상태에서 `BtnPointApply`가 disabled로 보이는 것은 누락이 아니라 shell visibility 계약이다.

## Missing Test Inventory
- 실제 UI 클릭을 모든 74개 버튼에 대해 1:1로 자동 순회하지는 않았다.
  - 대표 클릭 E2E는 통과했다.
  - 전체 커버리지는 `DebugBridge -> runtime` audit으로 보강했다.
- Tablet/bottom sheet 실제 클릭은 아직 대표 검증만 남아 있다.
  - Desktop panel locator 중심으로 검증했다.
  - `BottomTab*`, `BtnRunBottom`, `BtnStopBottom`, tablet sheet 안의 Point/I/O 버튼 actual click matrix가 필요하다.
- Popup confirm flow는 별도 E2E가 필요하다.
  - `Servo`, `Run`, `Reset`, `Stop`, fault recovery popup open/confirm/cancel 경로를 실제 클릭으로 확인해야 한다.
  - 현재 runtime direct handler는 통과했지만, confirm popup UX 정책은 별도다.
- View toolbar는 runtime audit은 통과했지만 actual click 대표만 부족하다.
  - `Base / Tool / Path / Ghost / Bound / Coll / Cam` 버튼을 화면에서 눌렀을 때 stage flag와 layout이 동시에 유지되는지 확인해야 한다.
- RobotStage visual capture는 이번 actual click 루프에서 새 스크린샷을 남기지 않았다.
  - 최소 `front / side / iso` 중 2각도에서 Joint/TCP/Point preview 후 ghost/path/robot pose 캡처가 필요하다.
- Run/Step/Undo/Redo는 아직 제품 의미가 partial이다.
  - sequence/program queue가 없으므로 “버튼 클릭 가능”과 “상용 pendant식 실행 의미”는 다르다.
  - Phase 4 전에는 full pass로 치면 안 된다.
- Point MoveJ production IK 검증은 아직 부족하다.
  - 현재는 saved joint target 우선 + numerical XYZ IK fallback이다.
  - RX/RY/RZ orientation, singularity, joint limit margin, collision, unreachable target failure UI가 필요하다.
- Safety/fault scenario actual test가 부족하다.
  - fault 상태 강제 진입 후 reset/recovery/detail 버튼의 화면 클릭과 runtime state 변화 검증이 필요하다.
- Live robot tests는 의도적으로 미실행이다.
  - 실제 FR5 이동, live `MoveJ/MoveL`, live DO/ToolDO, live `MoveGripper`는 Phase 6 안전 게이트 전까지 금지다.
  - 실기 전 1순위는 `GetGripperSdkSummaryForDebug(true)` readback-only 비교다.

## Missing Test Execution Kickoff
- 이번 세션에서 바로 진행할 첫 단위는 desktop actual UI click full matrix다.
- 방식:
  - `unityctl uitk click`로 target 버튼을 실제 클릭한다.
  - 연결/servo/panel visibility 같은 전제 조건은 테스트 독립성을 위해 각 case 시작 전에 세팅한다.
  - before/after runtime summary와 expected needle을 artifact에 저장한다.
- 산출물:
  - `Artifacts/robotcontrolv3-actual-click-matrix.json`
- 판정:
  - 실패는 무조건 기능 실패로 뭉뚱그리지 않고 `locator`, `disabled`, `runtime`, `popup-policy`, `product-pending`으로 분류한다.
  - 즉시 수정 가능한 binding/locator 문제는 같은 세션에서 고친다.

## Actual Click Matrix Execution Result
- 첫 외부 PowerShell matrix는 `95 cases / 51 pass / 44 fail`로 나왔지만, 분석 결과 false fail이 섞여 있었다.
  - 한글 expected needle 인코딩 깨짐.
  - PowerShell `-like`가 `[DryRun Apply]`의 `[]`를 wildcard 문자클래스로 해석.
  - 버튼마다 `unityctl` 프로세스를 새로 띄워 15분 제한을 넘김.
- 대응:
  - 외부 스크립트는 재현용으로 유지한다.
  - 실제 반복 검증은 Unity 내부 `ClickEvent` matrix로 승격했다.
- 실제 발견한 runtime gap:
  - `BtnRun`: pending preview가 있어도 실행하지 않고 안내 문구만 표시했다.
  - `BtnRunBottom`, `BtnStopBottom`: bottom bar 버튼이 runtime handler에 연결되어 있지 않았다.
- 수정:
  - `ExecutePrimaryAction()`이 pending MoveJ/MoveL preview를 우선 실행하도록 보강했다.
  - `ConnectionHomeController`가 `BtnRunBottom`, `BtnStopBottom`을 top run/stop handler와 공유하도록 연결했다.
- 최종 검증:
  - `unityctl check --type compile --json`: pass.
  - `script get-errors`: error 0.
  - Desktop actual UI click matrix: `ActualUiClickMatrix pass=95; fail=0`.
  - Tablet/bottom representative matrix: `TabletBottomClickMatrix pass=16; fail=0`.
- Artifact:
  - `Artifacts/robotcontrolv3-actual-click-matrix-internal.json`
  - `Artifacts/robotcontrolv3-tablet-bottom-click-matrix.json`
- 남은 누락:
  - popup confirm/cancel E2E.
  - RobotStage 다각도 screenshot evidence.
  - Point MoveJ orientation/unreachable/joint-limit/singularity/collision guard.
  - Safety/fault actual flow.
  - Live SDK readback-only 및 실기 command safety gate.

## Safety / Screenshot / Live Gate Result
- 추가 자동 검증:
  - `RunPopupConfirmCancelE2EForDebug()`: `pass=10; fail=0`.
  - `RunSafetyFaultActualFlowForDebug()`: `pass=5; fail=0`.
  - `RunPointMoveJProductionGuardMatrixForDebug()`: `pass=6; fail=0`.
  - `RunStageScreenshotEvidenceForDebug()`: front/side/iso screenshot 생성.
  - `RunLiveSdkReadbackGateForDebug()`: readback-only gate 생성.
- 수정:
  - popup trigger, status detail, preset state 버튼에 `ClickEvent` 명시 바인딩을 보강했다.
  - bottom/top run/stop과 popup/confirm 경로가 서로 검증 가능하도록 DebugBridge matrix를 분리했다.
  - Safety/Fault summary는 panel force initialize가 preview state를 덮어쓰지 않도록 전용 summary로 확인한다.
- Artifact:
  - `Artifacts/robotcontrolv3-popup-confirm-cancel-e2e.json`
  - `Artifacts/robotcontrolv3-safety-fault-actual-flow.json`
  - `Artifacts/robotcontrolv3-point-movej-production-guard.json`
  - `Artifacts/robotcontrolv3-live-sdk-readback-gate.txt`
  - `Artifacts/robotcontrolv3-stage-ready-front.png`
  - `Artifacts/robotcontrolv3-stage-ready-side.png`
  - `Artifacts/robotcontrolv3-stage-tcp-iso.png`
- 남은 실기 전 주의:
  - Point MoveJ orientation, joint-limit margin, singularity, collision은 아직 production solver/guard가 아니라 `product-pending`이다.
  - Live gate는 Mock SDK readback scaffold 기준이다.
  - 실제 FR5에서는 먼저 readback-only만 수행하고, operator safety confirm 전까지 live command는 금지다.

## Live Command Safety Gate Result
- `LiveCommandSafetyGate`를 추가했다.
- `Allowed / Blocked / ReadbackOnly / RequiresConfirm` 결과와 `Low / Medium / High / Critical` risk level을 분리했다.
- live command 앞단 연결:
  - `MoveJ`
  - `MoveL`
  - `Robot DO`
  - `Tool DO`
  - `MoveGripper`
- 기본 차단 조건:
  - 연결 없음
  - servo OFF
  - speed cap 10% 초과
  - operator confirm token 없음
  - E-stop / safety stop / fault / collision flag
  - dry-run preview artifact 없음
  - production IK guard 미통과
  - boundary/collision real data 없음
  - gripper readback 없음
- 검증:
  - `RunLiveCommandSafetyGateMatrixForDebug()`: `pass=12; fail=0`.
  - 기존 actual click / popup / safety / point guard matrix도 재실행해 green 유지.
- Artifact:
  - `Artifacts/robotcontrolv3-live-command-safety-gate.json`
- 현재 정책:
  - Mock/DryRun은 유지한다.
  - 실제 Live command는 gate 앞에서 계속 막는다.
  - saved joint target MoveJ만 live 후보가 될 수 있고, numerical XYZ IK fallback은 live 금지다.

## Next Session Handoff
- 기준 커밋: `9ad78f5 Add RobotControl V3 live command safety gate`.
- 다음 세션은 실기 명령을 여는 세션이 아니라, live command를 열기 위한 product UX/policy를 완성하는 세션으로 시작한다.
- 첫 재검증:
  - `unityctl check --type compile --json`
  - `RunLiveCommandSafetyGateMatrixForDebug()` -> `12/12 PASS`
  - `RunActualUiClickMatrixForDebug()` -> `95/95 PASS`
  - `RunPopupConfirmCancelE2EForDebug()` -> `10/10 PASS`
- 다음 구현 후보:
  - operator confirm popup이 실제 `GrantLiveCommandApprovalForDebug()` 같은 debug token이 아니라 제품 UI token을 발급하도록 연결.
  - boundary/collision real data를 `LiveCommandSafetyGateRequest`의 `IsBoundaryDataReady`, `IsCollisionDataReady`에 연결.
  - Point MoveJ production IK policy를 saved joint target / numerical fallback / unreachable / singularity / collision별로 명확히 분리.
  - Program Run/Step queue v1을 single-step queue로 제한해 정의.
- 유지해야 할 안전 원칙:
  - 모르는 조건은 허용이 아니라 차단이다.
  - 실제 FR5 이동/출력/그리퍼 명령은 readback-only와 operator safety confirm 전까지 계속 금지다.
  - Play direct QA 후에는 항상 `Always Start From Onboarding=true`로 원복한다.

## Verification Policy
- Mock+Unity 시뮬 기준으로 먼저 전부 닫는다.
- Live 실기 이동은 별도 Phase 6 안전 게이트 전까지 금지한다.
- 각 phase 완료 시 `progress-checklist.md`와 daily log를 갱신한다.
