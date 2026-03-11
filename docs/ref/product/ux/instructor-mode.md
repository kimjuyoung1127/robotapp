# Instructor Mode

## Purpose
- 강사용 시연 모드의 진입 방식과 기능 범위를 정의한다.

## Parent Doc
- [WIREFRAME](../../WIREFRAME.md)

## When To Read
- 강사용 기능, 시연 UI, step jump, 수업 운영 기능을 설계할 때

## Locked Decisions
- Instructor Mode는 강사를 위한 별도 운영 모드
- 특정 로봇과 특정 step으로 바로 진입할 수 있어야 한다
- 설명 강조와 카메라 제어가 필요하다
- `lesson jump`, `focus override`, `teaching note`, `demo preset`을 core 기능으로 둔다
- 강사용 흐름은 `RoboX`, `ABB`, `Visual Components`의 장점을 흡수하되 산업 툴 복잡도는 배제한다

## Open Questions
- 학습자용 UI와 강사용 UI를 완전히 분리할지

## Downstream Sync
- `docs/ref/WIREFRAME.md`
- `docs/ref/product/foundation/target-users.md`

## Last Updated
- 2026-03-11 (KST)

## Core Functions
- step jump
- robot quick load
- focus/highlight overlay
- camera lock/preset
- reset/demo mode
- teaching note
- demo preset
