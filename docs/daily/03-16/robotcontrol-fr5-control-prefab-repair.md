# 2026-03-16 — RobotControl FR5 Control Prefab Repair

## 증상
- `RobotControl`에서 slider, XYZ, preset 같은 UI 기능은 반응하지만 로봇 본체 mesh가 보이지 않았다.
- mesh를 showroom preview prefab으로 대신 보이게 하면 `Ready` / `Folded` 프리셋에서 관절이 분리되어 보였다.

## 원인 축소 과정
1. `unity-mcp-debug-playbook`, `unity-urdf-donor-preview-debug` 절차로 active scene / PlayMode / runtime hierarchy를 먼저 확인했다.
2. `RobotControl`은 `showroom preview`가 아니라 `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`를 써야 한다는 문서 계약을 재확인했다.
3. `FAIRINO_FR5_Control.prefab` YAML을 확인해 `MeshFilter.m_Mesh`와 `MeshCollider.m_Mesh`를 분리해서 봤다.
4. 이 환경에서는 raw URDF import 결과를 바로 control prefab으로 저장할 때 visual mesh leaf가 불안정하게 저장되는 흐름을 확인했다.
5. `showroom preview` fallback은 시각 확인엔 도움이 되지만, control hierarchy와 joint axis가 달라 preset 동작 검증에 부적합하다는 것을 확인했다.

## 실제 수정
- `Assets/Editor/KineTutor3D/QaToolsMenu.cs`
  - `ImportFairinoFr5Urdf()`에 control prefab repair 단계를 추가했다.
  - nested prefab instance를 complete unpack한다.
  - `Assets/Runtime/Robots/FAIRINO_FR5/meshes/*_0.asset` 기준으로 visual `MeshFilter`와 `MeshCollider`를 재바인딩한다.
  - control prefab 저장 직후 `sharedMesh + vertexCount` 검증을 수행한다.

## 재검증 결과
- `KineTutor3D/Robots/Import FAIRINO FR5 URDF` 재실행 후:
  - `FAIRINO_FR5.prefab` preview prefab 재생성 확인
  - `FAIRINO_FR5_Control.prefab` control prefab 재생성 확인
- editor scene `RobotControl` 기준:
  - `FR5_RuntimeRoot/FR5_UrdfInstance/.../Visuals/.../shoulder_link_0`
  - `FR5_RuntimeRoot/FR5_UrdfInstance/.../Visuals/.../wrist3_link_0`
  두 노드 모두 `MeshFilter.sharedMesh`가 실제 mesh asset을 가리키고, `MeshRenderer.bounds`가 0이 아니었다.
- PlayMode `RobotControl` 직접 진입 기준:
  - `FairinoUrdfJointDriver`가 다시 원래 `anchorRot` 기반 axis 로그를 출력했다.
  - 즉 showroom fallback이 아니라 원래 control hierarchy로 돌아왔다.

## 교훈
- `preview prefab`과 `control prefab`은 절대 같은 목적으로 취급하지 않는다.
- `MeshCollider.m_Mesh: {fileID: 0}`만 보고 control prefab 전체가 깨졌다고 결론내리지 않는다.
- 다른 PC로 이동해도 재발을 줄이려면 아래 둘을 같이 가져가야 한다.
  - 수정된 `QaToolsMenu.cs`
  - 재생성된 `FAIRINO_FR5.prefab`, `FAIRINO_FR5_Control.prefab`

## 다음 체크
- 다른 PC에서 pull 후 `KineTutor3D/Robots/Import FAIRINO FR5 URDF` 재실행
- `RobotControl` direct Play에서 `Ready` / `Folded` / slider preset이 정상인지 확인
- 문제가 재현되면 prefab YAML 차이와 visual leaf `sharedMesh` 상태를 먼저 비교
