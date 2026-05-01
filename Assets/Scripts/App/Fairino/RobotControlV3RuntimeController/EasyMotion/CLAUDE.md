# RobotControlV3RuntimeController/EasyMotion/

Easy Motion과 IO 패널이 쓰는 gripper operator backend를 담는 폴더입니다.

## 주요 역할
- gripper percent apply/open/close
- gripper live preflight와 operator path
- gripper probe/debug summary
- gripper visual apply 보조

## 주 소비자
- `EasyMotionController`
- `IoPanelController`

## 하지 말아야 할 것
- broad arm motion 실행 로직
- connection/sync lifecycle
- point sequence orchestration

## naming rule
- gripper operator entry는 `GripperControl` 파일에 둔다.
- Easy Motion 패널에 대응되는 runtime은 이 폴더에서만 늘린다.

## 현재 파일 인덱스
- `RobotControlV3RuntimeController.GripperControl.cs`
