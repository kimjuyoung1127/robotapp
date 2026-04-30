# KineTutor3D 프로젝트 상태

최종 업데이트: 2026-04-30 (KST)

> 이 파일은 이제 `현재 상태만 짧게 보는 front page`다.
> 2026-03-31까지의 긴 누적 로그는 [PROJECT-STATUS-2026-03-31.md](../archive/completed/status/PROJECT-STATUS-2026-03-31.md)로 archive했다.

## 지금 먼저 볼 문서

1. [ACTIVE-WORK-INDEX.md](./ACTIVE-WORK-INDEX.md)
2. [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md)
3. [robotcontrol-next-session-handoff.md](../ref/product/ux/robotcontrol-next-session-handoff.md)
4. [fr5-live-field-checklist.md](../ref/product/roadmap/fr5-live-field-checklist.md)
5. [progress-checklist.md](../ref/product/pendant-v3/progress-checklist.md)

## 현재 기준선

- 현재 1순위는 `FAIRINO FR5 connect/sync + live control baseline` 안정화다.
- `Pendant V3`는 실기 readback, evidence preservation, `33ms 기본 / 50ms 폴백`까지 현재 브랜치 기준으로 잠겨 있다.
- `Easy Motion` gripper는 2026-04-29 실기 기준 `적용 -> 이동 실행 확인` 경로로 discrete live write가 실제 움직임까지 확인됐다. 남은 일은 completion readback 해석과 confirm/debug flow 단순화다.
- current branch gripper calibration은 `0% -> raw 0`으로 갱신됐고, 같은 날짜 실기 확인에서 실제 그리퍼 접촉/닫힘이 `0%`에서 맞는 것으로 확인됐다. 다만 completion-grade readback은 아직 약하다.
- `RobotControlV3DebugBridge`에는 이제 `gripper / joint / TCP / point move` 공통 Live QA runner가 들어가서, 각 시도마다 `before/after movement`, approval, popup, `latest-state/latest-drift`, session ndjson tail을 `Artifacts/live/qa/*.json`으로 묶어 남길 수 있다.
- current branch tiny joint live path는 `J1~J6` 전 축에서 실제 기기 이동과 post-run readback delta까지 확인됐다. 기존 shell increment helper와 별도로 `1~5deg 직접 입력` 경로를 추가했고, 2026-04-30 기준 `J6 true +3deg`, `J6 true +5deg`, `J5 true +5deg / -5deg`는 popup-confirm product path와 artifact 기준으로 green이다. `+6deg`는 의도대로 `tiny MoveJ range exceeded`로 차단된다. 다만 blocked 직후 헤더/진단 문구는 아직 generic `readback-only` copy로 되돌아가서 후속 copy-layer 보강이 남아 있다.
- `NavPoints` teaching point live path는 saved `MoveJ` single-point apply, `PendantV3Points` two-point one-shot, 그리고 `QA0430_MANUAL_HOME_T6` 기준 custom `2-point / multi-axis` repeatability one-shot까지 현재 branch에서 green이다. point save/apply/run 앞에는 `SyncCurrentState + RefreshLiveEvidence` 확인을 강제로 넣고, operator가 manual mode에서 실제 자세를 바꾼 경우에는 그 synced pose를 먼저 `Home` point로 저장한 뒤 repeatability를 시작한다. 다만 live loop와 broad named sequence runtime은 아직 잠겨 있다.
- current branch에는 `mode truth / auto-manual transition` 1차 보강이 들어갔다. `LiveFairinoClient`는 `robot_mode` 외 alias와 optional getter fallback을 같이 읽고, `Fr5LiveStateRecorder`는 `controller-mode / drag-teach / servo-truth` 이벤트를 남기며, runtime/service는 `SetMode` 후 requested/actual match를 재확인한다.
- 2026-04-30 기준 `연결 + 위치 읽기` direct debug는 별도 success pattern으로 분리됐다. 기준은 `RobotControlV3` 재진입 -> `BtnConnect` 1회 -> refreshed `latest-state.json` / session `events.ndjson` 확인이며, disabled read failure가 나오면 background poll을 계속 두드리지 않도록 current branch에서 보강했다.
- 2026-04-29 field 확인 기준 외부 teach pendant `manual <-> auto` 토글 시 `latest-state.json`의 `mode` truth는 정상적으로 따라온다.
- `Pendant V3` 상단 헤더에는 `자동 / 수동` 버튼이 추가됐고, 이 버튼들은 같은 verified mode-transition path를 직접 호출한다.
- 2026-04-29 batch field smoke 기준 헤더 `수동 -> 자동` 왕복은 실기 `mode=0 -> 1 -> 0`으로 확인됐다.
- 다음 product track은 기존 teach pendant 없이 `auto/manual` 전환을 앱이 직접 책임지는 경로에서 `normal-case green` 다음 단계인 failure/recovery wording을 닫는 것이다. 기준선은 `fr5-auto-manual-mode-transition-plan.md`에 잠갔다.
- `Pendant V3` 내부 구조 정리는 진행 중이며, `RobotControlV3RuntimeController` 1차 분해 뒤 `PointMoveController`와 `RobotControlV3DebugBridge` 폴더화/partial 분해까지 반영됐다. 현재 검증 truth는 `dotnet build` 및 `RobotControlV3GizmoBehaviorTests` green, `RobotControlV3HardcodingGuardTests` 재검증만 Unity IPC 회복 이슈로 남아 있다.
- `main`에 바로 기대지 말고, 현장 기준 문서는 `FR5-LIVE-INTEGRATION-ROADMAP`과 `robotcontrol-next-session-handoff`를 먼저 본다.
- `V1/V2`는 아직 삭제 대상이 아니다. V3 실기 검증과 rollback evidence가 더 쌓인 뒤 별도 제거 판단을 한다.

## 완료되어 archive로 넘긴 큰 보드

- 상세 누적 상태 로그:
  [PROJECT-STATUS-2026-03-31.md](../archive/completed/status/PROJECT-STATUS-2026-03-31.md)
- 구 `RobotControl` 구현 보드:
  [ROBOTCONTROL-IMPL-BOARD-2026-04-01.md](../archive/completed/status/ROBOTCONTROL-IMPL-BOARD-2026-04-01.md)
- `realvirtual` 제거 계획:
  [REALVIRTUAL-REMOVAL-PLAN-2026-03-31.md](../archive/completed/status/REALVIRTUAL-REMOVAL-PLAN-2026-03-31.md)

## 아직 안 한 것

- [ ] 운영자가 디버그 브리지 없이도 믿을 수 있는 `tool/user/coordSystem + motion gate` 상시 노출 최종 정리
- [ ] `33ms -> 50ms` 자동 폴백이 실제 read 오류 상황에서도 기대대로 동작하는 field verification
- [ ] MacBook Unity Editor `4K UHD` 외 해상도에서 V3 레이아웃 안정화
- [ ] `PlayMode/bootstrap` 공통 초기화 실패 정리
- [ ] current direct-input tiny path의 `5deg` acceptance를 다른 일부 축으로 넓힌다
- [ ] 헤더 `다음 행동`과 Safety/Diagnostics taxonomy가 실제 failure state(`tiny range exceeded`, `activation not ready`, `mode mismatch`)에서 generic copy로 되돌아가지 않게 보강한다
- [ ] teaching point live loop와 broad named sequence runtime을 언제 열지 범위를 결정한다
- [ ] `Home` 기준 `2-point / multi-axis` repeatability green 범위를 broad named sequence generalization으로 넓힐지 결정한다
- [ ] 기존 teach pendant 없이 `auto/manual` 전환을 앱이 직접 책임지는 operator flow에 recovery copy와 실패 원인 표기를 보강한다
- [ ] drag/servo/fault exceptional case에서 mode 전환 실패 taxonomy를 운영자 문구로 정리한다

## 상태 해석 규칙

- 실기 truth는 `Unity 화면`이 아니라 `FR5 controller readback`이다.
- latest evidence는 `Artifacts/live/fr5/latest-state.json`, `latest-drift.json`, `sessions/*-events.ndjson`, `sessions/*-readback.ndjson`를 같이 본다.
- invalid zero/disconnect follow-up state는 실패가 아니라 preservation 보호 동작일 수 있다.
- `PRODUCT-DOC-BOARD.md`는 canonical product docs 상태용으로 계속 유지한다.
- `PHASE-EXECUTION-BOARD.md`는 broad phase board로 남겨두되, 현재 FR5/V3 실무 우선순위는 `ACTIVE-WORK-INDEX.md`를 먼저 본다.
