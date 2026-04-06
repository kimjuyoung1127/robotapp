# Pendant V3 Phase 1C Local State

## Date
- 2026-04-06 (KST)

## Summary
- `1C` 범위로 Pendant V3 셸의 로컬 상태 지속성을 추가했다.
- `view-data-key` 적용 범위를 `NavRail`, `WorkTabBar`, `MainSplit`, `BottomSheet`, `SpeedSlider`, 로컬 설정 버튼들까지 고정했다.
- 실제 저장은 `LocalSettingsStore` + `PendantV3LocalState`로 분리했고, `PendantV3ShellStateController`가 UI와 PlayerPrefs JSON 사이를 연결한다.
- 범위 통제:
  - 포함: nav/work/tablet tab 선택, split 비율, bottom sheet 확장 상태, 마지막 속도, 좌표계, 증분
  - 제외: 실데이터 바인딩, Undo/Redo 스택, AutoReconnect, 실제 패널 기능

## Updated Files
- `Assets/Scripts/App/Session/PendantV3LocalState.cs`
- `Assets/Scripts/App/Session/LocalSettingsStore.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.State.cs`
- `Assets/Scripts/UI/RobotControlV3/NavRailController.cs`
- `Assets/Scripts/App/RobotControlV3DebugBridge.cs`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/Tests/EditMode/Core/LocalSettingsStoreTests.cs`
- `Assets/Scripts/UI/RobotControlV3/CLAUDE.md`

## Verification
- `pwsh -NoLogo -NoProfile -File ./docs/ref/product/pendant-v3/check-v3-static.ps1`
  - pass, warning 1건: `RegisterValueChangedCallback` + `SetValueWithoutNotify` 조합 점검 알림
- `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - pass
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --filter 'KineTutor3D.Tests.EditMode.LocalSettingsStoreTests' --timeout 600 --json`
  - 2 passed
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.EditorTools.PendantV3SceneBuilder.AuthorSceneSafe()" --json`
  - `RobotControlV3.unity` 저장 확인
- `unityctl scene snapshot --project C:\Users\ezen601\Desktop\Jason\robotapp2 --scene-path 'Assets/Scenes/RobotControlV3.unity' --json`
  - `PendantV3ShellStateController` 부착 확인
- `unityctl screenshot capture --project C:\Users\ezen601\Desktop\Jason\robotapp2 --view game --include-overlay-ui --output 'Artifacts/V3/phase-1c-shell.png' --json`
  - game view 캡처 저장

## Notes
- `split ratio`는 금지된 `style.width` 대신 `flex-grow` 비율 조정으로 적용했다.
- debounce 저장 대기 중 `OnDisable`이 오면 마지막 변경이 유실될 수 있어서 pending save flush를 추가했다.
- 콘솔에 남은 `[unityctl] IPC connection error`는 중간 재시도 때 생긴 툴링 타임아웃 찌꺼기 1건이며, compile/test 자체는 green이다.
