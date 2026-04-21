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
- 2026-03-13 (KST)

## 현재 있는 기능
- [x] Onboarding 시작/건너뛰기 흐름
- [x] `Boot -> Onboarding -> Main` 씬 분리
- [x] `Boot -> RobotLibrary` 재진입 흐름
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
- [x] runtime snapshot + update cause foundation
- [x] track-aware step 저장/복귀 (pre_kinematics / core_kinematics)
- [x] joint 숫자 직접 입력 + slider/numeric 양방향 동기화
- [x] joint highlight (ring + link emission)
- [x] trail / target marker 공통 인프라
- [x] `Why It Moved` 패널 (WhyItMovedState/Formatter/Panel)
- [x] Beginner Lesson 0~3 초보자 진입 트랙 (BeginnerLessonFactory, BeginnerLeftPanel, CompareModePanelHelper, TargetFeedbackPanel)
- [x] Robot Library MVP 화면 구현
- [x] SCARA template/metadata 기반 첫 다중 로봇 활성화
- [x] Sandbox 씬 baseline + Robot Library sandbox 라우팅
- [x] resume / session context 저장/복귀
- [x] snapshot lite 저장/불러오기/빠른 비교
- [x] `math_readiness` 트랙 + `MathReadinessPanel`
- [x] UI Design System foundation + 4개 핵심 패널 2차 리팩터링
- [x] marker/icon curated subset 경로 + vendor fallback 해석
- [x] MathReadiness 정답/오답 색상 + 아이콘 시각 피드백
- [x] MathReadiness 질문 진행 뱃지 ("Q1/2")
- [x] MathReadiness 워밍업/본문제 시각 분리 (섹션 라벨 + 디바이더)
- [x] MathReadiness 피드백 fade-in 애니메이션
- [x] MathReadiness 적응형 힌트 (2회 오답 코치 힌트, 3회 오답 정답 하이라이트)
- [x] MathReadiness 컨셉별 테마 색상 (accent stripe)
- [x] WhyItMovedPanel 방향 화살표 아이콘 (수학모드)
- [x] 모드 기반 패널 격리 (HideAllContentPanels → Apply{Mode}Visibility 패턴)
- [x] MathReadiness 학습 셸 정리 (좌측 3블록, 워밍업 선행 노출, 우측 패널 숨김, 하단 컨트롤 바 재정렬)
- [x] MathReadiness 조작 우선 흐름 (목표 각도 도달 후 확인 질문 노출)
- [x] MathReadiness 3D 각도 기준선 (0°/90°/180° reference marker)
- [x] Onboarding 전역 QA navigation fallback (`OnboardingDebugNav`)
- [x] Robot Library 3D showroom 초기 3대 프리뷰 (`2DOF RR + SCARA + 6DOF placeholder`)
- [x] non-`realvirtual` vendor source 아카이브화 + curated runtime 자산을 `Assets/Runtime/*`로 재배치
- [x] FAIRINO FR5 RobotControl console baseline (`RobotControl.unity` 씬 + Build Settings index 6)
- [x] SceneCameraDirector: Main/Sandbox/RobotControl/Onboarding/Home 메인 카메라 중앙 관리
- [x] FR5 dual prefab 경로 분리 (showroom preview vs. control)
- [x] RobotLibrary FR5 CTA 확장: Robot Control 진입점 연결
- [x] IVisibilityControllable 인터페이스 + 18개 패널 적용
- [x] OnboardingViewBuilder 추출 (OnboardingManager → ViewBuilder 패턴)
- [x] UIDesignTokens 토큰 마이그레이션 (하드코딩 색상/fontSize → 토큰 참조)

## 지금 없는 기능
- [ ] Sandbox MVP polish 마감 (panel overlap 제거 + 버튼/아이콘 가독성 + exit clarity)
- [ ] tablet 기준 4DOF joint rail 최적화
- [ ] Instructor Mode 실제 화면 구현
- [ ] replay / compare / motion history
- [ ] constraint / workspace / singularity 시각화
- [ ] pick foundation 실제 흐름
- [ ] 3DOF / 6DOF 실제 전환
- [ ] Robot Library showroom 후속 프리뷰 확장 (`FANUC`, `IGUS`) + 페이지/캐러셀 규칙
- [ ] Progress 화면
- [ ] Challenge / Assessment
- [ ] LLM 설명층 연결
- [ ] Android/iPad 배포 준비 완료 상태

## 우선 추가할 기능
### P0
- [x] runtime snapshot + update cause foundation (Phase 5 — Complete)
- [x] track-aware step 저장/복귀 (Phase 5 — Complete)
- [x] joint 숫자 직접 입력 (Phase 5 — Complete)
- [x] slider + numeric input 동기화 (Phase 5 — Complete)
- [x] joint highlight (Phase 5 — Complete)
- [x] trail / target marker 공통 인프라 (Phase 5 — Complete)
- [x] `Why It Moved` (Phase 5 — Complete)
- [x] Beginner Lesson 0~3 (Phase 5 — Complete)
- [x] Robot Library MVP (Phase 5 — Complete)
- [ ] Sandbox MVP polish 마감
- [ ] tablet 기준 4DOF joint rail 최적화
- [ ] asset subset Git tracking 마무리

### P1
- [ ] replay / compare / motion history
- [ ] constraint / workspace / singularity 시각화
- [ ] Instructor demo mode
- [ ] Asset subset Git tracking 완료
- [ ] Robot Library showroom 후속 프리뷰 안정화 (`FANUC`, `IGUS`) + 다수 로봇 페이지 전환
- [ ] 3DOF / 6DOF 실제 전환
- [ ] URDF Import 기반 로봇 확장 (UR5, Puma560, Franka 사전 조사 완료)
- [ ] interactive matrix viz 확장 (ncase.me/matrix 패턴 디자인 레퍼런스 완료)
- [ ] workspace envelope 시각화 (2DOF 해석적 → N-DOF Monte Carlo)

### P2
- [ ] pick foundation
- [ ] Progress / assessment / challenge
- [ ] Android tablet internal build
- [ ] LLM teaching layer

## UI 품질 기준 연동
- 기능 존재 여부는 이 문서에서 보고, 현재 수동 QA 경로와 페이지별 점검 기준은 `docs/status/page-qa/README.md`에서 관리한다.
- 과거 `L1~L5` UI baseline은 `docs/archive/legacy/page-qa/page-quality-baseline.md`에 보관한다.
- 공유 컴포넌트 갭과 실제 페이지 점검은 page-qa runbook 기준으로 본다.

## Quick Read
- 지금 제품은 `2DOF + SCARA` 기준으로 `RobotLibrary`, `math_readiness`(조작 우선 + 3D 각도 기준선 + 좌측 3블록), Guided Lesson, Sandbox, snapshot lite를 연결한 학습 MVP이며, 모드별 패널 격리로 각 페이지가 자기 콘텐츠만 표시한다.
- Phase 5 P0 완료. 다음 구현 순서는 `Sandbox polish 마감 -> tablet 4DOF rail -> asset subset tracking -> replay / constraint preview -> Instructor demo`다.
- UI 품질 점검은 `docs/status/page-qa/README.md`를 참조한다.
