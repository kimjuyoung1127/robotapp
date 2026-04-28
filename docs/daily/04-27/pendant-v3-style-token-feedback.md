# Pendant V3 Style Token + Button Feedback

Date: 2026-04-27 (KST)

## Summary

- Pendant V3 색/radius 하드코딩 후보를 토큰 소비 구조로 정리했다.
- root token 정의는 `Assets/UI/PendantV3/pendant-v3.uss`에 모으고, 나머지 Pendant V3 USS는 `var(--rc-...)`를 소비하는 방식으로 맞췄다.
- 기본 버튼이 너무 회색이고 텍스트가 하얗게 떠서 읽기 어려운 문제를 줄이기 위해 `--rc-button-*` 토큰을 추가했다.
- hover / active / focus / pressed / selected 상태에서 배경과 border가 함께 변하게 해서 클릭/선택 피드백을 강화했다.
- UGUI 버튼 생성 경로는 `UiRuntimeStyle.EnsureButtonBackground(...)`로 rounded sliced background를 보장한다.

## Token Groups

- Button:
  - `--rc-button-bg`
  - `--rc-button-bg-hover`
  - `--rc-button-bg-active`
  - `--rc-button-bg-pressed`
  - `--rc-button-bg-primary`
  - `--rc-button-text`
  - `--rc-button-text-strong`
  - `--rc-button-border-active`
- Radius:
  - `--rc-radius-xs`
  - `--rc-radius-sm`
  - `--rc-radius-base`
  - `--rc-radius-lg`
  - `--rc-radius-xl`
  - `--rc-radius-card`
  - `--rc-radius-axis`
  - `--rc-radius-pill`
  - `--rc-radius-sheet`
- Domain UI:
  - stage
  - diagnostic
  - axis widget
  - debug/highlight
  - split handle
  - modal
  - status/safety

## Changed

- `Assets/UI/PendantV3/pendant-v3.uss`
  - root style tokens and global `.rc-root .unity-button` button baseline.
  - hover/active/focus/pressed state styling.
  - active class common border feedback.
- `Assets/UI/PendantV3/*.uss`
  - direct color and numeric radius consumers replaced with tokens.
- `Assets/Scripts/UI/DesignSystem/UiRuntimeStyle.cs`
  - added shared rounded button background setup.
- `Assets/Scripts/UI/DesignSystem/UIComponentFactory.cs`
  - icon buttons now use the same rounded background setup path.
- `Assets/Tests/EditMode/Validation/RobotControlV3HardcodingGuardTests.cs`
  - button token/state assertions added.
  - Pendant V3 USS consumer literal scan added.

## Validation

- `unityctl check --type compile`: pass.
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlV3HardcodingGuardTests`: `4/4 PASS`.
- Static style scan: no direct color/radius consumers in Pendant V3 USS outside root token definitions.
- `git diff --check -- Assets/UI/PendantV3 Assets/Tests/EditMode/Validation/RobotControlV3HardcodingGuardTests.cs`: pass.

## Follow-up

- Capture a runtime screenshot and confirm selected/pressed buttons read clearly in the actual pendant.
- Check tablet breakpoint once more for button text contrast and active border density.
