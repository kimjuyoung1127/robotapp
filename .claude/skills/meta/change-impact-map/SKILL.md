---
name: change-impact-map
description: "코드 변경 전에 영향 범위와 companion docs, verify matrix를 좁히는 meta 스킬"
---

## Trigger
아래 요청/상황에서 사용:
- `어디까지 영향 가는지 봐줘`
- `관련 문서/테스트도 같이 찾아줘`
- cross-module 변경 전 범위를 좁히고 싶을 때

## Input Context
- 바꾸려는 파일 또는 기능 표면
- 관련 로그/버그/요구사항
- known verification commands

## Read First
1. `AGENTS.md`
2. `CLAUDE.md`
3. `docs/ref/architecture-mermaid.md`
4. 관련 폴더의 `AGENTS.md` 또는 `CLAUDE.md`
5. `docs/status/SKILL-DOC-MATRIX.md`

## Do
1. 변경 중심 파일을 기준으로 `코드`, `문서`, `검증`, `운영 리스크` 4축으로 영향을 나눈다.
2. companion docs를 최소 집합으로 고른다.
3. 필요한 검증을 `compile`, `test`, `play/live`, `manual evidence`로 정리한다.
4. 변경이 FR5 live truth, V3 UI copy, or scene flow에 닿는지 별도로 표시한다.
5. 결과를 짧은 실행 계약처럼 남긴다.

## Do Not
1. 관련 없는 폴더 전체를 영향 범위로 부풀리지 않는다.
2. 문서만 찾고 검증 명령을 비워두지 않는다.
3. live motion risk가 있는 변경에서 evidence 경로를 생략하지 않는다.

## Validation
- [ ] 중심 변경 파일 또는 기능 표면이 명시됨
- [ ] companion docs가 1개 이상 지정됨
- [ ] 검증 명령 또는 수동 검증 경로가 1개 이상 지정됨
- [ ] live risk 또는 UI/operator risk가 있으면 따로 표시됨

## Output Template
```
[change-impact-map]
- center: {file or feature}
- code companions:
  - {path}
- doc companions:
  - {path}
- verify:
  - {compile/test/play/manual}
- special risk:
  - {live truth | operator copy | scene flow | none}
```
