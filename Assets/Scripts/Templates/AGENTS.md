# Templates Folder Guide

## Purpose
- pure robot template definitions, catalog registration, and template builder helpers

## Allowed Here
- per-robot DH/joint-limit template factories
- robot catalog and selection-order metadata wiring
- custom/default template builders

## Not Allowed Here
- `using UnityEngine`
- page/scene specific UI logic
- runtime orchestration, persistence, or rendering behavior

## Read First
1. root `AGENTS.md`
2. `docs/ref/csharp-master-harness.md`
3. `docs/ref/code-patterns.md`
4. `Template2DOF_RR.cs`
5. `RobotCatalog.cs`
6. `CustomTemplateBuilder.cs`

## Refactor Rule
- keep this folder organized by domain, not by page
- prefer flat structure while template count is still easy to scan
- if it grows, split by domain such as `Robots/`, `Catalog/`, `Builders/`, not by scene or feature page
