# KineTutor3D 프로젝트 상태

최종 업데이트: 2026-03-05 (KST)
기준 문서: `CLAUDE.md`, `KineTutor3D_Execution_Plan.md`

## 현재 Phase
- **Phase 0: Foundation** (완료)
- **Phase 1: Types + Math (TDD)** (완료)
- **Phase 2: Kinematics Core (DH + FK)** (완료)
- **Phase 3: Template 2DOF + App/UI 연결 (MVP)** (QA)
- **Phase 6: CI/CD (Unity tests workflow)** (진행 중)
- 병행 작업: **Phase 3 Student-Friendly UX 런타임 연결/데이터 실체화** 완료

## Phase 0 체크리스트
- [x] Unity 프로젝트 생성
- [x] unity-mcp 패키지 설치
- [x] Git 초기화 및 Unity `.gitignore` 적용
- [x] `Main.unity` 기준점 확정 (`Assets/Scenes/Main.unity`)
- [x] Build Settings index 0 설정 (`Main.unity` 단일)
- [x] MCP 연결 스모크 확인 (telemetry/scene/console 응답)
- [x] Unity Console 컴파일 에러 0 최종 확인 (MCP 시스템 로그 제외 기준)
- [x] 공식문서 근거 검증 완료 (`docs.unity3d.com` 링크 첨부 규칙)

## 이번 턴 반영 내용 (Phase 3 확장 + CI 고정)
1. App/Runtime 인터페이스 확장
   - `Assets/Scripts/App/AppController.cs`
   - `OnTemplateChanged`, `OnKinematicsUpdated` 이벤트 추가
   - Slider 입력 + DHTable 입력을 단일 FK 파이프라인으로 통합
2. UI 실기능 3개 구현
   - `Assets/Scripts/UI/TemplateSelector.cs` (2DOF 단일 옵션)
   - `Assets/Scripts/UI/DHTableEditor.cs` (`theta` read-only, `d/a/alpha` 편집)
   - `Assets/Scripts/UI/MatrixDisplay.cs` (`A1/A2/T02` 실시간 표시)
3. 테스트 확장
   - EditMode: `DHTableEditorValidationTests`
   - PlayMode: `TemplateSelector`, `DHTableEditor`, `MatrixDisplay` 연동 3케이스 추가
   - 결과: EditMode 42/42, PlayMode 10/10
4. CI 자동 실행 워크플로우 추가
   - `.github/workflows/unity-tests.yml`
   - `push/pr/workflow_dispatch` 트리거
   - self-hosted windows 러너에서 EditMode/PlayMode 분리 실행 + XML artifact 업로드

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
   - PlayMode 스모크 5건 기준 유지 (`Assets/Tests/PlayMode/UxFlowSmokeTests.cs`)

## Self-Review Gate (Cycle Result)
1. 기능 리뷰
   - `DHStandard`/`ForwardKinematics` 구현으로 표준 DH 및 누적 FK 경로가 동작함
   - `Types/Math/Kinematics`에서 UnityEngine 참조를 사용하지 않음
2. 코드 리뷰
   - NaN/Infinity 입력 가드가 DH/FK 입력 경계에 반영됨
   - Revolute/Prismatic 분기 처리 및 길이 불일치 가드가 반영됨
3. 테스트 리뷰
   - EditMode 테스트 확장 완료 (`DHStandardTests`, `FKTests`, `Template2DOF_RRTests`)
   - Unity Test Runner: EditMode 38/38 통과, PlayMode 7/7 통과
   - `dotnet build Assembly-CSharp.csproj` 오류 0 확인 (외부 패키지 경고만 존재)
   - Unity Console 에러는 MCP 시스템 로그 외 프로젝트 코드 에러 0
4. 문서 리뷰
   - `PROJECT-STATUS` / `PHASE-EXECUTION-BOARD` / `SKILL-DOC-MATRIX` 동기화 반영
   - 공식문서 근거 문서 경로를 상태 문서에 연결함
5. 운영 스킬화
   - 기존 `debug-success-capture` 포맷으로 결과 기록 유지

## 다음 작업
1. PR 기준으로 `unity-tests` 워크플로우 1회 실주행 확인(러너 라벨/UNITY_EXE/env 점검)
2. `Assembly-CSharp.csproj` 로컬 빌드 실패 원인(생성 csproj 불일치) 문서화
