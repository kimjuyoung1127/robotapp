# App Folder Guide

## Purpose
- scene flow, app runtime state, template selection, step flow orchestration

## Allowed Here
- `AppController` public state/events
- scene routing and navigation helpers
- UI auto-wire that belongs to application bootstrap
- kinematics runtime coordination (not raw math implementation)

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
- move step flow, UI binding, and kinematics runtime into helper/service classes when size or coupling grows
