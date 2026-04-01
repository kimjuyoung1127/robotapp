# 2026-03-31 — Unity 전용 Status 탭 설계

## Summary
- SimMachine의 Status 탭을 그대로 복제하지 않고, Unity다운 `상태 요약 + 최근 이벤트 + 세션 리포트` 중심으로 재해석하는 설계를 문서에 반영했다.

## What Changed
- `docs/ref/product/ux/robotcontrol-soft-teaching-pad.md`
  - Unity 전용 Status 탭 설계 추가
- `docs/ref/product/roadmap/robotcontrol-soft-teaching-pad-v1-backlog.md`
  - Status 탭 최소 버전을 선택 범위에 추가
- `docs/status/PROJECT-STATUS.md`
  - Status 탭 설계 반영

## Decision Notes
- `Log`와 `Query`는 운영자용 도구 성격이 강하다.
- V1은 복잡한 로그 테이블이나 파형 분석보다 `학습형 상태 요약`과 `복구 안내`가 우선이다.
