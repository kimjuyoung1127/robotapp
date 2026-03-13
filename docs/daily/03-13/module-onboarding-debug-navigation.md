# Onboarding Debug Navigation

## Summary
- `Onboarding.unity`에서 상단 전역 네비게이션을 숨기지 않도록 조정했다.
- QA/디버그 시 `Onboarding / Home / Main / Robot Library / Sandbox`로 즉시 이동할 수 있게 했다.

## Runtime Changes
- `Assets/Scripts/UI/OnboardingManager.cs`
  - Onboarding에서도 `SceneNavigationBar`를 비활성화하지 않도록 변경
  - 기존 legacy nav 숨김 로직 제거
- `Assets/Scenes/Onboarding.unity`
  - `SceneNavigationBar.hideOnOnboarding` 값을 `false`로 변경

## Notes
- 제품 기본 흐름은 여전히 `Boot -> Onboarding -> Home/Main` 기준이다.
- 이번 변경은 QA/디버깅 가속을 위한 전역 이동 수단 제공 목적이다.
