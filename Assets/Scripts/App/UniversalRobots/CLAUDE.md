# App/UniversalRobots/

UR5e용 RobotControl 템플릿과 Mock 클라이언트를 담는 폴더입니다.

## 주요 역할
- `UR5eRobotControlTemplateDefinition.cs` — UR5e RobotControl 구성 정의
- `UR5ePosePresets.cs` — 기본 포즈 프리셋
- `MockUR5eClient.cs` — 오프라인 시뮬레이션용 클라이언트

## 규칙
1. UR 전용 값만 두고 공통 RobotControl 파이프라인은 재사용
2. UR 계열 확장 시 템플릿 정의와 프리셋을 같이 갱신
