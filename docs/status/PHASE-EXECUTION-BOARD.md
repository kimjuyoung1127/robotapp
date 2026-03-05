# Phase 실행 보드

Phase/모듈별 실행 상태의 Source of Truth.

## 상태 흐름
`Ready → InProgress → QA → Done` (Hold = 차단됨)

| phase | module | priority | status | owner | skills_required | must_read_docs | last_updated |
|-------|--------|----------|--------|-------|----------------|----------------|--------------|
| Phase 0 | Foundation | P0 | InProgress | claude | unity-official-docs, asmdef-setup | docs/status/PROJECT-STATUS.md, .claude/skills/kinetutor-guide/ops/unity-official-docs/references/phase01-core.md | 2026-03-05 |
| Phase 1 | Types | P0 | Ready | - | math-module-add, unity-official-docs | docs/ref/dh-reference.md, .claude/skills/kinetutor-guide/ops/unity-official-docs/references/index.md | 2026-03-05 |
| Phase 1 | Math | P0 | Ready | - | math-module-add, editmode-test-add, unity-official-docs | docs/ref/dh-reference.md, .claude/skills/kinetutor-guide/ops/unity-official-docs/references/index.md | 2026-03-05 |
| Phase 2 | DH Standard | P0 | Ready | - | dh-algorithm-add, editmode-test-add | docs/ref/dh-reference.md, docs/ref/coordinate-mapping.md | 2026-03-05 |
| Phase 2 | FK Engine | P0 | Ready | - | dh-algorithm-add | docs/ref/test-reference-values.md | 2026-03-05 |
| Phase 3 | UI (DHTable/Sliders/Tutor) | P0 | Ready | - | tutor-step-add | docs/ref/architecture-diagrams.md | 2026-03-05 |
| Phase 3 | Template 2DOF | P0 | Ready | - | robot-template-add | docs/ref/test-reference-values.md | 2026-03-05 |
| Phase 4 | Visualization | P1 | Ready | - | - | docs/ref/coordinate-mapping.md | 2026-03-05 |
| Phase 4 | Validator | P1 | Ready | - | editmode-test-add | docs/ref/test-reference-values.md | 2026-03-05 |
| Phase 5 | Template 3DOF | P1 | Ready | - | robot-template-add | - | 2026-03-05 |
| Phase 5 | Template 6DOF | P2 | Ready | - | robot-template-add | - | 2026-03-05 |
| Phase 6 | CI/CD | P1 | Ready | - | pre-commit-validate | - | 2026-03-05 |
| Phase 7 | Documentation | P1 | Ready | - | sprint-docs-sync | - | 2026-03-05 |

## 모듈 의존성
```
Types → Math → Kinematics (DH Standard + FK Engine)
                    ↓
              Templates (2DOF → 3DOF → 6DOF)
                    ↓
            UI + Visualization
                    ↓
               Validator
```

## Zero-Drift 규칙
- `Assets/Scripts/` 디렉토리 구조 = 관리 모듈의 Source of Truth
- 이 보드의 module 열 = `SKILL-DOC-MATRIX.md`의 target_module 열과 동일해야 함
- 드리프트 감지: `code-doc-align` 자동화가 야간 확인
- Phase 0/1의 asmdef/tests/compile/serialization 결정은 `docs.unity3d.com` 링크 근거 필수
