# Pendant V3 Teaching Sequence Runtime Phase A

## Summary

- Phase A `Sequence Runtime Adapter`를 완료했다.
- `PendantV3Points`를 V3 런타임에서 load/select/preview/execute 할 수 있는 최소 adapter를 연결했다.

## Implementation

- `TeachingSequenceRuntime`
  - `PendantV3Points` load
  - selected index state
  - selected point detail summary
  - selected point preview
  - selected point execute
- `RobotControlV3RuntimeController`
  - teaching sequence facade methods 추가
  - `Waypoint` preview/apply를 기존 `PreviewJointAngles`, `PreviewTcpPose`, `ApplyJointAngles`, `ApplyTcpPose` 경로로 연결
- `RobotControlV3DebugBridge`
  - `RunTeachingSequenceMatrixForDebug()` 추가

## Verification

- `unityctl check --type compile --json`: pass
- `RunTeachingSequenceMatrixForDebug()`: `6/6 PASS`
- `RunManualReadbackTeachingMatrixForDebug()`: `6/6 PASS`
- `RunProductLiveConfirmTokenMatrixForDebug()`: `4/4 PASS`

## Self Review

- App responsibility: sequence runtime lives under `App/Fairino/Teaching`.
- UI responsibility: no BottomBar behavior was changed in this phase.
- Existing paths reused: point preview/apply goes through existing RobotControlV3 runtime methods.
- Scope guard: loop, order editing, and BottomBar binding remain Phase B+.

## Next

- Phase B: BottomBar `Run`, `StepForward`, `StepBack`, `Stop` binding to `PendantV3Points`.
