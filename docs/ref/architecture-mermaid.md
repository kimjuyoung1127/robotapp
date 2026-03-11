# KineTutor3D Architecture Mermaid

This is the fastest whole-system context document for new sessions.
Read this after `AGENTS.md` and before drilling into individual runtime files.

## 1. System Overview

```mermaid
flowchart TD
    Boot["Boot.unity"] --> Router["BootSceneRouter"]
    Router -->|first visit| Onboarding["Onboarding.unity"]
    Router -->|return visit| Main["Main.unity"]

    Onboarding --> Nav["SceneNavigationBar"]
    Main --> Nav

    Main --> App["AppController"]
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

## 2. Runtime Data Flow

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

## 4. Stable Invariants
- `frame_0`, `frame_1`, and `Frame_EE` are the canonical frame ownership points.
- `ScaraRobot.prefab` is the donor source; visual donor path uses `Base`, `Axis1`, `Axis2`, and `Axis3/Gripper`.
- `Pick` is a helper point, not a visual donor.
- `AppController` is the public runtime state and event facade.
- `RobotRenderer` is the public visualization facade.
- `Math`, `Types`, and `Kinematics` stay pure C# `double`-based domain code.
- Build Settings: `Boot`(0), `Onboarding`(1), `Main`(2), `RobotLibrary`(3).
- `KinematicsRuntimeState` holds previous/current snapshots and `RuntimeUpdateCause`.
- `RobotCatalog` (Templates) is the single registry for all robot metadata + template factories.
- `RobotSelectionBridge` (App) passes robot selection between scenes via PlayerPrefs.
- Phase 5 complete: 76 source files, EditMode 107/107, PlayMode 30/30.
