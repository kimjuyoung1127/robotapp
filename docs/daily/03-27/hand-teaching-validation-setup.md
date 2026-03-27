# 2026-03-27 — Hand Teaching Validation Setup

## What I Did

- Unity Editor를 재시작했다.
- 외부 검증용 오픈소스 레퍼런스를 프로젝트 밖 폴더에 내려받았다.
- 현재 버전 기준의 `Hand Teaching Mode` 검증 체크리스트 문서를 추가했다.

## Restart Result

- Unity는 재시작 후 새 프로세스로 다시 올라왔다.
- 다만 현재 시점의 `unityctl check --type compile`은 아직 `IPC not ready` 상태다.
- 해석:
  - 에디터는 켜졌지만
  - domain reload 또는 컴파일 초기화가 끝나지 않았다.

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
