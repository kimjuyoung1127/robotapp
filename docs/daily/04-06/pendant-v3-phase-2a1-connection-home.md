# Pendant V3 Phase 2A-1 Connection Home

## Date
- 2026-04-06 (KST)

## Summary
- `2A-1` 범위로 연결 홈 시안 패널을 추가했다.
- desktop `WorkPanel`과 tablet `BottomSheet`에 같은 연결 홈 자산을 주입하고, `NavHome` 선택 시에만 보이게 연결했다.
- 실데이터 바인딩 전 단계라 `ConnectionHomeController` 내부 preview state 6종으로 연결/서보/미동기화/Fault/재연결 흐름만 시안으로 돌린다.
- TopStatusBar도 같은 preview state를 받아 연결/모드/속도/안전/Fault 칩과 상단 버튼 활성 상태를 같이 바꾸게 맞췄다.

## Updated Files
- `Assets/UI/PendantV3/connection-home.uxml`
- `Assets/UI/PendantV3/connection-home.uss`
- `Assets/Scripts/UI/RobotControlV3/ConnectionHomeController.cs`
- `Assets/Scripts/UI/RobotControlV3/ConnectionHomeController.Data.cs`
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.State.cs`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `Assets/Scripts/App/RobotControlV3DebugBridge.cs`
- `Assets/Scripts/UI/RobotControlV3/CLAUDE.md`
- `Assets/Scenes/RobotControlV3.unity`

## Verification
- `pwsh -NoLogo -NoProfile -File ./docs/ref/product/pendant-v3/check-v3-static.ps1`
  - pass, warning 1건 유지: `RegisterValueChangedCallback` + `SetValueWithoutNotify`
- `dotnet build Assembly-CSharp-Editor.csproj -nologo`
  - success, error 0
- `unityctl exec --code "KineTutor3D.EditorTools.PendantV3SceneBuilder.AuthorSceneSafe()" --json`
  - `RobotControlV3.unity` 저장 확인

## Notes
- `unityctl` IPC가 asset refresh 뒤 불안정해서 최종 compile/screenshot 증빙은 로컬 csproj 빌드와 scene 저장본 확인으로 보강했다.
- `phase-2a1-home.png` 게임뷰 캡처는 신뢰도가 낮아서 최종 증빙으로 채택하지 않았다.
- `2A-2`로 넘어갈 때는 우측 `StatusCard`, `CoordStrip`을 현재 홈 시안과 같은 preview state source에 연결하면 된다.
