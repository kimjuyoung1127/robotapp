---
name: pre-commit-validate
description: "커밋 전 검증 — 커밋, pre-commit, 검증, validate, 빌드 체크"
---

## Trigger
커밋 요청 시, 또는 커밋 전 검증이 필요할 때.

## Input Context
- 변경된 파일 목록
- 변경 유형 (수학/기구학/UI/문서 등)

## Read First
1. `CLAUDE.md` — 테스트 정책 및 규칙
2. `docs/status/PHASE-EXECUTION-BOARD.md` — 현재 모듈 상태

## Do (엄격한 순서)
1. **Unity 컴파일 확인**: 프로젝트 컴파일 에러 0
2. **EditMode 테스트 실행**: Test Runner > EditMode > Run All — 전체 통과 필수
3. **PlayMode 테스트 실행**: 존재하는 경우 전체 통과 필수
4. **수치 허용 오차 검증**: 수학/기구학 변경 시 참조값 대비 검증
5. **XML doc summary 확인**: 새 C# 파일에 한국어 설명 존재
6. **BOARD 상태 확인**: PHASE-EXECUTION-BOARD.md가 현재 구현 상태 반영

## Do Not
1. 테스트 실패 상태로 커밋 진행 금지
2. 검증 단계 건너뛰기 금지
3. `--no-verify` 플래그 사용 금지

## Validation
- [ ] Unity 컴파일: 에러 0
- [ ] EditMode 테스트: 전체 통과
- [ ] PlayMode 테스트: 전체 통과 (또는 N/A)
- [ ] 수치 허용 오차: 검증됨 (또는 N/A)
- [ ] XML doc summary: 새 파일에 존재
- [ ] BOARD 상태: 최신 상태

## Output Template
```
[pre-commit-validate 완료]
- Unity 컴파일: 통과
- EditMode: {n}/{n} 통과
- PlayMode: {n}/{n} 통과 (또는 N/A)
- 수치 검증: 통과 (또는 N/A)
- XML doc: 확인
- BOARD: 최신 상태
- 커밋 준비: 완료
```
