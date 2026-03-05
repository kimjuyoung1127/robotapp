# KineTutor3D 프로젝트 상태

최종 업데이트: 2026-03-05 (KST)
기준 문서: `CLAUDE.md`, `KineTutor3D_Execution_Plan.md`

## 현재 Phase
- **Phase 0: Foundation** (완료)
- **Phase 1: Types + Math (TDD)** (완료)
- **Phase 2: Kinematics Core (DH + FK)** (완료)
- **Phase 3: Template 2DOF + App/UI 연결 (MVP)** (진행 중)
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

## 이번 턴 반영 내용 (Phase 3 MVP 착수)
1. 2DOF 템플릿 추가
   - `Assets/Scripts/Templates/KineTutor3D.Templates.asmdef`
   - `Assets/Scripts/Templates/Template2DOF_RR.cs`
2. Runtime asmdef 분리
   - `Assets/Scripts/KineTutor3D.Runtime.asmdef`
   - App/UI에서 Types/Math/Kinematics/Templates 의존성 명시
3. App/UI FK 연동
   - `Assets/Scripts/App/AppController.cs`
   - `joint_slider_1/2` 자동 바인딩, degree→radian 변환, FK 결과 캐시
4. 테스트 확장
   - EditMode: `Template2DOF_RRTests`
   - PlayMode: `SliderDrivenFk_*` 2케이스 추가

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
1. Phase 3 확장: DHTableEditor/TemplateSelector/MatrixDisplay의 실 UI 기능화
2. Unity Test Runner CLI 경로를 CI 파이프라인으로 고정
