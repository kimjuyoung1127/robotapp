# RobotControlV3RuntimeController/JointControl/

JointJog와 saved MoveJ가 쓰는 joint preview/apply backend를 담는 폴더입니다.

## 주요 역할
- joint preview / apply / restore / undo / redo
- joint operator approval entry
- saved-point MoveJ helper
- tiny-joint helper 진입점

## 주 소비자
- `JointJogController`
- `PointMoveController`

## 하지 말아야 할 것
- TCP move 로직
- gripper operator path
- connection lifecycle

## naming rule
- tiny 범위라도 이름은 `TinyMoveJ`가 아니라 `JointControl`로 둔다.
- joint preview/apply 중심 public API는 이 폴더에 모은다.

## 현재 파일 인덱스
- `RobotControlV3RuntimeController.JointControl.cs`
