# 2026-03-23 RobotLibrary Default SCARA Selection

- 문제: `RobotLibrarySelectionPanel`은 첫 진입 시 `selectedEntry == null` 경로로 들어가며 빈 placeholder 상태를 먼저 보여줬다.
- 수정: `Assets/Scripts/UI/RobotLibraryManager.cs`
  - `EnsureSelectionPanel()`에서 기본 fallback으로 `ShowRobot(null, ...)`를 호출하던 경로를 제거했다.
  - `EnsureDefaultSelectionEntry()`를 추가해 `selectedEntry`가 비어 있으면 우선 `SCARA_RV`를 채우도록 했다.
  - `RefreshSelectionPanel()` 시작부에서도 같은 기본 선택 보장을 걸었다.
- 기대 결과: 첫 진입 시 selection panel이 빈 상태가 아니라 `SCARA Robot` 정보를 기본값으로 보여준다.
- 검증: `unityctl check --type compile` 통과. 런타임 UI 조회는 Play 진입 직후 Unity IPC 재시작 때문에 이번 라운드에서 응답을 받지 못했다.
