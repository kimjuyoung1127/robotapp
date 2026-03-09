# Visualization Folder Guide

## Purpose
- apply FK results to Unity objects, frame gizmos, donor mesh visuals, and camera-facing visibility checks

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
3. `RobotRenderer.cs`
4. `FrameGizmo.cs`
5. `CoordConverter.cs`

## Refactor Rule
- keep `RobotRenderer` as a facade
- move donor resolution, mesh copy, visibility probing into helper classes before adding new behavior
