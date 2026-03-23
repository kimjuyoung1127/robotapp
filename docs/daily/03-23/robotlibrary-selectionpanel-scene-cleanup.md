# 2026-03-23 RobotLibrarySelectionPanel Scene Cleanup

- `RobotLibrarySelectionPanel` 코드 구조 변경 후, 씬 계층에는 레거시 자식이 남아 있어 새 구조가 즉시 반영되지 않았다.
- `unityctl`로 `RobotLibrary` 씬을 연 뒤 `RobotLibrarySelectionPanel`을 비활성/재활성하여 `ExecuteAlways` 경로를 다시 태웠다.
- 그 결과 씬 계층이 `Content -> HeaderBlock / Divider / ControlState / UtilityActionsRow / PrimaryActionsStack / ControlsViewport` 구조로 재생성되는 것을 확인했다.
- `ControlsViewport`는 현재 선택된 로봇이 없어서 비활성 상태로 존재한다.
- 마지막으로 `unityctl scene save --scene Assets/Scenes/RobotLibrary.unity` 로 씬 저장을 완료했다.
