# FR5 Live Official SDK Audit

Last Updated: 2026-04-29 (KST)

## Purpose

이 문서는 `/Users/family/jason/FR5UNITY/robotapp`의 현재 FR5 live 구현을 FAIRINO 공식 C# SDK 문서와 대조한 감사 기준선이다.

판단은 두 층으로 고정한다.

- `시그니처/의도 기준`: FAIRINO 공식 C# 문서와 예제 코드
- `동작 truth 기준`: 현재 branch의 실기 evidence, live artifact, operator 확인

문서와 실기기가 충돌하면:

- API 의미와 intended behavior는 공식 문서를 기준으로 본다.
- 현재 제품에서 실제로 믿을 동작 판정은 실기 evidence를 기준으로 본다.

## Official Source Set

섹션별 공식 기준은 아래 페이지로 잠근다.

- Base:
  [C# Robot Base](https://fairino-doc-en.readthedocs.io/3.7.7/SDKManual/C%23RobotBase.html)
- Motion:
  [C# Robot Movement](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotMovement.html)
- Gripper / Peripherals:
  [C# Robot Peripherals](https://fairino-doc-en.readthedocs.io/3.8.0/SDKManual/C%23RobotPeripherals.html)
- IO:
  [C# Robot IO](https://fairino-doc-en.readthedocs.io/3.8.0/SDKManual/C%23RobotIO.html)

주의:

- FAIRINO 공식 사이트는 섹션별 버전이 섞여 보인다.
- 이번 audit은 `섹션별 공식 페이지`를 source of truth로 사용한다.
- 전체 FR5 source map과 공식 자산 링크는 `docs/ref/product/robots/fairino-fr5-integration-reference.md`에 남기고, 현재 코드/실기 대조는 이 문서 한 곳에서만 정리한다.

## Verdict Labels

- `Match`: 공식 문서 의도와 현재 구현/실기 동작이 맞는다.
- `Adapted`: 공식 API를 wrapper/service/safety layer로 재구성한 의도적 차이다.
- `Stubbed`: 인터페이스나 UI는 있으나 live SDK 구현이 없다.
- `Blocked`: 구현은 있으나 safety gate 또는 controller 상태 때문에 현재 실기 불가다.
- `Divergent`: 파라미터/순서/의미 또는 completion 해석이 공식 문서와 어긋나 재검증이 필요하다.

## Audit Summary

| Area | Official example / concept | Current repo anchor | Current behavior | Field truth | Verdict | Next action |
|---|---|---|---|---|---|---|
| Base session | `RPC`, `CloseRPC`, `GetSDKVersion`, `Mode`, `DragTeachSwitch`, `RobotEnable` | `LiveFairinoClient`, `IFairinoRobotClient`, `FairinoConnectionService` | direct SDK를 reflection wrapper로 감싸고 service에서 상태/에러를 관리한다 | `연결 + 위치 읽기` one-step operator flow는 실기 green | `Adapted` + partial `Match` | Base path는 비교 완료로 보고, readback/session evidence만 계속 축적 |
| Status / readback | `GetRobotRealTimeState`, `GetActualJointPosDegree`, `GetActualTCPPose`, `GetActualTCPNum`, `GetActualWObjNum`, `GetCurToolCoord`, `GetCurWObjCoord`, `GetRobotErrorCode`, `GetSafetyStopState`, `GetSafetyCode`, `SetStatePeriod`, `GetStatePeriod` | `LiveFairinoClient`, `DirectReadbackFairinoClient`, `Fr5LiveStateRecorder` | direct/readback-only 양쪽을 adapter로 묶고 state recorder와 drift evidence를 남긴다 | joint/TCP readback과 evidence freshness는 green이고, current branch에는 controller mode truth alias/fallback과 mode/drag/servo evidence event도 들어갔다. 다만 외부 pendant manual<->auto follow field verify는 아직 남아 있다 | `Adapted` | status/readback 비교를 먼저 끝내고 readback-only gap과 mode-truth field verify를 표로 잠근다 |
| Motion | `MoveJ`, `MoveL`, `StopMotion`, `MotionQueueClear`, `ResetAllError`, official samples also use `SetSpeed` | `LiveFairinoClient`, `LiveCommandSafetyGate`, `RobotControlV3RuntimeController` | per-command speed + safety cap을 쓰며 `tool/user`는 현재 controller 문맥을 재사용한다 | current branch에서 `J1~J6` effective tiny helper path는 실기 green이고 `J5/J6` same-day repeatability도 green이지만, true `3deg` helper path와 broad arm motion은 아직 미일반화 상태다 | `Adapted` + partial `Match` | helper delta와 actual moved delta를 분리하고 broad arm pending boundary를 고정 |
| Gripper | `SetGripperConfig`, `GetGripperConfig`, `ActGripper`, `MoveGripper`, `GetGripperMotionDone`, `GetGripperCurPosition`, `GetGripperActivateStatus`, `GetGripperCurSpeed`, `GetGripperCurCurrent`, `GetGripperVoltage`, `GetGripperTemp` | `LiveFairinoClient`, `RobotControlPeripheralFacade`, `EasyMotion`, `gripper-only` session path | 공식 API family를 따르되 calibration, popup confirm, readback reliability 판정을 별도로 얹는다 | `70/100/50` discrete write는 실기 green, `0% -> raw 0` contact도 operator 확인, completion readback은 여전히 약함 | `Adapted` + partial `Match` + `Divergent` | completion-grade readback 해석을 별도 정리하고 slider follow 전 단계로 고정 |
| IO | `SetDO`, `SetToolDO`, `SetAO`, `SetToolAO` | `IoPanelController`, `RobotControlPeripheralFacade`, `LiveCommandSafetyGate` | UI surface는 있으나 실제 live SDK write path는 열려 있지 않고 mock/dry-run 성격이 강하다 | 공식 문서상 가능한 API지만 현재 branch는 live IO를 보내지 않는다 | `Stubbed` + `Blocked` | 공식 API 대비 실제 미구현 범위를 고정하고 live IO app contract 추가 여부를 결정 |

## Detailed Comparison

### 1. Base

공식 문서 핵심 호출:

- `Robot.RPC(ip)`
- `CloseRPC()`
- `GetSDKVersion(ref version)`
- `Mode(0/1)`
- `DragTeachSwitch(0/1)`
- `IsInDragTeach(ref state)`
- `RobotEnable(0/1)`

현재 repo 대응 symbol:

- `LiveFairinoClient.Connect()`
- `LiveFairinoClient.Disconnect()`
- `LiveFairinoClient.GetVersion()`
- `LiveFairinoClient.SetMode()`
- `LiveFairinoClient.ExitDragTeach()`
- `LiveFairinoClient.EnsureAutoMode()`
- `LiveFairinoClient.Enable()`
- `IFairinoRobotClient`
- `FairinoConnectionService`

현재 동작 상태:

- SDK 객체를 직접 노출하지 않고 `IFairinoRobotClient -> LiveFairinoClient -> FairinoConnectionService` 순서로 감싼다.
- `Enable()` 전 preflight에서 drag teach 종료와 auto mode 준비를 시도한다.
- operator flow는 공식 예제의 개별 버튼 조합이 아니라 `연결 + 위치 읽기`로 묶였다.

실기 truth:

- 현재 branch에서 `BtnConnect` 1회로 `연결 + 위치 읽기`와 `[Sync] 현재 자세 동기화 완료`까지 확인됐다.
- base session green 판정은 이미 실기 evidence가 있다.

판정:

- `Adapted`
- `RPC`, `CloseRPC`, version 읽기 family는 `Match`
- mode/drag/enable은 제품 안전 flow 때문에 `Adapted`

후속 조치:

- base session은 신규 코드 작업보다 evidence 누락 없이 audit 링크를 유지하는 쪽이 우선이다.

### 2. Status / Readback

공식 문서 핵심 호출:

- `GetRobotRealTimeState(...)`
- `GetActualJointPosDegree(...)`
- `GetActualTCPPose(...)`
- `GetActualTCPNum(...)`
- `GetActualWObjNum(...)`
- `GetCurToolCoord(...)`
- `GetCurWObjCoord(...)`
- `GetRobotErrorCode(...)`
- `GetSafetyStopState(...)`
- `GetSafetyCode()`
- `SetStatePeriod(period_ms)`
- `GetStatePeriod(...)`

현재 repo 대응 symbol:

- `LiveFairinoClient.ReadState()`
- `LiveFairinoClient.ReadCoordContext()`
- `LiveFairinoClient.ReadControllerFault()`
- `LiveFairinoClient.GetSafetyCode()`
- `LiveFairinoClient.SetRealtimeStateSamplePeriod()`
- `LiveFairinoClient.GetRealtimeStateSamplePeriod()`
- `DirectReadbackFairinoClient`
- `FairinoReadbackOnlyClientBase`
- `Fr5LiveStateRecorder`

현재 동작 상태:

- direct SDK path와 readback-only path를 모두 adapter 뒤로 감쌌다.
- 공식 `SetStatePeriod/GetStatePeriod`는 앱 계약에서 `SetRealtimeStateSamplePeriod/GetRealtimeStateSamplePeriod`로 이름을 바꿔 노출한다.
- recorder는 `latest-state.json`, `latest-drift.json`, session ndjson evidence를 남긴다.
- current branch에서는 `robot_mode` 외 `robotMode/mode` alias와 optional getter fallback도 같이 읽도록 보강됐다.
- recorder는 이제 `controller-mode`, `drag-teach`, `servo-truth` delta 이벤트도 남긴다.

실기 truth:

- joint/TCP readback, `tool/user/coord`, evidence freshness, `33ms 기본 / 50ms 폴백`은 현재 baseline으로 잠겨 있다.
- 다만 readback-only client는 일부 getter를 아직 `-80`으로 돌려주며, 공식 문서상 가능한 값을 모두 읽지는 못한다.
- `mode` truth follow는 코드상 보강됐지만, 외부 teach pendant `manual <-> auto` 토글 시 `latest-state.json`과 V3 mode surface가 같이 움직이는지는 아직 실기 재검증이 필요하다.

판정:

- 대부분 `Adapted`
- 일부 readback-only 경로는 `Stubbed`

후속 조치:

- status/readback 비교를 P0 첫 항목으로 완결한다.
- `readback-only gap`은 field blocker와 혼동되지 않도록 따로 표시한다.

readback-only gap inventory:

- `FairinoReadbackOnlyClientBase`
  - `Enable`, `Disable`, `MoveJ`, `ServoJ`, `MoveL`, `StopMotion`, `SetRealtimeStateSamplePeriod`, `ClearMotionQueue`, `SetMode`, `SetReconnect`, `ExitDragTeach`, `EnsureAutoMode`, `ResetErrors`, gripper commands are intentionally blocked with `-80`
  - `ReadGripperStatus()` and `ReadGripperConfig()` still return `-80`
- `DirectReadbackFairinoClient`
  - currently forwards `GetSafetyCode`, `GetRealtimeStateSamplePeriod`, `ReadCoordContext`, `ReadControllerFault` to the inner live client
  - still does not widen readback-only gripper coverage
- `FairinoBridgeClient`
  - surfaces `GetSafetyCode`, `GetRealtimeStateSamplePeriod`, `ReadCoordContext`, `ReadControllerFault` from cached bridge state
  - still does not surface gripper or IO readback

### 3. Motion

공식 문서 핵심 호출:

- `MoveJ(...)`
- `MoveL(...)`
- `StopMotion()`
- `MotionQueueClear()`
- `ResetAllError()`
- 예제에서는 `SetSpeed(20)` 같은 global speed 설정도 사용한다.

현재 repo 대응 symbol:

- `LiveFairinoClient.MoveJ()`
- `LiveFairinoClient.MoveL()`
- `LiveFairinoClient.StopMotion()`
- `LiveFairinoClient.ClearMotionQueue()`
- `LiveFairinoClient.ResetErrors()`
- `LiveCommandSafetyGate`
- `RobotControlV3RuntimeController`

현재 동작 상태:

- 공식 예제처럼 SDK 객체에 바로 `MoveJ/MoveL`을 보내지 않고 safety gate와 prepared-target 검사를 거친다.
- 공식 예제의 하드코딩 `tool/user` 대신 현재 controller 문맥 readback을 재사용한다.
- global `SetSpeed` surface는 앱 계약에 없고, per-command speed와 runtime speed cap으로 의미를 대체한다.
- `ServoJ/ServoCart`는 현재 bring-up 범위에서 의도적으로 비활성화되어 있다.

실기 truth:

- current branch에서는 motion path 자체가 실제 하드웨어 이동까지 도달했다.
- verified narrow path on 2026-04-29:
  - `J6 +1deg`
  - `J6 -1deg`
  - `J5 +1deg`
- 2026-04-30 기준으로 saved `MoveJ` single-point live apply와 default saved-point sequence `PendantV3Points` two-point live run-once도 current branch에서 green이다.
- current truth는 immediate artifact text가 아니라 post-run `SyncCurrentStateForDebug()`와 refreshed `latest-state.json` 기준으로 판정한다.
- earlier sessions did show `fault main=1 / sub=1`, but that is no longer the best description of the current branch narrow tiny-joint path.

판정:

- `MoveJ`, `MoveL`, `StopMotion`, `MotionQueueClear`, `ResetAllError`는 `Adapted`
- current narrow `J5/J6 tiny MoveJ` path는 partial `Match`
- arm live motion overall은 아직 broad `Match`가 아니다
- `SetSpeed`는 direct API 대신 다른 의미 체계로 바뀌어 `Adapted`

후속 조치:

- tiny `MoveJ` verified matrix를 `J5/J6 both directions + repeatability`까지 넓힌다.
- teaching point live는 `single point / two-point once / loop locked` 범위로 유지한다.
- 그 뒤에도 broad arm green으로 일반화하지 말고 `J1~J4`, `TCP`, `operator-only auto transition`, `live loop`를 별도 backlog로 유지한다.

### 4. Gripper

공식 문서 핵심 호출:

- `SetGripperConfig(company, device, softversion, bus)`
- `GetGripperConfig(...)`
- `ActGripper(index, act)`
- `MoveGripper(index, pos, vel, force, max_time, block, ...)`
- `GetGripperMotionDone(...)`
- `GetGripperCurPosition(...)`
- `GetGripperActivateStatus(...)`
- `GetGripperCurSpeed(...)`
- `GetGripperCurCurrent(...)`
- `GetGripperVoltage(...)`
- `GetGripperTemp(...)`

현재 repo 대응 symbol:

- `LiveFairinoClient.ConfigureGripper()`
- `LiveFairinoClient.ReadGripperConfig()`
- `LiveFairinoClient.ActivateGripper()`
- `LiveFairinoClient.MoveGripper()`
- `LiveFairinoClient.ReadGripperStatus()`
- `RobotControlPeripheralFacade`
- `EasyMotionController`
- `fr5-gripper-live-success-pattern.md`

현재 동작 상태:

- 공식 API family는 그대로 쓰되, 앱에서는 `FairinoGripperProfile`, calibration, popup confirm, readback reliability note를 따로 둔다.
- `MoveGripper`의 `block` 파라미터는 앱 계약에서 `Blocking bool`로 감싸고 내부에서 `0=blocking`, `1=non-blocking`으로 다시 풀어 쓴다.
- current branch calibration은 `user 0% -> raw 0`이다.

실기 truth:

- `70% -> raw 88`, `100% -> raw 100`, `50% -> raw 80` discrete live write는 실기 green이다.
- `0% -> raw 0`에서 실제 그리퍼 접촉/닫힘이 operator 기준으로 확인됐다.
- movement summary에는 `position=5`, `positionFault=0`이 보였지만, `motionFault=1`, `done=0`이 남아 completion-grade confirmation은 아직 약하다.
- 공식 예제의 gripper config 값은 현재 field-measured config와 다를 수 있으므로, 그 값 자체를 truth로 보지 않는다.

판정:

- command path는 `Adapted` + partial `Match`
- completion readback 해석은 현재 `Divergent`

후속 조치:

- P0 두 번째 항목으로 completion-grade readback 판정 규칙을 고정한다.
- 그 다음에만 slider follow와 continuous live tracking으로 넘어간다.

### 5. IO

공식 문서 핵심 호출:

- `SetDO(id, status, smooth, block)`
- `SetToolDO(id, status, smooth, block)`
- `SetAO(id, value, block)`
- `SetToolAO(id, value, block)`

현재 repo 대응 symbol:

- `IoPanelController`
- `RobotControlPeripheralFacade.SetRobotDigitalOutput()`
- `RobotControlPeripheralFacade.SetToolDigitalOutput()`
- `RobotControlV3RuntimeController.SetRobotDigitalOutput()`
- `RobotControlV3RuntimeController.SetToolDigitalOutput()`
- `LiveCommandSafetyGate`

현재 동작 상태:

- UI와 runtime summary에는 IO 자리가 있지만, live SDK write path는 현재 열려 있지 않다.
- `IoPanelController`는 이름과 달리 현재 그리퍼 패널이며, DO/AO operator UI가 아니다.
- runtime에는 `RobotDo` / `ToolDo` command kind와 summary 문자열이 있지만, facade는 mock/dry-run 중심으로만 값을 바꾼다.
- live에서는 `live blocked: I/O/Gripper SDK contract not enabled`로 차단한다.
- `IFairinoRobotClient`에는 아직 `SetDO/SetToolDO/SetAO/SetToolAO` 같은 app contract가 없다.
- AO는 contract, gate, UI 어느 층에도 아직 실체가 없다.

실기 truth:

- 공식 문서상 IO live write는 가능한 API다.
- 현재 branch는 실기 IO를 보내지 않는다.

판정:

- 공식 API 대비 현재 앱 계약은 `Stubbed`
- 제품 동작 기준으로는 `Blocked`

후속 조치:

- P0 세 번째 항목으로 `공식 API 존재 + 현재 앱 계약 부재`를 표로 고정한다.
- 그 뒤에야 `IFairinoRobotClient`에 live IO API를 추가할지 결정한다.

current IO gap inventory:

- `IFairinoRobotClient`에는 `SetDO`, `SetToolDO`, `SetAO`, `SetToolAO`가 없다.
- `RobotControlPeripheralFacade`는 `SetRobotDigitalOutput()` / `SetToolDigitalOutput()`만 가지며, live path에서는 `CanSimulateOrMock()`에 막혀 mock/dry-run 중심으로 동작한다.
- `RobotControlV3RuntimeController`는 `RobotDo` / `ToolDo` command kind와 gate artifact는 만들지만, 실제 SDK write contract가 없으므로 live success path로 이어지지 않는다.
- `IoPanelController`는 현재 그리퍼 패널이며, official IO API parity는 아직 없다.
- `RobotControlV3RuntimeSnapshot`와 `RobotControlPeripheralState`도 `RobotDoSummary` / `ToolDoSummary`와 digital output state까지만 다룬다.
- `LiveCommandSafetyGate`에는 `RobotAo` / `ToolAo` command kind가 없다.

## Intentional Product Adaptations

이 문서는 아래 차이를 `버그`가 아니라 현재 제품 전략으로 본다.

- SDK 객체 직접 호출 대신 `adapter + service + safety gate` 구조를 쓴다.
- `MoveJ/MoveL`은 공식 예제의 고정 `tool/user` 대신 현재 controller 문맥 readback을 재사용한다.
- `MoveGripper`는 공식 호출 family를 따르지만 calibration, popup confirm, readback reliability 해석이 추가된다.
- `readback-only client`는 공식 예제에는 없는 제품 전략이다.
- `SetSpeed` 같은 global speed surface는 direct UI API가 아니라 per-command speed + cap 정책으로 대체된다.

## Locked Backlog

우선순위는 아래 순서로 고정한다.

### P0

1. `Status/Readback` 비교 완결
   - done when readback-only gap inventory and direct/bridge parity notes are frozen in this audit SSOT
2. `Gripper` completion readback 해석 정리
   - done when `operator-visible contact success`와 `completion-grade sensor confirmation`의 구분 규칙이 명시된다
3. `IO live path` 공식 API 대비 실제 미구현 범위 고정
   - done when app-contract 부재와 runtime block layer가 분리 서술된다

### P1

4. arm `fault 1/1` 해제 조건을 tiny `MoveJ` QA로 실증

P1 executable unit:

- precondition:
  - `연결 + 위치 읽기`
  - fresh `latest-state.json`, `latest-drift.json`
  - `toolId/userId/coordSystem` 확인
- command:
  - one tiny target only, current baseline `J6 +0.5deg @ 5%`
- artifact:
  - shared Live QA runner output
  - motion gate summary
  - latest session `events/readback ndjson`
- success rule:
  - blocker가 여전히 있으면 `fault 1/1` unlock condition evidence로 기록
  - movement가 실제로 열리지 않으면 green으로 포장하지 않음

### P2

5. 그 다음에만 `joint/TCP` operator flow 단순화
6. gripper/joint/TCP debug runner와 UX wording 추가 정리

P2 executable unit:

- `P1`에서 arm blocker interpretation이 고정된 뒤에만 시작
- 목표는 safety model 제거가 아니라 operator/debug 절차 축약이다
- first scope:
  - joint/TCP operator wording 정리
  - debug helper entrypoint naming 통일
  - current audit verdict와 UI copy가 충돌하지 않게 정리

## Related Docs

- `docs/ref/product/robots/fairino-fr5-integration-reference.md`
- `docs/status/FR5-LIVE-INTEGRATION-ROADMAP.md`
- `docs/ref/product/roadmap/fr5-gripper-live-success-pattern.md`
- `docs/ref/product/roadmap/fr5-live-field-checklist.md`
- `docs/daily/04-29/fr5-gripper-zero-contact-calibration.md`
- `docs/daily/04-29/fr5-live-qa-runner-and-artifact-flow.md`
