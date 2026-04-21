# Pendant V3 TCP Overlay To Aux Panel

## Summary

- TCP 조그 탭에서 `CartesianArrowsOverlayHost`가 메인 `RobotStage`를 가리던 문제를 막기 위해 오버레이 호스트를 `ViewportHost` 공용 스크롤 쪽으로 이동했다.
- 특히 `Z / RX / RY / RZ` 조작은 메인 로봇 표시를 덮기 쉬워서, 메인 패널에 두지 않는 방향으로 기준을 다시 잠갔다.

## Updated Files

- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `docs/ref/product/pendant-v3/README.md`
- `docs/ref/product/pendant-v3/feature-3d-viewport.md`
- `docs/ref/product/pendant-v3/shell-layout.md`
- `docs/ref/product/pendant-v3/progress-checklist.md`

## Intent

- 메인 `RobotStage`는 로봇, 프레임, 트레일, 고스트, 바닥 격자, 선택 기즈모처럼 로봇에 직접 붙는 시각 정보만 보여준다.
- TCP 보조 조작 UI는 `ViewportHost` 보조 패널 안 공용 스크롤로 보내서 패널끼리 겹치지 않게 유지한다.
- 이후 `predicted path`, `collision`, `workspace boundary`를 붙일 때도 같은 원칙을 유지한다.

## Verification

- `unityctl check --type compile`
  - pass
- direct V3 play QA는 에디터의 `Always Start From Onboarding` / `playModeStartScene` 상태가 세션 중 흔들려 신뢰도 낮았다.
- 대신 코드 기준 확인:
  - `pendant-v3.uxml`에는 `CartesianArrowsOverlayHost`가 `ViewportPanelScroll` 하위로만 존재
  - `RobotStage` 쪽에는 더 이상 TCP 조작 호스트를 두지 않음
