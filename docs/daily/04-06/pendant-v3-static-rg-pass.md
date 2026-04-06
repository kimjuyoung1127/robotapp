# Pendant V3 Static rg Pass

## Date
- 2026-04-06 (KST)

## Summary
- `static-checks.md`를 `rg` 기반 실제 게이트 문서로 확장했다.
- 문서뿐 아니라 바로 실행 가능한 `check-v3-static.ps1`를 추가했고, 실행 확인까지 마쳤다.

## Updated Docs
- `docs/ref/product/pendant-v3/static-checks.md`
- `docs/ref/product/pendant-v3/check-v3-static.ps1`
- `docs/ref/product/pendant-v3/AGENT-CONTRACT.md`

## What Changed
- `Blocker / Warning` 등급을 문서화했다.
- 빠른 게이트 명령, 확장 `rg` 점검, allowlist 정책, future automation hook을 추가했다.
- 에이전트 계약 문서에 실제 실행 명령을 연결했다.

## Validation
- `powershell -ExecutionPolicy Bypass -File .\docs\ref\product\pendant-v3\check-v3-static.ps1`
- 결과: `V3 static checks passed with 0 warning group(s).`
