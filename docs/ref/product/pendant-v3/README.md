---
title: "Teaching Pendant V3"
doc_type: "readme"
status: "active"
domain: "product-reference"
audience: "human-and-agent"
canonical: true
last_updated: "2026-04-29"
---

# Teaching Pendant V3 - UI Toolkit 기반 소프트 티칭패드

## Purpose
- UI Toolkit(UXML/USS) 기반의 차세대 소프트 티칭패드 V3 설계 문서 허브.
- 실제 산업용 펜던트(UR PolyScope, KUKA smartPAD, Doosan DART, FAIRINO) 리서치를 반영한 효율적 레이아웃.
- V1/V2(uGUI) 대비 비교 평가 후 채택 여부를 결정하는 프로토타입 전략.

## Parent Docs
- [robotcontrol-soft-teaching-pad.md](../ux/robotcontrol-soft-teaching-pad.md) — V1/V2 UX 계획 (uGUI)
- [fairino-teaching-pad-feature-matrix.md](../robots/fairino-teaching-pad-feature-matrix.md) — FAIRINO 1:1 기능 매트릭스
- [robotcontrol-soft-teaching-pad-v1-backlog.md](../roadmap/robotcontrol-soft-teaching-pad-v1-backlog.md) — V1 백로그
- [robot-button-integration-plan.md](./robot-button-integration-plan.md) — V3 버튼-로봇 연동 SSOT

## Last Updated
- 2026-05-01 (KST)

---

## 전략 요약

### V3 핵심 결정
1. **UI Toolkit 채택**: Unity 6 Runtime UI Toolkit (UXML + USS + C# 바인딩)
2. **V1/V2와 병행 평가**: V3 셸을 먼저 만들고, 동일 기능으로 비교 후 채택 결정
3. **UI 먼저, 기능 나중에**: 셸 + 패널 레이아웃 완성 → V1에서 로직 가져오거나 새로 추가
4. **모듈식 문서**: 기능별 와이어프레임을 별도 파일로 분리, 이 README가 인덱스

### 현재 실행 스냅샷 (2026-04-22)
- `2C-1` 안전/진단: 배너 + fault overlay + diagnostics scaffold 완료, action/policy wiring 후속
- `2C-2` 뷰포트 보조 UI: viewport toolbar + boundary/collision visual scaffold 완료, visualization 실데이터 연동 후속
- `2D` 팝업/도움말: confirm/unsaved + move/warning/recovery + help-panel/WhyItMoved 최소 scaffold 완료, `first-run guide`와 정책 심화 후속
- tablet help 경로: `BottomTabHelp` actual smoke까지 확인 완료
- `3A` 1차 잠금: 표시 패널 5개 binder화 + scene bootstrap 순서 고정
- `3A-3` 우측 컬럼: `상태 / 좌표` 탭 분리 + `ContextPanelScroll` 도입으로 하단 카드 텍스트 잘림 해소
- 2026-04-20 viewport 시행착오 요약
  - `ViewportHost` 내장형/분리형, 툴바 재배치, RT/카메라 가시화 실험은 **채택 없이 rollback**
  - `8549b09` baseline 자체에 별도 `ViewportHost`가 이미 포함된다는 점을 재확인
  - 다음 세션에서는 구현 전에 "로봇을 어느 패널에 표시할지"를 먼저 잠그고 시작
- 2026-04-20 표시 패널 잠금
  - 현재 하이라이트된 `WorkPanel`을 **로봇 표시 핵심 패널**로 확정
  - `ViewportHost`는 메인 로봇 표시 패널이 아니라 **보조/유틸 패널**로 후퇴
  - 로봇은 `WorkPanel` 안에서 **RobotStage 단독 표시**로 유지
  - 탭별 조작/도움말/연결 홈은 `ViewportHost`의 **보조 작업 패널 + 공용 스크롤**로 이동
  - 1차 잠금 비율은 `WorkPanel 70% / ViewportHost 30%` 기준으로 본다
- 2026-04-21 보조/오른쪽 패널 compact lock
  - 가로 스크롤은 UX상 금지로 유지한다.
  - `ViewportPanelScroll`, `ContextPanelScroll`은 세로 전용 공용 스크롤로 잠근다.
  - 보조패널 툴바는 `Base / Tool / Path / Ghost / Bound / Coll / Cam` compact chip grid로 유지한다.
  - TCP/Cartesian 조작행은 `축+값+단위`와 `- / +` 버튼을 2줄로 분리해 좁은 폭에서 버튼이 잘리지 않게 한다.
  - 관절 조그는 `J축+입력+값`, `슬라이더`, `- / +` 버튼 행을 분리한다.
  - 오른쪽 `ContextPanel` 좌표/상태 카드는 compact grid/wrap으로 두고, 보조패널을 키워 해결하지 않는다.
  - 최신 재시작 검증 기준 `viewportClipped=0`, `contextClipped=0`, `horizontalVisible=False`, `scrollShare>=0.88`을 확인했다.
- 2026-04-21 버튼-로봇 연동 기준선
  - `robot-button-integration-plan.md`를 버튼 연동 SSOT로 추가했다.
  - 모든 V3 버튼은 `wired / partial / stub / pending / excluded` 중 하나로 분류한다.
  - 구현은 `V2 성공패턴(구조/검증)` + `FAIRINO 공식 SDK 기능 범위` + `V3 문서/레이아웃 잠금`을 같이 비교하며 진행한다.
  - Mock e2e와 live command safety gate scaffold는 통과했다.
  - Live 실기 이동은 `manual readback simulation`, `operator confirm UX`, `production IK policy`가 준비될 때까지 계속 금지한다.
- 2026-04-22 문서 정합성 잠금
  - 최신 handoff 기준 커밋은 `853d6c5 Document RobotControl V3 next session handoff`다.
  - 이후 우선순위는 teaching sequence/readback 기반을 먼저 보는 방향으로 재정렬했다.
- 2026-04-22 product live confirm token
  - `Run/Move` 확인 팝업에서 non-DryRun 명령용 1회성 live token을 발급/표시한다.
  - 확인 버튼은 token을 live gate 승인으로 승격하고, 취소는 token을 폐기한다.
  - 이후 논의에서 boundary/collision은 hard gate가 아니라 warning/future로 내리고, teaching sequence/readback 기반을 먼저 보기로 조정했다.
- 2026-04-22 teaching sequence execution plan
  - `PendantV3Points`를 단순 저장 목록에서 실행 가능한 티칭 시퀀스로 승격하는 전체 계획을 추가했다.
  - 오늘 이후 우선순위는 boundary/collision보다 `Program Run/Step queue`와 point sequence execution을 먼저 본다.
  - 실기기 연동 전에는 Unity/Mock에서 `manual readback -> RobotStage -> 값 표시 -> 포인트 저장 -> DryRun replay` 루프를 먼저 통과해야 한다.
  - Teaching sequence v1은 `readback 저장`, `Step preview only`, `Run pending-first then sequence`, `위/아래 순서변경`, `unique point name`으로 잠근다.
  - 왼쪽 Nav에는 새 `Program` 탭을 추가하지 않고, 기존 `NavPoints`를 `포인트 / 시퀀스 / 함수` 내부 확장 구조로 승격한다.
  - 기존 상용 티칭펜던트보다 쉬운 UX를 목표로 `현재 위치로 덮어쓰기`, readable point detail, `선택 지점부터 실행`, 쉬운 함수명(`Pick`, `Place`)을 계획에 포함한다.
  - 포인트 이름은 사용자 입력으로 잠그고, 빈 이름 저장 금지, 중복 이름은 확인 후 덮어쓰기, 실행 중 편집 금지, 실패 시 즉시 정지로 정한다.
- 최신 진행률/검증 수치는 [progress-checklist.md](./progress-checklist.md)를 SSOT로 본다.
- V3 구조 분해의 현재 스캔 결과와 다음 분리 순서는 [v3-componentization-priority-plan.md](./v3-componentization-priority-plan.md)를 SSOT로 본다.
- 오늘 구현 로그:
  - `docs/daily/04-22/pendant-v3-teaching-sequence-execution-plan.md`
  - `docs/daily/04-22/pendant-v3-product-live-confirm-token.md`
  - `docs/daily/04-22/pendant-v3-doc-consistency-refresh.md`
  - `docs/daily/04-21/pendant-v3-button-robot-integration-ssot.md`
  - `docs/daily/04-21/pendant-v3-aux-compact-no-horizontal-scroll.md`
  - `docs/daily/04-20/pendant-v3-workpanel-robot-display-lock.md`
  - `docs/daily/04-20/pendant-v3-viewport-reset-and-next-session-lock.md`
  - `docs/daily/04-15/pendant-v3-context-panel-scroll-fix.md`
  - `docs/daily/04-15/pendant-v3-context-panel-phase-3-tab-split.md`
  - `docs/daily/04-15/pendant-v3-context-panel-density-phase-2-status-safety-rebalance.md`
  - `docs/daily/04-15/pendant-v3-context-panel-density-phase-plan.md`
  - `docs/daily/04-15/pendant-v3-phase-3a-binder-scene-bootstrap-lock.md`
  - `docs/daily/04-13/pendant-v3-safety-diagnostics-scaffold.md`
  - `docs/daily/04-13/pendant-v3-viewport-toolbar-boundary-collision-scaffold.md`
  - `docs/daily/04-14/pendant-v3-phase-2d-popup-scaffold-and-smoke.md`
  - `docs/daily/04-14/pendant-v3-help-panel-and-why-it-moved-scaffold.md`
  - `docs/daily/04-14/pendant-v3-help-panel-tablet-route-followup.md`

### 기술 선택 근거

| 기준 | uGUI (V1/V2) | UI Toolkit (V3) |
|------|--------------|-----------------|
| 레이아웃 | RectTransform 수동 배치 | Yoga 기반 Flexbox-like 자동 배치 |
| 스타일링 | C# 코드에서 직접 설정 | USS 파일로 분리 |
| 데이터 바인딩 | 수동 이벤트 구독 | Runtime Data Binding (Unity 6 정식) |
| 반응형 Desktop/Tablet | UILayoutProfile로 수동 분기 | C# 클래스 토글 + USS (※ @media 쿼리 미지원) |
| 3D 월드스페이스 UI | 강함 (성숙) | Unity 6에서 추가됨 (성숙도 확인 필요) |
| 유지보수 | 코드와 스타일 혼재 | 구조(UXML)/스타일(USS)/로직(C#) 분리 |

### 하이브리드 전략
- **UI Toolkit**: 메인 셸, 모든 2D 패널, 상태바, 탭, 리스트, 입력 폼
- **uGUI/직접 렌더링 유지**: 3D 시각화 (조인트 하이라이트 링, 타겟 마커, 프레임 기즈모, EE 트레일)
  - Unity 6에서 UIDocument World Space가 추가되었으나 성숙도 미검증 → 3D 오버레이는 기존 방식 유지
- **기존 재사용**: `RobotControlViewState`, `IFairinoRobotClient`, 모션/연결 로직 전체

### WorkPanel 표시 잠금
- Desktop 메인 로봇 표시 영역은 `ViewportHost`가 아니라 `WorkPanel`이다.
- `WorkPanel`은 `RobotStage` 단일 책임으로 고정한다.
- `RobotStage`는 탭 전환과 무관하게 유지되는 대형 3D 표시 영역이다.
- 탭별 조작 UI, 도움말, 연결 홈은 `ViewportHost` 쪽 `보조 작업 패널`에서 스크롤로 이어 붙인다.
- `ViewportHost`는 1차 구현에서 제거하지 않더라도 메인 디지털 트윈 역할을 맡지 않는다.
- `Base축 / Tool축 / 궤적` 토글은 `ViewportHost` 상단 보조 툴바에서 제어하되, 실제 3D 반영은 `RobotStage`에서 본다.
- `선택 파츠 XYZ 기즈모`, `바닥 격자`처럼 로봇에 직접 붙는 시각 요소는 `RobotStage`에 둔다.
- `TCP 3D 화살표`, 특히 `Z / RX / RY / RZ` 조작 UI는 로봇을 가리지 않도록 `ViewportHost` 보조 패널에 둔다.
- 1차 목표는 "큰 작업 패널 안에는 로봇만 또렷하게 보이고, 조작은 옆 보조 패널로 흘린다"를 만족시키는 것이다.

### 기술 주의사항
- **USS @media 미지원**: 반응형 레이아웃은 C# `GeometryChangedEvent` + `AddToClassList()`/`RemoveFromClassList()`로 구현
- **UI Toolkit은 Unity 6 built-in 모듈**: `com.unity.ui` 별도 설치 불필요, `UnityEngine.UIElements` 네임스페이스 직접 사용
- **uGUI 공존**: 같은 프로젝트에서 사용 가능하나 이벤트 시스템이 다르므로 입력 관통/렌더링 순서(PanelSettings Sort Order) 주의
- **Yoga 레이아웃**: CSS Flexbox 완전 호환이 아닌 Yoga 엔진 기반. `flex-direction`, `flex-grow`, `align-items`, `justify-content` 등 핵심은 지원하나 일부 CSS 속성 차이 있음

---

## 설계 원칙 (실제 펜던트 리서치 기반)

### 1. 플랫 네비게이션 (UR PolyScope 참고)
- 메뉴 깊이 **최대 1단계** — 탭 전환으로 모든 기능 도달
- UR PolyScope 5는 5개 메인 탭(Run/Program/Move/Installation/Log)으로 깊이 1~2
- FANUC식 깊은 메뉴 트리(Menu > Browser > 기능, 멀티페이지 탐색) **금지**

### 2. 상단 상태 바 항상 가시 (전 펜던트 공통)
- 연결, 모드, 속도, 안전, 좌표계 — 1초 안에 파악

### 3. 좌측 사이드바 + 중앙 콘텐츠 (KUKA smartPAD 참고)
- 아이콘 + 짧은 레이블로 기능 영역 전환
- 태블릿에서는 하단 탭 바로 변환 (Doosan DART 참고)

### 4. 조그 모드 명시적 선택 (Doosan/UR 참고)
- FANUC식 순환 전환 대신, Joint/TCP를 별도 탭으로 분리
- 좌표계(Base/Tool/User)는 탭 내에서 직접 선택

### 5. 프로그램 제어 하단 고정 (UR PolyScope Footer 참고)
- Play/Stop/DryRun은 탭 전환에 영향받지 않음

### 6. 3D 뷰포트 중앙 배치 (FAIRINO 참고)
- 디지털 트윈 + 궤적 + 좌표축 시각화가 핵심 차별점

---

## 기능 분류 및 우선순위

### P0 - 필수 (V3 프로토타입에 반드시 포함)

| 기능 | 문서 | SSOT 상태 |
|------|------|-----------|
| 셸 레이아웃 | [shell-layout.md](./shell-layout.md) | 신규 |
| 연결/상태 표시 | [feature-connection-status.md](./feature-connection-status.md) | V2 셸에 있음 |
| 조인트 조그 | [feature-jog-motion.md](./feature-jog-motion.md) | V2 플레이스홀더 |
| TCP 조그 | [feature-jog-motion.md](./feature-jog-motion.md) | V3 first slice kickoff |
| 좌표 표시 | [feature-coordinates.md](./feature-coordinates.md) | ViewState에 정의 |
| E-stop/정지 계열 | [feature-safety-controls.md](./feature-safety-controls.md) | V2 버튼 있음 |
| 에러 진단/복구 | [feature-diagnostics.md](./feature-diagnostics.md) | V2 플레이스홀더 |
| 3D 프리뷰/디지털 트윈 | [feature-3d-viewport.md](./feature-3d-viewport.md) | RobotStage/toolbar scaffold wired, boundary/collision warning-only future |

### P1 - 선택 (V3 평가 후 순차 추가)

| 기능 | 문서 | SSOT 상태 |
|------|------|-----------|
| 포인트 저장/호출 | [feature-points-teaching.md](./feature-points-teaching.md) | PointMove v1 wired, production IK/sequence policy pending |
| IO/그리퍼 제어 | [feature-io-peripherals.md](./feature-io-peripherals.md) | mock/live-gated facade wired, live command UX pending |
| 초보자/전문가 모드 | [feature-user-modes.md](./feature-user-modes.md) | BeginnerMode 확장 |
| Undo/Redo + 히스토리 | [feature-history.md](./feature-history.md) | Roadmap P1 |

### P0 추가 확정 (주인님 요구 + 채택 제안)

| 기능 | 문서 | Phase |
|------|------|-------|
| 그리퍼 개폐 (쉬운 조작 탭) | [feature-jog-motion.md](./feature-jog-motion.md) | 2B-1 |
| 인풋 수정 시 0 자동선택 + 즉시 반영 | [feature-jog-motion.md](./feature-jog-motion.md) | 2B-2 |
| 데카르트 3D 화살표 방향 기기조작 | [feature-jog-motion.md](./feature-jog-motion.md) | 2B-3 |
| 속도 오버라이드 실시간 슬라이더 | [shell-layout.md](./shell-layout.md) | 1A |
| 버튼 클릭 시 확인 팝업 모달 | [feature-safety-controls.md](./feature-safety-controls.md) | 2D |
| 수정/삭제/창닫기 시 확인 팝업 | [feature-safety-controls.md](./feature-safety-controls.md) | 2D |
| 가독성 아이콘 → 함수별 매핑 | 전체 | 0~ |
| 작업공간 경계 시각화 | [feature-3d-viewport.md](./feature-3d-viewport.md) | 2C-2 |
| 경로 충돌 사전 검출 | [feature-3d-viewport.md](./feature-3d-viewport.md) | 2C-2 |
| Undo/Redo (BottomBar 상시) | [feature-history.md](./feature-history.md) | 3B |
| 로컬스토리지 임시 값 저장 | 신규 | 3B |
| 자동 재연결 (3초 재시도) | [feature-connection-status.md](./feature-connection-status.md) | 3B |

### P1 추가 확정

| 기능 | 문서 | Phase |
|------|------|-------|
| 블록 추가 + 단계별 루프 생성 | [feature-points-teaching.md](./feature-points-teaching.md) | 5 |
| 논리명령(IF/ELSE/LOOP) + 이동명령(MoveJ/L/C) 블록 | [feature-points-teaching.md](./feature-points-teaching.md) | 5 |
| MoveC 원호 이동 블록 | [feature-points-teaching.md](./feature-points-teaching.md) | 5 |
| 다중 포인트 경로 전체 미리보기 | [feature-3d-viewport.md](./feature-3d-viewport.md) | 5 |
| 스크린샷/상태 캡처 (PNG+JSON) | [feature-diagnostics.md](./feature-diagnostics.md) | 6 |
| 드래그 티칭 → 블록 자동 변환 | [feature-points-teaching.md](./feature-points-teaching.md) | 7 |
| 조그 감도 설정 (모드별) | [feature-user-modes.md](./feature-user-modes.md) | 7 |

### P2 - 차별화 (후속 단계)

| 기능 | 문서 | SSOT 상태 |
|------|------|-----------|
| AI 보조 티칭 | [feature-ai-assist.md](./feature-ai-assist.md) | Roadmap P2 |
| 비전 오버레이 | [feature-vision.md](./feature-vision.md) | 신규 |
| 작업 템플릿 | [feature-templates.md](./feature-templates.md) | 신규 |
| 프로그램 편집/실행 | [feature-program.md](./feature-program.md) | SSOT 제외 상태 |
| 연결 QR 코드 (태블릿 스캔) | [feature-connection-status.md](./feature-connection-status.md) | 신규 |

---

## V1/V2 재사용 자산

| 자산 | 재사용 방법 | 문서 |
|------|------------|------|
| `RobotControlViewState` | 그대로 사용 (UI 독립) | [migration-strategy.md](./migration-strategy.md) |
| `IFairinoRobotClient` | 그대로 사용 | |
| `FairinoConnectionService` | 그대로 사용 | |
| `UIDesignTokens.RobotControlV2` | USS 토큰으로 변환 | |
| `PreviewRiskSummary` | 그대로 사용 | |
| `RecoveryHintViewState` | 그대로 사용 | |
| 3D 시각화 (trail, highlight, frame) | uGUI 레이어로 유지 | |

---

## 잠금 규칙 (Unity 공식문서 기반, 변경 금지)

상세 규칙은 구현 플랜 파일 참조. 핵심만 요약:

### 네이밍 1대1 매핑
- UXML `kebab-case` ↔ Controller `PascalCase+Controller` ↔ 기능명 일치
- USS 클래스 `.rc-` 접두사, 토큰 `--rc-` 접두사, UXML name `PascalCase`
- 아이콘 `icon-{기능}.png`

### 파일 크기/구조 제한
- C# **300줄 초과 금지** → 컴포넌트 분리
- UXML **중첩 5단계 초과 금지** → 별도 Template 분리
- USS **200줄 초과 시** 기능별 분리

### UI Toolkit 생명주기 (공식문서 확인)
- `OnEnable()`에서 초기화 (Awake/Start 금지)
- `OnDisable()`에서 콜백 해제 (메모리 누수 방지)
- 값 갱신 시 `SetValueWithoutNotify()` (무한 루프 방지)
- 요소 쿼리는 `Q<T>("Name")` (타입만 쿼리 금지)

### Flexbox 안티패턴 (공식문서 명시)
- ❌ flex 부모에 고정 px → 반응형 깨짐
- ❌ `flex-shrink: 0` 남용 → 오버플로우
- ❌ absolute + flex 자식 혼합
- ❌ 인라인 스타일 과다 → USS 분리 필수

### ListView 필수 패턴
- `itemsSource` + `makeItem` + `bindItem` 3요소 필수
- `FixedHeight` 가상화 필수
- `fixedItemHeight` 명시

### PanelSettings / Panel Topology 잠금
- V3 런타임 SSOT는 `Assets/UI/PendantV3/PanelSettings/PendantV3PanelSettings.asset` 하나로 시작한다.
- `TopStatusBar`, `NavRail`, `MainContent`, `ContextPanel`, `BottomBar`, `Popups`, `BottomSheet`는 Phase 4 채택 결정 전까지 **같은 PanelSettings**를 공유한다.
- 별도 PanelSettings 분리는 `정량 성능 프로파일` 또는 `입력 우선순위 충돌`이 확인된 경우에만 허용한다.
- 다중 패널을 도입할 경우 `Sort Order`, 포커스 handoff, pointer 우선순위를 같은 턴에 문서화해야 한다.

### 입력 시스템 잠금
- V3는 `Input System Package` 기준으로 구현한다.
- V3 코드에서 `UnityEngine.Input` 직접 사용은 금지한다.
- 런타임 `EventSystem`은 씬당 1개만 허용하고, `InputSystemUIInputModule`을 단일 기준으로 사용한다.
- uGUI 공존 구간도 입력은 같은 EventSystem 경로로 통일한다.
- `ViewportHost`를 제외한 V3 UI 위 포인터 입력은 3D로 관통시키지 않는다.

### 포커스 / 네비게이션 잠금
- 장식 요소는 `focusable = false`, `tabIndex = -1`을 기본으로 한다.
- 기본 포커스 순서는 `TopStatusBar -> NavRail -> 조작 내부 탭(ControlDockHost) -> WorkPanel -> ContextPanel -> BottomBar -> Popup`으로 고정한다.
- 팝업이 열리면 포커스를 팝업 내부에 가두고, 닫힐 때 호출 버튼으로 포커스를 복원한다.
- `Escape = 취소/닫기`, `Enter = 기본 확인`, 방향키/D-pad = 같은 레벨 이동을 기본 규칙으로 한다.

### 바인딩 소유권 잠금
- 구조와 기본 스타일은 `UXML/USS`, 런타임 상태는 `C# Binder`가 소유한다.
- 같은 요소에 대해 `Runtime Data Binding`과 수동 `C# write`를 동시에 사용하지 않는다.
- 로봇 상태, 연결 상태, 모션 상태는 `RobotControlViewState`와 `PendantV3LocalState`를 통해서만 공급한다.
- 탭 선택, split 비율, 시트 확장 상태 같은 UI-로컬 값만 `viewDataKey` 또는 `LocalSettingsStore`에 저장한다.

### 실기 연동 literal 예외
- 기본 원칙은 `preview/demo` 문자열과 임시 sample 숫자를 runtime controller에 직접 박지 않는 것이다.
- 다만 이 저장소의 현재 RobotControl 실기 목표가 `FAIRINO_FR5` 중심으로 잠겨 있는 동안에는 `FAIRINO_FR5`, live endpoint IP/port, 현재 연결 계약을 표현하는 literal은 예외로 허용한다.
- 이런 literal은 임시 데모 하드코딩이 아니라 **앱-실기기 연동 계약**으로 취급한다.
- 반대로 포즈 예시값, 임시 경고 문구, 미리보기용 숫자, 화면 설명 카피는 계속 preview/demo asset이나 state SSOT로 분리한다.

### 텍스트 설정 잠금
- `Assets/UI/PendantV3/PanelSettings/PendantV3TextSettings.asset`를 만들고 `PendantV3PanelSettings.asset`에 직접 연결한다.
- 기본 폰트는 `Noto Sans KR`, 누락 글리프 대비용 fallback font 목록을 별도 지정한다.
- 한국어 UI이므로 `Korean Line Breaking Rules`를 켠다.
- 개발 중에는 missing glyph 경고를 끄지 않는다.
- Text/Sprite/Custom Style path는 모두 `Resources` 하위 경로로 고정한다.

### 리스트 / 탭 지속성 잠금
- 10개 이상 반복 항목, 재정렬 가능 목록, 가상화가 필요한 목록은 `ScrollView` 대신 `ListView`를 우선 사용한다.
- 기본 가상화 방식은 `FixedHeight`, `fixedItemHeight` 명시는 필수다. 가변 높이 콘텐츠일 때만 `DynamicHeight`를 허용한다.
- 목록 갱신은 `RefreshItems`/`RefreshItem`을 기본으로 하고, `Rebuild`는 item template 또는 타입 변경 때만 허용한다.
- 작업자용 주 탭(`NavRail`, 조작 내부 탭)은 재정렬을 허용하지 않는다.

### 성능 / 그래픽 자산 잠금
- 패널/시트/팝업 애니메이션은 `translate`, `scale`, `opacity` 중심으로 구현하고 `width`, `height`, `top`, `left` 애니메이션은 금지한다.
- 중첩 마스크는 최대 1단계만 허용한다. popup dim + card 조합 이상은 별도 승인 없이는 금지한다.
- 정적 pendant 아이콘은 `Sprite Atlas`로 묶고, 동적 생성 이미지에만 `dynamic atlas`를 의존한다.
- 자주 움직이는 컨테이너는 `usageHints`를 검토하고, 재생성 대신 `display`/`visibility` 토글을 우선 사용한다.

### 잠금 변수 (확정)

| 항목 | 값 |
|------|-----|
| 씬 | `RobotControlV3.unity` — 온보딩에서 버튼 이동 (SceneCatalog 등록) |
| 브랜치 | `codex/robotcontrol-v3-toolkit` |
| Scale Mode | `Scale With Screen Size`, Ref 1920x1080, Match 0.5 |
| 기본 속도 | 30% |
| DryRun 기본 | Live 첫 연결 시 ON |
| 색상 테마 | 다크 고정 (V2 Colors) |
| 아이콘 | PNG 64x64 @2x 투명배경 |
| 텍스트 리소스 | ScriptableObject (`PendantLocalization.asset`) — Inspector 수정 가능, 후속 다국어 |
| 포인트 저장 | JSON, `persistentDataPath/waypoints/PendantV3Points.json` |
| Undo 깊이 | 50개 |
| 이벤트 로그 | 200개 FIFO |
| 자동 재연결 | 10회 (3초 간격) |
| 루프 중첩 | 최대 3단계 |
| MoveC | 3점 원호 (시작→중간→끝) |
| **치수** | |
| TopStatusBar | 56px |
| NavRail | 72px (접힘 48px) |
| ContextPanel | 320px |
| BottomBar | 48px |
| 조작 내부 탭 | 40px, `ControlDockHost` 내부, `NavMotion` 전용 |
| 패널 간격 | 4px (margin) |
| 카드 패딩 | 12px |
| 터치 최소 | 44x44px |
| 프리셋 버튼 | 88x88px |
| **타이포** | |
| 기본 폰트 | **17px**, Noto Sans KR |
| 작은 텍스트 | 14px |
| 라벨 | 12px |
| 헤더 | 20px |
| **타이밍** | |
| 탭 전환 | 150ms ease-out |
| 팝업 등장/닫기 | 200ms / 150ms |
| 토스트 | 3초 |
| 조그 long-press | 300ms |
| **3D** | |
| 고스트 투명도 | 30% |
| 작업공간 경계 | 15% |
| EE 트레일 | 3초 유지 |
| **빌드** | |
| Build Index | 7 |
| asmdef | `KineTutor3D.UI.RobotControlV3` (uGUI 참조 금지) |
| **스타일** | |
| border-radius | 카드/버튼 6px, 팝업 12px, 입력 4px |
| 비활성 opacity | 0.4 |
| 팝업 딤 | rgba(0,0,0,0.6) |
| 스크롤바 | 6px, 2초 후 자동 숨김 |
| 포커스 | 2px solid AccentPrimary |
| **데이터** | |
| 단위 | deg/mm 고정 (전환 없음) |
| 소수점 | 1자리 |
| 속도 스텝 | 1% (1~100) |
| 최대 웨이포인트 | 100개 |
| 최대 시퀀스 스텝 | 200개 |
| 드래그 샘플 | 50ms (20Hz) |
| IO (FR5) | DO8 DI8 AO2 AI2 TDO2 |
| **단축키** | |
| Space | 긴급 정지 |
| Ctrl+Z/Y | Undo/Redo |
| Escape | 팝업 닫기 |
| 1~4 | 탭 전환 |
| **범위** | |
| 대상 로봇 | 단일 (FR5). 멀티는 Phase 8+ |

### 참조 문서 (구현 시 항상 확인)
- [UI 시스템 비교](https://docs.unity3d.com/6000.3/Documentation/Manual/UI-system-compare.html)
- [Runtime UI 시작](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-get-started-with-runtime-ui.html)
- [Flexbox 레이아웃](https://docs.unity3d.com/6000.3/Documentation/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/layouts.html)
- [이벤트 처리](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-Events-Handling.html)
- [Runtime Event System](https://docs.unity3d.com/Manual/UIE-Runtime-Event-System.html)
- [Focus Order](https://docs.unity3d.com/Manual/UIE-focus-order.html)
- [Navigation Events](https://docs.unity3d.com/Manual/UIE-Navigation-Events.html)
- [ListView](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-uxml-element-ListView.html)
- [UITK Text Settings](https://docs.unity3d.com/Manual/UIE-text-setting-asset.html)
- [Data Binding](https://docs.unity3d.com/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/data-binding.html)
- [Performance](https://docs.unity3d.com/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/optimizing-performance.html)
- [Graphic and Font Assets](https://docs.unity3d.com/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/graphic-and-font-assets-preparation.html)
- [Create a Panel](https://docs.unity3d.com/Manual/UIE-create-panel.html)
- [Configure Runtime UI](https://docs.unity3d.com/Manual/UIE-render-runtime-ui.html)
- [USS 속성](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-USS-Properties-Reference.html)

---

## 구현 Phase 계획

| Phase | 범위 | 산출물 |
|-------|------|--------|
| **Pre** | 폴더 구조 + CLAUDE.md 인덱스 | 폴더별 CLAUDE.md, 루트 링크 추가 |
| **Phase 0A** | 인프라 자산 | PanelSettings + TextSettings + SpriteAtlas + UIDocument + bootstrap 호출 경로 |
| **Phase 0B** | 루트 셸 뼈대 | `pendant-v3.uxml` + `pendant-v3.uss` 5영역 + 최소 `RobotControlV3.unity` |
| **Phase 0C** | 입력/포커스 계약 | EventSystem + InputModule + 기본 포커스 규칙 |
| **Phase 1A** | Desktop 셸 | TopStatusBar + NavRail + BottomBar + ContextPanel |
| **Phase 1B** | Tablet 셸 | BottomSheet + tablet 전환 + ★시안 리뷰 게이트 |
| **Phase 1C** | 탭 지속성 | `viewDataKey` + `LocalSettingsStore` |
| **Phase 2A-1** | 연결 홈 | 연결 카드 + 빠른 상태 + 행동 추천 |
| **Phase 2A-2** | 상태/좌표 패널 | StatusCard + CoordStrip |
| **Phase 2B-1** | 쉬운 조작 | 프리셋 + 그리퍼 |
| **Phase 2B-2** | 관절 조그 | 슬라이더 + 단일축 + 숫자입력 |
| **Phase 2B-3** | TCP 조그 | 좌표계 + XYZ/RPY + 3D 화살표 |
| **Phase 2B-4** | 포인트 이동 | 좌표입력 + IK + MoveJ/L/C 선택 |
| **Phase 2C-1** | 안전/진단 UI | 배너 + fault overlay + diagnostics |
| **Phase 2C-2** | 뷰포트 보조 UI | toolbar + 작업공간경계 + 충돌검출 |
| **Phase 2D** | 팝업/도움말 UI | 전체 팝업 시스템 + 컨텍스트 도움말 + WhyItMoved |
| **Phase 3A** | Binder/씬 부트스트랩 | Binder + SceneCoordinator + V3 씬 |
| **Phase 3B** | 로컬 서비스 | Undo/Redo + 로컬저장 + 자동재연결 |
| **Phase 3C** | Mock 종단 검증 | Mock 연결부터 tablet 전환까지 e2e smoke |
| **Phase 4** | V2 vs V3 비교 평가 | 평가 매트릭스 → 채택 결정 |
| **Phase 5** | 포인트/티칭/블록 에디터 | 블록(이동/IO/논리) + 루프 + MoveC + 다중경로 미리보기 |
| **Phase 6** | IO/그리퍼/진단/캡처 | DI/DO/AI/AO + 그리퍼 + 세션리포트 + 스크린샷캡처 |
| **Phase 7** | 모드 분리 + 드래그 티칭 | 초보자/전문가 + 드래그→블록 변환 + 조그감도 |
| **Phase 8** | P2 차별화 | AI보조 + 비전 + 템플릿 + QR연결 |

---

## 문서 인덱스

### 레이아웃
- [shell-layout.md](./shell-layout.md) — 전체 셸 구조 + Desktop/Tablet 와이어프레임

### P0 핵심 기능
- [feature-connection-status.md](./feature-connection-status.md) — 연결/상태/모드 표시
- [feature-jog-motion.md](./feature-jog-motion.md) — 조인트 조그 + TCP 조그 + 좌표계 전환
- [feature-coordinates.md](./feature-coordinates.md) — 실시간 좌표 표시 + 좌표계 시각화
- [feature-safety-controls.md](./feature-safety-controls.md) — E-stop/정지/리셋 + 안전 UX
- [feature-diagnostics.md](./feature-diagnostics.md) — 에러 코드/원인/해결/로그
- [feature-3d-viewport.md](./feature-3d-viewport.md) — 3D 디지털 트윈 + 프리뷰 + 궤적

### P1 선택 기능
- [feature-points-teaching.md](./feature-points-teaching.md) — 포인트 저장/호출/시퀀스 (`NavPoints` 내부 확장)
- [feature-io-peripherals.md](./feature-io-peripherals.md) — IO/그리퍼/외부장치
- [feature-user-modes.md](./feature-user-modes.md) — 초보자/전문가 모드
- [feature-history.md](./feature-history.md) — Undo/Redo + 포즈 히스토리

### P2 차별화 기능
- [feature-ai-assist.md](./feature-ai-assist.md) — AI 경고/추천/자연어 명령
- [feature-vision.md](./feature-vision.md) — 카메라/비전 오버레이
- [feature-templates.md](./feature-templates.md) — 작업 템플릿 (Pick&Place 등)
- [feature-program.md](./feature-program.md) — 프로그램 편집/실행

### 전략
- [migration-strategy.md](./migration-strategy.md) — V1/V2 재사용 + V3 전환 계획
- [implementation-plan.md](./implementation-plan.md) — **전체 구현 플랜** (Pre~Phase 8, 잠금 변수 + 운영 규칙 A~R)
- [teaching-sequence-execution-plan.md](./teaching-sequence-execution-plan.md) — `PendantV3Points` 실행 시퀀스 승격 계획
- [context-panel-density-remediation-plan.md](./context-panel-density-remediation-plan.md) — 오른쪽 컬럼 과밀 원인과 페이즈별 해결 계획
- [phase-3a-binder-scene-bootstrap-lock.md](./phase-3a-binder-scene-bootstrap-lock.md) — 3A 1차 범위 잠금과 책임 분리
- [progress-checklist.md](./progress-checklist.md) — **현재 진행률 체크리스트**
- [Daily Log Index](../../../daily/INDEX.md) — 일일 로그 통합 목차
- [AGENT-CONTRACT.md](./AGENT-CONTRACT.md) — V3 작업용 에이전트 계약 문서
- [static-checks.md](./static-checks.md) — V3 정적 체크 기준
- [unityctl-recipes.md](./unityctl-recipes.md) — V3 구현/검증용 `unityctl` 명령 레시피
- [verify-v3.json](./verify-v3.json) — `workflow verify`용 기본 검증 번들 (3C 종료 단독 대체 아님)
