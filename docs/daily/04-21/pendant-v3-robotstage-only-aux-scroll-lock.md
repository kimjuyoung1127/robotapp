# Pendant V3 RobotStage Only + Aux Scroll Lock

## Summary

- `WorkPanel` 본체를 `RobotStage` 단일 책임으로 더 강하게 잠갔다.
- 기존 `ControlDock` 역할은 `ViewportHost`의 보조 작업 패널로 이동하고, 공용 스크롤 규칙을 그대로 재사용하게 정리했다.
- `CartesianArrowsOverlayHost`는 로봇 직결 시각 요소로 보고 `RobotStage` 쪽으로 되돌렸다.
- `RobotControlV3RuntimeController`에 `RobotStageFloorGrid`, `RobotPartSelectionGizmo`를 붙여 바닥 격자와 파츠 선택 XYZ 기즈모 기반을 추가했다.

## Why

- 사용자가 "핵심 작업 패널에는 로봇만 보여야 한다"는 기준을 다시 명확히 줬다.
- 기존 `RobotStage + ControlDock` 혼합 구조는 메인 패널 밀도를 높여서 시선이 자꾸 분산됐다.
- 상용 티칭펜던트 장점 중 하나는 메인 시야에서 로봇 상태를 또렷하게 유지하고, 조작은 보조 패널로 나누는 점이라 그 방향으로 더 밀었다.

## Updated Areas

- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/Scripts/UI/RobotControlV3/RobotStageRenderSurface.cs`
- `Assets/Scripts/App/Fairino/RobotControlV3RuntimeController.cs`
- `Assets/Scripts/Visualization/RobotControl/Overlays/RobotStageFloorGrid.cs`
- `Assets/Scripts/Visualization/RobotControl/Overlays/RobotPartSelectionGizmo.cs`
- `docs/ref/product/pendant-v3/README.md`
- `docs/ref/product/pendant-v3/feature-3d-viewport.md`
- `docs/ref/product/pendant-v3/shell-layout.md`
- `docs/ref/product/pendant-v3/progress-checklist.md`

## Verification

- `unityctl check --type compile`
  - pass
- `unityctl exec ... GetPanelControllerSummary()`
  - `RobotStage` host size가 `448.7 x 520`로 커지고, `ViewportHost`는 보조 패널로 유지되는 것 확인
  - `cartesianOverlayParent=RobotStageHost` 확인
  - runtime summary 기준 `grid=True`, `gizmo=True` 확인
- `unityctl console clear -> exec GetDocumentDebugSummary() -> console get-entries`
  - 새 콘솔 기준 gameplay 에러 없음
  - 남은 에러는 `unityctl` IPC 재연결 로그만 관측

## Remaining Gaps

- 고스트 로봇 / predicted path는 아직 실제 별도 시각 오브젝트로 분리되지 않았다.
- 작업공간 경계 / 충돌 경고는 UI scaffold 단계고 실제 geometry 기반 계산은 아직 미연동이다.
- 파츠 선택 기즈모는 런타임 기반은 들어갔지만, `unityctl exec`에서 직접 때리는 QA helper는 여전히 불안정하다.
