# FR5 Tiny Joint Live Success Pattern

Date: 2026-04-29

## Summary

- motion-capable sibling session now rechecks `mode=0` before tiny `MoveJ` dispatch
- current branch tiny joint live path is green only in a narrow verified scope
- verified hardware movement now includes:
  - `J1 + / -`
  - `J2 + / -`
  - `J3 + / -`
  - `J4 + / -`
  - `J5 -1deg`
  - `J6 +1deg`
  - `J6 -1deg`
  - `J5 +1deg`
  - same-day repeat cycles on `J5/J6`

## Evidence Rule

이 날짜 기준 real joint delta truth는 immediate QA artifact `afterMovement`가 아니라:

1. execute summary `OK`
2. post-run `SyncCurrentStateForDebug()`
3. post-run `RefreshLiveEvidenceForDebug()`
4. `Artifacts/live/fr5/latest-state.json`

순서로 판정한다.

## Verified Runs

### J6 +1deg

- artifact: `Artifacts/live/qa/20260429-174934-joint-j6-plus.json`
- readback:
  - before `1.1638981103897095`
  - after `2.1635451316833496`
  - delta `+0.9996deg`

### J6 -1deg

- artifact: `Artifacts/live/qa/20260429-175135-joint-j6-minus.json`
- readback:
  - before `2.1637628078460693`
  - after `1.1636805534362793`
  - delta `-1.0001deg`

### J5 +1deg

- artifact: `Artifacts/live/qa/20260429-175158-joint-j5-plus.json`
- readback:
  - before `-90.0217514038086`
  - after `-89.02188873291016`
  - delta `+0.9999deg`

### J5 -1deg

- artifact: `Artifacts/live/qa/20260429-180034-joint-j5-minus.json`
- readback:
  - before `-89.02167510986328`
  - after `-90.02153778076172`
  - delta `-0.9999deg`

### J1~J4 all-joint check

- `J1 + / -`
  - artifacts:
    - `Artifacts/live/qa/20260429-180640-joint-j1-plus.json`
    - `Artifacts/live/qa/20260429-180644-joint-j1-minus.json`
  - observed delta:
    - `+1.0012deg`
    - `-1.0016deg`
- `J2 + / -`
  - artifacts:
    - `Artifacts/live/qa/20260429-180649-joint-j2-plus.json`
    - `Artifacts/live/qa/20260429-180653-joint-j2-minus.json`
  - observed delta:
    - `+1.0010deg`
    - `-1.0010deg`
- `J3 + / -`
  - artifacts:
    - `Artifacts/live/qa/20260429-180658-joint-j3-plus.json`
    - `Artifacts/live/qa/20260429-180703-joint-j3-minus.json`
  - observed delta:
    - `+1.0016deg`
    - `-1.0029deg`
- `J4 + / -`
  - artifacts:
    - `Artifacts/live/qa/20260429-180706-joint-j4-plus.json`
    - `Artifacts/live/qa/20260429-180711-joint-j4-minus.json`
  - observed delta:
    - `+1.0003deg`
    - `-0.9999deg`

### Repeatability check

- `J5 +1deg`
  - artifact: `Artifacts/live/qa/20260429-180121-joint-j5-plus.json`
  - delta `+1.0047deg`
- `J6 +1deg`
  - artifact: `Artifacts/live/qa/20260429-180125-joint-j6-plus.json`
  - delta `+1.0044deg`
- `J6 -1deg`
  - artifact: `Artifacts/live/qa/20260429-180128-joint-j6-minus.json`
  - delta `-1.0047deg`
- `J5 -1deg`
  - artifact: `Artifacts/live/qa/20260429-180154-joint-j5-minus.json`
  - delta `-1.0010deg`
- `J5 +1deg`
  - artifact: `Artifacts/live/qa/20260429-180157-joint-j5-plus.json`
  - delta `+1.0057deg`

### Requested `+-3deg` check

- user asked for `deg +-3`
- current helper path did not validate a true `3deg` move
- with current tiny helper settings, actual observed movement stayed around `1deg`
- when trying to raise shell increment, the path snapped to `5` and was blocked as `tiny MoveJ range exceeded`
- blocked artifact:
  - `Artifacts/live/qa/20260429-180810-joint-j1-plus.json`

## Interpretation

- this is enough to declare the current narrow `J1~J6 effective tiny helper path` green
- `J5/J6` additionally have same-day repeatability evidence
- this is still not enough to declare a true `3deg` tiny helper path green
- this is not enough to declare broad arm motion green
- pendant auto mode is still part of the present field procedure

## Follow-Up

- document the narrow success pattern as a reusable skill
- separate helper delta from actual moved delta and open a true `3deg` smoke path
- move next product track to pendant-free `auto/manual` mode transition
- do not relax safety gates just because this narrow path succeeded
