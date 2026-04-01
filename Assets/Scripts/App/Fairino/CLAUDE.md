# App/Fairino/

Fairino FR5 중심 RobotControl 런타임과 연결 서비스를 담는 폴더입니다.

## 주요 역할
- `RobotControlSceneCoordinator.cs` — RobotControl 씬 오케스트레이션
- `FairinoConnectionService.cs` — Mock/Live 연결, 상태 동기화, 초기 bring-up 정책 적용
- `LiveFairinoClient.cs` — FAIRINO C# SDK reflection wrapper + 실기 preflight
- `RobotControlTemplateDefinition.cs` — 로봇별 RobotControl 구성 정의
- `FairinoCoordContext.cs` / `FairinoControllerFault.cs` — tool/user 문맥과 fault 상태 캐시 DTO
- `PresetTransitionAnimator.cs` / `WaypointCycleRunner.cs` — 프리셋 전환과 teaching 재생

## 새 구조 기준 하위 폴더
- `Connection/` — 연결 수명주기, preflight, sync 진입점
- `Motion/` — move 실행 정책과 speed/acc guard
- `Teaching/` — points + motion sequence 최소 teaching state
- `Templates/` — RobotControl 셸이 소비할 템플릿/프리셋 정의
- `Shell/` — 최상위 셸 composition root

## 규칙
1. 실제 기구학 계산은 `RobotKinematicsFacade` 계열에 위임
2. 패널 표시 로직은 `UI/`에 두고, 여기서는 런타임 조율만 담당
3. Mock/Live 전환은 `IFairinoRobotClient` 추상화로 통일
4. Live bring-up 초기화는 `FairinoConnectionService.Connect()` 한 곳에서만 수행한다.
5. `ReadState()` 폴링은 가볍게 유지하고, tool/user/fault/safety 상세 갱신은 `Connect`, `Enable`, `Sync`, `ResetErrors` 같은 명시적 시점에만 수행한다.
6. Live v1 범위는 `Connect -> Auto/Drag 정리 -> Enable -> Sync -> small MoveJ/MoveL`까지다. `ServoJ` / `ServoCart`는 연속 서보 제어 단계 전까지 일반 bring-up 경로에 포함하지 않는다.
7. `RobotControl` 런타임 구현은 브랜치에서 진행하고, 각 Phase 종료 전 자기리뷰 + `unityctl` 검증 + 커밋을 수행한다.
8. 여기서는 상태 원천과 오케스트레이션만 관리한다. UI 패널별 표시 규칙이나 스타일 판단을 다시 끌어오지 않는다.
9. 기존 연결/상태/모션 코어를 최대한 재사용하고, 새 기능은 어댑터/서비스 추가로 확장한다.
10. `Program`, `Status`, `Application`, `Initial/System`의 고급 기능을 메인 `RobotControl` 런타임 경로에 무분별하게 섞지 않는다.
