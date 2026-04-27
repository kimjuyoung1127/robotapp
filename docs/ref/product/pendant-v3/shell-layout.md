# V3 셸 레이아웃 와이어프레임

## Purpose
- V3 티칭패드의 전체 셸 구조를 정의한다.
- Desktop과 Tablet 두 레이아웃의 와이어프레임을 제공한다.
- 실제 산업용 펜던트 리서치 결과를 반영한 배치 근거를 명시한다.

## Parent Doc
- [README.md](./README.md)

## Last Updated
- 2026-04-23 (KST)

---

## 설계 근거 (실제 펜던트 참고)

| 설계 결정 | 참고 펜던트 | 이유 |
|-----------|------------|------|
| 상단 상태 바 고정 | 전 펜던트 공통 | 작업자가 1초 안에 로봇 상태 파악 |
| 좌측 아이콘 사이드바 | KUKA smartPAD (컨텍스트 감응 메뉴) + UR PolyScope X (좌측 헤더) | 기능 확장 시에도 네비게이션 안 깨짐 |
| 중앙 3D 뷰포트 | FAIRINO 8영역 | Unity 최대 강점 활용 |
| 하단 실행 제어 바 | UR PolyScope Footer | 탭 전환에 영향 안 받는 고정 제어 |
| 우측 상태/컨텍스트 패널 | FAIRINO Pose/IO 영역 | 좌표 + 상태를 한눈에 |
| 플랫 탭 (깊이 1) | UR 5탭 구조 (Run/Program/Move/Installation/Log) | 초보자 길 잃지 않음 |

---

## Desktop 레이아웃 (1920x1080 기준)

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│                        TopStatusBar (56px)                                  │
│  [로봇명] [연결●] [모드:수동] [속도:30%] [좌표계:Base] [안전:정상]         │
│  ─────────────────────────────────────────────────────────────────────────  │
│  [서보ON] [시작] [■정지] [⏸일시정지] [동기화] [오류초기화]                │
└─────────────────────────────────────────────────────────────────────────────┘

┌────────┬────────────────────────────────────────────┬───────────────────────┐
│NavRail │              MainContent                    │   ContextPanel        │
│ (72px) │                                             │   (320px)             │
│        │                                             │                       │
│ ┌────┐ │                                             │  ┌─ CoordStrip ─────┐ │
│ │ 🏠 │ │                                             │  │ J1: 0.0°         │ │
│ │Home │ │                                             │  │ J2: -32.0°       │ │
│ ├────┤ │                                             │  │ J3: 84.0°        │ │
│ │ 🔧 │ │  ┌─ WorkPanel ──────────────────────────┐ │  │ J4: 0.0°         │ │
│ │조작 │ │  │ Header / Summary                     │ │  │ J5: 90.0°        │ │
│ ├────┤ │  ├──────────────────────────────────────┤ │  │ J6: 0.0°         │ │
│ │ 📍 │ │  │ RobotStage (메인)                    │ │  ├─────────────────┤ │
│ │포인트│ │  │ 3D 로봇만 표시                       │ │  │ TCP              │ │
│ ├────┤ │  │ 바닥 격자 / 선택 XYZ 기즈모           │ │  │ X: -497.0 mm    │ │
│ │ ⚡ │ │  │                                         │ │  │ Y: -130.0 mm    │ │
│ │I/O  │ │  │                                         │ │  │ Z: 477.0 mm     │ │
│ ├────┤ │  └──────────────────────────────────────┘ │  │ RX: 180.0°      │ │
│ │ 📊 │ │                                             │  │ RY: 0.0°        │ │
│ │상태 │ │  ┌─ ViewportHost (보조 작업 패널) ─────┐ │  │ RZ: 90.0°       │ │
│ ├────┤ │  │ [기본] [관절] [TCP] [좌표]             │ │  └─────────────────┘ │
│ │ ❓ │ │  │ [선택된 조작 모드 콘텐츠]              │ │                       │
│ │도움 │ │  │ [TCP 3D 화살표 / RPY 보조 조작]       │ │  ┌─ StatusCard ────┐ │
│ └────┘ │  │ [설명]                                  │ │  │ 상태: 정지       │ │
│        │  │ scroll                                 │ │  │ 모드: 수동       │ │
│        │  │                                         │ │  │ Fault: 없음      │ │
│        │  │                                         │ │  │ Safety: 정상     │ │
│        │  └──────────────────────────────────────┘ │  ├─────────────────┤ │
│        │                                             │  │ 다음 행동 추천   │ │
│        │                                             │  │ "Sync를 먼저     │ │
│        │                                             │  │  실행하세요"     │ │
│        │                                             │  └─────────────────┘ │
└────────┴────────────────────────────────────────────┴───────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                        BottomBar (48px)                                      │
│  [▶실행] [■정지] [▷DryRun] [⏮Step◀] [Step▶⏭]  속도:[━━●━━━━] 30%        │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Desktop 영역 사이즈

| 영역 | 너비 | 높이 | 비고 |
|------|------|------|------|
| TopStatusBar | 100% | 56px | 고정, 스크롤 안 됨 |
| NavRail | 72px | flex | 아이콘 + 짧은 레이블, 접기 가능 |
| MainContent | flex (나머지) | flex | WorkPanel + ViewportHost(보조) |
| ContextPanel | 320px | flex | CoordStrip + StatusCard, 접기 가능 |
| BottomBar | 100% | 48px | 고정, 실행 제어 전용 |

### Desktop 콘텐츠 분할 비율
- MainContent 내에서 `WorkPanel`이 메인 로봇 표시 패널을 맡는다.
- `WorkPanel` 본체는 `RobotStage` 단일 영역으로 잠근다.
- `ViewportHost`는 `보조 작업 패널`로 두고, 조작/도움말/연결 홈을 공용 스크롤 안에서 이어 붙인다.
- Desktop 폭 우선순위는 `WorkPanel(메인) > ViewportHost(보조) > ContextPanel(컨텐츠)`로 잠근다.
- 기본 기준은 `ViewportHost` 최소 360px, `ContextPanel` 320px이다.
- `조작` active 상태에서만 내부 탭 `기본 / 관절 / TCP / 좌표`를 `ControlDockHost` 첫 줄에 표시한다.
- `NavPoints` active 상태에서는 조작 내부 탭을 숨기고, 포인트 저장/시퀀스/함수 subview만 보인다.
- 보조패널 스크롤 순서는 `ControlDockHost -> CartesianArrowsOverlayHost -> ViewportDescriptionSection`로 잠근다.
- 조작 화면에서는 실제 조작 UI와 TCP 3D 방향 조작을 먼저 보여주고, 설명 카드는 그 아래에 둔다.
- 메인 `RobotStage`에는 로봇과 그에 직접 붙는 시각화만 보이게 둔다.
- 메인 `RobotStage` 우상단에는 카메라 방향 전용 orientation gizmo를 둔다.
- `Path / Ghost / Bound / Coll / Cam` 시각화 토글 카드는 `ContextPanelTabBar` 아래, `CoordStrip` 위에 둔다.
- `Z / RX / RY / RZ`처럼 로봇을 가릴 수 있는 TCP 보조 조작 UI는 `ViewportHost` 공용 스크롤 안으로 이동시켜 겹침을 예방한다.
- `ViewportHost`와 `ContextPanel`은 가로 스크롤을 쓰지 않는다.
- 좁은 폭에서 잘림이 생기면 패널 폭을 키우지 말고 내부 요소를 줄인다.
- `ViewportPanelScroll` / `ContextPanelScroll`은 세로 전용으로 유지하고, 내부 버튼/카드는 `max-width: 100%`, `min-width: 0`, `flex-wrap` 기반으로 접는다.
- 시각화 토글 카드는 `Path / Ghost / Bound / Coll / Cam` compact chip grid로 유지한다.
- TCP/Cartesian 조작행은 한 줄 고정 금지다. `축+값+단위` 상단, `- / +` 하단의 2줄 구조로 둔다.
- 관절 조그는 `J축+입력+값`, `슬라이더`, `- / +` 버튼 행을 분리한다.
- 최신 acceptance 기준은 `viewportHorizontalVisible=False`, `contextHorizontalVisible=False`, `viewportClipped=0`, `contextClipped=0`, `scrollShare>=0.88`이다.

---

## Tablet 레이아웃 (1024x768 ~ 1366x1024)

```text
┌─────────────────────────────────────────────────────┐
│              TopStatusBar (48px, 축소)               │
│  [FR5] [●연결] [수동] [30%] [정상]                  │
│  [서보] [시작] [■] [⏸] [Sync] [리셋]               │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│                                                      │
│                  3DViewport (메인)                    │
│                                                      │
│   3D 로봇 + 트레일                                   │
│   고스트 + 경로 미리보기                             │
│                                                      │
│   ┌─ CoordOverlay (반투명) ─┐                       │
│   │ J1:0.0 J2:-32.0 J3:84.0│                       │
│   │ X:-497 Y:-130 Z:477    │                       │
│   └─────────────────────────┘                       │
│                                                      │
│  우상단 orientation gizmo                           │
│                                                      │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│           BottomSheet (스와이프 확장, 240px)          │
│                                                      │
│  ┌─ BottomTabBar ─────────────────────────────────┐ │
│  │ [쉬운조작] [관절] [TCP] [포인트] [I/O] [상태]  │ │
│  └────────────────────────────────────────────────┘ │
│                                                      │
│  (선택된 탭의 콘텐츠)                                │
│  예: 관절 탭 → 6축 슬라이더 + [복원] [미리보기]    │
│                                                      │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│                  BottomBar (48px)                     │
│  [▶] [■] [▷DryRun] [Step◀] [Step▶]  [━●━━] 30%    │
└─────────────────────────────────────────────────────┘
```

### Tablet 적응 규칙

| 요소 | Desktop | Tablet |
|------|---------|--------|
| NavRail | 좌측 72px 세로 | 제거 → BottomTabBar로 이동 |
| ContextPanel | 우측 320px | 제거 → CoordOverlay로 축소 |
| MainContent | WorkPanel + 3DViewport 수직 분할 | 3DViewport 전체화면 |
| WorkPanel | MainContent 상단 | BottomSheet (스와이프 확장) |
| 3DViewport | MainContent 하단 | 전체 화면 |

---

## NavRail 탭 구성

| 순서 | 아이콘 | 레이블 | 연결 패널 | 우선순위 |
|------|--------|--------|-----------|----------|
| 1 | 🏠 | Home | 연결 홈 + 상태 요약 | P0 |
| 2 | 🔧 | 조작 | 보조패널 내부 탭 (기본/관절/TCP/좌표) | P0 |
| 3 | 📍 | 포인트 | 포인트 저장/호출/시퀀스 | P1 |
| 4 | ⚡ | I/O | 디지털/아날로그 IO + 그리퍼 | P1 |
| 5 | 📊 | 상태 | 세션 리포트 + 이벤트 로그 | P0 |
| 6 | ❓ | 도움 | 컨텍스트 도움말 + 진단 | P0 |

### NavRail 설계 원칙
- **최대 6개 아이콘** — 그 이상은 스크롤이 필요해져서 발견성이 떨어짐
- **현재 선택된 탭**: 배경색 `Accent` + 아이콘 `White`
- **비선택 탭**: 아이콘 `MutedText`
- **접기 가능**: NavRail을 아이콘만으로 축소 (레이블 숨김, 48px)

---

## 조작 내부 탭 구성 (조작 NavRail 선택 시)

| 순서 | 레이블 | 내용 | 우선순위 |
|------|--------|------|----------|
| 1 | 기본 | Home/Ready/Folded/Zero 프리셋 버튼 + 그리퍼 | P0 |
| 2 | 관절 | 6축 슬라이더 + 수치 입력 + 단일축 조그 | P0 |
| 3 | TCP | Base/Tool/User 좌표계 선택 + XYZ/RPY 조그 | P0 |
| 4 | 좌표 | 목표 좌표 입력 → IK 계산 → 이동 | P0 |
| - | 티칭 | 별도 조작 내부 탭으로 만들지 않음. `NavPoints` 내부 `포인트 / 시퀀스 / 함수` subview에서 처리 | P1 |

### Teaching / Points 확장 정책
- 왼쪽 메인 Nav에는 새 `Program` 탭을 추가하지 않는다.
- `NavPoints`가 포인트 저장, 시퀀스 실행, 후속 함수 묶음을 소유한다.
- 조작 내부 탭은 `기본 / 관절 / TCP / 좌표`까지만 유지한다.
- 저장 포인트 기반 실행 UI는 `ViewportHost` 보조패널의 `NavPoints` 내부 subview로 표시한다.

---

## ContextPanel 구성 (Desktop 우측)

| 순서 | 섹션 | 높이 | 내용 |
|------|------|------|------|
| 1 | CoordStrip | ~200px | Joint 6축 값 + TCP XYZ/RPY (항상 표시) |
| 2 | StatusCard | ~120px | 상태/모드/Fault/Safety 요약 |
| 3 | ActionHint | ~80px | 다음 행동 추천 (컨텍스트 기반) |
| 4 | WhyItMoved | flex | 마지막 이동 설명 (조건부 표시) |

---

## TopStatusBar 구성

```text
┌─────────────────────────────────────────────────────────────────┐
│ 정보 영역                                    │ 제어 영역        │
│ [로봇이름] [●연결] [모드:수동] [속도:30%]    │ [서보ON] [시작]  │
│ [좌표계:Base] [Tool:01] [User:00]            │ [■정지] [⏸]     │
│ [안전:정상] [Fault:없음]                     │ [Sync] [리셋]    │
└─────────────────────────────────────────────────────────────────┘
```

### 상태 색상 코딩

| 상태 | 색상 | 토큰 |
|------|------|------|
| 정상/연결됨 | 녹색 | `AccentSuccess` |
| 경고/주의 | 주황 | `AccentWarning` |
| 위험/정지/에러 | 빨강 | `AccentDanger` |
| 비활성/미연결 | 회색 | `MutedText` |
| 정보/기본 | 파랑 | `AccentPrimary` |

---

## UXML 구조 초안

```
PendantV3Root (UIDocument)
├── TopStatusBar
│   ├── InfoSection
│   │   ├── RobotName
│   │   ├── ConnectionIndicator
│   │   ├── ModeLabel
│   │   ├── SpeedLabel
│   │   ├── CoordSystemLabel
│   │   └── SafetyIndicator
│   └── ControlSection
│       ├── BtnServoEnable
│       ├── BtnRun
│       ├── BtnStop
│       ├── BtnPause
│       ├── BtnSync
│       └── BtnResetError
├── MiddleSection (flex-direction: row)
│   ├── NavRail
│   │   ├── NavItem[Home]
│   │   ├── NavItem[Motion]
│   │   ├── NavItem[Points]
│   │   ├── NavItem[IO]
│   │   ├── NavItem[Status]
│   │   └── NavItem[Help]
│   ├── MainContent (flex: 1)
│   │   ├── WorkPanel
│   │   │   ├── RobotStage
│   │   └── ViewportHost (보조)
│   │       └── ControlDock
│   │           ├── MotionSubTabs (기본/관절/TCP/좌표, NavMotion 전용)
│   │           └── ActivePanelHost
│   └── ContextPanel
│       ├── CoordStrip
│       ├── StatusCard
│       ├── ActionHint
│       └── WhyItMoved
└── BottomBar
    ├── BtnPlay
    ├── BtnStopBottom
    ├── BtnDryRun
    ├── BtnStepBack
    ├── BtnStepForward
    └── SpeedSlider
```

---

## USS 토큰 매핑 (UIDesignTokens → USS)

```css
:root {
    /* Colors — UIDesignTokens.RobotControlV2.Colors 대응 */
    --rc-backdrop: rgb(18, 18, 24);
    --rc-left-rail: rgb(24, 24, 32);
    --rc-card: rgb(30, 30, 40);
    --rc-card-alt: rgb(36, 36, 48);
    --rc-accent: rgb(80, 140, 255);
    --rc-success: rgb(60, 200, 120);
    --rc-warning: rgb(255, 180, 40);
    --rc-danger: rgb(255, 70, 70);
    --rc-title-text: rgb(240, 240, 245);
    --rc-muted-text: rgb(140, 140, 160);

    /* Sizes — UIDesignTokens.RobotControlV2.Sizes 대응 */
    --rc-nav-rail-width: 72px;
    --rc-context-panel-width: 320px;
    --rc-top-bar-height: 56px;
    --rc-bottom-bar-height: 48px;
    --rc-work-tab-height: 40px;

    /* Typography — 기본 17px 확정 */
    --rc-font-size: 17px;
    --rc-font-size-small: 14px;
    --rc-font-size-label: 12px;
    --rc-font-size-header: 20px;
    --rc-font-family: 'Noto Sans KR';
}
```
