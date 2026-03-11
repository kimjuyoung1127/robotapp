# Milestone Backlog

## Purpose
- P0/P1/P2 백로그와 선후 관계를 정의한다.

## Parent Doc
- [PRODUCT-ROADMAP](../../PRODUCT-ROADMAP.md)

## When To Read
- 구현 우선순위, 문서 동기화, release planning 시

## Locked Decisions
- P0는 Guided Lesson 중심 UX, Tablet first, 다중 로봇 구조, 비공개 자료 정책
- P0는 완전 초보자도 수학 이전 직관 단계로 진입할 수 있어야 한다
- 확장 순서는 `Robot Library -> Sandbox -> Instructor Mode`
- 경쟁제품 분석은 우선순위를 올리기보다 `무엇을 우선하고 무엇을 배제할지`를 정하는 기준으로 사용한다

## Open Questions
- Progress와 Challenge 중 어느 쪽을 먼저 제품 MVP에 넣을지

## Downstream Sync
- `docs/ref/PRODUCT-ROADMAP.md`
- `docs/status/PROJECT-STATUS.md`

## Last Updated
- 2026-03-11 (KST)

## Backlog
| priority | item | competitive source | priority rationale |
|---|---|---|---|
| P0 | runtime snapshot + update cause foundation | 내부 runtime 안정화 요구 | Why It Moved, compare/history, beginner lesson이 중복 상태 로직 없이 같은 기반을 써야 한다 |
| P0 | track-aware step foundation | scaffolded onboarding 요구 | `pre_kinematics`와 `core_kinematics`를 같은 step 시스템 위에서 안전하게 복귀시켜야 한다 |
| P0 | Beginner Lesson 0~3 | `RoboX`, `UR Academy`의 scaffolded onboarding | sin/cos, 삼각형, IK 유도, 행렬/DH를 모르는 사용자도 진입할 수 있어야 한다 |
| P0 | Guided Lesson scaffolded UX 강화 | `RoboX`, `UR Academy` | 제품의 중심 경험이고 교육 구조 품질을 좌우한다 |
| P0 | Why It Moved | `RoboX`의 설명 구조를 넘는 차별점 | KineTutor3D만의 핵심 차별점 |
| P0 | joint highlight + numeric input | `RoboDK`류 실습성 + 자체 UX 차별화 | 수학 입력과 3D 움직임 연결의 핵심 |
| P0 | trail / target marker 공통 인프라 | `RoboDK`, `MIT Manipulation` | beginner lesson과 compare/target UX가 같은 시각화 기반을 공유해야 한다 |
| P0 | frame / pose teaching bridge | `Modern Robotics` | Lesson 0~3에서 Core Track으로 넘어갈 때 frame/pose 개념을 쉽게 연결해야 한다 |
| P1 | robot metadata 기반 Robot Library | `RoboDK`, `Visual Components` 참조 | Phase 5 P0 안정화 이후 다중 로봇 확장과 입문자용 탐색 UX의 기반 |
| P1 | Sandbox replay / repeatability / constraint preview | `RoboDK`, `CoppeliaSim` 참고 | 자유 실습을 학습 경험으로 전환 |
| P1 | Instructor demo mode | `RoboX`, `ABB`, `Visual Components` | 강사용 가치 강화 |
| P1 | pick foundation | `RoboDK`, `MIT Manipulation` | 실습성과 실제 과제 연결 시작점 |
| P1 | target pose compare + pick state machine | `MIT Manipulation`, `MoveIt 2` | pick foundation을 목표 자세와 상태 전이로 쉽게 설명할 수 있어야 한다 |
| P1 | convention badges + robot metadata detail | `Robotics Toolbox for Python` | DH/MDH/URDF-ready 차이를 강사와 학습자 모두 빠르게 이해할 수 있어야 한다 |
| P2 | Progress / assessment / challenge | `RoboX`, `ABB` | 교육 제품 완성도 향상 |
| P2 | Android tablet internal build | 모바일 배포 전략 | 실제 수업/심사 검증 필요 |
| P2 | LLM teaching layer | KineTutor3D 차별화 | why-it-moved와 강사용 설명 확장 |
| P3 | iPad/TestFlight | 모바일 배포 전략 | 후속 플랫폼 확장 |
| P3 | 기관용 reporting / cohort features | `RoboX`, `ABB` 참고 | 교육기관/B2B 확장 단계 |
