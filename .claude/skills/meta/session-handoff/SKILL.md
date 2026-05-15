---
name: session-handoff
description: "다음 세션이 바로 이어서 시작할 수 있도록 entrypoint, blocker, first verify를 남기는 meta 스킬"
---

## Trigger
아래 요청/상황에서 사용:
- `다음 세션용 handoff 남겨줘`
- `여기서 끊고 다음에 이어갈 수 있게 정리해줘`
- 작업은 끝나지 않았지만 pause 해야 할 때

## Input Context
- 현재까지 끝난 것
- 아직 안 끝난 것
- next entrypoint 후보 문서/파일
- blocker와 recommended first verify

## Read First
1. `docs/status/ACTIVE-WORK-INDEX.md`
2. `docs/status/PROJECT-STATUS.md`
3. 관련 roadmap or success pattern SSOT
4. 이번 세션 diff와 검증 결과

## Do
1. 다음 세션이 가장 먼저 읽을 문서 1~3개를 고른다.
2. 현재 blocker와 그 이유를 짧게 적는다.
3. 첫 실행 단위를 `파일/화면/검증` 수준으로 구체화한다.
4. 재진입 직후 해야 할 first verify를 남긴다.
5. 필요 시 `locked scope`를 같이 적어 범위 확장을 막는다.

## Do Not
1. 배경 설명만 길게 남기고 다음 액션을 비우지 않는다.
2. archived 문서를 next entrypoint 맨 앞에 두지 않는다.
3. narrow verified scope를 broad green처럼 요약하지 않는다.

## Validation
- [ ] next entrypoint 문서가 1개 이상 있음
- [ ] blocker 또는 none이 명시됨
- [ ] next action이 구체적임
- [ ] first verify가 남음

## Output Template
```
[session-handoff]
- next entrypoint:
  - {doc 1}
  - {doc 2}
- blocker: {none|reason}
- next action: {구체 작업}
- first verify: {command or manual check}
- locked scope: {optional}
```
