---
title: "Active Work Index"
doc_type: "status-index"
status: "active"
domain: "status"
audience: "human-and-agent"
canonical: true
last_updated: "2026-04-30"
---

# Active Work Index

최종 업데이트: 2026-04-30 (KST)

## 목적

- 지금 당장 봐야 하는 문서만 앞에 둔다.
- 이미 끝난 큰 보드는 archive로 보내고, 남은 일만 따로 모아 본다.
- `FR5 실기 연동`과 `Pendant V3`의 현재 열린 이슈를 한눈에 본다.

## 지금 읽는 순서

1. [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md)
2. [robotcontrol-next-session-handoff.md](../ref/product/ux/robotcontrol-next-session-handoff.md) - 다음 FR5 운영 세션용 짧은 handoff
3. [fr5-live-official-sdk-audit.md](../ref/product/roadmap/fr5-live-official-sdk-audit.md)
4. [fr5-live-field-checklist.md](../ref/product/roadmap/fr5-live-field-checklist.md) - 현재 세션 체크리스트만 유지
5. [fr5-gripper-live-success-pattern.md](../ref/product/roadmap/fr5-gripper-live-success-pattern.md) - gripper live SSOT
6. [fr5-tiny-joint-live-success-pattern.md](../ref/product/roadmap/fr5-tiny-joint-live-success-pattern.md) - tiny joint live SSOT
7. [fr5-teaching-point-live-success-pattern.md](../ref/product/roadmap/fr5-teaching-point-live-success-pattern.md) - point/sequence live SSOT
8. [fr5-auto-manual-mode-transition-plan.md](../ref/product/roadmap/fr5-auto-manual-mode-transition-plan.md)
9. [fr5-connect-sync-debug-success-pattern.md](../ref/product/roadmap/fr5-connect-sync-debug-success-pattern.md)
10. [progress-checklist.md](../ref/product/pendant-v3/progress-checklist.md)
11. 과거 현장 서사가 필요할 때만 [fr5-live-field-history.md](../ref/product/roadmap/fr5-live-field-history.md)
12. 필요할 때만 [PRODUCT-DOC-BOARD.md](./PRODUCT-DOC-BOARD.md), [PHASE-EXECUTION-BOARD.md](./PHASE-EXECUTION-BOARD.md)

## 현재 활성 트랙

### 1. FR5 실기 연동

- 상태: `readback-only baseline 확보`
- 현재 truth:
  - direct readback 성공
  - latest evidence preservation 복구
  - Unity 재기동 후에도 `연결 + 위치 읽기` 뒤 시뮬레이션/실기 자세 동기화 확인
  - `33ms 기본 / 50ms 폴백` 정책 고정 및 강제 오류 상황 검증 완료
  - tiny `MoveJ` live path는 기존 live 세션 재사용까지 반영됨
  - `eth0 = 192.168.57.2` 변형 포트에서도 live reconnect / sync green 확인
  - Unity gripper live write는 `gripper-only` 기준으로 실제 readback 변화까지 확인
  - gripper는 이제 `Easy Motion -> 적용 -> 이동 실행 확인` operator flow로 discrete live write green
  - current branch gripper calibration은 `user 0% -> raw 0`으로 잠겼고, 실기 `0%` 접촉/닫힘도 operator 기준으로 확인됐다
  - `gripper / joint / TCP / point move` 공통 Live QA runner가 추가되어 `before/after state + approval + evidence`를 `Artifacts/live/qa/*.json`으로 남길 수 있음
  - official SDK audit SSOT가 추가되어 `공식 문서 의도`와 `실기 truth`를 분리한 비교 기준이 잠겼다
- current branch tiny joint live path는 이제 `J1~J6` 전 축에서 실제 기기 이동 + post-sync readback까지 확인됐다
- `연결 + 위치 읽기` direct debug는 별도 success pattern으로 분리됐고, current branch는 disabled read failure가 반복되면 background poll을 멈추도록 보강됐다
  - shell increment helper와 분리된 `1~5deg 직접 입력` 경로가 추가됐고, 2026-04-30 기준 `J6 true +3deg`, `J6 true +5deg`, `J5 true +5deg / -5deg`는 popup-confirm product path + artifact 기준으로 green이다
  - `+6deg`는 의도대로 `tiny MoveJ range exceeded`로 차단된다
  - 다만 blocked 직후 헤더/진단 surface는 아직 generic readback-only 문구로 되돌아가서 copy-layer 후속이 남아 있다
  - saved `MoveJ` single-point live apply와 `PendantV3Points` two-point live run-once는 현재 branch에서 green이다
  - point/sequence live 단계에는 `현재 위치 읽기 -> Sync + RefreshLiveEvidence -> Unity pose와 latest-state 일치 확인`이 필수 preflight로 들어갔다
  - operator가 manual mode에서 실제 로봇 자세를 바꾼 경우, 그 synced pose를 먼저 `Home` point로 저장한 뒤에만 `2-point / multi-axis` repeatability를 시작한다
  - `QA0430_MANUAL_HOME_T6` 기준 `QA0430_HOME_2PT_T6`와 `QA0430_HOME_MULTIAXIS_T5_T6` one-shot repeatability는 same-session live re-smoke까지 green이다
  - teaching point live 범위는 현재 `single point / two-point once / Home-based custom one-shot / loop locked`로 잠겨 있다
  - tiny joint truth는 immediate artifact가 아니라 post-run `SyncCurrentStateForDebug()` + `RefreshLiveEvidenceForDebug()` + `Artifacts/live/fr5/latest-state.json`으로 판정한다
  - current branch에는 `controller mode truth` 보강이 들어가서 `LiveFairinoClient`가 `robot_mode`뿐 아니라 `robotMode/mode` alias와 optional getter fallback을 같이 읽고, `Fr5LiveStateRecorder`가 `controller-mode / drag-teach / servo-truth` 이벤트를 남긴다
  - `auto/manual` 전환도 이제 요청만 보내는 경로가 아니라 `SetMode -> SyncCurrentState retry -> requested/actual mode match 확인`까지 service/runtime에서 묶인다
  - 외부 teach pendant `manual <-> auto` 토글 시 `latest-state.json`의 `mode` truth가 따라오는 것은 2026-04-29 기준으로 field 확인됐다
  - `Pendant V3` 상단 헤더에는 `자동 / 수동` 버튼이 추가됐고, 같은 verified mode-transition path를 operator surface에서 직접 호출할 수 있게 코드상 연결됐다
  - 2026-04-29 batch field smoke 기준 헤더 `수동 -> 자동` 왕복이 실기 `mode=0 -> 1 -> 0`으로 확인됐다
  - V3 `QuickControllerMode`에는 현재 controller truth와 마지막 mode transition summary가 같이 노출된다
  - arm track의 현재 open item은 더 이상 "tiny MoveJ가 아예 안 된다"가 아니라 `true 3deg helper`, `multi-cycle repeatability`, `TCP`, `manual recovery wording` 확장이다
- SSOT:
  - [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md)
  - [fr5-live-field-checklist.md](../ref/product/roadmap/fr5-live-field-checklist.md)
  - [fr5-gripper-live-success-pattern.md](../ref/product/roadmap/fr5-gripper-live-success-pattern.md)
  - [fr5-tiny-joint-live-success-pattern.md](../ref/product/roadmap/fr5-tiny-joint-live-success-pattern.md)
  - [fr5-teaching-point-live-success-pattern.md](../ref/product/roadmap/fr5-teaching-point-live-success-pattern.md)
  - [fr5-auto-manual-mode-transition-plan.md](../ref/product/roadmap/fr5-auto-manual-mode-transition-plan.md)
  - [robotcontrol-next-session-handoff.md](../ref/product/ux/robotcontrol-next-session-handoff.md)

### 2. Pendant V3 운영 노출

- 상태: `실기 readback과 연결됨, 운영자용 노출은 일부 후속`
- SSOT:
  - [progress-checklist.md](../ref/product/pendant-v3/progress-checklist.md)
  - [README.md](../ref/product/pendant-v3/README.md)

## 아직 안 한 것만 보기

| 상태 | 항목 | 어디를 보면 되는지 |
|---|---|---|
| open | official SDK audit `P0`를 완결: status/readback gap inventory, gripper completion-readback rule, IO app-contract gap 고정 | [fr5-live-official-sdk-audit.md](../ref/product/roadmap/fr5-live-official-sdk-audit.md), [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md) |
| open | official SDK audit `P1`: current `J5/J6 both directions + same-day repeatability`를 `multi-cycle repeatability`와 next-joint expansion 기준으로 넓히기 | [fr5-live-official-sdk-audit.md](../ref/product/roadmap/fr5-live-official-sdk-audit.md), [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md), [fr5-tiny-joint-live-success-pattern.md](../ref/product/roadmap/fr5-tiny-joint-live-success-pattern.md) |
| open | official SDK audit `P2`: tiny joint green을 바탕으로 joint/TCP operator/debug flow와 auto-mode dependency를 축약 | [fr5-live-official-sdk-audit.md](../ref/product/roadmap/fr5-live-official-sdk-audit.md), [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md), [fr5-auto-manual-mode-transition-plan.md](../ref/product/roadmap/fr5-auto-manual-mode-transition-plan.md) |
| open | `Easy Motion` gripper confirm/debug flow를 단순화하고 `미리보기 적용 / 실제 이동` 분리 semantics를 실기에서 다시 검증 | [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md), [fr5-gripper-live-success-pattern.md](../ref/product/roadmap/fr5-gripper-live-success-pattern.md) |
| open | 공통 Live QA runner로 `joint/TCP/gripper/point` field evidence를 같은 형식으로 축적 | [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md), [robotcontrol-next-session-handoff.md](../ref/product/ux/robotcontrol-next-session-handoff.md) |
| open | `Easy Motion` 슬라이더 조작이 실기 gripper를 실시간으로 따라오게 만들기 | [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md), [fr5-live-field-checklist.md](../ref/product/roadmap/fr5-live-field-checklist.md) |
| open | tiny joint direct-input 성공패턴의 `5deg` acceptance를 다른 일부 축으로 넓힌다 | [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md), [fr5-tiny-joint-live-success-pattern.md](../ref/product/roadmap/fr5-tiny-joint-live-success-pattern.md) |
| open | 헤더 `다음 행동` + Safety/Diagnostics taxonomy가 `tiny range exceeded` 같은 blocked state에서도 generic readback-only 문구로 되돌아가지 않게 보강한다 | [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md) |
| open | teaching point live는 `single point / two-point once / Home-based custom one-shot`까지 green이다. 다음은 broad named sequence generalization과 live loop 범위를 어디까지 열지 결정하는 일이다 | [fr5-teaching-point-live-success-pattern.md](../ref/product/roadmap/fr5-teaching-point-live-success-pattern.md), [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md) |
| open | 상단 헤더 `자동 / 수동` 버튼은 normal-case green으로 본다. 다음은 failure/recovery wording과 drag/servo exceptional case를 정리 | [fr5-auto-manual-mode-transition-plan.md](../ref/product/roadmap/fr5-auto-manual-mode-transition-plan.md), [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md) |
| open | tiny `MoveJ` 승인 게이트를 `tool/user/coordSystem + evidence freshness + drift + operator confirm` 기준으로 묶기 | [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md) |
| open | MacBook Unity Editor 비-4K 해상도 V3 레이아웃 안정화 | [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md), [robotcontrol-next-session-handoff.md](../ref/product/ux/robotcontrol-next-session-handoff.md) |
| open | `PlayMode/bootstrap` 공통 실패 정리 | [FR5-LIVE-INTEGRATION-ROADMAP.md](./FR5-LIVE-INTEGRATION-ROADMAP.md) |
| locked | current branch에서 broad arm motion green으로 일반화하지 않음. tiny `MoveJ` success는 현재 `J5/J6` narrow path로만 본다 | [fr5-tiny-joint-live-success-pattern.md](../ref/product/roadmap/fr5-tiny-joint-live-success-pattern.md) |

## 이미 끝난 것

- 긴 누적 상태 로그:
  [PROJECT-STATUS-2026-03-31.md](../archive/completed/status/PROJECT-STATUS-2026-03-31.md)
- 구 `RobotControl` 구현 보드:
  [ROBOTCONTROL-IMPL-BOARD-2026-04-01.md](../archive/completed/status/ROBOTCONTROL-IMPL-BOARD-2026-04-01.md)
- `realvirtual` 제거 계획:
  [REALVIRTUAL-REMOVAL-PLAN-2026-03-31.md](../archive/completed/status/REALVIRTUAL-REMOVAL-PLAN-2026-03-31.md)

## 해석 팁

- `done` 문서를 먼저 읽지 않는다. archive 경로로만 본다.
- `daily` 문서는 증빙 로그다. 현재판 판단은 `status`와 `ref/product` 문서를 먼저 본다.
- `PHASE-EXECUTION-BOARD`는 broad board라서 현시점 FR5/V3 우선순위를 그대로 보여주지 못할 수 있다. 지금 실무 판단은 이 문서와 FR5 roadmap을 같이 본다.
