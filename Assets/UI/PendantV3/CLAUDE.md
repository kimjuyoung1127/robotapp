# Assets/UI/PendantV3/

Pendant V3 UI Toolkit 에셋 루트.

## 역할
- V3 셸 UXML/USS
- PanelSettings / TextSettings
- 아이콘 atlas와 popup 마크업

## 핵심 규칙
1. UXML 구조가 레이아웃 SSOT다. 런타임에서 per-frame 재배치 훅 만들지 않는다.
2. 스타일 토큰은 `--rc-*`, 클래스는 `.rc-*`, 요소 name은 `PascalCase`로 고정한다.
3. 루트 셸은 `TopStatusBar`, `NavRail`, `MainContent`, `ContextPanel`, `BottomBar`, `PopupLayer` 이름을 유지한다.
4. 임시 실험용 인라인 `style=`은 최소화하고 최종값은 USS로 올린다.

## 파일 인덱스
- `pendant-v3.uxml` — V3 최소 셸
- `pendant-v3.uss` — 루트 토큰/레이아웃
- `pendant-v3-tablet.uss` — tablet override 자리
- `connection-home.uxml` / `.uss` — 연결 홈 패널
- `coord-strip.uxml` / `.uss` — 우측 좌표 패널
- `status-card.uxml` / `.uss` — 우측 상태 패널
- `easy-motion-panel.uxml` / `.uss` — 쉬운 조작 패널
- `joint-jog-panel.uxml` / `.uss` — 관절 조그 첫 슬라이스 패널
- `tcp-jog-panel.uxml` / `.uss` — TCP 조그 첫 슬라이스 패널
- `point-move-panel.uxml` / `.uss` — 포인트 이동 최소 scaffold 패널
- `cartesian-arrows-overlay.uxml` / `.uss` — 뷰포트 3D 방향 조작 오버레이
- `safety-diagnostics-panel.uxml` / `.uss` — 안전/진단 카드 + 복구/이벤트 scaffold
- `fault-overlay.uxml` / `.uss` — fault blocking overlay scaffold
- `viewport-toolbar.uxml` / `.uss` — 2C-2 뷰포트 보조 툴바 scaffold
- `workspace-boundary.uss` — 경계/충돌 토글 스타일 보조 토큰
- `help-panel.uxml` / `.uss` — 2D NavHelp 컨텍스트 도움말 scaffold
- `PanelSettings/` — PanelSettings + TextSettings
- `icons/` — pendant 전용 아이콘/atlas
- `popups/action-confirm.uxml` — 위험 동작 확인 팝업 body scaffold
- `popups/unsaved-confirm.uxml` — 미저장 변경 확인 팝업 body scaffold
- `popups/` — 공통 팝업 UXML
