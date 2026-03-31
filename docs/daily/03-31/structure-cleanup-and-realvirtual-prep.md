# 03-31 Structure Cleanup And realvirtual Prep

## 작업 요약
- `Assets/Scripts/App`, `Assets/Scripts/UI`, `Assets/Scripts/Visualization`를 역할/페이지 기준 하위 폴더로 재배치했다.
- 각 폴더의 `AGENTS.md` / `CLAUDE.md`와 루트 `AGENTS.md`를 새 탐색 구조 기준으로 갱신했다.
- `RobotCatalog`, `ScaraDonorStructureTests`, `RobotPreviewPod`에서 `realvirtual` 직접 참조를 줄였다.
- `asset-registry`, `asset-validation-report`, `PROJECT-STATUS`를 기준으로 `Assets/realvirtual`는 레거시 vendor source, `Assets/Runtime/Resources/Robots`는 현재 donor/runtime 경로로 문서화했다.

## 도메인 리로드 조사
- `realvirtual` editor bootstrap를 조사한 결과 `Assets/realvirtual/private/Editor/OnLoad.cs`가 `PackageManager.Client.List(true)`를 동기 대기하는 구조라 리로드 정체 후보로 판단했다.
- 해당 경로를 비동기 polling 방식으로 바꿔 메인 스레드 블로킹 가능성을 줄였다.
- 이후 로그를 더 좁혀본 결과, 현재 병목은 `ProcessInitializeOnLoadAttributes` 구간이며 `Assets/realvirtual/private/Editor/QuickEditToolbarIMGUI.cs`가 가장 유력한 후보로 남았다.
- `QuickEditToolbarIMGUI`는 `delayCall`, `EditorApplication.update`, `hierarchyChanged`, `afterAssemblyReload`를 동시에 사용하므로 도메인 리로드 직후 재초기화 루프를 만들 가능성이 높다.
- 원인 분리를 위해 QuickEdit bootstrap은 기본 비활성화 상태로 바꿨다 (`KineTutor3D.EnableRealvirtualQuickEditBootstrap=false` 기본값).

## 제거 판단
- 현재는 `realvirtual` 전체 삭제 단계가 아님.
- 이유:
  - 당시 기준으로는 runtime prefab sanitation과 settings 분리가 아직 끝나지 않았음
  - 현재는 두 단계 모두 완료되어 최종 삭제 rehearsal만 남은 상태임

## 현재 안전 상태
- compile green
- `ScaraDonorStructureTests` green
- 구조 정리와 문서 정합성 반영 완료

## 다음 작업
1. 런타임 로봇 prefab(`ScaraRobot`, `FanucCRX-10iA_L`, `igusRebel`)의 vendor component 흔적 제거 가능성 검증
2. `realvirtual` 제거 전 smoke checklist 정의
3. 필요 시 editor-side sanitation 툴을 별도 브랜치/세션에서 재시도

## 삭제 플랜
- 상태 문서 `docs/status/REALVIRTUAL-REMOVAL-PLAN.md`를 추가했다.
- 현재 결론은 `즉시 삭제 금지`, `runtime prefab sanitation 이후 제거 리허설`, `최종 삭제` 순서다.

## Runtime prefab sanitation
- `QaToolsMenu.SanitizeRuntimeRobotPrefabs()`를 실행해 runtime robot prefab의 `realvirtual` component를 제거했다.
- 결과:
  - `ScaraRobot.prefab`
  - `FanucCRX-10iA_L.prefab`
  - `igusRebel.prefab`
  에서 총 38개 vendor component 제거
- 검증:
  - `realvirtualController: {fileID: 0}` 및 대표 `realvirtual` script guid가 runtime robot prefab 검색에서 더 이상 나오지 않음
  - `ScaraDonorStructureTests` 통과
  - `RobotPreviewPodTests` 통과
  - compile green 유지

## Settings audit
- 당시 audit 기준으로는 `GraphicsSettings.asset`, `QualitySettings.asset`가 아직 `Assets/realvirtual/RenderPipelines/Resources/URP/URP-Default.asset` GUID를 직접 사용했다.
- 해당 `URP-Default.asset`가 참조하는 renderer asset 안에도 `realvirtual` 전용 renderer feature/script GUID가 남아 있었다.
- 이 단계의 결론은 **URP pipeline asset 세트 분리 필요**였고, 아래 `URP pipeline separation` 작업에서 실제로 해소했다.

## URP pipeline separation
- `Assets/Runtime/RenderPipelines/URP/` 아래에 project-owned URP asset 세트를 복제했다.
  - `KineTutor3D-URP.asset`
  - `Settings/KineTutor3D-URP-Default-Renderer.asset`
  - `Settings/KineTutor3D-URP-Thumbnail-Renderer.asset`
- copied renderer asset에서 `realvirtual` 전용 highlight / outline renderer feature block을 제거했다.
- `GraphicsSettings.asset`와 모든 `QualitySettings` tier를 새 URP asset GUID로 repoint했다.
- 검증:
  - compile green 유지
  - `Assets/Runtime` + `ProjectSettings` 범위 검색 기준 old realvirtual URP GUID / renderer GUID / renderer feature GUID 미검출
- 결과:
  - `realvirtual` 최종 제거의 핵심 blocker였던 **URP settings 직접 의존**은 해소된 상태다.
  - 남은 일은 `Assets/Gizmos/realvirtual` 검토와 실제 삭제 rehearsal이다.

## Deletion rehearsal
- backup copy를 만든 뒤 `unityctl asset delete`로 아래 두 경로를 실제 삭제처럼 제거했다.
  - `Assets/realvirtual`
  - `Assets/Gizmos/realvirtual`
- 삭제 직후 검증:
  - `asset get-info` 기준 두 경로 모두 `not found`
  - compile check pass
  - compile assembly 목록에서 `realvirtual.base` 미포함
- 이후 backup copy로 원복했고, Unity 상태는 다시 `Ready`로 복귀했다.
- 해석:
  - compile 기준으로는 `Assets/realvirtual` 최종 삭제 가능성이 높다.
  - 남은 확인은 핵심 scene smoke와 문서/레거시 문자열 정리다.

## Final removal
- 실제로 아래 두 경로를 최종 삭제했다.
  - `Assets/realvirtual`
  - `Assets/Gizmos/realvirtual`
- 삭제 후 검증:
  - compile green
  - compile assembly 목록에서 `realvirtual.base` 미포함
  - `Boot` play smoke 통과
  - `RobotLibrary`, `Sandbox`, `RobotControl` scene open smoke 통과
  - 콘솔 기준 신규 삭제 유발 오류는 없고 `unityctl` IPC noise만 관찰됨
- 남은 후속:
  - 문서의 `legacy kept` 표현을 `removed` 기준으로 정리
  - `QaToolsMenu`의 삭제된 source path 문자열 정리 여부 결정
  - prefab `.meta` provenance 문자열 정리 여부 결정

## Post-removal donor recovery
- 삭제 직후 `RobotLibrary`와 `MathReadiness`에서 `SCARA` donor mesh가 깨지고, `FANUC CRX-10iA/L`는 미표시, `IGUS REBEL`은 pink fallback이 발생했다.
- 원인:
  - 런타임 robot prefab/scene가 여전히 `realvirtual`에서 오던 mesh/material GUID 일부를 직접 사용하고 있었음
  - `MathReadiness`에는 삭제된 `ScaraDonorProbe` prefab 인스턴스 참조가 남아 있었음
  - `FrameGizmo` 축 LineRenderer는 공유 머티리얼을 명시적으로 물지 않아 pink shader fallback 위험이 있었음
- 대응:
  - `Assets/Runtime/Robots/Common/Models/`에 `ScaraRobot.fbx`, `Gripper.fbx`, `fanuc-crx10ial.fbx`, `SchunkEGH80Gripper.fbx` 복구
  - `Assets/Runtime/Robots/Common/Materials/`에 기존 GUID를 유지하는 최소 대체 재질 복구
  - `Assets/Runtime/Robots/Common/Prefabs/ScaraDonorProbe.prefab` 생성
  - `Assets/Scripts/Visualization/Shared/FrameGizmo.cs`에서 축 LineRenderer에 `SharedLineMaterial` 강제 적용
  - `Assets/Scenes/MathReadiness.unity`의 stale donor prefab GUID를 현재 런타임 SCARA prefab 기준으로 정리
- 결과:
  - compile green 유지
  - `MathReadiness`, `RobotLibrary` open 기준 missing donor prefab 에러 해소
  - 사용자 확인 기준으로 SCARA/FANUC/IGUS 시각 문제가 복구됨

## Runtime folder cleanup
- `Assets/Runtime/Recovered/LegacyVendor`를 `Assets/Runtime/Robots/Common`으로 승격 이동했다.
- 비어 있던 `Assets/Runtime/Recovered 1`와 `Assets/Runtime/Recovered`를 제거했다.
- 해석:
  - donor 복구 자산이 더 이상 임시 `Recovered` 폴더가 아니라, 로봇 공용 런타임 자산층으로 정리된 상태다.
