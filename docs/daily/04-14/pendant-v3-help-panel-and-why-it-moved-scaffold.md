# Pendant V3 Help Panel And Why It Moved Scaffold

## Date
- 2026-04-14 (KST)

## Design Lock
- 이번 단위는 `help-panel / WhyItMoved` 최소 scaffold까지만 닫는다.
- `NavHelp`일 때 work panel에서 도움말을 여는 구조로 잠그고, `WhyItMoved`는 우측 context card 전담 controller로 분리한다.
- 도움말 카피 심화, 실제 정책 연결, action hint 통합은 다음 단위로 남긴다.

## Summary
- `help-panel.uxml/.uss`와 `HelpPanelController.cs`를 추가해 `NavHelp` 전용 도움말 패널을 붙였다.
- `WhyItMovedController.cs`를 추가해 우측 최근 조작 메모 카드를 `StatusCardController`에서 분리했다.
- `PendantV3ShellStateController`가 새 controller까지 notify 하도록 연결했고, `AuthorSceneSafe()`가 씬에 직렬화 참조를 주입하도록 확장했다.

## Updated Files
- `Assets/UI/PendantV3/help-panel.uxml`
- `Assets/UI/PendantV3/help-panel.uss`
- `Assets/Scripts/UI/RobotControlV3/HelpPanelController.cs`
- `Assets/Scripts/UI/RobotControlV3/WhyItMovedController.cs`
- `Assets/Scripts/UI/RobotControlV3/StatusCardController.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.State.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3Document.cs`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `docs/ref/product/pendant-v3/progress-checklist.md`

## Verification
- `unityctl check --type compile`
  - pass
- `AuthorSceneSafe()` 후 `GetDocumentRootComponentSummary()`
  - `HelpPanelController`, `WhyItMovedController` 포함 확인
- play smoke
  - actual `NavHelp` click
    - `HelpPanelHost` visible + childCount=1
    - `WorkTabBar` class에 `rc-hidden` 반영
  - `WhyItMovedSummary`
    - text=`지금 상태: 연결됨 / 서보 OFF` 확인

## Self Review
- 역할 경계
  - 도움말 패널과 최근 조작 메모를 별도 controller로 분리해 `StatusCardController` 비대화를 막았다.
  - help/why-it-moved는 표시 전담만 담당하고, 상태 계산은 여전히 preview state 기준으로 유지했다.
- 남은 리스크
  - 도움말 문구는 아직 기본 scaffold 수준이라 탭별/위험도별 세분화가 덜 됐다.
  - `NavHelp` 전용 panel은 붙었지만 tablet help 흐름/first-run guide는 아직 없다.
