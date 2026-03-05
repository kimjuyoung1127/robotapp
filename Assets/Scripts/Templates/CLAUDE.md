# Templates/

로봇 설정 템플릿 (2DOF, 3DOF, 6DOF, 커스텀).

## 파일 (예정)
- `Template2DOF_RR.cs` — 2관절 Revolute-Revolute (베이스라인 참조 패턴)
- `Template3DOF_SCARA.cs` — 3관절 SCARA
- `Template6DOF_IndustryA.cs` — 6관절 산업용

## 규칙
1. 각 템플릿은 정의 필수: 기본 DH 파라미터, 관절 한계, DOF 수
2. `Template2DOF_RR.cs`가 참조 패턴 — 새 템플릿은 이 구조를 따름
3. 모든 템플릿에 최소 1개 FK 수치 테스트 필수
4. 템플릿 선택 시 DH 테이블, 슬라이더, 3D 모델 자동 동기화

## 관련 스킬
- `robot-template-add` — 새 로봇 템플릿 추가 시 사용
