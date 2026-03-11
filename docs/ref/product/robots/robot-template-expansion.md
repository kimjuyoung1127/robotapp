# Robot Template Expansion

## Purpose
- 로봇 확장 순서와 각 단계의 조건을 정의한다.

## Parent Doc
- [PRODUCT-ROADMAP](../../PRODUCT-ROADMAP.md)

## When To Read
- 새 로봇 template 추가, Lesson 범위 확장, 시각화 확장을 계획할 때

## Locked Decisions
- 확장 순서는 `2DOF -> SCARA -> 3DOF -> 6DOF`
- 새 로봇은 metadata, lesson 지원 여부, 시각화 경로를 함께 가져야 한다

## Open Questions
- 6DOF를 fully interactive로 할지 demo-first로 할지

## Downstream Sync
- `docs/ref/PRODUCT-ROADMAP.md`
- `docs/ref/product/robots/robot-model-library-spec.md`

## Last Updated
- 2026-03-11 (KST)

## Expansion Steps
- 2DOF: baseline lesson 유지
- SCARA: 산업 입문자 설명용 첫 확장
- 3DOF: 구조 비교용 교육 모델
- 6DOF: 산업용 대표 사례와 시연용

## URDF 사전 수집
- UR5, Puma560, Franka Emika Panda의 URDF 소스와 DH 파라미터를 사전 조사 완료
- 상세: [urdf-reference-collection.md](./urdf-reference-collection.md)
- URDF Import 전략 (Unity URDF Importer vs NASA JPL urdf-loaders) 비교 포함
