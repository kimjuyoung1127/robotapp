# Pendant V3 Product Live Confirm Token

## Summary

- `Operator live confirm UX` 1차 구현을 완료했다.
- `Run/Move` 확인 팝업에서 product live approval token을 생성하고 표시한다.
- 확인 버튼을 누른 경우에만 token이 live safety gate 승인으로 승격된다.
- 취소하면 pending token은 폐기된다.

## Implementation

- `RobotControlV3RuntimeController`
  - product token 생성, 확인, 취소, 1회성 소비 상태를 추가했다.
  - 기존 debug approval과 product approval을 분리했다.
  - pending command 기준으로 `MoveJ`, `MoveL`, `ReadbackOnly`를 판단하는 진입점을 추가했다.
- `PopupCoordinatorV3`
  - `run` / `move` popup body에 live approval token line을 동적으로 표시한다.
  - confirm 시 token을 승인으로 승격한 뒤 `ExecutePrimaryAction()`을 호출한다.
  - cancel 시 pending token을 폐기한다.
- `RobotControlV3DebugBridge`
  - `RunProductLiveConfirmTokenMatrixForDebug()`를 추가했다.

## Verification

- `unityctl check --type compile --json`: pass
- `RunProductLiveConfirmTokenMatrixForDebug()`: `4/4 PASS`
- `RunLiveCommandSafetyGateMatrixForDebug()`: `12/12 PASS`
- `RunPopupConfirmCancelE2EForDebug()`: `10/10 PASS`
- `RunActualUiClickMatrixForDebug()`: `95/95 PASS`

## Next

- `Boundary/Collision real data`
- `Point MoveJ production IK policy`
- `Program Run/Step queue`
