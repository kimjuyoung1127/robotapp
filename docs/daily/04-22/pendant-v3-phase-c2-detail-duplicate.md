# Pendant V3 Phase C2 Detail + Duplicate

## Date

- 2026-04-22 (KST)

## Scope

- Start `Phase C2 - Easy Editing`.
- Implement the first execution unit:
  - selected point detail UI
  - duplicate selected point

## Implementation

- Added selected point detail card to `point-move-panel.uxml`.
- Added compact detail styling to `point-move-panel.uss`.
- Added `PointDetailTitle`, `PointDetailMeta`, `PointDetailJoints`, and `PointDetailTcp` bindings.
- Added `BtnPointDuplicate`.
- Added duplicate behavior in `PointMoveController`.
  - Requires a selected point.
  - Copies saved joints, TCP, move type, speed preset, and dwell.
  - Inserts the copy directly after the source point.
  - Creates a unique name with `_COPY`, `_COPY_2`, and later suffixes if needed.
- Tightened normal point save to validate only the point name.
  - Dirty coordinate inputs are ignored by readback save.
  - Saved joints/TCP still come from current readback snapshot only.
- Added debug/QA entry points:
  - `DuplicatePointMoveForDebug(string pointName)`
  - `GetPointMoveDetailForDebug()`
- Expanded matrices:
  - `RunTeachingSequenceMatrixForDebug()`
  - `RunActualUiClickMatrixForDebug()`

## Verification

- `unityctl check --type compile`: pass
- `RunTeachingSequenceMatrixForDebug()`: `13/13 PASS`
- `RunActualUiClickMatrixForDebug()`: `99/99 PASS`
- `GetAuxLayoutSummaryForDebug()` on PointMove:
  - `viewportHorizontalVisible=False`
  - `viewportClipped=0`
  - `contextHorizontalVisible=False`
  - `contextClipped=0`

## Self Review

- UI detail and duplicate authoring stay in `PointMoveController`.
- Runtime sequence execution remains in App/Fairino teaching runtime.
- Point save still requires user-entered names; duplicate auto-suffixing is limited to the copy action.
- No live motion gate was opened.
- No boundary/collision hard gate was added.

## Remaining C2

- Speed/dwell editing.
- Delete/overwrite consequence confirmation copy.
- Duplicate-name overwrite confirmation.
- Execution-time edit lock in visible UI.
