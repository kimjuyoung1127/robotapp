# KineTutor3D 프로젝트 인덱스

KineTutor3D 작업 시작 시 가장 먼저 읽는 진입 문서입니다.
이 문서만 읽어도 현재 단계, 규칙, 다음 행동을 빠르게 파악할 수 있게 유지합니다.

## 저장소 경계
- Write Repo: `.` (저장소 루트 — clone 위치 무관)

## 시작 순서 (필수)
1. `AGENTS.md` (Codex) 또는 `CLAUDE.md` (Claude) - 동일 정책 진입 문서
2. `docs/ref/architecture-mermaid.md`
3. `docs/status/PRODUCT-DOC-BOARD.md`
4. `docs/ref/PRD.md`
5. `docs/ref/WIREFRAME.md`
6. `docs/ref/PRODUCT-ROADMAP.md`
7. `docs/ref/phase5-implementation-plan.md` (Phase 5 구현/검수 시 필수)

## 현재 상태 (2026-03-12)
- Phase 0: Done
- Phase 1: Done
- Phase 2: Done
- Phase 3 (Template 2DOF + App/UI): Done
- Phase 4 (Visualization core): Done
- Phase 5: InProgress (5A~5F Done, 5G remaining)
- Phase 6 (CI/CD): Hold (로컬 테스트 전용, runner 미등록)
- Stability Refactor (App/UI/Visualization componentization): Done
- Product Docs Governance (GameLab-style): InProgress

최근 확정 사항:
- Phase 5F 완료: Robot Library MVP — RobotMetadataInfo/RobotCatalogEntry(Types), RobotCatalog(Templates, 5개 로봇 등록), RobotSelectionBridge(App), RobotLibrary.unity 씬, RobotLibraryManager/RobotCardBuilder/RobotDetailDrawer(UI), SceneNavigationBar 버튼 재바인딩 안정화
- Phase 5E 완료: BeginnerLessonFactory(L0~L3), BeginnerLeftPanel, CompareModePanelHelper, TargetFeedbackPanel 추가, OnboardingManager 초보자 버튼 추가
- Phase 5D 완료: WhyItMovedState/Formatter/Panel 추가, AppController+AppUiBinder 연동
- GameLab-style 제품 문서 운영 이식 시작: canonical product docs 3종(`PRD`, `WIREFRAME`, `PRODUCT-ROADMAP`)과 `PRODUCT-DOC-BOARD`를 status/ref 계층에 추가
- Beginner Lesson 0~3를 `Pre-Kinematics` 진입 트랙으로 추가하고 `Core Track Step 1~8`과 분리
- `current-feature-checklist`를 기준으로 현재 구현 범위와 우선 추가 기능을 한 문서에서 추적
- 경쟁제품 synthesis, LLM teaching strategy, mobile release checklist를 제품 문서 체계에 통합
- 내부 패키지 자산을 `Assets/KineTutor_AssetCuration_BACKUP/`로 큐레이션하고 hierarchy validation report를 추가
- Phase 3 확장 완료: `TemplateSelector`, `DHTableEditor`, `MatrixDisplay` 실동작 연결
- Scene split 완료: `Boot.unity` -> `Onboarding.unity` / `Main.unity` 분기 구조 도입
- Build Settings 재구성: `Boot`(0), `Onboarding`(1), `Main`(2), `RobotLibrary`(3)
- Phase 4 확장: `frame_0`/`frame_1`을 canonical frame object로 통합, `Frame_EE` 유지
- Phase 4B 디버그: `ScaraRobot.prefab` donor path를 `Base -> Axis1 -> Axis2 -> Axis3 -> Gripper`로 명시 고정하고 `Pick`은 helper point로 제외
- Phase 4B 디버그: `Canvas`를 `Screen Space - Overlay` HUD로 전환하고 Scene/Game에서 동일한 학습 UI 구성을 사용
- Phase 4B HUD 디버그: `WelcomeModal` placeholder와 중앙 viewport 포커스 하이라이트를 기본 비활성화해 Play 중 중앙 흰 사각형이 더 이상 표시되지 않도록 수정
- HUD 아티팩트 정리: `GlossaryPanel` 기본 활성 상태를 제거하고 inactive-safe 자동 배선을 적용해 중앙 파란 박스 원인 경로를 차단
- HUD 아티팩트 정리: `SceneNavigationBar`/공통 UI 스타일 경로를 보강해 상단 네비 버튼의 red X/미표시 문제를 수정
- 안정성 우선 리팩터링 완료: `RobotRenderer`를 facade + binder/mapper/copier/probe helper 구조로 분리
- 안정성 우선 리팩터링 완료: `AppController`를 facade + `StepFlowService`/`KinematicsRuntimeService`/`AppUiBinder` 구조로 분리
- 안정성 우선 리팩터링 완료: `DHTableEditor`에서 parse/build 책임을 `DHTableValueFormatter`/`DHTableViewBuilder`로 분리
- 문서 탐색 규칙 추가: 루트 `AGENTS.md`와 `docs/ref/architecture-mermaid.md`를 새 세션 기본 진입점으로 고정
- Main 순수화: `Main.unity`는 로봇/HUD 전용 씬으로 유지하고 `OnboardingManager` 런타임 의존 제거
- 온보딩 분리: `Onboarding.unity`는 `OnboardingManager` + 전역 네비게이션만 담당
- 전역 씬 이동 추가: `SceneNavigator`, `SceneCatalog`, `SceneNavigationBar`, `BootSceneRouter` 도입
- 학습 화면 MVP 정리: `TopBar`/`LeftPanel`/`RightPanel`/`BottomBar` 4영역으로 정리하고 런타임 디버그성 흰 패널/텍스트를 공통 스타일 surface로 대체
- Phase 4 디버그: Built-in에서 URP(`com.unity.render-pipelines.universal@17.0.4`)로 전환하고 `GraphicsSettings`/`QualitySettings`를 `URP-Default.asset`에 고정
- Camera 정리: `Main Camera`를 Solid Color + 2DOF 학습 구도로 조정하고 donor mesh local offset/scale 보정 경로를 `RobotRenderer`에 고정
- Unity Test Runner 결과: EditMode `142`, PlayMode `44` (코드 어트리뷰트 기준, Runner 재확인 필요)
- CI 초안 추가: `.github/workflows/unity-tests.yml`

## 실행 규칙 (MUST)
1. 기존 코드/타입/유틸 우선 재사용, 중복 구현 금지
2. `Assets/Scripts/` 폴더 구조를 모듈 Source of Truth로 사용
3. Math/Types/Kinematics는 pure C# `double` 유지, `UnityEngine` 참조 금지
4. NaN/Infinity 입력은 FK 계산 전에 차단
5. `theta`는 Slider 단일 소스, DHTable에서는 read-only
6. 문서와 코드 상태가 다르면 코드/테스트 실제 상태를 우선
7. 명시 요청 없이는 임의 Git 파괴 명령 금지
8. **C# 파일 생성/수정 전에 `docs/ref/code-patterns.md`를 반드시 읽고 §8-9 패턴을 준수** (인코딩, 헤더, 네이밍, 수명주기)

## Skill 인덱스 (.claude/skills)
| # | Skill | Trigger 키워드 | 경로 |
|---|---|---|---|
| 1 | math-module-add | math, vector, matrix | `kinetutor-guide/core/math-module-add/` |
| 2 | dh-algorithm-add | DH, FK, kinematics | `kinetutor-guide/kinematics/dh-algorithm-add/` |
| 3 | robot-template-add | template, 2DOF/SCARA | `kinetutor-guide/templates/robot-template-add/` |
| 4 | tutor-step-add | step tutor, learning step | `kinetutor-guide/ui/tutor-step-add/` |
| 5 | editmode-test-add | editmode test | `kinetutor-guide/test/editmode-test-add/` |
| 6 | pre-commit-validate | pre-commit, validate | `kinetutor-guide/ops/pre-commit-validate/` |
| 7 | sprint-docs-sync | docs sync | `meta/sprint-docs-sync/` |
| 8 | asmdef-setup | asmdef, assembly definition | `kinetutor-guide/ops/asmdef-setup/` |
| 9 | scene-scaffold | Main.unity, scene scaffold | `kinetutor-guide/ui/scene-scaffold/` |
| 10 | unity-official-docs | Unity 공식문서 근거 | `kinetutor-guide/ops/unity-official-docs/` |
| 11 | student-friendly-ux | UX, onboarding, glossary, gate | `kinetutor-guide/ui/student-friendly-ux/` |
| 12 | debug-success-capture | debug, regression, playmode verification | `kinetutor-guide/ops/debug-success-capture/` |
| 13 | robotics-reference-to-lesson | 공개 robotics reference, concept map, lesson adaptation | `kinetutor-guide/content/robotics-reference-to-lesson/` |

## Skill 의존 규칙
- `robot-template-add` -> `dh-algorithm-add` + `editmode-test-add`
- `tutor-step-add` -> `robot-template-add`
- `student-friendly-ux` -> `tutor-step-add` + `scene-scaffold`
- `robotics-reference-to-lesson` -> `student-friendly-ux` + `tutor-step-add`
- `asmdef-setup` -> `unity-official-docs`
- `pre-commit-validate` -> `editmode-test-add` + `unity-official-docs`
- `debug-success-capture` -> `pre-commit-validate` + `student-friendly-ux`

## Source of Truth 문서
- 탐색 인덱스: `AGENTS.md`
- 제품 문서 보드: `docs/status/PRODUCT-DOC-BOARD.md`
- 제품 요구사항: `docs/ref/PRD.md`
- 제품 와이어프레임: `docs/ref/WIREFRAME.md`
- 제품 로드맵: `docs/ref/PRODUCT-ROADMAP.md`
- 제품 상세 문서 루트: `docs/ref/product/`
- 현재 기능 상태 체크리스트: `docs/ref/product/roadmap/current-feature-checklist.md`
- 초보자 lesson framework: `docs/ref/product/content/lesson-framework.md`
- 공개 로보틱스 레퍼런스 팩: `docs/ref/product/content/open-robotics-reference-pack.md`
- 경쟁제품 합성 문서: `docs/ref/product/foundation/competitive-synthesis.md`
- LLM teaching strategy: `docs/ref/product/content/llm-teaching-strategy.md`
- 모바일 릴리스 체크리스트: `docs/ref/product/roadmap/mobile-release-checklist.md`
- 에셋 수집 체크리스트: `docs/ref/product/roadmap/asset-sourcing-checklist.md`
- 에셋 큐레이션 맵: `docs/ref/asset-curation-map.md`
- 에셋 검증 리포트: `docs/ref/asset-validation-report.md`
- URDF 레퍼런스 수집: `docs/ref/product/robots/urdf-reference-collection.md`
- Workspace Envelope 알고리즘 메모: `docs/ref/product/roadmap/workspace-envelope-algorithm-memo.md`
- Interactive Matrix Viz 디자인 레퍼런스: `docs/ref/product/ux/interactive-matrix-viz-design-reference.md`
- Phase 5 구현 계획: `docs/ref/phase5-implementation-plan.md`
- 실행 계획: `KineTutor3D_Execution_Plan.md`
- 운영 상태: `docs/status/PROJECT-STATUS.md`
- 실행 보드: `docs/status/PHASE-EXECUTION-BOARD.md`
- 스킬 매트릭스: `docs/status/SKILL-DOC-MATRIX.md`
- 아키텍처: `docs/ref/architecture-diagrams.md`
- 빠른 아키텍처 맥락: `docs/ref/architecture-mermaid.md`
- 사용자 흐름: `docs/ref/USER-FLOW.md`
- 튜터 스텝: `docs/ref/tutor-step-plan.md`

## 테스트 표준
- Local 우선 순서:
1. EditMode 전체
2. PlayMode 스모크
- 현재 기준:
1. EditMode: 142 passed (코드 어트리뷰트 기준)
2. PlayMode: 44 passed (코드 어트리뷰트 기준)
- CI 워크플로우:
1. `.github/workflows/unity-tests.yml`
2. runner: `self-hosted`, `windows`
3. `UNITY_EXE` 환경변수 필요

## 즉시 다음 작업
1. Phase 5G Tests + Docs 최종 정리
2. Phase 6 CI/CD 계속: PR에서 `unity-tests` 워크플로우 실주행 1회 확인
3. Robot Library에서 데모퍼스트 로봇 → 실제 기구학 조작 연결 (SCARA/3DOF/6DOF 템플릿 추가 시)
4. `asset-validation-report` 기준으로 `SceneSelectables.prefab` 후속 수정 여부 판단
5. `Main.unity`를 prefab 단위 HUD/Robot rig 자산으로 더 분리할지 검토

## Task Routing
1. 제품 방향 변경: `docs/ref/PRD.md` + `docs/ref/product/foundation/*`
2. 현재 기능 상태/구현 범위 확인: `docs/ref/product/roadmap/current-feature-checklist.md`
3. Phase 5 구현/검수: `docs/ref/phase5-implementation-plan.md` -> `Assets/Scripts/App/AGENTS.md` -> `Assets/Scripts/UI/AGENTS.md` -> `Assets/Scripts/Visualization/AGENTS.md`
4. Beginner Lesson 0~3 / pre-kinematics 작업: `docs/ref/product/content/lesson-framework.md` -> `docs/ref/product/ux/guided-lesson.md` -> `docs/ref/tutor-step-plan.md` -> `docs/ref/USER-FLOW.md`
5. Guided Lesson 작업: `docs/ref/WIREFRAME.md` + `docs/ref/product/ux/guided-lesson.md`
6. Robot model 작업: `docs/ref/product/robots/robot-model-library-spec.md`
7. Sandbox 작업: `docs/ref/product/ux/sandbox.md`
8. Instructor 기능: `docs/ref/product/ux/instructor-mode.md`
9. Tablet/mobile 작업: `docs/ref/product/ux/tablet-first-policy.md`
10. 강의자료 활용 작업: `docs/ref/product/content/derived-course-content-policy.md` + `docs/ref/product/content/concept-to-ui-map.md`
11. 공개 robotics reference 반영: `docs/ref/product/content/open-robotics-reference-pack.md` + `.claude/skills/kinetutor-guide/content/robotics-reference-to-lesson/SKILL.md`
12. 경쟁제품 분석 반영: `docs/ref/product/foundation/competitive-synthesis.md` -> `docs/ref/product/foundation/product-positioning.md` / `docs/ref/product/roadmap/milestone-backlog.md`
13. LLM teaching 작업: `docs/ref/product/content/llm-teaching-strategy.md`
14. 모바일 배포 작업: `docs/ref/product/roadmap/mobile-release-checklist.md`
15. 에셋 작업: `docs/ref/product/roadmap/asset-sourcing-checklist.md` -> `docs/ref/asset-curation-map.md` -> `docs/ref/asset-validation-report.md` -> `docs/ref/asset-registry.md`
16. 플랜 변경 처리: `docs/ref/PRODUCT-ROADMAP.md` + `docs/ref/product/roadmap/release-gates.md`
