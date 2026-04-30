# realvirtual Removal Plan

최종 업데이트: 2026-03-31 (KST)

## 목적
- `Assets/realvirtual`가 에디터 리로드/초기화에 주는 영향을 줄이고
- 런타임 로봇/프리뷰 자산을 vendor-free 상태로 독립시킨 뒤
- 최종적으로 `Assets/realvirtual` 제거 가능 여부를 판단한다.

## 현재 판단
- **실삭제 완료**
- 현재 상태:
  - `Assets/realvirtual` 제거 완료
  - `Assets/Gizmos/realvirtual` 제거 완료
  - compile green 유지
  - `Boot` play smoke 통과
  - `RobotLibrary`, `Sandbox`, `RobotControl` scene open smoke 통과
  - 삭제 후 깨졌던 `SCARA` / `FANUC CRX-10iA/L` / `IGUS REBEL` donor mesh 참조는 `Assets/Runtime/Robots/Common/` 아래 최소 복구 자산으로 복원 완료
- 남은 일:
  - 문서의 `legacy kept` 표현을 `removed` 기준으로 정리
  - `QaToolsMenu` 안의 레거시 source 문자열 정리 여부 결정
  - prefab `.meta` provenance 문자열 정리 여부 결정

## 지금까지 완료
- `RobotCatalog`의 SCARA/Fanuc/igus `importSource`를 `Assets/Runtime/Resources/Robots/...`로 이관
- `ScaraDonorStructureTests`를 runtime donor prefab 기준으로 이관
- `RobotPreviewPod`에서 `realvirtual.Drive` reflection 제거
- 문서에서 `Assets/realvirtual`를 **레거시 vendor source**로 재정의
- editor bootstrap 완화
  - `OnLoad.cs`: PackageManager list 동기 대기 제거
  - `QuickEditToolbarIMGUI.cs`: 기본 부트스트랩 비활성화
  - `OnPostProcessImportAsset.cs`: 기본 import 후 자동화 비활성화

## 남은 직접 리스크

### 1. Runtime prefab vendor 흔적
- 현재 상태
  - `QaToolsMenu.SanitizeRuntimeRobotPrefabs()` 실행 완료
  - `ScaraRobot`, `FanucCRX-10iA_L`, `igusRebel` 런타임 prefab에서 총 38개 `realvirtual` component 제거
  - prefab 검색 기준 `realvirtualController: {fileID: 0}` 및 대표 `realvirtual` script guid 미검출
  - `Assets/Runtime/Robots/Common/` 아래에 donor mesh/재질 최소 복구 자산을 추가해 SCARA/FANUC/IGUS 시각 깨짐을 해소
- 판단
  - **직접 blocker 해소**

### 2. Editor/Project settings 경로
- 현재 상태
  - `Assets/Runtime/RenderPipelines/URP/KineTutor3D-URP.asset` 생성
  - `Assets/Runtime/RenderPipelines/URP/Settings/KineTutor3D-URP-Default-Renderer.asset` 생성
  - `Assets/Runtime/RenderPipelines/URP/Settings/KineTutor3D-URP-Thumbnail-Renderer.asset` 생성
  - 복제 renderer asset에서 `realvirtual` 전용 renderer feature block 제거
  - `GraphicsSettings.asset`, 모든 `QualitySettings` tier가 새 project-owned URP asset GUID를 사용하도록 전환
  - compile green 유지
- 남은 항목
  - `QaToolsMenu` 안의 레거시 source 경로 문자열 정리 여부 결정
  - 일부 prefab `.meta`의 provenance 문자열은 비기능 정보라 정리 여부만 판단하면 됨

### 3. 문서/운영 흔적
- `asset-registry.md`, `asset-validation-report.md`, `PROJECT-STATUS.md`는 레거시 source 문맥으로 정리했지만
- 최종 제거 시점에는 `legacy kept` → `removed`로 다시 갱신 필요

### 4. Post-removal visual recovery
- 삭제 직후 확인된 문제
  - `RobotLibrary`, `MathReadiness`에서 SCARA donor mesh 깨짐
  - `FANUC CRX-10iA/L` 미표시
  - `IGUS REBEL` 일부 메시/재질 pink fallback
- 대응
  - `Assets/Runtime/Robots/Common/Models/`에 SCARA/FANUC donor mesh source 복구
  - `Assets/Runtime/Robots/Common/Materials/`에 기존 GUID를 유지하는 대체 재질 복구
  - `Assets/Runtime/Robots/Common/Prefabs/ScaraDonorProbe.prefab` 추가
  - `FrameGizmo`가 축 LineRenderer에 `SharedLineMaterial`을 강제 적용하도록 수정
  - `MathReadiness.unity`의 stale `ScaraDonorProbe` prefab GUID를 현재 런타임 SCARA prefab 기준으로 정리
- 결과
  - compile green 유지
  - `MathReadiness`, `RobotLibrary` open 기준 missing donor prefab 에러 해소
  - 이후 `Assets/Runtime/Recovered/LegacyVendor`는 `Assets/Runtime/Robots/Common`으로 승격 이동

## 실행 순서

### Phase A. Editor 영향 최소화
- 목표: 파일 수정 후 domain reload 정체가 실질적으로 줄었는지 확인
- 할 일
  - 현재 차단한 bootstrap 상태 유지
  - 2~3회 script edit 후 reload 체감 재확인
- 통과 기준
  - `Reloading Domain` 모달이 기존처럼 장시간 고정되지 않음

### Phase B. Runtime prefab sanitation
- 목표: runtime robot prefab에서 `realvirtual.*` MonoBehaviour 제거
- 대상
  - `ScaraRobot.prefab`
  - `FanucCRX-10iA_L.prefab`
  - `igusRebel.prefab`
- 방법
  - YAML 직접 수정 금지
  - Editor utility 또는 Unity 수동 저장으로 vendor component 제거
  - 제거 후 prefab 재저장
- 현재 상태
  - `QaToolsMenu.SanitizeRuntimeRobotPrefabs()` 실행 완료
  - `ScaraRobot`, `FanucCRX-10iA_L`, `igusRebel` 런타임 prefab에서 총 38개 `realvirtual` component 제거
  - prefab 검색 기준 `realvirtualController: {fileID: 0}` 및 대표 `realvirtual` script guid 미검출
  - `ScaraDonorStructureTests`, `RobotPreviewPodTests`, compile green 유지
- 통과 기준
  - prefab 검색에서 `realvirtualController: {fileID: 0}` 미검출
  - runtime preview / donor path 유지

### Phase C. Settings and editor assets audit
- 목표: `realvirtual` 경로를 참조하는 프로젝트 설정/보조 자산 제거 가능 여부 확인
- 대상
  - `GraphicsSettings.asset`
  - `QualitySettings.asset`
  - `Assets/Gizmos/realvirtual`
- 현재 상태
  - project-owned URP pipeline asset 세트 생성 완료
  - copied renderer asset에서 `realvirtual` renderer feature/script GUID 제거 완료
  - `GraphicsSettings.asset`, `QualitySettings.asset`를 새 URP asset GUID로 repoint 완료
  - `Assets/Runtime` + `ProjectSettings` 범위 검색 기준 old URP GUID / old renderer GUID / `realvirtual` renderer feature GUID 미검출
- 통과 기준
  - 프로젝트가 `realvirtual` asset 경로 없이 compile + open 가능
 - 판단
  - **사실상 완료**, 최종 삭제 전에는 `Assets/Gizmos/realvirtual`와 rehearsal만 남음

### Phase D. Removal rehearsal
- 목표: 실제 삭제 전 dry-run 검증
- 할 일
  - `Assets/realvirtual` 임시 rename 또는 분리 브랜치에서 삭제 시뮬레이션
  - compile
  - 핵심 EditMode
  - 핵심 PlayMode/scene smoke
- 현재 상태
  - `unityctl asset delete` 기준으로 `Assets/realvirtual`, `Assets/Gizmos/realvirtual` 실제 삭제 rehearsal 수행
  - 삭제 직후 `asset get-info` 기준 두 경로 모두 not found 확인
  - 삭제 상태에서 compile check pass 확인
  - compile assembly 목록에서 `realvirtual.base` 제외 확인
  - rehearsal 후 backup copy로 원복 완료
- 통과 기준
  - compile green
  - SCARA/RobotLibrary/Sandbox/RobotControl 핵심 smoke 유지

### Phase E. Final removal
- `Assets/realvirtual` 제거 완료
- `Assets/Gizmos/realvirtual` 제거 완료
- compile 및 scene smoke 기준 기본 검증 완료
- 남은 것은 문서/레거시 문자열 후속 정리

## 중단 기준
- runtime prefab에서 missing script 발생
- SCARA/Fanuc/igus preview 또는 donor path 손상
- URP/scene opening이 깨짐
- domain reload 문제가 오히려 악화

## 권장 다음 액션
1. `asset-registry.md`, `asset-validation-report.md`, `PROJECT-STATUS.md`, 관련 daily log를 `removed` 기준으로 계속 정리
2. `QaToolsMenu`의 레거시 source 문자열과 obsolete menu path 정리 여부 결정
3. prefab `.meta` provenance 문자열을 유지할지 제거할지 결정
