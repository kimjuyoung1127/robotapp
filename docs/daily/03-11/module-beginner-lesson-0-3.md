# Module Log - Beginner Lesson 0~3

Date: 2026-03-11 (KST)

## What Changed
- `lesson-framework.md`에 `Pre-Kinematics Lesson 0~3`와 `Core Kinematics Step 1~8` 계층을 추가했다.
- `guided-lesson.md`에 `Beginner Mode`와 공식/행렬 최소화 규칙을 추가했다.
- `concept-to-ui-map.md`에 초보자용 개념 묶음(`rotation arc`, `end-effector path`, `reach vs not reach`, `inverse thinking`)을 추가했다.
- `tutor-step-plan.md`를 `L0~L3 -> S1~S8` 구조로 확장했다.
- `USER-FLOW.md`에 `완전 초보`와 `기본 개념 이해자` onboarding 분기를 추가했다.
- `current-feature-checklist.md`, `milestone-backlog.md`에 `Beginner Lesson 0~3`를 P0 항목으로 추가했다.
- `competitive-synthesis.md`에 공식-first 진입 배제 원칙을 추가했다.

## Why
- `sin/cos`, `삼각형`, `IK 유도`, `행렬/DH`를 모르는 사용자도 먼저 움직임 직관으로 진입할 수 있게 하기 위해서다.
- 기존 Step 1~8은 유지하되, 앞단에 수학 이전의 직관 계층을 붙이는 방식이 가장 안전한 확장 경로라고 판단했다.

## Follow-up
- Guided Lesson 구현 시 `Lesson 0~3`는 trail, target marker, why-it-moved 중심으로 먼저 구현한다.
- `S1~S8`에는 `지금 보는 DH는 Lesson 0~3에서 본 움직임을 정리하는 방법이다` 같은 브리지 문구를 실제 UI에 반영해야 한다.

## Integrity Follow-up
- root canonical 문서(`PRD`, `WIREFRAME`, `PRODUCT-ROADMAP`)와 `ai-context/project-context.md`도 beginner track 기준으로 다시 맞췄다.
- 이 보정으로 `leaf docs -> root canonical -> downstream context` 흐름의 정합성을 회복했다.
