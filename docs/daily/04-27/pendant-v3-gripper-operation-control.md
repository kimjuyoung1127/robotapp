# Pendant V3 Gripper Operation Control

Date: 2026-04-27 (KST)

## Decision

- `그리퍼 / I/O` 보조 패널은 `포인트`가 아니라 `조작 > 기본` 흐름에 둔다.
- 그리퍼 기본 상태는 `position=100`, visual open ratio `1.00`인 완전 열림이다.
- 완전 닫힘은 `position=0`, visual open ratio `0.00`이며 finger 안쪽이 서로 닿는 상태로 본다.
- 가운데 `TcpMarker` 구체 prefab이 grip target으로 감지되면 close 요청이 `0%`여도 visual/mock은 안전 정지선에서 멈추고 `holding object` 상태를 남긴다.

## Official SDK Basis

- FAIRINO C# SDK gripper flow:
  - `SetGripperConfig(company, device, softversion, bus)`
  - `ActGripper(index, action)`
  - `MoveGripper(index, pos, vel, force, max_time, block, type, rotNum, rotVel, rotTorque)`
- Readback:
  - `GetGripperMotionDone`
  - `GetGripperActivateStatus`
  - `GetGripperCurPosition`
  - `GetGripperCurSpeed`
  - `GetGripperCurCurrent`
  - `GetGripperVoltage`
  - `GetGripperTemp`
- SDK 문서상 position/speed/force/current는 `0~100` percentage 계약이다.

## Changed

- `IoPanelController` 표시 조건을 `NavMotion + TabEasyMotion` / `BottomTabEasyMotion`으로 옮겼다.
- `그리퍼 / I/O` 패널에 position slider와 numeric input을 추가했다.
- `RobotControlPeripheralFacade`를 bool open/close 중심에서 commanded/actual position percent 중심으로 확장했다.
- `RobotControlPeripheralState`와 `RobotControlV3RuntimeSnapshot`에 commanded/actual/speed/force/object-detected/holding/stop-percent 상태를 추가했다.
- `FR5EndEffectorAttachment`는 `TcpMarker` renderer가 있으면 grip object로 보고 close 중 stop ratio를 제공한다.
- legacy local state의 `NavIo` / `BottomTabIo`는 각각 `NavMotion` / `BottomTabEasyMotion`으로 normalize한다.

## Verification

- `dotnet build Assembly-CSharp.csproj --no-restore`: pass, errors `0`.
- `git diff --check`: pass.
- `unityctl status --wait`: timeout. Editor IPC가 ready 응답을 주지 않아 Unity runtime matrix는 재실행하지 못했다.

## Notes

- 실제 live `MoveGripper`는 기존 safety gate를 유지한다. operator confirm, readback, production policy 없이 바로 열지 않는다.
- 현재 object stop은 visual/mock 안전 모델이다. 실기에서는 SDK readback의 position/current/motionDone을 pendant 상태와 비교한 뒤 force/current threshold 정책을 별도 확정해야 한다.
