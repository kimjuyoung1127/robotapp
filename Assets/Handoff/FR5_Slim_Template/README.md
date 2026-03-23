# FR5 Slim Template

이 번들은 `FAIRINO_FR5_TEMPLATE` 개념을 다른 저장소에서 바로 재사용할 수 있도록 정리한 **슬림 FR5 템플릿**입니다.

## 포함 범위

- `FR5_Template_Demo.unity`
- `FR5TemplateMinimalController`
- `TemplateFAIRINO_FR5`
- `RobotKinematicsFacade`
- `FairinoUrdfJointDriver`
- `JointRotationHandle`
- `SharedLineMaterial`
- `FAIRINO_FR5.prefab`
- `FAIRINO_FR5_Control.prefab`
- `FAIRINO_FR5_Preview.mat`
- `Assets/Runtime/Robots/FAIRINO_FR5/` 전체

## 제외 범위

- full `RobotControlSceneCoordinator`
- 광범위한 `Assets/Scripts/UI`
- teaching / playback / diagnostics
- live SDK / DLL
- Onboarding / RobotLibrary / glossary / 일반 앱 흐름

## 템플릿 의미

- `FAIRINO_FR5_TEMPLATE`는 **select-only template entry** 개념입니다.
- 실제 preview source는 `FAIRINO_FR5` prefab입니다.
- 실제 3D control asset은 `FAIRINO_FR5_Control.prefab`입니다.

## 빠른 시작

1. `.unitypackage`를 import하거나 이 폴더의 `Assets/`를 대상 저장소에 복사합니다.
2. `INSTALL.md` 순서대로 패키지를 맞춥니다.
3. `Assets/Scenes/FR5_Template_Demo.unity`를 열어 Play 합니다.
4. 링을 드래그해 관절 각도와 3D 포즈가 함께 바뀌는지 확인합니다.

## 증거 산출물

- 대표샷: `evidence/fr5-template-ready.png`
- 포즈 비교샷: `evidence/fr5-template-neutral.png`, `evidence/fr5-template-showcase.png`
- 프레임 시퀀스: `evidence/sequence-frame-00-neutral.png` ~ `evidence/sequence-frame-03-wristturn.png`
