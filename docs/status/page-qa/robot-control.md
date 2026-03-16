# Robot Control QA Runbook

## Prep
- `KineTutor3D/Robots/Import FAIRINO FR5 URDF`를 먼저 실행한다.
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab`는 showroom preview용인지 확인한다.
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`는 RobotControl control용인지 확인한다.
- `RobotControl` 직접 검증이 필요하면 `KineTutor3D/Always Start From Onboarding`를 잠시 끈다.
- import 직후에는 `FAIRINO_FR5_Control.prefab` YAML에서 `MeshFilter`의 `m_Mesh`가 실제 mesh asset GUID를 가리키는지 확인한다.
- `MeshCollider.m_Mesh: {fileID: 0}`는 그대로 남을 수 있으므로, 이 값만 보고 control prefab 전체가 깨졌다고 단정하지 않는다.

## Entry Route
1. `Play`
2. `Robot Library`에서 `FAIRINO FR5` 카드 또는 detail drawer의 `Robot Control` CTA 클릭
3. `RobotControl` 씬 진입 확인

## Core Checks
- [ ] 기본 모드가 `Mock`로 시작한다.
- [ ] `ConnectionPanel`에서 Connect / Disconnect가 동작한다.
- [ ] `Details` 버튼으로 `Diagnostics Drawer`가 열리고 닫힌다.
- [ ] `Enable` / `Disable` 상태가 즉시 반영된다.
- [ ] `JointControlPanel`에 6축 슬라이더가 모두 보인다.
- [ ] 좌측 `ControlSummaryCard`에 현재 포즈/속도/DryRun 요약이 표시된다.
- [ ] `MoveJ`, `ServoJ`, `Stop`, `DryRun`이 모두 응답한다.
- [ ] `StatePanel`에 관절값과 TCP pose가 갱신된다.
- [ ] `Why It Moved`가 compact card로 읽기 가능하다.
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

## 2026-03-16 Follow-up Findings
- root cause는 `RobotControl` 로직보다 `FAIRINO_FR5_Control.prefab` 생성 파이프라인이었다.
- 단순 `showroom preview` fallback은 로봇을 보이게는 하지만 control hierarchy와 joint axis 계약이 달라 `Ready`/`Folded` 프리셋에서 관절 분리처럼 보이는 부작용을 만든다.
- `QaToolsMenu.ImportFairinoFr5Urdf()`에서 raw URDF hierarchy를 바로 control prefab으로 저장하면, 일부 환경에서 visual mesh leaf가 불안정하게 저장될 수 있었다.
- `QaToolsMenu`를 보강해 아래 순서로 control prefab을 재생성하도록 수정했다.
  1. STL mesh asset 전처리
  2. URDF import
  3. control hierarchy 안의 nested prefab instance 완전 unpack
  4. visual `MeshFilter` / `MeshCollider`에 `Assets/Runtime/Robots/FAIRINO_FR5/meshes/*_0.asset` 재바인딩
  5. control prefab 저장 직후 `sharedMesh + vertexCount` 검증
- 최신 재검증 결과:
  - editor scene의 `FR5_RuntimeRoot/FR5_UrdfInstance/.../Visuals/.../*_0` 노드들은 `MeshFilter.sharedMesh`가 실제 mesh asset을 가리킨다.
  - PlayMode `RobotControl` 직접 진입 시 `FairinoUrdfJointDriver`가 다시 원래 `anchorRot` 기반 joint axis 로그를 출력한다.
  - runtime visual leaf `shoulder_link_0`, `wrist3_link_0`의 `MeshRenderer.bounds`는 0이 아니다.
- 따라서 현재 기준 권장 복구 순서는 `fallback 코드 추가`가 아니라 `KineTutor3D/Robots/Import FAIRINO FR5 URDF` 재생성 + control prefab 검증이다.

## 2026-03-16 Self Review / Scene-Authored Pilot Findings
- `RobotControl`은 이제 `scene-authored UI` 파일럿 상태다.
  - `KineTutor3D/RobotControl/Author Scene UI` 메뉴가 `Canvas/RobotControlShell/*` 구조를 `Assets/Scenes/RobotControl.unity`에 저장한다.
  - `RobotControlSceneCoordinator`는 `FairinoRobotControlViewBuilder.TryBindExistingLayout(...)` 경로로 씬 authored UI를 우선 바인딩한다.
- 따라서 `BtnDiagnostics`, `DrawerPanel` 같은 레이아웃 위치는 더 이상 `BuildTopBar()` 또는 `EnsureLayout()` 숫자만 바꿔서는 바로 바뀌지 않는다.
  - 실제 source of truth는 `Assets/Scenes/RobotControl.unity`에 저장된 authored `RectTransform` 값이다.
  - 예: `BtnDiagnostics`는 scene YAML 기준 `m_AnchoredPosition: {x: -420, y: 0}`이고, runtime에서도 같은 값으로 올라왔다.
  - 예: `DrawerPanel`은 scene YAML 기준 닫힘 위치 `m_AnchoredPosition: {x: 380, y: -86.7}`로 저장돼 있다.
- 즉 UI 위치가 안 바뀌어 보일 때는 먼저 코드보다 `RobotControl.unity`의 authored 좌표를 확인해야 한다.
- 현재 상태는 "완전 scene-authored"라기보다 "scene-authored shell + code-bound refresh"에 가깝다.
  - 각 패널 `EnsurePresentation()`은 여전히 내부 위젯 anchor/size를 재적용한다.
  - 그래서 shell 배치는 씬에서 보고, 패널 내부 위젯은 런타임 재정렬 가능성을 함께 고려해야 한다.

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
  - 시각 표시만 먼저 살리는 목적이라도 `FAIRINO_FR5.prefab` donor preview를 기본 control 경로 대체로 고정하지 않는다.
  - preview fallback은 “화면 확인용 임시 triage”까지만 허용하고, 프리셋/조인트 제어 검증에는 사용하지 않는다.
  - 실제 joint state까지 3D에 반영하려면 `FairinoConnectionService.OnStateUpdated`를 받아 FR5 전용 visual adapter가 joint transforms 또는 articulation drives를 갱신하는 새 계층이 필요하다.

## Fast Triage Loop
1. `RobotControl` direct Play가 필요하면 `Always Start From Onboarding` 토글을 끈다.
2. `KineTutor3D/Robots/Import FAIRINO FR5 URDF`를 실행한다.
3. `FAIRINO_FR5_Control.prefab`에서 `MeshFilter.m_Mesh`가 7개 모두 실제 mesh asset GUID를 가리키는지 확인한다.
4. editor scene 또는 PlayMode에서 아래 visual leaf를 확인한다.
   - `FR5_RuntimeRoot/FR5_UrdfInstance/base_link/shoulder_link/Visuals/unnamed/shoulder_link/shoulder_link_0`
   - `FR5_RuntimeRoot/FR5_UrdfInstance/.../wrist3_link/Visuals/unnamed/wrist3_link/wrist3_link_0`
5. `sharedMesh`와 `MeshRenderer.bounds`가 정상인지 확인한다.
6. 그다음에만 `Ready` / `Folded` / slider / TCP control을 검증한다.
7. `Diagnostics Drawer`의 연결 상태 / FW / SDK / 최근 오류 / 재시도 힌트가 실제 상태와 일치하는지 확인한다.
8. `scene-authored UI`가 활성화된 이후에는 `BtnDiagnostics` / `DrawerPanel` 위치를 바꿀 때 먼저 `RobotControl.unity`의 `RectTransform` 값을 수정하고, 그다음에만 builder fallback 숫자를 맞춘다.
