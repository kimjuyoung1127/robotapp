---
name: evidence-review
description: "완료 선언 전 실제로 실행한 검증, 바뀐 문서, 남은 리스크를 정리하는 meta 스킬"
---

## Trigger
아래 요청/상황에서 사용:
- `마무리 전에 검증 정리해줘`
- `이제 완료라고 해도 되는지 봐줘`
- `verify랑 남은 리스크만 짧게 묶어줘`

## Input Context
- 이번 세션 수정 파일
- 실제 실행한 검증 명령
- 갱신한 문서 목록
- 남은 리스크 또는 미실행 검증

## Read First
1. `docs/status/PROJECT-STATUS.md`
2. `docs/status/ACTIVE-WORK-INDEX.md`
3. 관련 스킬/하네스 문서
4. 이번 세션 diff와 테스트 로그

## Do
1. 실제로 실행한 verify만 적는다.
2. 변경된 문서와 그 이유를 적는다.
3. 미실행 검증이나 live field residual risk를 숨기지 않고 적는다.
4. `not-ready`, `ready-for-review`, `ready-to-share` 중 release verdict를 고른다.
5. 필요 시 다음 세션 첫 검증도 같이 남긴다.

## Do Not
1. 실행하지 않은 테스트를 실행한 것처럼 적지 않는다.
2. 문서 변경을 빼고 완료 선언하지 않는다.
3. FR5 live 관련 risk를 일반 UI risk처럼 축소하지 않는다.

## Validation
- [ ] 실행 verify가 1개 이상 기록됨
- [ ] 변경 문서가 1개 이상 기록되거나 문서 변경 없음이 명시됨
- [ ] 남은 리스크 또는 미실행 검증이 명시됨
- [ ] release verdict가 선택됨

## Output Template
```
[evidence-review]
- verify:
  - {command/result}
- docs changed:
  - {path}
- residual risk:
  - {risk}
- release verdict: {not-ready|ready-for-review|ready-to-share}
- next verify: {optional}
```
