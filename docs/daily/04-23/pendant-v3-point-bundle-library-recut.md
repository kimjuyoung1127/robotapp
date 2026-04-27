# Pendant V3 Point / Bundle Library Re-cut

## Context

- User said the current `묶음` tab is too hard to use.
- Requested new split:
  - `묶음` tab should behave like a function library manager.
  - Point selection should happen in `포인트`.
  - `시퀀스` should be able to use saved functions directly.

## Implementation

- Moved bundle creation flow to `포인트` subview.
  - added `함수 라이브러리로 저장` card
  - candidate summary
  - bundle name input
  - clear candidate / save buttons
- Tightened the wording toward user intent.
  - `선택 묶음 준비`
  - `함수 이름`
  - `함수 등록`
- Registration card now explains the source more directly so users can register without first studying the old bundle candidate model.
- Slimmed `묶음` subview into library management only.
  - keep list, detail, rename, duplicate, delete, bulk duplicate/delete
  - removed create/run-centric actions from the bundle tab itself
- Reworked `시퀀스` bundle insertion into a picker modal.
  - `묶음 추가` opens `묶음 라이브러리` modal
  - user chooses one bundle
  - `시퀀스 추가` confirms and inserts a `BundleRef` block
- Tightened point-side creation fallback.
  - when bundle candidates are empty, the currently viewed point is used first
  - only then does it fall back to broader point sources
- Kept `현재 위치 저장` only in `포인트`
- Removed old detail-card speed/dwell editing controls so per-point timing stays in the edit modal

## Validation

- `unityctl check --type compile`: pass
- Actual click smoke:
  - `BtnSequenceSubview`: clicked
  - `BtnBlockAddBundle`: clicked
  - `GetPointMoveControllerSummary()`: `bundlePickerOpen=True`
  - `BtnBundlePickerConfirm`: clicked
  - `GetTeachingBlockSequenceSummaryForDebug()`: `blocks=1`, `BundleRef:ONE_PICK_TRUE`
- Current-point registration smoke:
  - `BtnPointSubview` click
  - clear selected point rows
  - recall current point
  - `CreateTeachingFunctionForDebug("ONE_PICK_TRUE")`
  - result: `steps=1`

## Notes

- Direct smoke required forcing `SceneNavigator.LoadByName("RobotControlV3")` during play because the default boot flow still returns to `Onboarding`.
- The structural intent is now:
  - `포인트` = select points and save into library
  - `묶음` = inspect/manage library
  - `시퀀스` = consume points and bundles
