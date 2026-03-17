# DEPENDENCIES

## 패키지

- Unity Input System
- Unity UI (UGUI)
- Unity Robotics URDF Importer

## 공용 코드 의존성

이 번들은 아래 공용 계층을 함께 포함해 export됩니다.

- `SceneId`
- `SceneCatalog`
- `SceneNavigator`
- `RobotSelectionBridge`
- `SceneCameraDirector`
- `UIDesignTokens`
- `UiRuntimeStyle`
- `UIComponentFactory`
- `JointInputValidator`
- `IVisibilityControllable`
- `Templates/*`
- `Types/*`

## Mock-only 정책

- `LiveFairinoClient` 코드는 포함될 수 있으나 실사용 대상이 아닙니다.
- `libfairino.dll`, `CookComputing.XmlRpcV2.dll`는 포함하지 않습니다.
- 따라서 이 번들은 실기 연결용이 아니라 **Mock 기반 RobotControl 템플릿**입니다.

## 주의

- 의존성이 누락되면 `RobotControl` 페이지는 완전 동작하지 않습니다.
- 특히 URDF Importer 패키지가 없으면 FR5 control prefab 관련 경로가 깨질 수 있습니다.
