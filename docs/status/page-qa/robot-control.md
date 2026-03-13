# Robot Control QA Runbook

## Prep
- `KineTutor3D/Robots/Import FAIRINO FR5 URDF`를 먼저 실행한다.
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab`는 showroom preview용인지 확인한다.
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`는 RobotControl control용인지 확인한다.

## Entry Route
1. `Play`
2. `Robot Library`에서 `FAIRINO FR5` 카드 또는 detail drawer의 `Robot Control` CTA 클릭
3. `RobotControl` 씬 진입 확인

## Core Checks
- [ ] 기본 모드가 `Mock`로 시작한다.
- [ ] `ConnectionPanel`에서 Connect / Disconnect가 동작한다.
- [ ] `Enable` / `Disable` 상태가 즉시 반영된다.
- [ ] `JointControlPanel`에 6축 슬라이더가 모두 보인다.
- [ ] `MoveJ`, `ServoJ`, `Stop`, `DryRun`이 모두 응답한다.
- [ ] `StatePanel`에 관절값과 TCP pose가 갱신된다.
- [ ] 3D FR5가 우측 메인 뷰에 표시된다.

## 2026-03-13 Runtime Findings
- `RobotControl` PlayMode 직접 진입 확인:
  - `KineTutor3D/Always Start From Onboarding`가 켜져 있으면 Play가 항상 `Onboarding.unity`로 시작한다.
  - `RobotControl` 검증 시에는 이 토글을 잠시 끄고 씬 직접 Play가 필요하다.
- `RobotControlSceneCoordinator` 로그 기준 control prefab 로드는 성공했다.
  - `Loaded FR5 control prefab with 7 MeshFilter(s) and 7 MeshRenderer(s).`
- 초기 blank screen 원인 1차:
  - URDF control prefab의 `ArticulationBody.useGravity = true` 상태로 시작해 `base_link`가 큰 음수 Y로 낙하했다.
  - coordinator에서 모든 articulation gravity를 끄고 `base_link`를 immovable로 고정하도록 보강했다.
- 초기 blank screen 원인 2차:
  - URDF import가 붙이는 기본 `Unity.Robotics.UrdfImporter.Control.Controller`가 legacy `UnityEngine.Input`을 읽어 Input System 예외를 반복했다.
  - coordinator에서 해당 controller를 runtime에서 비활성화하도록 보강했다.
- 현재 남은 시각 이슈:
  - runtime clone에서 visual `MeshFilter.sharedMesh`는 다시 묶였지만, MCP 기준 `MeshRenderer.bounds`가 여전히 `size = 0`으로 읽힌다.
  - Game screenshot에서도 3D FR5가 즉시 보이지 않아, 현재 `RobotControl`은 “control prefab 로드 + 낙하 방지 + input 예외 제거”까지는 확인됐고, 최종 visible state는 추가 디버깅이 필요하다.

## Asset Path Rules
- showroom = `Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab`
- robot control = `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`
- donor preview 문제는 `unity-urdf-donor-preview-debug` 스킬 checklist를 따른다.
- RobotControl 문제와 showroom donor 문제를 섞지 않는다.

## Quick Inspect Targets
- scene: `RobotControl`
- objects: `RobotControlCoordinator`, `FR5_RuntimeRoot`, `FR5_UrdfInstance`, `Canvas`, `TopBar`, `ConnectionPanel`, `JointControlPanel`, `StatePanel`

## Reuse Assessment
- `RobotLibrary` / showroom 경로:
  - `RobotPreviewFactory`가 `Resources/Robots/FAIRINO_FR5.prefab`를 donor preview로 로드하고 `DonorMeshCopier.CopyMeshOnly(...)`로 mesh-only clone을 만든다.
  - 이 경로는 현재 FR5 showroom screenshot에서 이미 안정적으로 서 있는 것이 확인되었다.
- `Main` / `Sandbox` 경로:
  - `RobotRenderer`는 `ScaraDonorMapper`, `Base/Axis1/Axis2/Axis3/Gripper`, `frame_0/frame_1/Frame_EE`에 강하게 묶여 있어 FR5 6축 URDF control 경로를 직접 재사용하기 어렵다.
- `RobotControl` salvage 방향:
  - 시각 표시만 먼저 살리는 목적이라면 `FAIRINO_FR5.prefab` donor preview를 `RobotControl`의 visual twin fallback으로 재사용하는 경로가 가장 현실적이다.
  - 실제 joint state까지 3D에 반영하려면 `FairinoConnectionService.OnStateUpdated`를 받아 FR5 전용 visual adapter가 joint transforms 또는 articulation drives를 갱신하는 새 계층이 필요하다.
