# KineTutor3D AGENTS Index

This is the source-of-truth navigation document for work in this repository.
Use this file for folder responsibility, file-discovery order, and refactor rules.

## Start Here
1. `AGENTS.md`
2. `docs/ref/architecture-mermaid.md`
3. `CLAUDE.md`
4. `docs/ref/csharp-master-harness.md` (when creating or editing C#)
5. `docs/ref/code-patterns.md` (implementation detail and patterns)
6. `docs/status/PRODUCT-DOC-BOARD.md`
7. `docs/ref/PRD.md`
8. `docs/ref/WIREFRAME.md`
9. `docs/ref/PRODUCT-ROADMAP.md`
10. `docs/ref/phase5-implementation-plan.md` (when implementing or reviewing Phase 5)

## Mandatory Navigation Rule
- Always read the root `AGENTS.md` before exploring files.
- When working inside `Assets/Scripts/App`, `Assets/Scripts/UI`, or `Assets/Scripts/Visualization`, read that folder's `AGENTS.md` first.
- When creating or editing C# files, read `docs/ref/csharp-master-harness.md` and `docs/ref/code-patterns.md` before patching code.
- For fast system-level context, read `docs/ref/architecture-mermaid.md` before chasing individual files.
- Product/planning work must read `docs/status/PRODUCT-DOC-BOARD.md` and the canonical product docs in `docs/ref/` before changing status files.
- Runtime implementation work should read `docs/ref/architecture-mermaid.md` after the canonical product docs.

## Current Runtime Truth
- Active scene flow is `Boot -> Onboarding -> RobotLibrary -> {MathReadiness, Sandbox, RobotControl}`.
- `RobotLibrary` is the main user entry scene after onboarding.
- `Home` and `Main` are historical scene names, not current runtime entry points.
- Prefer `docs/ref/architecture-mermaid.md` and `docs/ref/project-flow-code-review.md` when verifying current flow.

## Folder Responsibility Rule
- `Assets/Scripts/App`: application state, scene flow, orchestration, runtime coordination.
- `Assets/Scripts/UI`: HUD, onboarding, tutorial interaction, glossary, gates, navigation.
- `Assets/Scripts/Visualization`: Unity-side rendering, donor mesh binding, frame ownership, visibility checks.
- `Assets/Scripts/Math`, `Types`, `Kinematics`, `Templates`: domain logic and presets, not Unity HUD behavior.

## Authoring Rule
- Every `.cs` file in `Assets/Scripts/App`, `Assets/Scripts/UI`, and `Assets/Scripts/Visualization` must start with a short folder-role comment.
- If a file starts to mix folder responsibilities, split it into helper/service classes or move logic to the correct folder.
- When adding or refactoring files in those folders, update the nearest `AGENTS.md` if the folder contract changes.

## Current Structural Intent
- `AppController` is the public application facade.
- `RobotRenderer` is the public visualization facade.
- `DHTableEditor` stays view-oriented; parsing/building helpers live beside it.
- `frame_0`, `frame_1`, and `Frame_EE` remain the canonical frame ownership points.
- `ScaraRobot.prefab` remains the donor source, with `Pick` excluded from visual donor usage.

## Product Doc Governance
- Canonical product docs live only in `docs/ref/PRD.md`, `docs/ref/WIREFRAME.md`, and `docs/ref/PRODUCT-ROADMAP.md`.
- Detailed product specs branch under `docs/ref/product/`.
- Product doc status is tracked only in `docs/status/PRODUCT-DOC-BOARD.md`.
- Product doc changes must sync downstream status/context docs and leave a `docs/daily/MM-DD/` log entry.

## Fast Product Context
- Current feature inventory and immediate build gaps: `docs/ref/product/roadmap/current-feature-checklist.md`
- Beginner-first lesson track: `docs/ref/product/content/lesson-framework.md` + `docs/ref/tutor-step-plan.md` + `docs/ref/USER-FLOW.md`
- Asset curation and validation: `docs/ref/asset-curation-map.md` + `docs/ref/asset-validation-report.md` + `docs/ref/asset-registry.md`
- URDF reference robots (UR5, Puma560, Franka): `docs/ref/product/robots/urdf-reference-collection.md`
- Workspace envelope algorithm research: `docs/ref/product/roadmap/workspace-envelope-algorithm-memo.md`
- Interactive matrix visualization design: `docs/ref/product/ux/interactive-matrix-viz-design-reference.md`
- Phase 5 execution plan: `docs/ref/phase5-implementation-plan.md`

## Task Routing
- Product direction changes: `docs/ref/PRD.md` -> `docs/ref/product/foundation/*`
- Current feature scope / what's implemented: `docs/ref/product/roadmap/current-feature-checklist.md`
- Phase 5 runtime/UI implementation or review: `docs/ref/phase5-implementation-plan.md` -> `Assets/Scripts/App/AGENTS.md` -> `Assets/Scripts/UI/AGENTS.md` -> `Assets/Scripts/Visualization/AGENTS.md`
- Beginner Lesson 0~3 / pre-kinematics flow: `docs/ref/product/content/lesson-framework.md` -> `docs/ref/product/ux/guided-lesson.md` -> `docs/ref/tutor-step-plan.md` -> `docs/ref/USER-FLOW.md`
- Guided Lesson work: `docs/ref/WIREFRAME.md` -> `docs/ref/product/ux/guided-lesson.md`
- Robot model work: `docs/ref/product/robots/robot-model-library-spec.md`
- Sandbox work: `docs/ref/product/ux/sandbox.md`
- Instructor workflow: `docs/ref/product/ux/instructor-mode.md`
- Tablet/mobile policy: `docs/ref/product/ux/tablet-first-policy.md`
- Private lecture material adaptation: `docs/ref/product/content/derived-course-content-policy.md` + `docs/ref/product/content/concept-to-ui-map.md`
- Public robotics reference adaptation: `docs/ref/product/content/open-robotics-reference-pack.md` + `.claude/skills/kinetutor-guide/content/robotics-reference-to-lesson/SKILL.md`
- Competitive product synthesis: `docs/ref/product/foundation/competitive-synthesis.md` -> `docs/ref/product/foundation/product-positioning.md` / `docs/ref/product/roadmap/milestone-backlog.md`
- LLM teaching strategy: `docs/ref/product/content/llm-teaching-strategy.md`
- Mobile release planning: `docs/ref/product/roadmap/mobile-release-checklist.md`
- Asset sourcing / curation / validation: `docs/ref/product/roadmap/asset-sourcing-checklist.md` -> `docs/ref/asset-curation-map.md` -> `docs/ref/asset-validation-report.md` -> `docs/ref/asset-registry.md`
- Plan-change procedure: `docs/ref/PRODUCT-ROADMAP.md` -> `docs/ref/product/roadmap/release-gates.md`

## Subfolder Claude Routing
- App runtime, scene flow, session state: `Assets/Scripts/App/CLAUDE.md`
- RobotControl runtime and live/mock robot integration: `Assets/Scripts/App/Fairino/CLAUDE.md`
- UR5e-specific RobotControl setup: `Assets/Scripts/App/UniversalRobots/CLAUDE.md`
- Doosan-specific RobotControl setup: `Assets/Scripts/App/Doosan/CLAUDE.md`
- Meca500-specific RobotControl setup: `Assets/Scripts/App/Mecademic/CLAUDE.md`
- External hand tracking input: `Assets/Scripts/App/HandTracking/CLAUDE.md`
- HUD, onboarding, tutorial, navigation UI: `Assets/Scripts/UI/CLAUDE.md`
- UI config/data assets: `Assets/Scripts/UI/Data/CLAUDE.md`
- Visualization facade and donor/render binding: `Assets/Scripts/Visualization/CLAUDE.md`
- Shared visualization primitives and URDF helpers: `Assets/Scripts/Visualization/Shared/CLAUDE.md`
- Editor QA and authoring utilities: `Assets/Editor/KineTutor3D/CLAUDE.md`
- `unityctl exec` helpers and CLI automation entry points: `Assets/Editor/KineTutor3D/CliTools/CLAUDE.md`
- Test suite overview: `Assets/Tests/CLAUDE.md`
- EditMode test rules: `Assets/Tests/EditMode/CLAUDE.md`
- PlayMode smoke and flow tests: `Assets/Tests/PlayMode/CLAUDE.md`

## Unityctl Default Usage
- Default Unity automation tool for this repository is `unityctl`, not `unity-cli`.
- Fixed default path: `C:\Users\ezen601\Desktop\Jason\unityctl\src\Unityctl.Cli\bin\Debug\net10.0\unityctl.exe`
- First command in a new session should usually be:
  `& 'C:\Users\ezen601\Desktop\Jason\unityctl\src\Unityctl.Cli\bin\Debug\net10.0\unityctl.exe' status --project 'C:\Users\ezen601\Desktop\Jason\robotapp2' --wait --json`
- Preferred verification loop:
  `status --wait` -> `check --type compile` -> `play start/stop` or `test --mode edit/play` -> `console get-entries` / `exec`
- Use `exec` for project-specific runtime inspection when no dedicated `unityctl` command exists.
- Only fall back to MCP when `unityctl` has no equivalent command or IPC is unavailable.
- Treat `docs/ref/cli-tools-guide.md` and `unity-cli` commands as legacy/historical guidance only unless a task explicitly requires them.

## Unityctl Working Recipes
- Session bootstrap:
  `status --wait` -> `check --type compile` -> `console get-entries`
- Fast C# validation loop:
  edit files -> `check --type compile` -> `test --mode edit` -> inspect console if needed
- Scene/UI verification loop:
  `scene open` -> `play start` -> `console get-entries` -> `ui find/get/toggle/input` or `exec` -> `play stop`
- Runtime investigation loop:
  `status --wait` -> `play start` -> `exec` for project probes -> `console get-entries` -> `screenshot capture` or `scene snapshot`
- Regression closure loop:
  `check --type compile` -> targeted `test --mode edit` or `test --mode play` -> `console clear` -> rerun failing path
- Practical shell setup per session:
  set `$unityctl = 'C:\Users\ezen601\Desktop\Jason\unityctl\src\Unityctl.Cli\bin\Debug\net10.0\unityctl.exe'`
  set `$project = 'C:\Users\ezen601\Desktop\Jason\robotapp2'`
  then call `& $unityctl status --project $project --wait --json`
