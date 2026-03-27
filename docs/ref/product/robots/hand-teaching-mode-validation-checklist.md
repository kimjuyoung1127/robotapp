# Hand Teaching Mode Validation Checklist

## Purpose

- 현재 `robotapp2` 버전에서 `Hand Teaching Mode`를 안전하게 시작할 수 있는지 먼저 검증한다.
- 메인 프로젝트에 바로 무거운 플러그인을 심기 전에, 가장 낮은 리스크의 연동 경로를 단계별로 확인한다.

## Current Project Baseline

- Unity Editor: `6000.0.64f1`
- Render Pipeline: `URP 17.0.4`
- Input: `Input System 1.17.0`
- UI: `UGUI 2.0.0`
- Robot path: `URDF Importer v0.5.2`

## Downloaded Reference Libraries

외부 검증 자료는 메인 프로젝트 밖의 별도 폴더에 받는다.

```text
C:\Users\ezen601\Desktop\Jason\external\hand-teaching-mode\mediapipe-samples
C:\Users\ezen601\Desktop\Jason\external\hand-teaching-mode\MediaPipeUnityPlugin
```

### Why These Two

- `mediapipe-samples`
  - 폰 hand tracking 쪽의 가장 안전한 기준선
- `MediaPipeUnityPlugin`
  - Unity 직접 통합이 정말 필요한지 나중에 비교 검증할 기준

## Validation Strategy

- 1차 목표는 `폰 hand tracking -> robotapp2 preview 연동`이다.
- 2차 목표는 `save pose` teaching loop 확인이다.
- `MediaPipeUnityPlugin`을 메인 프로젝트에 바로 넣는 것은 이 1차 검증이 끝난 뒤로 미룬다.

## Phase 0. Editor Baseline

- [x] Unity Editor가 정상 재시작되었는지 확인
- [x] `unityctl` bridge가 `Ready` 상태로 올라오는지 확인
- [ ] `unityctl check --type compile`가 통과하는지 확인
- [ ] 현재 씬/패키지 기준선이 restart 전과 동일한지 확인

### Confirmed Baseline Result

- `ProjectSettings/UnityctlSettings.asset`를 추가해 `com.unityctl.bridge` bootstrap을 활성화했다.
- 최신 릴리스 `unityctl v0.3.5` 기준 `status` 결과:
  - `state = Ready`
  - `bridgeLoaded = true`
  - `ipcPipePresent = true`

## Phase 1. External Hand Tracking Baseline

- [ ] `mediapipe-samples` 안의 Android hand landmarker 샘플을 빌드 가능한 상태로 확인
- [ ] 폰에서 hand landmark 또는 축약 control value를 송신할 수 있는지 결정
- [ ] 최초 전송 포맷을 아래 둘 중 하나로 고정
  - `simple control values`
  - `raw landmarks`

### Recommended First Payload

```json
{
  "seq": 1,
  "handX": 0.12,
  "handY": -0.34,
  "pinch": 0.77,
  "tracked": true
}
```

## Phase 2. robotapp2 Receive Path

- [x] `Assets/Scripts/App/HandTracking/` 폴더 추가
- [x] UDP receiver 추가
- [x] 수신값 clamp / deadzone / timeout 처리 추가
- [x] preview-only 모드에서만 입력 허용
- [ ] FR5 preview가 손 입력에 따라 움직이는지 확인

### Step 1 Result

- [x] `RobotControl` 런타임에 `HandTrackingReceiver` 생성 확인
- [x] Android UDP sender에서 테스트 payload 송신 확인
- [x] Diagnostics drawer의 `Hand Input` 섹션에서 payload 수신 확인
- [x] Fresh 후 timeout에 따라 `Stale`로 전환되는 것 확인

### Observed Runtime Signal

- `Hand Input: Stale`
- `Port: 5005`
- `Sender: 192.168.0.23`
- `Seq: 1`
- `Tracked: Yes`
- `X/Y: 0.12 / -0.34`
- `Pinch: 0.77`
- `Source: android-debug`

해석:

- 패킷은 실제로 도착했다.
- 현재 timeout이 `0.5 sec`라서, 확인 시점에는 `Fresh`에서 `Stale`로 넘어가 있었다.
- 즉 Step 1의 목적이었던 "`폰 -> UDP -> robotapp2` 최소 연결 확인"은 달성했다.

## Phase 3. Hand Teaching Loop

- [ ] 현재 preview pose snapshot 가능
- [ ] `Save Pose`로 저장 가능
- [ ] 저장 목록에서 pose 재적용 가능
- [ ] 이름 변경 / 덮어쓰기 / 삭제 가능
- [ ] 저장된 pose가 preset/waypoint 확장에 재사용 가능한 형식인지 확인

## Required Locks Before Any Live Test

- [ ] preview-only 기본값
- [ ] 허용 sender IP 1대 고정
- [ ] 고정 포트 사용
- [ ] 입력 clamp
- [ ] deadzone
- [ ] max joint delta
- [ ] timeout stop
- [ ] explicit arm gate
- [ ] live robot send 기본 OFF

## Compatibility Verdict So Far

### Safe Now

- `폰에서 MediaPipe hand tracking`
- `robotapp2는 landmark/control 값만 수신`
- `preview + teaching loop`

### Defer

- `MediaPipeUnityPlugin` 직접 메인 프로젝트 통합
- `ServoJ` live send
- `ServoCart`
- `TCP hand-follow`

## Practical Go / No-Go Check

Go:

- Unity IPC bridge ready
- 폰이 값 송신 가능
- robotapp2가 값 수신 가능
- Diagnostics drawer에서 hand input 상태 확인 가능
- pose 저장/재적용 가능

No-Go:

- Unity compile baseline 미통과
- domain reload / IPC 불안정
- hand input jitter가 deadzone/smoothing 없이 과도함
- control prefab 기준선이 깨져 preview 검증 자체가 불가능함

## First Command Retry After Restart

```powershell
& 'C:\Users\ezen601\Desktop\Jason\unityctl\src\Unityctl.Cli\bin\Debug\net10.0\unityctl.exe' status --project 'C:\Users\ezen601\Desktop\Jason\robotapp2' --wait --json
& 'C:\Users\ezen601\Desktop\Jason\unityctl\src\Unityctl.Cli\bin\Debug\net10.0\unityctl.exe' check --project 'C:\Users\ezen601\Desktop\Jason\robotapp2' --type compile --json
```
