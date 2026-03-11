# KineTutor3D 프로젝트 상태

최종 업데이트: 2026-03-11 (KST)
기준 문서: `CLAUDE.md`, `KineTutor3D_Execution_Plan.md`

## 현재 Phase
- **Phase 0: Foundation** (완료)
- **Phase 1: Types + Math (TDD)** (완료)
- **Phase 2: Kinematics Core (DH + FK)** (완료)
- **Phase 3: Template 2DOF + App/UI 연결 (MVP)** (완료)
- **Phase 4: Visualization (FrameGizmo + RobotRenderer Core)** (완료)
- **Phase 5: Guided Lesson P0 구현 계획** (Planning Complete)
- **Phase 6: CI/CD (Unity tests workflow)** (진행 중)
- 병행 작업: **Phase 3 Student-Friendly UX 런타임 연결/데이터 실체화** 완료
- 병행 작업: **GameLab-style Product Docs Governance** 진행 중

## Phase 5 실행 원칙
1. Phase 5 P0는 기능 추가보다 `기반층 선행`이 우선이다.
2. 구현 순서는 `Runtime foundation -> Track-aware step foundation -> 공통 input/visualization -> Why It Moved -> Beginner Lesson 0~3`로 고정한다.
3. `Beginner Lesson 0~3`는 P0 범위지만 첫 구현 대상이 아니라 foundation 이후 consumer layer로 본다.
4. 문서 선행 sync 1회 후, 각 phase는 `구현 -> 테스트 -> self-review -> 문서 반영 -> git commit` 단위로 종료한다.

## 이번 턴 반영 내용 (Phase 5A Runtime foundation)
1. runtime state에 previous joint values, previous EE pose/position/transform, changed joint index, update cause를 추가했다.
2. `KinematicsRuntimeService`가 mutation 직전에 snapshot을 저장하고, `RecomputeForwardKinematics()`는 순수 재계산만 담당하게 유지했다.
3. `AppController` public facade에 previous/current runtime foundation 접근자를 추가했다.
4. EditMode 테스트를 `50/50` 통과해 기존 수학/FK 회귀와 새 foundation 계약이 함께 유지됨을 확인했다.

## Product Docs Governance
1. Canonical product docs 3종을 `docs/ref/PRD.md`, `docs/ref/WIREFRAME.md`, `docs/ref/PRODUCT-ROADMAP.md`로 고정했다.
2. 제품 상세 문서는 `docs/ref/product/` 아래로 가지치고, 루트 3문서는 요약/잠금 결정만 유지한다.
3. 제품 문서 상태는 `docs/status/PRODUCT-DOC-BOARD.md`에서만 추적한다.
4. 제품 문서 변경 시 downstream sync는 아래 규칙을 따른다.
   - `PRD.md` -> `PROJECT-STATUS.md`, `ai-context/project-context.md`, `ai-context/master-plan.md`
   - `WIREFRAME.md` -> `docs/ref/USER-FLOW.md`, `docs/ref/tutor-step-plan.md`, 필요 시 `docs/ref/architecture-diagrams.md`
   - `PRODUCT-ROADMAP.md` -> `PROJECT-STATUS.md`, `docs/status/PHASE-EXECUTION-BOARD.md`, `ai-context/master-plan.md`
5. 제품 문서 변경은 반드시 `docs/daily/MM-DD/` 로그를 남기고, 마일스톤 단위 변경이면 주간 롤업까지 반영한다.

## 이번 턴 반영 내용 (LLM / Mobile / UX-Concept 확장)
1. UX leaf 문서 확장
   - `guided-lesson.md`를 화면 단위 계약(`GL-01`~`GL-06`)으로 확장
   - `robot-library.md`를 grid/detail drawer/compare strip/mode routing 흐름으로 확장
   - `sandbox.md`를 numeric input, why-it-moved, snapshot/sequence, constraint preview, pick foundation까지 확장
2. concept 문서 확장
   - `concept-to-ui-map.md`를 15개 이상 concept와 `reference_family`, `prerequisite_concepts`, `visualization_mode` 기준으로 확장
   - 공개 자료 기준 문서 `open-robotics-reference-pack.md` 추가
3. LLM 후속 도입 문서화
   - `llm-teaching-strategy.md` 추가
   - deterministic runtime / teaching context / LLM response layer 분리 원칙 고정
4. 모바일 배포 문서화
   - `mobile-release-checklist.md` 추가
   - Android 태블릿 우선, iPad 후속, Play/App Store 준비 항목 고정
5. skill 확장
   - 기존 skill 6종에 `robot.md` 기반 규칙을 흡수
   - 새 skill `robotics-reference-to-lesson` 추가
   - `AGENTS.md`, `CLAUDE.md`, `SKILL-DOC-MATRIX.md`에 routing/매트릭스 반영
6. 현재 제품 상태 요약 문서화
   - `current-feature-checklist.md` 추가
   - 현재 있는 기능 / 없는 기능 / 우선 추가할 기능을 roadmap leaf 문서로 고정

## 이번 턴 반영 내용 (초보자 Lesson 0~3)
1. 초보자 진입 계층 추가
   - `lesson-framework.md`에 `Pre-Kinematics Lesson 0~3` / `Core Kinematics Step 1~8` 구조를 추가
   - `guided-lesson.md`에 `Beginner Mode`와 공식/행렬 최소화 규칙을 반영
2. 흐름 문서 동기화
   - `tutor-step-plan.md`를 `L0~L3 -> S1~S8` 구조로 확장
   - `USER-FLOW.md`에 `완전 초보` / `기본 개념 이해자` onboarding 분기를 추가
3. 개념/백로그 반영
   - `concept-to-ui-map.md`에 회전 원호, 끝점 경로, reach/not reach, inverse thinking 개념을 추가
   - `current-feature-checklist.md`, `milestone-backlog.md`에 `Beginner Lesson 0~3`를 P0로 반영
   - `competitive-synthesis.md`에 공식-first 진입 배제 원칙을 추가

## 이번 턴 반영 내용 (공개 레퍼런스 확장)
1. 공식 레퍼런스 적용 포인트 강화
   - `open-robotics-reference-pack.md`에 Modern Robotics, MIT Manipulation, Robotics Toolbox for Python, MoveIt 2, Unity Robotics Hub의 적용 포인트를 구체화
2. 새 content leaf 문서 추가
   - `frame-pose-teaching-notes.md`
   - `pick-foundation-state-machine.md`
3. 모델/실습 문서 동기화
   - `robot-model-library-spec.md`에 convention, joint limits, pose preset, import source 메타데이터를 추가
   - `sandbox.md`에 pick foundation 상태를 `pre_pick -> pick -> post_pick -> pre_place -> place -> post_place`로 정리

## 이번 턴 반영 내용 (에셋 수집 가이드)
1. `asset-sourcing-checklist.md`를 roadmap leaf 문서로 추가
2. 무료 소스 사이트, 검색어, intake checklist, folder placement 규칙을 고정
3. `asset-registry.md`에서 에셋 수집 기준 문서를 참조하도록 연결

## 이번 턴 반영 내용 (내부 에셋 큐레이션)
1. `Assets/KineTutor_AssetCuration_BACKUP/` 아래에 패키지별 선별본 폴더를 생성
2. `realvirtual`에서 로봇, props, UI 후보를 복사 정리
3. `HQP Studios`, `_Heathen Engineering`, `Glowing Rifts`에서 교육용 아이콘/타겟 후보를 복사 정리
4. `.meta` 파일은 의도적으로 복사하지 않고 `asset-curation-map.md`에 규칙과 분류 결과를 기록

## 이번 턴 반영 내용 (에셋 hierarchy 검증)
1. `Assets/KineTutor_AssetCuration_BACKUP/` 내 프리팹 `44`개를 `Main.unity` 임시 루트에 instantiate하는 스모크 테스트 수행
2. `43`개는 clean instantiate 확인
3. `SceneSelectables.prefab`은 instantiate 후 `NullReferenceException`이 발생해 `needs-fix`로 분류
4. 검증용 임시 루트는 테스트 후 삭제해 씬 오염 없이 정리
5. 상세 결과는 `docs/ref/asset-validation-report.md`에 기록

## 이번 턴 반영 내용 (Product Docs Governance 이식)
1. `docs/status/PRODUCT-DOC-BOARD.md` 추가
   - `prd`, `wireframe`, `product-roadmap` 3개 canonical 문서 상태를 전담 추적
2. canonical 제품 문서 3종 추가
   - `docs/ref/PRD.md`
   - `docs/ref/WIREFRAME.md`
   - `docs/ref/PRODUCT-ROADMAP.md`
3. branching 전략 반영
   - `docs/ref/product/` 아래에 foundation/ux/content/robots/roadmap leaf 문서를 추가
   - root canonical 문서는 잠금 결정 + 링크 + downstream sync만 유지하는 summary 문서로 재정리
4. 인덱스/컨텍스트 문서 동기화
   - 루트 `AGENTS.md`, `CLAUDE.md`, `docs/CLAUDE.md`, `ai-context/START-HERE.md`
   - `ai-context/master-plan.md`, `ai-context/project-context.md`
5. 운영 문서/자동화 연계 강화
   - `SKILL-DOC-MATRIX`, `INTEGRITY-REPORT`, `sprint-docs-sync`, `code-doc-align`, `docs-nightly-organizer`에 제품 문서 drift 규칙 연결

## Phase 0 체크리스트
- [x] Unity 프로젝트 생성
- [x] unity-mcp 패키지 설치
- [x] Git 초기화 및 Unity `.gitignore` 적용
- [x] 씬 baseline 확정 (`Assets/Scenes/Boot.unity`, `Assets/Scenes/Onboarding.unity`, `Assets/Scenes/Main.unity`)
- [x] Build Settings 순서 설정 (`Boot` 0, `Onboarding` 1, `Main` 2)
- [x] MCP 연결 스모크 확인 (telemetry/scene/console 응답)
- [x] Unity Console 컴파일 에러 0 최종 확인 (MCP 시스템 로그 제외 기준)
- [x] 공식문서 근거 검증 완료 (`docs.unity3d.com` 링크 첨부 규칙)

## 이번 턴 반영 내용 (안정성 우선 컴포넌트화 + AGENTS 계층)
1. Visualization 리팩터링
   - `RobotRenderer`를 facade로 축소
   - `RobotRigBinder`, `ScaraDonorMapper`, `DonorMeshCopier`, `RobotVisibilityProbe` 추가
   - donor path / canonical frame / visibility probe 계약 유지
2. App 리팩터링
   - `AppController`를 facade로 축소
   - `StepFlowService`, `KinematicsRuntimeService`, `KinematicsRuntimeState`, `AppUiBinder` 추가
   - step 흐름, FK 재계산, UI auto-wire 책임 분리
3. UI 경량 분리
   - `DHTableEditor`의 parse/build 책임을 helper로 분리
   - `DHTableValueFormatter`, `DHTableViewBuilder`, `DHTableRowRefs` 추가
4. 구조 문서화
   - 루트 `AGENTS.md`
   - `Assets/Scripts/App/AGENTS.md`
   - `Assets/Scripts/UI/AGENTS.md`
   - `Assets/Scripts/Visualization/AGENTS.md`
   - `docs/ref/architecture-mermaid.md`
5. 회귀 결과
   - `KineTutor3D.Runtime.csproj` 빌드 성공
   - EditMode 47/47, PlayMode 26/26 유지

## 이전 턴 반영 내용 (씬 분리 + 전역 네비게이션)
1. 씬 분리 완료
   - `Boot.unity`: 첫 방문 여부 판단 후 즉시 씬 전환
   - `Onboarding.unity`: 환영 패널과 시작/건너뛰기만 담당
   - `Main.unity`: 로봇/HUD/Visualization 전용 씬
2. 전역 씬 이동 추가
   - `Assets/Scripts/App/SceneId.cs`
   - `Assets/Scripts/App/SceneCatalog.cs`
   - `Assets/Scripts/App/SceneNavigator.cs`
   - `Assets/Scripts/App/BootSceneRouter.cs`
   - `Assets/Scripts/UI/SceneNavigationBar.cs`
3. `Main` 온보딩 의존 제거
   - `AppController`에서 `OnboardingManager.Initialize()` 경로 제거
   - `Canvas`의 `OnboardingManager`, `SpotlightOverlay` 제거
   - `TopBar`에 `SceneNavigationBar` 추가
4. 테스트 확장
   - `Assets/Tests/PlayMode/SceneFlowSmokeTests.cs` 추가
   - 결과: EditMode 47/47, PlayMode 26/26
5. HUD 아티팩트 후속 정리
   - `GlossaryPanel` 기본 활성 상태 제거 및 inactive-safe 검색으로 중앙 파란 박스 원인 경로 차단
   - `SceneNavigationBar`/공통 UI 스타일 경로 보강으로 상단 네비 버튼 red X/미표시 문제 수정
   - 현재 남은 정리 대상: `Main` 중앙 회색 상태 박스와 focus/highlight overlay 잔여물

## 이전 턴 반영 내용 (Phase 4 Visualization 마감 + URP 정상화)
1. Visualization 코어 3개 유지
   - `Assets/Scripts/Visualization/CoordConverter.cs`
   - `Assets/Scripts/Visualization/FrameGizmo.cs`
   - `Assets/Scripts/Visualization/RobotRenderer.cs`
2. `Main.unity` canonical frame 통합
   - 기존 `frame_0`, `frame_1`을 world/joint-1의 단일 source로 승격
   - `Frame_EE`는 EE 전용 표준 frame으로 유지
   - legacy duplicate frame(`WorldFrame`, `Frame_1`)은 비활성화
3. donor mesh 교체
   - `Assets/realvirtual/3DPrefabs/ScaraRobot.prefab`을 `ScaraDonorProbe` hidden donor source로 배치
   - vendor runtime 없이 `BaseVisual`, `Link0Visual`, `Link1Visual`, `EndEffectorVisualMesh`에 mesh-only 복제
   - 기존 primitive visual marker는 숨기고 FK 기반 anchor만 유지
4. 렌더 파이프라인/씬 정상화
   - `Packages/manifest.json`에 `com.unity.render-pipelines.universal@17.0.4` 추가
   - `ProjectSettings/GraphicsSettings.asset`, `ProjectSettings/QualitySettings.asset`를 `Assets/realvirtual/RenderPipelines/Resources/URP/URP-Default.asset`로 고정
   - URP 전환 과정에서 생성된 global settings / volume profile 자산을 프로젝트 기준값으로 반영
5. 학습 화면 MVP 정리
   - `TopBar` / `LeftPanel` / `RightPanel` / `BottomBar` 4영역 surface를 런타임 공통 스타일로 정리
   - 주요 UI 스크립트에 `ExecuteAlways`를 적용해 Scene View에서도 배치 구조가 보이도록 정리
   - `TemplateSelector`, `DHTableEditor`, `StepTutorPanel`, `MatrixDisplay`, `StepNavigator`가 씬 오브젝트 우선 배선 + 최소 fallback 생성 정책으로 동작
   - `TooltipSystem`, `ToastNotificationController`의 기본 시각을 디버그 텍스트에서 실제 패널 스타일로 교체
   - `Main Camera`를 Solid Color + 2DOF 학습 구도로 조정
   - Play 중 중앙 흰 사각형을 만들던 `WelcomeModal` placeholder와 viewport focus overlay를 기본 비활성으로 전환
   - 유효한 온보딩 모달이 없을 때는 placeholder를 띄우지 않고 Step 흐름으로 즉시 진입하도록 보정
6. 테스트 확장
   - EditMode: `CoordConverterTests` 추가
   - PlayMode: `VisualizationSmokeTests`에 Canvas HUD, explicit donor path, on-screen EE motion 검증 추가
   - 결과: EditMode 47/47, PlayMode 20/20

## 이전 턴 반영 내용 (Phase 0+1)
1. 공식문서 근거 문서 추가
   - `docs/ref/unity-official-evidence-phase01.md`
   - 주제: asmdef / test runner / serialization / script compilation / API compatibility
2. asmdef 3개 생성
   - `Assets/Scripts/Math/KineTutor3D.Math.asmdef`
   - `Assets/Scripts/Types/KineTutor3D.Types.asmdef`
   - `Assets/Tests/EditMode/KineTutor3D.Tests.EditMode.asmdef`
3. Phase 1 코드 구현
   - Types: `JointType`, `DHLink`, `RobotTemplate`, `Pose`
   - Math: `Vec3D`, `Mat3D`, `Mat4D`
4. EditMode 테스트 자산 추가
   - `TestTolerances`, `MatrixAssert`
   - `Vec3DTests`, `Mat3DTests`, `Mat4DTests`, `DHLinkTests`
5. 회귀 기준 유지
   - 초기 PlayMode 스모크 5건 기준을 구축했고, 현재는 `UxFlowSmokeTests` 11건 + `VisualizationSmokeTests` 4건으로 확장 완료

## Self-Review Gate (Cycle Result)
1. 기능 리뷰
   - `Main.unity` 기준 QA 범위를 고정하고 Phase 3 UI 흐름을 재검증함
   - `TemplateSelector` / `DHTableEditor` / `MatrixDisplay`가 현재 MVP 계약과 일치함
2. 코드 리뷰
   - UI 편집 경로가 기존 App/FK 파이프라인을 우회하지 않음을 유지함
   - `theta` 단일 소스 규칙(Slider only)과 입력 가드 정책이 유지됨
3. 테스트 리뷰
   - Unity Test Runner: EditMode 47/47 통과, PlayMode 20/20 통과
   - 씬 저장 확인: `Boot.unity`, `Onboarding.unity`, `Main.unity` 저장 완료
   - Unity Console 에러는 MCP 시스템 로그 외 프로젝트 코드 에러 0
   - `KineTutor3D.Runtime.csproj` 빌드는 경고만 있고 성공
   - `Assembly-CSharp.csproj`는 현재 QA 완료 기준이 아니며, 생성 csproj 불일치 이슈는 후속 추적으로 유지
4. 문서 리뷰
   - `CLAUDE.md` / `KineTutor3D_Execution_Plan` / `PROJECT-STATUS` / `PHASE-EXECUTION-BOARD` / `SKILL-DOC-MATRIX` 정합성 동기화
   - Phase 4 Visualization을 `Done`으로 유지하고 scene split/전역 네비게이션 정책을 문서에 고정함
5. 운영 스킬화
   - 기존 `debug-success-capture` 포맷으로 결과 기록 유지

## 다음 작업
1. Phase 5B Track-aware step foundation 구현: `pre_kinematics/core_kinematics` resume와 step 모델 확장
2. Phase 5C 공통 input/visualization 인프라 구현: numeric input, highlight, trail, target marker 기반 고정
3. Phase 5D `Why It Moved` explanation layer 구현
4. Phase 5E Beginner Lesson 0~3 연결 후 Phase 6 CI/CD 실주행 확인
5. `Assembly-CSharp.csproj` 로컬 빌드 불일치 원인(생성 csproj 동기화 이슈) 문서화
