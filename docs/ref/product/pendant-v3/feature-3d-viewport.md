# 3D 뷰포트 + 디지털 트윈

## Purpose
- 중앙 3D 뷰포트의 구성 요소와 인터랙션을 정의한다.
- 기존 펜던트에 없는 "보이는 펜던트" 차별화 포인트.

## Parent Doc
- [README.md](./README.md)

## Related Docs
- [implementation-plan.md](./implementation-plan.md)
- [unityctl-recipes.md](./unityctl-recipes.md)
- [feature-coordinates.md](./feature-coordinates.md)
- [feature-safety-controls.md](./feature-safety-controls.md)

## Last Updated
- 2026-04-23 (KST, orientation gizmo preset-click 반영)

---

## 뷰포트 구성 요소

```text
┌─ 3DViewport ─────────────────────────────────────────┐
│                                                       │
│  ┌─ Orientation Gizmo (우상단 고정) ───────────┐   │
│  │      X / Y / Z 카메라 방향 위젯             │   │
│  └───────────────────────────────────────────────┘   │
│                                                       │
│                                                       │
│           현재 로봇 메시 (불투명)                     │
│           선택 링크 강조 / 조작중 관절 링            │
│           EE Trail / Target / Ghost / PredictedPath   │
│                                                       │
│           [Ghost Robot] (명시적 미리보기일 때만)      │
│           ........→ 예상 경로 (점선)                  │
│                                                       │
│                                                       │
│  ┌─ 카메라 컨트롤 (뷰포트 우측 하단) ──────────┐    │
│  │  드래그: 회전 / 스크롤: 줌 / 중클릭: 팬     │    │
│  │  [정면] [측면] [상면] [아이소]               │    │
│  └──────────────────────────────────────────────┘    │
│                                                       │
└───────────────────────────────────────────────────────┘
```

## 2026-04-20 표시 위치 잠금

- Desktop 메인 로봇 표시는 별도 `ViewportHost`가 아니라 `WorkPanel` 내부 `RobotStage`에서 수행한다.
- `RobotStage`는 `WorkPanel` 본체를 거의 전부 차지하는 단독 3D 표시 영역으로 둔다.
- 탭별 조작 UI와 연결 홈/도움말은 `ViewportHost`의 보조 작업 패널로 이동한다.
- `ViewportHost`는 1차 구현에서 삭제하지 않더라도 메인 디지털 트윈 역할을 맡지 않는다.
- 따라서 이 문서의 `3DViewport`는 앞으로 **독립 패널**이 아니라 **WorkPanel 내부 임베디드 로봇 스테이지**로 해석한다.

## WorkPanel 임베디드 구조

```text
┌─ WorkPanel ──────────────────────────────────────────┐
│  Header                                              │
│  Tab summary / panel title                           │
│                                                      │
│  ┌─ RobotStage (robot only) ──────────────────────┐  │
│  │                                                │  │
│  │      3D 로봇 / 프레임 / 트레일 / 고스트        │  │
│  │      바닥 격자 / 선택 링크 강조 / 우상단 방향 위젯 │  │
│  │                                                │  │
│  └────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘

┌─ ViewportHost (보조 작업 패널) ──────────────────────┐
│  공용 시각화 토글 카드                               │
│  [궤적] [고스트] [경계] [충돌] [카메라↺]           │
│                                                      │
│  공용 Scroll                                          │
│  - TCP 3D 화살표 / RPY 보조 조작                      │
│  - 연결 홈                                            │
│  - 쉬운 조작                                          │
│  - 관절 조그                                          │
│  - TCP 조그                                           │
│  - 포인트 이동                                        │
└──────────────────────────────────────────────────────┘
```

---

## 레이어 시스템

| 레이어 | 내용 | 기본 상태 | 토글 |
|--------|------|-----------|------|
| Robot | 현재 로봇 메시 + 링크 | 항상 ON | - |
| OrientationWidget | 우상단 카메라 방향 위젯 | 항상 ON | - |
| Ghost | 목표 자세 반투명 로봇 | 명시적 미리보기 시 ON | 시각화 토글 카드 |
| Trail | EE 경로 궤적 (노란색, 3초 유지) | ON | 3D 툴바 |
| PredictedPath | 예상 이동 경로 (점선) | 미리보기 시 ON | 자동 |
| BaseFrame | 베이스 좌표축 (RGB) | 제품 경로 제외 | - |
| ToolFrame | 툴 좌표축 | 제품 경로 제외 | - |
| UserFrame | 사용자 좌표축 | OFF | 3D 툴바 |
| JointHighlight | 관절 링 + 링크 발광 | 조그 시 ON | 자동 |
| TargetMarker | 목표점 마커 (체크/경고) | 포인트 이동 시 ON | 자동 |
| CollisionZone | 충돌 위험 구간 하이라이트 | 위험 감지 시 ON | 자동 |
| WorkspaceBoundary | 작업공간 경계 (도달 범위 반투명 구체) (A1) | OFF | 3D 툴바 |
| CartesianArrows | 데카르트 화살표 (XYZ 이동 + RPY 회전 링) (U12) | TCP 조그 시 ON | 자동 |
| MultiPointPath | 다중 포인트 전체 경로 점선 (A7) | 시퀀스 미리보기 시 ON | 자동 |

---

## 고스트 미리보기 상세

### 동작 흐름
```text
1. 사용자가 목표 자세 설정 (슬라이더/좌표 입력)
2. [미리보기] 버튼 클릭
3. Ghost Robot이 목표 자세로 표시 (반투명 30%, 명시적 미리보기일 때만)
4. 현재 로봇 → 고스트 사이에 예상 경로 점선
5. 위험 구간이 있으면 빨간 하이라이트
6. [적용] 클릭 시 실제 이동 (또는 DryRun)
```

### 고스트 시각 스타일
- **색상**: 현재 로봇과 동일 메시, 투명도 30%
- **테두리**: `AccentPrimary` 와이어프레임 오버레이
- **위험 시**: 충돌 예상 링크에 `AccentDanger` 발광

### 예상 경로 표시
- **MoveJ**: 각 관절의 보간 궤적을 EE 경로로 표시 (곡선)
- **MoveL**: 직선 경로 (실선)
- **색상**: 안전 = `AccentPrimary`, 위험 구간 = `AccentDanger`

### 현재 상태 / 목표 상태 / 경로 구분 잠금

사용자는 1시간 사용 시점에도 `지금 로봇`, `내가 만들려는 목표`, `예상 경로`를 절대 헷갈리지 않아야 한다.
따라서 아래 시각 규칙을 고정한다.

| 요소 | 시각 규칙 | 사용자가 이해해야 하는 의미 |
|------|------|------|
| 현재 로봇 | 불투명 100%, 기본 재질 | "지금 실제 상태" |
| 목표 고스트 | 반투명 30%, 와이어 오버레이 | "아직 적용 전 목표 상태" |
| 예상 경로 | 점선 또는 반투명 선 | "적용하면 지나갈 경로" |
| 위험 구간 | 빨강 발광 + 빨강 구간 표시 | "그대로 실행하면 위험" |
| 선택 좌표축 | 100% 불투명 | "현재 조작 기준 좌표계" |
| 비선택 좌표축 | 30% 불투명 | "지금 기준은 아님" |

- 현재 로봇과 목표 고스트는 같은 색/투명도로 표시하지 않는다.
- 현재 상태가 변하지 않았는데 목표 상태만 바뀐 경우, 반드시 고스트만 움직여야 한다.
- `미리보기 해제` 시 고스트와 예상 경로는 즉시 사라지고 현재 로봇만 남는다.
- 기본 상태에서는 고스트를 켜 두지 않는다. 고스트는 `미리보기` 또는 전용 `Ghost` 토글이 켜진 동안에만 보인다.

---

## Orientation Gizmo

- `RobotStage` 우상단에는 Unity Scene Gizmo처럼 보이는 작은 `orientation widget`을 둔다.
- 이 위젯은 **카메라 방향 표시 전용**이다.
- `Base/Tool/선택 파츠` 기준 좌표축 역할을 대신하지 않는다.
- v1에서는 카메라 방향 표시가 기본 역할이다.
- 추가 클릭 동작은 카메라 프리셋 점프까지만 허용한다.
  - `Z` badge: `Front`
  - `X` badge: `Right`
  - `Y` badge: `Top`
  - 중심점: `Iso`
- 카메라 회전/팬/줌은 계속 `RobotStage` 포인터 입력이 담당한다.
- 색상은 `X=Red`, `Y=Green`, `Z=Blue` 고정.
- 월드 공간 오브젝트가 아니라 `RobotStage` 안 screen-space overlay widget으로 렌더링한다.
- `RobotStage` 기본 시각 정보는 아래 우선순위로 구분한다.
  1. 현재 로봇
  2. 선택 링크 강조 / 관절 링
  3. Ghost / 예상 경로
  4. 우상단 orientation widget

### Orientation Gizmo와 기존 기즈모 경계

- 우상단 orientation widget은 `카메라가 지금 어느 방향을 보고 있는지`만 알려준다.
- 우상단 orientation widget 클릭은 카메라 프리셋 점프만 수행한다.
- `BaseFrame`, `ToolFrame`은 과거 world gizmo 역할이며, 현재 메인패널 제품 경로에서는 기본 제외한다.
- 선택 파츠 작은 축은 `선택된 링크의 로컬 기준` 보조 표시다.
- 이 세 역할을 하나의 위젯으로 합치지 않는다.

---

## 카메라 프리셋

| 프리셋 | 위치 | 용도 |
|--------|------|------|
| 정면 (Front) | +Y 방향에서 촬영 | TCP 좌우 확인 |
| 측면 (Side) | +X 방향에서 촬영 | 도달 거리 확인 |
| 상면 (Top) | +Z 방향에서 촬영 | 작업 영역 전체 확인 |
| 아이소 (Iso) | 45° 대각선 | 기본 뷰, 전체 파악 |
| 카메라 리셋 | 아이소로 복귀 | 카메라 위치 초기화 |

### 카메라 인터랙션
- **좌클릭 드래그**: 궤도 회전 (OrbitCamera)
- **스크롤**: 줌 인/아웃
- **중클릭 드래그**: 팬
- **더블클릭**: 클릭한 링크/포인트를 중심으로 포커스

---

## 관절 조작 시 3D 연동

### 관절 조그 시
```text
사용자가 J2 슬라이더를 드래그
  → J2 관절에 하이라이트 링 표시 (LineRenderer, 노란색)
  → J2 링크에 emission 발광
  → EE trail이 실시간 갱신
  → 3D에서 로봇이 즉시 반응 (미리보기)
```

### TCP 조그 시
```text
사용자가 X+ 버튼 클릭
  → 3D에서 Base 좌표축 X 방향 화살표 강조
  → TCP가 X+ 방향으로 이동 미리보기
  → EE trail 갱신
```

---

## 기존 재사용 자산

| 컴포넌트 | 현재 위치 | V3 재사용 |
|----------|----------|-----------|
| `RobotRenderer` | `Visualization/` | 그대로 사용 |
| `EndEffectorTrail` | `Visualization/` | 그대로 사용 |
| `JointHighlightRing` | `Visualization/` | 그대로 사용 |
| `TargetMarkerVisual` | `Visualization/` | 그대로 사용 |
| `SceneCameraDirector` | `App/` | 그대로 사용 |
| `FrameGizmo` | `Visualization/` | 그대로 사용 |

> 3D 시각화 레이어는 uGUI 기반이 아니므로 UI Toolkit 전환에 영향 없음.
> 이것이 V3 하이브리드 전략의 핵심 이점.

---

## V3 경계 잠금

### UI Toolkit / 3D 역할 분리
- V3의 2D 셸은 `UI Toolkit`이 소유한다.
- 로봇 메시, 프레임 기즈모, 트레일, 고스트, 충돌 하이라이트, 타깃 마커는 기존 `Visualization/` 계층이 소유한다.
- `RobotStage`는 3D 카메라가 그려지는 메인 UI 컨테이너다. UI Toolkit 요소를 World Space로 직접 렌더링하지 않는다.
- `RobotStage` 우상단 orientation widget은 `UI Toolkit` overlay controller가 소유한다.
- `ViewportHost`는 별도 보조 컨테이너가 필요할 때만 유지하고, 메인 로봇 메시 표시 책임은 갖지 않는다.
- `ViewportHost` 내부 조작 패널은 공용 `ScrollView` 스타일을 공유해 탭별 패널 길이가 길어져도 같은 스크롤 규칙으로 흘린다.
- `TCP 3D 화살표`, 특히 `Z / RX / RY / RZ` 조작 UI는 로봇을 가리지 않도록 `RobotStage`가 아니라 `ViewportHost` 쪽 보조 패널에서 표시한다.

### 렌더 경계
- 메인 카메라는 기존 `SceneCameraDirector` 기준으로 유지한다.
- V3는 `WorkPanel` 안 `RobotStage` rect를 기준으로 카메라 viewport 또는 RenderTexture 표시 영역을 제어한다.
- `TopStatusBar`, `NavRail`, `ContextPanel`, `BottomBar`, popup은 3D 위 오버레이로 취급하고 3D가 이를 침범하지 않는다.

### 입력 경계
- `RobotStage` 내부 포인터 입력만 카메라 조작과 3D hit-test에 전달한다.
- `RobotStage` 외의 V3 UI 입력은 3D에 전달하지 않는다.
- TCP 화살표, 툴바 버튼, 카메라 프리셋 버튼은 모두 UI 이벤트로 처리하고, 3D 조작 명령은 presenter를 통해 visualization 계층에 전달한다.

### 채택 전 금지사항
- UI Toolkit World Space UI를 V3 주 경로에 도입하지 않는다.
- 3D 카메라 입력과 UI Toolkit 포인터 입력을 같은 요소에서 동시에 처리하지 않는다.
- V3 2D UI 안에서 3D 상태를 직접 계산하지 않는다. 3D 파생 상태는 `RobotControlViewState` 또는 preview state를 통해서만 소비한다.
