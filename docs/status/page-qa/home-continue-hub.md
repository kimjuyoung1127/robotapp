# Home / Continue Hub QA Runbook

## Prep
- 메뉴: `KineTutor3D/QA: Prep Home / Continue Hub`
- Play 시작 후 예상 active scene: `Home`

## Entry Route
1. `Play`
2. `Boot -> Home` 자동 전환 확인

## Core Checks
- [ ] `BtnContinueLatestContext`가 보인다.
- [ ] `BtnStartGuidedLesson`, `BtnStartMathReadiness`, `BtnOpenRobotLibrary`, `BtnOpenSandbox`가 보인다.
- [ ] `학습 시작` 클릭 시 Guided Lesson으로 들어간다.
- [ ] `수학 기초 워밍업` 클릭 시 Math Readiness가 열린다.
- [ ] `로봇 선택` 클릭 시 Robot Library로 이동한다.
- [ ] `샌드박스` 클릭 시 Sandbox로 이동한다.
- [ ] `Progress`, `Settings`가 disabled placeholder로 보이면 dead CTA처럼 오해되지 않는지 확인한다.

## Layout / UI Checks
- [ ] 버튼 간격이 일정하다.
- [ ] leading icon이 있는 버튼과 없는 버튼이 어색하게 섞이지 않는다.
- [ ] context card가 버튼을 가리지 않는다.
- [ ] 세로 비율에서도 하단 버튼이 화면 밖으로 밀리지 않는다.

## UX Checks
- [ ] 사용자가 “무엇을 먼저 해야 하는지” 바로 이해된다.
- [ ] `이어하기` 비활성 시 안내 문구가 이해 가능하다.
- [ ] Home에서 다른 페이지로 나갔다가 다시 돌아오는 흐름이 자연스럽다.

## Quick Inspect Targets
- scene: `Home.unity`
- objects: `BtnContinueLatestContext`, `BtnStartGuidedLesson`, `BtnStartMathReadiness`, `BtnOpenRobotLibrary`, `BtnOpenSandbox`, `BtnOpenProgress`, `BtnOpenSettings`
