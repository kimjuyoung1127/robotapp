# Current Feature Checklist

## Purpose
- 현재 있는 기능, 아직 없는 기능, 우선 추가할 기능을 한 문서에서 빠르게 확인할 수 있게 정리한다.

## Parent Doc
- [PRODUCT-ROADMAP](../../PRODUCT-ROADMAP.md)

## When To Read
- 현재 제품 상태를 빠르게 파악하거나, 구현 우선순위를 다시 정리할 때

## Locked Decisions
- 이 문서는 `현재 구현 상태 + 다음 우선순위`를 쉬운 체크리스트로 보여준다
- 세부 스펙은 leaf UX/content/roadmap 문서가 source of truth이고, 이 문서는 빠른 운영 인덱스 역할을 한다
- `우선 추가할 기능`은 현재 roadmap 우선순위와 동일한 방향을 유지한다
- Phase 5 P0는 `기반층 -> 공통 컴포넌트 -> explanation layer -> Beginner Lesson 0~3` 순서로 구현한다

## Open Questions
- Progress와 Challenge를 어느 시점에 현재 checklist의 우선 기능으로 올릴지

## Downstream Sync
- `docs/ref/PRODUCT-ROADMAP.md`
- `docs/status/PROJECT-STATUS.md`
- `ai-context/master-plan.md`

## Last Updated
- 2026-03-11 (KST)

## 현재 있는 기능
- [x] Onboarding 시작/건너뛰기 흐름
- [x] `Boot -> Onboarding -> Main` 씬 분리
- [x] Guided Lesson 기본 step 진행 구조
- [x] gate 기반 Next/Skip 흐름
- [x] 2DOF 로봇 학습 템플릿
- [x] joint slider 조작
- [x] `theta` read-only, `d/a/alpha` 편집 가능한 DH 테이블
- [x] `A1 / A2 / T02 / EE pose` 표시
- [x] 3D 로봇 렌더링과 frame 시각화
- [x] tooltip / glossary / toast / focus highlight
- [x] 방문 여부, 마지막 step, reduced motion 로컬 저장
- [x] 전역 scene navigation
- [x] EditMode / PlayMode 테스트 기반

## 지금 없는 기능
- [ ] Beginner Lesson 0~3 초보자 진입 트랙
- [ ] Robot Library 실제 화면 구현
- [ ] Sandbox 실제 화면 구현
- [ ] Instructor Mode 실제 화면 구현
- [ ] joint 숫자 직접 입력
- [ ] `Why It Moved` 실제 패널/설명 기능
- [ ] pose snapshot 저장
- [ ] replay / compare / motion history
- [ ] constraint / workspace / singularity 시각화
- [ ] pick foundation 실제 흐름
- [ ] 다중 로봇 로딩 구조의 런타임 구현
- [ ] SCARA / 3DOF / 6DOF 실제 전환
- [ ] Progress 화면
- [ ] Challenge / Assessment
- [ ] LLM 설명층 연결
- [ ] Android/iPad 배포 준비 완료 상태

## 우선 추가할 기능
### P0
- [ ] runtime snapshot + update cause foundation
- [ ] track-aware step 저장/복귀
- [ ] joint 숫자 직접 입력
- [ ] slider + numeric input 동기화
- [ ] joint highlight
- [ ] trail / target marker 공통 인프라
- [ ] `Why It Moved`
- [ ] Beginner Lesson 0~3

### P1
- [ ] robot metadata 기반 Robot Library
- [ ] Sandbox MVP
- [ ] snapshot 저장
- [ ] replay / compare
- [ ] constraint preview
- [ ] SCARA 추가
- [ ] Instructor demo mode

### P2
- [ ] pick foundation
- [ ] Progress / assessment / challenge
- [ ] Android tablet internal build
- [ ] LLM teaching layer

## Quick Read
- 지금 제품은 `2DOF 로봇의 DH/FK를 step-by-step으로 배우는 Guided Lesson MVP`다.
- 다음 구현 순서는 `기반 상태 계약 -> track-aware step -> joint input/highlight/trail/target -> why-it-moved -> Beginner Lesson 0~3`이고, 그 다음 순서는 `Robot Library + Sandbox + SCARA`다.
