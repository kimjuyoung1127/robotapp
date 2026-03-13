# Math Readiness Scene Split

## Summary
- `Math Readiness`를 `Main.unity`에서 분리해 별도 `MathReadiness.unity` 씬으로 옮겼다.
- 온보딩 `처음이에요`와 Home의 `수학 기초 워밍업` CTA가 새 씬으로 직접 진입하도록 라우팅을 변경했다.
- `Math Readiness` 마지막 단계(M3) 완료 시 `Main.unity`의 `PreKinematicsTrack`으로 bridge하도록 전환했다.

## Runtime Changes
- `Assets/Scenes/MathReadiness.unity`
  - `Main.unity` 기반의 별도 학습 씬 추가
- `Assets/Scripts/App/SceneId.cs`
  - `SceneId.MathReadiness=7` 추가
- `Assets/Scripts/App/SceneCatalog.cs`
  - `MathReadiness` 씬 메타데이터와 전역 네비게이션 항목 추가
- `Assets/Scripts/UI/OnboardingManager.cs`
  - `BeginAsBeginner()`가 `MathReadiness.unity`로 이동하도록 변경
- `Assets/Scripts/App/HomeContinueHubFlowService.cs`
  - `StartMathReadiness()`와 `ContinueLatestContext()`가 `math_readiness` track일 때 `MathReadiness.unity`로 진입하도록 변경
  - `StartGuidedLesson()`는 `CoreKinematicsTrack -> Main.unity`로 명시 고정
- `Assets/Scripts/App/AppController.cs`
  - `MathReadiness` 완료 시 `Main.unity`로 씬 전환 후 `PreKinematicsTrack`을 이어받도록 변경
- `Assets/Scripts/App/SceneCameraDirector.cs`
  - `MathReadiness`에 `Main/Sandbox`와 동일한 gameplay camera profile 적용
- `Assets/Scripts/UI/SceneNavigationBar.cs`
  - compact learning top bar 규칙이 `MathReadiness` 씬에도 동일 적용되도록 확장

## Test Notes
- `dotnet build robotapp2.sln`
  - 성공
  - 기존 Unity 패키지/third-party assembly 경고만 존재, 신규 컴파일 오류는 없음
- Unity EditMode
  - `KineTutor3D.Tests.EditMode` 실행됨
  - 기존 실패 4건 유지:
    - `FairinoConnectionServiceTests.SetMockMode_EmitsModeAndStateEvents`
    - `RuntimeFoundationTests.ApplyTemplate_MarksTemplateApply_AndRetainsPreviousJointSnapshot`
    - `RuntimeFoundationTests.SetJointAngleDegrees_CapturesPreviousJointState_AndCause`
    - `RuntimeFoundationTests.TrySetDhParameter_CapturesPreviousTransform_AndDhEditCause`
- Unity PlayMode
  - `MathReadinessFlowSmokeTests`를 시도했지만 Unity MCP test job이 초기화 timeout으로 실패
  - 콘솔 기준 새 씬 분리로 인한 compile/runtime exception은 확인되지 않았고, MCP test runner initialization blocker로 기록

## Docs Synced
- `docs/ref/architecture-mermaid.md`
- `docs/ref/USER-FLOW.md`
- `docs/ref/product/ux/guided-lesson.md`
- `docs/status/page-qa/onboarding.md`
- `docs/status/page-qa/math-readiness.md`
