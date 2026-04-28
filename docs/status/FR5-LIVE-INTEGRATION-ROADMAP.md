# FR5 Live Integration Roadmap

## Goal

`robotapp2`를 먼저 FAIRINO FR5 실기 연동 가능한 수준까지 끌어올린 뒤,
그중 재사용 가능한 안정 구간만 별도 템플릿으로 잘라 `robottemplete`에 이식한다.

## Scope

- 대상 프로젝트: `robotapp2`
- 대상 로봇: `FAIRINO FR5`
- 1차 목표: `Mock -> Live` 전환 가능한 RobotControl 실기 검증 경로 완성
- 2차 목표: 실기 연동에서 검증된 공통 계층만 템플릿화

## Current Baseline

### Already Implemented

- 실기 연동 추상화 계층 존재
  - `Assets/Scripts/App/Fairino/IFairinoRobotClient.cs`
  - `Assets/Scripts/App/Fairino/FairinoConnectionService.cs`
- Live / Mock 전환 구조 존재
  - `Assets/Scripts/App/Fairino/LiveFairinoClient.cs`
  - `Assets/Scripts/App/Fairino/MockFairinoClient.cs`
- RobotControl 씬과 UI 셸 존재
  - `Assets/Scripts/App/Fairino/RobotControlSceneCoordinator.cs`
  - `Assets/Scripts/UI/RobotControl/FairinoConnectionPanel.cs`
  - `Assets/Scripts/UI/RobotControl/FairinoJointControlPanel.cs`
  - `Assets/Scripts/UI/RobotControl/FairinoTcpControlPanel.cs`
  - `Assets/Scripts/UI/RobotControl/RobotControlDiagnosticsDrawer.cs`
- 실기용 SDK DLL staging 완료
  - `Assets/Plugins/Fairino/libfairino.dll`
  - `Assets/Plugins/Fairino/CookComputing.XmlRpcV2.dll`
- 공식 자료 source map 정리 완료
  - `docs/ref/product/robots/fairino-fr5-integration-reference.md`
- P0 1차 코드 보강 완료
  - `LiveFairinoClient`가 실제 SDK 시그니처 기준 reflection 호출로 보강됨
  - `GetVersion()`이 SDK `GetSDKVersion`, `GetSoftwareVersion`, `GetFirmwareVersion` 경로를 사용함
  - `ReadState()`가 `GetRobotRealTimeState` 우선, fallback getter 차선 경로를 사용함
  - `MoveJ`, `MoveL`, `ServoJ`, `StopMotion`이 실제 SDK 파라미터 형태에 맞춰짐
- Live smoke tooling 추가
  - `Assets/Editor/KineTutor3D/FairinoLiveSmokeTools.cs`
- SDK 존재 검증용 테스트 추가
  - `Assets/Tests/EditMode/Validation/LiveFairinoClientSdkTests.cs`
- Readback-only live monitor 추가
  - `Assets/Scripts/App/Fairino/FairinoRobotClientFactory.cs`
  - `Assets/Scripts/App/Fairino/FairinoSdkCompatibilityProbe.cs`
  - `Assets/Scripts/App/Fairino/DirectReadbackFairinoClient.cs`
  - `Assets/Scripts/App/Fairino/FairinoBridgeClient.cs`
  - `Assets/Scripts/App/Fairino/Fr5LiveStateRecorder.cs`
- 맥북 field-readback 기준선 추가
  - 구현 커밋: `d8c0726 Add FR5 readback-only live monitor`
  - 검증 브랜치: `codex/robotcontrol-v3-toolkit`
  - field guide: `docs/ref/product/roadmap/fr5-live-field-checklist.md`

### Not Finished Yet

- `Connect(ip, port)`는 여전히 SDK `RPC(ip)` 경로에 의존하며, `port`는 진단 메시지 수준으로만 사용함
- `Mode`, `SetStatePeriod`, `GetStatePeriod`, `GetSafetyCode`, queue clear, log download 계층 없음
- `ServoCart`는 SDK에 존재하지만 앱 계층 미연결
- live state를 3D joint mirror에 더 정밀하게 묶는 전용 adapter가 아직 없음
- 실기 현장 검증 이력은 `connect fail` 수준까지만 존재
- 실제 컨트롤러 handshake 성공 / 현재 관절/TCP readback 검증 미완료
- Enable / small `MoveJ`는 이번 readback-only 범위가 아니며, 별도 승인 전까지 차단 상태로 둠

## Implementation Readiness Estimate

이 퍼센트는 현재 코드 기준의 추정치다.

| Area | Status |
|---|---:|
| RobotControl UI / scene shell | 85% |
| Mock / Live adapter architecture | 75% |
| SDK binary staging | 70% |
| Live SDK method correctness | 60% |
| Real state mirroring fidelity | 45% |
| Safety / recovery / diagnostics completeness | 25% |
| On-hardware validation | 10% |
| **Overall live-integration maturity** | **62%** |

## External Source Baseline

Official source hub:

- `https://www.frtech.fr/DOWNLOAD2`

Expected official artifacts:

- FAIRINO C# SDK
- Robot 8083 Port Status Feedback Protocol
- Robot Controller Communication Command Protocol

Internal source map:

- `docs/ref/product/robots/fairino-fr5-integration-reference.md`

## Phase Plan

### P0

목표: 실기 연결을 먼저 "안전하게 읽는 수준"까지 올린다. motion은 readback 성공 뒤 별도 phase에서 연다.

#### P0-1. SDK handshake 진짜 구현

- `LiveFairinoClient.GetVersion()`을 실제 SDK reflection 호출로 교체
- 연결 직후 firmware / sdk / controller 식별값을 읽어 UI와 diagnostics drawer에 표시
- 실패 시 mock fallback이 아니라 명확한 live-mode 오류를 노출

#### P0-2. Live connect 경로 정밀화

- `Connect(ip, port)`의 현재 동작과 SDK 시그니처를 맞춘다
- reflection 메서드 탐색 실패 시 어떤 메서드가 없는지 구체적으로 로그 남김
- DLL 누락 / 타입 미탐지 / RPC 실패를 분리해 에러 번역

#### P0-3. Read-only state path 강화

- `ReadState()`를 실제 가능한 SDK getter 세트로 확장
- 최소 수집 대상:
  - actual joints
  - actual TCP pose
  - connection state
  - enable state
- `RobotControlSceneCoordinator`에서 live 상태를 3D joint mirror에 안정 반영

#### P0-4. Safe motion 최소 경로

- readback-only 성공 전에는 `Enable`, `MoveJ`, `MoveL`, `IO`, `Gripper`를 모두 차단
- readback 세션 2회 이상 성공하고 drift가 안정적일 때 별도 motion gate 문서를 작성
- 첫 motion 검증은 후속 phase에서 "작은 범위 MoveJ 1회"만 따로 승인

#### P0-5. Live mode guardrails

- live 모드에서 MoveJ / MoveL 실행 전 confirm dialog 강제
- mock / live에 따라 버튼 라벨, 위험 표시, helper text 명확화
- `StopMotion()`을 긴급 정지 행동으로 UI 상단에서 항상 접근 가능하게 유지

#### P0 Exit Criteria

- 실기 컨트롤러에 연결 성공
- 실제 버전 정보 표시 성공
- 실제 joint / TCP 읽기 성공
- `Artifacts/live/fr5/latest-state.json` 갱신 성공
- `Artifacts/live/fr5/latest-drift.json` 갱신 성공
- session NDJSON append 성공
- 실패 시 사용자에게 이유가 분리되어 보임

### P1

목표: 실기 운영 품질과 진단 가능성을 높인다.

#### P1-1. 상태 폴링 / 주기 제어

- `SetStatePeriod` / `GetStatePeriod` 반영
- polling rate를 mock/live별로 조정
- diagnostics drawer에 state cycle 표시

#### P1-2. 모드 / safety / recovery

- `Mode(...)` 계층 추가
- `GetSafetyCode()` 추가
- `MotionQueueClear()` 추가
- connection lost 이후 reconnect / reset 가이드 정리

#### P1-3. Diagnostics drawer 실전화

- 현재 placeholder인 로그/복사/수집 기능 연결
- version / last error / recent feedback / retry hint 외에
  - safety code
  - queue status
  - state period
  - controller mode
  를 추가

#### P1-4. Error translation 확장

- 공식 error code table 기반 세분화
- 카테고리:
  - network
  - parameter range
  - unreachable pose
  - safety
  - controller internal

#### P1-5. MoveL 안정화

- TCP 입력 검증 강화
- dry-run FK 결과와 live MoveL target을 같이 보여줌
- live mode에서는 confirm dialog 필수 유지

#### P1 Exit Criteria

- live diagnostics가 운영자에게 충분한 정보 제공
- queue / safety / mode / polling 관련 상태 확인 가능
- MoveJ / MoveL 모두 제어 가능
- reconnect와 stop 시나리오 검증 완료

### P2

목표: 고급 실기 조작과 템플릿화 준비를 마친다.

#### P2-1. Servo path

- `ServoJ` 운영 정책 정리
- 필요 시 `ServoCart` 추가
- slider / joystick 기반 teleop에 대한 rate limit / safety clamp 적용

#### P2-2. Waypoint / teaching live path

- `WaypointCycleRunner`를 실제 live mode와 더 단단히 연결
- playback 중 stop / abort / recover 절차 정리

#### P2-3. Controller log / artifact capture

- SDK 제공 log/data export API 연결 가능성 검토
- 장애 시 evidence 수집 플로우 추가

#### P2-4. Template extraction boundary

- `robottemplete`로 옮길 수 있는 공통층과 옮기면 안 되는 실기 의존층을 분리

템플릿 이식 가능:

- `IFairinoRobotClient`
- `FairinoResult`
- `FairinoErrorTranslator`
- `FairinoVersionInfo`
- `FairinoRobotState`
- 안전 검증기
- live/mock 분리 adapter 구조

템플릿 이식 보류:

- 현장 IP 기본값
- controller-specific handshake 정책
- 운영 로그 수집 정책
- 현장 safety 절차
- 실기 승인 UI 문구

#### P2 Exit Criteria

- Servo / waypoint / diagnostics까지 구조 정리 완료
- 템플릿 추출 경계가 명확함
- `robottemplete` 이식 대상 목록 확정

## Order Of Work

1. `robotapp2` P0 완료
2. 현장 연결 검증
3. `robotapp2` P1 완료
4. 운영 진단 품질 확보
5. `robotapp2` P2 완료
6. 공통층만 slim live adapter 패키지 또는 template add-on으로 분리
7. 마지막에 `robottemplete` 이식

## Immediate Next Changes

다음 작업은 P0 기준으로 아래 순서를 권장한다.

1. 맥북에서 `codex/robotcontrol-v3-toolkit` 브랜치 clone/pull
2. `ping 192.168.58.2`와 `nc -vz 192.168.58.2 8080` 기록
3. Unity direct SDK readback 시도
4. direct 실패 시 `FAIRINO_BRIDGE_URL=http://127.0.0.1:5055` bridge readback 재시도
5. `latest-state.json`, `latest-drift.json`, `sessions/*.ndjson`를 field evidence로 저장
6. 현장 결과를 바탕으로 main 병합과 motion gate 분리 판단

## Template Strategy

`robottemplete`는 계속 slim 유지가 원칙이다.

- 기본 템플릿: FR5 visual / prefab / URDF / minimal interaction
- 확장 템플릿 또는 add-on: live adapter / diagnostics / safe motion

즉, 최종 목표는 "실기 연동이 검증된 뒤 그 중 공통적인 live adapter 층만 별도 템플릿화"다.

## Latest Execution Note

2026-04-28 기준 로컬 실행:

- `unityctl check --type compile`
  - PASS
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.Fr5LiveReadbackTests`
  - PASS (`6 passed`)
- 구현 커밋:
  - `d8c0726 Add FR5 readback-only live monitor`
- 정책:
  - 맥북 field session은 `main`이 아니라 `codex/robotcontrol-v3-toolkit` 브랜치에서 먼저 수행
  - 안전 모니터링이 성공해도 live motion은 자동으로 열지 않음
  - direct C# SDK 실패 시 bridge fallback으로 readback-only 유지

2026-03-25 기준 로컬 실행:

- `unityctl check --project ...robotapp2 --json`
  - PASS
- `unityctl test --project ...robotapp2 --mode edit --filter KineTutor3D.Tests.EditMode.FairinoConnectionServiceTests --json`
  - PASS (`3 passed`)
- `unityctl exec --project ...robotapp2 --code "KineTutor3D.Editor.FairinoLiveSmokeTools.RunSmoke()"`
  - 실행 성공
  - 결과: `CONNECT_FAIL ip=192.168.58.2 port=8080 code=-2`
  - 해석: 코드 경로는 live SDK 호출까지 진입했지만, 현재 테스트 머신에서는 FR5 컨트롤러 네트워크 응답이 없음
