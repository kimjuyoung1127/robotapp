# RobotControl Scene-Authored Pilot Review

## What We Reviewed
- `RobotControl`의 FR5 control prefab 복구, 카메라 프로필, diagnostics drawer, compact summary, TCP validation 추가 이후 상태를 자기리뷰했다.
- `scene-authored UI` 파일럿이 실제로 동작하는지와, authored 좌표가 runtime builder 숫자보다 우선하는지 확인했다.

## Key Findings
- `RobotControl`은 이제 `Canvas/RobotControlShell/*` 구조가 `Assets/Scenes/RobotControl.unity`에 저장되는 파일럿 상태다.
- `RobotControlSceneCoordinator`는 `TryBindExistingLayout(...)`로 씬 authored UI를 우선 바인딩한다.
- 그래서 `BtnDiagnostics`와 `DrawerPanel` 위치가 코드에서 안 바뀌어 보였던 이유는 `BuildTopBar()` 숫자가 무시된 것이 아니라, 씬 authored `RectTransform` 값이 source of truth였기 때문이다.
- 실제 확인값:
  - `BtnDiagnostics`: scene YAML `m_AnchoredPosition = (-420, 0)`, runtime도 동일
  - `DrawerPanel`: scene YAML 닫힘 위치 `m_AnchoredPosition = (380, -86.7)`, runtime도 동일
- 결론: scene-authored 전환 이후 레이아웃 디버깅은 `FairinoRobotControlViewBuilder`보다 `RobotControl.unity`를 먼저 확인해야 한다.

## Test Notes
- Unity active scene: `RobotControl`
- Root hierarchy 확인:
  - `RobotControlCoordinator`
  - `FR5_RuntimeRoot`
  - `Main Camera`
  - `Directional Light`
  - `Canvas`
  - `EventSystem`
- PlayMode 진입 후 authored object 중복 생성 여부를 확인했다.
  - `Canvas` 1개
  - `EventSystem` 1개
  - `Canvas/RobotControlShell/TopBar/BtnDiagnostics` 1개
  - `Canvas/RobotControlShell/DiagnosticsDrawer` 1개
- EditMode 전체 테스트는 354개 중 기존 실패 6개로 red였다.
  - `FR5PosePresetsTests.Ready_HasExpectedAngles`
  - `OnboardingManagerTests.EnsurePresentation_ShowsOnboardingGlobalNavigationStrip`
  - `OnboardingViewBuilderTests.Build_CreatesHeadlineAndBody`
  - `RuntimeFoundationTests.ApplyTemplate_MarksTemplateApply_AndRetainsPreviousJointSnapshot`
  - `RuntimeFoundationTests.SetJointAngleDegrees_CapturesPreviousJointState_AndCause`
  - `RuntimeFoundationTests.TrySetDhParameter_CapturesPreviousTransform_AndDhEditCause`
- 이후 `FairinoRobotControlUxTests` 전용 실행은 3/3 passed였다.
  - `EnsureLayout_CreatesDiagnosticsDrawer_HiddenByDefault`
  - `EnsureLayout_CreatesDiagnosticsButton_InTopBar`
  - `TryBindExistingLayout_ReusesSceneAuthoredShell`
- 이번 `RobotControl` 파일럿으로 새로 생긴 compile error는 없었다.
- 콘솔에는 별도로 `The referenced script (Unknown) on this Behaviour is missing!`가 2건 남아 있어 환경 청소가 필요하다.
- 후속 패치에서 `ConnectionPanel`, `JointControlPanel`, `TcpControlPanel`, `DiagnosticsDrawer`는 authored child가 있으면 먼저 바인딩하고, 없을 때만 생성하도록 줄였다.
- PlayMode 진입 직후 MCP bridge가 끊겨 세부 component dump는 끝까지 못 받았지만, 끊기기 전 path query로는 `JointControlPanel`, `TcpControlPanel`, `DiagnosticsDrawer/DrawerPanel`, `TopBar/BtnDiagnostics`가 각각 1개씩만 확인됐다.

## Self-Review Risks
- `TryBindExistingLayout(...)`는 하나라도 빠지면 fallback 생성 경로로 내려가므로, 부분 authoring 상태에서는 중복 레이아웃이 생길 수 있다.
- 각 패널 `EnsurePresentation()`은 여전히 내부 위젯 위치를 재적용하므로, 현재 상태는 "완전 scene-authored"보다 "scene-authored shell + runtime refresh"에 가깝다.
- `RobotControlDiagnosticsDrawer.UnbindListeners()`의 backdrop listener 누락은 같은 패스에서 바로 수정했다.

## Recommended Next Step
1. `RobotControl.unity`의 authored 좌표를 직접 조정하는 기준을 고정한다.
2. 아직 줄이지 못한 `WhyItMovedLabel`, `StatePanel`, `MoveConfirmDialog`도 같은 authored-first 패턴으로 맞춘다.
3. `Onboarding`은 바로 옮기지 말고, `RobotControl`에서 "코드가 건드려도 되는 범위"와 "씬 authored가 절대 우선인 범위"를 먼저 문서화한다.

## Follow-up Completed
- `WhyItMovedLabel`, `StatePanel`, `MoveConfirmDialog`까지 authored-first bind 패턴으로 맞췄다.
- `scene-authored-routing-debug` 스킬을 추가해 Play 시작 씬 착시와 authored source-of-truth 점검 루프를 재사용 가능하게 고정했다.
- `OnboardingViewBuilder` / `OnboardingManager`도 scene-authored shell 우선 바인딩으로 보강했다.
  - `WelcomeModal`, `ModalSurface`, `CardRow`, `BtnBeginner`, `BtnStartLearning`, `BtnOnboardingSkip`가 씬에 있으면 먼저 재사용한다.
  - `HeadlineText` hidden text 참조를 명시적으로 만들고 `TryBindExisting(...)` 경로를 추가했다.
- `Play -> Onboarding` 기본 진입은 재검증 완료했다.
- `OnboardingViewBuilderTests`와 `FairinoRobotControlUxTests`를 함께 재실행하려 했지만, PlayMode와 겹친 EditMode runner가 Unity 쪽에서 `Test tree is not available` 상태로 꼬여 후반부 자동 재실행은 신뢰할 수 없었다.
- 이후 `Onboarding`은 전역 네비게이션만 코드 소유로 두고, 본문은 `bind only` authored-first로 전환했다.
- `OnboardingViewBuilderTests.TryBindExisting_PreservesAuthoredLayoutChanges`를 추가해 플레이 전 수정값이 Play 때 유지되는지 자동 검증했다.
