# FR5 Live Integration Roadmap

## Goal

`robotapp2`를 먼저 FAIRINO FR5 실기 연동 가능한 수준까지 끌어올린 뒤,
그중 재사용 가능한 안정 구간만 별도 템플릿으로 잘라 `robottemplete`에 이식한다.

## Scope

- 대상 프로젝트: `robotapp2`
- 대상 로봇: `FAIRINO FR5`
- 1차 목표: `Mock -> Live` 전환 가능한 RobotControl 실기 검증 경로 완성
- 2차 목표: 실기 연동에서 검증된 공통 계층만 템플릿화

## Current Baseline

### Already Implemented

- 실기 연동 추상화 계층 존재
  - `Assets/Scripts/App/Fairino/IFairinoRobotClient.cs`
  - `Assets/Scripts/App/Fairino/FairinoConnectionService.cs`
- Live / Mock 전환 구조 존재
  - `Assets/Scripts/App/Fairino/LiveFairinoClient.cs`
  - `Assets/Scripts/App/Fairino/MockFairinoClient.cs`
- RobotControl 씬과 UI 셸 존재
  - `Assets/Scripts/App/Fairino/RobotControlSceneCoordinator.cs`
  - `Assets/Scripts/UI/RobotControl/FairinoConnectionPanel.cs`
  - `Assets/Scripts/UI/RobotControl/FairinoJointControlPanel.cs`
  - `Assets/Scripts/UI/RobotControl/FairinoTcpControlPanel.cs`
  - `Assets/Scripts/UI/RobotControl/RobotControlDiagnosticsDrawer.cs`
- 실기용 SDK DLL staging 완료
  - `Assets/Plugins/Fairino/libfairino.dll`
  - `Assets/Plugins/Fairino/CookComputing.XmlRpcV2.dll`
- 공식 자료 source map 정리 완료
  - `docs/ref/product/robots/fairino-fr5-integration-reference.md`
- P0 1차 코드 보강 완료
  - `LiveFairinoClient`가 실제 SDK 시그니처 기준 reflection 호출로 보강됨
  - `GetVersion()`이 SDK `GetSDKVersion`, `GetSoftwareVersion`, `GetFirmwareVersion` 경로를 사용함
  - `ReadState()`가 `GetRobotRealTimeState` 우선, fallback getter 차선 경로를 사용함
  - `MoveJ`, `MoveL`, `ServoJ`, `StopMotion`이 실제 SDK 파라미터 형태에 맞춰짐
- Live smoke tooling 추가
  - `Assets/Editor/KineTutor3D/FairinoLiveSmokeTools.cs`
  - `Assets/Scripts/App/Fairino/FairinoLiveSmokeRunner.cs`
- SDK 존재 검증용 테스트 추가
  - `Assets/Tests/EditMode/Validation/LiveFairinoClientSdkTests.cs`
- Readback-only live monitor 추가
  - `Assets/Scripts/App/Fairino/FairinoRobotClientFactory.cs`
  - `Assets/Scripts/App/Fairino/FairinoSdkCompatibilityProbe.cs`
  - `Assets/Scripts/App/Fairino/DirectReadbackFairinoClient.cs`
  - `Assets/Scripts/App/Fairino/FairinoBridgeClient.cs`
  - `Assets/Scripts/App/Fairino/Fr5LiveStateRecorder.cs`
- 맥북 field-readback 기준선 추가
  - 구현 커밋: `d8c0726 Add FR5 readback-only live monitor`
  - 검증 브랜치: `codex/robotcontrol-v3-toolkit`
  - field guide: `docs/ref/product/roadmap/fr5-live-field-checklist.md`
- 맥 `unityctl` bridge 경로 복구 완료
  - `Packages/manifest.json`의 `com.unityctl.bridge`를 macOS 로컬 plugin source로 재연결
  - `unityctl status --wait --json` 기준 `Ready`, `bridgeLoaded=true`, `ipcPipePresent=true` 확인
  - `unityctl check --type compile` 통과
  - `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.Fr5LiveReadbackTests` 통과
- V3 readback-only live UX 정렬 완료
  - readback-only live에서는 primary action이 `서보 켜기`가 아니라 `현재 위치 읽기`를 우선 안내
  - smoke 출력에 `client=direct|bridge`, `sdkLoadStatus`, `sdkRuntime` 포함
- MacBook 실기 연동 baseline 확보
  - `ping 192.168.58.2` / `nc -vz 192.168.58.2 8080` 성공
  - direct live smoke `CONNECT_OK`
  - `client=direct`, `sdkLoadStatus=direct-ready`, `latest-drift.json severity=ok` 확인
- `eth0` field variant baseline 확보
  - FR5 controller ports were confirmed as `eth0 = 192.168.57.2`, `eth1 = 192.168.58.2`
  - MacBook `USB 10/100 LAN` was reconfigured to `192.168.57.10/24`
  - V3 default FR5 IP/resource strings were updated to `192.168.57.2`
  - direct live reconnect succeeded again on session `20260428-103717` / `20260428-103915`
  - `latest-state.json` / `latest-drift.json` stayed fresh with drift `0.0`
- 실기 pose truth 분리 완료
  - 현재 실기 기준 truth는 FR5 controller readback이다.
  - post-sync drift baseline은 여전히 near-zero로 trusted 한다.
  - latest evidence freshness와 `coordSystem` metadata persistence까지 current branch 기준으로 복구했다.
- latest evidence preservation 완료
  - `Fr5LiveStateRecorder`는 disconnected zero state와 connected placeholder zero state를 `latest-state.json` / `latest-drift.json` 위로 승격하지 않는다.
  - invalid follow-up states는 `readback-skip` event로만 남기고 마지막 정상 live readback을 보호한다.
- live gate automation 1차 고정 완료
  - `scripts/tests/run_fr5_live_checks.sh`가 current-session events, `connected=true`, `toolId`, `userId`, `coordSystem`, 그리고 preservation event를 구분해 판단한다.
  - current policy 기준 latest run: `25 passed / 0 failed / 3 skipped`
- readback-only motion gate summary 정합성 복구 완료
  - V3 `tiny MoveJ gate` debug summary가 더 이상 실기 readback-only 세션을 `dry-run simulation`으로 오인하지 않는다.
  - current live baseline에서는 `status=ReadbackOnly`, `ready=False`, `tool=1`, `user=1`을 포함한 readback summary가 노출된다.
- live poll baseline 고정 완료
  - 기본 live poll은 `33ms`, 오류 연속 시 자동 폴백은 `50ms`로 고정했다.
  - 2026-04-28 실측 기준 update rate는 `100ms ≈ 8.93Hz`, `50ms ≈ 18.66Hz`, `33ms ≈ 27.37Hz`였다.
  - 운영 기본값은 `33ms`이고, fast poll에서 read 오류가 연속으로 나면 같은 세션에서 `50ms`로 낮춘다.
- 재기동 후 live pose 재동기화 검증 완료
  - `restart_v3_live_loop.sh --connect --sync` 기준 재실행 루프를 통과했다.
  - session `20260428-064612`에서 `latest-state.json`, `latest-drift.json`이 current session으로 다시 갱신됐다.
  - `severity=ok`, `maxJointDeg=0.0`, `maxTcpMm=0.0`, `maxTcpRotDeg=0.0`를 확인했다.
  - `GetLiveStateComparisonForDebug()` 기준 `clientRead`, `serviceLast`, `runtimeCurrent`가 같은 live pose를 가리켰다.
  - 운영자 직접 시각 확인으로도 Unity 시뮬레이션 로봇과 실제 FR5 자세가 동기화된 것을 확인했다.
- `33ms -> 50ms` 자동 폴백 field verification 완료
  - `FairinoConnectionService`에 debug 전용 강제 read-failure 주입 경로를 추가해 live 세션에서 폴백을 재현 가능하게 만들었다.
  - `scripts/tests/run_fr5_poll_fallback_check.sh`가 restart -> connect -> sync -> `33ms` probe -> forced fallback -> gate/evidence refresh 루프를 자동 실행한다.
  - session `20260428-070036`에서 정상 probe는 `poll=0.033s`, 강제 오류 뒤 probe는 `poll=0.05s`로 전환됐다.
  - 같은 세션에서 `tiny MoveJ gate`는 계속 `status=ReadbackOnly`, `ready=False`로 잠겨 있었고, `latest-state.json` / `latest-drift.json` freshness는 유지됐다.
  - 회귀 검증으로 `FairinoConnectionServiceTests 7/7`, `Fr5LiveReadbackTests 10/10`을 다시 통과했다.
- tiny `MoveJ` live-path reuse + field retry 완료
  - tiny motion runtime은 이제 새 direct-motion RPC 세션을 다시 열지 않고, 기존 live readback 세션의 motion-capable client를 재사용한다.
  - `ReadCoordContext()` 실패는 마지막 `tool/user` 문맥을 유지하는 경고 fallback으로 낮췄다.
  - `FairinoErrorTranslator`에 `14`, `154` 의미를 추가했다.
  - 회귀 검증으로 `FairinoErrorTranslatorTests 10/10`, `FairinoConnectionServiceTests 8/8`을 통과했다.
  - 2026-04-28 session `20260428-084436`에서 tiny target `J6 +0.5deg @ 5%`를 다시 시도했지만, live blocker는 controller `fault main=1 / sub=1`이었다.
  - `ResetErrors()`는 `0/OK`였지만 후속 sync에서도 `fault=1/1`이 유지됐다.
  - 따라서 현재 tiny `MoveJ`는 gate 설계 pending이 아니라 controller fault clear pending 상태다.
- teach pendant + gripper field split 확인
  - legacy teach pendant reconnection changed the measured SDK gripper config from `company=3, device=4, soft=0, bus=0` to `company=1, device=3, soft=4, bus=0`
  - V3 gripper defaults were updated to the new measured config
  - under that profile, `ProbeLiveGripperForDebug()` reached `matchesExpected=True` and `activationFault=0`
  - however live smoke still ended at `motionFault=1`, `done=0`, `position=0`
  - teach pendant surfaced `그리퍼 485 시간초과` and `클램프 이동오류`, while robot joint motion still worked
  - this historical signal should now be treated as branch/time-specific evidence, not as the current discrete V3 gripper blocker without latest-branch reconfirmation
- operator connect/sync flow consolidation 완료
  - V3 Home `연결` is now `connect + current-position read` in one path
  - separate top-bar `현재 위치 읽기` button was removed from the shell
  - post-restart verification confirmed one `BtnConnect` click now reaches `[Sync] 현재 자세 동기화 완료`
  - this is now the operator baseline flow for live readback sessions
- connect/sync debug success pattern 분리
  - `BtnConnect` direct debug는 이제 별도 narrow SSOT로 관리한다: `RobotControlV3` 재진입 -> `BtnConnect` 1회 -> refreshed `latest-state.json` / current session `events.ndjson`
  - current branch added a disabled-read polling stop so `로봇 비활성 상태입니다. Enable 버튼을 눌러주세요.`가 반복될 때 background poll이 계속 같은 readback을 hammering하지 않는다
  - current branch also stopped re-reading SDK version on every state record and now mirrors `enabled` truth from realtime state payload
- gripper Easy Motion control baseline 확보
  - `Easy Motion` now exposes operator-friendly gripper percent controls
  - Unity-side live gripper write was revalidated in `gripper-only` mode
  - observed readback loops include `100 -> 96 -> 100`, `100 -> 88 -> 100`, and `100 -> 80` hold
  - operator visually confirmed the `70` test as noticeable
  - current calibration still maps user percent to SDK raw percent, so operator percent and raw percent are not yet 1:1
- Easy Motion discrete live operator flow green
  - `적용` now routes through product live approval instead of direct blocked write
  - current operator path is `연결 + 위치 읽기 -> percent input/preset -> 적용 -> 이동 실행 확인`
  - 2026-04-29 field verify on real hardware confirmed visible movement on `70`, `100`, and `50`
  - same-day calibration update also locked current branch `closedRawPercent=0`, and operator confirmed real gripper contact/close at `0%`
  - latest `0%` movement summary kept weak completion flags (`motionFault=1`, `done=0`) but did show embedded peripheral `position=5`, `positionFault=0`
  - current commanded/raw states were observed as `70 -> raw 88`, `100 -> raw 100`, `50 -> raw 80`
  - popup approval token lifetime now survives live readback updates long enough for one-shot confirm consumption
  - debug verification path was reduced by adding direct popup confirm/cancel bridge helpers
- Easy Motion preview/live surface split in progress
  - quick selection is being narrowed to explicit `100 / 50 / 0`
  - `미리보기 적용` and `실제 이동` are now separate operator intents in code
  - mock/readback-only should no longer be interpreted as successful live movement by the same button label
  - 2026-05-01 verification on current branch is mock-only compile/behavior check, not fresh field smoke
- Common live QA runner scaffold added
  - `RobotControlV3DebugBridge` now has shared QA entrypoints for `gripper`, `joint nudge`, `TCP nudge`, `point move`, and `snapshot`
  - each QA run captures `before/after movement`, approval summary, popup state, refreshed `latest-state/latest-drift`, and session ndjson tails
  - QA artifacts are written to `Artifacts/live/qa/*.json`
  - current intent is to reduce debug procedure complexity without weakening runtime safety gates
  - this by itself did not mean broad arm motion was green; runtime safety gates and narrow verified scope are still separate concerns
- Tiny joint live success pattern 확보
  - motion-capable sibling session now rechecks `mode=0` before tiny `MoveJ` dispatch
  - current narrow green scope is locked as:
    - `J1 + effective ~1deg`
    - `J1 - effective ~1deg`
    - `J2 + effective ~1deg`
    - `J2 - effective ~1deg`
    - `J3 + effective ~1deg`
    - `J3 - effective ~1deg`
    - `J4 + effective ~1deg`
    - `J4 - effective ~1deg`
    - `J5 +1deg`
    - `J5 -1deg`
    - `J6 +1deg`
    - `J6 -1deg`
    - `J1~J6 all-joint effective tiny motion`
    - `J5/J6 both directions`
    - same-day repeatability on the narrow path
  - verified readback deltas on 2026-04-29 were:
    - `J1: -80.0535659790039 -> -79.05239868164062`
    - `J1: -79.05239868164062 -> -80.05400085449219`
    - `J2: -89.86425018310547 -> -88.86329650878906`
    - `J2: -88.86329650878906 -> -89.86425018310547`
    - `J3: 90.04438018798828 -> 91.04598236083984`
    - `J3: 91.04598236083984 -> 90.04307556152344`
    - `J4: -89.9071044921875 -> -88.90680694580078`
    - `J4: -88.90680694580078 -> -89.90666961669922`
    - `J5: -90.0217514038086 -> -89.02188873291016`
    - `J5: -89.02167510986328 -> -90.02153778076172`
    - `J5: -90.02153778076172 -> -89.01688385009766`
    - `J5: -89.021240234375 -> -90.0221939086914`
    - `J5: -90.0221939086914 -> -89.01644897460938`
    - `J6: 1.1638981103897095 -> 2.1635451316833496`
    - `J6: 2.1637628078460693 -> 1.1636805534362793`
    - `J6: 1.163245439529419 -> 2.1676785945892334`
    - `J6: 2.1676785945892334 -> 1.1630278825759888`
  - current truth rule is:
    - execute summary `OK`
    - post-run `SyncCurrentStateForDebug()`
    - post-run `RefreshLiveEvidenceForDebug()`
    - refreshed `Artifacts/live/fr5/latest-state.json`
- old shell increment helper path did not yield a true `3deg` move
- 2026-04-30 current branch added a separate `1~5deg direct-input` path that does not depend on shell increment snapping
- current verified truth on that path is:
  - `J6 true +3deg` is green through the real popup-confirm product path
  - artifact records `requestedDeltaDeg` and `actualDeltaDeg`
  - header now exposes a one-line `다음 행동` summary and Safety/Diagnostics shows operator-facing failure taxonomy
- `5deg` is not yet locked green:
  - a tiny range tolerance patch was added to avoid floating-point `5.000xdeg` false blocks
  - final live re-smoke after the patch is still pending
- Teaching point live success pattern 확보
  - saved `MoveJ` point single live apply is green on the current branch
  - default saved-point sequence `PendantV3Points` two-point live `1회 실행` is now green
  - point save/apply/run 앞에는 `SyncCurrentStateForDebug() + RefreshLiveEvidenceForDebug()`와 Unity pose/readback 일치 확인을 강제한다
  - operator가 manual mode에서 실제 자세를 바꾼 경우에는 그 synced pose를 먼저 `Home` point로 저장한 뒤 repeatability를 시작한다
  - `QA0430_HOME_2PT_T6` one-shot live repeatability green
  - `QA0430_HOME_MULTIAXIS_T5_T6` one-shot live repeatability green
  - current verified scope is still narrow:
    - saved `MoveJ` points only
    - `single point` live apply
    - `two-point once`
    - `Home`-based custom one-shot repeatability
    - `loop locked`
  - current truth is still post-run `SyncCurrentStateForDebug()` + `RefreshLiveEvidenceForDebug()` + refreshed `latest-state.json`
  - this is not broad named-sequence/program-runtime green
- Official SDK audit SSOT added
  - `docs/ref/product/roadmap/fr5-live-official-sdk-audit.md` now compares FAIRINO official C# docs against current repo and field evidence
  - current summary is locked as:
    - `Base`: mostly `Adapted/Match`
    - `Status/Readback`: mostly `Adapted` with some readback-only gaps
    - `Motion`: `Adapted`, with current narrow `J5/J6 tiny MoveJ` path green but broader arm live still not generalized
    - `Gripper`: command path is `Adapted + partial Match`, completion readback remains weak/`Divergent`
    - `IO`: official APIs exist, but current app contract/live path is effectively `Stubbed/Blocked`
  - implementation priority now follows the audit backlog instead of ad-hoc API guessing
- Mode truth / auto-manual transition groundwork added
  - `LiveFairinoClient` now reads controller mode from `robot_mode` plus alias candidates such as `robotMode/mode`, with optional getter fallback when available
  - `Fr5LiveStateRecorder` now records `controller-mode`, `drag-teach`, and `servo-truth` event deltas alongside readback/session evidence
  - `FairinoConnectionService` now has a verified mode-change path that retries `SyncCurrentState()` until requested/actual mode truth matches
  - `RobotControlV3RuntimeController` now uses that verified path for `RequestAutoMode()` and `RequestManualMode()`
  - `QuickControllerMode` summary was widened to include current controller truth and the last transition summary
  - 2026-04-29 field verify confirmed external teach pendant `manual <-> auto` toggles now propagate into `latest-state.json` mode truth
  - `Pendant V3` top header now exposes `자동 / 수동` buttons that call the same verified mode-transition path
  - 2026-04-29 batch smoke confirmed header handler path can drive `mode=0 -> 1 -> 0` on real hardware
  - `DirectReadbackFairinoClient` now still blocks motion/IO/gripper but no longer blocks `SetMode / ExitDragTeach / EnsureAutoMode`
  - this is now `normal-case green`; recovery wording and exceptional-case operator UX are still pending
- Known issue 분리
  - V3 Unity Editor 레이아웃은 현재 MacBook에서 `4K UHD` GameView preset 외 해상도에서 붕괴
  - 이 문제는 실기 readback/direct SDK 성공 여부와 분리해서 추적
  - 다음 현장 세션 우선순위는 layout polish보다 hardware readback evidence 축적

### Not Finished Yet

- `Connect(ip, port)`는 여전히 SDK `RPC(ip)` 경로에 의존하며, `port`는 진단 메시지 수준으로만 사용함
- `Mode`, `SetStatePeriod/GetStatePeriod`, `GetSafetyCode`, `queue clear`, `log download` 영역은 공식 SDK 기준 개념은 잡혀 있지만, 모든 client mode/operator surface/field verification parity가 끝난 상태는 아님
- `ServoCart`는 SDK에 존재하지만 앱 계층 미연결
- live state를 3D joint mirror에 더 정밀하게 묶는 전용 adapter가 아직 없음
- 비-4K GameView 해상도에서 V3 레이아웃 안정화 미완료
- Enable / small `MoveJ`는 이번 readback-only 범위가 아니며, 별도 승인 전까지 차단 상태로 둠
- `tool/user/TCP` 운영 노출, motion gate UI 노출, PlayMode/bootstrap 정리, non-4K layout 문제는 아직 남아 있음
- current field blockers are split:
  - arm track: current `J1~J6` effective tiny path is green and `J6 true +3deg` direct-input path is green, but `5deg` direct-input closure, multi-cycle repeatability, `TCP`, and operator-only auto-mode transition are still pending
  - gripper: command path is green and `0%` contact baseline is now operator-confirmed on current branch, but completion-grade readback is still weak (`motionFault=1`, `done=0`, `positionFault=0`) and continuous slider follow is still pending
- teaching track: `single point / two-point once` is green, but live loop and broad named sequence runtime are still locked
- teaching repeatability track: `Home` 기준 `2-point / multi-axis` sequence는 sync-confirm 절차와 same-session live re-smoke까지 닫혔다. 다음 범위는 broad named sequence generalization과 live loop unlock 판단이다
- official-sdk audit backlog is now locked:
  - `P0`: status/readback comparison completion, gripper completion-readback interpretation, IO live-path gap fixation
  - `P1`: close `5deg` direct-input tiny smoke and extend repeatability from `J6 true +3deg` baseline
  - `P2`: simplify joint/TCP operator flow and reduce pendant auto-mode dependency through app-owned mode transition
- mode-control-specific next verify:
  - header `다음 행동`과 Safety/Diagnostics taxonomy가 `tiny range exceeded`, `mode mismatch`, `activation not ready` 같은 real failure state에서 충분한지 확인
  - drag/servo/fault exceptional case에서 mode transition taxonomy를 operator-facing copy로 정리
- live QA/debug path is now split from safety-model complexity:
  - `MoveJ/MoveL` approval token/target model stays for runtime safety
  - debug/field QA should prefer the new common QA runner so operator-side procedure becomes `run one helper -> inspect one artifact`
- Unity slider live control is not finished yet.
  - before continuous follow, `Easy Motion` must keep `preview-only commit` and `live write` clearly separate on the operator surface
  - gripper currently works through discrete button/smoke style writes and readback verification
  - current operator flow still has more approval/debug plumbing than the gripper path probably needs
  - continuous real-time slider-to-hardware following is the next implementation target
  - joint/TCP slider live control remains out of scope until gripper slider behavior is stable

## Implementation Readiness Estimate

이 퍼센트는 현재 코드 기준의 추정치다.

| Area | Status |
|---|---:|
| RobotControl UI / scene shell | 85% |
| Mock / Live adapter architecture | 75% |
| SDK binary staging | 70% |
| Live SDK method correctness | 60% |
| Real state mirroring fidelity | 45% |
| Safety / recovery / diagnostics completeness | 25% |
| On-hardware validation | 10% |
| **Overall live-integration maturity** | **62%** |

## External Source Baseline

Official source hub:

- `https://www.frtech.fr/DOWNLOAD2`

Expected official artifacts:

- FAIRINO C# SDK
- Robot 8083 Port Status Feedback Protocol
- Robot Controller Communication Command Protocol

Internal source map:

- `docs/ref/product/robots/fairino-fr5-integration-reference.md`

## Phase Plan

### P0

목표: 실기 연결을 먼저 "안전하게 읽는 수준"까지 올린다. motion은 readback 성공 뒤 별도 phase에서 연다.

#### P0-1. SDK handshake 진짜 구현

- `LiveFairinoClient.GetVersion()`을 실제 SDK reflection 호출로 교체
- 연결 직후 firmware / sdk / controller 식별값을 읽어 UI와 diagnostics drawer에 표시
- 실패 시 mock fallback이 아니라 명확한 live-mode 오류를 노출

#### P0-2. Live connect 경로 정밀화

- `Connect(ip, port)`의 현재 동작과 SDK 시그니처를 맞춘다
- reflection 메서드 탐색 실패 시 어떤 메서드가 없는지 구체적으로 로그 남김
- DLL 누락 / 타입 미탐지 / RPC 실패를 분리해 에러 번역

#### P0-3. Read-only state path 강화

- `ReadState()`를 실제 가능한 SDK getter 세트로 확장
- 최소 수집 대상:
  - actual joints
  - actual TCP pose
  - connection state
  - enable state
- `RobotControlSceneCoordinator`에서 live 상태를 3D joint mirror에 안정 반영

#### P0-4. Safe motion 최소 경로

- readback-only 성공 전에는 `Enable`, `MoveJ`, `MoveL`, `IO`, `Gripper`를 모두 차단
- readback 세션 2회 이상 성공하고 drift가 안정적일 때 별도 motion gate 문서를 작성
- 첫 motion 검증은 후속 phase에서 "작은 범위 MoveJ 1회"만 따로 승인

#### P0-5. Live mode guardrails

- live 모드에서 MoveJ / MoveL 실행 전 confirm dialog 강제
- mock / live에 따라 버튼 라벨, 위험 표시, helper text 명확화
- `StopMotion()`을 긴급 정지 행동으로 UI 상단에서 항상 접근 가능하게 유지

#### P0 Exit Criteria

- 실기 컨트롤러에 연결 성공
- 실제 버전 정보 표시 성공
- 실제 joint / TCP 읽기 성공
- `Artifacts/live/fr5/latest-state.json` 갱신 성공
- `Artifacts/live/fr5/latest-drift.json` 갱신 성공
- session NDJSON append 성공
- 실패 시 사용자에게 이유가 분리되어 보임

현재 상태 메모:

- evidence recorder freshness bug는 현재 브랜치에서 복구됐다.
- preservation policy까지 들어가서 마지막 정상 live readback이 invalid zero/disconnect follow-up state에 덮어써지지 않는다.
- 현재 기준선은 `latest-state.json`, `latest-drift.json`, `sessions/*-events.ndjson`의 `readback` 또는 `readback-skip` evidence를 함께 해석하는 것이다.
- live reconnect 뒤 pose 재정렬까지 numeric + visual 기준으로 green이다.
- 다음 남은 일은 두 갈래다.
  - arm track: common QA runner로 `joint/TCP/point` evidence를 같은 artifact 형식으로 쌓고, direct-input `requestedDeltaDeg / actualDeltaDeg`를 기준으로 `5deg + repeatability` 경로를 연다.
  - gripper track: confirm/debug flow를 더 단순화하고 completion readback 해석 기준을 정리한 뒤 slider live follow로 넘어간다.
  - 그 다음 트랙이 pendant-free `auto/manual` transition 구현과 non-4K layout 정리다.

### P1

목표: 실기 운영 품질과 진단 가능성을 높인다.

#### P1-1. 상태 폴링 / 주기 제어

- `SetStatePeriod` / `GetStatePeriod` 반영
- polling rate를 mock/live별로 조정
- diagnostics drawer에 state cycle 표시
- current baseline:
  - default live poll `33ms`
  - fallback live poll `50ms`
  - measured field rates on MacBook direct readback: `8.93Hz @100ms`, `18.66Hz @50ms`, `27.37Hz @33ms`

#### P1-2. 모드 / safety / recovery

- `Mode(...)` 계층 추가
- `GetSafetyCode()` 추가
- `MotionQueueClear()` 추가
- connection lost 이후 reconnect / reset 가이드 정리

#### P1-3. Diagnostics drawer 실전화

- 현재 placeholder인 로그/복사/수집 기능 연결
- version / last error / recent feedback / retry hint 외에
  - safety code
  - queue status
  - state period
  - controller mode
  를 추가

#### P1-4. Error translation 확장

- 공식 error code table 기반 세분화
- 카테고리:
  - network
  - parameter range
  - unreachable pose
  - safety
  - controller internal

#### P1-5. MoveL 안정화

- TCP 입력 검증 강화
- dry-run FK 결과와 live MoveL target을 같이 보여줌
- live mode에서는 confirm dialog 필수 유지

#### P1 Exit Criteria

- live diagnostics가 운영자에게 충분한 정보 제공
- queue / safety / mode / polling 관련 상태 확인 가능
- MoveJ / MoveL 모두 제어 가능
- reconnect와 stop 시나리오 검증 완료

### P2

목표: 고급 실기 조작과 템플릿화 준비를 마친다.

#### P2-1. Servo path

- `ServoJ` 운영 정책 정리
- 필요 시 `ServoCart` 추가
- slider / joystick 기반 teleop에 대한 rate limit / safety clamp 적용

#### P2-2. Waypoint / teaching live path

- `WaypointCycleRunner`를 실제 live mode와 더 단단히 연결
- playback 중 stop / abort / recover 절차 정리

#### P2-3. Controller log / artifact capture

- SDK 제공 log/data export API 연결 가능성 검토
- 장애 시 evidence 수집 플로우 추가

#### P2-4. Template extraction boundary

- `robottemplete`로 옮길 수 있는 공통층과 옮기면 안 되는 실기 의존층을 분리

템플릿 이식 가능:

- `IFairinoRobotClient`
- `FairinoResult`
- `FairinoErrorTranslator`
- `FairinoVersionInfo`
- `FairinoRobotState`
- 안전 검증기
- live/mock 분리 adapter 구조

템플릿 이식 보류:

- 현장 IP 기본값
- controller-specific handshake 정책
- 운영 로그 수집 정책
- 현장 safety 절차
- 실기 승인 UI 문구

#### P2 Exit Criteria

- Servo / waypoint / diagnostics까지 구조 정리 완료
- 템플릿 추출 경계가 명확함
- `robottemplete` 이식 대상 목록 확정

## Order Of Work

1. `robotapp2` P0 완료
2. 현장 연결 검증
3. `robotapp2` P1 완료
4. 운영 진단 품질 확보
5. `robotapp2` P2 완료
6. 공통층만 slim live adapter 패키지 또는 template add-on으로 분리
7. 마지막에 `robottemplete` 이식

## Immediate Next Changes

다음 작업은 P0 기준으로 아래 순서를 권장한다.

1. V3 상태/진단 패널에 `tool/user/coordSystem + tiny MoveJ gate`를 상시 노출
2. `motion gate` 문서화와 tiny `MoveJ` 승인 조건 설계
3. preservation/readback evidence를 운영자용 문구와 diagnostics에 더 잘 노출
4. PlayMode bootstrap과 non-4K layout 문제를 별도 트랙으로 정리

## Template Strategy

`robottemplete`는 계속 slim 유지가 원칙이다.

- 기본 템플릿: FR5 visual / prefab / URDF / minimal interaction
- 확장 템플릿 또는 add-on: live adapter / diagnostics / safe motion

즉, 최종 목표는 "실기 연동이 검증된 뒤 그 중 공통적인 live adapter 층만 별도 템플릿화"다.

## Latest Execution Note

2026-04-28 기준 로컬 실행:

- `unityctl check --type compile`
  - PASS
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.Fr5LiveReadbackTests`
  - PASS (`7 passed`)
- `unityctl status --wait --json`
  - PASS (`Ready`, `bridgeLoaded=true`, `ipcPipePresent=true`)
- 구현 커밋:
  - `d8c0726 Add FR5 readback-only live monitor`
- 정책:
  - 맥북 field session은 `main`이 아니라 `codex/robotcontrol-v3-toolkit` 브랜치에서 먼저 수행
  - 안전 모니터링이 성공해도 live motion은 자동으로 열지 않음
  - direct C# SDK 실패 시 bridge fallback으로 readback-only 유지
  - 현재 다음 개발 우선순위는 motion enable이 아니라 `tool/user/coordSystem + motion gate 운영 노출 정리`다

2026-03-25 기준 로컬 실행:

- `unityctl check --project ...robotapp2 --json`
  - PASS
- `unityctl test --project ...robotapp2 --mode edit --filter KineTutor3D.Tests.EditMode.FairinoConnectionServiceTests --json`
  - PASS (`3 passed`)
- `unityctl exec --project ...robotapp2 --code "KineTutor3D.Editor.FairinoLiveSmokeTools.RunSmoke()"`
  - 실행 성공
  - 결과: `CONNECT_FAIL ip=192.168.58.2 port=8080 code=-2`
  - 해석: 코드 경로는 live SDK 호출까지 진입했지만, 현재 테스트 머신에서는 FR5 컨트롤러 네트워크 응답이 없음
