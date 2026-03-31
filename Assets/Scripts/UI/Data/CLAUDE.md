# UI/Data/

튜토리얼, 용어집, 온보딩, 수학 준비도 UI가 사용하는 설정 데이터 모음입니다.

## 주요 역할
- `TutorStepConfig.cs` — 튜터 스텝 설정
- `GlossaryDatabase.cs` / `GlossaryEntryConfig.cs` — 용어집 데이터
- `OnboardingSequenceConfig.cs` — 온보딩 연출 데이터
- `MathReadinessQuestion.cs` — 수학 준비도 문항 데이터

## 규칙
1. 데이터 정의만 두고 로직은 `UI/` 또는 `App/`에서 처리
2. ScriptableObject와 직렬화 타입의 책임을 섞지 않음
