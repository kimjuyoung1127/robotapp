# FR5 Gripper TCP Calibration Spec

## Purpose

실기 FR5에 장착된 그리퍼의 `tool TCP`를 어떻게 보정하고, 프로젝트에서 무엇을 SSOT로 삼을지 잠근다.

- 메쉬 원점과 실제 작업 TCP를 분리한다.
- FAIRINO teaching pendant와 SDK 기준의 calibration 흐름을 우선한다.
- `MoveL`, 현재 TCP 읽기, 포인트 저장, ghost preview, EE marker가 같은 기준을 보도록 만든다.

## Scope

- 대상 로봇: `FR5`
- 대상 말단 장치: 현재 실기에 장착된 gripper
- 대상 프로젝트 surface:
  - `MoveL`
  - `TCP Jog`
  - `Point Move`
  - `Teaching Save/Run/Loop`
  - `ghost preview`
  - `scene EE marker`

## Evidence

- `official-doc`
  - `fairino-csharp-common-settings`
  - `fairino-tool-calibration-ui`
  - `fairino-version-intro-tool-tcp`
  - `fairino-teaching-pendant-manual`
- `product-decision`
  - KineTutor3D V2 preview / teaching / viewport alignment 정책

## Locked Decision

1. TCP SSOT는 `실제 gripper 작업점 calibration 결과`다.
2. `STL` / `STEP` 원점은 시각화 참고 자료일 뿐, 명령/상태 기준이 아니다.
3. `MoveL`, `GetActualTCPPose`, point save, point run, loop는 모두 `적용된 tool coordinate` 기준으로 해석한다.
4. scene에 그리퍼 모델이 아직 없어도, EE marker와 preview target은 `gripper TCP offset`을 반영해야 한다.
5. `toolcoord0 = flange center`는 fallback 기준일 뿐, 실기 운용 기준 TCP가 아니다.
6. 현재 실기 운용 기준 TCP는 `그리퍼 사이 정중앙 공중`으로 잠근다.

## Local Geometry Candidates

- `C:\Users\ezen601\Documents\카카오톡 받은 파일\PGEA-100-40.stl`
- `C:\Users\ezen601\Documents\카카오톡 받은 파일\PGEA-100-40-W-F_V1.0_3D_20241226.STEP`

## Current Local Visual TCP Point

- 현재 로컬 visual 기준은 `robottemplete`의 `FAIRINO_FR5_Control_PGEA10040.prefab`에서 수동 미세조정한 닫힌 finger 작업점이다.
- 단독 `PGEA_100_40.prefab`의 `TcpFrame=(0,0,0)`은 calibration truth가 아니다.
- Control prefab nested override 기준값:
  - `ToolMount`: identity
  - `PGEA_100_40 localPosition`: `(0.003, 0.1676, 0.031)`
  - `PGEA_100_40 localRotation`: quaternion `(0, 0, -0.7169106, 0.69716513)`
  - `TcpFrame localPosition`: `(-0.0677, 0, -0.0325)`
  - `TcpFrame localRotation`: identity
  - `PGEA-100-40_Model localPosition.z`: `-0.031`
- 이후 `MoveL`, `preview`, `point save`, `run`, `loop`, `EE marker`는 live calibration 전까지 이 visual TCP를 기준 후보로 본다.
- 최종 SSOT는 여전히 실기 pendant/SDK로 적용되고 readback되는 tool coordinate다.

## Coordinate Rule

### Parent Frame

- TCP는 `robot end-flange center`를 parent frame으로 하는 tool coordinate로 저장한다.
- 공식 C# Common Robot Settings 문서도 `coord tool center with respect to end-flange center`라고 설명한다.

### Project Interpretation

- robot visual donor 또는 URDF flange 끝점은 `mechanical mount frame`
- gripper tool TCP는 `operation frame`
- V2에서 사용자에게 보여주는 `현재 TCP`, `미리보기 목표`, `저장 포인트`는 전부 `operation frame` 기준
- 현재 로컬 후보 `operation frame`의 기준점은 `닫힌 finger 작업점`이다.
- 실기 calibration 후 pendant/SDK readback이 이 후보와 다르면 pendant/SDK 값이 이긴다.

## Accepted Calibration Paths

### Path A. Four-Point Method

공식 함수:

- `SetTcp4RefPoint(point_num)`
- `ComputeTcp4(ref DescPose tcp_pose)`
- `SetToolCoord(id, DescPose coord, int type, int install, int toolID, int loadNum)`

권장 상황:

- 일반 그리퍼에서 실제 grasp/work point를 중심으로 TCP를 잡을 때
- 운영자가 pendant에서 반복 측정 가능한 경우

### Path B. Six-Point Method

공식 함수:

- `SetToolPoint(point_num)`
- `ComputeTool(ref DescPose tcp_pose)`
- `SetToolCoord(...)`

권장 상황:

- 정밀 orientation까지 포함해 툴 자세를 안정적으로 맞춰야 할 때

### Path C. Pendant Auto/Guided Calibration

공식 근거:

- `base.html`
- `version_intro.html`

권장 상황:

- FAIRINO pendant에서 제공하는 tool TCP calibration UX를 먼저 신뢰할 수 있을 때
- 현장에서 SDK보다 pendant workflow가 더 안전할 때

## Required Stored Fields

하나의 gripper TCP calibration은 최소 아래 값을 가져야 한다.

| field | description |
|---|---|
| `toolCoordId` | 적용한 tool coordinate system 번호 |
| `toolPoseTcp` | flange 기준 TCP pose `[x,y,z,rx,ry,rz]` |
| `calibrationMethod` | `four-point` / `six-point` / `pendant-auto` |
| `installType` | `0 robot-end` 또는 공식 설치 타입 |
| `toolType` | `0 tool coordinate` 또는 공식 타입 |
| `loadNum` | 적용된 load profile 번호 |
| `loadWeight` | 실제 그리퍼+payload 기준 하중 |
| `loadCoord` | CoG 관련 load coordinate |
| `verifiedBy` | 누가 보정했는지 |
| `verifiedAt` | 언제 보정했는지 |
| `controllerVersion` | 현장 controller software version |
| `sdkVersion` | 현장 SDK version |

## Runtime Use Rule

### Must Read

live 세션 시작 후 아래는 반드시 확인한다.

- `GetActualTCPNum(...)`
- `GetCurToolCoord(...)`
- `GetActualTCPPose(...)`
- 필요 시 `GetActualWObjNum(...)`

gripper 세션 시작 후 아래는 반드시 확인한다.

- SDK method probe:
  - `SetGripperConfig`
  - `ActGripper`
  - `MoveGripper`
  - `GetGripperMotionDone`
  - `GetGripperActivateStatus`
  - `GetGripperCurPosition`
  - `GetGripperCurSpeed`
  - `GetGripperCurCurrent`
  - `GetGripperVoltage`
  - `GetGripperTemp`
- Readback:
  - activation mask
  - current position
  - current speed
  - current current/force proxy
  - voltage
  - temperature

현재 PGEA-100-40 후보 profile은 `company=4`, `device=0`, `soft=0`, `bus=2`, `index=2`다.
Open 후보 command는 `pos=100`, `vel=50`, `force=50`, `max_time=30000`, `block=1`이고,
Close 후보 command는 `pos=0`, `vel=50`, `force=50`, `max_time=30000`, `block=1`이다.

### Must Match

아래 3개가 서로 어긋나면 `MoveL`을 live-ready로 보지 않는다.

1. pendant/current application의 tool coordinate
2. app이 읽은 `GetActualTCPNum` / `GetCurToolCoord`
3. preview/EE marker가 사용하는 TCP offset

## KineTutor3D Application Rule

### State

- `RobotControlViewState.CurrentTcpPose`는 gripper TCP 기준이어야 한다.
- `FairinoCoordContext.ToolPose`는 현재 applied tool coordinate의 진실값이다.

### Preview

- `ghost preview`
- `predicted path`
- `preview target marker`
- `floor grid 대비 TCP 위치`

위 4개는 전부 flange가 아니라 gripper TCP를 기준으로 계산한다.
현재 로컬 visual 후보 TCP는 `robottemplete` Control prefab에서 가져온 닫힌 finger 작업점이다.

### Teaching

- `RememberCurrentPose`
- `SavePoint`
- `RunPoint`
- `RunLoop`

위 4개는 `joint snapshot + current gripper TCP + toolId + userId`를 함께 저장/사용한다.
여기서 `current gripper TCP`는 live calibration 전에는 `robottemplete` Control prefab의 닫힌 finger 작업점 후보이고, live 연결 후에는 pendant/SDK readback 값이다.

## Visual Model Rule

1. gripper STL/STEP를 scene에 붙이기 전에도 TCP offset은 문서값/보정값으로 먼저 반영할 수 있다.
2. 그리퍼 3D 모델을 붙인 뒤에는 아래를 다시 확인한다.
   - mesh tip
   - 실제 grasp point
   - calibrated TCP
3. mesh 끝과 calibrated TCP가 다르면 `calibrated TCP`가 이긴다.

## Validation Checklist

### Doc Lock

- `SetTcp4RefPoint` 또는 `SetToolPoint` 경로를 결정했다
- `SetToolCoord` 적용 파라미터를 기록했다
- `toolCoordId` / `loadNum` / `loadWeight`를 적었다

### Bench Check

- pendant에서 current tool coordinate 확인
- 앱에서 `GetActualTCPNum` 확인
- 앱에서 `GetCurToolCoord` 확인
- 현재 TCP 읽기 값과 pendant 표시값 비교
- 앱에서 `GetGripperSdkSummaryForDebug(true)` 실행
- pendant의 gripper 설정/활성/현재 위치와 앱 readback 비교
- 작은 `MoveL` 수행 후 실제 gripper tip 기준으로 방향 일치 확인

첫 실기 gripper 명령은 아래 순서가 모두 끝나기 전까지 금지한다.

- pendant에서 gripper 설정 확인
- SDK capability probe 통과
- gripper activation readback 확인
- 현재 gripper position readback 확인
- E-stop / speed / dry-run preview 확인
- 사용자가 현장 안전 확인 후 live execution gate를 수동으로 연다

### Preview Check

- current TCP marker가 실제 gripper 작업점과 맞는다
- ghost target이 flange가 아니라 gripper 기준으로 보인다
- point save 후 reload/run 시 같은 작업점으로 복귀한다
- 위 세 검증의 기준점은 `닫힌 finger 작업점`이며, 현장 pendant/SDK 값으로 최종 확정한다.

## Do Not

- STL origin을 그대로 TCP로 간주하지 않는다.
- pendant에서 tool coordinate를 바꿨는데 앱 preview offset을 그대로 두지 않는다.
- gripper가 달린 live FR5를 flange TCP 기준으로 MoveL 하지 않는다.
- point save를 joint-only snapshot으로 축소하지 않는다.

## Resolved Decision

- 로컬 visual 기준 후보는 `robottemplete` Control prefab에서 수동 조정한 `닫힌 finger 작업점`으로 결정되었다.
- 최종 현장 기준은 pendant/SDK에 실제 적용된 tool coordinate readback으로 확정한다.
- 남은 작업은 이 후보값을 공식 SDK의 gripper/tool-coordinate 동작, `GetActualTCPNum`, `GetCurToolCoord`, `GetActualTCPPose`, 실제 gripper open/close 동작과 비교해 production SSOT로 승격할지 판단하는 것이다.
