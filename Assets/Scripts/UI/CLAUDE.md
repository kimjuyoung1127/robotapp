# UI/

튜터 애플리케이션 사용자 인터페이스 패널.

## 파일 (예정)
- `DHTableEditor.cs` — 편집 가능한 DH 파라미터 테이블
- `JointSliderPanel.cs` — 관절 각도/변위 슬라이더
- `StepTutorPanel.cs` — 단계별 튜토리얼 텍스트 패널

## 규칙
1. UI 컴포넌트는 `UnityEngine.UI` 및 `UnityEngine` 참조 가능
2. 비즈니스 로직은 UI에 넣지 않음 — `App/AppController`에 위임
3. 입력 검증: NaN/Infinity 값 즉시 거부
4. 모든 표시 텍스트는 설정 가능하게 (하드코딩 금지)

## 관련 스킬
- `tutor-step-add` — 새 튜토리얼 스텝 추가 시 사용
