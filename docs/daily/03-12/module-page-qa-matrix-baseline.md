# Module Log: Page QA Matrix Baseline

Date: 2026-03-12 (KST)

## Summary
- 실제 진입 가능한 페이지 기준으로 첫 `Page QA Matrix` baseline을 만들었다.
- 감사 범위는 `Onboarding`, `Home / Continue Hub`, `Guided Lesson`, `Math Readiness`, `Robot Library`, `Sandbox`로 고정했다.
- `Boot`는 route-only, `Instructor Mode / Progress / Settings`는 미구현 IA gap으로만 기록했다.

## Updated Docs
- `docs/archive/legacy/page-qa/PAGE-QA-MATRIX-2026-03-23.md`
- `docs/status/PHASE-EXECUTION-BOARD.md`
- `docs/status/PROJECT-STATUS.md`

## Key Findings
- `Sandbox`는 compile blocker + panel overlap risk + 문서 핵심 기능 누락이 함께 있어 최우선 페이지로 분류했다.
- `Guided Lesson`은 core experience지만 `Instructor CTA`와 `Save/Replay entry` 계약이 비어 있어 후속 보강이 필요하다.
- `Robot Library`는 grid/detail/basic routing은 있으나 filter, compare strip, instructor routing이 비어 있다.
- `Home / Continue Hub`는 현재 범위에서는 가장 안정적인 편이지만 `Progress/Settings` placeholder 정책 정리가 남아 있다.

## Verification Notes
- `SceneFlowSmokeTests`, `UIPanelDesignSystemSmokeTests`, `UxFlowSmokeTests`, `MathReadinessFlowSmokeTests`를 근거로 route/핵심 CTA 구현 여부를 판정했다.
- `dotnet build KineTutor3D.Runtime.csproj`는 성공했다.
- Unity Console에서 `Assets/Scripts/App/AppController.cs(358,37): error CS7036` compile error가 확인되어 `Main/Sandbox` 공통 blocker로 기록했다.

## Follow-up In Same Turn
- `SandboxActionPanelViewBuilder`, `SnapshotLitePanelViewBuilder`를 수정해 Sandbox 전용 패널이 좌/우 side panel 내부 레이아웃을 채우도록 변경했다.
- Unity refresh 후 콘솔 재확인에서는 위 compile error가 다시 나타나지 않았다.
- `AppController.OpenCurrentRobotSandbox()`와 `TemplateSelector`의 `BtnLessonOpenSandbox`를 추가해 Guided Lesson에서 Sandbox 진입이 더 직접적으로 보이도록 했다.
- `RobotDetailDrawer`는 우측 drawer 대신 modal overlay 구조로 전환해 Robot Library grid 가림 문제를 줄였다.
- `docs/status/page-qa/` 아래에 페이지별 QA runbook 6종과 index를 추가했고, `QaToolsMenu`에 페이지별 준비 메뉴를 확장했다.
- `Boot`와 `Home`에는 실제 `Main Camera` 루트를 추가했고, `Onboarding`에서는 `SceneNavigationBar` 컴포넌트를 씬 자체에서 제거했으며, `RobotLibrary`의 `EventSystem`을 `InputSystemUIInputModule`로 통일했다.
- `HomeContinueHubController`와 `RobotLibraryManager`를 씬 저장 UI 우선 바인딩 구조로 바꿔 `Onboarding / Home / RobotLibrary`의 정적 씬 UI 우선 방향을 굳혔다.
- `OnboardingManager.BeginAsBeginner()`는 `Home` 대신 `Main`의 `Math Readiness`로 직접 진입하도록 수정했고, 관련 흐름 문서와 PlayMode 테스트를 함께 갱신했다.
