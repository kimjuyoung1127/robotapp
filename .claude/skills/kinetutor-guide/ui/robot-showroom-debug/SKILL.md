---
name: robot-showroom-debug
description: "Robot Library / Sandbox robot showroom 디버깅과 튜닝 — showroomoutput, comparestrip, preview pod, Game view vs Scene view 스케일 차이, hero/page selection, runtime root 중복"
---

## Trigger
Robot showroom이 Game 뷰에서 너무 작거나, `showroomoutput`에 기대한 수만큼 로봇이 보이지 않거나, `comparestrip`이 비거나 가려지거나, 좌우 화살표 이동 후 hero/회전 대상이 어긋날 때.
키워드: `robot showroom`, `showroomoutput`, `comparestrip`, `preview pod`, `robot library viewport`, `Game view`, `Scene view`, `hero selection`

## Read First
1. `Assets/Scripts/UI/RobotLibraryManager.cs`
2. `Assets/Scripts/UI/RobotShowroomManager.cs`
3. `Assets/Scripts/Visualization/RobotPreviewPod.cs`
4. `Assets/Scripts/Visualization/RobotPreviewFactory.cs`
5. `docs/ref/product/ux/robot-library.md`
6. `docs/status/SKILL-DOC-MATRIX.md`

## Check Order
1. 활성 씬과 PlayMode 여부를 먼저 확인한다.
2. `RobotShowroomRuntime`가 1개만 존재하는지 확인한다.
3. `showroomOutput` RenderTexture 크기가 `Screen`이 아니라 실제 `RectTransform` 크기를 따르는지 확인한다.
4. 현재 visible pod 수와 hero 로봇 ID가 기대와 맞는지 확인한다.
5. `comparestrip`이 RawImage 뒤에 가려지거나 detail overlay에 덮이지 않는지 확인한다.

## Guardrails
1. `RobotLibraryManager`는 `ExecuteAlways`여도 edit mode에서 showroom runtime root를 만들지 않는다.
2. `RobotShowroomRuntime`는 한 번에 1개만 유지한다.
3. 기본 hero는 첫 페이지의 가운데 로봇을 우선한다.
4. `PreviousPage`/`NextPage`는 페이지 시작 로봇이 아니라 해당 페이지의 hero로 이동해야 한다.
5. camera framing은 `showroomOutput`의 실제 rect와 visible pod 수를 기준으로 계산한다.
6. detail drawer는 명시적 선택 없이 자동으로 compare strip을 가리지 않는다.

## Validation
1. 첫 페이지에서 3개 pod가 보이고 가운데 hero가 회전한다.
2. 다음/이전 페이지 후 hero가 기대한 로봇으로 복귀한다.
3. Game 뷰와 Scene 뷰에서 상대 크기 차이가 과도하지 않다.
4. `RobotShowroomRuntime` 중복이 생기지 않는다.
5. `comparestrip` 텍스트가 좌/중/우 기대 위치에 보인다.

