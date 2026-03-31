# App/Doosan/

Doosan M1013용 RobotControl 템플릿과 Mock 클라이언트를 담는 폴더입니다.

## 주요 역할
- `DoosanM1013RobotControlTemplateDefinition.cs` — Doosan RobotControl 구성 정의
- `DoosanM1013PosePresets.cs` — 기본 포즈 프리셋
- `MockDoosanClient.cs` — 오프라인 시뮬레이션용 클라이언트

## 규칙
1. Doosan 전용 설정만 두고 공통 런타임 로직은 상위 `App/` 또는 `App/Fairino/` 공용 타입을 재사용
2. 새 Doosan 모델 추가 시 템플릿 정의와 프리셋을 분리 유지
