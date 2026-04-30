---
title: "FR5 Gripper Zero Contact Calibration"
doc_type: "daily-log"
status: "active"
domain: "fr5-live"
audience: "human-and-agent"
canonical: false
last_updated: "2026-04-29"
---

# FR5 Gripper Zero Contact Calibration

## Summary

- current branch gripper calibration baseline was changed from historical `user 0% -> raw 60` to `user 0% -> raw 0`
- visual fallback close travel was also bumped from `1.2` to `1.6` so the Unity fallback pose looks less open when readback is weak
- operator confirmed that the real FR5 gripper now physically contacts/closes at `0%`

## Code Changes

- `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/FairinoGripperModels.cs`
  - `GripperCalibrationProfile.Pgea10040Observed => new(0, 100, 70, 0.6f, 1f)`
- `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/FairinoRobotConfig.cs`
  - `closedRawPercent = 0`
- `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/FR5RobotControlTemplateDefinition.cs`
  - `closedRawPercent = 0`
- `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/Visualization/RobotControl/FR5EndEffectorAttachment.cs`
  - `ClosedContactTravelScale = 1.6f`

## Verification

- `dotnet build /Users/family/jason/FR5UNITY/robotapp/robotapp.slnx -nologo`
  - passed on the current branch after the calibration patch
- `GetMovementStateSummaryForDebug()`
  - latest live write summary for `0%` reported:
  - `요청 0% 전송 완료`
  - `readback 확인 안 됨 (motionFault=1; done=0; positionFault=0; raw=5)`
  - embedded peripheral readback showed `position=5`, `positionFault=0`
- `GetEasyMotionGripperInputStateForDebug()`
  - current input state reported `requested=0; actual=0; raw=0; draft=0`
- operator field confirmation
  - user confirmed the real FR5 gripper is now well contacted/closed at `0%`

## Interpretation

- `0% contact success` is now the operator-visible hardware baseline for the current branch
- this is not yet the same as reliable completion-grade sensor confirmation
- current readback remains weak because `motionFault=1` and `done=0` still persist after the write
- next gripper task stays the same:
  - keep confirm/debug flow simple
  - define completion-readback interpretation more clearly
  - only then move on to slider-follow work
