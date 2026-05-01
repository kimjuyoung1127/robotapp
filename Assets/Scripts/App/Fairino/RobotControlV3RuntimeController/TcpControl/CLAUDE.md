# RobotControlV3RuntimeController/TcpControl/

TcpJog와 point move 직선 이동이 쓰는 TCP preview/apply backend를 담는 폴더입니다.

## 주요 역할
- TCP preview / apply
- 직선 이동 공용 entry
- TCP target 생성과 반영

## 주 소비자
- `TcpJogController`
- `PointMoveController`

## 하지 말아야 할 것
- joint-only preview/apply
- gripper operator path
- connection/evidence orchestration

## naming rule
- TCP 관련 preview/apply runtime은 `TcpControl` 파일에 둔다.

## 현재 파일 인덱스
- `RobotControlV3RuntimeController.TcpControl.cs`
