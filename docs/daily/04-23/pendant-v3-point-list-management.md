# Pendant V3 Point List Management

## Context

- User needs the point tab to show point/function/speed counts at a glance.
- User needs multi-select edit/delete flows instead of only one-point-at-a-time actions.
- Long point lists should be scannable, so row action buttons need a collapse mode.
- Non-useful helper messages should be reduced.

## Implementation

- Added point inventory summary:
  - point count
  - function count
  - slow/medium/fast speed counts
  - selected point count
- Added point row multi-select.
- Added bulk actions:
  - clear selection
  - apply selected speed/dwell
  - add selected points to function candidates
  - delete selected points with confirmation
- Added row action collapse toggle.
- Reduced noisy feedback by hiding default helper messages and only showing confirmation/save/delete/bulk/error messages.

## Validation

- `unityctl check --type compile`: pass
- `RunTeachingSubviewActualClickMatrixForDebug()`: `25/25 PASS`
- `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
- `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
