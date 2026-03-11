# Module Log - Phase 5C Common Input Visualization

## Summary
- `JointInputRail` + `JointInputValidator`를 추가해 2DOF 숫자 입력과 슬라이더 양방향 동기화를 고정했다.
- `JointHighlightRing`, `LinkHighlighter`, `EndEffectorTrail`, `TargetMarkerVisual`을 추가해 공통 시각화 인프라를 분리했다.
- `AppController`, `AppUiBinder`, `RobotRenderer`를 최소 확장해 step flag 기반 토글과 public facade 계약을 유지했다.

## Verification
- `dotnet build KineTutor3D.Runtime.csproj`
- Unity EditMode `JointInputValidatorTests` 3/3 passed
- Unity PlayMode `Phase5CommonVisualsSmokeTests` 4/4 passed

## Notes
- Unity 6 built-in font 경로 변경으로 `Arial.ttf` fallback 예외가 발생해 `UiRuntimeStyle.ResolveFont()`를 `LegacyRuntime.ttf` 우선으로 수정했다.
- `showJointHighlight=false` step에서 입력 변화가 링 하이라이트를 다시 켜지 않도록 `AppController`에서 focus 요청을 gate 했다.
