# Pendant V3 Function v1 Scaffold

## Date

- 2026-04-22 (KST)

## Scope

- Implement the first usable `TeachingFunction` scaffold.
- Keep function behavior inside Unity teaching routines, not manufacturer program execution.

## Implementation

- Added `TeachingFunctionStore`.
- Added serializable models:
  - `TeachingFunction`
  - `TeachingFunctionStep`
- Added runtime facade methods for:
  - create from current ordered `PendantV3Points`
  - list / summary
  - detail
  - rename
  - duplicate
  - delete
  - RunOnce DryRun
- Added compact `함수` card to PointMove panel.
- Added function buttons:
  - `묶기`
  - `함수 실행`
  - `함수 이름`
  - `함수 복사`
  - `함수 삭제`
- Added function debug helpers and matrix coverage.

## Behavior

- Function v1 stores point-name references.
- Create uses the current ordered saved point sequence because multi-select point UI does not exist yet.
- RunOnce forces DryRun execution and does not open live gates.
- Missing future `FunctionRef`, IO/gripper steps, variables, and manufacturer scripts remain excluded.

## Verification

- `unityctl check --type compile`: pass
- `RunTeachingSequenceMatrixForDebug()`: `31/31 PASS`
- `RunActualUiClickMatrixForDebug()`: `110/110 PASS`
- `GetAuxLayoutSummaryForDebug()` on PointMove:
  - `viewportHorizontalVisible=False`
  - `viewportClipped=0`
  - `contextHorizontalVisible=False`
  - `contextClipped=0`

## Self Review

- Store/runtime belongs to `App/Fairino/Teaching`.
- UI remains in `PointMoveController`.
- No Program tab was added.
- No live motion gate was opened.
- Point sequence remains the execution base.

## Next

- Function v1 polish:
  - create-from-selected-points UI
  - missing-ref warning polish
  - function RunFromSelected
