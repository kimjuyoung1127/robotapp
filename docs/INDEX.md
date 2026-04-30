---
title: "KineTutor3D Docs Index"
doc_type: "index"
status: "active"
domain: "docs"
audience: "human-and-agent"
canonical: true
last_updated: "2026-04-29"
---

# docs/ INDEX

KineTutor3D 문서의 공용 루트 인덱스다.
에이전트와 사람이 같은 출발점에서 `status`, `ref`, `daily`, `archive`를 빠르게 구분하도록 만든다.

## 먼저 보는 문서

1. [README.md](./README.md)
2. [status/ACTIVE-WORK-INDEX.md](./status/ACTIVE-WORK-INDEX.md)
3. [ref/README.md](./ref/README.md)
4. [daily/README.md](./daily/README.md)

## 폴더 역할

- `status/`: 지금 판단해야 하는 active 보드, 운영 상태, 자동화 health
- `ref/`: 장기 기준 문서, 제품/아키텍처/로드맵 SSOT
- `daily/`: 날짜별 실행 로그와 증빙
- `weekly/`: 주간 롤업
- `archive/`: 완료되었거나 현재 1차 탐색 대상이 아닌 문서
- `benchmark/`: 도구/워크플로우 벤치마크와 측정 스크립트
- `templates/`: 재사용 가능한 문서 템플릿

## 읽기 규칙

- 현재 우선순위 판단은 항상 `status/ACTIVE-WORK-INDEX.md`를 먼저 본다.
- 장기 스펙 판단은 `ref/`를 본다.
- `daily/`는 실행 기록이며, 현재 정책 SSOT로 바로 쓰지 않는다.
- `archive/`는 historical context가 필요할 때만 본다.

## Frontmatter Standard

허브 문서는 아래 키를 공통으로 사용한다.

- `title`
- `doc_type`
- `status`
- `domain`
- `audience`
- `canonical`
- `last_updated`

권장 추가 키:

- `owner`
- `source_of_truth`
- `supersedes`

