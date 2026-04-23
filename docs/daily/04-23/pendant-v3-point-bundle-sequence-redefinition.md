# Pendant V3 Point / Bundle / Sequence Redefinition

## Context

- User clarified that `함수` means a reusable group of points, not a manufacturer program function.
- User asked to change terminology and tab order, and to allow the sequence tab to insert a `Pick` group as a single block.

## Implementation

- User-facing terminology changed from `함수` to `묶음`.
- Tab order changed to `포인트 / 묶음 / 시퀀스`.
- Added `TeachingSequenceBlock` and `TeachingBlockSequence`.
  - `PointRef`
  - `BundleRef`
  - stored as `PendantV3Blocks`
- Added `작업 시퀀스` UI inside the sequence tab.
  - add selected point
  - add selected bundle
  - preview
  - run
  - move row up/down
  - delete row
- Block sequence execution expands blocks into a temporary waypoint sequence and runs it through existing Unity/Mock DryRun runner.

## Validation

- `unityctl check --type compile`: pass
- `RunTeachingBlockSequenceMatrixForDebug()`: `9/9 PASS`
- `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
- `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
- `RunSequenceFunctionBulkManagementMatrixForDebug()`: `11/11 PASS`
- UITK tab text/order:
  - `BtnPointSubview = 포인트`
  - `BtnFunctionSubview = 묶음`
  - `BtnSequenceSubview = 시퀀스`
