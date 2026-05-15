---
name: task-intake-router
description: "새 요청을 구현, 검증, 문서 동기화, 리뷰, 영향 범위 분석 중 어디에 가까운지 먼저 분류하는 meta 스킬"
---

## Trigger
아래 요청/상황에서 사용:
- `이 요청부터 정리해줘`
- `어디서 시작할지 모르겠어`
- `먼저 읽을 문서와 스킬을 골라줘`
- 큰 요청인데 바로 손대기 전에 분류가 필요할 때

## Input Context
- 사용자 요청 원문
- 현재 작업 브랜치/변경 여부
- 관련 코드 또는 문서 경로

## Read First
1. `AGENTS.md`
2. `CLAUDE.md`
3. `harness/REGISTRY.md`
4. `docs/status/PROJECT-STATUS.md`
5. 필요 시 `docs/status/ACTIVE-WORK-INDEX.md`

## Do
1. 요청을 `implement`, `verify`, `doc-sync`, `review`, `impact-map`, `handoff` 중 하나 이상으로 분류한다.
2. 지금 바로 읽어야 할 문서 1~3개를 고른다.
3. 지금 세션에서 쓸 스킬 1~2개만 선택한다.
4. 필요한 경우 먼저 끝내야 할 검증이나 blocker를 명시한다.
5. 관련된 command가 있으면 `/intake`, `/impact-map`, `/handoff` 같은 형태로 같이 안내한다.

## Do Not
1. 관련 없는 스킬을 한 번에 많이 고르지 않는다.
2. 분류만 하고 다음 실행 단위를 비워두지 않는다.
3. FR5 live 작업에서 현재 truth 문서를 건너뛰지 않는다.

## Validation
- [ ] 요청 분류가 최소 1개 이상 나옴
- [ ] 다음에 읽을 문서가 1개 이상 지정됨
- [ ] 이번 세션 추천 스킬이 1~2개로 좁혀짐
- [ ] blocker 또는 first action이 명시됨

## Output Template
```
[task-intake-router]
- verdict: {implement|verify|doc-sync|review|impact-map|handoff}
- read first:
  - {doc 1}
  - {doc 2}
- chosen skill:
  - {skill 1}
  - {skill 2}
- first action: {바로 할 일}
- blocker: {없으면 none}
```
