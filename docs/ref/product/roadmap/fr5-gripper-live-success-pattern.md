# FR5 Gripper Live Success Pattern

Last Updated: 2026-05-01 (KST)

## Purpose

이 문서는 `/Users/family/jason/FR5UNITY/robotapp`에서 `Unity -> 실기 gripper` live 조작이 실제로 먹었던 조건만 고정한다.

목표는 두 가지다.

- 다음 세션에서 같은 성공 패턴을 다시 밟게 한다.
- `joint/tcp motion`과 `gripper live write`를 절대 섞지 않게 한다.

## Locked Truth

- 현재 대상은 `gripper-only` live control이다.
- arm `tiny MoveJ`와 gripper live는 같은 세션으로 열지 않는다.
- operator baseline flow는 `연결 + 위치 읽기` 1단계다.
- operator write flow는 `값 선택 -> 미리보기 적용 또는 실제 이동` 2버튼 분리다.
- current truth source는 FR5 controller readback이다.
- user percent와 SDK raw percent는 아직 `1:1`이 아니다.
- current branch calibration baseline은 `user 0% -> raw 0`이다.

## Known-Good Field Conditions

### Robot / Gripper

- 제조업체: `DAHUAN`
- 유형: `PGI-140`
- 소프트웨어 버전: `D1.0`
- 마운트 위치: `말단 1번 포트`

### Network

- FR5 `eth0 = 192.168.57.2`
- MacBook Ethernet = `192.168.57.10/24`
- current live baseline uses `192.168.57.2:8080`

### Session / Flags

- default session: `readback-only`
- write session: `gripper-only`
- required live smoke flag: `FAIRINO_ENABLE_LIVE_GRIPPER_SMOKE=1`

## Must-Pass Order

1. Unity 재기동 뒤 `RobotControlV3`로 진입
2. `연결 + 위치 읽기`
3. live evidence 확인
4. gripper readback 재확인
5. `Easy Motion`에서 목표 퍼센트 입력 또는 preset 선택
6. `미리보기 적용` 또는 `실제 이동` 중 하나를 명시적으로 고른다
7. `실제 이동`을 고른 경우에만 `이동 실행 확인` popup confirm
8. `gripper-only` 세션 write 1회
9. readback 재확인
10. 자동 `readback-only` 복귀 확인

## Readback Baseline

다음 값이 먼저 보이면 시작점으로 본다.

- `clientMode=direct`
- `toolId=1`
- `userId=1`
- `coordSystem=Base`
- `sdkConfig matchesExpected=True`
- `activationFault=0`
- `positionFault=0`

`motionFault=1`이 남아 있어도, 실제 위치 readback이 바뀌면 discrete gripper live write 자체는 먹을 수 있다.

## Known Successful Patterns

### Connect / Sync

- `BtnConnect` 1회
- expected result:
  - `status=ConnectedServoOff`
  - feedback = `[Sync] 현재 자세 동기화 완료`

### Gripper Open Baseline

- `open 100%`
- final readback = `position=100`

### Closed Contact Baseline

- `close 0%`
- current branch calibration은 `closedRawPercent=0` 기준이다.
- 2026-04-29 field verify에서 operator가 실제 FR5 gripper 접촉/닫힘을 `0%`에서 확인했다.
- 같은 시점 movement summary embedded peripheral readback은 `position=5`, `positionFault=0`까지는 보였지만, `motionFault=1`, `done=0`이라 completion-grade confirmation으로 보지는 않는다.

### Visible Test Pattern

- `100 -> 70 -> 100`
- operator visual check succeeded on this pattern
- current calibration 기준 readback은 대략 `100 -> 88 -> 100`으로 읽혔다
- 2026-04-29 Unity `Easy Motion` apply + confirm 경로에서도 같은 패턴이 다시 먹었다

### Hold Pattern

- `50%` command
- final readback hold = `position=80`

## Current Operator Path

- `BtnConnect` 한 번으로 `연결 + 위치 읽기`
- `Easy Motion`에서 `100 / 50 / 0` quick button 또는 숫자 입력
- quick button과 숫자 입력은 모두 `draft` 값만 바꾼다
- `미리보기 적용`은 화면 프리뷰만 갱신하고 실기 write는 보내지 않는다
- `실제 이동`만 popup confirm과 `gripper-only` live write 경로를 탄다
- 성공 후 자동 `readback-only` 복귀

## Current UI Separation Rule

- `100 / 50 / 0` quick button은 명시적 value selector다
- `미리보기 적용`과 `실제 이동`은 같은 버튼이 아니다
- mock / dryRun / disconnected / readback-only에서는 `실제 이동`이 green truth가 아니라 blocked reason 또는 preview-only 결과를 남기는 것이 정상이다
- 따라서 mock 검증에서는 `draft 유지`, `preview wording`, `blocked/live wording 분리`를 본다

## Current Evidence Notes

- 2026-04-29 실기 smoke에서 `70% -> raw 88`, `100% -> raw 100`, `50% -> raw 80` 명령 송신은 확인됐다.
- 같은 날짜 calibration patch 뒤 `0% -> raw 0` command에서 operator physical contact가 확인됐고, movement summary embedded peripheral readback은 `position=5`, `positionFault=0`을 남겼다.
- current SDK readback은 여전히 `motionFault=1`, `done=0`, `positionFault=0` 조합으로 completion confirmation이 약하다.
- 따라서 discrete smoke 성공 판정은 `operator visual movement + commanded/raw state change` 기준으로 먼저 본다.
- completion-grade sensor confirmation은 아직 follow-up 과제다.

## Simplification Note

- 현재 제품 safety gate가 요구하는 본질은 `operator confirm 1회`다.
- `token`은 popup과 runtime 사이에서 그 1회를 1-shot으로 전달하는 현재 구현 디테일이다.
- `MoveJ/MoveL`에는 target mismatch 보호가 있으므로 approval token/target model 유지 가치가 크다.
- `MoveGripper`는 current target key가 `none`이라서, 장기적으로는 `UI에 토큰을 노출하지 않는 gripper 전용 1-shot confirm latch`로 단순화할 수 있다.
- 다음 단순화 우선순위는 `operator visible token 제거 -> popup confirm direct path 정리 -> debug helper 유지`다.
- 2026-04-29 현재 debug/field QA는 ad-hoc 버튼 조합 대신 공통 Live QA runner로도 기록할 수 있다. gripper smoke artifact는 `Artifacts/live/qa/*.json`에 `before/after movement + approval + latest-state/latest-drift + ndjson tail`까지 같이 남기는 쪽을 기준선으로 본다.

## Interpretation Rule

- `user 70%`는 현재 raw `70`을 뜻하지 않는다.
- current branch 기준 `user 0%`는 raw `0`을 뜻한다.
- current calibration 기준으로 `70% user`는 raw `88%` 근처다.
- 따라서 운영자 UI는 `user%` 기준으로 보고, debug/readback 비교 때만 raw를 같이 본다.

## Do Not Mix

- `gripper-only` 세션에서 `MoveJ`, `MoveL`, `IO`, `ToolDO` 금지
- `tiny-movej-only` 세션에서 `MoveGripper`, `SetGripperConfig`, `ActGripper` 금지
- same session에서 arm motion과 gripper write를 같이 열지 않는다

## Stop Conditions

아래 중 하나면 즉시 추가 write를 멈춘다.

- `activationFault != 0`
- `positionFault != 0`
- readback이 아예 갱신되지 않음
- operator visual movement와 readback이 심하게 어긋남
- live evidence freshness가 깨짐

## Next Target

다음 구현 목표는 `discrete smoke` 자체가 아니다.

순서는 고정한다.

1. gripper confirm / debug flow 단순화
2. completion-grade readback 확인 범위 재정의
3. `Easy Motion` preview button / live button 분리 semantics를 실기에서 다시 검증
4. `Easy Motion` slider input throttling/commit policy 정리
5. slider 이동 중 live write cadence 고정
6. slider value와 readback value 차이 측정
7. 그다음에만 joint/tcp live slider 설계 검토
