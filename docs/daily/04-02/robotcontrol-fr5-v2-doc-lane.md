# RobotControl FR5 V2 Doc Lane

## Date
- 2026-04-02 (KST)

## Summary
- 기존 `robotcontrol-*` 문서와 직접 섞지 않기 위해 `docs/ref/product/robotcontrol-fr5-v2/` 전용 문서 lane을 만들었다.
- 새 lane은 `quarantine lane`으로 정의했고, 공식 문서/SDK/타사 공식 UI 참고/실기 검증/제품 결정을 분리해서 적도록 했다.
- `README.md`에서 isolation rule, evidence class, promotion rule을 먼저 잠갔다.
- `fr5-official-source-register.md`에 FAIRINO 공식 다운로드 허브, SDK 매뉴얼, teach pendant/manual teaching, C# movement, C# SDK 저장소를 등록했다.
- `fr5-command-ssot.md`에 `CommandId` 기준의 명령 SSOT 초안을 만들고, `candidate / researching / locked-doc / field-needed` 상태 체계를 분리했다.
- `fr5-ui-capability-map.md`에 SimMachine 스크린샷, FAIRINO 공식 문서, UR/Doosan 공식 UI 레퍼런스의 역할을 분리했다.
- `fr5-live-validation-plan.md`에 실기 bring-up 순서와 증거 요구사항을 따로 뒀다.
- `open-questions.md`에 공식 근거 또는 현장 검증이 더 필요한 항목만 분리했다.
- 같은 날 후속 업데이트로 `fr5-command-ssot.md`에 아래를 함수명 단위로 잠갔다.
  - `RPC`
  - `RobotEnable`
  - `GetActualJointPosDegree`
  - `GetActualTCPPose`
  - `MoveJ`
  - `MoveL`
  - `StopMotion`
  - `StartJOG` / `StopJOG` / `ImmStopJOG`
  - `SetTcp4RefPoint` / `ComputeTcp4` / `SetToolCoord`
- `SavePoint`와 `Loop`는 direct SDK 함수가 아니라 `official teaching point concept + product sequence layer`로 잠갔다.
- 실기 FR5에 그리퍼가 달려 있으면 `toolcoord0`이 아니라 `그리퍼 TCP`를 기준으로 motion/state/teaching point를 해석한다는 원칙을 문서에 추가했다.
- 추가로 `fr5-gripper-tcp-calibration-spec.md`를 새로 만들어, gripper TCP calibration을 별도 규격으로 분리했다.
- 이 문서에는 `mesh origin != TCP SSOT`, `SetTcp4RefPoint / ComputeTcp4 / SetToolCoord`, `GetActualTCPNum / GetCurToolCoord / GetActualTCPPose`, `preview/teaching/EE marker 일치 규칙`을 잠갔다.
- 같은 날 후속 결정으로 현재 실기 FR5의 작업 TCP를 `그리퍼 사이 정중앙 공중점`으로 잠갔다.
- 이에 따라 gripper TCP spec, command SSOT, open questions 문서에 동일 기준을 반영했다.
- 추가로 `fr5-live-capture-checklist.md`를 만들어, 실기 LAN 연결 후 `connect + read + log + disconnect` 순서로 raw evidence를 남기는 절차를 분리했다.
- 이 문서에는 subnet 준비, read-only first session, 최소 raw field 목록, raw evidence와 normalized SSOT의 분리 원칙을 적었다.

## Why
- FR5 실기 연동 관련 결정이 기존 V1/V2 handoff 문서와 섞이면, 무엇이 공식 근거인지와 무엇이 임시 UX인지가 다시 오염될 위험이 컸다.
- 공식 근거가 확보되기 전까지는 기존 canonical/product leaf 문서로 바로 승격하지 않는 흐름이 더 안전하다.

## Next
- FAIRINO 공식 문서와 공식 SDK에서 `Connect / Enable / Read actual pose / Sync / MoveJ / MoveL / point record / loop`의 실제 함수명과 선행조건을 채운다.
- `joint arrow jog`, `remember current pose`, `ghost preview`, `floor grid`를 `product-decision`으로 어디까지 잠글지 확정한다.
