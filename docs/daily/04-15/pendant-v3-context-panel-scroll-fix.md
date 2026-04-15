# Pendant V3 Context Panel Scroll Fix

## 목표
- 우측 `ContextPanel` 하단 카드 텍스트가 카드 밖으로 잘리는 문제를 없앤다.
- `상태 / 좌표` 탭 구조는 유지한 채, 세로 overflow를 안전하게 흡수한다.

## 변경
- `Assets/UI/PendantV3/pendant-v3.uxml`
  - `ContextPanel` 내부 카드 스택을 `ContextPanelScroll` `ScrollView`로 감쌌다.
- `Assets/UI/PendantV3/pendant-v3.uss`
  - `rc-context-panel`에 `min-height: 0`
  - `rc-context-scroll` 추가
  - `rc-context-card`에 `flex-shrink: 0`
  - `rc-context-flex` 강제 확장 제거
- `Assets/Scripts/App/RobotControlV3DebugBridge.cs`
  - `ScrollContextPanelToTopForDebug()`
  - `ScrollContextPanelToBottomForDebug()`
  - `GetContextPanelScrollSummary()`

## 원인 정리
- `ContextPanel`은 고정 폭 `320px` 세로 스택인데, 카드 총높이가 viewport보다 커도 스크롤 처리 없이 그대로 쌓였다.
- 그 상태에서 `WhyItMoved`가 `rc-context-flex`로 남는 공간을 잡으려다 하단 카드 텍스트가 눌렸다.
- 결과적으로 카드 문구 자체보다 `overflow 부재 + flex 압축`이 잘림의 직접 원인이었다.

## 검증
- `unityctl check --type compile`: pass
- play + `RobotControlV3`
- `GetContextPanelScrollSummary()`
  - top: `offsetY=0.0`
  - bottom: `offsetY=173.3~266.7`
  - viewport/content height 차이 확인
- visual smoke
  - `Artifacts/V3/status-bottom-controlled-right.png`
    - `다음 행동 추천` 카드 본문 전체 노출 확인
  - `Artifacts/V3/coordinate-bottom-controlled-verified-right.png`
    - `최근 조작 메모` 제목/본문 노출 확인

## 판단
- 우측 패널 텍스트 잘림 문제는 해결로 본다.
- 남은 건 기능 문제가 아니라 스크롤바 시각 polish다.
- 후속에서는 스크롤바 폭/스타일만 손보면 된다.
