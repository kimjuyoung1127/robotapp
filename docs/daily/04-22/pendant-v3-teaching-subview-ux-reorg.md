# Pendant V3 Teaching Subview UX Reorg

## Context

- `NavPoints` had grown into one long mixed panel.
- User asked to split the UX into `포인트 / 시퀀스 / 함수`, promote `현재 위치 저장`, move row actions inline, consolidate execution controls, and simplify function creation.

## Implementation

- Added internal segmented tabs:
  - `포인트`
  - `시퀀스`
  - `함수`
- Promoted `BtnPointSave` to the large `현재 위치 저장` primary action.
- Added row actions for saved points:
  - `이동`
  - `미리보기`
  - `수정`
  - `후보`
- Moved sequence execution controls into the sequence subview.
- Updated function copy to `선택한 포인트 N개 → 함수 만들기`.
- Added `RunTeachingSubviewActualClickMatrixForDebug()`.

## Validation

- `unityctl check --type compile`: pass
- `RunTeachingSubviewActualClickMatrixForDebug()`: `10/10 PASS`
- `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
- `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
- `RunTeachingActualClickMotionMatrixForDebug()`: `6/6 PASS`
- `GetAuxLayoutSummaryForDebug()`: horizontal visible false, clipped 0
- Screenshots:
  - `Artifacts/pendant-v3-subview-points.png`
  - `Artifacts/pendant-v3-subview-sequence.png`
  - `Artifacts/pendant-v3-subview-function.png`

## Note

- Full `RunActualUiClickMatrixForDebug()` can exceed the unityctl IPC 30s response window.
- The split matrices are the accepted verification path for this slice.
