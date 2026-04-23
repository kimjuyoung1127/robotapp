# Pendant V3 Point Row Action Modal

## Context

- User feedback: row `수정` was not useful because it only selected the point and forced the user to scroll down to edit.
- User feedback: `후보` was unclear; users need to know it means adding the point to a function-building selection.

## Decision

- Point row click remains selection/detail recall.
- Point row buttons are now explicit actions:
  - `실행`
  - `미리보기`
  - `편집`
  - `함수 추가`
- Each row action opens a point action modal first.
- `편집` modal owns point name, speed preset, dwell, overwrite, duplicate, and delete actions.

## Validation

- `unityctl check --type compile`: pass
- `RunTeachingSubviewActualClickMatrixForDebug()`: `16/16 PASS`
- `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
- `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
