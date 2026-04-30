# FR5 Doc Slimming And SSOT Links

Date: 2026-04-30 (KST)

## Summary

- Slimmed `robotcontrol-next-session-handoff.md` down to FR5 field operations only.
- Slimmed `fr5-live-field-checklist.md` down to current-session checks only.
- Kept gripper detail in `fr5-gripper-live-success-pattern.md` and replaced duplication with links.
- Recorded that `V1` and `V2` are no longer active operating surfaces and should keep only purpose/history context.
- Added a separate historical document for FR5 field narrative and trial/error: `fr5-live-field-history.md`.

## What Changed

- `docs/ref/product/ux/robotcontrol-next-session-handoff.md`
  - removed old V2 implementation strategy, branch plan, folder plan, and validation history
  - kept only current FR5 operating baseline, next-session start sequence, green baseline, and open items
- `docs/ref/product/roadmap/fr5-live-field-checklist.md`
  - removed historical field narrative and old runbook bulk
  - kept only current network baseline, session start checks, readback-only checks, gripper/tiny-joint gates, evidence files, and go/no-go
- `docs/status/ACTIVE-WORK-INDEX.md`
  - updated read-order labels so handoff/checklist/SSOT roles are clearer at a glance
- `docs/ref/product/roadmap/fr5-live-field-history.md`
  - restored the removed field narrative as a history-only document instead of putting it back into the active checklist

## Why

- The previous handoff and checklist documents mixed current operational truth with historical logs and V2 implementation planning.
- The current FR5 live path already has separate success-pattern SSOT documents for gripper and tiny joint work.
- The docs are easier to trust when summary surfaces link outward instead of repeating stale detail.

## Validation

- Re-read the slimmed handoff and checklist after editing to confirm they now point to the gripper/tiny-joint SSOT docs instead of duplicating them.
- Re-read `ACTIVE-WORK-INDEX.md` to confirm the read order still matches the current FR5 live priority.

## Follow-Up

- If `FR5-LIVE-INTEGRATION-ROADMAP.md` grows further, split current baseline vs verified history there as a separate cleanup pass.
- Keep future gripper detail only in `fr5-gripper-live-success-pattern.md` unless the summary docs truly need a local pointer.
