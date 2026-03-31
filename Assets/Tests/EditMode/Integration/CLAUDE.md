# EditMode/Integration

컴포넌트 결합과 런타임 wiring을 보는 EditMode 테스트.

## 포함 대상
- UI 패널 bind/update
- coordinator/service 상호작용
- prefab/resource 로드와 lightweight runtime 결합

## 주의
- private reflection 의존은 최소화
- 씬 전체 스캔 성격이면 `Validation/`으로 이동
