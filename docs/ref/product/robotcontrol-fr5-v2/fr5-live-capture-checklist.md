# FR5 Live Capture Checklist

## Purpose

실기 FR5를 LAN으로 연결해 `현재 컨트롤러 값`을 먼저 수집하고, 그 값을 raw evidence로 남기기 위한 체크리스트다.

- raw capture는 SSOT 그 자체가 아니다.
- 먼저 실제 값을 안전하게 수집하고
- 그 다음 `정규화 문서`로 옮기고
- 마지막에 `command/state/tcp` SSOT에 반영한다.

## Why This Helps

예, 도움이 크다.

특히 아래 항목은 실기에서 먼저 읽어오면 추측을 크게 줄일 수 있다.

- 현재 controller IP
- SDK / controller / firmware version
- 현재 joint pose
- 현재 TCP pose
- 현재 tool coordinate id
- 현재 tool coordinate pose
- 현재 workobject id
- 현재 mode
- 현재 drag teach 상태
- 현재 enable 상태
- 현재 safety / fault 상태

이 값들을 먼저 확보하면:

- `Sync`를 어떻게 정의할지
- `gripper TCP`가 실제로 적용되어 있는지
- preview와 실기 pose가 어디서 어긋나는지
- point save 시 joint/tcp/tool/user 중 무엇을 같이 저장해야 하는지

를 추측이 아니라 데이터로 잠글 수 있다.

## Safety Rule

1. 첫 연결 세션에서는 `읽기 전용 capture`만 한다.
2. `MoveJ`, `MoveL`, jog는 capture 세션과 분리한다.
3. emergency stop / pendant / 현장 승인 없이 motion command를 보내지 않는다.
4. 첫 세션 목표는 `connect + read + log + disconnect`다.

## Network Prep

FAIRINO 공식 문서 기준:

- 기본 controller IP 예시는 `192.168.58.2`
- control box/button box 측 통신 포트에 PC를 연결하는 방식이 설명되어 있다
- PC는 같은 subnet으로 맞춘다
  - 예: `192.168.58.10`

공식 참고:

- [System / Network settings](https://manual.fairino.support/latest/CobotsManual/system.html)
- [Installation / RJ45 network interface group](https://manual.fairino.support/latest/CobotsManual/installation.html)
- [SDK base / RPC(ip)](https://manual.fairino.support/latest/SDKManual/C%23RobotBase.html)

## Prepare Before Cable

### PC

- FAIRINO SDK DLL 존재 확인
  - `Assets/Plugins/Fairino/libfairino.dll`
- Unity Editor 실행 확인
- 방화벽/백신이 로컬 DLL 호출을 막지 않는지 확인
- PC LAN 어댑터를 같은 subnet으로 맞출 준비

### Robot / Controller

- teach pendant로 current network settings 확인
- 현재 tool coordinate id 확인
- 현재 tool coordinate가 `그리퍼 사이 정중앙 공중점` 기준으로 이미 보정되어 있는지 확인
- current mode / drag teach / enable 상태 확인

### Local Project

- existing smoke tool 확인
  - [FairinoLiveSmokeTools.cs](/C:/Users/ezen601/Desktop/Jason/robotapp2/Assets/Editor/KineTutor3D/FairinoLiveSmokeTools.cs)
- local wrapper 확인
  - [LiveFairinoClient.cs](/C:/Users/ezen601/Desktop/Jason/robotapp2/Assets/Scripts/App/Fairino/LiveFairinoClient.cs)
  - [FairinoConnectionService.cs](/C:/Users/ezen601/Desktop/Jason/robotapp2/Assets/Scripts/App/Fairino/FairinoConnectionService.cs)

## Capture Order

1. LAN 연결
2. PC subnet 설정
3. ping 또는 browser/web UI 접근 확인
4. `Connect`
5. `GetVersion`
6. `ReadState`
7. `GetSafetyCode`
8. `GetRealtimeStateSamplePeriod`
9. `ReadCoordContext`
10. `ReadControllerFault`
11. raw log 저장
12. `Disconnect`

## Minimum Raw Fields To Save

아래 항목을 한 번의 캡처 세션마다 저장한다.

| field | source |
|---|---|
| `capturedAt` | local time |
| `pcIp` | local NIC |
| `controllerIp` | pendant or SDK |
| `sdkVersion` | `GetSDKVersion` |
| `softwareVersion` | `GetSoftwareVersion` |
| `firmwareVersion` | `GetFirmwareVersion` |
| `isConnected` | wrapper |
| `isEnabled` | wrapper/state |
| `robotMode` | `ReadState` |
| `isInDragTeach` | `ReadState` |
| `jointPosDeg[6]` | `ReadState` |
| `tcpPose[6]` | `ReadState` or `GetActualTCPPose` |
| `toolId` | `ReadState` / `GetActualTCPNum` |
| `userId` | `ReadState` / `GetActualWObjNum` |
| `toolPose[6]` | `GetCurToolCoord` |
| `wobjPose[6]` | `GetCurWObjCoord` |
| `safetyCode` | `GetSafetyCode` |
| `mainErrorCode` | `ReadControllerFault` |
| `subErrorCode` | `ReadControllerFault` |

## Recommended Storage Split

### Raw evidence

- 파일 위치 예시:
  - `docs/private/fr5-captures/YYYY-MM-DD/`
  - 또는 Git 미추적 로컬 폴더
- 포맷:
  - `.json`
  - `.txt`
  - 필요 시 pendant 사진

### Normalized note

- `docs/daily/MM-DD/`에 요약
- `docs/ref/product/robotcontrol-fr5-v2/` 문서에는 해석된 결과만 반영

## Existing Smoke Entry

현재 저장소에는 motion 없는 read-only smoke 도구가 이미 있다.

- Menu: `KineTutor3D/RobotControl/Run FAIRINO Live Smoke Test`
- 파일: [FairinoLiveSmokeTools.cs](/C:/Users/ezen601/Desktop/Jason/robotapp2/Assets/Editor/KineTutor3D/FairinoLiveSmokeTools.cs)

현재 이 도구는:

1. `Connect`
2. `GetVersion`
3. `ReadState`
4. `Disconnect`

만 수행한다.

## Better First Capture

기존 smoke 다음 단계에서 아래를 추가하면 좋다.

- `GetSafetyCode`
- `GetRealtimeStateSamplePeriod`
- `ReadCoordContext`
- `ReadControllerFault`
- 결과를 JSON 파일로 저장

## Decision Rule

- raw capture 값은 곧바로 SSOT에 쓰지 않는다.
- 같은 조건에서 2회 이상 반복 확인되면 `normalized fact`로 승격한다.
- pendant 표시값과 SDK 읽기값이 다르면 우선 `blocked`로 두고 원인부터 찾는다.

## Good First Session Goal

한 번의 성공적인 첫 세션은 아래 정도면 충분하다.

- 연결 성공
- version 수집 성공
- 현재 joint/TCP 수집 성공
- tool/user context 수집 성공
- safety/fault 수집 성공
- disconnect 성공

이 정도만 확보해도 다음 개발은 훨씬 쉬워진다.
