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
- active backlog는 이미 구현 완료된 기반 항목을 제외하고 `다음 남은 우선순위`만 유지한다
- UX 완성 순서는 `Home / Continue Hub -> Sandbox polish -> Instructor Mode`
- 경쟁제품 분석은 우선순위를 올리기보다 `무엇을 우선하고 무엇을 배제할지`를 정하는 기준으로 사용한다

## Open Questions
- Progress와 Challenge 중 어느 쪽을 먼저 제품 MVP에 넣을지

## Downstream Sync
- `docs/ref/PRODUCT-ROADMAP.md`
- `docs/status/PROJECT-STATUS.md`

## Last Updated
- 2026-03-12 (KST)

## Backlog
| priority | item | competitive source | priority rationale |
|---|---|---|---|
| P0 | Home / Continue Hub | `Duolingo`, `Khan Academy`식 continue entry | 로봇/모드가 늘어난 현재 상태에서 재방문과 온보딩 skip의 공통 착지점이 필요하다 |
| P0 | Sandbox MVP polish | `RoboDK`의 실습성 | baseline scene이 실제 학습 공간으로 느껴지려면 zero/home/demo/reset, marker feedback, exit clarity가 먼저 필요하다 |
| P0 | resume / session context 확장 | scaffolded onboarding 요구 | `robot_id + mode`를 넘어서 마지막 로봇/모드/프리셋/복귀 타겟을 함께 기억해야 한다 |
| P0 | tablet-first 4DOF input usability | tablet-first 정책 | SCARA와 4DOF rail이 들어온 시점부터 viewport 우선 레이아웃과 입력 압축 전략이 필요하다 |
| P0 | snapshot lite | `RoboDK`, `Tinkercad`류 상태 저장 경험 | replay보다 먼저 현재 자세 저장/복귀/빠른 비교가 있어야 Sandbox 학습성이 생긴다 |
| P0 | Guided Lesson scaffolded UX 강화 | `RoboX`, `UR Academy` | 제품의 중심 경험이고 `Home / Continue Hub`와도 자연스럽게 연결돼야 한다 |
| P0 | frame / pose teaching bridge | `Modern Robotics` | Lesson 0~3에서 Core Track으로 넘어갈 때 frame/pose 개념을 쉽게 연결해야 한다 |
| P1 | replay / compare / motion history | `RoboDK`, `CoppeliaSim` 참고 | 자유 실습을 학습 경험으로 전환하는 다음 단계다 |
| P1 | constraint preview + workspace intuition | `robot-gui`, `Peter Corke RTB` | limit/workspace/singularity를 직관적으로 보여주는 후속 가치다 |
| P1 | Instructor demo mode | `RoboX`, `ABB`, `Visual Components` | 강사용 가치 강화 |
| P1 | asset subset Git tracking | 내부 운영 안정화 요구 | curated subset과 vendor fallback 기준을 배포/협업 기준에서도 안정화해야 한다 |
| P1 | pick foundation | `RoboDK`, `MIT Manipulation` | 실습성과 실제 과제 연결 시작점 |
| P1 | target pose compare + pick state machine | `MIT Manipulation`, `MoveIt 2` | pick foundation을 목표 자세와 상태 전이로 쉽게 설명할 수 있어야 한다 |
| P1 | convention badges + robot metadata detail | `Robotics Toolbox for Python` | DH/MDH/URDF-ready 차이를 강사와 학습자 모두 빠르게 이해할 수 있어야 한다 |
| P1 | URDF Import 기반 로봇 확장 | `Unity URDF Importer`, `ros-industrial` | UR5/Puma560/Franka URDF 사전 조사 완료, Robot Library 스케일링의 유일한 현실적 방법 |
| P1 | interactive matrix viz 확장 (ncase.me 패턴) | `ncase.me/matrix`, `Matrix Arcade` | 행렬 셀과 3D 좌표계 변화의 양방향 실시간 연결, 디자인 레퍼런스 문서화 완료 |
| P1 | workspace envelope 시각화 | `robot-gui`, `Peter Corke RTB` | DH 파라미터 변경에 따른 도달 영역 변화를 실시간 시각화, 2DOF 해석적 → N-DOF Monte Carlo |
| P2 | Progress / assessment / challenge | `RoboX`, `ABB` | 교육 제품 완성도 향상 |
| P2 | Android tablet internal build | 모바일 배포 전략 | 실제 수업/심사 검증 필요 |
| P2 | LLM teaching layer | KineTutor3D 차별화 | why-it-moved와 강사용 설명 확장 |
| P3 | iPad/TestFlight | 모바일 배포 전략 | 후속 플랫폼 확장 |
| P3 | 기관용 reporting / cohort features | `RoboX`, `ABB` 참고 | 교육기관/B2B 확장 단계 |
