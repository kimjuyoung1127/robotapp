# KineTutor3D Execution Plan (Current + UX Integration)

> **⚠ Historical Document (2026-03-09)**: 이 문서는 Phase 4 완료 시점 기준입니다. 최신 상태는 `docs/status/PROJECT-STATUS.md`와 `CLAUDE.md`를 참조하세요. 주요 변경: 씬 8개 체계(`Home`/`Sandbox`/`RobotControl`/`MathReadiness` 추가), SceneId 재번호, 테스트 282/50으로 확장.

- Project: KineTutor3D
- Updated: 2026-03-09 (KST)
- Unity Target: 6000.0.64f1 (Unity 6)

## 1) Current State Snapshot

1. 씬 분리 완료: `Assets/Scenes/Boot.unity`, `Assets/Scenes/Onboarding.unity`, `Assets/Scenes/Main.unity`.
2. MCP 연결 정상(telemetry/scene/console 조회 가능).
3. `Assets/realvirtual` 패키지 임포트 완료(소스 자산 보존 전략).
4. Student-Friendly UX 런타임/씬 배선/SO 데이터 실체화 완료.
5. `Assets/Tests/PlayMode/UxFlowSmokeTests.cs` 스모크 테스트 확장(현재 11건).
6. Phase 0 공식문서 근거 문서 추가 완료: `docs/ref/unity-official-evidence-phase01.md`.
7. Phase 1 Types/Math + EditMode 테스트 자산 구현 완료.
8. Phase 2 Kinematics(`DHStandard`, `ForwardKinematics`) 구현 및 수치 검증 완료.
9. Phase 3 확장: `TemplateSelector(2DOF 단일)`, `DHTableEditor(theta read-only, d/a/alpha editable)`, `MatrixDisplay(A1/A2/T02)` 실동작 연결 완료.
10. CI 초안 추가: `.github/workflows/unity-tests.yml` (`self-hosted windows`, EditMode/PlayMode 자동 실행 + 결과 artifact 업로드).
11. Phase 4 확장: `frame_0`/`frame_1`을 canonical frame object로 승격하고 `Frame_EE`를 표준 EE frame으로 유지.
12. Phase 4 확장: `Assets/realvirtual/3DPrefabs/ScaraRobot.prefab`을 hidden donor source로 배치하고, donor path를 `Base -> Axis1 -> Axis2 -> Axis3 -> Gripper`로 명시 고정한다. `Pick`은 helper point로만 유지한다.
13. 검증 결과: Unity Test Runner EditMode 47/47, PlayMode 26/26 통과 (2026-03-09 기준; 최신: EditMode 282, PlayMode 50), `Boot/Onboarding/Main` 씬 분기와 전역 네비게이션 포함 검증 완료.
14. 학습 화면 MVP 정리: `TopBar`/`LeftPanel`/`RightPanel`/`BottomBar` 4영역 surface 구성, donor mesh offset/scale 보정 경로 및 교육용 카메라 구도 반영.
15. Phase 4 디버그: Built-in -> URP(`com.unity.render-pipelines.universal@17.0.4`) 전환, `GraphicsSettings`/`QualitySettings`를 `URP-Default.asset`에 고정, `Main Camera`를 Solid Color로 전환.
16. Phase 4B HUD 디버그: 잘못된 `WelcomeModal` placeholder와 중앙 viewport focus 박스를 기본 비활성화하여 Play 중 중앙 흰 사각형 제거.
17. Scene flow 정리: `BootSceneRouter`가 첫 방문 시 `Onboarding`, 재방문 시 `Home`으로 `LoadSceneMode.Single` 전환. (※ `aaf1435` 이후 `Main` → `Home`으로 변경됨)
18. 전역 이동 바 추가: `SceneCatalog` 기반 `SceneNavigationBar`가 `Onboarding`과 `Main`을 상단 HUD에서 전환.
19. HUD 아티팩트 추가 정리: `GlossaryPanel` 기본 활성 상태를 제거하고 inactive-safe 자동 배선을 적용해 중앙 파란 박스 원인 경로를 차단.
20. HUD 가시성 보강: `SceneNavigationBar`/공통 UI 스타일 경로를 조정해 상단 네비 버튼 red X/미표시 문제를 수정.
21. 안정성 우선 리팩터링: `RobotRenderer`를 facade + `RobotRigBinder`/`ScaraDonorMapper`/`DonorMeshCopier`/`RobotVisibilityProbe`로 분리.
22. 안정성 우선 리팩터링: `AppController`를 facade + `StepFlowService`/`KinematicsRuntimeService`/`AppUiBinder`로 분리.
23. UI 경량 분리: `DHTableEditor`의 parse/build 책임을 `DHTableValueFormatter`/`DHTableViewBuilder`/`DHTableRowRefs`로 분리.
24. 구조 문서화: 루트/핵심 폴더 `AGENTS.md` 추가 및 `docs/ref/architecture-mermaid.md`를 빠른 전체 맥락 문서로 고정.

## 2) Locked Decisions

1. Scene baseline: `Boot.unity` 시작 씬, `Onboarding.unity`/`Main.unity` 분리.
2. Asset strategy: 벤더 소스(`Assets/realvirtual`) 보존 + 프로젝트 표준 경로로 재배치.
3. Test strategy: Unity Test Runner + CLI `-runTests` 병행.
4. UX strategy: `Hard gate + Skip`, `Reduced Motion` 지원, 한국어 우선.
5. Math/Types strategy: 순수 C# `double`, `UnityEngine` 참조 금지, NaN/Infinity 가드 필수.

## 3) Phase 0 Closure (Done)

1. Main 씬/빌드 인덱스 고정 완료.
2. 공식문서 근거(asmdef/test runner/serialization/script compilation/API compatibility) 완료.
3. 체크리스트 기준 `Phase 0 = Done`.

## 3.5) Phase 3 QA Closure (Done)

1. QA 대상 씬은 `Assets/Scenes/Boot.unity`, `Assets/Scenes/Onboarding.unity`, `Assets/Scenes/Main.unity` 3개 기준으로 재확인 완료.
2. `TemplateSelector`, `DHTableEditor`, `MatrixDisplay`의 현재 MVP 계약이 테스트/씬 상태와 일치함을 확인.
3. 오래된 검증 수치(`38/38`, `7/7`, `7건`, `5건 유지`) 제거 후 운영 문서와 상태 보드를 동기화함.

## 4) Phase 1 (Types + Math, TDD) (Done)

1. asmdef 3개(`Types`, `Math`, `Tests.EditMode`) 생성 완료.
2. 구현 완료:
   - Types: `JointType`, `DHLink`, `RobotTemplate`, `Pose`
   - Math: `Vec3D`, `Mat3D`, `Mat4D`
3. 테스트 자산 완료:
   - `TestTolerances`, `MatrixAssert`
   - `Vec3DTests`, `Mat3DTests`, `Mat4DTests`, `DHLinkTests`

## 5) Phase 3 UX Integration (기존 유지)

1. 상태 기반 UI 제어: `TutorStepConfig` 중심으로 Step 가시성/포커스/게이트 통합.
2. 런타임 11개 컴포넌트 운영.
3. 기존 연결점 유지:
   - `AppController` 이벤트 브로드캐스트
   - `StepNavigator` Next 잠금/Skip 처리
   - `StepTutorPanel` Step/게이트 상태 동기화

## 5.5) Phase 4 Visualization (Done)

1. `CoordConverter`가 robotics 좌표계를 Unity 좌표계로 변환하는 단일 경계를 담당한다.
2. `FrameGizmo`는 `LineRenderer` 기반 RGB 축을 canonical frame object(`frame_0`, `frame_1`, `Frame_EE`)에 직접 부착해 표시한다.
3. `RobotRenderer`는 생성기보다 binder/updater로 동작하며, 씬의 기존 `frame_0`/`frame_1`를 우선 바인딩하고 legacy duplicate frame(`WorldFrame`, `Frame_1`)은 비활성화한다.
4. 시각 자산은 `ScaraRobot.prefab`을 hidden donor source로 유지하고, `BaseVisual`, `Link0Visual`, `Link1Visual`, `EndEffectorVisualMesh`에 mesh-only 복제해 사용한다.
5. vendor script/drive/logic/runtime은 사용하지 않으며, 프로젝트 FK 결과만 시각 transform의 Source of Truth로 유지한다.
6. 1차 범위는 2DOF 전용이며 추가 축(`Axis3` 등)은 donor source에 남기더라도 런타임 제어 대상에서 제외한다.
7. UI는 씬 오브젝트 우선 배선 정책을 유지하고, `TopBar`/`LeftPanel`/`RightPanel`/`BottomBar`에 공통 panel surface를 적용해 학습 화면 MVP를 유지한다.
8. 렌더 파이프라인은 URP 기준으로 고정하고 donor mesh는 Built-in fallback이 아닌 URP material 경로를 사용한다.
9. `Main Camera`는 `RobotRoot`, `frame_0`, `frame_1`, `Frame_EE`가 동시에 보이는 Solid Color 교육용 구도를 기본값으로 유지한다.
10. 중앙 viewport를 덮는 큰 focus rectangle은 제품 HUD에서 사용하지 않으며, viewport focus는 문서/게이트 기준으로만 유지하고 시각 박스는 비활성화한다.
11. 온보딩은 `Onboarding.unity` 전용이며 `Main.unity`는 로봇 제어 HUD만 유지한다.
12. 상단 `SceneNavigationBar`는 `Onboarding`, `Main` 두 항목을 공통 제공하고 현재 씬 버튼은 비활성화한다.
13. `GlossaryPanel`과 focus/highlight overlay는 `Main`에서 기본 비활성 상태를 유지하고, 유효한 타깃 rect가 있을 때만 켠다.

## 6) Test Execution Standard

### A) Local
1. Test Runner에서 EditMode 우선 실행
2. UI/UX는 PlayMode 스모크로 온보딩/게이트/툴팁 경로 확인
3. PlayMode 스모크 기준: `Boot -> Onboarding/Main` 분기, 전역 씬 네비게이션, 온보딩 버튼 전환, Main HUD/게이트/툴팁/행렬/시각화 검증을 포함한 총 26건 유지

### B) CLI
```powershell
Unity.exe -batchmode -projectPath "C:\Users\ezen601\Desktop\Jason\robotapp2" -runTests -testPlatform EditMode -testResults "Logs\editmode-results.xml" -quit
```

```powershell
Unity.exe -batchmode -projectPath "C:\Users\ezen601\Desktop\Jason\robotapp2" -runTests -testPlatform PlayMode -testResults "Logs\playmode-results.xml" -quit
```

## 7) Next

1. Phase 6 CI/CD 계속: GitHub PR 1건 생성 후 `unity-tests` 워크플로우가 self-hosted 러너에서 실제 통과하는지 검증.
2. `Main.unity`를 HUD prefab / robot rig prefab 중심으로 더 분리할지 검토해 씬 YAML 유지보수 비용을 낮춘다.
3. `Assembly-CSharp.csproj` 로컬 빌드 실패(생성 csproj 동기화 이슈) 원인 정리 후 문서화.
