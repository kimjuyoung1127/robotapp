# Pendant V3 Phase E Function Group Planning

## Date

- 2026-04-22 (KST)

## Scope

- Lock the function/group model before implementation.
- Keep the current teaching sequence stable and avoid turning the UI into a manufacturer program editor.

## Decisions

- `TeachingFunction` is a Unity teaching routine.
- It is not a manufacturer program, Lua/script editor, or upload surface.
- Function UI lives inside `NavPoints` as the `함수` subview.
- No new top-level `Program` tab is added.
- V1 function steps use point-name references.
- Stable point IDs stay deferred.
- First implementation supports `PointRef` only.
- `FunctionRef`, IO/gripper steps, variables, IF/ELSE/LOOP blocks, and manufacturer script execution are excluded from Function v1.

## Data Model

```text
TeachingFunction
- name
- description
- steps
- created
- updated
```

```text
TeachingFunctionStep
- kind: PointRef | FunctionRef
- refName
- enabled
- note
```

## First Implementation Slice

- Create function from selected points.
- Show function list.
- Show ordered point references.
- Rename function.
- Duplicate function.
- Delete function.
- Run function once in DryRun.

## Example Functions

- `HomeReturn`
- `Pick`
- `Place`
- `Inspect`

## Verification

- `git diff --check`: pass
- `unityctl check --type compile`: pass

## Self Review

- Keeps function/group inside `NavPoints`.
- Avoids a new Program tab.
- Does not reopen live motion gates.
- Preserves point/sequence v1 as the execution base.

## Next

- Function v1 scaffold:
  - function store
  - create/list/detail
  - RunOnce DryRun
