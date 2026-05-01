# RobotControlV3RuntimeController/ConnectionHome/

ConnectionHome 패널이 쓰는 live 연결, sync, mode truth backend를 담는 폴더입니다.

## 주요 역할
- `Connect`, `Disconnect`, `Enable`, `Sync` 진입점
- `Auto / Manual` 전환 요청과 verification
- connection 이벤트 바인딩과 live truth 반영
- `ConnectionHomeController`가 기대하는 런타임 상태 전이 유지

## 주 소비자
- `ConnectionHomeController`
- `PendantV3ShellStateController`

## 하지 말아야 할 것
- gripper/joint/TCP 실제 move 실행 로직
- point/teaching sequence orchestration
- broad diagnostics summary 조립

## naming rule
- 연결 수명주기와 mode truth는 `Connection*` 파일에 둔다.
- connection event 반응은 `ConnectionEvents*` 파일에 둔다.

## 현재 파일 인덱스
- `RobotControlV3RuntimeController.Connection.cs`
- `RobotControlV3RuntimeController.ConnectionEvents.cs`
