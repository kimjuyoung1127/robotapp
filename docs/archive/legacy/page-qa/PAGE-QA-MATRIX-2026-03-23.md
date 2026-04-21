# Page QA Matrix

> Archive Note: 이 문서는 2026-03-23 시점 QA baseline이라 archive로 이동했다.

Last Updated: 2026-03-23 (KST)

## Purpose
- 현재 실제 진입 가능한 페이지를 기준으로 문서 계약 대비 기능 충족도, 진입 가능성, 레이아웃 무결성, UI 일관성, UX 흐름 품질을 한 번에 검증한다.
- 이번 문서는 `현재 상태를 잠그는 QA baseline`이며, 1차 목적은 수정이 아니라 페이지별 품질 상태와 우선순위를 수치화하는 것이다.

## Scope
- 감사 대상 페이지: `Onboarding`, `RobotLibrary`, `RobotControl`, `MathReadiness`
- 제외 페이지: `Sandbox` (현재 제작 중, 이번 UGUI 감사 라운드 제외)
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
| `boot_returning_user` | `Boot -> RobotLibrary` | `Pass` | `BootSceneRouter` — 재방문 시 RobotLibrary로 직행 | Home 제거됨 (2026-03-23) |
| `onboarding_to_robotlibrary` | `Onboarding -> RobotLibrary` | `Pass` | `OnboardingManager.BeginLearning()` | 학습 시작 → RobotLibrary |
| `onboarding_to_sandbox` | `Onboarding -> Sandbox` | `Pass` | `OnboardingManager.SkipToSandbox()` | 건너뛰기 → Sandbox |
| `onboarding_to_math` | `Onboarding -> MathReadiness` | `Pass` | `OnboardingManager` beginner flow | 초보자 시작 |
| `robot_library_robot_control` | `RobotLibrary -> RobotControl` | `Pass` | `RobotDetailDrawer.OnRobotControlClicked()` | FR5 전용 제어 콘솔 |
| `robot_library_sandbox` | `RobotLibrary -> Sandbox` | `Pass` | `RobotLibraryManager.OnOpenSandbox()`, `RobotDetailDrawer.OnSandboxClicked()` | Sandbox 지원 로봇만 허용 |
| `robot_library_math` | `RobotLibrary -> MathReadiness` | `Pass` | `SceneNavigationBar` | 수학 기초 진입 |
| `sandbox_to_library` | `Sandbox -> RobotLibrary` | `Pass` | `SandboxActionFlowService.BackToLibrary()` | 로봇 목록 복귀 |

## 2026-03-23 UGUI Audit Lock
- 실제 `Assets/Scenes` 기준 활성 씬은 `Boot`, `Onboarding`, `MathReadiness`, `RobotLibrary`, `RobotControl`, `Sandbox`다.
- 이번 UGUI 감사 라운드는 `Sandbox`를 의도적으로 제외한다.
- `Home/Main`은 현재 `Assets/Scenes`에 존재하지 않으므로 활성 QA 범위에서 제외하고 문서 drift로만 관리한다.

## QA Matrix

| page_id | scene_or_route | source_docs | entry_paths | doc_features_total | implemented_count | missing_features | accessible | overlap_status | ui_consistency_score | ux_flow_score | blocker_count | major_count | minor_count | notes |
|---|---|---|---|---:|---:|---|---|---|---:|---:|---:|---:|---:|---|
| `onboarding` | `Onboarding.unity` | `WIREFRAME`, `USER-FLOW` | `Boot -> Onboarding` | 4 | 4 | `-` | `Yes` | `Pass` | 13 | 14 | 0 | 0 | 1 | 카드형 2선택 + 하단 둘러보기 구조. 학습시작→RobotLibrary, 건너뛰기→Sandbox, 초보자→MathReadiness. |
| `robot_library` | `RobotLibrary.unity` | `robot-library`, `WIREFRAME` | `Boot -> RobotLibrary`, `Onboarding -> RobotLibrary` | 8 | 5 | `default filters`, `compare strip`, `Instructor Demo routing` | `Yes` | `AtRisk` | 12 | 10 | 0 | 2 | 1 | **메인 진입점**. grid/detail drawer/RobotControl CTA/Sandbox CTA 구현. detail drawer 좁은 해상도 가림 위험. |
| `robot_control` | `RobotControl.unity` | `fairino-fr5-integration`, `USER-FLOW` | `RobotLibrary -> RobotControl` | 10 | 10 | `-` | `Yes` | `Pass` | 14 | 14 | 0 | 0 | 0 | FR5 전용 제어 콘솔 구현 완료. 멀티로봇 확장 진행중. |
| `math_readiness` | `MathReadiness.unity` | `USER-FLOW`, `tutor-step-plan` | `Onboarding -> MathReadiness`, `RobotLibrary -> MathReadiness` | 7 | 7 | `-` | `Yes` | `Pass` | 13 | 14 | 0 | 0 | 1 | warmup, soft correction, single-joint rail, coach hint, RobotLibrary bridge. |

## IA Gaps (Not Scored This Pass)

| page_id | declared_in_docs | current_runtime_state | note |
|---|---|---|---|
| `instructor_mode` | `WIREFRAME`, `information-architecture`, `instructor-mode` | 미구현 | RobotLibrary에서 실제 진입 불가 |
| `progress` | `WIREFRAME`, `information-architecture` | 미구현 | Home 제거로 placeholder도 삭제됨 |
| `settings` | `WIREFRAME`, `information-architecture` | 미구현 | Home 제거로 placeholder도 삭제됨 |
| `home_main_docs` | `page-quality-baseline`, legacy docs | `Assets/Scenes` 기준 비활성 | Home/Main은 현재 활성 QA 범위에서 제외 |

## Priority Order

| rank | page_id | reason | action_bucket |
|---|---|---|---|
| 1 | `robot_library` | 메인 진입점, 구조 비대화 + 반응형/상태 UI 위험 동시 존재 | `Page QA Hardening`, `RobotLibraryViewBuilder` |
| 2 | `robot_control` | giant coordinator + 고정 좌표 의존 + 상태 UI 공통화 부족 | `Page QA Hardening`, `Panel split` |
| 3 | `onboarding` | builder는 있으나 반응형 약함 | `Onboarding responsiveness` |
| 4 | `math_readiness` | 기능 안정적, 파일 책임 과다 | `MathReadiness split` |
| 5 | `boot` | 최소 로딩 UI만 필요 | `Boot polish` |

## Immediate Fix Candidates
- `Robot Library`: filter/compare strip를 문서 기준 최소 버전으로 구현하고, detail drawer open 시 grid 폭 재계산 또는 modal화
- `Robot Control`: 멀티로봇 확장 (Part B: UR5e, Doosan M1013, Meca500)
- `Onboarding`: 고정 modal/card 크기 제거, `UILayoutProfile` 기준 카드 배치 전환

## Notes
- 2026-03-23 UGUI 감사 라운드는 `Sandbox`를 의도적으로 제외했다.
- `Home/Main`은 현재 `Assets/Scenes`에 존재하지 않으므로 활성 QA 범위에서 제외하고 문서 drift로만 관리한다.
- baseline 작성 시점에는 Unity Console compile blocker(`Assets/Scripts/App/AppController.cs(358,37): error CS7036`)가 관찰되었다.
- 이후 동일 턴에서 `SandboxActionPanelViewBuilder`, `SnapshotLitePanelViewBuilder`를 수정해 Sandbox 패널을 좌/우 패널 내부 레이아웃으로 귀속시켰고, Unity refresh 후 콘솔 재확인에서는 해당 compile error가 재발하지 않았다.
- 총점/게이트는 baseline 점수로 유지하며, 다음 재감사에서 시각 QA 기준으로 재산정한다.
