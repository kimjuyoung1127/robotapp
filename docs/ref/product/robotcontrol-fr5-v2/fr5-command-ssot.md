# FR5 Command SSOT

## Purpose

버튼 이름이 아니라 `명령 계약` 기준으로 FR5 V2를 잠근다.

- V1 패널을 그대로 복제하지 않는다.
- V1에서 재사용 가능한 런타임/SDK 호출을 `command` 단위로 분해한다.
- 각 명령은 `공식 근거`, `실기 검증 상태`, `V2 UI surface`, `preview/safety gate`를 함께 가진다.

## Contract Rules

1. `CommandId`가 SSOT다.
2. 패널/버튼 이름은 명령을 소비하는 surface일 뿐이다.
3. `live-ready` 판단은 `field-verified` 이전에 쓰지 않는다.
4. 공식 근거가 없는 명령은 `candidate` 또는 `researching`으로만 둔다.
5. `고스트 preview`, `바닥 격자`, `현재 pose 기억`은 SDK 명령이 아니라 `product-decision` UX 계약이다.

## Naming Rule For This Table

- `official_function`
  - FAIRINO 공식 문서/SDK에 나타난 함수명
- `local_wrapper`
  - 현재 저장소에서 해당 기능을 감싸는 wrapper 또는 service entry
- `source_ids`
  - [fr5-official-source-register.md](./fr5-official-source-register.md)에 등록한 공식 근거 ID

## Tool/TCP Lock

### Locked Decision

- 실기 FR5에 그리퍼가 장착되어 있으면 `tool TCP`는 bare flange가 아니라 `그리퍼 기준 TCP`로 잡는다.
- 즉, `MoveL`, 현재 TCP 읽기, teaching point 저장, point run, preview target은 모두 `적용된 tool coordinate`를 기준으로 해석한다.
- `toolcoord0`은 end flange 중심을 의미하므로, 그리퍼를 쓰는 실제 운용 기준으로는 최종 기준 TCP가 아니다.

### Why

- FAIRINO 공식 Base/Coordinate 문서는 "tool 설치 후 tool coordinate system을 보정/적용하지 않으면 motion command 시 tool center point의 위치/자세가 기대와 다르다"고 명시한다.
- FAIRINO C# Common Robot Settings 문서는 `SetTcp4RefPoint`, `ComputeTcp4`, `SetToolCoord`, `SetToolList`를 통해 tool coordinate를 계산/적용하는 흐름을 제시한다.

### Practical Rule

1. STL/STEP 모델 원점은 참고용일 뿐 SSOT가 아니다.
2. TCP는 실제 gripper 작업점으로 측정/보정한다.
3. 문서상 기준 함수는 아래 순서를 따른다.
   - `SetTcp4RefPoint(point_num)` 또는 `SetToolPoint(point_num)`
   - `ComputeTcp4(ref DescPose tcp_pose)` 또는 `ComputeTool(ref DescPose tcp_pose)`
   - `SetToolCoord(id, DescPose coord, int type, int install, int toolID, int loadNum)`
   - 필요 시 `SetToolList(...)`, `SetLoadWeight(...)`, `SetLoadCoord(...)`
4. 적용 확인은 아래를 읽어서 검증한다.
   - `GetActualTCPNum(...)`
   - `GetCurToolCoord(...)`
   - `GetActualTCPPose(...)`

### Current Project Impact

- 현재 프로젝트 robot visual에는 gripper가 없어서 live TCP와 scene EE marker가 어긋날 수 있다.
- 따라서 코딩 기준선은 아래 둘을 같이 요구한다.
  - 실기 명령/상태는 `gripper TCP` 기준
  - scene preview는 최소한 `gripper TCP offset`을 반영
- 현재 `gripper TCP`는 `그리퍼 사이 정중앙 공중점`으로 잠겨 있다.

### Local Gripper Reference Files

- `C:\Users\ezen601\Documents\카카오톡 받은 파일\PGEA-100-40.stl`
- `C:\Users\ezen601\Documents\카카오톡 받은 파일\PGEA-100-40-W-F_V1.0_3D_20241226.STEP`

위 파일은 `visual geometry candidate`로만 본다. TCP SSOT는 mesh origin이 아니라 실제 tool calibration 결과다.

## Command Table

| command_id | official_function | local_wrapper | source_ids | area | intent | v2_surface | v1_source | evidence | status | preview_gate | live_policy | notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `Connection.Connect` | `RPC(string ip)` | `IFairinoRobotClient.Connect` -> `LiveFairinoClient.Connect` | `fairino-csharp-sdk` | Connection | 로봇 연결 | `TopStatusBar`, `StatusSummary` | `FairinoConnectionPanel` | `official-doc`, `official-sdk` | locked-doc | none | field-needed | 현재 로컬 live client는 `port` 인자를 받지만 SDK 호출은 `RPC(ip)` 기준이다 |
| `Connection.Disconnect` | `CloseRPC()` | `IFairinoRobotClient.Disconnect` -> `LiveFairinoClient.Disconnect` | `fairino-csharp-sdk` | Connection | 연결 해제 | `TopStatusBar`, `StatusSummary` | `FairinoConnectionPanel` | `official-sdk` | locked-doc | none | field-needed | SDK에 `CloseRPC`가 있으면 호출, 없으면 세션 해제만 수행 |
| `Connection.EnableRobot` | `RobotEnable(1)` | `IFairinoRobotClient.Enable` -> `LiveFairinoClient.Enable` | `fairino-csharp-sdk`, `fairino-csharp-common-settings` | Connection | 서보/Enable 활성화 | `TopStatusBar` | `FairinoConnectionPanel` | `official-doc`, `official-sdk` | locked-doc | none | field-needed | live 전 `DragTeachSwitch(0)` + `Mode(0)` 준비를 선행 |
| `Connection.DisableRobot` | `RobotEnable(0)` | `IFairinoRobotClient.Disable` -> `LiveFairinoClient.Disable` | `fairino-csharp-sdk`, `fairino-csharp-common-settings` | Connection | 서보/Enable 비활성화 | `TopStatusBar` | `FairinoConnectionPanel` | `official-doc`, `official-sdk` | locked-doc | none | field-needed | power-off 전 stopped 상태와 함께 사용 |
| `State.SyncCurrentPose` | `GetActualJointPosDegree(...)` + `GetActualTCPPose(...)` + `GetActualTCPNum(...)` + `GetCurToolCoord(...)` | `FairinoConnectionService.RefreshControllerMetadata` + `ReadState` + V2 state overwrite | `fairino-csharp-movement`, `fairino-csharp-common-settings`, `fairino-csharp-sdk` | State | 현재 joint/TCP를 앱 상태와 동기화 | `TopStatusBar`, `EasyMotion`, `TcpJog`, `JointJog`, `PointMove` | `FairinoJointControlPanel` | `official-doc`, `official-sdk`, `product-decision` | locked-product | none | field-needed | FAIRINO SDK에 `Sync` 단일 함수는 보이지 않으므로 `read actual pose -> overwrite current state`를 공식 기반 제품 계약으로 잠근다 |
| `State.ReadActualJointPose` | `GetActualJointPosDegree(flag, ref JointPos)` 또는 `GetRobotRealTimeState(...)` | `IFairinoRobotClient.ReadState` -> `LiveFairinoClient.ReadState` / `ReadJointPositionsFallback` | `fairino-csharp-movement`, `fairino-csharp-sdk` | State | 현재 joint 읽기 | 공통 상태 원천 | `RobotControlSceneCoordinator` | `official-doc`, `official-sdk` | locked-doc | none | field-needed | `RobotControlViewState.CurrentJointValuesDeg` 원천 |
| `State.ReadActualTcpPose` | `GetActualTCPPose(flag, ref DescPose)` 또는 `GetRobotRealTimeState(...)` | `IFairinoRobotClient.ReadState` -> `LiveFairinoClient.ReadState` / `ReadTcpPoseFallback` | `fairino-csharp-movement`, `fairino-csharp-sdk` | State | 현재 TCP 읽기 | 공통 상태 원천 | `RobotControlSceneCoordinator` | `official-doc`, `official-sdk` | locked-doc | none | field-needed | `RobotControlViewState.CurrentTcpPose` 원천, 반드시 적용된 tool TCP 기준 |
| `Motion.JointArrowJog` | `StartJOG(refType, nb, dir, vel, acc, max_dis)` + `StopJOG(ref)` + `ImmStopJOG()` | 신규 V2 jog adapter 필요 | `fairino-csharp-movement` | Motion | 조인트별 화살표 버튼 실시간 조정 | `JointJogPanel` | split from `FairinoJointControlPanel` | `official-doc`, `product-decision` | locked-product | preview-optional | field-needed | slider 우선이 아니라 `J1~J6 -/+` 단발 또는 hold jog를 기본 surface로 잠근다 |
| `Motion.TcpArrowJog` | `StartJOG(refType, nb, dir, vel, acc, max_dis)` + `StopJOG(ref)` + `ImmStopJOG()` | 신규 V2 jog adapter 필요 | `fairino-csharp-movement`, `fairino-manual-teaching` | Motion | TCP 기준 증분 이동 | `TcpJogPanel` | split from `FairinoTcpControlPanel` | `official-doc`, `product-decision` | locked-product | preview-optional | field-needed | base/tool/wobj 기준 jog surface로 잠근다. 실제 좌표계는 현재 robot application 기준 |
| `Motion.MoveJPreset` | `MoveJ(...)` | `IFairinoRobotClient.MoveJ` + `FR5PosePresets` | `fairino-csharp-movement`, `fairino-csharp-sdk` | Motion | Home/Ready/Folded 등 프리셋 이동 | `EasyMotionPanel` | `FR5PosePresets`, `FairinoJointControlPanel` | `official-doc`, `official-sdk`, `product-decision` | locked-product | preview-required | field-needed | preview 후 confirm 기본 |
| `Motion.MoveJToJointTarget` | `MoveJ(JointPos joint_pos, DescPose desc_pos, int tool, int user, ...)` 또는 point-name 기반 `MoveJ(string point_name, float vel, int tool, int user)` | `IFairinoRobotClient.MoveJ` -> `LiveFairinoClient.MoveJ` | `fairino-csharp-movement`, `fairino-csharp-sdk`, `fairino-coding-local-points` | Motion | 저장된 joint target으로 실제 이동 | `JointJogPanel`, `EasyMotionPanel` | `FairinoJointControlPanel` | `official-doc`, `official-sdk` | locked-doc | preview-required | field-needed | 현재 프로젝트는 point-name 호출보다 explicit joint target 호출을 우선한다 |
| `Motion.MoveLToTcpTarget` | `MoveL(DescPose desc_pos, int tool, int user, ...)` 또는 point-name 기반 `MoveL(string point_name, float vel, int tool, int user)` | `IFairinoRobotClient.MoveL` -> `LiveFairinoClient.MoveL` | `fairino-csharp-movement`, `fairino-csharp-sdk`, `fairino-coding-local-points` | Motion | 저장된 TCP target으로 실제 이동 | `TcpJogPanel`, `PointMovePanel` | `FairinoTcpControlPanel` | `official-doc`, `official-sdk` | locked-doc | preview-required | field-needed | target은 bare flange가 아니라 적용된 gripper TCP 기준 |
| `Motion.Stop` | `StopMotion()` 또는 SDK fallback `MoveStopJ()` | `IFairinoRobotClient.StopMotion` -> `LiveFairinoClient.StopMotion` | `fairino-csharp-sdk` | Motion | 현재 motion 정지 | `TopStatusBar` | V1 joint/tcp panel stop | `official-sdk` | locked-doc | none | field-needed | motion stop와 sequence stop은 분리 |
| `Teaching.RememberCurrentPose` | `GetActualJointPosDegree(...)` + `GetActualTCPPose(...)` + current tool context | 신규 V2 capture service 필요 | `fairino-csharp-movement`, `fairino-manual-teaching`, `fairino-teaching-pendant-manual` | Teaching | 언제든 현재 로봇 위치를 기억 | `TopStatusBar`, `TeachingPanel` | partial V1 waypoint flow | `official-doc`, `product-decision` | locked-product | none | field-needed | endpoint 변경 시 기본자세 복귀 대신 `actual current pose snapshot`을 기준점으로 삼는다 |
| `Teaching.SavePoint` | direct SDK primitive 없음. 공식 개념은 `teaching point record` / `local teaching point` | `WaypointStore.AddWaypoint` + actual pose capture | `fairino-teaching-pendant-manual`, `fairino-manual-teaching`, `fairino-coding-local-points` | Teaching | 포인트 저장 | `TeachingPanel` | `WaypointStore`, `FairinoJointControlPanel` | `official-doc`, `product-decision` | locked-product | none | field-needed | 저장 포인트는 `joint + tcp + toolId + userId + moveType`를 함께 보관 |
| `Teaching.LoadPoint` | direct SDK primitive 없음 | `WaypointStore.Load` / `WaypointStore.LoadAllNames` | `fairino-manual-teaching`, `fairino-coding-local-points` | Teaching | 저장 포인트 불러오기 | `TeachingPanel`, `PointMovePanel` | `WaypointStore` | `product-decision` | locked-product | preview-required | field-needed | 공식 local teaching point 개념을 우리 session storage로 재해석 |
| `Teaching.RunPoint` | 공식 pendant 동작: local teaching point `Start Run`; motion primitive는 `MoveJ`/`MoveL` | `WaypointCycleRunner.PlayOnce` -> `MoveJ` or `MoveL` dispatch | `fairino-coding-local-points`, `fairino-manual-teaching` | Teaching | 선택 포인트 한 번 실행 | `TeachingPanel` | `WaypointCycleRunner` | `official-doc`, `product-decision` | locked-product | preview-required | field-needed | `single-point operation`으로 잠근다 |
| `Teaching.RunLoop` | direct SDK primitive 없음; loop는 product sequence | `WaypointCycleRunner.PlayLoop` -> repeated `MoveJ`/`MoveL` | `fairino-coding-local-points`, `product-decision` | Teaching | 저장 포인트 시퀀스 반복 | `TeachingPanel` | `WaypointCycleRunner` | `official-doc`, `product-decision` | locked-product | preview-required | field-needed | 공식 문서는 single-point/local point 개념까지 제공, 반복은 우리 product layer가 담당 |
| `Teaching.StopLoop` | loop 자체는 product sequence, motion 정지는 `StopMotion()` | `WaypointCycleRunner.Stop` + `IFairinoRobotClient.StopMotion` | `fairino-csharp-sdk`, `product-decision` | Teaching | 반복 실행 중지 | `TeachingPanel`, `TopStatusBar` | `WaypointCycleRunner` | `product-decision`, `official-sdk` | locked-product | none | field-needed | sequence stop와 robot stop을 같이 처리 |
| `Preview.GhostTarget` | n/a | 신규 V2 preview service 필요 | `product-decision` | Preview | 목표 pose 고스트 표시 | `CenterViewport` | new V2 | `product-decision` | candidate | n/a | n/a | 공식 SDK 기능이 아니라 V2 UX |
| `Preview.PathEstimate` | n/a | 신규 V2 preview service 필요 | `product-decision` | Preview | 예상 움직임 경로 표시 | `CenterViewport` | new V2 | `product-decision` | candidate | n/a | n/a | FK 기반 로컬 계산 |
| `Preview.RiskSummary` | n/a | 신규 V2 validation/presenter 필요 | `product-decision` | Preview | 위험/도달불가/차이량 요약 | `RightRail`, `Popup` | partial V1 warnings | `product-decision` | candidate | n/a | n/a | safety gate 텍스트 규격 필요 |
| `Viewport.FloorGrid` | n/a | 신규 V2 viewport visual 필요 | `product-decision` | Viewport | 바닥 격자 표시 | `CenterViewport` | new V2 | `product-decision` | candidate | n/a | n/a | 실기 명령이 아니라 공간 인지 UX |

## Explicitly Out For V1 Live Bring-Up

| command_id | reason |
|---|---|
| `Motion.ServoJLive` | V1 실기 bring-up 범위 밖 |
| `Motion.ServoCartLive` | V1 실기 bring-up 범위 밖 |
| `Program.GraphicalEditor` | teach pad 전체 복제 범위 밖 |
| `Application.SystemConfig` | 설치형/고급 설정 범위 밖 |

## Carry-Over Notes

- `MoveJ`, `MoveL`, `Stop`, `Sync`, `waypoint` 계열은 V1 구현 흔적이 이미 있으므로 `runtime reuse candidate`다.
- `JointArrowJog`, `GhostTarget`, `FloorGrid`, `RememberCurrentPose`는 V2 제품 요구가 더 강하므로 `new-v2 contract`로 본다.
- `SimMachine screenshot`은 UI 참조일 뿐 `command truth`가 아니다.
- `SavePoint`와 `RunLoop`는 FAIRINO SDK 단일 함수가 아니라 `official teaching concept + project runtime sequence` 조합으로 잠근다.

## Lock Before Coding

코딩 전 아래 항목이 먼저 채워져야 한다.

1. 각 `command_id`의 FAIRINO 공식 근거 링크
2. `mock / live / preview / stop / recover` 정책
3. `joint 기준 저장`인지 `tcp 기준 저장`인지
4. 현재값 기억 전략이 `sync 기반`인지 `continuous polling 기반`인지

## Current Answer

- `Connect / Enable / Sync / Read actual pose / MoveJ / MoveL`는 함수명까지 잠겼다.
- `Save point / Loop`는 `FAIRINO SDK direct call`이 아니라 `공식 teaching point 개념 + project product layer`로 잠갔다.
- 실제 FR5에 그리퍼가 달려 있으면 TCP/EE 기준은 `그리퍼 TCP`로 본다.
- 현재 그리퍼 TCP의 작업점은 `그리퍼 사이 정중앙 공중점`으로 잠겼다.
