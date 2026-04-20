# Pendant V3 Context Panel Phase 3 Tab Split

## 목표
- 오른쪽 컬럼을 `상태` / `좌표` 탭으로 나눈다.
- 상시 카드 과밀을 구조적으로 줄인다.

## 이번 페이즈 범위
- `상태` 탭: `StatusCard`, `SafetyDiagnostics`, `ActionHint`
- `좌표` 탭: `CoordStrip`, `WhyItMoved`
- `SafetyDiagnostics`, `WhyItMoved`는 각 탭 visibility를 존중하도록 연동

## 비목표
- 탭 상태 persistence
- tablet bottom sheet 구조 변경
- StatusCard/SafetyDiagnostics 추가 통합
