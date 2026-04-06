# Pendant V3 Phase 0C Input Contract

## Date
- 2026-04-06 (KST)

## Summary
- V3 최소 입력 계약을 `RobotControlV3` 씬에 반영했다.
- `EventSystem` 1개, `InputSystemUIInputModule`, 기본 포커스 시작점, non-viewport 포인터 차단을 저장본 씬 기준으로 확인했다.
- popup focus trap 기본형도 코드에 추가했지만, Unity IPC 불안정으로 menu-driven runtime probe는 완전 자동 검증까지 닫지 못했다.

## Added
- `Assets/Scripts/UI/RobotControlV3/PendantV3InputContract.cs`
- `Assets/Scripts/App/RobotControlV3DebugBridge.cs`

## Updated
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`

## Evidence
- `unityctl check --type compile` green
- `check-v3-static.ps1` green
- `RobotControlV3.unity` hierarchy 기준 `EventSystem`, `InputSystemUIInputModule`, `PendantV3InputContract` 확인
- `Artifacts/V3/robotcontrolv3-0c-shell-v2.png`
- `Artifacts/V3/robotcontrolv3-popup-opened.png`

## Self Review
- 역할 경계: 입력/포커스 계약은 `UI/RobotControlV3`에만 두고 App/Visualization 계산은 넣지 않았다.
- concrete 의존: concrete robot client 직접 참조는 추가하지 않았다.
- authored-first: 씬 저장본 기준으로 `EventSystem`, `UIDocument`, `PendantV3InputContract`가 남도록 authoring 루프를 유지했다.
- 범위 통제: `0C`에서 바인딩/도메인 상태 계산/패널 orchestration은 넣지 않았다.
- 남은 숙제: popup probe 자동 검증 표면은 아직 툴링 이슈가 있어 후속 1A 이전에 다시 볼 가치가 있다.

## Open Risk
- popup focus trap의 runtime probe를 `unityctl exec`/menu surface로 끝까지 자동 검증하는 루프는 남아 있다.
