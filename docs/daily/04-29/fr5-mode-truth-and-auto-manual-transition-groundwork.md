# FR5 Mode Truth And Auto Manual Transition Groundwork

Date: 2026-04-29 (KST)

## What Changed

- `LiveFairinoClient` mode readback was widened beyond `robot_mode`.
  - current code now probes `robot_mode`, `robotMode`, `mode`, and optional getter fallback names when available
  - goal is to stop `latest-state.json` mode truth from staying stale when joint/TCP readback is still fresh
- `FairinoConnectionService` gained a verified controller-mode path.
  - `SetMode` alone is no longer the only path
  - new flow is `SetMode -> SyncCurrentState retry -> requested/actual mode match 확인`
- `RobotControlV3RuntimeController` now uses the verified path for `RequestAutoMode()` and `RequestManualMode()`
- `Fr5LiveStateRecorder` now writes controller-truth deltas:
  - `controller-mode`
  - `drag-teach`
  - `servo-truth`
- V3 `QuickControllerMode` summary was widened to include:
  - current controller truth
  - last mode transition summary

## Why

- latest field report was:
  - joint/TCP latest-state values were moving correctly
  - but `mode` in `latest-state.json` did not follow external teach pendant `manual <-> auto` toggles
- this suggested a narrower problem than readback failure:
  - pose truth was alive
  - controller mode truth was either read too narrowly or not re-verified strongly enough

## Verification

- code verification:
  - `dotnet build /Users/family/jason/FR5UNITY/robotapp/robotapp.slnx -nologo`
  - result: `warnings only`, `errors 0`
- not yet verified on hardware in this entry:
  - external teach pendant `manual -> auto -> manual`
  - `Artifacts/live/fr5/latest-state.json` mode follow
  - V3 mode surface follow

## Current Interpretation

- `P0` truth unification is now materially in code
- `P1` auto transition path is now materially in code
- `P2` manual path shares the same verification backbone, but field proof is still pending
- `P3` operator-facing summary is partially in code, but dedicated wording and normal-case UX are still pending

## Next Verify

1. Toggle external teach pendant `manual -> auto -> manual`
2. Watch `Artifacts/live/fr5/latest-state.json` `mode`
3. Watch V3 `QuickMode / QuickControllerMode / StatusMode`
4. Only after this call `pendant-free mode truth` green
