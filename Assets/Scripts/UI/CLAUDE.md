# UI/

튜터 애플리케이션 사용자 인터페이스 패널.

## 페이지 구조
- `GuidedLesson/` — 메인 학습 HUD (DH Table, Step, Matrix, Why It Moved, Beginner helpers)
- `Onboarding/` — 온보딩 모달/카드/초기 분기
- `RobotLibrary/` — 카탈로그, detail drawer, showroom
- `RobotControl/` — 실기 제어 패널, diagnostics, axis overlay
- `MathReadiness/` — 수학 기초 워밍업 패널
- `Sandbox/` — Sandbox scene UI builder
- `Shared/` — 씬 공용 네비게이션, 툴팁, 토스트, 오버레이 계약
- `DesignSystem/` — 토큰, 타이포, 컴포넌트 팩토리
- `Data/` — 설정/콘텐츠 데이터

## Design System (핵심)
- `DesignSystem/UIDesignTokens.cs` — 색상/타이포/간격/치수/애니메이션 토큰
- `DesignSystem/UITypography.cs` — TMP/Legacy 타이포그래피 프리셋
- `DesignSystem/UIIconResolver.cs` — Resources/UI/Icons/ 아이콘 로딩 중앙화
- `DesignSystem/UIComponentFactory.cs` — 복합 위젯 빌더
- `DesignSystem/UILayoutProfile.cs` — 태블릿/데스크탑 반응형 보정
- `DesignSystem/UiRuntimeStyle.cs` — Legacy bridge

## 주요 파일
- `GuidedLesson/DHTableEditor.cs` — 편집 가능한 DH 파라미터 테이블
- `GuidedLesson/JointInputRail.cs` — 관절 각도/변위 슬라이더
- `GuidedLesson/StepTutorPanel.cs` — 단계별 튜토리얼 텍스트 패널
- `RobotLibrary/RobotCardBuilder.cs` — Robot Library 카드 빌더
- `MathReadiness/MathReadinessPanel.cs` — 수학 기초 워밍업 패널

## 규칙
1. UI 컴포넌트는 `UnityEngine.UI` 및 `UnityEngine` 참조 가능
2. 비즈니스 로직은 UI에 넣지 않음 — `App/AppController`에 위임
3. 입력 검증: NaN/Infinity 값 즉시 거부
4. 모든 표시 텍스트는 설정 가능하게 (하드코딩 금지)
5. **새 UI 코드는 `UIDesignTokens` 토큰 사용 필수** — `new Color()` 리터럴, `fontSize` 매직넘버, `GameObject.Find()` 금지
6. 신규 텍스트는 TMP 권장 (Legacy Text는 기존 코드 유지만 허용)
7. 새 폴더는 페이지 기준을 우선하고, 재사용성이 2개 페이지 이상일 때만 `Shared/` 또는 `DesignSystem/`으로 승격

## 관련 스킬
- `tutor-step-add` — 새 튜토리얼 스텝 추가 시 사용
- `ui-design-system` — 색상/토큰/타이포/컴포넌트 작업 시 사용
