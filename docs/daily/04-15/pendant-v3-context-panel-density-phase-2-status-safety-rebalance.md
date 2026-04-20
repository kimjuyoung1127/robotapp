# Pendant V3 Context Panel Density Phase 2

## 목표
- 오른쪽 컬럼 상시 카드 수를 더 줄인다.
- `StatusCard` 안에 짧은 안전 요약을 추가하고, `SafetyDiagnostics`는 warning/fault일 때만 보이게 축소한다.

## 오늘 변경
- `status-card.uxml/.uss`에 `StatusSafetySummary` 추가
- `StatusCardController`가 preview state 기준 안전 요약 타이틀/본문/톤을 갱신
- `SafetyDiagnosticsController`는 normal 상태에서 `SafetyDiagnosticsHost`를 숨기고, warning/fault에서만 표시

## 기대 효과
- 정상 상태에서는 오른쪽 컬럼에서 `SafetyDiagnostics` 카드가 빠져 상시 카드 수가 줄어든다.
- 안전 관련 핵심 문구는 `StatusCard` 상단에 남겨 정보 손실을 줄인다.

## 검증
- `unityctl check --type compile`: pass
- `RobotControlV3DebugBridge.GetPanelControllerSummary()`:
  - `status.summaryTitle=정상 대기`
  - `safety.hostHidden=True`
- `BtnPresetFault` actual UITK click:
  - `status.summaryTitle=Fault 복구 우선`
  - `safety.hostHidden=False`
  - `overlayVisible=True`
- `ConnectedServoOff` 복귀:
  - `status.summaryTitle=정상 대기`
  - `safety.hostHidden=True`
