---
title: "KineTutor3D Docs Agent Hub"
doc_type: "agent-hub"
status: "active"
domain: "docs"
audience: "agent"
canonical: true
last_updated: "2026-04-30"
---

# docs/

KineTutor3D 문서 인덱스.

## 구조

- `docs/status/` — 지금 판단할 active 보드와 운영 상태
- `docs/ref/` — 장기 스펙과 기준 문서
- `docs/daily/` — 일일 실행 로그와 증빙
- `docs/weekly/` — 주간 롤업
- `docs/archive/` — 완료되었거나 현재 앞줄에서 빼도 되는 문서

## 지금 읽기 순서

1. `docs/status/ACTIVE-WORK-INDEX.md`
2. `docs/status/PROJECT-STATUS.md`
3. `docs/status/FR5-LIVE-INTEGRATION-ROADMAP.md`
4. `docs/ref/product/ux/robotcontrol-next-session-handoff.md`
5. `docs/ref/product/roadmap/fr5-live-field-checklist.md`
6. `docs/ref/product/roadmap/fr5-gripper-live-success-pattern.md`
7. `docs/ref/product/roadmap/fr5-tiny-joint-live-success-pattern.md`
8. `docs/ref/product/pendant-v3/progress-checklist.md`
9. 과거 현장 서사가 필요할 때만 `docs/ref/product/roadmap/fr5-live-field-history.md`
10. 필요 시 `docs/status/PRODUCT-DOC-BOARD.md`
11. 필요 시 `docs/status/PHASE-EXECUTION-BOARD.md`

## 규칙

- active 판단은 먼저 `status/ACTIVE-WORK-INDEX.md`에서 한다.
- 완료된 큰 보드는 `docs/archive/`에 보관하고, 현재 경로에는 짧은 안내만 남길 수 있다.
- 자동화를 위해 status 파일명은 가능한 유지한다.
- 새로운 장기 스펙은 `docs/ref/`에만 추가한다.
- 제품 문서 3종은 `docs/ref/` 외 경로에 복제하지 않는다.
- 제품 문서 상태는 `docs/status/PRODUCT-DOC-BOARD.md`에서만 관리한다.
- FR5 live current truth는 `handoff -> field-checklist -> success-pattern` 순서로 읽고, history 문서는 필요할 때만 뒤에서 본다.
- `fr5-gripper-live-success-pattern.md`는 gripper live SSOT다. 다른 문서에는 상세 절차를 반복하지 말고 링크만 둔다.
- `fr5-tiny-joint-live-success-pattern.md`는 tiny joint live SSOT다. narrow verified scope는 이 문서만 기준으로 본다.
- `fr5-live-field-checklist.md`는 현재 세션 체크리스트만 유지한다. 과거 현장 서사와 시행착오는 `fr5-live-field-history.md`에 둔다.
- `V1`, `V2`는 active 운영 문서가 아니다. 남겨둘 것은 `개발 목적`과 `이력`뿐이며, 현재판 판단 근거로 재사용하지 않는다.
