---
name: change-class-doc-sync
trigger: 코드 변경 후 어떤 문서를 같이 고쳐야 할지 빠르게 분류해야 할 때
status: starter
---

# Change Class Doc Sync

## Change Classes

| Class | Example | Must Check |
|---|---|---|
| runtime/live | FR5 client, sync, poll, mode, motion gate | `ACTIVE-WORK-INDEX`, 관련 success pattern, roadmap |
| ui/surface | UXML, USS, operator copy, panel behavior | `PROJECT-STATUS`, 관련 UX ref, copy SSOT |
| scene/flow | Boot, Onboarding, RobotLibrary, routing | `architecture-mermaid`, `project-flow-code-review`, `PROJECT-STATUS` |
| tests/qa | EditMode, PlayMode, QA runner, artifacts | `SKILL-DOC-MATRIX`, `PROJECT-STATUS` |
| ops/docs | command, harness, automation, validation script | `harness/REGISTRY`, `SKILL-DOC-MATRIX`, 관련 README |

## Verify

- diff의 파일이 최소 1개 class에 분류됨
- class별 companion docs를 실제로 확인함
- 구조적 규칙 변경이면 `harness/REGISTRY.md` 또는 관련 인덱스를 같이 갱신함
