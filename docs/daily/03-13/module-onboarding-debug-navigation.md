# Onboarding Debug Navigation

## Summary
- `Onboarding.unity`에서 QA/디버그용 전역 이동 버튼을 안정적으로 보이게 조정했다.
- 기존 `TopBarBackground`/`SceneNavigationBar` 재사용 경로 대신, `Canvas` 직하위에 `OnboardingDebugNav`를 런타임 생성하는 방식으로 전환했다.
- QA/디버그 시 `Onboarding / Home / Main / Robot Library / Sandbox`로 즉시 이동할 수 있게 했다.
- `RobotLibrary` 쇼룸은 런타임 렌더 경로를 복구했고, 현재 시각 확인 기준으로 `2DOF RR + SCARA + 6DOF placeholder` 초기 3대 프리뷰가 동시에 보이는 상태까지 확인했다.

## Runtime Changes
- `Assets/Scripts/UI/OnboardingManager.cs`
  - Onboarding 전용 `OnboardingDebugNav`를 `Canvas` 직하위에 생성하도록 변경
  - child `SceneNavigationBar`를 다시 찾아 숨김 override를 해제하고, nav가 모달보다 앞으로 오도록 sibling 순서를 보정
  - `SceneNavigationBar`와 별도로 `DebugNavOnboarding/Home/Main/Robot Library/Sandbox` 버튼 행을 `Canvas` 직하위 fallback nav로 유지
- `Assets/Scripts/UI/SceneNavigationBar.cs`
  - 온보딩에서도 버튼 행이 잘리지 않도록 nav container 폭을 동적으로 계산
- `Assets/Scripts/UI/RobotLibraryManager.cs`
  - `RawImage + RenderTexture + 전용 Camera` 경로로 쇼룸 hero를 표시하도록 복구
  - PlayMode 진입 시 런타임 초기화 누락, 카드 재빌드 충돌, 스크롤 초기 위치 문제를 보정
- `Assets/Scripts/UI/RobotShowroomManager.cs`
  - 초기 hero 1개만 생성하던 구조를 `hero + 주변 preview` 초기 3대 생성 구조로 확장
- `Assets/Scripts/Visualization/RobotPreviewFactory.cs`
  - donor/procedural/placeholder 프리뷰를 bounds 기준으로 중앙 정렬/크기 정규화하도록 보정
- `Assets/Scripts/Visualization/RobotPreviewPod.cs`
  - edit/runtime 공용 정리 경로에서 안전한 destroy helper를 사용하도록 조정

## Observations
- `TopBarBackground` 내부의 기존 `SceneNavButtons`는 씬 YAML과 런타임 계층이 어긋나며, 활성/비활성 상태가 안정적이지 않았다.
- 런타임 확인 기준으로 `Canvas/OnboardingDebugNav/*` 버튼들이 실제 생성되는 것을 확인했다.
- `Onboarding` 런타임에서 `NavSandbox` 버튼까지 계층상 복구된 것을 확인했다.
- `RobotLibrary` 런타임에서는 `ShowroomPodContainer`와 카드 5개가 생성되며, 시각 검증 기준으로는 현재 `2DOF RR`, `SCARA`, `GENERIC_6DOF` 3개 프리뷰가 동시에 확인된다.

## Skillization Candidates
- `scene-ui-visibility` 확장 후보:
  - 정적 씬 UI와 런타임 생성 UI가 섞일 때 실제 활성 오브젝트 경로를 점검하는 절차
  - `TopBarBackground` 같은 정적 컨테이너가 실패할 때 `Canvas` 직하위 fallback nav를 생성하는 패턴
- 신규 스킬 후보:
  - `onboarding-debug-nav`
  - 목적: QA/디버그용 전역 페이지 이동 바를 안전하게 주입하고, 정적/동적 UGUI 계층 충돌 시 fallback 배치를 자동 적용
  - 포함 패턴: scene find -> active state 확인 -> static container 시도 -> canvas fallback 생성 -> playmode 재검증

## Notes
- 제품 기본 흐름은 여전히 `Boot -> Onboarding -> Home/Main` 기준이다.
- 이번 변경은 QA/디버깅 가속을 위한 전역 이동 수단 제공 목적이다.
- Robot Library 3D showroom은 초기 3대 동시 프리뷰 단계까지 왔고, 다음 확인 포인트는 `FANUC / IGUS` 추가 프리뷰와 페이지 전환 규칙 안정화다.
