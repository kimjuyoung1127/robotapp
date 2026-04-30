# FR5 Live QA Runner And Artifact Flow

## Summary

- `RobotControlV3DebugBridge`에 `gripper / joint nudge / TCP nudge / point move / snapshot` 공통 Live QA runner를 추가했다.
- 목적은 runtime safety model을 건드리지 않고도 field debug 절차를 `한 helper 실행 + 한 artifact 확인`으로 줄이는 것이다.
- build verification은 `dotnet build /Users/family/jason/FR5UNITY/robotapp/robotapp.slnx -nologo`로 통과했다.

## What Changed

- joint debug path에 `PreviewCurrentValuesForDebug()`, `ApplyCurrentValuesForDebug()`, `RestoreCurrentValuesForDebug()`를 추가했다.
- TCP debug path에 `PreviewCurrentPoseForDebug()`, `ApplyCurrentPoseForDebug()`를 추가했다.
- 새 공통 entrypoint를 추가했다.
  - `CaptureLiveQaSnapshotForDebug()`
  - `RunLiveGripperQaForDebug()`
  - `RunLiveJointNudgeQaForDebug()`
  - `RunLiveTcpNudgeQaForDebug()`
  - `RunLivePointMoveQaForDebug()`
- 각 QA run은 아래를 한 번에 묶는다.
  - runtime 준비
  - baseline evidence refresh
  - prepare/apply
  - popup confirm 시도
  - after evidence refresh
  - `Artifacts/live/qa/*.json` artifact write

## Artifact Shape

- artifact에는 다음이 같이 들어간다.
  - `before/after movement`
  - approval summary
  - popup state
  - `latest-state.json`
  - `latest-drift.json`
  - latest `*-events.ndjson` tail
  - latest `*-readback.ndjson` tail

## Interpretation

- 이 변경은 `debug/QA 절차 단순화`다.
- `MoveJ/MoveL` runtime safety gate를 제거한 것이 아니다.
- 따라서 gripper는 이 runner로 바로 field evidence를 쌓기 좋고,
- arm joint/TCP는 runner가 blocker를 더 빨리 보이게 해 주지만, 현재 live arm unblock 자체는 여전히 `fault 1/1` 확인이 먼저다.

## Next Use

- 다음 실기 세션에서는 수동으로 여러 debug helper를 조합하기보다 공통 QA runner를 우선 사용한다.
- 다음 목표는 두 갈래다.
  - `joint/TCP/point` artifact를 같은 형식으로 누적해 arm blocker 증거를 정리
  - gripper confirm/debug flow를 더 단순화한 뒤 slider live follow로 이동
