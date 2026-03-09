# KineTutor3D AGENTS Index

This is the source-of-truth navigation document for work in this repository.
Use this file for folder responsibility, file-discovery order, and refactor rules.

## Start Here
1. `AGENTS.md`
2. `docs/ref/architecture-mermaid.md`
3. `CLAUDE.md`
4. `KineTutor3D_Execution_Plan.md`
5. `docs/status/PROJECT-STATUS.md`
6. `docs/status/PHASE-EXECUTION-BOARD.md`

## Mandatory Navigation Rule
- Always read the root `AGENTS.md` before exploring files.
- When working inside `Assets/Scripts/App`, `Assets/Scripts/UI`, or `Assets/Scripts/Visualization`, read that folder's `AGENTS.md` first.
- For fast system-level context, read `docs/ref/architecture-mermaid.md` before chasing individual files.

## Folder Responsibility Rule
- `Assets/Scripts/App`: application state, scene flow, orchestration, runtime coordination.
- `Assets/Scripts/UI`: HUD, onboarding, tutorial interaction, glossary, gates, navigation.
- `Assets/Scripts/Visualization`: Unity-side rendering, donor mesh binding, frame ownership, visibility checks.
- `Assets/Scripts/Math`, `Types`, `Kinematics`, `Templates`: domain logic and presets, not Unity HUD behavior.

## Authoring Rule
- Every `.cs` file in `Assets/Scripts/App`, `Assets/Scripts/UI`, and `Assets/Scripts/Visualization` must start with a short folder-role comment.
- If a file starts to mix folder responsibilities, split it into helper/service classes or move logic to the correct folder.
- When adding or refactoring files in those folders, update the nearest `AGENTS.md` if the folder contract changes.

## Current Structural Intent
- `AppController` is the public application facade.
- `RobotRenderer` is the public visualization facade.
- `DHTableEditor` stays view-oriented; parsing/building helpers live beside it.
- `frame_0`, `frame_1`, and `Frame_EE` remain the canonical frame ownership points.
- `ScaraRobot.prefab` remains the donor source, with `Pick` excluded from visual donor usage.
