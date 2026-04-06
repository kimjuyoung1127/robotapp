# Pendant V3 Phase 1A Desktop Shell

## Date
- 2026-04-06 (KST)

## Summary
- Desktop 기준 TopStatusBar, NavRail, ContextPanel, BottomBar 구조를 문서 치수와 이름에 맞춰 고정했다.
- `PendantV3LayoutController`, `NavRailController`를 추가해 desktop 클래스와 nav 활성 상태를 최소한으로 유지했다.
- `RobotControlV3.unity` 저장본에 새 controller 구성을 반영했다.

## Added
- `Assets/Scripts/UI/RobotControlV3/PendantV3LayoutController.cs`
- `Assets/Scripts/UI/RobotControlV3/NavRailController.cs`

## Updated
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `Assets/Scenes/RobotControlV3.unity`
- `Assets/Scripts/UI/RobotControlV3/CLAUDE.md`

## Evidence
- `unityctl check --type compile` green
- `check-v3-static.ps1` green
- `scene hierarchy` 기준 `PendantV3Root`에 `PendantV3LayoutController`, `NavRailController` 부착 확인
- `uitk find --name TopStatusBar` 성공
- `uitk find --name NavMotion` 성공
- `uitk find --name BottomBar` 성공
- `Artifacts/V3/robotcontrolv3-1a-desktop.png`

## Self Review
- 역할 경계: layout/controller는 UI 폴더에만 두고 App/Visualization 계산은 넣지 않았다.
- 범위 통제: 1A는 desktop 셸 구획 고정까지만 처리하고 tablet/지속성/실데이터 바인딩은 안 건드렸다.
- 토큰 준수: 새 시각 규칙은 USS 클래스와 토큰 위주로 넣었고 인라인 스타일은 늘리지 않았다.
- 남은 숙제: `TopStatusBarController` 같은 패널별 controller 분리는 후속 2A 바인딩 전 정리할 가치가 있다.
