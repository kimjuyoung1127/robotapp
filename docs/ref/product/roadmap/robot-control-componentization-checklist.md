# Robot Control Componentization Checklist

Last updated: 2026-04-29

## Goals

- [ ] Reduce oversized runtime and UI classes into folder-scoped partial components.
- [ ] Keep Unity-facing entry classes stable while moving grouped behavior into partial files.
- [ ] Preserve serialized fields, public debug entry points, and existing scene/prefab bindings.
- [ ] Validate every phase with `dotnet build` plus targeted Unity EditMode tests.

## Guardrails

- [ ] Keep the root file name and main class name unchanged for each componentized target.
- [ ] Create a folder with the same class name and move the root file into that folder.
- [ ] Prefer partial classes before introducing brand-new runtime helper types.
- [ ] Avoid mixing two unrelated refactors into one partial file.
- [ ] Do not change runtime behavior while splitting files.
- [ ] Update `.meta` files and explicit `csproj` includes after every move.
- [ ] Run tests only after Unity is out of Play Mode.

## Priority

### High

- [ ] `Assets/Scripts/UI/RobotControlV3/PointMoveController.cs`
  - Reason: point CRUD, sequence flow, function flow, block sequence flow, modal state, path recording, and UI binding all live together.
  - Target folder: `Assets/Scripts/UI/RobotControlV3/PointMoveController/`
  - Recommended partials:
    - `PointMoveController.cs`
    - `PointMoveController.Elements.cs`
    - `PointMoveController.Initialization.cs`
    - `PointMoveController.ViewBinding.cs`
    - `PointMoveController.Points.cs`
    - `PointMoveController.Sequences.cs`
    - `PointMoveController.Functions.cs`
    - `PointMoveController.PathRecording.cs`

- [ ] `Assets/Scripts/App/RobotControlV3DebugBridge.cs`
  - Reason: shell, popup, live session, approval, teaching, gripper, stage camera, and point-move debug entry points are concentrated in one static surface.
  - Target folder: `Assets/Scripts/App/RobotControlV3DebugBridge/`
  - Recommended partials:
    - `RobotControlV3DebugBridge.cs`
    - `RobotControlV3DebugBridge.Shell.cs`
    - `RobotControlV3DebugBridge.Live.cs`
    - `RobotControlV3DebugBridge.Teaching.cs`
    - `RobotControlV3DebugBridge.Gripper.cs`
    - `RobotControlV3DebugBridge.PointMove.cs`
    - `RobotControlV3DebugBridge.StageCamera.cs`

- [ ] `Assets/Scripts/App/Fairino/RobotControlSceneCoordinator.cs`
  - Reason: scene bootstrap, runtime rig creation, visualization helpers, input listeners, and teaching flow are mixed in one MonoBehaviour.
  - Target folder: `Assets/Scripts/App/Fairino/RobotControlSceneCoordinator/`
  - Recommended partials:
    - `RobotControlSceneCoordinator.cs`
    - `RobotControlSceneCoordinator.Bootstrap.cs`
    - `RobotControlSceneCoordinator.RuntimeRig.cs`
    - `RobotControlSceneCoordinator.Visualization.cs`
    - `RobotControlSceneCoordinator.Teaching.cs`
    - `RobotControlSceneCoordinator.Listeners.cs`

### Medium

- [ ] `Assets/Scripts/App/Fairino/LiveFairinoClient.cs`
- [ ] `Assets/Scripts/UI/MathReadiness/MathReadinessPanel.cs`
- [ ] `Assets/Scripts/UI/RobotLibrary/RobotLibraryManager.cs`

### Low

- [ ] `Assets/Scripts/UI/RobotControl/FairinoJointControlPanel.cs`
- [ ] `Assets/Scripts/UI/RobotControlV3/ConnectionHomeController.cs`
- [ ] `Assets/Scripts/App/Fairino/Connection/PendantV3ConnectionSessionAdapter.cs`

## Phase Order

- [x] Phase 1: `PointMoveController` folderization and partial split
- [x] Phase 2: `RobotControlV3DebugBridge` folderization and partial split
- [ ] Phase 3: `RobotControlSceneCoordinator` folderization and partial split
- [ ] Phase 4: `LiveFairinoClient` adapter split
- [ ] Phase 5: secondary UI surfaces (`MathReadinessPanel`, `RobotLibraryManager`)

## Phase 1 Execution Checklist

- [x] Create `PointMoveController/` folder and move the root file plus `.meta`.
- [x] Keep `PointMoveController.Elements.cs` inside the same folder.
- [x] Split the root file into folder-scoped partials for public façade, runtime flow, and list/modal helpers.
- [x] Update `KineTutor3D.Runtime.csproj`.
- [x] Run `dotnet build`.
- [x] Run targeted RobotControlV3 EditMode coverage.
- Current partial set:
  - `PointMoveController.cs`
  - `PointMoveController.Bootstrap.cs`
  - `PointMoveController.ListsAndModals.cs`
  - `PointMoveController.Elements.cs`
- Current note:
  - first pass favored a safe coarse split over a many-file decomposition so existing serialized bindings and UITK authoring hooks stayed stable.

## Phase 2 Execution Checklist

- [x] Create `RobotControlV3DebugBridge/` folder and move the root file plus `.meta`.
- [x] Convert the root class to `partial`.
- [x] Keep shared object lookup and scene guard helpers in the root file.
- [x] Split the root file into folder-scoped partials for shell/core APIs and the remaining runtime-facing debug surface.
- [x] Update `KineTutor3D.Runtime.csproj`.
- [x] Run `dotnet build`.
- [x] Run targeted RobotControlV3 EditMode coverage.
- Current partial set:
  - `RobotControlV3DebugBridge.cs`
  - `RobotControlV3DebugBridge.Core.cs`
  - `RobotControlV3DebugBridge.LiveRuntime.cs`
- Current note:
  - this pass kept the existing public debug surface intact and moved only the file layout and partial ownership boundaries.

## Validation Checklist

- [x] `dotnet build /Users/family/jason/FR5UNITY/robotapp/Assembly-CSharp.csproj -nologo`
- [x] `unityctl status --project /Users/family/jason/FR5UNITY/robotapp --json`
- [x] `unityctl test --project /Users/family/jason/FR5UNITY/robotapp --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlV3GizmoBehaviorTests --json`
- [ ] `unityctl test --project /Users/family/jason/FR5UNITY/robotapp --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlV3HardcodingGuardTests --json`
- [ ] If live runtime files were touched, rerun the V3 restart loop and confirm the editor returns to `Ready`.

## Notes

- `RobotControlV3RuntimeController` was already moved into `Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/` before this checklist pass, so the current checklist starts from the next largest V3 surfaces instead of repeating the already-split runtime shell.
- Current validation truth for this checklist is:
  - `dotnet build`: pass
  - `RobotControlV3GizmoBehaviorTests`: pass
  - `RobotControlV3HardcodingGuardTests`: still pending a clean rerun after Unity IPC returns to `Ready`
- `RobotControlV3HardcodingGuardTests` was retried after `RobotControlV3GizmoBehaviorTests`, but the editor-side IPC bridge stayed in domain-reload recovery and did not return to `Ready` quickly enough for the same session retry.
- A follow-up batchmode test attempt completed process startup and recompilation cleanly, but did not emit the requested test result file, so it was not treated as a reliable pass result.
