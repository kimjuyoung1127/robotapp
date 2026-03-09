# KineTutor3D Execution Plan (Current + UX Integration)

- Project: KineTutor3D
- Updated: 2026-03-09 (KST)
- Unity Target: 6000.0.64f1 (Unity 6)

## 1) Current State Snapshot

1. `Assets/Scenes/Main.unity` 존재, Build index 0 고정 완료.
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
12. Phase 4 확장: `Assets/realvirtual/3DPrefabs/ScaraRobot.prefab`을 hidden donor source로 배치하고, vendor runtime 없이 mesh-only donor visual로 재사용.
13. 검증 결과: Unity Test Runner EditMode 45/45, PlayMode 15/15 통과, `Main.unity` 활성/Build index 0/프로젝트 코드 에러 0 확인.
14. 학습 화면 MVP 정리: `TopBar`/`LeftPanel`/`RightPanel`/`BottomBar` 4영역 surface 구성, donor mesh offset/scale 보정 경로 및 교육용 카메라 구도 반영.

## 2) Locked Decisions

1. Scene baseline: `Main.unity` 단일 시작 씬.
2. Asset strategy: 벤더 소스(`Assets/realvirtual`) 보존 + 프로젝트 표준 경로로 재배치.
3. Test strategy: Unity Test Runner + CLI `-runTests` 병행.
4. UX strategy: `Hard gate + Skip`, `Reduced Motion` 지원, 한국어 우선.
5. Math/Types strategy: 순수 C# `double`, `UnityEngine` 참조 금지, NaN/Infinity 가드 필수.

## 3) Phase 0 Closure (Done)

1. Main 씬/빌드 인덱스 고정 완료.
2. 공식문서 근거(asmdef/test runner/serialization/script compilation/API compatibility) 완료.
3. 체크리스트 기준 `Phase 0 = Done`.

## 3.5) Phase 3 QA Closure (Done)

1. QA 대상 씬은 `Assets/Scenes/Main.unity` 단일 기준으로 재확인 완료.
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

## 5.5) Phase 4 Visualization (InProgress)

1. `CoordConverter`가 robotics 좌표계를 Unity 좌표계로 변환하는 단일 경계를 담당한다.
2. `FrameGizmo`는 `LineRenderer` 기반 RGB 축을 canonical frame object(`frame_0`, `frame_1`, `Frame_EE`)에 직접 부착해 표시한다.
3. `RobotRenderer`는 생성기보다 binder/updater로 동작하며, 씬의 기존 `frame_0`/`frame_1`를 우선 바인딩하고 legacy duplicate frame(`WorldFrame`, `Frame_1`)은 비활성화한다.
4. 시각 자산은 `ScaraRobot.prefab`을 hidden donor source로 유지하고, `BaseVisual`, `Link0Visual`, `Link1Visual`, `EndEffectorVisualMesh`에 mesh-only 복제해 사용한다.
5. vendor script/drive/logic/runtime은 사용하지 않으며, 프로젝트 FK 결과만 시각 transform의 Source of Truth로 유지한다.
6. 1차 범위는 2DOF 전용이며 추가 축(`Axis3` 등)은 donor source에 남기더라도 런타임 제어 대상에서 제외한다.
7. UI는 씬 오브젝트 우선 배선 정책을 유지하고, `TopBar`/`LeftPanel`/`RightPanel`/`BottomBar`에 공통 panel surface를 적용해 학습 화면 MVP를 유지한다.
8. `Main Camera`는 `RobotRoot`, `frame_0`, `frame_1`, `Frame_EE`가 동시에 보이는 교육용 구도를 기본값으로 유지한다.

## 6) Test Execution Standard

### A) Local
1. Test Runner에서 EditMode 우선 실행
2. UI/UX는 PlayMode 스모크로 온보딩/게이트/툴팁 경로 확인
3. PlayMode 스모크 기준: 온보딩/게이트/Skip/패널가시성/툴팁+용어사전 + `TemplateSelector`/`DHTableEditor`/`MatrixDisplay` + canonical frame/donor mesh + UI MVP layout 검증을 포함한 총 15건 유지

### B) CLI
```powershell
Unity.exe -batchmode -projectPath "C:\Users\ezen601\Desktop\Jason\robotapp2" -runTests -testPlatform EditMode -testResults "Logs\editmode-results.xml" -quit
```

```powershell
Unity.exe -batchmode -projectPath "C:\Users\ezen601\Desktop\Jason\robotapp2" -runTests -testPlatform PlayMode -testResults "Logs\playmode-results.xml" -quit
```

## 7) Next

1. Phase 4 Visualization 계속: donor mesh 정렬/스케일 세부값 보정, `Frame_EE` 포함 수동 QA 마감
2. GitHub PR 1건 생성 후 `unity-tests` 워크플로우가 self-hosted 러너에서 실제 통과하는지 검증.
3. `Assembly-CSharp.csproj` 로컬 빌드 실패(생성 csproj 동기화 이슈) 원인 정리 후 문서화.
