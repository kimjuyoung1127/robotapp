# Pendant V3 Teaching Sequence SSOT Reconciliation

## Date

- 2026-04-22 (KST)

## Purpose

- Compare the teaching sequence SSOT against the current implementation after Phase C review fixes.
- Remove stale "not wired" statements for features that are now implemented.
- Lock the next missing work before starting Phase C2.

## Updated Docs

- `docs/ref/product/pendant-v3/teaching-sequence-execution-plan.md`
- `docs/ref/product/pendant-v3/progress-checklist.md`
- `docs/daily/INDEX.md`

## Current Implementation Confirmed

- Manual readback simulation is wired through the runtime readback path.
- Point save stores current readback joints/TCP.
- `PendantV3Points` loads into the teaching sequence runtime.
- `Run` resolves pending preview first, then saved sequence RunOnce.
- `Step▶` / `Step◀` select and preview saved points.
- Point order can be changed with up/down controls.
- Selected point can be overwritten with current readback.
- Actual click matrix includes point reorder/overwrite controls.

## Remaining Gaps

- Duplicate point.
- Duplicate-name overwrite confirmation.
- Visible point detail UI for joints/TCP/move type/speed/dwell.
- Speed/dwell editing.
- Delete/overwrite consequence confirmation copy.
- `선택 지점부터 실행`.
- Visible loop toggle/status.
- Execution-time edit lock in visible UI.
- Function/group model.
- IO/gripper sequence blocks.
- Stable point IDs if v1 name keys become limiting.

## Next Slice

- Proceed with `Phase C2 - Easy Editing`.
- First candidate execution unit: point detail UI + duplicate point, because those make the current teaching list easier than a basic commercial pendant workflow without opening live motion risk.
