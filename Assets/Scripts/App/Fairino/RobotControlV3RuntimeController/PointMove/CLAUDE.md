# RobotControlV3RuntimeController/PointMove/

PointMove 패널이 쓰는 saved point / sequence / teaching backend를 담는 폴더입니다.

## 주요 역할
- saved point apply
- named sequence run / loop / selected-start
- teaching path record / replay
- teaching function / block sequence

## 주 소비자
- `PointMoveController`

## 하지 말아야 할 것
- connection lifecycle
- gripper-only operator path
- broad diagnostics summary 조립

## naming rule
- point/sequence 중심 orchestration은 `PointMove*` partial에 둔다.
- teaching record/function/block/runtime/event는 `Teaching*` partial에 둔다.

## 현재 파일 인덱스
- `RobotControlV3RuntimeController.PointMove.cs`
- `RobotControlV3RuntimeController.PointMoveSequence.cs`
- `RobotControlV3RuntimeController.PointMoveHomeLoop.cs`
- `RobotControlV3RuntimeController.PointMoveMixedLive.cs`
- `RobotControlV3RuntimeController.PointMovePreview.cs`
- `RobotControlV3RuntimeController.Teaching.cs`
- `RobotControlV3RuntimeController.TeachingRecording.cs`
- `RobotControlV3RuntimeController.TeachingSequenceRuntime.cs`
- `RobotControlV3RuntimeController.TeachingFunctionBlock.cs`
- `RobotControlV3RuntimeController.TeachingRunnerEvents.cs`
