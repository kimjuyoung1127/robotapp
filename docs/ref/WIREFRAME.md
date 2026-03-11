# KineTutor3D Wireframe

Version: 1.1.0  
Last Updated: 2026-03-11 (KST)

## Purpose

이 문서는 KineTutor3D의 UX 구조와 화면 계층을 잠그는 root canonical wireframe 문서다. 화면별 세부 스펙과 tablet/mobile 정책은 하위 UX 문서에서 관리하고, 여기에는 마스터 IA와 읽기 경로만 유지한다.

## Locked Decisions

1. 제품 정보 구조는 `Onboarding / Home / Guided Lesson / Robot Library / Sandbox / Instructor Mode / Progress / Settings`로 간다.
2. `Guided Lesson`이 메인 경험이고, 나머지 화면은 준비/실습/운영을 보조한다.
3. `Guided Lesson` 안에서는 `완전 초보 -> Pre-Kinematics Lesson 0~3 -> Core Track Step 1~8` 흐름을 기본 학습 경로로 본다.
4. `Robot Library -> Guided Lesson/Sandbox`, `Instructor Mode -> Guided Lesson` 흐름을 기본으로 본다.
5. Desktop과 Tablet이 정식 UX 기준이며, Phone은 제한형 정책으로 다룬다.

## Master Flow

```text
Boot -> Onboarding -> Home
Home -> Guided Lesson
Guided Lesson -> Pre-Kinematics Lesson 0~3
Pre-Kinematics Lesson 0~3 -> Core Track Step 1~8
Home -> Robot Library
Robot Library -> Guided Lesson
Robot Library -> Sandbox
Guided Lesson -> Sandbox
Guided Lesson -> Challenge
Home -> Instructor Mode
Home -> Progress
Home -> Settings
```

## Change Summary

1. Wireframe을 root summary 문서로 축소했다.
2. IA, Guided Lesson, Robot Library, Sandbox, Instructor Mode, Tablet 정책을 `docs/ref/product/ux/` 아래로 분기했다.
3. 완전 초보자를 위한 `Pre-Kinematics Lesson 0~3`를 Guided Lesson 기본 경로에 추가했다.
4. 이후 구조 변경은 leaf 문서에서 먼저 정의하고, 이 문서에 잠금 구조만 반영한다.

## Read Next

- [information-architecture.md](./product/ux/information-architecture.md)
- [guided-lesson.md](./product/ux/guided-lesson.md)
- [robot-library.md](./product/ux/robot-library.md)
- [sandbox.md](./product/ux/sandbox.md)
- [instructor-mode.md](./product/ux/instructor-mode.md)
- [tablet-first-policy.md](./product/ux/tablet-first-policy.md)

## Downstream Sync

- `docs/ref/USER-FLOW.md`
- `docs/ref/tutor-step-plan.md`
- 필요 시 `docs/ref/architecture-diagrams.md`

## Branching Rule

1. 이 문서에는 세부 패널 규칙, 버튼 상태, 반응형 breakpoint를 넣지 않는다.
2. Guided Lesson 세부는 `docs/ref/product/ux/guided-lesson.md`에서만 관리한다.
3. Robot Library, Sandbox, Instructor Mode 세부는 각 leaf 문서에서만 관리한다.
