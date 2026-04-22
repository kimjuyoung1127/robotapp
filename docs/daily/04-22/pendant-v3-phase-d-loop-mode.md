# Pendant V3 Phase D Loop Mode

## Date

- 2026-04-22 (KST)

## Scope

- Implement `Phase D - Loop Mode`.
- Keep the visible loop control inside the Point/Teaching panel, not BottomBar.

## Implementation

- Added `반복 ON/OFF` toggle to the PointMove panel.
- Added loop status text in the Point/Teaching panel.
- Added runtime loop state:
  - `IsTeachingLoopEnabled`
  - `IsTeachingSequenceRunning`
  - `GetTeachingLoopSummaryForDebug()`
- `Run` behavior:
  - pending preview still executes first
  - saved sequence RunOnce remains the default when loop mode is OFF
  - saved sequence starts `WaypointCycleRunner.PlayLoop` when loop mode is ON
- `Stop` ends the loop runner.
- Runner frame updates now feed back into V3 RobotStage/snapshot updates during loop playback.
- Expanded matrices:
  - `RunTeachingSequenceMatrixForDebug()`
  - `RunActualUiClickMatrixForDebug()`

## Verification

- `unityctl check --type compile`: pass
- `RunTeachingSequenceMatrixForDebug()`: `24/24 PASS`
- `RunActualUiClickMatrixForDebug()`: `104/104 PASS`
- `GetAuxLayoutSummaryForDebug()` on PointMove:
  - `viewportHorizontalVisible=False`
  - `viewportClipped=0`
  - `contextHorizontalVisible=False`
  - `contextClipped=0`

## Self Review

- Loop execution state lives in App/Fairino runtime.
- PointMove owns only the visible toggle/status.
- BottomBar was not crowded with a new loop button.
- Pending-preview priority remains unchanged.
- No live motion gate was opened.
- Boundary/collision remains warning/future work.

## Next

- `Run From Selected`.
