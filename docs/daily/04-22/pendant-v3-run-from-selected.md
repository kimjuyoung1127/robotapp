# Pendant V3 Run From Selected

## Date

- 2026-04-22 (KST)

## Scope

- Implement commercial pendant style run-from-line behavior with beginner-facing wording.
- Keep the action inside the Point/Teaching panel.

## Implementation

- Added `선택부터` button to the PointMove action row.
- Added UI binding in `PointMoveController`.
- Added runtime execution entry:
  - `ExecuteTeachingSequenceFromPoint(string pointName)`
- Added debug entries:
  - `RunPointMoveFromSelectedForDebug(string pointName)`
  - `RunTeachingSequenceFromPointForDebug(string pointName)`
- Behavior:
  - selected point through end of `PendantV3Points`
  - one-shot execution even when loop mode is ON
  - clear feedback when the point is missing
  - blocks when another sequence runner is active

## Verification

- `unityctl check --type compile`: pass
- `RunTeachingSequenceMatrixForDebug()`: `26/26 PASS`
- `RunActualUiClickMatrixForDebug()`: `105/105 PASS`
- `GetAuxLayoutSummaryForDebug()` on PointMove:
  - `viewportHorizontalVisible=False`
  - `viewportClipped=0`
  - `contextHorizontalVisible=False`
  - `contextClipped=0`

## Self Review

- Runtime execution lives in App/Fairino.
- Visible action stays in `PointMoveController`.
- BottomBar was not crowded with another command.
- Loop mode behavior remains separate and explicit.
- No live motion gate was opened.

## Next

- `Phase E - Function / Group Planning`.
