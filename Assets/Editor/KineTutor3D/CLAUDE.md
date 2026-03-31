# Editor/KineTutor3D/

에디터 전용 QA, 씬 준비, 패키지 내보내기, 샘플 데이터 생성 도구를 담는 폴더입니다.

## 주요 역할
- Play 시작 씬 보정, QA 메뉴, 증거 캡처, 템플릿 패키지 export
- MathReadiness authored UI 구축과 UX 샘플 데이터 seeding

## 규칙
1. 런타임 코드 의존은 최소화하고 Editor API 경계 안에 유지
2. 반복 QA 절차는 메뉴/도구 형태로 재사용 가능하게 유지
