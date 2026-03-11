# Lesson Framework

## Purpose
- lesson과 step 콘텐츠를 어떤 단위로 설계할지 표준을 정의한다.

## Parent Doc
- [PRD](../../PRD.md)

## When To Read
- step 설계, lesson 데이터 구조, AI 설명 카드 작성 시

## Locked Decisions
- 각 step은 `학습목표 / 핵심 행동 / 완료 조건 / 실수 포인트`를 가진다
- Guided Lesson과 Sandbox는 다른 콘텐츠 목적을 가진다
- Guided Lesson은 `Pre-Kinematics Lesson 0~3`와 `Core Kinematics Step 1~8`의 2계층으로 간다
- 초보자에게 필요한 것은 수학 제거가 아니라 `수학 이전의 직관 단계`다

## Open Questions
- challenge 전용 콘텐츠를 같은 프레임워크로 묶을지

## Downstream Sync
- `docs/ref/tutor-step-plan.md`
- `docs/ref/product/ux/guided-lesson.md`

## Last Updated
- 2026-03-11 (KST)

## Step Template
- `step_id`
- `lesson_level`
- `math_prerequisite`
- `objective`
- `core_action`
- `gate_condition`
- `common_misunderstanding`
- `supporting_concepts`
- `visual_goal`
- `transition_to_next_lesson`

## Lesson Tracks
### `Pre-Kinematics Lesson 0~3`
- 목적: `sin/cos`, 삼각형, IK 유도, 행렬/DH를 몰라도 움직임 직관으로 진입하게 한다
- lesson type: `intuition lesson`
- `lesson_level`: `pre_kinematics`
- `math_prerequisite`: `none`

### `Core Kinematics Step 1~8`
- 목적: Lesson 0~3에서 만든 감각을 `DH / frame / matrix / FK` 언어로 연결한다
- `lesson_level`: `core_kinematics`
- `math_prerequisite`: `light` 또는 `required`
