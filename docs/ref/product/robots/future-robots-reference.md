# Future Robots Reference

> DH parameters, URDF sources, SDK protocols for robots not yet implemented.
> Use this document to start implementation immediately when prerequisites are met.

## Status
- Research / Documentation only — no code, no catalog entries
- Last Updated: 2026-03-23 (KST)

## Parent Docs
- [urdf-reference-collection.md](./urdf-reference-collection.md)
- [robot-template-expansion.md](./robot-template-expansion.md)
- [robot-model-library-spec.md](./robot-model-library-spec.md)

## When To Read
- Before starting implementation of xArm6, Kinova Gen3, or Franka Panda
- When evaluating scope for variable-DOF template support
- When sourcing STL-to-DAE conversion tooling

---

## Implementation Prerequisites

| Robot | Prerequisite | Status |
|-------|-------------|--------|
| xArm6 | STL to DAE mesh conversion for all 6 link visuals | Not started |
| xArm6 | Modified DH to Standard DH conversion layer in ForwardKinematics, or MDH support added to DHRow | Not started |
| Kinova Gen3 | STL to DAE mesh conversion for all 6 link visuals | Not started |
| Kinova Gen3 | gRPC client implementation (`IKinovaClient`) or gRPC-to-TCP bridge | Not started |
| Franka Panda | Variable DOF support in `RobotTemplate` and `ForwardKinematics` (currently fixed per template) | Not started |
| Franka Panda | 7-joint UI rail (`JointInputRail` currently sized per-robot, needs 7-slot variant) | Not started |
| Franka Panda | libfranka RT UDP protocol wrapper — highest complexity, requires real-time loop | Not started |

---

## xArm6

### Overview
- Manufacturer: UFACTORY
- DOF: 6
- Type: Articulated (6R), collaborative
- DH Convention: Modified DH (MDH) with per-joint theta offset — NOT Standard DH
- Payload: 5 kg
- Reach: 700 mm
- License: BSD-3-Clause (ROS2 package)

### DH Parameters (Modified DH, theta offset included)

> Note: KineTutor3D currently uses Standard DH exclusively. Implementing xArm6 requires
> either (a) a Modified DH computation path in `ForwardKinematics`, or (b) pre-converting
> these parameters to an equivalent Standard DH form and validating against official EE poses.

| Joint | a [mm] | d [mm] | alpha [rad] | theta offset [rad] |
|-------|--------|--------|-------------|-------------------|
| 1 | 0 | 267 | 0 | 0 |
| 2 | 0 | 0 | -pi/2 | -1.3849 |
| 3 | 289.49 | 0 | 0 | +1.3849 |
| 4 | 77.5 | 342.5 | -pi | 0 |
| 5 | 0 | 0 | pi/2 | 0 |
| 6 | 76 | 97 | -pi/2 | 0 |

Source: `xarm_ros2/xarm_description/urdf/xarm6/xarm6.urdf.xacro`, official UFACTORY DH sheet

### Joint Limits

| Joint | Min [deg] | Max [deg] | Max Velocity [deg/s] |
|-------|-----------|-----------|---------------------|
| 1 | -360 | +360 | 180 |
| 2 | -118 | +120 | 180 |
| 3 | -225 | +11 | 180 |
| 4 | -360 | +360 | 180 |
| 5 | -97 | +180 | 180 |
| 6 | -360 | +360 | 180 |

Source: UFACTORY xArm User Manual, URDF joint limit fields

### URDF / Mesh Source

- **ROS2 Package:** `xarm_ros2`
- **GitHub:** https://github.com/xArm-Developer/xarm_ros2
- **Mesh Path:** `xarm_description/meshes/xarm6/visual/`
- **Mesh Format:** STL (requires STL to DAE conversion before Unity import)
- **License:** BSD-3-Clause
- **URDF Entry Point:** `xarm_description/urdf/xarm6/xarm6.urdf.xacro`

#### Mesh Conversion Notes
- 6 visual STL files: `link1.STL` through `link6.STL` plus base
- Recommended tool: Blender (File > Import STL > Export Collada DAE) or `meshlabserver -c`
- After conversion: place DAE files under `Assets/Runtime/Resources/Robots/xArm6/`
- Scale: URDF uses meters; verify Unity import scale matches (expected 1.0 if DAE export preserves units)

### SDK Protocol

| Property | Value |
|----------|-------|
| Protocol | TCP socket + Modbus TCP |
| Primary Port | 502 (Modbus) |
| Control Port | 18333 (xArm TCP API) |
| State Port | 18334 |
| Report Port | 30001 / 30002 |
| Complexity | Medium-High |

#### Key Commands (xArm TCP API)
- `set_servo_angle` — MoveJ by joint angles (degrees)
- `set_position` — MoveL by Cartesian pose
- `get_servo_angle` — Read current joint angles
- `set_state(0)` — Enable robot motion
- `set_mode(0)` — Position control mode
- Official Python SDK: https://github.com/xArm-Developer/xArm-Python-SDK

#### C# Implementation Path
- Implement `IXArmClient : IRobotControlClient` using `TcpClient` (same pattern as `FairinoRobotClient`)
- Modbus TCP framing: function code 0x03 (read registers), 0x10 (write multiple registers)
- JSON command format available in xArm TCP API on port 18333 (simpler than raw Modbus)
- Mock: `MockXArmClient` returning fixed sine-wave joint states for editor testing

### Implementation Notes

1. **MDH conversion is the critical first step.** Without it, FK results will not match the physical robot. Either add an `MDHRow` struct alongside `DHRow` in `Assets/Scripts/Math/Types/`, or derive equivalent Standard DH parameters and document the conversion.
2. **Theta offsets** at joints 2 and 3 (+/-1.3849 rad) represent the non-zero home pose of the physical arm. These are not optional — omitting them produces wrong EE positions even at the canonical zero configuration.
3. **Skills to use when implementing:** `robot-template-add` (for `TemplateXArm6.cs`), `editmode-test-add` (for FK unit tests), `fairino-fr5-integration` (for TCP client pattern reference).
4. **File additions required (per plan architecture):**
   - `Assets/Scripts/Templates/TemplateXArm6.cs`
   - `Assets/Tests/EditMode/TemplateXArm6Tests.cs`
   - `Assets/Scripts/App/UFACTORY/MockXArmClient.cs`
   - `Assets/Scripts/App/UFACTORY/XArm6ControlDef.cs`
   - `RobotControlFactory.cs` — add `RobotId.XArm6` case

---

## Kinova Gen3 6DOF

### Overview
- Manufacturer: Kinova Robotics
- DOF: 6 (also available as 7DOF; this document covers 6DOF variant)
- Type: Articulated (6R), collaborative / research-grade
- DH Convention: URDF origin-based (requires extraction to DH form)
- Payload: 4 kg
- Reach: 902.9 mm
- License: BSD-3-Clause (ros2_kortex)

### DH Parameters (derived from URDF origin, approximate)

> Note: Kinova does not publish a formal DH table. Parameters below are extracted from
> `ros2_kortex` URDF joint origin fields. Validate against `get_measured_cartesian_pose`
> on real hardware before use in production lessons.

| Joint | d [m] | a [m] | alpha [rad] | Notes |
|-------|-------|-------|-------------|-------|
| 1 | 0.15643 | 0 | pi/2 | Base to shoulder |
| 2 | 0 | 0 | -pi/2 | Shoulder tilt |
| 3 | 0.41 | 0 | pi/2 | Upper arm |
| 4 | 0.20843 | 0 | -pi/2 | Elbow |
| 5 | 0.10593 | 0 | pi/2 | Wrist 1 |
| 6 | 0.10593 | 0 | 0 | Wrist 2 / EE |

Parameters: d1=0.15643 m, a3=0.41 m, d4=0.20843 m, d5=0.10593 m, d6=0.10593 m

### Joint Limits

| Joint | Min [deg] | Max [deg] | Notes |
|-------|-----------|-----------|-------|
| 1 | -Inf | +Inf | Continuous (no hard stop) |
| 2 | -128.9 | +128.9 | |
| 3 | -Inf | +Inf | Continuous |
| 4 | -147.8 | +147.8 | |
| 5 | -Inf | +Inf | Continuous |
| 6 | -Inf | +Inf | Continuous |

Source: Kinova Gen3 User Guide, `ros2_kortex` URDF joint limit fields

### URDF / Mesh Source

- **ROS2 Package:** `ros2_kortex`
- **GitHub:** https://github.com/Kinovarobotics/ros2_kortex
- **Mesh Path:** `kortex_description/arms/gen3/6dof/meshes/`
- **Mesh Format:** STL (requires STL to DAE conversion before Unity import)
- **License:** BSD-3-Clause
- **URDF Entry Point:** `kortex_description/arms/gen3/6dof/urdf/gen3_6dof.xacro`

#### Alternative URDF Sources
- Kinova official: https://github.com/Kinovarobotics/kortex (C++ SDK, includes URDF)
- `kortex_description` standalone: https://github.com/Kinovarobotics/kortex_description

#### Mesh Conversion Notes
- 6 visual STL files; base is separate
- File naming: `base.STL`, `shoulder.STL`, `halfArm1.STL`, `halfArm2.STL`, `foreArm.STL`, `wrist1.STL`, `wrist2.STL`, `bracelet.STL`
- After conversion: place under `Assets/Runtime/Resources/Robots/KinovaGen3/`

### SDK Protocol

| Property | Value |
|----------|-------|
| Protocol | gRPC (Protocol Buffers) |
| Port | 10000 |
| Transport | HTTP/2 |
| Auth | Session token (username + password handshake) |
| Complexity | High |

#### Key gRPC Services
- `Base` service: `PlayJointTrajectory`, `SendTwistCommand`, `GetMeasuredJointAngles`
- `BaseCyclic` service: high-frequency (1 kHz) state feedback via `Refresh` streaming RPC
- `DeviceConfig` service: firmware, serial number

#### C# Implementation Path
- Requires `Grpc.Net.Client` or `Grpc.Core` NuGet package in Unity
- Unity + gRPC: use `grpc_unity_package` from gRPC releases (native plugin required)
- gRPC proto files: `kortex_api/protos/` in the kortex SDK repo
- Code-generate C# stubs from `.proto` files using `protoc --csharp_out`
- Mock: `MockKinovaClient` returning fixed state; real client wraps `BaseClient.GetMeasuredJointAngles`
- Reference: https://github.com/Kinovarobotics/kortex/tree/master/api_cpp/examples

#### Complexity Warning
- gRPC in Unity requires native plugin setup and is significantly more complex than TCP socket approaches
- Consider a REST-to-gRPC bridge (`Kortex REST API` is available on port 10080) for simpler prototyping
- REST API endpoint: `GET /api/v1/base/joint_angles` returns current angles without gRPC setup

### Implementation Notes

1. **gRPC native plugin is the primary blocker.** Unity IL2CPP + gRPC has known compatibility issues. Test with Mono scripting backend first. Alternatively, use the Kortex REST API on port 10080 as a simpler fallback for the Mock/Live control path.
2. **URDF DH extraction** needs validation. The parameters above are URDF-origin-derived approximations. Cross-check using `get_measured_cartesian_pose` at known configurations (zero, stretch) against FK output.
3. **Continuous joints** (1, 3, 5, 6) mean the `JointInputRail` slider must not clamp at +/-360 for these joints. The slider should be configured with extended or unbounded range, or wrapped modulo 360.
4. **Skills to use when implementing:** `robot-template-add`, `editmode-test-add`, `fairino-fr5-integration` (TCP/gRPC client pattern), `asmdef-setup` (if adding gRPC NuGet requires new asmdef).
5. **File additions required (per plan architecture):**
   - `Assets/Scripts/Templates/TemplateKinovaGen3.cs`
   - `Assets/Tests/EditMode/TemplateKinovaGen3Tests.cs`
   - `Assets/Scripts/App/Kinova/MockKinovaClient.cs`
   - `Assets/Scripts/App/Kinova/KinovaGen3ControlDef.cs`
   - `RobotControlFactory.cs` — add `RobotId.KinovaGen3` case

---

## Franka Panda

### Overview
- Manufacturer: Franka Emika (now Franka Robotics)
- DOF: 7 (redundant manipulator)
- Type: Articulated (7R), collaborative / research-grade
- DH Convention: Modified DH (MDH), from Franka official documentation
- Payload: 3 kg
- Reach: 855 mm
- License: Apache-2.0 (franka_description, franka_ros2)

### DH Parameters (Modified DH, Franka official)

> Note: Franka publishes the MDH table in the official documentation at
> https://frankaemika.github.io/docs/control_parameters.html
> The table below is from the official source.

| Joint | d [m] | a [m] | alpha [rad] | Notes |
|-------|-------|-------|-------------|-------|
| 1 | 0.333 | 0 | 0 | Base to J1 |
| 2 | 0 | 0 | -pi/2 | J1 to J2 |
| 3 | 0.316 | 0 | pi/2 | J2 to J3 |
| 4 | 0 | 0.0825 | pi/2 | J3 to J4 |
| 5 | 0.384 | -0.0825 | -pi/2 | J4 to J5 |
| 6 | 0 | 0 | pi/2 | J5 to J6 |
| 7 | 0.107 | 0.088 | pi/2 | J6 to EE flange |

Source: Franka Emika official control parameters documentation

### Joint Limits

| Joint | Min [deg] | Max [deg] | Max Velocity [deg/s] | Max Acceleration [deg/s^2] |
|-------|-----------|-----------|---------------------|---------------------------|
| 1 | -166 | +166 | 150 | 1500 |
| 2 | -101 | +101 | 150 | 750 |
| 3 | -166 | +166 | 150 | 2500 |
| 4 | -176 | -4 | 150 | 1250 |
| 5 | -166 | +166 | 180 | 2500 |
| 6 | -1 | +215 | 180 | 2500 |
| 7 | -166 | +166 | 180 | 2500 |

Source: https://frankaemika.github.io/docs/control_parameters.html

### URDF / Mesh Source

- **ROS2 Package:** `franka_ros2`
- **GitHub:** https://github.com/frankaemika/franka_ros2
- **Standalone Description:** https://github.com/frankaemika/franka_description (Apache-2.0)
- **Mesh Path:** `franka_description/meshes/visual/`
- **Mesh Format:** DAE (Collada) — Unity-native, no conversion needed
- **License:** Apache-2.0
- **URDF Entry Point:** `franka_description/robots/panda/panda.urdf.xacro`

#### Alternative Sources
- MoveIt2 resources: https://github.com/moveit/moveit_resources (`panda_description`)
- Standalone franka_description: https://github.com/frankaemika/franka_description

#### Mesh Notes
- 7 visual DAE files: `link0.dae` through `link7.dae` plus `hand.dae`
- Meshes are high quality (smooth surfaces, good polygon count for visualization)
- DAE files are directly importable via Unity URDF Importer or manual drag-drop
- Place under `Assets/Runtime/Resources/Robots/FrankaPanda/` when ready

### SDK Protocol

| Property | Value |
|----------|-------|
| Protocol | UDP real-time (libfranka) |
| Interface | Dedicated FCI (Franka Control Interface) network port |
| Loop Rate | 1 kHz control loop (1 ms cycle time) |
| Auth | FCI license required (separate purchase) |
| Complexity | Very High |

#### libfranka Architecture
- `franka::Robot` class establishes UDP connection to robot controller
- Control loop callback: `robot.control(callback)` — callback called at 1 kHz
- Callback receives `RobotState` struct (joint positions, velocities, torques, EE pose)
- Callback returns `Torques`, `JointVelocities`, `JointPositions`, or `CartesianPose`
- Hard real-time guarantees required — standard Windows/Linux scheduler is insufficient

#### C# Implementation Path
- There is no official C# libfranka binding
- Options:
  1. **P/Invoke wrapper**: compile libfranka as a native DLL and call from C# via P/Invoke (requires Linux or patched Windows environment)
  2. **ROS2 bridge**: run `franka_ros2` on a Linux machine, expose joint states via a simple TCP relay, connect KineTutor3D to the relay (eliminates real-time requirement from Unity side)
  3. **Mock-only in editor**: implement `MockFrankaClient` with realistic 7DOF sine-wave state simulation; defer real hardware until options above are evaluated
- For teaching purposes, option 2 (ROS2 relay) is the most practical path
- Reference: https://github.com/frankaemika/franka_ros2

### 7DOF Considerations

This section describes what must change in KineTutor3D before Franka Panda can be implemented.

#### Current Architecture Constraints
- `RobotTemplate` base class defines `GetDHTable()` returning `DHRow[]` — array length is fixed per subclass
- `ForwardKinematics.ComputeFK()` iterates over the DH array; length is not hard-coded but the result is a single 4x4 matrix for the last joint
- `JointInputRail` creates one slider per joint; the count is driven by `template.GetJointCount()` — this already supports variable counts
- `DHTableEditor` renders one row per joint; also count-driven

#### What Needs to Change
1. **`ForwardKinematics`**: Currently the FK chain computation assumes Standard DH. Franka uses Modified DH. Either add `ComputeFKModified(DHRow[] table)` alongside the existing method, or add a `convention` flag to `DHRow` and branch internally.
2. **`WhyItMovedFormatter`**: Currently formats 6-joint summaries. Adding a 7th joint requires the formatter to be DOF-agnostic (iterate over all joints, not a fixed 6).
3. **`MathReadinessLessonFactory`**: Lesson content references joint counts; 7DOF adds null-space / redundancy concepts not currently in the lesson set.
4. **UI rail**: `JointInputRail` already uses `GetJointCount()` so it will render 7 sliders automatically. Verify the panel layout does not overflow on tablet viewport at 7 sliders.
5. **Template class**: `TemplateFrankaPanda.cs` would be `7 x DHRow` with MDH values. Add alongside existing 6DOF templates.

#### Educational Value of 7DOF
- Redundant DOF (null space) is a graduate-level concept not covered in current lesson content
- Recommend gating Franka Panda behind an "Advanced" difficulty badge in Robot Library
- Useful teaching topics: self-motion, joint-space optimization, singularity avoidance with redundancy
- These topics map to `docs/ref/product/content/lesson-framework.md` — add as Phase C content if implemented

#### Skills to Use When Implementing
- `robot-template-add` — for `TemplateFrankaPanda.cs` and FK tests
- `editmode-test-add` — 7DOF FK validation against official EE pose at known configs
- `fairino-fr5-integration` — for ROS2 relay TCP client pattern (closest analog to libfranka bridge)
- `dh-algorithm-add` — if MDH support is added to `ForwardKinematics`

#### File Additions Required (per plan architecture)
- `Assets/Scripts/Templates/TemplateFrankaPanda.cs` — 7 DHRow (MDH)
- `Assets/Tests/EditMode/TemplateFrankaPandaTests.cs` — FK at qz and qr configurations
- `Assets/Scripts/App/Franka/MockFrankaClient.cs` — 7-joint sine-wave simulation
- `Assets/Scripts/App/Franka/FrankaPandaControlDef.cs` — template definition
- `RobotControlFactory.cs` — add `RobotId.FrankaPanda` case
- Optionally: `Assets/Scripts/Kinematics/ForwardKinematicsMDH.cs` — MDH computation path

---

## Implementation Priority Order

When prerequisites are met, suggested order:

1. **xArm6** — TCP protocol is similar to existing FR5 implementation; main blocker is mesh conversion and MDH support
2. **Kinova Gen3** — Mesh conversion same as xArm6; gRPC is harder but REST fallback is viable for a first pass
3. **Franka Panda** — Requires 7DOF architectural changes and real-time protocol; defer until after xArm6 and Kinova are complete and the variable-DOF path is validated

## Related Documents

- [urdf-reference-collection.md](./urdf-reference-collection.md) — existing URDF sources (UR5, Puma560, Franka)
- [robot-template-expansion.md](./robot-template-expansion.md) — template expansion plan
- [robot-model-library-spec.md](./robot-model-library-spec.md) — robot metadata schema
- [fairino-fr5-integration-reference.md](./fairino-fr5-integration-reference.md) — FR5 SDK reference (TCP pattern to reuse)
- Plan file: `.claude/plans/lazy-launching-sifakis.md` — full multi-robot plan with B-phase breakdown
