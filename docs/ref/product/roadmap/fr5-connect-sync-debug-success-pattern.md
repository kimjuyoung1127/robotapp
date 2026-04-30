# FR5 Connect Sync Debug Success Pattern

최종 업데이트: 2026-04-30 (KST)

## 목적

- `RobotControlV3`의 `연결 + 위치 읽기` 경로를 디버깅할 때 매번 같은 순서로 재현하고 판정하기 위한 좁은 SSOT다.
- 이 문서는 `gripper`, `tiny joint`, `teaching point`보다 앞단의 baseline인 `connect + sync` 안정화만 다룬다.

## 현재 기준선

- current branch 기본 live 세션 모드는 `LiveControl`이다.
- Home `BtnConnect`는 operator 기준 `연결 + 위치 읽기` 1회 동작으로 본다.
- 2026-04-30 direct test 기준 `BtnConnect` 실제 click 후:
  - `latest-state.json`은 새 session으로 갱신됐다
  - `connected=true`
  - `enabled=true`
  - `mode=1`
  - `toolId=1`, `userId=1`, `coordSystem=Base`
- 같은 test에서 session `events.ndjson`은 `latest-state updated ...`를 연속 기록했고, 예전처럼 즉시 `FR5 연결 해제`로 떨어지지 않았다.

## 이번에 잠근 디버깅 규칙

1. `latest-state.json`이 헤더 표시보다 더 강한 truth다.
2. `mode/enabled`가 UI와 다르면 `readback evidence`를 우선한다.
3. `연결 위치읽기`가 멈춘 것처럼 보여도 먼저 세 가지를 분리한다.
   - Unity crash
   - Unity IPC churn
   - controller disabled/manual readback stall
4. `비활성 상태입니다. Enable 버튼을 눌러주세요.`가 readback에서 나오면, background poll을 계속 두드리지 말고 그 상태를 operator-facing 실패로 유지한다.
5. `RobotControlV3`는 Play 후 `Onboarding`으로 auto-route될 수 있으므로, debug 시 항상 active scene을 먼저 확인한다.

## 직접 재현 절차

1. Unity가 살아 있는지 확인한다.
2. `RobotControlV3`로 다시 진입한다.
3. `BtnConnect`를 1회 누른다.
4. 2~3초 뒤 아래를 본다.
   - `Artifacts/live/fr5/latest-state.json`
   - current session `Artifacts/live/fr5/sessions/*-events.ndjson`
5. 필요할 때만 `Editor.log`를 추가로 본다.

## 성공 판정

- `BtnConnect` click 자체가 실제로 들어간다.
- `latest-state.json` timestamp가 현재 session으로 갱신된다.
- `toolId > 0`, `userId > 0`, `coordSystem` truth가 같이 남는다.
- session events가 `readback updated`를 계속 찍는다.
- 즉시 disconnect loop로 떨어지지 않는다.

## 실패 판정 분류

### 1. Unity crash

- 새 `Unity-*.ips` crash report가 생긴다.
- 우선순위는 UI/binder recursion 정리다.

### 2. Unity IPC churn

- Unity는 살아 있는데 `unityctl exec`만 pipe close로 흔들린다.
- 이 경우 operator path와 robot path를 분리해서 본다.

### 3. Controller disabled/manual readback stall

- `latest-state.json` 또는 `events.ndjson`에서 `enabled=false`, `mode=1` 또는 disabled/not-ready 에러가 보인다.
- 이 경우 `connect/sync` path가 아니라 controller truth mismatch가 blocker다.

## 현재 코드 기준으로 잠근 변화

- `FairinoConnectionService`는 disabled read failure가 반복되면 poll을 중단한다.
- `LiveFairinoClient`는 realtime state에서 `enabled` truth를 같이 갱신한다.
- `LiveFairinoClient`는 `sdk version`을 매 readback마다 다시 읽지 않는다.
- async connect/sync 실패는 runtime snapshot에 remembered blocked reason으로 남긴다.

## 해석 팁

- `enabled`와 `isRobotEnabled`가 혼재할 수 있으므로 둘 다 본다.
- `mode=1`이 곧 crash 원인은 아니다. 먼저 live readback이 계속 갱신되는지 본다.
- `Onboarding`에 있으면 `BtnConnect` 디버깅은 무의미하다.

## 다음 단계

- 이 baseline이 green이면 그다음에만 `gripper`, `joint`, `point/sequence` live test로 넘어간다.
