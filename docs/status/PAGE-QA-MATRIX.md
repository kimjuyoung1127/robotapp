# Page QA Matrix

Last Updated: 2026-03-12 (KST)

## Purpose
- 현재 실제 진입 가능한 페이지를 기준으로 문서 계약 대비 기능 충족도, 진입 가능성, 레이아웃 무결성, UI 일관성, UX 흐름 품질을 한 번에 검증한다.
- 이번 문서는 `현재 상태를 잠그는 QA baseline`이며, 1차 목적은 수정이 아니라 페이지별 품질 상태와 우선순위를 수치화하는 것이다.

## Scope
- 감사 대상 페이지: `Onboarding`, `Home / Continue Hub`, `Guided Lesson`, `Math Readiness`, `Robot Library`, `Sandbox`
- 별도 체크 전용: `Boot` (사용자 UI 페이지가 아니라 라우팅 시스템 페이지)
- 점수 매트릭스 제외: `Instructor Mode`, `Progress`, `Settings`

## Evidence Sources
- 문서: `docs/ref/WIREFRAME.md`, `docs/ref/USER-FLOW.md`, `docs/ref/tutor-step-plan.md`, `docs/ref/product/ux/*.md`
- 코드/라우팅: `SceneId`, `SceneCatalog`, `SceneNavigator`, 각 페이지 controller/view builder
- PlayMode smoke: `SceneFlowSmokeTests`, `UIPanelDesignSystemSmokeTests`, `UxFlowSmokeTests`, `MathReadinessFlowSmokeTests`
- Unity 상태: active scene/hierarchy 확인, Unity Console 확인
- 빌드 보조 근거: `dotnet build KineTutor3D.Runtime.csproj` 성공, Unity Console compile error 별도 기록
- 수동 QA runbook: `docs/status/page-qa/README.md` 및 페이지별 runbook 6종

## Scoring Model
- 기능 충족도: 30점
- 진입 가능성: 20점
- 레이아웃 무결성: 20점
- UI 일관성: 15점
- UX 흐름 품질: 15점
- 총점: 100점

## Gate Rules
- `Blocker`: 페이지 진입 불가, 핵심 CTA 불가, 주요 패널 겹침으로 핵심 행동 차단, 복귀 불가, 필수 문서 기능 미구현
- `Major`: 진입은 되지만 핵심 보조 흐름 누락, 읽기 어려운 겹침/클리핑, 구조적 UI 패턴 불일치, UX 혼란이 크지만 우회 가능
- `Minor`: 간격, 정렬, 아이콘, 카피, 비핵심 상태 표현 불일치

## Value Legend
- `accessible`: `Yes | Partial | No`
- `overlap_status`: `Pass | AtRisk | Fail`

## Route Audit

| route_id | expected | status | evidence | notes |
|---|---|---|---|---|
| `boot_first_visit` | `Boot -> Onboarding` | `Pass` | `SceneFlowSmokeTests.Boot_FirstVisit_RoutesToOnboarding` | Boot는 라우팅 시스템 페이지로만 감사 |
| `boot_returning_user` | `Boot -> Home` | `Pass` | `SceneFlowSmokeTests.Boot_Visited_RoutesToHome` | 재방문 허브 기준 충족 |
| `onboarding_to_home` | `Onboarding -> Home` | `Pass` | `SceneFlowSmokeTests.Onboarding_StartLearning_LoadsHome_AndMarksVisited`, `Onboarding_Skip_LoadsHome`, `MathReadinessFlowSmokeTests.Onboarding_Beginner_LoadsHome_AndSetsMathTrack` | 3개 버튼 모두 Home 연결 |
| `home_continue` | `Home -> Main/Sandbox resume` | `Pass` | `HomeContinueHubFlowService.ContinueLatestContext()` | 세션 컨텍스트 기반 |
| `home_guided_lesson` | `Home -> Guided Lesson` | `Pass` | `HomeContinueHubFlowService.StartGuidedLesson()` | `Main` 라우팅 |
| `home_math_readiness` | `Home -> Math Readiness` | `Pass` | `HomeContinueHubFlowService.StartMathReadiness()` | `Main + math_readiness` 트랙 |
| `home_robot_library` | `Home -> Robot Library` | `Pass` | `HomeContinueHubFlowService.OpenRobotLibrary()` | 씬 라우팅 구현 완료 |
| `home_sandbox` | `Home -> Sandbox` | `Pass` | `HomeContinueHubFlowService.OpenSandbox()` | fallback robot 선택 포함 |
| `robot_library_guided_lesson` | `Robot Library -> Guided Lesson` | `Pass` | `RobotLibraryManager.OnStartLesson()`, `RobotDetailDrawer.OnLessonClicked()` | 카드/상세 진입 가능 |
| `robot_library_sandbox` | `Robot Library -> Sandbox` | `Pass` | `RobotLibraryManager.OnOpenSandbox()`, `RobotDetailDrawer.OnSandboxClicked()` | Sandbox 지원 로봇만 허용 |
| `guided_lesson_sandbox` | `Guided Lesson -> Sandbox` | `Partial` | `SceneNavigationBar`, `SceneCatalog` | 글로벌 nav는 있으나 lesson shell 전용 CTA 계약은 미충족 |
| `guided_lesson_home_return` | `Guided Lesson -> Home` | `Pass` | `SceneNavigationBar`, `SceneCatalog` | 전역 nav 기준 복귀 가능 |

## QA Matrix

| page_id | scene_or_route | source_docs | entry_paths | doc_features_total | implemented_count | missing_features | accessible | overlap_status | ui_consistency_score | ux_flow_score | blocker_count | major_count | minor_count | notes |
|---|---|---|---|---:|---:|---|---|---|---:|---:|---:|---:|---:|---|
| `onboarding` | `Onboarding.unity` | `WIREFRAME`, `USER-FLOW`, `information-architecture` | `Boot -> Onboarding` | 4 | 4 | `-` | `Yes` | `Pass` | 13 | 14 | 0 | 0 | 1 | 카드형 2선택 + 하단 둘러보기 구조로 정리되었고, 3개 진입 버튼은 `ModalSurface` 내부에 귀속된다. |
| `home_continue_hub` | `Home.unity` | `WIREFRAME`, `information-architecture`, `current-feature-checklist` | `Boot -> Home`, `Onboarding -> Home` | 6 | 6 | `Progress/Settings는 IA 선언만 있고 현재 disabled placeholder` | `Yes` | `Pass` | 14 | 13 | 0 | 0 | 1 | Continue, Guided Lesson, Math Readiness, Robot Library, Sandbox 진입 모두 존재. Progress/Settings는 점수 제외, 미구현 IA 항목으로 별도 기록. |
| `guided_lesson` | `Main.unity (core/pre-kinematics)` | `guided-lesson`, `tutor-step-plan`, `USER-FLOW` | `Home -> Main`, `Robot Library -> Main` | 8 | 6 | `lesson shell instructor CTA`, `save snapshot/compare entry` | `Partial` | `Pass` | 12 | 11 | 1 | 2 | 1 | Step/gate, joint input, matrix, beginner flow는 구현됐지만 `GL-01`과 `GL-06` 계약이 부분 미충족. Unity Console compile error가 현재 Main/Sandbox 신뢰성 QA를 막는 blocker로 기록됨. |
| `math_readiness` | `Main.unity (math_readiness track)` | `USER-FLOW`, `tutor-step-plan`, `current-feature-checklist` | `Onboarding Beginner -> Home -> Main`, `Home -> Main` | 7 | 7 | `-` | `Partial` | `Pass` | 13 | 14 | 1 | 0 | 1 | warmup, soft correction, single-joint rail, coach hint, pre-kinematics bridge가 smoke test로 확인됨. Main 공용 compile blocker의 영향은 동일하게 받음. |
| `robot_library` | `RobotLibrary.unity` | `robot-library`, `information-architecture` | `Home -> RobotLibrary` | 8 | 4 | `default filters`, `compare strip`, `card-level 3 CTA contract`, `Instructor Demo routing` | `Yes` | `AtRisk` | 12 | 9 | 0 | 3 | 1 | grid/detail drawer/basic routing은 구현. detail drawer가 우측 overlay 방식이라 좁은 해상도에서 grid 가림 위험이 높고, 문서 핵심 기능 누락이 큼. |
| `sandbox` | `Sandbox.unity` | `sandbox`, `tablet-first-policy`, `current-feature-checklist` | `Home -> Sandbox`, `Robot Library -> Sandbox`, `Guided Lesson -> Sandbox` | 10 | 6 | `replay/history`, `constraint preview`, `pick foundation`, `tablet 4DOF rail optimization` | `Partial` | `Fail` | 10 | 8 | 1 | 3 | 1 | zero/home/demo/reset, numeric input, why-it-moved, snapshot lite, exit buttons는 구현. 고정 폭 overlay 패널 구조와 기존 status 문서의 overlap 미해결 상태 때문에 레이아웃 실패로 판정. Unity Console compile error도 blocker. |

## IA Gaps (Not Scored This Pass)

| page_id | declared_in_docs | current_runtime_state | note |
|---|---|---|---|
| `instructor_mode` | `WIREFRAME`, `information-architecture`, `instructor-mode` | 미구현 | Home/Robot Library 어디에서도 실제 진입 불가 |
| `progress` | `WIREFRAME`, `information-architecture` | 미구현 | Home에 disabled placeholder 버튼만 존재 |
| `settings` | `WIREFRAME`, `information-architecture` | 미구현 | Home에 disabled placeholder 버튼만 존재 |

## Priority Order

| rank | page_id | reason | action_bucket |
|---|---|---|---|
| 1 | `sandbox` | compile blocker + overlap fail + P0 문서 기능 누락이 동시에 존재 | `Sandbox polish`, `tablet 4DOF`, `Page QA Hardening` |
| 2 | `guided_lesson` | core experience인데 lesson shell CTA와 save/replay entry 계약이 비어 있음 | `Home flow`, `Guided Lesson UX`, `Page QA Hardening` |
| 3 | `robot_library` | 문서 대비 기능 누락이 가장 크고 detail drawer overlay 위험이 큼 | `Robot Library UX`, `Page QA Hardening` |
| 4 | `math_readiness` | 기능은 충족하지만 Main 공용 compile blocker에 영향을 받음 | `Guided Lesson UX`, `Page QA Hardening` |
| 5 | `home_continue_hub` | 기능은 안정적이지만 Progress/Settings placeholder 정책을 정리해야 함 | `Home flow` |
| 6 | `onboarding` | 현재 범위에서는 가장 안정적임 | 유지 |

## Immediate Fix Candidates
- `Sandbox/Guided Lesson`: Unity Console compile error (`AppController -> SandboxSceneCoordinator.ApplySandboxPresentation(...)` 호출 계약 불일치) 해소
- `Sandbox`: 패널 overlay를 세로 비율/태블릿 기준으로 재배치하거나 접기/스크롤 구조로 바꿔 겹침을 제거
- `Guided Lesson`: `save snapshot/compare` 진입점과 lesson shell 전용 `Open Sandbox`/`Instructor` CTA 계약 정리
- `Robot Library`: filter/compare strip를 문서 기준 최소 버전으로 구현하고, detail drawer open 시 grid 폭 재계산 또는 modal화
- `Home`: `Progress/Settings`를 숨기거나 명시적 `Coming Soon` 정책으로 바꿔 dead-end 인상을 줄이기

## Notes
- baseline 작성 시점에는 Unity Console compile blocker(`Assets/Scripts/App/AppController.cs(358,37): error CS7036`)가 관찰되었다.
- 이후 동일 턴에서 `SandboxActionPanelViewBuilder`, `SnapshotLitePanelViewBuilder`를 수정해 Sandbox 패널을 좌/우 패널 내부 레이아웃으로 귀속시켰고, Unity refresh 후 콘솔 재확인에서는 해당 compile error가 재발하지 않았다.
- 총점/게이트는 baseline 점수로 유지하며, 다음 재감사에서 시각 QA 기준으로 재산정한다.
