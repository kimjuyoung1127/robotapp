# Pendant V3 Phase 0A Kickoff

## Date
- 2026-04-06 (KST)

## Summary
- `codex/robotcontrol-v3-toolkit` 브랜치에서 V3 구현을 `0A`부터 시작했다.
- PanelSettings, TextSettings, UIDocument bootstrap 기준선을 실제 프로젝트 자산/코드로 추가했다.

## Added
- `Assets/UI/PendantV3/PanelSettings/PendantV3PanelSettings.asset`
- `Assets/UI/PendantV3/PanelSettings/PendantV3TextSettings.asset`
- `Assets/Scripts/UI/RobotControlV3/PendantV3Document.cs`
- `Assets/Editor/KineTutor3D/CliTools/PendantV3BootstrapTool.cs`

## Locked Defaults Applied
- PanelSettings reference resolution `1920x1080`
- PanelSettings match `0.5`
- PanelSettings sorting order `100`
- PanelSettings -> TextSettings 연결
- TextSettings warnings enabled
- modern Hangul line breaking enabled

## Validation
- `unityctl check --project ... --type compile --json` 통과

## Open Item
- `PendantV3.spriteatlas`는 자동 생성 타입/호출 표면 이슈로 이번 턴에 실자산 생성까지 닫지 못했다.
- 다음 단위에서는 에디터 호출 경로를 정리해 sprite atlas 생성까지 마무리한다.
