# Pendant V3 Mac Compatibility Pass

## Date
- 2026-04-06 (KST)

## Summary
- V3 문서와 정적 체크 실행 예시를 macOS에서도 읽고 따라갈 수 있도록 플랫폼 중립 형태로 정리했다.

## Updated Docs
- `docs/ref/product/pendant-v3/unityctl-recipes.md`
- `docs/ref/product/pendant-v3/AGENT-CONTRACT.md`
- `docs/ref/product/pendant-v3/static-checks.md`

## Changes
- `powershell -ExecutionPolicy Bypass` 예시를 `pwsh -NoLogo -NoProfile` 기준으로 교체했다.
- `unityctl` 절대 경로와 Windows 사용자 경로 예시를 제거했다.
- `$unityctl`은 `UNITYCTL` 환경 변수 또는 `PATH`에서 찾도록 변경했다.
- 산출물 경로는 `Join-Path`와 상대 경로 기반으로 바꿨다.
- `workflow verify` 경로 예시도 `/` 기반 상대 경로로 정리했다.

## Validation
- `pwsh -NoLogo -NoProfile -File ./docs/ref/product/pendant-v3/check-v3-static.ps1`
- 결과: `V3 static checks passed with 0 warning group(s).`
- `docs/ref/product/pendant-v3` 범위에서 Windows 전용 경로/실행 예시 검색 결과 없음

## Remaining Limit
- 현재 세션은 Windows 환경이므로 `unityctl` 전체 레시피를 macOS Editor 대상으로 실동작 검증한 것은 아니다.
- 다만 문서, 스크립트, 경로 표기 자체는 macOS 친화적으로 정리됐다.
