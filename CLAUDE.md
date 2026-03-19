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

## 현재 상태 (2026-03-15)
- Phase 0: Done
- Phase 1: Done
- Phase 2: Done
- Phase 3 (Template 2DOF + App/UI): Done
- Phase 4 (Visualization core): Done
- Phase 5: Done (5A~5G Complete)
- Phase 6 (CI/CD): Hold (로컬 테스트 전용, runner 미등록)
- Stability Refactor (App/UI/Visualization componentization): Done
- Product Docs Governance (GameLab-style): InProgress
- Current Cycle: InProgress (FR5 RobotControl Console QA + Page QA Hardening + Sandbox polish)

최근 확정 사항:
- RobotControl Phase 8 구현 완료 (2026-03-15): 프리셋 애니메이션 전환(PresetTransitionAnimator, 1.5초 EaseInOutCubic), Speed Selector UI(Slow 10%/Medium 30%/Fast 60% 3단 버튼, JointControlPanel+TcpControlPanel), 연결 끊김 안전 처리(3회 연속 에러→OnConnectionLost+패널 비활성화+빨간 재연결 안내), 연결 초기화 보간(0.8초), 자기리뷰 2건 수정(ShowConnectionLost 컴파일 에러, duration<=0 가드). EditMode 345/351 passed (6 failed=기존)
- Ready 프리셋 기본 포즈 조정 (2026-03-15): EE 하향 자세 `{0, -45, 0, -59, -92, -42}` 확정. Mock 초기값 + Coordinator 시작 포즈 동기화. Live 연결 시 0.8초 보간으로 실제 로봇 포즈 자동 전환되므로 영향 없음
- RobotControl P0~P5 전체 구현 완료: 카메라 프로파일 고정, 관절 회전 핸들(JointRotationHandle ×6), TCP 직교 제어(FairinoTcpControlPanel + MoveL), 프리셋→Sync 동기화(SyncCurrentState + Current 동적 프리셋), EE 변위 화살표(DisplacementArrow), TopBar 기즈모 토글/트레일 Clear, EE XYZ RGB 색상 코딩, WhyItMoved 다관절 요약+XYZ 성분
- 공용 컴포넌트 추출: SharedLineMaterial(Material 캐시 3개 통합), FairinoRobotConfig.GetMediumSpeedAcc+GetSpeedAcc(speed 해소 중복 제거), FR5PosePresets.All 캐시
- 자기리뷰 버그 6건 수정: OnHandleDragged 슬라이더 동기화, 핸들 히트 판정(Ray-plane 교차), EndDrag 선택 해제, 공유 Material 색상 오염, TCP 패널 재진입 가드, 핸들 이벤트 중복 바인딩
- FR5 RobotControl 슬라이더→3D 파이프라인 완성: Transform 기반 관절 제어 전환 + Slider GraphicRaycaster 감지 수정(`UIComponentFactory.CreateSlider`에 자체 Image 추가) + EventSystem `AssignDefaultActions` + overlay `raycastTarget=false`. 슬라이더 드래그→3D 회전 확인 완료
- EE 트레일 공유 코어 통합: `EETrailRenderer`를 공유 컴포넌트로 리팩터링(거리게이팅+FIFO+파랑→금색 그라데이션). `EndEffectorTrail`은 AppController 이벤트 바인딩 어댑터로 전환. 새 로봇 추가 시 `EETrailRenderer`만 사용하면 궤적 표시 가능
- FR5 RobotControl Transform 전환 완료: `FairinoUrdfJointDriver`를 ArticulationBody xDrive→Transform.localRotation 직접 제어로 전환. non-root AB `enabled=false`로 물리 덮어쓰기 차단. `TeleportRootUpright`를 프리팹 포즈 동기화(`TeleportRoot(현재pos, 현재rot)`)로 교체하여 베이스 -Y 방향(바닥) 유지. `Always Start From Onboarding` 재활성화로 Onboarding→Home→RobotLibrary→RobotControl 전체 흐름 복구
- FR5 RobotControl baseline 추가: `RobotControl.unity` 생성, `SceneId.RobotControl=6`, Build Settings index 6 등록, `RobotControlSceneCoordinator` + `FairinoConnectionPanel` + `FairinoJointControlPanel` + `FairinoStatePanel` + `FairinoConnectionService` 기반 제어 콘솔 경로 정리
- FR5 로봇 사용 기준 분리: showroom은 `Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab` donor preview 사용, RobotControl은 `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab` control prefab 사용. `QaToolsMenu`의 FR5 import는 preview/control prefab을 각각 저장
- FR5 donor preview 튜닝 반영: `RobotPreviewFactory`의 FR5 전용 donor preview pose 보정으로 showroom에서 자연스럽게 서도록 정리, 기준 스크린샷 `robotlibrary-playmode-fr5-standing-tuned.png`
- 카메라 중앙 관리 도입: `SceneCameraDirector` 추가로 `Main / Sandbox / RobotControl / Onboarding / Home` 메인 카메라 구도를 한 파일에서 관리. `RobotLibrary` showroom 카메라는 `RobotLibraryManager`가 별도 관리
- RobotLibrary FR5 CTA 확장: FR5 카드/detail drawer에서 `Robot Control` CTA를 제공하도록 연결
- MathReadiness UX 고도화 완료 (Phase A+B+C): 정답/오답 색상+아이콘(AccentSuccess/AccentDanger), 진행 뱃지("Q1/2"), 워밍업/본문제 시각 분리(섹션 라벨+디바이더), 코치 힌트 leading icon, 피드백 fade-in(0.25s), 카드 전환 slide(0.3s), 적응형 힌트(2회 오답→코치힌트, 3회→정답 하이라이트), 컨셉별 테마 색상(Orange/Blue/Purple/Green accent stripe), WhyItMoved 방향 화살표 아이콘, 마스터리 분기 플래그
- 모드별 패널 격리 리팩터링 완료: `AppController.ApplyFeatureState()`를 `HideAllContentPanels()` → `Apply{Mode}Visibility()` 패턴으로 리팩터링. MathReadiness 모드에서 StepTutorPanel/DHTableEditor/MatrixDisplay/TemplateSelector 완전 숨김. `MatrixDisplay.SetVisible(bool)`, `TemplateSelector.SetVisible(bool)` 신규 추가. `scene-ui-visibility` 스킬에 모드별 가시성 매트릭스 추가
- QA 흐름 정비 완료: Editor Play Mode 시작 씬을 `Boot.unity`로 고정하는 `BootScenePlayModeSetup` 추가 (`KineTutor3D > Always Start From Boot` 토글). `QaToolsMenu`로 First-Time/Returning User PlayerPrefs 리셋 메뉴 추가. 어떤 씬이 열려있든 `Boot → Onboarding → Home → Main/Sandbox` 전체 흐름 QA 가능
- Sandbox 패널 겹침 수정: `SandboxActionPanel`/`SnapshotLitePanel`에 `SetVisible(bool)` API 추가. `SandboxSceneCoordinator`가 학습 패널 GameObject를 `SetActive(false)`로 완전 숨기고 Sandbox 패널을 명시적으로 활성화. `AppController`가 학습 모드에서 Sandbox 패널을 숨기도록 배타 제어 추가. `AppUiBinder`에 Sandbox 패널 AutoWire 추가
- 씬 UI 가시성 정비 완료: 배타 그룹 패널(`BeginnerLeftPanel`, `MathReadinessPanel`, `TargetFeedbackPanel`, `WhyItMovedPanel`)의 `Awake()` → `OnEnable()+SetVisible(false)` 전환으로 1프레임 깜빡임 제거. 런타임 전용 5개 컴포넌트에서 `[ExecuteAlways]` 제거. `StepTutorPanel`/`DHTableEditor`에 `SetVisible(bool)` API 추가
- Home / Continue Hub 실구현: `Boot -> Home`, `Onboarding -> Home`, `HomeContinueHubController`/`HomeContinueHubFlowService`, `SessionContextStore`, `AppSessionContextService`로 재진입 흐름 고정
- Math Readiness 도입: `math_readiness` track, `MathReadinessLessonFactory`, `MathReadinessPanel`, `MathReadinessFormatter`, 관련 EditMode/PlayMode smoke 추가
- UI Design System 2차 적용: `HomeContinueHubViewBuilder`, `MathReadinessPanel`, `SnapshotLitePanelViewBuilder`, `SandboxActionPanelViewBuilder`를 `UIComponentFactory`/`UILayoutProfile`/`UIIconResolver` 기반으로 리팩터링
- Sandbox 상태: 패널 겹침 해결 완료. 버튼/아이콘 가독성 후속 정리 중
- UI Design System 도입: `UIDesignTokens`(색상 25+, 타이포 7단계, 간격 7단계, 컴포넌트 치수), `UITypography`(TMP 프리셋), `UIIconResolver`, `UIComponentFactory`, `UILayoutProfile` 추가. `UiRuntimeStyle`을 Obsolete bridge로 전환. Heathen 아이콘 25개 `Resources/UI/Icons/`로 큐레이션. `GameObject.Find` 10개 → `Transform.Find`/`FindFirstObjectByType`로 교체. 하드코딩 색상 8+ → 토큰 참조로 교체. `Unity.TextMeshPro` asmdef 참조 추가
- 실제 에셋 기준선 정리: `HQP Studios`, `_Heathen Engineering`, `Glowing Rifts` 복구 후 curated runtime subset(`Assets/Runtime/Art/UI/Icons`, `Assets/Runtime/Prefabs/Teaching/Markers`, `Assets/Runtime/Prefabs/Teaching/RobotLibrary`) 도입
- SCARA 활성화: `TemplateSCARA_RV`, expanded `RobotMetadataInfo`, 4DOF-aware `JointInputRail`, `Sandbox.unity`, Robot Library lesson/sandbox routing 추가
- Phase 5G 완료: Tests + Docs 최종 정리 — 전체 문서 동기화 (PROJECT-STATUS, PHASE-EXECUTION-BOARD, PRODUCT-DOC-BOARD, INTEGRITY-REPORT, master-plan, project-context, current-feature-checklist, architecture-mermaid), 스킬 라우팅 검증 (13/13 스킬, 112/114 문서 도달), EditMode 107/107 PlayMode 30/30
- Phase 5F 완료: Robot Library MVP — RobotMetadataInfo/RobotCatalogEntry(Types), RobotCatalog(Templates, 5개 로봇 등록), RobotSelectionBridge(App), RobotLibrary.unity 씬, RobotLibraryManager/RobotCardBuilder/RobotDetailDrawer(UI), SceneNavigationBar 버튼 재바인딩 안정화, EditMode 107/107 PlayMode 31/31
- Phase 5E 완료: BeginnerLessonFactory(L0~L3), BeginnerLeftPanel, CompareModePanelHelper, TargetFeedbackPanel 추가, OnboardingManager 초보자 버튼 추가, EditMode 87/87 PlayMode 31/31
- Phase 5D 완료: WhyItMovedState/Formatter/Panel 추가, AppController+AppUiBinder 연동
- GameLab-style 제품 문서 운영 이식 시작: canonical product docs 3종(`PRD`, `WIREFRAME`, `PRODUCT-ROADMAP`)과 `PRODUCT-DOC-BOARD`를 status/ref 계층에 추가
- Beginner Lesson 0~3를 `Pre-Kinematics` 진입 트랙으로 추가하고 `Core Track Step 1~8`과 분리
- `current-feature-checklist`를 기준으로 현재 구현 범위와 우선 추가 기능을 한 문서에서 추적
- 경쟁제품 synthesis, LLM teaching strategy, mobile release checklist를 제품 문서 체계에 통합
- 내부 패키지 자산을 `Assets/KineTutor_AssetCuration_BACKUP/`로 큐레이션하고 hierarchy validation report를 추가
- Phase 3 확장 완료: `TemplateSelector`, `DHTableEditor`, `MatrixDisplay` 실동작 연결
- Scene split 완료: `Boot.unity` -> `Onboarding.unity` / `Main.unity` 분기 구조 도입
- Build Settings 재구성: `Boot`(0), `Onboarding`(1), `Home`(2), `Main`(3), `RobotLibrary`(4), `Sandbox`(5), `RobotControl`(6), `MathReadiness`(7)
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
- Unity Test Runner 결과: EditMode `282`, PlayMode `50` (코드 어트리뷰트 기준, 2026-03-13 grep 재집계)
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
9. **Unity Editor 상태 조회/검증 시 `unity-cli` 도구를 MCP보다 우선 사용한다.** CLI 도구가 존재하는 작업은 MCP 없이 `Bash`로 `unity-cli <tool>` 호출. CLI로 불가능한 작업만 MCP 폴백. 전체 도구 목록: `.claude/known-cli-tools.txt`
10. **PowerShell에서 `unity-cli`를 호출할 때는 `$PSNativeCommandArgumentPassing = "Standard"`를 우선 설정한다.** 커스텀 툴은 `compile_check_tool` 같은 등록명과 `--params '{"key":"value"}'` 형식을 우선 사용한다.

## CLI 도구 라우팅 (자연어 → unity-cli)
| 키워드 | CLI 명령어 | 설명 |
|--------|-----------|------|
| 컴파일, 빌드 에러 | `unity-cli compile_check_tool` | 컴파일 에러/경고 카운트 |
| 콘솔, 로그, 에러 로그 | `unity-cli console_check_tool --params '{"type":"error"}'` | 콘솔 로그 조회 |
| 테스트, 테스트 실행 | `unity-cli run_tests_tool --params '{"mode":"edit"}'` | EditMode/PlayMode 테스트 |
| 씬 검증, missing script | `unity-cli scene_validate_tool --params '{"name":"all"}'` | 씬 무결성 검사 |
| 프리팹 검증 | `unity-cli prefab_validate_tool --params '{"path":"Assets/..."}'` | 프리팹 무결성 검사 |
| 씬 구조, 오브젝트 목록 | `unity-cli scene_hierarchy_tool --params '{"depth":2}'` | 씬 GameObject 계층 |
| 로봇 목록, 카탈로그 | `unity-cli robot_catalog_tool` | 등록된 로봇 목록 |
| FK, 순기구학 | `unity-cli fk_compute_tool --params '{"template":"2DOF_RR","joints":"45,30"}'` | FK 계산 |
| DH 파라미터, DH 테이블 | `unity-cli dh_table_tool --params '{"template":"2DOF_RR"}'` | DH 파라미터 조회 |
| 관절 한계, 제한 | `unity-cli joint_limit_tool --params '{"template":"FR5"}'` | 관절 제한 범위 |
| 카메라, 카메라 위치, 카메라 설정 | `unity-cli camera_capture_tool --params '{"action":"current"}'` | 카메라 캡처/저장/적용 |
| 포즈 비교, EE 거리 | `unity-cli pose_compare_tool --params '{"template":"FR5","joints_a":"...","joints_b":"..."}'` | 두 포즈 EE 거리 |
| 에셋 크기, 리소스 크기 | `unity-cli asset_size_tool --params '{"top":10}'` | Resources 크기 분석 |
| 씬 비교, 씬 차이 | `unity-cli scene_diff_tool --params '{"scene_a":"A","scene_b":"B"}'` | 씬 간 비교 |
| QA, 검수 준비 | `unity-cli qa_prep_tool --params '{"scenario":"first-time"}'` | QA 사전 점검 |
| 빌드 세팅 | `unity-cli build_settings_tool` | Build Settings 검증 |
| PlayerPrefs, 설정값 | `unity-cli player_prefs_inspect_tool` | PlayerPrefs 조회 |
| 리소스 검증 | `unity-cli resource_validate_tool` | Resources 폴더 검증 |
| 세션, 진행 상태 | `unity-cli session_context_tool` | 세션 상태 조회 |
| 튜터 스텝 | `unity-cli tutor_step_validate_tool` | 튜터 스텝 에셋 검증 |
| 용어집, 글로서리 | `unity-cli glossary_validate_tool` | 용어집 검증 |
| FR5, 로봇 연결, 진단 | `unity-cli fr5_diagnostic_tool` | FR5 연결/상태 진단 |
| Canvas, UI 검증 | `unity-cli canvas_validate_tool` | Canvas 설정 검증 |
| asmdef, 어셈블리 | `unity-cli asmdef_validate_tool` | asmdef 참조 검증 |
| LearningTabs, 탭 | `unity-cli learning_tabs_tool --params '{"robot_id":"all"}'` | LearningTabs JSON 검증 |

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
| 14 | ui-design-system | color, token, typography, spacing, component | `kinetutor-guide/ui/ui-design-system/` |
| 15 | scene-ui-visibility | panel visibility, UI 겹침, 씬 UI, 초기 상태, ExecuteAlways | `kinetutor-guide/ui/scene-ui-visibility/` |
| 16 | fairino-fr5-integration | Fairino, FR5, 실제 로봇, C# SDK, Unity 제어, 상태 피드백 | `kinetutor-guide/content/fairino-fr5-integration/` |
| 17 | robot-showroom-debug | robot showroom, showroomoutput, comparestrip, preview pod, Game/Scene size mismatch | `kinetutor-guide/ui/robot-showroom-debug/` |
| 18 | main-learning-tabs-json | Main 탭 JSON, LearningTabs, robot-specific tab content, JsonUtility fallback, MainLearningTabsLoader | `kinetutor-guide/ui/main-learning-tabs-json/` |
| 19 | viewbuilder-extract | ViewBuilder, UI 분리, Refs struct, 패널 추출 | `kinetutor-guide/ui/viewbuilder-extract/` |
| 20 | waypoint-teaching | waypoint, teaching, playback, sequence, loop, export | `kinetutor-guide/content/waypoint-teaching/` |

## Skill 의존 규칙
- `robot-template-add` -> `dh-algorithm-add` + `editmode-test-add`
- `tutor-step-add` -> `robot-template-add` + `ui-design-system`
- `student-friendly-ux` -> `tutor-step-add` + `scene-scaffold` + `ui-design-system`
- `scene-scaffold` -> `ui-design-system` + `scene-ui-visibility`
- `scene-ui-visibility` -> `ui-design-system`
- `robot-showroom-debug` -> `scene-scaffold` + `scene-ui-visibility` + `ui-design-system`
- `main-learning-tabs-json` -> `ui-design-system` + `scene-ui-visibility`
- `fairino-fr5-integration` -> `robotics-reference-to-lesson` + `robot-template-add`
- `waypoint-teaching` -> `fairino-fr5-integration` + `ui-design-system`
- `robotics-reference-to-lesson` -> `student-friendly-ux` + `tutor-step-add`
- `asmdef-setup` -> `unity-official-docs`
- `pre-commit-validate` -> `editmode-test-add` + `unity-official-docs`
- `debug-success-capture` -> `pre-commit-validate` + `student-friendly-ux`
- `viewbuilder-extract` -> `ui-design-system` + `scene-ui-visibility`

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
- FAIRINO FR5 실기 연동 레퍼런스: `docs/ref/product/robots/fairino-fr5-integration-reference.md`
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
1. EditMode: `dotnet build` + `dotnet test` green
2. PlayMode: Home/Sandbox in-editor visual smoke 확인, Unity batch runner는 점유 충돌로 미확정
- CI 워크플로우:
1. `.github/workflows/unity-tests.yml`
2. runner: `self-hosted`, `windows`
3. `UNITY_EXE` 환경변수 필요

## 즉시 다음 작업
1. Sandbox UI polish 마감: overlap 제거 + 버튼/아이콘 가독성 정리
2. tablet 4DOF rail 사용성 기준 정리
3. `asset subset Git tracking` 마무리
4. replay / constraint preview 설계 진입
5. Phase 6 CI/CD: self-hosted runner 등록 후 PR에서 `unity-tests` 워크플로우 실주행 1회 확인

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
12. FAIRINO FR5 실기 연동 작업: `docs/ref/product/robots/fairino-fr5-integration-reference.md` + `.claude/skills/kinetutor-guide/content/fairino-fr5-integration/SKILL.md`
13. 경쟁제품 분석 반영: `docs/ref/product/foundation/competitive-synthesis.md` -> `docs/ref/product/foundation/product-positioning.md` / `docs/ref/product/roadmap/milestone-backlog.md`
14. LLM teaching 작업: `docs/ref/product/content/llm-teaching-strategy.md`
15. 모바일 배포 작업: `docs/ref/product/roadmap/mobile-release-checklist.md`
16. 에셋 작업: `docs/ref/product/roadmap/asset-sourcing-checklist.md` -> `docs/ref/asset-curation-map.md` -> `docs/ref/asset-validation-report.md` -> `docs/ref/asset-registry.md`
17. 플랜 변경 처리: `docs/ref/PRODUCT-ROADMAP.md` + `docs/ref/product/roadmap/release-gates.md`
