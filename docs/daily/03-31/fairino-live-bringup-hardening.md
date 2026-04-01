# 2026-03-31 — FAIRINO Live Bring-Up Hardening

## Summary
- FR5 `RobotControl` 실기 연결 경로를 `Connect -> Auto/Drag 정리 -> Enable -> Sync -> small MoveJ/MoveL` 기준으로 정리했다.
- Live v1 범위에서 `ServoJ` / `ServoCart`를 일반 bring-up 조작에서 비활성화했다.
- UI는 authored-first 원칙을 유지하고, shared 디자인 토큰 경로를 그대로 사용했다.

## What Changed
- `IFairinoRobotClient`에 reconnect, drag teach 종료, auto mode, coord context, controller fault, reset error 계약을 추가했다.
- `LiveFairinoClient`에 `tool/user` 문맥 캐시와 fault/safety 조회를 추가했다.
- `FairinoConnectionService`에서 Live 초기화 시퀀스를 한 곳으로 모았다.
- `ReadState()` 폴링은 경량화하고, 상세 상태 갱신은 `Connect`, `Enable`, `Sync`, `ResetErrors` 시점으로 옮겼다.
- `Disconnect` / connection lost 시 `tool/user/fault/safety/sample` 캐시를 함께 비우도록 바꿨다.
- `FairinoConnectionPanel`, `FairinoStatePanel`, `RobotControlDiagnosticsDrawer`에 `Mode`, `Drag`, `Tool/User`, `Safety`, `Fault`, `Reset Error` 표시를 추가했다.
- `FAIRINO_FR5.json`에 `liveDefaults`를 추가했다.

## Validation
- `unityctl check --project C:/Users/ezen601/Desktop/Jason/robotapp2 --type compile --json`
  - passed
- `unityctl test --project C:/Users/ezen601/Desktop/Jason/robotapp2 --mode edit --json`
  - `438 passed / 9 failed`
  - 남은 실패는 기존 축:
    - `MathReadinessPanelTests` 7건
    - `RobotControlSceneCoordinatorTests.FindSceneRuntimeRoot_UsesExistingSceneRoot`
    - `SceneCameraDirectorTests.ConfigureForScene_Sandbox_AppliesZoomedOutProfile`

## Operational Notes
- Live 폴링은 가볍게 유지한다. 상태 틱마다 `coord/fault/safety/sample`을 다시 읽지 않는다.
- 연결이 끊기면 UI가 이전 `Tool/User`나 fault를 계속 보여주지 않도록 캐시를 즉시 초기화한다.
- 실기 bring-up에서는 `ServoJ` / `ServoCart`를 쓰지 않고 `MoveJ` / `MoveL`만 사용한다.
