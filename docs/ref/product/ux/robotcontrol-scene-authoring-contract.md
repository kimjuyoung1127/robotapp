# RobotControlV2 Scene Authoring Contract

## Purpose
- `RobotControlV2.unity`를 Unity 씬에서 직접 수정할 때, 무엇은 자유롭게 바꿔도 되고 무엇은 이름/구조를 유지해야 하는지 잠근다.
- `authored-first` 원칙을 실제 씬 편집 워크플로우로 연결한다.
- 이후 세션에서 “왜 Play 후 되돌아왔는지”, “어떤 노드는 건드리면 안 되는지”를 빠르게 판단하게 한다.

## Parent Doc
- [robotcontrol-scene-hierarchy.md](./robotcontrol-scene-hierarchy.md)
- [robotcontrol-v2-naming-ssot.md](./robotcontrol-v2-naming-ssot.md)
- [robotcontrol-implementation-bridge.md](./robotcontrol-implementation-bridge.md)
- [robotcontrol-next-session-handoff.md](./robotcontrol-next-session-handoff.md)

## Last Updated
- 2026-04-01 (KST)

## Scope
- 대상 씬: `Assets/Scenes/RobotControlV2.unity`
- 대상 패널:
  - `TcpJogPanel`
  - `JointJogPanel`
  - `PointMovePanel`
  - `TeachingPanel`

## Core Rule
1. `scene authored 값`이 source of truth다.
2. 이미 존재하는 authored 오브젝트의 `RectTransform`은 코드가 다시 덮지 않는 것을 기준으로 한다.
3. 구조를 유지한 채 내부 배치/패딩/크기를 바꾸는 것이 기본 authoring 방식이다.
4. 이름이 바뀌면 binder path가 끊길 수 있으므로, 바인딩 대상 오브젝트 이름은 바꾸지 않는다.

## Safe To Edit

### 직접 수정해도 되는 것
- `RectTransform`
  - `anchoredPosition`
  - `sizeDelta`
  - `anchorMin`
  - `anchorMax`
  - `pivot`
  - `offsetMin`
  - `offsetMax`
- `Image`
  - 배경색
  - sprite
  - alpha
- `Text`
  - font size
  - alignment
  - color
  - 줄간격
- `CanvasGroup`
  - alpha
  - interactable
  - blocksRaycasts
- 버튼/입력칸의 폭, 높이, 내부 텍스트 정렬

### 직접 수정해도 되는 authored section
- `TcpJogPanel`
  - `Header`
  - `CoordinateRow`
  - `IncrementCard`
  - `AxisGrid`
  - `ActionRow`
  - `InfoCard`
- `JointJogPanel`
  - `Header`
  - `SingleAxisCard`
  - `MultiAxisCard`
  - `SummaryCard`
  - `SliderStack`
  - `SingleAxisRow_*`
  - `MultiAxisRow_*`
- `PointMovePanel`
  - `Header`
  - `TargetCard`
  - `PoseGrid`
  - `ActionRow`
  - `XCard ~ RZCard`
- `TeachingPanel`
  - `Header`
  - `QuickActionRow`
  - `PointListCard`
  - `TpdCard`
  - `SummaryCard`
  - `PointRow_*`

## Do Not Rename

### Panel root
- `TcpJogPanel`
- `JointJogPanel`
- `PointMovePanel`
- `TeachingPanel`

### Required child names
- `Header`
- `ActionRow`
- `SummaryCard`
- `InfoCard`
- `TargetCard`
- `PoseGrid`
- `CoordinateRow`
- `IncrementCard`
- `AxisGrid`
- `SingleAxisCard`
- `MultiAxisCard`
- `SliderStack`
- `QuickActionRow`
- `PointListCard`
- `TpdCard`

### Required repeated names
- `SingleAxisRow_0 ~ SingleAxisRow_5`
- `MultiAxisRow_0 ~ MultiAxisRow_5`
- `PointRow_0 ~ PointRow_3`
- `XCard`, `YCard`, `ZCard`, `RXCard`, `RYCard`, `RZCard`

## Do Not Delete Without Replacing
- panel root에 붙은 패널 컴포넌트
  - `KineTutor3D.UI.TcpJogPanel`
  - `KineTutor3D.UI.JointJogPanel`
  - `KineTutor3D.UI.PointMovePanel`
  - `KineTutor3D.UI.TeachingPanel`
- panel root의 `CanvasGroup`
- 패널 안 상태 텍스트 노드
  - `InfoCard/CurrentPose`
  - `InfoCard/StateText`
  - `SummaryCard/JointSummary`
  - `TargetCard/CurrentPose`
  - `TargetCard/TargetText`
  - `SummaryCard/SummaryText`

## Current Runtime Guarantees
- `RobotControlShellBinder`는 이미 존재하는 authored 부모 rect를 다시 stretch/anchor 하지 않는다.
- 각 패널 스크립트는 scene-authored 구조가 완전하면 fallback 생성 레이아웃으로 들어가지 않는다.
- authoring bridge / builder는 네 패널 내부 rect를 다시 freeze 숫자로 덮지 않는다.

## Authoring Workflow
1. `RobotControlV2.unity`를 연다.
2. `TcpJogPanel`, `JointJogPanel`, `PointMovePanel`, `TeachingPanel` 중 하나를 선택한다.
3. child section의 `RectTransform`과 시각 속성을 직접 조절한다.
4. 필요 시 한 패널만 보이게 `CanvasGroup` 또는 active 상태를 잠깐 바꿔 확인한다.
5. `scene save`
6. `unityctl check --type compile`
7. Play 진입 후 값이 유지되는지 확인한다.

## If You Need To Rebuild Structure
- child가 사라졌거나 path가 깨졌다면 다음 순서를 따른다.
1. `AuthorOpenScene()` 또는 `Author V2 Shell`로 최소 구조를 복구한다.
2. 복구 직후 패널 내부 rect를 다시 씬에서 authoring한다.
3. 구조 복구 후에는 bridge 숫자보다 scene saved 값이 기준이다.

## Verification Checklist
- `scene snapshot`에서 패널 child 이름이 기대값과 같은가
- `gameobject find` 기준으로 `LayoutGroup`이 다시 붙지 않았는가
- Play 진입 후 패널 위치가 유지되는가
- `RobotControlShellBinderTests`가 통과하는가

## Known Limit
- `EasyMotionPanel`은 아직 `VerticalLayoutGroup`을 유지하는 영역이 남아 있어, 위 네 패널과 동일한 수준의 자유 authoring 대상으로 보지 않는다.
- Game View 스크린샷은 상황에 따라 overlay UI를 정확히 보여주지 못할 수 있으므로, 실제 편집 확인에는 Scene View 또는 데스크톱 캡처를 같이 쓴다.
