# Sandbox Stability And RobotControl Comparison

## Summary

- Compared `Sandbox` against `RobotControl` to understand why some robots fell under gravity or failed to visually respond during Play.
- Confirmed that the issue is not a platform limitation. `RobotControl` already contains a working stabilization pattern for URDF-based robots.
- Updated `Assets/Scripts/App/SandboxSceneCoordinator.cs` to reuse the same stabilization direction in Sandbox:
  - re-run stabilization when reusing an existing runtime robot
  - disable URDF controller scripts without destroying runtime state
  - force `ArticulationBody` gravity off, zero velocities, disable non-root bodies, and mark the root body immovable
  - teleport the root articulation body after setup
  - resolve the robot anchor recursively instead of assuming `base_link` is a direct child

## Technical Comparison

### RobotControl

- Validates and loads a control prefab through a robot-specific template definition.
- Repairs visible meshes before use.
- Stabilizes the robot by disabling non-root articulations, turning gravity off, zeroing velocities, fixing the root articulation body, and teleporting the root upright.
- Uses a robot-specific driver path that is stricter about joint ownership and control prefab expectations.

### Sandbox

- Uses the same template definitions but had a lighter stabilization path.
- Previously disabled only non-root `ArticulationBody` components and set `Rigidbody.isKinematic`, which left root articulation stabilization weaker than RobotControl.
- Previously assumed `base_link` was a direct child of the instantiated robot root, which is not robust for all imported prefabs.
- Falls back to a placeholder when no control prefab is defined.

## Validation

### Compile

- `unityctl check --project C:/Users/ezen601/Desktop/Jason/robotapp2 --type compile --json`
  - PASS

### Sandbox routing and pose reaction

Verified Robot Library -> Sandbox routing and then used `Demo` / `Home` preset buttons to see whether the end-effector info label changed.

Supported and reacting in Sandbox:

- `SCARA_RV`
- `FAIRINO_FR5`
- `UR5e`
- `DOOSAN_M1013`
- `MECA500`

Special case:

- `2DOF_RR`
  - enters Sandbox
  - does not react to `Demo` / `Home`
  - current cause is structural: `RobotControlFactory` defines an empty `ControlPrefabResourcePath`, so Sandbox uses a placeholder path instead of a real controllable visual

Non-Sandbox entries that remained in Robot Library as expected:

- `FAIRINO_FR5_TEMPLATE`
- `FANUC_CRX10`
- `IGUS_REBEL`

## Runtime Interpretation

- The Sandbox instability observed for URDF robots is technically fixable and is now aligned more closely with the proven RobotControl setup path.
- The remaining `2DOF_RR` gap is not a physics-stability problem. It needs a dedicated Sandbox visual/control path, likely procedural or based on existing simple-arm visualization assets.
- Some imported robots still emit mesh collider warnings during Play, but those warnings did not block preset-driven kinematic updates in the validated 6-DOF and SCARA cases.

## Next Steps

1. Add a dedicated Sandbox visual/control implementation for `2DOF_RR`.
2. If visual slipping is still observed in-editor for a specific robot, inspect that prefab's root articulation hierarchy and anchor naming rather than treating Sandbox as globally unstable.
3. Optionally add a PlayMode regression test that asserts Sandbox root stabilization for supported robot prefabs.
