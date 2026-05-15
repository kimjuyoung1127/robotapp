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

## Minimum Read Sets

### Session Minimum
- `AGENTS.md`
- `harness/REGISTRY.md`
- `docs/ref/architecture-mermaid.md`
- `CLAUDE.md`

### C# Edit Minimum
- `AGENTS.md`
- `harness/REGISTRY.md`
- `docs/ref/architecture-mermaid.md`
- `CLAUDE.md`
- `docs/ref/csharp-master-harness.md`
- `docs/ref/code-patterns.md`

### RobotControlV2 Minimum
- `AGENTS.md`
- `harness/REGISTRY.md`
- `docs/ref/architecture-mermaid.md`
- `CLAUDE.md`
- `docs/ref/csharp-master-harness.md`
- `docs/ref/code-patterns.md`
- `docs/ref/product/ux/robotcontrol-next-session-handoff.md`
- `docs/ref/product/ux/robotcontrol-implementation-bridge.md`
- `docs/ref/product/roadmap/robotcontrol-soft-teaching-pad-v1-backlog.md`

## Mandatory Navigation Rule
- Always read the root `AGENTS.md` before exploring files.
- For new requests or fuzzy scope, read `harness/REGISTRY.md` early and start with the meta routing loop (`task-intake-router` -> `change-impact-map` -> `evidence-review` or `session-handoff`) before broad file chasing.
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

## RobotControl Implementation Guardrails
- `RobotControl` 구현은 full rewrite보다 `구조적 재조립`을 기본 전략으로 삼는다.
- 기존 `App/Fairino` 런타임 로직, 상태 타입, 시각화 코어는 최대한 재사용하고, UI 셸과 패널 구조를 먼저 재편한다.
- 구현은 `branch-first`로 진행한다. 권장 브랜치 접두사는 `codex/robotcontrol-*` 이다.
- 문서 기준선은 메인에 남기고, 실제 구현 변경은 기능 브랜치에서 진행한다.
- 구현 Phase는 `셸 -> 필수 패널 -> 3D 차별화 -> UX 보강 -> Tablet` 순서를 기본으로 둔다.
- `RobotControlV2`의 색/크기/상태 표현은 `UIDesignTokens.RobotControlV2`를 SSOT로 사용하고, 셸/상단바/후속 authored 패널은 이 토큰을 직접 소비한다.
- `RobotControlV2` authored 레이아웃을 지킬 때는 `play 시작 1회 authored-lock` 훅만 허용한다. per-frame UI 재배치 훅이나 지속적인 layout rewrite는 금지한다.
- `Always Start From Onboarding`는 유지한다. `RobotControlV2` 검증도 `Onboarding -> RobotLibrary -> RobotControlV2` 사용자 흐름 안에서 수행한다.
- 각 Phase 종료 전에는 반드시 자기리뷰를 수행한다.
  - 역할 경계가 지켜졌는지
  - 하드코딩 색/간격/폰트가 늘지 않았는지
  - authored-first 바인딩이 유지되는지
  - `필수 / 선택 / 제외` 범위를 넘지 않았는지
- 각 Phase 종료 전에는 반드시 검증 루프를 돈다.
  - `unityctl check --type compile`
  - 관련 `EditMode` 테스트
  - 필요 시 `play start`, `scene snapshot`, `screenshot capture`
- 문서 업데이트만 하고 작업을 멈추지 않는다. 문서 기준선을 갱신한 뒤에는 같은 세션에서 즉시 다음 구현/검증 루프로 이어간다.
- 문서 업데이트가 발생한 턴에는 최소 `다음 실행 단위 1개`를 바로 진행한다.
- 각 Phase는 검증이 끝난 뒤 커밋하고 다음 Phase로 넘어간다.
- 커밋에는 해당 Phase 범위만 포함하고, unrelated 변경은 섞지 않는다.
- `Program`, `Status`, `Application`, `Initial/System`의 고급 기능은 문서에서 `제외`로 분류된 한 메인 구현 범위에 끼워 넣지 않는다.
- 패널이 비대해지면 즉시 쪼갠다. 한 패널 안에 레이아웃, 입력 파싱, 상태 계산, 통신 호출, 도움말 생성이 동시에 모이지 않게 한다.
- 상태 원천은 한 곳에 두고, 여러 패널이 개별 계산으로 중복 소유하지 않게 한다.

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

## Harness Adoption Policy
- This repository keeps `AGENTS.md`, `CLAUDE.md`, and `docs/ref/csharp-master-harness.md` as the local source of truth.
- Generic harness material may be absorbed only as supporting governance or reusable templates.
- Do not replace this repository's loading order, Unity verification loop, or folder-routing rules with an external harness.
- If a generic rule conflicts with local runtime facts or local docs, prefer local code and local docs.
- Promote a rule from reusable guidance into local SSOT only after it proves useful here and does not weaken project-specific clarity.

## Meta Ops Loop
- Request triage: `.claude/commands/intake.md` or `.claude/skills/meta/task-intake-router/SKILL.md`
- Impact narrowing: `.claude/commands/impact-map.md` or `.claude/skills/meta/change-impact-map/SKILL.md`
- Completion gate: `.claude/commands/evidence-review.md` or `.claude/skills/meta/evidence-review/SKILL.md`
- Pause or session transfer: `.claude/commands/handoff.md` or `.claude/skills/meta/session-handoff/SKILL.md`
- Keep this loop lightweight. It routes to local FR5/V3 SSOT docs and does not replace them.

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
- Guided lesson HUD and shared lesson panels: `Assets/Scripts/UI/GuidedLesson/CLAUDE.md`
- Onboarding page UI: `Assets/Scripts/UI/Onboarding/CLAUDE.md`
- Robot Library page UI: `Assets/Scripts/UI/RobotLibrary/CLAUDE.md`
- RobotControl page UI: `Assets/Scripts/UI/RobotControl/CLAUDE.md`
- MathReadiness page UI: `Assets/Scripts/UI/MathReadiness/CLAUDE.md`
- Sandbox page UI builders: `Assets/Scripts/UI/Sandbox/CLAUDE.md`
- Shared cross-page UI widgets: `Assets/Scripts/UI/Shared/CLAUDE.md`
- UI design system primitives: `Assets/Scripts/UI/DesignSystem/CLAUDE.md`
- UI config/data assets: `Assets/Scripts/UI/Data/CLAUDE.md`
- Visualization facade and donor/render binding: `Assets/Scripts/Visualization/CLAUDE.md`
- Visualization renderer facade and donor binding: `Assets/Scripts/Visualization/Renderer/CLAUDE.md`
- Visualization RobotLibrary preview helpers: `Assets/Scripts/Visualization/RobotLibrary/CLAUDE.md`
- Visualization RobotControl-specific drivers: `Assets/Scripts/Visualization/RobotControl/CLAUDE.md`
- Visualization MathReadiness helpers: `Assets/Scripts/Visualization/MathReadiness/CLAUDE.md`
- Visualization target/highlight helpers: `Assets/Scripts/Visualization/Targets/CLAUDE.md`
- Shared visualization primitives and URDF helpers: `Assets/Scripts/Visualization/Shared/CLAUDE.md`
- Domain value types and robot metadata: `Assets/Scripts/Types/CLAUDE.md`
- Robot template definitions and catalog: `Assets/Scripts/Templates/CLAUDE.md`
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
- Treat `docs/archive/legacy/unity-cli/cli-tools-guide.md` and `unity-cli` commands as legacy/historical guidance only unless a task explicitly requires them.

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
