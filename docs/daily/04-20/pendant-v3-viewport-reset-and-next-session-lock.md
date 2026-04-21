# Pendant V3 Viewport Reset And Next Session Lock

## Summary

- 오늘 viewport 관련 구현은 **채택 없이 rollback**으로 정리했다.
- 현재 기준선은 `8549b09`이며, 이 커밋 자체가 이미 `MainSplitHandle + ViewportHost` 별도 패널 구조를 포함한다.
- 그래서 화면에 계속 보이던 별도 viewport는 "오늘 수정이 남은 것"이 아니라 **baseline 원구조**였다.

## What Happened

1. `ViewportHost`를 `WorkPanel` 안으로 옮기는 실험을 반복했다.
2. `Base축 / Tool축 / 궤적` 툴바를 viewport 안/밖으로 재배치하는 실험도 했다.
3. `RenderTexture`, `RawImage bridge`, camera framing 같은 가시화 접근도 시도했다.
4. 하지만 어느 시점에도 "메인패널에 로봇이 자연스럽게 표시된다"는 사용감 기준을 만족하지 못했다.
5. 그래서 오늘 viewport 구조 실험은 전부 버리고 baseline으로 복귀시켰다.

## Trial And Error To Remember

- `8549b09` 이전으로만 내리면 viewport 실험 전으로 돌아갈 줄 알았는데, 실제로는 `ViewportHost` 자체가 더 초기 V3 scaffold부터 이미 있었다.
- play 중 runtime 화면을 보고 "원복이 안 됐다"고 판단했지만, 나중에 확인해보니 git/file 기준으로는 이미 원복된 상태였다.
- `Always Start From Onboarding` 훅 때문에 현재 열어둔 씬에서 play가 안 시작되는데, 이걸 모르고 V3 씬 자체가 고장난 것처럼 보이는 구간이 있었다.
- `RobotControlV3DebugBridge`가 현재 baseline 타입들과 맞지 않아 stale compile error를 만들었고, 이게 판단을 더 꼬이게 했다.

## Current Truth

- baseline: `8549b09`
- V3 scene asset exists: `Assets/Scenes/RobotControlV3.unity`
- normal play start: `Onboarding`부터 시작
- QA direct play: `Always Start From Onboarding`를 끄고 V3 씬을 직접 열면 `SceneId=7`에서 play 가능
- 현재 목표 기능은 **아직 미구현**
  - 메인패널 안에 로봇을 자연스럽게 표시
  - 컨트롤 패널과 로봇 패널의 경계를 사용자 의도대로 고정

## Next Session Lock

다음 세션에서는 구현 전에 아래 3개를 먼저 문장으로 잠그고 시작한다.

1. 로봇을 **어느 패널에 표시할지** 먼저 확정한다.
2. `Base축 / Tool축 / 궤적` 패널이 로봇 패널과 **같은 패널인지, 별도 패널인지** 먼저 확정한다.
3. 세션 중간에 `ViewportHost 유지 ↔ 제거`, `내장형 ↔ 별도형`을 다시 뒤집지 않는다.

## Recommended Next Session Order

1. `Onboarding -> V3 버튼 -> RobotControlV3` 자연 진입 경로 확인/복구
2. 로봇 표시 대상 패널을 문서로 1회 잠금
3. 그 패널에만 로봇 가시화 구현
4. 툴바/좌표/오버레이는 그 다음 분리/배치

## Verification Notes

- `unityctl check --type compile --json`: pass
- QA direct play after temporary play-start override: `SceneId=7` confirmed
- `Always Start From Onboarding` restored after QA
