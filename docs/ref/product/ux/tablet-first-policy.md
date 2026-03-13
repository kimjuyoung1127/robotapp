# Tablet First Policy

## Purpose
- Desktop/Tablet/Phone 정책과 제한형 모바일 UX를 정의한다.

## Parent Doc
- [WIREFRAME](../../WIREFRAME.md)

## When To Read
- 반응형 레이아웃, 태블릿 지원, 모바일 축소 정책을 설계할 때

## Locked Decisions
- 정식 지원은 Desktop + Tablet
- Phone은 제한형 UX
- 작은 화면에서는 탭 전환형 구성을 우선한다
- 4DOF 이상 joint rail은 tablet에서 접기/스크롤/분할 구성을 허용한다

## Open Questions
- WebGL 태블릿 성능 기준을 어디서 끊을지

## Downstream Sync
- `docs/ref/WIREFRAME.md`
- 필요 시 `docs/ref/architecture-diagrams.md`

## Last Updated
- 2026-03-12 (KST)

## Policy
- Desktop/Tablet: 3D + 설명 + 정보 + 조작 동시 제공
- Tablet(특히 4DOF 이상): 3D viewport 우선, joint rail은 접기/스크롤/분할로 내려도 된다
- Phone: 3D 뷰 중심, 설명/행렬은 탭 전환, 제한형 Guided Lesson 우선
