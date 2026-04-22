# Pendant V3 Function v1 Polish

## Date

- 2026-04-22 (KST)

## Scope

- Polish Function v1 after the first scaffold.
- Add selected-point based function creation.
- Add missing-reference warning.
- Add function RunFromSelected.
- Verify with screenshots.

## Implementation

- Added function candidate point selection:
  - `선택 추가`
  - `선택 초기화`
  - compact candidate summary
- Function create now uses selected point refs when candidates exist.
- Function create falls back to all saved points when no candidates are selected.
- Function detail now reports missing point refs from current `PendantV3Points`.
- Added `함수 선택부터` action.
- Compacted visible function summary/detail copy so debug raw lists do not overflow the aux panel.
- Added `RunFunctionActualClickMatrixForDebug()` to keep function button click verification under the unityctl IPC response limit.

## Verification

- `unityctl check --type compile`: pass
- `RunTeachingSequenceMatrixForDebug()`: `33/33 PASS`
- `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
- `GetAuxLayoutSummaryForDebug()` on PointMove:
  - `viewportHorizontalVisible=False`
  - `viewportClipped=0`
  - `contextHorizontalVisible=False`
  - `contextClipped=0`

## Screenshots

- `Artifacts/pendant-v3-function-polish-final.png`
- `Artifacts/pendant-v3-function-polish-action-buttons.png`

## Notes

- Full `RunActualUiClickMatrixForDebug()` is now long enough to hit the current unityctl IPC 30s response limit.
- Function actual click coverage is split into `RunFunctionActualClickMatrixForDebug()`.

## Self Review

- Runtime/store remains in App/Fairino.
- UI remains in PointMove.
- No Program tab was added.
- No live motion gate was opened.

## Next

- Missing-ref repair UX, or
- IO/gripper sequence block design.
