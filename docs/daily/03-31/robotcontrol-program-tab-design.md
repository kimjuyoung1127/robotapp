# 2026-03-31 — Unity 전용 Program 탭 설계

## Summary
- SimMachine의 Program 탭을 그대로 복제하지 않고, Unity다운 `3D 기반 동작 시퀀서 + 교육형 디버거`로 재해석하는 설계를 문서에 반영했다.

## What Changed
- `docs/ref/product/ux/robotcontrol-soft-teaching-pad.md`
  - Unity 전용 Program 탭 상세 설계 추가
  - 블록 종류, 속성 패널, 3D 프리뷰, 검증 바, 초보자/고급 모드 분리 추가
- `docs/ref/product/roadmap/robotcontrol-soft-teaching-pad-v1-backlog.md`
  - Program 탭 최소 버전을 선택 범위에 추가
- `docs/status/PROJECT-STATUS.md`
  - Program 탭 설계 반영

## Decision Notes
- `Coding`, `Graphical`, `Node Graph`는 V1에서 복제하지 않는다.
- `Points`는 일부 채택하고, V1 Program 탭은 `Points + Motion Sequence + Preview` 중심으로 축소한다.
- Unity 강점은 코드 편집기보다 `시각적 동작 시퀀스`와 `실시간 3D 디버그`에 있다.
