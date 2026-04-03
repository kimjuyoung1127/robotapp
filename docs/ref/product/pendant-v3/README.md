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

### P0 추가 확정 (주인님 요구 + 채택 제안)

| 기능 | 문서 | Phase |
|------|------|-------|
| 그리퍼 개폐 (쉬운 조작 탭) | [feature-jog-motion.md](./feature-jog-motion.md) | 2B |
| 인풋 수정 시 0 자동선택 + 즉시 반영 | [feature-jog-motion.md](./feature-jog-motion.md) | 2B |
| 데카르트 3D 화살표 방향 기기조작 | [feature-jog-motion.md](./feature-jog-motion.md) | 2B |
| 속도 오버라이드 실시간 슬라이더 | [shell-layout.md](./shell-layout.md) | 2B |
| 버튼 클릭 시 확인 팝업 모달 | [feature-safety-controls.md](./feature-safety-controls.md) | 2D |
| 수정/삭제/창닫기 시 확인 팝업 | [feature-safety-controls.md](./feature-safety-controls.md) | 2D |
| 가독성 아이콘 → 함수별 매핑 | 전체 | 0~ |
| 작업공간 경계 시각화 | [feature-3d-viewport.md](./feature-3d-viewport.md) | 2C |
| 경로 충돌 사전 검출 | [feature-3d-viewport.md](./feature-3d-viewport.md) | 2C |
| Undo/Redo (BottomBar 상시) | [feature-history.md](./feature-history.md) | 3 |
| 로컬스토리지 임시 값 저장 | 신규 | 3 |
| 자동 재연결 (3초 재시도) | [feature-connection-status.md](./feature-connection-status.md) | 3 |

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

### 잠금 변수 (확정)

| 항목 | 값 |
|------|-----|
| 씬 | `RobotControlV3.unity` — 온보딩에서 버튼 이동 (SceneCatalog 등록) |
| 브랜치 | `main` |
| Scale Mode | `Scale With Screen Size`, Ref 1920x1080, Match 0.5 |
| 기본 속도 | 30% |
| DryRun 기본 | Live 첫 연결 시 ON |
| 색상 테마 | 다크 고정 (V2 Colors) |
| 아이콘 | PNG 64x64 @2x 투명배경 |
| 텍스트 리소스 | ScriptableObject (`PendantLocalization.asset`) — Inspector 수정 가능, 후속 다국어 |
| 포인트 저장 | JSON, `persistentDataPath/points/` |
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
| WorkTabBar | 40px |
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
- [ListView](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-uxml-element-ListView.html)
- [USS 속성](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-USS-Properties-Reference.html)

---

## 구현 Phase 계획

| Phase | 범위 | 산출물 |
|-------|------|--------|
| **Pre** | 폴더 구조 + CLAUDE.md 인덱스 | 폴더별 CLAUDE.md, 루트 링크 추가 |
| **Phase 0** | UI Toolkit 인프라 | PanelSettings, USS 토큰, UIDocument 패턴, 아이콘 폴더, PendantLocalization.asset |
| **Phase 1** | 셸 + 탭 + BottomBar | 빈 셸 Desktop/Tablet + ★시안 리뷰 게이트 |
| **Phase 2A** | 연결/상태 패널 UI | TopStatusBar + 연결홈 + StatusCard |
| **Phase 2B** | 조그/모션 패널 UI | 쉬운조작+그리퍼 + 관절 + TCP+3D화살표 + 포인트이동 + 속도슬라이더 |
| **Phase 2C** | 안전/좌표/3D UI | 배너 + 에러3단 + CoordStrip + 뷰포트툴바 + 작업공간경계 + 충돌검출 |
| **Phase 2D** | 팝업/도움말 UI | 전체 팝업 시스템 + 컨텍스트 도움말 + WhyItMoved |
| **Phase 3** | ViewState 바인딩 + Mock | 바인딩 + Undo/Redo + 로컬저장 + 자동재연결 |
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
- [implementation-plan.md](./implementation-plan.md) — **전체 구현 플랜** (Pre~Phase 8, 잠금 변수 82개, 잠금 규칙 A~L)
