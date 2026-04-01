# 2026-03-31 — FAIRINO 소프트 티칭패드 V1 backlog

## Summary
- 소프트 티칭패드 기획 문서를 실제 구현 순서로 바꾸는 V1 backlog 문서를 추가했다.
- V1은 제조사 패드 전체 복제가 아니라 `초보자도 안 무서운 한국어 FR5 소프트 티칭패드`를 만드는 범위로 고정했다.

## What Changed
- `docs/ref/product/roadmap/robotcontrol-soft-teaching-pad-v1-backlog.md`
  - V1 goal, in/out scope, success criteria를 정의했다.
  - `P0/P1`, `구현 묶음`, `추천 구현 순서`, `담당 경계`를 정리했다.
  - `연결 홈`, `쉬운 조작`, `팝업`, `preview`, `위험 경고`, `태블릿 1차`를 상단에 고정했다.
- `docs/status/PROJECT-STATUS.md`
  - V1 backlog 문서 추가와 범위 고정을 현재 상태에 반영했다.

## Decision Notes
- V1은 작게 잡는다.
- `Program/TPD/I/O/Gripper/Servo Live`는 V1 완료 전까지 상단 backlog로 올리지 않는다.
- `실기 핵심 + 초보자 가치 + 태블릿 사용성`이 먼저다.

## Next Suggested Step
- backlog 1번인 `RobotControl L3 정리`부터 실제 구현으로 들어가는 것이 가장 자연스럽다.
- 그 다음 묶음은 `연결 홈 재구성 -> 쉬운 조작 -> 한국어 팝업` 순서를 권장한다.
