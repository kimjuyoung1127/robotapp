# RobotControl 구현 보드

> 최종 갱신: 2026-03-15

## 재활용 컴포넌트 매트릭스

| 기존 컴포넌트 | 위치 | 재활용도 | RobotControl 용도 |
|---|---|---|---|
| `FrameGizmo` | Visualization/ | HIGH (drop-in) | 관절 좌표 프레임 기즈모 |
| `RobotRigBinder` | Visualization/ | HIGH (drop-in) | 씬 계층 유틸리티 |
| `CoordConverter` | Visualization/Shared/ | HIGH (drop-in) | 로보틱스↔Unity 좌표 변환 |
| `SharedLineMaterial` | Visualization/Shared/ | HIGH (drop-in) | LineRenderer Material 캐시 (신규 공유) |
| `WhyItMovedFormatter` | UI/ | HIGH (drop-in) | EE 이동 설명 텍스트 |
| `SnapshotLitePanel` 패턴 | UI/ | HIGH (data model swap) | 프리셋 포즈 저장/비교 |
| `ForwardKinematics` | Kinematics/ | HIGH (순수 수학) | FK 계산 엔진 |
| `TemplateFAIRINO_FR5` | Templates/ | HIGH (순수 정의) | FR5 DH 파라미터 |
| `DHTableValueFormatter` | UI/ | HIGH (순수 로직) | DH 값 포맷/파싱 |
| `KinematicsRuntimeService` | App/ | MEDIUM (core 추출) | FK facade 기반 |
| `JointInputValidator` | UI/ | MEDIUM (어댑터) | 관절값 검증 |
| `WhyItMovedPanel` | UI/ | LOW (재작성) | 학습 컨텍스트 너무 밀접 |
| `AppController` | App/ | LOW (사용 안함) | FR5 전용 facade 대체 |

---

## Phase별 구현 체크리스트

### Phase 0: 문서 + 사전 준비
- [x] `docs/status/ROBOTCONTROL-IMPL-BOARD.md` 생성
- [x] `docs/ref/code-patterns.md` §8-9 준수 확인

### Phase 1 (P0): 3D 관절 미러 + 방향 수정
- [x] `Visualization/FairinoUrdfJointDriver.cs` 신규 → Transform 기반 제어로 전환 완료
- [x] `App/Fairino/RobotControlSceneCoordinator.cs` 수정 → AB 비활성화 + TeleportRoot 프리팹 포즈 유지
- [x] Play Mode 검증: 베이스 -Y(바닥) 방향 유지, 로봇 정립 확인 (2026-03-14)

### Phase 2 (P1): 탭 UI + 슬라이더 프리뷰
- [x] `UI/FairinoJointControlPanel.cs` 수정
- [x] `UI/FairinoRobotControlViewBuilder.cs` 수정 — EventSystem `AssignDefaultActions` + overlay `raycastTarget=false`
- [x] `UI/UIComponentFactory.cs` 수정 — Slider 자체 GameObject에 Image 추가 (GraphicRaycaster 감지)
- [x] `App/Fairino/RobotControlSceneCoordinator.cs` 추가 수정
- [x] Play Mode 검증: 슬라이더→3D 회전 확인 (2026-03-14, D7 해결 후)

### Phase 3 (P2): DH 파라미터 탭 + FK 교육
- [x] `App/Fairino/FR5KinematicsFacade.cs` 신규
- [x] `UI/FairinoDHPanel.cs` 신규 (탭에서 TCP Control로 대체, 파일 유지)
- [x] Play Mode 검증: DH 탭→값 편집→FK 매트릭스 갱신→3D 프리뷰

### Phase 4 (P3): 좌표 프레임 기즈모 + EE 궤적 트레일
- [x] `Visualization/FrameGizmoFactory.cs` 신규
- [x] `Visualization/EETrailRenderer.cs` 공유 코어로 리팩터링 → SharedLineMaterial 통합
- [x] `Visualization/EndEffectorTrail.cs` EETrailRenderer 위임 어댑터로 전환
- [x] Play Mode 검증: EE 트레일 표시 확인됨

### Phase 5 (P4): 관절 델타 + Why It Moved
- [x] `UI/FairinoStatePanel.cs` 수정 — EE XYZ RGB 색상 코딩 추가
- [x] `UI/FairinoWhyItMovedLabel.cs` 수정 — 다관절 동시 변경 시 상위 2개 요약 + XYZ 성분별 표시
- [x] Play Mode 검증: WhyItMoved 한국어 표시 확인됨

### Phase 6 (P5): 프리셋 포즈 + 안전 장치
- [x] `App/Fairino/FR5PosePresets.cs` 수정 — Current 동적 프리셋 + 캐시된 All 프로퍼티
- [x] `UI/FairinoMoveConfirmDialog.cs` 신규
- [x] `UI/FairinoConnectionPanel.cs` 수정
- [x] Play Mode 검증: 프리셋 원클릭→포즈 적용, Live 확인 대화상자

### Phase 7 (P0~P5 확장): 카메라 + TCP + 핸들 + 동기화 + UI 편의
- [x] **P0 카메라**: `SceneCameraDirector.cs` — RobotControl CameraProfile 좌표 확정
- [x] **P1 회전 핸들**: `Visualization/Shared/JointRotationHandle.cs` 신규 — 6관절 1축 회전 링, 색상별 구분, 드래그→슬라이더 동기화
- [x] **P1 핸들 API**: `FairinoUrdfJointDriver.cs` — `GetJointTransform(int)`, `GetJointRotationAxis(int)` 추가
- [x] **P2 TCP 탭**: `UI/FairinoTcpControlPanel.cs` 신규 — X/Y/Z/Rx/Ry/Rz 입력 + MoveL/ServoCart + DryRun
- [x] **P2 MoveL**: `IFairinoRobotClient` + `MockFairinoClient` + `LiveFairinoClient` — MoveL 인터페이스/구현
- [x] **P2 탭 교체**: `FairinoRobotControlViewBuilder.cs` — "DH Params" → "TCP Control" 탭 교체
- [x] **P3 동기화**: `FairinoConnectionService.cs` — `SyncCurrentState()` 추가
- [x] **P3 Sync UI**: `FairinoJointControlPanel.cs` — Sync 버튼 + Mock 비활성화
- [x] **P4 변위 화살표**: `Visualization/Shared/DisplacementArrow.cs` 신규 — EE 변위 벡터 화살표
- [x] **P5 TopBar**: `FairinoRobotControlViewBuilder.cs` — 기즈모 토글 + 트레일 Clear 버튼
- [x] **공용화**: `Visualization/Shared/SharedLineMaterial.cs` 신규 — Material 캐시 통합
- [x] **공용화**: `FairinoRobotConfig.cs` — `GetMediumSpeedAcc()` + `GetSpeedAcc(preset)` 속도/가속 헬퍼
- [ ] Play Mode 검증: 전체 회전 핸들 + TCP 제어 + 변위 화살표 시각 검증

### Phase 8: 프리셋 애니메이션 + Speed Selector + 연결 안전
- [x] **프리셋 애니메이션**: `App/Fairino/PresetTransitionAnimator.cs` 신규 — EaseInOutCubic 보간 (1.5초)
- [x] **Speed Selector**: `FairinoJointControlPanel.cs` + `FairinoTcpControlPanel.cs` — Slow/Medium/Fast 속도 선택
- [x] **연결 끊김**: `FairinoConnectionService.cs` — 3회 연속 에러 → `OnConnectionLost` + 자동 해제
- [x] **패널 비활성화**: `SetControlsEnabled(bool)` — JointControlPanel + TcpControlPanel
- [x] **재연결 안내**: `FairinoConnectionPanel.ShowConnectionLost()` — 빨간색 상태 텍스트
- [x] **연결 초기화**: `OnStateUpdated` 첫 프레임 → 0.8초 보간 전환
- [x] **UI 토큰**: `UIDesignTokens.Anim.PresetTransition=1.5f`, `ConnectionSync=0.8f`
- [x] **EditMode 테스트**: `PresetTransitionAnimatorTests.cs` — EaseInOutCubic + LerpDouble 13개
- [ ] Play Mode 검증: 프리셋 보간 + Speed 선택 + 연결 끊김/복구 시각 검증

### Phase 9: 최종 검증
- [x] EditMode 테스트 green (FR5KinematicsFacade + FR5PosePresets + PresetTransitionAnimator + Integration)
- [ ] 전체 Mock 루프: 슬라이더→핸들→3D + TCP 탭 검증
- [ ] 스크린샷 캡처

---

## UI 계층 트리

```
RobotControl.unity
├── FR5_RuntimeRoot
│   ├── FR5_UrdfInstance (FAIRINO_FR5_Control prefab)
│   │   └── base_link → shoulder_link → upperarm_link → forearm_link → wrist1 → wrist2 → wrist3
│   ├── FrameGizmos (FrameGizmoFactory — 6관절 좌표 프레임)
│   ├── EETrail (EETrailRenderer — End Effector 궤적)
│   ├── DisplacementArrow (EE 변위 벡터)
│   └── JointHandles
│       ├── Handle_J1..J6 (JointRotationHandle — 관절 회전 링)
│
├── Canvas (Screen Space - Overlay)
│   ├── TopBar
│   │   ├── GizmoToggle (Frame Gizmo On/Off)
│   │   └── ClearTrailButton
│   ├── TabBar (Joint Control / TCP Control / State)
│   ├── ConnectionPanel (FairinoConnectionPanel)
│   │   ├── IP Input + Connect/Disconnect
│   │   ├── Enable/Disable + Mock Toggle
│   │   ├── StatusLabel (AccentSuccess/AccentDanger)
│   │   └── VersionLabel
│   ├── JointControlPanel (FairinoJointControlPanel)
│   │   ├── JointRow_1..6 (Slider + Label, 관절 색상 코딩)
│   │   ├── SpeedButtons [Slow 10%] [Medium 30%] [Fast 60%]
│   │   ├── MoveJ / ServoJ / Stop / Sync
│   │   ├── DryRun Toggle
│   │   ├── FeedbackLabel
│   │   └── PresetButtons [Zero] [Home] [Ready] [Current]
│   ├── TcpControlPanel (FairinoTcpControlPanel)
│   │   ├── TcpRow_0..5 (X/Y/Z/Rx/Ry/Rz InputField)
│   │   ├── SpeedButtons [Slow 10%] [Medium 30%] [Fast 60%]
│   │   ├── MoveL / ServoCart
│   │   ├── DryRun Toggle
│   │   ├── CurrentTcpLabel (FK 결과 읽기 전용)
│   │   └── FeedbackLabel
│   ├── StatePanel (FairinoStatePanel — EE XYZ RGB)
│   ├── WhyItMovedLabel (관절→TCP 이동 설명)
│   └── MoveConfirmDialog (Live 모드 확인)
│
├── Main Camera (OrbitCameraController)
├── Directional Light
└── EventSystem
```

## 데이터 흐름 다이어그램

```
┌─────────────────────────────────────────────────────────────────────┐
│                    RobotControlSceneCoordinator                     │
│  (오케스트레이터: 모든 이벤트 라우팅 + 시각화 동기화)                 │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
   ┌──────────────┬────────────┼────────────┬───────────────┐
   │              │            │            │               │
   ▼              ▼            ▼            ▼               ▼
Connection    JointControl  TcpControl  PresetTransition  ConnectionService
  Panel         Panel        Panel       Animator          (Mock/Live)
   │              │            │            │               │
   │  ┌───────────┘            │            │               │
   │  │ OnJointSliderPreview   │            │               │
   │  │ OnPresetApplied        │            │               │
   │  │ OnSyncRequested        │            │               │
   │  │                        │            │               │
   │  ▼                        │            │               │
   │  ┌────────────────┐       │            │               │
   │  │ Speed Selector │───────┼───► GetSpeedAcc(preset)    │
   │  │ [S] [M] [F]    │       │     ──────►FairinoRobot    │
   │  └────────────────┘       │            Config          │
   │                           │                            │
   │                           ▼                            │
   │                    OnTcpMoveRequested                   │
   │                                                        │
   ▼                                                        ▼
Connect/                                              Tick(deltaTime)
Disconnect ──► OnConnectionStateChanged              ReadState() 폴링
               OnConnectionLost (3회 실패)                  │
               isFirstStateAfterConnect                     │
                        │                                   │
                        ▼                                   ▼
                  ┌──────────────────────────────────────────┐
                  │        Coordinator Event Handlers         │
                  │                                          │
                  │  OnStateUpdated:                         │
                  │    첫 연결 → Animator(0.8s 보간)          │
                  │    이후    → 즉시 적용                    │
                  │                                          │
                  │  OnPresetApplied:                        │
                  │    → Animator(1.5s EaseInOutCubic 보간)   │
                  │    → Trail/Arrow Clear                   │
                  │                                          │
                  │  OnJointSliderPreview / OnHandleDragged: │
                  │    → 애니메이션 Cancel                    │
                  │    → 즉시 JointDriver + FK + 시각화       │
                  │                                          │
                  │  OnConnectionLost:                       │
                  │    → 애니메이션 Cancel                    │
                  │    → SetControlsEnabled(false)           │
                  │    → ShowConnectionLost()                │
                  │                                          │
                  │  OnConnectionStateChanged(true):         │
                  │    → SetControlsEnabled(true)            │
                  │    → isFirstStateAfterConnect = true     │
                  └──────────────┬───────────────────────────┘
                                 │
              ┌──────────────────┼──────────────────┐
              ▼                  ▼                  ▼
     FairinoUrdfJoint    FR5Kinematics      Visualization
        Driver            Facade            (Gizmo/Trail/Arrow)
     (Transform 직접)   (FK 계산)          (3D 렌더링)
```

## 실기 연결 체크리스트

1. FR5 전원 ON + 네트워크 연결 확인
2. Mock Toggle → OFF (Live 모드 전환)
3. IP 입력 → Connect → StatusLabel 녹색 확인
4. Enable 버튼 → 서보 활성 확인
5. DryRun OFF → MoveJ 또는 MoveL 실행
6. 연결 끊김 테스트: 케이블 분리 → 3회 폴링 실패 → 빨간 "연결 끊김" 확인
7. 재연결: Connect → 패널 자동 활성화 + 0.8초 포즈 보간

---

## 디버깅 이력 (2026-03-14)

### 해결 완료

| # | 문제 | 원인 | 수정 |
|---|---|---|---|
| D1 | 로봇 옆으로 누움 | `Transform.localRotation` 직접 설정 → ArticulationBody가 무시 | `ArticulationBody.TeleportRoot()` 사용 |
| D2 | 파츠 분리 (관절 떨어짐) | `NormalizeScale()`이 부모 scale 변경 → ArticulationBody가 부모 scale 무시하여 물리 위치 불일치 | `NormalizeScale` 제거, 카메라를 실물 크기에 맞춤 |
| D3 | `EETrailRenderer` MissingComponentException | `GetComponent<T>() ?? AddComponent<T>()`에서 Unity fake-null 미처리 | 명시적 `== null` 체크 후 `AddComponent` |
| D4 | `FR5PosePresets` 불변성 깨짐 | `double[]` getter가 내부 배열 참조 직접 반환 | getter에서 `Clone()` 반환 |
| D5 | 슬라이더 드래그 시 3D 로봇 미동작 | AB가 FixedUpdate에서 Transform을 덮어씀 | AB→Transform 전환: non-root AB `enabled=false` |
| D6 | TeleportRoot 후 베이스가 X축을 바라봄 | `Euler(90,0,0)`이 프리팹 포즈를 덮어씀 | `TeleportRoot(현재 position, 현재 rotation)` |
| D7 | 슬라이더 조작 불가 | Slider GameObject에 `Image` 없음 → `GraphicRaycaster` 감지 불가 | Slider에 `Image` 추가 |

### 자기리뷰 수정 (2026-03-14)

| # | 문제 | 원인 | 수정 |
|---|---|---|---|
| R1 | `OnHandleDragged` 슬라이더 미동기화 | 길이 1 배열로 `SetSliderValues` 호출 | FK 현재값 기반 해당 관절만 교체 후 전체 전파 |
| R2 | 핸들 히트 판정 부정확 | 링 평면 무시, 먼 곳 클릭도 히트 | Ray-plane 교차 후 반경 비교로 변경 |
| R3 | 드래그 종료 시 강조 유지 | `SetSelected(false)` 누락 | `EndDrag()`에 추가 |
| R4 | 공유 Material 색상 오염 | `sharedMaterial.color` 직접 수정 | `SharedLineMaterial.CreateInstance(color)` |
| R5 | TCP 패널 재진입 가드 없음 | `EnsurePresentation` 재실행 | `currentTcpLabel != null` 가드 |
| R6 | 핸들 이벤트 중복 바인딩 | `UnbindListeners`에서 핸들 해제 누락 | `UnbindHandleListeners()` 추가 |

### 핵심 교훈: ArticulationBody vs Transform

> **최종 결론 (2026-03-14):**
> 교육용 3D 미러에는 ArticulationBody 물리 제어가 불필요.
> non-root AB를 `enabled=false`로 비활성화하고 Transform.localRotation을 직접 제어하는 것이 안정적.
>
> **Transform 기반 관절 제어 패턴:**
> 1. `CacheJoints()`: AB의 `anchorRotation * Vector3.right`로 회전축 계산, `localRotation` 캐싱
> 2. `ApplyJointAngles()`: `localRotation = initial * AngleAxis(deg, axis)`
> 3. `DisableArticulationBodies()`: non-root AB `enabled=false` (root는 immovable 유지)
> 4. `TeleportRoot(현재 pos, 현재 rot)`: root AB 내부 상태를 프리팹 트랜스폼에 동기화

---

## 파일 총괄 (신규 5 + 수정 12 + 기존 유지 11 = 28)

| # | 파일 | Phase | 작업 | 역할 |
|---|------|-------|------|------|
| 1 | Visualization/FairinoUrdfJointDriver.cs | P1 | 수정 | Transform 관절 제어 + GetJointTransform/GetJointRotationAxis API |
| 2 | App/Fairino/RobotControlSceneCoordinator.cs | ALL | 전면 재작성 | 전체 코디네이터: 애니메이션/TCP/핸들/화살표/기즈모/Sync/연결끊김 통합 |
| 3 | UI/FairinoJointControlPanel.cs | P2,P3,P8 | 수정 | 슬라이더 + 프리셋 + Sync + Speed Selector + SetControlsEnabled |
| 4 | UI/FairinoRobotControlViewBuilder.cs | P2,P5 | 전면 재작성 | 3탭(Joint/TCP/State) + TopBar(기즈모토글/Clear Trail) |
| 5 | UI/UIComponentFactory.cs | P2 | 수정 | Slider Image 추가 |
| 6 | App/Fairino/FR5KinematicsFacade.cs | P3 | 기존 | FK facade |
| 7 | UI/FairinoDHPanel.cs | P3 | 기존 유지 | DH 테이블 (탭에서 제외, 파일 보존) |
| 8 | Visualization/Shared/FrameGizmoFactory.cs | P4 | 기존 | 6관절 기즈모 |
| 9 | Visualization/Shared/EETrailRenderer.cs | P4 | 수정 | SharedLineMaterial 전환 |
| 10 | UI/FairinoStatePanel.cs | P5 | 수정 | EE XYZ RGB 색상 코딩 |
| 11 | UI/FairinoWhyItMovedLabel.cs | P5 | 수정 | 다관절 요약 + XYZ 성분 |
| 12 | App/Fairino/FR5PosePresets.cs | P6 | 수정 | Current 동적 프리셋 + 캐시 |
| 13 | UI/FairinoMoveConfirmDialog.cs | P6 | 기존 | Live 확인 대화상자 |
| 14 | App/SceneCameraDirector.cs | P0 | 수정 | RobotControl 카메라 프로파일 |
| 15 | UI/FairinoTcpControlPanel.cs | P2,P8 | 수정 | TCP 제어 + Speed Selector + SetControlsEnabled |
| 16 | Visualization/Shared/JointRotationHandle.cs | P1 | 기존 | 관절 회전 링 핸들 |
| 17 | Visualization/Shared/DisplacementArrow.cs | P4 | 기존 | EE 변위 벡터 화살표 |
| 18 | Visualization/Shared/SharedLineMaterial.cs | 공용 | 기존 | LineRenderer Material 캐시 |
| 19 | App/Fairino/IFairinoRobotClient.cs | P2 | 수정 | MoveL 인터페이스 추가 |
| 20 | App/Fairino/MockFairinoClient.cs | P2 | 수정 | MoveL mock 구현 |
| 21 | App/Fairino/LiveFairinoClient.cs | P2 | 수정 | MoveL SDK 연동 |
| 22 | App/Fairino/FairinoConnectionService.cs | P3,P8 | 수정 | SyncCurrentState + OnConnectionLost (3회 실패) |
| 23 | App/Fairino/FairinoRobotConfig.cs | P8 | 수정 | GetSpeedAcc(preset) 추가 |
| 24 | UI/FairinoConnectionPanel.cs | P6,P8 | 수정 | ShowConnectionLost() 추가 |
| 25 | UI/UIDesignTokens.cs | P8 | 수정 | Anim.PresetTransition + ConnectionSync |
| 26 | **App/Fairino/PresetTransitionAnimator.cs** | P8 | **신규** | EaseInOutCubic 관절 보간 |
| 27 | KineTutor3D.Runtime.asmdef | - | 기존 | Unity.InputSystem 참조 |
| 28 | App/Fairino/FairinoRobotState.cs | - | 기존 | 불변 상태 구조체 |

## 테스트 파일 (신규 4 + 수정 1)

| # | 파일 | 테스트 수 | 상태 |
|---|------|----------|------|
| 1 | Tests/EditMode/FR5KinematicsFacadeTests.cs | 22 | 22 Passed |
| 2 | Tests/EditMode/FR5PosePresetsTests.cs | 10 | 수정 (4프리셋 + UpdateCurrent) |
| 3 | Tests/EditMode/FR5KinematicsFacadeIntegrationTests.cs | 6 | 6 Passed |
| 4 | **Tests/EditMode/PresetTransitionAnimatorTests.cs** | 13 | **신규** (EaseInOutCubic + LerpDouble) |
