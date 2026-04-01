# 2026-03-31 — FAIRINO 소프트 티칭패드 기획 정리

## Summary
- FAIRINO 공식 티칭패드/WebApp 기능을 현재 `robotapp2` `RobotControl` 구현과 대조하는 한국어 기능 매트릭스를 추가했다.
- `초보자도 쉽게 쓰는 나만의 티칭패드`를 목표로 한국어 UX/구현 계획 문서를 추가했다.
- 이번 문서화는 제조사 복제 범위와 Unity 차별화 범위를 동시에 정리하는 목적이다.

## What Changed
- `docs/ref/product/robots/fairino-teaching-pad-feature-matrix.md`
  - 제조사 기능을 `있음 / 부분 구현 / 없음 / 실기 검증 필요`로 분류했다.
  - 현재 강한 영역, 비어 있는 영역, 실기 검증 필요 영역을 나눴다.
  - `공식 출처`, `대표 API`, `공식 제약 메모`를 추가해 문서 근거를 강화했다.
  - 우선순위와 추가 가치 기능을 정리했다.
- `docs/ref/product/ux/robotcontrol-soft-teaching-pad.md`
  - 한국어 우선, 초보자 우선, preview first, safe by default 기준을 고정했다.
  - authored-first, 디자인 토큰 공유, 태블릿 대응, 기능별 컴포넌트 분리 구조를 정리했다.
  - 공식 문서 기준선과 공식 제약 기반 필수 팝업 규칙을 추가했다.
  - Unity 차별화 포인트와 단계별 구현 순서를 정리했다.
- `docs/status/PROJECT-STATUS.md`
  - 위 기획 문서 추가 사실을 현재 상태 요약에 반영했다.

## Decision Notes
- 목표는 제조사 티칭패드의 픽셀 복제가 아니라 `교육형 + 운영형 소프트 티칭패드`다.
- 실기 명령은 `preview -> 안내 -> 확인 -> 실행` 흐름을 기본으로 본다.
- `RobotControl`은 이후 `쉬운 조작`, `관절`, `TCP`, `티칭`, `진단` 중심 구조로 확장하는 것이 적합하다.

## Follow-up
- 다음 단계는 기능 매트릭스 기준으로 `MVP`, `차별화`, `운영 확장`을 분리해 실제 구현 backlog로 떨어뜨리는 작업이다.
- 특히 `초보자 안내 팝업`, `태블릿 레이아웃`, `ghost preview`, `위험 사전 경고`가 초기 제품 가치를 크게 올릴 후보로 정리되었다.
