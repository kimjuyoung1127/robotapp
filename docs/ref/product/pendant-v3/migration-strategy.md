# V1/V2 → V3 마이그레이션 전략

## Purpose
- V1/V2(uGUI)에서 V3(UI Toolkit)로의 전환 계획을 정의한다.
- 재사용 가능한 자산 목록과 새로 만들어야 할 것을 구분한다.
- 비교 평가 기준을 명시하여 V3 채택/폐기 결정을 지원한다.

## Parent Doc
- [README.md](./README.md)

## Last Updated
- 2026-04-03 (KST)

---

## 전환 전략: "UI 먼저, 로직 나중에"

```text
Phase 0: 인프라
  └─ PanelSettings 생성 + USS 토큰 변환 + UIDocument 패턴 확립 (UI Toolkit은 Unity 6 built-in)

Phase 1: 빈 셸
  └─ TopBar + NavRail + MainContent + ContextPanel + BottomBar
  └─ Desktop/Tablet 양쪽 레이아웃 동작 확인
  └─ 이 시점에서 V2 셸과 시각 품질 비교

Phase 2: P0 패널 UI
  └─ 연결 홈, 관절 조그, TCP 조그, 좌표 표시, 안전 배너
  └─ 레이아웃만 — 데이터 바인딩 없음
  └─ 디자인 토큰이 USS로 잘 변환되는지 검증

Phase 3: ViewState 바인딩
  └─ RobotControlViewState → UI Toolkit 바인딩 연결
  └─ Mock 모드에서 전체 흐름 동작 확인
  └─ 이 시점에서 V2 vs V3 코드량/개발 속도 비교

Phase 4: 비교 평가
  └─ 평가 기준표로 V2 vs V3 점수 비교
  └─ 채택 결정

Phase 5 (채택 시): P1 기능 추가 + V2 폐기
Phase 5 (미채택 시): V3 브랜치 보존, V2 계속 개발
```

---

## 재사용 가능 자산 (변경 없이 사용)

### App 레이어 (UI 독립)
| 자산 | 경로 | 이유 |
|------|------|------|
| `RobotControlViewState` | `App/Fairino/Shell/` | 순수 데이터, UI 무관 |
| `IFairinoRobotClient` | `App/Fairino/` | 인터페이스, UI 무관 |
| `MockFairinoClient` | `App/Fairino/` | Mock 로직, UI 무관 |
| `LiveFairinoClient` | `App/Fairino/` | SDK wrapper, UI 무관 |
| `FairinoConnectionService` | `App/Fairino/` | 연결 수명주기, UI 무관 |
| `FairinoErrorTranslator` | `App/Fairino/` | 에러 코드→메시지 |
| `FairinoRobotConfig` | `App/Fairino/` | 로봇 설정 |
| `FairinoRobotState` | `App/Fairino/` | 상태 캐시 |
| `PreviewRiskSummary` | `App/Fairino/Shell/` | 위험 평가 데이터 |
| `RecoveryHintViewState` | `App/Fairino/Shell/` | 복구 안내 데이터 |
| `RobotControlFactory` | `App/` | 팩토리 패턴, UI 무관 |
| `PresetTransitionAnimator` | `App/Fairino/` | 프리셋 전환 로직 |
| `WaypointCycleRunner` | `App/Fairino/` | 시퀀스 재생 |

### Visualization 레이어 (3D, uGUI 유지)
| 자산 | 경로 | 이유 |
|------|------|------|
| `RobotRenderer` | `Visualization/` | 3D 렌더링, UI Toolkit 무관 |
| `EndEffectorTrail` | `Visualization/` | TrailRenderer |
| `JointHighlightRing` | `Visualization/` | LineRenderer |
| `TargetMarkerVisual` | `Visualization/` | 3D 마커 |
| `SceneCameraDirector` | `App/` | 카메라 제어 |
| `FrameGizmo` 계열 | `Visualization/` | 좌표축 표시 |

---

## 새로 만들어야 할 것 (V3 전용)

### UI Toolkit 인프라
| 파일 | 설명 |
|------|------|
| `pendant-v3.uxml` | 루트 UXML (전체 셸 구조) |
| `pendant-v3.uss` | 루트 USS (디자인 토큰 변수) |
| `pendant-v3-tablet.uss` | 태블릿 레이아웃 오버라이드 |
| `PendantV3Document.cs` | UIDocument MonoBehaviour |
| `PendantV3Binder.cs` | ViewState ↔ VisualElement 바인딩 |

### 패널별 UXML/USS
| 패널 | UXML | USS |
|------|------|-----|
| TopStatusBar | `top-status-bar.uxml` | `top-status-bar.uss` |
| NavRail | `nav-rail.uxml` | `nav-rail.uss` |
| WorkTabBar | `work-tab-bar.uxml` | (공유 USS) |
| 쉬운 조작 | `easy-motion-panel.uxml` | `motion-panels.uss` |
| 관절 조그 | `joint-jog-panel.uxml` | `motion-panels.uss` |
| TCP 조그 | `tcp-jog-panel.uxml` | `motion-panels.uss` |
| 포인트 이동 | `point-move-panel.uxml` | `motion-panels.uss` |
| CoordStrip | `coord-strip.uxml` | `coord-strip.uss` |
| StatusCard | `status-card.uxml` | `status-card.uss` |
| BottomBar | `bottom-bar.uxml` | `bottom-bar.uss` |

### 패널별 C# 바인딩
| 파일 | 역할 |
|------|------|
| `TopStatusBarController.cs` | 상태 인디케이터 갱신 |
| `NavRailController.cs` | 탭 전환 |
| `JointJogPanelController.cs` | 슬라이더 + 입력 + ViewState |
| `TcpJogPanelController.cs` | 방향 버튼 + 좌표계 전환 |
| `CoordStripController.cs` | 좌표값 갱신 |
| `BottomBarController.cs` | 실행 제어 |

---

## V2 코드 중 폐기 대상 (V3 채택 시)

| 파일 | 이유 |
|------|------|
| `UI/RobotControl/Shell/TopStatusBar.cs` | UI Toolkit으로 대체 |
| `UI/RobotControl/Shell/RobotControlShellBinder.cs` | UI Toolkit으로 대체 |
| `UI/RobotControl/Motion/*.cs` | UI Toolkit으로 대체 |
| `UI/RobotControl/Status/*.cs` | UI Toolkit으로 대체 |
| `UI/RobotControl/Teaching/*.cs` | UI Toolkit으로 대체 |
| `UI/RobotControl/FairinoJointControlPanel.cs` | 관절 조그 V3로 대체 |
| `UI/RobotControl/FairinoTcpControlPanel.cs` | TCP 조그 V3로 대체 |
| `UI/RobotControl/FairinoConnectionPanel.cs` | 연결 패널 V3로 대체 |

> **주의**: `App/Fairino/Shell/` 폴더의 Coordinator, ViewState는 **폐기하지 않음**.
> UI 레이어만 교체, 로직은 유지.

---

## 비교 평가 기준표

### Phase 4에서 사용할 비교 매트릭스

| 기준 | 가중치 | V2 (uGUI) | V3 (UI Toolkit) | 측정 방법 |
|------|--------|-----------|-----------------|-----------|
| **개발 속도** | 25% | ? | ? | 동일 패널 구현 시간 비교 |
| **반응형 레이아웃** | 20% | ? | ? | Desktop→Tablet 전환 코드량/품질 |
| **데이터 바인딩** | 15% | ? | ? | ViewState→UI 갱신 코드량 |
| **스타일 유지보수** | 15% | ? | ? | 색상/간격 변경 시 수정 범위 |
| **성능** | 10% | ? | ? | Draw Call, 렌더 시간 프로파일 |
| **학습 곡선** | 10% | ? | ? | 새 패널 추가 시 참고 시간 |
| **3D 통합** | 5% | ? | ? | 월드스페이스 UI + 2D UI 혼합 품질 |

### 채택 기준
- V3 총점이 V2보다 **20% 이상** 높으면 → V3 채택, V2 폐기
- V3 총점이 V2보다 **20% 미만** 차이면 → V2 유지, V3 참고용 보존
- V3가 특정 기준에서 **치명적 문제** 발견 시 → V2 유지

---

## 병행 운영 규칙

V3 평가 기간 동안:
1. V2는 `codex/robotcontrol-shell` 브랜치에서 계속 개발
2. V3는 `codex/robotcontrol-v3-toolkit` 브랜치에서 별도 개발
3. `App/Fairino/` 로직 변경은 양쪽 브랜치에 동시 반영
4. `Visualization/` 변경은 양쪽에 동시 반영
5. `UI/RobotControl/` 변경은 해당 브랜치에서만
