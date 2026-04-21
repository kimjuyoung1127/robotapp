# Information Architecture

> Archive Note: 이 문서는 `Home / Continue Hub` 중심 target IA 문서라 archive로 이동했다.

## Purpose
- 전체 제품 화면 구조와 진입 관계를 정의한다.

## Parent Doc
- [WIREFRAME](../../WIREFRAME.md)

## When To Read
- Home, Robot Library, Sandbox, Instructor Mode 사이 관계를 설계할 때

## Locked Decisions
- 제품 구조는 `Onboarding / Home / Guided Lesson / Robot Library / Sandbox / Instructor Mode / Progress / Settings`
- `Home / Continue Hub`가 차기 기본 허브이자 재진입 surface다
- `Guided Lesson`이 main learning surface

## Open Questions
- `Challenge`를 독립 화면으로 둘지 Guided Lesson 하위 completion path로 둘지

## Downstream Sync
- `docs/ref/WIREFRAME.md`
- `docs/ref/USER-FLOW.md`
- 필요 시 `docs/ref/architecture-diagrams.md`

## Last Updated
- 2026-03-12 (KST)

## Screen Graph
- `Onboarding -> Home / Continue Hub`
- `Home / Continue Hub -> Guided Lesson`
- `Home / Continue Hub -> Continue Latest Context`
- `Home / Continue Hub -> Robot Library`
- `Home / Continue Hub -> Sandbox`
- `Robot Library -> Guided Lesson`
- `Robot Library -> Sandbox`
- `Home / Continue Hub -> Instructor Mode`
- `Home / Continue Hub -> Progress`
- `Home / Continue Hub -> Settings`
