# 2026-03-31 — Unity 전용 Application 탭 범위 컷

## Summary
- SimMachine `Application` 탭은 일반 조작 기능이 아니라 산업 확장 앱/공정 패키지로 보고, V1 메인 범위에서는 제외하는 방향으로 문서화했다.

## What Changed
- `docs/ref/product/ux/robotcontrol-soft-teaching-pad.md`
  - Unity 전용 Application 탭 해석 추가
- `docs/ref/product/roadmap/robotcontrol-soft-teaching-pad-v1-backlog.md`
  - `Application 전체`를 제외 항목으로 추가
- `docs/status/PROJECT-STATUS.md`
  - Application 탭 범위 컷 반영

## Decision Notes
- `Tool App`, `Process Package`는 조작기보다 산업 확장 기능 성격이 강하다.
- V1은 `조작 + 상태 + preview + 안전 안내`에 집중해야 하므로 Application은 제외가 맞다.
