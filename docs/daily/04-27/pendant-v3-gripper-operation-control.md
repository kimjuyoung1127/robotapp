# Pendant V3 Gripper Operation Control

Date: 2026-04-27 (KST)

## Decision

- 그리퍼 보조 패널은 `포인트`가 아니라 `조작 > 기본` 흐름에 둔다.
- 패널 UI는 실제 조작에 쓰는 `열기`, `닫기`, position slider, numeric input, `위치 적용`만 남긴다.
- 그리퍼 기본 상태는 `position=100`, visual open ratio `1.00`인 완전 열림이다.
- 완전 닫힘은 `position=0`, visual open ratio `0.00`이며 finger 안쪽이 서로 닿는 상태로 본다.
- `TcpMarker` 구체 prefab은 제거한다. grip target은 `TcpFrame`과 finger center로 계산하고, object hold는 핑거 사이 실제 collider/renderer가 감지될 때만 발생한다.
- 실제 물체가 감지되면 close 요청이 `0%`여도 visual/mock은 안전 정지선에서 멈추고 `holding object` 상태를 남긴다.

## Official SDK Basis

- FAIRINO C# SDK gripper flow:
  - `SetGripperConfig(company, device, softversion, bus)`
  - `ActGripper(index, action)`
  - `MoveGripper(index, pos, vel, force, max_time, block, type, rotNum, rotVel, rotTorque)`
- Readback:
  - `GetGripperMotionDone`
  - `GetGripperActivateStatus`
  - `GetGripperCurPosition`
  - `GetGripperCurSpeed`
  - `GetGripperCurCurrent`
  - `GetGripperVoltage`
  - `GetGripperTemp`
- SDK 문서상 position/speed/force/current는 `0~100` percentage 계약이다.
- Official references:
  - FAIRINO C# robot peripherals: https://fairino-doc-en.readthedocs.io/3.8.0/SDKManual/C%23RobotPeripherals.html
  - Fairino USA Robot Peripherals: https://www.fairino.us/cobots-manual/11.-robot-peripherals

## Changed

- `IoPanelController` 표시 조건을 `NavMotion + TabEasyMotion` / `BottomTabEasyMotion`으로 옮겼다.
- 그리퍼 패널에 position slider와 numeric input을 추가하고, DO/ToolDO 버튼과 상태/피드백 복사 문구는 제거했다.
- `RobotControlPeripheralFacade`를 bool open/close 중심에서 commanded/actual position percent 중심으로 확장했다.
- `RobotControlPeripheralState`와 `RobotControlV3RuntimeSnapshot`에 commanded/actual/speed/force/object-detected/holding/stop-percent 상태를 추가했다.
- `FR5EndEffectorAttachment`는 `TcpMarker` 이름을 더 이상 grip object로 보지 않는다. 명시적 `gripObjectRoot` 또는 핑거 사이 probe에 걸린 외부 collider/renderer만 grip object로 본다.
- legacy local state의 `NavIo` / `BottomTabIo`는 각각 `NavMotion` / `BottomTabEasyMotion`으로 normalize한다.
- 후속 수정:
  - slider와 numeric input 값 변경 시 즉시 `SetGripperPositionPercent(...)`를 호출하게 했다.
  - finger visual은 닫힌 기준에서 바깥으로 여는 방식이 아니라, authored open pose를 기준으로 캡처하고 close 때 `TcpFrame` 또는 실제 grip object 방향으로 안쪽 이동한다.
  - `RecaptureGripperAuthoredOpenForDebug()`를 추가했다. Unity에서 finger transform을 손으로 맞춘 뒤 호출하면 현재 위치를 새 authored open 기준으로 잡는다.
  - close/open 명령은 authored open 기준을 유지한 채 `gripperMotionDuration` 동안 보간한다. 기본 완전 열림은 그대로 두고, 닫기 버튼을 누르면 finger가 가운데 구체 방향으로 서서히 닫힌다.
  - gripper visual 생성 시 peripheral snapshot이 아직 없으면 fallback을 완전 열림(`openRatio=1.0`)으로 둔다.

## Verification

- `dotnet build Assembly-CSharp.csproj --no-restore`: pass, errors `0`.
- `git diff --check`: pass.
- `unityctl check --type compile`: pass.
- `unityctl exec invoke KineTutor3D.App.RobotControlV3DebugBridge.GetGripperVisualSummaryForDebug`: gripper visual attached, target `TcpFrame`, object not detected when no real object is present.
- `unityctl exec invoke ... SetGripperPositionForDebug [100]`: actual/open visual reaches `openRatio=1.00`.
- `unityctl exec invoke ... SetGripperPositionForDebug [0]`: object-detected close clamps to actual `35%`, visual reaches `openRatio=0.35`.
- Follow-up fix:
  - replaced fixed local stroke movement with rendered finger-center travel, because PGEA finger parent transforms can share the same authored local origin.
  - reset obviously distorted runtime finger offsets back to the prefab authored-open baseline before recapturing the open pose.
  - set the PGEA prefab serialized default `gripperOpenRatio` to `1`.
  - runtime check after reset: open starts with finger parent offsets `0,0,0`; close command moves left/right in opposite directions and reduces target distances before stopping at object-safe `openRatio=0.35`.
- Post-restart success confirmation:
  - Unity restarted under PID `2584`; `unityctl status --wait` returned `Playing`, `bridgeLoaded=True`, `ipcPipePresent=True`.
  - open baseline: `Cmd 100% / Actual 100%`, visual `openRatio=1.00`, finger parent offsets stay at authored open `(0,0,0)`.
  - previous marker-based close request with the center cube present: `SetGripperPositionForDebug [0]` returned `Cmd 0% / Actual 35% / Object Hold (0.35)`.
  - this established the intended hold pattern, but the marker itself is no longer the object source after the real-object basis update below.
  - after confirmation the debug state was returned to `Cmd 100% / Actual 100%`.
- Real-object basis update:
  - removed the serialized `TcpMarker` child from `Assets/Runtime/Resources/EndEffectors/PGEA_100_40.prefab`.
  - runtime cleanup removes legacy `TcpFrame/TcpMarker` children from already-authored attachment instances.
  - `FR5EndEffectorAttachment` now excludes robot-owned geometry and legacy marker names from grip-object detection.
  - no-object verification after asset refresh and Onboarding -> RobotControlV3: `renderers=5`, `target=TcpFrame`, `objectDetected=False`, `objectStop=0`.
  - close verification without real object: `SetGripperPositionForDebug [0]` returns `Cmd 0% / Actual 0%`, visual `openRatio=0.00`.
  - this matches the official SDK readback model: Unity visual/mock hold should come from real object/contact evidence, while live 판단은 `GetGripperCurPosition`, `GetGripperCurCurrent`, and `GetGripperMotionDone` readback 비교로 확정한다.
- Zero-position visual calibration:
  - FAIRINO SDK `pos=0` is a 0~100 percentage command/readback, not a universal mesh-contact guarantee.
  - In this Unity mock, the no-object visual contract is stricter: `position=0` should look fully closed, with the inner finger faces meeting.
  - `GripperCalibrationProfile` separates user percent, FAIRINO raw percent, and Unity visual input/pose ratio.
  - Current observed PGEA calibration maps user `0%` closed/contact to raw `60%` and previous visual input `0.60`, user `100%` open to raw `100%` and visual input `1.00`, and object-stop to raw `70%`.
  - UI slider/input and `GripperSummary` show user percent. Debug/SDK summaries also expose raw command/readback percent.
  - If a real workpiece is detected between the fingers, object-stop still takes priority and can keep actual above `0%`.
- Authored closed-pose update:
  - Close travel scalar tuning is not treated as the final source of truth. If the PGEA mesh still does not visually meet at `0%`, use an authored closed pose.
  - `FR5EndEffectorAttachment` now stores optional `fingerLeftClosed` / `fingerRightClosed` local positions and interpolates `authored open -> authored closed` when enabled.
  - `FR5EndEffectorAttachment` receives the calibrated visual input ratio and resolves `visualClosedAt=0.60` to pose `0.00` closed/contact. It does not need to know the raw `60%` SDK contact calibration.
  - Mock object stop defaults above the raw contact point so a detected workpiece stops before the no-object finger-contact pose.
  - Debug bridge entry points:
    - `RecaptureGripperAuthoredOpenForDebug()`: capture the current local finger transforms as the 100% open baseline.
    - `RecaptureGripperAuthoredClosedForDebug()`: after manually aligning the fingers in Unity, capture the current local finger transforms as the 0% closed baseline.
    - `ClearGripperAuthoredClosedForDebug()`: return to computed close travel fallback.
  - This remains visual-only. FAIRINO live commands still use SDK `MoveGripper(... pos ...)` and readback comparison.

## Notes

- 실제 live `MoveGripper`는 기존 safety gate를 유지한다. operator confirm, readback, production policy 없이 바로 열지 않는다.
- 현재 object stop은 visual/mock 안전 모델이다. 실기에서는 SDK readback의 position/current/motionDone을 pendant 상태와 비교한 뒤 force/current threshold 정책을 별도 확정해야 한다.
- 성공 패턴은 Codex skill `C:\Users\ezen601\.codex\skills\robotapp2-gripper-success-pattern\SKILL.md`에도 남겼다. 같은 증상이 재발하면 authored-open, close-direction, object-stop 검증 순서부터 재사용한다.
