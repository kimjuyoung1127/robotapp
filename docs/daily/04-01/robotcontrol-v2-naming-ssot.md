# RobotControlV2 Naming SSOT Update

## Date
- 2026-04-01 (KST)

## Summary
- `RobotControlV2`의 UI 이름, authored 오브젝트 이름, 코드 심볼, 사용자 표시 라벨을 한 표로 잠갔다.
- `UIDesignTokens.RobotControlV2`와 `RobotControlViewState`처럼 이미 있던 SSOT와 별개로, 이번에는 `이름 SSOT`를 leaf 문서로 분리했다.
- `robotcontrol-next-session-handoff.md`와 `robotcontrol-implementation-bridge.md`에 새 이름 SSOT 문서 링크를 추가했다.
- 코드에도 바로 반영했다. `SceneCatalog`의 V2 표시명을 `로봇 제어 V2`로 맞추고, `RobotControlWorkTab`을 `TcpJog`, `JointJog`, `PointMove`로 정리했다.
- `TopStatusBar`, `StatusSummaryPanel`, `HelpPanel`, `RobotControlWhyItMovedPanel`, `RobotControlShellBinder`의 V2 placeholder copy를 한국어 기준으로 맞췄다.

## Key Decisions
- 코드 심볼은 영어 PascalCase를 유지한다.
- scene authored 오브젝트 이름은 코드 심볼과 1:1로 맞춘다.
- 사용자-facing 라벨은 한국어를 기본으로 한다.
- `WhyItMoved`는 곧바로 삭제하지 않고 deprecated alias로 남긴다.
- `RobotControlWorkTab`은 다음 구현 패스에서 `TcpJog`, `JointJog`, `PointMove`로 맞춘다.

## Self Review
- 구조 SSOT, 상태 SSOT, 토큰 SSOT와 이름 SSOT를 분리해 문서 책임이 겹치지 않게 했다.
- 지금 바로 흔들면 위험한 이름인 `SceneId.RobotControlV2`, scene asset 이름, binder path는 유지 대상으로 분리했다.
- 표가 실제 코드 수정 순서와 연결되도록 `display copy -> enum -> panel/class` 순으로 rollout order를 적었다.
- 2차 자기리뷰에서 current/target 이름이 한 표에 섞여 보일 수 있다는 점을 반영해 `Current Code Symbol`, `Target Code Symbol`, `Status` 열을 추가했다.

## Next
- `MotionExplainer`의 코드 심볼과 authored path 정책을 확정한다.
- `WhyItMoved` 계열 명칭을 `MotionExplainer` 기준으로 단계적 이관한다.
