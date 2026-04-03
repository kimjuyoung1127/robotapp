# Teaching Pendant V3 - UI Toolkit 기반 소프트 티칭패드

## Purpose
- UI Toolkit(UXML/USS) 기반의 차세대 소프트 티칭패드 V3 설계 문서 허브.
- 실제 산업용 펜던트(UR PolyScope, KUKA smartPAD, Doosan DART, FAIRINO) 리서치를 반영한 효율적 레이아웃.
- V1/V2(uGUI) 대비 비교 평가 후 채택 여부를 결정하는 프로토타입 전략.

## Parent Docs
- [robotcontrol-soft-teaching-pad.md](../ux/robotcontrol-soft-teaching-pad.md) — V1/V2 UX 계획 (uGUI)
- [fairino-teaching-pad-feature-matrix.md](../robots/fairino-teaching-pad-feature-matrix.md) — FAIRINO 1:1 기능 매트릭스
- [robotcontrol-soft-teaching-pad-v1-backlog.md](../roadmap/robotcontrol-soft-teaching-pad-v1-backlog.md) — V1 백로그

## Last Updated
- 2026-04-03 (KST)

---

## 전략 요약

### V3 핵심 결정
1. **UI Toolkit 채택**: Unity 6 Runtime UI Toolkit (UXML + USS + C# 바인딩)
2. **V1/V2와 병행 평가**: V3 셸을 먼저 만들고, 동일 기능으로 비교 후 채택 결정
3. **UI 먼저, 기능 나중에**: 셸 + 패널 레이아웃 완성 → V1에서 로직 가져오거나 새로 추가
4. **모듈식 문서**: 기능별 와이어프레임을 별도 파일로 분리, 이 README가 인덱스

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
| TCP 조그 | [feature-jog-motion.md](./feature-jog-motion.md) | V2 플레이스홀더 |
| 좌표 표시 | [feature-coordinates.md](./feature-coordinates.md) | ViewState에 정의 |
| E-stop/정지 계열 | [feature-safety-controls.md](./feature-safety-controls.md) | V2 버튼 있음 |
| 에러 진단/복구 | [feature-diagnostics.md](./feature-diagnostics.md) | V2 플레이스홀더 |
| 3D 프리뷰/디지털 트윈 | [feature-3d-viewport.md](./feature-3d-viewport.md) | Phase 5에서 구현 |

### P1 - 선택 (V3 평가 후 순차 추가)

| 기능 | 문서 | SSOT 상태 |
|------|------|-----------|
| 포인트 저장/호출 | [feature-points-teaching.md](./feature-points-teaching.md) | SSOT 추가 필요 |
| IO/그리퍼 제어 | [feature-io-peripherals.md](./feature-io-peripherals.md) | SSOT 추가 필요 |
| 초보자/전문가 모드 | [feature-user-modes.md](./feature-user-modes.md) | BeginnerMode 확장 |
| Undo/Redo + 히스토리 | [feature-history.md](./feature-history.md) | Roadmap P1 |

### P2 - 차별화 (후속 단계)

| 기능 | 문서 | SSOT 상태 |
|------|------|-----------|
| AI 보조 티칭 | [feature-ai-assist.md](./feature-ai-assist.md) | Roadmap P2 |
| 비전 오버레이 | [feature-vision.md](./feature-vision.md) | 신규 |
| 작업 템플릿 | [feature-templates.md](./feature-templates.md) | 신규 |
| 프로그램 편집/실행 | [feature-program.md](./feature-program.md) | SSOT 제외 상태 |

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

## 구현 Phase 계획

| Phase | 범위 | 산출물 |
|-------|------|--------|
| **Phase 0** | UI Toolkit 인프라 | PanelSettings 생성, USS 토큰 변환, UIDocument 패턴 확립 |
| **Phase 1** | 셸 + TopBar + 탭 네비게이션 | 빈 셸이 Desktop/Tablet에서 작동 |
| **Phase 2** | P0 패널 UI (연결, 조그, 좌표, 안전) | 레이아웃만, 로직 없음 |
| **Phase 3** | V1 로직 연결 | ViewState 바인딩 + Mock 모드 동작 |
| **Phase 4** | V2 vs V3 비교 평가 | 개발속도, 반응형, 바인딩, 성능 비교 |
| **Phase 5** | 채택 결정 후 P1 기능 추가 | 포인트, IO, 모드 분리 |

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
- [feature-points-teaching.md](./feature-points-teaching.md) — 포인트 저장/호출/시퀀스
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
