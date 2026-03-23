# INSTALL

## 필수 패키지

- Unity 6 (`6000.0.64f1` 기준 검증)
- URP
- UGUI
- Input System
- Unity Robotics URDF Importer

## 설치 순서

1. 대상 Unity 프로젝트를 닫거나 백업합니다.
2. `FAIRINO_FR5_TEMPLATE_Slim.unitypackage`를 import하거나 `Assets/`를 그대로 복사합니다.
3. `Packages/manifest.json`에서 아래 계열이 준비되어 있는지 확인합니다.
   - `com.unity.render-pipelines.universal`
   - `com.unity.inputsystem`
   - `com.unity.ugui`
   - `com.unity.robotics.urdf-importer`
4. Unity를 열고 script compile이 끝날 때까지 기다립니다.
5. `Assets/Scenes/FR5_Template_Demo.unity`를 엽니다.
6. Play 후 FR5 control prefab과 링 핸들이 보이는지 확인합니다.

## 경로 유지 규칙

- `Resources/Robots/FAIRINO_FR5.prefab`
- `Resources/Robots/FAIRINO_FR5_Control.prefab`
- `Resources/Robots/FAIRINO_FR5_Preview.mat`
- `Assets/Runtime/Robots/FAIRINO_FR5/`

위 경로는 바꾸지 않는 것을 권장합니다.

## Import 후 첫 점검

- Console compile error 0
- `FAIRINO_FR5_Control` 로드 성공
- 링 6개 표시
- 드래그 시 3D 포즈 반영
