# RobotControlV2 Shell Authored Update

## Date
- 2026-04-01 (KST)

## Summary
- `RobotControlV2`는 old `RobotControlSceneCoordinator` 누적 경로가 아니라 V2 전용 composition root를 사용하도록 정리했다.
- scene 기준으로 `SceneBootstrap`, `RuntimeRoot`, `RobotRuntimeRoot`, `RobotControlShell/SafeArea/...` authored 구조를 고정했다.
- old FR5 panel authored 오브젝트와 old shell 잔재(`RobotControlOverlay`, `TabBar`, `Tab_0`, `Tab_1`)를 제거했다.
- `TopStatusBar`, `EasyMotionPanel`의 duplicated child를 제거했다.
- `ExecuteAlways` 기반 edit-mode hierarchy mutation을 제거했다.
- `DiagnosticsDrawer`를 debug-only 우하단 anchor로 이동했다.
- `WorkTabBar`를 3열 grid 구조로 정규화하고, red-X 원인을 제거했다.
- `TopStatusBar`, `WorkTabBar`, `EasyMotionPanel`에 compact 규칙을 추가해 `16:9` / `4:3` 대응 기반을 마련했다.
- `TcpJogPanel`, `JointJogPanel`, `PointMovePanel`, `TeachingPanel`을 authored child 구조까지 확장했다.
- popup 3종과 `BottomSheets` 하위 sheet를 authored shell 구조로 추가했다.
- `play 시작 1회 authored-lock` 훅을 추가해 runtime 정규화가 authored 위치를 매번 덮어쓰지 않도록 했다.
- `Onboarding` 강제 시작은 유지하고, V2 검증 기준도 `Onboarding -> RobotLibrary -> RobotControlV2` 진입 흐름으로 재정렬했다.

## Verification
- `unityctl check --type compile` 통과
- `scene snapshot`으로 `RobotControlV2` authored shell 계층 확인
- restart 후 `RobotControlV2` 다시 열고 compile 확인

## Next
- `Onboarding -> RobotLibrary -> RobotControlV2` 실제 진입 흐름에서 authored-lock 유지 확인
- GameView `16:9` / `4:3` 실제 시각 점검
- Mock binding 착수
