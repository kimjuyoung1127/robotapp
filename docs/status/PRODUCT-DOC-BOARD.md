# Product Doc Board

Source of truth for canonical product-document status.

| doc_id | label | category | priority | status | owner | source_docs | downstream_sync | last_updated |
|---|---|---|---|---|---|---|---|---|
| `prd` | PRD | Product Strategy | P0 | QA | codex | `docs/ref/architecture-mermaid.md`, `docs/status/PROJECT-STATUS.md`, `docs/ref/asset-registry.md` | `docs/status/PROJECT-STATUS.md`, `ai-context/project-context.md`, `ai-context/master-plan.md` | 2026-03-12 |
| `wireframe` | Wireframe | Product UX | P0 | QA | codex | `docs/ref/USER-FLOW.md`, `docs/ref/tutor-step-plan.md`, `docs/ref/architecture-diagrams.md` | `docs/ref/USER-FLOW.md`, `docs/ref/tutor-step-plan.md`, `docs/ref/architecture-diagrams.md` | 2026-03-12 |
| `product-roadmap` | Product Roadmap | Planning | P0 | QA | codex | `docs/status/PROJECT-STATUS.md`, `docs/status/PHASE-EXECUTION-BOARD.md`, `ai-context/master-plan.md` | `docs/status/PROJECT-STATUS.md`, `docs/status/PHASE-EXECUTION-BOARD.md`, `ai-context/master-plan.md` | 2026-03-12 |

## Status Flow

`Ready -> InProgress -> QA -> Done` (`Hold` for blocked work).

## Rules

1. Canonical product docs live only in `docs/ref/PRD.md`, `docs/ref/WIREFRAME.md`, and `docs/ref/PRODUCT-ROADMAP.md`.
2. Branch docs live under `docs/ref/product/` and are not tracked directly in this board.
3. Product doc status is tracked only in this board.
4. Product doc changes must sync downstream docs on the same turn and leave a `docs/daily/MM-DD/` log entry.
5. Milestone-level product doc changes should also update the current weekly rollup.
6. Root docs must not move to `QA` or `Done` until their downstream sync is complete.
