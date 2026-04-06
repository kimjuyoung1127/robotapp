# FR5 Live Validation Plan

## Purpose

문서 기준선을 잠근 뒤, 어떤 순서로 실기 bring-up을 검증할지 정의한다.

- 문서만으로 `live-ready` 판정을 하지 않는다.
- 실기 동작은 단계별로 닫힌 루프로 검증한다.
- 실패했을 때 어떤 문서로 되돌아가야 하는지 남긴다.

## Validation States

- `doc-locked`
  - 공식 문서/SDK 근거가 확보됨
- `sim-validated`
  - mock 또는 local preview로 UX 흐름 확인
- `bench-needed`
  - 실기 테스트 대기
- `bench-verified`
  - 실기 장비에서 확인 완료
- `blocked`
  - 공식 근거 부족 또는 장비/네트워크 미확보

## Bring-Up Sequence

1. `Connect`
2. `Read actual joint/TCP`
3. `Enable`
4. `Sync current pose`
5. `Joint arrow jog`
6. `TCP jog`
7. `MoveJ preset with preview`
8. `MoveL point move with preview`
9. `Remember current pose`
10. `Save point`
11. `Run single point`
12. `Run loop`
13. `Stop`
14. `Recovery / re-sync`

## Evidence Required Per Step

| step | official evidence | local evidence | live evidence |
|---|---|---|---|
| Connect | SDK connect/disconnect | mock connect state | actual controller connection |
| Read actual pose | SDK read API | state update in V2 | actual robot pose reflected |
| Enable | SDK/manual mode note | UI state gate | real enable success/fail path |
| Sync | official sync or read strategy | state rewrite in V2 | pose overwrite behavior correct |
| Joint arrow jog | jog API/manual mode | preview and button UX | real jog response and stop safety |
| TCP jog | cartesian jog API/manual mode | preview and grid UX | real TCP response and stop safety |
| MoveJ/MoveL | movement API | preview/confirm popup | actual move path |
| Save/Run/Loop | local point or product-decision flow | waypoint loop in sim | real point sequence behavior |

## Special Checks For This Project

### Current Pose Memory

- endpoint가 조금이라도 바뀌었을 때 기본자세로 튀지 않는가
- `Sync current pose` 이후 V2 상태가 새 현재값을 기준점으로 다시 잡는가
- 저장 포인트가 `old preview target`이 아니라 `actual current pose`를 기록하는가

### Preview

- ghost target이 현재 pose와 목표 pose 차이를 명확히 보이는가
- path estimate가 move type(`MoveJ`/`MoveL`)과 맞는가
- preview risk summary가 move 전에 먼저 보이는가

### Floor Grid

- 바닥 격자가 viewport 기준선으로 유지되는가
- 로봇/고스트/경로가 격자와 함께 공간 인지에 실제 도움이 되는가

## Exit Rule

아래가 없으면 `bench-verified`로 올리지 않는다.

1. 테스트 날짜
2. 사용한 로봇/컨트롤러 버전
3. 사용한 SDK 버전
4. 성공/실패 결과
5. 재현 가능한 실패 메모
