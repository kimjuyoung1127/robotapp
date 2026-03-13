# 마스터 플랜

최종 업데이트: 2026-03-12 (KST)

## 현재 기준선
- 완료: Phase 0~5 (5A~5G Complete)
- 현재 baseline:
  - `2DOF + SCARA` runtime
  - `Home / Continue Hub`
  - `math_readiness` track
  - `Beginner Lesson 0~3`
  - `Why It Moved`
  - `Robot Library MVP`
  - `Sandbox.unity` + `snapshot lite` + `Sandbox actions`
  - curated asset subset + vendor fallback
- 현재 진행:
  - `Product Docs Governance`
  - `Phase 6 CI/CD`는 hold

## 근접 P0 순서
1. `Sandbox polish 마감`
2. `tablet 4DOF input usability`
3. `asset subset Git tracking`
4. `replay / compare / motion history`
5. `constraint / workspace / singularity preview`

## 후속 P1 순서
1. `replay / compare / motion history`
2. `constraint / workspace / singularity preview`
3. `Instructor demo mode`
4. `3DOF template`
5. `6DOF demo-first`
6. `URDF Import`

## P2 이후
1. `pick foundation`
2. `Progress / assessment / challenge`
3. `LLM teaching layer`
4. `Android tablet internal build`
5. `CI/CD 실주행`

## 현재 스프린트 목표
- `Home / Continue Hub`, `resume / session context`, `snapshot lite`, `math_readiness`를 현재 baseline으로 유지
- Sandbox를 실제 학습 공간으로 느껴지게 polish 마감
- SCARA 4DOF 입력을 태블릿 기준에서 usable 상태로 만들기
- replay 전에 `snapshot lite`를 안정화하고 샌드박스 버튼/패널 구조를 정리

## 운영 규칙
- phase 상태는 `docs/status/PHASE-EXECUTION-BOARD.md`
- canonical product docs 상태는 `docs/status/PRODUCT-DOC-BOARD.md`
- 현재 구현 범위와 다음 우선순위는 `docs/ref/product/roadmap/current-feature-checklist.md`
- 문서 drift가 보이면 실제 코드/테스트 상태를 우선
