# Information Architecture

## Purpose
- 전체 제품 화면 구조와 진입 관계를 정의한다.

## Parent Doc
- [WIREFRAME](../../WIREFRAME.md)

## When To Read
- Home, Robot Library, Sandbox, Instructor Mode 사이 관계를 설계할 때

## Locked Decisions
- 제품 구조는 `Onboarding / Home / Guided Lesson / Robot Library / Sandbox / Instructor Mode / Progress / Settings`
- `Home`이 future default hub
- `Guided Lesson`이 main learning surface

## Open Questions
- `Challenge`를 독립 화면으로 둘지 Guided Lesson 하위 completion path로 둘지

## Downstream Sync
- `docs/ref/WIREFRAME.md`
- `docs/ref/USER-FLOW.md`
- 필요 시 `docs/ref/architecture-diagrams.md`

## Last Updated
- 2026-03-11 (KST)

## Screen Graph
- `Onboarding -> Home`
- `Home -> Guided Lesson`
- `Home -> Robot Library`
- `Robot Library -> Guided Lesson`
- `Robot Library -> Sandbox`
- `Home -> Instructor Mode`
- `Home -> Progress`
- `Home -> Settings`
