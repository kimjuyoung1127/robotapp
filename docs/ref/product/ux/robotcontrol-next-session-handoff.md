# RobotControl Next Session Handoff

## 목적

- 이 문서는 `다음 FR5 실기 세션`에서 바로 필요한 운영 정보만 담는다.
- 과거 현장 서사와 시행착오는 [fr5-live-field-history.md](../roadmap/fr5-live-field-history.md)에 따로 보존한다.
- 상세 gripper 절차는 [fr5-gripper-live-success-pattern.md](../roadmap/fr5-gripper-live-success-pattern.md)를 SSOT로 본다.
- arm tiny joint 절차는 [fr5-tiny-joint-live-success-pattern.md](../roadmap/fr5-tiny-joint-live-success-pattern.md)를 SSOT로 본다.
- teaching point live 절차는 [fr5-teaching-point-live-success-pattern.md](../roadmap/fr5-teaching-point-live-success-pattern.md)를 SSOT로 본다.
- `연결 + 위치 읽기` 디버깅 절차는 [fr5-connect-sync-debug-success-pattern.md](../roadmap/fr5-connect-sync-debug-success-pattern.md)를 SSOT로 본다.

## 먼저 읽는 순서

1. [FR5-LIVE-INTEGRATION-ROADMAP.md](../../../status/FR5-LIVE-INTEGRATION-ROADMAP.md)
2. [fr5-live-field-checklist.md](../roadmap/fr5-live-field-checklist.md)
3. [fr5-gripper-live-success-pattern.md](../roadmap/fr5-gripper-live-success-pattern.md)
4. [fr5-tiny-joint-live-success-pattern.md](../roadmap/fr5-tiny-joint-live-success-pattern.md)
5. [fr5-teaching-point-live-success-pattern.md](../roadmap/fr5-teaching-point-live-success-pattern.md)
6. [fr5-connect-sync-debug-success-pattern.md](../roadmap/fr5-connect-sync-debug-success-pattern.md)

## 현재 운영 기준

- 기본 연결 흐름은 `연결 = connect + 현재 위치 읽기`다.
- 현재 MacBook FR5 live baseline은 `eth0 = 192.168.57.2`, `eth1 = 192.168.58.2`다.
- current branch 기본 세션 모드는 `LiveControl`이다.
- broad arm motion은 열지 않는다.
- arm live motion은 `tiny joint success pattern` 범위 안에서만 다룬다.
- gripper live write는 `gripper-only` 세션에서만 다룬다.
- `Easy Motion` gripper 조작은 `100 / 50 / 0` value selector + `미리보기 적용 / 실제 이동` 2버튼 분리 기준으로 본다.
- teaching point live는 현재 `single point / two-point once`만 다룬다.
- 실기 truth 판정은 UI 문구보다 `Artifacts/live/fr5/latest-state.json`, `latest-drift.json`, session ndjson evidence를 우선한다.
- teaching point/sequence 단계마다 `현재 위치 읽기 -> Sync + RefreshLiveEvidence -> Unity pose와 latest-state 일치 확인`을 끼워 넣는다.
- `연결 위치읽기`가 멈춘 것처럼 보이면 먼저 `latest-state.json`과 current session `events.ndjson`으로 `enabled/mode` truth를 확인한다.

## 다음 세션 시작 절차

1. MacBook 네트워크가 현재 꽂힌 포트 대역과 맞는지 확인한다.
2. Unity를 재기동하고 `RobotControlV3`에 다시 진입한다.
3. `연결` 버튼 1회로 `connect + 현재 위치 읽기`를 같이 수행한다.
4. `latest-state.json`이 fresh한지 확인한다.
5. `toolId > 0`, `userId > 0`, `coordSystem` truth, drift `ok`를 확인한다.
6. 이번 세션이 `gripper-only`인지 `tiny-movej-only`인지 먼저 잠근다.
7. operator가 manual mode에서 실제 자세를 바꿨다면, 그 synced pose를 먼저 `Home` point로 저장하고 나서 teaching repeatability를 시작한다.

## 현재 green baseline

- direct readback reconnect는 green이다.
- `연결` 1회로 sync까지 가는 operator flow는 green이다.
- `33ms 기본 / 50ms 폴백` 검증은 닫혔다.
- gripper discrete live write는 green이다.
- tiny joint live write는 success pattern 문서 범위에서만 narrow green이다.
- teaching point live는 saved `MoveJ` single-point apply와 `PendantV3Points` two-point one-shot까지 green이다.
- `Home` 기준 custom `2-point / multi-axis` repeatability는 live green이다.
- 현재 stage 이름:
  - `QA0430_MANUAL_HOME_T6`
  - `QA0430_HOME_2PT_T6`
  - `QA0430_HOME_MULTIAXIS_T5_T6`

## 현재 open item

- `Easy Motion` gripper slider가 실기를 연속 추종하게 만드는 작업
- `Easy Motion` preview button / live button 분리 semantics를 실기에서 다시 검증
- gripper operator confirm/debug flow 단순화
- tiny joint `true 3deg` 검증 경로
- teaching point live loop / broad named sequence runtime
- auto/manual exceptional recovery wording
- non-4K 레이아웃 안정화
- PlayMode/bootstrap 공통 실패 정리

## V1 / V2 메모

- `V1`, `V2`는 계속 확장할 현행 운영 표면이 아니다.
- 남겨둘 것은 `왜 만들었는지`, `어떤 개발 이력이 있었는지` 수준의 역사 정보뿐이다.
- 다음 세션 운영 판단은 V1/V2 구현 전략 문서가 아니라 현재 FR5 live SSOT와 success pattern 문서를 기준으로 한다.
