# FR5 Header Mode Switch Live Smoke

Date: 2026-04-29 (KST)

## Goal

- `Pendant V3` 상단 헤더 `수동` / `자동` 버튼이 기존 teach pendant 없이 normal case mode 전환을 실제로 닫는지 확인한다.

## What Was Verified

- external teach pendant 없이 app-owned mode path를 사용했다
- direct readback client는 여전히 motion/IO/gripper는 막지만, current branch에서는 `SetMode / ExitDragTeach / EnsureAutoMode`는 inner live client로 통과시킨다
- header button handler path 기준 결과:
  - initial `latest-state.json mode=0`
  - header `수동` -> `latest-state.json mode=1`
  - header `자동` -> `latest-state.json mode=0`

## Smoke Shape

- RobotControlV3 runtime initialize
- live connect + sync
- header `수동` handler path invoke
- `latest-state.json` refresh
- header `자동` handler path invoke
- `latest-state.json` refresh

## Result

- `mode=0 -> 1 -> 0` 왕복 성공
- 이로써 current branch `auto/manual` normal case는 `header operator surface green`으로 본다

## Remaining Work

- exceptional case에서 왜 실패했는지 operator wording 정리
- `drag-teach`, `servo`, `fault`, `network` taxonomy를 회복 안내와 함께 surface에 노출
