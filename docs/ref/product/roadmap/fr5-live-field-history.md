# FR5 Live Field History

## 목적

- 이 문서는 `현재 운영 체크리스트`에서 분리한 과거 현장 서사와 시행착오를 보존한다.
- 현재 판단은 [fr5-live-field-checklist.md](./fr5-live-field-checklist.md), [fr5-gripper-live-success-pattern.md](./fr5-gripper-live-success-pattern.md), [fr5-tiny-joint-live-success-pattern.md](./fr5-tiny-joint-live-success-pattern.md)를 먼저 본다.
- 이 문서는 `왜 현재 기준선이 생겼는지`를 설명하는 역사 보관용이다.

## 역사 타임라인

### 2026-04-28 1차 현장 기준선

- MacBook `unityctl` bridge 문제의 실제 원인은 macOS 미지원이 아니라 `Packages/manifest.json`에 남아 있던 Windows `com.unityctl.bridge` 경로였다.
- `unityctl init --project /Users/family/jason/FR5UNITY/robotapp --source /Users/family/jason/unityctl/src/Unityctl.Plugin`로 맥 로컬 bridge를 다시 연결했다.
- direct SDK readback smoke가 `192.168.58.2:8080`에서 green이었다.
- `현재 위치 읽기` 후 Unity 시뮬레이션 자세와 실기 자세가 drift `0.0` 수준으로 수렴하는 baseline을 확보했다.
- 이때의 핵심 교훈은 `실기 truth는 controller readback`이고, sync 전 화면 상태를 truth로 보면 안 된다는 점이었다.

### 2026-04-28 evidence / preservation 정리

- `latest-state.json`, `latest-drift.json`, session ndjson이 현재 실기 truth 판정의 기본 증거 체계로 굳어졌다.
- disconnected zero state나 placeholder zero state가 마지막 정상 live evidence를 덮지 않도록 preservation policy를 넣었다.
- `33ms 기본 / 50ms 폴백` 정책도 같은 날 field verification으로 고정됐다.

### 2026-04-28 tiny MoveJ 초기 재도전

- tiny `MoveJ`는 새 motion RPC를 따로 열지 않고 기존 live 세션을 재사용하도록 바꿨다.
- 그 뒤 남은 blocker는 `readback-only` 자체가 아니라 controller `fault=1/1`이었다.
- `ResetErrors()`는 `0/OK`였지만 fault latch는 풀리지 않았다.
- 이 시점부터 tiny joint 문제는 `게이트 설계`보다 `controller fault clear 조건` 문제로 분리됐다.

### 2026-04-28 포트 분기와 teach pendant 재연결

- FR5 controller 포트가 `eth0 = 192.168.57.2`, `eth1 = 192.168.58.2`로 갈린다는 사실을 field에서 확인했다.
- 맥북 Ethernet IP는 실제 꽂힌 포트 대역과 맞춰야 한다는 운영 기준이 이때 생겼다.
- legacy teach pendant를 다시 연결한 뒤 gripper diagnostics가 바뀌었고, historical readback config도 여러 번 달라졌다.
- 이 구간의 핵심 교훈은 `gripper 문제를 arm live blocker와 섞어 해석하지 말 것`이었다.

### 2026-04-28 gripper 485 시행착오

- 팬던트에서 `그리퍼 485 시간초과`, `클램프 이동오류`가 확인됐다.
- robot arm joint motion은 같은 팬던트에서 정상이라 문제 범위가 `gripper RS-485 chain`으로 좁혀졌다.
- gripper LED red blink는 field에서 보였지만, later session에서 항상 hard-fault 신호로 해석할 수는 없다는 점도 같이 남겼다.
- 이 구간의 핵심 교훈은 `gripper adapted-device/profile mismatch`와 `485 chain issue`를 분리해서 봐야 한다는 점이었다.

### 2026-04-29 operator baseline 재정의

- V3 Home `연결`을 `connect + 현재 위치 읽기` 1단계로 묶었다.
- top-bar `현재 위치 읽기` 버튼은 제거했다.
- 이때부터 operator baseline은 `연결 1회 -> sync 완료 확인`으로 바뀌었다.

### 2026-04-29 gripper success pattern 고정

- teach pendant 정상 설정이 `DAHUAN / PGI-140 / D1.0 / 말단 1번 포트`로 확인됐다.
- Unity expected gripper profile도 이 기준선으로 재정렬됐다.
- `gripper-only` 세션에서 Unity discrete live write가 다시 green이 되었고, `100 -> 70 -> 100`, `50 -> hold` 같은 성공 패턴이 확보됐다.
- 이후 그리퍼 상세는 [fr5-gripper-live-success-pattern.md](./fr5-gripper-live-success-pattern.md)에 따로 잠갔다.

### 2026-04-29 tiny joint success pattern 확장

- tiny joint live path는 `J1~J6` effective tiny move까지 narrow green으로 넓어졌다.
- 다만 true `3deg` helper는 아직 미검증이라, success pattern 문서에는 `effective tiny move`와 `true 3deg`를 분리해서 남겼다.

## 역사 문서로만 남길 것

- 과거 `192.168.58.2` 중심 runbook bulk
- bridge 복구 시행착오 전체 서사
- old gripper profile trial-and-error 상세
- teach pendant 재연결 중간 상태
- V1/V2 삭제 전 긴 구현 전략과 rollback 전제

## 현재 문서와의 역할 분리

- 현재 세션 체크: [fr5-live-field-checklist.md](./fr5-live-field-checklist.md)
- gripper 현재 SSOT: [fr5-gripper-live-success-pattern.md](./fr5-gripper-live-success-pattern.md)
- tiny joint 현재 SSOT: [fr5-tiny-joint-live-success-pattern.md](./fr5-tiny-joint-live-success-pattern.md)
- 다음 운영 세션 요약: [robotcontrol-next-session-handoff.md](../ux/robotcontrol-next-session-handoff.md)
