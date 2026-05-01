# RobotControlV3RuntimeController/

`RobotControlV3RuntimeController`의 패널 우선 backend partial을 모으는 폴더입니다.

## 주요 역할
- V3 패널별 runtime responsibility 분리
- panel controller와 backend partial의 대응 관계 고정
- shared approval/evidence/safety 경계 유지
- stage/runtime 시각화 helper 분리

## 하위 폴더
- `ConnectionHome/` — connect/sync/mode truth와 connection events
- `EasyMotion/` — gripper operator/live path
- `JointControl/` — joint preview/apply와 tiny-joint entry
- `TcpControl/` — TCP preview/apply
- `PointMove/` — sequence/teaching/point orchestration
- `StatusSafety/` — evidence, diagnostics summary, live safety gate
- `Stage/` — stage runtime/render helper
- `Shared/` — approval/session/common helper

## legacy partial
- `RobotControlV3RuntimeController.cs` — composition root, fields, lifecycle, 최소 orchestration
- `RobotControlV3RuntimeController.Helpers.cs` — 공용 helper와 operator blocked reason helper
- `RobotControlV3RuntimeController.ReadbackAsync.cs` — async connect/sync loop
- `RobotControlV3RuntimeController.ViewState.cs` — snapshot/view-state 조립
- `RobotControlV3RuntimeController.StageCamera.cs` — stage camera 전용 조작
- `RobotControlV3RuntimeController.Motion.cs` — 잔여 legacy motion helper

## 규칙
1. 새 panel/backend 책임은 먼저 이 폴더 안의 대응 subfolder에 넣는다.
2. 새 partial 파일 첫 줄은 `// Folder: ...` 역할 주석으로 시작한다.
3. broad live block 정책은 `StatusSafety/LiveSafety`에만 모으고 panel 파일에 흩뿌리지 않는다.
4. 패널 전용 로직은 `Shared/`에 두지 않는다.
