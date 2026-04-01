# 2026-03-31 — RobotControl 씬 계층 구조 설계 + SimMachine 한글 점검

## Summary
- `RobotControl.unity`를 새 authored-first 구조로 재조립하기 위한 씬 계층 설계 문서를 추가했다.
- SimMachine WebApp이 구조적으로 한국어 지원이 가능한지 패키지 기준으로 점검 메모를 남겼다.

## What Changed
- `docs/ref/product/ux/robotcontrol-scene-hierarchy.md`
  - 권장 씬 계층
  - UI 하위 계층 상세
  - 기존 컴포넌트 매핑
  - Desktop/Tablet 레이아웃 규칙
  - 마이그레이션 순서
- `docs/ref/product/robots/fairino-simmachine-screen-structure-draft.md`
  - `ko.json`, `login.html`, `login.js` 기준 한국어 지원 구조 메모 추가
- `docs/status/PROJECT-STATUS.md`
  - 이번 설계/점검 내용을 상태 문서에 반영

## Key Notes
- 지금은 full rewrite보다 `구조적 재조립`이 맞다.
- SimMachine은 코드상 `한국어` 언어 항목과 `set_sys_language` 경로를 가지고 있다.
- 현재 글자 깨짐은 한국어 미지원이 아니라 언어 적용/폰트/렌더링 쪽 문제일 가능성이 높다.
