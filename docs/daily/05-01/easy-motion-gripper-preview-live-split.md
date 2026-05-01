# 2026-05-01 Easy Motion Gripper Preview/Live Split

## Summary

- `Easy Motion` gripper 조작부에서 quick button과 실행 버튼 의미를 더 분리했다.
- quick button은 현재 `100 / 50 / 0` value selector로만 쓴다.
- 실행은 `미리보기 적용`과 `실제 이동` 두 경로로 나눈다.

## What Changed

- `Easy Motion` gripper draft/requested/actual 표시를 더 분리했다.
- mock / dryRun / readback-only 상태에서 quick button 또는 input 값이 곧바로 live 성공처럼 보이지 않게 wording을 조정했다.
- `Joint/TCP` apply는 preview와 더 분리했고, `PointMove` dryRun/preview 문구도 live run처럼 읽히지 않게 정리했다.
- status surface에서는 `프리뷰 / 읽기 전용 / live write 가능` 표현을 더 직접적으로 쓰기 시작했다.

## Verified This Turn

- Unity compile check passed.
- mock-only debug에서 `Easy Motion` input `80`은 즉시 `100`으로 회귀하지 않고 `draft=80; pending=True`로 유지됐다.
- 이번 턴은 실기 재검증이 아니라 mock-only 의미 분리와 compile 확인까지다.

## Not Yet Verified

- `미리보기 적용`과 `실제 이동` 2버튼 분리 semantics의 fresh field smoke
- `100 / 50 / 0` quick selection 기준 실기 operator flow 재검증
- slider continuous follow

## Next

1. fresh live session에서 `100 / 50 / 0 -> 실제 이동` discrete smoke 재검증
2. `미리보기 적용`이 실기 write 없이 preview만 바꾸는지 field에서도 다시 확인
3. 그 다음에만 slider follow cadence를 다시 연다
