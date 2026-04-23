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

## 2026-04-23 Runtime Text Recheck

- User reported the point row still showed old labels: `이동 / 미리보기 / 수정 / 후보`.
- Source search confirmed old row labels no longer exist in `PointMoveController`.
- Forced refresh path:
  - `play stop`
  - `asset refresh`
  - `unityctl check --type compile`
  - reopen/play `RobotControlV3`
  - reload `RobotControlV3` scene during Play for direct runtime QA
- Runtime UITK text check confirmed:
  - `BtnPointRowMove`: `실행`
  - `BtnPointRowEdit`: `편집`
  - `BtnPointRowFunctionCandidate`: `함수 추가`
- `RunTeachingSubviewActualClickMatrixForDebug()`: `19/19 PASS`
- Conclusion: if the old labels are still visible on-screen, the active Play UI tree is stale and needs Play restart or RobotControlV3 scene re-entry.
