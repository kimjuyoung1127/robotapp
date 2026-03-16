# Shared UI Shell Prefab Extraction

## Summary
- `페이지별 씬 유지 + 공통 shell prefab` 방향의 첫 수직 슬라이스를 구현했다.
- 이번 라운드의 실제 공통화 대상은 `SceneNavigationBar`였다.
- `Home`에도 authored-first + authoring menu 기반을 추가했다.

## Implemented
- `QaToolsMenu`에 아래 메뉴를 추가했다.
  - `KineTutor3D/UI/Author Shared SceneNavigationBar Prefab`
  - `KineTutor3D/Home/Author Scene UI`
- `SceneNavigationBar` 공통 prefab asset을 생성했다.
  - `Assets/Runtime/UI/Prefabs/SceneNavigationBar.prefab`
- `HomeContinueHubViewBuilder.TryBindExisting(...)`가 authored shell 내부의 버튼/텍스트를 재귀적으로 찾도록 보강했다.
- `HomeContinueHubViewBuilderTests`에 authored shell 재사용 테스트를 추가했다.

## Verification
- `HomeContinueHubViewBuilderTests` 3/3 passed
  - `Build_CreatesMathReadinessButton`
  - `Build_ContinueButton_HasLeadingIcon`
  - `TryBindExisting_ReusesSceneAuthoredHomeShell`
- 이후 authored-first 관련 묶음 재검증에서도 `HomeContinueHubViewBuilderTests`는 다시 3/3 passed였다.
- Unity 콘솔에서 아래 로그를 확인했다.
  - `[QA] Shared SceneNavigationBar prefab authored at 'Assets/Runtime/UI/Prefabs/SceneNavigationBar.prefab'.`

## Caveat
- `Home/Author Scene UI` 메뉴는 코드상 추가됐지만, MCP가 다른 Unity instance/Main scene에 붙는 상황이 겹쳐 active scene을 `Home.unity`로 맞춘 상태에서 끝까지 검증하진 못했다.
- 따라서 다음 라운드에서는 `Home.unity`를 직접 연 상태에서 해당 메뉴를 한 번 더 실행해 scene YAML이 정상화됐는지 확인하는 것이 좋다.

## Next
1. `Home.unity` authoring menu 실제 실행 검증
2. `SceneNavigationBar.prefab`을 `RobotLibrary` 또는 `Sandbox`에서 공통 shell로 시범 적용
3. 필요 시 `ModalShell` prefab 추출 검토
