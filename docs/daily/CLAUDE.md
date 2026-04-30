---
title: "Daily Logs Agent Guide"
doc_type: "agent-hub"
status: "active"
domain: "daily"
audience: "agent"
canonical: true
last_updated: "2026-04-29"
---

# docs/daily/

일일 실행 로그. `docs-nightly-organizer` 자동화의 입력.

## 파일명 컨벤션
- 폴더: `MM-DD/`
- 파일: `module-{모듈슬러그}.md`
- 예시: `03-05/module-math-vec3d.md`

## 규칙
- 작업 완료 시 해당 날짜 폴더에 로그 작성.
- `docs-nightly-organizer`가 weekly 롤업으로 집계.
