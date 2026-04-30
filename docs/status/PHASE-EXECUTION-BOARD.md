# Phase 실행 보드

> Note: 이 문서는 broad phase board다.
> 현재 FR5 / Pendant V3의 실제 우선순위와 열린 이슈는 먼저 [ACTIVE-WORK-INDEX.md](./ACTIVE-WORK-INDEX.md)를 본다.

상태값: `Ready | InProgress | QA | Done | Hold`

| phase | module | priority | status | owner | skills_required | must_read_docs | last_updated |
|-------|--------|----------|--------|-------|----------------|----------------|--------------|
| Phase 0 | Foundation | P0 | Done | codex | unity-official-docs, asmdef-setup | docs/status/PROJECT-STATUS.md, docs/ref/unity-official-evidence-phase01.md | 2026-03-05 |
| Phase 1 | Types | P0 | Done | codex | math-module-add, unity-official-docs | docs/ref/dh-reference.md, docs/ref/unity-official-evidence-phase01.md | 2026-03-05 |
| Phase 1 | Math | P0 | Done | codex | math-module-add, editmode-test-add, unity-official-docs | docs/ref/dh-reference.md, docs/ref/test-reference-values.md, docs/ref/unity-official-evidence-phase01.md | 2026-03-05 |
| Phase 2 | DH Standard | P0 | Done | codex | dh-algorithm-add, editmode-test-add | docs/ref/dh-reference.md, docs/ref/coordinate-mapping.md | 2026-03-05 |
| Phase 2 | FK Engine | P0 | Done | codex | dh-algorithm-add, editmode-test-add | docs/ref/test-reference-values.md | 2026-03-05 |
| Phase 3 | UI (DHTable/Sliders/Tutor) | P0 | Done | codex | tutor-step-add, student-friendly-ux | docs/ref/architecture-diagrams.md | 2026-03-09 |
| Phase 3 | Student-Friendly UX | P0 | Done | codex | student-friendly-ux, tutor-step-add | docs/ref/tutor-step-plan.md, docs/ref/USER-FLOW.md | 2026-03-05 |
| Phase 3 | Template 2DOF | P0 | Done | codex | robot-template-add, tutor-step-add | docs/ref/test-reference-values.md, docs/ref/architecture-diagrams.md | 2026-03-09 |
| Phase 4 | Visualization | P1 | Done | codex | scene-scaffold, editmode-test-add | docs/ref/coordinate-mapping.md, docs/ref/architecture-diagrams.md | 2026-03-09 |
| Phase 4 | Scene Flow | P1 | Done | codex | scene-scaffold, sprint-docs-sync | docs/ref/USER-FLOW.md, docs/ref/architecture-diagrams.md | 2026-03-09 |
| Phase 4 | Validator | P1 | Ready | - | editmode-test-add | docs/ref/test-reference-values.md | 2026-03-05 |
| Phase 5 | Runtime foundation (snapshot/update cause) | P0 | Done | codex | tutor-step-add, student-friendly-ux | docs/ref/phase5-implementation-plan.md, docs/ref/tutor-step-plan.md, docs/ref/USER-FLOW.md | 2026-03-11 |
| Phase 5 | Track-aware step foundation | P0 | Done | codex | tutor-step-add, student-friendly-ux | docs/ref/phase5-implementation-plan.md, docs/ref/tutor-step-plan.md, docs/ref/USER-FLOW.md | 2026-03-11 |
| Phase 5 | Joint Numeric Input + Highlight | P0 | Done | codex | student-friendly-ux, scene-scaffold | docs/ref/phase5-implementation-plan.md, docs/ref/product/roadmap/current-feature-checklist.md | 2026-03-11 |
| Phase 5 | Visualization Helpers (trail/target) | P0 | Done | codex | scene-scaffold, editmode-test-add | docs/ref/phase5-implementation-plan.md, docs/ref/asset-validation-report.md | 2026-03-11 |
| Phase 5 | Why It Moved explanation layer | P0 | Done | codex | student-friendly-ux, tutor-step-add | docs/ref/phase5-implementation-plan.md, docs/ref/product/ux/guided-lesson.md | 2026-03-12 |
| Phase 5 | Beginner Lesson L0~L3 integration | P0 | Done | codex | tutor-step-add, student-friendly-ux | docs/ref/phase5-implementation-plan.md, docs/ref/tutor-step-plan.md, docs/ref/USER-FLOW.md | 2026-03-12 |
| Phase 5 | Math Readiness Track | P0 | Done | codex | tutor-step-add, student-friendly-ux | docs/ref/tutor-step-plan.md, docs/ref/USER-FLOW.md | 2026-03-12 |
| Phase 5 | Robot Library MVP | P1 | Done | codex | robot-template-add, student-friendly-ux | docs/ref/phase5-implementation-plan.md, docs/ref/product/ux/robot-library.md | 2026-03-12 |
| Phase 5 | Tests + Docs (5G) | P0 | Done | codex | sprint-docs-sync, pre-commit-validate | docs/ref/phase5-implementation-plan.md, docs/status/PROJECT-STATUS.md | 2026-03-12 |
| Phase 5 | RobotLibrary Main Entry | P0 | Done | codex | student-friendly-ux, scene-scaffold | docs/ref/WIREFRAME.md, docs/ref/USER-FLOW.md | 2026-03-12 |
| Phase 5 | Scene UI Visibility Cleanup | P0 | Done | codex | scene-ui-visibility, ui-design-system | Assets/Scripts/UI/CLAUDE.md, .claude/skills/kinetutor-guide/ui/scene-ui-visibility/SKILL.md | 2026-03-12 |
| Phase 5 | Page QA Hardening | P0 | InProgress | codex | student-friendly-ux, scene-ui-visibility, ui-design-system | docs/status/page-qa/README.md, docs/ref/product/ux/guided-lesson.md, docs/ref/product/ux/sandbox.md | 2026-03-12 |
| Phase 5 | Sandbox Polish | P0 | InProgress | codex | student-friendly-ux, scene-scaffold | docs/ref/product/ux/sandbox.md, docs/ref/product/roadmap/current-feature-checklist.md | 2026-03-12 |
| Phase 5 | Resume / Session Context | P0 | Done | codex | tutor-step-add, student-friendly-ux | docs/ref/USER-FLOW.md, docs/ref/product/roadmap/current-feature-checklist.md | 2026-03-12 |
| Phase 5 | Tablet 4DOF Input Usability | P0 | Ready | - | student-friendly-ux | docs/ref/product/ux/tablet-first-policy.md, docs/ref/product/ux/guided-lesson.md | 2026-03-12 |
| Phase 5 | Snapshot Lite | P0 | Done | codex | student-friendly-ux, editmode-test-add | docs/ref/product/ux/sandbox.md, docs/ref/product/roadmap/current-feature-checklist.md | 2026-03-12 |
| Phase 5 | UI Design System 2nd Pass (4 Panels) | P1 | Done | codex | ui-design-system, student-friendly-ux | Assets/Scripts/UI/CLAUDE.md, docs/status/PROJECT-STATUS.md | 2026-03-12 |
| Phase 5 | MathReadiness UX Enhancement (A+B+C) | P0 | Done | codex | student-friendly-ux, ui-design-system, scene-ui-visibility | docs/ref/tutor-step-plan.md, docs/ref/product/ux/guided-lesson.md | 2026-03-12 |
| Phase 5 | MathReadiness Manipulation-First + Angle Reference | P0 | Done | codex | student-friendly-ux, scene-scaffold, ui-design-system | docs/ref/product/ux/guided-lesson.md, docs/ref/tutor-step-plan.md, docs/ref/USER-FLOW.md | 2026-03-12 |
| Phase 5 | Mode-Based Panel Isolation | P0 | Done | codex | scene-ui-visibility, ui-design-system | .claude/skills/kinetutor-guide/ui/scene-ui-visibility/SKILL.md | 2026-03-12 |
| Phase 5 | Replay / Compare / Motion History | P1 | Ready | - | student-friendly-ux, editmode-test-add | docs/ref/product/ux/sandbox.md, docs/ref/product/roadmap/current-feature-checklist.md | 2026-03-12 |
| Phase 5 | Constraint Preview | P1 | Ready | - | student-friendly-ux, scene-scaffold | docs/ref/product/ux/sandbox.md, docs/ref/product/roadmap/current-feature-checklist.md | 2026-03-12 |
| Phase 5 | Instructor Demo Mode | P1 | Ready | - | student-friendly-ux | docs/ref/product/ux/instructor-mode.md, docs/ref/product/roadmap/current-feature-checklist.md | 2026-03-12 |
| Phase 5 | FAIRINO FR5 Robot Control Console | P1 | QA | codex | fairino-fr5-integration, scene-scaffold, ui-design-system | docs/ref/product/robots/fairino-fr5-integration-reference.md, docs/status/ROBOTCONTROL-IMPL-BOARD.md | 2026-03-15 |
| Phase 5 | Gameplay Camera Centralization | P1 | Done | codex | scene-scaffold | docs/status/PROJECT-STATUS.md, Assets/Scenes/CLAUDE.md | 2026-03-13 |
| Phase 5 | IVisibilityControllable + Token Migration | P1 | Done | codex | ui-design-system, scene-ui-visibility | Assets/Scripts/UI/CLAUDE.md, docs/status/PROJECT-STATUS.md | 2026-03-13 |
| Phase 5 | OnboardingViewBuilder Extract | P1 | Done | codex | viewbuilder-extract, ui-design-system | Assets/Scripts/UI/CLAUDE.md | 2026-03-13 |
| Phase 5 | Template 3DOF | P1 | Ready | - | robot-template-add | docs/ref/product/robots/robot-template-expansion.md | 2026-03-12 |
| Phase 5 | Template 6DOF | P2 | Ready | - | robot-template-add | docs/ref/product/robots/robot-template-expansion.md | 2026-03-12 |
| Phase 6 | CI/CD | P2 | Hold | - | pre-commit-validate | .github/workflows/unity-tests.yml | 2026-03-12 |
| Phase 7 | Documentation | P1 | Done | codex | sprint-docs-sync | AGENTS.md, docs/ref/architecture-mermaid.md | 2026-03-12 |

## Zero-Drift 규칙
1. `Assets/Scripts/` 구조를 코드 모듈 Source of Truth로 간주한다.
2. 보드의 module 집합과 `SKILL-DOC-MATRIX.md`의 target_module 집합은 동기화한다.
3. Phase 0/1의 asmdef/tests/compile/serialization 결정은 `docs.unity3d.com` 링크 근거를 필수로 남긴다.
4. Phase 4 Visualization은 `frame_0`/`frame_1`/`Frame_EE` ownership과 donor mesh source 정책을 유지한다.
5. 학습 화면 MVP는 `TopBar`/`LeftPanel`/`RightPanel`/`BottomBar` 4영역 surface를 기준으로 유지한다.
6. Phase 4 렌더 기준은 URP와 Solid Color camera를 사용한다.
7. 시작 흐름은 `Boot -> Onboarding` (첫 방문) / `Boot -> RobotLibrary` (재방문) 분기와 `LoadSceneMode.Single`을 기준으로 유지한다. Editor Play Mode는 `BootScenePlayModeSetup`으로 Boot.unity 시작을 보장한다.
8. 학습/제어 진입 허브는 `RobotLibrary`이고, 온보딩은 `Onboarding` 씬 전용 책임으로 분리한다.
9. 학습용 overlay root(`GlossaryPanel`, focus/highlight 계열)는 해당 active scene에서 기본 inactive 상태를 유지하고, 유효한 HUD target이 있을 때만 활성화한다.
10. 루트 `AGENTS.md`와 폴더 `AGENTS.md`를 파일 탐색의 1차 진입점으로 사용하고, 전체 맥락은 `docs/ref/architecture-mermaid.md`로 먼저 파악한다.
11. `PRODUCT-ROADMAP.md`의 릴리스 게이트와 이 보드의 phase 상태는 충돌 없이 유지한다.
12. 재진입 surface는 `RobotLibrary`다. 온보딩 `초보자 시작`은 `MathReadiness`로 직행하고, 나머지는 `RobotLibrary`로 연결한다.
13. Sandbox/학습 모드 패널 배타 제어: `SandboxSceneCoordinator`가 학습 패널 GameObject를 숨기고 Sandbox 패널을 명시 활성화. `AppController.ApplyFeatureState()`가 학습 모드에서 Sandbox 패널을 숨김.
14. 페이지 품질 점검은 `docs/status/page-qa/README.md`와 각 runbook을 기준으로 관리한다. 2026-03-23 baseline 매트릭스는 archive 문서로만 유지한다.
