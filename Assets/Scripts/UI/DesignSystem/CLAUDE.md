# UI/DesignSystem

UI 디자인 시스템과 스타일 프리미티브.

## 포함 대상
- tokens, typography, icons
- runtime style bridge
- component/layout factories

## 규칙
1. `RobotControlV2` 전용 색상/치수/상태 표현은 `UIDesignTokens.RobotControlV2`에만 정의한다.
2. `RobotControl` 계열 시각 변경이 반복되면 셸 로컬 상수로 남기지 말고 토큰으로 승격한다.
3. authored-first 패널이 소비해야 하는 값은 토큰에서 읽을 수 있어야 한다.
