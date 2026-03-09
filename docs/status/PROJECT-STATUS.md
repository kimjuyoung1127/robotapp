# KineTutor3D 프로젝트 상태

최종 업데이트: 2026-03-09 (KST)
기준 문서: `CLAUDE.md`, `KineTutor3D_Execution_Plan.md`

## 현재 Phase
- **Phase 0: Foundation** (완료)
- **Phase 1: Types + Math (TDD)** (완료)
- **Phase 2: Kinematics Core (DH + FK)** (완료)
- **Phase 3: Template 2DOF + App/UI 연결 (MVP)** (완료)
- **Phase 4: Visualization (FrameGizmo + RobotRenderer Core)** (완료)
- **Phase 6: CI/CD (Unity tests workflow)** (진행 중)
- 병행 작업: **Phase 3 Student-Friendly UX 런타임 연결/데이터 실체화** 완료

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
1. Phase 6 CI/CD 계속: PR 기준으로 `unity-tests` 워크플로우 1회 실주행 확인(러너 라벨/`UNITY_EXE`/env 점검)
2. `Main.unity`를 prefab/HUD asset 기준으로 더 쪼갤지 검토해 씬 YAML 유지보수 비용을 낮추기
3. `Assembly-CSharp.csproj` 로컬 빌드 불일치 원인(생성 csproj 동기화 이슈) 문서화
