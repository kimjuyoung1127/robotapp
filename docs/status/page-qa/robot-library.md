# Robot Library QA Runbook

## Prep
- 메뉴: `KineTutor3D/QA: Prep Robot Library`
- Play 시작 후 예상 active scene: `RobotLibrary` (재방문) 또는 `Onboarding -> RobotLibrary` (첫 방문)

## Entry Route
1. `Play`
2. `Boot -> RobotLibrary` 자동 전환 확인 (재방문 시)

## Core Checks
- [ ] 카드 그리드가 보인다.
- [ ] 각 카드에 이름, DOF, 난이도, CTA가 보인다.
- [ ] 템플릿 지원 로봇은 `학습 시작` 또는 `샌드박스`로 들어갈 수 있다.
- [ ] `상세` 클릭 시 상세 패널이 열린다.
- [ ] 상세 패널에서 `학습 시작`, `샌드박스 열기`, `닫기`가 동작한다.
- [ ] `Back` 클릭 시 Onboarding으로 복귀한다.

## Layout / UI Checks
- [ ] 상세가 열려도 카드 그리드가 완전히 가려지지 않고, modal overlay 구조가 어색하지 않다.
- [ ] 세로 비율에서 카드가 화면 밖으로 잘리지 않는다.
- [ ] 스크롤이 자연스럽다.
- [ ] 상세 패널 텍스트가 clipping 없이 보인다.

## UX Checks
- [ ] 어떤 로봇이 지금 바로 사용 가능한지 쉽게 구분된다.
- [ ] `Coming Soon` 상태가 오해되지 않는다.
- [ ] 상세를 열고 닫는 흐름이 자연스럽다.

## Quick Inspect Targets
- scene: `RobotLibrary.unity`
- objects: `GridContent`, `BtnBack`, `DetailOverlay`, `BtnDetailLesson`, `BtnDetailSandbox`, `BtnDetailClose`
