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

    App --> UI["HUD UI\nDHTableEditor / MatrixDisplay / StepNavigator"]
    App --> Viz["RobotRenderer"]
    Viz --> Frame["FrameGizmo"]
    Viz --> Donor["Scara donor visuals\nBase / Axis1 / Axis2 / Gripper"]
```

## 2. Runtime Data Flow

```mermaid
flowchart LR
    Input["Slider / DH input"] --> App["AppController"]
    App --> Step["StepFlowService"]
    App --> Runtime["KinematicsRuntimeService"]
    App --> Binder["AppUiBinder"]

    Runtime --> FK["DHStandard + ForwardKinematics"]
    FK --> State["CurrentA1 / CurrentA2 / CurrentT02 / Pose"]
    State --> HUD["MatrixDisplay / DHTableEditor / StepTutorPanel"]
    State --> Render["RobotRenderer"]
    Render --> Rig["RobotRigBinder"]
    Render --> DonorMap["ScaraDonorMapper"]
    Render --> Copy["DonorMeshCopier"]
    Render --> Probe["RobotVisibilityProbe"]
```

## 3. Folder Responsibility Map

```mermaid
flowchart TD
    Scripts["Assets/Scripts"] --> App["App\nscene flow / app state / orchestration"]
    Scripts --> UI["UI\nHUD / tutorial interaction / onboarding"]
    Scripts --> Viz["Visualization\nUnity render binding / donor visuals"]
    Scripts --> Math["Math\npure double math"]
    Scripts --> Types["Types\nimmutable robotics types"]
    Scripts --> Kin["Kinematics\nDH / FK algorithms"]
    Scripts --> Templates["Templates\nrobot presets"]

    App --> UI
    App --> Viz
    App --> Templates
    App --> Kin
    UI -. no FK math .-> Kin
    Viz -. no tutorial state .-> UI
```

## 4. Scene Build Settings (index → scene)

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

## 5. Stable Invariants
- `frame_0`, `frame_1`, and `Frame_EE` are the canonical frame ownership points.
- `ScaraRobot.prefab` is the donor source; visual donor path uses `Base`, `Axis1`, `Axis2`, and `Axis3/Gripper`.
- `Pick` is a helper point, not a visual donor.
- `AppController` is the public runtime state and event facade.
- `RobotRenderer` is the public visualization facade.
- `Math`, `Types`, and `Kinematics` stay pure C# `double`-based domain code.
- Scene cameras are managed by `SceneCameraDirector` (except RobotLibrary showroom).
- `IVisibilityControllable.SetVisible(bool)` is the standard panel visibility contract.
