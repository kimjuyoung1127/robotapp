# KineTutor3D 아키텍처

## 모듈 의존성 다이어그램

```
┌─────────────────────────────────────────────────────────┐
│                    Assets/Scripts/                        │
│                                                          │
│  ┌──────────┐   ┌──────────┐   ┌───────────────┐       │
│  │  Types/   │◄──│  Math/   │◄──│  Kinematics/  │       │
│  │           │   │          │   │               │       │
│  │ JointType │   │ Vec3D    │   │ DHStandard    │       │
│  │ DHLink    │   │ Mat3D    │   │ ForwardKin    │       │
│  │ Robot     │   │ Mat4D    │   │               │       │
│  │ Template  │   │          │   │               │       │
│  │ Pose      │   │          │   │               │       │
│  └──────────┘   └──────────┘   └───────┬───────┘       │
│       pure C# / double only             │               │
│  ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─│─ ─ ─ ─ ─ ─   │
│       UnityEngine 허용 경계             │               │
│                                         ▼               │
│  ┌──────────────┐   ┌──────────┐   ┌──────────┐       │
│  │  Templates/   │   │   UI/    │   │  Visual  │       │
│  │               │   │          │   │ ization/ │       │
│  │ Template2DOF  │   │ DHTable  │   │ FrameGiz │       │
│  │ Template3DOF  │   │ Slider   │   │ VectorArr│       │
│  │ Template6DOF  │   │ StepTutor│   │ StepAnim │       │
│  └───────┬───────┘   └────┬─────┘   └────┬─────┘       │
│          │                │              │               │
│          └────────┬───────┘──────────────┘               │
│                   ▼                                      │
│            ┌──────────┐                                  │
│            │   App/    │                                  │
│            │           │                                  │
│            │ AppController                               │
│            └──────────┘                                  │
└─────────────────────────────────────────────────────────┘
```

## 데이터 흐름

```
사용자 입력 (슬라이더/DH 테이블)
    │
    ▼
AppController.OnParameterChanged()
    │
    ├─→ RobotTemplate.GetDHLinks()
    │       │
    │       ▼
    │   ForwardKinematics.Compute(links)
    │       │
    │       ├─→ DHStandard.ComputeA(link_i)  ──→  Mat4D
    │       │
    │       ▼
    │   T = A₁ · A₂ · ... · Aₖ  ──→  Mat4D (누적)
    │       │
    │       ▼
    │   Pose (position: Vec3D, rotation: Mat3D)
    │
    ├─→ UI 업데이트 (DHTable, StepTutor 행렬 표시)
    │
    └─→ Visualization 업데이트 (FrameGizmo, VectorArrow)
            │
            └─→ double → float 변환 (렌더링 경계)
```

## Assembly Definition 구조 (예정)

```
KineTutor3D.Types.asmdef        → Types/
KineTutor3D.Math.asmdef         → Math/ (참조: Types)
KineTutor3D.Kinematics.asmdef   → Kinematics/ (참조: Types, Math)
KineTutor3D.Templates.asmdef    → Templates/ (참조: Types, Math, Kinematics)
KineTutor3D.UI.asmdef           → UI/ (참조: Types, Templates, UnityEngine.UI)
KineTutor3D.Visualization.asmdef→ Visualization/ (참조: Types, Math, UnityEngine)
KineTutor3D.App.asmdef          → App/ (참조: 전체)
KineTutor3D.Tests.EditMode.asmdef → Tests/EditMode/ (참조: Types, Math, Kinematics)
KineTutor3D.Tests.PlayMode.asmdef → Tests/PlayMode/ (참조: 전체)
```
