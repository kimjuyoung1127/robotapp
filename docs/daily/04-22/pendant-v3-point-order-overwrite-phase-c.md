# Pendant V3 Point Order And Overwrite Phase C

## Summary

- Phase C `Order Editing`을 완료했다.
- Point panel에 `위로`, `아래`, `덮어쓰기` 버튼을 추가했다.
- 저장 포인트 순서 변경과 현재 readback 기준 덮어쓰기 경로를 연결했다.

## Implementation

- `point-move-panel.uxml`
  - `BtnPointUp`
  - `BtnPointDown`
  - `BtnPointOverwrite`
- `PointMoveController`
  - selected point move up/down
  - selected point overwrite with current readback
  - overwrite keeps name/moveType/speedPreset/dwellSec
- `RobotControlV3DebugBridge`
  - order/overwrite cases added to `RunTeachingSequenceMatrixForDebug()`

## Verification

- `unityctl check --type compile --json`: pass
- `RunTeachingSequenceMatrixForDebug()`: `11/11 PASS`
- `RunManualReadbackTeachingMatrixForDebug()`: `6/6 PASS`
- `RunActualUiClickMatrixForDebug()`: `98/98 PASS`

## Self Review

- UI responsibility: point edit buttons stay in PointMove panel.
- App/runtime responsibility: sequence runtime remains under App/Fairino.
- Locked policy: overwrite updates pose values while preserving name/moveType/speed/dwell.
- Scope guard: duplicate point, speed/dwell editing, and visible Loop UI remain follow-up.

## Next

- Phase C2 easy editing:
  - duplicate point
  - duplicate-name confirm
  - speed/dwell detail display
