---
name: robot-urdf-import
description: "URDF 임포트 파이프라인 — 새 로봇 URDF 작성, DAE 메시 배치, Unity URDF Importer 실행, URP Material 교체, prefab 생성"
---

## Trigger
새 로봇의 3D 메시를 Unity에 임포트하여 prefab을 생성할 때.
DAE/STL 메시 파일이 이미 확보된 상태에서 URDF 작성 + prefab 조립이 필요한 경우.

## Input Context
- 로봇 이름/ID (예: "UR5e", "DOOSAN_M1013")
- 메시 파일 위치 (DAE/STL)
- 관절 구조 (URDF joint origin xyz/rpy, 링크 이름)
- 메시 스케일 (DAE=1.0, 일부 로봇 STL/DAE=0.001 밀리미터 단위)
- Visual mesh offset (메시 원점 보정 xyz/rpy)
- 라이선스 정보

## Read First
1. `Assets/Runtime/Robots/FAIRINO_FR5/fairino5_v6.urdf` — FR5 URDF 참조 패턴
2. `Assets/Runtime/Robots/UR5e/ur5e.urdf` — UR5e URDF (공식 config 기반)
3. `Assets/Runtime/Robots/DOOSAN_M1013/m1013.urdf` — Doosan URDF (plain, scale 0.001)
4. `Assets/Editor/KineTutor3D/QaToolsMenu.cs` — ImportGenericRobotUrdf 헬퍼 + ReplaceWithUrpLitMaterials
5. `docs/ref/code-patterns.md` — C# 코딩 패턴

## Do
1. **메시 소스 확보**: ROS2 description 패키지에서 visual DAE 메시 다운로드 (git sparse-checkout 활용)
2. **라이선스 복사**: `LICENSE-BSD3.txt` 등을 메시 폴더에 포함
3. **폴더 구조 생성**:
   - `Assets/Runtime/Resources/Robots/{RobotId}/` — Resources용 메시 (카탈로그 참조)
   - `Assets/Runtime/Robots/{ROBOT_ID}/meshes/` — URDF Importer용 메시 복사본
   - `Assets/Runtime/Robots/{ROBOT_ID}/{robot}.urdf` — URDF 파일
4. **URDF 작성** (plain XML, xacro 금지):
   - 공식 kinematics config (yaml)에서 joint origin xyz/rpy 추출
   - 공식 visual_parameters config에서 mesh offset xyz/rpy 추출
   - joint origin rpy는 **roll/pitch/yaw 순서** 주의 (pitch가 아닌 roll에 pi/2인 경우 다수)
   - 메시 경로는 `meshes/{filename}.dae` 상대 경로
   - Doosan 등 밀리미터 메시는 `scale="0.001 0.001 0.001"` 필수
5. **QaToolsMenu에 임포트 메뉴 추가**:
   - `[MenuItem("KineTutor3D/Robots/Import {Robot} URDF", priority = 14X)]`
   - `ImportGenericRobotUrdf()` 헬퍼 호출
   - control prefab: `Assets/Runtime/Resources/Robots/{RobotId}/{RobotId}_Control.prefab`
   - preview prefab: `Assets/Runtime/Resources/Robots/{RobotId}/{RobotId}.prefab`
6. **임포트 실행**: `UnityEditor.EditorApplication.ExecuteMenuItem(...)` via unityctl exec
7. **URP Material 자동 교체**: `ReplaceWithUrpLitMaterials()`가 Standard shader → URP Lit 에셋으로 변환

## Do Not
- xacro 사용 (Unity URDF Importer 미지원)
- joint origin rpy를 추측하지 말고 공식 config/URDF에서 추출
- 메시를 한 곳에만 두지 말 것 (Resources + Robots/meshes 양쪽 필요)
- FR5 임포트 코드 수정

## Validation
- `uc.sh check` 컴파일 통과
- prefab 파일 2개 생성 확인 (Control + Preview)
- prefab 내 material GUID가 `Materials_URP/` 폴더의 .mat 에셋을 참조
- Unity에서 RobotLibrary 진입 시 분홍색 아닌 정상 렌더링

## Output Template
```
✅ {RobotId} URDF 임포트 완료
- URDF: Assets/Runtime/Robots/{ROBOT_ID}/{robot}.urdf
- Control prefab: Assets/Runtime/Resources/Robots/{RobotId}/{RobotId}_Control.prefab
- Preview prefab: Assets/Runtime/Resources/Robots/{RobotId}/{RobotId}.prefab
- URP Materials: Assets/Runtime/Resources/Robots/{RobotId}/Materials_URP/
- 메시: {N}개 DAE (BSD-3)
```
