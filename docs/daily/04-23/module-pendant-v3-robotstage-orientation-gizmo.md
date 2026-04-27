# Pendant V3 RobotStage Orientation Gizmo

## Context

- 메인패널 world gizmo를 계속 늘리는 방향보다, `RobotStage` 우상단에 Unity Scene Gizmo 스타일의 방향 위젯을 두는 쪽이 덜 헷갈린다는 사용자 피드백이 들어왔다.
- 기존 `Base/Tool/선택 파츠` 축과 카메라 방향 위젯의 역할을 문서상 분리할 필요가 있었다.

## Doc Update

- `docs/ref/product/pendant-v3/feature-3d-viewport.md`를 갱신했다.
- `RobotStage` 우상단 orientation widget을 공식 표시 요소로 추가했다.
- orientation widget은 카메라 방향 표시 전용이며 `BaseFrame`, `ToolFrame`, 선택 파츠 축을 대체하지 않는다고 명시했다.
- 고스트는 기본 OFF, 명시적 미리보기나 전용 토글일 때만 켜진다고 정리했다.

## Implementation

- `RobotStageHost` 안에 `RobotStageOrientationHost` overlay를 추가했다.
- `RobotStageOrientationGizmoController`를 새로 추가해 `V3StageCamera` 방향을 기준으로 `X/Y/Z` badge를 우상단에 배치한다.
- `X` 클릭은 `Right`, `Y` 클릭은 `Top`, `Z` 클릭은 `Front`, 중앙점 클릭은 `Iso`로 카메라 프리셋 점프를 수행한다.
- 메인패널 기존 world `Base/Tool` gizmo는 제품 경로에서 제외하고, 시각화 카드도 `Path / Ghost / Bound / Coll / Cam` 중심으로 유지한다.
- bootstrap 경로는 `PendantV3SceneCoordinator`에서 같이 초기화되도록 연결했다.

## Validation

- `unityctl check --type compile`: pass
- orientation host가 `RobotStageHost` 우상단 overlay로 생성되는 경로를 코드 기준으로 확인
- `RobotStageOrientationGizmoControllerTests`: `2/2 PASS`

## Follow-up Doc Sync

- `shell-layout.md`의 보조패널/컨텍스트 패널 배치를 현재 구현 기준으로 다시 맞췄다.
- `progress-checklist.md`의 `ViewportToolbarHost -> ViewportSelectionSection` 순서와 `Base / Tool` 툴바 카피를 현재 구현 기준으로 정리했다.
