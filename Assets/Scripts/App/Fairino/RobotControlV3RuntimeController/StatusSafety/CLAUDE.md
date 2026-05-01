# RobotControlV3RuntimeController/StatusSafety/

Safety/Status/WhyItMoved 패널이 쓰는 diagnostics, evidence, live safety backend를 담는 폴더입니다.

## 주요 역할
- evidence freshness/tool/user/coord truth
- gate summary / blocked reason / next action
- live safety evaluation과 audit artifact
- operator-facing 상태/안전 문구 보조

## 주 소비자
- `SafetyDiagnosticsController`
- `StatusCardController`
- `WhyItMovedController`

## 하지 말아야 할 것
- connection lifecycle
- gripper/joint/TCP actual dispatch
- point/teaching sequence orchestration

## naming rule
- evidence truth는 `Evidence` 파일에 둔다.
- diagnostics/operator wording은 `StatusSafety` 파일에 둔다.
- centralized safety evaluation은 `LiveSafety` 파일에 격리한다.

## 현재 파일 인덱스
- `RobotControlV3RuntimeController.Evidence.cs`
- `RobotControlV3RuntimeController.StatusSafety.cs`
- `RobotControlV3RuntimeController.LiveSafety.cs`
