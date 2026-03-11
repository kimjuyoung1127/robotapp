# KineTutor3D AGENTS Index

This is the source-of-truth navigation document for work in this repository.
Use this file for folder responsibility, file-discovery order, and refactor rules.

## Start Here
1. `AGENTS.md`
2. `CLAUDE.md`
3. `docs/status/PRODUCT-DOC-BOARD.md`
4. `docs/ref/PRD.md`
5. `docs/ref/WIREFRAME.md`
6. `docs/ref/PRODUCT-ROADMAP.md`

## Mandatory Navigation Rule
- Always read the root `AGENTS.md` before exploring files.
- When working inside `Assets/Scripts/App`, `Assets/Scripts/UI`, or `Assets/Scripts/Visualization`, read that folder's `AGENTS.md` first.
- For fast system-level context, read `docs/ref/architecture-mermaid.md` before chasing individual files.
- Product/planning work must read `docs/status/PRODUCT-DOC-BOARD.md` and the canonical product docs in `docs/ref/` before changing status files.
- Runtime implementation work should read `docs/ref/architecture-mermaid.md` after the canonical product docs.

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

## Task Routing
- Product direction changes: `docs/ref/PRD.md` -> `docs/ref/product/foundation/*`
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
- Plan-change procedure: `docs/ref/PRODUCT-ROADMAP.md` -> `docs/ref/product/roadmap/release-gates.md`
