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
- `PanelSettings/` — PanelSettings + TextSettings
- `icons/` — pendant 전용 아이콘/atlas
- `popups/` — 공통 팝업 UXML
