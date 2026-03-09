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
| Phase 4 | Visualization | P1 | InProgress | codex | scene-scaffold, editmode-test-add | docs/ref/coordinate-mapping.md, docs/ref/architecture-diagrams.md | 2026-03-09 |
| Phase 4 | Validator | P1 | Ready | - | editmode-test-add | docs/ref/test-reference-values.md | 2026-03-05 |
| Phase 5 | Template 3DOF | P1 | Ready | - | robot-template-add | - | 2026-03-05 |
| Phase 5 | Template 6DOF | P2 | Ready | - | robot-template-add | - | 2026-03-05 |
| Phase 6 | CI/CD | P1 | InProgress | codex | pre-commit-validate | .github/workflows/unity-tests.yml | 2026-03-09 |
| Phase 7 | Documentation | P1 | Ready | - | sprint-docs-sync | - | 2026-03-05 |

## Zero-Drift 규칙
1. `Assets/Scripts/` 구조를 코드 모듈 Source of Truth로 간주한다.
2. 보드의 module 집합과 `SKILL-DOC-MATRIX.md`의 target_module 집합은 동기화한다.
3. Phase 0/1의 asmdef/tests/compile/serialization 결정은 `docs.unity3d.com` 링크 근거를 필수로 남긴다.
4. Phase 4 Visualization은 `frame_0`/`frame_1`/`Frame_EE` ownership과 donor mesh source 정책을 유지한다.
5. 학습 화면 MVP는 `TopBar`/`LeftPanel`/`RightPanel`/`BottomBar` 4영역 surface를 기준으로 유지한다.
