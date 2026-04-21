# KineTutor3D Wireframe

Version: 1.2.0  
Last Updated: 2026-03-12 (KST)

## Purpose

이 문서는 KineTutor3D의 UX 구조와 화면 계층을 잠그는 root canonical wireframe 문서다. 화면별 세부 스펙과 tablet/mobile 정책은 하위 UX 문서에서 관리하고, 여기에는 마스터 IA와 읽기 경로만 유지한다.

## Locked Decisions

1. 제품 정보 구조는 `Onboarding / Robot Library / RobotControl / Sandbox / MathReadiness / Instructor Mode / Progress / Settings`로 간다. (Home/Main 제거됨, 2026-03-23)
2. `RobotLibrary`가 메인 진입점이며 로봇 카탈로그 + 3D showroom을 제공한다.
3. `RobotLibrary`를 재방문 기본 진입점으로 두고, 사용자가 `로봇 제어(RobotControl) / 자유 조작(Sandbox) / 수학 기초(MathReadiness)`를 선택할 수 있게 한다.
4. `Guided Lesson` 안에서는 `완전 초보 -> Pre-Kinematics Lesson 0~3 -> Core Track Step 1~8` 흐름을 기본 학습 경로로 본다.
5. `Robot Library -> Guided Lesson/Sandbox`, `Instructor Mode -> Guided Lesson` 흐름을 기본으로 본다.
6. Desktop과 Tablet이 정식 UX 기준이며, Phone은 제한형 정책으로 다룬다.

## Master Flow

```text
Boot -> Onboarding (첫 방문)
Boot -> RobotLibrary (재방문)
Onboarding -> RobotLibrary (학습 시작 / 건너뛰기)
Onboarding -> MathReadiness (초보자 시작)
RobotLibrary -> RobotControl (로봇 선택 → 전용 제어)
RobotLibrary -> Sandbox (로봇 선택 → 자유 조작)
RobotLibrary -> MathReadiness (수학 기초)
MathReadiness -> RobotLibrary (M3 완료)
Sandbox -> RobotLibrary (로봇 목록 복귀)
```

## Change Summary

1. Wireframe을 root summary 문서로 축소했다.
2. IA, Guided Lesson, Robot Library, Sandbox, Instructor Mode, Tablet 정책을 `docs/ref/product/ux/` 아래로 분기했다.
3. 완전 초보자를 위한 `Pre-Kinematics Lesson 0~3`를 Guided Lesson 기본 경로에 추가했다.
4. `Home / Continue Hub`를 차기 P0 허브로 승격했다.
5. 이후 구조 변경은 leaf 문서에서 먼저 정의하고, 이 문서에 잠금 구조만 반영한다.

## Read Next

- [USER-FLOW.md](./USER-FLOW.md)
- [page-qa/README.md](../status/page-qa/README.md)
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
