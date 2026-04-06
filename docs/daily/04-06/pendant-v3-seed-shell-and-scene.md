# Pendant V3 Seed Shell And Scene

## Date
- 2026-04-06 (KST)

## Summary
- V3 기준선 문서를 현재 브랜치/검증 순서에 맞게 정리했다.
- `0A` bootstrap 호출 surface를 `unityctl exec` + editor menu로 잠갔다.
- `0B` 최소 셸 자산과 `RobotControlV3.unity` 생성 경로를 실제 코드로 추가했다.

## Added
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `Assets/UI/PendantV3/CLAUDE.md`
- `Assets/UI/PendantV3/icons/CLAUDE.md`
- `Assets/UI/PendantV3/popups/CLAUDE.md`
- `Assets/Scripts/UI/RobotControlV3/CLAUDE.md`
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/UI/PendantV3/pendant-v3-tablet.uss`

## Updated
- `docs/ref/product/pendant-v3/README.md`
- `docs/ref/product/pendant-v3/implementation-plan.md`
- `docs/ref/product/pendant-v3/AGENT-CONTRACT.md`
- `docs/ref/product/pendant-v3/unityctl-recipes.md`
- `Assets/Editor/KineTutor3D/CliTools/PendantV3BootstrapTool.cs`
- `Assets/Scripts/App/SceneId.cs`
- `Assets/Scripts/App/SceneCatalog.cs`
- `Assets/Scenes/CLAUDE.md`

## Intent Locked
- V3 구현 브랜치는 `codex/robotcontrol-v3-toolkit`
- `RobotControlV3.unity` 최소 씬은 `0B`부터 존재해야 함
- `verify-v3.json`은 기본 번들이고, `3C` 종료는 추가 수동 레시피까지 같이 필요

## Next
- `unityctl exec`로 Phase 0A 자산과 V3 씬을 실제 생성
- `scene snapshot` + screenshot으로 최소 셸 증빙 확보
