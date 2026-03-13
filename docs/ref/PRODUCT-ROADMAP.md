# KineTutor3D Product Roadmap

Version: 1.2.0  
Last Updated: 2026-03-12 (KST)

## Purpose

이 문서는 제품 우선순위와 릴리스 게이트를 잠그는 root canonical roadmap 문서다. 세부 백로그, 게이트, 플랜 변경 SOP는 하위 로드맵 문서에서 관리하고, 여기에는 현재 우선순위와 읽기 경로만 유지한다.

## Locked Priorities

1. P0는 `Beginner Lesson 0~3`, `Guided Lesson 중심 UX`, `Home / Continue Hub`, `Sandbox polish`, `tablet-first 사용성`, `비공개 자료의 UI 재구성 정책`이다.
2. 차기 UX 완성 순서는 `Home / Continue Hub -> Sandbox polish -> Instructor Mode`다.
3. 로봇 확장 순서는 `2DOF -> SCARA -> 3DOF -> 6DOF`다.
4. `snapshot lite`는 `replay / history / constraint preview`보다 먼저 구현한다.
5. LLM은 deterministic runtime 위의 설명층으로만 추가한다.
6. 최종 모바일 배포는 `Android Tablet -> iPad -> WebGL -> Phone 제한형` 우선순위를 따른다.
7. 플랜 변경 처리는 `leaf -> root canonical -> downstream -> board -> logs` 순서를 따른다.
8. Phase 5G 이후 근접 P0 구현 순서는 `Home / Continue Hub -> Sandbox polish -> resume / session context -> tablet 4DOF input usability -> snapshot lite`를 따른다.

## Timeline Summary

- 30일: 제품 방향/IA/Home 허브 정책/Sandbox MVP 기준/비공개 자료 정책 고정
- 60일: Home / Continue Hub, Sandbox polish, resume context, snapshot lite 정리
- 90일: Instructor Mode, 로봇 확장, 태블릿 최적화, 배포 준비

## Change Summary

1. 로드맵 문서를 root summary 문서로 축소했다.
2. 세부 백로그와 릴리스 게이트를 `docs/ref/product/roadmap/` 아래로 분기했다.
3. 모바일 릴리스 체크리스트를 roadmap leaf 문서로 추가했다.
4. 현재 있는 기능 / 없는 기능 / 우선 추가 기능을 빠르게 보는 `current-feature-checklist.md`를 추가했다.
5. `Beginner Lesson 0~3`를 roadmap P0 잠금 결정과 timeline에 반영했다.
6. 플랜 변경 SOP를 roadmap leaf 문서 기준으로 정리했다.
7. `Home / Continue Hub`, `Sandbox polish`, `snapshot lite`를 차기 P0 우선순위로 승격했다.

## Read Next

- [milestone-backlog.md](./product/roadmap/milestone-backlog.md)
- [current-feature-checklist.md](./product/roadmap/current-feature-checklist.md)
- [asset-sourcing-checklist.md](./product/roadmap/asset-sourcing-checklist.md)
- [release-gates.md](./product/roadmap/release-gates.md)
- [mobile-release-checklist.md](./product/roadmap/mobile-release-checklist.md)
- [workspace-envelope-algorithm-memo.md](./product/roadmap/workspace-envelope-algorithm-memo.md)
- [robot-template-expansion.md](./product/robots/robot-template-expansion.md)
- [urdf-reference-collection.md](./product/robots/urdf-reference-collection.md)
- [interactive-matrix-viz-design-reference.md](./product/ux/interactive-matrix-viz-design-reference.md)

## Downstream Sync

- `docs/status/PROJECT-STATUS.md`
- `docs/status/PHASE-EXECUTION-BOARD.md`
- `ai-context/master-plan.md`

## Branching Rule

1. 이 문서에는 세부 태스크 목록과 예외 케이스를 길게 적지 않는다.
2. milestone 세부는 `milestone-backlog.md`에서만 관리한다.
3. 플랜 변경 SOP와 릴리스 기준은 `release-gates.md`에서만 관리한다.
