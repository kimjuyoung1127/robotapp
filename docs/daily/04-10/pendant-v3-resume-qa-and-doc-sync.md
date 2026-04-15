# Pendant V3 Resume QA And Doc Sync

## Date
- 2026-04-10 (KST)

## Summary
- `RobotLibrary -> RobotControlV3` 재진입 시 `ResumeLastSession`이 실제 플레이 기준으로 유지되는지 닫았다.
- `TabJointJog`, `TabTcpJog`, `BtnTcpCoordTool`가 실제 `uitk click` 경로에서도 동작하는지 다시 확인했다.
- 진행 체크리스트를 현재 코드/플레이 결과에 맞춰 갱신했다.

## Updated Files
- `Assets/Tests/PlayMode/Flows/RobotLibrarySandboxRoutingTests.cs`
- `docs/ref/product/pendant-v3/progress-checklist.md`

## What Changed
- `RobotLibrarySandboxRouteProbe`
  - 기존 sandbox probe에 `InvokeOpenRobotControl(string robotId)`를 추가했다.
  - `RobotLibraryManager.OnOpenRobotControl(...)` private 경로를 reflection으로 호출해 FR5 재진입 검증을 자동화했다.
- progress checklist
  - `2B-2`, `2B-3` note를 실제 UITK 클릭 QA 완료 기준으로 갱신했다.
  - `RobotLibrary restore UX 플레이 검증`을 완료로 표시했다.
  - 최신 play 검증 결과를 문서에 반영했다.

## Verification
- `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - pass
- `play start -> Onboarding -> BtnOpenRobotControlV3`
  - `SceneId = 7`
- `uitk click TabTcpJog`
  - `tcp desktopVisible=True`
- `uitk click BtnTcpCoordTool`
  - `coord=Tool`
- `exec SceneNavigator.LoadByName("RobotLibrary")`
  - `SceneId = 2`
- `exec invoke KineTutor3D.Tests.PlayMode.RobotLibrarySandboxRouteProbe.InvokeOpenRobotControl ["FAIRINO_FR5"]`
  - `invoked:FAIRINO_FR5`
- 복귀 후 `RobotControlV3DebugBridge.GetShellControllerSummary()`
  - `work=TabTcpJog`
  - `tablet=BottomTabTcpJog`
  - `coord=Tool`

## Remaining Follow-up
- `StatusCard / CoordStrip / ActionHint` 시각 완성도 최종 확인
- placeholder 잔여 텍스트 제거
- `PointMove` 재시도 전 stale placeholder 제거 판단
