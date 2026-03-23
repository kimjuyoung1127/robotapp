# Sandbox QA Runbook

## Prep
- 메뉴: `KineTutor3D/QA: Prep Sandbox`
- Play 시작 후 예상 active scene: `RobotLibrary`

## Entry Route
1. `Play`
2. `RobotLibrary`에서 로봇 선택 → `Sandbox` 클릭
3. `Sandbox` 진입 확인

## Core Checks
- [ ] `BtnZeroPose`, `BtnHomePose`, `BtnDemoPose`, `BtnResetPose`가 보인다.
- [ ] `BtnSaveSnapshot`, `BtnLoadSnapshot`, `BtnCompareSnapshot`가 보인다.
- [ ] `BtnBackToLibrary` (로봇 목록 복귀)가 보인다.
- [ ] joint input rail이 보이고 실제 조작된다.
- [ ] why-it-moved / marker / trail이 샌드박스 계약과 충돌 없이 보인다.
- [ ] Robot Library 복귀가 가능하다.

## Layout / UI Checks
- [ ] 액션 패널이 `LeftPanel` 안에 깔끔하게 들어간다.
- [ ] 스냅샷 패널이 `RightPanel` 안에 깔끔하게 들어간다.
- [ ] 세로 비율/태블릿 비율에서 액션 패널과 스냅샷 패널이 서로 겹치지 않는다.
- [ ] BottomBar의 joint rail이 좌우 패널을 침범하지 않는다.

## UX Checks
- [ ] zero/home/demo/reset의 차이가 이해 가능하다.
- [ ] snapshot 상태 메시지가 이해 가능하다.
- [ ] 사용자가 지금 Sandbox 안에 있다는 것이 명확하다.
- [ ] Robot Library로 나가는 exit 경로가 분명하다.

## Known Follow-up Focus
- `tablet 4DOF rail`
- `replay / history`
- `constraint preview`
- `pick foundation`

## Quick Inspect Targets
- scene: `Sandbox.unity`
- objects: `SandboxActionContent`, `SnapshotContent`, `BtnZeroPose`, `BtnSaveSnapshot`, `BtnBackToLibrary`
