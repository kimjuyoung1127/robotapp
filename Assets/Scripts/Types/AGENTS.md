# Types Folder Guide

## Purpose
- pure domain value types for robotics state, metadata, limits, and template contracts

## Allowed Here
- immutable structs and sealed domain classes
- robot metadata and library interaction enums
- pose, DH, joint-limit, template contracts

## Not Allowed Here
- `using UnityEngine`
- scene/page specific UI concepts
- runtime orchestration, persistence, or rendering behavior

## Read First
1. root `AGENTS.md`
2. `docs/ref/csharp-master-harness.md`
3. `docs/ref/code-patterns.md`
4. `DHLink.cs`
5. `RobotTemplate.cs`
6. `RobotMetadataInfo.cs`

## Refactor Rule
- keep this folder organized by domain, not by page
- prefer flat structure while file count stays small and the types remain broadly shared
- if it grows, split by domain contracts such as `Kinematics/` and `RobotCatalog/`, not by scene or feature page
