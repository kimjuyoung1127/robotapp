# Onboarding QA Runbook

## Prep
- 메뉴: `KineTutor3D/QA: Reset to First-Time User`
- Play 시작 후 예상 active scene: `Onboarding`

## Entry Route
1. `Play`
2. `Boot -> Onboarding` 자동 전환 확인

## Core Checks
- [ ] `BtnStartLearning`, `BtnBeginner`, `BtnOnboardingSkip` 3개 버튼이 보인다.
- [ ] 3개 버튼이 모두 `ModalSurface` 카드 안에 들어가 있다.
- [ ] `학습 시작` 클릭 시 `RobotLibrary`로 이동한다.
- [ ] `초보자 시작` 클릭 시 `MathReadiness`로 이동하고 `MathReadinessPanel`이 바로 열린다.
- [ ] `건너뛰기` 클릭 시 `Sandbox`로 이동한다.
- [ ] 상단 전역 네비게이션은 보이지 않는다.

## Layout / UI Checks
- [ ] 모달이 화면 밖으로 나가지 않는다.
- [ ] 카드 2개와 하단 `둘러보기 →` 버튼이 서로 겹치지 않는다.
- [ ] 카드 2개가 같은 크기로 보인다.
- [ ] 제목/본문 텍스트가 잘리지 않는다.
- [ ] 카드 안의 아이콘/제목/설명/CTA가 `ModalSurface` 내부에서 또렷하게 보인다.

## UX Checks
- [ ] `처음이에요`, `알고 있어요`, `둘러보기 →`의 의미가 명확하다.
- [ ] 초보자/기본/건너뛰기 선택지가 헷갈리지 않는다.
- [ ] 다음 페이지(RobotLibrary/Sandbox/MathReadiness)로 이동한 뒤 dead-end가 없다.

## Quick Inspect Targets
- scene: `Onboarding.unity`
- objects: `Canvas`, `WelcomeModal`, `ModalSurface`, `BtnStartLearning`, `BtnBeginner`, `BtnOnboardingSkip`
