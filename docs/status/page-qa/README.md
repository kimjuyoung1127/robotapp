# Page QA Runbooks

Last Updated: 2026-03-12 (KST)

## Purpose
- 실제 진입 가능한 페이지를 한 페이지씩 수동 QA할 수 있도록 준비 상태, 진입 경로, 체크 항목, 실패 시 확인 포인트를 고정한다.

## Common Setup
1. Unity 메뉴에서 `KineTutor3D > Always Start From Onboarding`가 켜져 있는지 확인한다.
2. 검사할 페이지에 맞는 QA 준비 메뉴를 먼저 실행한다.
3. `Play`를 누른 뒤 아래 runbook의 `Entry Route`를 그대로 따라간다.
4. 각 페이지는 최소 `진입`, `핵심 행동 1회`, `복귀/다음 이동`, `겹침`, `UI 일관성`까지 본다.

## QA Prep Menus
- `KineTutor3D/QA: Reset to First-Time User`
- `KineTutor3D/QA: Reset to Returning User (skip onboarding)`
- `KineTutor3D/QA: Prep Guided Lesson (Core Step 1)`
- `KineTutor3D/QA: Prep Math Readiness`
- `KineTutor3D/QA: Prep Robot Library`
- `KineTutor3D/QA: Prep Sandbox`

## Page Runbooks
- [Onboarding](./onboarding.md)
- [Guided Lesson](./guided-lesson.md)
- [Math Readiness](./math-readiness.md)
- [Robot Library](./robot-library.md)
- [Robot Control](./robot-control.md)
- [Sandbox](./sandbox.md)

## Capture Rule
- `Blocker`: 진입 불가, 핵심 CTA 불가, 복귀 불가, 심한 겹침
- `Major`: 우회 가능하지만 핵심 보조 흐름 손상, 읽기 어려운 clipping/overlap
- `Minor`: 카피, 간격, 아이콘, 정렬, 비핵심 시각 불일치

## Result Logging
- 현재 페이지별 결과는 이 runbook 묶음과 `docs/status/PROJECT-STATUS.md`를 함께 본다.
- 2026-03-23 초기 baseline 비교가 필요하면 `docs/archive/legacy/page-qa/PAGE-QA-MATRIX-2026-03-23.md`를 본다.
- `Home / Continue Hub` runbook이 필요하면 `docs/archive/legacy/page-qa/home-continue-hub-runbook.md`를 본다.
- 재감사 후 점수/게이트를 바꾸려면 오늘자 daily log에 함께 남긴다.
