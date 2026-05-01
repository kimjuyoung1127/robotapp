# RobotControlV3RuntimeController/Shared/

여러 패널이 같이 쓰는 approval/session/common helper를 담는 폴더입니다.

## 주요 역할
- session mode 전환
- live approval token lifecycle
- pending operator command state
- live loop approval context

## 주 소비자
- `EasyMotionController`
- `JointJogController`
- `PointMoveController`
- `PopupCoordinatorV3`

## 하지 말아야 할 것
- panel-specific move orchestration
- connection lifecycle
- broad diagnostics/evidence 조립

## naming rule
- 공용 approval/session helper만 이 폴더에 둔다.
- 특정 패널 전용 로직은 각 패널 폴더로 되돌린다.

## 현재 파일 인덱스
- `RobotControlV3RuntimeController.LiveApproval.cs`
