# Phase 실행 보드

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
| Phase 5 | Runtime foundation (snapshot/update cause) | P0 | Ready | codex | tutor-step-add, student-friendly-ux | docs/ref/phase5-implementation-plan.md, docs/ref/tutor-step-plan.md, docs/ref/USER-FLOW.md | 2026-03-11 |
| Phase 5 | Track-aware step foundation | P0 | Ready | codex | tutor-step-add, student-friendly-ux | docs/ref/phase5-implementation-plan.md, docs/ref/tutor-step-plan.md, docs/ref/USER-FLOW.md | 2026-03-11 |
| Phase 5 | Joint Numeric Input + Highlight | P0 | Ready | codex | student-friendly-ux, scene-scaffold | docs/ref/phase5-implementation-plan.md, docs/ref/product/roadmap/current-feature-checklist.md | 2026-03-11 |
| Phase 5 | Visualization Helpers (trail/target) | P0 | Ready | codex | scene-scaffold, editmode-test-add | docs/ref/phase5-implementation-plan.md, docs/ref/asset-validation-report.md | 2026-03-11 |
| Phase 5 | Why It Moved explanation layer | P0 | Ready | codex | student-friendly-ux, tutor-step-add | docs/ref/phase5-implementation-plan.md, docs/ref/product/ux/guided-lesson.md | 2026-03-11 |
| Phase 5 | Beginner Lesson L0~L3 integration | P0 | Ready | codex | tutor-step-add, student-friendly-ux | docs/ref/phase5-implementation-plan.md, docs/ref/tutor-step-plan.md, docs/ref/USER-FLOW.md | 2026-03-11 |
| Phase 5 | Robot Library MVP (deferred appendix) | P1 | Hold | - | robot-template-add, student-friendly-ux | docs/ref/phase5-implementation-plan.md, docs/ref/product/ux/robot-library.md | 2026-03-11 |
| Phase 5 | Template 3DOF | P1 | Ready | - | robot-template-add | docs/ref/product/robots/robot-template-expansion.md | 2026-03-11 |
| Phase 5 | Template 6DOF | P2 | Ready | - | robot-template-add | docs/ref/product/robots/robot-template-expansion.md | 2026-03-11 |
| Phase 6 | CI/CD | P1 | InProgress | codex | pre-commit-validate | .github/workflows/unity-tests.yml | 2026-03-09 |
| Phase 7 | Documentation | P1 | Done | codex | sprint-docs-sync | AGENTS.md, docs/ref/architecture-mermaid.md | 2026-03-09 |

## Zero-Drift 규칙
1. `Assets/Scripts/` 구조를 코드 모듈 Source of Truth로 간주한다.
2. 보드의 module 집합과 `SKILL-DOC-MATRIX.md`의 target_module 집합은 동기화한다.
3. Phase 0/1의 asmdef/tests/compile/serialization 결정은 `docs.unity3d.com` 링크 근거를 필수로 남긴다.
4. Phase 4 Visualization은 `frame_0`/`frame_1`/`Frame_EE` ownership과 donor mesh source 정책을 유지한다.
5. 학습 화면 MVP는 `TopBar`/`LeftPanel`/`RightPanel`/`BottomBar` 4영역 surface를 기준으로 유지한다.
6. Phase 4 렌더 기준은 URP와 Solid Color camera를 사용한다.
7. 시작 흐름은 `Boot -> Onboarding/Main` 분기와 `LoadSceneMode.Single`을 기준으로 유지한다.
8. `Main`은 로봇/HUD 전용 씬이고, 온보딩은 `Onboarding` 씬 전용 책임으로 분리한다.
9. `Main`의 overlay root(`GlossaryPanel`, focus/highlight 계열)는 기본 inactive 상태를 유지하고, 유효한 HUD target이 있을 때만 활성화한다.
10. 루트 `AGENTS.md`와 폴더 `AGENTS.md`를 파일 탐색의 1차 진입점으로 사용하고, 전체 맥락은 `docs/ref/architecture-mermaid.md`로 먼저 파악한다.
11. `PRODUCT-ROADMAP.md`의 릴리스 게이트와 이 보드의 phase 상태는 충돌 없이 유지한다.
