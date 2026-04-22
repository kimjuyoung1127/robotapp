# Pendant V3 Teaching Sequence Execution Plan

## Summary

- `PendantV3Points`를 단순 저장 목록에서 실행 가능한 teaching sequence로 승격하는 전체 계획을 추가했다.
- 오늘 이후 우선순위는 boundary/collision hard gate가 아니라 저장 포인트 기반 실행 흐름이다.

## New SSOT

- `docs/ref/product/pendant-v3/teaching-sequence-execution-plan.md`

## Current Finding

- V3는 point save/recall/list/delete/rename/export/cleanup을 이미 지원한다.
- 저장된 point는 단일 preview/apply 가능하다.
- `WaypointCycleRunner`는 이미 `PlayOnce`, `PlayLoop`, `Stop`을 지원한다.
- 하지만 V3 `BtnRun`, `StepForward`, `StepBack`, `Stop`은 아직 `PendantV3Points` 실행 queue와 연결되어 있지 않다.

## Planned First Slice

0. Simulate manual robot readback in Unity/Mock and prove `readback -> RobotStage -> values -> save`.
1. Load `PendantV3Points` as active teaching sequence.
2. Add selected/running index state.
3. Make `Run` execute saved point sequence when no pending preview exists.
4. Make `StepForward` / `StepBack` navigate saved points.
5. Make `Stop` stop sequence runner.
6. Add debug matrix `RunManualReadbackTeachingMatrixForDebug()`.
7. Add debug matrix `RunTeachingSequenceMatrixForDebug()`.

## Live Readiness Rule

- Do not start real-device motion tests until Unity/Mock proves the manual teaching readback loop.
- First real-device test must be readback-only: move robot by hand, verify RobotStage and values follow, save point, inspect saved data.

## Locked Decisions

- Point save source is current readback snapshot.
- Direct coordinate save is a future separate mode.
- `Run` executes pending preview first, then saved sequence.
- `StepForward` / `StepBack` are preview-only in v1.
- Order editing uses up/down buttons, not drag reorder.
- Point name is the v1 unique key.
- `MoveJ` uses saved joints first; numerical IK fallback remains live-blocked.
- Default point speed/dwell are `medium` and `0`.
- No new left-nav Program tab for v1.
- `NavPoints` expands internally into `포인트 / 시퀀스 / 함수`.
- UX target is easier than commercial teach pendants.
- Required ease-of-use features include readable point detail, current readback overwrite, run from selected, duplicate point, and simple function names.
- Point name is user input; empty name blocks save.
- Duplicate point name requires overwrite confirmation.
- Sequence order is user order, not alphabetical.
- Editing is locked while sequence is running.
- Failed step stops the sequence immediately.
- V1 point storage is Base-coordinate authoritative.

## Deferred

- Function/group blocks.
- IO/gripper sequence blocks.
- IF/ELSE/LOOP block editor.
- Boundary/collision hard gate.
