# KineTutor3D Project Flow (Code-Reviewed)

Last Reviewed: 2026-03-31 (KST)

This document is a code-derived flow snapshot for quick project orientation.
It was reviewed against the current runtime entry points instead of older planning-only diagrams.

Primary source files:
- `Assets/Scripts/App/BootSceneRouter.cs`
- `Assets/Scripts/App/SceneCatalog.cs`
- `Assets/Scripts/App/SceneNavigator.cs`
- `Assets/Scripts/App/RobotSelectionBridge.cs`
- `Assets/Scripts/App/AppController.cs`
- `Assets/Scripts/App/SandboxSceneCoordinator.cs`
- `Assets/Scripts/App/Fairino/RobotControlSceneCoordinator.cs`
- `Assets/Scripts/UI/Onboarding/OnboardingManager.cs`
- `Assets/Scripts/UI/RobotLibrary/RobotLibraryManager.cs`

## 1. Scene Entry Flow

```mermaid
flowchart TD
    Boot["Boot.unity\nBootSceneRouter"] --> Visit{"StepProgressSaver.HasVisited()?"}
    Visit -->|No| Onboarding["Onboarding.unity\nOnboardingManager"]
    Visit -->|Yes| Library["RobotLibrary.unity\nRobotLibraryManager"]

    Onboarding -->|Start Learning| Library
    Onboarding -->|Begin as Beginner| MathReady["MathReadiness.unity\nAppController"]
    Onboarding -->|Skip to Sandbox| Sandbox["Sandbox.unity\nSandboxSceneCoordinator"]

    Library -->|Guided lesson capable robot| Sandbox
    Library -->|Sandbox| Sandbox
    Library -->|Robot Control| RobotControl["RobotControl.unity\nRobotControlSceneCoordinator"]
    Library -->|Back| Onboarding

    MathReady -->|track complete| Library
    MathReady -->|Open Sandbox| Sandbox
```

## 2. Selection And Mode Bridge

```mermaid
flowchart LR
    UI["Onboarding / RobotLibrary / TemplateSelector / MathReadiness"] --> Bridge["RobotSelectionBridge\nPlayerPrefs"]
    Bridge --> RobotId["selected robotId"]
    Bridge --> Mode["selected mode\n guided_lesson / sandbox / robot_control"]

    RobotId --> Sandbox["SandboxSceneCoordinator"]
    RobotId --> RobotControl["RobotControlSceneCoordinator"]
    RobotId --> App["AppController\ninitial template load"]
    Mode --> Sandbox
    Mode --> RobotControl
```

## 3. Guided Lesson Runtime Flow

```mermaid
flowchart TD
    Input["Joint sliders / DH edits / step UI"] --> App["AppController"]
    App --> Step["StepFlowService"]
    App --> Runtime["KinematicsRuntimeService"]
    App --> Binder["AppUiBinder"]

    Runtime --> FK["ForwardKinematics / DHStandard"]
    FK --> State["A1 / A2 / T02 / End Effector Pose"]

    State --> UI["DHTableEditor / MatrixDisplay / StepTutorPanel / BeginnerLeftPanel / MathReadinessPanel"]
    State --> Viz["RobotRenderer / EndEffectorTrail / TargetMarkerVisual / MathVisualOrchestrator / FKDiagramPanel"]

    App --> Progress["StepProgressSaver / SessionContext"]
    Progress --> Resume["resume step / current track"]
```

## 4. Sandbox Runtime Flow

```mermaid
flowchart TD
    Enter["SandboxSceneCoordinator.Awake"] --> Select["RobotSelectionBridge -> robotId"]
    Select --> Factory["RobotControlFactory\ncreate template definition"]
    Factory --> Prefab["Load control prefab"]
    Factory --> Kin["RobotKinematicsFacade"]

    Prefab --> Driver["UrdfJointDriver"]
    Kin --> Driver
    Kin --> Gizmo["FrameGizmoFactory"]
    Kin --> Trail["EETrailRenderer"]

    UI["SandboxViewBuilder\nsliders / presets / info / nav"] --> Coord["SandboxSceneCoordinator"]
    Coord --> Kin
    Coord --> Driver
    Coord --> Gizmo
    Coord --> Trail
    Coord --> Orbit["OrbitCameraController"]

    Coord -->|Back / Change Robot| Library["RobotLibrary.unity"]
```

## 5. RobotControl Runtime Flow

```mermaid
flowchart TD
    Enter["RobotControlSceneCoordinator.Awake"] --> Select["RobotSelectionBridge -> robotId"]
    Select --> Factory["RobotControlFactory\nrobot-specific template"]
    Factory --> Conn["FairinoConnectionService"]
    Factory --> Kin["RobotKinematicsFacade"]
    Factory --> Prefab["Load control prefab"]

    Prefab --> Driver["FairinoUrdfJointDriver"]
    Kin --> Driver
    Kin --> Gizmo["FrameGizmoFactory"]
    Kin --> Trail["EETrailRenderer"]
    Kin --> Arrow["DisplacementArrow"]

    Panels["Connection / Joint / TCP / State / Diagnostics / WhyItMoved"] --> Coord["RobotControlSceneCoordinator"]
    Coord --> Conn
    Coord --> Kin
    Coord --> Driver
    Coord --> Gizmo
    Coord --> Trail
    Coord --> Arrow
    Coord --> Handles["JointRotationHandle[]"]
    Coord --> Teach["WaypointCycleRunner / PresetTransitionAnimator"]
```

## 6. Notes

- The best existing whole-system diagram is still `docs/ref/architecture-mermaid.md`.
- `docs/ref/architecture-diagrams.md` contains older naming such as `Main.unity`, so treat it as historical unless it is refreshed.
- The actual navigable scenes in code are `MathReadiness`, `RobotLibrary`, `Sandbox`, and `RobotControl`; `Boot` and `Onboarding` are not shown in the regular navigation list.
