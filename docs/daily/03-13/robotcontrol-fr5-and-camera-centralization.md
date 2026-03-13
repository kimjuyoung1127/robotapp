# 2026-03-13 — RobotControl FR5 + Camera Centralization

## 반영 내용
1. `RobotControl.unity`를 추가하고 Build Settings index `6`에 등록했다.
2. `RobotControlSceneCoordinator`가 FR5 전용 제어 콘솔 씬의 panel auto-wire, Mock 기본 시작, FR5 selection 고정, control prefab 복원을 담당하도록 보강했다.
3. FR5 자산 사용 기준을 분리했다.
   - showroom preview: `Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab`
   - robot control: `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`
4. `QaToolsMenu`의 FR5 import는 preview/control prefab을 각각 저장하도록 확장했다.
5. `RobotLibrary`와 detail drawer에서 FR5 전용 `Robot Control` CTA를 추가했다.
6. `SceneCameraDirector`를 추가해 `Main / Sandbox / RobotControl / Onboarding / Home` 메인 카메라 구도와 FOV를 중앙 관리하도록 정리했다.
7. `RobotLibraryManager` showroom camera와 gameplay scene camera를 분리된 정책으로 유지했다.

## 검증
- `RobotControl.unity` 씬 자산 생성 확인
- Build Settings에서 `RobotControl` buildIndex `6` 확인
- `SceneCameraDirectorTests` 통과
- FAIRINO 관련 EditMode 테스트 묶음 통과
- Unity compile 에러 없음
- `RobotControl` 직접 Play 확인:
  - `Always Start From Onboarding` 토글을 잠시 끄면 `RobotControl` 자체를 Play할 수 있다.
  - runtime에서 `FR5_UrdfInstance`가 생성되고 coordinator 로그에 `7 MeshFilter / 7 MeshRenderer`가 찍힌다.
- `RobotControl` runtime issue triage:
  - 초기에는 URDF articulation gravity로 인해 `base_link`가 큰 음수 Y로 낙하했다.
  - URDF 기본 `Controller`가 legacy `UnityEngine.Input`을 읽어 Input System 예외를 반복했다.
  - coordinator에서 gravity 제거, base immovable 고정, runtime controller 비활성화, `sharedMesh` 재바인딩 보강을 적용했다.
- 현재 잔존 이슈:
  - MCP 기준 visual renderer bounds가 여전히 `0`으로 읽히는 노드가 있어, control prefab은 로드되지만 Game screenshot에서 3D가 즉시 확인되지 않는다.
  - 즉 placeholder 단계는 벗어났지만, 최종 visible state는 추가 시각화 디버깅이 필요하다.

## 다른 페이지 로딩 경로 비교
- `RobotLibrary` / showroom:
  - `RobotPreviewFactory`가 `Resources/Robots/FAIRINO_FR5.prefab`를 donor preview로 로드한다.
  - `DonorMeshCopier.CopyMeshOnly(...)` 기반 mesh-only clone + preview pose 튜닝 경로라 현재 가장 안정적이다.
- `Main` / `Sandbox`:
  - `RobotRenderer`는 `ScaraDonorMapper`, `Base/Axis1/Axis2/Axis3/Gripper`, `frame_0/frame_1/Frame_EE`에 강하게 묶여 있어 FR5 6축 URDF control 경로를 그대로 재사용하기 어렵다.
- 결론:
  - `RobotControl`에서 3D를 먼저 살리는 목적이라면 showroom용 `FAIRINO_FR5.prefab` donor preview를 visual fallback/twin으로 재사용하는 방안은 유효하다.
  - 다만 서비스 상태를 3D joint motion으로 반영하는 계층은 아직 없으므로, FR5 전용 visual adapter를 별도로 만들어야 한다.

## 남은 확인
- PlayMode에서 `Robot Library -> FR5 -> Robot Control` 진입 후 `FR5_UrdfInstance` 시각 확인
- 카메라 줌 상태는 실제 Game view 기준으로 한 번 더 수동 QA 필요
- FR5 3D를 donor preview fallback으로 먼저 붙일지, URDF control prefab visible repair를 계속 밀지 결정 필요
