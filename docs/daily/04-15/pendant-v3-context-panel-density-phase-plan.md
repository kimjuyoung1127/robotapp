# Pendant V3 Context Panel Density Phase Plan

## 배경
- 오른쪽 컬럼이 `CoordStrip`, `StatusCard`, `SafetyDiagnostics`, `ActionHint`, `WhyItMoved`를 동시에 노출하고 있어서 겹침처럼 답답하게 보인다.
- 구조상 겹침보다는 상시 노출 과밀 문제로 판단했다.

## 오늘 잠근 내용
- 원인/해결안 문서를 `context-panel-density-remediation-plan.md`로 추가
- 작은 페이즈 기준으로 진행
  1. `CoordStrip` 접기/토글화
  2. 상태 카드 재배치
  3. 우측 컬럼 탭 분리

## 오늘 실행 단위
- `Phase 1`인 `CoordStrip` 접기/토글화 구현

## 결과
- `coord-strip.uxml`에 `BtnCoordStripToggle`, `CoordStripBody` 추가
- `StatusCardController`가 접기/펼치기 토글 상태를 관리
- compile pass
- actual UITK click smoke에서 버튼 텍스트가 `접기 -> 펼치기`로 바뀌는 것 확인
