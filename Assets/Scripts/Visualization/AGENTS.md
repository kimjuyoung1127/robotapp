# Visualization Folder Guide

## Purpose
- apply FK results to Unity objects, frame gizmos, donor mesh visuals, and camera-facing visibility checks

## Role Folders
- `Renderer/`: `RobotRenderer` facade, donor binding, rig/bounds helpers
- `RobotLibrary/`: showroom preview pod/factory helpers
- `RobotControl/`: RobotControl-specific visualization drivers
- `MathReadiness/`: angle/length/grid/simple-arm teaching visuals
- `Targets/`: target markers, highlight helpers
- `Shared/`: cross-scene primitives such as gizmos, trails, handles, URDF driver core

## Allowed Here
- frame ownership and binding
- donor path mapping
- mesh/material copy helpers
- aggregate bounds / frustum checks

## Not Allowed Here
- tutorial gate state
- glossary/tooltip/HUD logic
- DH/FK algorithm implementation itself

## Read First
1. root `AGENTS.md`
2. `docs/ref/architecture-mermaid.md`
3. `Renderer/RobotRenderer.cs`
4. `Shared/FrameGizmo.cs`
5. `Shared/CoordConverter.cs`

## Refactor Rule
- keep `RobotRenderer` as a facade
- organize by visualization role first, and only keep cross-scene primitives in `Shared/`
- move donor resolution, mesh copy, visibility probing into helper classes before adding new behavior
