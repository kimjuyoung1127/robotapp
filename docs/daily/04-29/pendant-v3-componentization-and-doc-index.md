# Pendant V3 Componentization And Doc Index

Date: 2026-04-29 (KST)

## Summary

- Added the first componentization checklist SSOT at `docs/ref/product/roadmap/robot-control-componentization-checklist.md`.
- Recorded the completed folderization/partial split passes for `RobotControlV3RuntimeController`, `PointMoveController`, and `RobotControlV3DebugBridge`.
- Added the top-level docs index plus folder `README.md` hubs so an agent can orient on `docs/status`, `docs/ref`, `docs/daily`, `docs/archive`, `docs/benchmark`, `docs/templates`, and `docs/weekly` with less path hunting.
- Normalized frontmatter on the main docs hubs that an agent is expected to read first.
- Reconfirmed that the safe verification truth for the componentization pass is still `dotnet build` plus targeted EditMode coverage, not "all Unity checks green".

## What Changed

- `RobotControlV3RuntimeController` now lives under `Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/` with partials for stage camera, view state, helpers, and motion preview/apply flow.
- `PointMoveController` now lives under `Assets/Scripts/UI/RobotControlV3/PointMoveController/`.
  - current first-pass partials:
    - `PointMoveController.cs`
    - `PointMoveController.Bootstrap.cs`
    - `PointMoveController.ListsAndModals.cs`
    - `PointMoveController.Elements.cs`
- `RobotControlV3DebugBridge` now lives under `Assets/Scripts/App/RobotControlV3DebugBridge/`.
  - current first-pass partials:
    - `RobotControlV3DebugBridge.cs`
    - `RobotControlV3DebugBridge.Core.cs`
    - `RobotControlV3DebugBridge.LiveRuntime.cs`
- Docs navigation now starts from:
  - `docs/INDEX.md`
  - `docs/README.md`
  - per-folder `README.md` files

## Validation

- `dotnet build /Users/family/jason/FR5UNITY/robotapp/Assembly-CSharp.csproj -nologo`: pass
- `unityctl test --project /Users/family/jason/FR5UNITY/robotapp --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlV3GizmoBehaviorTests --json`: pass
- `unityctl test --project /Users/family/jason/FR5UNITY/robotapp --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlV3HardcodingGuardTests --json`: pending reliable rerun

## Why The Last Test Is Still Pending

- After the gizmo test pass, the editor-side IPC bridge stayed in domain-reload recovery and did not return to `Ready` quickly enough for the same-session retry.
- A fallback batchmode attempt completed startup and recompilation, but it did not emit the requested result XML, so it was not counted as a trustworthy pass.

## Follow-Up

- Keep `robot-control-componentization-checklist.md` as the canonical home for future `SceneCoordinator` / `LiveFairinoClient` split tracking.
- When Unity IPC is healthy again, rerun `RobotControlV3HardcodingGuardTests` and update the checklist instead of duplicating the result across multiple docs.
- Keep `ACTIVE-WORK-INDEX.md` unchanged for now because the repo-wide top priority is still FR5 live/readback and not code-organization work.
