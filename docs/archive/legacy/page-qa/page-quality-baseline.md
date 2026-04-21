# Page Quality Baseline

> Archive Note: 이 문서는 `Home/Main` 기준의 초기 UI 품질 baseline이라 archive로 이동했다.

## Purpose
- 각 페이지(씬)의 UI 완성도를 L1~L5 등급으로 정의하고, 현재 상태와 목표를 추적한다.
- 공유 컴포넌트 갭을 식별하고 공유화 우선순위를 관리한다.
- "Done"이라고 표시된 항목도 이 기준에 따라 실제 완성도를 재평가한다.

## Parent Doc
- [WIREFRAME](../../WIREFRAME.md)
- [information-architecture.md](./information-architecture.md)

## When To Read
- 페이지별 UI 개선 작업 착수 시
- 공유 컴포넌트 추출/리팩터링 시
- 릴리스 게이트 판단 시

## Locked Decisions
- L3 이상이 되어야 "UI 완성"으로 간주한다
- L4 이상이 되어야 "배포 후보"로 간주한다
- 모든 페이지는 공유 인프라(Design System, SharedPageShell, 상태 UI)를 사용해야 한다
- 페이지별 고유 콘텐츠는 JSON 기반으로 관리하는 것을 기본으로 한다

## Open Questions
- L5 접근성 기준을 어느 수준까지 잡을지 (WCAG AA? 커스텀?)
- Settings 페이지 설계 시점

## Downstream Sync
- `docs/ref/product/roadmap/current-feature-checklist.md`
- `docs/status/PROJECT-STATUS.md`

## Last Updated
- 2026-03-13 (KST)

---

## §1 완성도 등급 정의

| 등급 | 이름 | 조건 | 검증 방법 |
|------|------|------|----------|
| **L1** | 뼈대 (Skeleton) | 씬 진입 가능, 최소 1개 UI 패널 표시, coordinator/manager 존재 | PlayMode 진입 확인 |
| **L2** | 기능 (Functional) | 핵심 인터랙션 작동 (버튼, 슬라이더, 네비게이션), 데이터 흐름 연결 | PlayMode 시나리오 수동 확인 |
| **L3** | 디자인 (Polished) | UIDesignTokens/UIComponentFactory 100% 적용, 하드코딩 색상/간격 0, ViewBuilder 패턴 사용 | 코드 grep (하드코딩 색상 0건) |
| **L4** | 반응형 (Responsive) | UILayoutProfile 적용, 터치 타겟 44px+, Desktop/Tablet 양쪽 레이아웃 검증 | Tablet 해상도 테스트 |
| **L5** | 출시 (Release) | 빈 상태/로딩/에러 UI, 접근성, 성능 최적화, QA 회귀 테스트 통과 | 체크리스트 전항목 통과 |

### L5 세부 체크리스트
- [ ] 빈 상태 (Empty State) UI — 데이터 없을 때 안내 메시지
- [ ] 로딩 상태 (Loading) UI — 씬 전환/데이터 로드 시 피드백
- [ ] 에러 상태 (Error) UI — 실패 시 복구 안내
- [ ] 키보드/게임패드 네비게이션 (해당 시)
- [ ] 화면 전환 애니메이션 일관성
- [ ] PlayMode 스모크 테스트 통과
- [ ] 메모리 프로파일링 (씬 전환 시 누수 없음)

---

## §2 페이지별 현재 상태

### 요약 매트릭스

| # | 페이지 | Scene | L1 | L2 | L3 | L4 | L5 | 현재% | 목표 등급 |
|---|--------|-------|----|----|----|----|-----|-------|----------|
| 0 | Boot | Boot.unity | ✅ | ✅ | ❌ | ❌ | ❌ | 40% | L3 |
| 1 | Onboarding | Onboarding.unity | ✅ | ✅ | ❌ | ❌ | ❌ | 30% | L4 |
| 2 | Home | Home.unity | ✅ | ✅ | ✅ | △ | ❌ | 45% | L5 |
| 3 | Main (학습) | Main.unity | ✅ | ✅ | △ | ❌ | ❌ | 35% | L5 |
| 4 | MathReadiness | MathReadiness.unity | ✅ | ✅ | ✅ | ❌ | ❌ | 40% | L4 |
| 5 | RobotLibrary | RobotLibrary.unity | ✅ | ✅ | △ | ❌ | ❌ | 30% | L4 |
| 6 | Sandbox | Sandbox.unity | ✅ | ✅ | △ | ❌ | ❌ | 25% | L4 |
| 7 | RobotControl | RobotControl.unity | ✅ | △ | △ | ❌ | ❌ | 20% | L3 |

> ✅ = 충족, △ = 부분 충족, ❌ = 미충족

### 페이지별 상세

#### 0. Boot (40% → L3 목표)
- **현재**: `BootSceneRouter`로 라우팅만 수행. 카메라/라이트 fallback 존재
- **L3 갭**:
  - [ ] 스플래시/로딩 UI (앱 로고 + 프로그레스)
  - [ ] UIDesignTokens 기반 배경색 적용
- **L3 달성 시 완료로 간주** (Boot는 전환 씬이므로 L4/L5 불필요)

#### 1. Onboarding (30% → L4 목표)
- **현재**: `OnboardingManager`가 직접 UI 생성. 하드코딩 레이아웃/색상 다수
- **L3 갭**:
  - [ ] `OnboardingViewBuilder` 신규 작성 (UIComponentFactory 기반)
  - [ ] 하드코딩 색상/간격 → UIDesignTokens 전환
  - [ ] UiRuntimeStyle 호출 제거
  - [ ] 카드 버튼 스타일 UIComponentFactory.CreateButton 전환
- **L4 갭**:
  - [ ] UILayoutProfile 적용 (태블릿 카드 레이아웃)
  - [ ] 터치 타겟 44px+ 확인
  - [ ] 시퀀스 데이터 JSON 외부화 (`OnboardingSequenceConfig` → JSON)

#### 2. Home (45% → L5 목표)
- **현재**: `HomeContinueHubViewBuilder` 적용됨. UIComponentFactory 사용. 세션 컨텍스트 연동
- **L4 갭**:
  - [ ] UILayoutProfile 태블릿 검증
  - [ ] CTA 버튼 터치 타겟 확인
  - [ ] 재진입 시 빈 세션 상태 UI
- **L5 갭**:
  - [ ] 로딩 상태 (세션 복원 중)
  - [ ] 에러 상태 (세션 데이터 손상 시)
  - [ ] 화면 진입 애니메이션
  - [ ] PlayMode 스모크 테스트

#### 3. Main — 학습 (35% → L5 목표)
- **현재**: 패널 15+개 중 일부만 Design System 적용. `UiRuntimeStyle` 잔존. 기능적으로 완전 작동
- **L3 갭**:
  - [ ] `StepTutorPanel` ViewBuilder 전환
  - [ ] `JointInputRail` UIDesignTokens 전환
  - [ ] `MatrixDisplay` UIDesignTokens 전환
  - [ ] `FKDiagramPanel` UIDesignTokens 전환
  - [ ] `GlossaryPanelController` UIDesignTokens 전환
  - [ ] `TemplateSelector` UIDesignTokens 전환
  - [ ] UiRuntimeStyle 호출 전수 제거
  - [ ] 하드코딩 색상 전수 교체
- **L4 갭**:
  - [ ] UILayoutProfile 전체 패널 적용
  - [ ] 4DOF rail 태블릿 최적화
  - [ ] 좌/우 패널 폭 반응형 조정
- **L5 갭**:
  - [ ] 빈 상태 (로봇 미선택, 템플릿 없음)
  - [ ] Step 전환 애니메이션 일관성
  - [ ] PlayMode 전체 흐름 스모크

#### 4. MathReadiness (40% → L4 목표)
- **현재**: 고도화 A+B+C 완료. UIComponentFactory 사용. 색상/아이콘/애니메이션 적용
- **L4 갭**:
  - [ ] UILayoutProfile 적용 (질문 카드 폭, 버튼 배치)
  - [ ] 터치 타겟 확인
  - [ ] 3D 뷰포트 + 질문 패널 태블릿 비율 조정

#### 5. RobotLibrary (30% → L4 목표)
- **현재**: `RobotLibraryManager` 직접 UI 생성. `RobotCardBuilder`/`RobotDetailDrawer` 존재하나 스타일 혼재
- **L3 갭**:
  - [ ] `RobotLibraryViewBuilder` 신규 작성
  - [ ] `RobotCardBuilder` UIComponentFactory 전환
  - [ ] `RobotDetailDrawer` UIDesignTokens 전환
  - [ ] 하드코딩 색상/레이아웃 제거
  - [ ] 페이지네이션 UI 디자인 시스템 적용
- **L4 갭**:
  - [ ] UILayoutProfile 적용 (카드 그리드 컬럼 수 반응형)
  - [ ] 3D showroom + 카드 영역 비율 태블릿 최적화
  - [ ] 터치 타겟 확인

#### 6. Sandbox (25% → L4 목표)
- **현재**: `SandboxActionPanelViewBuilder`/`SnapshotLitePanelViewBuilder` 적용됨. overlap 수정됨
- **L3 갭**:
  - [ ] 버튼 아이콘 가독성 개선
  - [ ] exit clarity (Sandbox → 이전 화면 복귀 UX)
  - [ ] 로봇 피커 오버레이 UIDesignTokens 전환
  - [ ] 액션 버튼 라벨/아이콘 일관성
- **L4 갭**:
  - [ ] UILayoutProfile 적용
  - [ ] 슬라이더 + 액션 패널 태블릿 레이아웃
  - [ ] 스냅샷 패널 터치 최적화

#### 7. RobotControl (20% → L3 목표)
- **현재**: `FairinoRobotControlViewBuilder` 존재. SDK 미연결 (Mock만). 기능 부분 작동
- **L2 갭**:
  - [ ] 연결 상태 피드백 완성
  - [ ] 조인트 컨트롤 전체 6DOF 슬라이더 작동 확인
  - [ ] 상태 패널 실시간 업데이트 (Mock 기준)
- **L3 갭**:
  - [ ] UIDesignTokens 전체 적용
  - [ ] 연결/조인트/상태 패널 스타일 통일
  - [ ] 에러 메시지 UI (JSON 에러맵 활용)

---

## §3 공유 컴포넌트 갭 분석

### 현재 공유 인프라

| 컴포넌트 | 파일 | 사용 페이지 | 사용률 |
|---------|------|-----------|--------|
| `UIDesignTokens` | UI/UIDesignTokens.cs | Home, MathReadiness, Sandbox, Snapshot, DH, FR5 | ~60% |
| `UIComponentFactory` | UI/UIComponentFactory.cs | Home, Sandbox, Snapshot, DH, FR5 | ~50% |
| `UITypography` | UI/UITypography.cs | Home, MathReadiness | ~40% |
| `UIIconResolver` | UI/UIIconResolver.cs | 일부 builder | ~30% |
| `UILayoutProfile` | UI/UILayoutProfile.cs | Main(일부) | ~20% |
| `SceneNavigationBar` | UI/SceneNavigationBar.cs | 전체 navigable 씬 | ~90% |
| `UiRuntimeStyle` | UI/UiRuntimeStyle.cs (Obsolete) | Onboarding, Main, RobotLibrary | 레거시 |

### 필요한 신규 공유 컴포넌트

#### A. SharedPageShell (우선순위: 높음)
- **목적**: 모든 navigable 페이지의 공통 레이아웃 쉘
- **포함**: SceneNavigationBar + Content Area + 상태 표시 영역 (로딩/에러/빈 상태)
- **효과**: 페이지 간 일관된 구조, 신규 페이지 추가 시 boilerplate 제거
- **적용 대상**: 전 페이지 (Boot 제외)

#### B. 상태 UI 3종 (우선순위: 높음)
- **EmptyStateView**: 데이터 없을 때 안내 (아이콘 + 메시지 + CTA)
- **LoadingStateView**: 로딩 중 피드백 (스피너 + 메시지)
- **ErrorStateView**: 에러 시 복구 안내 (아이콘 + 메시지 + 재시도 CTA)
- **구현**: `UIComponentFactory`에 factory method 추가
- **적용 대상**: Home, Main, RobotLibrary, Sandbox, RobotControl

#### C. IVisibilityControllable 인터페이스 (우선순위: 중간)
- **목적**: `SetVisible(bool)` 계약을 전 패널에 표준화
- **현재**: 일부 패널만 SetVisible 구현, 나머지는 GameObject.SetActive 직접 호출
- **효과**: coordinator가 패널 목록을 일괄 제어 가능
- **적용 대상**: 전체 UI 패널 (~15개)

#### D. PageContentConfig JSON 표준 (우선순위: 중간)
- **목적**: 페이지별 텍스트/CTA/시퀀스를 코드에서 분리
- **현재**: LearningTabs만 JSON 기반, 나머지 하드코딩
- **확대 대상**:
  - Onboarding 시퀀스 → `onboarding-sequence.json`
  - Home CTA 텍스트/아이콘 → `home-hub.json`
  - RobotLibrary 카드 메타 → `RobotCatalog` 이미 존재하나 UI 텍스트 분리 가능
  - Sandbox 버튼 라벨 → `sandbox-actions.json`

#### E. ViewBuilder 확산 (우선순위: 높음)
- **현재 적용**: HomeContinueHubViewBuilder, SandboxActionPanelViewBuilder, SnapshotLitePanelViewBuilder, DHTableViewBuilder, FairinoRobotControlViewBuilder
- **미적용 (신규 필요)**:
  - `OnboardingViewBuilder`
  - `RobotLibraryViewBuilder` (또는 기존 RobotCardBuilder/RobotDetailDrawer 리팩터링)
  - `StepTutorPanelViewBuilder`
  - `MainLearningViewBuilder` (JointInputRail, MatrixDisplay, FKDiagram 통합)

---

## §4 공유화 실행 순서

### Phase A: 공유 기반 (전 페이지 영향)
1. `IVisibilityControllable` 인터페이스 정의 + 전 패널 적용
2. `UIComponentFactory`에 `EmptyState`/`Loading`/`Error` factory method 추가
3. `UiRuntimeStyle` 잔존 호출 전수 교체 → `UIComponentFactory`/`UIDesignTokens`
4. 하드코딩 색상/간격 전수 grep → 토큰 교체

### Phase B: ViewBuilder 확산 (페이지별)
5. `OnboardingViewBuilder` — Onboarding L3 달성
6. `RobotLibraryViewBuilder` — RobotLibrary L3 달성
7. `StepTutorPanelViewBuilder` + Main 패널 토큰화 — Main L3 달성
8. Sandbox 버튼/아이콘 가독성 — Sandbox L3 달성

### Phase C: 반응형 (L4 달성)
9. `UILayoutProfile` 전 페이지 적용
10. Tablet 해상도 테스트 라운드
11. 4DOF rail 태블릿 최적화

### Phase D: 출시 품질 (L5 달성)
12. 상태 UI 3종 전 페이지 적용
13. 화면 전환 애니메이션 통일
14. PlayMode 스모크 테스트 전 페이지
15. 메모리 프로파일링

---

## §5 완성도 추적 규칙

1. 페이지의 등급이 올라가면 이 문서의 매트릭스를 즉시 업데이트한다
2. 등급 변경 시 검증 방법 열의 조건을 실제로 확인한 뒤 올린다
3. `current-feature-checklist.md`와 동기화한다 — 기능 존재 여부는 checklist, UI 품질은 이 문서
4. △ (부분 충족)은 해당 등급의 50% 이상 항목을 만족할 때 표기한다
5. 공유 컴포넌트 추가/변경 시 §3 테이블을 업데이트한다
