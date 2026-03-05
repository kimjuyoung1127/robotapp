---
name: unity-official-docs
description: "Unity 공식 문서 근거 기반 의사결정 스킬 — unity 공식문서, asmdef, test runner, serialization, script compilation, API 호환성 관련 결정 시 사용"
---

## Trigger
아래 키워드가 포함된 작업에서 반드시 사용:
- `unity 공식문서`
- `asmdef`
- `test runner`
- `serialization`
- `script compilation`
- `API 호환성`

## Input Context
- Unity 버전 (우선: `6000.0.64f1`, 보조: `2022.3 LTS`)
- 작업영역 (`asmdef` / `tests` / `compile` / `serialization`)
- 결정 대상 (예: asmdef 필드, 테스트 설정, 직렬화 제약)

## Read First
1. `references/index.md` — 주제별 공식 문서 링크 맵
2. `references/phase01-core.md` — Phase 0+1 핵심 적용 요약
3. `CLAUDE.md` — 프로젝트 전역 규칙
4. `docs/status/PROJECT-STATUS.md` — 현재 Phase 기준

## Do (엄격한 순서)
1. 입력 컨텍스트에서 결정 대상을 1줄로 명확화
2. `references/index.md`에서 해당 주제의 공식 링크 1개 이상 선택
3. 링크 도메인이 `docs.unity3d.com`인지 확인
4. 아래 출력 포맷으로 결론 작성:
   - `결론`
   - `공식 문서 근거(링크)`
   - `프로젝트 적용 규칙`
   - `버전 차이 메모(필요 시)`
5. 결정사항을 적용할 대상 스킬/문서(예: `asmdef-setup`, `pre-commit-validate`)에 반영

## Do Not
1. `docs.unity3d.com` 외 출처(포럼, 블로그, Q/A 사이트) 인용 금지
2. 공식 링크 없는 규칙 추가 금지
3. 버전 혼용 시 차이 메모 생략 금지

## Validation
- [ ] 공식 링크 1개 이상 포함
- [ ] 링크 도메인 `docs.unity3d.com` 확인
- [ ] 결론-근거 불일치 없음
- [ ] 프로젝트 적용 규칙이 실행 가능한 문장으로 작성됨
- [ ] 버전 차이가 있으면 명시됨

## Output Template
```
[unity-official-docs 결정]
- 결정 대상: {대상}
- 결론: {결론}
- 공식 문서 근거(링크):
  - {https://docs.unity3d.com/...}
- 프로젝트 적용 규칙:
  - {규칙 1}
  - {규칙 2}
- 버전 차이 메모: {없음 | 6000.0 vs 2022.3 차이}
```
