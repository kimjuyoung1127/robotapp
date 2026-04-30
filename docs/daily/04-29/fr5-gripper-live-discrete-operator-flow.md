# FR5 Gripper Live Discrete Operator Flow

Date: 2026-04-29 (KST)

## Summary

- Revalidated the real-hardware `Easy Motion` gripper path with the new operator flow `연결 + 위치 읽기 -> percent input/preset -> 적용 -> 이동 실행 확인`.
- Found and fixed the approval-state invalidation bug where live readback updates were clearing the pending gripper confirm state before popup confirm could consume it.
- Added direct popup confirm/cancel debug helpers so live verification no longer depends on brittle generic UITK click simulation for the popup layer.
- Confirmed that discrete gripper live writes now reach the real FR5 on the latest branch.

## Field Results

- `70%` confirm:
  - visible real gripper movement confirmed
  - commanded/raw state observed as `Cmd 70%`, `raw 88%/88%`
- `100%` confirm:
  - visible real gripper movement confirmed
  - commanded/raw state observed as `Cmd 100%`, `raw 100%/100%`
- `50%` confirm:
  - visible real gripper movement confirmed
  - commanded/raw state observed as `Cmd 50%`, `raw 80%/80%`

## Current Limitation

- Completion-grade SDK readback is still weak.
- Current post-command signatures still include:
  - `motionFault=1`
  - `done=0`
  - `positionFault=0`
- Because of that, current smoke success is judged by:
  - operator-visible movement
  - commanded percent change
  - raw state change

## Process Complexity Check

- Product safety-wise, the required concept is `operator confirm 1회`.
- The current `token` model is an implementation detail that carries that one-shot confirm across popup/runtime boundaries.
- For `MoveJ/MoveL`, token + target matching still has clear value.
- For `MoveGripper`, the current target fingerprint is effectively `none`, so the visible token flow is probably heavier than needed.

## Recommended Simplification Order

1. Keep the safety popup for gripper live writes.
2. Remove operator-visible token wording for gripper-only confirms.
3. Internally replace the gripper path with a simpler one-shot confirm latch while preserving the stricter token/target model for motion commands.
4. Keep the new direct debug helpers, because they already cut live verification steps.
5. Only after that, move on to slider live follow design.

## Validation

- `dotnet build /Users/family/jason/FR5UNITY/robotapp/robotapp.slnx -nologo`: pass
- Unity restart + live reconnect + real hardware smoke:
  - `70 -> confirm`: pass
  - `100 -> confirm`: pass
  - `50 -> confirm`: pass
