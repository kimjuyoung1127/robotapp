# Pendant V3 Phase 3A Lock

Last Updated: 2026-04-15 (KST)

## 목적
- Phase 3A 1차는 큰 리팩터링이 아니라 `표시 패널 5개를 binder 아래로 모으고`, `씬 bootstrap 순서`를 고정하는 데만 집중한다.
- motion panel, 실기 정책, visualization 계산, undo/redo는 이번 범위에서 제외한다.

## 잠금 규칙

### 상태 원천
| 영역 | 3A 잠금 |
|------|---------|
| preview state | `ConnectionHomeController`를 그대로 SSOT로 둔다 |
| shell local state | `PendantV3ShellStateController.GetStateSnapshot()`만 읽는다 |
| binder | 상태를 새로 소유하지 않고 읽어서 표시 패널로 뿌리기만 한다 |

### 범위
| 포함 | 제외 |
|------|------|
| `StatusCardController` | `JointJogController` |
| `SafetyDiagnosticsController` | `TcpJogController` |
| `ViewportToolbarController` | `PointMoveController` |
| `HelpPanelController` | 실기 connect/move 정책 |
| `WhyItMovedController` | visualization 계산 |
| `PendantV3Binder` | undo/redo |
| `PendantV3SceneCoordinator` | 자동 재연결 |

## 책임 분리

### `PendantV3Binder`
- `ConnectionHomeController.CurrentPreviewDefinition`
- `PendantV3ShellStateController.GetStateSnapshot()`
- 위 두 값을 읽어 표시 패널 5개를 refresh
- `PreviewChanged` 직접 구독은 binder 한 곳으로 모은다

### `PendantV3SceneCoordinator`
- `PendantV3Document` 준비 확인
- `ConnectionHomeController -> PendantV3Binder -> motion/popup controller` 초기화 순서 고정
- bootstrap/orchestration만 담당

### `PendantV3Document`
- `UIDocument`와 `VisualTreeAsset` 보장만 담당
- 여러 controller `ForceInitialize()` 루프는 더 키우지 않는다

## 패널 소비 범위
| 패널 | preview 사용 | shell snapshot 사용 | 메모 |
|------|-------------|--------------------|------|
| `StatusCardController` | O | X | 표시 전용 |
| `SafetyDiagnosticsController` | O | X | fault/warning banner |
| `ViewportToolbarController` | O | X | preview 기반 자동 collision 표시만 binder 범위 |
| `HelpPanelController` | O | O | help visible/title/body 계산 |
| `WhyItMovedController` | O | X | 최근 조작 메모 표시 |

## wiring 변경점
- `PendantV3SceneBuilder`는 `PendantV3Binder`, `PendantV3SceneCoordinator`를 같은 composition root에 부착한다.
- `RobotControlV3DebugBridge`는 binder/coordinator summary와 preview smoke 진입점을 제공한다.

## 검증 기준
1. `unityctl check --type compile`
2. binder/coordinator debug summary 확인
3. preview state smoke에서 표시 패널 5개가 직접 구독 없이 갱신되는지 확인

## 비목표
- 새 preview source 추가
- `LocalSettingsStore` 저장 구조 변경
- panel별 motion/action handler 재정리
- V3 scene 구조 대수술
