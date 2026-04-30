# FR5 Connect Sync Debug Success Pattern

날짜: 2026-04-30

## 요약

- `연결 + 위치 읽기` direct debug 흐름을 별도 success pattern으로 분리했다.
- 목적은 `gripper/joint/point` 트랙으로 들어가기 전에 `connect + sync baseline`이 정말 살아 있는지 같은 방식으로 재현하는 것이다.

## 오늘 직접 확인한 사실

- `BtnConnect`는 실제로 click됐다.
- `RobotControlV3` scene 재진입 후 test했다.
- post-click `latest-state.json`은 새 session으로 갱신됐다.
- same-session `events.ndjson`에는 `latest-state updated`가 연속 기록됐다.
- 이전처럼 즉시 `Enable 버튼을 눌러주세요 -> FR5 연결 해제` 루프로 떨어지는 재현은 이번 direct click에서는 보이지 않았다.

## 오늘 코드 보강

- disabled read failure가 나오면 background poll이 계속 readback을 hammering하지 않게 했다.
- realtime state에서 `enabled` truth를 캐시에 반영하게 했다.
- SDK version 조회를 매 state record마다 다시 하지 않게 했다.
- async connect/sync 실패 문구를 snapshot blocked reason으로 유지하게 했다.

## 새 기준

- `latest-state.json` > stale header label
- `RobotControlV3` active scene 확인 > 버튼 재현
- `BtnConnect` 1회 > `latest-state` / `events` / 필요 시 `Editor.log`

## 산출물

- skill:
  - `/Users/family/.codex/skills/fr5-connect-sync-debug-success-pattern/SKILL.md`
- SSOT:
  - `/Users/family/jason/FR5UNITY/robotapp/docs/ref/product/roadmap/fr5-connect-sync-debug-success-pattern.md`
