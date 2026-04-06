# Pendant V3 Agent Contract

## Date
- 2026-04-06 (KST)

## Summary
- V3 작업의 운영 강제력을 높이기 위해 `AGENT-CONTRACT.md`, `static-checks.md`, `verify-v3.json`을 추가했다.
- 목적은 문서를 "읽으면 좋음" 수준에서 "이 순서와 검증을 따라야 함" 수준으로 끌어올리는 것이다.

## Updated Docs
- `docs/ref/product/pendant-v3/AGENT-CONTRACT.md`
- `docs/ref/product/pendant-v3/static-checks.md`
- `docs/ref/product/pendant-v3/verify-v3.json`
- `docs/ref/product/pendant-v3/README.md`
- `docs/ref/product/pendant-v3/implementation-plan.md`
- `docs/ref/product/pendant-v3/unityctl-recipes.md`

## Decisions Added
- 에이전트는 `README -> AGENT-CONTRACT -> implementation-plan -> unityctl-recipes -> feature` 순으로 읽고 시작한다.
- 실행 단위 범위 밖 수정 금지, 종료 조건 미충족 상태의 완료 주장 금지.
- `workflow verify` 기본 번들 파일을 `verify-v3.json`으로 고정.
- 정적 체크 기준을 별도 문서로 분리해 수동/자동 점검 기준을 만들었다.
