# LLM Teaching Strategy

## Purpose
- LLM을 KineTutor3D에 설명층으로만 도입하는 전략과 금지 규칙을 정의한다.

## Parent Doc
- [PRD](../../PRD.md)

## When To Read
- LLM 힌트, why-it-moved 설명, 강사용 해설, tutoring prompt를 설계할 때

## Locked Decisions
- LLM은 `정답 계산기`가 아니라 `설명층`이다
- FK/pose, joint limit, 저장 데이터, pick 상태의 진실값은 deterministic runtime이 결정한다
- LLM은 로컬 문서, skill, reference pack을 우선 컨텍스트로 사용한다

## Open Questions
- future cloud inference와 on-device inference를 분리할지

## Downstream Sync
- `docs/ref/PRD.md`
- `docs/ref/product/content/open-robotics-reference-pack.md`
- `docs/ref/product/ux/guided-lesson.md`

## Last Updated
- 2026-03-11 (KST)

## System Layers
### `Deterministic Runtime Layer`
- `AppController`
- `KinematicsRuntimeService`
- future history store
- future constraint checker

### `Teaching Context Layer`
- `concept-to-ui-map`
- `lesson-framework`
- `guided-lesson`
- `open-robotics-reference-pack`
- 비공개 강의자료의 파생 개념

### `LLM Response Layer`
- 현재 robot
- 현재 step
- 현재 joint 값
- EE pose
- recent snapshot/history
- audience mode

## LLM Modes
| llm_mode | required_runtime_context | goal | forbidden_outputs | fallback_policy |
|---|---|---|---|---|
| `Beginner Explain` | `robot_id`, `step_id`, `concept_refs`, `joint_values_deg`, `ee_pose` | 초심자에게 현재 개념을 쉽게 설명 | 계산값 변경, 미정 값을 단정 | `확정 불가`, `현재 값 기준으로만 설명 가능` |
| `Why It Moved` | `changed_joint`, `previous_value`, `current_value`, `delta`, `ee_pose`, `constraint_flags` | joint 입력이 왜 그런 움직임을 만들었는지 설명 | FK/pose 재계산 추측, 존재하지 않는 프레임 언급 | `앱 계산 결과 범위 안에서만 설명` |
| `Instructor Explain` | `lesson_id`, `concept_refs`, `reference_family`, `current_pose_summary` | 강사가 수업에서 바로 쓸 해설 제공 | 과도한 장문 교과서 인용, source confusion | `강사용 설명은 현재 lesson 범위 기준` |

## Prompt Policy
- 계산값은 앱 상태를 그대로 넣는다.
- LLM은 숫자를 수정하거나 새 값으로 교정하지 않는다.
- 모르는 내용은 추측 대신 `확정 불가`로 응답한다.
- `DH vs MDH` 혼동 가능성이 있으면 먼저 convention 여부를 확인하라고 안내한다.
