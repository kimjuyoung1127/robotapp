---
name: debug-success-capture
description: "디버깅 성공 결과를 스킬/문서/테스트 자산으로 고정하는 운영 스킬. 디버깅 성공시 스킬화, 원인-조치-검증 기록, 재발 방지 체크리스트가 필요할 때 사용"
---

## Trigger
아래 요청/상황에서 사용:
- `디버깅 성공시 스킬화`
- `원인/조치/검증 정리`
- `재발 방지`
- `이번 수정을 운영 규칙으로 남겨줘`

## Input Context
- 이슈 증상(로그/에러 메시지)
- 최종 원인
- 적용한 수정 파일 목록
- 검증 결과(테스트/콘솔/수동 검증)

## Read First
1. `references/playmode-debug-checklist.md`
2. `references/known-fixes.md`
3. `docs/status/PROJECT-STATUS.md`
4. `docs/status/PHASE-EXECUTION-BOARD.md`
5. `docs/status/SKILL-DOC-MATRIX.md`

## Do
1. 디버깅 성공 결과를 `원인 -> 조치 -> 검증 -> 재발 방지` 4단계로 정리한다.
2. 반복 가능성이 높은 해결 절차는 스킬 규칙으로 승격한다.
3. 테스트 자산(PlayMode/EditMode)으로 회귀 검증 경로를 남긴다.
4. 문서 3종을 동기화한다:
   - `PROJECT-STATUS`
   - `PHASE-EXECUTION-BOARD`
   - `SKILL-DOC-MATRIX`
5. 콘솔 에러 0(프로젝트 코드 기준)과 테스트 결과를 명시한다.

## Do Not
1. 원인 추정 상태를 확정 사실처럼 기록하지 않는다.
2. 검증 없이 `Done` 상태로 올리지 않는다.
3. 수동 절차만 남기고 자동/반자동 검증 경로를 생략하지 않는다.

## Validation
- [ ] 원인/조치/검증/재발방지 4항목이 모두 기록됨
- [ ] 수정 파일 경로가 명시됨
- [ ] PlayMode 또는 EditMode 검증 결과가 기록됨
- [ ] `PROJECT-STATUS` / `PHASE-EXECUTION-BOARD` / `SKILL-DOC-MATRIX` 동기화됨
- [ ] 다음 실패 시 바로 재현 가능한 체크리스트가 남음

## Output Template
```
[debug-success-capture 완료]
- 증상: {요약}
- 원인: {확정 원인}
- 조치: {적용한 수정}
- 검증:
  - Console: {에러 0 / 이슈}
  - Tests: {PlayMode/EditMode 결과}
- 재발 방지:
  - {체크리스트 항목 1}
  - {체크리스트 항목 2}
- 동기화 문서:
  - docs/status/PROJECT-STATUS.md
  - docs/status/PHASE-EXECUTION-BOARD.md
  - docs/status/SKILL-DOC-MATRIX.md
```
