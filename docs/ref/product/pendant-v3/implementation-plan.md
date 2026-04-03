# V3 티칭패드 구현 플랜

## Context
V2(uGUI) 셸이 플레이스홀더 상태로 존재하는 `codex/robotcontrol-shell` 브랜치에서,
UI Toolkit 기반 V3를 `main` 브랜치에 별도 구현한다.
Phase 0~3에서 V2와 비교 평가 → Phase 4에서 채택 결정 → Phase 5~8에서 전체 기능 완성.

SSOT 3개 문서(V1 백로그 117항목, Feature Matrix, Soft Teaching Pad UX) +
주인님 추가 요구 13개 + 제안 채택 10개 = **총 140개 항목** 누락 없이 매핑.

### 추가 확정 기능 (SSOT 반영 필요)

| ID | 기능 | Phase |
|----|------|-------|
| U1 | 그리퍼 개폐 (쉬운 조작 탭) | 2B |
| U2 | 블록 추가 + 단계별 루프 생성 (논리/이동 블록) | 5 |
| U3 | 가독성 아이콘 → 함수별 매핑 | 전체 |
| U4 | 버튼 클릭 시 확인 팝업 모달 | 2D |
| U5 | 인풋 수정 시 0 자동선택 + 즉시 반영 | 2B |
| U6 | 로컬스토리지 임시 값 저장 (PlayerPrefs/JSON) | 3 |
| U7 | Undo/Redo (BottomBar 상시 표시) | 3 |
| U8 | Phase 1 후 디자인 시안 리뷰 게이트 | 1 |
| U9 | UI Toolkit 공식문서/스킬 참조 필수 | 전체 |
| U10 | 수정/삭제/창닫기 시 확인 팝업 | 2D |
| U11 | 논리명령(IF/ELSE/LOOP) + 이동명령(MoveJ/L/C) 블록 | 5 |
| U12 | 데카르트 3D 화살표 방향 기기조작 | 2B |
| U13 | MoveC (원호 이동) 블록 | 5 |
| A1 | 작업공간 경계 시각화 (도달 범위 구체) | 2C |
| A2 | 자동 재연결 (3초 재시도 + 카운트다운) | 3 |
| A3 | 속도 오버라이드 실시간 슬라이더 (BottomBar) | 2B |
| A4 | 경로 충돌 사전 검출 (빨간 하이라이트) | 2C |
| A5 | 스크린샷/상태 캡처 버튼 (PNG+JSON) | 6 |
| A6 | 드래그 티칭 모드 통합 → 블록 자동 변환 | 7 |
| A7 | 다중 포인트 경로 전체 미리보기 | 5 |
| A8 | 조그 감도 설정 (모드별 자동 적용) | 7 |
| A9 | 연결 QR 코드 (태블릿 스캔 연결) | 8 |

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

## Phase 0: UI Toolkit 인프라 (1세션)

### 목표
UI Toolkit 런타임 기반을 프로젝트에 확립한다.

### 산출물
| 파일 | 설명 |
|------|------|
| `Assets/UI/PendantV3/pendant-v3.uss` | 루트 USS — `--rc-*` 디자인 토큰 변수 정의 |
| `Assets/UI/PendantV3/pendant-v3.uxml` | 루트 UXML — 빈 셸 5영역 (TopBar/NavRail/Main/Context/Bottom) |
| `Assets/UI/PendantV3/PanelSettings/PendantV3PanelSettings.asset` | PanelSettings ScriptableObject |
| `Assets/Scripts/UI/RobotControlV3/PendantV3Document.cs` | UIDocument MonoBehaviour — 씬 부트스트랩 |
| `Assets/UI/PendantV3/icons/` | 아이콘 에셋 폴더 + 기본 아이콘 세트 (U3) |

### SSOT 매핑
- #95 Authored-First → UIDocument + UXML authored 패턴 확립
- #96 UIDesignTokens → USS 토큰 변수로 변환
- #98 UILayoutProfile → C# 클래스 토글 방식으로 대체
- U3 가독성 아이콘 폴더 구조 확립
- U9 UI Toolkit 공식문서/스킬 참조 — `/ui-toolkit-verify` 실행으로 시작

### 검증
```bash
unityctl check --type compile
# /ui-toolkit-verify 스킬로 가용성 재확인
```

---

## Phase 1: 빈 셸 + 탭 네비게이션 (1세션)

### 목표
5영역 셸이 Desktop/Tablet 양쪽에서 동작하는 빈 레이아웃을 만든다.
**★ 완료 후 주인님 디자인 시안 리뷰 (U8)**

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

## Phase 3: ViewState 바인딩 + Mock 동작 (1~2세션)

### 목표
ViewState 바인딩 + Mock 전체 플로우 + Undo/Redo + 로컬 저장 + 자동 재연결.

### 산출물
| 파일 | 설명 |
|------|------|
| `PendantV3Binder.cs` | ViewState ↔ VisualElement 일괄 바인딩 |
| `PendantV3SceneCoordinator.cs` | V3 씬 부트스트랩 (V2 패턴 재현) |
| `RobotControlV3.unity` | V3 전용 씬 |
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
| 2 | 브랜치 | `main` (메인 브랜치 사용) | 주인님 확정. 별도 브랜치 안 씀 |
| 3 | 씬 진입 | 온보딩에서 버튼으로 이동 (다른 페이지와 동일) | 주인님 확정. SceneCatalog에 등록 |
| 4 | PanelSettings Scale Mode | `Scale With Screen Size`, Ref 1920x1080, Match 0.5 | 패드 반응형 지원. 태블릿+데스크탑 양쪽 최적 |
| 5 | 기본 속도 Preset | 30% | 주인님 확정 |
| 6 | CoordStrip 기본 표시 | `Both` (Joint + TCP 동시) | 초보자도 두 값을 동시에 봐야 이해 빠름 |
| 7 | DryRun 기본값 | Live 첫 연결 시 **ON** | Safe By Default 원칙 |
| 8 | 증분 기본값 | 초보자: 5°/5mm 고정, 전문가: 자유선택(기본 1°/1mm) | 모드별 분리 |
| 9 | 포인트 저장 형식 | JSON, `Application.persistentDataPath/points/` | 이식성+가독성 최적. Inspector 편집 불필요 |
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
- 브랜치: `main`

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
