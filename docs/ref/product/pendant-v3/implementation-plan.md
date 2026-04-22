# V3 티칭패드 구현 플랜

## Purpose
- V3 티칭패드의 구현 순서, 잠금 규칙, 종료 조건, 검증 루프를 한 문서에 고정한다.
- 실제 구현은 이 문서의 실행 단위와 `unityctl` 레시피를 기준으로 진행한다.

## Parent Docs
- [README.md](./README.md)
- [migration-strategy.md](./migration-strategy.md)
- [unityctl-recipes.md](./unityctl-recipes.md)
- [AGENT-CONTRACT.md](./AGENT-CONTRACT.md)

## Related Docs
- [shell-layout.md](./shell-layout.md)
- [feature-connection-status.md](./feature-connection-status.md)
- [feature-safety-controls.md](./feature-safety-controls.md)
- [feature-3d-viewport.md](./feature-3d-viewport.md)
- [static-checks.md](./static-checks.md)
- [verify-v3.json](./verify-v3.json)

## Last Updated
- 2026-04-22 (KST)

## Context
V2(uGUI) 셸이 플레이스홀더 상태로 존재하는 `codex/robotcontrol-shell` 브랜치에서,
UI Toolkit 기반 V3를 `codex/robotcontrol-v3-toolkit` 브랜치에 별도 구현한다.
초기 실행은 `Phase 0A~3C`의 잘게 쪼갠 단위로 진행하고, 그 결과를 바탕으로 Phase 4에서 채택 결정 → Phase 5~8에서 전체 기능 완성으로 이어간다.

SSOT 3개 문서(V1 백로그 117항목, Feature Matrix, Soft Teaching Pad UX) +
주인님 추가 요구 13개 + 제안 채택 10개 = **총 140개 항목** 누락 없이 매핑.

### 추가 확정 기능 (SSOT 반영 필요)

| ID | 기능 | Phase |
|----|------|-------|
| U1 | 그리퍼 개폐 (쉬운 조작 탭) | 2B-1 |
| U2 | 블록 추가 + 단계별 루프 생성 (논리/이동 블록) | 5 |
| U3 | 가독성 아이콘 → 함수별 매핑 | 전체 |
| U4 | 버튼 클릭 시 확인 팝업 모달 | 2D |
| U5 | 인풋 수정 시 0 자동선택 + 즉시 반영 | 2B-2 |
| U6 | 로컬스토리지 임시 값 저장 (PlayerPrefs/JSON) | 3B |
| U7 | Undo/Redo (BottomBar 상시 표시) | 3B |
| U8 | Phase 1 후 디자인 시안 리뷰 게이트 | 1B |
| U9 | UI Toolkit 공식문서/스킬 참조 필수 | 전체 |
| U10 | 수정/삭제/창닫기 시 확인 팝업 | 2D |
| U11 | 논리명령(IF/ELSE/LOOP) + 이동명령(MoveJ/L/C) 블록 | 5 |
| U12 | 데카르트 3D 화살표 방향 기기조작 | 2B-3 |
| U13 | MoveC (원호 이동) 블록 | 5 |
| A1 | 작업공간 경계 시각화 (도달 범위 구체) | 2C-2 |
| A2 | 자동 재연결 (3초 재시도 + 카운트다운) | 3B |
| A3 | 속도 오버라이드 실시간 슬라이더 (BottomBar) | 1A |
| A4 | 경로 충돌 사전 검출 (빨간 하이라이트) | 2C-2 |
| A5 | 스크린샷/상태 캡처 버튼 (PNG+JSON) | 6 |
| A6 | 드래그 티칭 모드 통합 → 블록 자동 변환 | 7 |
| A7 | 다중 포인트 경로 전체 미리보기 | 5 |
| A8 | 조그 감도 설정 (모드별 자동 적용) | 7 |
| A9 | 연결 QR 코드 (태블릿 스캔 연결) | 8 |

---

## 선행 잠금 결정 (공식문서 기준)

### 1. Panel topology
- V3는 `Assets/UI/PendantV3/PanelSettings/PendantV3PanelSettings.asset` 하나를 메인 SSOT로 사용한다.
- `TopStatusBar`, `NavRail`, `MainContent`, `ContextPanel`, `BottomBar`, `Popups`, `BottomSheet`는 채택 결정 전까지 같은 PanelSettings를 공유한다.
- 다중 PanelSettings는 `Sort Order`, 입력 우선순위, 포커스 handoff까지 같은 턴에 문서화할 수 있을 때만 허용한다.

### 2. 입력 시스템
- V3는 `Input System Package`를 기준으로 구현한다.
- V3 범위에서 `UnityEngine.Input` 직접 사용은 금지한다.
- 런타임 `EventSystem`은 씬당 1개만 허용하고 `InputSystemUIInputModule`을 단일 기준으로 쓴다.

### 3. 포커스 / 네비게이션
- 기본 포커스 순서는 `TopStatusBar -> NavRail -> WorkTabBar -> WorkPanel -> ContextPanel -> BottomBar -> Popup`으로 고정한다.
- 장식 요소는 `focusable = false`, `tabIndex = -1`을 기본으로 한다.
- 팝업은 포커스를 내부에 가두고 닫힐 때 호출자에게 복원한다.

### 4. 바인딩 소유권
- 구조/스타일은 `UXML/USS`, 런타임 상태 쓰기는 `C# Binder`가 소유한다.
- 동일 요소에 `Runtime Data Binding`과 수동 `C# write`를 동시에 사용하지 않는다.
- 로봇/연결/모션 상태는 `RobotControlViewState`, V3 전용 UI 상태는 `PendantV3LocalState`로 분리한다.

### 5. 텍스트 / 그래픽 자산
- `PendantV3TextSettings.asset`를 별도 생성해 PanelSettings에 연결한다.
- 기본 폰트는 `Noto Sans KR`, fallback font 목록을 명시하고 `Korean Line Breaking Rules`를 켠다.
- 정적 아이콘은 `Sprite Atlas`로 묶고, 동적 생성 이미지에만 `dynamic atlas`를 의존한다.

### 6. 리스트 / 성능
- 10개 이상 반복 항목, 재정렬 목록, 로그 목록은 `ListView` 우선으로 구현한다.
- 기본 가상화는 `FixedHeight`, `fixedItemHeight` 명시는 필수다.
- 팝업/시트/패널 전환은 `translate`, `scale`, `opacity` 중심으로 애니메이션하고 layout 속성 애니메이션은 금지한다.

---

## Pre-Phase: 폴더 구조 + CLAUDE.md 인덱스 확립

### 목표
구현 시작 전에 새 폴더 구조를 만들고, 각 폴더에 CLAUDE.md 인덱스를 배치한다.
루트 CLAUDE.md에 V3 폴더 링크를 추가한다.

### 규칙
1. **새 폴더마다 `CLAUDE.md`** — 폴더 역할 + 파일 인덱스 + 규칙
2. **새 C# 파일 최상단** — `// Folder: {Module} - {기능 설명}` 헤더 (code-patterns.md §8.1)
3. **새 UXML/USS 파일 최상단** — `<!-- Pendant V3 - {영역}: {설명} -->` 주석
4. **루트 CLAUDE.md** — 작업별 링크 허브에 V3 경로 추가

### 생성할 CLAUDE.md 목록 (구현 최우선 — 코드 전에 생성)

| 경로 | 내용 |
|------|------|
| `Assets/UI/PendantV3/CLAUDE.md` | V3 UXML/USS 에셋 루트. 셸 구조, 토큰, 아이콘, 네이밍 규칙, 파일 인덱스 |
| `Assets/UI/PendantV3/icons/CLAUDE.md` | 아이콘 에셋. 명명 규칙(`icon-{기능}.png`), 아이콘→기능 매핑표 |
| `Assets/UI/PendantV3/popups/CLAUDE.md` | 팝업 UXML. 트리거 조건표, 팝업 종류, 버튼 순서 규칙 |
| `Assets/Scripts/UI/RobotControlV3/CLAUDE.md` | V3 Controller 루트. 생명주기(OnEnable/OnDisable), 바인딩 패턴, ViewState 경계, 300줄 규칙 |

### 루트 CLAUDE.md 추가 항목 (작업별 링크 허브에 추가)
```
### RobotControl V3 (UI Toolkit)
- V3 UXML/USS 에셋: `Assets/UI/PendantV3/CLAUDE.md`
- V3 Controller: `Assets/Scripts/UI/RobotControlV3/CLAUDE.md`
- V3 설계 문서: `docs/ref/product/pendant-v3/README.md`
```

### 루트 CLAUDE.md 추가 항목
```
### RobotControl V3 (UI Toolkit)
- V3 UXML/USS 에셋: `Assets/UI/PendantV3/CLAUDE.md`
- V3 Controller: `Assets/Scripts/UI/RobotControlV3/CLAUDE.md`
- V3 설계 문서: `docs/ref/product/pendant-v3/README.md`
```

### 검증
- 모든 새 폴더에 CLAUDE.md 존재 확인
- 루트 CLAUDE.md에 V3 링크 확인

---

## 세분화된 실행 순서 (권장)

초기 V3 구현은 큰 Phase 이름보다 아래 실행 단위를 기준으로 커밋한다.
원칙은 `한 단위 = 한 검증 루프 = 한 커밋 후보`다.
각 실행 단위에서 사용할 실제 `unityctl` 명령은 [unityctl-recipes.md](./unityctl-recipes.md)를 SSOT로 본다.

| 실행 단위 | 목표 | 완료 기준 |
|------|------|------|
| `0A` | UI Toolkit 인프라 자산 고정 | PanelSettings/TextSettings/SpriteAtlas/UIDocument/호출 surface 생성 |
| `0B` | 루트 셸 뼈대 작성 | `pendant-v3.uxml` + `pendant-v3.uss` + 최소 `RobotControlV3.unity` 확보 |
| `0C` | 입력/포커스 계약 반영 | EventSystem/InputModule/기본 포커스 순서 점검 |
| `1A` | Desktop 셸 고정 | TopStatusBar/NavRail/BottomBar/ContextPanel desktop 배치 |
| `1B` | Tablet 셸 고정 | BottomSheet + tablet class 전환 + 시안 리뷰 |
| `1C` | 탭 지속성/로컬 레이아웃 상태 | `viewDataKey`/`LocalSettingsStore` 적용 |
| `2A-1` | 연결 홈 | ConnectionHome 레이아웃 완료 |
| `2A-2` | 상태/좌표 우측 패널 | StatusCard/CoordStrip 완료 |
| `2B-1` | 쉬운 조작 | Preset + 그리퍼 버튼 완료 |
| `2B-2` | 관절 조그 | Slider/숫자입력/0 자동선택 완료 |
| `2B-3` | TCP 조그 | Base/Tool/User + 3D 화살표 오버레이 완료 |
| `2B-4` | 포인트 이동 | 좌표 입력 + IK + MoveJ/L/C 선택 UI 완료 |
| `2C-1` | 안전/진단 | 배너 + Fault overlay + diagnostics 완료 |
| `2C-2` | 3D 뷰포트 툴바 | toolbar + 작업공간 경계 + 충돌 표시 완료 |
| `2D` | 팝업/도움말 | 모든 확인/복구/도움말 팝업 완료 |
| `3A` | Binder/Scene bootstrap | `PendantV3Binder` + `PendantV3SceneCoordinator` 완료 |
| `3B` | 로컬 서비스 | Undo/Redo, LocalSettings, AutoReconnect 완료 |
| `3C` | Mock 종단 검증 | Mock 연결부터 tablet 전환까지 e2e smoke 완료 |

---

## Phase Loop Governance

V3는 각 실행 단위를 `SSOT 정합성 루프`로 닫는다.
원칙은 `한 실행 단위 = 한 문서 기준선 = 한 검증 루프 = 한 커밋 후보`다.

### 루프 순서

1. **실행 단위 선언**
   - 현재 작업이 `0A`, `0B`, `0C`, `1A`처럼 어느 실행 단위인지 먼저 고정한다.
   - 서로 다른 실행 단위를 한 턴/한 커밋에 섞지 않는다.

2. **SSOT 입력 잠금**
   - 공통 입력: `README.md`, `AGENT-CONTRACT.md`, 이 문서, `unityctl-recipes.md`, `static-checks.md`
   - 기능 입력: 현재 실행 단위와 직접 연결된 `feature-*.md`

3. **범위 안 구현**
   - 현재 실행 단위 종료 조건을 만족시키는 코드/자산만 추가한다.
   - 다음 실행 단위 요구사항을 앞당겨 넣지 않는다.

4. **정적 게이트**
   - `pwsh -NoLogo -NoProfile -File ./docs/ref/product/pendant-v3/check-v3-static.ps1`
   - Blocker가 하나라도 나오면 완료 주장 금지

5. **컴파일 게이트**
   - `unityctl check --type compile`
   - compile green이 아니면 다음 증빙 단계로 넘어가지 않는다.

6. **증빙 게이트**
   - 실행 단위에 맞는 `unityctl` 레시피를 실행한다.
   - 산출물은 가능하면 `Artifacts/V3/` 기준으로 모은다.

7. **SSOT parity review**
   - 문서 산출물이 실제 파일/씬/엔트리로 존재하는지 확인
   - SceneCatalog / BuildSettings / validation recipe가 서로 맞는지 확인
   - 콘솔 신규 치명 에러가 없는지 확인
   - 다음 실행 단위 전제가 실제로 준비됐는지 확인

8. **문서/로그 동기화**
   - 문서 변경이 있었다면 같은 턴에 `docs/daily/MM-DD/` 로그를 남긴다.
   - 로그만 남기고 멈추지 말고, 같은 턴에 최소 다음 실행 단위 1개를 착수한다.

9. **커밋 후보 정리**
   - 현재 실행 단위 범위만 묶는다.
   - unrelated 변경은 섞지 않는다.

### 실행 단위별 parity checklist

| 실행 단위 | parity 기준 |
|------|------|
| `0A` | PanelSettings/TextSettings/SpriteAtlas/UIDocument/호출 surface 존재 |
| `0B` | `pendant-v3.uxml`/`.uss` + `RobotControlV3.unity` + 최소 hierarchy 존재 |
| `0C` | `EventSystem` 1개 + `InputSystemUIInputModule` + 기본 포커스 순서 + non-viewport 포인터 차단 + popup focus trap 기본형 |
| `1A` | Desktop 셸 구조와 치수가 `shell-layout.md`와 일치 |
| `1B` | Tablet 전환 구조와 시안 리뷰 증빙 확보 |
| `1C` | `viewDataKey` / `LocalSettingsStore` 경계가 문서와 일치 |
| `2A~2D` | 각 패널/팝업 산출물이 feature 문서와 일치 |
| `3A~3C` | Binder/service/mock flow가 문서 종료 조건과 검증 번들을 모두 충족 |

### 완료 보고 형식

각 실행 단위 종료 보고는 아래 3줄을 최소로 포함한다.

1. 문서 산출물과 실제 산출물이 어디까지 일치하는지
2. 어떤 검증 레시피를 통과했는지
3. 남은 불일치가 무엇인지

---

## Phase별 unityctl 레시피 매핑

| 실행 단위 | 기본 레시피 |
|------|------|
| `0A` | `Session Bootstrap` + `Recipe 0A: Infrastructure Assets` |
| `0B`, `0C`, `1A`, `1B`, `1C` | `Recipe 0B-1C: Shell And Layout` |
| `2A-1`, `2A-2`, `2B-1`, `2B-2`, `2B-3`, `2B-4`, `2C-1`, `2C-2`, `2D` | `Recipe 2A-2D: Panel UI` |
| `3A`, `3B`, `3C` | `Recipe 3A-3C: Binder And Mock Flow` |
| `2C-2`, `3C`, `4` | `Recipe 3D Viewport Verification` |
| compile/입력/포커스 이상 조사 | `Diagnostics Recipe` |
| `4` 전 최종 확인 | `Preflight Recipe` |

### 명령 작성 원칙
- 명령은 가능하면 `--json`으로 실행해 에이전트와 사람이 같은 산출물을 본다.
- 세션 시작 시 `editor select`와 `editor current`로 현재 프로젝트 선택 상태를 먼저 확인한다.
- 검증용 스크린샷은 `Artifacts/V3/` 아래에 모은다.
- 반복 검증이 굳어지면 `workflow verify --file ...`로 번들화한다.

---

## Phase 0 묶음: 인프라와 셸 기반 (0A~0C)

### 목표
UI Toolkit 런타임 기반을 프로젝트에 확립한다.

### `0A` 인프라 자산
- `PendantV3PanelSettings.asset`
- `PendantV3TextSettings.asset`
- `PendantV3.spriteatlas`
- `PendantV3Document.cs`
- `PendantV3BootstrapTool` 호출 경로 (`unityctl exec` + editor menu)

### `0B` 루트 셸 뼈대
- `pendant-v3.uxml`
- `pendant-v3.uss`
- TopBar / NavRail / Main / Context / Bottom 5영역만 먼저 고정
- 최소 `RobotControlV3.unity` 씬 생성 + `UIDocument` 연결

### `0C` 입력 / 포커스 계약
- 단일 `EventSystem`
- `InputSystemUIInputModule`
- 기본 포커스 순서
- popup focus trap 기본 구현

### 산출물
| 파일 | 설명 |
|------|------|
| `Assets/UI/PendantV3/pendant-v3.uss` | 루트 USS — `--rc-*` 디자인 토큰 변수 정의 |
| `Assets/UI/PendantV3/pendant-v3.uxml` | 루트 UXML — 빈 셸 5영역 (TopBar/NavRail/Main/Context/Bottom) |
| `Assets/UI/PendantV3/PanelSettings/PendantV3PanelSettings.asset` | PanelSettings ScriptableObject |
| `Assets/UI/PendantV3/PanelSettings/PendantV3TextSettings.asset` | UITK Text Settings — Noto Sans KR + fallback + line breaking |
| `Assets/UI/PendantV3/icons/PendantV3.spriteatlas` | 정적 아이콘 Sprite Atlas |
| `Assets/Scripts/UI/RobotControlV3/PendantV3Document.cs` | UIDocument MonoBehaviour — 씬 부트스트랩 |
| `Assets/Scenes/RobotControlV3.unity` | `0B~1C` 레이아웃 검증용 최소 V3 씬 |
| `Assets/UI/PendantV3/icons/` | 아이콘 에셋 폴더 + 기본 아이콘 세트 (U3) |

### SSOT 매핑
- #95 Authored-First → UIDocument + UXML authored 패턴 확립
- #96 UIDesignTokens → USS 토큰 변수로 변환
- #98 UILayoutProfile → C# 클래스 토글 방식으로 대체
- U3 가독성 아이콘 폴더 구조 확립
- U9 UI Toolkit 공식문서/스킬 참조 — `/ui-toolkit-verify` 실행으로 시작
- Panel topology 단일 SSOT 확정
- Input System / EventSystem 계약 확정
- Text Settings / Sprite Atlas 계약 확정

### 검증
```bash
unityctl check --type compile
# /ui-toolkit-verify 스킬로 가용성 재확인
```

---

## Phase 1 묶음: 반응형 셸 (1A~1C)

### 목표
5영역 셸이 Desktop/Tablet 양쪽에서 동작하는 빈 레이아웃을 만든다.
**★ 완료 후 주인님 디자인 시안 리뷰 (U8)**

### `1A` Desktop 셸
- Desktop 기준 TopStatusBar, NavRail, MainContent, ContextPanel, BottomBar 배치 고정
- 속도 슬라이더, Undo/Redo 위치만 먼저 고정

### `1B` Tablet 셸 + 시안 리뷰
- BottomSheet 구조
- Desktop ↔ Tablet class 전환
- 이 단위 완료 후 디자인 시안 리뷰

### `1C` 탭 지속성 / 로컬 레이아웃 상태
- `viewDataKey` 적용 범위 고정
- `LocalSettingsStore`에 탭 선택/시트 확장/split 비율 저장

### 산출물
| 파일 | 설명 |
|------|------|
| `pendant-v3-tablet.uss` | 태블릿 USS 오버라이드 |
| `top-status-bar.uxml` + `.uss` | 상단 바 구조 (빈 라벨) |
| `nav-rail.uxml` + `.uss` | 좌측 6아이콘 NavRail (아이콘 매핑) |
| `work-tab-bar.uxml` | 조작 탭 바 (쉬운조작/관절/TCP/포인트이동) |
| `bottom-bar.uxml` + `.uss` | 하단 실행 제어 바 + Undo/Redo + 속도 슬라이더 |
| `coord-strip.uxml` + `.uss` | 우측 좌표 표시 스트립 |
| `status-card.uxml` + `.uss` | 우측 상태 요약 카드 |
| `NavRailController.cs` | 탭 전환 로직 |
| `PendantV3LayoutController.cs` | Desktop↔Tablet 전환 (GeometryChangedEvent) |

### SSOT 매핑
- #101 태블릿 레이아웃 — BottomSheet 구조
- #102 반응형 설계 — Desktop+Tablet 분기
- #104 바텀시트 — Tablet UI
- #105 터치 친화 — 44px 이상 버튼
- U7 Undo/Redo BottomBar 배치 (빈 버튼)
- A3 속도 슬라이더 BottomBar 배치 (빈 슬라이더)
- U3 NavRail 아이콘 매핑 (Home🏠/조작🔧/포인트📍/IO⚡/상태📊/도움❓)

### 게이트
```
★ Phase 1 완료 후 주인님 시안 리뷰 → 디자인 변경 필요 시 반영 후 Phase 2 진행
```

### 검증
```bash
unityctl check --type compile
unityctl scene open --scene RobotControlV3
unityctl screenshot capture
```

---

## Phase 2A: P0 패널 — 연결/상태 (1세션)

### 목표
TopStatusBar + 연결 홈 + StatusCard 레이아웃 완성. 데이터 바인딩 없음.

### `2A-1` 연결 홈
- `connection-home.uxml`
- 빠른 상태
- 다음 행동 추천
- 상태별 `Primary Next Action` 1개만 강조
- "왜 이걸 먼저 해야 하는지" 보조 설명 포함

### `2A-2` 상태/좌표 우측 패널
- `StatusCardController.cs`
- `CoordStrip` 레이아웃
- 연결 요약과 좌표 요약 분리

### 산출물
| 파일 | 설명 |
|------|------|
| `TopStatusBarController.cs` | 정보(로봇명/연결/모드/속도/안전) + 제어 버튼 6개 |
| `connection-home.uxml` | 연결 카드 + 빠른 상태 + 다음 행동 추천 |
| `ConnectionHomeController.cs` | 연결 홈 로직 |
| `StatusCardController.cs` | 우측 상태 요약 |
| `qr-connect.uxml` (구조만) | 연결 QR 코드 표시 영역 (A9, Phase 8에서 구현) |

### SSOT 매핑 (14개)
- #1 연결/해제/Enable, #3 모드, #4 Drag, #5 서보, #6 Sync, #7 에러초기화
- #8 연결상태, #9 모드, #10 Fault/Safety, #11 Tool/Wobj/Load
- #12 속도, #15 RC-01 연결홈, #99 한국어

### 검증
```bash
unityctl check --type compile
unityctl screenshot capture
```

---

## Phase 2B: P0 패널 — 조그/모션 (1세션)

### 목표
4개 조작 탭 + 그리퍼 개폐 + 3D 화살표 조작의 레이아웃 완성.

### `2B-1` 쉬운 조작
- Home / Ready / Folded / Zero
- 그리퍼 열기 / 닫기

### `2B-2` 관절 조그
- Joint slider
- 단일축 조그
- 숫자 입력 0 자동선택

### `2B-3` TCP 조그
- Base / Tool / User 선택
- XYZ / RPY 조그
- 3D 화살표 오버레이

### `2B-4` 포인트 이동
- 좌표 입력
- IK 결과 표시
- MoveJ / MoveL / MoveC 선택 UI

### 산출물
| 파일 | 설명 |
|------|------|
| `easy-motion-panel.uxml` + `EasyMotionController.cs` | Home/Ready/Folded/Zero (88x88px) + **그리퍼 열기/닫기** (U1) |
| `joint-jog-panel.uxml` + `JointJogController.cs` | 슬라이더 + 단일축 + **인풋 0 자동선택** (U5) |
| `tcp-jog-panel.uxml` + `TcpJogController.cs` | Base/Tool/User + XYZ/RPY ± + **3D 화살표 방향 조작** (U12) |
| `point-move-panel.uxml` + `PointMoveController.cs` | 좌표 입력 + IK + MoveJ/MoveL/**MoveC** 선택 |
| `cartesian-arrows-overlay.uxml` | 3D 뷰포트 위 데카르트 화살표 오버레이 (U12) |

### SSOT 매핑 (24개 + 추가 5개)
- #16 조작 탭, #20~#22 Base/Tool/Wobj Jog, #23~#25 TCP XYZ/RPY/증분
- #27~#30 단일축/다축/Ring/Numeric, #31~#35 MoveJ/MoveL/포인트이동/IK
- #37 쉬운조작, #38 복원, #45 DryRun, #103 큰 버튼
- U1 그리퍼 개폐, U5 인풋 자동선택, U12 데카르트 화살표
- A3 속도 오버라이드 슬라이더 (BottomBar 연동)
- U13 MoveC 원호 이동 (선택 UI만, 로직은 Phase 5)

### 인풋 UX 규칙 (U5)
- TextField `focusIn` 이벤트 → 기존 값 전체 선택 (SelectAll)
- 입력 즉시 미리보기 반영 (onChange, Enter 불필요)
- NaN/Infinity 입력 시 즉시 거부 + 이전값 복원

### 검증
```bash
unityctl check --type compile
unityctl screenshot capture
```

---

## Phase 2C: P0 패널 — 안전/좌표/3D (1세션)

### 목표
안전 배너, 좌표 표시, 3D 뷰포트 툴바 + 작업공간 경계 + 충돌 사전 검출.

### `2C-1` 안전 / 진단
- `safety-banner`
- `fault-overlay`
- `diagnostics-panel`
- 위험 버튼 결과 설명 문구
- `정지 = E-Stop 아님` 상시 안내

### `2C-2` 뷰포트 툴바 / 경계 / 충돌
- `viewport-toolbar`
- 작업공간 경계 토글
- 충돌 사전 검출 강조

### 산출물
| 파일 | 설명 |
|------|------|
| `safety-banner.uxml` + `.uss` | 4단계 배너 (정상/경고/Fault/SafetyStop) |
| `fault-overlay.uxml` | 풀스크린 Fault + 해결순서 |
| `diagnostics-panel.uxml` + `DiagnosticsController.cs` | 에러 3단(상태→원인→해결) + 로그 |
| `CoordStripController.cs` | Joint 6축 + TCP XYZ/RPY 실시간 |
| `viewport-toolbar.uxml` | Base축/Tool축/궤적/고스트/경계/카메라 |
| `ActionHintController.cs` | 다음 행동 추천 카드 |
| `workspace-boundary.uss` | 작업공간 경계 토글 스타일 (A1) |

### SSOT 매핑 (18개 + 추가 2개)
- #13~#14 Joint/TCP/3D 표시, #26 좌표계 시각화
- #39~#44 고스트/경로/비교/궤적/충돌/3D시뮬
- #46~#49 도달불가/특이점/JointLimit/큰차이
- #17 RC-03 프리뷰
- A1 작업공간 경계 시각화 (도달 범위 반투명 구체)
- A4 경로 충돌 사전 검출 (빨간 하이라이트)

### 검증
```bash
unityctl check --type compile
```

---

## Phase 2D: P0 패널 — 팝업/도움말 (1세션)

### 목표
모든 확인/경고/복구/닫기 팝업 + 컨텍스트 도움말 완성.

### 산출물
| 파일 | 설명 |
|------|------|
| `popups/move-confirm.uxml` | 이동 확인 (위험 요약) |
| `popups/warning-dialog.uxml` | 경고 |
| `popups/recovery-dialog.uxml` | 복구 안내 (해결 순서) |
| `popups/first-run-guide.uxml` | 첫 실행 가이드 |
| `popups/unsaved-confirm.uxml` | **수정/삭제/닫기 확인** (U10) |
| `popups/action-confirm.uxml` | **범용 버튼 클릭 확인** (U4) |
| `PopupCoordinatorV3.cs` | 팝업 상태 관리 (모든 팝업 통합) |
| `help-panel.uxml` + `HelpPanelController.cs` | 컨텍스트 도움말 |
| `WhyItMovedController.cs` | 마지막 이동 설명 |

### 팝업 트리거 규칙 (U4, U10)
- **위험 동작** (MoveJ/MoveL, 서보ON, 에러초기화): 항상 확인 팝업
- **수정 저장 안 한 상태에서 닫기/이동**: "저장하지 않은 변경이 있습니다" 팝업
- **삭제 동작** (포인트 삭제, 시퀀스 삭제): "정말 삭제하시겠습니까?" 팝업
- **일반 탭 전환**: 팝업 없음

### SSOT 매핑 (8개 + 추가 2개)
- #19 RC-05 도움말, #52~#55 연결/Enable/Move/복구 팝업, #57 WhyItMoved
- U4 버튼 확인 팝업, U10 수정/삭제/닫기 확인 팝업

### 검증
```bash
unityctl check --type compile
```

---

## Phase 3 묶음: ViewState 바인딩 + Mock 동작 (3A~3C, 1~2세션)

### 목표
ViewState 바인딩 + Mock 전체 플로우 + Undo/Redo + 로컬 저장 + 자동 재연결.

### `3A` Binder / Scene bootstrap
- `PendantV3Binder.cs`
- `PendantV3SceneCoordinator.cs`
- 기존 `RobotControlV3.unity`에 composition root 추가

### `3B` 로컬 서비스
- `UndoRedoService.cs`
- `LocalSettingsStore.cs`
- `AutoReconnectService.cs`

### `3C` Mock 종단 검증
- 연결 → 서보 → 조그 → 미리보기 → 적용 → Undo
- TCP / 3D 화살표 / Fault / 재연결 / Tablet 전환까지 종단 확인

### `3C` 종료 조건
- 아래 시나리오를 **3회 연속** 재현할 수 있어야 한다.
  1. 연결 → 서보 ON → 관절 조그 → 미리보기 → 적용 → Undo
  2. TCP 조그 → 좌표계 전환 → 3D 화살표 조작
  3. 쉬운 조작 → Home / Ready → 그리퍼 열기 / 닫기
  4. Fault 발생 → 배너 표시 → 오류 초기화
  5. Desktop ↔ Tablet 전환 → 포커스 / 탭 상태 유지
  6. 연결 끊김 → 자동 재연결 카운트다운 → 복귀
- 허용 종료 조건:
  - compile green
  - EditMode green 또는 문서화된 기존 실패만 존재
  - 치명적 콘솔 에러 0건
  - 입력 경합 0건
  - popup focus trap 실패 0건
  - `현재 로봇`, `목표 고스트`, `예상 경로`가 시각적으로 명확히 구분됨
  - `미리보기`, `적용`, `동기화`, `복원`의 의미 차이가 화면에서 드러남
  - `3D viewport`, `desktop shell`, `tablet shell` 스크린샷 각 1장 이상 확보
- 비허용 종료 조건:
  - `UnityEngine.Input` 관련 예외
  - 포인터 입력이 UI와 3D에 동시에 전달되는 현상
  - popup open 상태에서 배경 포커스 이동
  - tablet 전환 후 탭/시트 상태 유실
  - 현재 상태와 목표 상태를 사용자가 구분하기 어려운 시각 충돌

### 산출물
| 파일 | 설명 |
|------|------|
| `PendantV3Binder.cs` | ViewState ↔ VisualElement 일괄 바인딩 |
| `PendantV3SceneCoordinator.cs` | V3 씬 부트스트랩 (V2 패턴 재현) |
| `RobotControlV3.unity` | Phase 0B에 만든 최소 씬을 V3 composition root로 승격 |
| `UndoRedoService.cs` | **Undo/Redo 스택** (U7) — 이동 명령 히스토리 50개 |
| `LocalSettingsStore.cs` | **로컬 저장** (U6) — 마지막 속도/좌표계/증분/탭 선택 |
| `AutoReconnectService.cs` | **자동 재연결** (A2) — 3초 간격 재시도 + 카운트다운 |

### 바인딩 대상 (ViewState → UI)
```
IsConnected → 연결 인디케이터
IsEnabled → 서보 버튼 상태
ControllerMode → 모드 라벨
IsMockMode → Mock/Live 표시
IsDragMode → Drag 표시
SpeedPreset → 속도 라벨 + BottomBar 슬라이더
FaultSummary → Fault 인디케이터 + 배너
SafetySummary → Safety 인디케이터
ToolId/UserId → Tool/User 라벨
CurrentJointValuesDeg[] → CoordStrip Joint + 슬라이더 동기화
CurrentTcpPose → CoordStrip TCP + 화살표 오버레이
PreviewRiskSummary → 위험 배너 + 충돌 하이라이트
RecoveryHintViewState → 다음 행동 카드
LastCommandSummary → Teaching 요약
UndoStack/RedoStack → BottomBar Undo/Redo 활성화 상태
```

### Undo/Redo 규칙 (U7)
- 기록 대상: 실제 이동 명령 (MoveJ/MoveL/조그)
- 기록 제외: 미리보기, UI 조작, 설정 변경
- Undo = 이전 자세로 MoveJ (확인 다이얼로그)
- 히스토리 깊이: 50개, 세션 경계 유지, 앱 종료 시 삭제

### 로컬 저장 규칙 (U6)
- 저장 항목: 마지막 속도%, 좌표계 선택, 증분값, 선택 탭, 연결 IP
- 저장 시점: 값 변경 즉시 (debounce 0.5초)
- 로드 시점: 씬 부트스트랩 시

### Mock 동작 검증 플로우
1. 연결 → 서보 ON → 관절 슬라이더 → 미리보기 → 적용 → **Undo**
2. TCP 조그 → 좌표계 전환 → **3D 화살표 클릭** → ±버튼
3. 쉬운 조작 → Home/Ready → **그리퍼 열기/닫기**
4. Fault 발생 → 배너 → 오류 초기화
5. Desktop ↔ Tablet 전환
6. **인풋에 값 입력 → 0 자동선택 확인**
7. **연결 끊김 → 자동 재연결 카운트다운**
8. **앱 재시작 → 로컬 저장값 복원 확인**

### SSOT 매핑
- Phase 2 레이아웃 전체의 **동작 연결** (필수 62개)
- V1 백로그 P0 #1~#8 전체 완료
- U6 로컬 저장, U7 Undo/Redo, A2 자동 재연결

### 검증
```bash
unityctl check --type compile
unityctl test --mode edit
unityctl play start
unityctl console get-entries --limit 50
unityctl screenshot capture
unityctl play stop
```

---

## Phase 4: V2 vs V3 비교 평가 (1세션)

### 평가 기준 (migration-strategy.md)

| 기준 | 가중치 | 측정 |
|------|--------|------|
| 개발 속도 | 25% | 동일 패널 구현 시간 |
| 반응형 레이아웃 | 20% | Desktop↔Tablet 코드량/품질 |
| 데이터 바인딩 | 15% | ViewState→UI 갱신 코드량 |
| 스타일 유지보수 | 15% | 색상 변경 시 수정 범위 |
| 성능 | 10% | Draw Call, 렌더 시간 |
| 학습 곡선 | 10% | 새 패널 추가 시간 |
| 3D 통합 | 5% | 2D+3D 혼합 품질 |

### 채택 기준
- V3 > V2 20% 이상 → **V3 채택**, V2 폐기
- 차이 < 20% → V2 유지, V3 보존
- V3 치명적 문제 → V2 유지

### 산출물
- `docs/ref/product/pendant-v3/evaluation-result.md`

---

## Phase 5: 포인트/티칭/블록 에디터 (채택 후, 1~2세션)

### 목표
포인트 관리 + 블록 기반 시퀀스 편집 + 논리/이동 명령 + 다중 경로 미리보기.

### 산출물
| 파일 | 설명 |
|------|------|
| `points-panel.uxml` + `PointsController.cs` | 저장/이름/목록/불러오기/삭제/순서/export/import |
| `teaching-sequence.uxml` + `TeachingSequenceController.cs` | 블록 에디터 |
| `block-palette.uxml` | 블록 팔레트 (아이콘 매핑) |
| `PointDataStore.cs` | 포인트 JSON 저장/로드 |
| `SequenceRunner.cs` | 시퀀스 시뮬레이션 + 실행 |
| `MultiPointPathPreview.cs` | 다중 경로 전체 미리보기 (A7) |

### 블록 종류 (U2, U11, U13)

**이동 명령 블록** (아이콘: 화살표 계열)
| 블록 | 아이콘 | 설명 |
|------|--------|------|
| MoveJ | ↗️ 곡선 화살표 | 관절 기준 이동 |
| MoveL | → 직선 화살표 | 직선 이동 |
| MoveC | ↩️ 원호 화살표 | 원호 보간 이동 (U13) |

**IO 블록** (아이콘: 전기 계열)
| 블록 | 아이콘 | 설명 |
|------|--------|------|
| 그리퍼 열기 | ✋ 열린 손 | 그리퍼 Open |
| 그리퍼 닫기 | ✊ 닫힌 손 | 그리퍼 Close |
| DO ON/OFF | ⚡ 번개 | 디지털 출력 |

**논리 블록** (아이콘: 제어 계열) (U11)
| 블록 | 아이콘 | 설명 |
|------|--------|------|
| Wait | ⏱️ 시계 | N초 대기 |
| Loop | 🔄 순환 | N회 반복 (시작~끝 범위 지정) |
| IF | ❓ 분기 | 조건 분기 (DI 상태 기준) |
| Call | 📞 호출 | 다른 시퀀스 호출 |

### 루프 생성 UX (U2)
1. 블록 팔레트에서 Loop 아이콘 드래그
2. 루프 범위를 시각적으로 선택 (시작 블록 ~ 끝 블록)
3. 반복 횟수 입력 (기본 1)
4. 루프 블록이 하위 블록을 들여쓰기로 표시
5. 루프 안에 루프 중첩 가능 (최대 3단계)

### SSOT 매핑 (9개 + 추가 6개)
- #58~#65 포인트 관리 8개, #18 RC-04 티칭
- U2 블록+루프, U11 논리/이동 블록, U13 MoveC
- A7 다중 경로 미리보기
- #71~#75 시퀀스/블록/시뮬레이션/편집/검증 (선택→포함으로 승격)

### 검증
```bash
unityctl check --type compile
unityctl test --mode edit
unityctl play start → 시퀀스 시뮬레이션 확인
```

---

## Phase 6: IO/그리퍼/진단/캡처 (채택 후, 1세션)

### 산출물
| 파일 | 설명 |
|------|------|
| `io-panel.uxml` + `IoController.cs` | DI/DO/AI/AO 상태+제어 |
| `gripper-panel.uxml` + `GripperController.cs` | 그리퍼 상태/위치/힘 제어 |
| `session-report.uxml` + `SessionReportController.cs` | 세션 리포트 |
| `ScreenshotCaptureService.cs` | 스크린샷 + 상태 JSON 캡처 (A5) |
| `help-context-map.json` | 버튼→도움말 컨텍스트 매핑 |

### SSOT 매핑 (11개 + 추가 1개)
- #77~#82 IO (P1 승격), #83~#85 그리퍼
- #56 도움말 패널, #89 Status 탭, #90 세션 리포트
- A5 스크린샷/캡처

### 검증
```bash
unityctl check --type compile
unityctl test --mode edit
```

---

## Phase 7: 모드 분리 + 드래그 티칭 + 감도 (채택 후, 1세션)

### 산출물
| 파일 | 설명 |
|------|------|
| `UserModeController.cs` | 초보자/전문가 기능 가시성 매트릭스 |
| `mode-select.uxml` | 모드 선택 카드 (첫 실행 + 설정) |
| `DragTeachRecorder.cs` | 드래그 티칭 궤적 기록 → 블록 변환 (A6) |
| `JogSensitivityProfile.cs` | 모드별 조그 감도 프로파일 (A8) |

### SSOT 매핑
- #106~#108 초보자/전문가/강사 모드
- #66~#69 TPD 기록/재생/편집 (드래그 티칭으로 통합)
- #94 강사 데모 모드
- A6 드래그 티칭 통합, A8 조그 감도

### 검증
```bash
unityctl check --type compile
unityctl play start → 모드 전환 + 드래그 티칭 확인
```

---

## Phase 8: P2 차별화 (후속, 복수 세션)

### 포함 항목
- AI 보조 (deterministic 경고 → rule-based 추천 → LLM 설명)
- 비전 오버레이 (카메라 PiP + 검출)
- 작업 템플릿 (Pick&Place/Palletizing 위저드)
- A9 연결 QR 코드 (태블릿 스캔)
- #91~#93 진단 Drawer, 로그 수집, 버전 정보

---

## 제외 항목 (SSOT 확인, 구현 안 함)

| # | 항목 | 이유 |
|---|------|------|
| #2 | Program load/run/pause/resume (제조사 Lua) | SSOT 명시 제외 |
| #36 | ServoJ/ServoCart | 연속 서보 단계 전까지 제외 |
| #86~#88 | 외부축/FT/Force | 제품 2차 범위 |
| #100 | 다국어 | 한국어 우선 |
| #109 | 사용자 권한 레벨 | V1 scope out |
| #110~#117 | 고급 설정/유지보수/SimMachine | V1 scope out |

---

## 잠금 규칙 (공식문서 기반, 변경 금지)

### A. 네이밍 1대1 매핑 규칙

| UXML | USS | Controller | 기능명 |
|------|-----|-----------|--------|
| `joint-jog-panel.uxml` | `joint-jog-panel.uss` | `JointJogController.cs` | 관절 조그 |
| `tcp-jog-panel.uxml` | `tcp-jog-panel.uss` | `TcpJogController.cs` | TCP 조그 |
| `easy-motion-panel.uxml` | `easy-motion-panel.uss` | `EasyMotionController.cs` | 쉬운 조작 |
| `connection-home.uxml` | `connection-home.uss` | `ConnectionHomeController.cs` | 연결 홈 |
| `points-panel.uxml` | `points-panel.uss` | `PointsController.cs` | 포인트 관리 |
| `teaching-sequence.uxml` | `teaching-sequence.uss` | `TeachingSequenceController.cs` | 티칭 시퀀스 |
| `io-panel.uxml` | `io-panel.uss` | `IoController.cs` | IO 제어 |
| `gripper-panel.uxml` | `gripper-panel.uss` | `GripperController.cs` | 그리퍼 |
| `diagnostics-panel.uxml` | `diagnostics-panel.uss` | `DiagnosticsController.cs` | 에러 진단 |
| `help-panel.uxml` | `help-panel.uss` | `HelpPanelController.cs` | 도움말 |

- UXML 파일명: `kebab-case`
- USS 파일명: UXML과 동일
- C# Controller: `PascalCase` + `Controller` 접미사
- USS 클래스명: `.rc-` 접두사 + `kebab-case` — `.rc-top-bar`, `.rc-nav-item--active`
- USS 토큰 변수: `--rc-` 접두사 — `--rc-accent`, `--rc-card`
- UXML 요소 name: `PascalCase` — `name="TopStatusBar"`, `name="JointSlider1"`
- 아이콘 파일명: `icon-{기능}.png` — `icon-move-j.png`, `icon-gripper-open.png`
- **스타일은 USS 클래스로, 바인딩은 UXML name으로**

### B. 파일 크기 규칙

- **C# 파일 300줄 초과 금지** → 반드시 컴포넌트 분리
- **UXML 중첩 최대 5단계** → 그 이상은 별도 UXML Template으로 분리
- **USS 파일 200줄 초과 시** → 기능별 분리 (예: `motion-panels.uss`, `status-panels.uss`)

### C. UI Toolkit 생명주기 (공식문서 확인)

```csharp
// ✅ 공식 권장: OnEnable에서 초기화 (UXML이 이미 인스턴스화된 시점)
void OnEnable()
{
    var root = GetComponent<UIDocument>().rootVisualElement;
    var button = root.Q<Button>("MyButton");
    button.RegisterCallback<ClickEvent>(OnClick);
}

// ✅ 반드시 OnDisable에서 해제 (메모리 누수 방지)
void OnDisable()
{
    var root = GetComponent<UIDocument>().rootVisualElement;
    var button = root.Q<Button>("MyButton");
    button?.UnregisterCallback<ClickEvent>(OnClick);
}

// ❌ 금지: Awake/Start에서 UI 초기화 (UXML 미로드 상태)
```

### D. 요소 쿼리 패턴 (공식문서 확인)

```csharp
// ✅ 이름으로 쿼리 (UXML name 속성)
var slider = root.Q<Slider>("JointSlider1");

// ✅ 클래스로 쿼리 (USS 클래스)
var items = root.Query<Button>(className: "rc-nav-item").ToList();

// ❌ 금지: Q<T>() 타입만으로 쿼리 (여러 요소가 매칭될 위험)
```

### E. 이벤트 처리 패턴 (공식문서 확인)

```csharp
// ✅ 값 변경 콜백 (Slider, TextField, Toggle 등)
slider.RegisterValueChangedCallback(evt => {
    // evt.newValue 사용
    // evt.previousValue 사용 가능
});

// ✅ 값 변경 없이 UI만 갱신 (무한 루프 방지)
slider.SetValueWithoutNotify(newValue);

// ✅ 버튼 클릭
button.RegisterCallback<ClickEvent>(OnClick);

// ✅ 포커스 이벤트 (인풋 0 자동선택용)
textField.RegisterCallback<FocusInEvent>(evt => {
    textField.SelectAll(); // U5: 기존값 전체 선택
});
```

### F. Flexbox 레이아웃 규칙 (공식문서 확인)

```xml
<!-- ✅ 3패널 레이아웃 공식 패턴 -->
<engine:VisualElement name="MainContainer" style="flex-direction: row; flex-grow: 1;">
    <!-- 좌측: 고정폭, 축소 안 됨 -->
    <engine:VisualElement name="NavRail" style="width: 72px; flex-shrink: 0;" />
    
    <!-- 중앙: 남은 공간 전부 차지 -->
    <engine:VisualElement name="MainContent" style="flex-grow: 1;" />
    
    <!-- 우측: 고정폭, 축소 안 됨 -->
    <engine:VisualElement name="ContextPanel" style="width: 320px; flex-shrink: 0;" />
</engine:VisualElement>
```

**안티패턴 (공식문서 명시):**
- ❌ flex 부모에 고정 px 사이즈 → 반응형 깨짐
- ❌ `flex-shrink: 0` 남용 → 오버플로우 발생
- ❌ absolute + flex 자식 혼합 → 예측 불가 레이아웃
- ❌ 중첩 100% width → 누적 오버플로우
- ❌ 인라인 스타일 과다 → USS 분리 필수

### G. ListView 규칙 (공식문서 확인)

```csharp
// ✅ 런타임 ListView 필수 3요소
listView.itemsSource = dataList;                    // 데이터
listView.makeItem = () => new Label();              // 아이템 생성
listView.bindItem = (el, i) => ((Label)el).text = dataList[i]; // 바인딩

// ✅ 성능: 가상화 + 고정 높이
listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
listView.fixedItemHeight = 32;

// ✅ 이벤트
listView.selectionChanged += OnSelectionChanged;
listView.itemsChosen += OnItemChosen; // 더블클릭
```

### H. ViewState 경계 규칙

| 규칙 | 내용 |
|------|------|
| ViewState는 V2/V3 공유 | V3 전용 필드는 `PendantV3LocalState`에 분리 |
| UI → App 방향 | Controller가 ViewState 직접 수정 금지 → Command 패턴 |
| App → UI 방향 | `ViewState.Changed` 이벤트 → Controller가 UI 갱신 |
| 값 갱신 시 | `SetValueWithoutNotify()` 사용 (무한 루프 방지) |

### I. 팝업 규칙

| 규칙 | 내용 |
|------|------|
| 팝업 필요 시 | 위험 동작, 삭제, 미저장 닫기 |
| 팝업 불필요 시 | 탭 전환, 조그 모드 전환, 좌표계 전환 |
| 동시 최대 | 1개 (이전 닫힌 후 새 팝업) |
| 버튼 순서 | 왼쪽=취소(MutedText), 오른쪽=확인(Accent 또는 Danger) |
| 외부 클릭 | 경고/확인=무시, 도움말=닫기 |

### J. 3D-UI 경계 규칙

| 규칙 | 내용 |
|------|------|
| PanelSettings Sort Order | V3 UI Toolkit = 100, 기존 uGUI 3D = 50 |
| 입력 우선 | UI Toolkit 최우선 → UI 위 클릭은 3D 관통 안 함 |
| ViewportHost | UI Toolkit에서 빈 영역 (Camera 직접 렌더링) |
| 3D 이벤트 | ViewportHost 영역의 이벤트만 카메라에 전달 |

### K. 성능 규칙

| 규칙 | 값 | 이유 |
|------|-----|------|
| CoordStrip 갱신 주기 | 100ms (10fps) | 매 프레임 불필요 |
| ListView 가상화 | 필수 (FixedHeight) | 포인트/로그 목록 최적화 |
| USS 변수 갱신 | 앱 시작 시 1회 | 런타임 변경 최소화 |
| VisualElement 동적 생성 | 최소화 | UXML에 미리 정의 + `display: none` 토글 |
| 인라인 스타일 | 금지 | USS 파일로 분리 |

### L. 공식문서 참조 링크 (구현 시 항상 확인)

| 주제 | URL |
|------|-----|
| UI 시스템 비교 | https://docs.unity3d.com/6000.3/Documentation/Manual/UI-system-compare.html |
| Runtime UI 시작 | https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-get-started-with-runtime-ui.html |
| Runtime Data Binding | https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-runtime-binding.html |
| USS 속성 레퍼런스 | https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-USS-Properties-Reference.html |
| Flexbox 레이아웃 | https://docs.unity3d.com/6000.3/Documentation/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/layouts.html |
| 이벤트 처리 | https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-Events-Handling.html |
| ListView | https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-uxml-element-ListView.html |
| 바인딩 콜백 | https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-create-a-binding-callback-any-properties.html |
| 런타임 UI 예제 | https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-HowTo-CreateRuntimeUI.html |

### M. Panel topology 규칙

| 규칙 | 내용 |
|------|------|
| 메인 PanelSettings | `PendantV3PanelSettings.asset` 단일 SSOT |
| 공유 범위 | `TopStatusBar`, `NavRail`, `MainContent`, `ContextPanel`, `BottomBar`, `Popups`, `BottomSheet` |
| 다중 패널 허용 조건 | 성능 프로파일 또는 입력 우선순위 문제를 수치로 확인한 경우만 |
| 도입 시 필수 문서화 | `Sort Order`, 포커스 handoff, pointer 우선순위, 렌더 순서 |

### N. 입력 시스템 규칙

| 규칙 | 내용 |
|------|------|
| Active Input Handling | `Input System Package` 기준 |
| V3 금지 API | `UnityEngine.Input` 직접 호출 금지 |
| EventSystem | 씬당 1개 |
| UI 입력 모듈 | `InputSystemUIInputModule` 단일 기준 |
| 입력 관통 | `ViewportHost` 외 V3 UI 위 포인터 입력은 3D로 관통 금지 |

### O. 포커스 / 네비게이션 규칙

| 규칙 | 내용 |
|------|------|
| 기본 포커스 순서 | `TopStatusBar -> NavRail -> WorkTabBar -> WorkPanel -> ContextPanel -> BottomBar -> Popup` |
| 장식 요소 | `focusable = false`, `tabIndex = -1` |
| 팝업 포커스 | 오픈 시 내부에 가두고, 닫힐 때 호출자로 복원 |
| 기본 키 동작 | `Escape=닫기/취소`, `Enter=기본 확인`, 방향키/D-pad=동레벨 이동 |
| 포커스 시각 | `2px solid AccentPrimary` 유지 |

### P. 바인딩 / 지속성 규칙

| 규칙 | 내용 |
|------|------|
| 구조/스타일 | `UXML/USS` 소유 |
| 런타임 상태 쓰기 | `C# Binder` 소유 |
| 이중 바인딩 | 동일 요소에 `Runtime Data Binding` + 수동 `C# write` 동시 사용 금지 |
| 공유 상태 | 로봇/연결/모션은 `RobotControlViewState` |
| V3 전용 상태 | UI-로컬 파생 상태는 `PendantV3LocalState` |
| 저장 대상 | 탭 선택, split 비율, 시트 확장 상태만 `viewDataKey` 또는 `LocalSettingsStore` |

### Q. 텍스트 / 리스트 규칙

| 규칙 | 내용 |
|------|------|
| Text Settings asset | `PendantV3TextSettings.asset`를 PanelSettings에 연결 |
| 기본 폰트 | `Noto Sans KR` |
| fallback | 심볼/이모지/누락 글리프 대비 fallback font 목록 유지 |
| 한국어 줄바꿈 | `Korean Line Breaking Rules` 사용 |
| 경고 정책 | 개발 중 missing glyph 경고 비활성화 금지 |
| 리스트 기본 | 10개 이상 반복 항목은 `ListView` 우선 |
| 가상화 기본 | `FixedHeight` + `fixedItemHeight` 명시 |
| 갱신 API | `RefreshItems`/`RefreshItem` 우선, `Rebuild`는 구조 변경 시만 |
| 재정렬 허용 | 포인트/티칭 시퀀스만 허용, `NavRail`/`WorkTabBar`는 금지 |

### R. 성능 / 그래픽 자산 규칙

| 규칙 | 내용 |
|------|------|
| 애니메이션 허용 속성 | `translate`, `scale`, `opacity` |
| 애니메이션 금지 속성 | `width`, `height`, `top`, `left` |
| 중첩 마스크 | 최대 1단계 |
| 정적 아이콘 | `Sprite Atlas`로 패킹 |
| 동적 이미지 | 필요 시 `dynamic atlas` 사용 |
| usageHints | 이동 패널/시트/팝업 컨테이너에 검토 |
| 표시/숨김 | 재생성보다 `display`/`visibility` 토글 우선 |

---

## 모든 Phase 공통 규율

### 자기리뷰 체크리스트
- [ ] 역할 경계 유지 (App/UI/Visualization 혼합 금지)
- [ ] USS 토큰 `--rc-*` 사용 (인라인 스타일/하드코딩 금지) — 규칙K
- [ ] authored-first 유지
- [ ] 필수/선택/제외 범위 누수 없음
- [ ] **C# 300줄 이하** — 초과 시 컴포넌트 분리 — 규칙B
- [ ] **UXML↔Controller 이름 1대1 매핑** — 규칙A
- [ ] **OnEnable 초기화 / OnDisable 해제** — 규칙C
- [ ] **SetValueWithoutNotify 사용** (무한 루프 방지) — 규칙E,H
- [ ] 한국어 기본 언어
- [ ] preview → 확인 → 실행 흐름
- [ ] **수정/삭제/닫기 시 확인 팝업** (U10) — 규칙I
- [ ] **아이콘 가독성** — 함수별 매핑 (U3) — 규칙A
- [ ] **인풋 FocusIn → SelectAll** (U5) — 규칙E
- [ ] **ListView 가상화 FixedHeight** — 규칙G,K
- [ ] **공식문서 참조 확인** (U9) — 규칙L

### unityctl 검증 루프
```bash
unityctl check --type compile
unityctl test --mode edit
# 필요 시:
unityctl play start → console get-entries → screenshot → play stop
```

### 잠금 변수 (확정, 변경 금지)

| # | 항목 | 확정값 | 근거 |
|---|------|--------|------|
| 1 | 씬 이름 | `RobotControlV3.unity` | 주인님 확정 |
| 2 | 브랜치 | `codex/robotcontrol-v3-toolkit` | 현재 구현 브랜치 고정 |
| 3 | 씬 진입 | 온보딩에서 버튼으로 이동 (다른 페이지와 동일) | 주인님 확정. SceneCatalog에 등록 |
| 4 | PanelSettings Scale Mode | `Scale With Screen Size`, Ref 1920x1080, Match 0.5 | 패드 반응형 지원. 태블릿+데스크탑 양쪽 최적 |
| 5 | 기본 속도 Preset | 30% | 주인님 확정 |
| 6 | CoordStrip 기본 표시 | `Both` (Joint + TCP 동시) | 초보자도 두 값을 동시에 봐야 이해 빠름 |
| 7 | DryRun 기본값 | Live 첫 연결 시 **ON** | Safe By Default 원칙 |
| 8 | 증분 기본값 | 초보자: 5°/5mm 고정, 전문가: 자유선택(기본 1°/1mm) | 모드별 분리 |
| 9 | 포인트 저장 형식 | JSON, `Application.persistentDataPath/waypoints/PendantV3Points.json` | 기존 `WaypointStore` 재사용. Inspector 편집 불필요 |
| 10 | 이벤트 로그 보존 | 최대 **200개**, FIFO 자동 삭제 | 메모리 안전 + 충분한 히스토리 |
| 11 | 자동 재연결 최대 시도 | **10회** (3초 간격 = 30초) | 너무 짧으면 포기 빠름, 너무 길면 대기 피로 |
| 12 | Undo 히스토리 깊이 | **50개** | 대부분의 세션을 커버하면서 메모리 안전 |
| 13 | USS 색상 테마 | **다크 테마 고정** (V2 Colors 그대로) | 산업 HMI 표준. 라이트 옵션 없음 |
| 14 | 아이콘 형식 | **PNG**, 64x64px @2x, 투명 배경 | Unity UI Toolkit은 SVG 런타임 렌더링 비용 높음 |
| 15 | 텍스트 리소스 | **ScriptableObject** (`PendantLocalization.asset`) | 주인님 확정. Inspector에서 수정 가능, 후속 다국어 확장 |
| 16 | 루프 최대 중첩 | **3단계** | 가독성 한계. 3 이상은 서브시퀀스 Call로 분리 |
| 17 | MoveC 중간점 수 | **1개** (3점 원호: 시작→중간→끝) | 공식 SDK `MoveC` API가 3점 기반 |
| **UI 치수** | | | |
| 18 | TopStatusBar 높이 | **56px** | shell-layout 확정 |
| 19 | NavRail 너비 | **72px** (접힘 48px) | shell-layout 확정 |
| 20 | ContextPanel 너비 | **320px** | shell-layout 확정 |
| 21 | BottomBar 높이 | **48px** | shell-layout 확정 |
| 22 | WorkTabBar 높이 | **40px** | shell-layout 확정 |
| 23 | 패널 간 간격 | **4px** (margin) | USS gap 미지원, margin 사용 |
| 24 | 카드 내부 패딩 | **12px** | 가독성+터치 영역 확보 |
| 25 | 버튼 최소 터치 영역 | **44x44px** | Touch Friendly 원칙 |
| 26 | 프리셋 버튼 크기 | **88x88px** | 초보자 대형 버튼 |
| 27 | 슬라이더 트랙 높이 | **8px** (히트 영역 44px) | 시각 얇게, 터치 넓게 |
| **타이포그래피** | | | |
| 28 | 기본 폰트 크기 | **17px** | 주인님 확정 |
| 29 | 작은 텍스트 | **14px** | 보조 정보 |
| 30 | 라벨 텍스트 | **12px** | 축 이름, 단위 등 |
| 31 | 헤더 텍스트 | **20px** | 패널 제목 |
| 32 | 폰트 패밀리 | **Noto Sans KR** (TMP) | 한국어+영문+숫자 |
| **애니메이션/타이밍** | | | |
| 33 | 탭 전환 | **150ms** ease-out | 빠르고 자연스럽게 |
| 34 | 팝업 등장 | **200ms** fade-in + scale(0.95→1.0) | 부드러운 진입 |
| 35 | 팝업 닫기 | **150ms** fade-out | 빠르게 사라짐 |
| 36 | 토스트 지속 | **3초** 후 자동 닫기 | |
| 37 | 슬라이더 debounce | **16ms** (매 프레임) | 실시간 미리보기 |
| 38 | 인풋 debounce | **300ms** | 미리보기 갱신 주기 |
| 39 | 로컬 저장 debounce | **500ms** | 잦은 쓰기 방지 |
| 40 | 조그 long-press | **300ms** 후 연속 이동 | FAIRINO 참고 |
| 41 | 값 변경 셀 플래시 | **500ms** AccentPrimary | CoordStrip 갱신 강조 |
| **카메라/3D** | | | |
| 42 | 초기 뷰 | 아이소메트릭 (45° 대각선) | 전체 파악 최적 |
| 43 | 궤도 회전 감도 | **0.3°/px** | |
| 44 | 줌 범위 | **0.5x ~ 3.0x** | |
| 45 | EE 트레일 유지 | **3초** | |
| 46 | 고스트 투명도 | **30%** | |
| 47 | 작업공간 경계 투명도 | **15%** | |
| **빈 상태 텍스트** | | | |
| 48 | 포인트 비어있음 | "저장된 포인트가 없습니다. [현재 위치 저장]" | |
| 49 | 시퀀스 비어있음 | "스텝이 없습니다. [블록 추가]" | |
| 50 | 이벤트 로그 비어있음 | "이벤트가 없습니다" | |
| 51 | 연결 중 로딩 | 스피너 + "연결 중..." | |
| 52 | IK 계산 중 | "계산 중..." (300ms 이상 시만) | |
| **어셈블리/빌드** | | | |
| 53 | V3 asmdef | `KineTutor3D.UI.RobotControlV3` | V2와 분리 |
| 54 | V3 참조 허용 | `KineTutor3D.Runtime`, `UIElementsModule` | |
| 55 | V3 참조 금지 | `UnityEngine.UI` (uGUI 직접 금지) | |
| 56 | Build Index | **7** (RobotControl=6 다음) | |
| 57 | SceneCatalog | `SceneCatalog.RobotControlV3` 추가 | |
| **스타일 디테일** | | | |
| 58 | 카드/버튼 border-radius | **6px** | |
| 59 | 팝업 border-radius | **12px** | |
| 60 | 입력 필드 border-radius | **4px** | |
| 61 | 비활성 상태 opacity | **0.4** | |
| 62 | 팝업 배경 딤 | **rgba(0,0,0,0.6)** | |
| 63 | 스크롤바 너비 | **6px**, 자동 숨김 (2초 후 fade) | |
| 64 | 포커스 하이라이트 | **2px solid AccentPrimary** | |
| **데이터 형식/범위** | | | |
| 65 | 각도 단위 | **deg 고정** (rad 없음) | |
| 66 | 위치 단위 | **mm 고정** (m 없음) | |
| 67 | 소수점 자릿수 | **1자리** | feature-coordinates.md 확정 |
| 68 | 속도 슬라이더 스텝 | **1%** (1~100) | |
| 69 | 최대 웨이포인트 수 | **100개** | |
| 70 | 최대 시퀀스 스텝 수 | **200개** | |
| 71 | 드래그 티칭 샘플 주기 | **50ms** (20Hz) | |
| 72 | IO 채널 수 (FR5) | **DO8 DI8 AO2 AI2 TDO2** | |
| **세션/Export** | | | |
| 73 | 세션 리포트 형식 | **JSON** | 후속 PDF 변환 가능 |
| 74 | 스크린샷 해상도 | **현재 화면 해상도 그대로** | |
| 75 | Import 검증 실패 | 토스트 "파일 형식이 올바르지 않습니다" + 거부 | |
| **키보드 단축키** | | | |
| 76 | Space | 긴급 정지 (Stop) | |
| 77 | Ctrl+Z | Undo | |
| 78 | Ctrl+Y | Redo | |
| 79 | Escape | 팝업 닫기 / 미리보기 취소 | |
| 80 | Tab | 다음 인풋 포커스 이동 | |
| 81 | 1~4 | WorkTabBar 탭 전환 | |
| **멀티로봇** | | | |
| 82 | V3 대상 로봇 | **단일 로봇** (FR5). 멀티는 Phase 8+ | |

### 모델별 역할 분담
- **Opus 4.6**: 코딩 (C#, UXML, USS 작성/수정)
- **Sonnet/Haiku**: 문서 작업, 코드 스캔, 웹 검색, 리서치
- Agent 서브태스크에서 `model` 파라미터로 적절한 모델 지정

### 커밋 규칙
- 각 Phase 범위만 포함, unrelated 변경 금지
- 브랜치: `codex/robotcontrol-v3-toolkit`

### 페이즈 리뷰 규칙
- **매 Phase 종료 시 주인님 확인 후 다음 Phase 진행**
- Phase 1은 디자인 시안 리뷰 게이트 (U8)
- Phase 4는 V2 vs V3 채택 결정 게이트

---

## 핵심 파일 경로

### 재사용 (변경 없음)
- `Assets/Scripts/App/Fairino/Shell/RobotControlViewState.cs`
- `Assets/Scripts/App/Fairino/IFairinoRobotClient.cs`
- `Assets/Scripts/App/Fairino/MockFairinoClient.cs`
- `Assets/Scripts/App/Fairino/FairinoConnectionService.cs`
- `Assets/Scripts/App/Fairino/FairinoErrorTranslator.cs`
- `Assets/Scripts/App/Fairino/Shell/PreviewRiskSummary.cs`
- `Assets/Scripts/App/Fairino/Shell/RecoveryHintViewState.cs`
- `Assets/Scripts/App/Fairino/PresetTransitionAnimator.cs`
- `Assets/Scripts/App/Fairino/WaypointCycleRunner.cs`
- `Assets/Scripts/Visualization/` (전체 3D 레이어)

### 신규 생성
- `Assets/UI/PendantV3/` — UXML/USS/아이콘
- `Assets/Scripts/UI/RobotControlV3/` — Controller
- `Assets/Scenes/RobotControlV3.unity` — V3 씬

### 참조 문서
- `docs/ref/product/pendant-v3/README.md`
- `docs/ref/product/pendant-v3/shell-layout.md`
- `docs/ref/product/pendant-v3/feature-*.md` (14개)
- `docs/ref/product/pendant-v3/migration-strategy.md`
