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

## Verification Policy
- Mock+Unity 시뮬 기준으로 먼저 전부 닫는다.
- Live 실기 이동은 별도 Phase 6 안전 게이트 전까지 금지한다.
- 각 phase 완료 시 `progress-checklist.md`와 daily log를 갱신한다.
