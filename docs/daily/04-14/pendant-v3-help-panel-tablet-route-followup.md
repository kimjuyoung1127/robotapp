# Pendant V3 Help Panel Tablet Route Follow-Up

## Date
- 2026-04-14 (KST)

## Summary
- `HelpPanelController` 원복 버그를 막기 위해 desktop/tablet 도움말 활성 상태를 분리했다.
- tablet help 진입 경로로 `BottomTabHelp`를 추가했고, `HelpSheetHost` authored 자리까지 연결했다.
- `WhyItMovedController`는 기존 `StatusCardController`에서 분리한 상태를 유지했다.

## Verification
- `unityctl check --type compile`
  - pass
- desktop play smoke
  - actual `NavHelp` click -> `HelpPanelHost` visible
  - `WorkTabBar` class에 `rc-hidden` 반영
  - `WhyItMovedSummary` text 갱신 확인
- authored route check
  - `pendant-v3.uxml` 기준 `BottomTabHelp`, `HelpSheetHost` 존재 확인
- tablet play smoke
  - actual `BottomTabHelp` click -> `HelpSheetHost` visible + childCount=1
  - `BottomSheetTitle=BottomSheet · 도움말`
  - actual `BottomTabTcpJog` click -> `BottomSheetTitle=BottomSheet · TCP`
- help copy polish
  - preview state + `coord/increment/speed` 기반으로 도움말 문구 1차 보강

## Self Review
- 역할 경계
  - 도움말은 `HelpPanelController`, 최근 조작 메모는 `WhyItMovedController`가 전담하게 유지했다.
- 남은 리스크
  - tablet actual 진입/복귀 smoke는 닫았지만, `first-run guide`와 `help-panel` 탭별 세분화는 더 필요하다.
  - `2D` 전체를 done으로 보려면 help-panel/WhyItMoved policy 깊이와 popup 정책 연동을 더 다듬어야 한다.
