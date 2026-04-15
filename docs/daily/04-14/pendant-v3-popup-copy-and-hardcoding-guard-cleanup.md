# Pendant V3 Popup Copy And Hardcoding Guard Cleanup

## Date
- 2026-04-14 (KST)

## Summary
- `PopupCoordinatorV3`에서 popup 제목/요약/버튼 문구를 코드에서 빼고 popup UXML meta copy로 옮겼다.
- `ViewportToolbarController`의 상태 문구도 controller에서 빼고 `viewport-toolbar.uxml` copy bank로 옮겼다.
- `RobotControlV3HardcodingGuardTests`는 실기 계약 literal(`FAIRINO_FR5`, live IP/port)은 예외 허용하고, runtime controller의 preview/demo 숫자와 UI copy 하드코딩만 막도록 정책을 재정렬했다.

## Updated Files
- `Assets/Scripts/UI/RobotControlV3/PopupCoordinatorV3.cs`
- `Assets/Scripts/UI/RobotControlV3/ViewportToolbarController.cs`
- `Assets/Tests/EditMode/Validation/RobotControlV3HardcodingGuardTests.cs`
- `Assets/UI/PendantV3/popups/action-confirm.uxml`
- `Assets/UI/PendantV3/popups/action-reset-confirm.uxml`
- `Assets/UI/PendantV3/popups/action-run-confirm.uxml`
- `Assets/UI/PendantV3/popups/unsaved-confirm.uxml`
- `Assets/UI/PendantV3/viewport-toolbar.uxml`
- `docs/ref/product/pendant-v3/progress-checklist.md`

## Verification
- `unityctl check --type compile`
  - pass
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlV3HardcodingGuardTests`
  - `1 passed / 0 failed / 0 skipped`
- grep 기준 재확인
  - `PopupCoordinatorV3.cs`, `ViewportToolbarController.cs`에서 방금 금지한 popup/viewport UI 문구 literal 제거 확인

## Self Review
- 역할 경계
  - 실기 계약 literal과 demo/UI copy hardcoding을 분리해 문서 정책과 테스트 정책을 맞췄다.
  - popup/viewport 문구는 asset SSOT로 옮겼고 runtime controller는 orchestration만 남겼다.
- 남은 리스크
  - `move-confirm / warning / recovery` 본체 popup은 아직 미생성이라 2D 전체 완료는 아님.
  - `RobotControlV3HardcodingGuardTests` 필터 결과가 1건으로만 보이므로, Unity test runner의 표시 방식은 후속 점검이 필요하다.
