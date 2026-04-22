# Pendant V3 Teaching Sequence Run/Step Phase B

## Summary

- Phase B `BottomBar Run/Step Binding`을 완료했다.
- `Run`은 pending preview를 우선 실행하고, pending preview가 없으면 `PendantV3Points` 저장 sequence를 실행한다.
- `StepForward` / `StepBack`은 saved point preview-only로 연결했다.

## Implementation

- `RobotControlV3RuntimeController`
  - `ExecutePrimaryAction()`에 saved sequence fallback을 추가했다.
  - `StepForward()` / `StepBackward()`를 teaching sequence preview로 연결했다.
  - `StopMotion()`이 running `WaypointCycleRunner`를 정리할 수 있게 유지했다.
- `RobotControlV3DebugBridge`
  - `RunTeachingSequenceMatrixForDebug()`를 `9/9` 케이스로 확장했다.

## Verification

- `unityctl check --type compile --json`: pass
- `RunTeachingSequenceMatrixForDebug()`: `9/9 PASS`
- `RunManualReadbackTeachingMatrixForDebug()`: `6/6 PASS`
- `RunProductLiveConfirmTokenMatrixForDebug()`: `4/4 PASS`
- `RunActualUiClickMatrixForDebug()`: `95/95 PASS`

## Self Review

- Run priority follows the locked policy: pending preview first, saved sequence second.
- Step buttons remain preview-only and do not execute motion.
- No new left-nav tab or visible Loop UI was added.
- Sequence execution reuses existing runtime preview/apply paths.

## Next

- Phase C: point order/editing
  - move up/down
  - overwrite with current readback
  - duplicate-name confirmation
