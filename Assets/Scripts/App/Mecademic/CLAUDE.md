# App/Mecademic/

Mecademic Meca500용 RobotControl 템플릿과 Mock 클라이언트를 담는 폴더입니다.

## 주요 역할
- `Meca500RobotControlTemplateDefinition.cs` — Meca500 RobotControl 구성 정의
- `Meca500PosePresets.cs` — 기본 포즈 프리셋
- `MockMecademicClient.cs` — 오프라인 시뮬레이션용 클라이언트

## 규칙
1. Mecademic 전용 설정만 이 폴더에 유지
2. 공통 통신/코디네이터 로직은 상위 공용 구조를 재사용
