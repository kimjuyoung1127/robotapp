# V3 셸 레이아웃 와이어프레임

## Purpose
- V3 티칭패드의 전체 셸 구조를 정의한다.
- Desktop과 Tablet 두 레이아웃의 와이어프레임을 제공한다.
- 실제 산업용 펜던트 리서치 결과를 반영한 배치 근거를 명시한다.

## Parent Doc
- [README.md](./README.md)

## Last Updated
- 2026-04-03 (KST)

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
│ ┌────┐ │  ┌─ WorkTabBar ──────────────────────────┐ │  ┌─ CoordStrip ─────┐ │
│ │ 🏠 │ │  │ [쉬운조작] [관절] [TCP] [포인트] ... │ │  │ J1: 0.0°         │ │
│ │Home │ │  └──────────────────────────────────────┘ │  │ J2: -32.0°       │ │
│ ├────┤ │                                             │  │ J3: 84.0°        │ │
│ │ 🔧 │ │  ┌─ WorkPanel ──────────────────────────┐ │  │ J4: 0.0°         │ │
│ │조작 │ │  │                                      │ │  │ J5: 90.0°        │ │
│ ├────┤ │  │  (선택된 탭에 따라 내용 변경)         │ │  │ J6: 0.0°         │ │
│ │ 📍 │ │  │                                      │ │  ├─────────────────┤ │
│ │포인트│ │  │  예: 관절 탭 선택 시                │ │  │ TCP              │ │
│ ├────┤ │  │  [J1 ◀━━━━━●━━━━━▶ 0.0°]            │ │  │ X: -497.0 mm    │ │
│ │ ⚡ │ │  │  [J2 ◀━━●━━━━━━━▶ -32.0°]           │ │  │ Y: -130.0 mm    │ │
│ │I/O  │ │  │  [J3 ◀━━━━━━━●━▶ 84.0°]            │ │  │ Z: 477.0 mm     │ │
│ ├────┤ │  │  ...                                  │ │  │ RX: 180.0°      │ │
│ │ 📊 │ │  │  [복원] [미리보기] [적용]             │ │  │ RY: 0.0°        │ │
│ │상태 │ │  │                                      │ │  │ RZ: 90.0°       │ │
│ ├────┤ │  └──────────────────────────────────────┘ │  └─────────────────┘ │
│ │ ❓ │ │                                             │                       │
│ │도움 │ │  ┌─ 3DViewport ────────────────────────┐ │  ┌─ StatusCard ────┐ │
│ └────┘ │  │                                      │ │  │ 상태: 정지       │ │
│        │  │   3D 로봇 + 프레임 + 트레일          │ │  │ 모드: 수동       │ │
│        │  │   고스트 + 경로 미리보기              │ │  │ Fault: 없음      │ │
│        │  │                                      │ │  │ Safety: 정상     │ │
│        │  │  [Base축] [Tool축] [궤적] [리셋]     │ │  ├─────────────────┤ │
│        │  └──────────────────────────────────────┘ │  │ 다음 행동 추천   │ │
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
| MainContent | flex (나머지) | flex | WorkTabBar(40px) + WorkPanel + 3DViewport |
| ContextPanel | 320px | flex | CoordStrip + StatusCard, 접기 가능 |
| BottomBar | 100% | 48px | 고정, 실행 제어 전용 |

### Desktop 콘텐츠 분할 비율
- MainContent 내에서 WorkPanel과 3DViewport는 **수직 분할**
- 기본 비율: WorkPanel 45% / 3DViewport 55%
- 드래그로 리사이즈 가능 (최소 30% / 최대 70%)

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
│   3D 로봇 + 프레임 + 트레일                         │
│   고스트 + 경로 미리보기                             │
│                                                      │
│   ┌─ CoordOverlay (반투명) ─┐                       │
│   │ J1:0.0 J2:-32.0 J3:84.0│                       │
│   │ X:-497 Y:-130 Z:477    │                       │
│   └─────────────────────────┘                       │
│                                                      │
│  [Base축] [Tool축] [궤적] [리셋]                    │
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
| 2 | 🔧 | 조작 | WorkTabBar (쉬운조작/관절/TCP) | P0 |
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

## WorkTabBar 탭 구성 (조작 NavRail 선택 시)

| 순서 | 레이블 | 내용 | 우선순위 |
|------|--------|------|----------|
| 1 | 쉬운 조작 | Home/Ready/Folded/Zero 프리셋 버튼 | P0 |
| 2 | 관절 | 6축 슬라이더 + 수치 입력 + 단일축 조그 | P0 |
| 3 | TCP | Base/Tool/User 좌표계 선택 + XYZ/RPY 조그 | P0 |
| 4 | 포인트 이동 | 목표 좌표 입력 → IK 계산 → 이동 | P0 |
| 5 | 티칭 | 포인트 시퀀스 편집/재생 | P1 |

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
│   │   ├── WorkTabBar
│   │   ├── WorkPanel (flex: 0.45)
│   │   └── ViewportHost (flex: 0.55)
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

    /* Typography */
    --rc-font-size: 15px;
    --rc-font-size-small: 13px;
    --rc-font-size-label: 11px;
}
```
