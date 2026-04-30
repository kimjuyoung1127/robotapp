---
title: "Status Docs Agent Guide"
doc_type: "agent-hub"
status: "active"
domain: "status"
audience: "agent"
canonical: true
last_updated: "2026-04-29"
---

# docs/status/

운영 상태 보드 및 자동화 아티팩트.

## 규칙

- 파일명은 가능하면 유지한다. 외부 자동화가 이 파일명에 의존한다.
- long history는 `docs/archive/completed/status/`로 보내고, 현재 경로에는 짧은 front file을 둘 수 있다.
- active 우선순위는 먼저 `ACTIVE-WORK-INDEX.md`에서 본다.
- 제품 문서 상태는 `PRODUCT-DOC-BOARD.md`에서만 관리한다.
- NDJSON 파일은 append-only로 유지한다.
