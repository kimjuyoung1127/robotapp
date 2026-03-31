# App/Fairino/

Fairino FR5 중심 RobotControl 런타임과 연결 서비스를 담는 폴더입니다.

## 주요 역할
- `RobotControlSceneCoordinator.cs` — RobotControl 씬 오케스트레이션
- `FairinoConnectionService.cs` — Mock/Live 연결, 상태 동기화, 폴링
- `RobotControlTemplateDefinition.cs` — 로봇별 RobotControl 구성 정의
- `PresetTransitionAnimator.cs` / `WaypointCycleRunner.cs` — 프리셋 전환과 teaching 재생

## 규칙
1. 실제 기구학 계산은 `RobotKinematicsFacade` 계열에 위임
2. 패널 표시 로직은 `UI/`에 두고, 여기서는 런타임 조율만 담당
3. Mock/Live 전환은 `IFairinoRobotClient` 추상화로 통일
