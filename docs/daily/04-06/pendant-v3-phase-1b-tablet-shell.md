# Pendant V3 Phase 1B Tablet Shell

## Date
- 2026-04-06 (KST)

## Summary
- Tablet 전환 구조를 루트 셸에 추가했다.
- `BottomSheet`, `BottomTabBar`, `CoordOverlay`를 UXML/USS에 추가하고 `PendantV3LayoutController`에 `Auto/Desktop/Tablet` preview mode를 넣었다.
- editor bridge를 통해 tablet 모드 스크린샷을 찍고 다시 auto 모드로 복귀시켰다.

## Added
- `Assets/Editor/KineTutor3D/PendantV3TabletPreviewBridge.cs`

## Updated
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/Scripts/UI/RobotControlV3/PendantV3LayoutController.cs`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `Assets/Scenes/RobotControlV3.unity`

## Evidence
- `unityctl check --type compile` green
- `check-v3-static.ps1` green
- `exec invoke PendantV3TabletPreviewBridge.SetTabletMode` 결과 `mode=Tablet`
- `uitk find --name BottomSheet` 성공
- `uitk find --name CoordOverlay` 성공
- `Artifacts/V3/robotcontrolv3-1b-tablet.png`

## Self Review
- 역할 경계: tablet 전환은 layout/controller/USS에만 두고 runtime 상태 계산은 안 넣었다.
- 범위 통제: 1B는 bottom sheet와 tablet class 전환만 처리했고 지속성/실데이터 바인딩은 안 건드렸다.
- authored-first: tablet preview를 다시 auto로 돌린 뒤 `AuthorSceneSafe`로 씬 저장본을 정리했다.
- 남은 숙제: 1B 시안 리뷰 게이트는 구현 증빙까지 확보한 상태고, 다음은 1C 지속성 경계 정리다.
