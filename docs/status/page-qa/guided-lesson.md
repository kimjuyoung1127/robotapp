# Guided Lesson QA Runbook

## Prep
- 메뉴: `KineTutor3D/QA: Prep Guided Lesson (Core Step 1)`
- Play 시작 후 예상 active scene: `RobotLibrary`

## Entry Route
1. `Play`
2. `RobotLibrary`에서 로봇 선택 → `Sandbox` 클릭
3. `Sandbox` 진입 확인 (Guided Lesson은 Sandbox 내 학습 모드로 제공)

## Core Checks
- [ ] `TopBar`, `LeftPanel`, `RightPanel`, `BottomBar` 4영역이 보인다.
- [ ] 스텝 타이틀과 목표 텍스트가 보인다.
- [ ] `BtnPrev`, `BtnNext`, `BtnSkip` 동작이 정상이다.
- [ ] 슬라이더 이동 시 로봇 자세가 바뀐다.
- [ ] `Why It Moved` 또는 step별 피드백 패널이 스텝 계약대로 나타난다.
- [ ] 상단 바의 `BtnLessonOpenSandbox`가 보이고 Sandbox로 이동한다.
- [ ] 상단 네비 또는 상위 흐름을 통해 RobotLibrary로 복귀 가능하다.

## Layout / UI Checks
- [ ] 상단 바의 템플릿 드롭다운, glossary 버튼, Sandbox 버튼이 겹치지 않는다.
- [ ] 오른쪽 패널 텍스트가 clipping 없이 읽힌다.
- [ ] 하단 joint rail과 Prev/Next/Skip이 서로 가리지 않는다.
- [ ] 초보자/코어 전환 스텝에서 패널이 동시에 중복 노출되지 않는다.

## UX Checks
- [ ] 현재 step에서 해야 할 행동이 명확하다.
- [ ] gate 잠금 상태와 해제 상태가 이해 가능하다.
- [ ] Sandbox 진입 CTA가 이제 전역 네비 외에도 lesson 안에서 명확히 보인다.

## Quick Inspect Targets
- scene: `Sandbox.unity` (Guided Lesson은 Sandbox 학습 모드)
- objects: `TopBar`, `StepTitleText`, `BtnPrev`, `BtnNext`, `BtnSkip`, `joint_slider_1`, `joint_slider_2`, `BtnLessonOpenSandbox`
