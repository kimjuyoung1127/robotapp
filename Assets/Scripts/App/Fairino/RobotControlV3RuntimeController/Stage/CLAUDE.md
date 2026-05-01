# RobotControlV3RuntimeController/Stage/

Robot stage와 visual runtime helper를 담는 폴더입니다.

## 주요 역할
- runtime root / robot instance / joint driver
- end-effector attach / gripper visual
- selection/highlight
- frame/trail/ghost/boundary/collision stage helper

## 주 소비자
- `RobotStageRenderSurface`
- `RobotStageOrientationGizmoController`

## 하지 말아야 할 것
- connection lifecycle
- gripper/joint/TCP operator dispatch
- diagnostics/evidence summary

## naming rule
- stage visual/runtime 초기화는 `StageRuntime` 파일에 둔다.
- camera orbit/pan/zoom은 기존 `StageCamera` 축을 유지한다.

## 현재 파일 인덱스
- `RobotControlV3RuntimeController.StageRuntime.cs`
