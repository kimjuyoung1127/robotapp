# Pendant V3 PointMove MoveJ Hold UX Lock

## Date
- 2026-04-13 (KST)

## Summary
- `PointMoveController`에서 `MoveJ`를 IK 전까지 명시적으로 보류하는 UX를 고정했다.
- `MoveJ` 상태에서는 `BtnPointApply`를 비활성 처리하고 버튼 문구를 `적용 (MoveJ 준비중)`으로 바꿨다.
- `MoveL` 상태에서는 기존대로 `BtnPointApply` 활성 + 실제 dispatch 경로를 유지했다.
- `Apply` 가능 상태 판단을 preview 기준이 아니라 `CanApply()` 기준으로 분리했다.
- PointMove 경로에서 실기 연동 리스크를 줄이기 위한 5개 guard lock을 추가했다.

## Updated Files
- `Assets/Scripts/UI/RobotControlV3/PointMoveController.cs`
- `docs/ref/product/pendant-v3/progress-checklist.md`

## Verification
- `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - pass
- Play + `SceneNavigator.LoadByName("RobotControlV3")` + `SetShellSelection("NavMotion","TabPointMove","BottomTabEasyMotion")`
  - `SetPointMoveMotionKindForDebug("MoveJ")` 후 `BtnPointApply` click 시도
    - result: `UI Toolkit element 'BtnPointApply' is disabled in hierarchy.`
    - `BtnPointApply` text: `적용 (MoveJ 준비중)`
  - `SetPointMoveMotionKindForDebug("MoveL")` 후 `BtnPointApply`
    - enabled + text `적용` 확인
    - actual click feedback: `[Dispatch] MoveL 완료 · speed 30% · X -497.0 / Y -130.0 / Z 477.0`
  - 5-lock 확인
    - hidden 상태 `PreviewPointMoveForDebug()` -> `포인트 이동 패널이 열려 있을 때만 미리보기를 실행한다.`
    - hidden 상태 `ApplyPointMoveForDebug()` -> `포인트 이동 패널이 열려 있을 때만 적용할 수 있다.`
    - visible 상태 `PointNameInput=""` + `BtnPointApply` -> `포인트 이름을 먼저 넣어라.`
    - visible 상태 `PointNameInput` 클래스에 `rc-point-name-input--danger` 반영
    - visible 상태 `MoveL`에서만 `BtnPointApply` 활성

## Notes
- 현재 `MoveJ`는 UX 레벨에서 보류를 명확히 했고, 실제 `MoveJ` command dispatch는 여전히 pending이다.
- 다음 구현 단위는 `2C-1` 안전/진단 패널 최소 scaffold 착수 준비로 넘긴다.
