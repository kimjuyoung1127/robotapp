# FR5 Status Copy SSOT

Last Updated: 2026-04-28 (KST)

## Purpose

- Pendant V3 운영자 화면의 상태 문구를 일반인 기준으로 고정한다.
- 내부 상태값, debug summary, SDK/gate raw token을 운영자 UI에 직접 노출하지 않는다.
- 새 상태 문구를 추가할 때는 이 문서를 먼저 보고 같은 표현을 재사용한다.

## Scope

우선 적용 범위:

- `Assets/Scripts/App/Fairino/RobotControlV3RuntimeController.cs`
- `Assets/Scripts/App/Fairino/RobotControlV3RuntimeSnapshot.cs`
- `Assets/Scripts/UI/RobotControlV3/*`
- `Assets/UI/PendantV3/*`

예외:

- `latest-state.json`, `latest-drift.json`, session NDJSON
- debug bridge summary
- test assertion
- audit artifact

위 예외는 evidence/debug truth라서 raw token이 남아도 된다. 운영자 UI에만 이 SSOT를 강제한다.

## Canonical Copy

| 의미 | canonical 표현 |
|---|---|
| readback-only session | `위치 확인 전용` |
| disconnected | `미연결` |
| connected before sync | `연결됨 · 위치 확인 전` |
| connected after sync | `연결됨 · 위치 확인 완료` |
| actual move blocked | `실제 이동: 잠겨 있음` |
| actual move allowed | `실제 이동: 가능` |
| coord base | `좌표 기준: 로봇 기준` |
| coord tool | `좌표 기준: 툴 기준` |
| coord user | `좌표 기준: 작업 기준` |
| tool id known | `도구 설정: N번` |
| tool id unknown | `도구 설정: 미확인` |
| user id known | `작업 기준: N번` |
| user id unknown | `작업 기준: 미확인` |
| current position unread | `현재 위치 읽음: 아직 안 함` |
| current position read complete | `현재 위치 읽음: 완료` |

## Locked Exposure Set

운영자가 항상 바로 읽어야 하는 최소 노출 항목:

- 연결 상태
- 현재 위치 읽음 완료 여부
- 도구 설정 번호
- 작업 기준 번호
- 좌표 기준
- 실제 이동 가능/잠김
- 잠금 이유
- 실시간 추적 상태

## Banned Raw Tokens

운영자 UI에는 아래 예시를 직접 쓰지 않는다.

- `ReadbackOnly`
- `ready=False`
- `coordSystem=Base`
- `tool=1`
- `user=1`
- `dry-run simulation`

## Review Loop

1. 상태 문구를 바꾸기 전에 이 문서를 먼저 본다.
2. 가능하면 `RobotControlV3OperatorStatusCopy` 같은 중앙 formatter에서만 문구를 바꾼다.
3. 변경 후 `scripts/tests/check_status_copy_tokens.sh`를 돌려 금지 토큰이 UI 경로에 남지 않았는지 본다.
4. live session 작업이면 `.claude/commands/live-gate-review.md`도 같이 본다.
