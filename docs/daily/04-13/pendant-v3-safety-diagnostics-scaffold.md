# Pendant V3 Safety Diagnostics Scaffold

## Date
- 2026-04-13 (KST)

## Summary
- `2C-1` 최소 스코프(안전 배너 + fault 오버레이 + 복구/이벤트 카드)를 UI Toolkit으로 붙였다.
- `SafetyDiagnosticsController`를 추가해서 `ConnectionHomeController.PreviewChanged` 상태를 받아 배너/칩/오버레이 표시를 반영한다.
- `PendantV3SceneBuilder.AuthorSceneSafe()`가 새 템플릿과 컨트롤러 직렬화 참조를 자동으로 주입하도록 확장했다.
- actual play 기준으로 `Ready / Unsynced / Fault` 상태 전환에서 배너/오버레이 반응이 맞는지 닫았다.

## Updated Files
- `Assets/UI/PendantV3/safety-diagnostics-panel.uxml`
- `Assets/UI/PendantV3/safety-diagnostics-panel.uss`
- `Assets/UI/PendantV3/fault-overlay.uxml`
- `Assets/UI/PendantV3/fault-overlay.uss`
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/Scripts/UI/RobotControlV3/SafetyDiagnosticsController.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3Document.cs`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `docs/ref/product/pendant-v3/progress-checklist.md`

## Verification
- `unityctl status --project C:\Users\ezen601\Desktop\Jason\robotapp2 --wait --json`
  - Ready
- `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - pass
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --filter RobotControlMotionRuntimeTests --json`
  - `0 passed / 0 failed / 0 skipped` (`total=0`, short-name filter 불안정)
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlMotionRuntimeTests --json`
  - `2 passed / 0 failed / 0 skipped`
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --json`
  - `439 passed / 18 failed / 0 skipped` (`total=457`)
- `unityctl exec --project ... --code "KineTutor3D.EditorTools.PendantV3SceneBuilder.AuthorSceneSafe()" --json`
  - `saved=True`
- play smoke (`SceneNavigator.LoadByName("RobotControlV3")` + shell `NavHome`)
  - `BtnPresetUnsynced` actual click
    - `SafetyBannerText=안전 상태: 주의 · 동기화/재연결 확인`
    - `SafetyBanner` class=`rc-safety-banner--warning`
    - `FaultOverlayHost` class에 `rc-hidden` 유지
  - `BtnPresetFault` actual click
    - `SafetyBannerText=안전 상태: Fault 감지 · 조작 잠금`
    - `SafetyBanner` class=`rc-safety-banner--danger`
    - `FaultOverlayHost` class에서 `rc-hidden` 제거(visible)
    - `FaultOverlaySummary=코드 F203 · Safety 정지`
  - `BtnPresetReady` actual click
    - `SafetyBannerText=안전 상태: 정상`
    - `SafetyBanner` class=`rc-safety-banner--safe`
    - `FaultOverlayHost` class에 `rc-hidden` 재적용
- play console
  - gameplay 에러는 없고 `unityctl` IPC 재연결 로그만 반복 관측

## Self Review
- 역할 경계
  - `SafetyDiagnosticsController`는 UI 상태 표시/클래스 토글만 담당하고 실행 정책은 건드리지 않았다.
  - 실제 복구 명령/실기 통신은 App 계층으로 남겼다.
- 스코프 경계
  - `2C-1` 최소 scaffold만 반영했고 `2C-2`(작업공간 경계/충돌 시각화)는 의도적으로 미포함 유지했다.
- 남은 리스크
  - 복구 버튼은 현재 enable/label 반영까지만 되어 있고, action wiring은 다음 단위에서 policy 경계 맞춰 붙여야 한다.
  - fault close/reset 상호작용은 실제 App policy와 연결 전까지 임시 표시 수준이다.
