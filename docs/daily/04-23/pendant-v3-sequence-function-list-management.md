# Pendant V3 Sequence And Function List Management

## Context

- User asked to apply the point-list management pattern to the `시퀀스` and `함수` tabs too.
- The same pattern means:
  - inventory count summary
  - multi-select
  - bulk actions
  - row button collapse
  - reduced noisy messages

## Implementation

- Sequence tab:
  - Added sequence inventory summary.
  - Added multi-select row state.
  - Added bulk clear/delete.
  - Protected `PendantV3Points` from sequence bulk delete.
  - Added row action collapse toggle.
- Function tab:
  - Added function inventory summary.
  - Added multi-select row state.
  - Added bulk clear/duplicate/delete.
  - Added row action collapse toggle.
- Debug summary optimization:
  - Switched function UI matrix summary to compact output so large accumulated function stores do not exceed unityctl IPC response limits.
  - Added focused matrix `RunSequenceFunctionBulkManagementMatrixForDebug()`.

## Validation

- `unityctl check --type compile`: pass
- `RunSequenceFunctionBulkManagementMatrixForDebug()`: `11/11 PASS`
- `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
- `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
