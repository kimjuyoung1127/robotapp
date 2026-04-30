---
title: "FR5 Auto Manual Mode Transition Plan"
doc_type: "roadmap"
status: "active"
domain: "fr5-live"
audience: "human-and-agent"
last_updated: "2026-04-29"
---

# FR5 Auto Manual Mode Transition Plan

## Goal

기존 teach pendant에 의존하지 않고 `RobotControlV3` 안에서 아래 흐름을 끝까지 책임지는 것이다.

- 현재 mode 읽기
- drag/teach 상태 읽기
- auto 진입 시도
- manual/teach 복귀 시도
- 실패 원인 표시
- operator recovery hint 제공

## Why This Is Next

2026-04-29 current branch 기준으로:

- `J1~J6` all-joint tiny smoke는 green이다
- 다만 현재 tiny motion success는 `operator or teach pendant가 auto mode를 먼저 맞춰둔 상태`를 전제로 한다
- 따라서 다음 제품 과제는 broad arm motion 확장보다 `팬던트 없는 auto/manual transition`을 앱이 직접 책임지는 것이다

## Current Constraints

- base readback path와 motion-capable sibling session이 분리돼 있다
- motion session은 현재 dispatch 직전에 `mode=0` recheck를 한다
- app can attempt `DragTeachSwitch(0)` and `Mode(0)`, but operator-facing recovery flow is 아직 부족하다
- current narrow motion success does not mean pendant-free operation is finished

## Current Progress

2026-04-29 current branch code 기준:

- `P0`:
  - `LiveFairinoClient`가 `robot_mode` 외 `robotMode/mode` alias와 optional mode getter fallback을 같이 읽도록 보강됐다
  - `Fr5LiveStateRecorder`가 `controller-mode / drag-teach / servo-truth` 이벤트를 session evidence에 남긴다
- `P1`:
  - `FairinoConnectionService`에 `SetMode -> SyncCurrentState retry -> requested/actual match 확인` 경로가 추가됐다
  - `RobotControlV3RuntimeController`는 `RequestAutoMode/RequestManualMode`에서 이 검증 경로를 사용한다
- `P2`:
  - manual path도 같은 verification loop를 공유하게 되어 generic request보다 구체적인 failure string을 낼 준비가 됐다
- `P3`:
  - V3 `QuickControllerMode` summary에는 current controller truth와 last mode transition summary가 같이 실리도록 코드상 준비됐다
  - `Pendant V3` 상단 헤더에 `자동 / 수동` 버튼이 추가되어 operator surface에서 직접 mode transition을 요청할 수 있게 됐다

field verify status:

- 외부 teach pendant에서 `manual -> auto -> manual` 토글 시 `latest-state.json`의 `mode` truth가 실제로 같이 바뀌는 것은 2026-04-29 기준으로 확인됐다
- app-owned auto/manual path는 2026-04-29 batch smoke 기준 normal case에서 green이다
- confirmed result:
  - header `수동` -> `latest-state.json mode=1`
  - header `자동` -> `latest-state.json mode=0`
- remaining open item은 normal case 자체가 아니라 exceptional case recovery wording이다

## Scope

이 계획은 아래만 다룬다.

- `controller mode` read/write
- `drag/teach` exit/readback
- operator-facing `auto/manual` transition UI/feedback
- runtime/session synchronization between readback session and motion session

아직 scope 밖인 것:

- broad TCP motion policy
- full teleop
- full pendant replacement for all maintenance menus

## Product Rules

1. app must always show current mode truth from controller readback
2. app must not claim `auto ready` until motion-capable session also confirms the same truth
3. auto transition failure must be shown as a concrete reason, not a generic failure string
4. manual transition is allowed to be a controlled recovery action, not just a debug helper
5. pendant should become optional for normal motion bring-up, not for every exceptional service condition

## Phases

### P0. Truth Unification

- add one SSOT summary for:
  - `mode`
  - `drag/teach`
  - `enabled`
  - `fault/safety`
  - `tool/user`
- make readback session and motion session expose the same mode vocabulary
- log transition attempts as first-class evidence in `Artifacts/live/qa` or session events

Exit:

- operator and debug view both show the same `mode/drag` truth
- auto/manual transition attempts leave readable evidence
- current code status:
  - `implemented in code`
  - `field verify pending`

### P1. Auto Transition Command Path

- add runtime service path for:
  - `ExitDragTeach`
  - `EnsureAutoMode`
  - optional `Enable` precondition
- keep retries bounded and evidence-backed
- do not dispatch motion until the motion session itself confirms `mode=0`

Exit:

- operator can press one app action to request auto-ready state
- app either reaches auto-ready or reports a specific blocker
- current code status:
  - `implemented in code`
  - `motion-session/operator field verify pending`

### P2. Manual/Teach Recovery Path

- add explicit operator recovery action for manual/teach return
- expose whether the controller is still in drag/teach
- if manual return is not allowed from current state, explain why

Exit:

- operator can intentionally return to manual/teach from app
- app can explain whether the request succeeded
- current code status:
  - `partial in code`
  - `true manual/teach recovery field verify pending`

### P3. UI and Operator Flow

- add visible `Mode` card or status chip in V3
- show:
  - current mode
  - last transition attempt
  - last failure reason
  - recovery hint
- reduce dependence on debug bridge for normal mode management

Exit:

- normal field operator can manage `auto/manual` without debug-only helpers
- current code status:
  - `operator surface verified for normal case`
  - `dedicated recovery wording still pending`

## Failure Taxonomy

최소한 아래 이유는 구분해서 보여야 한다.

- `drag/teach still on`
- `auto mode command rejected`
- `servo not ready`
- `safety stop active`
- `controller fault present`
- `mode mismatch between readback and motion session`
- `network/SDK unavailable`

## Acceptance Criteria

- app-only flow can bring the robot from manual/teach to motion-ready auto state without teach pendant in the normal case
- app-only flow can return to manual/teach or explain why not
- tiny `MoveJ` smoke can start from app-initiated auto-ready state
- operator no longer needs pendant just to run standard live tiny motion verification

## Risks

- controller may expose mode transitions that succeed on one session and lag on another
- some service or safety conditions may still require physical pendant/operator intervention
- false-positive `auto ready` UI would be worse than a conservative block

## Suggested Implementation Order

1. truth unification and evidence
2. auto transition path
3. manual/teach return path
4. operator UI/status surface
5. only then broader motion UX simplification

## Current Success Pattern

2026-04-29 current branch 기준 성공패턴은 아래로 잠근다.

1. `latest-state.json`에서 현재 `mode` truth가 살아 있는지 확인한다.
2. 외부 teach pendant `manual <-> auto` 토글이 `latest-state.json`의 `mode`를 실제로 따라오게 하는지 본다.
3. 같은 verified path를 `Pendant V3` 상단 헤더 `자동 / 수동` 버튼으로 호출한다.
4. `requested mode -> SyncCurrentState retry -> requested/actual mode match`가 닫히는지 본다.
5. 실패하면 generic fail이 아니라 `mode mismatch / drag-teach / servo / network` 등 concrete reason을 surface에 남긴다.

## Verified Result

2026-04-29 batch smoke 기준:

- initial `mode=0`
- header `수동` handler path after connect/sync -> `mode=1`
- header `자동` handler path after reconnect/verify -> `mode=0`

해석:

- 외부 teach pendant 없이도 current branch normal-case auto/manual 전환은 앱 책임 경로에서 닫힌다
- current remaining work is no longer "can we switch mode?" but "how do we explain and recover from failure cases cleanly?"
