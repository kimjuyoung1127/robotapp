# Assets/Scripts/UI/RobotControlV3/

Pendant V3 UI Toolkit controller 루트.

## 역할
- UIDocument bootstrap
- panel controller
- binder와 local UI state 연결

## 규칙
1. 새 `.cs` 파일 첫 줄은 폴더 역할 주석으로 시작한다.
2. UI Toolkit 초기화는 `OnEnable`, 해제는 `OnDisable`로 고정한다.
3. `RobotControlViewState` 전체를 패널이 통으로 먹지 말고 필요한 slice만 본다.
4. UI는 concrete 로봇 클라이언트 구현이 아니라 interface/facade 경계에만 의존한다.
5. 하나의 controller가 둘 이상 패널 orchestration을 먹기 시작하면 바로 쪼갠다.
6. `UI/RobotControlV3`에서는 preview/demo 용 샘플 숫자나 임시 로봇별 예시값을 runtime controller에 직접 하드코딩하지 않는다.
7. 초기값은 `RobotSelectionBridge`, `RobotControlTemplateDefinition`, `FairinoRobotConfig`, `PendantV3PreviewState`, shell local state 같은 SSOT에서만 가져온다.
8. 단, 현재 저장소의 실기 연동 목표가 `FAIRINO_FR5` 기준으로 잠겨 있는 동안에는 `FAIRINO_FR5`, live endpoint IP/port 같은 **실기 계약 literal**은 예외로 허용한다. 이런 값은 임시 데모 문자열이 아니라 현재 앱-기기 연결 계약으로 취급한다.
9. 새 demo 문자열이나 sample 숫자가 필요하면 preview/demo 전용 asset에만 두고 runtime controller에 직접 박지 않는다.

## 현재 파일
- `PendantV3Document.cs` — 최소 UIDocument bootstrap
- `PendantV3Binder.cs` — 3A 표시 패널 preview/shell snapshot binder
- `PendantV3InputContract.cs` — 기본 포커스 순서와 non-viewport 입력 차단
- `PendantV3LayoutController.cs` — desktop/tablet 루트 클래스 적용
- `ConnectionHomeController.cs` — 2A-1 연결 홈 시안과 next action 프리셋
- `StatusCardController.cs` — 2A-2 우측 StatusCard + CoordStrip 시안
- `EasyMotionController.cs` — 2B-1 쉬운 조작 preset + 그리퍼 시안
- `JointJogController.cs` — 2B-2 관절 조그 첫 슬라이스 (슬라이더/단일축/입력)
- `TcpJogController.cs` — 2B-3 TCP 조그 첫 슬라이스 (좌표계/XYZ-RPY/3D 오버레이)
- `PointMoveController.cs` — 2B-4 포인트 이동 최소 scaffold (좌표 입력/MoveJ·MoveL 후보)
- `SafetyDiagnosticsController.cs` — 2C-1 안전/진단 배너 + fault overlay 표시 scaffold
- `ViewportToolbarController.cs` — 2C-2 뷰포트 툴바/경계/충돌 표시 scaffold
- `PopupCoordinatorV3.cs` — 2D 확인/미저장 팝업 최소 orchestration scaffold
- `HelpPanelController.cs` — 2D NavHelp 전용 컨텍스트 도움말 panel scaffold
- `WhyItMovedController.cs` — 2D 최근 조작 메모 카드 전담
- `PendantV3ShellStateController.cs` — 1C 로컬 탭/레이아웃 상태 저장
- `NavRailController.cs` — nav 선택 상태 최소 제어

## 실기 연동 메모
- V3 패널은 `FairinoConnectionService`를 직접 조립하지 말고 App/Fairino facade를 통해 connect/enable/move 정책을 위임한다.
- `PointMoveController`는 실기/mock 준비 로직을 직접 소유하지 않는다.
- `PointMoveController`가 RobotStage preview, ghost, predicted path를 갱신해야 할 때는 `RobotControlV3RuntimeController` App facade만 호출한다.
- Live 실기 dispatch는 `RobotControlV3RuntimeController -> RobotControlMotionRuntime -> FairinoConnectionService` 경로를 유지한다.

## RobotControlV3RuntimeController panel-first backend
- runtime backend를 읽을 때는 `/Users/family/jason/FR5UNITY/robotapp/Assets/Scripts/App/Fairino/RobotControlV3RuntimeController/CLAUDE.md`부터 본다.
- 패널별 backend 대응:
  - `ConnectionHomeController` -> `ConnectionHome/`
  - `EasyMotionController` -> `EasyMotion/`
  - `JointJogController` -> `JointControl/`
  - `TcpJogController` -> `TcpControl/`
  - `PointMoveController` -> `PointMove/`
  - `SafetyDiagnosticsController`, `StatusCardController`, `WhyItMovedController` -> `StatusSafety/`
  - stage render/orientation surface -> `Stage/`
