# KineTutor3D 프로젝트 인덱스

KineTutor3D 작업 시작 시 가장 먼저 읽는 진입 문서입니다.
이 문서만 읽어도 현재 단계, 규칙, 다음 행동을 빠르게 파악할 수 있게 유지합니다.

## 저장소 경계
- Write Repo: `C:\Users\ezen601\Desktop\Jason\robotapp2`

## 시작 순서 (필수)
1. `AGENTS.md` (Codex) 또는 `CLAUDE.md` (Claude) - 동일 정책 진입 문서
2. `KineTutor3D_Execution_Plan.md`
3. `docs/status/PROJECT-STATUS.md`
4. `docs/status/PHASE-EXECUTION-BOARD.md`
5. `docs/status/SKILL-DOC-MATRIX.md`

## 현재 상태 (2026-03-09)
- Phase 0: Done
- Phase 1: Done
- Phase 2: Done
- Phase 3 (Template 2DOF + App/UI): Done
- Phase 4 (Visualization core): Done
- Phase 6 (CI/CD): InProgress
- Stability Refactor (App/UI/Visualization componentization): Done

최근 확정 사항:
- Phase 3 확장 완료: `TemplateSelector`, `DHTableEditor`, `MatrixDisplay` 실동작 연결
- Scene split 완료: `Boot.unity` -> `Onboarding.unity` / `Main.unity` 분기 구조 도입
- Build Settings 재구성: `Boot`(0), `Onboarding`(1), `Main`(2)
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
- Unity Test Runner 결과: EditMode `47/47`, PlayMode `26/26`
- CI 초안 추가: `.github/workflows/unity-tests.yml`

## 실행 규칙 (MUST)
1. 기존 코드/타입/유틸 우선 재사용, 중복 구현 금지
2. `Assets/Scripts/` 폴더 구조를 모듈 Source of Truth로 사용
3. Math/Types/Kinematics는 pure C# `double` 유지, `UnityEngine` 참조 금지
4. NaN/Infinity 입력은 FK 계산 전에 차단
5. `theta`는 Slider 단일 소스, DHTable에서는 read-only
6. 문서와 코드 상태가 다르면 코드/테스트 실제 상태를 우선
7. 명시 요청 없이는 임의 Git 파괴 명령 금지

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

## Skill 의존 규칙
- `robot-template-add` -> `dh-algorithm-add` + `editmode-test-add`
- `tutor-step-add` -> `robot-template-add`
- `student-friendly-ux` -> `tutor-step-add` + `scene-scaffold`
- `asmdef-setup` -> `unity-official-docs`
- `pre-commit-validate` -> `editmode-test-add` + `unity-official-docs`
- `debug-success-capture` -> `pre-commit-validate` + `student-friendly-ux`

## Source of Truth 문서
- 탐색 인덱스: `AGENTS.md`
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
1. EditMode: 47 passed
2. PlayMode: 26 passed
- CI 워크플로우:
1. `.github/workflows/unity-tests.yml`
2. runner: `self-hosted`, `windows`
3. `UNITY_EXE` 환경변수 필요

## 즉시 다음 작업
1. Phase 6 CI/CD 계속: PR에서 `unity-tests` 워크플로우 실주행 1회 확인
2. `Main.unity`를 prefab 단위 HUD/Robot rig 자산으로 더 분리할지 검토
3. `Assembly-CSharp.csproj` 로컬 빌드 불일치 원인 문서화
