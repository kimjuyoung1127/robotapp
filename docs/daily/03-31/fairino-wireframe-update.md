# 2026-03-31 — FAIRINO 공식 기능 1:1 와이어프레임 업데이트

## Summary
- 실제 SimMachine 캡처와 공식 문서 기능을 1대1로 매칭해 Unity `RobotControl`의 Desktop/Tablet 와이어프레임 계획을 문서에 반영했다.

## What Changed
- `docs/ref/product/ux/robotcontrol-soft-teaching-pad.md`
  - 공식 기능 1:1 매칭 표 추가
  - Desktop 와이어프레임 추가
  - Tablet 와이어프레임 추가
  - 패널별 버튼 이름과 라벨 표준 추가
- `docs/ref/product/robots/fairino-simmachine-screen-structure-draft.md`
  - 실제 캡처 기준 검증 메모 추가
- `docs/status/PROJECT-STATUS.md`
  - 이번 와이어프레임 구체화 내용을 반영

## Decision Notes
- 공식 문서의 구조는 유지하되, SimMachine처럼 기능이 여러 툴바에 과하게 흩어지지 않도록 재구성한다.
- Unity판 V1은 `중앙 3D + 좌측 작업 패널 + 상단 상태 바 + 하단 보조 모듈 바`를 기본 구조로 둔다.
- 공식 기능 이름은 진단/설정 쪽에서 유지하고, 사용자-facing 버튼은 쉬운 한국어 라벨로 전환한다.
