# App Folder Guide

## Purpose
- scene flow, app runtime state, template selection, step flow orchestration

## Allowed Here
- `AppController` public state/events
- scene routing and navigation helpers
- UI auto-wire that belongs to application bootstrap
- kinematics runtime coordination (not raw math implementation)

## Subfolders
- `Runtime/`: kinematics runtime state, update causes, FK facade helpers
- `Session/`: resume context, persisted progress, track/session storage
- `Lessons/`: lesson factory defaults and step-flow helper services
- `Fairino/Template/`: FR5 slim template extraction/demo assets and manifests

## Not Allowed Here
- donor mesh traversal/copy
- HUD widget styling/rendering details
- raw DH/FK math algorithm implementation

## Read First
1. root `AGENTS.md`
2. `docs/ref/architecture-mermaid.md`
3. `AppController.cs`
4. `BootSceneRouter.cs`
5. `SceneNavigator.cs`

## Refactor Rule
- keep `AppController` as the public facade
- keep scene entry helpers (`BootSceneRouter`, `SceneNavigator`, `SceneCatalog`, `SceneId`) at App root
- move step flow, UI binding, and kinematics runtime into helper/service classes when size or coupling grows
