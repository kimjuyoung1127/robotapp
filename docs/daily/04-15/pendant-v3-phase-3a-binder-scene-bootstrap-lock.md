# Pendant V3 3A Binder/Scene Bootstrap Lock

## 오늘 잠근 내용
- preview source는 `ConnectionHomeController` 유지
- shell local state source는 `PendantV3ShellStateController.GetStateSnapshot()` 유지
- 표시 패널 5개만 `PendantV3Binder` 아래로 이동
- `PendantV3SceneCoordinator`는 bootstrap 순서만 담당

## 오늘 코드 작업
- `PendantV3Binder.cs` 추가
- `PendantV3SceneCoordinator.cs` 추가
- 표시 패널 5개 direct `PreviewChanged` 구독 제거 시작
- `PendantV3Document` 의존 초기화 루프 제거
- `PendantV3SceneBuilder`, `RobotControlV3DebugBridge`에 3A wiring 추가

## 남은 확인
- compile clean
- binder/coordinator debug summary 확인
- preview smoke에서 상태 전파 확인
