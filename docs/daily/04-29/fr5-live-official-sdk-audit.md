# FR5 Live Official SDK Audit

## Summary

- FAIRINO 공식 C# 문서와 현재 `/robotapp` FR5 live 구현을 `base`, `status/readback`, `motion`, `gripper`, `IO` 5개 축으로 비교하는 canonical audit SSOT를 추가했다.
- 이번 audit의 판단 규칙은 `문서 시그니처/의도`와 `실기 evidence`를 분리하는 것이다.
- 결과적으로:
  - `Base`는 mostly `Adapted/Match`
  - `Status/Readback`은 mostly `Adapted` with readback-only gaps
  - `Motion`은 `Adapted`, but arm live is still `Blocked(fault 1/1)`
  - `Gripper`는 `Adapted + partial Match`, but completion readback is still weak/`Divergent`
  - `IO`는 official API exists, but current app contract is effectively `Stubbed/Blocked`

## Why This Was Added

- 기존 source map은 공식 링크와 아키텍처 방향은 잘 정리했지만,
- `공식 예제 코드 대비 현재 코드가 정확히 어디까지 구현됐는지`
- `실기 evidence 기준으로 어디를 믿어야 하는지`
- `다음 구현 우선순위가 무엇인지`
  를 한 문서에서 바로 판단하기는 어려웠다.

그래서 이번 audit 문서는 아래 3가지를 한 곳에 고정한다.

1. 공식 C# 문서의 핵심 API family
2. 현재 repo의 실제 대응 symbol
3. current branch의 실기 truth와 blocker

## Docs Updated

- `docs/ref/product/roadmap/fr5-live-official-sdk-audit.md`
  - 새 canonical audit SSOT
- `docs/ref/product/robots/fairino-fr5-integration-reference.md`
  - source map 문서에서 새 audit SSOT로 연결
- `docs/status/FR5-LIVE-INTEGRATION-ROADMAP.md`
  - current audit summary와 locked backlog를 짧게 반영

## Locked Backlog

- `P0`
  - status/readback comparison completion
  - gripper completion-readback interpretation
  - IO live path gap fixation
- `P1`
  - arm `fault 1/1` unlock condition proof by tiny `MoveJ` QA
- `P2`
  - only after that, joint/TCP operator flow simplification

## Interpretation

- 이 audit은 새 기능 추가가 아니다.
- `어떤 API가 공식 문서와 같고, 어떤 부분이 제품 어댑트이며, 어떤 부분이 아직 미구현인지`를 decision-complete 상태로 잠근 것이다.
- 다음 구현은 이 문서의 `P0 -> P1 -> P2` 순서를 기준으로 진행한다.

## Concrete P0 Findings

- status/readback
  - `LiveFairinoClient`는 `GetSafetyCode`, `GetRobotRealtimeStateSamplePeriod`, `SetRobotRealtimeStateSamplePeriod`, `ReadCoordContext`, `ReadControllerFault`까지 이미 구현했다.
  - `DirectReadbackFairinoClient`와 `FairinoBridgeClient`도 safety/period/context/fault 일부는 넘겨주지만, gripper/status parity는 아직 좁다.
  - 현재 진짜 gap은 `readback-only gripper/config/status surface`와 일부 write-only blocked methods다.
- gripper
  - 실기 command path는 green이지만 `motionFault=1`, `done=0`이 남아 completion-grade confirmation은 아직 약하다.
  - 따라서 `operator contact success`와 `sensor completion success`를 같은 것으로 쓰면 안 된다.
- IO
  - runtime에는 `RobotDo` / `ToolDo` command kind와 gate artifact가 있지만, `IFairinoRobotClient`에 공식 SDK IO API 대응 계약이 없다.
  - 지금은 `공식 문서상 가능한 live IO`와 `현재 제품에서 차단된 UI shell`을 분리해서 보는 게 맞다.
  - `IoPanelController`는 이름과 달리 그리퍼 패널이고, DO/AO operator UI는 없다.
  - AO는 contract/gate/UI 어디에도 아직 실체가 없다.

## Execution Notes

- `P0`
  - 이번 턴 기준으로 문서 SSOT와 gap inventory가 거의 고정된 상태다.
- `P1`
  - 다음 실제 실행 단위는 `J6 +0.5deg @ 5%` tiny MoveJ QA artifact 1회다.
  - 목적은 motion success가 아니라 `fault 1/1` unlock condition evidence를 남기는 것이다.
- `P2`
  - `P1` 해석이 고정되기 전에는 joint/TCP operator UX를 단순화하지 않는다.
