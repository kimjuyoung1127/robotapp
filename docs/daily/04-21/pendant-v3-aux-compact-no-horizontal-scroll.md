# Pendant V3 Aux Compact No-Horizontal Scroll

## Summary
- 보조패널과 오른쪽 패널은 가로 스크롤 없이 세로 스크롤만 사용하도록 잠갔다.
- 버튼 잘림은 패널 폭 확장이 아니라 내부 compact/wrap 레이아웃으로 해결했다.
- TCP/Cartesian/Joints/Point/Easy/Context 카드의 최소폭과 행 구조를 좁은 보조패널 기준으로 재정리했다.

## Changes
- TCP 조그는 `축+값+단위` 상단, `- / +` 하단의 2줄 행으로 변경했다.
- Cartesian 3D 방향 조작도 축 값과 버튼 행을 분리했다.
- Joint 조그는 `J축+입력+값`, `슬라이더`, `- / +` 버튼 행으로 분리했다.
- Point/Easy/Coord/Status/Safety 카드는 `min-width: 0`, `max-width: 100%`, wrap/compact 규칙을 적용했다.
- Viewport toolbar copy는 `Base / Tool / Path / Ghost / Bound / Coll / Cam` 기준으로 축약했다.
- `RobotControlV3DebugBridge.GetAuxLayoutSummaryForDebug()`를 추가해 스크롤 모드, content/viewport 폭, horizontal scroller 표시 여부, clipped count를 확인할 수 있게 했다.

## Self Review
- 역할 경계: 통과. 제품 로직은 바꾸지 않고 UI 구조/스타일과 App debug bridge만 수정했다.
- 메인/보조 경계: 통과. 메인 `RobotStage`에는 조작 UI를 되돌리지 않았다.
- 가로 스크롤 정책: 통과. `ViewportPanelScroll`, `ContextPanelScroll`은 vertical-only 유지다.
- 하드코딩 리스크: 주의. compact 수치가 USS에 남아 있으나 기존 V3 스타일 파일의 layout constant 성격이며, 색/토큰 하드코딩 증가는 최소화했다.
- 검증 리스크: `Always Start From Onboarding` 때문에 full user-flow play screenshot은 별도 검증이 필요하다.

## Verification
- `unityctl check --type compile --json`: pass
- Unity 재시작 후 새 PID `28380`에서 `GetAuxLayoutSummaryForDebug` callable 노출 확인.
- TCP: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextClipped=0`
- Joint: `viewportHorizontalVisible=False`, `viewportClipped=0`
- Point: `viewportHorizontalVisible=False`, `viewportClipped=0`, `contextClipped=0`
- Easy: `viewportHorizontalVisible=False`, `viewportClipped=0`
- Scroll share: viewport/context 모두 `0.88~0.90` 범위로 2/3 이상.
- Screenshot: `Artifacts/robotcontrolv3-compact-no-clipping-v25.png`

## Notes
- `git diff --check`는 기존 `Assets/Scenes/RobotControlV3.unity` trailing whitespace 때문에 실패한다.
- 이번 작업에서는 해당 씬 trailing whitespace를 범위 밖으로 보고 건드리지 않았다.
