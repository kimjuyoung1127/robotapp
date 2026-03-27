# 2026-03-27 — Hand Teaching Validation Setup

## What I Did

- Unity Editor를 재시작했다.
- 외부 검증용 오픈소스 레퍼런스를 프로젝트 밖 폴더에 내려받았다.
- 현재 버전 기준의 `Hand Teaching Mode` 검증 체크리스트 문서를 추가했다.
- `ProjectSettings/UnityctlSettings.asset`를 추가해 unityctl bridge를 활성화했다.
- Android UDP sender에서 보낸 테스트 payload가 `RobotControl`의 `Hand Input` 섹션까지 도달하는 것을 확인했다.

## Restart Result

- Unity는 재시작 후 새 프로세스로 다시 올라왔다.
- `unityctl v0.3.5` 기준 bridge가 정상 초기화됐다.
- 확인 결과:
  - `state = Ready`
  - `bridgeLoaded = true`
  - `ipcPipePresent = true`

## Downloaded Repos

```text
C:\Users\ezen601\Desktop\Jason\external\hand-teaching-mode\mediapipe-samples
C:\Users\ezen601\Desktop\Jason\external\hand-teaching-mode\MediaPipeUnityPlugin
```

## Added Doc

- `docs/ref/product/robots/hand-teaching-mode-validation-checklist.md`

## Notes

- 현재 권장 검증 경로는 `폰 MediaPipe -> robotapp2 value receive -> preview teaching`이다.
- `MediaPipeUnityPlugin`은 비교 검토용으로 받아뒀지만, 메인 프로젝트 직접 통합은 보류한다.
- 현재 Step 1은 "preview를 움직이는 것"이 아니라 "연결 신호가 실제로 앱 안에 들어오는지 확인"이다.
- 이번 검증에서 Android UDP sender payload는 `Hand Input` 섹션에 아래 값으로 반영됐다.
  - `Sender: 192.168.0.23`
  - `Seq: 1`
  - `Tracked: Yes`
  - `X/Y: 0.12 / -0.34`
  - `Pinch: 0.77`
  - `Source: android-debug`
- 표시 시점에는 timeout 때문에 `Fresh`가 아니라 `Stale`였지만, 이는 패킷이 도착했다는 증거다.
