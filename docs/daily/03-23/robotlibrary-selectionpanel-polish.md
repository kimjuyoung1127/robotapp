# 2026-03-23 RobotLibrarySelectionPanel Polish

- `RobotLibrarySelectionPanel`를 `HeaderBlock / UtilityActionsRow / PrimaryActionsStack / ControlsViewport` 구조로 재정리했다.
- 목표는 `글씨를 키우고`, `정렬 기준을 단순화하고`, `CTA는 항상 보이게` 만드는 것이었다.
- `RobotLibraryManager.ApplySelectionPanelLayout()`도 폭만 반응형으로 바뀌고 중심은 하단 중앙에 유지되도록 조정했다.
- `unityctl`로 `RobotLibrarySelectionPanel`을 비활성/재활성한 뒤, 새 구조가 실제 씬 계층에 반영된 것을 확인했고 씬 저장까지 마쳤다.
- 현재 확인된 씬 계층:
  - `Content`
  - `HeaderBlock`
  - `Divider`
  - `ControlState`
  - `UtilityActionsRow`
  - `PrimaryActionsStack`
  - `ControlsViewport`
- `ControlsViewport`는 로봇 선택 전 기본 상태에서는 비활성으로 존재한다.
