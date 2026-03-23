# DEPENDENCIES

## Unity 패키지

- `com.unity.render-pipelines.universal`
- `com.unity.inputsystem`
- `com.unity.ugui`
- `com.unity.robotics.urdf-importer`

## 포함 스크립트 계층

- `Assets/Scripts/KineTutor3D.Runtime.asmdef`
- `Assets/Scripts/Math/*`
- `Assets/Scripts/Kinematics/*`
- `Assets/Scripts/Types/*`
- `Assets/Scripts/Templates/TemplateFAIRINO_FR5.cs`
- `Assets/Scripts/App/RobotKinematicsFacade.cs`
- `Assets/Scripts/App/FR5TemplateMinimalController.cs`
- `Assets/Scripts/App/FR5TemplatePoseCatalog.cs`
- `Assets/Scripts/App/FR5TemplateSlimManifest.cs`
- `Assets/Scripts/Visualization/FairinoUrdfJointDriver.cs`
- `Assets/Scripts/Visualization/Shared/JointRotationHandle.cs`
- `Assets/Scripts/Visualization/Shared/OrbitCameraController.cs`
- `Assets/Scripts/Visualization/Shared/SharedLineMaterial.cs`

## 비포함 항목

- `Assets/Scripts/UI/*`
- `Assets/Scripts/App/Fairino/RobotControlSceneCoordinator.cs`
- live Fairino SDK DLL
- teaching / playback / diagnostics

## 주의

- `JointRotationHandle`는 Input System을 사용합니다.
- `FairinoUrdfJointDriver`는 URDF prefab의 `base_link`와 `ArticulationBody` 구조를 전제로 합니다.
