# KineTutor3D Execution Plan (PRD v2.1.0 Aligned)

- Project: KineTutor3D (Robot Kinematics Learning Tool)
- Date: 2026-03-05
- Target Unity: 2022.3 LTS (confirmed: 2022.3.62f3)
- Rendering: Built-in Render Pipeline (`3D Core` template)
- Language: C# (Unity 2022 baseline)
- Math precision: `double` fixed (`Vec3D/Mat3D/Mat4D`)

## 1) Final Technical Decisions

1. Unity version: `2022.3.62f3 LTS` (PRD compliant)
2. Project template: `3D Core` (Built-in RP)
3. API Compatibility: `.NET Standard 2.1`
4. Input Handling: `Both` (compatibility-safe) or `Input Manager (Old)`
5. Initial build target: `Windows`
6. No external API keys/network dependencies for MVP

## 2) Unity Project Setup Checklist

1. Create project with Unity Hub
   - Template: `3D Core`
   - Path: `C:\Users\ezen601\Desktop\Jason\robotapp2`
2. Open Project Settings
   - `Edit > Project Settings > Player > Other Settings`
   - `Api Compatibility Level = .NET Standard 2.1`
   - `Active Input Handling = Both`
3. Verify Built-in RP
   - `Edit > Project Settings > Graphics`
   - `Scriptable Render Pipeline Settings = None`
4. Create startup scene
   - `Assets/Scenes/Main.unity`
   - Add basic camera/light/ground for initial smoke checks

## 3) Unity MCP Setup Guide (2022.3-compatible path)

Recommended implementation: CoplayDev `unity-mcp`

### 3.1 Install package
1. Ensure Git is installed on Windows.
2. Unity menu: `Window > Package Manager`
3. Click `+` > `Add package from git URL...`
4. Use:
   - `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`

### 3.2 Start MCP server in Unity
1. Open: `Window > MCP for Unity`
2. Press `Start Server`
3. Default endpoint: `http://localhost:8080/mcp`

### 3.3 Connect MCP client
1. Use `MCP Client` dropdown > `Configure`
2. If manual config needed, use:

```json
{
  "mcpServers": {
    "unityMCP": {
      "url": "http://localhost:8080/mcp"
    }
  }
}
```

### 3.4 Connectivity smoke test
1. Confirm status shows `Connected`.
2. Run one basic read action from MCP client.
3. Confirm Unity editor receives and executes action.

## 4) PRD-driven Build Phases

## Phase 0: Foundation (Day 0)
Goal: reproducible baseline and repo hygiene

Tasks:
1. Initialize Git repo (if not already)
2. Add `.gitignore` for Unity
3. Create root docs:
   - `README.md` (skeleton)
   - `Master_PRD.md` (source reference)
   - `MIGRATION_NOTES.md`
   - `RELEASE_NOTES.md`
4. Confirm clean open/play in Unity editor

Done when:
1. Project opens with zero compile errors
2. Main scene saves and reloads correctly

## Phase 1: Step 1 + Step 2 (File tree + Types/Math) (Day 1)
Goal: compile-first base domain model and matrix library

Required tree under `Assets/Scripts`:
1. `Types/JointType.cs`
2. `Types/DHLink.cs`
3. `Types/RobotTemplate.cs`
4. `Types/Pose.cs`
5. `Math/Vec3D.cs`
6. `Math/Mat3D.cs`
7. `Math/Mat4D.cs`

Rules:
1. Pure functions first
2. No Unity-specific dependencies in core math modules
3. Guard against NaN/Infinity constructors and ops

Done when:
1. Unity compile succeeds
2. Basic math self-test method returns pass on known identity case

## Phase 2: Kinematics Core + EditMode Tests (Day 2)
Goal: mathematically correct Standard DH + FK

Implement:
1. `Kinematics/DHStandard.cs`
   - `A_i = Rz(theta) * Tz(d) * Tx(a) * Rx(alpha)`
2. `Kinematics/ForwardKinematics.cs`
   - cumulative product `T = A1...Ak`
   - extract `R` and `p`

Tests (`Assets/Tests/EditMode`):
1. `DHStandardTests.cs`
2. `ForwardKinematicsTests.cs`

Minimum cases:
1. identity-like parameter case
2. 2DOF RR numeric check (Appendix A equations)
3. 3DOF SCARA numeric sanity

Done when:
1. EditMode tests pass
2. Numeric outputs match expected tolerance

## Phase 3: Minimal Tutor UI + 2DOF runtime (Day 3)
Goal: first user-visible learning loop

Implement:
1. `UI/DHTableEditor.cs`
2. `UI/JointSliderPanel.cs`
3. `UI/StepTutorPanel.cs` (text MVP)
4. `Templates/Template2DOF_RR.cs`
5. `App/AppController.cs`

Behavior:
1. edit DH values
2. move sliders
3. realtime update of `A_i`, `^0T_n`, EE pose

Done when:
1. 2DOF robot moves in scene
2. step text and matrix display update together
3. no NaN/Infinity accepted in UI

## Phase 4: Visualization + Validator + PlayMode smoke (Day 4)
Goal: confidence and pedagogical clarity

Implement:
1. `Visualization/FrameGizmo.cs`
2. `Visualization/VectorArrow.cs`
3. `Visualization/StepAnimator.cs`
4. `Kinematics/Validator.cs`
5. `Tests/PlayMode/ValidationSmokeTests.cs`

Validator metrics:
1. `position error = |p_dh - p_unity|`
2. `rotation error = angle(R_dh^T * R_unity)`

Acceptance thresholds:
1. `position < 1e-4 m`
2. `rotation < 1e-3 rad`

Done when:
1. validation pass/fail visibly shown
2. PlayMode smoke test passes

## Phase 5: 3DOF + 6DOF templates (Day 5)
Goal: scaling structure

Implement:
1. `Templates/Template3DOF_SCARA.cs`
2. `Templates/Template6DOF_IndustryA.cs`

Rules:
1. template selection auto-syncs DH table/sliders/model
2. joint limit clamp enforced

Done when:
1. both templates load without crash
2. FK updates stable during slider changes

## Phase 6: CI/CD and build automation (Day 6)
Goal: merge-safe pipeline

GitHub Actions pipeline:
1. optional lint/style
2. EditMode tests
3. PlayMode tests
4. Windows build artifact zip

Conventions:
1. branch strategy: trunk-based + `codex/feature-*`
2. commits: conventional commits (`feat:`, `fix:`, `test:`)
3. no merge if tests fail

Done when:
1. workflow green on push/PR
2. build artifact downloadable

## Phase 7: Documentation + release readiness (Day 7)
Goal: handoff quality

Required docs:
1. `README.md`
2. `RELEASE_NOTES.md`
3. `MIGRATION_NOTES.md`

README must include:
1. install/run
2. templates (2DOF/3DOF/6DOF)
3. short DH/FK concept
4. validator usage
5. local test/CI commands

Done when:
1. fresh user can run project and tests from docs only

## 5) Acceptance Checklist (must all pass)

1. Windows build success
2. EditMode tests pass
3. PlayMode tests pass
4. NaN/Infinity input blocked
5. 2DOF numeric result matches reference equation
6. Validator threshold satisfied
7. Step Tutor steps 1-8 functional

## 6) Suggested Initial Commits

1. `chore: initialize Unity 2022.3.62f3 project (3D Core)`
2. `feat: add core Types and double-precision Math modules`
3. `feat: implement Standard DH and FK engine`
4. `test: add EditMode tests for DH and FK`
5. `feat: add MVP Step Tutor UI and 2DOF template`
6. `feat: add validator and PlayMode smoke tests`
7. `feat: add 3DOF and 6DOF templates`
8. `ci: add GitHub Actions for tests and Windows build`
9. `docs: finalize README, release notes, migration notes`

## 7) Daily Reporting Format (for AI agent or team)

After each phase, report:
1. Changed files list
2. Test result summary
3. Remaining risks
4. Next phase entry criteria

Example:
- Files: `Assets/Scripts/Math/Mat4D.cs`, `Assets/Tests/EditMode/DHStandardTests.cs`
- Tests: `EditMode 12/12 pass`
- Risks: `Unity coordinate mapping for frame visualization pending`
- Next: `Phase 3 UI wiring`

## 8) High-Risk Items and Mitigations

1. Coordinate frame mismatch (robotics vs Unity)
   - Fix: lock mapping spec in code comments + validator checks
2. Precision drift
   - Fix: core math in `double`, cast to Unity types only at render boundary
3. 6DOF template complexity
   - Fix: deliver 2DOF/3DOF first, keep 6DOF numeric-only initially
4. UI readability
   - Fix: step-level detail toggle (`formula/intermediate/final`) in tutor panel

## 9) Immediate Next Actions (today)

1. Create Unity project with `3D Core` in this folder
2. Install and verify `unity-mcp`
3. Generate `Assets/Scripts/Types/*` and `Assets/Scripts/Math/*`
4. Compile check
5. Add first math self-test and one EditMode test

---

This plan is designed to satisfy the PRD v2.1.0 constraints while minimizing early-stage risk and preserving expansion path to 6DOF and future Modified DH/IK/Jacobian work.
