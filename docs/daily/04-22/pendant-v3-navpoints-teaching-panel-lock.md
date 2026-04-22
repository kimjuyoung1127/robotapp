# Pendant V3 NavPoints Teaching Panel Lock

## Context

- The latest SSOT says saved points, sequence execution, and function grouping belong under `NavPoints`.
- The current implementation still surfaced the PointMove teaching controls primarily through `NavMotion > TabPointMove`, which made the user flow feel like a motion tab feature instead of a teaching point feature.

## Decision

- `NavPoints` now opens the PointMove teaching panel in the auxiliary panel.
- `NavMotion > TabPointMove` remains as a compatibility/direct coordinate move route.
- When `NavPoints` is active, shell copy should say `티칭 포인트` and WorkTabBar should not be the teaching navigation surface.

## Implementation

- `PointMoveController` desktop visibility now accepts `NavPoints`.
- `ConnectionHomeController` hides WorkTabBar while `NavPoints` is active.
- Shell title, PointMove panel title, help copy, and viewport auxiliary copy now describe `NavPoints` as teaching points.
- `RunTeachingSequenceMatrixForDebug()` includes `navpoints-opens-teaching-point-panel`.
- `RobotControlV3RuntimeController.ApplyVisualState()` now ignores stale TCP previews shorter than XYZ before moving the target marker.

## Validation

- `unityctl check --type compile`: pass
- `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
- `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
- `GetAuxLayoutSummaryForDebug()` on NavPoints: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextHorizontalVisible=False`, `contextClipped=0`
- Screenshot: `Artifacts/pendant-v3-navpoints-teaching-panel.png`

## Self Review

- UI responsibility stays in `Assets/Scripts/UI/RobotControlV3`.
- Runtime teaching state remains in `Assets/Scripts/App/Fairino`.
- No live motion gate was opened.
- No new top-level Program tab was introduced.
