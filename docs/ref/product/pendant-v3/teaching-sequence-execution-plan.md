# Pendant V3 Teaching Sequence Execution Plan

## Purpose

- `PendantV3Points`를 단순 저장 목록에서 실행 가능한 티칭 시퀀스로 승격한다.
- 현장 워크플로우인 `수동 이동 -> 포인트 저장 -> 순서 조정 -> 단일/전체/루프 실행`을 V3의 핵심 티칭 흐름으로 고정한다.
- 제조사 Lua/program 파일 실행은 계속 제외하고, Unity 내부 teaching sequence만 다룬다.

## Last Updated

- 2026-04-22 (KST)

## Parent Docs

- [README.md](./README.md)
- [feature-points-teaching.md](./feature-points-teaching.md)
- [feature-program.md](./feature-program.md)
- [robot-button-integration-plan.md](./robot-button-integration-plan.md)
- [progress-checklist.md](./progress-checklist.md)

---

## Current State

### Already Wired

- `PointMoveController` saves points into `WaypointStore` sequence `PendantV3Points`.
- Each point stores:
  - `name`
  - `jointsDeg`
  - `tcpMm`
  - `moveType`
  - `speedPreset`
  - `dwellSec`
- Point v1 supports:
  - save
  - recall
  - list/select
  - delete
  - rename
  - export
  - cleanup
  - single point preview/apply
- Saved joint target is preferred for recalled `MoveJ`.
- `WaypointCycleRunner` already supports `PlayOnce`, `PlayLoop`, and `Stop`.

### Not Yet Wired

- V3 `BtnRun` does not execute saved point sequence when no pending preview exists.
- V3 `BtnStepForward` / `BtnStepBack` are currently preview undo/redo, not teaching sequence step controls.
- `PendantV3Points` is not loaded into a V3 runtime execution queue.
- Stored point order cannot be changed from V3.
- There is no current sequence index, selected step, loop mode, or run status exposed in V3.
- Point groups/functions are not modeled.

---

## Product Principle

Manual teaching must stay free.

- Manual jog / hand-guided teaching should warn, not block.
- Saving a point should be allowed even if later execution needs review.
- Automatic run / loop execution should be more conservative than manual teaching.
- Boundary/collision work is not the first priority for this slice.
- Before connecting a real robot, the full teaching loop must be proven in Unity/Mock.
- Live robot integration should only start after the readback -> RobotStage -> value display -> point save -> sequence run loop is stable in simulation.
- The teaching flow should be easier than a traditional industrial teach pendant.
- Commercial pendant concepts are allowed only when they reduce teaching friction, not when they add menu depth or jargon.

Primary goal:

> 저장한 포인트를 믿고 순서대로 실행하고, 필요하면 한 줄씩 확인하며 반복 실행할 수 있게 한다.

Secondary live-readiness goal:

> 실제 기기를 손으로 움직였을 때 들어오는 readback 값이 Unity 메인 로봇, 좌표값, 저장 포인트에 같은 값으로 반영될 수 있는 기반을 먼저 만든다.

---

## Easier Than Commercial Pendant UX

The target user should not need to understand industrial pendant terminology before saving and replaying points.

### UX Goals

- Make the first successful teaching loop obvious:
  1. move robot manually
  2. save current point
  3. repeat
  4. preview sequence
  5. run sequence in DryRun
- Keep every dangerous action behind plain-language confirmation.
- Show what will happen before execution.
- Prefer visible point cards over hidden registers.
- Prefer `현재 위치 저장`, `선택 실행`, `전체 미리보기`, `전체 실행` over abstract program terms.
- Keep manufacturer Lua/program concepts out of the main workflow.

### Commercial Feature Comparison

| Commercial pendant concept | V3 easier equivalent | First-slice policy |
|---|---|---|
| Position register / waypoint table | Point cards in `NavPoints` | Include |
| Touch-up / overwrite position | `현재 위치로 덮어쓰기` | Include after save/readback gate |
| Program line list | Sequence subview | Include |
| Step mode | `Step▶` preview-only first | Include |
| Run from cursor | `선택 지점부터 실행` | Phase 2 |
| Loop | `반복 실행` toggle | Phase 2 |
| Subprogram | `함수` subview | Phase 3 |
| IO action block | `그리퍼/IO 스텝` | Phase 3 |
| Registers / variables | Hidden advanced concept | Exclude from v1 |
| Manufacturer script/Lua | Advanced external program | Exclude |

### Beginner-First Rules

- Every saved point shows both name and plain action summary.
  - Example: `PICK_1 · MoveJ · 중간 속도`
- Every sequence line shows:
  - step number
  - point name
  - move type
  - speed/dwell summary
  - current selected/running state
- Point detail must show saved `Joint`, `TCP`, `MoveType`, `Speed`, `Dwell`.
- Editing should be direct:
  - rename
  - duplicate
  - delete
  - overwrite with current readback
  - move up/down
- Dangerous operations must describe the consequence:
  - delete: removes point from sequence
  - overwrite: replaces saved pose with current readback
  - run: moves through saved points

### Language Policy

- Use Korean action names in UI.
- Avoid exposing "register", "program node", "script", "cursor" in the first slice.
- Use:
  - `포인트`
  - `순서`
  - `현재 위치로 저장`
  - `현재 위치로 덮어쓰기`
  - `선택 미리보기`
  - `선택 실행`
  - `전체 미리보기`
  - `전체 실행`
  - `반복`
- Keep internal code names technical, but UI copy must stay task-oriented.

---

## Locked Decisions

These decisions are no longer open questions for the first implementation slice.

### Save Source

- Point save uses the current readback snapshot as the source of truth.
- Preview values must not silently replace readback values during normal save.
- Direct coordinate entry save is a separate future mode/button.
- A saved point must record the same joint/TCP values visible in the readback-driven UI.

### Run Priority

`Run` resolves in this order:

1. If a pending preview exists, execute that single pending preview.
2. Else if `PendantV3Points` has saved points, run the ordered sequence once.
3. Else show "실행할 저장 포인트 없음" style feedback.

### Step Semantics

- `Step▶` selects and previews the next saved point only.
- `Step◀` selects and previews the previous saved point only.
- Step buttons must not execute motion in the first slice.
- Preview undo/redo remains on `Undo` / `Redo`, not on `Step▶` / `Step◀`.

### Loop Placement

- Loop mode is not part of the first visible UI slice.
- First implementation may expose loop through debug/internal state only.
- Visible loop toggle is added after RunOnce is stable.
- When visible, loop mode should live in the Point/Teaching panel first, not crowded into BottomBar.

### Point Ordering

- V1 order editing uses `위로` / `아래로` buttons.
- Drag reorder is deferred.
- Reordered sequence must persist back to `WaypointStore`.

### Point Detail / Edit

- Point detail is required before the feature can be called teaching-ready.
- Detail must show saved joints, TCP, move type, speed preset, dwell, and last feedback.
- `현재 위치로 덮어쓰기` is required because it is the simplest version of commercial touch-up.
- Overwrite updates only `jointsDeg` and `tcpMm` from current readback.
- Overwrite keeps `name`, `moveType`, `speedPreset`, and `dwellSec`.
- Duplicate is a near-term follow-up, not a first-slice blocker.

### Run From Selected

- `Run All` is first.
- `Run From Selected` is second.
- This mirrors commercial run-from-line behavior without exposing "cursor" terminology.

### Point Identity

- V1 uses unique point names as identifiers.
- Duplicate names are not allowed.
- Rename must preserve order and saved pose data.
- A future v2 may add stable `id`, but this first slice must not depend on it.

### Point Naming

- Point name is user input.
- V1 does not auto-generate point names.
- Empty point name blocks save with clear feedback.
- Duplicate name requires overwrite confirmation before replacing an existing point.
- Duplicate overwrite preserves the existing sequence position.

### Move Type Policy

- Saved `moveType` is part of the point contract.
- Manual teaching save default is `MoveJ`.
- `MoveJ` uses saved `jointsDeg` first.
- `MoveL` uses saved `tcpMm` first.
- Numerical XYZ IK fallback remains preview/DryRun only and is live-blocked.

### Speed / Dwell Defaults

- First slice default is `speedPreset=medium`.
- First slice default is `dwellSec=0`.
- Per-point speed/dwell editing is deferred until sequence execution is stable.
- Speed/dwell must still be visible in point detail even before editing is added.

### Execution Locking

- Sequence editing is locked while sequence is running.
- Running sequence blocks delete, reorder, rename, overwrite, and duplicate.
- Stop returns the sequence to editable state.
- Any failed step stops the sequence immediately.
- Failed step remains selected and highlighted.

### Run From Selected

- `선택 실행` executes one selected point.
- `선택 지점부터 실행` executes selected point through the end of the sequence.
- If a step fails, restart should default to `선택 지점부터 실행` from the failed point.

### Coordinate Storage

- V1 point storage is Base-coordinate authoritative.
- `jointsDeg` and `tcpMm` are both saved from current readback.
- Tool/User frames may be displayed, but v1 saved execution uses Base readback values.
- Direct coordinate input mode is future work and must be visually distinct from readback save.

### Sequence Ordering

- Sequence order is user order.
- Do not sort alphabetically by point name.
- New point is appended to the end unless it overwrites an existing point.
- Reorder uses up/down controls and persists immediately.

### Dirty Input Policy

- Readback save ignores partially edited coordinate input unless user is in an explicit direct-coordinate mode.
- If direct-coordinate mode is added later, dirty input must be visibly marked.
- Current v1 save path is readback-only.

### Live Readiness

- Real-device motion testing is blocked until both simulation matrices pass:
  - `RunManualReadbackTeachingMatrixForDebug()`
  - `RunTeachingSequenceMatrixForDebug()`
- First real-device test is readback-only.
- Real-device MoveJ/MoveL remains behind product confirm and live gate.

---

## Scope

### In Scope For This Plan

- Simulate the manual teaching readback loop in Unity before real hardware.
- Load `PendantV3Points` as the active teaching sequence.
- Maintain an ordered execution list.
- Select a current step.
- Preview selected point.
- Execute selected point once.
- Execute all points once.
- Execute all points in loop mode.
- Stop running sequence.
- Move selected point up/down.
- Surface current execution state in V3 UI/debug summaries.
- Keep live execution behind existing product confirm and live safety gate.

### Out Of Scope For This Plan

- Manufacturer Lua/program load/run.
- Full block editor.
- IF/ELSE/LOOP logic blocks.
- IO/gripper sequence blocks.
- Function call blocks.
- Mesh/capsule collision engine.
- Advanced production IK solver.

These remain follow-up phases after the basic teaching sequence is reliable.

---

## Data Model

### Existing Data

`WaypointSequence`

```text
name: string
created: string
waypoints: Waypoint[]
```

`Waypoint`

```text
name: string
jointsDeg: double[6]
tcpMm: double[6]
moveType: "MoveJ" | "MoveL"
speedPreset: "slow" | "medium" | "fast"
dwellSec: double
```

### V3 Runtime State To Add

`TeachingSequenceRuntimeState`

```text
sequenceName: "PendantV3Points"
pointCount: int
selectedIndex: int
runningIndex: int
isRunning: bool
isLooping: bool
runMode: "Idle" | "Preview" | "Step" | "RunOnce" | "Loop"
lastSequenceFeedback: string
```

Initial implementation can keep this state inside `RobotControlV3RuntimeController` or a small `TeachingSequenceRuntime` helper. If the code grows, split it into `Assets/Scripts/App/Fairino/Teaching/`.

---

## UX Contract

### Point Panel

Existing point panel remains the authoring surface.

Main left navigation does not get a new Program tab for this slice.

- `NavPoints` is promoted from "point storage" to "teaching points + executable sequence".
- `NavPoints` owns point save/recall, ordered sequence, step execution, and later function grouping.
- A separate left-nav `Program` tab remains excluded to avoid confusing Unity teaching sequences with manufacturer Lua/program files.
- If the feature grows, add internal subviews inside `NavPoints`, not a new top-level nav item.

Planned internal subviews:

- `포인트`: saved point list, details, save/recall/edit/delete.
- `시퀀스`: ordered execution list, run/step/stop, reorder.
- `함수`: grouped teaching routines, added later after sequence v1 is stable.

Required additions:

- Show ordered point rows.
- Show active selected point.
- Show selected point detail.
- Buttons:
  - `위로`
  - `아래로`
  - `현재 위치로 덮어쓰기`
  - `복사`
  - `선택 실행`
  - `선택 미리보기`
- Row click selects and recalls point.
- Point names must be unique in v1.

### Bottom Bar

Bottom bar becomes execution control for the active teaching sequence.

- `Run`
  - If pending preview exists: execute pending preview.
  - Else if `PendantV3Points` has points: execute sequence once.
  - Else: show no sequence feedback.
- `Run From Selected`
  - Not a separate BottomBar button in v1.
  - Add as Point/Sequence subview action after Run All is stable.
- `Stop`
  - Stops current sequence runner or current motion.
- `Step▶`
  - Selects and previews next point only in the first slice.
- `Step◀`
  - Selects/previews previous point.
- `DryRun`
  - DryRun ON means sequence animates in Unity without live motion.
  - DryRun OFF still requires product confirm and live safety gate.

### Status / Feedback

Show a concise status line:

```text
Teaching: 2/5 P_PICK · MoveJ · DryRun
```

or:

```text
Teaching: Loop ON · 1/4 HOME 이동 중
```

Point detail example:

```text
PICK_1
MoveJ · medium · dwell 0.0s
J: 0.0 / -45.0 / 0.0 / -59.0 / -92.0 / -42.0
TCP: X 542.2 / Y 135.2 / Z 433.3 / RX 180.0 / RY 0.0 / RZ 90.0
```

---

## Execution Semantics

### Single Point

1. Select point.
2. Preview point.
3. Apply point.
4. Use `MoveJ` with saved `jointsDeg` when available.
5. Use `MoveL` with saved `tcpMm` for linear move points.

### Step

1. `Step▶` moves selection from current index to next index.
2. First slice: preview only.
3. Later slice: optional `Step Execute` mode can apply immediately after confirm, but this is not part of v1.

### Run Once

1. Load `PendantV3Points`.
2. Validate sequence has at least one waypoint.
3. Run points from first to last.
4. DryRun uses `WaypointCycleRunner.PlayOnce(sequence, dryRun: true)`.
5. Live uses existing confirm/gate before each motion or before sequence start.

### Loop

1. Load `PendantV3Points`.
2. Run points repeatedly until Stop.
3. First slice can expose loop through debug/API only if UI is not ready.
4. UI loop toggle comes after run-once is stable.

---

## Safety Policy

### Manual Teaching

- No hard blocking except invalid input and impossible command state.
- Save point allowed.
- Warning text can be added later.

### Sequence Run

- DryRun first.
- Live requires:
  - operator confirm token
  - existing live safety gate
  - saved joint target for `MoveJ`
- Numerical XYZ IK fallback remains live-blocked.
- Boundary/collision stays future/warning-only until this sequence flow is stable.

---

## Live Manual Teaching Readback Loop

This is a required precondition before real-device teaching.

The target real workflow is:

```text
사용자가 실기기를 수동/drag/manual mode로 이동
-> app reads robot state
-> RobotStage updates to the same pose
-> joint/TCP values update visibly
-> user saves current point
-> saved point stores the same readback joints/TCP
-> saved points can be replayed in Unity/Mock first
-> only then attempt live execution
```

### Current Code Path

The existing architecture already has the intended path:

```text
IFairinoRobotClient.ReadState()
-> FairinoConnectionService.Tick()
-> FairinoConnectionService.OnStateUpdated
-> RobotControlV3RuntimeController.HandleStateUpdated()
-> ApplyVisualState()
-> RefreshSnapshot()
-> RobotStage / CoordStrip / StatusCard / PointMoveController
```

This path must be validated in simulation before live hardware.

### Simulation Before Live

Because the real robot is not connected yet, the first implementation must use Unity/Mock probes.

Simulation requirements:

- Provide a debug/mock method that changes the mock readback joint pose as if the physical robot was moved by hand.
- Ensure `FairinoConnectionService.Tick()` or explicit sync emits `OnStateUpdated`.
- Ensure `RobotStage` changes pose from readback, not only from UI preview commands.
- Ensure `StatusCard` / `CoordStrip` show changed joint/TCP values.
- Ensure `PointMoveController` can save that readback state into `PendantV3Points`.
- Ensure saved `jointsDeg` and `tcpMm` match the readback snapshot used on screen.

### Live Hardware Gate

Do not attempt real-device teaching until the Unity/Mock path passes:

- readback changes RobotStage pose
- readback changes displayed joint/TCP values
- saving current point stores readback values
- recalled saved point previews the same pose
- sequence run in DryRun can replay saved points

Real-device connection should start as readback-only:

1. Connect to robot.
2. Do not send MoveJ/MoveL.
3. Move robot manually.
4. Confirm app readback changes.
5. Confirm RobotStage follows.
6. Confirm save stores current readback.
7. Only after that, test low-speed DryRun/live-gated execution.

---

## Implementation Phases

### Phase 0 - Simulated Manual Readback Gate

Goal: prove the real teaching input path in Unity/Mock before hardware.

Tasks:

- Add a debug/mock entry point to simulate external manual robot movement.
- Route simulated readback through `FairinoConnectionService.OnStateUpdated`, not directly through UI fields.
- Verify RobotStage pose follows readback.
- Verify displayed joint/TCP values follow readback.
- Verify point save stores the current readback pose.
- Add debug matrix:
  - `RunManualReadbackTeachingMatrixForDebug()`

Done when:

- Matrix proves readback -> RobotStage -> values -> save -> recall works without live hardware.

Status:

- Done on 2026-04-22.
- `RunManualReadbackTeachingMatrixForDebug()`: `6/6 PASS`.
- Artifact: `Artifacts/robotcontrolv3-manual-readback-teaching.json`.
- Self-review: readback simulation lives in `App/Fairino/Teaching`, uses `FairinoConnectionService.OnStateUpdated`, and does not write UI fields directly.

### Phase A - Sequence Runtime Adapter

Goal: make `PendantV3Points` loadable as a V3 execution queue.

Tasks:

- Add helper to load `WaypointStore.Load("PendantV3Points")`.
- Add selected/running index state.
- Add debug summary:
  - point count
  - selected point
  - run state
  - loop state
- Add methods:
  - `LoadTeachingSequence()`
  - `SelectTeachingPoint(int index)`
  - `PreviewSelectedTeachingPoint()`
  - `ExecuteSelectedTeachingPoint()`
- Add selected point detail summary.

Done when:

- Debug call can create two points, load sequence, select point 0/1, preview selected point.
- Point names are unique and duplicate save attempts replace or reject explicitly.

### Phase B - Bottom Bar Run/Step Binding

Goal: make existing `Run`, `Step▶`, `Step◀`, `Stop` meaningful for saved points.

Tasks:

- `Run` fallback:
  - pending preview first
  - else run `PendantV3Points` once
- `Step▶` selects/previews next point.
- `Step◀` selects/previews previous point.
- `Stop` stops `WaypointCycleRunner`.
- Update feedback text.
- On first failed step, stop and keep that step selected.

Done when:

- `Run` executes all saved points in DryRun.
- `Step▶` previews next saved point.
- `Step▶` does not execute motion in this slice.
- `Stop` returns runner to idle.

### Phase C - Order Editing

Goal: make point order editable.

Tasks:

- Add move up/down helpers in `PointMoveController` or teaching helper.
- Add overwrite selected point with current readback.
- Persist reordered `WaypointSequence`.
- Update point list display.
- Add debug matrix for reorder.

Done when:

- P2 can move above P1 and persisted order is reflected after reload.
- Selected point can be overwritten with current readback and persists after reload.

### Phase C2 - Easy Editing

Goal: reduce friction compared with commercial pendants.

Tasks:

- Add duplicate point.
- Add speed/dwell detail display.
- Add speed/dwell editing after run/step is stable.
- Add delete/overwrite confirmation copy that explains the consequence.
- Add duplicate-name overwrite confirm flow.

Done when:

- User can copy a point, touch up with current readback, and rerun sequence without leaving `NavPoints`.

### Phase D - Loop Mode

Goal: repeat saved points safely.

Tasks:

- Add loop toggle state.
- `Run` uses `PlayLoop` when loop mode is ON.
- `Stop` ends loop.
- Status shows loop mode.

Done when:

- DryRun loop starts and can be stopped.
- Visible loop UI can wait until this is green.

### Phase E - Function / Group Planning

Goal: prepare for commercial-style grouped teaching routines.

Tasks:

- Define `TeachingFunction` concept.
- A function references ordered waypoint names or embeds a sequence.
- Functions can be duplicated, renamed, deleted, and called later.
- Function feature stays inside `NavPoints`.
- First useful functions should be simple named groups like `Pick`, `Place`, `HomeReturn`.

Done when:

- Documented and scoped; no need to implement in this first slice.
- Function/group UI should be an internal `NavPoints` subview, not a new left-nav item.

---

## Verification Plan

### Static / Compile

```powershell
unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json
```

### Runtime Debug Matrix

Add a new debug matrix:

```text
RunManualReadbackTeachingMatrixForDebug()
RunTeachingSequenceMatrixForDebug()
```

Cases:

- simulated manual readback changes snapshot joints
- simulated manual readback changes RobotStage pose summary
- simulated manual readback updates CoordStrip/Status values
- save current point after readback stores matching `jointsDeg`
- recall saved point restores same saved pose
- empty sequence blocks run
- save P1/P2
- load sequence count=2
- select index 0
- step forward selects index 1
- step back selects index 0
- run once in DryRun completes
- stop returns idle
- reorder P2 up persists
- overwrite selected point stores current readback
- selected point detail includes joints/TCP/moveType/speed/dwell

### Existing Regression

Keep these green:

- `RunActualUiClickMatrixForDebug()` -> `95/95 PASS`
- `RunPopupConfirmCancelE2EForDebug()` -> `10/10 PASS`
- `RunProductLiveConfirmTokenMatrixForDebug()` -> `4/4 PASS`

---

## Acceptance Criteria

- Unity/Mock can simulate physical manual movement through the readback path.
- RobotStage follows simulated readback.
- Displayed joint/TCP values follow simulated readback.
- Point save stores current readback, not stale preview.
- Point names are unique.
- A user can save at least two points.
- The stored points remain in a visible ordered list.
- A selected point has readable detail.
- A selected point can be overwritten with current readback.
- A selected point can be previewed and applied.
- `Run` can execute the ordered list in DryRun.
- `Step▶` and `Step◀` navigate saved points, not just undo/redo previews.
- `Step▶` and `Step◀` do not execute motion in the first slice.
- `Stop` stops sequence playback.
- Live execution remains gated and cannot bypass product confirm.
- Boundary/collision is not required for this slice.

---

## Deferred Design Questions

- Function/group model: define `TeachingFunction` later after `WaypointSequence` run/step is stable.
- IO/gripper blocks: defer until point sequence run is stable.
- Stable point IDs: consider after v1 name-based workflow proves useful.
