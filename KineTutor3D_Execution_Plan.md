# KineTutor3D Execution Plan (Current + UX Integration)

- Project: KineTutor3D
- Updated: 2026-03-05 (KST)
- Unity Target: 6000.0.64f1 (Unity 6)

## 1) Current State Snapshot

1. `Assets/Scenes/Main.unity` 존재, Build index 0 고정 완료.
2. MCP 연결 정상(telemetry/scene/console 조회 가능).
3. `Assets/realvirtual` 패키지 임포트 완료(소스 자산 보존 전략).
4. Student-Friendly UX 런타임/씬 배선/SO 데이터 실체화 완료.
5. `Assets/Tests/PlayMode/UxFlowSmokeTests.cs` 스모크 테스트 유지(기준 5건).
6. Phase 0 공식문서 근거 문서 추가 완료: `docs/ref/unity-official-evidence-phase01.md`.
7. Phase 1 Types/Math + EditMode 테스트 자산 구현 완료.
8. 검증 결과: Unity Test Runner EditMode 23/23, PlayMode 5/5 통과.

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

## 6) Test Execution Standard

### A) Local
1. Test Runner에서 EditMode 우선 실행
2. UI/UX는 PlayMode 스모크로 온보딩/게이트/툴팁 경로 확인
3. PlayMode 스모크 기준: 온보딩/게이트/Skip/패널가시성/툴팁+용어사전 경로 5건 유지

### B) CLI
```powershell
Unity.exe -batchmode -projectPath "C:\Users\ezen601\Desktop\Jason\robotapp2" -runTests -testPlatform EditMode -testResults "Logs\editmode-results.xml" -quit
```

```powershell
Unity.exe -batchmode -projectPath "C:\Users\ezen601\Desktop\Jason\robotapp2" -runTests -testPlatform PlayMode -testResults "Logs\playmode-results.xml" -quit
```

## 7) Next

1. Phase 2 착수: `DHStandard`, `ForwardKinematics` 구현 + 수치 검증.
2. CI에서 EditMode/PlayMode 자동 실행 파이프라인 고정.
