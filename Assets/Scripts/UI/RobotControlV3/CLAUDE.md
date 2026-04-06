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

## 현재 파일
- `PendantV3Document.cs` — 최소 UIDocument bootstrap
- `PendantV3InputContract.cs` — 기본 포커스 순서와 non-viewport 입력 차단
