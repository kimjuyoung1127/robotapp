# UI/

튜터 애플리케이션 사용자 인터페이스 패널.

## Design System (핵심)
- `UIDesignTokens.cs` — 색상/타이포/간격/치수/애니메이션 토큰 (모든 시각 상수의 단일 진입점)
- `UITypography.cs` — TMP/Legacy 타이포그래피 프리셋 (DisplayLg~Tiny 7단계)
- `UIIconResolver.cs` — Resources/UI/Icons/ 아이콘 로딩 중앙화
- `UIComponentFactory.cs` — 복합 위젯 빌더 (패널, 버튼, 뱃지, 슬라이더 등)
- `UILayoutProfile.cs` — 태블릿/데스크탑 반응형 보정
- `UiRuntimeStyle.cs` — Legacy bridge (Obsolete, 기존 코드 호환용)

## 주요 파일
- `DHTableEditor.cs` — 편집 가능한 DH 파라미터 테이블
- `JointInputRail.cs` — 관절 각도/변위 슬라이더
- `StepTutorPanel.cs` — 단계별 튜토리얼 텍스트 패널
- `HomeContinueHubViewBuilder.cs` — Home 화면 빌더
- `RobotCardBuilder.cs` — Robot Library 카드 빌더
- `MathReadinessPanel.cs` — 수학 기초 워밍업 패널

## 규칙
1. UI 컴포넌트는 `UnityEngine.UI` 및 `UnityEngine` 참조 가능
2. 비즈니스 로직은 UI에 넣지 않음 — `App/AppController`에 위임
3. 입력 검증: NaN/Infinity 값 즉시 거부
4. 모든 표시 텍스트는 설정 가능하게 (하드코딩 금지)
5. **새 UI 코드는 `UIDesignTokens` 토큰 사용 필수** — `new Color()` 리터럴, `fontSize` 매직넘버, `GameObject.Find()` 금지
6. 신규 텍스트는 TMP 권장 (Legacy Text는 기존 코드 유지만 허용)

## 관련 스킬
- `tutor-step-add` — 새 튜토리얼 스텝 추가 시 사용
- `ui-design-system` — 색상/토큰/타이포/컴포넌트 작업 시 사용
