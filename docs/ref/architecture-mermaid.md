# KineTutor3D Architecture Mermaid

This is the fastest whole-system context document for new sessions.
Read this after `AGENTS.md` and before drilling into individual runtime files.

## 1. System Overview

```mermaid
flowchart TD
    Boot["Boot.unity"] --> Router["BootSceneRouter"]
    Router -->|first visit| Onboarding["Onboarding.unity"]
    Router -->|return visit| Home["Home.unity"]

    Onboarding -->|시작/건너뛰기| Home
    Onboarding -->|초보자 시작| MathReady["MathReadiness.unity"]

    Home --> Nav["SceneNavigationBar"]
    Home -->|이어하기| Main["Main.unity"]
    Home -->|수학 기초| MathReady
    Home -->|로봇 선택| RobotLib["RobotLibrary.unity"]
    Home -->|샌드박스| Sandbox["Sandbox.unity"]

    Main --> Nav
    MathReady --> Nav
    RobotLib --> Nav
    Sandbox --> Nav

    RobotLib -->|FR5 제어| RobotCtrl["RobotControl.unity"]
    RobotLib -->|학습 시작| Main
    RobotLib -->|샌드박스| Sandbox

    Main --> App["AppController"]
    MathReady --> App
    Sandbox --> SandboxCoord["SandboxSceneCoordinator"]
    RobotCtrl --> RobotCtrlCoord["RobotControlSceneCoordinator"]

    App --> UI["HUD UI\nDHTableEditor / MatrixDisplay / StepNavigator\nWhyItMovedPanel / JointInputRail / BeginnerLeftPanel"]
    App --> Viz["RobotRenderer"]
    Viz --> Frame["FrameGizmo"]
    Viz --> Donor["Scara donor visuals\nBase / Axis1 / Axis2 / Gripper"]
    Viz --> Trail["EndEffectorTrail / JointHighlightRing"]
    Viz --> Target["TargetMarkerVisual"]

    Router -->|robot library| RobotLib["RobotLibrary.unity"]
    RobotLib --> RLM["RobotLibraryManager\nRobotCardBuilder / RobotDetailDrawer"]
    RLM --> Catalog["RobotCatalog\n5 robots registered"]
```

## 2. RobotControl Scene Data Flow

```mermaid
flowchart TD
    subgraph Input
        Slider["JointSliders (6축)"]
        Handle["JointRotationHandle (6축 링)"]
        TcpInput["TcpControlPanel\nX/Y/Z/Rx/Ry/Rz"]
        Preset["FR5PosePresets\nHome/Ready/Folded/Current"]
        SyncBtn["Sync 버튼"]
    end

    subgraph Coordinator["RobotControlSceneCoordinator"]
        OnSlider["OnJointSliderPreview"]
        OnHandle["OnHandleDragged"]
        OnPreset["OnPresetApplied"]
        OnTcp["OnTcpMoveRequested"]
        OnSync["OnSyncRequested"]
    end

    subgraph FK["FK 파이프라인"]
        Facade["FR5KinematicsFacade"]
        DH["DHStandard + ForwardKinematics"]
    end

    subgraph Output3D["3D 출력"]
        Driver["FairinoUrdfJointDriver\nTransform.localRotation"]
        Gizmo["FrameGizmoFactory\n6관절 좌표 프레임"]
        TrailR["EETrailRenderer\n거리게이팅+FIFO"]
        Arrow["DisplacementArrow\nEE 변위 화살표"]
    end

    subgraph OutputUI["UI 출력"]
        State["FairinoStatePanel\nEE XYZ RGB 색상"]
        Why["FairinoWhyItMovedLabel\n다관절 요약+XYZ 성분"]
        Conn["FairinoConnectionPanel"]
    end

    Slider --> OnSlider
    Handle --> OnHandle
    TcpInput --> OnTcp
    Preset --> OnPreset
    SyncBtn --> OnSync

    OnSlider --> Facade
    OnHandle --> Facade
    OnPreset --> Facade

    Facade --> DH
    DH --> Driver
    DH --> Gizmo
    DH --> TrailR
    DH --> Arrow
    DH --> State
    DH --> Why

    OnSync -->|Live| Conn
    OnTcp -->|MoveL| Conn
```

## 3. Runtime Data Flow (Main/Sandbox)

```mermaid
flowchart LR
    Input["Slider / DH / JointInputRail"] --> App["AppController"]
    App --> Step["StepFlowService"]
    App --> Runtime["KinematicsRuntimeService"]
    App --> Binder["AppUiBinder"]

    Runtime --> FK["DHStandard + ForwardKinematics"]
    Runtime --> Snap["CapturePreviousState\nsnapshot / update cause"]
    FK --> State["CurrentA1 / CurrentA2 / CurrentT02 / Pose"]
    State --> HUD["MatrixDisplay / DHTableEditor / StepTutorPanel"]
    State --> WhyMoved["WhyItMovedPanel\nWhyItMovedState / Formatter"]
    State --> Render["RobotRenderer"]
    Render --> Rig["RobotRigBinder"]
    Render --> DonorMap["ScaraDonorMapper"]
    Render --> Copy["DonorMeshCopier"]
    Render --> Probe["RobotVisibilityProbe"]
    Render --> TrailViz["EndEffectorTrail"]
    Render --> Highlight["JointHighlightRing / LinkHighlighter"]

    State --> Beginner["BeginnerLeftPanel\nCompareModePanelHelper\nTargetFeedbackPanel"]
```

## 4. Folder Responsibility Map

```mermaid
flowchart TD
    Scripts["Assets/Scripts"] --> App["App/\nscene flow · orchestration"]
    Scripts --> AppFairino["App/Fairino/\nFR5 연결·제어·FK facade"]
    Scripts --> UI["UI/\nHUD · tutorial · Fairino panels"]
    Scripts --> Viz["Visualization/\nUnity render binding"]
    Scripts --> VizShared["Visualization/Shared/\n공용 컴포넌트 (로봇 무관)"]
    Scripts --> Math["Math/\npure double math"]
    Scripts --> Types["Types/\nimmutable robotics types"]
    Scripts --> Kin["Kinematics/\nDH · FK algorithms"]
    Scripts --> Templates["Templates/\nrobot presets"]

    App --> UI
    App --> Viz
    App --> Templates
    App --> Kin
    AppFairino --> Kin
    UI -. no FK math .-> Kin
    Viz -. no tutorial state .-> UI
```

## 5. Visualization/Shared/ Component Tree

```mermaid
flowchart TD
    Shared["Visualization/Shared/"]
    Shared --> SLM["SharedLineMaterial\n공유 Material 캐시 + ConfigureLineRenderer"]
    Shared --> Coord["CoordConverter\n로보틱스↔Unity 좌표"]
    Shared --> Trail["EETrailRenderer\nEE 궤적 (거리게이팅·FIFO·그라데이션)"]
    Shared --> TrailAdp["EndEffectorTrail\nEETrailRenderer 어댑터"]
    Shared --> FG["FrameGizmo\n단일 좌표 프레임"]
    Shared --> FGF["FrameGizmoFactory\n6관절 기즈모 관리"]
    Shared --> Orbit["OrbitCameraController\n좌클릭회전·스크롤줌·우클릭팬"]
    Shared --> Arrow["DisplacementArrow\nEE 변위 벡터 화살표"]
    Shared --> Handle["JointRotationHandle\n관절 회전 링 핸들"]

    SLM -.->|material| Trail
    SLM -.->|material| Arrow
    SLM -.->|material| Handle
```

## 6. RobotControl UI Layout Tree

```mermaid
flowchart TB
    Canvas["Canvas (Screen Space Overlay)"]
    Canvas --> Shell["RobotControlShell"]

    Shell --> TopBar["TopBar\nTitle · Mode · GizmoToggle · ClearTrail · BackToLibrary"]
    Shell --> TabBar["TabBar\nJoint Control | TCP Control | State"]
    Shell --> ConnPanel["ConnectionPanel\nIP·Port·Connect·Enable"]

    Shell --> JointPanel["JointControlPanel\n6축 슬라이더 · MoveJ/ServoJ/Stop/Sync\nDryRun 토글 · 프리셋(Home/Ready/Folded/Current)"]
    Shell --> TcpPanel["TcpControlPanel\nX/Y/Z/Rx/Ry/Rz 입력 · MoveL/ServoCart\nDryRun 토글 · 현재 TCP 표시"]

    Shell --> StatePanel["StatePanel\n관절값+델타 · TCP XYZ(RGB) · 에러"]
    Shell --> WhyLabel["WhyItMovedLabel\n다관절 요약 + XYZ 성분"]
    Shell --> Dialog["MoveConfirmDialog\nLive 이동 확인 오버레이"]

    TabBar -->|Tab 0| JointPanel
    TabBar -->|Tab 1| TcpPanel
```

## 7. Scene Build Settings (index → scene)

| Index | Scene | 역할 |
|-------|-------|------|
| 0 | Boot | 라우터 전용 (첫 방문 판단) |
| 1 | Onboarding | 환영 모달, 초보자/기본 분기 |
| 2 | Home | 재진입 허브 (Continue Hub) |
| 3 | Main | Guided Lesson + 로봇/HUD |
| 4 | RobotLibrary | 로봇 카탈로그 + 3D showroom |
| 5 | Sandbox | 자유 실험 |
| 6 | RobotControl | FR5 실기 제어 콘솔 |
| 7 | MathReadiness | 수학 기초 워밍업 |

## 8. Stable Invariants
- `frame_0`, `frame_1`, and `Frame_EE` are the canonical frame ownership points.
- `ScaraRobot.prefab` is the donor source; visual donor path uses `Base`, `Axis1`, `Axis2`, and `Axis3/Gripper`.
- `Pick` is a helper point, not a visual donor.
- `AppController` is the public runtime state and event facade (Main/MathReadiness/Sandbox).
- `RobotControlSceneCoordinator` is the RobotControl scene facade (FR5 전용).
- `RobotRenderer` is the public visualization facade (2DOF/SCARA).
- `FairinoUrdfJointDriver` is the FR5 visualization driver (Transform-based).
- `Math`, `Types`, and `Kinematics` stay pure C# `double`-based domain code.
- Build Settings: `Boot`(0), `Onboarding`(1), `Home`(2), `Main`(3), `RobotLibrary`(4), `Sandbox`(5), `RobotControl`(6), `MathReadiness`(7).
- `KinematicsRuntimeState` holds previous/current snapshots and `RuntimeUpdateCause`.
- `RobotCatalog` (Templates) is the single registry for all robot metadata + template factories.
- `RobotSelectionBridge` (App) passes robot selection between scenes via PlayerPrefs.
- Scene cameras are managed by `SceneCameraDirector` (except RobotLibrary showroom).
- `IVisibilityControllable.SetVisible(bool)` is the standard panel visibility contract.
- `SharedLineMaterial` is the single LineRenderer material cache for all Visualization/Shared/ components.
- `IFairinoRobotClient` abstracts Mock/Live robot communication; `MoveJ`/`MoveL`/`ServoJ`/`StopMotion`.
