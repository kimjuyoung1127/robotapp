---
name: scene-authored-routing-debug
description: "Unity Play 시작 씬, scene-authored UI source-of-truth, authored-first binder, RobotControl/Onboarding 라우팅 착시를 함께 디버깅하는 운영 스킬"
---

## Trigger
아래 상황에서 사용:
- `Play를 누르면 왜 다른 씬이 뜨지?`
- `온보딩이 아니라 RobotControl이 바로 보여`
- `코드는 바꿨는데 UI 위치가 안 바뀌어`
- `scene-authored UI인지 runtime-built UI인지 먼저 확인해줘`
- `TryBindExistingLayout 이후 좌표가 왜 코드와 다르지?`
- `플레이 전에 수정한 온보딩 위치가 Play 하면 원래대로 돌아가`

키워드:
`Always Start From Onboarding`, `playModeStartScene`, `BootSceneRouter`, `scene-authored`, `authored-first`, `RobotControl`, `Onboarding`, `BtnDiagnostics`, `DrawerPanel`, `bind only`, `normalizer`

## Read First
1. `Assets/Editor/KineTutor3D/BootScenePlayModeSetup.cs`
2. `Assets/Scripts/App/BootSceneRouter.cs`
3. `Assets/Scripts/App/Fairino/RobotControlSceneCoordinator.cs`
4. `Assets/Scripts/UI/FairinoRobotControlViewBuilder.cs`
5. `Assets/Scenes/RobotControl.unity`
6. `docs/status/page-qa/robot-control.md`
7. `docs/daily/03-16/robotcontrol-scene-authored-pilot-review.md`

## Check Order
1. 현재 active scene과 PlayMode 여부를 먼저 증명한다.
2. `Always Start From Onboarding` 토글이 켜져 있는지 확인한다.
3. Play 시작이 잘못되면:
   - 에디터 토글 문제인지
   - `BootSceneRouter`의 `HasVisited()` 분기 문제인지
   - 현재 열린 씬 그대로 시작한 것인지
   를 나눈다.
4. UI 위치가 안 바뀌면 `runtime-built`인지 `scene-authored`인지 먼저 판정한다.
5. `TryBindExistingLayout(...)` 경로가 활성화됐다면 코드 숫자보다 `.unity`의 `RectTransform` 값이 source of truth인지 확인한다.
6. `Canvas/RobotControlShell/...` 경로에서 실제 object 개수를 확인해 중복 생성 여부를 본다.
7. authored shell이면 `.unity` 좌표를 먼저 고치고, builder 숫자는 fallback용으로만 맞춘다.
8. `scene-authored`처럼 보여도 `TryBindExisting...()`가 내부에서 anchor/size/text를 다시 쓰는 `runtime normalizer`인지 확인한다.
9. 전역 네비게이션처럼 코드 소유가 필요한 레이어와, 페이지 본문처럼 `bind only`여야 하는 레이어를 분리한다.

## Guardrails
1. active scene를 확인하기 전에는 MCP screenshot이나 hierarchy를 믿지 않는다.
2. Play 시작 씬 문제를 `RobotControl` 코드 버그로 바로 결론내리지 않는다.
3. `scene-authored UI` 전환 이후에는 `BuildTopBar()` 숫자만 바꾸고 끝냈다고 판단하지 않는다.
4. `TryBindExistingLayout(...)`가 하나라도 못 찾으면 fallback 생성으로 내려갈 수 있으니 부분 authored 상태를 경계한다.
5. authored 좌표 확인 없이 `왜 안 바뀌지`를 반복하지 않는다.
6. 온보딩 검증을 위해 토글을 바꿨으면 마지막에 원래 상태로 복구한다.
7. 페이지 본문이 authored-first여야 한다면 `TryBindExisting...()`에서 anchor/size/text를 다시 쓰지 않는다.
8. 기준 레이아웃 재정렬이 필요하면 runtime에서 자동 보정하지 말고 `Author Scene UI` 메뉴로 분리한다.

## MCP Loop
1. `manage_scene get_active`
2. `manage_editor stop` 후 토글/씬 상태 조정
3. `manage_editor play`
4. `manage_scene get_active`로 실제 시작 씬 재확인
5. `find_gameobjects by_path Canvas/RobotControlShell/...`
6. 필요 시 `read_mcp_resource mcpforunity://scene/gameobject/{id}/components`

## Source-of-Truth Rules
1. `BootScenePlayModeSetup`는 Play 시작 씬 강제 규칙이다.
2. `BootSceneRouter`는 Boot 씬에서 첫 방문/재방문 흐름을 가른다.
3. `RobotControlSceneCoordinator + TryBindExistingLayout`가 성공하면 authored shell이 우선이다.
4. authored shell이 우선인 상태에서는 `RobotControl.unity`의 `RectTransform` 값이 실제 배치 기준이다.
5. builder의 anchor 숫자는 fallback 생성 경로를 위한 값으로 취급한다.
6. `bind only` 페이지에서는 Play 전 씬에서 바꾼 위치/텍스트가 Play 후에도 유지되어야 한다.
7. `runtime normalizer`는 전역 네비게이션, 필수 시스템 bootstrap 같은 코드 소유 레이어에만 허용한다.

## Validation
1. 기본 상태에서 `Play` 후 active scene이 `Onboarding`인지 확인한다.
2. 토글을 잠시 끄면 현재 열린 `RobotControl`이 직접 시작되는지 확인한다.
3. `Canvas/RobotControlShell/TopBar/BtnDiagnostics`가 1개만 존재하는지 확인한다.
4. `Canvas/RobotControlShell/DiagnosticsDrawer/DrawerPanel`이 1개만 존재하는지 확인한다.
5. authored 위치 수정 후에는 `.unity` 값과 runtime `RectTransform` 값이 일치하는지 확인한다.
6. authored-first 본문에서는 Play 전 수정한 위치/텍스트를 바꾼 뒤 `TryBindExisting...()`가 그대로 유지하는지 EditMode 테스트로 검증한다.
7. `Author Scene UI` 메뉴 실행 시에만 기준 레이아웃이 다시 씬에 구워지는지 확인한다.

## Output Template
```
[scene-authored-routing-debug 완료]
- active scene: {scene}
- play start source: {Always Start From Onboarding / BootSceneRouter / current scene}
- UI source of truth: {scene YAML / runtime-built fallback}
- 중복 여부: {object path별 count}
- 조치:
  - {토글 복구 여부}
  - {authored 좌표 수정 여부}
  - {binder/fallback 수정 여부}
- 검증:
  - {Onboarding 시작 확인}
  - {RobotControl direct play 확인}
  - {specific path count 확인}
```
