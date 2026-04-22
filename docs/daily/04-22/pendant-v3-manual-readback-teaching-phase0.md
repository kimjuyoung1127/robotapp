# Pendant V3 Manual Readback Teaching Phase 0

## Summary

- Phase 0 `Manual readback teaching simulation`을 완료했다.
- Mock에서 실기기 수동 이동 readback을 시뮬레이션하고, 기존 상태 이벤트 경로로 RobotStage/값/저장/recall까지 흐르는지 검증했다.

## Implementation

- `Assets/Scripts/App/Fairino/Teaching/ManualReadbackTeachingProbe.cs`
  - Mock manual readback을 `FairinoConnectionService` 경유로 발행한다.
- `Assets/Scripts/App/Fairino/Teaching/TeachingPointStoreAdapter.cs`
  - `PendantV3Points` 저장소 요약/로드 경계를 제공한다.
- `Assets/Scripts/App/Fairino/Teaching/TeachingSequenceState.cs`
  - 후속 sequence runtime state DTO를 추가했다.
- `FairinoConnectionService.SimulateExternalReadbackForDebug(...)`
  - Mock 상태를 갱신한 뒤 `OnStateUpdated`를 발행한다.
- `MockFairinoClient.SimulateExternalReadback(...)`
  - 외부 수동 이동처럼 joint/TCP 상태를 갱신한다.
- `RobotControlV3DebugBridge.RunManualReadbackTeachingMatrixForDebug()`
  - readback -> RobotStage -> values -> save -> recall matrix를 추가했다.

## Verification

- `unityctl check --type compile --json`: pass
- `RunManualReadbackTeachingMatrixForDebug()`: `6/6 PASS`
- `RunProductLiveConfirmTokenMatrixForDebug()`: `4/4 PASS`
- `RunActualUiClickMatrixForDebug()`: `95/95 PASS`

## Self Review

- App responsibility: readback simulation and store adapter live under `App/Fairino/Teaching`.
- UI responsibility: no UI fields are written directly by the probe.
- State source: simulated readback flows through `FairinoConnectionService.OnStateUpdated`.
- Live safety: no real live command path was opened.

## Next

- `Program Run/Step queue`
- `Point MoveJ production IK policy`
