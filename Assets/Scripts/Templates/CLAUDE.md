# Templates/

로봇 설정 템플릿과 카탈로그를 관리하는 순수 도메인 폴더.

## 현재 파일
- `Template2DOF_RR.cs` — 2관절 RR 입문 템플릿 (베이스라인 참조 패턴)
- `TemplateSCARA_RV.cs` — 4자유도 SCARA donor 템플릿
- `TemplateFAIRINO_FR5.cs` — FAIRINO FR5 6축 템플릿
- `TemplateUR5e.cs` — UR5e 6축 템플릿
- `TemplateDoosanM1013.cs` — Doosan M1013 6축 템플릿
- `TemplateMeca500.cs` — Meca500 6축 템플릿
- `RobotCatalog.cs` — Robot Library 노출 순서, 메타데이터, 팩토리 연결
- `CustomTemplateBuilder.cs` — 커스텀/기본 템플릿 조립 헬퍼

## 규칙
1. 이 폴더는 순수 템플릿/카탈로그 계층이며 `using UnityEngine`를 두지 않는다.
2. 각 템플릿은 기본 DH 파라미터, 관절 한계, DOF 수를 완결된 형태로 제공한다.
3. `Template2DOF_RR.cs`를 새 템플릿의 기본 참조 패턴으로 사용한다.
4. 카탈로그 노출 순서와 Robot Library 메타데이터 변경은 `RobotCatalog.cs`에서만 관리한다.
5. 템플릿 추가/수정 시 FK 수치 테스트와 카탈로그 메타데이터 일관성을 같이 확인한다.

## 관련 스킬
- `robot-template-add` — 새 로봇 템플릿 추가 시 사용
