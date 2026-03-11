# Robot Model Library Spec

## Purpose
- 다중 로봇 모델 메타데이터와 지원 모드를 정의한다.

## Parent Doc
- [WIREFRAME](../../WIREFRAME.md)

## When To Read
- 로봇 선택, 템플릿 추가, Robot Library 카드, 로봇별 지원 범위를 설계할 때

## Locked Decisions
- 로봇은 template + metadata 조합으로 관리한다
- Guided Lesson과 Sandbox 지원 여부를 분리한다
- 난이도와 설명 규칙을 로봇별로 가진다
- 로봇 메타데이터는 수업용 badge와 입력/지원 범위를 직접 구동할 수 있을 정도로 충분해야 한다
- `DH / MDH / URDF-ready` 같은 convention 정보는 detail drawer와 강사용 노트에서 반드시 보일 수 있어야 한다

## Open Questions
- 강사용 추천 scenario를 metadata에 둘지 별도 데이터로 둘지

## Downstream Sync
- `docs/ref/product/ux/robot-library.md`
- `docs/ref/product/robots/robot-template-expansion.md`
- `docs/ref/product/robots/urdf-reference-collection.md`

## Last Updated
- 2026-03-11 (KST)

## Metadata Fields
- `robot_id`
- `display_name`
- `dof`
- `robot_type`
- `difficulty`
- `guided_lesson_supported`
- `sandbox_supported`
- `instructor_recommended`
- `description`
- `supported_lessons`
- `input_modes`
- `visualization_level`
- `convention`
- `joint_limits`
- `zero_pose`
- `home_pose`
- `demo_pose`
- `pick_foundation_supported`
- `import_source`

## Display Rules
- `difficulty`, `dof`, `convention`, `supported_modes`는 Robot Library 카드나 detail drawer에서 badge로 노출할 수 있어야 한다.
- `guided_lesson_supported`, `sandbox_supported`, `pick_foundation_supported`는 CTA 활성/비활성의 근거가 된다.
- `convention`은 최소 `DH`, `MDH`, `URDF-ready` 중 하나로 분류한다.
- `zero_pose`, `home_pose`, `demo_pose`는 replay, instructor demo, reset 기준값으로 재사용한다.
