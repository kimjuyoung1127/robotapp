# Math Readiness QA Runbook

## Prep
- 메뉴: `KineTutor3D/QA: Prep Math Readiness`
- Play 시작 후 예상 active scene: `RobotLibrary` 또는 `Onboarding`

## Entry Route
1. `Play`
2. `Onboarding`에서 `초보자 시작` 클릭, 또는 `RobotLibrary`에서 `수학 기초` 진입
3. `MathReadiness` 진입 후 Math Readiness 패널 표시 확인

## Core Checks
- [ ] `MathReadinessPanel`이 보인다.
- [ ] warmup choice 버튼이 보인다.
- [ ] 오답 선택 시 soft correction 메시지가 나온다.
- [ ] 정답 + 슬라이더 이동 후 `Next`가 활성화된다.
- [ ] 사용하지 않는 joint row는 숨겨진다.
- [ ] 마지막 단계 후 `RobotLibrary`로 이동한다.

## Layout / UI Checks
- [ ] `MRP_Content`가 LeftPanel 범위를 넘지 않는다.
- [ ] warmup/question card가 서로 겹치지 않는다.
- [ ] coach hint 토글이 다른 CTA를 가리지 않는다.
- [ ] BottomBar의 rail과 step nav가 동시에 깨지지 않는다.

## UX Checks
- [ ] “수학 시험”처럼 느껴지지 않고 보강 단계처럼 느껴진다.
- [ ] 피드백 문구가 부드럽고 이해 가능하다.
- [ ] 초보자가 다음 행동을 쉽게 찾을 수 있다.

## Quick Inspect Targets
- scene: `MathReadiness.unity`
- objects: `MRP_Content`, `BtnWarmupChoice_0`, `BtnReadinessChoice_0`, `BtnReadinessChoice_1`, `MRP_CoachHintBody`, `BtnNext`
