# KineTutor3D PRD

Version: 1.1.0  
Last Updated: 2026-03-12 (KST)

## Purpose

이 문서는 KineTutor3D의 제품 방향을 잠그는 root canonical PRD다. 상세 전략, 타겟, 성과 기준, 비공개 강의자료 정책은 하위 문서에서 관리하고, 여기에는 현재 결정과 읽기 경로만 유지한다.

## Locked Decisions

1. 주요 타겟은 `산업 입문자 + 강사`다.
2. 제품의 중심 경험은 `Guided Lesson`이다.
3. 완전 초보자는 `Pre-Kinematics Lesson 0~3`로 먼저 진입하고, 이후 `Core Track`으로 연결한다.
4. 플랫폼 정책은 `Desktop + Tablet first`다.
5. 로봇 구조는 `2DOF 고정`이 아니라 `다중 로봇 확장형`으로 간다.
6. 비공개 강의 PNG는 저장소/빌드/Git에 넣지 않고, 파생 개념과 UI 요구사항만 문서화한다.

## Change Summary

1. PRD를 root summary 문서로 축소했다.
2. 전략/타겟/성과 기준/비공개 자료 정책을 `docs/ref/product/foundation/`와 `docs/ref/product/content/` 아래로 분기했다.
3. 공개 로보틱스 reference pack과 LLM teaching strategy를 content leaf 문서로 추가했다.
4. 완전 초보자를 위한 `Pre-Kinematics Lesson 0~3` 진입 계층을 제품 잠금 결정에 추가했다.
5. 이후 제품 방향 변경은 leaf-first 규칙으로 하위 문서에서 먼저 다룬 뒤 이 문서에 잠금 결정을 반영한다.

## Read Next

- [product-positioning.md](./product/foundation/product-positioning.md)
- [target-users.md](./product/foundation/target-users.md)
- [success-metrics.md](./product/foundation/success-metrics.md)
- [lesson-framework.md](./product/content/lesson-framework.md)
- [derived-course-content-policy.md](./product/content/derived-course-content-policy.md)
- [open-robotics-reference-pack.md](./product/content/open-robotics-reference-pack.md)
- [llm-teaching-strategy.md](./product/content/llm-teaching-strategy.md)

## Downstream Sync

- `docs/status/PROJECT-STATUS.md`
- `ai-context/project-context.md`
- `ai-context/master-plan.md`

## Branching Rule

1. 이 문서에는 세부 화면 설계나 lesson step 상세를 넣지 않는다.
2. 제품 전략 세부는 `docs/ref/product/foundation/` 아래에서만 관리한다.
3. 비공개 자료 처리 세부는 `docs/ref/product/content/derived-course-content-policy.md`에서만 관리한다.
