# FR5 Direct Operator Acceptance And UI Surface

Date: 2026-04-30
Repo: `/Users/family/jason/FR5UNITY/robotapp`

## What Changed

- added a direct-input tiny joint path that accepts `requestedDeltaDeg` explicitly instead of relying on shell increment snapping
- wired joint numeric input `적용` into the same popup-confirm product path used by the verified live motion flow
- added QA artifact fields:
  - `requestedDeltaDeg`
  - `actualDeltaDeg`
  - `requestedSequenceKind`
  - `operatorNextAction`
  - `failureCategory`
- added operator-facing copy surface:
  - header `다음 행동`
  - Safety/Diagnostics failure taxonomy + recovery hint
- seeded successful tiny MoveJ target back into the live baseline so consecutive direct-input tests do not reuse stale readback state
- added a small tiny-range tolerance to avoid floating-point `5.000xdeg` false blocks at the `5deg` boundary

## Verified Today

- gripper repeat path still green on the current branch
  - `100 -> 70 -> 0 -> 100`
- `J6 true +3deg` is green on the direct-input popup-confirm product path
  - artifact: `Artifacts/live/qa/20260430-140235-joint-j6-plus-3.json`
  - `requestedDeltaDeg=3.0`
  - `actualDeltaDeg=3.0004deg`
- header and Safety/Diagnostics surface is now populated from runtime/operator copy fields
  - verified on the readback-only locked state:
    - header next action text present
    - diagnostics `now / primary / why` text present

## Still Open

- `J6 +5deg` was still blocked before the tolerance patch at the exact tiny-range boundary
  - artifact before final re-smoke: `Artifacts/live/qa/20260430-140238-joint-j6-plus-5.json`
- final live re-smoke after the tolerance patch is still pending
- failure UI still needs one more field pass on real blocked states such as:
  - `tiny range exceeded`
  - `mode != 0`
  - `gripper activation not ready`

## Interpretation

- current branch no longer treats `shell increment` as the only tiny-joint operator path
- direct-input joint apply now has its own acceptance track
- today’s locked truth is:
  - `true +3deg` green on `J6`
  - `5deg` code path adjusted, but final live confirmation still pending
